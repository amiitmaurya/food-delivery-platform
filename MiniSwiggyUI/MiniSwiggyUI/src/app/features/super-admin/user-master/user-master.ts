import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserMaster, UserStats, RoleMaster } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-user-master',
  templateUrl: './user-master.html',
  styleUrl: './user-master.css',
  standalone: false
})
export class UserMasterComponent implements OnInit {
  private userService = inject(UserService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  users: UserMaster[] = [];
  roles: RoleMaster[] = [];
  stats: UserStats = {
    totalUsers: 0,
    totalSuperAdmins: 0,
    totalAdmins: 0,
    totalCustomers: 0,
    totalDeliveryPartners: 0,
    totalRestaurantOwners: 0,
    activeUsers: 0,
    inactiveUsers: 0
  };

  isLoading = true;
  searchQuery = '';
  selectedRoleFilter = 'All';
  selectedStatusFilter = 'All';

  // Modal States
  showAddEditModal = false;
  isEditing = false;
  editingUserId = 0;

  showDeleteModal = false;
  userToDelete: UserMaster | null = null;
  isDeleting = false;

  showResetPasswordModal = false;
  userToResetPassword: UserMaster | null = null;
  resetPasswordModel = {
    newPassword: '',
    confirmPassword: ''
  };

  // Form Data for Create / Edit
  userFormData = {
    fullName: '',
    email: '',
    phoneNumber: '',
    password: '',
    roleId: 2,
    isActive: true,
    emailVerified: true,
    imageUrl: ''
  };

  get filteredUsers(): UserMaster[] {
    let result = [...this.users];

    if (this.selectedRoleFilter !== 'All') {
      const rf = this.selectedRoleFilter.toLowerCase();
      result = result.filter(u => {
        const rName = (u.roleName || '').toLowerCase();
        if (rf === 'deliverypartner') {
          return rName.includes('delivery');
        }
        return rName === rf;
      });
    }

    if (this.selectedStatusFilter === 'Active') {
      result = result.filter(u => u.isActive);
    } else if (this.selectedStatusFilter === 'Inactive') {
      result = result.filter(u => !u.isActive);
    }

    if (this.searchQuery && this.searchQuery.trim()) {
      const tokens = this.searchQuery.trim().toLowerCase().split(/\s+/);
      result = result.filter(u => {
        const str = `${u.fullName} ${u.email} ${u.phoneNumber} ${u.roleName}`.toLowerCase();
        return tokens.every(token => str.includes(token));
      });
    }

    return result;
  }

  ngOnInit(): void {
    this.loadRoles();
    this.loadStats();
    this.loadUsers();
  }

  loadRoles(): void {
    this.userService.getRoles().pipe(
      timeout(4000),
      catchError(() => of([]))
    ).subscribe(data => {
      this.roles = data || [];
      this.cdr.detectChanges();
    });
  }

  loadStats(): void {
    this.userService.getStats().pipe(
      timeout(4000),
      catchError(() => of({
        totalUsers: 0,
        totalSuperAdmins: 0,
        totalAdmins: 0,
        totalCustomers: 0,
        totalDeliveryPartners: 0,
        totalRestaurantOwners: 0,
        activeUsers: 0,
        inactiveUsers: 0
      }))
    ).subscribe(stats => {
      if (stats) {
        this.stats = stats;
        this.cdr.detectChanges();
      }
    });
  }

  loadUsers(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.userService.getAll().pipe(
      timeout(5000),
      catchError(err => {
        console.error('Error fetching users:', err);
        return of([]);
      })
    ).subscribe({
      next: (data) => {
        this.users = data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.users = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openCreateModal(): void {
    this.isEditing = false;
    this.editingUserId = 0;
    this.userFormData = {
      fullName: '',
      email: '',
      phoneNumber: '',
      password: '',
      roleId: this.roles[0]?.id || 2,
      isActive: true,
      emailVerified: true,
      imageUrl: ''
    };
    this.showAddEditModal = true;
    this.cdr.detectChanges();
  }

  openEditModal(user: UserMaster): void {
    this.isEditing = true;
    this.editingUserId = user.id;
    this.userFormData = {
      fullName: user.fullName || '',
      email: user.email || '',
      phoneNumber: user.phoneNumber || '',
      password: '',
      roleId: user.roleId || 2,
      isActive: user.isActive,
      emailVerified: user.emailVerified,
      imageUrl: user.imageUrl || ''
    };
    this.showAddEditModal = true;
    this.cdr.detectChanges();
  }

  onlyNumbers(event: KeyboardEvent): boolean {
    const charCode = event.which ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
      event.preventDefault();
      return false;
    }
    return true;
  }

  onPhoneInput(event: any): void {
    let val = (event.target.value || '').replace(/\D/g, '');
    if (val.startsWith('91') && val.length > 10) {
      val = val.substring(2);
    }
    if (val.length > 10) {
      val = val.substring(0, 10);
    }
    this.userFormData.phoneNumber = val;
    event.target.value = val;
  }

  saveUser(): void {
    if (!this.userFormData.fullName || !this.userFormData.fullName.trim()) {
      this.toast.error('Please enter Full Name');
      return;
    }

    if (this.userFormData.fullName.trim().length < 2) {
      this.toast.error('Full Name must be at least 2 characters');
      return;
    }

    if (!this.userFormData.email || !this.userFormData.email.trim()) {
      this.toast.error('Please enter Email Address');
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.userFormData.email.trim())) {
      this.toast.error('Please enter a valid Email Address');
      return;
    }

    if (this.userFormData.phoneNumber && this.userFormData.phoneNumber.trim()) {
      if (!/^[6-9]\d{9}$/.test(this.userFormData.phoneNumber.trim())) {
        this.toast.error('Please enter a valid 10-digit Phone Number');
        return;
      }
    }

    if (!this.userFormData.roleId || Number(this.userFormData.roleId) <= 0) {
      this.toast.error('Please select User Role');
      return;
    }

    if (!this.isEditing) {
      if (!this.userFormData.password) {
        this.toast.error('Please enter Password');
        return;
      }

      if (this.userFormData.password.length < 6) {
        this.toast.error('Password must be at least 6 characters');
        return;
      }

      this.userService.create({
        fullName: this.userFormData.fullName.trim(),
        email: this.userFormData.email.trim().toLowerCase(),
        phoneNumber: this.userFormData.phoneNumber?.trim() || '',
        password: this.userFormData.password,
        roleId: Number(this.userFormData.roleId),
        isActive: this.userFormData.isActive,
        imageUrl: this.userFormData.imageUrl
      }).subscribe({
        next: (res: any) => {
          this.toast.success(res?.message || 'User created successfully!');
          this.showAddEditModal = false;
          this.loadUsers();
          this.loadStats();
        },
        error: (err) => {
          const msg = err.error?.message || 'Failed to create user.';
          this.toast.error(msg);
        }
      });
    } else {
      this.userService.update(this.editingUserId, {
        id: this.editingUserId,
        fullName: this.userFormData.fullName.trim(),
        email: this.userFormData.email.trim().toLowerCase(),
        phoneNumber: this.userFormData.phoneNumber?.trim() || '',
        roleId: Number(this.userFormData.roleId),
        isActive: this.userFormData.isActive,
        emailVerified: this.userFormData.emailVerified,
        imageUrl: this.userFormData.imageUrl
      }).subscribe({
        next: (res: any) => {
          this.toast.success(res?.message || 'User updated successfully!');
          this.showAddEditModal = false;
          this.loadUsers();
          this.loadStats();
        },
        error: (err) => {
          const msg = err.error?.message || 'Failed to update user.';
          this.toast.error(msg);
        }
      });
    }
  }

  toggleStatus(user: UserMaster): void {
    const currentLoggedUser = this.authService.currentUserValue;
    if (currentLoggedUser?.email === user.email) {
      this.toast.error('You cannot change the active status of your own account.');
      return;
    }

    this.userService.toggleStatus(user.id).subscribe({
      next: (res: any) => {
        user.isActive = res.isActive !== undefined ? res.isActive : !user.isActive;
        this.toast.success(`User ${user.fullName} is now ${user.isActive ? 'Active' : 'Inactive'}.`);
        this.loadStats();
        this.cdr.detectChanges();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to toggle status.';
        this.toast.error(msg);
      }
    });
  }

  openDeleteModal(user: UserMaster): void {
    const currentLoggedUser = this.authService.currentUserValue;
    if (currentLoggedUser?.email === user.email) {
      this.toast.error('You cannot delete your own logged-in account.');
      return;
    }
    this.userToDelete = user;
    this.showDeleteModal = true;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (!this.userToDelete) return;
    this.isDeleting = true;

    this.userService.delete(this.userToDelete.id).subscribe({
      next: (res: any) => {
        this.isDeleting = false;
        this.toast.success(res?.message || 'User deleted successfully.');
        this.showDeleteModal = false;
        this.userToDelete = null;
        this.loadUsers();
        this.loadStats();
      },
      error: (err) => {
        this.isDeleting = false;
        const msg = err.error?.message || 'Failed to delete user.';
        this.toast.error(msg);
      }
    });
  }

  openResetPasswordModal(user: UserMaster): void {
    this.userToResetPassword = user;
    this.resetPasswordModel = {
      newPassword: '',
      confirmPassword: ''
    };
    this.showResetPasswordModal = true;
    this.cdr.detectChanges();
  }

  saveResetPassword(): void {
    if (!this.userToResetPassword) return;

    if (!this.resetPasswordModel.newPassword) {
      this.toast.error('Please enter New Password');
      return;
    }

    if (this.resetPasswordModel.newPassword.length < 6) {
      this.toast.error('New Password must be at least 6 characters');
      return;
    }

    if (!this.resetPasswordModel.confirmPassword) {
      this.toast.error('Please enter Confirm Password');
      return;
    }

    if (this.resetPasswordModel.newPassword !== this.resetPasswordModel.confirmPassword) {
      this.toast.error('New Password and Confirm Password do not match');
      return;
    }

    this.userService.resetPassword(this.userToResetPassword.id, this.resetPasswordModel).subscribe({
      next: (res: any) => {
        this.toast.success(res?.message || `Password reset for ${this.userToResetPassword?.fullName}!`);
        this.showResetPasswordModal = false;
        this.userToResetPassword = null;
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to reset password.';
        this.toast.error(msg);
      }
    });
  }

  getRoleBadgeClass(roleName?: string): string {
    const r = (roleName || '').toLowerCase();
    if (r === 'superadmin') return 'badge-superadmin';
    if (r === 'admin') return 'badge-admin';
    if (r === 'customer') return 'badge-customer';
    if (r.includes('delivery')) return 'badge-delivery';
    if (r.includes('restaurant')) return 'badge-owner';
    return 'badge-secondary';
  }

  getRoleIcon(roleName?: string): string {
    const r = (roleName || '').toLowerCase();
    if (r === 'superadmin') return 'fa-solid fa-crown';
    if (r === 'admin') return 'fa-solid fa-shield-halved';
    if (r === 'customer') return 'fa-solid fa-user';
    if (r.includes('delivery')) return 'fa-solid fa-person-biking';
    if (r.includes('restaurant')) return 'fa-solid fa-store';
    return 'fa-solid fa-user-tag';
  }
}

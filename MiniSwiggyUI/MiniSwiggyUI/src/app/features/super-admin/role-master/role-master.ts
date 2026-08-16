import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { RoleService } from '../../../core/services/role.service';
import { ToastService } from '../../../core/services/toast.service';
import { RoleMaster } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-role-master',
  templateUrl: './role-master.html',
  styleUrl: './role-master.css',
  standalone: false
})
export class RoleMasterComponent implements OnInit {
  private roleService = inject(RoleService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  roles: RoleMaster[] = [];
  isLoading = true;

  showModal = false;
  isEditing = false;
  editingRoleId = 0;

  showDeleteModal = false;
  roleToDelete: RoleMaster | null = null;
  isDeleting = false;

  formData = {
    name: '',
    description: ''
  };

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.roleService.getAll().pipe(
      timeout(5000),
      catchError(() => of([]))
    ).subscribe({
      next: (data) => {
        this.roles = data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.roles = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openCreateModal(): void {
    this.isEditing = false;
    this.editingRoleId = 0;
    this.formData = {
      name: '',
      description: ''
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  openEditModal(role: RoleMaster): void {
    this.isEditing = true;
    this.editingRoleId = role.id;
    this.formData = {
      name: role.name,
      description: role.description || ''
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  saveRole(): void {
    if (!this.formData.name || !this.formData.name.trim()) {
      this.toast.error('Please enter Role Name');
      return;
    }

    if (this.formData.name.trim().length < 2) {
      this.toast.error('Role Name must be at least 2 characters');
      return;
    }

    if (this.formData.name.trim().length > 50) {
      this.toast.error('Role Name cannot exceed 50 characters');
      return;
    }

    if (!this.isEditing) {
      this.roleService.create({
        name: this.formData.name.trim(),
        description: this.formData.description?.trim()
      }).subscribe({
        next: (res: any) => {
          this.toast.success(res?.message || 'Role created successfully!');
          this.showModal = false;
          this.loadRoles();
        },
        error: (err) => {
          const msg = err.error?.message || 'Failed to create role.';
          this.toast.error(msg);
        }
      });
    } else {
      this.roleService.update(this.editingRoleId, {
        id: this.editingRoleId,
        name: this.formData.name.trim(),
        description: this.formData.description?.trim()
      }).subscribe({
        next: (res: any) => {
          this.toast.success(res?.message || 'Role updated successfully!');
          this.showModal = false;
          this.loadRoles();
        },
        error: (err) => {
          const msg = err.error?.message || 'Failed to update role.';
          this.toast.error(msg);
        }
      });
    }
  }

  toggleStatus(role: RoleMaster): void {
    if (this.isBuiltInRole(role.name)) {
      this.toast.error(`System role '${role.name}' is a core system role and cannot be deactivated.`);
      return;
    }
    role.isActive = role.isActive === false ? true : false;
    this.toast.success(`Role '${role.name}' status updated to ${role.isActive ? 'Active' : 'Inactive'}.`);
    this.cdr.detectChanges();
  }

  isBuiltInRole(name: string): boolean {
    const builtIn = ['superadmin', 'admin', 'customer', 'deliverypartner', 'deliveryboy', 'restaurantowner'];
    return builtIn.includes(name.toLowerCase());
  }

  getRoleIcon(name: string): string {
    const r = name.toLowerCase();
    if (r === 'superadmin') return 'fa-solid fa-crown text-warning';
    if (r === 'admin') return 'fa-solid fa-shield-halved text-primary';
    if (r === 'customer') return 'fa-solid fa-user text-success';
    if (r.includes('delivery')) return 'fa-solid fa-person-biking text-warning';
    if (r.includes('restaurant')) return 'fa-solid fa-store text-purple';
    return 'fa-solid fa-id-badge text-muted';
  }
}

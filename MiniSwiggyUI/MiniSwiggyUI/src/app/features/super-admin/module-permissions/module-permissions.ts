import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../../core/services/user.service';
import { PermissionService } from '../../../core/services/permission.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserMaster, UserModulePermission } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

export const ALL_SYSTEM_MODULES: UserModulePermission[] = [
  // Customer Modules
  { moduleKey: 'restaurants', moduleName: 'Explore Restaurants', moduleCategory: 'Customer', routePath: '/restaurant', iconClass: 'fa-solid fa-store', isAllowed: true, userId: 0 },
  { moduleKey: 'cart', moduleName: 'Shopping Cart', moduleCategory: 'Customer', routePath: '/cart', iconClass: 'fa-solid fa-basket-shopping', isAllowed: true, userId: 0 },
  { moduleKey: 'orders', moduleName: 'My Orders', moduleCategory: 'Customer', routePath: '/my-orders', iconClass: 'fa-solid fa-clock-rotate-left', isAllowed: true, userId: 0 },
  { moduleKey: 'wishlist', moduleName: 'Wishlist & Favorites', moduleCategory: 'Customer', routePath: '/wishlist', iconClass: 'fa-solid fa-heart', isAllowed: true, userId: 0 },
  { moduleKey: 'addresses', moduleName: 'Saved Addresses', moduleCategory: 'Customer', routePath: '/addresses', iconClass: 'fa-solid fa-location-dot', isAllowed: true, userId: 0 },

  // Admin Modules
  { moduleKey: 'admin_dashboard', moduleName: 'Admin Dashboard', moduleCategory: 'Admin', routePath: '/dashboard', iconClass: 'fa-solid fa-chart-line', isAllowed: true, userId: 0 },
  { moduleKey: 'restaurant_master', moduleName: 'Restaurant Master', moduleCategory: 'Admin', routePath: '/admin/restaurants', iconClass: 'fa-solid fa-store', isAllowed: true, userId: 0 },
  { moduleKey: 'category_master', moduleName: 'Category Master', moduleCategory: 'Admin', routePath: '/categories', iconClass: 'fa-solid fa-layer-group', isAllowed: true, userId: 0 },
  { moduleKey: 'food_master', moduleName: 'Food Item Master', moduleCategory: 'Admin', routePath: '/food-item', iconClass: 'fa-solid fa-burger', isAllowed: true, userId: 0 },
  { moduleKey: 'order_master', moduleName: 'Order Master', moduleCategory: 'Admin', routePath: '/admin/orders', iconClass: 'fa-solid fa-boxes-packing', isAllowed: true, userId: 0 },
  { moduleKey: 'coupon_master', moduleName: 'Coupon & Promo Master', moduleCategory: 'Admin', routePath: '/coupons', iconClass: 'fa-solid fa-ticket', isAllowed: true, userId: 0 },

  // Super Admin Modules
  { moduleKey: 'superadmin_dashboard', moduleName: 'Master Command Center', moduleCategory: 'SuperAdmin', routePath: '/superadmin/dashboard', iconClass: 'fa-solid fa-gauge-high', isAllowed: true, userId: 0 },
  { moduleKey: 'user_master', moduleName: 'User Master Directory', moduleCategory: 'SuperAdmin', routePath: '/superadmin/users', iconClass: 'fa-solid fa-users-gear', isAllowed: true, userId: 0 },
  { moduleKey: 'role_master', moduleName: 'Role Master', moduleCategory: 'SuperAdmin', routePath: '/superadmin/roles', iconClass: 'fa-solid fa-shield-halved', isAllowed: true, userId: 0 },
  { moduleKey: 'fleet_master', moduleName: 'Delivery Fleet Master', moduleCategory: 'SuperAdmin', routePath: '/superadmin/delivery-partners', iconClass: 'fa-solid fa-person-biking', isAllowed: true, userId: 0 },
  { moduleKey: 'review_master', moduleName: 'Review & Rating Master', moduleCategory: 'SuperAdmin', routePath: '/superadmin/reviews', iconClass: 'fa-solid fa-comments', isAllowed: true, userId: 0 },
  { moduleKey: 'permission_master', moduleName: 'User Module Access Master', moduleCategory: 'SuperAdmin', routePath: '/superadmin/permissions', iconClass: 'fa-solid fa-key', isAllowed: true, userId: 0 },
  { moduleKey: 'delivery_console', moduleName: 'Rider Delivery Console', moduleCategory: 'Delivery', routePath: '/delivery-partner/dashboard', iconClass: 'fa-solid fa-motorcycle', isAllowed: true, userId: 0 }
];

@Component({
  selector: 'app-module-permissions',
  templateUrl: './module-permissions.html',
  styleUrl: './module-permissions.css',
  standalone: false
})
export class ModulePermissionsComponent implements OnInit {
  private userService = inject(UserService);
  private permissionService = inject(PermissionService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  users: UserMaster[] = [];
  selectedUser: UserMaster | null = null;
  userSearchQuery = '';
  
  permissions: UserModulePermission[] = [];
  isLoadingUsers = true;
  isLoadingPermissions = false;
  isSaving = false;

  get filteredUsers(): UserMaster[] {
    if (!this.userSearchQuery.trim()) return this.users;
    const q = this.userSearchQuery.toLowerCase().trim();
    return this.users.filter(u =>
      u.fullName.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q) ||
      u.roleName.toLowerCase().includes(q)
    );
  }

  get customerModules(): UserModulePermission[] {
    return this.permissions.filter(p => p.moduleCategory === 'Customer');
  }

  get adminModules(): UserModulePermission[] {
    return this.permissions.filter(p => p.moduleCategory === 'Admin');
  }

  get superAdminModules(): UserModulePermission[] {
    return this.permissions.filter(p => p.moduleCategory === 'SuperAdmin' || p.moduleCategory === 'Delivery');
  }

  get enabledCount(): number {
    return this.permissions.filter(p => p.isAllowed).length;
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoadingUsers = true;
    this.cdr.detectChanges();

    this.userService.getAll().pipe(
      timeout(5000),
      catchError(() => of([]))
    ).subscribe((list: UserMaster[]) => {
      this.users = list || [];
      this.isLoadingUsers = false;

      if (this.users.length > 0) {
        this.selectUser(this.users[0]);
      }
      this.cdr.detectChanges();
    });
  }

  selectUser(user: UserMaster): void {
    this.selectedUser = user;
    this.loadUserPermissions(user.id);
  }

  onUserDropdownChange(userId: any): void {
    const id = Number(userId);
    const u = this.users.find(x => x.id === id);
    if (u) {
      this.selectUser(u);
    }
  }

  loadUserPermissions(userId: number): void {
    this.isLoadingPermissions = true;
    this.cdr.detectChanges();

    this.permissionService.getUserPermissions(userId).pipe(
      timeout(4000),
      catchError(() => of([]))
    ).subscribe(perms => {
      const roleName = this.selectedUser?.roleName?.toLowerCase() || 'customer';

      // Always populate the full list of 17 system modules
      this.permissions = ALL_SYSTEM_MODULES.map(m => {
        const found = (perms || []).find(p => p.moduleKey.toLowerCase() === m.moduleKey.toLowerCase());
        let allowed = false;

        if (found) {
          allowed = found.isAllowed;
        } else {
          // Default role rules
          if (roleName.includes('superadmin')) {
            allowed = true;
          } else if (roleName.includes('admin')) {
            allowed = m.moduleCategory === 'Admin' || m.moduleCategory === 'SuperAdmin' || m.moduleCategory === 'Customer';
          } else if (roleName.includes('delivery')) {
            allowed = m.moduleKey === 'delivery_console' || m.moduleKey === 'addresses';
          } else {
            allowed = m.moduleCategory === 'Customer';
          }
        }

        return {
          ...m,
          userId: userId,
          isAllowed: allowed
        };
      });

      this.isLoadingPermissions = false;
      this.cdr.detectChanges();
    });
  }

  togglePermission(perm: UserModulePermission): void {
    perm.isAllowed = !perm.isAllowed;
    this.cdr.detectChanges();
  }

  setAll(allowed: boolean): void {
    this.permissions.forEach(p => p.isAllowed = allowed);
    this.toast.info(`All modules marked as ${allowed ? 'Allowed' : 'Denied'}. Click Save to commit.`);
    this.cdr.detectChanges();
  }

  resetToDefaults(): void {
    if (!this.selectedUser) return;
    if (!confirm(`Reset all module access permissions for ${this.selectedUser.fullName} to default ${this.selectedUser.roleName} role rules?`)) return;

    this.isLoadingPermissions = true;
    this.permissionService.resetUserPermissions(this.selectedUser.id).subscribe({
      next: () => {
        this.toast.success(`Permissions for ${this.selectedUser?.fullName} reset to defaults!`);
        this.loadUserPermissions(this.selectedUser!.id);
      },
      error: () => {
        this.toast.error('Failed to reset permissions.');
        this.isLoadingPermissions = false;
        this.cdr.detectChanges();
      }
    });
  }

  savePermissions(): void {
    if (!this.selectedUser) return;

    this.isSaving = true;
    const request = {
      userId: this.selectedUser.id,
      permissions: this.permissions.map(p => ({
        moduleKey: p.moduleKey,
        isAllowed: p.isAllowed
      }))
    };

    this.permissionService.updateUserPermissions(request).subscribe({
      next: () => {
        this.isSaving = false;
        this.toast.success(`Access permissions for '${this.selectedUser?.fullName}' saved `);
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isSaving = false;
        this.toast.error('Error saving permissions');
        this.cdr.detectChanges();
      }
    });
  }
}

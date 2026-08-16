import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of } from 'rxjs';
import { UserModulePermission, UpdateUserPermissionsRequest } from '../models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'https://localhost:7241/api/ModulePermission';
  private storageKey = 'miniswiggy_user_permissions';

  private myPermissionsSubject = new BehaviorSubject<Map<string, boolean>>(new Map());
  myPermissions$ = this.myPermissionsSubject.asObservable();

  constructor() {
    this.restoreCachedPermissions();

    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.loadMyPermissions();
      } else {
        this.clearCache();
      }
    });
  }

  private restoreCachedPermissions(): void {
    try {
      const cached = localStorage.getItem(this.storageKey);
      if (cached) {
        const obj = JSON.parse(cached);
        const map = new Map<string, boolean>();
        Object.keys(obj).forEach(k => map.set(k.toLowerCase(), !!obj[k]));
        this.myPermissionsSubject.next(map);
      }
    } catch {
      // Ignore parse error
    }
  }

  private saveToCache(map: Map<string, boolean>): void {
    try {
      const obj: Record<string, boolean> = {};
      map.forEach((val, key) => { obj[key] = val; });
      localStorage.setItem(this.storageKey, JSON.stringify(obj));
    } catch {
      // Ignore storage error
    }
  }

  private clearCache(): void {
    localStorage.removeItem(this.storageKey);
    this.myPermissionsSubject.next(new Map());
  }

  /**
   * Fetches real permissions for the current logged-in user directly from SQL Database.
   */
  loadMyPermissions(): void {
    if (!this.authService.isLoggedIn()) return;

    this.http.get<UserModulePermission[]>(`${this.apiUrl}/my-permissions`)
      .pipe(
        catchError(() => of([]))
      )
      .subscribe((permissions) => {
        const map = new Map<string, boolean>();
        if (permissions && permissions.length > 0) {
          permissions.forEach(p => map.set(p.moduleKey.toLowerCase(), p.isAllowed));
        } else {
          // If empty in DB, populate based on role
          const isSuperOrAdmin = this.authService.isSuperAdmin() || this.authService.isAdmin();
          if (isSuperOrAdmin) {
            ['restaurants', 'cart', 'orders', 'wishlist', 'addresses', 'admin_dashboard', 'superadmin_dashboard', 'user_master', 'role_master', 'permission_master', 'restaurant_master', 'category_master', 'food_master', 'order_master', 'coupon_master', 'fleet_master', 'review_master'].forEach(k => map.set(k, true));
          }
        }
        this.myPermissionsSubject.next(map);
        this.saveToCache(map);
      });
  }

  /**
   * Checks if the active user is allowed to access/view a given module.
   */
  canAccess(moduleKey: string): boolean {
    if (!this.authService.isLoggedIn()) {
      return moduleKey === 'restaurants';
    }

    const key = moduleKey.toLowerCase();
    const permMap = this.myPermissionsSubject.value;

    // If permission exists in real DB map, respect the DB value
    if (permMap && permMap.has(key)) {
      return !!permMap.get(key);
    }

    // Role-based immediate fallback
    if (this.authService.isSuperAdmin() || this.authService.isAdmin()) {
      return true; // Full access for SuperAdmin & Admin
    }

    if (this.authService.isDeliveryPartner()) {
      return key === 'delivery_console' || key === 'addresses';
    }

    // Default customer
    return ['restaurants', 'cart', 'orders', 'wishlist', 'addresses'].includes(key);
  }

  /**
   * SuperAdmin: Get permissions for a specific user from SQL Database.
   */
  getUserPermissions(userId: number): Observable<UserModulePermission[]> {
    return this.http.get<UserModulePermission[]>(`${this.apiUrl}/user/${userId}`);
  }

  /**
   * SuperAdmin: Save modified user permissions directly to SQL Database.
   */
  updateUserPermissions(request: UpdateUserPermissionsRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/update`, request).pipe(
      tap(() => {
        // If updating currently logged in user, refresh live sidebar
        const currentUserId = (this.authService.currentUserValue as any)?.id;
        if (!currentUserId || currentUserId === request.userId) {
          const map = new Map<string, boolean>();
          request.permissions.forEach(p => map.set(p.moduleKey.toLowerCase(), p.isAllowed));
          this.myPermissionsSubject.next(map);
          this.saveToCache(map);
        }
      })
    );
  }

  /**
   * SuperAdmin: Reset user permissions to role defaults in SQL Database.
   */
  resetUserPermissions(userId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset/${userId}`, {}).pipe(
      tap(() => {
        this.loadMyPermissions();
      })
    );
  }
}

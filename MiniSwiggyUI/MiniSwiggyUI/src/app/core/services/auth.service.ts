import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, User } from '../models';

export interface RegisterRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/Auth';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  currentUser$ = this.currentUserSubject.asObservable();

  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  register(model: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, model);
  }

  login(model: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, model).pipe(
      tap((res: any) => {
        if (res.token) {
          localStorage.setItem('token', res.token);
          const rawImg = res.profileImageUrl || res.imageUrl || res.ImageUrl;
          const user: User = {
            fullName: res.fullName || 'User',
            email: res.email || model.email,
            phoneNumber: res.phoneNumber || '',
            role: res.role || this.decodeTokenRole(res.token),
            profileImageUrl: rawImg
          };
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
      })
    );
  }

  uploadProfileImage(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('File', file);
    return this.http.post(`${this.apiUrl}/upload-profile-image`, formData);
  }

  updateProfileBackend(model: { fullName: string; phoneNumber: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/profile`, model);
  }

  getProfile(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/profile`);
  }

  changePassword(model: { oldPassword?: string; newPassword: string; confirmPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, model);
  }

  updateProfile(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('miniswiggy_delivery_profile');
    localStorage.removeItem('miniswiggy_user_image');
    this.currentUserSubject.next(null);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  isSuperAdmin(): boolean {
    const user = this.currentUserSubject.value;
    if (!user) return false;
    const rawRole = user.role;
    const roleStr = typeof rawRole === 'string' ? rawRole : (rawRole as any)?.name || (rawRole as any)?.RoleName || '';
    return roleStr.trim().toLowerCase() === 'superadmin';
  }

  isAdmin(): boolean {
    const user = this.currentUserSubject.value;
    if (!user) return false;
    const rawRole = user.role;
    const roleStr = typeof rawRole === 'string' ? rawRole : (rawRole as any)?.name || (rawRole as any)?.RoleName || '';
    const lower = roleStr.trim().toLowerCase();
    return lower === 'admin' || lower === 'superadmin';
  }

  isDeliveryPartner(): boolean {
    const user = this.currentUserSubject.value;
    if (!user) return false;
    const rawRole = user.role;
    const roleStr = typeof rawRole === 'string' ? rawRole : (rawRole as any)?.name || (rawRole as any)?.RoleName || '';
    const lower = roleStr.toLowerCase();
    return lower.includes('delivery');
  }

  private getUserFromStorage(): User | null {
    const stored = localStorage.getItem('user');
    if (!stored) return null;
    try {
      return JSON.parse(stored);
    } catch {
      return null;
    }
  }

  private decodeTokenRole(token: string): string {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Customer';
    } catch {
      return 'Customer';
    }
  }
}

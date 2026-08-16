import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserMaster, UserStats, RoleMaster } from '../models';

export interface CreateUserPayload {
  fullName: string;
  email: string;
  phoneNumber: string;
  password?: string;
  roleId: number;
  isActive: boolean;
  imageUrl?: string;
}

export interface UpdateUserPayload {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  roleId: number;
  isActive: boolean;
  emailVerified: boolean;
  imageUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/User';

  getAll(search?: string, role?: string): Observable<UserMaster[]> {
    let params = new HttpParams();
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }
    if (role && role !== 'All') {
      params = params.set('role', role);
    }
    return this.http.get<UserMaster[]>(this.apiUrl, { params });
  }

  getStats(): Observable<UserStats> {
    return this.http.get<UserStats>(`${this.apiUrl}/stats`);
  }

  getRoles(): Observable<RoleMaster[]> {
    return this.http.get<RoleMaster[]>(`${this.apiUrl}/roles`);
  }

  getById(id: number): Observable<UserMaster> {
    return this.http.get<UserMaster>(`${this.apiUrl}/${id}`);
  }

  create(payload: CreateUserPayload): Observable<any> {
    return this.http.post(this.apiUrl, payload);
  }

  update(id: number, payload: UpdateUserPayload): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  toggleStatus(id: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/toggle-status`, {});
  }

  resetPassword(id: number, payload: { newPassword: string; confirmPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/reset-password`, payload);
  }
}

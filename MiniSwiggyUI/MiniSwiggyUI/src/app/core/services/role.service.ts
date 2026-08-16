import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RoleMaster } from '../models';

export interface CreateRolePayload {
  name: string;
  description?: string;
}

export interface UpdateRolePayload {
  id: number;
  name: string;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/Role';

  getAll(): Observable<RoleMaster[]> {
    return this.http.get<RoleMaster[]>(this.apiUrl);
  }

  getById(id: number): Observable<RoleMaster> {
    return this.http.get<RoleMaster>(`${this.apiUrl}/${id}`);
  }

  create(payload: CreateRolePayload): Observable<any> {
    return this.http.post(this.apiUrl, payload);
  }

  update(id: number, payload: UpdateRolePayload): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}

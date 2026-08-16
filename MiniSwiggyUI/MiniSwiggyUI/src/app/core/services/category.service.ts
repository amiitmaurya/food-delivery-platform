import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/Category';

  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  getById(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`);
  }

  create(category: any): Observable<any> {
    const payload = {
      restaurantId: Number(category.restaurantId) || 1,
      name: category.name,
      description: category.description || '',
      imageUrl: category.imageUrl || '',
      displayOrder: Number(category.displayOrder) || 1,
      isActive: category.isActive ?? true
    };
    return this.http.post(this.apiUrl, payload);
  }

  update(id: number, category: any): Observable<any> {
    const payload = {
      id: id,
      restaurantId: Number(category.restaurantId) || 1,
      name: category.name,
      description: category.description || '',
      imageUrl: category.imageUrl || '',
      displayOrder: Number(category.displayOrder) || 1,
      isActive: category.isActive ?? true
    };
    return this.http.put(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  uploadImage(id: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('Image', file);
    return this.http.post(`${this.apiUrl}/${id}/upload-image`, formData);
  }
}

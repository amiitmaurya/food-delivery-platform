import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Restaurant, CreateRestaurantRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class RestaurantService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/Restaurant';

  getAll(): Observable<Restaurant[]> {
    return this.http.get<Restaurant[]>(this.apiUrl);
  }

  getById(id: number): Observable<Restaurant> {
    return this.http.get<Restaurant>(`${this.apiUrl}/${id}`);
  }

  create(restaurant: CreateRestaurantRequest): Observable<any> {
    return this.http.post(this.apiUrl, restaurant);
  }

  update(id: number, restaurant: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, restaurant);
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

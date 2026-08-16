import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FoodItem, CreateFoodItemRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class FoodItemService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/FoodItem';

  getAll(): Observable<FoodItem[]> {
    return this.http.get<FoodItem[]>(this.apiUrl);
  }

  getByCategory(categoryId: number): Observable<FoodItem[]> {
    return this.http.get<FoodItem[]>(`${this.apiUrl}/category/${categoryId}`);
  }

  getById(id: number): Observable<FoodItem> {
    return this.http.get<FoodItem>(`${this.apiUrl}/${id}`);
  }

  create(foodItem: CreateFoodItemRequest): Observable<any> {
    return this.http.post(this.apiUrl, foodItem);
  }

  update(foodItem: any): Observable<any> {
    return this.http.put(this.apiUrl, foodItem);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  searchFoods(searchQuery?: string, categoryId?: number, isVegetarian?: boolean): Observable<FoodItem[]> {
    let params = new HttpParams();
    if (searchQuery) params = params.set('searchQuery', searchQuery);
    if (categoryId) params = params.set('categoryId', categoryId.toString());
    if (isVegetarian !== undefined && isVegetarian !== null) params = params.set('isVegetarian', isVegetarian.toString());

    return this.http.get<FoodItem[]>(`${this.apiUrl}/search`, { params });
  }

  uploadImage(id: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('File', file);
    return this.http.post(`${this.apiUrl}/${id}/upload-image`, formData);
  }
}

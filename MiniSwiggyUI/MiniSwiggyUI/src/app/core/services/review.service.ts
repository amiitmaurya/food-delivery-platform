import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

import {
  Review,
  AddReviewRequest
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {

  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:7241/api/Review';

  getByRestaurant(restaurantId: number): Observable<Review[]> {
    return this.getRestaurantReviewDetails(restaurantId).pipe(
      map(res => res.reviews)
    );
  }

  getRestaurantReviewDetails(restaurantId: number): Observable<{ averageRating: number; totalReviews: number; reviews: Review[] }> {
    return this.http.get<any>(`${this.apiUrl}/restaurant/${restaurantId}`).pipe(
      map(res => {
        if (!res) return { averageRating: 0, totalReviews: 0, reviews: [] };
        const rawList = res.reviews || (Array.isArray(res) ? res : []);
        const formattedReviews = rawList.map((r: any) => ({
          id: r.id || r.Id,
          userId: r.userId || r.UserId,
          userName: r.userName || r.UserName || 'Customer',
          userImageUrl: r.userImageUrl || r.UserImageUrl,
          foodItemId: r.foodItemId || r.FoodItemId,
          foodName: r.foodName || r.FoodName,
          foodImageUrl: r.foodImageUrl || r.FoodImageUrl || r.image || r.Image,
          restaurantName: r.restaurantName || r.RestaurantName,
          restaurantImageUrl: r.restaurantImageUrl || r.RestaurantImageUrl,
          rating: r.rating || r.Rating || 5,
          comment: r.comment || r.Comment || '',
          createdOn: r.createdOn || r.CreatedOn || r.createdAt || r.CreatedAt
        }));
        return {
          averageRating: res.averageRating ?? res.AverageRating ?? 0,
          totalReviews: res.totalReviews ?? res.TotalReviews ?? formattedReviews.length,
          reviews: formattedReviews
        };
      })
    );
  }

  addReview(review: AddReviewRequest): Observable<any> {
    return this.http.post(this.apiUrl, review);
  }

  updateReview(id: number, data: { rating: number; comment: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  deleteReview(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}

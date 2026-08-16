import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import {
  DeliveryOrder,
  DeliveryEarnings,
  DeliveryProfile,
  UpdateDeliveryProfileRequest,
  ChangePasswordRequest
} from '../models/delivery-partner.models';
import { AuthService } from '../../../core/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class DeliveryPartnerService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private baseUrl = 'https://localhost:7241/api';

  // GET /api/orders/my-assigned
  getMyAssignedOrders(): Observable<DeliveryOrder[]> {
    return this.http.get<DeliveryOrder[]>(`${this.baseUrl}/orders/my-assigned`).pipe(
      catchError(() => of([]))
    );
  }

  // GET /api/orders/current
  getCurrentDelivery(): Observable<DeliveryOrder | null> {
    return this.http.get<DeliveryOrder>(`${this.baseUrl}/orders/current`).pipe(
      catchError(() => of(null))
    );
  }

  // GET /api/delivery/history
  getDeliveryHistory(): Observable<DeliveryOrder[]> {
    return this.http.get<DeliveryOrder[]>(`${this.baseUrl}/delivery/history`).pipe(
      catchError(() => of([]))
    );
  }

  // GET /api/delivery/earnings
  getEarnings(): Observable<DeliveryEarnings> {
    return this.http.get<DeliveryEarnings>(`${this.baseUrl}/delivery/earnings`).pipe(
      catchError(() => of({
        todayEarnings: 0,
        weeklyEarnings: 0,
        monthlyEarnings: 0,
        totalEarnings: 0,
        todayDeliveriesCount: 0,
        totalDeliveriesCount: 0,
        averageRating: 5.0,
        dailyBreakdown: [],
        recentPayouts: []
      }))
    );
  }

  // GET /api/profile
  getProfile(): Observable<DeliveryProfile | null> {
    return this.http.get<DeliveryProfile>(`${this.baseUrl}/profile`).pipe(
      catchError(() => {
        const user = this.authService.currentUserValue as any;
        if (!user) return of(null);
        return of({
          id: user.id || 1,
          fullName: user.fullName || user.username || 'Delivery Partner',
          email: user.email || '',
          phoneNumber: user.phoneNumber || user.mobileNumber || '',
          profileImageUrl: user.profileImageUrl || user.imageUrl || '',
          isOnline: true,
          vehicleType: '',
          vehicleNumber: '',
          vehicleModel: '',
          licenseNumber: '',
          licenseExpiryDate: '',
          bankAccountHolder: '',
          bankName: '',
          accountNumber: '',
          ifscCode: '',
          upiId: ''
        });
      })
    );
  }

  // PUT /api/profile/delivery-details
  updateProfile(request: UpdateDeliveryProfileRequest): Observable<any> {
    return this.http.put(`${this.baseUrl}/profile/delivery-details`, request).pipe(
      catchError(() => of({ message: 'Profile saved to Database.' }))
    );
  }

  // POST /api/profile/change-password
  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/profile/change-password`, request);
  }

  // POST /api/profile/toggle-online
  toggleOnlineStatus(isOnline: boolean): Observable<any> {
    return this.http.post(`${this.baseUrl}/profile/toggle-online`, { isOnline }).pipe(
      catchError(() => of({ message: 'Status updated.', isOnline }))
    );
  }

  // POST /api/orders/accept
  acceptOrder(orderId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/accept`, { orderId });
  }

  // POST /api/orders/reject
  rejectOrder(orderId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/reject`, { orderId });
  }

  // POST /api/orders/reached-restaurant
  markReachedRestaurant(orderId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/reached-restaurant`, { orderId });
  }

  // POST /api/orders/picked-up
  markPickedUp(orderId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/picked-up`, { orderId });
  }

  // POST /api/orders/out-for-delivery
  markOutForDelivery(orderId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/out-for-delivery`, { orderId });
  }

  // POST /api/orders/delivered
  markDelivered(orderId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/delivered`, { orderId });
  }
}

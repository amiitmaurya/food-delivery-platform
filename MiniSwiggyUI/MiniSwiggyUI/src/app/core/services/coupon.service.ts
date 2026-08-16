import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Coupon } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CouponService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/Coupon';

  private mapFromBackend(c: any): Coupon {
    return {
      id: Number(c.id || Date.now()),
      code: String(c.code || '').toUpperCase(),
      discountPercentage: Number(c.discountValue || c.discountPercentage || 0),
      minOrderAmount: Number(c.minimumOrderAmount || c.minOrderAmount || 0),
      maxDiscountAmount: Number(c.maximumDiscount || c.maxDiscountAmount || 0),
      expiryDate: c.expiryDate || new Date().toISOString(),
      isActive: Boolean(c.isActive ?? true)
    };
  }

  private mapToBackend(coupon: Omit<Coupon, 'id'>) {
    return {
      code: coupon.code.toUpperCase(),
      description: `Discount ${coupon.discountPercentage}% OFF`,
      discountType: 'Percentage',
      discountValue: Number(coupon.discountPercentage),
      minimumOrderAmount: Number(coupon.minOrderAmount),
      maximumDiscount: Number(coupon.maxDiscountAmount),
      startDate: new Date().toISOString(),
      expiryDate: coupon.expiryDate ? new Date(coupon.expiryDate).toISOString() : new Date().toISOString(),
      usageLimit: 500,
      isActive: Boolean(coupon.isActive ?? true)
    };
  }

  getAll(): Observable<Coupon[]> {
    return this.http.get<any[]>(this.apiUrl).pipe(
      map(list => Array.isArray(list) ? list.map(c => this.mapFromBackend(c)) : [])
    );
  }

  applyCoupon(code: string, orderAmount: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/apply`, {
      couponCode: (code || '').trim().toUpperCase(),
      cartTotal: Number(orderAmount)
    });
  }

  create(coupon: Omit<Coupon, 'id'>): Observable<any> {
    const payload = this.mapToBackend(coupon);
    return this.http.post(this.apiUrl, payload);
  }

  update(id: number, coupon: Coupon): Observable<any> {
    const payload = { id, ...this.mapToBackend(coupon) };
    return this.http.put(this.apiUrl, payload);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}


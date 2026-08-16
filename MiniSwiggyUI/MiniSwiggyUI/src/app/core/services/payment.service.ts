import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PaymentTransaction {
  id: number;
  orderId: number;
  orderNumber?: string;
  userId?: number;
  customerName?: string;
  customerEmail?: string;
  amount: number;
  paymentMethod: number; // 1 = CashOnDelivery, 2 = UPI, etc.
  paymentMethodName?: string;
  paymentStatus: number; // 1 = Pending, 2 = Paid, 3 = Failed, 4 = Refunded
  paymentStatusName?: string;
  transactionId?: string;
  gatewayOrderId?: string;
  createdOn: string;
  paidOn?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/Payment';

  private normalizeList(list: any[]): PaymentTransaction[] {
    if (!Array.isArray(list)) return [];
    return list.map(item => {
      let createdOn = item.createdOn || new Date().toISOString();
      if (typeof createdOn === 'string' && !createdOn.endsWith('Z') && !createdOn.includes('+') && !createdOn.includes('z')) {
        createdOn = createdOn + 'Z';
      }
      return {
        ...item,
        createdOn: createdOn
      };
    });
  }

  // Get current customer's personal payment history
  getMyPayments(): Observable<PaymentTransaction[]> {
    return this.http.get<PaymentTransaction[]>(`${this.apiUrl}/my-payments`).pipe(
      map(list => this.normalizeList(list))
    );
  }

  // Get all platform transactions (Admin / SuperAdmin)
  getAllPayments(): Observable<PaymentTransaction[]> {
    return this.http.get<PaymentTransaction[]>(this.apiUrl).pipe(
      map(list => this.normalizeList(list))
    );
  }

  // Get by ID
  getById(id: number): Observable<PaymentTransaction> {
    return this.http.get<PaymentTransaction>(`${this.apiUrl}/${id}`).pipe(
      map(item => {
        let createdOn = item.createdOn || new Date().toISOString();
        if (typeof createdOn === 'string' && !createdOn.endsWith('Z') && !createdOn.includes('+') && !createdOn.includes('z')) {
          createdOn = createdOn + 'Z';
        }
        return { ...item, createdOn };
      })
    );
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Order, PlaceOrderRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/Order';

  private mapOrder(order: any): Order {
    const statusMap: Record<number | string, string> = {
      1: 'Placed',
      2: 'Confirmed',
      3: 'Preparing',
      4: 'Out for Delivery',
      5: 'Delivered',
      6: 'Cancelled',
      'Pending': 'Placed',
      'Confirmed': 'Confirmed',
      'Preparing': 'Preparing',
      'OutForDelivery': 'Out for Delivery',
      'Delivered': 'Delivered',
      'Cancelled': 'Cancelled'
    };

    const statusVal = order.status;
    const formattedStatus = statusMap[statusVal] || (typeof statusVal === 'string' ? statusVal : 'Placed');

    let orderDate = order.orderDate || order.createdAt || new Date().toISOString();
    if (typeof orderDate === 'string' && !orderDate.endsWith('Z') && !orderDate.includes('+') && !orderDate.includes('z')) {
      orderDate = orderDate + 'Z';
    }

    return {
      id: order.id || order.orderId,
      orderNumber: order.orderNumber || `ORD-${order.id || order.orderId}`,
      userId: order.userId || order.user?.id,
      userName: order.userName || order.user?.fullName || order.user?.name || order.customerName || '',
      userEmail: order.userEmail || order.user?.email || order.email || '',
      userPhone: order.userPhone || order.phoneNumber || order.user?.phoneNumber || order.phone || '',
      phoneNumber: order.phoneNumber || order.userPhone || order.user?.phoneNumber || '',
      orderDate: orderDate,
      totalAmount: Number(order.totalAmount || order.finalAmount || order.totalPrice || 0),
      deliveryCharge: Number(order.deliveryCharge || 0),
      discount: Number(order.discount || 0),
      tax: Number(order.tax || 0),
      finalAmount: Number(
        order.finalAmount ||
        (
          Number(order.totalAmount || order.totalPrice || 0) +
          Number(order.deliveryCharge || 0) +
          Number(order.tax || 0) -
          Number(order.discount || 0)
        )
      ),
      status: formattedStatus,
      deliveryAddress: order.deliveryAddress || order.address || '',
      restaurantName: order.restaurantName || order.restaurant?.name || '',
      items: Array.isArray(order.items) ? order.items.map((i: any) => ({
        id: i.id || i.foodItemId,
        foodItemId: i.foodItemId || i.id,
        foodItemName: i.foodItemName || i.foodName || i.name || '',
        foodItemPrice: Number(i.foodItemPrice || i.price || 0),
        quantity: Number(i.quantity || 1)
      })) : []
    };
  }

  placeOrder(request: PlaceOrderRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/place`, request);
  }

  getAllOrders(): Observable<Order[]> {
    return this.http.get<any[]>(`${this.apiUrl}/all`).pipe(
      map(list => (Array.isArray(list) ? list.map(o => this.mapOrder(o)) : []))
    );
  }

  getMyOrders(): Observable<Order[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my-orders`).pipe(
      map(list => (Array.isArray(list) ? list.map(o => this.mapOrder(o)) : []))
    );
  }

  getOrderById(id: number): Observable<Order> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(o => this.mapOrder(o))
    );
  }

  cancelOrder(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/cancel`, {});
  }

  updateOrderStatus(id: number, statusStr: string): Observable<any> {
    const statusMap: Record<string, number> = {
      'Placed': 1,
      'Pending': 1,
      'Confirmed': 2,
      'Preparing': 3,
      'Out for Delivery': 4,
      'OutForDelivery': 4,
      'Delivered': 5,
      'Cancelled': 6
    };
    const enumVal = statusMap[statusStr] || 1;
    return this.http.put(`${this.apiUrl}/${id}/status`, { status: enumVal });
  }
}


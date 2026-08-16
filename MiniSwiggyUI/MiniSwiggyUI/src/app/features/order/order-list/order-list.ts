import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';


@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.html',
  styleUrl: './order-list.css',
  standalone: false
})
export class OrderListComponent implements OnInit {
  private orderService = inject(OrderService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  orders: Order[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.orderService.getMyOrders()
      .pipe(timeout(3000))
      .subscribe({
        next: (data) => {
          this.orders = data || [];
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.orders = [];
          this.isLoading = false;
          this.toast.error('Failed to load orders');
          this.cdr.detectChanges();
        }
      });
  }

  cancelOrder(id: number): void {
    if (!confirm('Are you sure you want to cancel this order?')) return;

    this.orderService.cancelOrder(id).subscribe({
      next: () => {
        this.toast.success('Order cancelled successfully');
        this.loadOrders();
      },
      error: () => {
        this.toast.error('Unable to cancel order');
      }
    });
  }


  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'delivered': return 'badge-success';
      case 'preparing':
      case 'confirmed':
      case 'out for delivery': return 'badge-warning';
      case 'cancelled': return 'badge-danger';
      default: return 'badge-info';
    }
  }

  getStatusStep(status: string): number {
    const s = status.toLowerCase();
    if (s === 'placed' || s === 'pending') return 1;
    if (s === 'confirmed') return 2;
    if (s === 'preparing') return 3;
    if (s === 'out for delivery' || s === 'outfordelivery') return 4;
    if (s === 'delivered') return 5;
    return 0;
  }
}

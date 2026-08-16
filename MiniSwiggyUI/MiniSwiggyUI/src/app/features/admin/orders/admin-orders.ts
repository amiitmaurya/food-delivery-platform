import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-admin-orders',
  templateUrl: './admin-orders.html',
  styleUrl: './admin-orders.css',
  standalone: false
})
export class AdminOrdersComponent implements OnInit {
  private orderService = inject(OrderService);

  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  orders: Order[] = [];
  isLoading = true;
  statusOptions = ['Placed', 'Confirmed', 'Preparing', 'Out for Delivery', 'Delivered', 'Cancelled'];

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.orderService.getAllOrders()
      .pipe(
        timeout(5000)
      )
      .subscribe({
        next: (orders: Order[]) => {
          this.orders = orders || [];

          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading orders:', err);

          this.orders = [];
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  changeStatus(order: Order, newStatus: string) {
    const oldStatus = order.status;
    order.status = newStatus;
    this.cdr.detectChanges();

    this.orderService.updateOrderStatus(order.id, newStatus).subscribe({
      next: () => {
        this.toast.success(`Order #${order.id} status updated to "${newStatus}" in DB!`);
      },
      error: () => {
        order.status = oldStatus;
        this.toast.error(`Failed to update Order #${order.id} status`);
        this.cdr.detectChanges();
      }
    });
  }

  cancelOrder(order: Order) {
    if (order.status === 'Delivered') {
      this.toast.error('Delivered orders cannot be cancelled.');
      return;
    }

    if (!confirm(`Are you sure you want to cancel Order #${order.id}?`)) return;

    this.orderService.cancelOrder(order.id).subscribe({
      next: () => {
        order.status = 'Cancelled';
        this.toast.success(`Order #${order.id} cancelled successfully in Database!`);
        this.loadOrders();
      },
      error: () => {
        this.toast.error(`Failed to cancel Order #${order.id}`);
      }
    });
  }
}

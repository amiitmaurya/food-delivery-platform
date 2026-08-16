import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DeliveryPartnerService } from '../../services/delivery-partner.service';
import { DeliveryOrder } from '../../models/delivery-partner.models';
import { ToastService } from '../../../../core/services/toast.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-assigned-orders',
  standalone: false,
  templateUrl: './assigned-orders.html',
  styleUrl: './assigned-orders.css'
})
export class AssignedOrdersComponent implements OnInit {
  private deliveryService = inject(DeliveryPartnerService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  assignedOrders: DeliveryOrder[] = [];
  selectedOrder: DeliveryOrder | null = null;
  isLoading = true;

  acceptedOrderIds = new Set<number>();
  rejectedOrderIds = new Set<number>();
  processingOrderIds = new Set<number>();

  ngOnInit(): void {
    this.loadAssignedOrders();
  }

  loadAssignedOrders(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    // Safety timeout ensuring loading spinner never freezes
    setTimeout(() => {
      if (this.isLoading) {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    }, 500);

    this.deliveryService.getMyAssignedOrders().subscribe({
      next: (orders) => {
        this.assignedOrders = orders || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Assigned orders error:', err);
        this.assignedOrders = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  isOrderAccepted(order: DeliveryOrder | null): boolean {
    if (!order) return false;
    return this.acceptedOrderIds.has(order.id) || order.status === 'Accepted';
  }

  isOrderRejected(order: DeliveryOrder | null): boolean {
    if (!order) return false;
    return this.rejectedOrderIds.has(order.id) || order.status === 'Rejected';
  }

  isOrderDisabled(order: DeliveryOrder | null): boolean {
    if (!order) return false;
    return this.isOrderAccepted(order) || this.isOrderRejected(order) || this.processingOrderIds.has(order.id);
  }

  acceptOrder(order: DeliveryOrder): void {
    if (this.isOrderDisabled(order)) return;

    this.processingOrderIds.add(order.id);
    this.cdr.detectChanges();

    this.deliveryService.acceptOrder(order.id).subscribe({
      next: (res) => {
        this.processingOrderIds.delete(order.id);
        this.acceptedOrderIds.add(order.id);
        order.status = 'Accepted';
        this.toast.show(res.message || 'Order accepted! Redirecting...');
        this.cdr.detectChanges();

        setTimeout(() => {
          this.router.navigate(['/delivery-partner/active-delivery']);
        }, 1200);
      },
      error: (err) => {
        this.processingOrderIds.delete(order.id);
        this.toast.show(err.error?.message || 'Failed to accept order', 'error');
        this.cdr.detectChanges();
      }
    });
  }

  rejectOrder(order: DeliveryOrder): void {
    if (this.isOrderDisabled(order)) return;
    if (!confirm('Are you sure you want to reject this assigned order?')) return;

    this.processingOrderIds.add(order.id);
    this.cdr.detectChanges();

    this.deliveryService.rejectOrder(order.id).subscribe({
      next: (res) => {
        this.processingOrderIds.delete(order.id);
        this.rejectedOrderIds.add(order.id);
        order.status = 'Rejected';
        this.toast.show(res.message || 'Order rejected.');
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.processingOrderIds.delete(order.id);
        this.toast.show(err.error?.message || 'Failed to reject order', 'error');
        this.cdr.detectChanges();
      }
    });
  }

  viewDetails(order: DeliveryOrder): void {
    this.selectedOrder = order;
  }

  closeModal(): void {
    this.selectedOrder = null;
  }
}

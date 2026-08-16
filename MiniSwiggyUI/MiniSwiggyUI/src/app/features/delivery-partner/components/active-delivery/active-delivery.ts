import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DeliveryPartnerService } from '../../services/delivery-partner.service';
import { DeliveryOrder } from '../../models/delivery-partner.models';
import { ToastService } from '../../../../core/services/toast.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-active-delivery',
  standalone: false,
  templateUrl: './active-delivery.html',
  styleUrl: './active-delivery.css'
})
export class ActiveDeliveryComponent implements OnInit {
  private deliveryService = inject(DeliveryPartnerService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  activeOrder: DeliveryOrder | null = null;
  isLoading = true;
  isUpdating = false;

  ngOnInit(): void {
    this.loadActiveDelivery();
  }

  loadActiveDelivery(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    setTimeout(() => {
      if (this.isLoading) {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    }, 500);

    this.deliveryService.getCurrentDelivery().subscribe({
      next: (order) => {
        this.activeOrder = order;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.activeOrder = null;
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getStepState(stepName: string): 'completed' | 'active' | 'pending' {
    if (!this.activeOrder) return 'pending';
    const status = this.activeOrder.status;

    const sequence = ['Assigned', 'Accepted', 'ReachedRestaurant', 'PickedUp', 'OutForDelivery', 'Delivered'];
    const currentIndex = sequence.indexOf(status);
    const stepIndex = sequence.indexOf(stepName);

    if (stepIndex < currentIndex) return 'completed';
    if (stepIndex === currentIndex) return 'active';
    return 'pending';
  }

  acceptOrder(): void {
    if (!this.activeOrder) return;
    this.isUpdating = true;
    this.deliveryService.acceptOrder(this.activeOrder.id).subscribe({
      next: (res) => {
        this.toast.show(res.message || 'Order accepted successfully!');
        this.loadActiveDelivery();
        this.isUpdating = false;
      },
      error: (err) => {
        this.toast.show(err.error?.message || 'Action failed', 'error');
        this.isUpdating = false;
      }
    });
  }

  markReachedRestaurant(): void {
    if (!this.activeOrder) return;
    this.isUpdating = true;
    this.deliveryService.markReachedRestaurant(this.activeOrder.id).subscribe({
      next: (res) => {
        this.toast.show(res.message || 'Reached restaurant!');
        this.loadActiveDelivery();
        this.isUpdating = false;
      },
      error: (err) => {
        this.toast.show(err.error?.message || 'Action failed', 'error');
        this.isUpdating = false;
      }
    });
  }

  markPickedUp(): void {
    if (!this.activeOrder) return;
    this.isUpdating = true;
    this.deliveryService.markPickedUp(this.activeOrder.id).subscribe({
      next: (res) => {
        this.toast.show(res.message || 'Order picked up!');
        this.loadActiveDelivery();
        this.isUpdating = false;
      },
      error: (err) => {
        this.toast.show(err.error?.message || 'Action failed', 'error');
        this.isUpdating = false;
      }
    });
  }

  markOutForDelivery(): void {
    if (!this.activeOrder) return;
    this.isUpdating = true;
    this.deliveryService.markOutForDelivery(this.activeOrder.id).subscribe({
      next: (res) => {
        this.toast.show(res.message || 'Order out for delivery!');
        this.loadActiveDelivery();
        this.isUpdating = false;
      },
      error: (err) => {
        this.toast.show(err.error?.message || 'Action failed', 'error');
        this.isUpdating = false;
      }
    });
  }

  markDelivered(): void {
    if (!this.activeOrder) return;
    this.isUpdating = true;
    this.deliveryService.markDelivered(this.activeOrder.id).subscribe({
      next: (res) => {
        this.toast.show(res.message || 'Order delivered successfully!');
        if (this.activeOrder) {
          this.activeOrder.status = 'Delivered';
        }
        this.isUpdating = false;
        this.cdr.detectChanges();

        setTimeout(() => {
          this.router.navigate(['/delivery-partner/delivery-history']);
        }, 1500);
      },
      error: (err) => {
        this.toast.show(err.error?.message || 'Action failed', 'error');
        this.isUpdating = false;
        this.cdr.detectChanges();
      }
    });
  }

  openNavigationMap(address: string): void {
    const encodedAddress = encodeURIComponent(address);
    window.open(`https://www.google.com/maps/search/?api=1&query=${encodedAddress}`, '_blank');
  }
}

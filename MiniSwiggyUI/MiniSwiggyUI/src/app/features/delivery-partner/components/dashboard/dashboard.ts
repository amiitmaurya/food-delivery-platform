import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DeliveryPartnerService } from '../../services/delivery-partner.service';
import { DeliveryEarnings, DeliveryOrder, DeliveryProfile } from '../../models/delivery-partner.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-delivery-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DeliveryDashboardComponent implements OnInit {
  private deliveryService = inject(DeliveryPartnerService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  profile: DeliveryProfile | null = null;
  earnings: DeliveryEarnings | null = null;
  activeOrder: DeliveryOrder | null = null;
  assignedCount = 0;
  isLoading = true;

  defaultAvatar = 'https://ui-avatars.com/api/?name=Delivery+Partner&background=ff5200&color=fff';
  defaultRestaurantImage = 'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=500';

  ngOnInit(): void {
    this.loadDashboardData();
  }

  onAvatarError(event: any): void {
    event.target.src = this.defaultAvatar;
  }

  onRestaurantImgError(event: any): void {
    event.target.src = this.defaultRestaurantImage;
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    // Read local cache first for instant saved profile details
    const localProfileStr = localStorage.getItem('miniswiggy_delivery_profile');
    let localProfile: any = {};
    if (localProfileStr) {
      try { localProfile = JSON.parse(localProfileStr); } catch {}
    }

    setTimeout(() => {
      if (this.isLoading) {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    }, 500);

    this.deliveryService.getProfile().subscribe({
      next: (data) => {
        this.profile = {
          ...(data || {} as any),
          ...localProfile
        };
        this.cdr.detectChanges();
      },
      error: () => {
        if (localProfile && localProfile.vehicleNumber) {
          this.profile = localProfile;
          this.cdr.detectChanges();
        }
      }
    });

    this.deliveryService.getEarnings().subscribe({
      next: (data) => {
        this.earnings = data;
        this.cdr.detectChanges();
      },
      error: () => {}
    });

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

    this.deliveryService.getMyAssignedOrders().subscribe({
      next: (orders) => {
        this.assignedCount = orders.length;
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  toggleOnline(): void {
    if (!this.profile) {
      this.profile = { isOnline: false } as DeliveryProfile;
    }

    const currentStatus = !!(this.profile && this.profile.isOnline);
    const newStatus = !currentStatus;

    if (this.profile) {
      this.profile.isOnline = newStatus;
    }

    localStorage.setItem('miniswiggy_delivery_online', String(newStatus));

    this.toast.show(newStatus ? 'You are now ONLINE to receive orders!' : 'You are now OFFLINE.');
    this.cdr.detectChanges();

    this.deliveryService.toggleOnlineStatus(newStatus).subscribe({
      next: () => {},
      error: () => {}
    });
  }
}

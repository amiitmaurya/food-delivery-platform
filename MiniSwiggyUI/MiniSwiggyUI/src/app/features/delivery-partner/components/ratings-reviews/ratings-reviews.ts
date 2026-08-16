import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DeliveryPartnerService } from '../../services/delivery-partner.service';
import { DeliveryEarnings, DeliveryOrder } from '../../models/delivery-partner.models';

@Component({
  selector: 'app-ratings-reviews',
  standalone: false,
  templateUrl: './ratings-reviews.html',
  styleUrl: './ratings-reviews.css'
})
export class RatingsReviewsComponent implements OnInit {
  private deliveryService = inject(DeliveryPartnerService);
  private cdr = inject(ChangeDetectorRef);

  earnings: DeliveryEarnings | null = null;
  deliveredOrders: DeliveryOrder[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadRatings();
  }

  get ratedOrders(): DeliveryOrder[] {
    return this.deliveredOrders.filter(o => o.customerRating && o.customerRating > 0);
  }

  loadRatings(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    setTimeout(() => {
      if (this.isLoading) {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    }, 500);

    this.deliveryService.getEarnings().subscribe({
      next: (data) => {
        this.earnings = data;
        this.cdr.detectChanges();
      },
      error: () => {}
    });

    this.deliveryService.getDeliveryHistory().subscribe({
      next: (orders) => {
        this.deliveredOrders = orders || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.deliveredOrders = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}

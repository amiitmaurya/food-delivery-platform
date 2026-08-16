import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DeliveryPartnerService } from '../../services/delivery-partner.service';
import { DeliveryOrder } from '../../models/delivery-partner.models';

@Component({
  selector: 'app-delivery-history',
  standalone: false,
  templateUrl: './delivery-history.html',
  styleUrl: './delivery-history.css'
})
export class DeliveryHistoryComponent implements OnInit {
  private deliveryService = inject(DeliveryPartnerService);
  private cdr = inject(ChangeDetectorRef);

  historyOrders: DeliveryOrder[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    setTimeout(() => {
      if (this.isLoading) {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    }, 500);

    this.deliveryService.getDeliveryHistory().subscribe({
      next: (orders) => {
        this.historyOrders = orders || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.historyOrders = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}

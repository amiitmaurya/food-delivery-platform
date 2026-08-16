import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DeliveryPartnerService } from '../../services/delivery-partner.service';
import { DeliveryEarnings } from '../../models/delivery-partner.models';

@Component({
  selector: 'app-earnings',
  standalone: false,
  templateUrl: './earnings.html',
  styleUrl: './earnings.css'
})
export class EarningsComponent implements OnInit {
  private deliveryService = inject(DeliveryPartnerService);
  private cdr = inject(ChangeDetectorRef);

  earnings: DeliveryEarnings | null = null;
  isLoading = true;

  ngOnInit(): void {
    this.loadEarnings();
  }

  loadEarnings(): void {
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
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.earnings = null;
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}

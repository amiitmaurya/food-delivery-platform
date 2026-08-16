import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { DeliveryPartnerRoutingModule } from './delivery-partner-routing.module';
import { DeliveryDashboardComponent } from './components/dashboard/dashboard';
import { AssignedOrdersComponent } from './components/assigned-orders/assigned-orders';
import { ActiveDeliveryComponent } from './components/active-delivery/active-delivery';
import { DeliveryHistoryComponent } from './components/delivery-history/delivery-history';
import { EarningsComponent } from './components/earnings/earnings';
import { RatingsReviewsComponent } from './components/ratings-reviews/ratings-reviews';
import { DeliveryProfileComponent } from './components/profile/profile';

@NgModule({
  declarations: [
    DeliveryDashboardComponent,
    AssignedOrdersComponent,
    ActiveDeliveryComponent,
    DeliveryHistoryComponent,
    EarningsComponent,
    RatingsReviewsComponent,
    DeliveryProfileComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DeliveryPartnerRoutingModule
  ]
})
export class DeliveryPartnerModule { }

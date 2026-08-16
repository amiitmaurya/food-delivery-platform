import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DeliveryDashboardComponent } from './components/dashboard/dashboard';
import { AssignedOrdersComponent } from './components/assigned-orders/assigned-orders';
import { ActiveDeliveryComponent } from './components/active-delivery/active-delivery';
import { DeliveryHistoryComponent } from './components/delivery-history/delivery-history';
import { EarningsComponent } from './components/earnings/earnings';
import { RatingsReviewsComponent } from './components/ratings-reviews/ratings-reviews';
import { DeliveryProfileComponent } from './components/profile/profile';
import { DeliveryPartnerGuard } from './guards/delivery-partner.guard';

const routes: Routes = [
  {
    path: 'delivery-partner',
    canActivate: [DeliveryPartnerGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DeliveryDashboardComponent },
      { path: 'assigned-orders', component: AssignedOrdersComponent },
      { path: 'active-delivery', component: ActiveDeliveryComponent },
      { path: 'delivery-history', component: DeliveryHistoryComponent },
      { path: 'earnings', component: EarningsComponent },
      { path: 'ratings-reviews', component: RatingsReviewsComponent },
      { path: 'profile', component: DeliveryProfileComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DeliveryPartnerRoutingModule { }

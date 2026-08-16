import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { RestaurantListComponent } from './features/restaurant/restaurant-list/restaurant-list';
import { RestaurantDetailComponent } from './features/restaurant/restaurant-detail/restaurant-detail';
import { CartCheckoutComponent } from './features/order/cart-checkout/cart-checkout';
import { OrderListComponent } from './features/order/order-list/order-list';
import { DashboardComponent } from './features/admin/dashboard/dashboard';
import { AdminRestaurantsComponent } from './features/admin/restaurants/admin-restaurants';
import { AdminOrdersComponent } from './features/admin/orders/admin-orders';
import { CategoryComponent } from './features/admin/categories/category';
import { FooditemComponent } from './features/admin/food-item/food-item';
import { AddressListComponent } from './features/profile/addresses/address-list';
import { WishlistComponent } from './features/profile/wishlist/wishlist';
import { CouponListComponent } from './features/admin/coupons/coupon-list';
import { ChangePasswordComponent } from './features/auth/change-password/change-password';
import { EditProfileComponent } from './features/profile/edit-profile/edit-profile';
import { PaymentHistoryComponent } from './features/payment/payment-history/payment-history';

// Super Admin Components & Guard
import { SuperAdminGuard } from './core/guards/super-admin.guard';
import { SuperAdminDashboardComponent } from './features/super-admin/dashboard/super-admin-dashboard';
import { UserMasterComponent } from './features/super-admin/user-master/user-master';
import { RoleMasterComponent } from './features/super-admin/role-master/role-master';
import { DeliveryPartnerMasterComponent } from './features/super-admin/delivery-partner-master/delivery-partner-master';
import { ReviewMasterComponent } from './features/super-admin/review-master/review-master';
import { ModulePermissionsComponent } from './features/super-admin/module-permissions/module-permissions';

// Delivery Partner Dedicated Components & Guard
import { DeliveryDashboardComponent } from './features/delivery-partner/components/dashboard/dashboard';
import { AssignedOrdersComponent } from './features/delivery-partner/components/assigned-orders/assigned-orders';
import { ActiveDeliveryComponent } from './features/delivery-partner/components/active-delivery/active-delivery';
import { DeliveryHistoryComponent } from './features/delivery-partner/components/delivery-history/delivery-history';
import { EarningsComponent } from './features/delivery-partner/components/earnings/earnings';
import { RatingsReviewsComponent } from './features/delivery-partner/components/ratings-reviews/ratings-reviews';
import { DeliveryProfileComponent } from './features/delivery-partner/components/profile/profile';
import { DeliveryPartnerGuard } from './features/delivery-partner/guards/delivery-partner.guard';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'restaurant', component: RestaurantListComponent },
  { path: 'restaurant/:id', component: RestaurantDetailComponent },
  { path: 'cart', component: CartCheckoutComponent },
  { path: 'my-orders', component: OrderListComponent },
  { path: 'addresses', component: AddressListComponent },
  { path: 'wishlist', component: WishlistComponent },
  { path: 'payment-history', component: PaymentHistoryComponent },
  { path: 'change-password', component: ChangePasswordComponent },
  { path: 'profile/edit', component: EditProfileComponent },

  // Admin & Master Routes
  { path: 'dashboard', component: DashboardComponent },
  { path: 'admin/restaurants', component: AdminRestaurantsComponent },
  { path: 'admin/orders', component: AdminOrdersComponent },
  { path: 'admin/payments', component: PaymentHistoryComponent },
  { path: 'categories', component: CategoryComponent },
  { path: 'food-item', component: FooditemComponent },
  { path: 'coupons', component: CouponListComponent },

  // Super Admin Dedicated Master Routes
  { path: 'superadmin', redirectTo: 'superadmin/dashboard', pathMatch: 'full' },
  { path: 'superadmin/dashboard', component: SuperAdminDashboardComponent, canActivate: [SuperAdminGuard] },
  { path: 'superadmin/users', component: UserMasterComponent, canActivate: [SuperAdminGuard] },
  { path: 'superadmin/roles', component: RoleMasterComponent, canActivate: [SuperAdminGuard] },
  { path: 'superadmin/delivery-partners', component: DeliveryPartnerMasterComponent, canActivate: [SuperAdminGuard] },
  { path: 'superadmin/reviews', component: ReviewMasterComponent, canActivate: [SuperAdminGuard] },
  { path: 'superadmin/permissions', component: ModulePermissionsComponent, canActivate: [SuperAdminGuard] },

  // Delivery Partner Dedicated Routes
  { path: 'delivery-partner', redirectTo: 'delivery-partner/dashboard', pathMatch: 'full' },
  { path: 'delivery-partner/dashboard', component: DeliveryDashboardComponent, canActivate: [DeliveryPartnerGuard] },
  { path: 'delivery-partner/assigned-orders', component: AssignedOrdersComponent, canActivate: [DeliveryPartnerGuard] },
  { path: 'delivery-partner/active-delivery', component: ActiveDeliveryComponent, canActivate: [DeliveryPartnerGuard] },
  { path: 'delivery-partner/delivery-history', component: DeliveryHistoryComponent, canActivate: [DeliveryPartnerGuard] },
  { path: 'delivery-partner/earnings', component: EarningsComponent, canActivate: [DeliveryPartnerGuard] },
  { path: 'delivery-partner/ratings-reviews', component: RatingsReviewsComponent, canActivate: [DeliveryPartnerGuard] },
  { path: 'delivery-partner/profile', component: DeliveryProfileComponent, canActivate: [DeliveryPartnerGuard] },

  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

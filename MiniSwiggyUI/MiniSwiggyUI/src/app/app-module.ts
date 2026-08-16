import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';

import { AuthInterceptor } from './core/interceptors/auth.interceptor';

import { SidebarComponent } from './shared/components/sidebar/sidebar';
import { TopNavbarComponent } from './shared/components/top-navbar/top-navbar';
import { FooterComponent } from './shared/components/footer/footer';
import { ToastComponent } from './shared/components/toast/toast';

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

// Super Admin Components
import { SuperAdminDashboardComponent } from './features/super-admin/dashboard/super-admin-dashboard';
import { UserMasterComponent } from './features/super-admin/user-master/user-master';
import { RoleMasterComponent } from './features/super-admin/role-master/role-master';
import { DeliveryPartnerMasterComponent } from './features/super-admin/delivery-partner-master/delivery-partner-master';
import { ReviewMasterComponent } from './features/super-admin/review-master/review-master';
import { ModulePermissionsComponent } from './features/super-admin/module-permissions/module-permissions';

import { DeliveryPartnerModule } from './features/delivery-partner/delivery-partner.module';

@NgModule({
  declarations: [
    App,
    SidebarComponent,
    TopNavbarComponent,
    FooterComponent,
    ToastComponent,
    LoginComponent,
    RegisterComponent,
    RestaurantListComponent,
    RestaurantDetailComponent,
    CartCheckoutComponent,
    OrderListComponent,
    DashboardComponent,
    AdminRestaurantsComponent,
    AdminOrdersComponent,
    CategoryComponent,
    FooditemComponent,
    AddressListComponent,
    WishlistComponent,
    CouponListComponent,
    ChangePasswordComponent,
    EditProfileComponent,
    PaymentHistoryComponent,
    SuperAdminDashboardComponent,
    UserMasterComponent,
    RoleMasterComponent,
    DeliveryPartnerMasterComponent,
    ReviewMasterComponent,
    ModulePermissionsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    DeliveryPartnerModule
  ],
  providers: [
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [App]
})
export class AppModule { }

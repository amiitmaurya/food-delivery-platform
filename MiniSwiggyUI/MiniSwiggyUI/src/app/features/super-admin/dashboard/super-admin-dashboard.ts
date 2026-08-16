import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../../core/services/user.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { CategoryService } from '../../../core/services/category.service';
import { FoodItemService } from '../../../core/services/food-item.service';
import { OrderService } from '../../../core/services/order.service';
import { CouponService } from '../../../core/services/coupon.service';
import { UserStats, Order } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-super-admin-dashboard',
  templateUrl: './super-admin-dashboard.html',
  styleUrl: './super-admin-dashboard.css',
  standalone: false
})
export class SuperAdminDashboardComponent implements OnInit {
  private userService = inject(UserService);
  private restaurantService = inject(RestaurantService);
  private categoryService = inject(CategoryService);
  private foodItemService = inject(FoodItemService);
  private orderService = inject(OrderService);
  private couponService = inject(CouponService);
  private cdr = inject(ChangeDetectorRef);

  userStats: UserStats = {
    totalUsers: 0,
    totalSuperAdmins: 0,
    totalAdmins: 0,
    totalCustomers: 0,
    totalDeliveryPartners: 0,
    totalRestaurantOwners: 0,
    activeUsers: 0,
    inactiveUsers: 0
  };

  totalRestaurants = 0;
  totalCategories = 0;
  totalFoodItems = 0;
  totalCoupons = 0;
  totalRevenue = 0;
  recentOrders: Order[] = [];

  isLoading = true;

  ngOnInit(): void {
    this.loadAllMetrics();
  }

  loadAllMetrics(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.userService.getStats().pipe(timeout(3000), catchError(() => of(null))).subscribe(s => {
      if (s) this.userStats = s;
      this.cdr.detectChanges();
    });

    this.restaurantService.getAll().pipe(timeout(3000), catchError(() => of([]))).subscribe(res => {
      this.totalRestaurants = res ? res.length : 0;
      this.cdr.detectChanges();
    });

    this.categoryService.getAll().pipe(timeout(3000), catchError(() => of([]))).subscribe(cats => {
      this.totalCategories = cats ? cats.length : 0;
      this.cdr.detectChanges();
    });

    this.foodItemService.getAll().pipe(timeout(3000), catchError(() => of([]))).subscribe(foods => {
      this.totalFoodItems = foods ? foods.length : 0;
      this.cdr.detectChanges();
    });

    this.couponService.getAll().pipe(timeout(3000), catchError(() => of([]))).subscribe(coupons => {
      this.totalCoupons = coupons ? coupons.length : 0;
      this.cdr.detectChanges();
    });

    this.orderService.getAllOrders().pipe(
      timeout(4000),
      catchError(() => this.orderService.getMyOrders().pipe(catchError(() => of([]))))
    ).subscribe(orders => {
      if (orders && orders.length > 0) {
        this.recentOrders = orders.slice(0, 6);
        this.totalRevenue = orders
          .filter(o => o.status !== 'Cancelled')
          .reduce((sum, o) => sum + (Number(o.finalAmount) || Number(o.totalAmount) || 0), 0);
      } else {
        this.recentOrders = [];
        this.totalRevenue = 0;
      }
      this.isLoading = false;
      this.cdr.detectChanges();
    });
  }
}

import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { CategoryService } from '../../../core/services/category.service';
import { FoodItemService } from '../../../core/services/food-item.service';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  private restaurantService = inject(RestaurantService);
  private categoryService = inject(CategoryService);
  private foodItemService = inject(FoodItemService);
  private orderService = inject(OrderService);
  private cdr = inject(ChangeDetectorRef);

  totalRestaurants = 0;
  totalCategories = 0;
  totalFoodItems = 0;
  totalRevenue = 0;
  recentOrders: Order[] = [];

  isLoading = true;

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.restaurantService.getAll().pipe(timeout(2000), catchError(() => of([]))).subscribe(res => {
      this.totalRestaurants = res ? res.length : 0;
      this.cdr.detectChanges();
    });

    this.categoryService.getAll().pipe(timeout(2000), catchError(() => of([]))).subscribe(cats => {
      this.totalCategories = cats ? cats.length : 0;
      this.cdr.detectChanges();
    });

    this.foodItemService.getAll().pipe(timeout(2000), catchError(() => of([]))).subscribe(foods => {
      this.totalFoodItems = foods ? foods.length : 0;
      this.cdr.detectChanges();
    });

    this.orderService.getAllOrders().pipe(timeout(2000), catchError(() => of([]))).subscribe(orders => {
      if (orders && orders.length > 0) {
        this.recentOrders = orders.slice(0, 5);
        this.totalRevenue = orders.reduce((sum, o) => sum + (Number(o.totalAmount) || 0), 0);
      } else {
        this.recentOrders = [];
        this.totalRevenue = 0;
      }
      this.isLoading = false;
      this.cdr.detectChanges();
    });
  }
}

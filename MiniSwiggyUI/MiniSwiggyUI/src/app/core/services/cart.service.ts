import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { CartItem, FoodItem } from '../models';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  private http = inject(HttpClient);
  private toast = inject(ToastService);

  private readonly apiUrl = 'https://localhost:7241/api/Cart';

  private cartItemsSubject = new BehaviorSubject<CartItem[]>([]);
  cartItems$ = this.cartItemsSubject.asObservable();

  cartCount$ = this.cartItems$.pipe(
    map(items => items.reduce((sum, item) => sum + Number(item.quantity), 0))
  );

  cartSubtotal$ = this.cartItems$.pipe(
    map(items =>
      items.reduce(
        (sum, item) =>
          sum + Number(item.foodItemPrice) * Number(item.quantity),
        0
      )
    )
  );

  constructor() {
    this.fetchCartFromApi();
  }

  private cleanCartItems(items: any[]): CartItem[] {
    return (items || []).map(item => ({
      id: item.id,
      foodItemId: item.foodItemId,
      foodItemName: item.foodItemName,

      // Backend returns unitPrice
      foodItemPrice: Number(item.unitPrice),

      quantity: Number(item.quantity),

      imageUrl: item.imageUrl ?? '',
      restaurantId: item.restaurantId ?? 0,
      isVegetarian: item.isVegetarian ?? true
    }));
  }

  fetchCartFromApi(): void {
    this.http.get<any>(this.apiUrl).subscribe({
      next: (response) => {

        const items = Array.isArray(response)
          ? response
          : response.items ?? [];

        this.cartItemsSubject.next(
          this.cleanCartItems(items)
        );
      },
      error: (err) => {
        console.error(err);
        this.cartItemsSubject.next([]);
      }
    });
  }

  addToCart(food: FoodItem, quantity: number = 1): void {

    this.http.post(this.apiUrl, {
      foodItemId: food.id,
      quantity
    }).subscribe({
      next: () => {
        this.toast.success(`${food.name} added to cart`);
        this.fetchCartFromApi();
      },
      error: err => {
        console.error(err);
        this.toast.error('Failed to add item');
      }
    });
  }

  updateQuantity(cartItemId: number, quantity: number): void {

    if (quantity <= 0) {
      this.removeItem(cartItemId);
      return;
    }

    this.http.put(`${this.apiUrl}/${cartItemId}`, { quantity })
      .subscribe({
        next: () => this.fetchCartFromApi(),
        error: err => {
          console.error(err);
          this.toast.error('Failed to update quantity');
        }
      });
  }

  removeItem(cartItemId: number): void {

    this.http.delete(`${this.apiUrl}/${cartItemId}`)
      .subscribe({
        next: () => {
          this.toast.info('Item removed');
          this.fetchCartFromApi();
        },
        error: err => {
          console.error(err);
          this.toast.error('Failed to remove item');
        }
      });
  }

  clearCart(): void {

    this.http.delete(`${this.apiUrl}/clear`)
      .subscribe({
        next: () => {
          this.cartItemsSubject.next([]);
        },
        error: err => {
          console.error(err);
          this.toast.error('Failed to clear cart');
        }
      });
  }
}

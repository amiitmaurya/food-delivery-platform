import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, of } from 'rxjs';
import { Restaurant } from '../models';
import { ToastService } from './toast.service';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private authService = inject(AuthService);
  private apiUrl = 'https://localhost:7241/api/Wishlist';

  private wishlistSubject = new BehaviorSubject<Restaurant[]>([]);
  wishlist$ = this.wishlistSubject.asObservable();

  constructor() {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.loadWishlist();
      } else {
        this.wishlistSubject.next([]);
      }
    });
  }

  loadWishlist(): void {
    if (!this.authService.isLoggedIn()) {
      this.wishlistSubject.next([]);
      return;
    }

    this.http.get<any>(this.apiUrl).pipe(
      catchError(() => of([]))
    ).subscribe((res) => {
      const list = Array.isArray(res) ? res : (res?.items || []);
      this.wishlistSubject.next(list);
    });
  }

  isWishlisted(id: number): boolean {
    if (!id || !this.wishlistSubject.value || this.wishlistSubject.value.length === 0) return false;
    return this.wishlistSubject.value.some(r => Number(r.id || (r as any).foodItemId) === Number(id));
  }

  toggleWishlist(restaurant: Restaurant): void {
    if (!this.authService.isLoggedIn()) {
      this.toast.info('Please login to save favorite restaurants to your Wishlist.');
      return;
    }

    const isCurrentlyWishlisted = this.isWishlisted(restaurant.id);
    const currentList = [...this.wishlistSubject.value];

    // Optimistic UI update
    if (isCurrentlyWishlisted) {
      this.wishlistSubject.next(currentList.filter(r => Number(r.id) !== Number(restaurant.id)));
      this.toast.info(`Removed ${restaurant.name} from Wishlist`);
    } else {
      this.wishlistSubject.next([...currentList, restaurant]);
      this.toast.success(`❤️ Added ${restaurant.name} to Wishlist!`);
    }

    this.http.post(`${this.apiUrl}/toggle/${restaurant.id}`, {}).pipe(
      catchError(() => {
        // Rollback on error
        this.wishlistSubject.next(currentList);
        this.toast.error('Failed to update Wishlist in database.');
        return of(null);
      })
    ).subscribe((res) => {
      if (res) {
        this.loadWishlist();
      }
    });
  }

  removeFromWishlist(id: number): void {
    const currentList = [...this.wishlistSubject.value];
    this.wishlistSubject.next(currentList.filter(r => Number(r.id) !== Number(id)));
    this.toast.info('Removed from Wishlist');

    this.http.delete(`${this.apiUrl}/${id}`).pipe(
      catchError(() => {
        this.wishlistSubject.next(currentList);
        this.toast.error('Failed to remove from Wishlist');
        return of(null);
      })
    ).subscribe(() => {
      this.loadWishlist();
    });
  }
}

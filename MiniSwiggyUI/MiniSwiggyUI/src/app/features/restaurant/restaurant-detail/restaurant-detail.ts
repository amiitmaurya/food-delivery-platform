import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { FoodItemService } from '../../../core/services/food-item.service';
import { CartService } from '../../../core/services/cart.service';
import { ReviewService } from '../../../core/services/review.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { Restaurant, FoodItem, CartItem, Review, AddReviewRequest } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-restaurant-detail',
  templateUrl: './restaurant-detail.html',
  styleUrl: './restaurant-detail.css',
  standalone: false
})
export class RestaurantDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private restaurantService = inject(RestaurantService);
  private foodItemService = inject(FoodItemService);
  private reviewService = inject(ReviewService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  cartService = inject(CartService);

  restaurantId: number = 0;
  restaurant: Restaurant | null = null;
  foodItems: FoodItem[] = [];
  filteredFoodItems: FoodItem[] = [];
  cartItems: CartItem[] = [];
  reviews: Review[] = [];

  isLoading = true;
  vegOnlyFilter = false;
  nonVegOnlyFilter = false;
  searchQuery = '';

  // Add review form state
  showReviewModal = false;
  newReview = {
    foodItemId: 0,
    rating: 5,
    comment: ''
  };

  defaultRestaurantImg = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';
  defaultDishImg = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';
  defaultUserAvatar = 'https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png';

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.restaurantId = +params['id'];
      if (this.restaurantId) {
        this.loadRestaurantDetails();
        // this.loadReviews();
      }
    });

    this.cartService.cartItems$.subscribe(items => {
      this.cartItems = items;
      this.cdr.detectChanges();
    });
  }

  formatImageUrl(url?: string, isDish: boolean = false): string {
    const fallback = isDish ? this.defaultDishImg : this.defaultUserAvatar;
    if (!url || url.trim() === '' || url.includes('No_Image_Available')) {
      return fallback;
    }
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;
  }

  onImgError(event: any, isDish: boolean = false): void {
    event.target.src = isDish ? this.defaultDishImg : this.defaultUserAvatar;
  }

  // loadRestaurantDetails(): void {
  //   this.isLoading = true;
  //   this.cdr.detectChanges();

  //   this.restaurantService.getById(this.restaurantId).pipe(
  //     timeout(5000),
  //     catchError(() => of(null))
  //   ).subscribe((res) => {
  //     if (res) {
  //       this.restaurant = {
  //         ...res,
  //         imageUrl: this.formatImageUrl(res.imageUrl),
  //         rating: res.rating,
  //         cuisineType: res.cuisineType,
  //         deliveryTime: res.deliveryTime,
  //         averageCostForTwo: res.averageCostForTwo,
  //         minimumOrderAmount: res.minimumOrderAmount
  //       };
  //     } else {
  //       this.restaurant = null;
  //       this.isLoading = false;
  //       this.toast.error('Restaurant not found.');
  //     }
  //     this.cdr.detectChanges();
  //   });

  //   this.foodItemService.getAll().pipe(
  //     timeout(5000),
  //     catchError(() => of([]))
  //   ).subscribe((items) => {
  //     if (items && items.length > 0) {
  //       const resItems = items.filter(i => !i.restaurantId || i.restaurantId === this.restaurantId);
  //       this.foodItems = (resItems.length > 0 ? resItems : items).map(i => ({
  //         ...i,
  //         imageUrl: this.formatImageUrl(i.imageUrl || i.image, true)
  //       }));
  //     } else {
  //       this.foodItems = [];
  //     }
  //     this.applyFilters();
  //     this.isLoading = false;
  //     this.cdr.detectChanges();
  //   });
  // }

  // loadReviews(): void {
  //   this.reviewService.getRestaurantReviewDetails(this.restaurantId).subscribe((res) => {
  //     const currentUser = this.authService.currentUserValue;
  //     this.reviews = (res.reviews || []).map(rev => {
  //       const matchingFood = this.foodItems.find(f => f.id === rev.foodItemId);
  //       const isCurrentUser = currentUser && (rev.userId === currentUser.id || rev.userName === currentUser.fullName);
  //       const currentProfileImg = currentUser ? (currentUser.profileImageUrl || (currentUser as any).imageUrl) : null;
  //       const userImg = (isCurrentUser && currentProfileImg) ? currentProfileImg : rev.userImageUrl;
  //       return {
  //         ...rev,
  //         userImageUrl: userImg,
  //         foodImageUrl: rev.foodImageUrl || matchingFood?.imageUrl
  //       };
  //     });
  //     if (this.restaurant) {
  //       this.restaurant.rating = res.averageRating;
  //     }
  //     this.cdr.detectChanges();
  //   });
  // }

  loadRestaurantDetails(): void {

    this.isLoading = true;
    this.cdr.detectChanges();

    this.restaurantService.getById(this.restaurantId).pipe(
      catchError(() => of(null))
    ).subscribe(res => {

      if (res) {

        this.restaurant = {
          ...res,
          imageUrl: this.formatImageUrl(res.imageUrl),
          rating: res.rating,
          cuisineType: res.cuisineType,
          deliveryTime: res.deliveryTime,
          averageCostForTwo: res.averageCostForTwo,
          minimumOrderAmount: res.minimumOrderAmount
        };

      }

      this.cdr.detectChanges();

    });

    this.foodItemService.getAll().pipe(
      catchError(() => of([]))
    ).subscribe(items => {

      const restaurantFoods = items.filter(x => x.restaurantId == this.restaurantId);

      this.foodItems = restaurantFoods.map(f => ({

        ...f,

        imageUrl: this.formatImageUrl(
          f.imageUrl || (f as any).image,
          true
        )

      }));

      this.applyFilters();

      // IMPORTANT
      this.loadReviews();

      this.isLoading = false;

      this.cdr.detectChanges();

    });

  }

  loadReviews(): void {

    this.reviewService.getRestaurantReviewDetails(this.restaurantId)
      .subscribe(res => {

        const currentUser = this.authService.currentUserValue;

        this.reviews = (res.reviews || []).map(rev => {

          const food = this.foodItems.find(f => f.id === rev.foodItemId);

          const isCurrentUser =
            currentUser &&
            (rev.userId === currentUser.id ||
              rev.userName === currentUser.fullName);

          const profileImage =
            isCurrentUser
              ? (currentUser.profileImageUrl || (currentUser as any).imageUrl)
              : rev.userImageUrl;

          return {

            ...rev,

            userImageUrl: this.formatImageUrl(profileImage, false),

            foodImageUrl: this.formatImageUrl(
              rev.foodImageUrl || food?.imageUrl,
              true
            )

          };

        });

        if (this.restaurant) {
          this.restaurant.rating = res.averageRating;
        }

        this.cdr.detectChanges();

      });

  }



  authService = inject(AuthService);
  editingReviewId = 0;

  openCreateReviewModal(): void {
    this.editingReviewId = 0;
    this.newReview = {
      foodItemId: this.filteredFoodItems[0]?.id || 0,
      rating: 5,
      comment: ''
    };
    this.showReviewModal = true;
    this.cdr.detectChanges();
  }

  openEditReviewModal(rev: Review): void {
    this.editingReviewId = rev.id;
    this.newReview = {
      foodItemId: rev.foodItemId || 0,
      rating: rev.rating || 5,
      comment: rev.comment || ''
    };
    this.showReviewModal = true;
    this.cdr.detectChanges();
  }

  deleteReview(reviewId: number): void {
    if (!confirm('Are you sure you want to delete this review?')) return;

    this.reviewService.deleteReview(reviewId).subscribe({
      next: () => {
        this.toast.success('★ Review deleted successfully!');
        this.loadRestaurantDetails();
        this.loadReviews();
      },
      error: () => {
        this.toast.error('Failed to delete review');
      }
    });
  }

  canModifyReview(rev: Review): boolean {
    const user = this.authService.currentUserValue;
    if (!user) return false;
    if (user.role?.toLowerCase() === 'admin') return true;
    return rev.userName === user.fullName || rev.userId === user.id;
  }

  submitReview(): void {
    if (this.newReview.foodItemId === 0) {
      this.toast.error('Please select a food item.');
      return;
    }

    if (!this.newReview.comment.trim()) {
      this.toast.error('Please write a review.');
      return;
    }

    if (this.editingReviewId > 0) {
      this.reviewService.updateReview(this.editingReviewId, {
        rating: Number(this.newReview.rating),
        comment: this.newReview.comment.trim()
      }).subscribe({
        next: () => {
          this.toast.success('★ Review updated successfully!');
          this.showReviewModal = false;
          this.editingReviewId = 0;
          this.newReview = { foodItemId: 0, rating: 5, comment: '' };
          this.loadRestaurantDetails();
          this.loadReviews();
        },
        error: () => {
          this.toast.error('Failed to update review');
        }
      });
    } else {
      const reviewObj: AddReviewRequest = {
        foodItemId: this.newReview.foodItemId,
        rating: Number(this.newReview.rating),
        comment: this.newReview.comment.trim()
      };

      this.reviewService.addReview(reviewObj).subscribe({
        next: () => {
          this.toast.success('★ Review submitted successfully!');
          this.showReviewModal = false;
          this.newReview = { foodItemId: 0, rating: 5, comment: '' };
          this.loadRestaurantDetails();
          this.loadReviews();
        },
        error: err => {
          console.error(err);
          this.toast.error('Failed to submit review');
        }
      });
    }
  }

  toggleVegFilter() {
    this.vegOnlyFilter = !this.vegOnlyFilter;
    if (this.vegOnlyFilter) this.nonVegOnlyFilter = false;
    this.applyFilters();
  }

  toggleNonVegFilter() {
    this.nonVegOnlyFilter = !this.nonVegOnlyFilter;
    if (this.nonVegOnlyFilter) this.vegOnlyFilter = false;
    this.applyFilters();
  }

  applyFilters() {
    // Hide inactive / out of stock dishes from customer menu
    let result = this.foodItems.filter(item => item.isAvailable !== false);

    if (this.vegOnlyFilter) {
      result = result.filter(item => item.isVegetarian);
    }

    if (this.nonVegOnlyFilter) {
      result = result.filter(item => !item.isVegetarian);
    }

    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      result = result.filter(item => item.name.toLowerCase().includes(q) || item.description.toLowerCase().includes(q));
    }

    this.filteredFoodItems = result;
    this.cdr.detectChanges();
  }

  getQuantityInCart(foodId: number): number {
    const item = this.cartItems.find(i => i.foodItemId === foodId);
    return item ? item.quantity : 0;
  }

  getCartItemId(foodId: number): number {
    const item = this.cartItems.find(i => i.foodItemId === foodId);
    return item ? item.id : 0;
  }

  addItem(food: FoodItem) {
    if (this.authService.isDeliveryPartner()) {
      this.toast.error('Delivery Partners can only pick up & drop orders. Ordering is disabled.');
      return;
    }
    this.cartService.addToCart(food, 1);
  }

  increaseQuantity(food: FoodItem) {
    if (this.authService.isDeliveryPartner()) {
      this.toast.error('Delivery Partners can only pick up & drop orders. Ordering is disabled.');
      return;
    }
    const cartItemId = this.getCartItemId(food.id);
    const currentQty = this.getQuantityInCart(food.id);
    this.cartService.updateQuantity(cartItemId || food.id, currentQty + 1);
  }

  decreaseQuantity(food: FoodItem) {
    if (this.authService.isDeliveryPartner()) {
      this.toast.error('Delivery Partners can only pick up & drop orders. Ordering is disabled.');
      return;
    }
    const cartItemId = this.getCartItemId(food.id);
    const currentQty = this.getQuantityInCart(food.id);
    this.cartService.updateQuantity(cartItemId || food.id, currentQty - 1);
  }

  goBack() {
    this.router.navigate(['/restaurant']);
  }
}

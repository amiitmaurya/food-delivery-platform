import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { CategoryService } from '../../../core/services/category.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { Restaurant, Category } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-restaurant-list',
  templateUrl: './restaurant-list.html',
  styleUrl: './restaurant-list.css',
  standalone: false
})
export class RestaurantListComponent implements OnInit {
  private restaurantService = inject(RestaurantService);
  private categoryService = inject(CategoryService);
  wishlistService = inject(WishlistService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  restaurants: Restaurant[] = [];
  filteredRestaurants: Restaurant[] = [];
  paginatedRestaurants: Restaurant[] = [];
  categories: Category[] = [];

  isLoading = true;
  selectedCategoryName: string | null = null;
  vegOnlyFilter = false;
  nonVegFilter = false;
  highRatingFilter = false;
  searchQuery = '';
  sortBy: 'rating' | 'name' = 'rating';

  // Pagination controls
  currentPage = 1;
  pageSize = 6;
  totalPages = 1;

  heroBanners = [
    { title: 'Craving Something Spicy?', subtitle: 'Flat 50% OFF on Top Biryani Places', code: 'WELCOME50', bg: 'linear-gradient(135deg, #ff9966 0%, #ff5e62 100%)', icon: 'fa-fire' },
    { title: 'Super Fast Food Delivery', subtitle: 'Free Delivery on Orders Above ₹199', code: 'FREEDEL', bg: 'linear-gradient(135deg, #00b09b 0%, #96c93d 100%)', icon: 'fa-bolt' },
    { title: 'Gourmet Desserts & Shakes', subtitle: 'Buy 1 Get 1 Free from Selected Bakers', code: 'SWEETBOGO', bg: 'linear-gradient(135deg, #8a2387 0%, #e94057 100%, #f27121 100%)', icon: 'fa-ice-cream' }
  ];

  categoryImages: Record<string, string> = {
    'biryani': 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg',
    'pizza': 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg',
    'burger': 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg',
    'chinese': 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg',
    'desserts': 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg',
    'south indian': 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg',
    'default': 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg'
  };

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.searchQuery = params['q'] || '';
      this.applyFilters();
    });
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.categoryService.getAll().pipe(
      timeout(3000),
      catchError(() => of([]))
    ).subscribe((cats) => {
      this.categories = cats || [];
      this.cdr.detectChanges();
    });

    this.restaurantService.getAll().pipe(
      timeout(3000),
      catchError(() => of([]))
    ).subscribe((data) => {
      if (data && data.length > 0) {
        this.restaurants = data.map((r, idx) => ({
          ...r,
          imageUrl: this.formatImageUrl(r.imageUrl, r.name, r.cuisineType, idx),
          rating: r.rating || 0,
          cuisineType: r.cuisineType || ''
        }));
      } else {
        this.restaurants = [];
      }
      this.applyFilters();
      this.isLoading = false;
      this.cdr.detectChanges();
    });
  }

  formatImageUrl(url?: string, name: string = '', cuisine: string = '', idx: number = 0): string {
    if (url && (url.startsWith('http://') || url.startsWith('https://'))) return url;
    if (url && url.length > 5) return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;

    const text = (name + ' ' + cuisine).toLowerCase();
    if (text.includes('biryani')) return this.categoryImages['biryani'];
    if (text.includes('pizza')) return this.categoryImages['pizza'];
    if (text.includes('burger')) return this.categoryImages['burger'];
    if (text.includes('chinese') || text.includes('noodle')) return this.categoryImages['chinese'];
    if (text.includes('dessert') || text.includes('cake')) return this.categoryImages['desserts'];
    if (text.includes('dosa') || text.includes('south')) return this.categoryImages['south indian'];

    const fallbackList = Object.values(this.categoryImages);
    return fallbackList[idx % fallbackList.length];
  }

  toggleWishlist(restaurant: Restaurant, event: Event) {
    event.stopPropagation();
    this.wishlistService.toggleWishlist(restaurant);
    this.cdr.detectChanges();
  }

  isWishlisted(id: number): boolean {
    return this.wishlistService.isWishlisted(id);
  }

  selectCategoryByName(name: string | null) {
    this.selectedCategoryName = this.selectedCategoryName === name ? null : name;
    this.applyFilters();
  }

  toggleVegFilter() {
    this.vegOnlyFilter = !this.vegOnlyFilter;
    if (this.vegOnlyFilter) this.nonVegFilter = false;
    this.applyFilters();
  }

  toggleNonVegFilter() {
    this.nonVegFilter = !this.nonVegFilter;
    if (this.nonVegFilter) this.vegOnlyFilter = false;
    this.applyFilters();
  }

  toggleRatingFilter() {
    this.highRatingFilter = !this.highRatingFilter;
    this.applyFilters();
  }

  onSortChange(event: any) {
    this.sortBy = event.target.value;
    this.applyFilters();
  }

  applyFilters() {
    // Hide inactive / closed restaurants from customer list
    let result = this.restaurants.filter(r => r.isOpen !== false);

    if (this.searchQuery && this.searchQuery.trim()) {
      const tokens = this.searchQuery.trim().toLowerCase().split(/\s+/).filter(t => t.length > 0);
      result = result.filter(r => {
        const searchableText = `${r.name || ''} ${r.description || ''} ${r.cuisineType || ''} ${r.city || ''} ${r.address || ''} ${r.ownerName || ''}`.toLowerCase();
        return tokens.every(token => searchableText.includes(token));
      });
    }

    if (this.selectedCategoryName) {
      const cat = this.selectedCategoryName.toLowerCase();
      result = result.filter(r => 
        (r.cuisineType && r.cuisineType.toLowerCase().includes(cat)) ||
        r.name.toLowerCase().includes(cat)
      );
    }

    if (this.vegOnlyFilter) {
      result = result.filter(r => 
        (r.cuisineType && (r.cuisineType.toLowerCase().includes('veg') || r.cuisineType.toLowerCase().includes('south') || r.cuisineType.toLowerCase().includes('bakery')))
      );
    }

    if (this.nonVegFilter) {
      result = result.filter(r => 
        (r.cuisineType && (r.cuisineType.toLowerCase().includes('chicken') || r.cuisineType.toLowerCase().includes('biryani') || r.cuisineType.toLowerCase().includes('kebab') || r.cuisineType.toLowerCase().includes('non-veg')))
      );
    }

    if (this.highRatingFilter) {
      result = result.filter(r => (r.rating || 0) >= 4.3);
    }

    // Apply Sorting
    if (this.sortBy === 'rating') {
      result.sort((a, b) => (b.rating || 0) - (a.rating || 0));
    } else if (this.sortBy === 'name') {
      result.sort((a, b) => a.name.localeCompare(b.name));
    }

    this.filteredRestaurants = result;
    this.totalPages = Math.ceil(this.filteredRestaurants.length / this.pageSize) || 1;
    this.currentPage = 1;
    this.updatePaginatedList();
    this.cdr.detectChanges();
  }

  updatePaginatedList() {
    const start = (this.currentPage - 1) * this.pageSize;
    this.paginatedRestaurants = this.filteredRestaurants.slice(start, start + this.pageSize);
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.updatePaginatedList();
      this.cdr.detectChanges();
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePaginatedList();
      this.cdr.detectChanges();
    }
  }

  viewRestaurantMenu(id: number) {
    this.router.navigate(['/restaurant', id]);
  }
}

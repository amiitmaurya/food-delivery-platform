import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FoodItemService } from '../../../core/services/food-item.service';
import { CategoryService } from '../../../core/services/category.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { ToastService } from '../../../core/services/toast.service';
import { FoodItem, Category, Restaurant } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-food-item',
  templateUrl: './food-item.html',
  styleUrl: './food-item.css',
  standalone: false
})
export class FooditemComponent implements OnInit {
  private foodItemService = inject(FoodItemService);
  private categoryService = inject(CategoryService);
  private restaurantService = inject(RestaurantService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  foodItems: FoodItem[] = [];
  categories: Category[] = [];
  restaurants: Restaurant[] = [];
  isLoading = true;
  searchQuery = '';

  get filteredFoodItems(): FoodItem[] {
    if (!this.searchQuery || !this.searchQuery.trim()) {
      return this.foodItems;
    }
    const tokens = this.searchQuery.trim().toLowerCase().split(/\s+/);
    return this.foodItems.filter(item => {
      const text = `${item.name || ''} ${item.description || ''} ${item.price || ''} ${item.isVegetarian ? 'veg vegetarian' : 'nonveg non-veg'}`.toLowerCase();
      return tokens.every(token => text.includes(token));
    });
  }

  showModal = false;
  editingId = 0;

  formData = {
    name: '',
    description: '',
    price: '',
    offerPrice: '',
    isVegetarian: true,
    isAvailable: true,
    categoryId: 1,
    restaurantId: 1,
    imageUrl: ''
  };

  selectedFile: File | null = null;
  imagePreview: string | null = null;
  defaultDishImg = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    // Load Categories
    this.categoryService.getAll()
      .pipe(
        timeout(5000),
        catchError(err => {
          console.error('Category Error:', err);
          return of([]);
        })
      )
      .subscribe((cats: Category[]) => {
        this.categories = cats;
        this.cdr.detectChanges();
      });

    // Load Restaurants
    this.restaurantService.getAll()
      .pipe(
        timeout(5000),
        catchError(err => {
          console.error('Restaurant Error:', err);
          return of([]);
        })
      )
      .subscribe((res: Restaurant[]) => {
        this.restaurants = res;
        this.cdr.detectChanges();
      });

    // Load Food Items
    this.foodItemService.getAll()
      .pipe(
        timeout(5000)
      )
      .subscribe({
        next: (items: FoodItem[]) => {
          this.foodItems = items.map(item => ({
            ...item,
            isVegetarian: (item as any).isVegetarian !== undefined ? Boolean((item as any).isVegetarian) : Boolean((item as any).isVeg),
            imageUrl: this.formatImageUrl(item.imageUrl)
          }));

          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Food Item Error:', err);

          this.foodItems = [];
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  formatImageUrl(url?: string): string {
    if (!url) return this.defaultDishImg;
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;
  }

  openCreateModal() {
    this.editingId = 0;
    this.selectedFile = null;
    this.imagePreview = null;
    this.formData = {
      name: '',
      description: '',
      price: '',
      offerPrice: '',
      isVegetarian: true,
      isAvailable: true,
      categoryId: this.categories[0]?.id || 1,
      restaurantId: this.restaurants[0]?.id || 1,
      imageUrl: ''
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  openEditModal(item: any) {
    this.editingId = item.id;
    this.selectedFile = null;
    this.imagePreview = item.imageUrl ? this.formatImageUrl(item.imageUrl) : null;
    this.formData = {
      name: item.name,
      description: item.description || '',
      price: item.price,
      offerPrice: item.offerPrice || item.price,
      isVegetarian: item.isVegetarian !== undefined ? Boolean(item.isVegetarian) : Boolean(item.isVeg),
      isAvailable: item.isAvailable ?? true,
      categoryId: item.categoryId || 1,
      restaurantId: item.restaurantId || 1,
      imageUrl: item.imageUrl || ''
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  onFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(file);
    }
  }

  removeSelectedFile(): void {
    this.selectedFile = null;
    this.imagePreview = this.formData.imageUrl ? this.formatImageUrl(this.formData.imageUrl) : null;
    this.cdr.detectChanges();
  }

  saveFoodItem() {
    if (!this.formData.name || !this.formData.name.trim()) {
      this.toast.error('Please enter Dish Name');
      return;
    }

    if (this.formData.name.trim().length < 2) {
      this.toast.error('Dish Name must be at least 2 characters');
      return;
    }

    if (this.formData.name.trim().length > 100) {
      this.toast.error('Dish Name cannot exceed 100 characters');
      return;
    }

    if (!this.formData.restaurantId || Number(this.formData.restaurantId) <= 0) {
      this.toast.error('Please select a Restaurant');
      return;
    }

    if (!this.formData.categoryId || Number(this.formData.categoryId) <= 0) {
      this.toast.error('Please select a Category');
      return;
    }

    const priceNum = Number(this.formData.price);
    if (this.formData.price === '' || this.formData.price === null || this.formData.price === undefined || isNaN(priceNum) || priceNum <= 0) {
      this.toast.error('Please enter a valid Price (greater than ₹0)');
      return;
    }

    if (this.formData.offerPrice !== '' && this.formData.offerPrice !== null && this.formData.offerPrice !== undefined) {
      const offerNum = Number(this.formData.offerPrice);
      if (isNaN(offerNum) || offerNum < 0) {
        this.toast.error('Offer Price cannot be negative');
        return;
      }
      if (offerNum > priceNum) {
        this.toast.error('Offer Price cannot be greater than actual Price');
        return;
      }
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.toast.error('Description cannot exceed 500 characters');
      return;
    }

    const isVegFlag = Boolean(this.formData.isVegetarian);
    const payload = {
      categoryId: Number(this.formData.categoryId) || 1,
      restaurantId: Number(this.formData.restaurantId) || 1,
      name: this.formData.name,
      description: this.formData.description || '',
      price: Number(this.formData.price),
      offerPrice: Number(this.formData.offerPrice) || Number(this.formData.price),
      isVeg: isVegFlag,
      isVegetarian: isVegFlag,
      image: this.selectedFile ? null : (this.formData.imageUrl || this.defaultDishImg),
      imageUrl: this.selectedFile ? null : (this.formData.imageUrl || this.defaultDishImg),
      isAvailable: Boolean(this.formData.isAvailable)
    };

    if (this.editingId === 0) {
      this.foodItemService.create(payload as any).subscribe({
        next: (res: any) => {
          const newId = res?.id || res?.Id;
          if (this.selectedFile && newId) {
            this.foodItemService.uploadImage(newId, this.selectedFile).subscribe({
              next: () => {
                this.toast.success('Food item created successfully!');
                this.showModal = false;
                this.loadData();
              },
              error: () => {
                this.toast.success('Food item created successfully!');
                this.showModal = false;
                this.loadData();
              }
            });
          } else {
            this.toast.success('Food item created successfully!');
            this.showModal = false;
            this.loadData();
          }
        },
        error: (err) => {
          const msg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Failed to create food item');
          this.toast.error(msg);
        }
      });
    } else {
      this.foodItemService.update({ id: this.editingId, ...payload } as any).subscribe({
        next: () => {
          if (this.selectedFile) {
            this.foodItemService.uploadImage(this.editingId, this.selectedFile).subscribe({
              next: () => {
                this.toast.success('Food item updated successfully!');
                this.showModal = false;
                this.loadData();
              },
              error: () => {
                this.toast.success('Food item updated successfully!');
                this.showModal = false;
                this.loadData();
              }
            });
          } else {
            this.toast.success('Food item updated successfully!');
            this.showModal = false;
            this.loadData();
          }
        },
        error: (err) => {
          const msg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Failed to update food item');
          this.toast.error(msg);
        }
      });
    }
  }

  toggleStatus(item: FoodItem) {
    const newStatus = !item.isAvailable;
    const isVegFlag = item.isVegetarian !== undefined ? Boolean(item.isVegetarian) : Boolean((item as any).isVeg);
    const payload = {
      id: item.id,
      categoryId: item.categoryId || 1,
      restaurantId: item.restaurantId || 1,
      name: item.name,
      description: item.description || '',
      price: Number(item.price),
      offerPrice: Number(item.offerPrice) || Number(item.price),
      isVeg: isVegFlag,
      isVegetarian: isVegFlag,
      image: item.imageUrl || this.defaultDishImg,
      imageUrl: item.imageUrl || this.defaultDishImg,
      isAvailable: newStatus
    };

    this.foodItemService.update(payload as any).subscribe({
      next: () => {
        item.isAvailable = newStatus;
        this.toast.success(`Dish marked as ${newStatus ? 'Active (In Stock)' : 'Inactive (Out of Stock)'}`);
        this.cdr.detectChanges();
      },
      error: () => {
        this.toast.error('Failed to update status');
      }
    });
  }

  showDeleteModal = false;
  foodToDelete: FoodItem | null = null;
  isDeleting = false;

  openDeleteModal(item: FoodItem): void {
    this.foodToDelete = item;
    this.showDeleteModal = true;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (!this.foodToDelete) return;
    this.isDeleting = true;

    this.foodItemService.delete(this.foodToDelete.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.toast.success('Food item deleted successfully!');
        this.showDeleteModal = false;
        this.foodToDelete = null;
        this.loadData();
      },
      error: (err) => {
        this.isDeleting = false;
        this.toast.error(err.error?.message || 'Failed to delete food item');
      }
    });
  }

  onImgError(event: any) {
    event.target.src = this.defaultDishImg;
  }
}

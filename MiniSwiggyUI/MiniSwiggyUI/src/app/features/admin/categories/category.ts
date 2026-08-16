import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CategoryService } from '../../../core/services/category.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { ToastService } from '../../../core/services/toast.service';
import { Category } from '../../../core/models';
import { Restaurant } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-category',
  templateUrl: './category.html',
  styleUrl: './category.css',
  standalone: false
})
export class CategoryComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  private restaurantService = inject(RestaurantService);

  categories: Category[] = [];
  filteredCategories: Category[] = [];
  restaurants: Restaurant[] = [];
  
  isLoading = true;

  showModal = false;
  editingId = 0;
  formData = {
    restaurantId: 0,
    name: '',
    description: '',
    imageUrl: '',
    displayOrder: 1,
    isActive: true
  };

  searchQuery = '';
  defaultCatImg = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  formatImageUrl(url?: string): string {
    if (!url) return this.defaultCatImg;
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;
  }

  ngOnInit(): void {
    this.loadRestaurants();
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.categoryService.getAll()
      .pipe(
        timeout(5000)
      )
      .subscribe({
        next: (data: Category[]) => {
          this.categories = (data || []).map(cat => ({
            ...cat,
            imageUrl: this.formatImageUrl(cat.imageUrl)
          }));
          this.applySearch();

          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading categories:', err);

          this.categories = [];
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }
  loadRestaurants(): void {
    this.restaurantService.getAll().subscribe({
      next: (data) => {
        this.restaurants = data;
      },
      error: (err) => {
        console.error('Failed to load restaurants:', err);
        this.toast.error('Failed to load restaurants');
      }
    });
  }

  getRestaurantName(restaurantId?: number): string {
    if (!restaurantId) return 'N/A';
    const found = this.restaurants.find(r => r.id === restaurantId);
    return found ? found.name : `Restaurant #${restaurantId}`;
  }

  applySearch(): void {
    if (!this.searchQuery.trim()) {
      this.filteredCategories = [...this.categories];
    } else {
      const q = this.searchQuery.toLowerCase();
      this.filteredCategories = this.categories.filter(c =>
        c.name.toLowerCase().includes(q) || 
        (c.description && c.description.toLowerCase().includes(q)) ||
        this.getRestaurantName(c.restaurantId).toLowerCase().includes(q)
      );
    }
    this.cdr.detectChanges();
  }

  openCreateModal() {
    this.editingId = 0;
    this.selectedFile = null;
    this.imagePreview = null;
    this.formData = {
      restaurantId: this.restaurants.length > 0 ? this.restaurants[0].id : 0,
      name: '',
      description: '',
      imageUrl: this.defaultCatImg,
      displayOrder: this.categories.length + 1,
      isActive: true
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  openEditModal(cat: any) {
    this.editingId = cat.id;
    this.selectedFile = null;
    this.imagePreview = cat.imageUrl ? this.formatImageUrl(cat.imageUrl) : null;
    this.formData = {
      restaurantId: cat.restaurantId || (this.restaurants.length > 0 ? this.restaurants[0].id : 0),
      name: cat.name,
      description: cat.description || '',
      imageUrl: cat.imageUrl || this.defaultCatImg,
      displayOrder: cat.displayOrder || 1,
      isActive: cat.isActive ?? true
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

  saveCategory(): void {
    if (!this.formData.name || !this.formData.name.trim()) {
      this.toast.error('Please enter Category Name');
      return;
    }

    if (this.formData.name.trim().length < 2) {
      this.toast.error('Category Name must be at least 2 characters');
      return;
    }

    if (this.formData.name.trim().length > 100) {
      this.toast.error('Category Name cannot exceed 100 characters');
      return;
    }

    if (!this.formData.restaurantId || Number(this.formData.restaurantId) <= 0) {
      this.toast.error('Please select a Restaurant');
      return;
    }

    if (this.formData.displayOrder === undefined || this.formData.displayOrder === null || Number(this.formData.displayOrder) < 0) {
      this.toast.error('Please enter a valid Display Order');
      return;
    }

    if (this.formData.description && this.formData.description.length > 500) {
      this.toast.error('Description cannot exceed 500 characters');
      return;
    }

    if (!this.formData.imageUrl && !this.selectedFile) {
      this.formData.imageUrl = this.defaultCatImg;
    }

    if (this.editingId === 0) {
      this.categoryService.create(this.formData).subscribe({
        next: (res: any) => {
          const newId = res?.id || res?.Id;
          if (this.selectedFile && newId) {
            this.categoryService.uploadImage(newId, this.selectedFile).subscribe({
              next: () => {
                this.toast.success('Category created successfully!');
                this.showModal = false;
                this.loadCategories();
              },
              error: () => {
                this.toast.success('Category created successfully!');
                this.showModal = false;
                this.loadCategories();
              }
            });
          } else {
            this.toast.success('Category created successfully!');
            this.showModal = false;
            this.loadCategories();
          }
        },
        error: (err) => {
          const msg = err.error?.message || (typeof err.error === 'string' ? err.error : null) || 'Failed to create category';
          this.toast.error(msg);
        }
      });
    } else {
      this.categoryService.update(this.editingId, this.formData).subscribe({
        next: () => {
          if (this.selectedFile) {
            this.categoryService.uploadImage(this.editingId, this.selectedFile).subscribe({
              next: () => {
                this.toast.success('Category updated successfully!');
                this.showModal = false;
                this.loadCategories();
              },
              error: () => {
                this.toast.success('Category updated successfully!');
                this.showModal = false;
                this.loadCategories();
              }
            });
          } else {
            this.toast.success('Category updated successfully!');
            this.showModal = false;
            this.loadCategories();
          }
        },
        error: (err) => {
          this.toast.error('Failed to update category');
        }
      });
    }
  }

  toggleStatus(cat: Category) {
    const newStatus = !(cat.isActive ?? true);
    const payload = {
      id: cat.id,
      restaurantId: cat.restaurantId || 1,
      name: cat.name,
      description: cat.description || '',
      imageUrl: cat.imageUrl || '',
      displayOrder: cat.displayOrder || 1,
      isActive: newStatus
    };

    this.categoryService.update(cat.id, payload).subscribe({
      next: () => {
        cat.isActive = newStatus;
        this.toast.success(`Category marked as ${newStatus ? 'Active' : 'Inactive'}`);
        this.cdr.detectChanges();
      },
      error: () => {
        this.toast.error('Failed to update category status');
      }
    });
  }

  showDeleteModal = false;
  catToDelete: Category | null = null;
  isDeleting = false;

  openDeleteModal(cat: Category): void {
    this.catToDelete = cat;
    this.showDeleteModal = true;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (!this.catToDelete) return;
    this.isDeleting = true;

    this.categoryService.delete(this.catToDelete.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.toast.success('Category deleted successfully!');
        this.showDeleteModal = false;
        this.catToDelete = null;
        this.loadCategories();
      },
      error: (err) => {
        this.isDeleting = false;
        this.toast.error(err.error?.message || 'Failed to delete category');
      }
    });
  }

  onImgError(event: any) {
    event.target.src = this.defaultCatImg;
  }
}

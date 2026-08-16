import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { ToastService } from '../../../core/services/toast.service';
import { Restaurant } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-admin-restaurants',
  templateUrl: './admin-restaurants.html',
  styleUrl: './admin-restaurants.css',
  standalone: false
})
export class AdminRestaurantsComponent implements OnInit {
  private restaurantService = inject(RestaurantService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  restaurants: Restaurant[] = [];
  searchQuery = '';
  isLoading = true;
  showModal = false;
  editingId = 0;

  get filteredRestaurants(): Restaurant[] {
    if (!this.searchQuery || !this.searchQuery.trim()) {
      return this.restaurants;
    }
    const tokens = this.searchQuery.trim().toLowerCase().split(/\s+/);
    return this.restaurants.filter(r => {
      const text = `${r.name || ''} ${r.ownerName || ''} ${r.city || ''} ${r.cuisineType || ''} ${r.address || ''} ${r.mobileNumber || ''}`.toLowerCase();
      return tokens.every(token => text.includes(token));
    });
  }

  formData = {
    name: '',
    description: '',
    ownerName: '',
    mobileNumber: '',
    email: '',
    address: '',
    city: '',
    state: '',
    pincode: '',
    deliveryTime: '',
    deliveryCharge: '',
    minimumOrderAmount: '',
    averageCostForTwo: '',
    openingTime: '',
    closingTime: '',
    cuisineType: '',
    isOpen: true,
    imageUrl: ''
  };

  defaultFallbackImg = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  ngOnInit(): void {
    this.loadRestaurants();
  }

  formatImageUrl(url?: string): string {
    if (!url) return this.defaultFallbackImg;
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;
  }

  onImgError(event: any): void {
    event.target.src = this.defaultFallbackImg;
  }

  loadRestaurants(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.restaurantService.getAll()
      .pipe(
        timeout(5000),
        catchError(err => {
          console.error('Restaurant load error:', err);
          return of([]);
        })
      )
      .subscribe({
        next: (data) => {
          this.restaurants = (data || []).map(r => ({
            ...r,
            imageUrl: this.formatImageUrl(r.imageUrl)
          }));
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  openCreateModal(): void {
    this.editingId = 0;
    this.selectedFile = null;
    this.imagePreview = null;
    this.formData = {
      name: '',
      description: '',
      ownerName: '',
      mobileNumber: '',
      email: '',
      address: '',
      city: '',
      state: '',
      pincode: '',
      deliveryTime: '',
      deliveryCharge: '',
      minimumOrderAmount: '',
      averageCostForTwo: '',
      openingTime: '',
      closingTime: '',
      cuisineType: '',
      isOpen: true,
      imageUrl: ''
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  openEditModal(res: any): void {
    this.editingId = res.id;
    this.selectedFile = null;
    this.imagePreview = res.imageUrl ? this.formatImageUrl(res.imageUrl) : null;
    this.formData = {
      name: res.name || '',
      description: res.description || '',
      ownerName: res.ownerName || '',
      mobileNumber: res.mobileNumber || res.phoneNumber || '',
      email: res.email || '',
      address: res.address || '',
      city: res.city || '',
      state: res.state || '',
      pincode: res.pincode || '',
      deliveryTime: res.deliveryTime !== undefined && res.deliveryTime !== null ? res.deliveryTime : '',
      deliveryCharge: res.deliveryCharge !== undefined && res.deliveryCharge !== null ? res.deliveryCharge : '',
      minimumOrderAmount: res.minimumOrderAmount !== undefined && res.minimumOrderAmount !== null ? res.minimumOrderAmount : '',
      averageCostForTwo: res.averageCostForTwo !== undefined && res.averageCostForTwo !== null ? res.averageCostForTwo : '',
      openingTime: res.openingTime ? String(res.openingTime).slice(0, 5) : '',
      closingTime: res.closingTime ? String(res.closingTime).slice(0, 5) : '',
      cuisineType: res.cuisineType || '',
      isOpen: res.isOpen ?? true,
      imageUrl: res.imageUrl || ''
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

  onlyNumbers(event: KeyboardEvent): boolean {
    const charCode = event.which ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
      event.preventDefault();
      return false;
    }
    return true;
  }

  onPhoneInput(event: any): void {
    let val = (event.target.value || '').replace(/\D/g, '');
    if (val.startsWith('91') && val.length > 10) {
      val = val.substring(2);
    }
    if (val.length > 10) {
      val = val.substring(0, 10);
    }
    this.formData.mobileNumber = val;
    event.target.value = val;
  }

  saveRestaurant(): void {
    if (!this.formData.name || !this.formData.name.trim()) {
      this.toast.error('Please enter Restaurant Name');
      return;
    }

    if (!this.formData.cuisineType || !this.formData.cuisineType.trim()) {
      this.toast.error('Please enter Cuisine Type');
      return;
    }

    if (!this.formData.ownerName || !this.formData.ownerName.trim()) {
      this.toast.error('Please enter Owner Name');
      return;
    }

    if (!this.formData.mobileNumber || !this.formData.mobileNumber.trim()) {
      this.toast.error('Please enter Mobile Number');
      return;
    }

    if (!/^[6-9]\d{9}$/.test(this.formData.mobileNumber.trim())) {
      this.toast.error('Please enter a valid 10-digit Mobile Number');
      return;
    }

    if (this.formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.formData.email.trim())) {
      this.toast.error('Please enter a valid Email Address');
      return;
    }

    if (!this.formData.city || !this.formData.city.trim()) {
      this.toast.error('Please enter City');
      return;
    }

    if (!this.formData.address || !this.formData.address.trim()) {
      this.toast.error('Please enter Full Street Address');
      return;
    }

    if (this.formData.deliveryTime && Number(this.formData.deliveryTime) < 0) {
      this.toast.error('Delivery Time cannot be negative');
      return;
    }

    if (this.formData.deliveryCharge && Number(this.formData.deliveryCharge) < 0) {
      this.toast.error('Delivery Charge cannot be negative');
      return;
    }

    if (this.formData.minimumOrderAmount && Number(this.formData.minimumOrderAmount) < 0) {
      this.toast.error('Minimum Order Amount cannot be negative');
      return;
    }

    if (this.formData.averageCostForTwo && Number(this.formData.averageCostForTwo) < 0) {
      this.toast.error('Average Cost for Two cannot be negative');
      return;
    }

    const fullPayload = {
      name: this.formData.name.trim(),
      description: (this.formData.description || '').trim(),
      cuisineType: (this.formData.cuisineType || '').trim(),
      ownerName: (this.formData.ownerName || '').trim(),
      mobileNumber: (this.formData.mobileNumber || '').trim(),
      email: (this.formData.email || '').trim(),
      address: (this.formData.address || '').trim(),
      city: (this.formData.city || '').trim(),
      state: (this.formData.state || '').trim(),
      pincode: (this.formData.pincode || '').trim(),
      deliveryTime: this.formData.deliveryTime ? Number(this.formData.deliveryTime) : 0,
      deliveryCharge: this.formData.deliveryCharge ? Number(this.formData.deliveryCharge) : 0,
      minimumOrderAmount: this.formData.minimumOrderAmount ? Number(this.formData.minimumOrderAmount) : 0,
      averageCostForTwo: this.formData.averageCostForTwo ? Number(this.formData.averageCostForTwo) : 0,
      openingTime: this.formData.openingTime && this.formData.openingTime.length === 5 ? `${this.formData.openingTime}:00` : (this.formData.openingTime || '00:00:00'),
      closingTime: this.formData.closingTime && this.formData.closingTime.length === 5 ? `${this.formData.closingTime}:00` : (this.formData.closingTime || '00:00:00'),
      imageUrl: this.formData.imageUrl || '',
      rating: 4.5,
      isOpen: this.formData.isOpen,
      isActive: true
    };

    if (this.editingId === 0) {
      this.restaurantService.create(fullPayload as any).subscribe({
        next: (res: any) => {
          const newId = res?.id || res?.Id;
          if (this.selectedFile && newId) {
            this.restaurantService.uploadImage(newId, this.selectedFile).subscribe({
              next: () => {
                this.toast.success('Restaurant created successfully!');
                this.showModal = false;
                this.loadRestaurants();
              },
              error: () => {
                this.toast.success('Restaurant created successfully!');
                this.showModal = false;
                this.loadRestaurants();
              }
            });
          } else {
            this.toast.success('Restaurant created successfully!');
            this.showModal = false;
            this.loadRestaurants();
          }
        },
        error: (err) => {
          this.toast.error(this.getErrorMessage(err, 'Failed to create restaurant'));
        }
      });
    } else {
      const updatePayload = {
        id: this.editingId,
        ...fullPayload
      };
      this.restaurantService.update(this.editingId, updatePayload).subscribe({
        next: () => {
          if (this.selectedFile) {
            this.restaurantService.uploadImage(this.editingId, this.selectedFile).subscribe({
              next: () => {
                this.toast.success('Restaurant updated successfully!');
                this.showModal = false;
                this.loadRestaurants();
              },
              error: () => {
                this.toast.success('Restaurant updated successfully!');
                this.showModal = false;
                this.loadRestaurants();
              }
            });
          } else {
            this.toast.success('Restaurant updated successfully!');
            this.showModal = false;
            this.loadRestaurants();
          }
        },
        error: (err) => {
          this.toast.error(this.getErrorMessage(err, 'Failed to update restaurant'));
        }
      });
    }
  }

  private getErrorMessage(err: any, fallback: string): string {
    if (!err) return fallback;
    if (typeof err.error === 'string') return err.error;
    if (err.error?.errors) {
      const errs = err.error.errors;
      const firstKey = Object.keys(errs)[0];
      if (firstKey && Array.isArray(errs[firstKey]) && errs[firstKey].length > 0) {
        return errs[firstKey][0];
      }
    }
    return err.error?.message || err.error?.title || fallback;
  }

  toggleStatus(res: Restaurant): void {
    const newOpenStatus = !res.isOpen;
    const updatePayload = {
      id: res.id,
      name: res.name || '',
      description: res.description || '',
      cuisineType: res.cuisineType || '',
      ownerName: res.ownerName || '',
      mobileNumber: res.mobileNumber || '',
      email: res.email || '',
      address: res.address || '',
      city: res.city || '',
      state: res.state || '',
      pincode: res.pincode || '',
      deliveryTime: res.deliveryTime || 0,
      deliveryCharge: res.deliveryCharge || 0,
      minimumOrderAmount: res.minimumOrderAmount || 0,
      averageCostForTwo: res.averageCostForTwo || 0,
      openingTime: res.openingTime ? String(res.openingTime).slice(0, 5) : '00:00:00',
      closingTime: res.closingTime ? String(res.closingTime).slice(0, 5) : '00:00:00',
      imageUrl: res.imageUrl || '',
      rating: res.rating || 4.5,
      isOpen: newOpenStatus,
      isActive: true
    };

    this.restaurantService.update(res.id, updatePayload).subscribe({
      next: () => {
        res.isOpen = newOpenStatus;
        this.toast.success(`Restaurant marked as ${newOpenStatus ? 'Active (Open)' : 'Inactive (Closed)'}`);
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toast.error(err.error?.message || 'Failed to update restaurant status');
      }
    });
  }

  showDeleteModal = false;
  resToDelete: Restaurant | null = null;
  isDeleting = false;

  openDeleteModal(res: Restaurant): void {
    this.resToDelete = res;
    this.showDeleteModal = true;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (!this.resToDelete) return;
    this.isDeleting = true;

    this.restaurantService.delete(this.resToDelete.id).subscribe({
      next: (res: any) => {
        this.isDeleting = false;
        this.toast.success(res?.message || 'Restaurant deleted successfully!');
        this.showDeleteModal = false;
        this.resToDelete = null;
        this.loadRestaurants();
      },
      error: (err) => {
        this.isDeleting = false;
        const msg = err.error?.message || 'Failed to delete restaurant';
        this.toast.error(msg);
      }
    });
  }
}

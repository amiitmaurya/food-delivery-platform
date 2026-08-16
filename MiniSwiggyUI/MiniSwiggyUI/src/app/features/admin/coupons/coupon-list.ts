import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CouponService } from '../../../core/services/coupon.service';
import { ToastService } from '../../../core/services/toast.service';
import { Coupon } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-coupon-list',
  templateUrl: './coupon-list.html',
  styleUrl: './coupon-list.css',
  standalone: false
})
export class CouponListComponent implements OnInit {
  private couponService = inject(CouponService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  coupons: Coupon[] = [];
  isLoading = true;
  showModal = false;
  editingId = 0;

  formData: Omit<Coupon, 'id'> = {
    code: '',
    discountPercentage: 20,
    minOrderAmount: 199,
    maxDiscountAmount: 100,
    expiryDate: new Date(Date.now() + 30 * 86400000).toISOString().split('T')[0],
    isActive: true
  };

  ngOnInit(): void {
    this.loadCoupons();
  }

  loadCoupons(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.couponService.getAll().pipe(
      timeout(3000),
      catchError(() => of([]))
    ).subscribe({
      next: (data) => {
        this.coupons = data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.coupons = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openCreateModal(): void {
    this.editingId = 0;
    this.formData = {
      code: '',
      discountPercentage: 20,
      minOrderAmount: 199,
      maxDiscountAmount: 100,
      expiryDate: new Date(Date.now() + 30 * 86400000).toISOString().split('T')[0],
      isActive: true
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  openEditModal(coupon: Coupon): void {
    this.editingId = coupon.id;
    this.formData = {
      code: coupon.code,
      discountPercentage: coupon.discountPercentage,
      minOrderAmount: coupon.minOrderAmount,
      maxDiscountAmount: coupon.maxDiscountAmount,
      expiryDate: coupon.expiryDate ? coupon.expiryDate.split('T')[0] : new Date().toISOString().split('T')[0],
      isActive: coupon.isActive ?? true
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  saveCoupon(): void {
    if (!this.formData.code || !this.formData.code.trim()) {
      this.toast.error('Please enter Coupon Code');
      return;
    }

    if (this.formData.code.trim().length < 3) {
      this.toast.error('Coupon Code must be at least 3 characters');
      return;
    }

    if (this.formData.code.trim().length > 30) {
      this.toast.error('Coupon Code cannot exceed 30 characters');
      return;
    }

    const discountNum = Number(this.formData.discountPercentage);
    if (!this.formData.discountPercentage || isNaN(discountNum) || discountNum <= 0 || discountNum > 100) {
      this.toast.error('Please enter a valid Discount Percentage (1 to 100%)');
      return;
    }

    const maxDiscountNum = Number(this.formData.maxDiscountAmount);
    if (!this.formData.maxDiscountAmount || isNaN(maxDiscountNum) || maxDiscountNum <= 0) {
      this.toast.error('Please enter a valid Max Discount Cap (greater than ₹0)');
      return;
    }

    const minOrderNum = Number(this.formData.minOrderAmount);
    if (this.formData.minOrderAmount == null || isNaN(minOrderNum) || minOrderNum < 0) {
      this.toast.error('Please enter a valid Minimum Order Amount');
      return;
    }

    if (!this.formData.expiryDate) {
      this.toast.error('Please select Expiry Date');
      return;
    }

    const codeUpper = this.formData.code.trim().toUpperCase();

    if (this.editingId === 0) {
      this.couponService.create({ ...this.formData, code: codeUpper }).subscribe({
        next: () => {
          this.toast.success(`Coupon ${codeUpper} created successfully!`);
          this.showModal = false;
          this.loadCoupons();
        },
        error: (err) => {
          this.toast.error(err.error?.message || 'Failed to create coupon');
        }
      });
    } else {
      this.couponService.update(this.editingId, { id: this.editingId, ...this.formData, code: codeUpper }).subscribe({
        next: () => {
          this.toast.success(`Coupon ${codeUpper} updated!`);
          this.showModal = false;
          this.loadCoupons();
        },
        error: (err) => {
          this.toast.error(err.error?.message || 'Failed to update coupon');
        }
      });
    }
  }

  toggleStatus(coupon: Coupon): void {
    const newStatus = !coupon.isActive;
    const payload = {
      id: coupon.id,
      code: coupon.code,
      discountPercentage: coupon.discountPercentage,
      minOrderAmount: coupon.minOrderAmount,
      maxDiscountAmount: coupon.maxDiscountAmount,
      expiryDate: coupon.expiryDate,
      isActive: newStatus
    };

    this.couponService.update(coupon.id, payload).subscribe({
      next: () => {
        coupon.isActive = newStatus;
        this.toast.success(`Coupon marked as ${newStatus ? 'Active' : 'Inactive'}`);
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toast.error(err.error?.message || 'Failed to update coupon status');
      }
    });
  }

  showDeleteModal = false;
  couponToDelete: Coupon | null = null;
  isDeleting = false;

  openDeleteModal(coupon: Coupon): void {
    this.couponToDelete = coupon;
    this.showDeleteModal = true;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (!this.couponToDelete) return;
    this.isDeleting = true;

    this.couponService.delete(this.couponToDelete.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.toast.success('Coupon deleted successfully!');
        this.showDeleteModal = false;
        this.couponToDelete = null;
        this.loadCoupons();
      },
      error: (err) => {
        this.isDeleting = false;
        this.toast.error(err.error?.message || 'Failed to delete coupon');
      }
    });
  }

  copyCode(code: string): void {
    navigator.clipboard.writeText(code);
    this.toast.success(`Coupon code "${code}" copied to clipboard!`);
  }
}

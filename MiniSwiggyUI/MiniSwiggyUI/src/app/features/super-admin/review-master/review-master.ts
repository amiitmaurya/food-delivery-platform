import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../../core/services/toast.service';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-review-master',
  templateUrl: './review-master.html',
  styleUrl: './review-master.css',
  standalone: false
})
export class ReviewMasterComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  reviews: any[] = [];
  isLoading = true;
  searchQuery = '';
  selectedRatingFilter = 0;

  showDeleteModal = false;
  reviewToDelete: any = null;
  isDeleting = false;

  get filteredReviews(): any[] {
    let result = [...this.reviews];

    if (this.selectedRatingFilter > 0) {
      result = result.filter(r => Math.round(r.rating) === this.selectedRatingFilter);
    }

    if (this.searchQuery && this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase().trim();
      result = result.filter(r =>
        (r.userName || '').toLowerCase().includes(q) ||
        (r.foodName || '').toLowerCase().includes(q) ||
        (r.restaurantName || '').toLowerCase().includes(q) ||
        (r.comment || '').toLowerCase().includes(q)
      );
    }

    return result;
  }

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.http.get<any[]>('https://localhost:7241/api/Review/all')
      .pipe(
        timeout(5000),
        catchError(() => of([]))
      )
      .subscribe({
        next: (data) => {
          this.reviews = data || [];
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.reviews = [];
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  openDeleteModal(review: any): void {
    this.reviewToDelete = review;
    this.showDeleteModal = true;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (!this.reviewToDelete) return;
    this.isDeleting = true;

    this.http.delete(`https://localhost:7241/api/Review/${this.reviewToDelete.id}`).subscribe({
      next: () => {
        this.isDeleting = false;
        this.toast.success('Review deleted successfully from database.');
        this.showDeleteModal = false;
        this.reviewToDelete = null;
        this.loadReviews();
      },
      error: () => {
        this.isDeleting = false;
        this.toast.error('Failed to delete review.');
      }
    });
  }
}

import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { PaymentService, PaymentTransaction } from '../../../core/services/payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-payment-history',
  templateUrl: './payment-history.html',
  styleUrl: './payment-history.css',
  standalone: false
})
export class PaymentHistoryComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  payments: PaymentTransaction[] = [];
  filteredPayments: PaymentTransaction[] = [];
  isLoading = true;

  isAdminOrSuperAdmin = false;
  currentRole = '';
  userName = '';
  protected readonly Math = Math;

  // Filters
  searchTerm = '';
  statusFilter = 'ALL';
  methodFilter = 'ALL';

  // Pagination
  currentPage = 1;
  pageSize = 10;

  ngOnInit(): void {
    const user = this.authService.currentUserValue;
    this.currentRole = user?.role || 'Customer';
    this.userName = user?.fullName || 'User';
    this.isAdminOrSuperAdmin = this.currentRole === 'Admin' || this.currentRole === 'SuperAdmin';

    this.loadPayments();
  }

  loadPayments(): void {
    this.isLoading = true;

    const request$ = this.isAdminOrSuperAdmin 
      ? this.paymentService.getAllPayments() 
      : this.paymentService.getMyPayments();

    request$.subscribe({
      next: (data) => {
        this.payments = data || [];
        this.applyFilters();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load payment transactions', err);
        this.toast.error('Failed to load payment history');
        this.payments = [];
        this.filteredPayments = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters(): void {
    let list = [...this.payments];

    // Status filter
    if (this.statusFilter !== 'ALL') {
      const statusNum = Number(this.statusFilter);
      list = list.filter(p => p.paymentStatus === statusNum);
    }

    // Method filter
    if (this.methodFilter !== 'ALL') {
      const methodNum = Number(this.methodFilter);
      list = list.filter(p => p.paymentMethod === methodNum);
    }

    // Search term
    if (this.searchTerm.trim()) {
      const q = this.searchTerm.toLowerCase().trim();
      list = list.filter(p => 
        (p.orderNumber && p.orderNumber.toLowerCase().includes(q)) ||
        (p.transactionId && p.transactionId.toLowerCase().includes(q)) ||
        (p.customerName && p.customerName.toLowerCase().includes(q)) ||
        (p.customerEmail && p.customerEmail.toLowerCase().includes(q)) ||
        (p.amount && p.amount.toString().includes(q))
      );
    }

    this.filteredPayments = list;
    this.currentPage = 1;
    this.cdr.detectChanges();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.statusFilter = 'ALL';
    this.methodFilter = 'ALL';
    this.applyFilters();
  }

  // Summary Metrics
  getTotalAmount(): number {
    return this.filteredPayments.reduce((acc, p) => acc + (p.amount || 0), 0);
  }

  getPaidCount(): number {
    return this.filteredPayments.filter(p => p.paymentStatus === 2).length;
  }

  getPendingCount(): number {
    return this.filteredPayments.filter(p => p.paymentStatus === 1).length;
  }

  getCodTotal(): number {
    return this.filteredPayments
      .filter(p => p.paymentMethod === 1)
      .reduce((acc, p) => acc + (p.amount || 0), 0);
  }

  getUpiTotal(): number {
    return this.filteredPayments
      .filter(p => p.paymentMethod === 2)
      .reduce((acc, p) => acc + (p.amount || 0), 0);
  }

  // Pagination getters
  get paginatedPayments(): PaymentTransaction[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredPayments.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredPayments.length / this.pageSize) || 1;
  }

  setPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  // Formatting helpers
  getStatusBadgeClass(status: number): string {
    switch (status) {
      case 2: return 'badge-success';
      case 1: return 'badge-warning';
      case 3: return 'badge-danger';
      case 4: return 'badge-info';
      default: return 'badge-secondary';
    }
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Pending';
      case 2: return 'Paid / Success';
      case 3: return 'Failed';
      case 4: return 'Refunded';
      default: return 'Unknown';
    }
  }

  getMethodLabel(method: number): string {
    switch (method) {
      case 1: return 'Cash on Delivery';
      case 2: return 'Instant UPI';
      case 3: return 'Card';
      case 4: return 'Net Banking';
      case 5: return 'Wallet';
      case 6: return 'Razorpay';
      case 7: return 'Stripe';
      default: return 'Online Payment';
    }
  }

  getMethodIcon(method: number): string {
    switch (method) {
      case 1: return 'fa-solid fa-hand-holding-dollar';
      case 2: return 'fa-solid fa-qrcode';
      case 3: return 'fa-solid fa-credit-card';
      default: return 'fa-solid fa-money-bill-wave';
    }
  }
}

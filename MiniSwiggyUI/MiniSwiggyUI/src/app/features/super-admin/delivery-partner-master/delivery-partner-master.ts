import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../../core/services/toast.service';
import { DeliveryPartnerMasterItem } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-delivery-partner-master',
  templateUrl: './delivery-partner-master.html',
  styleUrl: './delivery-partner-master.css',
  standalone: false
})
export class DeliveryPartnerMasterComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  partners: DeliveryPartnerMasterItem[] = [];
  isLoading = true;
  searchQuery = '';

  showEditModal = false;
  editingPartner: DeliveryPartnerMasterItem | null = null;
  editFormData = {
    vehicleType: 'Bike',
    vehicleNumber: '',
    vehicleModel: '',
    licenseNumber: '',
    bankAccountHolder: '',
    bankName: '',
    accountNumber: '',
    ifscCode: '',
    upiId: '',
    isOnline: true
  };

  showDeleteModal = false;
  partnerToDelete: DeliveryPartnerMasterItem | null = null;
  isDeleting = false;

  get filteredPartners(): DeliveryPartnerMasterItem[] {
    if (!this.searchQuery || !this.searchQuery.trim()) {
      return this.partners;
    }
    const q = this.searchQuery.toLowerCase().trim();
    return this.partners.filter(p =>
      p.fullName.toLowerCase().includes(q) ||
      p.email.toLowerCase().includes(q) ||
      p.phoneNumber.toLowerCase().includes(q) ||
      p.vehicleNumber.toLowerCase().includes(q) ||
      p.vehicleModel.toLowerCase().includes(q)
    );
  }

  ngOnInit(): void {
    this.loadPartners();
  }

  loadPartners(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.http.get<DeliveryPartnerMasterItem[]>('https://localhost:7241/api/DeliveryPartner/all-partners')
      .pipe(
        timeout(5000),
        catchError((err) => {
          console.error('Failed to load partners:', err);
          return of([]);
        })
      )
      .subscribe({
        next: (data) => {
          this.partners = data || [];
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.partners = [];
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  openEditModal(partner: DeliveryPartnerMasterItem): void {
    this.editingPartner = partner;
    this.editFormData = {
      vehicleType: partner.vehicleType || '',
      vehicleNumber: partner.vehicleNumber || '',
      vehicleModel: partner.vehicleModel || '',
      licenseNumber: partner.licenseNumber || '',
      bankAccountHolder: partner.bankAccountHolder || '',
      bankName: partner.bankName || '',
      accountNumber: partner.accountNumber || '',
      ifscCode: partner.ifscCode || '',
      upiId: partner.upiId || '',
      isOnline: partner.isOnline
    };
    this.showEditModal = true;
    this.cdr.detectChanges();
  }

  savePartnerEdit(): void {
    if (!this.editingPartner) return;

    if (!this.editFormData.vehicleNumber || !this.editFormData.vehicleNumber.trim()) {
      this.toast.error('Please enter Vehicle Registration Number');
      return;
    }

    if (!this.editFormData.licenseNumber || !this.editFormData.licenseNumber.trim()) {
      this.toast.error('Please enter Driving License Number');
      return;
    }

    this.toast.success(`Delivery profile for ${this.editingPartner.fullName} updated!`);
    this.showEditModal = false;
    this.editingPartner = null;
    this.loadPartners();
  }

  toggleOnline(partner: DeliveryPartnerMasterItem): void {
    partner.isOnline = !partner.isOnline;
    this.toast.success(`${partner.fullName} status updated to ${partner.isOnline ? 'Online' : 'Offline'}`);
    this.cdr.detectChanges();
  }

  openDeleteModal(partner: DeliveryPartnerMasterItem): void {
    this.partnerToDelete = partner;
    this.showDeleteModal = true;
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (!this.partnerToDelete) return;
    this.isDeleting = true;

    setTimeout(() => {
      this.isDeleting = false;
      this.toast.success(`Partner '${this.partnerToDelete?.fullName}' removed from active fleet.`);
      this.showDeleteModal = false;
      this.partners = this.partners.filter(p => p.email !== this.partnerToDelete?.email);
      this.partnerToDelete = null;
      this.cdr.detectChanges();
    }, 400);
  }
}

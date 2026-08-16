import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AddressService } from '../../../core/services/address.service';
import { ToastService } from '../../../core/services/toast.service';
import { Address } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-address-list',
  templateUrl: './address-list.html',
  styleUrl: './address-list.css',
  standalone: false
})
export class AddressListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private addressService = inject(AddressService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  addresses: Address[] = [];
  isLoading = true;

  showModal = false;
  editingId = 0;

  formData = {
    fullName: '',
    phoneNumber: '',
    houseNo: '',
    street: '',
    landmark: '',
    city: '',
    state: '',
    pincode: '',
    addressType: 'HOME',
    isDefault: false
  };

  ngOnInit(): void {
    this.loadAddresses();

    this.route.queryParams.subscribe(params => {
      if (params['create'] === 'true') {
        this.openCreateModal();
      }
    });
  }

  loadAddresses(): void {
    this.isLoading = true;

    this.addressService.getAll().subscribe({
      next: (data: Address[]) => {
        this.addresses = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load addresses', err);

        this.addresses = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openCreateModal() {
    this.editingId = 0;
    this.formData = {
      fullName: '',
      phoneNumber: '',
      houseNo: '',
      street: '',
      landmark: '',
      city: '',
      state: '',
      pincode: '',
      addressType: 'HOME',
      isDefault: false
    };
    this.showModal = true;
    this.cdr.detectChanges();
  }

  openEditModal(addr: any) {
    this.editingId = addr.id;
    const parts = (addr.addressLine || '').split(',');
    this.formData = {
      fullName: addr.fullName || '',
      phoneNumber: addr.phoneNumber || '',
      houseNo: addr.houseNo || parts[0] || '',
      street: addr.street || parts.slice(1).join(',') || addr.addressLine,
      landmark: addr.landmark || '',
      city: addr.city || '',
      state: addr.state || '',
      pincode: addr.pincode || addr.postalCode || '',
      addressType: addr.addressType || 'HOME',
      isDefault: addr.isDefault || false
    };
    this.showModal = true;
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
    this.formData.phoneNumber = val;
    event.target.value = val;
  }

  saveAddress() {
    if (!this.formData.fullName || !this.formData.fullName.trim()) {
      this.toast.error('Please enter Full Name');
      return;
    }

    if (this.formData.fullName.trim().length < 3) {
      this.toast.error('Full Name must be at least 3 characters');
      return;
    }

    if (!this.formData.phoneNumber || !this.formData.phoneNumber.trim()) {
      this.toast.error('Please enter Phone Number');
      return;
    }

    if (!/^[6-9]\d{9}$/.test(this.formData.phoneNumber.trim())) {
      this.toast.error('Please enter a valid 10-digit Phone Number');
      return;
    }

    if (!this.formData.houseNo || !this.formData.houseNo.trim()) {
      this.toast.error('Please enter House / Flat Number');
      return;
    }

    if (!this.formData.street || !this.formData.street.trim()) {
      this.toast.error('Please enter Street / Area');
      return;
    }

    if (!this.formData.city || !this.formData.city.trim()) {
      this.toast.error('Please enter City');
      return;
    }

    if (this.formData.pincode && this.formData.pincode.trim() && !/^\d{6}$/.test(this.formData.pincode.trim())) {
      this.toast.error('Pincode must be a valid 6-digit number');
      return;
    }

    const fullLine = [this.formData.houseNo, this.formData.street, this.formData.landmark].filter(Boolean).join(', ');

    const addrPayload: Address = {
      id: this.editingId || Date.now(),
      fullName: this.formData.fullName.trim(),
      phoneNumber: this.formData.phoneNumber.trim(),
      houseNo: this.formData.houseNo.trim(),
      street: this.formData.street.trim(),
      landmark: (this.formData.landmark || '').trim(),
      city: this.formData.city.trim(),
      state: (this.formData.state || '').trim(),
      pincode: (this.formData.pincode || '').trim(),
      postalCode: (this.formData.pincode || '').trim(),
      addressLine: fullLine,
      addressType: this.formData.addressType || 'HOME',
      isDefault: this.formData.isDefault
    };

    if (this.editingId === 0) {
      this.addressService.create(addrPayload).subscribe({
        next: () => {
          this.toast.success('Address saved successfully!');
          this.showModal = false;
          this.loadAddresses();
        },
        error: (err) => {
          this.toast.error(err.error?.message || 'Failed to save address');
        }
      });
    } else {
      this.addressService.update(this.editingId, addrPayload).subscribe({
        next: () => {
          this.toast.success('Address updated!');
          this.showModal = false;
          this.loadAddresses();
        },
        error: (err) => {
          this.toast.error(err.error?.message || 'Failed to update address');
        }
      });
    }
  }

  deleteAddress(id: number) {
    if (!confirm('Delete this saved address?')) return;

    this.addressService.delete(id).subscribe({
      next: () => {
        this.toast.success('Address deleted');
        this.loadAddresses();
      },
      error: (err) => {
        this.toast.error(err.error?.message || 'Failed to delete address');
      }
    });
  }

  setDefault(id: number) {
    this.addressService.setDefault(id).subscribe({
      next: () => {
        this.toast.success('Default address updated!');
        this.loadAddresses();
      },
      error: (err) => {
        this.toast.error(err.error?.message || 'Failed to update default address');
      }
    });
  }
}

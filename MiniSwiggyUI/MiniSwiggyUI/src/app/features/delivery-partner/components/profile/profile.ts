import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DeliveryPartnerService } from '../../services/delivery-partner.service';
import { DeliveryProfile, UpdateDeliveryProfileRequest } from '../../models/delivery-partner.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-delivery-profile',
  standalone: false,
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class DeliveryProfileComponent implements OnInit {
  private deliveryService = inject(DeliveryPartnerService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  profile: DeliveryProfile | null = null;
  isLoading = true;
  isSaving = false;

  activeTab: 'profile' | 'vehicle' | 'license' | 'bank' = 'vehicle';

  // Toggle Edit Modes for cards
  editMode: Record<string, boolean> = {
    profile: false,
    vehicle: false,
    license: false,
    bank: false
  };

  defaultAvatar = 'https://ui-avatars.com/api/?name=Delivery+Partner&background=ff5200&color=fff';

  profileForm: UpdateDeliveryProfileRequest = {
    fullName: '',
    phoneNumber: '',
    profileImageUrl: '',
    vehicleType: 'Bike',
    vehicleNumber: '',
    vehicleModel: '',
    licenseNumber: '',
    licenseExpiryDate: '',
    bankAccountHolder: '',
    bankName: '',
    accountNumber: '',
    ifscCode: '',
    upiId: ''
  };

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    // Check localStorage cache for instant load
    const cached = localStorage.getItem('miniswiggy_delivery_profile');
    if (cached) {
      try {
        const parsed = JSON.parse(cached);
        this.profileForm = { ...this.profileForm, ...parsed };
        this.profile = { ...(this.profile || {} as any), ...this.profileForm };
        this.isLoading = false;
        this.cdr.detectChanges();
      } catch {}
    }

    setTimeout(() => {
      if (this.isLoading) {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    }, 400);

    this.deliveryService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        if (data) {
          this.syncFormWithProfile(data);
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onAvatarError(event: any): void {
    event.target.src = this.defaultAvatar;
  }

  private compressAndProcessImage(file: File, maxWidth = 300, maxHeight = 300): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onerror = () => reject('Failed to read file');
      reader.onload = (e: any) => {
        const img = new Image();
        img.onerror = () => reject('Failed to load image');
        img.onload = () => {
          let width = img.width;
          let height = img.height;
          if (width > height) {
            if (width > maxWidth) {
              height = Math.round((height * maxWidth) / width);
              width = maxWidth;
            }
          } else {
            if (height > maxHeight) {
              width = Math.round((width * maxHeight) / height);
              height = maxHeight;
            }
          }
          const canvas = document.createElement('canvas');
          canvas.width = width;
          canvas.height = height;
          const ctx = canvas.getContext('2d');
          if (ctx) {
            ctx.drawImage(img, 0, 0, width, height);
            resolve(canvas.toDataURL('image/jpeg', 0.8));
          } else {
            resolve(e.target.result);
          }
        };
        img.src = e.target.result;
      };
      reader.readAsDataURL(file);
    });
  }

  async onFileSelected(event: any): Promise<void> {
    const fileInput = event.target;
    const file = fileInput.files?.[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      this.toast.show('Image size should be less than 10MB');
      fileInput.value = '';
      return;
    }

    try {
      const compressedUrl = await this.compressAndProcessImage(file, 300, 300);
      this.profileForm.profileImageUrl = compressedUrl;
      if (this.profile) {
        this.profile.profileImageUrl = compressedUrl;
      }
      try {
        localStorage.setItem('miniswiggy_user_image', compressedUrl);
      } catch (err) {}

      this.saveProfileSection('Profile Picture', 'profile');
    } catch (err) {
      this.toast.show('Failed to process image. Please try another image.');
    } finally {
      fileInput.value = '';
      this.cdr.detectChanges();
    }
  }

  removeProfilePicture(): void {
    this.profileForm.profileImageUrl = '';
    if (this.profile) {
      this.profile.profileImageUrl = '';
    }
    try {
      localStorage.removeItem('miniswiggy_user_image');
    } catch (err) {}
    this.saveProfileSection('Profile Picture', 'profile');
    this.cdr.detectChanges();
  }

  syncFormWithProfile(data: DeliveryProfile): void {
    let savedLocalImage = '';
    try {
      savedLocalImage = localStorage.getItem('miniswiggy_user_image') || '';
    } catch (e) {}

    this.profileForm = {
      fullName: data.fullName || this.profileForm.fullName || '',
      phoneNumber: data.phoneNumber || this.profileForm.phoneNumber || '',
      profileImageUrl: data.profileImageUrl || this.profileForm.profileImageUrl || savedLocalImage,
      vehicleType: data.vehicleType || this.profileForm.vehicleType || 'Bike',
      vehicleNumber: (data.vehicleNumber || this.profileForm.vehicleNumber || '').toUpperCase(),
      vehicleModel: data.vehicleModel || this.profileForm.vehicleModel || '',
      licenseNumber: (data.licenseNumber || this.profileForm.licenseNumber || '').toUpperCase(),
      licenseExpiryDate: data.licenseExpiryDate || this.profileForm.licenseExpiryDate || '',
      bankAccountHolder: data.bankAccountHolder || this.profileForm.bankAccountHolder || '',
      bankName: data.bankName || this.profileForm.bankName || '',
      accountNumber: data.accountNumber || this.profileForm.accountNumber || '',
      ifscCode: (data.ifscCode || this.profileForm.ifscCode || '').toUpperCase(),
      upiId: data.upiId || this.profileForm.upiId || ''
    };

    if (this.profileForm.profileImageUrl) {
      try {
        localStorage.setItem('miniswiggy_user_image', this.profileForm.profileImageUrl);
      } catch (e) {}
    }
  }

  toggleEdit(tabKey: string, forceState?: boolean): void {
    this.editMode[tabKey] = forceState !== undefined ? forceState : !this.editMode[tabKey];
    this.cdr.detectChanges();
  }

  formatUppercaseFields(): void {
    if (this.profileForm.vehicleNumber) {
      this.profileForm.vehicleNumber = this.profileForm.vehicleNumber.toUpperCase();
    }
    if (this.profileForm.licenseNumber) {
      this.profileForm.licenseNumber = this.profileForm.licenseNumber.toUpperCase();
    }
    if (this.profileForm.ifscCode) {
      this.profileForm.ifscCode = this.profileForm.ifscCode.toUpperCase();
    }
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
    this.profileForm.phoneNumber = val;
    event.target.value = val;
  }

  saveProfileSection(sectionName: string, tabKey: string): void {
    this.formatUppercaseFields();

    if (tabKey === 'profile') {
      if (!this.profileForm.fullName || !this.profileForm.fullName.trim()) {
        this.toast.error('Please enter Full Name');
        return;
      }
      if (!this.profileForm.phoneNumber || !this.profileForm.phoneNumber.trim()) {
        this.toast.error('Please enter Phone Number');
        return;
      }
      if (!/^[6-9]\d{9}$/.test(this.profileForm.phoneNumber.trim())) {
        this.toast.error('Please enter a valid 10-digit Phone Number');
        return;
      }
    } else if (tabKey === 'vehicle') {
      if (!this.profileForm.vehicleNumber || !this.profileForm.vehicleNumber.trim()) {
        this.toast.error('Please enter Vehicle Number');
        return;
      }
    } else if (tabKey === 'license') {
      if (!this.profileForm.licenseNumber || !this.profileForm.licenseNumber.trim()) {
        this.toast.error('Please enter Driving License Number');
        return;
      }
    } else if (tabKey === 'bank') {
      if (!this.profileForm.bankAccountHolder || !this.profileForm.bankAccountHolder.trim()) {
        this.toast.error('Please enter Account Holder Name');
        return;
      }
      if (!this.profileForm.bankName || !this.profileForm.bankName.trim()) {
        this.toast.error('Please enter Bank Name');
        return;
      }
      if (!this.profileForm.accountNumber || !this.profileForm.accountNumber.trim()) {
        this.toast.error('Please enter Account Number');
        return;
      }
      if (!this.profileForm.ifscCode || !this.profileForm.ifscCode.trim()) {
        this.toast.error('Please enter IFSC Code');
        return;
      }
    }

    this.isSaving = true;

    // Instant local UI update
    if (this.profile) {
      this.profile = {
        ...this.profile,
        ...this.profileForm
      };
    } else {
      this.profile = { ...this.profileForm, id: 1, email: 'delivery@miniswiggy.com', isOnline: true } as any;
    }

    try {
      if (this.profileForm.profileImageUrl) {
        localStorage.setItem('miniswiggy_user_image', this.profileForm.profileImageUrl);
      }
      localStorage.setItem('miniswiggy_delivery_profile', JSON.stringify(this.profileForm));
    } catch (e) {}

    this.editMode[tabKey] = false;
    this.toast.show(`${sectionName} saved successfully!`);
    this.isSaving = false;
    this.cdr.detectChanges();

    // Background DB sync
    this.deliveryService.updateProfile(this.profileForm).subscribe({
      next: () => {},
      error: () => {}
    });
  }

  deleteSection(sectionKey: 'vehicle' | 'license' | 'bank' | 'profile'): void {
    if (!confirm(`Are you sure you want to delete/clear this ${sectionKey} master details?`)) return;

    if (sectionKey === 'vehicle') {
      this.profileForm.vehicleType = 'Bike';
      this.profileForm.vehicleNumber = '';
      this.profileForm.vehicleModel = '';
    } else if (sectionKey === 'license') {
      this.profileForm.licenseNumber = '';
      this.profileForm.licenseExpiryDate = '';
    } else if (sectionKey === 'bank') {
      this.profileForm.bankAccountHolder = '';
      this.profileForm.bankName = '';
      this.profileForm.accountNumber = '';
      this.profileForm.ifscCode = '';
      this.profileForm.upiId = '';
    }

    this.saveProfileSection(`${sectionKey.toUpperCase()} Master Cleared`, sectionKey);
  }
}

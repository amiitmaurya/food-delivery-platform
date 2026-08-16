import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { User } from '../../../core/models';

@Component({
  selector: 'app-edit-profile',
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css',
  standalone: false
})
export class EditProfileComponent implements OnInit {
  private authService = inject(AuthService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  profileData = {
    fullName: '',
    email: '',
    phoneNumber: '',
    profileImageUrl: ''
  };

  defaultAvatar = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';
  selectedFile: File | null = null;

  ngOnInit(): void {
    const user = this.authService.currentUserValue;
    if (user) {
      this.profileData = {
        fullName: user.fullName || '',
        email: user.email || '',
        phoneNumber: user.phoneNumber || '',
        profileImageUrl: this.formatImageUrl(user.profileImageUrl)
      };
    } else {
      const local = localStorage.getItem('user');
      if (local) {
        try {
          const parsed = JSON.parse(local);
          this.profileData = {
            fullName: parsed.fullName || '',
            email: parsed.email || '',
            phoneNumber: parsed.phoneNumber || '',
            profileImageUrl: this.formatImageUrl(parsed.profileImageUrl)
          };
        } catch (e) {}
      }
    }

    // Always fetch latest profile directly from Database to ensure phone number & fields are up to date
    this.authService.getProfile().subscribe({
      next: (dbProfile: any) => {
        if (dbProfile) {
          const dbPhone = dbProfile.phoneNumber || dbProfile.PhoneNumber || dbProfile.mobileNumber || dbProfile.MobileNumber || '';
          this.profileData.fullName = dbProfile.fullName || dbProfile.FullName || this.profileData.fullName;
          this.profileData.email = dbProfile.email || dbProfile.Email || this.profileData.email;
          if (dbPhone) {
            this.profileData.phoneNumber = dbPhone;
          }
          if (dbProfile.imageUrl || dbProfile.ImageUrl) {
            this.profileData.profileImageUrl = this.formatImageUrl(dbProfile.imageUrl || dbProfile.ImageUrl);
          }

          // Update current user cache as well
          const currentUser = this.authService.currentUserValue;
          if (currentUser) {
            this.authService.updateProfile({
              ...currentUser,
              fullName: this.profileData.fullName,
              phoneNumber: this.profileData.phoneNumber,
              email: this.profileData.email,
              profileImageUrl: this.profileData.profileImageUrl
            });
          }
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        console.warn('Failed to load profile from backend:', err);
      }
    });
  }

  formatImageUrl(url?: string): string {
    if (!url) return this.defaultAvatar;
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;
  }

  onFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (file) {
      this.selectedFile = file;
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
    this.profileData.phoneNumber = val;
    event.target.value = val;
  }

  saveProfile(): void {
    if (!this.profileData.fullName || !this.profileData.fullName.trim()) {
      this.toast.error('Please enter Full Name');
      return;
    }

    if (this.profileData.fullName.trim().length < 2) {
      this.toast.error('Full Name must be at least 2 characters');
      return;
    }

    if (!this.profileData.email || !this.profileData.email.trim()) {
      this.toast.error('Please enter Email Address');
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.profileData.email.trim())) {
      this.toast.error('Please enter a valid Email Address');
      return;
    }

    if (this.profileData.phoneNumber && this.profileData.phoneNumber.trim()) {
      if (!/^[6-9]\d{9}$/.test(this.profileData.phoneNumber.trim())) {
        this.toast.error('Please enter a valid 10-digit Phone Number');
        return;
      }
    }

    if (this.selectedFile) {
      this.authService.uploadProfileImage(this.selectedFile).subscribe({
        next: (res: any) => {
          const rawUrl = res?.imageUrl || res?.ImageUrl || res?.data?.imageUrl;
          if (rawUrl) {
            this.profileData.profileImageUrl = this.formatImageUrl(rawUrl);
          }
          this.performProfileUpdate();
        },
        error: (err) => {
          console.error(err);
          this.performProfileUpdate();
        }
      });
    } else {
      this.performProfileUpdate();
    }
  }

  private performProfileUpdate(): void {
    this.authService.updateProfileBackend({
      fullName: this.profileData.fullName,
      phoneNumber: this.profileData.phoneNumber
    }).subscribe({
      next: () => {},
      error: () => {}
    });

    const updatedUser: User = {
      fullName: this.profileData.fullName,
      email: this.profileData.email,
      phoneNumber: this.profileData.phoneNumber,
      profileImageUrl: this.profileData.profileImageUrl || this.defaultAvatar,
      role: this.authService.currentUserValue?.role || 'Customer'
    };

    this.authService.updateProfile(updatedUser);
    this.toast.success('★ Profile updated successfully!');
    this.cdr.detectChanges();
    this.router.navigate(['/restaurant']);
  }

  onAvatarError(event: any) {
    event.target.src = this.defaultAvatar;
  }
}

import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.html',
  styleUrl: './change-password.css',
  standalone: false
})
export class ChangePasswordComponent {
  private toast = inject(ToastService);
  private router = inject(Router);
  private authService = inject(AuthService);

  oldPassword = '';
  newPassword = '';
  confirmPassword = '';

  showOldPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;
  isSubmitting = false;

  toggleOldPassword() {
    this.showOldPassword = !this.showOldPassword;
  }

  toggleNewPassword() {
    this.showNewPassword = !this.showNewPassword;
  }

  toggleConfirmPassword() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onChangePassword() {
    if (!this.oldPassword || !this.oldPassword.trim()) {
      this.toast.error('Please enter Current Password');
      return;
    }

    if (!this.newPassword || !this.newPassword.trim()) {
      this.toast.error('Please enter New Password');
      return;
    }

    if (this.newPassword.length < 6) {
      this.toast.error('New Password must be at least 6 characters long');
      return;
    }

    if (!this.confirmPassword || !this.confirmPassword.trim()) {
      this.toast.error('Please enter Confirm Password');
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.toast.error('New Password and Confirm Password do not match');
      return;
    }

    this.isSubmitting = true;

    this.authService.changePassword({
      oldPassword: this.oldPassword.trim(),
      newPassword: this.newPassword.trim(),
      confirmPassword: this.confirmPassword.trim()
    }).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.toast.success(res?.message || '🔒 Password updated successfully in Database!');
        this.oldPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';

        setTimeout(() => {
          if (this.authService.isDeliveryPartner()) {
            this.router.navigate(['/delivery-partner/dashboard']);
          } else {
            this.router.navigate(['/restaurant']);
          }
        }, 1200);
      },
      error: (err) => {
        this.isSubmitting = false;
        const msg = err.error?.message || (typeof err.error === 'string' ? err.error : null) || 'Current password entered is incorrect!';
        this.toast.error('❌ ' + msg);
      }
    });
  }
}

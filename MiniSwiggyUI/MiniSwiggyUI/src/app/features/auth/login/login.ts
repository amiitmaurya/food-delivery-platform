import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrl: './login.css',
  standalone: false
})
export class LoginComponent {
  private authService = inject(AuthService);
  private toast = inject(ToastService);
  private router = inject(Router);

  loginModel = {
    email: '',
    password: ''
  };

  showPassword = false;
  isSubmitting = false;

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  login() {
    if (!this.loginModel.email || !this.loginModel.email.trim()) {
      this.toast.error('Please enter Email Address');
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.loginModel.email.trim())) {
      this.toast.error('Please enter a valid Email Address');
      return;
    }

    if (!this.loginModel.password) {
      this.toast.error('Please enter Password');
      return;
    }

    this.isSubmitting = true;

    this.authService.login({
      email: this.loginModel.email.trim(),
      password: this.loginModel.password
    }).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.toast.success(`Welcome back, ${response.fullName || 'User'}!`);
        
        if (this.authService.isSuperAdmin()) {
          this.router.navigate(['/superadmin/dashboard']);
        } else if (this.authService.isAdmin()) {
          this.router.navigate(['/dashboard']);
        } else if (this.authService.isDeliveryPartner()) {
          this.router.navigate(['/delivery-partner/dashboard']);
        } else {
          this.router.navigate(['/restaurant']);
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        const msg = error.error?.message || (typeof error.error === 'string' ? error.error : null) || 'Invalid Email or Password';
        this.toast.error(msg);
      }
    });
  }
}

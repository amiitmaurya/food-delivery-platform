import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrl: './register.css',
  standalone: false
})
export class RegisterComponent {
  private authService = inject(AuthService);
  private toast = inject(ToastService);
  private router = inject(Router);

  registerModel = {
    fullName: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: '',
    acceptTerms: false
  };

  showPassword = false;
  isSubmitting = false;

  togglePassword() {
    this.showPassword = !this.showPassword;
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
    this.registerModel.phoneNumber = val;
    event.target.value = val;
  }

  register() {
    if (!this.registerModel.fullName?.trim()) {
      this.toast.error('Please enter your full name.');
      return;
    }

    if (!this.registerModel.email?.trim()) {
      this.toast.error('Please enter your email address.');
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.registerModel.email.trim())) {
      this.toast.error('Please enter a valid email address.');
      return;
    }

    if (!this.registerModel.phoneNumber?.trim()) {
      this.toast.error('Please enter your phone number.');
      return;
    }

    if (!/^[6-9]\d{9}$/.test(this.registerModel.phoneNumber.trim())) {
      this.toast.error('Please enter a valid 10-digit Phone Number.');
      return;
    }

    if (!this.registerModel.password) {
      this.toast.error('Please enter a password.');
      return;
    }

    if (this.registerModel.password.length < 8) {
      this.toast.error('Password must be at least 8 characters long.');
      return;
    }

    if (!/[A-Z]/.test(this.registerModel.password)) {
      this.toast.error('Password must contain at least one uppercase letter.');
      return;
    }

    if (!/[a-z]/.test(this.registerModel.password)) {
      this.toast.error('Password must contain at least one lowercase letter.');
      return;
    }

    if (!/[0-9]/.test(this.registerModel.password)) {
      this.toast.error('Password must contain at least one number.');
      return;
    }

    if (!/[^a-zA-Z0-9]/.test(this.registerModel.password)) {
      this.toast.error('Password must contain at least one special character.');
      return;
    }

    if (!this.registerModel.confirmPassword) {
      this.toast.error('Please enter confirm password.');
      return;
    }

    if (this.registerModel.password !== this.registerModel.confirmPassword) {
      this.toast.error('Passwords do not match.');
      return;
    }

    if (!this.registerModel.acceptTerms) {
      this.toast.error('Please accept Terms & Conditions.');
      return;
    }

    this.isSubmitting = true;

    this.authService.register({
      fullName: this.registerModel.fullName.trim(),
      email: this.registerModel.email.trim(),
      phoneNumber: this.registerModel.phoneNumber.trim(),
      password: this.registerModel.password,
      confirmPassword: this.registerModel.confirmPassword
    }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.toast.success('Registration successful! Please log in.');
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.isSubmitting = false;
        const msg = error.error?.message || (typeof error.error === 'string' ? error.error : null) || 'Registration Failed';
        this.toast.error(msg);
      }
    });
  }
}

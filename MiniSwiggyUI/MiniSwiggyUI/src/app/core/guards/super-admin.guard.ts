import { Injectable, inject } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class SuperAdminGuard implements CanActivate {
  private authService = inject(AuthService);
  private router = inject(Router);

  canActivate(): boolean | UrlTree {
    if (this.authService.isLoggedIn() && (this.authService.isSuperAdmin() || this.authService.isAdmin())) {
      return true;
    }
    return this.router.parseUrl('/login');
  }
}

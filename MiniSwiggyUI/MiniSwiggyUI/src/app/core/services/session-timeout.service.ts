import { Injectable, inject, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, fromEvent, merge, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SessionTimeoutService {
  private authService = inject(AuthService);
  private router = inject(Router);
  private ngZone = inject(NgZone);

  // Inactivity timeout duration in milliseconds (15 minutes)
  private readonly INACTIVITY_LIMIT_MS = 15 * 60 * 1000;
  // Check interval for JWT expiry in ms (15 seconds)
  private readonly CHECK_INTERVAL_MS = 15 * 1000;

  private isSessionExpiredSubject = new BehaviorSubject<boolean>(false);
  isSessionExpired$ = this.isSessionExpiredSubject.asObservable();

  private activitySubscription?: Subscription;
  private timerHandle?: any;
  private expiryCheckIntervalHandle?: any;
  private lastActivityTimestamp = Date.now();

  constructor() {
    this.initActivityMonitoring();
    this.initPeriodicExpiryCheck();
  }

  /**
   * Initializes user interaction event listeners to detect inactivity.
   */
  private initActivityMonitoring(): void {
    if (typeof window === 'undefined') return;

    this.ngZone.runOutsideAngular(() => {
      const mouseEvents$ = fromEvent(window, 'mousemove');
      const clickEvents$ = fromEvent(window, 'mousedown');
      const keyEvents$ = fromEvent(window, 'keydown');
      const scrollEvents$ = fromEvent(window, 'scroll');
      const touchEvents$ = fromEvent(window, 'touchstart');

      const allEvents$ = merge(mouseEvents$, clickEvents$, keyEvents$, scrollEvents$, touchEvents$).pipe(
        debounceTime(500)
      );

      this.activitySubscription = allEvents$.subscribe(() => {
        this.lastActivityTimestamp = Date.now();
      });

      this.startInactivityTimer();
    });
  }

  private startInactivityTimer(): void {
    if (this.timerHandle) {
      clearInterval(this.timerHandle);
    }

    this.timerHandle = setInterval(() => {
      if (this.authService.isLoggedIn() && !this.isSessionExpiredSubject.value) {
        const inactiveTime = Date.now() - this.lastActivityTimestamp;
        if (inactiveTime >= this.INACTIVITY_LIMIT_MS) {
          this.ngZone.run(() => {
            this.triggerSessionTimeout();
          });
        }
      }
    }, 10000);
  }

  /**
   * Periodically checks if the JWT token has expired based on its 'exp' claim.
   */
  private initPeriodicExpiryCheck(): void {
    if (typeof window === 'undefined') return;

    this.expiryCheckIntervalHandle = setInterval(() => {
      if (this.authService.isLoggedIn() && !this.isSessionExpiredSubject.value) {
        const token = localStorage.getItem('token');
        if (token && this.isTokenExpired(token)) {
          this.ngZone.run(() => {
            this.triggerSessionTimeout();
          });
        }
      }
    }, this.CHECK_INTERVAL_MS);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const parts = token.split('.');
      if (parts.length < 2) return true;
      const payload = JSON.parse(atob(parts[1]));
      if (!payload || !payload.exp) return false;
      const expiryDate = payload.exp * 1000;
      return Date.now() >= expiryDate;
    } catch {
      return false;
    }
  }

  /**
   * Triggers the session timeout state: automatically logs out the user and shows the popup.
   */
  triggerSessionTimeout(): void {
    if (this.isSessionExpiredSubject.value) return;

    // Auto-logout the user immediately
    this.authService.logout();

    // Show the session expired popup
    this.isSessionExpiredSubject.next(true);
  }

  /**
   * Closes the session expired popup and navigates to the login screen.
   */
  dismissSessionExpired(): void {
    this.isSessionExpiredSubject.next(false);
    this.router.navigate(['/login']);
  }
}

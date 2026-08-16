import { Injectable, inject } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { SessionTimeoutService } from '../services/session-timeout.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private sessionTimeoutService = inject(SessionTimeoutService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');

    let headers = req.headers
      .set('Cache-Control', 'no-cache, no-store, must-revalidate')
      .set('Pragma', 'no-cache')
      .set('Expires', '0');

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    const cloned = req.clone({ headers });

    return next.handle(cloned).pipe(
      catchError((error: HttpErrorResponse) => {
        // If 401 Unauthorized and not on login or register endpoints
        if (error.status === 401) {
          const isAuthEndpoint = req.url.includes('/api/Auth/login') || req.url.includes('/api/Auth/register');
          if (!isAuthEndpoint) {
            this.sessionTimeoutService.triggerSessionTimeout();
          }
        }
        return throwError(() => error);
      })
    );
  }
}

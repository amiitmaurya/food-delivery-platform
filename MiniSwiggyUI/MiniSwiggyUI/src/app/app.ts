import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { SessionTimeoutService } from './core/services/session-timeout.service';

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  public router = inject(Router);
  public sessionTimeoutService = inject(SessionTimeoutService);

  onSessionExpiredOkay(): void {
    this.sessionTimeoutService.dismissSessionExpired();
  }
}

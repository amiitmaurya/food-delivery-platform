import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { SidebarService } from '../../../core/services/sidebar.service';
import { PermissionService } from '../../../core/services/permission.service';
import { Router, NavigationEnd } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class SidebarComponent implements OnInit, OnDestroy {
  authService = inject(AuthService);
  sidebarService = inject(SidebarService);
  permissionService = inject(PermissionService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  private subs = new Subscription();

  ngOnInit(): void {
    // Listen for permission updates and instantly refresh sidebar without page reloads
    this.subs.add(
      this.permissionService.myPermissions$.subscribe(() => {
        this.cdr.markForCheck();
        this.cdr.detectChanges();
      })
    );

    this.subs.add(
      this.authService.currentUser$.subscribe(() => {
        this.cdr.markForCheck();
        this.cdr.detectChanges();
      })
    );

    this.subs.add(
      this.router.events.subscribe(event => {
        if (event instanceof NavigationEnd) {
          this.sidebarService.close();
          this.cdr.markForCheck();
          this.cdr.detectChanges();
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  closeSidebar(): void {
    this.sidebarService.close();
  }
}

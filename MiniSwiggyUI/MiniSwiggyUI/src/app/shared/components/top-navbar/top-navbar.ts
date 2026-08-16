import { Component, OnInit, ChangeDetectorRef, inject, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { AddressService } from '../../../core/services/address.service';
import { ToastService } from '../../../core/services/toast.service';
import { Address, User } from '../../../core/models';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

import { SidebarService } from '../../../core/services/sidebar.service';

@Component({
  selector: 'app-top-navbar',
  standalone: false,
  templateUrl: './top-navbar.html',
  styleUrls: ['./top-navbar.css']
})
export class TopNavbarComponent implements OnInit {
  cartService = inject(CartService);
  authService = inject(AuthService);
  sidebarService = inject(SidebarService);
  private addressService = inject(AddressService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  userName = 'Guest';
  userRole = 'Customer';
  userImage = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';
  location = 'Connaught Place, New Delhi';
  searchQuery = '';
  showDropdown = false;

  // Compact Popover State
  showLocationDrawer = false;
  addresses: Address[] = [];
  selectedAddressId: number = 0;

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.updateUserInfo(user);
    });

    this.addressService.addressUpdated$.subscribe(() => {
      this.loadAddresses();
    });

    this.loadAddresses();
  }

  updateUserInfo(user: User | null): void {
    // 1. Get current logged in user from AuthService / Storage
    let savedUser: any = null;
    try {
      const uStr = localStorage.getItem('user');
      if (uStr) savedUser = JSON.parse(uStr);
    } catch {}

    const activeUser = user || savedUser || this.authService.currentUserValue;

    // 2. Check role from active authenticated session
    const isDelivery = this.authService.isDeliveryPartner() || 
      (activeUser?.role && activeUser.role.toString().toLowerCase().includes('delivery'));

    const isAdmin = this.authService.isAdmin() || 
      (activeUser?.role && activeUser.role.toString().toLowerCase() === 'admin');

    // 3. Set Role
    if (isAdmin) {
      this.userRole = 'Admin';
    } else if (isDelivery) {
      this.userRole = 'Delivery Partner';
    } else if (activeUser?.role) {
      this.userRole = activeUser.role;
    } else if (this.authService.isLoggedIn()) {
      this.userRole = 'Customer';
    } else {
      this.userRole = 'Guest';
    }

    // 4. Set Name
    if (activeUser?.fullName && activeUser.fullName !== 'User') {
      this.userName = activeUser.fullName;
    } else if (isDelivery) {
      let localDeliveryProfile: any = null;
      try {
        const cached = localStorage.getItem('miniswiggy_delivery_profile');
        if (cached) localDeliveryProfile = JSON.parse(cached);
      } catch {}
      this.userName = localDeliveryProfile?.fullName || 'Delivery Partner';
    } else if (isAdmin) {
      this.userName = 'System Admin';
    } else {
      this.userName = activeUser?.email ? activeUser.email.split('@')[0] : 'Guest User';
    }

    // 5. Set Image
    let localImage = '';
    try {
      localImage = localStorage.getItem('miniswiggy_user_image') || '';
    } catch {}

    if (isDelivery && localImage) {
      this.userImage = localImage;
    } else if (activeUser?.profileImageUrl) {
      this.userImage = this.formatImageUrl(activeUser.profileImageUrl);
    } else {
      this.userImage = `https://ui-avatars.com/api/?name=${encodeURIComponent(this.userName)}&background=ff5200&color=fff`;
    }

    this.cdr.detectChanges();
  }

  // loadAddresses(): void {
  //   this.addressService.getAll().pipe(
  //     catchError(() => of([]))
  //   ).subscribe({
  //     next: (data) => {
  //       if (data && data.length > 0) {
  //         this.addresses = data;
  //         const defaultAddr = data.find(a => a.isDefault) || data[0];
  //         this.selectedAddressId = defaultAddr.id;
  //         this.location = `${defaultAddr.street || defaultAddr.houseNo || defaultAddr.addressLine || 'Connaught Place'}, ${defaultAddr.city}`;
  //       } else {
  //         this.loadAddresses();
  //       }
  //       this.cdr.detectChanges();
  //     },
  //     error: () => this.loadAddresses()
  //   });
  // }

  loadAddresses(): void {
    this.addressService.getAll().subscribe({
      next: (data: Address[]) => {

        this.addresses = data ?? [];

        if (this.addresses.length > 0) {

          const defaultAddress =
            this.addresses.find(x => x.isDefault) ??
            this.addresses[0];

          this.selectedAddressId = defaultAddress.id;

          const streetPart = defaultAddress.street || defaultAddress.addressLine || 'Main Street';
          const housePart = defaultAddress.houseNo && !streetPart.includes(defaultAddress.houseNo) ? `${defaultAddress.houseNo}, ` : '';
          this.location = `${housePart}${streetPart}, ${defaultAddress.city}`;

        } else {

          // No address in DB
          this.location = 'Select delivery location';
          this.getCurrentLocation();

        }

        this.cdr.detectChanges();
      },
      error: (err) => {

        console.error(err);

        this.addresses = [];
        this.location = 'Select delivery location';

        this.getCurrentLocation();

        this.cdr.detectChanges();
      }
    });
  }

  openLocationDrawer() {
    this.showLocationDrawer = !this.showLocationDrawer;
    if (this.showLocationDrawer) {
      this.loadAddresses();
    }
    this.cdr.detectChanges();
  }

  closeLocationDrawer() {
    this.showLocationDrawer = false;
    this.cdr.detectChanges();
  }

  selectAddress(addr: Address) {
    this.selectedAddressId = addr.id;
    this.location = `${addr.street || addr.houseNo || addr.addressLine || 'Main Market'}, ${addr.city}`;
    this.toast.success(`Delivery address changed to ${this.location}`);
    this.closeLocationDrawer();
  }

  goToAddAddressPage() {
    this.closeLocationDrawer();
    this.router.navigate(['/addresses'], { queryParams: { create: true } });
  }

  toggleSidebar() {
    this.sidebarService.toggle();
  }

  toggleDropdown(event?: Event) {
    if (event) {
      event.stopPropagation();
    }
    this.showDropdown = !this.showDropdown;
    if (this.showDropdown) {
      this.updateUserInfo(this.authService.currentUserValue);
    }
  }

  closeDropdown() {
    this.showDropdown = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target) return;

    // Check if clicked outside profile dropdown
    const isProfileDropdownClicked = target.closest('.profile-dropdown-wrapper');
    if (!isProfileDropdownClicked && this.showDropdown) {
      this.showDropdown = false;
      this.cdr.detectChanges();
    }

    // Check if clicked outside location drawer
    const isLocationBadgeClicked = target.closest('.location-popover-wrapper');
    if (!isLocationBadgeClicked && this.showLocationDrawer) {
      this.showLocationDrawer = false;
      this.cdr.detectChanges();
    }
  }

  @HostListener('document:keydown.escape')
  onEscapePress(): void {
    if (this.showDropdown || this.showLocationDrawer) {
      this.showDropdown = false;
      this.showLocationDrawer = false;
      this.cdr.detectChanges();
    }
  }

  logout() {
    this.showDropdown = false;
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  goToCart() {
    this.router.navigate(['/cart']);
  }

  private searchDebounceTimer: any;

  onSmartSearch(): void {
    clearTimeout(this.searchDebounceTimer);
    this.searchDebounceTimer = setTimeout(() => {
      const q = this.searchQuery ? this.searchQuery.trim() : '';
      if (q) {
        this.router.navigate(['/restaurant'], { queryParams: { q: q }, queryParamsHandling: 'merge' });
      } else {
        this.router.navigate(['/restaurant'], { queryParams: { q: null }, queryParamsHandling: 'merge' });
      }
    }, 120);
  }

  clearSearch(): void {
    this.searchQuery = '';
    clearTimeout(this.searchDebounceTimer);
    this.router.navigate(['/restaurant'], { queryParams: { q: null }, queryParamsHandling: 'merge' });
  }

  onSearch(): void {
    const q = this.searchQuery ? this.searchQuery.trim() : '';
    if (q) {
      this.router.navigate(['/restaurant'], { queryParams: { q: q } });
    } else {
      this.router.navigate(['/restaurant']);
    }
  }

  getCurrentLocation() {
    if (!navigator.geolocation) {
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        const lat = position.coords.latitude;
        const lng = position.coords.longitude;

        this.http.get<any>(
          `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}`
        ).pipe(
          timeout(2000),
          catchError(() => of(null))
        ).subscribe({
          next: (data) => {
            if (data && data.address) {
              const address = data.address;
              const locationName =
                address.suburb ||
                address.city ||
                address.town ||
                address.village ||
                'Connaught Place, New Delhi';
              this.location = locationName;
              this.toast.info(`GPS Location: ${locationName}`);
              this.cdr.detectChanges();
            }
          }
        });
      },
      () => {},
      { enableHighAccuracy: false, timeout: 2500 }
    );
  }

  formatImageUrl(url?: string): string {
    if (!url) return 'https://ui-avatars.com/api/?name=User&background=ff5200&color=fff';
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) return url;
    return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;
  }

  onImgError(event: any): void {
    if (event && event.target) {
      (event.target as HTMLImageElement).src = 'https://ui-avatars.com/api/?name=User&background=ff5200&color=fff';
    }
  }
}

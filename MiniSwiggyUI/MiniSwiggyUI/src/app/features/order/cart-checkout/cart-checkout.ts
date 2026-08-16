import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { OrderService } from '../../../core/services/order.service';
import { AddressService } from '../../../core/services/address.service';
import { CouponService } from '../../../core/services/coupon.service';
import { ToastService } from '../../../core/services/toast.service';
import { CartItem, Address } from '../../../core/models';
import { RestaurantService } from '../../../core/services/restaurant.service';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-cart-checkout',
  templateUrl: './cart-checkout.html',
  styleUrl: './cart-checkout.css',
  standalone: false
})
export class CartCheckoutComponent implements OnInit {
  cartService = inject(CartService);
  private authService = inject(AuthService);
  private orderService = inject(OrderService);
  private addressService = inject(AddressService);
  private couponService = inject(CouponService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private restaurantService = inject(RestaurantService);

  cartItems: CartItem[] = [];
  addresses: Address[] = [];
  selectedAddressId: number = 0;

  couponCode = '';
  appliedCoupon: string | null = null;
  discountAmount = 0;
  deliveryFee = 0;
  taxAmount = 0;

  paymentMethod: 'COD' | 'UPI' = 'COD';
  upiId: string = 'amitmaury921@ybl';
  isPlacingOrder = false;

  getUpiUrl(): string {
    const amount = this.getGrandTotal();
    return `upi://pay?pa=${this.upiId}&pn=MiniSwiggy&am=${amount}&cu=INR&tn=Order%20Payment`;
  }

  copyUpiId(): void {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(this.upiId);
    }
    this.toast.success('UPI ID copied: ' + this.upiId);
  }

  showAddressModal = false;
  editingAddressId = 0;

  // Separate individual fields matching Saved Addresses
  newAddress = {
    fullName: '',
    phoneNumber: '',
    houseNo: '',
    street: '',
    landmark: '',
    city: '',
    state: '',
    pincode: '',
    addressType: 'HOME',
    isDefault: false
  };

  defaultFallbackImg = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';

  ngOnInit(): void {
    this.cartService.cartItems$.subscribe(items => {
      this.cartItems = items;

      if (items.length > 0) {
        this.loadRestaurantDetails();
      }

      this.cdr.detectChanges();
    });

    this.loadAddresses();
  }

  loadRestaurantDetails() {
    const restaurantId = this.cartItems[0]?.restaurantId;
    if (!restaurantId) return;

    this.restaurantService.getById(restaurantId).subscribe({
      next: (res) => {
        this.deliveryFee = Number(res.deliveryCharge) || 0;
        this.cdr.detectChanges();
      }
    });
  }

  loadAddresses(): void {
    this.addressService.getAll().subscribe({
      next: (data) => {
        this.addresses = data || [];

        if (this.addresses.length > 0) {
          const defaultAddr =
            this.addresses.find(a => a.isDefault) ?? this.addresses[0];

          this.selectedAddressId = defaultAddr.id;
        }

        this.cdr.detectChanges();
      },
      error: () => {
        this.addresses = [];
        this.toast.error('Failed to load addresses');
        this.cdr.detectChanges();
      }
    });
  }

  setAsPrimary(addr: Address, event: Event) {
    event.stopPropagation();
    this.selectedAddressId = addr.id;
    this.addressService.setDefault(addr.id).subscribe({
      next: () => {
        this.toast.success(`'${addr.addressType || 'Home'}' set as Primary Delivery Address!`);
        this.loadAddresses();
      },
      error: () => {
        this.loadAddresses();
      }
    });
  }

  openCreateAddressModal() {
    this.editingAddressId = 0;
    this.newAddress = {
      fullName: '',
      phoneNumber: '',
      houseNo: '',
      street: '',
      landmark: '',
      city: '',
      state: '',
      pincode: '',
      addressType: 'HOME',
      isDefault: this.addresses.length === 0
    };
    this.showAddressModal = true;
    this.cdr.detectChanges();
  }

  openEditAddressModal(addr: any, event: Event) {
    event.stopPropagation();
    this.editingAddressId = addr.id;
    const parts = (addr.addressLine || '').split(',');
    this.newAddress = {
      fullName: addr.fullName || '',
      phoneNumber: addr.phoneNumber || '',
      houseNo: addr.houseNo || parts[0] || '',
      street: addr.street || parts.slice(1).join(',') || addr.addressLine,
      landmark: addr.landmark || '',
      city: addr.city || '',
      state: addr.state || '',
      pincode: addr.pincode || addr.postalCode || '',
      addressType: addr.addressType || 'HOME',
      isDefault: addr.isDefault || false
    };
    this.showAddressModal = true;
    this.cdr.detectChanges();
  }

  deleteAddress(id: number, event: Event) {
    event.stopPropagation();

    if (!confirm('Delete this address?')) return;

    this.addressService.delete(id).subscribe({
      next: () => {
        this.toast.success('Address deleted');
        this.loadAddresses();
      },
      error: () => {
        this.toast.error('Failed to delete address');
      }
    });
  }

  getItemPrice(item: CartItem): number {
    const price = Number(item.foodItemPrice);
    return Number.isFinite(price) ? price : 0;
  }

  getItemQty(item: CartItem): number {
    const qty = Number(item.quantity);
    return Number.isFinite(qty) ? qty : 0;
  }

  getItemTotal(item: CartItem): number {
    return this.getItemPrice(item) * this.getItemQty(item);
  }

  updateQty(item: CartItem, delta: number) {
    const currentQty = this.getItemQty(item);
    this.cartService.updateQuantity(item.id || item.foodItemId, currentQty + delta);
  }

  removeItem(item: CartItem) {
    this.cartService.removeItem(item.id || item.foodItemId);
  }

  clearAll() {
    this.cartService.clearCart();
  }

  applyCouponCode() {
    if (!this.couponCode.trim()) return;

    const code = this.couponCode.trim().toUpperCase();
    const subtotal = this.getSubtotal();

    this.couponService.applyCoupon(code, subtotal).subscribe({
      next: (res) => {
        if (res && res.isValid) {
          this.discountAmount = Number(res.discountAmount || res.discount || 0);
          this.appliedCoupon = code;
          this.toast.success(res.message || `🎉 Coupon ${code} applied successfully!`);
        } else {
          this.toast.error(res?.message || 'Invalid or expired coupon');
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        const msg = err.error?.message || (typeof err.error === 'string' ? err.error : null) || 'Invalid or expired coupon';
        this.toast.error(msg);
      }
    });
  }

  removeCoupon() {
    this.appliedCoupon = null;
    this.discountAmount = 0;
    this.couponCode = '';
    this.toast.info('Coupon removed');
  }

  getSubtotal(): number {
    if (!this.cartItems || this.cartItems.length === 0) return 0;
    return this.cartItems.reduce((acc, i) => acc + this.getItemTotal(i), 0);
  }

  getGrandTotal(): number {
    const subtotal = this.getSubtotal();
    if (subtotal === 0) return 0;
    this.taxAmount = subtotal * 0.05;
    const total = subtotal + this.deliveryFee + this.taxAmount - this.discountAmount;
    return Math.max(total, 0);
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
    this.newAddress.phoneNumber = val;
    event.target.value = val;
  }

  saveNewAddress() {
    if (!this.newAddress.fullName || !this.newAddress.fullName.trim()) {
      this.toast.error('Please enter Full Name');
      return;
    }

    if (this.newAddress.fullName.trim().length < 3) {
      this.toast.error('Full Name must be at least 3 characters');
      return;
    }

    if (!this.newAddress.phoneNumber || !this.newAddress.phoneNumber.trim()) {
      this.toast.error('Please enter Phone Number');
      return;
    }

    if (!/^[6-9]\d{9}$/.test(this.newAddress.phoneNumber.trim())) {
      this.toast.error('Please enter a valid 10-digit Phone Number');
      return;
    }

    if (!this.newAddress.houseNo || !this.newAddress.houseNo.trim()) {
      this.toast.error('Please enter House / Flat Number');
      return;
    }

    if (!this.newAddress.street || !this.newAddress.street.trim()) {
      this.toast.error('Please enter Street / Area');
      return;
    }

    if (!this.newAddress.city || !this.newAddress.city.trim()) {
      this.toast.error('Please enter City');
      return;
    }

    if (this.newAddress.pincode && this.newAddress.pincode.trim() && !/^\d{6}$/.test(this.newAddress.pincode.trim())) {
      this.toast.error('Pincode must be a valid 6-digit number');
      return;
    }

    const fullLine = [this.newAddress.houseNo, this.newAddress.street, this.newAddress.landmark].filter(Boolean).join(', ');

    const addrPayload: Address = {
      id: this.editingAddressId || Date.now(),
      fullName: this.newAddress.fullName.trim(),
      phoneNumber: this.newAddress.phoneNumber.trim(),
      houseNo: this.newAddress.houseNo.trim(),
      street: this.newAddress.street.trim(),
      landmark: (this.newAddress.landmark || '').trim(),
      city: this.newAddress.city.trim(),
      state: (this.newAddress.state || '').trim(),
      pincode: (this.newAddress.pincode || '').trim(),
      postalCode: (this.newAddress.pincode || '').trim(),
      addressLine: fullLine,
      addressType: this.newAddress.addressType || 'HOME',
      isDefault: this.newAddress.isDefault
    };

    if (this.editingAddressId === 0) {
      this.addressService.create(addrPayload).subscribe({
        next: (created: any) => {
          this.toast.success('Address saved successfully!');
          this.showAddressModal = false;
          this.addressService.getAll().subscribe({
            next: (data) => {
              this.addresses = data || [];
              const newlyAdded = (created && created.id) ? this.addresses.find(a => a.id === created.id) : this.addresses[this.addresses.length - 1];
              if (newlyAdded) {
                this.selectedAddressId = newlyAdded.id;
              } else if (this.addresses.length > 0) {
                this.selectedAddressId = this.addresses[0].id;
              }
              this.cdr.detectChanges();
            }
          });
        },
        error: (err) => {
          this.toast.error(err.error?.message || 'Failed to save address');
        }
      });
    } else {
      this.addressService.update(this.editingAddressId, addrPayload).subscribe({
        next: () => {
          this.toast.success('Address updated!');
          this.showAddressModal = false;
          this.loadAddresses();
        },
        error: (err) => {
          this.toast.error(err.error?.message || 'Failed to update address');
        }
      });
    }
  }

  formatImageUrl(url?: string): string {
    if (!url) return this.defaultFallbackImg;
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;
  }

  onImgError(event: any) {
    event.target.src = this.defaultFallbackImg;
  }

  placeOrder() {
    if (this.cartItems.length === 0) {
      this.toast.error('Your cart is empty');
      return;
    }

    const selectedAddress =
      this.addresses.find(a => a.id === this.selectedAddressId);

    if (!selectedAddress) {
      this.toast.error('Please select Delivery Address');
      return;
    }

    const rawPhone = (selectedAddress.phoneNumber || this.authService.currentUserValue?.phoneNumber || '').trim();
    if (!rawPhone) {
      this.toast.error('Please provide a contact Phone Number for delivery');
      return;
    }

    if (!/^[6-9]\d{9}$/.test(rawPhone)) {
      this.toast.error('Please provide a valid 10-digit Phone Number for delivery');
      return;
    }

    this.isPlacingOrder = true;

    const paymentEnumMap: Record<string, number> = {
      COD: 1,
      UPI: 2
    };

    const fullAddr = selectedAddress.addressLine || `${selectedAddress.houseNo ? selectedAddress.houseNo + ', ' : ''}${selectedAddress.street || ''}${selectedAddress.landmark ? ', ' + selectedAddress.landmark : ''}, ${selectedAddress.city}${selectedAddress.state ? ', ' + selectedAddress.state : ''} - ${selectedAddress.pincode || selectedAddress.postalCode || ''}`;

    const payload = {
      deliveryAddress: fullAddr,
      phoneNumber: rawPhone,
      paymentMethod: paymentEnumMap[this.paymentMethod],
      couponCode: this.appliedCoupon || undefined,
      discountAmount: this.discountAmount > 0 ? this.discountAmount : undefined
    };

    this.orderService.placeOrder(payload).subscribe({
      next: () => {
        this.isPlacingOrder = false;
        this.cartService.clearCart();
        this.toast.success('Order placed successfully');
        this.router.navigate(['/my-orders']);
      },
      error: () => {
        this.isPlacingOrder = false;
        this.toast.error('Failed to place order');
      }
    });
  }

  continueShopping() {
    this.router.navigate(['/restaurant']);
  }
}

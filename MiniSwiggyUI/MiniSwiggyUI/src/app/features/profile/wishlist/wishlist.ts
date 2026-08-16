import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { WishlistService } from '../../../core/services/wishlist.service';
import { Restaurant } from '../../../core/models';

@Component({
  selector: 'app-wishlist',
  templateUrl: './wishlist.html',
  styleUrl: './wishlist.css',
  standalone: false
})
export class WishlistComponent implements OnInit {
  wishlistService = inject(WishlistService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  wishlistItems: Restaurant[] = [];
  defaultImage = 'https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg';

  ngOnInit(): void {
    this.wishlistService.loadWishlist();
    this.wishlistService.wishlist$.subscribe(items => {
      this.wishlistItems = items || [];
      this.cdr.detectChanges();
    });
  }

  formatImageUrl(url?: string): string {
    if (!url || url.trim() === '' || url.includes('No_Image_Available')) {
      return this.defaultImage;
    }
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    return `https://localhost:7241${url.startsWith('/') ? '' : '/'}${url}`;
  }

  onImgError(event: any): void {
    event.target.src = this.defaultImage;
  }

  removeFromWishlist(id: number, event?: Event) {
    if (event) event.stopPropagation();
    this.wishlistService.removeFromWishlist(id);
  }

  viewMenu(id: number) {
    this.router.navigate(['/restaurant', id]);
  }
}

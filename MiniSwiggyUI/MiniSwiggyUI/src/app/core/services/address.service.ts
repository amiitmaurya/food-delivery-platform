import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, map, tap, catchError, of } from 'rxjs';
import { Address } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AddressService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7241/api/Address';

  private addressUpdatedSubject = new BehaviorSubject<boolean>(true);
  public addressUpdated$ = this.addressUpdatedSubject.asObservable();

  triggerAddressUpdated() {
    this.addressUpdatedSubject.next(true);
  }

  private mapFromBackend(a: any): Address {
    const house = (a.houseNo || '').trim();
    const street = (a.street || '').trim();
    const city = (a.city || '').trim();
    const state = (a.state || '').trim();
    const pin = (a.pincode || a.postalCode || '').trim();

    // Clean address line without repeated parts
    let line = '';
    if (house && street && street.toLowerCase().includes(house.toLowerCase())) {
      line = street;
    } else if (house && street) {
      line = `${house}, ${street}`;
    } else {
      line = street || house || 'Main Street';
    }

    const tag = (a.landmark && (a.landmark.toUpperCase() === 'WORK' || a.landmark.toUpperCase() === 'OFFICE')) ? 'OFFICE' : (a.landmark === 'OTHER' ? 'OTHER' : 'HOME');

    return {
      id: Number(a.id || Date.now()),
      fullName: a.fullName || 'Customer',
      phoneNumber: a.phoneNumber || '',
      houseNo: house,
      street: street,
      landmark: a.landmark || tag,
      city: city,
      state: state,
      pincode: pin,
      postalCode: pin,
      addressLine: line,
      addressType: tag,
      isDefault: Boolean(a.isDefault)
    };
  }

  private mapToBackend(a: any) {
    const tag = a.addressType || a.landmark || 'HOME';
    return {
      fullName: a.fullName || 'Customer',
      phoneNumber: a.phoneNumber || '9999999999',
      houseNo: (a.houseNo || '').trim() || '1',
      street: (a.street || a.addressLine || '').trim() || 'Main Street',
      landmark: tag,
      city: (a.city || '').trim() || 'Campierganj',
      state: (a.state || '').trim() || 'Uttar Pradesh',
      pincode: String(a.pincode || a.postalCode || '273158').trim(),
      isDefault: Boolean(a.isDefault)
    };
  }

  getAll(): Observable<Address[]> {
    return this.http.get<any[]>(this.apiUrl).pipe(
      map(list => Array.isArray(list) ? list.map(a => this.mapFromBackend(a)) : []),
      catchError(() => of([]))
    );
  }

  create(address: any): Observable<any> {
    const payload = this.mapToBackend(address);
    return this.http.post(this.apiUrl, payload).pipe(
      tap(() => this.triggerAddressUpdated())
    );
  }

  update(id: number, address: any): Observable<any> {
    const payload = { id, ...this.mapToBackend(address) };
    return this.http.put(`${this.apiUrl}/${id}`, payload).pipe(
      tap(() => this.triggerAddressUpdated())
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.triggerAddressUpdated())
    );
  }

  setDefault(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/default/${id}`, {}).pipe(
      tap(() => this.triggerAddressUpdated())
    );
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ProductReferenceDto } from '../api/payment-api-types';

@Injectable({ providedIn: 'root' })
export class ProductCatalog {
  private readonly http = inject(HttpClient);

  async addToCart(dto: ProductReferenceDto): Promise<void> {
    await firstValueFrom(this.http.post<void>('/api/payment/cart/items', dto));
  }
}

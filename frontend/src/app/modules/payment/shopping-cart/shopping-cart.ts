import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CheckoutDto, ProductReferenceDto } from '../api/payment-api-types';

@Injectable({ providedIn: 'root' })
export class ShoppingCart {
  private readonly http = inject(HttpClient);

  async removeItem(dto: ProductReferenceDto): Promise<void> {
    await firstValueFrom(
      this.http.delete<void>(`/api/payment/cart/items/${dto.type}/${dto.entityId}`),
    );
  }

  async checkout(couponCode?: string, giftCardId?: string): Promise<void> {
    const dto: CheckoutDto = { couponCode: couponCode ?? null, giftCardId: giftCardId ?? null };
    await firstValueFrom(this.http.post<void>('/api/payment/cart/checkout', dto));
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PurchaseGiftCardDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/gift-cards';

@Injectable({ providedIn: 'root' })
export class GiftCards {
  private readonly http = inject(HttpClient);

  async purchase(dto: PurchaseGiftCardDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(BASE_URL, dto));
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateCouponDto } from '../api/payment-api-types';

@Injectable({ providedIn: 'root' })
export class Coupons {
  private readonly http = inject(HttpClient);

  async createCoupon(dto: CreateCouponDto): Promise<void> {
    await firstValueFrom(this.http.post<void>('/api/payment/coupons', dto));
  }
}

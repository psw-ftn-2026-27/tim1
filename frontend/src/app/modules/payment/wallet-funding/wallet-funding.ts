import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { RequestTopUpDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/wallet';

@Injectable({ providedIn: 'root' })
export class WalletFunding {
  private readonly http = inject(HttpClient);

  async requestTopUp(dto: RequestTopUpDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/top-up-requests`, dto));
  }
}

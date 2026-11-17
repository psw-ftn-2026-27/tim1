import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { RejectRequestDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/wallet/requests';

@Injectable({ providedIn: 'root' })
export class WalletRequestReview {
  private readonly http = inject(HttpClient);

  async approveTopUp(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/top-ups/${id}/approve`, {}));
  }

  async rejectTopUp(id: string, dto: RejectRequestDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/top-ups/${id}/reject`, dto));
  }

  async approveWithdrawal(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/withdrawals/${id}/approve`, {}));
  }

  async rejectWithdrawal(id: string, dto: RejectRequestDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/withdrawals/${id}/reject`, dto));
  }
}

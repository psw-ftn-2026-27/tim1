import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/follow-requests';

@Injectable({ providedIn: 'root' })
export class FollowRequesting {
  private readonly http = inject(HttpClient);

  async send(recipientId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${recipientId}`, {}));
  }

  async delete(requestId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${requestId}`));
  }
}

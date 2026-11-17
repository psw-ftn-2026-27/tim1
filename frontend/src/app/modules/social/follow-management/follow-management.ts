import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social';

@Injectable({ providedIn: 'root' })
export class FollowManagement {
  private readonly http = inject(HttpClient);

  async removeFollower(requestId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/followers/${requestId}`));
  }

  async stopFollowing(requestId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/following/${requestId}`));
  }

  async sendFollowRequest(recipientId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/follow-requests/${recipientId}`, {}));
  }
}

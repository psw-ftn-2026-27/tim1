import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubMembership {
  private readonly http = inject(HttpClient);

  async accept(clubId: string, requesterId: string): Promise<void> {
    await firstValueFrom(
      this.http.put<void>(`${BASE_URL}/${clubId}/membership-requests/${requesterId}/accept`, {}),
    );
  }

  async reject(clubId: string, requesterId: string): Promise<void> {
    await firstValueFrom(
      this.http.put<void>(`${BASE_URL}/${clubId}/membership-requests/${requesterId}/reject`, {}),
    );
  }
}

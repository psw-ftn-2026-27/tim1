import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubBrowsing {
  private readonly http = inject(HttpClient);

  async requestMembership(clubId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${clubId}/membership-requests`, {}));
  }

  async withdrawMembershipRequest(clubId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${clubId}/membership-requests/mine`));
  }
}

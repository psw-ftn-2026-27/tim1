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

  async promoteToModerator(clubId: string, memberId: string): Promise<void> {
    await firstValueFrom(
      this.http.put<void>(`${BASE_URL}/${clubId}/members/${memberId}/promote`, {}),
    );
  }

  async transferOwnership(clubId: string, memberId: string): Promise<void> {
    await firstValueFrom(
      this.http.put<void>(`${BASE_URL}/${clubId}/members/${memberId}/transfer-ownership`, {}),
    );
  }

  async removeMember(clubId: string, memberId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${clubId}/members/${memberId}`));
  }
}

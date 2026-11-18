import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubInviting {
  private readonly http = inject(HttpClient);

  async invite(clubId: string, userId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${clubId}/invitations/${userId}`, {}));
  }

  async accept(clubId: string): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${clubId}/invitations/mine/accept`, {}));
  }

  async decline(clubId: string): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${clubId}/invitations/mine/decline`, {}));
  }
}

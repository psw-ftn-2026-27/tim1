import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/games/location-challenges';

@Injectable({ providedIn: 'root' })
export class LocationChallengeSolving {
  private readonly http = inject(HttpClient);

  async activate(challengeId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${challengeId}/activate`, {}));
  }
}

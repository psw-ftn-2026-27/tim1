import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TourPreferenceDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tour-preferences';

@Injectable({ providedIn: 'root' })
export class TourPreferenceEditing {
  private readonly http = inject(HttpClient);

  async save(dto: TourPreferenceDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/mine`, dto));
  }

  async delete(): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/mine`));
  }
}

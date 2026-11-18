import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SaveLocationChallengeDto } from '../api/games-api-types';

const BASE_URL = '/api/games/location-challenges';

@Injectable({ providedIn: 'root' })
export class LocationChallengeManagement {
  private readonly http = inject(HttpClient);

  async create(dto: SaveLocationChallengeDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async update(id: string, dto: SaveLocationChallengeDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }

  async retire(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/retire`, {}));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, {}));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }

  // The backend serves this from its ChallengeImages group; it lives here because the form
  // of this group uploads the photos. Multipart form with the field name the controller
  // binds (IFormFile file).
  async uploadImage(file: File): Promise<string> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<string>('/api/games/images', body));
  }
}

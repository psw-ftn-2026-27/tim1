import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AddKeypointDto, UpdateKeypointDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class KeypointAuthoring {
  private readonly http = inject(HttpClient);

  async add(tourId: string, dto: AddKeypointDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${tourId}/keypoints`, dto));
  }

  async update(tourId: string, keypointId: string, dto: UpdateKeypointDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${tourId}/keypoints/${keypointId}`, dto));
  }

  async remove(tourId: string, keypointId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${tourId}/keypoints/${keypointId}`));
  }

  async setVisibility(tourId: string, keypointId: string, publiclyVisible: boolean): Promise<void> {
    await firstValueFrom(
      this.http.put<void>(`${BASE_URL}/${tourId}/keypoints/${keypointId}/visibility`, {
        publiclyVisible,
      }),
    );
  }

  // Sends one image and returns its new id.
  async uploadImage(file: File): Promise<string> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/keypoint-images`, body));
  }
}

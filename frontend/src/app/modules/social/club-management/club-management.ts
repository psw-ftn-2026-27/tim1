import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ClubDto, CreateClubDto, UpdateClubDto } from '../api/social-api-types';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubManagement {
  private readonly http = inject(HttpClient);

  async create(dto: CreateClubDto): Promise<ClubDto> {
    return firstValueFrom(this.http.post<ClubDto>(BASE_URL, dto));
  }

  async update(id: string, dto: UpdateClubDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BlogDto, CreateBlogDto } from '../api/social-api-types';

const BASE_URL = '/api/social/blogs';

@Injectable({ providedIn: 'root' })
export class BlogAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateBlogDto): Promise<BlogDto> {
    return firstValueFrom(this.http.post<BlogDto>(BASE_URL, dto));
  }

  // Multipart form with the field name the controller binds (IFormFile file).
  async uploadImage(file: File): Promise<string> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/images`, body));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }
}

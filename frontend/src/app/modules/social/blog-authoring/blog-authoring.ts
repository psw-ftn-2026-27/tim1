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

  async uploadImage(file: File): Promise<string> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/images`, body));
  }

  async getUnattachedImage(imageId: string): Promise<Blob> {
    return firstValueFrom(this.http.get(`${BASE_URL}/images/${imageId}`, { responseType: 'blob' }));
  }

  async getOwnBlogImage(blogId: string, imageId: string): Promise<Blob> {
    return firstValueFrom(
      this.http.get(`${BASE_URL}/mine/${blogId}/images/${imageId}`, { responseType: 'blob' }),
    );
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }
}

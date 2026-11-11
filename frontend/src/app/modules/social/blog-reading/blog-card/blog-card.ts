import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { BlogDto } from '../../api/social-api-types';
import { BlogDescription } from '../blog-description/blog-description';

@Component({
  imports: [BlogDescription, ProtectedImage, RouterLink],
  selector: 'app-blog-card',
  styleUrl: './blog-card.scss',
  templateUrl: './blog-card.html',
})
export class BlogCard {
  readonly blog = input.required<BlogDto>();

  // The server sends dates as ISO text; this shows them in the reader's local time.
  protected formatDate(value: string): string {
    return new Date(value).toLocaleString('sr-Latn');
  }
}

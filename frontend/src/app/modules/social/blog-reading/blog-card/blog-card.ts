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

  // The server sends dates as ISO text; this shows the date in the reader's local time.
  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { dateStyle: 'medium' });
  }

  // A positive score gets a plus sign, a negative one a real minus sign.
  protected formatScore(score: number): string {
    if (score > 0) {
      return `+${score}`;
    }
    return score < 0 ? `−${Math.abs(score)}` : '0';
  }
}

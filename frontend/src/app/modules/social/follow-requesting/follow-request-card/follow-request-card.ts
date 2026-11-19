import { Component, input, output } from '@angular/core';
import { FollowRequestDto } from '../../api/social-api-types';

@Component({
  selector: 'app-follow-request-card',
  styleUrl: './follow-request-card.scss',
  templateUrl: './follow-request-card.html',
})
export class FollowRequestCard {
  readonly request = input.required<FollowRequestDto>();
  readonly disabled = input(false);
  readonly deleteClicked = output<string>();

  // The server sends dates as ISO text; this shows the date in the reader's local time.
  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { dateStyle: 'medium' });
  }
}

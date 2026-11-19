import { Component, input, output } from '@angular/core';
import { ReceivedFollowRequestDto } from '../../api/social-api-types';

@Component({
  selector: 'app-received-request-card',
  styleUrl: './received-request-card.scss',
  templateUrl: './received-request-card.html',
})
export class ReceivedRequestCard {
  readonly request = input.required<ReceivedFollowRequestDto>();
  readonly disabled = input(false);
  readonly acceptClicked = output<string>();
  readonly rejectClicked = output<string>();

  // The server sends dates as ISO text; this shows the date in the reader's local time.
  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { dateStyle: 'medium' });
  }
}

import { Component, input, output } from '@angular/core';
import { FollowingDto } from '../../api/social-api-types';

@Component({
  selector: 'app-following-card',
  styleUrl: './following-card.scss',
  templateUrl: './following-card.html',
})
export class FollowingCard {
  readonly following = input.required<FollowingDto>();
  readonly disabled = input(false);
  readonly stopClicked = output<string>();
}

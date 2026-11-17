import { Component, input, output } from '@angular/core';
import { FollowerDto } from '../../api/social-api-types';

@Component({
  selector: 'app-follower-card',
  styleUrl: './follower-card.scss',
  templateUrl: './follower-card.html',
})
export class FollowerCard {
  readonly follower = input.required<FollowerDto>();
  readonly disabled = input(false);
  readonly removeClicked = output<string>();
  readonly sendClicked = output<string>();
}

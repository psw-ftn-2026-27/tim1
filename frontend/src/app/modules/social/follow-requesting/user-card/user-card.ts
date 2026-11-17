import { Component, input, output } from '@angular/core';
import { UserToFollowDto } from '../../api/social-api-types';

@Component({
  selector: 'app-user-card',
  styleUrl: './user-card.scss',
  templateUrl: './user-card.html',
})
export class UserCard {
  readonly user = input.required<UserToFollowDto>();
  readonly disabled = input(false);
  readonly sendClicked = output<string>();
}

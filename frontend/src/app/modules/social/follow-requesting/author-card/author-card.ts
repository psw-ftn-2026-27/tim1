import { Component, input, output } from '@angular/core';
import { AuthorToFollowDto } from '../../api/social-api-types';

@Component({
  selector: 'app-author-card',
  styleUrl: './author-card.scss',
  templateUrl: './author-card.html',
})
export class AuthorCard {
  readonly author = input.required<AuthorToFollowDto>();
  readonly disabled = input(false);
  readonly sendClicked = output<string>();
}

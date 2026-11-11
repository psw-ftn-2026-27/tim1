import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ClubDto } from '../../api/social-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-club-card',
  styleUrl: './club-card.scss',
  templateUrl: './club-card.html',
})
export class ClubCard {
  readonly club = input.required<ClubDto>();
}

import { httpResource } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { ClubDto, MyInvitationDto } from '../../api/social-api-types';
import { ClubCard } from '../club-card/club-card';

@Component({
  imports: [ClubCard, RouterLink, RouterLinkActive],
  selector: 'app-club-list',
  styleUrl: './club-list.scss',
  templateUrl: './club-list.html',
})
export class ClubList {
  protected readonly auth = inject(Auth);

  protected readonly clubs = httpResource<PageResult<ClubDto>>(
    () => '/api/social/clubs?page=1&pageSize=20',
  );

  // Only the number of invitations is shown here, next to "My invitations".
  protected readonly invitations = httpResource<MyInvitationDto[]>(() =>
    this.auth.isLoggedIn() ? '/api/social/clubs/invitations/mine' : undefined,
  );
}

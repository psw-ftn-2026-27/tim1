import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { ClubDto } from '../../api/social-api-types';
import { ClubCard } from '../club-card/club-card';

@Component({
  imports: [ClubCard, RouterLink],
  selector: 'app-club-list',
  styleUrl: './club-list.scss',
  templateUrl: './club-list.html',
})
export class ClubList {
  protected readonly clubs = httpResource<PageResult<ClubDto>>(
    () => '/api/social/clubs?page=1&pageSize=20',
  );
}

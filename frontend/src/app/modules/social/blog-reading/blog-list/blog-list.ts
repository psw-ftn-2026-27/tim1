import { httpResource } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { BlogDto, FollowCountsDto } from '../../api/social-api-types';
import { BlogCard } from '../blog-card/blog-card';

@Component({
  imports: [BlogCard, RouterLink, RouterLinkActive],
  selector: 'app-blog-list',
  styleUrl: './blog-list.scss',
  templateUrl: './blog-list.html',
})
export class BlogList {
  protected readonly auth = inject(Auth);

  protected readonly blogs = httpResource<PageResult<BlogDto>>(
    () => '/api/social/blogs/published?page=1&pageSize=20',
  );

  protected readonly counts = httpResource<FollowCountsDto>(() =>
    this.auth.isLoggedIn() ? '/api/social/follow-counts' : undefined,
  );

  protected readonly pendingCount = httpResource<number>(() =>
    this.auth.isLoggedIn() ? '/api/social/follow-requests/received/pending-count' : undefined,
  );
}

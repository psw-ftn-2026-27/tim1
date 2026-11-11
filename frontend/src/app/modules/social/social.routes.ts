import { Routes } from '@angular/router';
import { CreateBlog } from './blog-authoring/create-blog/create-blog';
import { EditBlog } from './blog-authoring/edit-blog/edit-blog';
import { MyBlogs } from './blog-authoring/my-blogs/my-blogs';
import { FollowRequests } from './follow-requesting/follow-requests/follow-requests';
import { People } from './follow-requesting/people/people';
import { BlogDetail } from './blog-reading/blog-detail/blog-detail';
import { BlogList } from './blog-reading/blog-list/blog-list';
import { ClubDetail } from './club-browsing/club-detail/club-detail';
import { ClubList } from './club-browsing/club-list/club-list';
import { CreateClub } from './club-management/create-club/create-club';
import { MyClubs } from './club-management/my-clubs/my-clubs';

export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'mine/:id/edit', component: EditBlog },
  { path: 'create', component: CreateBlog },
  { path: 'clubs', component: ClubList },
  { path: 'clubs/mine', component: MyClubs },
  { path: 'clubs/create', component: CreateClub },
  { path: 'follow-requests', component: FollowRequests },
  { path: 'people', component: People },
  { path: 'clubs/:id', component: ClubDetail },
  { path: ':id', component: BlogDetail },
];

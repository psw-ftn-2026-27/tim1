export type BlogStatus = 'Draft' | 'Published' | 'Closed';

export interface CommentDto {
  id: string;
  userId: string;
  text: string;
  createdAt: string;
}

export interface BlogDto {
  id: string;
  authorId: string;
  title: string;
  description: string;
  createdAt: string;
  lastModifiedAt: string;
  images: string[];
  status: BlogStatus;
  comments: CommentDto[];
}

export interface CreateBlogDto {
  title: string;
  description: string;
  images: string[] | null;
}

export interface CreateCommentDto {
  text: string;
}

export interface UpdateCommentDto {
  text: string;
}

export type FollowRequestStatus = 'Pending' | 'Accepted' | 'Rejected';

export type AuthorFollowState = 'NoRequest' | 'RequestSent' | 'Following';

export interface FollowRequestDto {
  id: string;
  recipientId: string;
  status: FollowRequestStatus;
  createdAt: string;
}

export interface AuthorToFollowDto {
  authorId: string;
  blogTitles: string[];
  state: AuthorFollowState;
}

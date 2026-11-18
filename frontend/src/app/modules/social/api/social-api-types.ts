export type BlogStatus = 'Draft' | 'Published' | 'Archived';

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

export interface UpdateBlogDto {
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

export interface ClubDto {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  images: string[];
}

export interface CreateClubDto {
  name: string;
  description: string;
  images: string[] | null;
}

export interface UpdateClubDto {
  name: string;
  description: string;
  images: string[] | null;
}

export type FollowRequestStatus = 'Pending' | 'Accepted' | 'Rejected';

export interface FollowRequestDto {
  id: string;
  recipientId: string;
  recipientDisplayName: string;
  status: FollowRequestStatus;
  createdAt: string;
}

export interface UserToFollowDto {
  id: string;
  displayName: string;
}

export interface ReceivedFollowRequestDto {
  id: string;
  senderId: string;
  senderDisplayName: string;
  status: FollowRequestStatus;
  createdAt: string;
}

export interface FollowerDto {
  id: string;
  userId: string;
  displayName: string;
  isFollowedByUser: boolean;
  hasPendingRequest: boolean;
}

export interface FollowingDto {
  id: string;
  userId: string;
  displayName: string;
}

export interface FollowCountsDto {
  followerCount: number;
  followingCount: number;
}

export type ClubRole = 'Owner' | 'Moderator' | 'Member';

export interface ClubMemberDto {
  userId: string;
  role: ClubRole;
}

export type MembershipStatus = 'NotMember' | 'PendingRequest' | 'Member';

export interface MyMembershipDto {
  status: MembershipStatus;
}

export interface MembershipRequestDto {
  userId: string;
  createdAt: string;
}

export interface UserToInviteDto {
  id: string;
  displayName: string;
}

export interface SentInvitationDto {
  userId: string;
  userDisplayName: string;
  createdAt: string;
}

export interface MyInvitationDto {
  clubId: string;
  clubName: string;
  createdAt: string;
}

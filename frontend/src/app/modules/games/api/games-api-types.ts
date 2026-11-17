export type SocialChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface SocialChallengeDto {
  id: string;
  name: string;
  activeFrom: string;
  activeUntil: string;
  requiredParticipants: number;
  radiusMeters: number;
  experiencePoints: number;
  completionMessage: string;
  latitude: number;
  longitude: number;
  status: SocialChallengeStatus;
}

export interface CreateSocialChallengeDto {
  name: string;
  activeFrom: string;
  activeUntil: string;
  requiredParticipants: number;
  radiusMeters: number;
  experiencePoints: number;
  completionMessage: string;
  latitude: number | null;
  longitude: number | null;
}

export interface UpdateSocialChallengeDto {
  name: string;
  activeFrom: string;
  activeUntil: string;
  requiredParticipants: number;
  radiusMeters: number;
  experiencePoints: number;
  completionMessage: string;
  latitude: number | null;
  longitude: number | null;
}

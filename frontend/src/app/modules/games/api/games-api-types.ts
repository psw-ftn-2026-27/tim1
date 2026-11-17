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

export type QuizStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface QuizAnswerDto {
  text: string;
  isCorrect: boolean;
}

export interface QuizQuestionDto {
  id: string;
  text: string;
  answers: QuizAnswerDto[];
}

export interface QuizDto {
  id: string;
  name: string;
  instructions: string;
  experiencePoints: number;
  status: QuizStatus;
  createdAt: string;
  questions: QuizQuestionDto[];
}

export interface QuizSummaryDto {
  id: string;
  name: string;
  status: QuizStatus;
}

export interface QuizQuestionInputDto {
  id: string | null;
  text: string | null;
  answers: QuizAnswerDto[] | null;
}

export interface SaveQuizDto {
  name: string | null;
  instructions: string | null;
  experiencePoints: number | null;
  questions: QuizQuestionInputDto[] | null;
}

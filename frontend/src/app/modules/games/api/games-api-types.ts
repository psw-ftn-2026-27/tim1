import { PageResult } from '../../../shared/api/page-result';

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
  latitude: number | null;
  longitude: number | null;
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
  latitude: number | null;
  longitude: number | null;
}

export type LocationChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface HiddenPointDto {
  id: string;
  location: LocationDto;
  imageId: string;
}

export interface LocationChallengeDto {
  id: string;
  name: string;
  startLocation: LocationDto;
  experiencePoints: number;
  status: LocationChallengeStatus;
  hiddenPoints: HiddenPointDto[];
}

export interface LocationChallengeSummaryDto {
  id: string;
  name: string;
  hiddenPointCount: number;
  experiencePoints: number;
  status: LocationChallengeStatus;
}

export interface HiddenPointInputDto {
  location: LocationDto;
  imageId: string;
}

export interface SaveLocationChallengeDto {
  name: string;
  startLocation: LocationDto;
  experiencePoints: number;
  hiddenPoints: HiddenPointInputDto[];
}

export interface ChallengeOverviewItemDto {
  id: string;
  name: string;
  type: string;
  status: string;
  createdAt: string | null;
}

export interface ChallengeOverviewDto {
  page: PageResult<ChallengeOverviewItemDto>;
  hasChallenges: boolean;
}

export interface NearbyLocationChallengeDto {
  id: string;
  name: string;
  experiencePoints: number;
  distanceMeters: number;
  canActivate: boolean;
  isStarted: boolean;
}

export interface NearbyLocationChallengesDto {
  position: LocationDto | null;
  challenges: NearbyLocationChallengeDto[];
}

export interface SearchCircleDto {
  center: LocationDto;
  diameterMeters: number;
}

export interface AttemptPhotoDto {
  imageId: string;
  isDiscovered: boolean;
}

export interface LocationChallengeAttemptDto {
  id: string;
  challengeId: string;
  challengeName: string;
  circle: SearchCircleDto;
  photos: AttemptPhotoDto[];
  remainingPointCount: number;
  isSolved: boolean;
  startedAt: string;
  solvedAt: string | null;
  awardedExperiencePoints: number | null;
}

export interface AvailableQuizDto {
  id: string;
  name: string;
  experiencePoints: number;
  hasLocation: boolean;
  inProgress: boolean;
}

export type QuizPlayKind = 'NotStarted' | 'InProgress' | 'Completed' | 'Unavailable';

export interface QuizPlayQuestionDto {
  id: string;
  text: string;
  answers: string[];
}

export interface QuizPlayStateDto {
  quizId: string;
  kind: QuizPlayKind;
  quizName: string | null;
  experiencePoints: number | null;
  latitude: number | null;
  longitude: number | null;
  question: QuizPlayQuestionDto | null;
  questionNumber: number | null;
  questionCount: number | null;
  points: number | null;
  lastAnswerWasCorrect: boolean | null;
  correctAnswers: number | null;
  percent: number | null;
  experiencePointsAwarded: number | null;
}

export interface ActivateQuizDto {
  latitude: number;
  longitude: number;
}

export interface AnswerQuestionDto {
  questionId: string;
  answerIndex: number;
}

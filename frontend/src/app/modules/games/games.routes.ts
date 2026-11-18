import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { Auth } from '../../core/auth/auth';
import { ChallengeList } from './challenge-overview/challenge-list/challenge-list';
import { GamesHome } from './games-home/games-home';
import { CreateLocationChallenge } from './location-challenge-management/create-location-challenge/create-location-challenge';
import { EditLocationChallenge } from './location-challenge-management/edit-location-challenge/edit-location-challenge';
import { LocationChallenges } from './location-challenge-management/location-challenges/location-challenges';
import { LocationChallengeAttempt } from './location-challenge-solving/location-challenge-attempt/location-challenge-attempt';
import { NearbyLocationChallenges } from './location-challenge-solving/nearby-location-challenges/nearby-location-challenges';
import { CreateQuiz } from './quiz-management/create-quiz/create-quiz';
import { EditQuiz } from './quiz-management/edit-quiz/edit-quiz';
import { Quizzes } from './quiz-management/quizzes/quizzes';
import { CreateSocialChallenge } from './social-challenge-management/create-social-challenge/create-social-challenge';
import { EditSocialChallenge } from './social-challenge-management/edit-social-challenge/edit-social-challenge';
import { AvailableQuizzes } from './quiz-playing/available-quizzes/available-quizzes';
import { PlayQuiz } from './quiz-playing/play-quiz/play-quiz';
import { SocialChallenges } from './social-challenge-management/social-challenges/social-challenges';

const requireAdministrator: CanActivateFn = () => {
  const auth = inject(Auth);
  return auth.user()?.roles.includes('administrator') === true
    ? true
    : inject(Router).createUrlTree(['/games']);
};

const requireExplorer: CanActivateFn = () => {
  const auth = inject(Auth);
  return auth.user()?.roles.includes('explorer') === true
    ? true
    : inject(Router).createUrlTree(['/games']);
};

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList, canActivate: [requireAdministrator] },
  { path: 'quizzes', component: Quizzes, canActivate: [requireAdministrator] },
  { path: 'quizzes/new', component: CreateQuiz, canActivate: [requireAdministrator] },
  { path: 'quizzes/:id/edit', component: EditQuiz, canActivate: [requireAdministrator] },
  { path: 'play/quizzes', component: AvailableQuizzes, canActivate: [requireExplorer] },
  { path: 'play/quizzes/:id', component: PlayQuiz, canActivate: [requireExplorer] },
  { path: 'social-challenges', component: SocialChallenges },
  { path: 'social-challenges/new', component: CreateSocialChallenge },
  { path: 'social-challenges/:id/edit', component: EditSocialChallenge },
  {
    path: 'location-challenges',
    component: LocationChallenges,
    canActivate: [requireAdministrator],
  },
  {
    path: 'location-challenges/new',
    component: CreateLocationChallenge,
    canActivate: [requireAdministrator],
  },
  {
    path: 'location-challenges/:id/edit',
    component: EditLocationChallenge,
    canActivate: [requireAdministrator],
  },
  {
    path: 'location-challenges/nearby',
    component: NearbyLocationChallenges,
    canActivate: [requireExplorer],
  },
  {
    path: 'location-challenges/:id/attempt',
    component: LocationChallengeAttempt,
    canActivate: [requireExplorer],
  },
];

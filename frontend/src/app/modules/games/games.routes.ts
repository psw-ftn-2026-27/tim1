import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { Auth } from '../../core/auth/auth';
import { GamesHome } from './games-home/games-home';
import { CreateQuiz } from './quiz-management/create-quiz/create-quiz';
import { EditQuiz } from './quiz-management/edit-quiz/edit-quiz';
import { Quizzes } from './quiz-management/quizzes/quizzes';
import { CreateSocialChallenge } from './social-challenge-management/create-social-challenge/create-social-challenge';
import { EditSocialChallenge } from './social-challenge-management/edit-social-challenge/edit-social-challenge';
import { SocialChallenges } from './social-challenge-management/social-challenges/social-challenges';

const requireAdministrator: CanActivateFn = () => {
  const auth = inject(Auth);
  return auth.user()?.roles.includes('administrator') === true
    ? true
    : inject(Router).createUrlTree(['/games']);
};

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'quizzes', component: Quizzes, canActivate: [requireAdministrator] },
  { path: 'quizzes/new', component: CreateQuiz, canActivate: [requireAdministrator] },
  { path: 'quizzes/:id/edit', component: EditQuiz, canActivate: [requireAdministrator] },
  { path: 'social-challenges', component: SocialChallenges },
  { path: 'social-challenges/new', component: CreateSocialChallenge },
  { path: 'social-challenges/:id/edit', component: EditSocialChallenge },
];

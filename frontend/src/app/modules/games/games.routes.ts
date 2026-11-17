import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { CreateSocialChallenge } from './social-challenge-management/create-social-challenge/create-social-challenge';
import { EditSocialChallenge } from './social-challenge-management/edit-social-challenge/edit-social-challenge';
import { SocialChallenges } from './social-challenge-management/social-challenges/social-challenges';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'social-challenges', component: SocialChallenges },
  { path: 'social-challenges/new', component: CreateSocialChallenge },
  { path: 'social-challenges/:id/edit', component: EditSocialChallenge },
];

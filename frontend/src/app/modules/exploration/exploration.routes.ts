import { inject, isDevMode } from '@angular/core';
import { Router } from '@angular/router';
import { Auth } from '../../core/auth/auth';
import { PositionTest } from './user-positioning/position-test/position-test';
import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { TourPreferences } from './tour-preference-editing/tour-preferences/tour-preferences';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  {
    path: 'position-test',
    component: PositionTest,
    canActivate: [() => (isDevMode() && inject(Auth).user()?.roles.includes('explorer')) ||
      inject(Router).createUrlTree(['/exploration'])],
  },
  { path: 'mine', component: MyTours },
  { path: 'create', component: CreateTour },
  { path: 'preferences', component: TourPreferences },
];

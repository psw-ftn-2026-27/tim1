import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { AddKeypoint } from './keypoint-authoring/add-keypoint/add-keypoint';
import { EditKeypoint } from './keypoint-authoring/edit-keypoint/edit-keypoint';
import { EditTour } from './tour-authoring/edit-tour/edit-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { TourPreferences } from './tour-preference-editing/tour-preferences/tour-preferences';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'create', component: CreateTour },
  { path: 'tour/:id/edit', component: EditTour },
  { path: 'tour/:id/add-keypoint', component: AddKeypoint },
  { path: 'tour/:id/keypoint/:keypointId/edit', component: EditKeypoint },
  { path: 'preferences', component: TourPreferences },
];

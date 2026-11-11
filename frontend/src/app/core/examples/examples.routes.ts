import { Routes } from '@angular/router';
import { MapPins } from './map-pins/map-pins';
import { MapRoute } from './map-route/map-route';

export const exampleRoutes: Routes = [
  { path: 'map-pins', component: MapPins },
  { path: 'map-route', component: MapRoute },
];

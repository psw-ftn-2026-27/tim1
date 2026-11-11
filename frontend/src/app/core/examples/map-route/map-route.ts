import { Component, computed, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Map } from '../../../shared/ui/map/map';
import type { MapPin, MapRouteMode, MapRouteRequest } from '../../../shared/ui/map/map';

@Component({
  selector: 'app-map-route',
  imports: [FormField, Map, RouterLink],
  templateUrl: './map-route.html',
  styleUrl: './map-route.scss',
})
export class MapRoute {
  protected readonly points: readonly MapPin[] = [
    { latitude: 45.254891, longitude: 19.852782 },
    { latitude: 45.257046, longitude: 19.851583 },
    { latitude: 45.252182, longitude: 19.862732 },
  ];

  protected readonly form = form(signal({ mode: 'Walking' as MapRouteMode }));
  protected readonly routeRequest = computed<MapRouteRequest>(() => ({
    points: this.points,
    mode: this.form.mode().value(),
  }));
  protected readonly lengthKm = signal<number | null>(null);
  protected readonly formattedLength = computed(() =>
    this.lengthKm()?.toLocaleString('sr-Latn-RS', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }),
  );
}

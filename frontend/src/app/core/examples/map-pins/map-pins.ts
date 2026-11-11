import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Map } from '../../../shared/ui/map/map';
import type { MapPin } from '../../../shared/ui/map/map';

@Component({
  selector: 'app-map-pins',
  imports: [Map, RouterLink],
  templateUrl: './map-pins.html',
  styleUrl: './map-pins.scss',
})
export class MapPins {
  protected readonly pins: readonly MapPin[] = [
    { latitude: 45.2551, longitude: 19.8448 },
    { latitude: 45.2572, longitude: 19.8512 },
    { latitude: 45.2521, longitude: 19.8628 },
  ];
  protected readonly selectedLocation = signal<MapPin | null>(null);
}

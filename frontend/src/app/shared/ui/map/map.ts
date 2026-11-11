import {
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { featureGroup, icon, map, marker, tileLayer } from 'leaflet';
import type { FeatureGroup, Map as LeafletMap } from 'leaflet';

export interface MapPin {
  latitude: number;
  longitude: number;
}

const pinIcon = icon({
  iconUrl: 'leaflet/marker-icon.png',
  iconRetinaUrl: 'leaflet/marker-icon-2x.png',
  shadowUrl: 'leaflet/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  shadowSize: [41, 41],
});

@Component({
  selector: 'app-map',
  templateUrl: './map.html',
  styleUrl: './map.scss',
})
export class Map {
  readonly pins = input.required<readonly MapPin[]>();

  protected readonly tileLoadFailed = signal(false);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly mapView = signal<{ map: LeafletMap; markers: FeatureGroup } | null>(null);

  constructor() {
    afterNextRender(() => {
      const container = this.host.nativeElement.querySelector<HTMLElement>('.map');
      if (!container) return;

      const view = map(container);
      const markers = featureGroup().addTo(view);
      const tiles = tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution:
          '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      });

      let loadedTile = false;
      tiles.on('loading', () => (loadedTile = false));
      tiles.on('tileload', () => {
        loadedTile = true;
        this.tileLoadFailed.set(false);
      });
      tiles.on('load', () => this.tileLoadFailed.set(!loadedTile));
      tiles.addTo(view);

      this.destroyRef.onDestroy(() => view.remove());
      this.mapView.set({ map: view, markers });
    });

    // Leaflet owns the map container's contents, so update its markers when the input changes.
    effect(() => {
      const view = this.mapView();
      if (!view) return;

      view.markers.clearLayers();
      for (const { latitude, longitude } of this.pins()) {
        marker([latitude, longitude], { icon: pinIcon }).addTo(view.markers);
      }

      const bounds = view.markers.getBounds();
      if (bounds.isValid()) {
        view.map.fitBounds(bounds, { padding: [24, 24], maxZoom: 15, animate: false });
      } else {
        view.map.setView([44, 21], 6);
      }
    });
  }
}

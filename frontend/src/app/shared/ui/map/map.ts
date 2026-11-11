import {
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { featureGroup, icon, map, marker, polyline, tileLayer } from 'leaflet';
import type { FeatureGroup, Map as LeafletMap } from 'leaflet';
import { MapRouting } from './map-routing';

export interface MapPin {
  latitude: number;
  longitude: number;
}

export type MapRouteMode = 'Walking' | 'Bicycle' | 'Car';

export interface MapRouteRequest {
  points: readonly MapPin[];
  mode: MapRouteMode;
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
  readonly routeRequest = input<MapRouteRequest | null>(null);
  readonly routeLengthKm = output<number | null>();

  protected readonly tileLoadFailed = signal(false);
  protected readonly routeStatus = signal<'idle' | 'loading' | 'not-found' | 'error'>('idle');
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly routing = inject(MapRouting);
  private readonly mapView = signal<{
    map: LeafletMap;
    markers: FeatureGroup;
    route: FeatureGroup;
  } | null>(null);

  constructor() {
    afterNextRender(() => {
      const container = this.host.nativeElement.querySelector<HTMLElement>('.map');
      if (!container) return;

      const view = map(container);
      const markers = featureGroup().addTo(view);
      const route = featureGroup().addTo(view);
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
      this.mapView.set({ map: view, markers, route });
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

    effect((onCleanup) => {
      const view = this.mapView();
      const request = this.routeRequest();
      if (!view) return;

      view.route.clearLayers();
      this.routeLengthKm.emit(null);
      if (!request || request.points.length < 2) {
        this.routeStatus.set('idle');
        return;
      }

      const controller = new AbortController();
      onCleanup(() => controller.abort());
      this.routeStatus.set('loading');
      void this.showRoute(view, request, controller.signal);
    });
  }

  private async showRoute(
    view: { map: LeafletMap; route: FeatureGroup },
    request: MapRouteRequest,
    signal: AbortSignal,
  ): Promise<void> {
    try {
      const result = await this.routing.findRoute(request, signal);
      if (signal.aborted || this.routeRequest() !== request) return;

      if (result === null) {
        this.routeStatus.set('not-found');
        this.routeLengthKm.emit(null);
        return;
      }

      const line = polyline(result.coordinates).addTo(view.route);
      view.map.fitBounds(line.getBounds(), { padding: [24, 24], maxZoom: 15, animate: false });
      this.routeStatus.set('idle');
      this.routeLengthKm.emit(result.lengthKm);
    } catch {
      if (signal.aborted || this.routeRequest() !== request) return;

      view.route.clearLayers();
      this.routeStatus.set('error');
      this.routeLengthKm.emit(null);
    }
  }
}

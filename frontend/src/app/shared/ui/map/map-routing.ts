import { Injectable } from '@angular/core';
import { latLng } from 'leaflet';
import type { LatLngTuple } from 'leaflet';
import type { MapRouteRequest } from './map';
import { ROUTING_PROFILES } from './routing-profiles';

interface RoutingResponse {
  code: string;
  routes?: { geometry: { coordinates: [longitude: number, latitude: number][] } }[];
}

interface MapRouteResult {
  coordinates: LatLngTuple[];
  lengthKm: number;
}

@Injectable({ providedIn: 'root' })
export class MapRouting {
  private nextRequestAt = 0;

  async findRoute(request: MapRouteRequest, signal: AbortSignal): Promise<MapRouteResult | null> {
    const points = request.points.map(({ longitude, latitude }) => `${longitude},${latitude}`);
    const url = `${ROUTING_PROFILES[request.mode]}/${points.join(';')}?overview=full&geometries=geojson`;

    // Several maps can wait for the same slot. Check again after each timer fires.
    let waitMs = this.nextRequestAt - performance.now();
    while (waitMs > 0) {
      await new Promise<void>((resolve) => setTimeout(resolve, Math.ceil(waitMs)));
      signal.throwIfAborted();
      waitMs = this.nextRequestAt - performance.now();
    }
    signal.throwIfAborted();
    this.nextRequestAt = performance.now() + 1000;
    const response = await fetch(url, { credentials: 'omit', signal });

    const result: RoutingResponse = await response.json();
    if (result.code === 'NoRoute' || result.code === 'NoSegment') return null;

    const geometry = result.routes?.[0]?.geometry?.coordinates;
    if (!response.ok || result.code !== 'Ok' || !geometry || geometry.length < 2) {
      throw new Error('Neispravan odgovor servisa za rutiranje.');
    }

    const coordinates = geometry.map(([longitude, latitude]): LatLngTuple => [latitude, longitude]);
    let lengthMeters = 0;
    for (let index = 1; index < coordinates.length; index++) {
      lengthMeters += latLng(coordinates[index - 1]).distanceTo(coordinates[index]);
    }
    return { coordinates, lengthKm: lengthMeters / 1000 };
  }
}

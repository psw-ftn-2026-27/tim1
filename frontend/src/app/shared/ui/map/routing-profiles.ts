import type { MapRouteMode } from './map';

export const ROUTING_PROFILES: Record<MapRouteMode, string> = {
  Walking: 'https://routing.openstreetmap.de/routed-foot/route/v1/driving',
  Bicycle: 'https://routing.openstreetmap.de/routed-bike/route/v1/driving',
  Car: 'https://routing.openstreetmap.de/routed-car/route/v1/driving',
};

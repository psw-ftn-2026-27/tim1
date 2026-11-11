import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { MapRouteRequest } from './map';
import { MapRouting } from './map-routing';

const walking: MapRouteRequest = {
  mode: 'Walking',
  points: [
    { latitude: 0, longitude: 0 },
    { latitude: 0, longitude: 1 },
    { latitude: 1, longitude: 1 },
  ],
};

function routeResponse(): Response {
  return Response.json({
    code: 'Ok',
    routes: [
      {
        geometry: {
          coordinates: [
            [0, 0],
            [1, 0],
            [1, 1],
          ],
        },
      },
    ],
  });
}

describe('MapRouting', () => {
  const request = vi.fn<typeof fetch>();
  let routing: MapRouting;

  beforeEach(() => {
    vi.useFakeTimers();
    vi.stubGlobal('fetch', request);
    request.mockResolvedValue(routeResponse());
    routing = TestBed.inject(MapRouting);
  });

  afterEach(() => {
    request.mockReset();
    vi.unstubAllGlobals();
    vi.useRealTimers();
  });

  it('prosleđuje tačke redom i računa dužinu iz cele iscrtane geometrije', async () => {
    const result = await routing.findRoute(walking, new AbortController().signal);

    expect(request.mock.calls[0][0]).toContain('/routed-foot/route/v1/driving/0,0;1,0;1,1?');
    expect(request.mock.calls[0][1]?.credentials).toBe('omit');
    expect(result?.coordinates).toEqual([
      [0, 0],
      [0, 1],
      [1, 1],
    ]);
    expect(result?.lengthKm).toBeCloseTo(222.38985328911747, 8);
  });

  it('zadržava razmak između zahteva i preskače otkazani izbor', async () => {
    request.mockImplementation(async () => routeResponse());
    const old = new AbortController();
    const first = routing.findRoute(walking, old.signal);
    old.abort();
    await first;

    const previous = new AbortController();
    const discarded = routing.findRoute({ ...walking, mode: 'Bicycle' }, previous.signal);
    const discardedError = expect(discarded).rejects.toMatchObject({ name: 'AbortError' });
    previous.abort();
    const latest = routing.findRoute({ ...walking, mode: 'Car' }, new AbortController().signal);

    await vi.advanceTimersByTimeAsync(999);
    expect(request).toHaveBeenCalledTimes(1);
    await vi.advanceTimersByTimeAsync(1);
    await discardedError;
    await latest;
    expect(request).toHaveBeenCalledTimes(2);
    expect(request.mock.calls[1][0]).toContain('/routed-car/');
  });

  it('izdvaja nepronađenu rutu od greške pružaoca', async () => {
    request.mockResolvedValueOnce(Response.json({ code: 'NoRoute' }, { status: 400 }));
    const first = routing.findRoute(walking, new AbortController().signal);
    await expect(first).resolves.toBeNull();

    request.mockResolvedValueOnce(Response.json({ code: 'ServiceUnavailable' }, { status: 503 }));
    const second = routing.findRoute(walking, new AbortController().signal);
    const rejected = expect(second).rejects.toThrow('Neispravan odgovor servisa za rutiranje.');
    await vi.advanceTimersByTimeAsync(1000);
    await rejected;
  });
});

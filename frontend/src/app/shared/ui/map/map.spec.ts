import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Map as LeafletMap, Marker, Polyline, SVG } from 'leaflet';
import { Map, MapPin, MapRouteRequest } from './map';
import { MapRouting } from './map-routing';

type RouteResult = NonNullable<Awaited<ReturnType<MapRouting['findRoute']>>>;

function deferredRoute() {
  let resolve!: (route: RouteResult) => void;
  let reject!: (error: Error) => void;
  const promise = new Promise<RouteResult>((resolvePromise, rejectPromise) => {
    resolve = resolvePromise;
    reject = rejectPromise;
  });
  return { promise, resolve, reject };
}

describe('Prikaz zadatih pinova', () => {
  const pins: readonly MapPin[] = [
    { latitude: 45.2551, longitude: 19.8448 },
    { latitude: 45.2572, longitude: 19.8512 },
    { latitude: 45.2521, longitude: 19.8628 },
  ];
  let fixture: ComponentFixture<Map>;
  let element: HTMLElement;
  let view: LeafletMap;
  const createdViews: LeafletMap[] = [];
  const findRoute = vi.fn<MapRouting['findRoute']>();

  LeafletMap.addInitHook(function (this: LeafletMap) {
    this.options.renderer = new SVG();
    createdViews.push(this);
  });

  function finishTiles(result: 'load' | 'error'): void {
    const tiles = element.querySelectorAll('.leaflet-tile');
    expect(tiles.length).toBeGreaterThan(0);
    for (const tile of tiles) {
      tile.dispatchEvent(new Event(result));
    }
    TestBed.tick();
  }

  beforeEach(() => {
    vi.spyOn(HTMLElement.prototype, 'clientWidth', 'get').mockReturnValue(640);
    vi.spyOn(HTMLElement.prototype, 'clientHeight', 'get').mockReturnValue(400);
    TestBed.configureTestingModule({
      imports: [Map],
      providers: [{ provide: MapRouting, useValue: { findRoute } }],
    });
    fixture = TestBed.createComponent(Map);
    element = fixture.nativeElement;
    fixture.componentRef.setInput('pins', pins);
    fixture.detectChanges();
    TestBed.tick();
    view = createdViews.pop()!;
  });

  afterEach(() => {
    fixture.destroy();
    vi.restoreAllMocks();
    vi.clearAllMocks();
    findRoute.mockReset();
  });

  it('prikazuje zadate pinove bez linija i zadržava njihove lokacije pri korišćenju mape', () => {
    finishTiles('load');
    expect(element.querySelectorAll('.leaflet-tile-loaded').length).toBeGreaterThan(0);
    expect(element.querySelector('[role="status"]')).toBeNull();

    const coordinates = (): MapPin[] => {
      const locations: MapPin[] = [];
      view.eachLayer((layer) => {
        expect(layer).not.toBeInstanceOf(Polyline);
        if (layer instanceof Marker) {
          const { lat, lng } = layer.getLatLng();
          locations.push({ latitude: lat, longitude: lng });
        }
      });
      return locations;
    };

    expect(coordinates()).toEqual(pins);
    const initialCenter = view.getCenter();
    view.panBy([64, 32], { animate: false });
    expect(view.getCenter().equals(initialCenter)).toBe(false);
    const initialZoom = view.getZoom();
    view.setView(view.getCenter(), initialZoom + 1, { animate: false });
    expect(view.getZoom()).toBe(initialZoom + 1);
    view.getContainer().dispatchEvent(new MouseEvent('click', { bubbles: true }));
    expect(coordinates()).toEqual(pins);
    expect(element.querySelectorAll('.leaflet-marker-icon')).toHaveLength(pins.length);
    expect(findRoute).not.toHaveBeenCalled();
  });

  it('prikazuje poruku uz mapu kada učitavanje podloge ne uspe', () => {
    finishTiles('error');

    const mapContainer = element.querySelector('[aria-label="Geografska mapa"]');
    expect(mapContainer?.nextElementSibling?.textContent?.trim()).toBe(
      'Podloga mape nije mogla da se učita. Pokušajte ponovo kasnije.',
    );
  });

  it('iscrtava pronađenu geometriju i emituje nezaokruženu dužinu', async () => {
    const length = vi.fn();
    fixture.componentInstance.routeLengthKm.subscribe(length);
    const result: RouteResult = {
      coordinates: [
        [45.25, 19.84],
        [45.26, 19.85],
        [45.25, 19.86],
      ],
      lengthKm: 2.68319,
    };
    findRoute.mockResolvedValue(result);
    const request: MapRouteRequest = { points: pins, mode: 'Walking' };

    fixture.componentRef.setInput('routeRequest', request);
    TestBed.tick();
    expect(element.textContent).toContain('Tražimo rutu…');
    expect(length).toHaveBeenLastCalledWith(null);
    await Promise.resolve();
    TestBed.tick();

    expect(findRoute).toHaveBeenCalledWith(request, expect.any(AbortSignal));
    const paths: Polyline[] = [];
    view.eachLayer((layer) => {
      if (layer instanceof Polyline) paths.push(layer);
    });
    expect(paths).toHaveLength(1);
    expect(paths[0].getLatLngs()).toMatchObject(
      result.coordinates.map(([lat, lng]) => ({ lat, lng })),
    );
    expect(length).toHaveBeenLastCalledWith(2.68319);
    expect(element.textContent).not.toContain('Tražimo rutu…');
    expect(
      element.querySelector('a[href="https://www.openstreetmap.org/fixthemap"]'),
    ).not.toBeNull();
  });

  it('sklanja staru rutu i ignoriše zakašnjele uspešne i neuspešne odgovore', async () => {
    const length = vi.fn();
    fixture.componentInstance.routeLengthKm.subscribe(length);
    const route: RouteResult = {
      coordinates: [
        [45.25, 19.84],
        [45.26, 19.85],
      ],
      lengthKm: 1.22,
    };
    findRoute.mockResolvedValueOnce(route);
    const staleSuccess = deferredRoute();
    const staleFailure = deferredRoute();
    const current = deferredRoute();
    findRoute
      .mockReturnValueOnce(staleSuccess.promise)
      .mockReturnValueOnce(staleFailure.promise)
      .mockReturnValueOnce(current.promise);

    fixture.componentRef.setInput('routeRequest', { points: pins, mode: 'Walking' });
    TestBed.tick();
    await Promise.resolve();
    TestBed.tick();
    expect(length).toHaveBeenLastCalledWith(1.22);

    for (const mode of ['Bicycle', 'Car', 'Walking'] as const) {
      fixture.componentRef.setInput('routeRequest', { points: pins, mode });
      TestBed.tick();
    }

    const paths: Polyline[] = [];
    view.eachLayer((layer) => {
      if (layer instanceof Polyline) paths.push(layer);
    });
    expect(paths).toHaveLength(0);
    expect(length).toHaveBeenLastCalledWith(null);
    expect(element.querySelectorAll('.leaflet-marker-icon')).toHaveLength(pins.length);
    expect(findRoute.mock.calls[1][1].aborted).toBe(true);
    expect(findRoute.mock.calls[2][1].aborted).toBe(true);

    const pendingEmissions = length.mock.calls.length;
    staleSuccess.resolve({ ...route, lengthKm: 99 });
    staleFailure.reject(new Error('zastarela greška'));
    await Promise.resolve();
    TestBed.tick();
    expect(element.textContent).toContain('Tražimo rutu…');
    expect(element.textContent).not.toContain('Ruta nije mogla da se učita.');
    expect(length).toHaveBeenCalledTimes(pendingEmissions);

    current.resolve({ ...route, lengthKm: 2.43 });
    await Promise.resolve();
    TestBed.tick();
    expect(element.textContent).not.toContain('Tražimo rutu…');
    expect(length).toHaveBeenLastCalledWith(2.43);
  });

  it('razlikuje nepronađenu rutu od greške i zadržava pinove', async () => {
    const length = vi.fn();
    fixture.componentInstance.routeLengthKm.subscribe(length);
    findRoute.mockResolvedValueOnce(null).mockRejectedValueOnce(new Error('nedostupno'));

    fixture.componentRef.setInput('routeRequest', { points: pins, mode: 'Walking' });
    TestBed.tick();
    await Promise.resolve();
    TestBed.tick();
    expect(element.textContent).toContain(
      'Nije pronađena ruta kroz zadate tačke za izabrani način kretanja.',
    );
    expect(length).toHaveBeenLastCalledWith(null);

    fixture.componentRef.setInput('routeRequest', { points: pins, mode: 'Car' });
    TestBed.tick();
    await Promise.resolve();
    TestBed.tick();
    expect(element.textContent).toContain('Ruta nije mogla da se učita. Pokušajte ponovo kasnije.');
    expect(element.textContent).not.toContain('Tražimo rutu…');
    expect(element.querySelectorAll('.leaflet-marker-icon')).toHaveLength(pins.length);
    expect(length).toHaveBeenLastCalledWith(null);
  });
});

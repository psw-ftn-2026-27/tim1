import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Marker, Polyline, map } from 'leaflet';
import type { Map as LeafletMap } from 'leaflet';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { Map, MapPin } from './map';

vi.mock(import('leaflet'), async (importOriginal) => {
  const leaflet = await importOriginal();
  return { ...leaflet, map: vi.fn(leaflet.map) };
});

describe('Prikaz zadatih pinova', () => {
  const pins: readonly MapPin[] = [
    { latitude: 45.2551, longitude: 19.8448 },
    { latitude: 45.2572, longitude: 19.8512 },
    { latitude: 45.2521, longitude: 19.8628 },
  ];
  let fixture: ComponentFixture<Map>;
  let element: HTMLElement;

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
    TestBed.configureTestingModule({ imports: [Map] });
    fixture = TestBed.createComponent(Map);
    element = fixture.nativeElement;
    fixture.componentRef.setInput('pins', pins);
    fixture.detectChanges();
    TestBed.tick();
  });

  afterEach(() => {
    fixture.destroy();
    vi.restoreAllMocks();
    vi.clearAllMocks();
  });

  it('prikazuje zadate pinove bez linija i zadržava njihove lokacije pri korišćenju mape', () => {
    finishTiles('load');
    expect(element.querySelectorAll('.leaflet-tile-loaded').length).toBeGreaterThan(0);
    expect(element.querySelector('[role="status"]')).toBeNull();

    const view: LeafletMap = vi.mocked(map).mock.results[0].value;
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
  });

  it('prikazuje poruku uz mapu kada učitavanje podloge ne uspe', () => {
    finishTiles('error');

    const mapContainer = element.querySelector('[aria-label="Geografska mapa"]');
    expect(mapContainer?.nextElementSibling?.textContent?.trim()).toBe(
      'Podloga mape nije mogla da se učita. Pokušajte ponovo kasnije.',
    );
  });
});

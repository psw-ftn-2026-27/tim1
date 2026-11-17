import { Component, input, output } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { describe, expect, it } from 'vitest';
import { Map } from '../../../shared/ui/map/map';
import type { MapPin, MapRouteRequest } from '../../../shared/ui/map/map';
import { MapRoute } from './map-route';

@Component({ selector: 'app-map', template: '' })
class ExampleMap {
  readonly pins = input.required<readonly MapPin[]>();
  readonly routeRequest = input.required<MapRouteRequest>();
  readonly routeLengthKm = output<number | null>();
}

describe('Probni primer putanje', () => {
  it('prosleđuje izbor mapi i formatira samo dostupnu dužinu', () => {
    TestBed.configureTestingModule({ imports: [MapRoute], providers: [provideRouter([])] });
    TestBed.overrideComponent(MapRoute, {
      remove: { imports: [Map] },
      add: { imports: [ExampleMap] },
    });
    const fixture = TestBed.createComponent(MapRoute);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;
    const map: ExampleMap = fixture.debugElement.query(By.directive(ExampleMap)).componentInstance;
    const select = element.querySelector('select')!;

    expect(select.value).toBe('Walking');
    expect(map.routeRequest()).toEqual({ mode: 'Walking', points: map.pins() });
    map.routeLengthKm.emit(2.68319);
    TestBed.tick();
    expect(element.textContent).toContain('Dužina rute: 2,68 km');

    select.value = 'Bicycle';
    select.dispatchEvent(new Event('input', { bubbles: true }));
    TestBed.tick();
    expect(map.routeRequest()).toEqual({ mode: 'Bicycle', points: map.pins() });

    map.routeLengthKm.emit(null);
    TestBed.tick();
    expect(element.textContent).not.toContain('Dužina rute:');
  });
});

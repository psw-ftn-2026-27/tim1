import { Component, input, output } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { describe, expect, it } from 'vitest';
import { Map } from '../../../shared/ui/map/map';
import type { MapPin } from '../../../shared/ui/map/map';
import { MapPins } from './map-pins';

@Component({ selector: 'app-map', template: '' })
class ExampleMap {
  readonly pins = input.required<readonly MapPin[]>();
  readonly selectionEnabled = input(false);
  readonly locationSelected = output<MapPin>();
}

describe('Probna forma za izbor lokacije', () => {
  it('prikazuje poslednje koordinate i počinje prazna pri ponovnom otvaranju', () => {
    TestBed.configureTestingModule({ imports: [MapPins], providers: [provideRouter([])] });
    TestBed.overrideComponent(MapPins, {
      remove: { imports: [Map] },
      add: { imports: [ExampleMap] },
    });

    const fixture = TestBed.createComponent(MapPins);
    fixture.detectChanges();
    const map: ExampleMap = fixture.debugElement.query(By.directive(ExampleMap)).componentInstance;
    const latitude: HTMLInputElement = fixture.nativeElement.querySelector('#latitude');
    const longitude: HTMLInputElement = fixture.nativeElement.querySelector('#longitude');

    expect(map.pins()).toHaveLength(3);
    expect(map.selectionEnabled()).toBe(true);
    expect(latitude.readOnly && longitude.readOnly).toBe(true);
    expect([latitude.value, longitude.value]).toEqual(['', '']);

    map.locationSelected.emit({ latitude: 45.2551, longitude: 19.8448 });
    TestBed.tick();
    expect([latitude.value, longitude.value]).toEqual(['45.2551', '19.8448']);

    map.locationSelected.emit({ latitude: 45.2572, longitude: 19.8512 });
    TestBed.tick();
    expect([latitude.value, longitude.value]).toEqual(['45.2572', '19.8512']);

    fixture.destroy();
    const reopened = TestBed.createComponent(MapPins);
    reopened.detectChanges();
    expect(reopened.nativeElement.querySelector('#latitude').value).toBe('');
    expect(reopened.nativeElement.querySelector('#longitude').value).toBe('');
  });
});

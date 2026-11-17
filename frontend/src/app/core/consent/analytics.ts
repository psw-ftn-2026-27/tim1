import { Injectable, effect, inject } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Consent } from './consent';

declare global {
  interface Window {
    dataLayer?: unknown[];
    gtag?: (...args: unknown[]) => void;
  }
}

// Šalje page_view u Google Analytics, ali samo uz saglasnost i samo ako je ID merenja podešen.
@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly consent = inject(Consent);
  private readonly router = inject(Router);
  private readonly measurementId = environment.analyticsMeasurementId;
  private loaded = false;

  constructor() {
    // effect: izbor i završena navigacija pokreću slanje van stabla komponenti (u GA).
    // Pokreće se pri promeni izbora (uključujući prvo davanje saglasnosti) i pri svakoj
    // završenoj navigaciji, pa jedan kod pokriva i P2 i page_view pri prihvatanju.
    effect(() => {
      const choice = this.consent.choice();
      const navigation = this.router.lastSuccessfulNavigation();
      if (this.measurementId === '') {
        return;
      }
      try {
        if (choice === 'granted') {
          if (navigation !== null) {
            this.sendPageView(this.router.serializeUrl(navigation.finalUrl ?? navigation.extractedUrl));
          }
        } else if (choice === 'denied') {
          this.stop();
        }
      } catch {
        // GA greške se tiho gutaju (P8)
      }
    });
  }

  private sendPageView(url: string): void {
    this.load();
    // Samo putanja: bez upita i fragmenta (P9)
    const path = url.split('?')[0].split('#')[0];
    this.setDisabled(false);
    window.gtag?.('event', 'page_view', {
      page_path: path,
      page_location: window.location.origin + path,
      page_title: document.title,
    });
  }

  // Učitava gtag.js tek posle saglasnosti; automatski page_view je isključen.
  private load(): void {
    if (this.loaded) {
      return;
    }
    this.loaded = true;
    window.dataLayer = window.dataLayer ?? [];
    window.gtag = function () {
      // gtag.js traži baš objekat arguments u dataLayer-u
      // eslint-disable-next-line prefer-rest-params
      window.dataLayer?.push(arguments);
    };
    window.gtag('js', new Date());
    window.gtag('config', this.measurementId, { send_page_view: false });

    const script = document.createElement('script');
    script.async = true;
    script.src = `https://www.googletagmanager.com/gtag/js?id=${this.measurementId}`;
    document.head.appendChild(script);
  }

  // GA čita ovu globalnu promenljivu pre svakog slanja
  private setDisabled(disabled: boolean): void {
    (window as unknown as Record<string, unknown>)[`ga-disable-${this.measurementId}`] = disabled;
  }

  // Odbijanje i povlačenje: GA ne šalje više ništa, a njegovi kolačići se brišu (P5).
  private stop(): void {
    this.setDisabled(true);
    const names = ['_ga', `_ga_${this.measurementId.replace('G-', '')}`];
    const host = window.location.hostname;
    const parts = host.split('.');
    for (const name of names) {
      // GA postavlja kolačić na domen i njegove nadređene domene, pa brišemo sve varijante
      document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
      for (let i = 0; i < parts.length - 1; i++) {
        const domain = parts.slice(i).join('.');
        document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; domain=${domain}`;
      }
    }
  }
}

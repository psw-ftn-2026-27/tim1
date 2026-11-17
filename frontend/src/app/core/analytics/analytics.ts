import { Injectable, effect, inject } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Auth } from '../auth/auth';
import { Consent } from '../consent/consent';
import { EventName, EventParams } from './event-catalog';

declare global {
  interface Window {
    dataLayer?: unknown[];
    gtag?: (...args: unknown[]) => void;
  }
}

// Jedini servis koji šalje u Google Analytics: page_view, događaji iz kataloga i uloga korisnika.
// Šalje samo uz saglasnost i samo ako je ID merenja podešen.
@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly consent = inject(Consent);
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);
  private readonly measurementId = environment.analyticsMeasurementId;
  private loaded = false;

  constructor() {
    // effect: prijava i odjava menjaju svojstvo korisnika u GA-u (van stabla komponenti).
    // Mora da bude pre effect-a za page_view, da bi page_view nosio ulogu (P6).
    effect(() => {
      const role = this.currentRole();
      const choice = this.consent.choice();
      if (this.measurementId === '' || choice !== 'granted' || !this.loaded) {
        return;
      }
      try {
        this.setRole(role);
      } catch {
        // GA greške se tiho gutaju
      }
    });

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

  // Poziv modula: šalje događaj iz kataloga, ali samo uz saglasnost i neprazan ID (P4).
  // Nikad ne baca izuzetak pozivaocu.
  track<N extends EventName>(name: N, params: EventParams[N]): void {
    if (this.measurementId === '' || this.consent.choice() !== 'granted') {
      return;
    }
    try {
      this.load();
      this.setDisabled(false);
      window.gtag?.('event', name, params);
    } catch {
      // GA greške se tiho gutaju
    }
  }

  // Sistemska uloga: administrator ima prednost; neprijavljen korisnik nema ulogu (P5, P6, P7).
  private currentRole(): 'explorer' | 'administrator' | null {
    const user = this.auth.user();
    if (user === null) {
      return null;
    }
    const roles = user.roles.map((role) => role.toLowerCase());
    if (roles.includes('administrator')) {
      return 'administrator';
    }
    if (roles.includes('explorer')) {
      return 'explorer';
    }
    return null;
  }

  // Uloga je svojstvo korisnika u GA-u, ne parametar događaja; null je briše.
  private setRole(role: string | null): void {
    window.gtag?.('set', 'user_properties', { user_role: role });
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
    this.setRole(this.currentRole());
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

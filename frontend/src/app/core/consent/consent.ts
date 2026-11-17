import { Injectable, signal } from '@angular/core';

const CONSENT_KEY = 'explorer.analytics-consent';

export type ConsentChoice = 'granted' | 'denied' | 'none';

function readChoice(): ConsentChoice {
  try {
    const stored = localStorage.getItem(CONSENT_KEY);
    if (stored === 'granted' || stored === 'denied') {
      return stored;
    }
  } catch {
    // localStorage nije dostupan: izbor važi samo dok je stranica otvorena
  }
  return 'none';
}

// Drugo zajedničko stanje u core/ (pored prijavljenog korisnika): izbor posetioca o merenju.
@Injectable({ providedIn: 'root' })
export class Consent {
  // Signal je i memorijski rezervni čuvar kada localStorage ne radi.
  readonly choice = signal<ConsentChoice>(readChoice());
  // Traka je otvorena dok izbora nema, ili kada je posetilac sam otvori.
  readonly bannerOpen = signal<boolean>(this.choice() === 'none');

  choose(choice: 'granted' | 'denied'): void {
    this.choice.set(choice);
    this.bannerOpen.set(false);
    try {
      localStorage.setItem(CONSENT_KEY, choice);
    } catch {
      // ostaje samo u memoriji
    }
  }

  openBanner(): void {
    this.bannerOpen.set(true);
  }

  closeBanner(): void {
    this.bannerOpen.set(false);
  }
}

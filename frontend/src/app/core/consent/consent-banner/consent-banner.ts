import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Consent } from '../consent';

@Component({
  selector: 'app-consent-banner',
  templateUrl: './consent-banner.html',
  styleUrl: './consent-banner.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConsentBanner {
  protected readonly consent = inject(Consent);

  // Oznaka trenutnog izbora; prazna dok izbora nema (traka za prvi dolazak)
  protected readonly currentLabel = computed(() => {
    const choice = this.consent.choice();
    if (choice === 'granted') {
      return 'Trenutni izbor: prihvaćeno';
    }
    if (choice === 'denied') {
      return 'Trenutni izbor: odbijeno';
    }
    return '';
  });
}

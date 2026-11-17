import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';
import { Analytics } from './core/consent/analytics';
import { Consent } from './core/consent/consent';
import { ConsentBanner } from './core/consent/consent-banner/consent-banner';

@Component({
  imports: [ConsentBanner, RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);
  protected readonly consent = inject(Consent);
  // Samo se stvara, da bi počeo da prati izbor i navigaciju
  private readonly analytics = inject(Analytics);
}

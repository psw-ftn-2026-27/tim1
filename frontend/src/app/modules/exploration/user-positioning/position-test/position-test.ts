import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MapPin } from '../../../../shared/ui/map/map';
import { CurrentPositionDto } from '../../api/exploration-api-types';
import { PositionPanel } from '../position-panel/position-panel';
import { UserPositioning, currentPositionUrl } from '../user-positioning';

@Component({
  selector: 'app-position-test',
  imports: [PositionPanel, RouterLink],
  templateUrl: './position-test.html',
})
export class PositionTest {
  private readonly positioning = inject(UserPositioning);
  protected readonly open = signal(false);
  protected readonly saving = signal(false);
  private readonly saved = signal(false);
  private readonly saveFailed = signal(false);
  private readonly previous = signal<CurrentPositionDto | null>(null);
  protected readonly position = httpResource<CurrentPositionDto | null>(() =>
    this.open() ? currentPositionUrl : undefined,
  );
  protected readonly confirmed = computed(() =>
    this.position.hasValue() ? this.position.value() : this.previous(),
  );
  protected readonly message = computed(() => {
    if (this.saving()) return 'Čuvamo poziciju…';
    if (this.saveFailed()) return 'Pozicija nije sačuvana. Pokušajte ponovo.';
    if (this.position.error()) {
      return this.saved()
        ? 'Pozicija je sačuvana, ali prikaz nije osvežen. Pokušajte ponovo da učitate poziciju.'
        : 'Poziciju nije moguće učitati.';
    }
    if (this.position.isLoading()) return 'Učitavanje pozicije…';
    if (this.saved() && this.confirmed()) return 'Pozicija je sačuvana';
    return this.confirmed() ? '' : 'Još nema sačuvane pozicije.';
  });

  protected show(): void {
    this.previous.set(null);
    this.saved.set(false);
    this.saveFailed.set(false);
    this.open.set(true);
  }

  protected async select(point: MapPin): Promise<void> {
    if (this.saving() || this.position.isLoading()) return;
    this.previous.set(this.confirmed());
    this.saveFailed.set(false);
    this.saved.set(false);
    this.saving.set(true);
    try {
      await this.positioning.save(point);
      this.saved.set(true);
      if (this.open()) this.position.reload();
    } catch {
      this.saveFailed.set(true);
    } finally {
      this.saving.set(false);
    }
  }

  protected reload(): void {
    this.saveFailed.set(false);
    this.position.reload();
  }
}

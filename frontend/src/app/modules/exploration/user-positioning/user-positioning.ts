import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SaveCurrentPositionDto } from '../api/exploration-api-types';

export const currentPositionUrl = '/api/exploration/user-positions/mine';

@Injectable({ providedIn: 'root' })
export class UserPositioning {
  private readonly http = inject(HttpClient);

  save(position: SaveCurrentPositionDto): Promise<void> {
    return firstValueFrom(this.http.put<void>(currentPositionUrl, position));
  }
}

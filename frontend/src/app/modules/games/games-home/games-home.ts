import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-games-home',
  imports: [RouterLink],
  template: `
    <h1>Games</h1>
    <a routerLink="/examples/map-pins">Probni primer mape — pinovi</a>
  `,
})
export class GamesHome {}

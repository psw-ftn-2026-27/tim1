import { Component, input } from '@angular/core';

// Kontura poklona u uglu poklon kartice. Na narandžastoj kartici je jača, na tamnoj blaža.
@Component({
  imports: [],
  selector: 'app-gift-card-art',
  styleUrl: './gift-card-art.scss',
  templateUrl: './gift-card-art.html',
})
export class GiftCardArt {
  readonly onAccent = input(false);
}

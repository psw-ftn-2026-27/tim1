// Katalog ključnih događaja: jedino mesto gde se definišu ime i parametri događaja.
// Uputstvo za dodavanje događaja je u README.md pored ove datoteke.
// Parametar nikad ne sme da nosi ime, mejl ili drugi lični podatak.
// Uloga korisnika nije parametar: servis je šalje kao svojstvo korisnika.

// Ime svakog događaja i tip njegovih parametara.
// Događaj koji čeka poziv ima parametre `never`: niko ne može da ga pozove dok tim koji je
// vlasnik funkcionalnosti ne dopiše parametre. Parametar nikad ne sme da nosi lični podatak.
export interface EventParams {
  tour_published: { tour_id: string; difficulty: string };
  view_item: never;
  add_to_cart: { product_type: string; product_id: string; price: number };
  purchase: { product_types: string; item_count: number; value: number };
  tour_started: never;
  tour_completed: never;
  challenge_solved: never;
}

export type EventName = keyof EventParams;

// Koraci izveštaja toka, redom: pregled -> korpa -> kupovina.
// Izveštaj se podešava u GA-u; ovde su samo označeni koraci.
export const FUNNEL_STEPS: EventName[] = ['view_item', 'add_to_cart', 'purchase'];

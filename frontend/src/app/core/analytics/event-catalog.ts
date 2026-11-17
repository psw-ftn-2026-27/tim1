// Katalog ključnih događaja: jedino mesto gde se definišu ime i parametri događaja (P2).
// Uputstvo za dodavanje događaja je u README.md pored ove datoteke.
// Parametar nikad ne sme da nosi ime, mejl ili drugi lični podatak (P8).
// Uloga korisnika nije parametar: servis je šalje kao svojstvo korisnika (P6).

// Ime svakog događaja i tip njegovih parametara; ovo je jedino mesto gde se definišu (P2).
// Događaj koji čeka poziv ima parametre `never`: niko ne može da ga pozove dok tim koji je
// vlasnik funkcionalnosti ne dopiše parametre. Parametar nikad ne sme da nosi lični podatak (P8).
export interface EventParams {
  tour_published: { tour_id: string; difficulty: string };
  view_item: never;
  add_to_cart: never;
  purchase: never;
  tour_started: never;
  tour_completed: never;
  challenge_solved: never;
}

export type EventName = keyof EventParams;

// Koraci izveštaja toka, redom: pregled -> korpa -> kupovina (P10).
// Izveštaj se podešava u GA-u; ovde su samo označeni koraci.
export const FUNNEL_STEPS: EventName[] = ['view_item', 'add_to_cart', 'purchase'];

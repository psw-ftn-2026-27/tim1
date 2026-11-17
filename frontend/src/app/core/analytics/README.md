# Beleženje događaja (`core/analytics/`)

Ovde žive tri stvari: servis `Analytics` (jedini koji šalje u Google Analytics), katalog
`event-catalog.ts` (ime i parametri svakog događaja, uz oznaku koraka izveštaja toka) i ovo
uputstvo. Pripada platformskom timu; timovi modula dodaju događaje po postupku ispod.

Servis šalje samo ako je posetilac dao saglasnost i ako je ID merenja podešen. Uloga
korisnika (`explorer` ili `administrator`) ide kao svojstvo korisnika, ne kao parametar, pa
je ne šaljete i nema je u katalogu.

## Kako dodajete događaj

1. Otvorite `event-catalog.ts`. Ako imena vašeg događaja nema, dodajte ga u `EventParams`.
2. Umesto `never` upišite tip parametara svog događaja; `track` prima samo objekat tog oblika, pa kompajler odbija drugačiji. Parametar ne sme da nosi ime, mejl ili drugi
   lični podatak (identifikator ture je u redu, ime autora nije).
3. U svom kodu (stranica ili servis grupe) ubrizgajte `Analytics` iz `core/analytics/` i
   pozovite `track` tek posle `await` komande, kad server potvrdi uspeh. Ako komanda baci
   izuzetak, do poziva se ne stiže, pa se neuspela radnja ne beleži.
4. Nemojte da pozivate GA direktno (`gtag`, `dataLayer`) i nemojte da uvozite ništa iz
   `core/consent`. `track` sam proverava saglasnost i nikad ne baca izuzetak.
5. Koraci izveštaja toka označeni su u `event-catalog.ts`. Ne menjate ih bez dogovora sa
   platformom.

Promena imena ili parametara događaja koji već neko poziva traži dogovor sa timom koji ga
poziva.

## Kako proveravate

1. Pokrenite aplikaciju sa podešenim ID-jem merenja i prihvatite kolačiće.
2. Otvorite mrežni panel pregledača (Network) i filtrirajte po `collect`.
3. Izvedite radnju. Treba da vidite tačno jedan događaj (GA može da spoji više događaja u jedan zahtev, pa
   gledajte događaje u zahtevu, ne broj zahteva); u upitu je ime događaja i vaši parametri, a uloga je u parametru `up.user_role`.
4. Neuspela radnja ne sme da pošalje ništa. Bez saglasnosti ne sme da ode nijedan zahtev.

## Podešavanje GA-a (radi vlasnik property-ja, van koda)

1. U Admin > Custom definitions registrujte `user_role` kao dimenziju sa opsegom Korisnik
   (User), jer je uloga svojstvo korisnika. Za svaki parametar iz kataloga koji vlasnik želi u
   izveštaju registrujte dimenziju sa opsegom Događaj (Event); bez toga GA parametar prima, ali
   ga u izveštajima ne prikazuje.
2. U Data streams > Enhanced measurement isključite "Page changes based on browser history
   events". Aplikacija sama šalje `page_view` pri svakoj promeni rute, pa bi uključena opcija
   davala dupli `page_view`.
3. Izveštaj toka pravi se u Explore > Funnel exploration od koraka označenih u
   `event-catalog.ts`.

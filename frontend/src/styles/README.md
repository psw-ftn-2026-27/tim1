# Tema i globalni stilovi

Ovaj direktorijum drži izgled cele aplikacije. Pripada platformskom timu; timovi modula ga
koriste, ali ga ne menjaju. Ako vam treba nova boja ili nova globalna klasa, uputite zahtev
po protokolu `docs/protocols/zahtevi-platformskom-timu.md`.

> **Status:** paleta i klase su usklađene sa dizajnom u `frontend/docs/design/` (redizajn
> Payment modula, grana `frontend/desing`). Otvorena pitanja su na kraju.

## Datoteke

| Datoteka | Šta sadrži |
|---|---|
| `_tokens.scss` | sve boje, razmaci, zaobljenja, senke i tipografija kao CSS promenljive |
| `_base.scss` | osnovna pravila za `body`, naslove i linkove |
| `_layout.scss` | `.container`, `.stack`, `.grid` i prelaz između stranica |
| `_components.scss` | globalne klase: dugme, kartica, polje, tabela, poruka, bedž, iznos |

Zaglavlje aplikacije (navigacija) nije ovde, već u `src/app/app.scss`. Levo je logo
(planina u narandžastom krugu i naziv) i vodi na početnu stranicu. Deo aplikacije u kom
se korisnik nalazi ima blagu narandžastu podlogu; klasu `active` dodaje `routerLinkActive`
(asistent ga je odobrio i dodat je u podskup u `frontend/AGENTS.md`). Desno su „Kolačići",
krug sa prvim slovom emaila, email i Logout. Na uskom ekranu delovi prelaze u novi red.

Prelaz između stranica je u `_layout.scss`: nova stranica se pojavi postepeno (250 ms, samo
providnost). Bez pomeranja, jer bi `transform` poremetio pozadinu naslovne stranice koja
koristi `position: fixed`. Ko u sistemu uključi „smanji animacije", prelaz ne vidi.

## Tri pravila za timove

1. **Nikad boja ili razmak direktno u kodu.** U `.scss` svoje komponente pišite
   `var(--color-accent)` i `var(--space-3)`, ne `#f28b43` i `12px`. Tako cela aplikacija
   menja izgled izmenom jednog tokena.
2. **Prvo potražite globalnu klasu** u katalogu ispod. Tek ako je nema, napišite stil u
   `.scss` svoje komponente.
3. **Ne prepisujte globalnu klasu lokalno** (npr. `.card { background: ... }` u svojoj
   komponenti). Ako vam treba drugačiji izgled, tražite varijantu od platformskog tima.

## Paleta: „Noćna ekspedicija"

Tamna zeleno-narandžasta paleta, kao na naslovnoj stranici, u nijansama iz dizajna
(`frontend/docs/design/`). Osećaj: šuma u sumrak, logorska vatra, avantura.

### Boje

| Token | Vrednost | Upotreba |
|---|---|---|
| `--color-bg` | `#131c1a` | pozadina stranice i polja za unos |
| `--color-surface` | `#182320` | kartice, paneli, dugme |
| `--color-surface-raised` | `#1d2a26` | površina unutar kartice (npr. forma ispod reda tabele) |
| `--color-surface-hover` | `#21302b` | hover na dugmetu i redu tabele |
| `--color-surface-gift` | `#1d3a35` | tamnozelena površina: poklon kartica kupljena za drugog |
| `--color-border` | `#24332e` | ivice kartica i linije |
| `--color-border-field` | `#2f413b` | ivica polja i dugmeta |
| `--color-text` | `#eef2ef` | glavni tekst i naslovi |
| `--color-text-muted` | `#a9b6b1` | opisi, oznake, `.meta` |
| `--color-text-subtle` | `#8d9b96` | najslabiji: zaglavlja tabela, napomene |
| `--color-text-disabled` | `#8d9b96` | neaktivni elementi |
| `--color-accent` | `#f08a3c` narandžasta | **primarna**: primarna dugmad, fokus, broj na tabu |
| `--color-accent-hover` | `#f59b55` | primarna na hover |
| `--color-accent-text` | `#1e1206` | tekst na primarnoj boji (**tamni**, ne beli) |
| `--color-accent-bg` | narandžasta 14 % | blaga podloga: aktivni meni, bedž `--accent` |
| `--color-link` / `-hover` | `#f5a262` / `#ffc08e` | narandžasta kao tekst: linkovi |
| `--color-secondary` / `-hover` / `-text` | providna / narandžasta 10 % / `#f5a262` | sekundarno (obrubljeno) dugme |
| `--color-gold` | `#e3a72f` senf | titule, pečati; kao pozadina **uvek sa tamnim tekstom** |

Tema je tamna, pa `_tokens.scss` postavlja i `color-scheme: dark`: browser tada sam crta
tamne scrollbarove, padajuće liste i kalendar u poljima za unos.

### Statusne boje

Statusna boja je **svetla** i koristi se za tekst; pozadina (`-bg`) i ivica (`-border`) su
ista boja, ali providna.

| Token | Tekst | Pozadina (`-bg`) | Ivica (`-border`) |
|---|---|---|---|
| `--color-success` | `#8fe0b4` | zelena 16 % | zelena 40 % |
| `--color-warning` | `#f0cf78` | žuta 14 % | (boja teksta) |
| `--color-danger` | `#f4b3b3` | crvena 10 % | crvena 40 % |
| `--color-info` | `#c4dbf8` | plava 10 % | (boja teksta) |

`--color-notification` (`#e5484d`) je crveni krug na ikonici kada ima nešto novo (npr. važeća
primljena poklon kartica na ikonici poklona uz naslov „Wallet").

### Kontrast

Tekst mora imati kontrast najmanje 4.5:1 prema pozadini (standard WCAG AA). Tamni tekst na
narandžastoj (`#1e1206` na `#f08a3c`) ima kontrast oko 7:1, a beli oko 2:1, zato je tekst
na narandžastoj tamni. `--color-text-subtle` je samo za sporedan tekst (zaglavlja tabela,
napomene), ne za sadržaj.

### Tipografija

| Token | Vrednost | Upotreba |
|---|---|---|
| `--font-family` | Inter | sav tekst |
| `--font-family-heading` | Nunito | naslovi `h1`–`h3`, veliki iznosi |
| `--font-family-mono` | sistemski monospace | identifikatori (npr. ID korisnika) |
| `--font-weight-normal` / `-bold` / `-heading` / `-display` | 400 / 600 / 800 / 900 | običan / istaknut / naslov / `h1` i veliki iznosi |
| `--font-size-xs` … `-xl`, `-2xl` | 0.75 / 0.875 / 1 / 1.25 / 1.75 / 2.75rem | xs bedž, sm oznake, md tekst, lg `h2`, 2xl `h1` (na telefonu xl) |

Fontovi se učitavaju sa Google Fonts u `src/index.html` (Inter 400–700, Nunito 700–900;
uključuju slova č, ć, đ, š, ž).

### Razmaci, zaobljenja, senke

| Token | Vrednosti |
|---|---|
| `--space-1` … `--space-7` | 0.25 / 0.5 / 0.75 / 1 / 1.5 / 2 / 3rem |
| `--radius-sm` / `--radius` / `--radius-lg` / `--radius-pill` | 0.25 / 0.75rem (dugme, polje) / 1.25rem (kartica) / potpuno zaobljeno (bedž) |
| `--control-height` | 2.75rem (44px): najmanja visina dugmeta i polja |
| `--shadow-sm` / `--shadow-md` | senka kartice / izraženija, na hover (jače nego na svetloj temi, inače se ne vide) |

## Katalog klasa

### Dugme `.button`

| Varijanta | Kada |
|---|---|
| `.button` | obična radnja („Otkaži", „Izmeni"): tamna površina sa ivicom |
| `.button--primary` | glavna radnja na stranici („Sačuvaj", „Kupi turu", „Istraži ture") — jedna po formi |
| `.button--secondary` | obrubljeno narandžasto dugme uz primarno („Request withdrawal") |
| `.button--danger` | obrubljeno crveno: odbijanje, brisanje, arhiviranje |
| `.button--ghost` | sporedna radnja koja ne sme da odvlači pažnju |
| `.button--small` | dugme u redu tabele ili na kartici (36px; samo u redu tabele) |
| `.button--danger-solid` | puno crveno: konačno „Delete" u potvrdi brisanja (`--color-danger-solid`) |

Stanja su automatska: hover, fokus tastaturom (`:focus-visible`), `disabled` (dugme izbledi).
Klasa važi i za link: `<a class="button" routerLink="…">`.
**Učitavanje:** dugme dobija `disabled`, a tekst se menja, npr. „Čuvanje…".

```html
<div class="form-actions">
  <button class="button button--primary" type="submit" [disabled]="saving()">
    {{ saving() ? 'Čuvanje…' : 'Sačuvaj' }}
  </button>
  <button class="button" type="button">Otkaži</button>
</div>
```

`.form-actions` poravnava dugmad u red ispod forme ili u ćeliji tabele.

### Polje forme `.field`

```html
<div class="field">
  <label for="name">Naziv ture</label>
  <input id="name" [formField]="form.name" />
</div>

<div class="field field--error">
  <label for="price">Cena</label>
  <input id="price" [formField]="form.price" />
  <p class="error">Cena mora biti pozitivna.</p>
</div>
```

Važi za `input`, `select` i `textarea`. Fokus i `disabled` su automatski; greška se uključuje
klasom `field--error`.

### Kartica `.card`

```html
<article class="card stack">…</article>
<article class="card card--interactive stack">…</article>
```

`.card--interactive` na hover dobija narandžastu ivicu i jaču senku; koristi se za kartice
na koje se klikće.

### Tabela `.table`

```html
<table class="table">
  <thead><tr><th>Datum</th><th>Iznos</th></tr></thead>
  <tbody>…</tbody>
</table>
<p class="empty">Još nemaš nijednu kupovinu.</p>
```

Red se osenči na hover. `.table--compact` daje manje razmake. Zaglavlje kolona je malim
velikim slovima.

Tabela u kartici, sa naslovom i vodoravnim pomeranjem na telefonu:

```html
<section class="table-card">
  <div class="table-card-header"><h2>My requests</h2><span class="meta">3 requests</span></div>
  <div class="table-scroll"><table class="table">…</table></div>
</section>
```

### Prazno stanje `.empty`

```html
<p class="empty">Još nemaš nijednu kupovinu.</p>

<div class="empty empty--boxed">
  <span class="empty-icon"><svg>…</svg></span>
  <h2>Your cart is empty</h2>
  <p>Find a tour you like and add it to your cart.</p>
  <a class="button" routerLink="…">Browse products</a>
</div>
```

`--boxed` dobija isprekidan okvir kada prazno stanje stoji samo, van kartice. Isti izgled
ima i poruka o učitavanju. U tabeli prazno stanje ide u ćeliju:
`<td colspan="4"><p class="empty">…</p></td>`.

### Tabovi `.tabs`

```html
<nav class="tabs" aria-label="Payment">
  <a routerLink="/payment" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Wallet</a>
  <a routerLink="/payment/cart" routerLinkActive="active">Cart <span class="tab-count">2</span></a>
</nav>
```

Za prelaz između srodnih stranica jednog modula. `.tab-count` je broj uz tab.

### Poruka na stranici `.alert`

```html
<p class="alert alert--success">Kupovina je uspešna.</p>
<p class="alert alert--warning">Na novčaniku imaš manje od cene ture.</p>
<p class="alert alert--danger">Kupon je istekao.</p>
<p class="alert alert--info">Tura je dostupna i bez interneta.</p>
```

Ikonica na početku poruke je inline SVG sa `stroke="currentColor"`, pa uzima boju poruke.

Za grešku jednog polja koristite `.error` ispod polja, ne `.alert`.

### Bedž `.badge`

```html
<span class="badge">Nacrt</span>
<span class="badge badge--success">Objavljena</span>
<span class="badge badge--warning">Na čekanju</span>
<span class="badge badge--danger">Arhivirana</span>
<span class="badge badge--info">Član kluba</span>
<span class="badge badge--accent">U korpi</span>
<span class="badge badge--gold">Otkriveno</span>
```

`badge--gold` je za titule i pečate (Games, Social).

### Iznos `.amount`

```html
<span class="amount">{{ formatAmount(price) }} EC</span>
<span class="amount amount--old">1.500 EC</span>
<span class="amount amount--positive">+500 EC</span>
<span class="amount amount--negative">−1.200 EC</span>
```

Broj se formatira funkcijom `formatAmount` iz `shared/util/format-amount.ts` (tačka za
hiljade, zarez za decimale: `5.200`, `1.200,5`). Komponenta je izloži šablonu kao
`protected readonly formatAmount = formatAmount;`.

Cifre su iste širine, pa se iznosi u koloni tabele poravnavaju. `--old` je precrtana cena
pored cene sa kuponom.

### Izabrano dugme `.button.is-selected`

```html
<button class="button" [class.is-selected]="myVote() === 'Upvote'" [attr.aria-pressed]="myVote() === 'Upvote'">Upvote</button>
```

Dugme iz grupe koje je trenutno izabrano (npr. glas korisnika). Uvek ide uz `aria-pressed`.

### Dugme sa ikonicom `.icon-btn` i kružić `.dot`

```html
<button class="icon-btn icon-btn--success" type="button" aria-label="Accept request from Ana">…svg…</button>
<button class="icon-btn icon-btn--danger" type="button" aria-label="Reject request from Ana">…svg…</button>
<a class="icon-btn" routerLink="/social/received-requests" aria-label="Received follow requests, 2 pending">
  …svg…
  <span class="dot"></span>
</a>
```

Kvadrat 44×44 (najmanja površina za dodir) sa samo ikonicom, zato je `aria-label` obavezan. `.dot` je crveni kružić
(`--color-notification`) u gornjem desnom uglu kad ima nešto novo.

### Kartica u redu `.list-card`

```html
<div class="list-card">
  <span class="list-card__avatar">A</span>
  <div class="list-card__main"><strong>Ana</strong><span class="meta">Sent Nov 18, 2026</span></div>
  <div class="list-card__end"><span class="badge badge--warning">Pending</span>…dugmad…</div>
</div>
```

Za liste ljudi (zahtevi, pratioci, članovi, pozivi). Na uskom ekranu `__end` prelazi u novi
red. `.confirm` unutar kartice zauzima ceo red.

### Potvrda u redu `.confirm`

```html
<div class="confirm">
  <span class="confirm__text">Archive this blog? It can’t be edited afterwards.</span>
  <button class="button button--small button--primary" type="button">Confirm</button>
  <button class="button button--small" type="button">Cancel</button>
</div>
```

Projekat nema dijaloge: potvrda zamenjuje dugmad u istom redu tabele ili na istoj kartici.
Otvorena je najviše jedna potvrda na stranici.

### Sličica `.thumb`

```html
<img class="thumb" [src]="club.images[0]" alt="" />
<app-protected-image class="thumb thumb--sm" [src]="…" />
```

Kvadrat 56px (`--sm`: 44px), slika ispunjava kvadrat (`object-fit: cover`). `--wide` je slika
pune širine u odnosu 16:9 (naslovna slika kartice, velika slika galerije).

### Obaveštenje `.notice`

```html
<p class="notice">…svg… This blog is archived. Comments and voting are closed.</p>
```

Neutralna traka za stanje koje nije greška (arhiviran blog, zatvoren klub). Za grešku se
koristi `.alert--danger`.

### Straničenje `.pager`

```html
<nav class="pager" aria-label="Pages">
  <button class="button button--small" type="button">Previous</button>
  <span class="meta">Page 1 of 3</span>
  <button class="button button--small" type="button">Next</button>
</nav>
```

### Ostalo

`.error` (tekst greške), `.meta` (sporedan tekst), `.back` (veza nazad na vrhu stranice,
„← Blogs"), a iz `_layout.scss`: `.container`, `.stack` (vertikalni razmak između dece),
`.grid` (mreža kartica), `.page-header` (vrh stranice: levo `h1` i podnaslov u `p`, desno
tabovi ili dugmad), `.page-narrow` (uska kolona do 820px) i `.layout-side` sa `__aside`
(oko 260px) i `__main` (dve kolone koje se na telefonu slažu jedna ispod druge; kolona koja
je prva u kodu stoji levo).

## Games

Izgled Games ekrana dolazi iz referenci u `frontend/docs/design/games/` (PNG, statički HTML i
`games-reference.css`). Reference imaju malo drugačije mere i boje od ostatka aplikacije
(npr. kartica 14px umesto 20px, dugme 10px, tekst 15px). Globalne klase se zbog toga **ne
menjaju**, jer bi se promenili drugi moduli. Umesto toga koren svake Games stranice ima klasu
`.games`, a pravila `.games .x` na kraju `_components.scss` daju globalnim klasama izgled
reference samo unutar Games-a.

```html
<div class="games">…lista punom širinom…</div>
<div class="games games--narrow">…forma ili detalji, kolona od 900px…</div>
```

### Boje reference → tokeni

Razlika je merena kao ΔE (CIE76, u Lab prostoru; providni tokeni su prvo stopljeni sa
podlogom na kojoj stoje). Razlika do oko 4 se golim okom ne vidi na tamnoj pozadini, pa ta
boja koristi postojeći token. Za veću razliku dodat je novi token.

| Hex reference | Uloga | Token | ΔE | |
|---|---|---|---|---|
| `#121a18` | pozadina stranice | `--color-bg` | 1.0 | postojeći |
| `#eef2f1` | tekst | `--color-text` | 1.0 | postojeći |
| `#ffffff` | naslovi, ime u tabeli, velike vrednosti | `--color-text-strong` | 0 | **novi** |
| `#f2913f` | link, CTA, veza nazad | `--color-accent` | 3.3 | postojeći |
| `#f7ad6e` | link na hover | `--color-accent-hover` | 4.0 | postojeći |
| `#1a2421` | kartica, tabela | `--color-surface` | 0.7 | postojeći |
| `#172120` | podnožje kartice, kartica pitanja, statistika | `--color-surface` | 1.8 | postojeći |
| `#26312e` | ivica kartice, podloga neutralnog bedža | `--color-border` | 2.3 | postojeći |
| `#222c29` | linija između redova tabele | `--color-border` | 2.0 | postojeći |
| `#33403c` / `#2c3835` / `#2a3532` | ivica dugmeta, polja, mape, kartice pitanja | `--color-border-field` | 2.5–3.3 | postojeći |
| `#141c1a` / `#111816` | podloga dugmeta, odgovora i polja | `--color-bg` | 0.5 / 2.2 | postojeći |
| `#f28b3c` / `#f59b55` | primarna / na hover | `--color-accent` / `--color-accent-hover` | 0.8 / 0 | postojeći |
| `#1f140b` | tekst na primarnoj | `--color-accent-text` | 1.2 | postojeći |
| `#f39a9a` | tekst obrubljenog crvenog dugmeta, ikonica „Not this time" | `--color-error-strong` | 0 | **novi** |
| `#3a2020` | krug oko ikonice „Not this time" | `--color-error-strong-bg` | 0 | **novi** |
| `#5c3434` | ivica crvenog dugmeta i poruke o grešci | `--color-error-border` | 0 | **novi** |
| `#d0504f` | puno crveno dugme „Delete" | `--color-danger-solid` (vrednost promenjena) | 0 | postojeći |
| `#3a3420` / `#ecc95f` | Draft, upozorenje | `--color-state-draft-bg` / `--color-state-draft` | 0 | **novi** |
| `#1f3a2c` / `#74d6a0` | Published, „Correct!" | `--color-state-published-bg` / `--color-state-published` | 0 | **novi** |
| `#3f2c20` / `#f3a96b` | Retired | `--color-state-retired-bg` / `--color-state-retired` | 0 | **novi** |
| `#2b3540` / `#bac6d2` | Archived | `--color-state-archived-bg` / `--color-state-archived` | 0 | **novi** |
| `#22344a` / `#9cc0f0` | bedž „Quiz" | `--color-type-quiz-bg` / `--color-type-quiz` | 0 | **novi** |
| `#3a2a1e` | bedž `--accent`, ikonica kartice huba, pehar | `--color-accent-soft` | 0 | **novi** |
| `#f6a160` | tekst bedža `--accent` | `--color-link` | 1.2 | postojeći |
| `#c9d2cf` | neutralni bedž, slovo odgovora, ikonica praznog stanja | `--color-text-soft` | 0 | **novi** |
| `#a3aeab` / `#b4bfbc` | prigušen tekst, podnaslov | `--color-text-muted` | 3.1 / 3.8 | postojeći |
| `#8e9a97` | najslabiji tekst | `--color-text-subtle` | 1.4 | postojeći |
| `#7d8986` | placeholder | `--color-text-placeholder` | 0 | **novi** |
| `#f2c14e` | XP | `--color-xp` | 0 | **novi** |
| `#251d1a` | red sa potvrdom brisanja | `--color-row-danger-bg` | 0 | **novi** |
| `#f6cfcf` | pitanje u potvrdi brisanja | `--color-confirm` | 0 | **novi** |
| `#2e1c1c` / `#f6c4c4` | poruka o grešci | `--color-error-bg` / `--color-error` | 0 | **novi** |
| `#1b3226` / `#2e5a40` | poruka o uspehu: podloga / ivica | `--color-success-bg` / `--color-success-border` | 2.1 / 2.6 | postojeći |
| `#a9e8c3` | poruka o uspehu: tekst | `--color-alert-success` | 0 | **novi** |
| `#1c2836` / `#2e4560` | info poruka: podloga / ivica | `--color-alert-info-bg` / `--color-alert-info-border` | 0 | **novi** |
| `#bcd4f2` | info poruka: tekst | `--color-info` | 2.6 | postojeći |
| `#1c2725` | odgovor u listi pitanja | `--color-surface-raised` | 2.1 | postojeći |
| `#1d3328` / `#bfeccf` | tačan odgovor | `--color-correct-bg` / `--color-correct` | 0 | **novi** |
| `#1d1a16` | odgovor na hover | `--color-option-hover-bg` | 0 | **novi** |
| `#6fa8ff` | legenda „You" na mapi | (nije korišćeno, vidi napomenu o mapi) | – | – |

Mere iz reference koje ne postoje na skali dobile su tokene: razmaci `--space-1-5` (6px),
`--space-2-5` (10px), `--space-3-5` (14px), `--space-4-5` (20px), `--space-5-5` (28px),
`--space-6-5` (40px); zaobljenja `--radius-xs` (8px), `--radius-control` (10px),
`--radius-card` (14px); tekst `--font-size-caption` (13px), `--font-size-ui` (15px),
`--font-size-title-sm` (18px), `--font-size-stat` (30px), `--font-size-title` (40px); i
`--control-height-field` (46px, polje za unos). Razmaci od 18 i 22px su zaokruženi na 16
odnosno 24px (razlika 2px).

### Klase reference → klase aplikacije

| Referenca | Aplikacija | Kako |
|---|---|---|
| `.page`, `.page--narrow` (948px) | `.games`, `.games--narrow` (900px sadržaja) | nova |
| `.back-link` | `.back` | postojeća, Games stil (narandžasta, razmak 6px) |
| `.page-header`, `.lead` | `.page-header` i `p` u njemu | postojeća, Games stil (h1 40px) |
| `.button`, `--primary`, `--sm`, `--ghost` | `.button`, `--primary`, `--small` | postojeća, Games stil (10px, 15px, tamnija podloga) |
| `.button--danger` | `.button--danger` | postojeća, Games stil (boje reference) |
| `.button--danger-solid` | `.button--danger-solid` | postojeća (iz prethodnog Games rada), nova boja |
| `.card` | `.card` | postojeća, Games stil (14px) |
| `.card-head`, `.card-body`, `.card-foot`, `.card-title` | `.card--flush` + `.card-head`, `.card-body`, `.card-footer`, `.card-title` | nove |
| `.badge`, `--neutral` | `.badge` | postojeća, Games stil (svetliji siv tekst) |
| `.badge--info` (Quiz) | `.badge--quiz` | nova (globalni `--info` izgleda drugačije) |
| `.badge--accent` | `.badge--accent` | postojeća, Games stil (puna podloga) |
| `.badge--draft`, `--published`, `--retired`, `--archived` | iste | nove |
| `.table-card`, `.table-scroll`, `.table` | iste | postojeće, Games stil (15px, zaglavlje, razmaci) |
| `td.name a`, `.num`, `.end` | iste | Games stil |
| `.table-foot`, `.pagination` | `.table-card-footer`, `.pagination` | nove |
| `tr.is-confirming` | `tr.row--danger` | nova |
| `tr.row-message` | `tr.row-message` | nova |
| `.inline-confirm` | `.confirm` + `.confirm__text` | postojeća (Social), Games stil (boja pitanja) |
| `.actions` | `.row-actions` (u redu i zaglavlju), `.form-actions` (forma) | nova / postojeća |
| `.alert`, `--error`, `--success`, `--info` | `.alert`, `--danger`, `--success`, `--info` | postojeća, Games stil (boje reference, tekst u `<p>`) |
| dugme × u poruci | `.alert__dismiss` | nova (površina 44px, ikonica 16px) |
| `.field`, `.label`, `.label-hint`, `.input`, `.help` | `.field`, `.label`, `.label-hint`, `.help` | postojeća `.field`, Games stil (oznaka svetla, polje 46px); ostale nove |
| `.form-section` | `.form-section` | nova |
| `.cols-2`, `.cols-3`, `.split` | iste | nove |
| `.map` (SVG crtež) | `<app-map>` u `.map-frame`; natpis `.map-hint` | nove; mapa ostaje I11 komponenta |
| `.empty`, `--boxed`, `.empty-icon` | iste | postojeće, Games stil (siva ikonica, 14px) |
| `.dl`, `.wide`, `dd.text` | `.data-list`, `__wide`, `.data-list__text` | nova |
| `.muted` + kurziv | `.unknown` | nova |
| `.tile-grid`, `.tile`, `.tile-icon`, `.tile-cta`, `.section-title` | iste (+ `.tile-title`, `.tile-text`) | nove |
| `.toolbar`, `.search`, `.select-field` | `.toolbar`, `.search-box` (lokalno u „All challenges") | lokalne |
| `.question-card`, `.question-head`, `.answer-row`, `.correct-toggle` | `.question-card` (globalna); ostale lokalno u formi kviza | nova / lokalne |
| `.icon-button` | `.icon-btn` | postojeća (Social), Games stil (prigušena) |
| `.add-link`, `.warn-text` | iste | nove |
| `.q-list`, `.q-item`, `.q-num`, `.a-list` | iste, lokalno u detaljima kviza | lokalne |
| `.quiz-grid`, `.quiz-card`, `.tags`, `.foot` | `.quiz-grid`, `.quiz-card`, `__tags`, `__name`, `__footer` | nove |
| `.play-meta`, `.points`, `.progress`, `.question-text` | iste (`.progress__fill` umesto `span`) | nove |
| `.options`, `.option`, `.option-key` | iste | nove |
| `.outcome`, `.outcome-icon--ok/--no` | `.outcome`, `.outcome__icon--success/--danger` | nove |
| `.stats`, `.stat` | `.stats`, `.stat`, `.stat__value`, `.stat__label` | nove |
| `.xp` | `.xp` | nova |

### Katalog Games klasa

```html
<!-- Bedž stanja izazova, isti na svim ekranima; „Quiz" i „Social" bedž tipa -->
<span [class]="'badge badge--' + status.toLowerCase()">{{ status }}</span>
<span class="badge badge--quiz">Quiz</span> <span class="badge badge--accent">Social</span>

<!-- XP: žuto i podebljano, na svakom mestu -->
<span class="xp">40 XP</span>

<!-- Kartica-link na hubu -->
<h2 class="section-title">Administration</h2>
<div class="tile-grid">
  <a class="tile" routerLink="/games/quizzes">
    <span class="tile-icon"><svg aria-hidden="true">…</svg></span>
    <h3 class="tile-title">Quizzes</h3>
    <p class="tile-text">One sentence about the page.</p>
    <span class="tile-cta">Manage quizzes <svg aria-hidden="true">…</svg></span>
  </a>
</div>

<!-- Kartica sa zaglavljem, telom i podnožjem -->
<form class="card card--flush">
  <div class="card-head"><h2 class="card-title">Details</h2></div>
  <div class="card-body">…</div>
  <div class="card-footer">
    <span class="meta">New quizzes start as drafts.</span>
    <div class="form-actions"><a class="button">Cancel</a><button class="button button--primary">Save draft</button></div>
  </div>
</form>

<!-- Poruka sa dugmetom za zatvaranje -->
<div class="alert alert--danger" role="alert">
  <svg aria-hidden="true">…</svg><p>Instructions are required.</p>
  <button class="alert__dismiss" type="button" aria-label="Dismiss"><svg aria-hidden="true">…</svg></button>
</div>

<!-- Tabela: potvrda brisanja u redu i red-poruka ispod reda -->
<tr class="row--danger">… <td><div class="confirm"><span class="confirm__text">Delete this challenge?</span>
  <button class="button button--small">Cancel</button>
  <button class="button button--small button--danger-solid">Delete</button></div></td></tr>
<tr class="row-message"><td colspan="5"><div class="alert alert--danger" role="alert">…</div></td></tr>
<div class="table-card-footer"><span>1–10 of 19 challenges</span><nav class="pagination">…</nav></div>

<!-- Lista podataka i mapa u dve kolone -->
<div class="card-body split">
  <dl class="data-list">
    <div><dt>XP</dt><dd class="xp">40</dd></div>
    <div class="data-list__wide"><dt>Instructions</dt><dd class="data-list__text">…</dd></div>
  </dl>
  <app-map [pins]="pins()" />
</div>
<span class="unknown">Unknown</span>

<!-- Forma: odeljak, kolone, mapa sa natpisom, pitanje -->
<section class="form-section"><h2>Quiz location <span class="label-hint">· optional</span></h2><p class="help">…</p></section>
<div class="cols-2">…</div> <div class="cols-3">…</div>
<div class="map-frame"><app-map … /><span class="map-hint">Click to set the location</span></div>
<div class="question-card">…<button class="add-link">+ Add answer</button><p class="warn-text">…</p></div>

<!-- Rešavanje kviza -->
<div class="quiz-grid"><a class="quiz-card">
  <div class="quiz-card__tags"><span class="badge">At a place</span></div>
  <h2 class="quiz-card__name">Mostovi Novog Sada</h2>
  <div class="quiz-card__footer"><span class="xp">up to 40 XP</span><span class="tile-cta">Open</span></div>
</a></div>
<div class="play-meta"><span>Question <b>2</b> of 5</span><span class="points">★ 1 point</span></div>
<div class="progress" role="progressbar" aria-valuemin="0" aria-valuemax="5" aria-valuenow="1">
  <div class="progress__fill" [style.width.%]="20"></div>
</div>
<h1 class="question-text">…</h1>
<div class="options"><button class="option"><span class="option-key">A</span> Most slobode</button></div>
<div class="outcome"><span class="outcome__icon outcome__icon--success">…</span><h2>Correct!</h2><p>…</p></div>
<div class="stats"><div class="stat"><span class="stat__value">4 / 5</span><span class="stat__label">correct answers</span></div></div>
```

Beli tekst na punom crvenom dugmetu (`#d0504f`, iz reference) ima kontrast oko 4.2:1, malo
ispod 4.5:1 za običan tekst. Boja je ostavljena kao u referenci; ako se kontrast pooštri,
dovoljno je potamneti `--color-danger-solid`.

`.outcome__icon` bez varijante je narandžast (pehar), `--success` zelen, `--danger` crven.
`.option` je visok najmanje 60px. Datum („03 Nov 2026") formatira `formatDate` iz
`shared/util/format-date.ts`.

## Odluke

- **Tema prati naslovnu stranicu.** Prvi nacrt („Gradski putopis": svetla pozadina,
  petrolej i terakota) napušten je kada je naslovna stranica napravljena u tamnoj
  zeleno-narandžastoj paleti. Dve palete u istoj aplikaciji delovale bi kao dve aplikacije.
- **Tamna tema, bez svetle varijante.** Jedna paleta je jednostavnija za učenje i održavanje;
  prekidač svetla/tamna bi tražio drugi skup tokena i kod.
- **Tekst na narandžastoj je tamni.** Beli tekst na `#f28b43` ima kontrast oko 2:1, a tamni
  oko 7:1. Isto važi za senf.
- **Kartica je neutralna površina.** Boju nose sadržaj i dugmad, ne kartica.
- **Web fontovi (Inter, Nunito) umesto sistemskog fonta**, da bi aplikacija izgledala kao
  dizajn. Ranija odluka (sistemski font, bez zahteva ka spoljnom servisu) je napuštena;
  posledica je da browser pri svakom učitavanju šalje zahtev serverima Google Fonts.
- **Nazivi tokena se ne menjaju.** Primarna boja i dalje je `--color-accent`, jer je moduli
  već koriste; promenjene su samo vrednosti.
- **`--color-highlight` je uklonjen.** Imao je istu vrednost kao primarna boja i nije ga
  koristila nijedna klasa. Ako ikonice kasnije dobiju posebnu boju, token se vraća.
- **Sekundarno dugme je obrubljeno narandžastom**, a obično dugme (`.button`) je tamna
  površina sa ivicom. Druga puna boja pored narandžaste bi se otimala za pažnju.
- **Neaktivno dugme samo izbledi** (`opacity: 0.4`), pa se i dalje vidi koje je vrste.
- **Klase iz redizajna Social-a su globalne iako ih za sada koristi samo Social.**
  `.list-card`, `.icon-btn`, `.dot`, `.confirm`, `.thumb`, `.notice`, `.pager`, `.back`,
  `.page-narrow` i `.layout-side` koriste dve ili više stranica, a isti obrasci (lista
  ljudi, potvrda u redu, veza nazad) postoje i u Payment-u i Games-u. Zato su odmah
  globalne, a ne kopirane u petnaestak komponenti. Ovo je izuzetak od pravila „više od
  jednog modula" iz `frontend/AGENTS.md`.
- **Games ima svoj izgled pod `.games`.** Reference Games-a imaju druge mere i nijanse od
  ostatka aplikacije. Globalne klase se ne menjaju (promenili bi se drugi moduli); pravila
  `.games .x` važe samo unutar Games stranica. Nove klase (bedževi stanja, `.xp`, `.tile`,
  `.quiz-card`…) su globalne po zadatku redizajna, iako ih za sada koristi samo Games.
- **`.icon-btn` je 44×44** (bio je 40×40), da bi svako dugme imalo površinu za dodir od
  najmanje 44px. Jedini izuzetak su `--small` dugmad u redu tabele (36px).
- **Datum se prikazuje kao „03 Nov 2026"** funkcijom `formatDate` iz `shared/util/format-date.ts`.
- **Statusni tokeni se nisu menjali u redizajnu Social-a.** Skica ima malo drugačije
  vrednosti (npr. `#7fd6a4` za uspeh), ali tokeni `--color-success`, `-warning`, `-danger`
  i `-info` već postoje i važe za celu aplikaciju.

## Za timove modula

Postoje lokalni stilovi koje sada pokriva globalna klasa. Svaki tim ih menja sam, u svom
modulu:

| Modul | Lokalno | Globalna zamena |
|---|---|---|
| Exploration (`my-tours`) | `.actions` | `.form-actions` |
| Exploration (`tour-card`) | `.tags li` | `.badge` |

## Otvorena pitanja

- **Modal.** `<dialog>` zahteva referencu na element, što podskup ne dozvoljava. Do odluke:
  potvrda na samoj stranici ili posebna stranica.
- **Toast obaveštenja.** Zahtevaju komponentu u `shared/ui` i servis. Do odluke: `.alert`.
- **Formatiranje iznosa** je rešeno običnom funkcijom `formatAmount` u `shared/util/`, bez
  pipe-a. Za sada je koristi samo Payment.
- **Lokalni tokeni na naslovnoj.** `core/home/home.scss` na `:host` ponovo definiše
  `--color-bg`, `--color-surface`, `--color-text`, `--color-accent` i ostale. Naslovna treba
  da koristi globalne tokene; vrednosti su sada iste, pa se te linije brišu u posebnoj
  izmeni (ostaje samo `--alt`).

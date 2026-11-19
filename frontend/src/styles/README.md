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

`--color-danger-solid` (`#d4393e`, `-hover`, `-text` beli) je puno crveno dugme za konačno
brisanje. Beli tekst na njemu ima kontrast oko 4.7:1.

Stanja izazova (Games) imaju svoje boje: Draft koristi `--color-warning`, Published
`--color-success`, a Retired (narandžasto-smeđa) i Archived (sivo-plava) imaju tokene
`--color-retired` / `--color-retired-bg` i `--color-archived` / `--color-archived-bg`.

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
| `.button--danger-solid` | puno crveno: konačno „Delete" u potvrdi brisanja |

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

Stanja izazova (Games) imaju svoj bedž, isti na svim ekranima:

```html
<span [class]="'badge badge--' + status.toLowerCase()">{{ status }}</span>
<!-- badge--draft (žuto-smeđi), badge--published (zeleni), badge--retired (narandžasto-smeđi),
     badge--archived (sivo-plavi) -->
```

### XP `.xp`

```html
<span class="xp">40 XP</span>
```

Iskustveni poeni su uvek senf boje i podebljani, sa ciframa iste širine.

### Kartica-link na hubu `.tile`

```html
<div class="tile-grid">
  <a class="tile" routerLink="/games/quizzes">
    <span class="tile-icon"><svg aria-hidden="true">…</svg></span>
    <h3 class="tile-title">Quizzes</h3>
    <p class="tile-text">One sentence about the page.</p>
    <span class="tile-cta">Open quizzes →</span>
  </a>
</div>
```

Cela kartica je link; na hover ivica postaje narandžasta. Mreža stavlja karticu od najmanje
300px.

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

Potvrda **brisanja** u redu tabele: red dobija `.row--danger` (blago crvena podloga), a
dugmad su „Cancel" i puno crveno „Delete":

```html
<tr class="row--danger">
  …
  <td>
    <div class="confirm">
      <span class="confirm__text">Delete this challenge?</span>
      <button class="button button--small" type="button">Cancel</button>
      <button class="button button--small button--danger-solid" type="button">Delete</button>
    </div>
  </td>
</tr>
```

### Red-poruka u tabeli `.row-message`

```html
<tr class="row-message">
  <td colspan="5"><p class="alert alert--danger" role="alert">Can't publish: …</p></td>
</tr>
```

Greška akcije pokrenute iz jednog reda, odmah ispod tog reda, preko svih kolona.

### Podnožje tabele `.table-card-footer`

```html
<section class="table-card">
  <div class="table-scroll"><table class="table">…</table></div>
  <div class="table-card-footer"><span class="meta">1–10 of 19 challenges</span><nav>…</nav></div>
</section>
```

### Podnožje forme `.card-footer`

```html
<form class="card stack">
  …
  <div class="card-footer">
    <p class="meta">New quizzes start as drafts.</p>
    <a class="button" routerLink="…">Cancel</a>
    <button class="button button--primary" type="submit">Save draft</button>
  </div>
</form>
```

Napomena (`.meta` kao prvo dete) stoji levo, dugmad desno.

### Lista podataka `.data-list` i `.unknown`

```html
<dl class="data-list">
  <div><dt>XP</dt><dd><span class="xp">40</span></dd></div>
  <div class="data-list__wide"><dt>Instructions</dt><dd>…</dd></div>
</dl>
<span class="unknown">Unknown</span>
```

Prikaz sačuvanih podataka pre izmene: oznaka iznad vrednosti, više kolona na širem ekranu.
`__wide` zauzima ceo red. `.unknown` je prigušena vrednost u kurzivu kad podatak nedostaje.

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

### Games: rešavanje kviza

```html
<!-- Kartica kviza u mreži (.grid), cela kartica je link -->
<a class="quiz-card" routerLink="…">
  <div class="quiz-card__tags"><span class="badge">At a place</span></div>
  <h2 class="quiz-card__name">Mostovi Novog Sada</h2>
  <div class="quiz-card__footer"><span class="xp">up to 40 XP</span><span class="tile-cta">Open →</span></div>
</a>

<!-- Napredak -->
<div class="progress" role="progressbar" aria-valuemin="0" aria-valuemax="5" aria-valuenow="2">
  <div class="progress__fill" [style.width.%]="40"></div>
</div>

<!-- Odgovor kao dugme pune širine -->
<button class="option" type="button"><span class="option-key">A</span> Most slobode</button>

<!-- Ishod, centriran -->
<div class="outcome">
  <span class="outcome__icon outcome__icon--success"><svg aria-hidden="true">…</svg></span>
  <h2>Correct!</h2>
</div>

<!-- Statistike -->
<div class="stats">
  <div class="stat"><span class="stat__value">4 / 5</span><span class="stat__label">correct answers</span></div>
</div>
```

`.outcome__icon` bez varijante je narandžast (pehar), `--success` zelen, `--danger` crven.
`.option` je visok najmanje 60px.

### Ostalo

`.error` (tekst greške), `.meta` (sporedan tekst), `.back` (veza nazad na vrhu stranice,
„← Blogs"), a iz `_layout.scss`: `.container`, `.stack` (vertikalni razmak između dece),
`.grid` (mreža kartica), `.page-header` (vrh stranice: levo `h1` i podnaslov u `p`, desno
tabovi ili dugmad), `.page-narrow` (uska kolona do 820px) i `.layout-side` sa `__aside`
(oko 260px) i `__main` (dve kolone koje se na telefonu slažu jedna ispod druge; kolona koja
je prva u kodu stoji levo).

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
- **Klase iz redizajna Games-a su globalne po zadatku redizajna.** Bedževi stanja, `.xp`,
  `.tile`, `.card-footer`, `.data-list` i klase za tabele mogu da koriste i drugi moduli.
  Klase za rešavanje kviza (`.quiz-card`, `.progress`, `.option`, `.outcome`, `.stats`) za sada
  koristi samo Games; zadatak je tražio da budu u katalogu, pa su izdvojene u poseban odeljak
  na kraju `_components.scss`.
- **`.icon-btn` je 44×44** (bio je 40×40), da bi svako dugme imalo površinu za dodir od
  najmanje 44px. Jedini izuzetak su `--small` dugmad u redu tabele (36px).
- **Datum se prikazuje kao „15 Nov 2026"** funkcijom `formatDate` iz `shared/util/format-date.ts`.
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

# Tema i globalni stilovi

Ovaj direktorijum drži izgled cele aplikacije. Pripada platformskom timu; timovi modula ga
koriste, ali ga ne menjaju. Ako vam treba nova boja ili nova globalna klasa, uputite zahtev
po protokolu `docs/protocols/zahtevi-platformskom-timu.md`.

> **Status:** nacrt (W1, D11 – Projektovanje teme i UI komponenti). Grana `platform/tema`,
> još nije spojena u `development`. Otvorena pitanja su na kraju.

## Datoteke

| Datoteka | Šta sadrži |
|---|---|
| `_tokens.scss` | sve boje, razmaci, zaobljenja, senke i tipografija kao CSS promenljive |
| `_base.scss` | osnovna pravila za `body`, naslove i linkove |
| `_layout.scss` | `.container`, `.stack`, `.grid` i prelaz između stranica |
| `_components.scss` | globalne klase: dugme, kartica, polje, tabela, poruka, bedž, iznos |

Zaglavlje aplikacije (navigacija) nije ovde, već u `src/app/app.scss`. Levo je logo, isti
SVG kao na naslovnoj, i vodi na početnu stranicu. Deo aplikacije u kom
se korisnik nalazi označen je narandžastom bojom i linijom ispod linka; klasu `active`
dodaje `routerLinkActive` (asistent ga je odobrio i dodat je u podskup u
`frontend/AGENTS.md`).

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

Paleta je preuzeta sa naslovne stranice (`core/home`), da bi cela aplikacija izgledala kao
njen nastavak. Osećaj: šuma u sumrak, logorska vatra, avantura.

### Boje

| Token | Vrednost | Upotreba |
|---|---|---|
| `--color-accent` | `#f28b43` narandžasta | **primarna**: primarna dugmad, linkovi, istaknute reči, fokus polja |
| `--color-accent-hover` | `#f5a066` | primarna na hover (svetlija) |
| `--color-accent-text` | `#101d1b` | tekst na primarnoj boji (**tamni**, ne beli) |
| `--color-secondary` | `transparent` | sekundarno dugme je obrubljeno, bez pozadine |
| `--color-secondary-hover` | `#273d37` | pozadina sekundarnog dugmeta na hover |
| `--color-secondary-text` | `#f5f5ef` | tekst i ivica sekundarnog dugmeta |
| `--color-gold` | `#e3a72f` senf | bedževi, titule, pečati „otkriveno"; kao pozadina **uvek sa tamnim tekstom** |
| `--color-bg` | `#101d1b` tamna šumsko-zelena | pozadina stranice |
| `--color-surface` | `#1c302c` | kartice, forme, polja za unos |
| `--color-surface-hover` | `#273d37` | hover na dugmetu i redu tabele |
| `--color-border` | `#424c4a` | ivice i linije |
| `--color-text` | `#f5f5ef` topla bela | glavni tekst i naslovi |
| `--color-text-muted` | `#c3c5c0` | opisi, oznake, `.meta` |
| `--color-text-disabled` | `#7d8a86` | neaktivni elementi |

Tema je tamna, pa `_tokens.scss` postavlja i `color-scheme: dark`: browser tada sam crta
tamne scrollbarove, padajuće liste i kalendar u poljima za unos.

### Statusne boje

Na tamnoj pozadini statusna boja je **svetla** (koristi se za tekst i ivicu), a njena
pozadina (`-bg`) je tamna nijansa iste boje.

| Token | Vrednost | Pozadina (`-bg`) |
|---|---|---|
| `--color-success` | `#7dd3a0` | `#18392c` |
| `--color-warning` | `#e3a72f` senf | `#3a3018` |
| `--color-danger` | `#ff8a7a` | `#3d1f1c` |
| `--color-info` | `#7cc4e8` | `#16323f` |

### Kontrast

Tekst mora imati kontrast najmanje 4.5:1 prema pozadini (standard WCAG AA). Provereno:

| Kombinacija | Kontrast | |
|---|---|---|
| `--color-text` na `--color-bg` / `--color-surface` | ~14:1 / ~11:1 | prolazi |
| `--color-text-muted` na `--color-surface` | ~8:1 | prolazi |
| tamni tekst (`--color-accent-text`) na `--color-accent` | ~7:1 | prolazi |
| `--color-accent` kao tekst na `--color-bg` / `--color-surface` | ~7:1 / ~5.7:1 | prolazi |
| `--color-accent` kao tekst na `--color-surface-hover` | ~4.7:1 | prolazi, tesno |
| tamni tekst (`--color-bg`) na `--color-gold` | ~8:1 | prolazi |
| statusna boja na svojoj `-bg` pozadini (sve četiri) | 6–7:1 | prolazi |
| beli tekst na `--color-accent` | ~2:1 | **ne prolazi**, zato je tekst na narandžastoj tamni |
| `--color-text-disabled` na `--color-surface` | nizak | dozvoljeno, pravilo ne važi za neaktivne elemente |

### Tipografija

| Token | Vrednost | Upotreba |
|---|---|---|
| `--font-family` | Segoe UI, sistemski | sav tekst |
| `--font-family-heading` | isto što i `--font-family` | naslovi `h1`–`h3` (boja: `--color-text`) |
| `--font-weight-normal` / `-bold` / `-heading` | 400 / 600 / 800 | običan / istaknut / naslov |
| `--letter-spacing-heading` | -0.01em | malo zbijenija slova u naslovima |
| `--font-size-xs` … `-xl` | 0.75 / 0.875 / 1 / 1.25 / 1.75rem | xs bedž, sm oznake, md tekst, lg `h2`, xl `h1` |

Font već postoji na računaru korisnika, ništa se ne preuzima sa interneta. Istaknutu reč u
naslovu, kao „poznato." na naslovnoj, obojite primarnom bojom u `.scss` svoje komponente:
`color: var(--color-accent)`.

### Razmaci, zaobljenja, senke

| Token | Vrednosti |
|---|---|
| `--space-1` … `--space-7` | 0.25 / 0.5 / 0.75 / 1 / 1.5 / 2 / 3rem |
| `--radius-sm` / `--radius` / `--radius-lg` / `--radius-pill` | 0.25 / 0.5 / 1rem / potpuno zaobljeno |
| `--shadow-sm` / `--shadow-md` | senka kartice / izraženija, na hover (jače nego na svetloj temi, inače se ne vide) |

## Katalog klasa

### Dugme `.button`

| Varijanta | Kada |
|---|---|
| `.button` | obična radnja („Otkaži", „Izmeni") |
| `.button--primary` | glavna radnja na stranici („Sačuvaj", „Kupi turu", „Istraži ture") — jedna po formi |
| `.button--secondary` | obrubljeno dugme uz primarno („Pogledaj blogove", „Registruj se") |
| `.button--danger` | brisanje, arhiviranje |
| `.button--ghost` | sporedna radnja koja ne sme da odvlači pažnju |
| `.button--small` | dugme u redu tabele ili na kartici |

Stanja su automatska: hover, fokus tastaturom (`:focus-visible`), `disabled`.
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

Red se osenči na hover. `.table--compact` daje manje razmake. `.empty` je poruka kada lista
ili tabela nema stavki.

### Poruka na stranici `.alert`

```html
<p class="alert alert--success">Kupovina je uspešna.</p>
<p class="alert alert--warning">Na novčaniku imaš manje od cene ture.</p>
<p class="alert alert--danger">Kupon je istekao.</p>
<p class="alert alert--info">Tura je dostupna i bez interneta.</p>
```

Za grešku jednog polja koristite `.error` ispod polja, ne `.alert`.

### Bedž `.badge`

```html
<span class="badge">Nacrt</span>
<span class="badge badge--success">Objavljena</span>
<span class="badge badge--warning">Na čekanju</span>
<span class="badge badge--danger">Arhivirana</span>
<span class="badge badge--info">Član kluba</span>
<span class="badge badge--gold">Otkriveno</span>
```

`badge--gold` je za titule i pečate (Games, Social).

### Iznos `.amount`

```html
<span class="amount">1.200,00 RSD</span>
<span class="amount amount--old">1.500,00 RSD</span>
<span class="amount amount--positive">+500,00 RSD</span>
<span class="amount amount--negative">−1.200,00 RSD</span>
```

Cifre su iste širine, pa se iznosi u koloni tabele poravnavaju. `--old` je precrtana cena
pored cene sa kuponom.

### Ostalo

`.error` (tekst greške), `.meta` (sporedan tekst), a iz `_layout.scss`: `.container`,
`.stack` (vertikalni razmak između dece), `.grid` (mreža kartica).

## Odluke

- **Tema prati naslovnu stranicu.** Prvi nacrt („Gradski putopis": svetla pozadina,
  petrolej i terakota) napušten je kada je naslovna stranica napravljena u tamnoj
  zeleno-narandžastoj paleti. Dve palete u istoj aplikaciji delovale bi kao dve aplikacije.
- **Tamna tema, bez svetle varijante.** Jedna paleta je jednostavnija za učenje i održavanje;
  prekidač svetla/tamna bi tražio drugi skup tokena i kod.
- **Tekst na narandžastoj je tamni.** Beli tekst na `#f28b43` ima kontrast oko 2:1, a tamni
  oko 7:1. Isto važi za senf.
- **Kartica je neutralna površina.** Boju nose sadržaj i dugmad, ne kartica.
- **Sistemski font, bez web fonta.** Naslovi se izdvajaju debljinom (800), kao na naslovnoj,
  a ne posebnim fontom. Nema nove zavisnosti ni zahteva ka spoljnom servisu.
- **Nazivi tokena se ne menjaju.** Primarna boja i dalje je `--color-accent`, jer je moduli
  već koriste; promenjene su samo vrednosti.
- **`--color-highlight` je uklonjen.** Imao je istu vrednost kao primarna boja i nije ga
  koristila nijedna klasa. Ako ikonice kasnije dobiju posebnu boju, token se vraća.
- **Sekundarno dugme je obrubljeno**, kao „Pogledaj blogove" na naslovnoj. Druga puna boja
  pored narandžaste bi se otimala za pažnju.

## Za timove modula

Postoje lokalni stilovi koje sada pokriva globalna klasa. Svaki tim ih menja sam, u svom
modulu:

| Modul | Lokalno | Globalna zamena |
|---|---|---|
| Payment (`wallet`) | `.balance` | `.amount` |
| Exploration (`my-tours`), Social (`my-clubs`, grana `social/vodjenje-klubova`) | `.actions` | `.form-actions` |
| Exploration (`tour-card`) | `.tags li` | `.badge` |
| Social (`blog-comments`) | stil `input` u komentarima | `.field` |

## Otvorena pitanja

- **Modal.** `<dialog>` zahteva referencu na element, što podskup ne dozvoljava. Do odluke:
  potvrda na samoj stranici ili posebna stranica.
- **Toast obaveštenja.** Zahtevaju komponentu u `shared/ui` i servis. Do odluke: `.alert`.
- **Formatiranje iznosa** („1.200,00 RSD"). Stil postoji (`.amount`), ali formatiranje je
  kod; Angular `currency` pipe nije na spisku podskupa.
- **Lokalni tokeni na naslovnoj.** `core/home/home.scss` na `:host` ponovo definiše
  `--color-bg`, `--color-surface`, `--color-text`, `--color-accent` i ostale. Naslovna treba
  da koristi globalne tokene; vrednosti su sada iste, pa se te linije brišu u posebnoj
  izmeni (ostaje samo `--alt`).

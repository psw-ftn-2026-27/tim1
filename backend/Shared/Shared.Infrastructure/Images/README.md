# Deljeni upload slika (serverski deo)

Ovaj direktorijum sadrži deljeni procesor slika (`ImageProcessor`) i deljeno skladište slika (`LocalImageStorage`). Svaki modul kome treba upload slike koristi ove dve klase, a sam piše samo svoja pravila: ko kači sliku, na šta i koliko slika. Kod je u vlasništvu platformskog tima.

Procesor i skladište ne znaju za šta je slika vezana. Zapis o slici (agregat, tabela u šemi modula, brisanje nevezanih slika) pripada modulu koji sliku koristi. Prvi korisnik je modul Social (`Social.Domain/Images/Image.cs`).

## Kako ih modul koristi

Aplikacioni sloj modula ne sme da referencira `Shared.Infrastructure`. Zato modul u svom `Application` projektu deklariše interfejse `IImageProcessor` i `IImageStorage`, a u svom `Infrastructure` projektu ih implementira malim adapterima nad ovim klasama (u modulu Social: `Social.Infrastructure/Images/`). Interfejs obrće strelu zavisnosti: aplikacioni sloj zavisi od sopstvenog interfejsa, a ne od deljene infrastrukture.

## Bezbednosne granice (iste za sve module, nijedan modul ih ne menja, P9)

| Provera | Gde i kada | Zašto postoji |
|---|---|---|
| Fajl veći od 5 MB se odbija (P2). | Dva puta. Host odbija prevelik zahtev pre nego što ga primi ceo (videti ispod), a procesor ponovo broji bajtove dok čita i prekida čitanje čim pređe granicu. | Veliki fajl troši memoriju, disk i vreme obrade. Procesor proverava i sam, jer je to njegova garancija nezavisno od toga kako ga endpoint poziva. |
| Prihvataju se samo JPEG, PNG i WebP, prepoznati po prvim bajtovima sadržaja (P3). | Pre dekodiranja. Ime fajla i tip koji šalje klijent se ne gledaju. | Ime i tip šalje klijent, pa mogu da lažu. SVG se odbija jer je to tekst (XML) koji može da sadrži skriptu, a drugi formati (BMP, GIF, ...) nisu potrebni i samo šire površinu za napad na dekoder. |
| Slika veća od 8000×8000 ili sa više od 40 miliona piksela se odbija (P4). | Iz zaglavlja slike, pre punog dekodiranja. | Mali komprimovan fajl može da se raspakuje u ogromnu sliku („dekompresiona bomba“). Dekodirana slika zauzima 4 bajta po pikselu, pa 40 miliona piksela znači oko 160 MB memorije. |
| Fajl koji ne može da se dekodira se odbija (P5). | Pri dekodiranju; i delimično dekodiran (odsečen) fajl se odbija. | Oštećen ili namerno izmenjen fajl ne sme da postane sačuvana slika. |
| Sačuvana slika je uvek ponovo kodirana kao WebP (P6). | Posle dekodiranja. Originalni bajtovi se nikad ne čuvaju. | Ono što se čuva i kasnije šalje drugim korisnicima su pikseli koje je napravio server, a ne bajtovi koje je poslao korisnik, pa skriveni sadržaj u originalnom fajlu ne preživljava. |
| Sačuvana slika nema metapodatke (EXIF, GPS), a orijentacija joj je zadržana (P7). | Pri ponovnom kodiranju. Orijentacija iz EXIF-a se pre toga primeni na same piksele. | Metapodaci otkrivaju lokaciju i uređaj korisnika. Pošto nova slika nema EXIF, orijentacija mora da se „upiše“ u piksele, inače bi se fotografija sa telefona prikazala okrenuta. |
| Ime pod kojim se slika čuva određuje sistem (P8). | `LocalImageStorage` prima samo čisto ime fajla (bez putanje), inače baca `ArgumentException`. Modul ime pravi iz identifikatora slike. | Ime koje pošalje korisnik nikad ne ulazi u putanju, pa korisnik ne može da upiše fajl van foldera slika (napad tipa `../../`) ni da pregazi tuđu sliku. |

Fajl koji ne prođe neku od ovih provera izaziva `DomainException`, koju host pretvara u odgovor 400 sa razlogom, bez putanja i tehničkih detalja (P14).

## Granice namene (bira ih modul, P10 i P11)

`Process(content, minSide, maxLongSide)` prima dve granice namene. Modul ih bira prema tome čemu slika služi. Vrednosti su poslovno pravilo modula i imenuju se u modulu. Modul koji ih ne navede dobija podrazumevane vrednosti (Social, P23).

| Granica | Podrazumevano | Okvir platforme | Ponašanje |
|---|---|---|---|
| `minSide`, najmanja dimenzija (P10) | 100 | najmanje 100 (`MinSideFloor`) | Slika kojoj je ijedna strana kraća se odbija, iz zaglavlja, pre punog dekodiranja. |
| `maxLongSide`, najveća duža strana sačuvane slike (P11) | 2048 | najviše 2048 (`MaxLongSideCeiling`) | Veća slika se srazmerno smanjuje i nikad se zbog toga ne odbija. Manja slika se ne uvećava. |

Obe vrednosti moraju da budu između 100 i 2048. Vrednost van tog okvira je greška u kodu modula, a ne greška korisnika: procesor baca `ArgumentOutOfRangeException`, host je beleži kao grešku 500, a korisnik nikad ne vidi njen tekst.

## Globalno ograničenje tela zahteva

`Host.Api/Program.cs` postavlja jedno ograničenje tela zahteva za celu aplikaciju: `ImageProcessor.MaxFileBytes` (5 MB) plus 4096 bajtova rezerve za multipart omotač (zaglavlja dela forme, granične linije). Jedini izvor broja 5 MB je konstanta `ImageProcessor.MaxFileBytes`.

Ograničenje se primenjuje na dva načina:
- Kestrel (`MaxRequestBodySize`) prekida čitanje bilo kog tela koje pređe granicu, i kada klijent ne pošalje dužinu unapred.
- Kratak middleware u `Program.cs` odgovara statusom 413 čim zahtev najavi (`Content-Length`) veće telo, pre nego što se telo čita. Pregledač uvek šalje dužinu za formu sa fajlom. Ovaj middleware poštuje i testni server, koji ne primenjuje Kestrel podešavanja.

Zato i endpoint koji „zaboravi“ atribut ipak ne prima veći fajl, pa P2 i P9 važe za svaki modul. Endpoint za upload zato nema atribut `[RequestSizeLimit]`.

Fajl koji je tek malo veći od 5 MB (manje od rezerve od 4096 bajtova) prođe ograničenje tela, ali ga procesor odbije sa 400. To je posledica rezerve za omotač, a ne propust.

**Posledica: ograničenje važi za sve zahteve u aplikaciji.** Budući endpoint kome treba veće telo (na primer upload GPX ili PDF fajla) mora izričito da podigne ograničenje na svom endpointu, atributom `[RequestSizeLimit(...)]` na toj akciji. I Kestrel i middleware poštuju vrednost sa endpointa. Globalno ograničenje se ne podiže zbog jednog endpointa.

## Skladište

`LocalImageStorage` čuva fajlove na lokalnom disku, u jednom podfolderu po modulu (`<koren>/social/...`). Koren se čita iz konfiguracionog ključa `Images:RootFolder`. Kada ključ nije postavljen, koren je `Explorer/images` u lokalnom folderu aplikacionih podataka korisnika (na Linuxu `~/.local/share/Explorer/images`). Brisanje fajla koji je već obrisan ne izaziva grešku, pa čišćenje može bezbedno da se ponovi.

# Exploration: vizuelne reference

Ove datoteke su izvor istine za izgled Exploration ekrana.

- `png/`: snimak svakog ekrana u širini 1440 px. Fajlovi `*--mobile.png` su snimci istog ekrana u širini 390 px.
- `screens/`: isti ekrani kao statički HTML, koji se otvara direktno u browseru. Za `Tours` i `MyPreferences` postoje samo PNG skice.
- `exploration-reference.css`: sve vrednosti iz referenci (boje, razmaci, radijusi, veličine fonta).

Reference su statičke i ne koriste Angular. Njihove klase su samo približne. U kodu se koriste klase iz `styles/README.md`, a hex vrednosti se prevode u tokene iz `_tokens.scss`. Mapa i slike u referencama su crteži koji samo zauzimaju mesto. U aplikaciji se koriste prava mapa iz `shared/ui` i prave slike.

| Ruta | Referenca | Napomena |
|---|---|---|
| `/exploration` | `Tours` | postojeća skica; prazno stanje je u `ExplorationStates` |
| ruta za preferencije | `MyPreferences` | postojeća skica |
| `/exploration/mine` | `MyTours` | uključuje otvoren odeljak za objavu sa greškom cene |
| `/exploration/create` | `CreateTour` | |
| `/exploration/tour/:id/manage` | `ManageTour` | |
| `/exploration/tour/:id/add-keypoint` | `AddKeypoint` | |
| `/exploration/tour/:id/keypoint/:keypointId/edit`, pregled | `KeypointView` | |
| `/exploration/tour/:id/keypoint/:keypointId/edit`, forma | `KeypointEdit` | |
| prazna stanja, greške, odeljci u redu tabele | `ExplorationStates` | |
| detalji ture pre kupovine | `TourPreview` | planirano |
| detalji ture za autora | `TourAuthor` | planirano |
| upravljanje objavljenom turom koja ima kupce | `ManagePublished` | planirano |

# Games: vizuelne reference

Ove datoteke su izvor istine za izgled Games ekrana.

- `png/`: snimak svakog ekrana u širini 1440 px. Fajlovi `*--mobile.png` su snimci u širini 390 px.
- `screens/`: isti ekrani kao statički HTML, koji se otvara direktno u browseru.
- `games-reference.css`: sve vrednosti iz referenci (boje, razmaci, radijusi, veličine fonta).

Reference su statičke i ne koriste Angular. Njihove klase su samo približno iste kao u aplikaciji, pa se u kodu koriste klase iz `styles/README.md`, a hex vrednosti se prevode u tokene iz `_tokens.scss`.

| Ruta | Referenca |
|---|---|
| `/games` (administrator) | `Main` |
| `/games` (explorer) | `ExplorerGames` |
| `/games/challenges` | `AllChallenges` |
| `/games/social-challenges` | `SocialChallenges` |
| `/games/social-challenges/new` | `SocialChallengeNew` |
| `/games/social-challenges/:id/edit`, izazov koji nije draft | `SocialChallengePublished` |
| `/games/quizzes` | `Quizzes` |
| `/games/quizzes/new` | `QuizNew` |
| `/games/quizzes/:id/edit`, pregled | `QuizDetails` |
| `/games/quizzes/:id/edit`, izmena | `QuizEdit` |
| `/games/play/quizzes` | `PlayQuizzes` |
| `/games/play/quizzes/:id`, pre aktivacije | `PlayActivate` |
| `/games/play/quizzes/:id`, pitanje | `PlayQuestion` |
| `/games/play/quizzes/:id`, posle odgovora | `PlayAnswered` |
| `/games/play/quizzes/:id`, kviz završen | `PlayResult` |
| prazna stanja, greške, „više nije dostupno“ | `States` |

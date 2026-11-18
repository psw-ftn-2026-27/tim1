using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftCityWalk = new(
        "City walk", "Read the signs.", 0,
        [new QuizQuestion("Which sign?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)]),
         new QuizQuestion("Which street?", [])]);

    public static readonly Quiz DraftMuseum;
    public static readonly Quiz PublishedSquare;
    public static readonly Quiz RetiredPark;
    public static readonly Quiz ArchivedBridge;
    public static readonly Quiz PublishedLandmarkWalk;
    public static readonly Quiz PublishedFountainTrail;
    public static readonly Quiz PublishedMonumentTrail;

    public static Quiz[] All =>
    [
        DraftCityWalk, DraftMuseum, PublishedSquare, RetiredPark, ArchivedBridge,
        PublishedLandmarkWalk, PublishedFountainTrail, PublishedMonumentTrail
    ];

    static QuizSeed()
    {
        DraftMuseum = new Quiz("The museum", "Read the gallery signs.", 10,
            [new QuizQuestion("Which gallery?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)])]);
        PublishedSquare = new Quiz("The square", "Visit the square.", 10,
            [new QuizQuestion("Which monument?", [new QuizAnswer("First", false), new QuizAnswer("Second", true)])]);
        PublishedSquare.Publish();
        RetiredPark = new Quiz("The park", "Read the plaque.", 10,
            [new QuizQuestion("Which tree?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)])]);
        RetiredPark.Publish();
        RetiredPark.Retire();
        ArchivedBridge = new Quiz("The bridge", "Walk across the bridge.", 10,
            [new QuizQuestion("Which river?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)])]);
        ArchivedBridge.Publish();
        ArchivedBridge.Retire();
        ArchivedBridge.Archive();
        PublishedLandmarkWalk = new Quiz("The landmark walk", "Find the landmark.", 100,
            [new QuizQuestion("Which landmark?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)]),
             new QuizQuestion("Which year?", [new QuizAnswer("First", false), new QuizAnswer("Second", true)])],
            45.2517, 19.8369);
        PublishedLandmarkWalk.Publish();
        PublishedFountainTrail = new Quiz("The fountain trail", "Follow the fountains.", 25,
            [new QuizQuestion("Which fountain?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)]),
             new QuizQuestion("Which statue?", [new QuizAnswer("First", false), new QuizAnswer("Second", true)])]);
        PublishedFountainTrail.Publish();
        PublishedMonumentTrail = new Quiz("The monument trail", "Visit the monuments.", 15,
            [new QuizQuestion("Which monument?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)]),
             new QuizQuestion("Which sculptor?", [new QuizAnswer("First", false), new QuizAnswer("Second", true)])]);
        PublishedMonumentTrail.Publish();
    }
}

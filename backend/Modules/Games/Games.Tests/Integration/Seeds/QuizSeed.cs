using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftCityWalk = new(
        "City walk", "Read the signs.", 0,
        [new QuizQuestion("Which sign?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)]),
         new QuizQuestion("Which street?", [])]);

    public static readonly Quiz PublishedSquare = CreatePublishedSquare();

    public static Quiz[] All => [DraftCityWalk, PublishedSquare];

    private static Quiz CreatePublishedSquare()
    {
        var quiz = new Quiz("The square", "Visit the square.", 10,
            [new QuizQuestion("Which monument?", [new QuizAnswer("First", false), new QuizAnswer("Second", true)])]);
        quiz.Publish();
        return quiz;
    }
}

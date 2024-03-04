namespace Poemem.Quiz
{
	delegate string LineSubstitution(string s);

    record QuizOptions(IPoem Poem, Range VerseRange, Difficulty Difficulty)
    {
        public LineSubstitution Substitution { get; init; } = it => it;
    }

    interface IQuizResult
    {
        string ToString();
    }

    record QuizResult(string message) : IQuizResult
    {
        public override string ToString() => message;
    }

    record ScoreResult(int Score, int Total) : IQuizResult
    {
        public override string ToString()
        {
            var rate = (double)Score / Total;
            return $"You got {Score}/{Total} correct, thats {rate * 100:N0}%";
        }
    }

    interface IPoemQuiz
    {
        IQuizResult? Execute(QuizOptions options);
    }
}

using System.Text.RegularExpressions;
using Poemem.Common;

namespace Poemem.Quiz
{
    class BlankQuiz : IPoemQuiz
	{
		public IQuizResult? Execute(QuizOptions options)
		{
			var score = 0;
			int total = 0;

			foreach (var verse in options.Poem.Verses)
			{
				foreach (var line in verse)
				{
					var s = options.Substitution(line);
					var blanks = Line.Current.WriteBlanks(s, it => SelectBlanks(it, options.Difficulty));
					var results = Line.Current.QuizBlanks(blanks);
					if (results is null)
					{
						Line.Current.NewLine();
						return null;
					}
					score += results.Score;
					total += results.Total;
				}

				Line.Current.NewLine();
			}

			return new ScoreResult(score, total);
		}

		static IEnumerable<Match> SelectBlanks(string line, Difficulty difficulty)
		{
			var words = Regex.Matches(line, @"\p{L}+");

			if (difficulty == Difficulty.Extreme)
				return words.SkipAtRandom();

			return words
				.Where(it => it.Value.Length > 3)
				.SelectAtRandom(difficulty switch
				{
					Difficulty.Easy => 1,
					Difficulty.Medium => 2,
					Difficulty.Hard => 4,
					_ => throw new ArgumentOutOfRangeException()
				});
		}
	}
}

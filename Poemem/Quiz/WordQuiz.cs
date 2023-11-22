using System.Text.RegularExpressions;

namespace Poemem.Quiz
{
	class WordQuiz : IPoemQuiz
	{
		public IQuizResult? Execute(QuizOptions options)
		{
			var score = 0;
			int total = 0;
			var verses = options.Verses;

			foreach (var verse in verses)
			{
				foreach (var line in verse)
				{
					var s = options.Substitution(line);
					var blanks = Line.Current.WriteBlanks(s, it => SelectWords(it, options.difficulty));
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

		static IEnumerable<Match> SelectWords(string line, Difficulty difficulty)
		{
			var matches = Regex.Matches(line, @"\p{L}+");

			if (difficulty == Difficulty.Extreme)
				return matches.SkipAtRandom();

			return matches
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

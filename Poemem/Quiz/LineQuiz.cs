using System.Text.RegularExpressions;

namespace Poemem.Quiz
{
	class LineQuiz : IPoemQuiz
	{
		public IQuizResult? Execute(QuizOptions options)
		{
			var score = 0;
			var total = 0;

			foreach (var verse in options.Verses)
			{
				for (int l = 0; l < verse.Length; l++)
				{
					var line = options.Substitution(verse[l]);
					if (l % 2 == 0)
					{
						Line.Current.Write(line).NewLine();
					}
					else
					{
						var blanks = Line.Current.WriteBlanks(verse[l],
							 it => Regex.Matches(it, @"\p{L}+"));

						var results = Line.Current.QuizBlanks(blanks);
						if (results is null)
						{
							Line.Current.NewLine();
							return null;
						}
						score += results.Score;
						total += results.Total;
					}
				}
				Line.Current.NewLine();
			}

			return new ScoreResult(score, total);
		}
	}
}

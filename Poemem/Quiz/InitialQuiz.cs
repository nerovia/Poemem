using Poemem.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Poemem.Quiz
{
	internal class InitialQuiz : IPoemQuiz
	{
		public IQuizResult? Execute(QuizOptions options)
		{
			var score = 0;
			var total = 0;

			foreach (var verse in options.Poem.Verses)
			{
				foreach (var line in verse)
				{
					var s = options.Substitution(line);
					var blanks = Line.Current.WriteBlanks(s, line => SelectBlanks(line, options.Difficulty));
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

		public IEnumerable<Match> SelectBlanks(string line, Difficulty difficulty)
		{
			var words = Regex.Matches(line, @"\p{L}+");

			return difficulty switch
			{
				Difficulty.Easy => words.Skip(2),
				Difficulty.Medium => words.Skip(1),
				Difficulty.Hard => words.Skip(1),
				Difficulty.Extreme => words.Skip(1).Prepend(Regex.Match(words.First().Value, @"(?<=^.).+")),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}

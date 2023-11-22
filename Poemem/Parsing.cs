using System.CommandLine.Parsing;
using System.Text.RegularExpressions;

namespace Poemem
{
	internal static class Parsing
	{
		public static Range ParseRange(ArgumentResult result)
		{
			Index parse(string s, Index fallback) => string.IsNullOrEmpty(s) ? fallback : int.Parse(s);

			var s = result.Tokens.Single().Value;
			var match = Regex.Match(s, @"(\d*)\.{2}(\d*)");
			if (match.Success)
				return new Range(parse(match.Groups[1].Value, Index.Start), parse(match.Groups[2].Value, Index.End));
			var i = int.Parse(s);
			return new Range(i, i);
		}
	}
}

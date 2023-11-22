using System.Text.Json.Serialization;

namespace Poemem
{
	interface IPoem 
	{
		string Title { get; }
		string[][] Verses { get; }
	}

	[Serializable]
	internal class Poem : IPoem
	{
		Lazy<string[][]> _lazyVerses;

		public Poem()
		{
			_lazyVerses = new(ExtractVerses);
		}

		[JsonPropertyName("author")]
		public required string Author { get; init; }

		[JsonPropertyName("title")]
		public required string Title { get; init; }

		[JsonPropertyName("lines")]
		public required string[] Lines { get; init; }

		public string[][] Verses { get => _lazyVerses.Value; }

		string[][] ExtractVerses()
		{
			// Maybe a bit wasteful, but it simplifies things...

			string[][] verses = new string[Lines.Count(string.IsNullOrEmpty) + 1][];
			
			int start = 0;
			for (int i = 0; i < verses.Length; i++)
			{
				int end = Array.FindIndex(Lines, start, string.IsNullOrEmpty);
				var length = end < 0 ? Lines.Length - start : end - start;
				verses[i] = new string[length];
				Array.Copy(Lines, start, verses[i], 0, length);
				start = end + 1;
			}

			return verses;			
		}

		[Obsolete($"Use Verses.Length instead")]
		public int VerseCount { get => Lines.Count(string.IsNullOrEmpty) + 1; }

		[Obsolete($"Use Verses instead")]
		public IEnumerable<string> EnumerateVerses(Range verseRange)
		{
			var (offset, length) = verseRange.GetOffsetAndLength(VerseCount);
			return Lines
				.SkipWhile(it => (string.IsNullOrEmpty(it) ? offset-- : offset) > 0)
				.TakeWhile(it => (string.IsNullOrEmpty(it) ? --length : length) >= 0);
		}

		[Obsolete($"User Verses instead")]
		public IEnumerable<IEnumerable<string>> EnumerateVerses()
		{
			var indicies = Lines.Select((line, i) => (line, i))
				.Where(it => string.IsNullOrEmpty(it.line))
				.Select(it => it.i);
			return indicies.Zip(indicies.Prepend(0))
				.Select(it => Lines[new Range(it.First, it.Second)]);
		}
	}
}

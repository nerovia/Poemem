using System.Diagnostics;
using System.Text;

namespace Poemem
{
	public class Line
	{
		public static Line Current { get; private set; } = new Line().Style(0);

		private Line() { Log("created"); }

		public void EnsureCurrent()
		{
			if (this != Current)
				throw new Exception("This line has expired");
		}

		internal void Log(string message)
		{
			Debug.WriteLine($"[{Offset:00}/{Length:00}] {message}");
		}

		/// <summary>
		/// The amount of stuff that has been written on the current Line
		/// </summary>
		public int Length { get; private set; }

		public int Offset { get; private set; }

		public Line Write(object obj) => Write(obj.ToString() ?? "");

		public Line Write(string s)
		{
			EnsureCurrent();
			if (s.Length == 0)
				return this;

			if (s.Any(char.IsControl))
				throw new ArgumentException();

			var left = Console.CursorLeft;

			Offset += s.Length;
			Length = int.Max(Length, Offset);
			Console.Write(s);
			Log($"written '{s}' [{s.Length}]");

			// some terminals don't move the cursor to the next line...
			if (left == Console.CursorLeft)
			{
				Console.CursorLeft = 0;
				Console.CursorTop++;
			}

			return this;
		}

		public Line Erase(int n, char c = ' ')
		{
			EnsureCurrent();
			var front = Offset == Length;
			n = int.Min(n, Offset);
			if (n > 0)
			{
				Console.CursorVisible = false;
				Move(-n);
				Write(new string(c, n));
				Move(-n);
				if (front)
					Length = Offset;
				Console.CursorVisible = true;
			}
			Log($"erased {n} with '{c}'");
			return this;
		}

		public Line MoveToHead() => Move(int.MaxValue);

		public Line MoveToTail() => Move(int.MinValue);

		/// <summary>
		/// Moves the cursor within the length of the line.
		/// </summary>
		/// <param name="n">the offset to move</param>
		/// <returns></returns>
		public Line Move(int n)
		{
			EnsureCurrent();

			n = int.Clamp(n, -Offset, Length - Offset);
			Offset += n;

			var offset = Console.CursorTop * Console.BufferWidth + Console.CursorLeft;
			(Console.CursorTop, Console.CursorLeft) = int.DivRem(offset + n, Console.BufferWidth);

			Log($"moved {n}");
			return this;
		}

		public Line NewLine(int n = 1)
		{
			EnsureCurrent();
			MoveToHead();
			if (n < 1) throw new ArgumentException();
			for (int i = n; i > 0; --i)
				Console.WriteLine();
			Log($"next line {n}");
			Current = new Line();
			return Current;
		}

		public Line Style(params int[] args)
		{
			Console.Write($"\x1b[{string.Join(';', args)}m");
			return this;
		}

		public string? Read(ReadOptions options)
		{
			var builder = new StringBuilder();
			while (true)
			{
				var key = Console.ReadKey(true);

				switch (key.Key)
				{
					case ConsoleKey.Escape:
						return null;

					case ConsoleKey.Tab:
					case ConsoleKey.Enter:
						if (builder.Length >= options.MinLength)
							return builder.ToString();
						break;

					case ConsoleKey.Backspace:
						if (builder.Length > 0)
						{
							if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
							{
								Erase(builder.Length, options.EraseChar);
								builder.Clear();
							}
							else
							{
								Erase(1, options.EraseChar);
								builder.Remove(builder.Length - 1, 1);
							}
						}
						break;

					default:
						if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
							continue;

						if (char.IsControl(key.KeyChar))
							continue;

						if (char.IsWhiteSpace(key.KeyChar) && !options.AllowSpace)
							continue;

						if (builder.Length < options.MaxLength)
						{
							Write(key.KeyChar);
							builder.Append(key.KeyChar);
						}

						if (builder.Length >= options.MaxLength)
						{
							if (options.AutoSubmit)
								return builder.ToString();
							//Console.CursorVisible = false;
						}
						break;
				}
			}
		}
	
		public Line Read(ReadOptions options, out string s)
		{
			s = Read(options);
			return this;
		}

		public Span Span(string s)
		{
			var span = new Span(this, Offset, s.Length);
			Write(s);
			return span;
		}

		public Line Span(string s, out Span span)
		{
			span = Span(s);
			return this;
		}

	}

	public class Span
	{
		public Line Line { get; }
		public int Offset { get; }
		public int Length { get; }
		
		public Span(Line line, int offset, int length) 
		{
			Line = line;
			Offset = offset;
			Length = length;
		}

		public Line Move()
		{
			if (Line.Offset != Offset)
				Line.Move(Offset - Line.Offset);
			return Line;
		}

		public Span Write(string s)
		{
			Move().Write(s.Length < Length ? s : s.Substring(0, Length));
			return this;
		}

		public Span Style(params int[] args)
		{
			Line.Style(args);
			return this;
		}

		public string? Read(ReadOptions options)
		{
			return Move().Read(options with { MaxLength = int.Min(Length, options.MaxLength) });
		}
	}

	public record class ReadOptions
	{
		public int MinLength { get; init; } = 0;
		public int MaxLength { get; init; } = int.MaxValue;
		public char EraseChar { get; init; } = ' ';
		public bool AutoSubmit { get; init; } = false;
		public bool AllowEscape { get; init; } = false;
		public bool AllowSpace { get; init; } = false;
	}

	[Obsolete]
	class StringStyle
	{
		SortedSet<int> Args = new();

		public const char EscapeChar = '\x1b';

		public StringStyle Mode(AnsiiMode mode)
		{
			Args.Add((int)mode);
			return this;
		}

		public StringStyle Bold() => Mode(AnsiiMode.Bold);

		public StringStyle Italic() => Mode(AnsiiMode.Italic);

		public StringStyle Underline() => Mode(AnsiiMode.Underline);

		public StringStyle Foreground(AnsiiColor color)
		{
			Args.Add((int)color + 30);
			return this;
		}

		public StringStyle Background(AnsiiColor color)
		{
			Args.Add((int)color + 40);
			return this;
		}

		public string Apply(string s)
		{
			return $"{EscapeChar}[{string.Join(';', Args)}m{s}{EscapeChar}[0m";

			return EscapeChar + '[' + string.Join(';', Args) + 'm' + s + EscapeChar + "[0m";
		}
	}

	public enum AnsiiMode
	{
		None,
		Bold = 1,
		Faint = 2,
		Italic = 3,
		Underline = 4,
		Blink = 5,
		Inverse = 6,
		Invisible = 7,
		Strikethrough = 8,
	}

	public enum AnsiiColor
	{
		Black = 0,
		Red = 1,
		Green = 2,
		Yellow = 3,
		Blue = 4,
		Magenta = 5,
		Cyan = 6,
		White = 7,
		Default = 9,
	}
}

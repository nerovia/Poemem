﻿using System.Text.RegularExpressions;
using Poemem.Quiz;

namespace Poemem.Common
{
    internal delegate IEnumerable<Match> BlankSelector(string s);

    internal record Blank(string Word, Span Span);

    internal static class Utility
    {
        internal static IEnumerable<Blank> WriteBlanks(this Line line, string text, BlankSelector blankSelector)
        {
            var words = blankSelector(text);

            int i = 0;
            var blanks = new List<Blank>();
            foreach (var word in words)
            {
                line.Write(text.Substring(i, word.Index - i));
                var blank = Line.Current.Span(new string('_', word.Value.Length));
                blanks.Add(new(word.Value, blank));
                i = word.Index + word.Length;
            }
            line.Write(text.Substring(i));

            return blanks;
        }

        internal static ScoreResult? QuizBlanks(this Line line, IEnumerable<Blank> blanks)
        {
            int total = 0;
            int nbrCorrect = 0;
            foreach (var blank in blanks)
            {
                ++total;
                var answer = blank.Span.Read(new ReadOptions() { AutoSubmit = true, EraseChar = '_', AllowEscape = true });
                if (answer == null)
                {
                    line.NewLine();
                    return null;
                }

                if (string.Compare(answer, blank.Word, true) == 0)
                {
                    ++nbrCorrect;
                    blank.Span.Write(answer, AnsiiStyle.Foreground(AnsiiColor.Green));
                }
                else
                {
                    blank.Span.Write(blank.Word, AnsiiStyle.Italic + AnsiiStyle.Foreground(AnsiiColor.Red));
                }
            }

            line.NewLine();
            return new ScoreResult(nbrCorrect, total);
        }

        internal static Line WriteTitle(this Line line, string title, int totalLength)
        {
            title = ' ' + title + ' ';
            line.Write(title
                .PadLeft((totalLength - title.Length) / 2, '=')
                .PadRight(totalLength, '='));
            return line;
        }

        internal static IEnumerable<T> SelectAtRandom<T>(this IEnumerable<T> that, int count)
        {
            var list = that.ToList();
            count = int.Min(count, list.Count);
            var size = (list.Count + count - 1) / count;
            for (int i = 0; i < list.Count; i += size)
                yield return list.ElementAt(Random.Shared.Next(i, int.Min(i + size, list.Count)));
        }

        internal static IEnumerable<T> SkipAtRandom<T>(this IEnumerable<T> that)
        {
            var i = Random.Shared.Next(0, that.Count());
            return that.Select((it, i) => (it, i)).Where(it => it.i != i).Select(it => it.it);
        }

        internal static string ToTitleCase(this string s)
        {
            s = Regex.Replace(s, @"[\p{Lu}-]", @" $&");
            s = Regex.Replace(s, @"\s+", @" ");
            return s.Trim();
        }
    }
}

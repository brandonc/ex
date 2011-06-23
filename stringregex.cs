using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
#if DOTNET4
using System.Collections.Concurrent;
#endif

namespace System
{
    public class MatchData 
    {
        private List<string> indexcaptures = new List<string>();
        private Dictionary<string, string> namedcaptures = null;

        public string this[int index]
        {
            get
            {
                return indexcaptures[index];
            }
        }

        public string this[string name]
        {
            get
            {
                if (namedcaptures == null)
                    return null;

                return namedcaptures[name];
            }
        }

        private void AddMatch(Regex regex, Match match)
        {
            for (int index = 0; index < match.Groups.Count; index++)
            {
                Group group = match.Groups[index];
                string name = regex.GroupNameFromNumber(index);
                int tryint;

                if (Int32.TryParse(name, out tryint))
                    this.indexcaptures.Add(group.Value);
                else
                {
                    if (namedcaptures == null)
                        namedcaptures = new Dictionary<string, string>();

                    if (namedcaptures.ContainsKey(name))
                        this.namedcaptures[name] = group.Value;
                    else
                        this.namedcaptures.Add(name, group.Value);
                }
            }
        }

        public MatchData(Regex regex, Match match)
        {
            AddMatch(regex, match);
        }

        public MatchData(Regex regex, MatchCollection matches)
        {
            if (matches == null || matches.Count == 0)
                return;

            foreach (Match match in matches)
            {
                AddMatch(regex, match);
            }
        }

        public int Count
        {
            get { return indexcaptures.Count + (namedcaptures == null ? 0 : namedcaptures.Count); }
        }
    }

    public static class StringRegex
    {
        static readonly Dictionary<char, RegexOptions> _optionChars = new Dictionary<char, RegexOptions> {
            { 'i', RegexOptions.IgnoreCase },
            { 's', RegexOptions.Singleline },
            { 'x', RegexOptions.IgnorePatternWhitespace },
            { 'c', RegexOptions.Compiled },
            { 'e', RegexOptions.ExplicitCapture },
            { 'r', RegexOptions.RightToLeft }
        };

#if DOTNET4
        static readonly Dictionary<string, Regex> _cache = new Dictionary<string, Regex>(16);
#endif

        public static bool IsMatch(this string input, string pattern)
        {
            return pattern.ToRegex().IsMatch(input);
        }

        /// <summary>
        /// Tests whether an input string matches a string regex pattern with optional regex options.
        /// </summary>
        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static bool IsMatch(this string input, string pattern, string options)
        {
            return pattern.ToRegex(GetOptions(options)).IsMatch(input);
        }

        /// <summary>
        /// Tests whether an input string matches a string regex pattern
        /// </summary>
        public static bool IsMatch(this string input, string pattern, int startat)
        {
            return pattern.ToRegex().IsMatch(input, startat);
        }

        /// <summary>
        /// Tests whether an input string matches a string regex pattern
        /// </summary>
        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static bool IsMatch(this string input, string pattern, int startat, string options)
        {
            return pattern.ToRegex(GetOptions(options)).IsMatch(input, startat);
        }

        public static Dictionary<string, int> NamedCaptures(this string pattern)
        {
            var re = pattern.ToRegex();
            var result = new Dictionary<string,int>();
            Array.ForEach<string>(re.GetGroupNames(), x => result.Add(x, re.GroupNumberFromName(x)));

            return result;
        }

        public static MatchData Matches(this string input, string pattern)
        {
            var re = pattern.ToRegex();
            return new MatchData(re, re.Matches(input));
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static MatchData Matches(this string input, string pattern, string options)
        {
            var re = pattern.ToRegex(GetOptions(options));
            return new MatchData(re, re.Matches(input));
        }

        public static MatchData Matches(this string input, string pattern, int startat)
        {
            var re = pattern.ToRegex();
            return new MatchData(re, re.Matches(input, startat));
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static MatchData Matches(this string input, string pattern, int startat, string options)
        {
            var re = pattern.ToRegex(GetOptions(options));
            return new MatchData(re, re.Matches(input, startat));
        }

        public static MatchData Match(this string input, string pattern)
        {
            var re = pattern.ToRegex();
            return new MatchData(re, re.Match(input));
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static MatchData Match(this string input, string pattern, string options)
        {
            var re = pattern.ToRegex(GetOptions(options));
            return new MatchData(re, re.Match(input));
        }

        public static MatchData Match(this string input, string pattern, int startat)
        {
            var re = pattern.ToRegex();
            return new MatchData(re, re.Match(input, startat));
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static MatchData Match(this string input, string pattern, int startat, string options)
        {
            var re = pattern.ToRegex(GetOptions(options));
            return new MatchData(re, re.Match(input, startat));
        }

        public static MatchData Match(this string input, string pattern, int beginning, int length)
        {
            var re = pattern.ToRegex();
            return new MatchData(re, re.Match(input, beginning, length));
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static MatchData Match(this string input, string pattern, int beginning, int length, string options)
        {
            var re = pattern.ToRegex(GetOptions(options));
            return new MatchData(re, re.Match(input, beginning, length));
        }

        public static string Sub(this string input, string pattern, string replacement)
        {
            return pattern.ToRegex().Replace(input, replacement, 1);
        }

        public static string Sub(this string input, string pattern, MatchEvaluator evaluator)
        {
            return pattern.ToRegex().Replace(input, evaluator);
        }

        public static string Sub(this string input, string pattern, MatchEvaluator evaluator, int startat)
        {
            return pattern.ToRegex().Replace(input, evaluator, 1, startat);
        }

        public static string GSub(this string input, string pattern, string replacement)
        {
            return pattern.ToRegex().Replace(input, replacement);
        }

        public static string Sub(this string input, string pattern, string replacement, int startat)
        {
            return pattern.ToRegex().Replace(input, replacement, 1, startat);
        }

        public static string GSub(this string input, string pattern, MatchEvaluator evaluator, int startat)
        {
            return pattern.ToRegex().Replace(input, evaluator, startat);
        }

        public static string GSub(this string input, string pattern, string replacement, int startat)
        {
            return pattern.ToRegex().Replace(input, replacement, startat);
        }

        private static Regex ToRegex(this string pattern)
        {
            return new Regex(pattern);
        }

        private static Regex ToRegex(this string pattern, RegexOptions options)
        {
            return new Regex(pattern, options);
        }

        private static RegexOptions GetOptions(string options)
        {
            return (RegexOptions)options.Select(c => (int)_optionChars[c]).Sum();
        }
    }
}

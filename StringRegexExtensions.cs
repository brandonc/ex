// Uncomment this line if you're compiling with Visual Studio 2008
// #define NET35

using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
#if !NET35
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
#endif

namespace System
{
    /// <summary>
    /// Simplified collection of regex captures.
    /// </summary>
    /// <example>
    /// var matches = "John Wilkes Booth".Matches(@"(?&lt;firstname&gt;\w+)\s(\w+)\s(?&lt;lastname&gt;\w+)");
    /// Assert.AreEqual("John Wilkes Booth", matches[0]);
    /// Assert.AreEqual("John", matches["firstname"]);
    /// Assert.AreEqual("Wilkes", matches[1]);
    /// Assert.AreEqual("Booth", matches["lastname"]);
    /// Assert.AreEqual(4, matches.Count);
    /// </example>
    /// <remarks>
    /// See http://stackoverflow.com/questions/2250335/differences-among-net-capture-group-match/2251774#2251774 for a good
    /// explanation of why MatchCollection is so complicated.
    /// </remarks>
    public class MatchData : IEnumerable<string>
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

                    this.namedcaptures[name] = group.Value;
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

        IEnumerator<string> GetEnumeratorInternal()
        {
            foreach (string capture in indexcaptures)
            {
                yield return capture;
            }

            if (namedcaptures != null)
            {
                foreach (KeyValuePair<string, string> capture in namedcaptures)
                {
                    yield return capture.Value;
                }
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }
    }

    public static class StringRegexExtensions
    {
        static readonly Dictionary<char, RegexOptions> _optionChars = new Dictionary<char, RegexOptions> {
            { 'i', RegexOptions.IgnoreCase },
            { 's', RegexOptions.Singleline },
            { 'x', RegexOptions.IgnorePatternWhitespace },
            { 'c', RegexOptions.Compiled },
            { 'e', RegexOptions.ExplicitCapture },
            { 'r', RegexOptions.RightToLeft }
        };

#if !NET35
        static readonly ConcurrentDictionary<string, Regex> _cache = new ConcurrentDictionary<string, Regex>();

        static Regex ToRegex(this string pattern)
        {
            Regex result;
            if (_cache.TryGetValue(pattern, out result))
                return result;
            else {
                result = new Regex(pattern);
                _cache.AddOrUpdate(pattern, result, (key, old) => {
                    return old;
                });
                return result;
            }
        }

        static Regex ToRegex(this string pattern, RegexOptions options)
        {
            string key = String.Format("{0}-{1}", pattern, GetOptionChars(options));

            Regex result;
            if (_cache.TryGetValue(key, out result))
            {
                return result;
            }
            else 
            {
                result = new Regex(pattern, options);
                _cache.AddOrUpdate(key, result, (patternpluskey, old) =>
                {
                    if (options != old.Options)
                        return new Regex(patternpluskey.Substring(0, patternpluskey.LastIndexOf('-')), options);

                    return old;
                });
                return result;
            }
        }

        public static int CacheCount
        {
            get
            {
                return _cache.Count;
            }
        }
#else
        static Regex ToRegex(this string pattern)
        {
            return new Regex(pattern);
        }

        static Regex ToRegex(this string pattern, RegexOptions options)
        {
            return new Regex(pattern, options);
        }
#endif

        static RegexOptions GetOptions(string options)
        {
            return (RegexOptions)options.Select(c => (int)_optionChars[c]).Sum();
        }

        static string GetOptionChars(RegexOptions options)
        {
            return String.Join("", _optionChars.Select<KeyValuePair<char, RegexOptions>, string>(pair =>
            {
                return (options & pair.Value) == pair.Value ? pair.Key.ToString() : "";
            }));
        }

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
    }
}

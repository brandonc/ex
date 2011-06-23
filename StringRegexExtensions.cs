// Uncomment the next line if you're compiling with Visual Studio 2008
// #define NET35

// Use .NET 4.0 to enable thread safe cacheing.

using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
#if !NET35
using System.Collections.Concurrent;
#endif

namespace System
{
    /// <summary>
    /// A simplified collection of regex captures.
    /// </summary>
    /// <example>
    /// var matches = "John Wilkes Booth".MatchesPattern(@"(?&lt;firstname&gt;\w+)\s(\w+)\s(?&lt;lastname&gt;\w+)");
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
        List<Capture> indexcaptures = new List<Capture>();
        Dictionary<string, Capture> namedcaptures = null;

        /// <summary>
        /// Gets a numbered capture value. The first index (0) is always the entire match.
        /// </summary>
        /// <param name="index">The index of the numbered capture</param>
        /// <returns>The value of the specified numbered capture</returns>
        public string this[int index]
        {
            get
            {
                try
                {
                    return indexcaptures[index].Value;
                } catch (ArgumentOutOfRangeException ex)
                {
                    throw new IndexOutOfRangeException(String.Format("The index {0} was out of range", index), ex);
                }
            }
        }

        /// <summary>
        /// Gets a named capture value.
        /// </summary>
        /// <param name="name">The name of the named capture</param>
        /// <returns>The value of the specified named capture</returns>
        public string this[string name]
        {
            get
            {
                Capture result = null;
                if (namedcaptures == null || !namedcaptures.TryGetValue(name, out result))
                    return null;

                return result.Value;
            }
        }

        /// <summary>
        /// Gets the position of the specifed numbered capture
        /// </summary>
        /// <param name="index">The index of the numbered capture</param>
        /// <returns>The position of the capture</returns>
        public int Begin(int index)
        {
            try
            {
                Capture cap = indexcaptures[index];
                return cap.Index;
            } catch (ArgumentOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException(String.Format("The index {0} was out of range", index), ex);
            }
        }

        /// <summary>
        /// Gets the position of the character immediately following the end of the specified numbered capture
        /// </summary>
        /// <param name="index">The index of the numbered capture</param>
        /// <returns>The position of the character immediately following the end of the specified numbered capture</returns>
        public int End(int index)
        {
            try
            {
                Capture cap = indexcaptures[index];
                return cap.Index + cap.Length;
            } catch (ArgumentOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException(String.Format("The index {0} was out of range", index), ex);
            }
        }

        /// <summary>
        /// Gets the position of the named capture
        /// </summary>
        /// <param name="name">The name of the named capture</param>
        /// <returns>The position of the capture</returns>
        public int Begin(string name)
        {
            Capture cap;
            if(namedcaptures == null || !namedcaptures.TryGetValue(name, out cap))
                return -1;
            
            return cap.Index;
        }

        /// <summary>
        /// Gets the position of the character immediately following the end of the specified named capture
        /// </summary>
        /// <param name="index">The index of the named capture</param>
        /// <returns>The position of the character immediately following the end of the specified named capture</returns>
        public int End(string name)
        {
            Capture cap;
            if (namedcaptures == null || !namedcaptures.TryGetValue(name, out cap))
                return -1;

            return cap.Index + cap.Length;
        }

        /// <summary>
        /// Gets the total number of captures
        /// </summary>
        public int Count
        {
            get { return indexcaptures.Count + (namedcaptures == null ? 0 : namedcaptures.Count); }
        }

        /// <summary>
        /// Retrieves an array of capture names from the matches.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public string[] GetNames()
        {
            if(namedcaptures == null)
                return new string[0];

            return this.namedcaptures.Keys.ToArray();
        }

        void AddMatch(Regex regex, Match match)
        {
            for (int index = 0; index < match.Groups.Count; index++)
            {
                Group group = match.Groups[index];
                string name = regex.GroupNameFromNumber(index);
                int tryint;

                // We only record the LAST capture in this group. This simplifies matching so
                // that multiple captures in the same group are overwritten.
                if (Int32.TryParse(name, out tryint))
                {
                    this.indexcaptures.Add(group.Captures[group.Captures.Count - 1]);
                }
                else
                {
                    if (namedcaptures == null)
                        namedcaptures = new Dictionary<string, Capture>();

                    this.namedcaptures[name] = group.Captures[group.Captures.Count - 1];
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

        public IEnumerable<Capture> GetCaptures()
        {
            // First return numbered capture values, then 
            foreach (Capture capture in indexcaptures)
            {
                yield return capture;
            }

            if (namedcaptures != null)
            {
                foreach (KeyValuePair<string, Capture> capture in namedcaptures)
                {
                    yield return capture.Value;
                }
            }
        }

        IEnumerator<string> GetEnumeratorInternal()
        {
            foreach (Capture cap in GetCaptures())
            {
                yield return cap.Value;
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
            { 'r', RegexOptions.RightToLeft }
        };

#if !NET35
        static readonly ConcurrentDictionary<Tuple<string, RegexOptions>, Regex> _cache = new ConcurrentDictionary<Tuple<string, RegexOptions>, Regex>();

        static Tuple<string, RegexOptions> MakeCacheKey(string pattern, RegexOptions opt)
        {
            return new Tuple<string, RegexOptions>(pattern, opt);
        }

        static Tuple<string, RegexOptions> MakeCacheKey(string pattern)
        {
            return new Tuple<string, RegexOptions>(pattern, RegexOptions.None);
        }

        static Regex ToRegex(this string pattern)
        {
            return _cache.GetOrAdd(MakeCacheKey(pattern), p =>
            {
                return new Regex(pattern);
            });
        }

        static Regex ToRegex(this string pattern, RegexOptions options)
        {
            return _cache.GetOrAdd(MakeCacheKey(pattern, options), p =>
            {
                return new Regex(pattern, options);
            });
        }

        /// <summary>
        /// The number of Regex objects that occupy the cache.
        /// </summary>
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
            return (RegexOptions)options.Select(c => {
                try { return (int)_optionChars[c]; } catch (KeyNotFoundException) { return 0; }
            }).Sum();
        }

        static string GetOptionChars(RegexOptions options)
        {
            return String.Join("", _optionChars.Select<KeyValuePair<char, RegexOptions>, string>(pair =>
            {
                return (options & pair.Value) == pair.Value ? pair.Key.ToString() : "";
            }));
        }

        /// <summary>
        /// Tests whether this string matches a string regex pattern
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool IsPatternMatch(this string input, string pattern)
        {
            return pattern.ToRegex().IsMatch(input);
        }

        /// <summary>
        /// Tests whether this string matches a string regex pattern with optional regex options.
        /// </summary>
        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, r: right to left</param>
        public static bool IsPatternMatch(this string input, string pattern, string options)
        {
            return pattern.ToRegex(GetOptions(options)).IsMatch(input);
        }

        /// <summary>
        /// Tests whether this string matches a string regex pattern
        /// </summary>
        public static bool IsPatternMatch(this string input, string pattern, int startat)
        {
            return pattern.ToRegex().IsMatch(input, startat);
        }

        /// <summary>
        /// Tests whether this string matches a string regex pattern
        /// </summary>
        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, r: right to left</param>
        public static bool IsPatternMatch(this string input, string pattern, int startat, string options)
        {
            return pattern.ToRegex(GetOptions(options)).IsMatch(input, startat);
        }

        /// <summary>
        /// Returns matches within the string that match a specified regex pattern
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <returns>The <see cref="MatchData"/> associated with the pattern match.</returns>
        public static MatchData MatchesPattern(this string input, string pattern)
        {
            var re = pattern.ToRegex();
            return new MatchData(re, re.Matches(input));
        }

        /// <summary>
        /// Returns matches within the string that match a specified regex pattern with the specified regex options
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, r: right to left</param>
        /// <returns>The <see cref="MatchData"/> associated with the pattern match.</returns>
        public static MatchData MatchesPattern(this string input, string pattern, string options)
        {
            var re = pattern.ToRegex(GetOptions(options));
            return new MatchData(re, re.Matches(input));
        }

        /// <summary>
        /// Returns matches within the string that match a specified regex pattern beginning at the specified offset
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <param name="startat">The offset at which to begin matching</param>
        /// <returns>The <see cref="MatchData"/> associated with the pattern match.</returns>
        public static MatchData MatchesPattern(this string input, string pattern, int startat)
        {
            var re = pattern.ToRegex();
            return new MatchData(re, re.Matches(input, startat));
        }

        /// <summary>
        /// Returns matches within the string that match a specified regex pattern beginning at the specified offset with the specified regex options
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <param name="startat">The offset at which to begin matching</param>
        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, r: right to left</param>
        /// <returns>The <see cref="MatchData"/> associated with the pattern match.</returns>
        public static MatchData MatchesPattern(this string input, string pattern, int startat, string options)
        {
            var re = pattern.ToRegex(GetOptions(options));
            return new MatchData(re, re.Matches(input, startat));
        }

        /// <summary>
        /// Returns a copy of this string with the first occurrence of the specified regex pattern replaced with the specified replacement text
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <param name="replacement">The text replacement to use</param>
        /// <returns>A copy of this string with specified pattern replaced</returns>
        public static string Sub(this string input, string pattern, string replacement)
        {
            return pattern.ToRegex().Replace(input, replacement, 1);
        }

        /// <summary>
        /// Returns a copy of this string with the first occurrence of the specified regex pattern replaced with the specified replacement text
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <param name="replacement">The text replacement to use</param>
        /// <returns>A copy of this string with specified pattern replaced</returns>
        public static string Sub(this string input, string pattern, Func<Match, string> evaluator)
        {
            return pattern.ToRegex().Replace(input, delegate(Match arg) { return evaluator(arg); }, 1);
        }

        /// <summary>
        /// Returns a copy of this string with all occurrences of the specified regex pattern replaced with the specified replacement text
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <param name="replacement">The text replacement to use</param>
        /// <returns>A copy of this string with specified pattern replaced</returns>
        public static string GSub(this string input, string pattern, string replacement)
        {
            return pattern.ToRegex().Replace(input, replacement);
        }

        /// <summary>
        /// Returns a copy of this string with all occurrences of the specified regex pattern replaced with the text returned from the give function
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <param name="evaluator">A function that returns either the replacement text or the original string</param>
        /// <returns>A copy of this string with specified pattern replaced</returns>
        public static string GSub(this string input, string pattern, Func<Match, string> evaluator)
        {
            return pattern.ToRegex().Replace(input, delegate(Match arg) { return evaluator(arg); });
        }
    }
}

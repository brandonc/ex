using System.Text.RegularExpressions;
namespace System
{
    public static class StringRegex
    {
        public static bool IsMatch(this string pattern, string input)
        {
            return pattern.ToRegex().IsMatch(input);
        }

        /// <summary>
        /// Tests whether an input string matches a string regex pattern with optional regex options.
        /// </summary>
        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static bool IsMatch(this string pattern, string input, string options)
        {
            return pattern.ToRegex(GetOptions(options)).IsMatch(input);
        }

        /// <summary>
        /// Tests whether an input string matches a string regex pattern
        /// </summary>
        public static bool IsMatch(this string pattern, string input, int startat)
        {
            return pattern.ToRegex().IsMatch(input, startat);
        }

        /// <summary>
        /// Tests whether an input string matches a string regex pattern
        /// </summary>
        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static bool IsMatch(this string pattern, string input, int startat, string options)
        {
            return pattern.ToRegex(GetOptions(options)).IsMatch(input, startat);
        }

        public static MatchCollection Matches(this string pattern, string input)
        {
            return pattern.ToRegex().Matches(input);
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static MatchCollection Matches(this string pattern, string input, string options)
        {
            return pattern.ToRegex(GetOptions(options)).Matches(input);
        }

        public static MatchCollection Matches(this string pattern, string input, int startat)
        {
            return pattern.ToRegex().Matches(input, startat);
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static MatchCollection Matches(this string pattern, string input, int startat, string options)
        {
            return pattern.ToRegex(GetOptions(options)).Matches(input, startat);
        }

        public static Match Match(this string pattern, string input)
        {
            return pattern.ToRegex().Match(input);
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static Match Match(this string pattern, string input, string options)
        {
            return pattern.ToRegex(GetOptions(options)).Match(input);
        }

        public static Match Match(this string pattern, string input, int startat)
        {
            return pattern.ToRegex().Match(input, startat);
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static Match Match(this string pattern, string input, int startat, string options)
        {
            return pattern.ToRegex(GetOptions(options)).Match(input, startat);
        }

        public static Match Match(this string pattern, string input, int beginning, int length)
        {
            return pattern.ToRegex().Match(input, beginning, length);
        }

        /// <param name="options">Combine any characters -- i: ignore case, s: single line mode (period [.] matches newlines), x: ignore whitespace, c: compiled, e: explicit capture only, r: right to left</param>
        public static Match Match(this string pattern, string input, int beginning, int length, string options)
        {
            return pattern.ToRegex(GetOptions(options)).Match(input, beginning, length);
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

        public static Regex CompileRegex(this string pattern)
        {
            return new Regex(pattern, RegexOptions.Compiled);
        }

        private static RegexOptions GetOptions(string options)
        {
            var opts = RegexOptions.None;
            opts |= options.IndexOf('i') >= 0 ? RegexOptions.IgnoreCase : RegexOptions.None;
            opts |= options.IndexOf('s') >= 0 ? RegexOptions.Singleline : RegexOptions.None;
            opts |= options.IndexOf('x') >= 0 ? RegexOptions.IgnorePatternWhitespace : RegexOptions.None;
            opts |= options.IndexOf('c') >= 0 ? RegexOptions.Compiled : RegexOptions.None;
            opts |= options.IndexOf('e') >= 0 ? RegexOptions.ExplicitCapture : RegexOptions.None;
            opts |= options.IndexOf('r') >= 0 ? RegexOptions.RightToLeft : RegexOptions.None;

            return opts;
        }

        static StringRegex()
        {
        }
    }
}
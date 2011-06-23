# ex #

> A drop-in c# extension class that greatly simplifies framework regular expressions.

# What It Does #

1. strings have been suped up with Regex-powered methods `IsPatternMatch`, `MatchesPattern`, `Sub`, and `GSub`
2. The MatchesPattern method returns a new class `MatchData` that simplifies access to captures. It's a little like ruby's class of the same name. See [this answer][1] on stackoverflow for a better explanation of why the default implementation of MatchCollection is so complicated.
3. Any `RegexOptions` can be expressed as a character string
4. When using .NET 4, regex objects are cached

# What It Does Not Do #

1. This is not a `System.Text.RegularExpressions` replacement. It just greases the wheels a little.
2. Compile on framework versions below 3.5

# Examples #

### Test a match ###

    "Adam & Steve".IsPatternMatch(
        @"Adam (&|and) (?<someone_else>\w+)"
    ); // Returns true

### Test a match with case insensitivity option: ###

    "ADAM AND STEVE".IsPatternMatch(
        @"adam (&|and) (?<someone_else>\w+)",
        "i"   // RegexOptions are expressed as a character string (see below for reference)
    ); // Returns true

### Substitution: ###

    // Replace all with GSub
    "A man, a plan, a canal, panama".GSub("a.", "ax");  // "A max, axplax, axcaxax, paxaxa"

    // Replace first with Sub
    "A man, a plan, a canal, panama".Sub("a.", "ax");  // "A max, a plan, a canal, panama"

### Match Data ###

    string fullname = "Lee Harvey Oswald";
    var m = fullname.MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");

    // m["firstname"] == "Lee"
    // m["lastname"] == "Oswald"
    // m[0] == "Lee Harvey Oswald"  (First element is always the entire match)
    // m[1] == "Harvey"             (Second element is always the first numbered match)
    // m.Begin(1) == 4              (This is the beginning offset of "Harvey")
    // m.End(1) == 10               (This is the end offset "Harvey")

Isn't this refreshing?

# Option Reference #

    i: Ignore case
    s: Singleline mode (period [.] matches newlines)
    x: Ignore pattern whitespace
    c: compile pattern
    r: right to left evaluation

Thank you to [SLaks][2] at [codereview.stackexchange.com][3] for his helpful review.

[1]: http://stackoverflow.com/questions/2250335/differences-among-net-capture-group-match/2251774#2251774
[2]: http://codereview.stackexchange.com/users/4994/slaks
[3]: http://codereview.stackexchange.com

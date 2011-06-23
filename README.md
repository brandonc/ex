# ex #

> A drop-in c# extension class that greatly simplifies framework regular expressions.

# What It Does #

1. strings are extended with regex-powered methods IsPatternMatch, MatchesPattern, Sub, and GSub
2. The MatchesPattern method returns a new class called MatchData that simplifies access to captures. It's a little like ruby's MatchData class. See [this answer][1] on stackoverflow for a better explanation of why MatchCollection is so complicated.
3. 'RegexOptions' (optional) can be expressed as a character string.

# Examples #

### Test a match ###

    "Adam & Steve".IsPatternMatch(@"Adam (&|and) (?<someone_else>\w+)"); // Returns true

### Test a match with case insensitivity option: ###

    "ADAM AND STEVE".IsPatternMatch(@"adam (&|and) (?<someone_else>\w+)", "i"); // Returns true

### Substitution: ###

    "A man, a plan, a canal, panama".GSub("a.", "ax");  // "A max, axplax, axcaxax, paxaxa"
    "A man, a plan, a canal, panama".Sub("a.", "ax");  // "A max, a plan, a canal, panama"

### Match Data ###

    string fullname = "John Wilkes Booth";
    var m = fullname.MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");

    // "John" == m["firstname"]
    // "Booth" == m["lastname"]
    // "John Wilkes Booth" == m[0]
    // "Wilkes" == m[1]

Isn't that better?

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

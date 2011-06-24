# ex #

> A well-tested c# extension class that makes regular expressions evaluation fun again.

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

### Find the first match ###

    "haystack needle haystack".FindPattern(@"\s(needle)\s");  // returns " needle "

### Find the first match, but return the first capture group.

    "haystack needle haystack".FindPattern(@"\s(needle)\s", 1);  // returns "needle"

### Substitution: ###

    // Replace all with GSub
    "A man, a plan, a canal, panama".GSub("a.", "ax");  // "A max, axplax, axcaxax, paxaxa"

    // Replace first with Sub
    "A man, a plan, a canal, panama".Sub("a.", "ax");  // "A max, a plan, a canal, panama"

    // Replace all using a lambda evaluator (no need for MatchEvaluator delegate!)
    var actors = "Alec Baldwin, Daniel Baldwin, William Baldwin";
    actors.GSub(
        @"(?<firstname>\w+)\s(?<lastname>\w+)",
        match =>
        {
            // Replaces any name found with "Steven Baldwin"
            return "Steven Baldwin";
        }
    ); // returns "Steven Baldwin, Steven Baldwin, Steven Baldwin"

### Scan: ###

    string headers = @"HTTP/1.1 200 OK
    Server: nginx/1.0.4
    Date: Fri, 24 Jun 2011 21:52:36 GMT
    Content-Type: text/html; charset=utf-8
    Transfer-Encoding: chunked
    Connection: keep-alive
    Status: 200 OK
    Etag: 924990f60843c36a22f65ec789ea33f3
    X-Runtime: 8ms
    Cache-Control: private, max-age=0, must-revalidate
    Strict-Transport-Security: max-age=2592000
    Content-Encoding: gzip"
    
    headers.Scan(@"([a-zA-Z\-]+): (.+)", (name, value) =>
    {
        // name, value are both strings containing the next captured values
    });

### Match Data ###

`MatchData` is a replacement for `MatchCollection` except it's actually a joy to use. You can use it as a dictionary of captures or to retrieve capture details. It only has a minor limitation compared to `MatchCollection`, and that limitation is a corner case.

    string fullname = "Lee Harvey Oswald";
    var m = fullname.MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");

    // m["firstname"] == "Lee"
    // m["lastname"] == "Oswald"
    // m[0] == "Lee Harvey Oswald"  (The first element is always the entire match [ask Larry Wall])
    // m[1] == "Harvey"             (The second element is always the first numbered match)
    
    // m.Begin(1) == 4              (This is the beginning offset of "Harvey")
    // m.End(1) == 10               (This is the end offset of "Harvey")

    // m.Begin("lastname") == 11    (This is the beginning offset of "Oswald")
    // m.End("lastname") == 17      (This is the end offset of "Oswald")

Isn't this refreshing?

# Option Reference #

    i: Ignore case
    s: Singleline mode (period [.] matches newlines)
    x: Ignore pattern whitespace
    c: compile pattern
    r: right to left evaluation

Thank you to [SLaks][2] at [codereview.stackexchange.com][3] for his helpful review.

[1]: http://stackoverflow.com/questions/2250335/differences-among-net-capture-group-match/2251774#2251774
[2]: http://stackoverflow.com/users/34397/slaks
[3]: http://codereview.stackexchange.com

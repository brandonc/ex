// #define NET35

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;

namespace Tests
{
    [TestClass]
    public class StringRegexTests
    {
        [TestMethod]
        public void sub_replaces_first_occurrence()
        {
            Assert.AreEqual("fuzbarfoobaz", "foobarfoobaz".Sub("foo", "fuz"));
        }

        [TestMethod]
        public void sub_evaluator_replaces_first_occurrence()
        {
            Assert.AreEqual("fuzbarfuzbaz", "foobarfuzbaz".Sub("foo", match => {
                return (match.Value == "foo" ? "fuz" : match.Value);
            }));
        }

        [TestMethod]
        public void gsub_replaces_all_occurrences()
        {
            Assert.AreEqual("fuzbarfuzbaz", "foobarfoobaz".GSub("foo", "fuz"));
        }

        [TestMethod]
        public void gsub_evaluator_replaces_all_occurrences()
        {
            Assert.AreEqual("foobazfooxxx", "foobarfoobaz".GSub("ba(r|z)", match =>
            {
                return (match.Value == "bar" ? "baz" : "xxx");
            }));

            Assert.AreEqual("Steven Baldwin Steven Baldwin Steven Baldwin", "Alec Balwin Daniel Balwdin William Baldwin".GSub(@"(?<firstname>\w+)\s(?<lastname>\w+)", match =>
            {
                // Replaces any name found with "Steven Baldwin"
                return "Steven Baldwin";
            }));
        }

        [TestMethod]
        public void matchdata_includes_numbered_captures()
        {
            var matches = "fuzbarfuzbaz".MatchesPattern("ba(r|z)");
            Assert.AreEqual("bar", matches[0]);
            Assert.AreEqual("r", matches[1]);
            Assert.AreEqual("baz", matches[2]);
            Assert.AreEqual("z", matches[3]);
        }

        [TestMethod]
        public void matchdata_includes_named_captures()
        {
            var matches = "fuzbarfuzbaz".MatchesPattern("ba(?<named>r|z)");
            Assert.AreEqual(3, matches.Count);
            Assert.AreEqual("bar", matches[0]);
            Assert.AreEqual("baz", matches[1]);
            Assert.AreEqual("z", matches["named"]);
        }

        [TestMethod]
        public void HasPattern_matches()
        {
            Assert.IsTrue("fuzbarfoobar".HasPattern("f(uz|oo)bar"));
        }

        [TestMethod]
        public void single_option_is_enabled()
        {
            Assert.AreEqual(4, "fuzbarFOOBAR".MatchesPattern("f(uz|oo)bar", "i").Count);
        }

        [TestMethod]
        public void scan_yields_ungrouped_strings()
        {
            "i am a sentence".Scan(@"\w+", w =>
            {
                Assert.IsFalse(String.IsNullOrEmpty(w));
            });
        }

        [TestMethod]
        public void last_index_of_pattern_returns_last_index()
        {
            Assert.AreEqual(7, "hello world".LastIndexOfPattern("[aeiou]"));
        }

        [TestMethod]
        public void scan_with_one_capture_and_two_parameters_yields_one_strings()
        {
            "hello world!".Scan(@"(..)", (c1, c2) =>
            {
                Assert.AreEqual(2, c1.Length);
                Assert.IsNull(c2);
            });
        }

        [TestMethod]
        public void scan_with_three_captures_and_two_parameters_yields_two_strings()
        {
            "hello world!".Scan(@"(..)(..)(..)", (c1, c2) =>
            {
                Assert.AreEqual(2, c1.Length);
                Assert.AreEqual(2, c2.Length);
            });
        }

        [TestMethod]
        public void scan_with_four_captures_and_four_parameters_yields_four_strings()
        {
            "hello world!".Scan(@"(..)(..)(..)(..)", (c1, c2, c3, c4) =>
            {
                Assert.AreEqual(2, c1.Length);
                Assert.AreEqual(2, c2.Length);
                Assert.AreEqual(2, c3.Length);
                Assert.AreEqual(2, c4.Length);
            });
        }

        [TestMethod]
        public void scan_with_groups_but_no_captures_yields_first_group()
        {
            int count = 0;
            "hello world!".Scan("(..)(..)", c =>
            {
                // Only the first capture is given
                Assert.AreEqual(2, c.Length);
                count++;
            });

            Assert.AreEqual("hello world!".Length / 4, count);
        }

        [TestMethod]
        public void scan_with_subgroups_yields_two_strings()
        {
            int count = 0;
            "hello world!".Scan("(.(.).)", (c1, c2) =>
            {
                // Only the first capture is given
                Assert.AreEqual(3, c1.Length);
                Assert.AreEqual(1, c2.Length);
                count++;
            });

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void scan_with_two_parameters_returns_two_outermost_subgroups()
        {
            int count = 0;
            "hello world!".Scan("(.(.(.).).)(.)", (c1, c2) =>
            {
                Assert.AreEqual(5, c1.Length);
                Assert.AreEqual(3, c2.Length);
                count++;
            });

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void scan_with_four_parameters_returns_all_captures()
        {
            "hello world!".Scan("(.(.(.).).)(.)", (c1, c2, c3, c4) =>
            {
                Assert.AreEqual(5, c1.Length);  // first group, outermost capture
                Assert.AreEqual(3, c2.Length);  // first group, inner capture
                Assert.AreEqual(1, c3.Length);  // first group, innermost capture
                Assert.AreEqual(1, c4.Length);  // second capture
            });
        }

        [TestMethod]
        public void partition_returns_correct_array()
        {
            var part = "goodbye, cruel world!".Partition("ue");
            Assert.AreEqual(3, part.Length);
            Assert.AreEqual("goodbye, cr", part[0]);
            Assert.AreEqual("ue", part[1]);
            Assert.AreEqual("l world!", part[2]);
        }

        [TestMethod]
        public void find_pattern_returns_first_match()
        {
            Assert.AreEqual("el", "hello, world".FindPattern(@"[aeiou](.)"));
        }

        [TestMethod]
        public void find_pattern_returns_first_numbered_match()
        {
            Assert.AreEqual("l", "hello, world".FindPattern(@"[aeiou](.)", 1));
        }

        [TestMethod]
        public void find_pattern_does_backreferencing()
        {
            Assert.AreEqual("ell", "hello, world".FindPattern(@"[aeiou](.)\1"));
        }

        [TestMethod]
        public void readme_examples_work()
        {
            Assert.AreEqual("A max, axplax, axcaxax, paxaxa", "A man, a plan, a canal, panama".GSub("a.", "ax"));
            Assert.AreEqual("A max, a plan, a canal, panama", "A man, a plan, a canal, panama".Sub("a.", "ax"));
            Assert.IsTrue("Adam & Steve".HasPattern(@"Adam (&|and) (?<someone_else>\w+)"));
            Assert.IsTrue("ADAM AND STEVE".HasPattern(@"adam (&|and) (?<someone_else>\w+)", "i"));

            string fullname = "Lee Harvey Oswald";
            var m = fullname.MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");

            Assert.AreEqual("Lee", m["firstname"]);
            Assert.AreEqual("Oswald", m["lastname"]);
            Assert.AreEqual("Lee Harvey Oswald", m[0]);
            Assert.AreEqual("Harvey", m[1]);
            Assert.AreEqual(4, m.Begin(1));
            Assert.AreEqual(10, m.End(1));
            Assert.AreEqual(11, m.Begin("lastname"));
            Assert.AreEqual(17, m.End("lastname"));

            string actors = @"Colin Firth,
							  Daniel Day-Lewis,
							  Sean Penn,
							  Will Smith,
							  Ryan Gosling";

            Assert.IsFalse(actors.HasPattern("(Alec|Steven|William|Daniel) Baldwin"));

            string input = "80304-6667";

            var postalcode = input.MatchesPattern(@"^(\d{5})\-(\d{4})$");
			Assert.AreEqual("80304", postalcode[1]);
			Assert.AreEqual("6667", postalcode[2]);
            Assert.AreEqual("80304-6667", postalcode[0]);

            Assert.AreEqual("STEVE", "ADAM AND STEVE".MatchesPattern(
                @"^adam (&|and) (?<someone_else>.+$)",
                "i"   // RegexOptions are expressed as a character string (see below for reference)
            )["someone_else"]); // "STEVE"

            Assert.AreEqual("needle", "haystack needle haystack".FindPattern(@"\s(needle)\s", 1));
            Assert.AreEqual(" needle ", "haystack needle haystack".FindPattern(@"\sneedle\s"));

            var response = @"HTTP/1.1 200 OK
Server: nginx/1.0.4
Date: Fri, 24 Jun 2011 21:52:36 GMT
Content-Type: text/html; charset=utf-8
Transfer-Encoding: chunked
Connection: keep-alive
Status: 200 OK
Cache-Control: max-age=0, must-revalidate
Content-Encoding: gzip";
            int found = 0;
response.Scan(@"^(?<header>[a-z\-]+): (?<value>.+)$", "im", (name, value) =>
{
    found++;
});

Assert.AreEqual(8, found);

var tvguide = @"Space Jam                      ★½
                Slap Shot 2: Breaking the Ice  ★
                Cop and a Half                 ½
                Battlefield Earth              ★
                Gigli                          ★½
                Weird Science                  ★★★
                Stop or My Mom Will Shoot!     ½";

// First movie with at least 3 stars
Assert.AreEqual("Weird Science", tvguide.FindPattern(@"^\s*([a-z0-9\s\!:\-,\.]+)\s*(★{3,}).*$", "mi", 1).Trim());
            // "Weird Science"

            /*using (var resp = WebRequest.Create("http://www.google.com").GetResponse())
            {
                using (var reader = new StreamReader(resp.GetResponseStream(), true))
                {
                    reader.ReadToEnd().Scan("href=\"(.+)\".*>(.*)</a>", (anchor, text) =>
                    {
                        Console.WriteLine("\"{0}\": [{1}]", text, anchor);
                    });
                }
            }*/
        }

        [TestMethod]
        public void matchdata_contains_names_in_named_captures()
        {
            string[] expected = { "firstname", "lastname" };
            string[] names = "Steven Baldwin".MatchesPattern(@"(?<firstname>\w+)\s(?<lastname>\w+)").GetNames();

            Assert.AreEqual(String.Join(",", expected), String.Join(",", names));
        }

        [TestMethod]
        public void matchdata_includes_multiple_named_captures()
        {
            string pattern = @"(?<firstname>\w+)\s(?<lastname>\w+)";
            var matches = "Steven Baldwin".MatchesPattern(pattern);

            Assert.AreEqual(matches["firstname"], "Steven");
            Assert.AreEqual(matches["lastname"], "Baldwin");
        }

        [TestMethod]
        public void matchdata_overwrites_last_capture()
        {
            var data = @"foo%123%456%789".MatchesPattern(@"^([a-z]+)(?:%([0-9]+))+$");
            Assert.AreEqual("foo%123%456%789", data[0]);
            Assert.AreEqual("foo", data[1]);
            Assert.AreEqual("789", data[2]);
            Assert.AreEqual(3, data.Count);
        }

        [TestMethod]
        public void matchdata_includes_numbered_and_named_captures()
        {
            var matches = "John Wilkes Booth".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");
            Assert.AreEqual("John Wilkes Booth", matches[0]);
            Assert.AreEqual("John", matches["firstname"]);
            Assert.AreEqual("Wilkes", matches[1]);
            Assert.AreEqual("Booth", matches["lastname"]);
            Assert.AreEqual(4, matches.Count);
        }

        [TestMethod]
        public void httpheaders_can_be_scanned()
        {
            string[] headersnames = { "Server", "Date", "Content-Type", "Transfer-Encoding", "Connection", "Status", "Etag", "X-Runtime", "Cache-Control", "Strict-Transport-Security", "Content-Encoding" };
            int found = 0;
            @"HTTP/1.1 200 OK
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
             .Scan(@"([a-z\-]+): (.+)", "i", (name, value) =>
             {
                 Assert.IsTrue(Array.IndexOf<string>(headersnames, name) >= 0);
                 Assert.IsFalse(String.IsNullOrEmpty(value));
                 found++;
             });

            Assert.AreEqual(11, found);
        }

        [TestMethod]
        public void matchdata_can_be_enumerated()
        {
            var matches = "John Wilkes Booth".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");
            int index = 0;
            foreach (string capture in matches)
            {
                switch (index)
                {
                    case 0:
                        Assert.AreEqual("John Wilkes Booth", capture);
                        break;
                    case 1:
                        Assert.AreEqual("Wilkes", capture);
                        break;
                    case 2:
                        Assert.AreEqual("John", capture);
                        break;
                    case 3:
                        Assert.AreEqual("Booth", capture);
                        break;
                }
                index++;
            }

            Assert.AreEqual(4, index);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void matchdata_invalid_index_throws_indexoutofrange()
        {
            var matches = "John Wilkes Booth".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");
            string s = matches[4];
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void matchdata_invalid_begin_index_throws_indexoutofrange()
        {
            var matches = "Jon Bon Jovie".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");
            int pos = matches.Begin(2);
        }

        [TestMethod]
        public void matchdata_invalid_begin_named_returns_negative_1()
        {
            var matches = "Jon Bon Jovie".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");
            Assert.AreEqual(-1, matches.Begin("middlename"));
        }

        [TestMethod]
        public void invalid_options_are_ignored()
        {
            var matches = "JON BON JOVIE".MatchesPattern(@"(?<firstname>\w+)   (?# First name)" +
                                                         @"\s(\w+)\s           (?# Middle name surrounded by spaces)" +
                                                         @"(?<lastname>\w+)    (?# Last name)", "ikx");

            Assert.AreEqual(4, matches.Count);
        }

        [TestMethod]
        public void matchdata_only_contains_matches_found_after_index()
        {
            var matches = "I Love You Jon Bon Jovie".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)", "I Love You".Length);
            Assert.AreEqual(4, matches.Count);
        }

        [TestMethod]
        public void matchdata_invalid_name_returns_null()
        {
            var matches = "John Wilkes Booth".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");
            Assert.IsNull(matches["middlename"]);
        }

        [TestMethod]
        public void matchdata_named_captures_contains_valid_positions()
        {
            var matches = "John Wilkes Booth".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");
            Assert.AreEqual(0, matches.Begin("firstname"));
            Assert.AreEqual(4, matches.End("firstname"));

            Assert.AreEqual(12, matches.Begin("lastname"));
            Assert.AreEqual(17, matches.End("lastname"));
        }

        [TestMethod]
        public void matchdata_numbered_capture_contains_valid_positions()
        {
            var matches = "John Wilkes Booth".MatchesPattern(@"(?<firstname>\w+)\s(\w+)\s(?<lastname>\w+)");
            Assert.AreEqual(5, matches.Begin(1));
            Assert.AreEqual(11, matches.End(1));
        }

#if !NET35
        [TestMethod]
        public void regex_objects_are_cached()
        {
            int startEntities = StringRegexExtensions.CacheCount;
            var data = @"uuu|iii|ooo|ppp";
            data.MatchesPattern("uuu");
            data.MatchesPattern("uuu", "i");
            data.MatchesPattern("uuu", "i");
            data.MatchesPattern("uuu", "i");
            data.MatchesPattern("uuu");
            Assert.AreEqual(startEntities + 2, StringRegexExtensions.CacheCount);
            data.HasPattern("|.+|", "ixc");
            data.HasPattern("|.+|", "ic");
            data.HasPattern("|.+|", "ixc");
            data.HasPattern("|.+|", "ixc");
            data.HasPattern("|.+|", "ixc");
            data.HasPattern("|.+|", "ixc");
            data.HasPattern("|.+|", "ixc");
            data.HasPattern("|.+|", "ixc");
            Assert.AreEqual(startEntities + 4, StringRegexExtensions.CacheCount);
        }
#endif
    }
}

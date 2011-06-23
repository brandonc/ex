using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var matches = "fuzbarfuzbaz".MatchesPattern("ba(?<named>r|z)", "e");
            Assert.AreEqual(3, matches.Count);
            Assert.AreEqual("bar", matches[0]);
            Assert.AreEqual("baz", matches[1]);
            Assert.AreEqual("z", matches["named"]);
        }

        [TestMethod]
        public void ispatternmatch_matches()
        {
            Assert.IsTrue("fuzbarfoobar".IsPatternMatch("f(uz|oo)bar"));
        }

        [TestMethod]
        public void single_option_is_enabled()
        {
            Assert.AreEqual(4, "fuzbarFOOBAR".MatchesPattern("f(uz|oo)bar", "i").Count);
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
            var matches = "Brandon Croft".MatchesPattern(pattern);

            Assert.AreEqual(matches["firstname"], "Brandon");
            Assert.AreEqual(matches["lastname"], "Croft");
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
            data.MatchesPattern("uuu");
            Assert.AreEqual(startEntities + 2, StringRegexExtensions.CacheCount);
            data.IsPatternMatch("|.+|", "ixc");
            data.IsPatternMatch("|.+|", "ic");
            data.IsPatternMatch("|.+|", "ixc");
            data.IsPatternMatch("|.+|", "ixc");
            data.IsPatternMatch("|.+|", "ixc");
            data.IsPatternMatch("|.+|", "ixc");
            data.IsPatternMatch("|.+|", "ixc");
            data.IsPatternMatch("|.+|", "ixc");
            Assert.AreEqual(startEntities + 4, StringRegexExtensions.CacheCount);
        }
#endif
    }
}

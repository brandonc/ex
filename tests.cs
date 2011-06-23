using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace test
{
    [TestClass]
    public class StringRegexTests
    {
        [TestMethod]
        public void TestSub()
        {
            Assert.AreEqual("fuzbarfuzbaz", "foobarfuzbaz".Sub("foo", "fuz"));
        }

        [TestMethod]
        public void TestGSub()
        {
            Assert.AreEqual("fuzbarfuzbaz", "foobarfoobaz".GSub("foo", "fuz"));
        }

        [TestMethod]
        public void TestMatch()
        {
            var matches = "ba(r|z)".Matches("fuzbarfuzbaz");
            Assert.AreEqual("bar", matches[0]);
            Assert.AreEqual("r", matches[1]);
            Assert.AreEqual("baz", matches[2]);
            Assert.AreEqual("z", matches[3]);
        }

        [TestMethod]
        public void TestExplicitCaptureMatch()
        {
            var matches = "ba(?<named>r|z)".Matches("fuzbarfuzbaz", "e");
            Assert.AreEqual(3, matches.Count);
            Assert.AreEqual("bar", matches[0]);
            Assert.AreEqual("baz", matches[1]);
            Assert.AreEqual("z", matches["named"]);
        }

        [TestMethod]
        public void TestIsMatch()
        {
            Assert.IsTrue("f(uz|oo)bar".IsMatch("fuzbarfoobar"));
        }

        [TestMethod]
        public void TestOptions()
        {
            Assert.AreEqual(4, "f(uz|oo)bar".Matches("fuzbarFOOBAR", "i").Count);
        }

        [TestMethod]
        public void TestNamedCaptures()
        {
            string pattern = @"(?<firstname>\w+)\s(?<lastname>\w+)";
            var matches = pattern.Matches("Brandon Croft");

            Assert.AreEqual(matches["firstname"], "Brandon");
            Assert.AreEqual(matches["lastname"], "Croft");
        }

        [TestMethod]
        public void TestCaptureGroups()
        {
            var data = @"^([a-z]+)(?:%([0-9]+))+$".Matches(@"foo%123%456%789");
            Assert.AreEqual("foo%123%456%789", data[0]);
            Assert.AreEqual("foo", data[1]);
            Assert.AreEqual("789", data[2]);
        }
    }
}

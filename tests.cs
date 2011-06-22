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
            Assert.AreEqual(2, "ba(r|z)".Matches("fuzbarfuzbaz").Count);
        }

        [TestMethod]
        public void TestIsMatch()
        {
            Assert.IsTrue("f(uz|oo)bar".IsMatch("fuzbarfoobar"));
        }

        [TestMethod]
        public void TestOptions()
        {
            Assert.AreEqual(2, "f(uz|oo)bar".Matches("fuzbarFOOBAR", "i").Count);
        }
    }
}

using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32;

namespace Say32
{
    [TestClass]
    public class TempTest
    {
        [TestMethod]
        public void BraceExperssionHeaderMatchTest()
        {
            const string text = "fdsfjak {B:4{key}1} and {test} and {{skip}}";
            var pattern = @"(?'brace_open'{{)|{(?'header'\s*\w+):|{(?'key'\s*\w+)}";

            var match = Regex.Match(text, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            Assert.AreEqual(true, match.Success);
            Assert.IsFalse(match.Groups["brace_open"].Success);
            Assert.IsFalse(match.Groups["key"].Success);
            Assert.IsTrue(match.Groups["header"].Success);

            var header = match.Groups["header"].Value;
            Assert.AreEqual("B", header);

            (var before, var after) = Cut(text, "{" + header + ":");

            Assert.AreEqual("fdsfjak ", before);
            Assert.AreEqual("4{key}1} and {test} and {{skip}}", after);
        }

        [TestMethod]
        public void BraceExperssionHeaderMatchTest2()
        {
            const string text = "fdsfjak { B :4{key}1} and { test} and {{skip}}";

            var header = HeaderParser.Parse(text);
            Assert.AreEqual("B", header.Value);
            Assert.AreEqual(" B ", header.Original);
            Assert.AreEqual(BraceExpressionHeader.Types.Header, header.Type);

            (var before, var after) = Cut(text, "{" + header.Original + ":");

            Assert.AreEqual("fdsfjak ", before);
            Assert.AreEqual("4{key}1} and { test} and {{skip}}", after);

            var bexp = new BraceExpression
            {
                Header = header,

                Body = new WorkingExpression { Original = after }
            };

        }

        private (string before, string after) Cut( string text, string delimiter )
        {
            var index = text.IndexOf(delimiter);

            if (index == -1)
                return (text, "");

            return (text.Substring2(0, index - 1), text.Substring(index + delimiter.Length));
        }

        class BraceExpressionHeader
        {
            public enum Types
            {
                Key, Header
            }

            public BraceExpressionHeader(string original, Types type )
            {
                Type = type;
                Original = original;
            }

            public Types Type { get; }
            public string Value => Original.Trim();
            public string Original { get; }
            public override string ToString() => Value;
        }

        class BraceExpression
        {
            public BraceExpressionHeader Header { get; set; }

            public WorkingExpression Body { get; set; }
        }

        class WorkingExpression
        {
            public string Original { get; set; }
            public string Working { get; set; }

            public string Result { get; set; }
        }

        class HeaderParser
        {
            private const string _pattern = @"(?'brace_open'{{)|{(?'header'\s*\w+\s*):|{(?'key'\s*\w+)}";
            private static readonly Regex _regex = new Regex(_pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            public static BraceExpressionHeader Parse(string text)
            {
                var match = _regex.Match(text);

                if (!match.Success || match.Groups["brace_open"].Success)
                    return null;
                else if (match.Groups["key"].Success)
                    return new BraceExpressionHeader(match.Groups["key"].Value, BraceExpressionHeader.Types.Key);
                else
                    return new BraceExpressionHeader(match.Groups["header"].Value, BraceExpressionHeader.Types.Header);
            }
        }
    }
}

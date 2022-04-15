using System;
using System.Collections.Generic;
using System.Text;

namespace Say32
{
    public static class TextParsing
    {

        public static string GetEnclosed( this string str, string openMark, string closeMark, out string remaining, bool alreadyOpen = false )
        {
            if (openMark == closeMark)
                return GetEnclosed(str, openMark, out remaining, alreadyOpen);

            var result = "";

            int open = alreadyOpen ? 1 : 0;
            int close = 0;

            foreach (var text in StringUtil.Split(str, closeMark))
            {
                result += text;

                open += text.Count(openMark);
                close++;

                if (open == close)
                {
                    remaining = str.TrimStart($"{(alreadyOpen ? "" : openMark)}{result}{closeMark}");
                    return result;
                }

                result += closeMark;
            }

            throw new ParsingException($"{nameof(GetEnclosed)}(open='{openMark}', close='{closeMark}')");

            // handle duplicated brace expressions
            //return new ValueExpressionProcessor(result).Process(_arg).ResultString;

        }

        public static string GetEnclosed( this string str, string openCloseMark, out string remaining, bool alreadyOpen = false )
        {
            var result = "";

            int open = alreadyOpen ? 1 : 0;

            foreach (var text in StringUtil.Split(str, openCloseMark))
            {
                result += text + openCloseMark;

                if (open == 0)
                {
                    if (text.Trim() != "")
                        throw new ParsingException($"[{nameof(GetEnclosed)}] non empty string before open mark({openCloseMark})");

                    open++;
                    continue;
                }
                else // already open
                {
                    remaining = str.TrimStart(result);
                    return text;
                }

            }

            throw new ParsingException($"{nameof(GetEnclosed)}(open/close='{openCloseMark}')");

            // handle duplicated brace expressions
            //return new ValueExpressionProcessor(result).Process(_arg).ResultString;

        }

        public static bool ParsingStartsWith(this string str, out string match, out string remaining, params string[] values )
        {
            var trimmed = str.TrimStart();

            foreach (var value in values)
            {
                if (trimmed.StartsWith(value))
                {
                    var index = str.IndexOf(value);
                    match = str.Substring2(0, index - 1) + value;
                    remaining = str.TrimStart(match);
                    return true;
                }
            }

            remaining = "";
            match = "";

            return false;
        }
    }



    [Serializable]
    public class ParsingException : Exception
    {
        public ParsingException() { }
        public ParsingException( string message ) : base(message) { }
        public ParsingException( string message, Exception inner ) : base(message, inner) { }
        protected ParsingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }
}

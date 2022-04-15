using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32
{
    public static class StringUtil
    {
        public static int CountLines(this string text) 
            => string.IsNullOrEmpty(text) ? 0 : text.Count(c => c == '\n') + 1;

        public static bool IsWhiteSpace(this string text) => text.Trim() == "";
        public static bool IsWhiteSpaceSafe(this string text, bool defaultValue = true) => text != null ? text.Trim() == "" : defaultValue;

        public static string FirstLine(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var lines = text.Split('\n');
            return lines[0];
        }

        public static string LastLine(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var lines = text.Split('\n');
            return lines[lines.Length - 1];            
        }

        public static string ToCamelCase(this string text) => Char.ToLowerInvariant(text[0]) + text.Substring(1);

        public static string? Format(this string format, params object[] args)
        {
            if (format == null) return null;

            return string.Format(format, args);
        }

        public static string Trim( this string str, string stringToTrim )
        {
            return str.TrimStart(stringToTrim).TrimEnd(stringToTrim);
        }


        public static string TrimStart( this string str, string stringToTrim )
        {
            return str.StartsWith(stringToTrim) ? str.Substring(stringToTrim.Length) : str;
        }

        public static string TrimEnd(this string str, string stringToTrim)
        {
            return str.EndsWith(stringToTrim) ? str.Substring(0, str.Length - stringToTrim.Length) : str;
        }



        public static string Substring2(this string str, int startIndex, int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex + 1);
        }

        public static (string before, string after) Cut( this string str, char delimiter )
        {
            var index = str.IndexOf(delimiter);

            if (index == -1)
                return (str, "");

            return (str.Substring2(0, index - 1), str.Substring(index + 1));
        }

        public static (string before, string after) Cut( this string str, string delimiter )
        {
            var index = str.IndexOf(delimiter);

            if (index == -1)
                return (str, "");

            return (str.Substring2(0, index - 1), str.Substring(index + delimiter.Length));
        }

        public static bool StartsWith(this string str, params string[] values)
        {
            foreach (var value in values)
            {
                if (str.StartsWith(value))
                    return true;
            }
            return false;
        }

        public static int Count(this string src, string substring)
        {
            return src.Select(( c, i ) => src.Substring(i))
                    .Count(sub => sub.StartsWith(substring));
        }

        public static string StripQuatations(this string str, bool force = false, bool trim = false)
        {
            var str0 = str;
            if (trim) str = str.Trim();

            if (force || str.EnclosedWith("\'") )
                return str.Trim('\'');
            else
                return str0;
        }

        public static string StripDoubleQuatations( this string str, bool force = false, bool trim = false )
        {
            var str0 = str;
            if (trim)  str = str.Trim();

            if (force || str.EnclosedWith("\""))
                return str.Trim('\"');
            else
                return str0;
        }

        public static string StripAllQuatations( this string str, bool force = false, bool trim = false )
        {
            var str0 = str;
            if (trim) str = str.Trim();

            if (force || str.EnclosedWith("\'"))
                return str.Trim('\'');
            else if (force || str.EnclosedWith("\""))
                return str.Trim('\"');
            else
                return str0;
        }

        public static bool EnclosedWith(this string str, string withThis)
        {
            return str.StartsWith(withThis) && str.EndsWith(withThis);
        }

        public static string[] Split(this string str, string delimiter)
        {
            List<string> r = new List<string>();

            int newIndex=0, index = 0;

            while((newIndex = str.IndexOf(delimiter, index)) != -1)
            {
                r.Add(str.Substring2(index, newIndex - 1));
                index = newIndex+1;
            }

            r.Add(str.Substring(index));

            return r.ToArray();
        }

        public static string[] Split( this string str, params string[] delimiters )
        {
            List<string> r = new List<string>();

            int newIndex = 0, index = 0;

            while ((newIndex = str.IndexOfAny(index, delimiters)) != -1)
            {
                r.Add(str.Substring2(index, newIndex - 1));
                index = newIndex + 1;
            }

            r.Add(str.Substring(index));

            return r.ToArray();

        }

        public static int IndexOfAny(this string str, int startIndex, params string[] delimiters )
        {
            var indexes = new List<int>();

            delimiters.ForEach(d =>
            {
                var index = str.IndexOf(d, startIndex);
                if (index != -1)
                {
                    indexes.Add(index);
                }
            });

            if (indexes.Count == 0) return -1;

            return indexes.Min();
        }

        public static Name? ToName( this string? str ) => str == null ? null : (Name)str;

        public static string Repeat(this string str, int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        public static string TrimStartRepeat(this string str, string toBeTrimmed, int count)
        {
            for (int i = 0; i < count; i++)
            {
                str = str.TrimStart(toBeTrimmed);
            }

            return str;
        }

        public static string TrimEndRepeat(this string str, string toBeTrimmed,  int count)
        {   
            for (int i = 0; i < count; i++)
            {
                str = str.TrimEnd(toBeTrimmed);
            }

            return str;
        }


        public static QuotatedStringPart? GetDoubleQuotated(this string text, int from = 0)
        {
            var start = GetDoubleQuotationIndex(text, from);
            if (start == -1) return null;

            var end = GetDoubleQuotationIndex(text, start);
            if (end == -1) return null;


            return new QuotatedStringPart(text, start, end);
        }

        internal static int GetDoubleQuotationIndex(this string text, int start = 0)
        {   

            while (true)
            {
                var idx = text.IndexOf('\"', start + 1);

                if (idx == -1 || idx == 0) return idx;

                if (text[idx - 1] == '\\')
                {
                    start = idx + 1;
                }
                else
                    return idx;
            }
        }

        public static List<StringPart> SplitByQuotatedParts(this string text)
        {
            var list = new List<StringPart>();
            var from = 0;
            QuotatedStringPart? lastQp = null;

            while(true)
            {
                var qp = text.GetDoubleQuotated(from);
                if (qp == null)
                {
                    if (lastQp == null)
                        list.Add(new StringPart(text, 0, text.Length - 1));
                    else
                        list.Add(new StringPart(text, lastQp.EndAt + 1, text.Length - 1));

                    return list;
                }

                list.Add(new StringPart(text, from, qp.StartAt - 1));
                list.Add(qp);

                from = qp.EndAt + 1;
                if (from == text.Length) return list;

                lastQp = qp;
            }

        }

        public static string ApplyIndent( this string text, string indent )
        {
            //if (text == null) return null;

            var lines = text.Split('\n');
            if (lines.Length == 1)
                return indent + text;

            var sb = new StringBuilder();
            var i = 0;
            foreach (var line in lines)
                if (i++ == 0)
                    sb.AppendLine(indent + line);
                else
                    sb.AppendLine($"{indent + '\t'}{line}");

            return sb.ToString();
        }

        public static string PadRightForce(this string text, int width)
        {
            if (text.Length > width)
                return text.Substring(0, width);
            else
                return text.PadRight(width);
        }

        public static string PadLeftForce( this string text, int width )
        {
            if (text.Length > width)
                return text.Substring(0, width);
            else
                return text.PadLeft(width);
        }

        public static int ToInt( this string text ) => int.Parse(text);
        public static int ToIntSafe(this string text )
        {
            if (int.TryParse(text, out int result))
                return result;
            else
                return default;
        }

        public static string GetUntil( this string @string, string openMark, string closeMark, bool alreadyOpen )
        {
            var result = "";

            int open = alreadyOpen ? 1 : 0;
            int close = 0;

            foreach (var text in @string.Split(closeMark))
            {
                result += text;

                open += text.Count(openMark);
                close++;

                if (open == close)
                    return result;

                result += closeMark;
            }

            throw new Exception($"{nameof(GetUntil)}(open='{openMark}', close='{closeMark}')");

            // handle duplicated brace expressions
            //return new ValueExpressionProcessor(result).Process(_arg).ResultString;
        }
    }


    public class StringPart
    {
        public StringPart(string text, int start, int end)
        {
            Target = text;

            StartAt = start;
            EndAt = end;

            Before = text.Substring(0, start);
            After = text.Substring2(end + 1, text.Length - 1);
            Body = text.Substring2(start, end);
        }

        public bool Quotated { get; protected set; } = false;

        public string Target { get; }

        public int StartAt { get; }
        public int EndAt { get; }

        public string Body { get; protected private set; }

        public string Before { get; }
        public string After { get; }

        public override string ToString()
        {
            return Body;
        }

        public static implicit operator string (StringPart part) => part.ToString();
    }

    public class QuotatedStringPart : StringPart
    {
        public QuotatedStringPart(string text, int start, int end)
            :base(text, start, end)
        {
            Quotated = true;
            Body = text.Substring2(start + 1, end - 1);            
        }

        public override string ToString()
        {
            return $"\"{Body}\"";
        }
    }


    public class Indent
    {
        public string Tab { get; set; } = "\t";
        public string Value { get; private set; } = "";

        public Indent Increase() => new Indent { Value = Value + Tab, Tab = Tab };
        public Indent Decrease() => new Indent { Value = Value.TrimStart(Tab), Tab = Tab };

        public static implicit operator string(Indent indent) => indent.Value;

        public override string ToString() => Value;

        public static Indent operator +(Indent indent, int increase) => increase switch
        {
            int zero when increase == 0 => new Indent { Tab = indent.Tab, Value = indent.Value },
            int positive when increase > 0 => new Indent { Tab = indent.Tab, Value = indent.Value + indent.Tab.Repeat(increase) },
            int negative when increase > 0 => new Indent { Tab = indent.Tab, Value = indent.Value.TrimEndRepeat(indent.Tab, -increase) },
            _ => throw new Exception("This is impossible")
        };
    }


}

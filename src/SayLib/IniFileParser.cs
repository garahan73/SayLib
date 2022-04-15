using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Say32
{
    public abstract class IniFileParser
    {
        protected string PreviousCategory { get; private set; } = "";
        protected string Category { get; set; } = "";


        public async Task LoadAndParseIniFileAsync(string iniFilePath)
        {
            var @try = new TryAsync<string>(() => Task.Run(() => File.ReadAllText(iniFilePath)));
            if (await @try.RunAsync())
            {
                ParseContents(@try.Result);
            }
            else
            {
                throw new LoadIniFileException(iniFilePath, @try.Exception);
            }

        }

        public void ParseContents(string config)
        {
            ParseLines(config.Split('\n'));
        }

        public void ParseContentsAsync(string config) => Task.Run(() => ParseContents(config));

        internal void ParseLines(string[] lines)
        {
            OnParsingStart();

            Category = "";
            PreviousCategory = "";

            var lineNo = 0;

            foreach (var line0 in lines)
            {
                lineNo++;

                var line = line0.Trim();

                if (line == "" || line.StartsWith("//") || line.StartsWith("#"))
                    continue;

                try
                {
                    if (IsCategory(line))
                    {
                        PreviousCategory = Category;
                        Category = ParseCategory(line);
                        ValidateCategory();
                        continue;
                    }

                    (var item, var value) = line.Cut("=");
                    item = item.Trim();

                    if (item == "")
                        throw new Exception("INI file key can't be empty");

                    // remove comments from value
                    (value, _) = value.Cut("//");
                    (value, _) = value.Cut("#");
                    value = value.Trim();

                    ParseLine(item, value);

                }
                catch (Exception ex)
                {
                    throw new IniFileParsingException($"line({lineNo}) = '{line}'. Message={ex.Message}", ex);
                }
            }

            OnParsingEnd();
        }

        protected abstract void OnParsingStart();
        protected abstract void OnParsingEnd();

        protected abstract void ValidateCategory();

        private string ParseCategory(string line)
        {
            //if (!IsCategory(line))
            //    throw new ParsingException($"Illegal format. Missing '[' or ']'");

            return line.Trim().TrimStart("[").TrimEnd("]").Trim().ToLower();

        }

        private bool IsCategory(string line) => line.Trim().StartsWith("[") && line.EndsWith("]");

        protected abstract void ParseLine(string item, string value);
    }



    [Serializable]
    public class LoadIniFileException : Exception
    {
        public LoadIniFileException() { }
        public LoadIniFileException(string message) : base(message) { }
        public LoadIniFileException(string message, Exception? inner) : base(message, inner) { }
        protected LoadIniFileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class IniFileParsingException : Exception
    {
        public IniFileParsingException() { }
        public IniFileParsingException(string message) : base(message) { }
        public IniFileParsingException(string message, Exception inner) : base(message, inner) { }
        protected IniFileParsingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
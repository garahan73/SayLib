using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Say32
{
    public class IniFileParser2
    {
        protected string PreviousCategory { get; private set; } = "";
        protected string Category { get; set; } = "";

        public string CategorySeparator { get; set; } = "::";

        private IniData _result = new IniData();


        public async Task<IniData> StartAsync( string iniFilePath )
        {
            _result = new IniData {  CategorySeparator = CategorySeparator };

            var trying = new TryAsync<string>(() => Task.Run(()=>File.ReadAllText(iniFilePath)));
            if (await trying.RunAsync())
            {
                ParseLines(trying.Result.Split('\n'));

                return _result;
            }
            else 
            {
                throw new LoadIniFileException(iniFilePath, trying.Exception);
            }

        }

        private void ParseLines( string[] lines )
        {
            //OnParsingStart();

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
                    if (ParseCategory(line))
                    {
                        continue;
                    }

                    (var itemKey, var value) = line.Cut("=");

                    if (value == "")
                        continue;

                    itemKey = itemKey.Trim();

                    if (itemKey == "")
                        throw new Exception("INI file key can't be empty");

                    // remove comments from value
                    (value, _) = value.Cut("//");
                    (value, _) = value.Cut("#");
                    value = value.Trim();

                    //ParseLine(itemKey, value);
                    _result.Add(Category, itemKey, value);


                }
                catch (Exception ex)
                {
                    throw new IniFileParsingException($"line({lineNo}) = '{line}'. Message={ex.Message}", ex);
                }
            }

            //OnParsingEnd();
        }

        //protected abstract void OnParsingStart();
        //protected abstract void OnParsingEnd();
        //protected abstract void ValidateCategory();

        private bool ParseCategory( string line )
        {
            var isCategory = line.Trim().StartsWith("[") && line.EndsWith("]");
            if (!isCategory)
                return false;

            PreviousCategory = Category;
            Category = line.Trim().TrimStart("[").TrimEnd("]").Trim();

            return true;
        }
    }


    public class IniData
    {
        private readonly Dictionary<string, string> _data = new Dictionary<string, string>();

        public string CategorySeparator { get; set; } = "::";

        public int Count => _data.Count;

        public void Add(string category, string itemKey, string itemValue)
        {
            _data.Add(ToFullKey(category, itemKey), itemValue);
        }

        public string ToFullKey(string category, string itemKey) => $"{category}{CategorySeparator}{itemKey}";

        public string this[string fullKey] => _data[fullKey];
        public string GetValue(string category, string itemKey) => _data[ToFullKey(category, itemKey)];
    }





}

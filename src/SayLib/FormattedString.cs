using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32
{
    public class FormattedString
    {
        public string? Format { get; set; }
        public object[]? Parameters { get; set; }

        public void SetParameters(params object[] parameters) => Parameters = parameters;

        public bool IsValid => Format != null;

        public override string? ToString() => Format == null ? null :
            Parameters == null ? Format : string.Format(Format, Parameters);

        public void Update(FormattedString fString)
        {
            if (fString.Format != null)
                Format = fString.Format;

            if (fString.Parameters != null)
                Parameters = fString.Parameters;
        }

        public static FormattedString Create(string format, params object[] parameters) => new FormattedString { Format = format, Parameters = parameters };

        public static implicit operator string?(FormattedString value) => value?.ToString();
        public static implicit operator FormattedString(string value) => new FormattedString { Format = value };



        public static FormattedString operator +(FormattedString fs, object parameter)
        {
            if (fs.Parameters == null)
                fs.Parameters = new object[] { };

            var result = new FormattedString { Format = fs.Format };

            var list = fs.Parameters.ToList();
            list.Add(parameter);
            result.Parameters = list.ToArray();

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Say32
{
    public class Name
    {
        public const string DEFAULT_DELIMITER = ".";

        public string Delimiter { get; set; } = DEFAULT_DELIMITER;

        public string NameSpace { get; set; } = "";
        public string NameOnly { get; set; } = "";

        public string FullName => !HasNameSpace? NameOnly : $"{NameSpace}{Delimiter}{NameOnly}";

        public bool HasNameSpace => NameSpace != "";

        public Name( string fullName, string? delimiter = null )
        {
            Delimiter = delimiter ?? DEFAULT_DELIMITER;

            var index = fullName.LastIndexOf(Delimiter);
            if (index == -1)
            {
                NameOnly = fullName;
                return;
            }

            NameSpace = fullName.Substring2(0, index - 1);
            NameOnly = fullName.Substring(index + Delimiter.Length);
        }

        public override string ToString() => FullName;

        public static implicit operator Name( string fullName ) => new Name(fullName);
        public static implicit operator string( Name name ) => name.FullName;

        public override bool Equals( object obj ) => obj switch
        {
            null => false,
            Name n => FullName == n.FullName,
            string s => ((Name)s).FullName == FullName,
            _ => false
        };

        public override int GetHashCode() => FullName.GetHashCode();


    }
}

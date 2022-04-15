using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Say32
{
    public class LazyLogContents
    {
        public string? String;
        public Func<string>? Func;

        public static implicit operator LazyLogContents(string? str) => new LazyLogContents { String = str };
        public static implicit operator LazyLogContents(Func<string>? func) => new LazyLogContents { Func = func };

        public override string ToString() => Func != null ? Func() : String ?? "";
    }


#pragma warning disable CS8601, CS8604 // 가능한 null 참조 할당입니다.
    public class DebugLogger<TEnumLevel> where TEnumLevel: Enum
    {
        public string? HEADER;
        public bool Enabled = false;

        public TEnumLevel Level = default;

        public bool CanLog(TEnumLevel level) => Enabled && Level.GetLongValue() >= level.GetLongValue();

        public void Log(Func<string> func, TEnumLevel level = default) => Log((LazyLogContents)func, level);
        public void Log(LazyLogContents message, TEnumLevel level = default)
        {
            if (CanLog(level))
                Raw($"[{HEADER}] {message}");
        }

        public void Log(string category, Func<string> func, TEnumLevel level = default) => Log(category, (LazyLogContents)func, level);

        public void Log(string category, LazyLogContents message, TEnumLevel level = default)
        {
            if (CanLog(level))
                Log($"{category}: {message}", level);
        }

        public void SubLog(Func<string> func, TEnumLevel level = default) => SubLog((LazyLogContents)func, level);

        public void SubLog(LazyLogContents message, TEnumLevel level = default)
        {
            if (CanLog(level))
                Log($"\t- {message}", level);
        }

        public void Raw(Func<string> func, TEnumLevel level = default) => Raw((LazyLogContents)func, level);
        public void Raw(LazyLogContents message, TEnumLevel level = default)
        {
            if (CanLog(level))
                Debug.WriteLine(message);
        }

    }

#pragma warning restore CS8601, 8604 // 가능한 null 참조 할당입니다.

    public class DebugLoggerOld
    {
        public string? HEADER;
        public bool Enabled = true;
        public int Level = 0;

        public bool CanLog(int level) => Enabled && Level >= level;

        public void Log(Func<string> func, int level = 0) => Log((LazyLogContents)func, level);
        public void Log(LazyLogContents message, int level = 0)
        {
            if (Enabled && (int)level <= (int)Level)
                Debug.WriteLine($"[{HEADER}] {message}");
        }

        public void Log(string category, Func<string> func, int level = 0) => Log(category, (LazyLogContents)func, level);

        public void Log(string category, LazyLogContents message, int level = 0)
        {
            if (Enabled && (int)level <= (int)Level)
                Log($"{category}: {message}", level);
        }

        public void SubLog(Func<string> func, int level = 0) => SubLog((LazyLogContents)func, level);

        public void SubLog(LazyLogContents message, int level = 0)
        {
            if (Enabled && (int)level >= (int)Level)
                Log($"\t- {message}", level);
        }
    }
}

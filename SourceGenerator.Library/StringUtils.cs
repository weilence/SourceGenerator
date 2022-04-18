using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceGenerator.Library
{
    public class StringUtils
    {
        public static List<string> SplitName(string s)
        {
            var parts = new List<string>();
            if (s.Length == 0)
            {
                return parts;
            }

            if (s[0] == '_')
            {
                s = s.Substring(1);
            }

            if (s.Contains("_"))
            {
                return s.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => m.ToLower().Replace(".", "_"))
                    .ToList();
            }

            var result = new List<string>();
            var word = s[0].ToString();
            for (var i = 1; i < s.Length; i++)
            {
                var c = s[i];
                if (char.IsUpper(c) && !char.IsUpper(s[i - 1]))
                {
                    result.Add(word);
                    word = "";
                }

                if (c == '.')
                {
                    word += '_';
                }
                else
                {
                    word += char.ToLower(c);
                }
            }

            if (word != "")
            {
                result.Add(word);
            }

            return result;
        }

        public static string ToCamelCase(string s)
        {
            return string.Join("",
                SplitName(s).Select((m, index) => index == 0 ? m : char.ToUpper(m[0]) + m.Substring(1)));
        }

        public static string ToPascalCase(string s)
        {
            return string.Join("", SplitName(s).Select(m => char.ToUpper(m[0]) + m.Substring(1)));
        }

        public static string ToFieldName(string s)
        {
            return "_" + ToCamelCase(s);
        }
    }
}
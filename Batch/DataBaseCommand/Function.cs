using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SBM.DataBaseCommand
{
    internal static class Function
    {
        private static string REGEX_WILCARDS = @"\[([^\[)]*[^\]])\]";
        private static string DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";

        public static string Wilcard(this string @string)
        {
            string result = @string;

            var now = DateTime.Now;

            var withWilcards = Regex.Replace(@string, REGEX_WILCARDS, "*");

            foreach (Match match in Regex.Matches(@string, REGEX_WILCARDS))
            {
                var newValue = match.Value
                    .Remove(0, 1)
                    .Replace("YYYY", string.Format("{0,4:0000}", now.Year))
                    .Replace("YYY", string.Format("{0,3:000}", now.Year - 2000))
                    .Replace("YY", string.Format("{0,2:00}", now.Year - 2000))
                    .Replace("MM", string.Format("{0,2:00}", now.Month))
                    .Replace("DD", string.Format("{0,2:00}", now.Day))
                    .Replace("HH", string.Format("{0,2:00}", now.Hour))
                    .Replace("mm", string.Format("{0,2:00}", now.Minute))
                    .Replace("ss", string.Format("{0,2:00}", now.Second));

                newValue = newValue.Remove(newValue.Length - 1);

                int count = newValue.Count(c => c == '#');
                if (count > 0)
                {
                    int last = 0;
                    if (Directory.Exists(Path.GetDirectoryName(withWilcards)))
                    {
                        var list = Directory.GetFiles(Path.GetDirectoryName(withWilcards), Path.GetFileName(withWilcards));

                        foreach (var file in list)
                        {

                            int current = 0;

                            int idxStart = result.IndexOf(match.Value);
                            int length = match.Value.Length;

                            if (idxStart + length - 2 <= file.Length &&
                                int.TryParse(file.Substring(idxStart, length - 2), out current))
                            {
                                if (current > last) last = current;
                            }
                        }
                    }

                    newValue = newValue.Replace(new String('#', count),
                        string.Format("{0," + count + ":" + new String('0', count) + "}", last + 1));
                }

                result = result.Replace(match.Value, newValue);
            }

            return result;
        }


        public static object ToInt64OrDBNull(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return DBNull.Value;
            }
            else
            {
                return Convert.ToInt64(@string);
            }
        }

        public static object ToInt32OrDBNull(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return DBNull.Value;
            }
            else
            {
                return Convert.ToInt32(@string);
            }
        }

        public static object ToInt16OrDBNull(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return DBNull.Value;
            }
            else
            {
                return Convert.ToInt16(@string);
            }
        }

        public static object ToDecimalOrDBNull(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return DBNull.Value;
            }
            else
            {
                return Convert.ToDecimal(@string);
            }
        }

        public static object ToDoubleOrDBNull(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return DBNull.Value;
            }
            else
            {
                return Convert.ToDouble(@string);
            }
        }

        public static object ToBoolOrDBNull(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return DBNull.Value;
            }
            else
            {
                return Convert.ToBoolean(@string);
            }
        }

        public static object ToDateOrDBNull(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return DBNull.Value;
            }
            else
            {
                return DateTime.ParseExact(@string, DATE_FORMAT, null);
            }
        }

        public static object ToGuidOrDBNull(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return DBNull.Value;
            }
            else
            {
                return Guid.Parse(@string);
            }
        }

        public static string ToStringFormatted(this DateTime @DateTime)
        {
            return @DateTime.ToString(DATE_FORMAT);
        }

        public static string ToStringFormatted(this DateTimeOffset @DateTimeOffset)
        {
            return @DateTimeOffset.ToString(DATE_FORMAT);
        }

        public static NameValueCollection ParseAlias(this string @string)
        {
            var alias = new NameValueCollection();

            var fieldsDef = @string.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var field in fieldsDef)
            {
                var pair = field.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length > 0)
                {
                    if (pair.Length == 1)
                    {
                        alias.Add(pair[0].Trim(), pair[0].Trim());
                    }
                    else
                    {
                        alias.Add(pair[0].Trim(), pair[1].Trim());
                    }
                }
                else
                {
                    throw new Exception(@"Invalid format {x,x}{x}");
                }
            }

            return alias;
        }
    }
}

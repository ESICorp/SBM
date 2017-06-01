using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SBM.MailIn
{
    internal static class Function
    {
        private static string REGEX_WILCARDS = @"\[([^\[)]*[^\]])\]";
        private static string DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";
        //private static string DATE_FORMAT_ISO = "yyyy-MM-ddTHH:mm:ss.fff";

        public static string DontOverwrite(this string @string)
        {
            var path = @string;
            var extension = Path.GetExtension(path);

            var regexCopyof = new Regex(@"_copy\(\d+\)$");

            while (File.Exists(path))
            {
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                var copyof = regexCopyof.Match(nameWithoutExtension);

                string newName = null;
                if (copyof.Success)
                {
                    var newIndex = Convert.ToInt16(copyof.Value.Substring(6, copyof.Value.Length - 7)) + 1;

                    newName = regexCopyof.Replace(nameWithoutExtension, string.Format("_copy({0})", newIndex));
                }
                else
                {
                    newName = nameWithoutExtension + "_copy(1)";
                }

                if (!string.IsNullOrWhiteSpace(extension))
                {
                    newName += "." + extension;
                }

                path = Path.Combine(Path.GetFullPath(path), newName);
            }

            return path;
        }

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
                    long last = 0;
                    if (Directory.Exists(Path.GetDirectoryName(withWilcards)))
                    {
                        var list = Directory.GetFiles(Path.GetDirectoryName(withWilcards), Path.GetFileName(withWilcards));

                        foreach (var file in list)
                        {
                            long current = 0;

                            int idxStart = result.IndexOf(match.Value);
                            int length = match.Value.Length;

                            if (idxStart + length - 2 <= file.Length &&
                                long.TryParse(file.Substring(idxStart, length - 2), out current))
                            {
                                if (current > last) last = current;
                            }
                        }
                    }

                    if (last + 1 > Convert.ToInt64(new String('9', count)))
                    {
                        last = 0;
                    }

                    newValue = newValue.Replace(new String('#', count),
                        string.Format("{0," + count + ":" + new String('0', count) + "}", last + 1));
                }

                result = result.Replace(match.Value, newValue);
            }

            return result;
        }

        public static string ToStringFormatted(this DateTime @DateTime)
        {
            return @DateTime.ToString(DATE_FORMAT);
        }

        public static string ToStringFormatted(this DateTimeOffset @DateTimeOffset)
        {
            return @DateTimeOffset.ToString(DATE_FORMAT);
        }
    }
}

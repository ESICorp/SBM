using System;
using System.Collections.Specialized;
using System.Text;

namespace SBM.Transfer
{
    public static class StringExtensions
    {
        public static bool HasValue(this string @string, string value)
        {
            if (!string.IsNullOrEmpty(@string))
            {
                foreach (string part in @string.Split(',', ';'))
                {
                    if (string.Compare(part,value,true) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string GetValue(this string value, string field)
        {
            var coll = new NameValueCollection();
            foreach (string fieldvalue in value.Split(',',';') )
            {
                var pair = fieldvalue.Split('=');
                if (pair.Length == 2)
                {
                    coll.Add(pair[0].Trim().ToLower(), pair[1].Trim());
                }
            }

            return coll[field.ToLower()];
        }

        public static bool IsUNC(this string value)
        {
            return value.TrimStart().StartsWith(@"\\");
        }

        public static string GetShared(this string value)
        {
            if (value == null) return null;

            var arr = value.Split(new char[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length < 2) return string.Empty;

            StringBuilder build = new StringBuilder();
            build.Append(@"\\");
            build.Append(arr[0]);
            build.Append(@"\");
            build.Append(arr[1]);

            return build.ToString();
        }
    }
}

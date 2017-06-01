namespace SBM.Model
{
    public static class  StringExtension
    {
        public static string Left(this string @string, int len) 
        {
            return @string.Substring(0, @string.Length > len ? len : @string.Length);
        }
    }
}

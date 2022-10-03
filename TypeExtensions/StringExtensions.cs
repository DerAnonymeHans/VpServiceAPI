namespace VpServiceAPI.TypeExtensions.String
{
    public static class StringExtensions
    {
        public static string? SubstringSurroundedBy(this string str, string before, string after)
        {
            int beforeIdx = str.IndexOf(before);
            if (beforeIdx < 0) return null;

            int afterIdx = str.IndexOf(after, beforeIdx + before.Length);
            if(afterIdx < 0) return null;

            return str.Substring(beforeIdx + before.Length, afterIdx - (beforeIdx + before.Length));
        }

        public static string CutToLength(this string str, int maxLength)
        {
            if (maxLength > str.Length) return str;
            return str[..maxLength];
        }
    }
}

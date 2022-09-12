using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Tools
{
    public sealed class AttackDetector
    {
        private static readonly string[] XSSTags = new[] { "script", "iframe" };
        private static readonly string[] SqlInjectKeywords = new[] { "SELECT", "DELETE", "UPDATE", "CREATE", "INSERT" };
        public static void Detect(params string[] inputs)
        {
            DetectXSS(inputs);
            DetectSqlInjection(inputs);
        }
        public static void DetectXSS(params string[] inputs)
        {
            foreach(string tag in XSSTags)
            {
                foreach(string input in inputs)
                {
                    if (input.Contains($"<{tag}")) throw new AppException("Cross-Site Scripting? Das ist aber nicht nett.");
                }
            }
        }
        public static void DetectSqlInjection(params string[] inputs)
        {
            foreach (string input in inputs)
            {
                if (!input.Contains("'")) continue;
                foreach(string keyword in SqlInjectKeywords)
                {
                    if (input.Contains(keyword, System.StringComparison.OrdinalIgnoreCase)) throw new AppException("SqlInjection? Unkreativ, aber vor allem nicht nett. Die Datenbank ist wichtig.");
                }
            }
            
        }
    }
}

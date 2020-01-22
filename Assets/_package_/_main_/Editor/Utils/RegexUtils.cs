using System.Text.RegularExpressions;

namespace UPMTool
{
    public class RegexUtils
    {
        public static bool RegexMatch(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern);

            return match.Success;
        }
    }
}
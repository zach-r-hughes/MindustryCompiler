using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mindustry_Compiler
{
    public static class StringExtensions
    {

        public static string ReplaceMatch(this string txt, Match match, string newValue) =>
            txt.ReplaceSection(match.Index, match.Length, newValue);

        public static string ReplaceSection(this string txt, int start, int len, string newvalue)
        {
            string before = txt.Substring(0, start);
            string after = txt.Substring(start + len);
            return before + newvalue + after;
        }

        public static string ScanToClosing(this string code, int startOpenIndex, char open = '(', char close = ')')
        {
            int endIndex;
            return ScanToClosing(code, startOpenIndex, out endIndex, open, close);
        }
        

        public static string ScanToClosing(this string code, int startOpenIndex, out int endIndex, char open = '(', char close = ')')
        {
            int pdepth = 0;
            var sb = new StringBuilder();
            endIndex = -1;

            for (int i = startOpenIndex + 1; i < code.Length; i++)
            {
                if (code[i] == open)
                    pdepth++;
                else if (code[i] == close)
                {
                    pdepth--;
                    if (pdepth < 0)
                    {
                        endIndex = i + 1;
                        break;
                    }
                }
                sb.Append(code[i]);
            }

            return sb.ToString();
        }

        public static List<string> SplitByParamCommas(this string code)
        {
            const char open = '(', close = ')';
            int pdepth = 0;
            var sb = new StringBuilder();

            var output = new List<string>();

            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == open)
                    pdepth++;
                else if (code[i] == close)
                {
                    pdepth--;
                    if (pdepth < 0)
                    {
                        throw new Exception("Error parsing parameter list.");
                    }
                }
                else if(code[i] == ',' && pdepth == 0)
                {
                    output.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                    sb.Append(code[i]);
            }

            // Add final sb? ...
            if (sb.Length > 0)
            {
                string lastSb = sb.ToString().Trim();
                if (lastSb.Length > 0)
                    output.Add(lastSb);
            }

            // Remove typenames from each string ...
            return output;
        }
    }
}

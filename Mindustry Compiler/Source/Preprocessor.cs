using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {

        Dictionary<string, MatchEvaluator> preprocessDefines;


        /// <summary>
        /// Matches any preprocessor.
        /// </summary>
        readonly Regex rxPreprocessor = new Regex(@"^(\s*\w+\s*:)*\s*#.*$");


        /// <summary>
        /// Matches a preprocessor define.
        /// Groups: 'a', 'b'
        /// </summary>
        readonly Regex rxPreprocessorDefine = new Regex(@"^(\s*\w+\s*:)*\s*#\s*define\s(?<a>\w+)\s+(?<b>.*)\s*$");

        /// <summary>
        /// Matches a preprocessor define function definition.
        /// Groups: 'a', 'params', 'b'
        /// </summary>
        readonly Regex rxPreprocessorDefineFunction = new Regex(@"^(\s*\w+\s*:)*#\s*define\s(?<a>\w+)\((?<params>[^\)]+)\)\s+(?<b>.*)\s*$");



        void InitializePreprocessor()
        {
            preprocessDefines = new Dictionary<string, MatchEvaluator>();
        }

        bool PreprocessLine(List<string> lines, int i)
        {
            // Not a preprocessor? Skip...
            if (rxPreprocessor.IsMatch(lines[i]))
            {
                // Keep jump label ...
                string l = lines[i];
                l = PopJumpLabel(ref l);
                code.Add(l);

                return PreprocessLine_Preprocessor(lines[i]);
            }

            // Apply preprocess defines to line ...
            foreach (var pre in preprocessDefines)
                lines[i] = Regex.Replace(lines[i], pre.Key, pre.Value);

            return false;
        }

        /// <summary>
        /// Parse and add a preprocessor directive (define, etc.)
        /// </summary>
        bool PreprocessLine_Preprocessor(string l)
        { 
            
            Match match;


            // ~~~~~~~~ Define?
            match = rxPreprocessorDefine.Match(l);
            if (match.Success)
            {
                string a = match.GetStr("a");
                string b = match.GetStr("b");

                a = @"\b" + a + @"\b";

                // Add, or update?
                if (preprocessDefines.ContainsKey(a))
                    preprocessDefines[a] = e => b;
                else
                    preprocessDefines.Add(a, e => b);

                // Consume line ...
                return true;
            }


            // ~~~~~~~~ Function definition?
            match = rxPreprocessor.Match(l);


            // Return true (skip normal parse) if was preprocessor
            return true;
        }
    }
}

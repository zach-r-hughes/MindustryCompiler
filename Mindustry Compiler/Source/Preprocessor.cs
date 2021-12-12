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

        Dictionary<string, string> preprocessorDefines;
        Dictionary<string, PreprocessorFuncParamNfo> preprocessorFunctions;


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
        /// Matches a preprocessor undefine. 
        /// Groups: 'a'
        /// </summary>
        readonly Regex rxPreprocessorUndefine = new Regex(@"^(\s*\w+\s*:)*\s*#\s*undef\s(?<a>\w+)\s*$");

        /// <summary>
        /// Matches a preprocessor define function definition.
        /// Groups: 'a', 'b', 'c'
        /// </summary>
        readonly Regex rxPreprocessorDefineFunction = new Regex(@"^(\s*\w+\s*:)*\s*#\s*define\s*(?<a>\w+)\((?<b>[^\)]+)\)\s+(?<c>.*)\s*$");



        class PreprocessorFuncParamNfo
        {
            public string replacement;
            public Dictionary<string, string> paramMap = new Dictionary<string, string>();
        }



        void InitializePreprocessor(ref string source)
        {
            preprocessorDefines = new Dictionary<string, string>();
            preprocessorFunctions = new Dictionary<string, PreprocessorFuncParamNfo>();

            // Run preprocessor on source ...
            var lines = new List<string>(source.Split('\n'));
            for (int i = 0; i < lines.Count;)
            {
                if (PreprocessLine(ref lines, i))
                    lines.RemoveAt(i);
                else
                    i++;
            }
            source = string.Join("\n", lines);
        }


        bool PreprocessLine(ref List<string> lines, int i)
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
            foreach (var pre in preprocessorDefines)
                lines[i] = Regex.Replace(lines[i], pre.Key, pre.Value);

            // Apply preprocess function defs ...
            PreprocessLine_ApplyFunctions(ref lines, i);

            return false;
        }

        /// <summary>
        /// Parse and add a preprocessor directive (define, etc.)
        /// </summary>
        bool PreprocessLine_Preprocessor(string l)
        { 
            
            Match match;


            // ~~~~~~~~ Function def
            match = rxPreprocessorDefineFunction.Match(l);
            if (match.Success)
            {
                string a = match.GetStr("a");   // handle
                string b = match.GetStr("b");   // params
                var psplit = b.SplitByParamCommas();
                string c = match.GetStr("c");   // text

                var fnParamNfo = new PreprocessorFuncParamNfo();
                foreach (string p in psplit)
                {
                    string pattern = @"\b" + Regex.Escape(p) + @"\b";
                    int pidx = fnParamNfo.paramMap.Count;
                    fnParamNfo.paramMap.Add(p, pattern);
                    c = Regex.Replace(c, pattern, "{" + pidx.ToString() + "}");
                }
                fnParamNfo.replacement = c;
                preprocessorFunctions.Add(a, fnParamNfo);
                return true;
            }

            // ~~~~~~~~ Define
            match = rxPreprocessorDefine.Match(l);
            if (match.Success)
            {
                string a = match.GetStr("a");
                string b = match.GetStr("b");

                a = @"\b" + a + @"\b";

                // Add, or update?
                if (preprocessorDefines.ContainsKey(a))
                    preprocessorDefines[a] = b;
                else
                    preprocessorDefines.Add(a, b);

                // Consume line ...
                return true;
            }

            // ~~~~~~~~ Undef
            match = rxPreprocessorUndefine.Match(l);
            if (match.Success)
            {
                string a = match.GetStr("a");
                if (preprocessorDefines.ContainsKey(a))
                    preprocessorDefines.Remove(a);

                // Consume line ...
                return true;
            }

            // Return true (skip normal parse) if was preprocessor
            return true;
        }


        void PreprocessLine_ApplyFunctions(ref List<string> lines, int i)
        {
            // Apply preprocess function defs to line ...
            foreach (var pre in preprocessorFunctions)
            {
                var fnName = pre.Key;
                var fnParamNfo = pre.Value;
                string rxPattern = @"\b" + pre.Key + @"\s*(?<open>\()";
                var match = Regex.Match(lines[i], rxPattern);

                while (match.Success)
                {
                    // Get parameters ...
                    int openIndex = match.Groups["open"].Index;
                    int closeIndex;
                    string inner = lines[i].ScanToClosing(openIndex, out closeIndex);

                    // Read parameters ...
                    var args = inner.SplitByParamCommas();
                    string replacement = string.Format(fnParamNfo.replacement, args.ToArray());

                    // Replace the match ...
                    lines[i] = lines[i].ReplaceMatch(match, replacement);
                    match = Regex.Match(lines[i], rxPattern);
                }
            }
        }
    }
}

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
        public enum PreprocessorClass
        {
            Define,
            Function,
            Invalid
        }


        public PreprocessorClass ClassifyPreprocessor(out Match match, string l)
        {
            match = rxPreprocessorDefineFunction.Match(l);
            if (match.Success) return PreprocessorClass.Function;

            match = rxPreprocessorDefine.Match(l);
            if (match.Success) return PreprocessorClass.Define;

            return PreprocessorClass.Invalid;
        }

        public void ParsePreprocessorDefine(string l, Dictionary<Regex, MatchEvaluator> preprocessorMap)
        {
            Match match;
            var preprocessorClass = ClassifyPreprocessor(out match, l);
            
            switch(preprocessorClass)
            {
                case PreprocessorClass.Define:
                    {
                        string handle = match.GetStr("a");
                        string value = match.GetStr("b");

                        preprocessorMap.Add(
                            new Regex(@"\b" + handle + @"\b"),      // Handle
                            e => value                              // Replacement
                            );
                    }
                    break;

                case PreprocessorClass.Function:
                    {
                        string handle = match.GetStr("a");
                        string value = match.GetStr("b");

                        preprocessorMap.Add(
                            new Regex(@"\b" + handle + @"\b"),      // Handle
                            e => value                              // Replacement
                            );
                    }
                    break;
            }
        }
    }
}

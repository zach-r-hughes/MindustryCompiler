using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {
        Stack<string> jumpAliasProcessLineStack;

        public void InitializeAliases()
        {
            jumpAliasIndex = 0;
            intermediateValueIndex = 0;
            jumpAliasProcessLineStack = new Stack<string>();
        }

        //==============================================================================
        /// <summary>
        /// Line number alias, unique variable name generator. Used for labeling lines.
        /// </summary>
        int jumpAliasIndex;
        readonly private string jumpAliasPrefix = "__line";

        /// <summary>
        /// Returns a new, uniuque line number alias
        /// </summary>
        string getNewJumpAlias() 
        {
            return jumpAliasPrefix + (jumpAliasIndex++).ToString() + "_";
        }

        /// <summary>
        /// Removes a jump label from a string and returns it
        /// </summary>
        string PopJumpLabel(ref string str)
        {
            string label = "";
            var match = rxJumpLineTarget.Match(str);
            if (match.Success)
            {
                label += match.GetStr("alias");
                str = match.GetStr("code");
            }
            return label;
        }

        /// <summary>
        /// Scan through each line of code, and apply the finailized jump addresses
        /// </summary>
        void FinalizeAliases()
        {
            lastCode = "Finalize Aliases";
            var lineRefMap = new Dictionary<string, string>();

            // Find 'targets', save/remove prepend
            for (int i = 0; i < code.Count; i++)
            {
                var match = rxJumpLineTarget.Match(code[i]);
                while (match.Success)
                {
                    int target = i;
                    string alias = match.GetStr("alias");
                    if (alias.Contains("+")) 
                        target++;

                    lineRefMap.Add(alias.Replace(":", "").Replace("+", ""), target.ToString());
                    code[i] = match.GetStr("code").Trim();

                    // Empty line after jump alias? prepend...
                    match = rxJumpLineTarget.Match(code[i]);
                }

                // Empy line now? Remove...
                if (code[i].Length == 0)
                {
                    code.RemoveAt(i);
                    i--;
                }
            }


            string codestr = string.Join("\n", code);

            // Jump aliases ...
            foreach (var strAlias in lineRefMap)
            {
                var rxAlias = new Regex(@"\b" + strAlias.Key + @"\b");
                codestr = rxAlias.Replace(codestr, e => strAlias.Value.ToString());
            }

            // String literal aliases ...
            foreach (var strAlias in strAliasMap)
            {
                var rxAlias = new Regex(@"\b(?<a>" + strAlias.Key + @")\b");
                codestr = rxAlias.Replace(codestr, e => strAlias.Value);
            }


            code = new List<string>(codestr.Split('\n'));
        }

        //==============================================================================
        /// <summary>
        /// Intermediate, unique variable name generator. Used in Rvalue parsing (arithmetic, etc.)
        /// </summary>
        int intermediateValueIndex;
        readonly private string intermediatePrefix = "__reg";

        /// <summary>
        /// Returns a new, uniuque intermediate variable index
        /// </summary>
        int getNextIntermediateIndex() => intermediateValueIndex++;

        /// <summary>
        /// Turns a unique intermediate index into a unique variable name
        /// </summary>
        string getIntermediateName(int idx) => intermediatePrefix + (idx).ToString() + "_";

        /// <summary>
        /// Returns a new, unique variable name. Shorthand for 'getIntermediateName(getNextIntermediateIndex());'
        /// </summary>
        string getNewIntermediateName() => getIntermediateName(getNextIntermediateIndex());

        /// <summary>
        /// Converts a string like "<4>" into an intermediate variable name with index 4
        /// </summary>
        string getRefdIntermediateName(string valstr) => intermediatePrefix + valstr.Substring(1, valstr.Length - 2) + "_";
    }
}

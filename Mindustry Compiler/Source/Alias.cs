using System.Collections.Generic;

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {
        public void InitializeAliases()
        {
            
            jumpAliasIndex = 0;
            intermediateValueIndex = 0;
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
        string getNewJumpAlias(bool addOne = false) 
        {
            return jumpAliasPrefix + (jumpAliasIndex++).ToString() + "_" + (addOne ? "+" : "");
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
                label = match.GetStr("alias");
                if (!label.StartsWith("__"))
                    label = "__" + label;
                str = match.GetStr("code");
            }
            return label;
        }

        /// <summary>
        /// Scan through each line of code, and apply the finailized jump addresses
        /// </summary>
        void FinalizeAliases()
        {
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

                    lineRefMap.Add(alias.Replace("+", ""), target.ToString());
                    code[i] = match.GetStr("code").Trim();

                    // Empty line after jump alias? prepend...
                    if (code[i].Length == 0 && i < code.Count - 1)
                    {
                        code.RemoveAt(i);
                        match = rxJumpLineTarget.Match(code[i]);
                        i--;
                    }
                    else
                    {
                        match = rxJumpLineTarget.Match(code[i]);
                    }
                }
            }

            // Find references to line num aliases
            for (int i = 0; i < code.Count; i++)
            {
                var match = rxLineNumReference.Match(code[i]);
                while (match.Success)
                {
                    string alias = match.GetStr("alias").Replace("+", "");

                    // Not a line ref? skip...
                    if (lineRefMap.ContainsKey(alias))
                        code[i] = code[i].ReplaceMatch(match, lineRefMap[alias].ToString());

                    match = match.NextMatch();
                }
            }


            // Re-emplace string aliases ...
            foreach (var strAlias in strAliasMap)
            {
                var rxStrAlias = strAlias.Key;
                var original = strAlias.Value;
                
                for (int i = 0; i < code.Count; i++)
                {
                    var match = rxStrAlias.Match(code[i]);
                    while (match.Success)
                    {
                        code[i] = code[i].ReplaceMatch(match, original);
                        match = rxStrAlias.Match(code[i]);
                    }
                }
            }
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

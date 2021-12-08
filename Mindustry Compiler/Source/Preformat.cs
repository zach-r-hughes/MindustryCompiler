using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

//================================================================================
/// <summary>
/// Preformat source code before beginning compilation.
/// Remove blank lines, address comments, etc.
/// </summary>

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {
        /// <summary>
        /// String literal alias map
        /// </summary>
        Dictionary<Regex, string> strAliasMap;

        /// <summary>
        /// Remove comments, strip blank lines, etc.
        /// </summary>
        /// <param name="source"></param>
        void PreFormatSource(ref string source)
        {
            compileLineIndex = -1;
            PreFormat_StripComments(ref source);
            PreFormat_AliasStringLiterals_Input(ref source);
            PreFormat_ParseEnums(ref source);
            PreFormat_Switch(ref source);
            Preformat_IncDec(ref source);
            PreFormat_DoWhile(ref source);
            PreFormat_SingleLineControl(ref source);
            PreFormat_FormatNewlines(ref source);
        }

        /// <summary>
        /// strLiteral, unique variable name generator. Used in Rvalue parsing (arithmetic, etc.)
        /// </summary>
        int strLiteralValueIndex;
        readonly private string strLiteralPrefix = "__strlit";

        /// <summary>
        /// Returns a new, uniuque strLiteral variable index
        /// </summary>
        int getNextStrLiteralIndex() => strLiteralValueIndex++;

        /// <summary>
        /// Turns a unique strLiteral index into a unique variable name
        /// </summary>
        string getStrLiteralName(int idx) => strLiteralPrefix + (idx).ToString() + "_";

        /// <summary>
        /// Returns a new, unique variable name. Shorthand for 'getstrLiteralName(getNextstrLiteralIndex());'
        /// </summary>
        string getNewStrLiteralName() => getStrLiteralName(getNextStrLiteralIndex());


        

        //================================================================================

        /// <summary>
        /// Remove comments and multi-line comments from source
        /// </summary>
        void PreFormat_StripComments(ref string source)
        {
            lastLineClass = "Strip Comments";

            // Comments (single-line)
            source = Regex.Replace(source, @"\/\/.*(\n|$)", e => "").Trim();

            // Comments (multi-line)
            source = Regex.Replace(source, @"(\/\*)((?!\*\/)(.|\n|\r))*(\*\/)", e => "").Trim();
        }

        /// <summary>
        /// Transform post-inc/decrements into parsable '+= 1' and '-= 1'
        /// </summary>
        void Preformat_IncDec(ref string source)
        {
            lastLineClass = "Parse Increments/Decrements";

            // Fix 'cell' increment/decrement ...
            var rxMemIncDec = new Regex(@"\b(?<a>(?!\b=\b).*)(?<b>\+\+|--)\s*;?\s*(\n|$)");
            source = rxMemIncDec.Replace(source, e => e.GetStr("a") + " " + e.GetStr("b").Substring(0, 1) + "= 1; \n");
        }

        /// <summary>
        /// Format new-lines (no blanks, semicolons = newline, etc.)
        /// </summary>
        void PreFormat_FormatNewlines(ref string source)
        {
            // Remove 'return carraige' character
            source = source.Replace("\r", "");

            // Remove blank lines ...
            var rxBlankLine = new Regex(@"\n\s*\n");
            var match = rxBlankLine.Match(source);
            while (match.Success)
            {
                source = source.ReplaceMatch(match, "\n");
                match = rxBlankLine.Match(source);
            }
            if (source[0] == '\n') source = source.Substring(1);
        }

        /// <summary>
        /// Convert all string literals into alias names.
        /// Fixes problems where strings contain code-like text.
        /// After compilation, string literals are re-emplaced.
        /// </summary>
        public void PreFormat_AliasStringLiterals_Input(ref string source)
        {
            Match match;
            strAliasMap = new Dictionary<Regex, string>();


            // Asm literals
            var rxAsmPreformat = new Regex(@"(\n|^)(?<indent>\s*)asm\s*\((?<v>[^#].*)\s*\)\s*;?\s*(\n|$)");
            match = rxAsmPreformat.Match(source);
            while(match.Success)
            {
                string str = match.GetStr("v");
                string alias = getNewStrLiteralName();
                strAliasMap.Add(new Regex(@"\b" + alias + @"\b"), str);
                source = source.ReplaceMatch(match, "\n" + match.GetStr("indent") + "asm(#" + alias + ");\n");
                match = rxAsmPreformat.Match(source);
            }
            source = source.Replace("asm(#", "asm(");


            // String literals
            var rxStringLiteral = new Regex(@"(?<str>(?!(\\""))""(?!(\\"")).*"")");
            match = rxStringLiteral.Match(source);
            while (match.Success)
            {
                string str = match.GetStr("str");
                string alias = getNewStrLiteralName();
                strAliasMap.Add(new Regex(@"\b" + alias + @"\b"), str);
                source = source.ReplaceMatch(match, alias);
                match = rxStringLiteral.Match(source);
            }


            // Special literals ("@spore-pod", etc.)
            foreach (var rxLRS in literalRemapSpecial)
            {
                match = rxLRS.Match(source);
                while (match.Success)
                {
                    string str = match.Value;
                    string alias = getNewIntermediateName();
                    strAliasMap.Add(new Regex(@"\b" + alias + @"\b"), str);
                    source = source.ReplaceMatch(match, alias);
                    match = rxLRS.Match(source);
                }
            }
        }

        /// <summary>
        /// Changes single line conditionals/branches/loops into parseable '{'+'}' multi-line
        /// </summary>
        public void PreFormat_SingleLineControl(ref string source)
        {
            Match match;

            // ~~~~~~~~~~ Condition one-liners (if, else if, while)
            // Groups: 'type', 'rest'
            var rxIfElseIfWhileDo = new Regex(@"\b(?<type>if|else\s*if|while)\s*(?<rest>\((\n|[^\{;])+;)");
            match = rxIfElseIfWhileDo.Match(source);
            while (match.Success)
            {
                string type = match.GetStr("type");
                string cond;
                var restGroup = match.Groups["rest"];
                string rest = restGroup.Value;

                // Scan one-liner condition/code ...
                int parenOpen = 0;
                int parenClose;
                cond = "(" + rest.ScanToClosing(parenOpen, out parenClose, '(', ')').Trim() + ")";
                string oneLiner = rest.Substring(parenClose + 1).Trim();


                // Now we have the branch-type, condition, and code. Rebuild...
                string newSource = type + cond + "\n{\n\t" + oneLiner + "\n}\n";
                source = source.ReplaceSection(match.Index, restGroup.Index + restGroup.Length - match.Index, newSource);
                match = rxIfElseIfWhileDo.Match(source);
            }


            // ~~~~~~~~~~ No-condition one-liners (else, do-while)
            // Groups: 'type', 'rest'
            var rxElse = new Regex(@"\b(?<type>else|do)\s*(?<rest>(\n|[^\{;])+;)");
            match = rxElse.Match(source);
            while (match.Success)
            {
                string type = match.GetStr("type");
                var restGroup = match.Groups["rest"];
                string rest = restGroup.Value;

                // Now we have the branch-type, condition, and code. Rebuild...
                string newSource = type + "\n{\n\t" + rest + "\n}\n";
                source = source.ReplaceSection(match.Index, restGroup.Index + restGroup.Length - match.Index, newSource);
                match = rxElse.Match(source);
            }


            // ~~~~~~~~~~ For one-liner
            // Groups: 'type', 'rest'
            var rxFor = new Regex(@"\b(?<type>for)\s*(?<rest>\([^{;]*;[^{;]*;[^{;]+);");
            match = rxFor.Match(source);
            while (match.Success)
            {
                string type = match.GetStr("type");
                string cond;
                var restGroup = match.Groups["rest"];
                string rest = restGroup.Value;

                // Scan one-liner condition/code ...
                int parenOpen = 0;
                int parenClose;
                cond = "(" + rest.ScanToClosing(parenOpen, out parenClose, '(', ')').Trim() + ")";
                string oneLiner = rest.Substring(parenClose + 1).Trim();


                // Now we have the branch-type, condition, and code. Rebuild...
                string newSource = type + cond + "\n{\n\t" + oneLiner + "\n}\n";
                source = source.ReplaceSection(match.Index, restGroup.Index + restGroup.Length - match.Index, newSource);
                match = rxIfElseIfWhileDo.Match(source);
            }
        }

        /// <summary>
        /// Preprocess enum defines. Create 'enum' aliases, add to 'enumAliasMap'
        /// </summary>
        public void PreFormat_ParseEnums(ref string source)
        {
            var enumAliasMap = new Dictionary<Regex, string>();

            var rxEnumFind = new Regex(@"\benum\s*(?<name>\w+)\s*($|\n)?\s*(?<open>{)");
            var match = rxEnumFind.Match(source);

            // For each enum define, create 'startup' definition + add to map ...
            while (match.Success)
            {

                // Read definitions ...
                string enumName = match.GetStr("name");
                int enumEndIndex;
                string enumInner = source.ScanToClosing(match.Groups["open"].Index, out enumEndIndex, '{', '}');
                enumInner = Regex.Replace(enumInner, @"(\s|\n)*", e => "");

                // Split definitions ...
                var split = enumInner.Split(',');
                int outputVal = -1;
                for (int i = 0; i < split.Length; i++)
                {
                    string name = split[i];
                    if (name.Length == 0)
                        continue;

                    // Define remap values ...
                    if (name.Contains("="))
                    {
                        int equalIdx = name.IndexOf('=');
                        string val = name.Substring(equalIdx + 1);
                        name = name.Substring(0, equalIdx);

                        // 'val' is constant?
                        if (!IsRvalNumericConstant(val) || val.Contains('.'))
                            throw new Exception("Enum must be a constant integer");

                        outputVal = int.Parse(val);
                    }
                    else
                        outputVal++;

                    // Add alias mappings (local and namespace style)...
                    string pattern = string.Format(@"\b(({0}\s*::\s*{1})|({1}))\b", enumName, name);
                    enumAliasMap.Add(new Regex(pattern), outputVal.ToString());
                }

                source = source.ReplaceSection(match.Index, enumEndIndex - match.Index, "");
                match = rxEnumFind.Match(source);
            }

            // Replace 'in source' references to enum with handle ...
            foreach (var enumRef in enumAliasMap)
                source = enumRef.Key.Replace(source, e => enumRef.Value);
        }

        /// <summary>
        /// Pre-format switch statements. Pre-converts switch statements into jumps.
        /// </summary>
        public void PreFormat_Switch(ref string source)
        {
            int switchIndex = 0;
            var rxSwitch = new Regex(@"\bswitch\s*(?<v>\()");
            var match = rxSwitch.Match(source);

            while(match.Success)
            {
                // Find switch value (in parenthesis)
                string value = source.ScanToClosing(match.Groups["v"].Index);
                string defaultLabel = "";
                string endSwitchJumpAlias = string.Format("__sw{0}_end_", switchIndex);

                // Find curly-brace open ...
                int switchOpenIndex = source.IndexOf('{', match.Index + value.Length + 1);
                int switchCloseIndex;
                string switchInner = source.ScanToClosing(switchOpenIndex, out switchCloseIndex, '{', '}');

                var rxCaseLabelPattern = @"\b(case\s*(?<v>\w+)|(?<v>default))\s*:";
                var caseLabelGroups = Regex.Matches(switchInner, rxCaseLabelPattern);
                var caseInner = Regex.Split(switchInner, rxCaseLabelPattern);

                // Create alias for labels
                var caseJumpAlias = new string[caseLabelGroups.Count];
                var caseLabel = new string[caseLabelGroups.Count];
                int defaultIndex = -1;
                {
                    var seenLabels = new HashSet<string>();
                    for (int i = 0; i < caseJumpAlias.Length; i++)
                    {
                        string caseText = caseLabelGroups[i].GetStr("v");

                        if (seenLabels.Contains(caseText))
                            throw new Exception("Switch case label cannot appear more than once.");

                        else if (caseText == "default")
                        {
                            if (defaultLabel.Length > 0)
                                throw new Exception("Multiple 'default' labels in switch case.");
                            defaultLabel = caseJumpAlias[caseJumpAlias.Length - 1];
                            defaultIndex = i;
                        }

                        else if (!IsRvalNumericConstant(caseText))
                            throw new Exception("Switch case labels must be constant/enum.");

                        // Set case label ...
                        caseJumpAlias[i] = string.Format("__sw{0}_c{1}", switchIndex, i);
                        caseLabel[i] = caseText;
                    }
                }


                // Replace 'case x:' with jump alias...
                for (int i = caseJumpAlias.Length; --i >=0;)
                    switchInner = switchInner.ReplaceMatch(caseLabelGroups[i], caseJumpAlias[i] + ":");

                // Replace 'break' with jump to alias
                string breakJumpAsm = BuildCode(
                    "set",                  // Op
                    "@counter",             // Destination
                    endSwitchJumpAlias      // Value
                    );
                switchInner = Regex.Replace(switchInner, @"\bbreak;", e => 
                    "asm(" + breakJumpAsm + ");");

                // Put 'default' jump last
                if (defaultIndex != -1)
                {
                    string tmp;

                    tmp = caseJumpAlias[caseJumpAlias.Length - 1];
                    caseJumpAlias[caseJumpAlias.Length - 1] = caseJumpAlias[defaultIndex];
                    caseJumpAlias[defaultIndex] = tmp;

                    tmp = caseLabel[caseLabel.Length - 1];
                    caseLabel[caseLabel.Length - 1] = caseLabel[defaultIndex];
                    caseLabel[defaultIndex] = tmp;
                }


                // Create condition 'jump' code ...
                var preJumpCode = new List<string>();
                for (int i = 0; i < caseJumpAlias.Length - (defaultIndex != -1 ? 1 : 0); i++)
                {
                    string jumpToCaseAsm = BuildCode(
                        "jump",                 // Op
                        caseJumpAlias[i],       // Line num
                        "equal",                // Comparision
                        value,                  // Operand 1
                        caseLabel[i]            // Operand 2
                        );
                    preJumpCode.Add("asm(" + jumpToCaseAsm + ");");
                }

                // Has 'default'? Go there - else go end ...
                int defi = caseJumpAlias.Length - 1;
                string noMatchTarget = defaultIndex == -1 ? endSwitchJumpAlias : caseJumpAlias[defi];
                preJumpCode.Add("asm(" + BuildCode(
                    "set",                  // Op
                    "@counter",             // Destination
                    noMatchTarget          // Line num
                    ) + ");");

                // Replace 'source'...
                string newSwitchCode = 
                    string.Join("\n", preJumpCode) + "\n" +
                    switchInner + "\n" +
                    endSwitchJumpAlias + ":";

                source = source.ReplaceSection(match.Index, switchCloseIndex - match.Index, newSwitchCode);

                switchIndex++;
                match = rxSwitch.Match(source);
            }
        }

        /// <summary>
        /// Pre-arranges do-while loops with their condition.
        /// </summary>
        public void PreFormat_DoWhile(ref string source)
        {
            var rxDoWhile = new Regex(@"\b(?<a>do)(\s*|\n)*(?<open>{)");
            var rxWhileCond = new Regex(@"(\s*|\n)*while\s*(?<open>\()[^;]*;");
            var match = rxDoWhile.Match(source);

            while (match.Success)
            {
                // Scan to closing brace ...
                int bodyStart = match.Groups["open"].Index;
                int bodyEnd;
                string body = source.ScanToClosing(bodyStart, out bodyEnd, '{', '}');

                // Grab the 'while' condition...
                var matchWhile = rxWhileCond.Match(source, bodyEnd);
                int condStart = matchWhile.Groups["open"].Index;
                int condEnd;
                string cond = source.ScanToClosing(condStart, out condEnd, '(', ')');


                // Remove while+condition from source ...
                source = source.ReplaceMatch(matchWhile, "");

                // Add 'condition' to 'do'
                source = source.ReplaceMatch(match.Groups["a"], "do while(" + cond + ")");

                // scan forward for the 'while (condition)' part ...
                match = rxDoWhile.Match(source);
            }



            // Single line 'do-while'....
            var rxDoWhileOneLine = new Regex(@"(\s|\n)*(?<do>do(\s*|\n)*(?<fn>[^;]*);(\s|\n)*while(\s|\n)*(?<open>\([^;]*);)");
            match = rxDoWhileOneLine.Match(source);

            while (match.Success)
            {
                int condOpen = match.Groups["open"].Index;
                int condClose;
                string cond = source.ScanToClosing(condOpen, out condClose);
                string cmd = match.Groups["fn"].Value.Trim();

                source = source.ReplaceMatch(match.Groups["do"],
                    "do while(" + cond + ")" +
                    " \n{\n\t" + cmd + (cmd.EndsWith(";") ? "" : ";") +
                    "\n}\n");

                match = rxDoWhileOneLine.Match(source);
            }
        }
    }
}

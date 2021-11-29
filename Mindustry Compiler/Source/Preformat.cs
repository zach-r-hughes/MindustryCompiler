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
            // Comments (single-line)
            source = Regex.Replace(source, @"\/\/.*(\n|$)", e => "").Trim();

            // Comments (multi-line)
            source = Regex.Replace(source, @"(\/\*)((?!\*\/)(.|\n|\r))*(\*\/)", e => "").Trim();
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
        }

        /// <summary>
        /// Convert all string literals into alias names.
        /// Fixes problems where strings contain code-like text.
        /// After compilation, string literals are re-emplaced.
        /// </summary>
        public string PreFormat_AliasStringLiterals_Input(ref string source)
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
            return source;
        }

    }
}

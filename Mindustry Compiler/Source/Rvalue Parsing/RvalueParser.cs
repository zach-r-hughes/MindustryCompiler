using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;


/// <summary>
/// Parses r-values into single variables.
/// Handles math, function calls, arithmetic, etc.
/// </summary>
namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {
        Dictionary<string, string> dotPropertyRemapSpecial = new Dictionary<string, string>()
        {
            { "type", "config" },
        };

        List<Regex> literalRemapSpecial = new List<Regex>()
        {
            new Regex(@"@?spore-pod"),
            new Regex(@"@?blast-compound"),
            new Regex(@"@?phase-fabric"),
            new Regex(@"@?surge-alloy"),
        };

        //==============================================================================
        /// <summary>
        /// Takes in an R-value string, and returns a variable name holding its output value
        /// </summary>
        public string ParseRval(string rval)
        {
            rval = ParseRval_BoolLogic(rval);
            rval = ParseRval_DotProperty(rval);
            rval = ParseRval_FunctionCalls(rval);
            rval = ParseRval_MemLoad(rval);
            rval = ParseRval_Arithmetic(rval);
            return rval;
        }

        /// <summary>
        /// Parses boolean logic.
        /// Example: 'x == 3 && y - 2 < 6 * z'
        /// </summary>
        public string ParseRval_BoolLogic(string rval)
        {
            // And/Or
            var rxHasBool = new Regex(@"(?<a>.*)\s*(&&|\|\||==|!=|<|>|>=|<=)\s*(?<b>.*)");
            var match = rxHasBool.Match(rval);

            if (match.Success)
                rval = ParseBool(rval);

            return rval;
        }

        /// <summary>
        /// Parses an object dot property (sensor).
        /// Example: 'container1.totalItems'
        /// </summary>
        public string ParseRval_DotProperty(string rval)
        {
            // Convert '.type' to '.config' ...
            var rxRvalConfig = new Regex(@"(?<obj>(\w+))\.@?(?<prop>\w+)");
            var match = rxRvalConfig.Match(rval);

            while (match.Success)
            {
                string obj = match.GetStr("obj").Trim();
                string destination = getNewIntermediateName();
                string prop = match.GetStr("prop").Trim();

                // obj is actually a number eg("123.4")?
                if (obj.Length == 0 || char.IsDigit(obj[0]))
                    return rval;

                // Special dot property remappings
                if (dotPropertyRemapSpecial.ContainsKey(prop))
                    prop = dotPropertyRemapSpecial[prop];

                // Prepend '@' for property
                if (!prop.StartsWith("@"))
                    prop = "@" + prop;

                string asm = BuildCode(
                    "sensor",               // Op
                    destination,            // Destination
                    obj,                    // Building
                    prop                    // Property
                    );
                code.Add(asm);

                rval = rval.ReplaceSection(match.Index, match.Length, destination);
                match = rxRvalConfig.Match(rval);
            }

            return rval;
        }

        /// <summary>
        /// Parses function calls. Returns variable name of result.
        /// </summary>
        public string ParseRval_FunctionCalls(string rval)
        {
            var match = rxFunctionRval.Match(rval);
            int curIndex = 0;

            // ~~~~~~~~ Function calls
            while (match.Success)
            {
                if (match.Index >= curIndex)
                {
                    string fnName = match.GetStr("fname");
                    int nextIndex;
                    string inner = rval.ScanToClosing(match.Index + match.Length - 1, out nextIndex, '(', ')');

                    // Get fn info object from text...
                    var fnObj = new FunctionInfo(fnName, inner);
                    string destination;

                    // Built in function
                    if (builtInFunctions.Contains(fnObj))
                    {
                        if (fnName == "main")
                            throw new Exception("Cannot call main function manually.");

                        destination = getNewIntermediateName();
                        var pvals = inner.SplitByParamCommas();
                        string a = pvals.Count >= 1 ? ParseRval(pvals[0]) : "a";
                        string b = pvals.Count >= 2 ? ParseRval(pvals[1]) : "b";

                        string asm = BuildCode(
                            "op",
                            fnName,                 // Function name
                            destination,            // Destination
                            a,                      // Operand 1
                            b                       // Operand 2
                            );
                        code.Add(asm);
                    }

                    // Custom user function
                    else
                    {
                        destination = getNewIntermediateName();
                        var fnCode = CreateFunctionCall(fnName, inner);
                        code.AddRange(fnCode);

                        string setToRetAsm = BuildCode(
                            "set",                  // Op
                            destination,            // Destination
                            retValName              // Value
                            );
                        code.Add(setToRetAsm);
                    }
                    rval = rval.ReplaceSection(match.Index, nextIndex - match.Index, destination);
                    curIndex = nextIndex;
                }
                match = rxFunctionRval.Match(rval);
            }
            return rval;
        }

        /// <summary>
        /// Parse values which need to be loaded from memory cells.
        /// </summary>
        public string ParseRval_MemLoad(string rval)
        {
            var match = rxMemLoad.Match(rval);

            while(match.Success)
            {
                string lhs = match.GetStr("a");
                string destination = getNewIntermediateName();

                int closingIndex;
                string inner = match.GetStr("inner").ScanToClosing(0, out closingIndex, '[', ']');               
                inner = ParseRval(inner);

                // Swap 'cell' with 'cell1' ...
                if (lhs == "cell")
                    lhs = "cell1";

                string asm = BuildCode(
                    "read",             // Op
                    destination,        // Destination
                    lhs,                // Cell
                    inner               // Index
                    );

                code.Add(asm);

                rval = rval.ReplaceMatch(match, destination);
                match = rxMemLoad.Match(rval);
            }

            return rval;
        }

        /// <summary>
        /// Parses the product of an arithmetic. Follows PEMDAS.
        /// Example: 'a + b / 2 + 9'
        /// </summary>
        public string ParseRval_Arithmetic(string rval)
        {
            rval = rval.Replace(" ", "");

            // ~~~~~ Convert r-vals into alias (letters) ...
            var rvalAliasMap = new Dictionary<char, string>();
            {
                var rxValueAlias = new Regex(@"@?[a-zA-Z0-9\[\]\._]+");
                var rxMatch = rxValueAlias.Match(rval);

                char alias = 'a';
                string rvalNew = "";
                int lastIndex = 0;
                int numMatches = 0;

                
                while (rxMatch != null && rxMatch.Success)
                {
                    rvalNew += rval.Substring(lastIndex, rxMatch.Index - lastIndex) + alias.ToString();
                    rvalAliasMap[alias] = rxMatch.Value;

                    lastIndex = rxMatch.Index + rxMatch.Length;
                    rxMatch = rxMatch.NextMatch();
                    alias++;
                    numMatches++;
                }

                // Special case: single match, no r-value parsing)
                if (numMatches <= 1) 
                    return rval;

                rval = rvalNew;
            }
            rval = PrefixConverter.infixToPrefix(rval);



            // ~~~~~~ Convert r-values into discrete instructions
            // Regex - splits into groups:
            //      op, v1, v2
            // also group 'all' which encapsulates entire expression
            var rxPrefixExtract = new Regex(@"(?<all>(?<op>[+\-*/%^<>=!]{1})(?<v1>([a-zA-Z$]|(<[0-9]+>)))(?<v2>([a-zA-Z$]|(<[0-9]+>))))");

            Match rxPrefixMatch = rxPrefixExtract.Match(rval);
            while (rxPrefixExtract != null && rxPrefixMatch.Success)
            {
                string operation = rxPrefixMatch.GetStr("op");
                int destIndex = getNextIntermediateIndex();
                string dest = getIntermediateName(destIndex);
                string value1 = rxPrefixMatch.GetStr("v1");
                string value2 = rxPrefixMatch.GetStr("v2");

                // Operation to mindustry op
                operation = opMap[operation];

                // values are previously parsed?
                if (value1.StartsWith("<")) value1 = getRefdIntermediateName(value1);
                else value1 = rvalAliasMap[value1[0]];

                if (value2.StartsWith("<")) value2 = getRefdIntermediateName(value2);
                else value2 = rvalAliasMap[value2[0]];


                // Convert each operation into an rvalue instruction
                string asm = BuildCode(
                    "op",
                    operation,          // Operation
                    dest,               // Destination
                    value1,             // Operand 1
                    value2              // Operand 2
                    );

                code.Add(asm);
                rval = rval.Substring(0, rxPrefixMatch.Index) +
                    "<" + destIndex + ">" +
                    rval.Substring(rxPrefixMatch.Index + rxPrefixMatch.GetStr("all").Length);
                rxPrefixMatch = rxPrefixExtract.Match(rval);
            }

            // Return the intermediate variable name which has the output rval
            return getRefdIntermediateName(rval);
        }
    }
}

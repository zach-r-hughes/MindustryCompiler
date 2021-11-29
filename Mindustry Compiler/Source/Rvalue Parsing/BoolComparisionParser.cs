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
        /// <summary>
        /// Parses bool logic ('x == 3 && y == 1').
        /// Optionally outputs instructions to a list (or just 'code' if null)
        /// Returns: a variable name holding the parsed rvalue name
        /// </summary>
        string ParseBool(string line)
        {
            // Preprocess whitespace ...
            line = Regex.Replace(line, @"\s+", e => " ").Trim();

            var rxBoolLogic = new Regex(@"(?<comparison>[^&\|]*)(?<andor>&&|\|\|)?");
            var match = rxBoolLogic.Match(line);
            if (!match.Success)
                throw new Exception("Comparison invalid");

            var boolValNames = new List<string>();
            var andorValues = new List<string>();

            int curIndex = 0;
            while(match.Success && match.HasGroup("comparison") && match.Length > 0)
            {
                if (match.Index >= curIndex)
                {
                    string comp = match.GetStr("comparison").Trim();
                    if (comp.Length == 0) break;
                    string andor = match.GetStr("andor").Trim();

                    boolValNames.Add(ParseBoolComparisionToIntermediate(comp));
                    if (andor.Length > 0) andorValues.Add(andor);
                }

                curIndex = match.Index + match.Length;
                match = match.NextMatch();
            }

            // Combine bool logic (and/or)
            if (andorValues.Count > 0)
            {
                string andorDest = getNewIntermediateName();
                for (int i = 0; i < andorValues.Count; i++)
                {
                    string boolOp = andorValues[i] == "&&" ? "land" : "or";
                    string a = i == 0 ? boolValNames[i] : andorDest;
                    string b = boolValNames[i + 1];

                    string andorAsm = BuildCode(
                        "op",           // Op
                        boolOp,         // Bool op (and/or)
                        andorDest,      // Destination
                        a,              // Operand 1
                        b               // Operand 2
                        );
                    code.Add(andorAsm);
                }
                boolValNames.Add(andorDest);
            }

            return boolValNames.Last();
        }

        /// <summary>
        /// Parses a single comparison ('x - 2 == y + 5').
        /// Optionally outputs instructions to a list (or just 'code' if null)
        /// Returns an R-value intermediate variable name.
        /// </summary>
        string ParseBoolComparisionToIntermediate(string inner)
        {
            // Split 'x == 3' into 'a', 'b', 'c'
            var rxIfCondSplit = new Regex(@"(?<a>(?!==|<=|>=|<|>|!=).*)(?<b>==|<=|>=|<|>|!=)(?<c>(?!==|<=|>=|<|>|!=).*)");
            var match = rxIfCondSplit.Match(inner);

            string destination = getNewIntermediateName();
            string op1 = match.GetStr("a");
            string comp = match.GetStr("b");
            string op2 = match.GetStr("c");

            comp = comparisonMap[comp];
            op1 = ParseRval(op1);
            op2 = ParseRval(op2);

            string asm = BuildCode(
                "op",           // Op
                comp,           // Comp type
                destination,    // Destination
                op1,            // Operand 1
                op2             // Operand 2
                );
            code.Add(asm);
            return destination;
        }
    }
}

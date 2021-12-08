using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


/// <summary>
/// The if stack helps linking together 'If/Else If/Else' statements.
/// </summary>

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {
        int ifStackDepth;
        int ifStackEndIfAliasIndex;
        string ifStackLastNextIfAlias;
        string ifStackLastEndIfAlias;

        /// <summary>
        /// Runs before a 'compile'
        /// </summary>
        public void InitializeIfStack()
        {
            lastLineClass = "Initialize If Stack";
            ifStackDepth = 0;
            ifStackEndIfAliasIndex = 0;
            ifStackLastNextIfAlias = "";
            ifStackLastEndIfAlias = "";
        }

        /// <summary>
        /// Register an 'if' statement.
        /// Takes a unique jump alias (remembers it).
        /// </summary>
        public void IfStack_PushHistory(string ifNextAlias)
        {
            ifStackDepth++;
            ifStackLastNextIfAlias = ifNextAlias;
        }

        public void Branch_AddJumpAsmOptimized(string jumpToAlias, string conditionInner, bool invertCondition = false)
        {
            // Parse the condition ...
            int startCodeCount = code.Count;
            string boolRval = ParseBool(conditionInner);

            // Check if previous instruction is a simple comparison
            var rxSimpleCompare = new Regex(@"^op (?<op>\w+) (?<dest>\w+) (?<rest>.*)$");
            var match = rxSimpleCompare.Match(code[code.Count - 1]);
            var op = match.GetStr("op");

            // ~~~~~~~~ Steal last comparison?
            if (startCodeCount < code.Count && op.Length > 0 && compMapAsmToInverse.ContainsKey(op))
            {
                op = invertCondition ? op : compMapAsmToInverse[op];
                string rest = match.GetStr("rest");
                string asm = BuildCode(
                    "jump",                         // Op
                    jumpToAlias,                    // Line num
                    op,                             // Comp type
                    rest                            // Operand 1 & 2
                    );
                code[code.Count - 1] = asm;
            }

            // ~~~~~~~~ Use bool r-value ...
            else
            {
                string asm = BuildCode(
                    "jump",                         // Op
                    jumpToAlias,                    // Line num
                    "notEqual",                     // Comp type
                    boolRval,                       // Operand 1
                    "true"                          // Operand 2
                    );
                code.Add(asm);
            }
        }

        public string Branch_InvertConditionAndReplaceJump(string code, string jumpTo)
        {
            var rxInvCond = new Regex(@"jump (?<jump>\w+) (?<cond>\w+)");
            var match = rxInvCond.Match(code);

            var jumpGroup = match.Groups["jump"];
            var condGroup = match.Groups["cond"];

            code = code.ReplaceMatch(condGroup, compMapAsmToInverse[condGroup.Value]);
            code = code.ReplaceMatch(jumpGroup, jumpTo);
            return code;
        }

        /// <summary>
        /// At the end of an 'if' statement (not an else if).
        /// Generates a new 'end if' statement in case of upcoming else/else-if's.
        /// </summary>
        public void IfStack_PopHistory()
        {
            ifStackDepth--;

            // Make a new 'endif' alias (in case an 'else if' is coming)
            ifStackLastEndIfAlias = "__endif_" + (++ifStackEndIfAliasIndex).ToString() + "_"; ;
        }

        /// <summary>
        /// Removes previous jump aliases from the last if block.
        /// They are repositioned after adding 'skip next else block' asm.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string IfStack_PopIfAliases(string code) =>
            Regex.Replace(code, @"(" + ifStackLastEndIfAlias + "|" + ifStackLastNextIfAlias + @")\+?\s*:", e => "");
    }
}

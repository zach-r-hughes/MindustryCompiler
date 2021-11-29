using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        /// <summary>
        /// At the end of an 'if' statement (not an else if).
        /// Generates a new 'end if' statement in case of upcoming else/else-if's.
        /// </summary>
        public void IfStack_PopHistory()
        {
            ifStackDepth--;

            // Make a new 'endif' alias (in case an 'else if' is coming)
            ifStackLastEndIfAlias = ifStackLastEndIfAlias = "__endif_" + (++ifStackEndIfAliasIndex).ToString() + "_+"; ;
        }

        /// <summary>
        /// Removes previous jump aliases from the last if block.
        /// They are repositioned after adding 'skip next else block' asm.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string IfStack_PopEndIfAliasForElseIf(string code) =>
            code
                .Replace(ifStackLastEndIfAlias + ":", "")
                .Replace(ifStackLastNextIfAlias + ":", "");
    }
}

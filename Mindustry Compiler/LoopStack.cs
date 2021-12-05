using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {
        public Stack<string> __loopBreakStack;

        public void InitializeLoopBreakStack()
        {
            __loopBreakStack = new Stack<string>();
        }

        void LoopBreakStackPush(string breakJumpAlias) => 
            __loopBreakStack.Push(breakJumpAlias.Replace("+", ""));

        string LoopBreakStackPop() => 
            __loopBreakStack.Pop();

        string LoopBreakStackGetTop() => 
            __loopBreakStack.Peek();

    }
}

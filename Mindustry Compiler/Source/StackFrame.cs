using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {

        public void InitalizeStackFrames()
        {
            baseFrame = new StackFrame(this, null);
            stackFrames = new Stack<StackFrame>();
            stackFrames.Push(baseFrame);
        }

        public class StackFrame
        {
            public List<string> parentCode;
            public List<string> code = new List<string>();
            public Dictionary<string, string> data = new Dictionary<string, string>();
            public int parentStartIndex = 0;

            /// <summary>
            /// Called before add code to parent. Args: this, parent
            /// </summary>
            public Action<StackFrame> EndAction_PreDumpToParent = null;

            /// <summary>
            /// Called after add code to parent. Args: this, parent
            /// </summary>
            public Action<StackFrame> EndAction = null;



            public StackFrame(MindustryCompilerForm frm, List<string> parentCode)
            {
                parentStartIndex = parentCode != null ? parentCode.Count : 0;
                this.parentCode = parentCode;
                frm.code = this.code;
            }

            /// <summary>
            /// Dumps all instructions onto the other frame's code. 
            /// Returns the start/end index in the other frame.
            /// </summary>
            /// <param name="parent"></param>
            /// <returns></returns>
            public void EndFrame(MindustryCompilerForm frm)
            {
                // Finalize action ?
                if (EndAction_PreDumpToParent != null)
                    EndAction_PreDumpToParent(this);

                // Dump code into parent frame ...
                for (int i = 0; i < code.Count; i++)
                    parentCode.Add(code[i]);

                // End action ?
                if (EndAction != null)
                    EndAction(this);

                code.Clear();

                frm.code = parentCode;
            }
        }
    }
}

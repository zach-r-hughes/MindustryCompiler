using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindustry_Compiler
{
    /// <summary>
    /// Remaps (temporarily) all code output into 'tempCodeList'.
    /// On dispose, un-remaps back to original.
    /// Usage: 'using (var tir = new TemporaryInstructionRetarget(this, tempInstructions)) ...'
    /// </summary>
    class TemporaryInstructionRetarget : IDisposable
    {
        MindustryCompilerForm frm;
        List<string> codeOriginal;
        public TemporaryInstructionRetarget(MindustryCompilerForm frm, List<string> tempCodeList)
        {
            this.frm = frm;
            codeOriginal = frm.code;
            frm.code = tempCodeList;
        }

        public void Dispose()
        {
            frm.code = codeOriginal;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {
        int compileLineIndex = 0;
        string lastLineClass = "None";
        string lastPart = "None";
        bool isNextStackLinked = false;


        //==============================================================================
        public string Compile(string source)
        {
            try
            {
                // ~~~~~~ Ensure curly-braces are on discrete lines ...
                source = source
                .Replace("\r", "")
                .Replace("\n\n", "\n")
                .Replace("{", "\n{\n").Replace("\n\n{", "\n{").Replace("{\n\n", "{\n")
                .Replace("}", "\n}\n").Replace("\n\n}", "\n}").Replace("}\n\n", "}\n");


                var preprocessDefines = new Dictionary<Regex, MatchEvaluator>();
                doPrintFlush = false;

                // ~~~~~~ Create base program frame, and stack ...
                compileLineIndex = -1;
                lastPart = "Initialize";
                PreFormatSource(ref source);
                InitializeAliases();
                InitalizeStackFrames();
                InitializeFunctions(ref source);
                InitializeIfStack();


                // ~~~~~~ Process lines
                lines = new List<string>();
                lines.AddRange(source.Split('\n'));

                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i] = lines[i].Trim();
                    compileLineIndex = i;


                    // Preprocessor?
                    var preprocMatch = rxPreprocessor.Match(lines[i]);
                    if (preprocMatch.Success)
                    {
                        ParsePreprocessorDefine(lines[i], preprocessDefines);
                        continue;
                    }
                    else if (preprocessDefines.Count > 0)
                    {
                        // Replace define keywords
                        foreach (var pair in preprocessDefines)
                            lines[i] = pair.Key.Replace(lines[i], pair.Value);
                    }

                    // Actually process line ...
                    ProcessLine(lines[i]);
                }

                // Printed something? ...
                if (doPrintFlush)
                    code.Insert(code.Count - 1, BuildCode("printflush", "message1"));

                FinalizeFunctions();
                FinalizeAliases();
            }
            catch (Exception e)
            {
                var err = new StringBuilder();
                err.AppendLine("Compilation Error:")
                    .Append("  Line# ").Append(compileLineIndex).Append(":  ").Append(lastPart)
                    .Append("  Parse Type: ").Append(lastLineClass).Append("\n\n")
                    .Append(e.Message).Append("\n\n")
                    .Append(e.StackTrace);

                txtAsm.Text = "";
                txtCompileMsg.Text = err.ToString();
                txtCompileMsg.ForeColor = Color.Red;
                return "";
            }

            txtCompileMsg.Text = "Compilation successful";
            txtCompileMsg.ForeColor = Color.DarkGreen;

            // Copy asm to clipboard ...
            return String.Join("\n", code.ToArray());
        }



        


        //==============================================================================
        public void ProcessLine(string l)
        {
            // Pop jump label
            int codeCountStart = code.Count;
            var codeListStart = code;
            string lineJumpLabel = PopJumpLabel(ref l);
            
            lineClass = GetCodeLineClass(l);
            lastLineClass = Enum.GetName(typeof(LineClass), lineClass);
            lastPart = l;
            Console.WriteLine("Parse line " + (compileLineIndex + 1).ToString() + ": " + lastLineClass);

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Preprocess ...
            switch (lineClass)
            {
                case LineClass.Assign:
                    {
                        string lhs = lineMatch.GetStr("lhs");
                        string assignOp = lineMatch.GetStr("assignop");
                        string rhs = lineMatch.GetStr("rhs");
                        rhs = ParseRval(rhs);

                        string asm;
                        switch (assignOp[0])
                        {
                            case '=':
                                {
                                    asm = BuildCode(
                                        "set",          // Op
                                        lhs.Trim(),     // Destination
                                        rhs             // Value
                                        );
                                }
                                break;

                            default:
                                {
                                    string op = opMap[assignOp.Substring(0, 1)];
                                    asm = BuildCode(
                                        "op",
                                        op,             // Operation
                                        lhs,            // Destination
                                        lhs,            // Operand 1
                                        rhs             // Operand 2    
                                        );
                                }
                                break;
                        }
                        code.Add(asm);
                    }
                    break;

                case LineClass.Assign_SimpleMemLoad:
                    {
                        string dest = lineMatch.GetStr("a");
                        string cell = lineMatch.GetStr("b");
                        string index = lineMatch.GetStr("inner");

                        index = ParseRval(index);

                        // Cell doesn't hav num? Add '1' ...
                        if (cell.Length > 0 && !char.IsDigit(cell[cell.Length - 1]))
                            cell += "1";

                        string asm = BuildCode(
                            "read",             // Op
                            dest,               // Destination
                            cell,               // Memory cell
                            index               // Index
                            );
                        code.Add(asm);
                    }
                    break;

                case LineClass.Assign_SimpleDotProperty:
                    {
                        string dest = lineMatch.GetStr("a");
                        string obj = lineMatch.GetStr("b");
                        string property = lineMatch.GetStr("c");

                        if (!property.StartsWith("@"))
                            property = "@" + property;

                        string asm = BuildCode(
                            "sensor",           // Op
                            dest,               // Destination
                            obj,                // Memory cell
                            property            // Property
                            );
                        code.Add(asm);
                    }
                    break;

                case LineClass.Assign_SimpleComparison:
                    {
                        string dest = lineMatch.GetStr("dest");
                        string comp = lineMatch.GetStr("comp");
                        string a = lineMatch.GetStr("a");
                        string b = lineMatch.GetStr("b");

                        comp = comparisonMap[comp];
                        a = ParseRval(a);
                        b = ParseRval(b);

                        string asm = BuildCode(
                            "op",               
                            comp,               // Operation
                            dest,               // Destination
                            a,                  // Operand 1
                            b                   // Operand 2
                            );
                        code.Add(asm);
                    }
                    break;

                case LineClass.WriteMem:
                    {
                        string lhs = lineMatch.GetStr("lhs");
                        string index = lineMatch.GetStr("index");
                        string assignOp = lineMatch.GetStr("assignop");
                        string rhs = lineMatch.GetStr("rhs");

                        // lhs doesn't end with num? add '1' ..
                        if (lhs.Length > 0 && !char.IsDigit(lhs[lhs.Length - 1]))
                            lhs += "1";

                        index = ParseRval(index);
                        rhs = ParseRval(rhs);

                        if (assignOp == "=")
                        {
                            string asm = BuildCode(
                                    "write",        // Op
                                    rhs,            // Value
                                    lhs,            // Memory cell
                                    index           // Index
                                );
                            code.Add(asm);
                        }
                        else
                        {
                            // Read memory to 'tmpvar'...
                            string tmpvar = getNewIntermediateName();
                            code.Add(BuildCode(
                                "read",             // Op
                                tmpvar,             // Destination
                                lhs,                // Memory cell
                                index               // Index
                                ));


                            // Do operator to 'tmpvar' ...
                            string op = opMap[assignOp.Substring(0, 1)];
                            code.Add(BuildCode(
                                "op",               // Op
                                op,                 // Operation
                                tmpvar,             // Destination
                                tmpvar,             // Operand 1
                                rhs                 // Operand 2
                                ));

                            // Write 'tmpvar' to memory
                            string asm = BuildCode(
                                "write",            // Op
                                tmpvar,             // Value
                                lhs,                // Memory cell
                                index               // Index
                                );
                            code.Add(asm);
                        }
                    }
                    break;

                case LineClass.Print:
                    {
                        string lhs = lineMatch.GetStr("lhs");
                        int endIndex;
                        string inner = l.ScanToClosing(lineMatch.Index + lineMatch.Length - 1, out endIndex, '(', ')'); ;
                        
                        // Split commas/inner rval
                        var rxCommaSplit = new Regex(@"(?<rv>[^,]+)(,|\+)?");
                        var match = rxCommaSplit.Match(inner);
                        while (match.Success)
                        {
                            string rv = ParseRval(match.GetStr("rv"));
                            string asm = BuildCode(
                                "print",        // Op
                                rv              // Value
                            );
                            code.Add(asm);
                            match = match.NextMatch();
                        }

                        if (lhs == "println")
                            code.Add("print \"\\n\"");

                        doPrintFlush = true;
                    }
                    break;

                case LineClass.ControlEnable:
                    {
                        string obj = lineMatch.GetStr("obj");
                        string rv = lineMatch.GetStr("val");
                        rv = ParseRval(rv);

                        string asm = BuildCode(
                            "control",      // Op
                            "enabled",      // Property
                            obj,            // Object
                            rv              // Value
                            );
                        code.Add(asm);
                    }
                    break;

                case LineClass.ControlType:
                    {
                        string obj = lineMatch.GetStr("obj");
                        string rv = lineMatch.GetStr("val");
                        rv = ParseRval(rv);

                        string asm = BuildCode(
                            "control",      // Op
                            "configure",    // Property
                            obj,            // Object
                            rv              // Value
                            );
                        code.Add(asm);
                    }
                    break;

                case LineClass.Increment:
                    {
                        string varName = lineMatch.GetStr("v");
                        string asm = BuildCode(
                            "op",           // Op
                            "add",          // Add operation
                            varName,        // Destination
                            varName,        // Operand 1
                            "1"             // Operand 2
                            );
                        code.Add(asm);
                    }
                    break;

                case LineClass.Decrement:
                    {
                        string varName = lineMatch.GetStr("v");
                        string asm = BuildCode(
                            "op",           // Op
                            "sub",          // Add operation
                            varName,        // Destination
                            varName,        // Operand 1
                            "1"             // Operand 2
                            );
                        code.Add(asm);
                    }
                    break;

                case LineClass.StackStart:
                    {
                        if (!isNextStackLinked)
                            stackFrames.Push(new StackFrame(this, code));
                        isNextStackLinked = false;
                    }
                    break;

                case LineClass.StackEnd:
                    {
                        var endedFrame = stackFrames.Pop();
                        endedFrame.EndFrame(this);
                    }
                    break;

                case LineClass.If:
                    {
                        string condition = lineMatch.GetStr("inner");
                        string result = ParseBool(condition);
                        string ifNextAlias = getNewJumpAlias(true);
                        IfStack_PushHistory(ifNextAlias);

                        string asm = BuildCode(
                            "jump",         // Op
                            ifNextAlias,    // Line num
                            "notEqual",     // Comp type
                            result,         // Operand 1
                            "true"          // Operand 2
                            );
                        code.Add(asm);

                        stackFrames.Push(new StackFrame(this, code)
                        {
                            EndAction = (self) =>
                            {
                                int iLast = self.parentCode.Count - 1;
                                self.parentCode[iLast] = ifNextAlias + ":" + self.parentCode[iLast];

                                IfStack_PopHistory();
                            }
                        });
                        isNextStackLinked = true;
                    }
                    break;


                case LineClass.ElseIf:
                    {
                        // Remove (reposition) the 'end if' label
                        code[code.Count - 1] = IfStack_PopEndIfAliasForElseIf(code[code.Count - 1]);

                        // Insert 'jumpToEndIf' at end of 'if' body ...
                        string skipElseAsm = BuildCode(
                            "jump",                     // Op
                            ifStackLastEndIfAlias,      // Line num
                            "always",                   // Comp type
                            "0",                        // Operand 1
                            "0");                       // Operand 2
                        code.Add(ifStackLastNextIfAlias + ":" + skipElseAsm);


                        // Create new 'if-next' jump alias
                        ifStackLastNextIfAlias = getNewJumpAlias(true);

                        // Add else condition
                        string condition = lineMatch.GetStr("inner");
                        string result = ParseBool(condition);
                        string asm = BuildCode(
                            "jump",                     // Op
                            ifStackLastNextIfAlias,     // Line num
                            "notEqual",                 // Comp type
                            result,                     // Operand 1
                            "true"                      // Operand 2
                            );
                        code.Add(asm);


                        // Update most recent 

                        stackFrames.Push(new StackFrame(this, code)
                        {
                            EndAction = (self) =>
                            {
                                int iLast = self.parentCode.Count - 1;
                                self.parentCode[iLast] = ifStackLastEndIfAlias + ":" + ifStackLastNextIfAlias+ ":" + self.parentCode[iLast];
                            }
                        });
                        isNextStackLinked = true;
                    }
                    break;


                case LineClass.Else:
                    {
                        // Remove (reposition) the 'end if' label
                        code[code.Count - 1] = IfStack_PopEndIfAliasForElseIf(code[code.Count - 1]);

                        // Insert 'jumpToEndIf' at end of 'if' body ...
                        string skipElseAsm = BuildCode(
                            "jump",                     // Op
                            ifStackLastEndIfAlias,      // Line num
                            "always",                   // Comp type
                            "0",                        // Operand 1
                            "0");                       // Operand 2
                        code.Add(ifStackLastNextIfAlias + ":" + skipElseAsm);

                        stackFrames.Push(new StackFrame(this, code)
                        {
                            EndAction = (self) =>
                            {
                                int iLast = self.parentCode.Count - 1;
                                self.parentCode[iLast] = ifStackLastEndIfAlias + ":" + self.parentCode[iLast];
                            }
                        });
                        isNextStackLinked = true;
                    }
                    break;


                case LineClass.FunctionCall:
                    {
                        var fnCallCode = CreateFunctionCall(lineMatch.GetStr("fname"), lineMatch.GetStr("params"));
                        code.AddRange(fnCallCode);
                    }
                    break;

                case LineClass.ForLoop:
                    {
                        // Process assign (as separate line)
                        string init = lineMatch.GetStr("a");
                        string comp = lineMatch.GetStr("b");
                        string inc = lineMatch.GetStr("c");

                        // ~~~~~~ A: initialization
                        string[] forInits = init.Replace(",", "\n").Split('\n');
                        if (forInits.Length > 0)
                        {
                            // remove 'int' type ...
                            if (forInits[0].StartsWith("int "))
                                forInits[0] = forInits[0].Substring(4);

                            foreach (string forInit in forInits)
                                ProcessLine(forInit);
                        }

                        // ~~~~~~ B: comparison/conditional jump instructions
                        var compInstructions = new List<string>();
                        string jumpEndAsm;
                        string compRval;
                        string jumpEndAlias = getNewJumpAlias(true);
                        string jumpBodyAlias = getNewJumpAlias();

                        using (var tir = new TemporaryInstructionRetarget(this, compInstructions))
                        {
                            compRval = ParseBool(comp);
                            jumpEndAsm = BuildCode(
                                "jump",         // Op
                                jumpEndAlias,      // Line num
                                "equal",     // Comp type
                                compRval,       // Condition
                                "true"          // True
                                );
                        }



                        // ~~~~~~ C: increment step
                        var incInstructions = new List<string>();
                        string[] incCommands = inc.Replace(",", "\n").Split('\n');
                        using (var tir = new TemporaryInstructionRetarget(this, incInstructions))
                        { 
                            foreach(string incCommand in incCommands)
                                ProcessLine(incCommand);
                        }


                        // Initial jump/skip codeblock (condition == false)?
                        code.AddRange(compInstructions);
                        code.Add(jumpEndAsm);
                        
                        
                        // Body + go to start
                        string initJump = getNewJumpAlias();
                        stackFrames.Push(new StackFrame(this, code)
                        {
                            EndAction_PreDumpToParent = (self) =>
                            {
                                if (self.code.Count > 0)
                                    self.code[0] = jumpBodyAlias + ":" + self.code[0];
                            },

                            EndAction = (self) =>
                            {
                                string jumpBodyAsm = BuildCode(
                                "jump",         // Op
                                jumpBodyAlias,  // Line num
                                "notEqual",     // Comp type
                                compRval,       // Condition
                                "true"          // True
                                );


                                // End of loop (next -> jump to body again?)
                                self.parentCode.AddRange(incInstructions);
                                self.parentCode.AddRange(compInstructions);
                                self.parentCode.Add(jumpEndAlias + ":" + jumpBodyAsm);
                            },
                        });

                        isNextStackLinked = true;
                    }
                    break;


                case LineClass.GoTo:
                    {
                        string label = lineMatch.GetStr("label");
                        string asm = BuildCode(
                            "set",          // Op
                            "@counter",     // Destination
                            label
                            );
                        code.Add(asm);
                    }
                    break;


                case LineClass.Wait:
                    {
                        string cond = lineMatch.GetStr("cond").Trim();
                        if (cond.Length == 0)
                            throw new Exception("Wait function is empty");

                        var waitCode = new List<string>();
                        string condRval;
                        using (var tcr = new TemporaryInstructionRetarget(this, waitCode))
                            condRval = ParseRval(cond);

                        string jumpAlias = getNewJumpAlias();
                        waitCode[0] = jumpAlias + ":" + waitCode[0];

                        // Build loop
                        waitCode.Add(BuildCode(
                            "jump",             // Op
                            jumpAlias,          // Line num
                            "notEqual",         // Comp type
                            condRval,           // Condition
                            "true"              // True
                            ));
                        code.AddRange(waitCode);
                    }
                    break;


                case LineClass.Asm:
                    code.Add(lineMatch.GetStr("v"));
                    break;

                case LineClass.Return:
                    {
                        // Return value ?
                        string retv = lineMatch.GetStr("v");

                        // In 'main'? End (reset) ...
                        if (code == baseFrame.code)
                        {
                            if (doPrintFlush) code.Add("printflush message1");
                            code.Add("end");
                        }

                        // In function - return value?
                        else if (retv.Length > 0)
                        {
                            retv = ParseRval(retv);
                            code.Add(BuildCode(
                                "set",
                                retValName,
                                retv
                                ));
                        }

                        // In function - jump to return
                        else
                            code.Add("set @counter __retadr_");
                    }
                    break;
            }


            // Prepend label again?
            if (lineJumpLabel != "")
            {
                if (codeListStart.Count > 0)
                    codeListStart[codeCountStart] = lineJumpLabel + ":" + codeListStart[codeCountStart];
                else
                    codeListStart.Add(lineJumpLabel + ":");
            }
        }
    }
}

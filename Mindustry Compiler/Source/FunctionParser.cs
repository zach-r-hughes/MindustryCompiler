using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Parse funciton calls and definitions.
/// </summary>

namespace Mindustry_Compiler
{
    public partial class MindustryCompilerForm
    {
        /// <summary>
        /// Map of jump-alias for a user defined function call.
        /// </summary>
        Dictionary<string, FunctionInfo> functionMap;
        List<string> functionCode;
        bool hasMainFunction = false;

        readonly string retValName = "__retv_";

        static readonly List<FunctionInfo> builtInFunctions = new List<FunctionInfo>()
        {
            new FunctionInfo("main", 0),
            new FunctionInfo("sleep", 1){paramNames = { "seconds" } },
            new FunctionInfo("wait", 1) { paramNames = {"condition"} },
            new FunctionInfo("asm", 1),
            new FunctionInfo("angle"),

            new FunctionInfo("max"),
            new FunctionInfo("min"),
            new FunctionInfo("abs", 1),
            new FunctionInfo("floor", 1),
            new FunctionInfo("ceil", 1),

            new FunctionInfo("len", 1),
            new FunctionInfo("noise", 2),

            new FunctionInfo("log", 1),
            new FunctionInfo("log10", 1),
            new FunctionInfo("sqrt"),

            new FunctionInfo("sin", 1),
            new FunctionInfo("cos", 1),
            new FunctionInfo("tan", 1),
            
            new FunctionInfo("rand"),
        };
        static FunctionInfo mainFunctionInfo => builtInFunctions[0];
        static FunctionInfo sleepFunctionInfo => builtInFunctions[1];
        static FunctionInfo waitFunctionInfo => builtInFunctions[2];
        

        public class FunctionInfo
        {
            public string name;
            public int paramCount;
            public List<string> paramNames = new List<string>();

            public FunctionInfo(string fnName, int paramCount = -1)
            {
                name = fnName;
                this.paramCount = paramCount;
            }

            public FunctionInfo(string fnName, string paramInner)
            {
                this.name = fnName;
                this.paramCount = 
                    paramInner.Trim().Length == 0 ? 0 :
                    paramInner.Count(c => c == ',') + 1;

                // Save param names
                var pnames = paramInner.Split(',');
                var rxTypenameStrip = new Regex(@"(?<pname>\w+)\s*(\n|$)");
                for (int i = 0; i < pnames.Length; i++)
                {
                    // Remove typenames
                    pnames[i] = rxTypenameStrip.Match(pnames[i]).GetStr("pname");
                    paramNames.Add(pnames[i].Trim());
                }
            }

            public string Alias { get => "__" + name + "_" + paramCount.ToString() + "_"; }

            public string GetAliasFromCallStr(string s)
            {
                return "";
            }

            public override bool Equals(object obj)
            {
                if (!(obj is FunctionInfo))
                    return false;

                var other = obj as FunctionInfo;

                if (this.name != other.name)
                    return false;

                if (this.paramCount == -1 || other.paramCount == -1)
                    return true;

                return this.paramCount == other.paramCount;
            }

            public override int GetHashCode()
            {
                int hashCode = -918409233;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
                hashCode = hashCode * -1521134295 + paramCount.GetHashCode();
                return hashCode;
            }
        }


        void InitializeFunctions(ref string source)
        {
            lastCode = "Initialize Functions";
            hasMainFunction = false;
            functionMap = new Dictionary<string, FunctionInfo>();
            functionCode = new List<string>();
            ParseFunctions(ref source);
            FixEmptyJumpAliasLines(ref source);
        }

        /// <summary>
        // Dump functions to the end of the asm output.
        /// </summary>
        void FinalizeFunctions()
        {
            lastCode = "Finalize Functions";

            // Dump fn code into base frame ...
            baseFrame.code.AddRange(functionCode);

            // No main function? Add 'end'...
            if (!hasMainFunction)
                baseFrame.code.Add("end");
        }

        FunctionInfo FindFunctionInfo(string fnName, int paramCount)
        {
            foreach (var nfo in functionMap)
                if (nfo.Value.name == fnName && paramCount == nfo.Value.paramCount)
                    return nfo.Value;
            return null;            
        }

        /// <summary>
        /// Returns the declaration+body of a function from an 'rxFunctionDefinition' match.
        /// </summary>
        string GetFunctionBody(Match match, ref string source, out int startIndex, out int endIndex)
        {
            startIndex = match.Index + (match.Value.StartsWith("\n") ? 1 : 0);
            int fnStart = source.IndexOf('{', match.Index + match.Length);
            string body = source.ScanToClosing(fnStart, out endIndex, '{', '}');
            return body.Trim();
        }

        /// <summary>
        /// Parse custom user function declarations
        /// </summary>
        void ParseFunctions(ref string source)
        {
            using (var tcr = new TemporaryInstructionRetarget(this, functionCode))
            {
                Match match = rxFunctionDefinition.Match(source);
                while (match.Success)
                {
                    string fnName = match.GetStr("a");
                    string paramInner = match.GetStr("params");
                    var fnObj = new FunctionInfo(fnName, paramInner);

                    // ~~~~~~~~ 'main()'?
                    if (fnObj.name == "main" && fnObj.paramCount == 0)
                    {
                        if (hasMainFunction)
                            throw new Exception("Multiple main functions defined.");
                        ParseMainFunction(match, ref source);
                    }

                    else if(builtInFunctions.Contains(fnObj))
                    {
                        continue;
                    }

                    // ~~~~~~~~ Custom function definition
                    else
                    {
                        // Disalllow built-in function names
                        if (builtInFunctions.Contains(fnObj))
                            throw new Exception("Custom function defnition matches name/param of built-in function: " +
                                fnName + "([" + fnObj.paramCount + "])");

                        // Add to 'fn map'
                        functionMap.Add(fnName, fnObj);

                        // Remove 'fn' from source ...
                        int fnStart, fnEnd;
                        string fnBody = GetFunctionBody(match, ref source, out fnStart, out fnEnd);
                        source = source.Remove(fnStart, fnEnd - fnStart);

                        // ~~~~~~~ Build 'fn' body asm
                        var fnLines = fnBody.Split('\n');
                        if (fnLines.Length == 0)
                            fnLines = new string[]{ "set __nop 0" };

                        // Body ..
                        code.Add("print ====================================");
                        for (int i = 0; i < fnLines.Length; i++)
                            ProcessLine(fnLines[i]);
                        
                        // Return address
                        code.Add(BuildCode(
                            "set",
                            "@counter",
                            "__retadr_"));


                        code[1] = fnObj.Alias + ":" + code[1];
                    }
                    // Find next function ...
                    match = rxFunctionDefinition.Match(source);
                }
            }
        }

        /// <summary>
        /// Arrange main function to the last part of the program.
        /// </summary>
        void ParseMainFunction(Match match, ref string source)
        {
            lastCode = "Parse Main Function";

            // ~~~~~~ Move 'main()' last
            hasMainFunction = true;

            // Position 'main()' after init
            int fnStart, fnEnd;
            string mainBody = GetFunctionBody(match, ref source, out fnStart, out fnEnd);
            source = source
                .Remove(fnStart, fnEnd - fnStart)
                .Insert(fnStart, mainFunctionInfo.Alias + ":" + mainBody);

            // Add main loop jump
            string mainLoopAsm = BuildCode(
                "jump",                     // Op
                mainFunctionInfo.Alias,     // Line Number
                "always",                   // Operation
                "0",                        // Operand 1
                "0"                         // Operand 2
                );
            source += "\nasm(" + mainLoopAsm + ")\n";
        }

        /// <summary>
        /// Generates the code for a function call
        /// </summary>
        List<string> CreateFunctionCall(string fnName, string paramsInner)
        {
            // Split params
            var pvals = paramsInner.SplitByParamCommas();

            // ~~~~~~~~ Built-in function?
            var fnObj = new FunctionInfo(fnName, pvals.Count);
            if (builtInFunctions.Contains(fnObj))
            {
                code.Add(BuildCode(
                "op",               // Op
                fnName,             // Function
                ""
                ));
            }


            // Get function obj ref ...
            fnObj = FindFunctionInfo(fnName, pvals.Count);

            if (fnObj == null)
                throw new Exception("Could not find function def: " + fnName + "(" + pvals.Count + ")");

            if (pvals.Count != fnObj.paramCount)
                throw new Exception("Error with function parameter count.");

            var fnCode = new List<string>();
            using (var tcr = new TemporaryInstructionRetarget(this, fnCode))
            {

                // ~~~~~~~ Params ...
                for (int i = 0; i < fnObj.paramCount; i++)
                {
                    // Add 'save old param value'
                    code.Add(BuildCode(
                        "set",                          // Op
                        "__p" + i.ToString() + "__",    // Destination
                        fnObj.paramNames[i]             // Value
                        ));

                    // Parse param 'rvals'
                    pvals[i] = ParseRval(pvals[i]);
                    code.Add(BuildCode(
                        "set",                          // Op
                        fnObj.paramNames[i],            // Destination
                        pvals[i]                        // Value
                        ));
                }

                // ~~~~~~~ Set return address ...
                code.Add(BuildCode(
                    "op",
                    "add",                          // Operation
                    "__retadr_",                    // Destination
                    "@counter",                     // Operand 1
                    "1"                             // Operand 2
                    ));

                // ~~~~~~~ Jump to fn
                code.Add(BuildCode(
                    "set",                              // Op
                    "@counter",                         // Destination
                    fnObj.Alias                         // Value
                    ));

                // ~~~~~~~ Restore params ...
                for (int i = 0; i < fnObj.paramCount; i++)
                {
                    // Add 'save old param value'
                    code.Add(BuildCode(
                        "set",                          // Op
                        fnObj.paramNames[i],            // Destination
                        "__p" + i.ToString() + "__"     // Value
                        ));
                }
            }
            return fnCode;
        }


        void FixEmptyJumpAliasLines(ref string source)
        {
            var rxEmptyJumpAlias = new Regex(@"(^|\n)(?<lbls>\w+\s*:\s*)\s*($|\n)");
            var match = rxEmptyJumpAlias.Match(source);
            while(match.Success)
            {
                string fixedSource = "\n" + match.GetStr("lbls");
                source = source.ReplaceMatch(match, fixedSource);
                match = rxEmptyJumpAlias.Match(source);
            }
        }
    }
}
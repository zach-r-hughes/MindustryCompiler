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
        enum LineClass
        {
            Empty,
            Comment,
            PreprocessorDefine,
            Assign,
            Assign_SimpleDotProperty,
            Assign_SimpleMemLoad,
            Assign_SimpleComparison,
            GoTo,
            Asm,
            WriteMem,
            Amount,
            Increment,
            Decrement,
            Print,
            Load,
            ForLoop,
            WhileLoop,
            DoWhileLoop,
            ControlEnable,
            ControlType,
            StackStart,
            StackEnd,
            Return,
            If,
            ElseIf,
            Else,
            FunctionCall,
            Wait,
            Invalid,
        }

        /// <summary>
        /// Maps operation characters to their op-codes
        /// </summary>
        Dictionary<string, string> opMap = new Dictionary<string, string>
        {
            {"+", "add" },
            {"-", "sub" },
            {"*", "mul" },
            {"/", "div" },
            {"%", "mod" },
            {"^", "pow" },
            {"==", "equal" },
            {"!=", "notEqual" },
            {">", "greaterThan" },
            {"<", "lessThan" },
            {">=", "greaterThanEqual" },
            {"<=", "lessThanEqual" },
        };

        /// <summary>
        /// Maps comparison strings to their op-codes
        /// </summary>
        Dictionary<string, string> comparisonMap = new Dictionary<string, string>
        {
            { "==", "equal" },
            { "!=", "notEqual" },
            { "<", "lessThan" },
            { ">", "greaterThan" },
            { "<=", "lessThanEq" },
            { ">=", "greaterThanEq" }
        };

        /// <summary>
        /// Maps comparison strings to their opposite op-codes
        /// </summary>
        Dictionary<string, string> comparisonMapInverse = new Dictionary<string, string>
        {
            { "==", "notEqual" },
            { "!=", "equal" },
            { "<",  "greaterThanEq"},
            { ">", "lessThanEq" },
            { "<=", "greaterThan" },
            { ">=", "lessThan" },
        };

        /// <summary>
        /// Matches a memory load.
        /// Groups: 'a', 'inner'
        /// </summary>
        readonly Regex rxMemLoad = new Regex(@"(?<a>cell[0-9]*)(?<inner>\[.*)");

        /// <summary>
        /// Matches an increment 'i++'.
        /// Groups: 'v'
        /// </summary>
        readonly Regex rxIncrement = new Regex(@"(?<v>\w+)\s*\+\+\;?");

        /// <summary>
        /// Matches a decrement 'i--'.
        /// Groups: 'v'
        /// </summary>
        readonly Regex rxDecrement = new Regex(@"(?<v>\w+)\s*\-\-\;?");

        /// <summary>
        /// Matches an 'object.enable' assignment.
        /// Groups: 'obj', 'val'
        /// </summary>
        readonly Regex rxControlEnable = new Regex(@"(?<obj>\w+).enabled?\s*=\s*(?<val>[^;]+)");

        /// <summary>
        /// Matches an 'object.config' assignment.
        /// Groups: 'obj', 'val'
        /// </summary>
        readonly Regex rxControlConfig = new Regex(@"(?<obj>(\w+))\.(type|config)\s=(?<val>[^;]*)");

        /// <summary>
        /// Matches a 'return' operation.
        /// Groups: 'v'
        /// </summary>
        readonly Regex rxReturn = new Regex(@"^\s*return\s*(?<v>[^;]*)\s*;?$");

        /// <summary>
        /// Matches an assignment.
        /// Groups: 'lhs', 'assignop', 'rhs'.
        /// </summary>
        readonly Regex rxAssign = new Regex(@"\s*(?<lhs>\w+)\s*(?<assignop>(=|\+=|-=|\*=|\/=|\^=|%=))\s*(?<rhs>[^;]*);?");

        /// <summary>
        /// Matches a simple memory load into a variable.
        /// Groups: 'a', 'inner', 'b'.
        /// </summary>
        readonly Regex rxAssign_SimpleMemLoad = new Regex(@"(?<a>\w+)\s*=\s*(?<b>cell([0-9]+)?)\[(?<inner>.*)\s*\]\s*;");

        /// <summary>
        /// Matches a simple sensor into a simple variable.
        /// Groups: 'a', 'b', 'c'.
        /// </summary>
        readonly Regex rxAssign_SimpleDotProperty = new Regex(@"(?<a>\w+)\s*=\s*(?<b>\w+)\s*\.\s*(?<c>\w+)\s*;");

        /// <summary>
        /// Matches a simple assign to comparison (true/false).
        /// Groups: 'dest', 'a', 'comp', 'b'
        /// </summary>
        readonly Regex rxAssign_SimpleComparison = new Regex(@"^\s*(?<dest>\w+)\s*=\s*(?<a>[^=!<>&\|]+)\s*(?<comp>(==)|(!=)|(>=?)|(<=?))\s*(?<b>[^=!<>&\|;]+)\s*;?\s*$");

        /// <summary>
        /// Writes (saves) to a memory cell at index
        /// Groups: 'lhs', 'index', 'assignop', 'rhs'
        /// </summary>
        readonly Regex rxWriteMem = new Regex(@"^\s*(?<lhs>(?!\[)\w+)\[(?<index>(?!\]).+)\]\s*(?<assignop>(=|\+=|-=|\*=|\/=))\s*(?<rhs>[^;]*);?");

        /// <summary>
        /// Matches a print statement (either print() or println())
        /// Groups: 'lhs', 'v'
        /// </summary>
        readonly Regex rxPrint = new Regex(@"(?<lhs>print(ln)?)\s*\(");

        /// <summary>
        /// Matches an if statement.
        /// Groups: 'inner'
        /// </summary>
        readonly Regex rxIf = new Regex(@"(\n|^)\s*if\s*\((?<inner>.*)\)\s*(\n|$)");

        /// <summary>
        /// Matches an else-if statement.
        /// Groups: 'inner'
        /// </summary>
        readonly Regex rxElseIf = new Regex(@"(\n|^)\s*else\s*if\s*\((?<inner>.*)\)\s*(\n|$)");

        /// <summary>
        /// Matches an else statement.
        /// Groups: 'inner'
        /// </summary>
        readonly Regex rxElse = new Regex(@"^\s*else\s*$");

        /// <summary>
        /// Matches a function call. 
        /// Groups: 'fname'
        /// </summary>
        readonly Regex rxFunctionRval = new Regex(@"\s*(?<fname>\w+)\s*\(");

        /// <summary>
        /// Matches a for loop.
        /// Groups: 'a', 'b', 'c'
        /// </summary>
        readonly Regex rxForLoop = new Regex(@"for\s*\((?<a>[^;]+);(?<b>[^;]+);(?<c>.*)\)");

        /// <summary>
        /// Matches a line num reference.
        /// Groups: 'num'
        /// </summary>
        readonly Regex rxLineNumReference = new Regex(@"(?<alias>__\w+_\+?)");

        /// <summary>
        /// Matches a line num target reference.
        /// Groups: 'alias', 'code'
        /// </summary>
        readonly Regex rxJumpLineTarget = new Regex(@"^\s*(?<alias>\w+\+?)\s*:\s*(?<code>.*)$");

        /// <summary>
        /// Matches any preprocessor derective.
        /// </summary>
        readonly Regex rxPreprocessor = new Regex(@"^\s*#.*$");

        /// <summary>
        /// Matches a preprocessor define.
        /// Groups: 'a', 'b'
        /// </summary>
        readonly Regex rxPreprocessorDefine = new Regex(@"^#\s*define\s(?<a>\w+)\s+(?<b>.*)\s*$");

        /// <summary>
        /// Matches a preprocessor define function definition.
        /// Groups: 'a', 'params', 'b'
        /// </summary>
        readonly Regex rxPreprocessorDefineFunction = new Regex(@"^#\s*define\s(?<a>\w+)\((?<params>[^\)]+)\)\s+(?<b>.*)\s*$");

        /// <summary>
        /// Matches a 'go to' statement.
        /// Groups: 'label'
        /// </summary>
        readonly Regex rxGoTo = new Regex(@"^\s*goto\s*(?<label>\w+)\s*;?\s*$");

        /// <summary>
        /// Matches the beginning of a stack-frame
        /// </summary>
        readonly Regex rxStackStart = new Regex(@"\s*{\s*");

        /// <summary>
        /// Matches the beginning of a stack-frame
        /// </summary>
        readonly Regex rxStackEnd = new Regex(@"\s*}\s*");

        /// <summary>
        /// Matches a simple function call.
        /// Groups: 'fname', 'params'
        /// </summary>
        readonly Regex rxFunctionCall = new Regex(@"^\s*(?<fname>\w+)\s*\((?<params>.*)\)\s*;?\s*$");

        /// <summary>
        /// Matches a 'wait for condition' function call.
        /// Groups: 'cond'
        /// </summary>
        readonly Regex rxWaitFunction = new Regex(@"^\s*wait\s*\(\s*(?<cond>.*)\s*\)\s*;?\s*$");

        /// <summary>
        /// Matches a function definition
        /// </summary>
        readonly Regex rxFunctionDefinition = new Regex(@"(\n|^)\s*((?!(if|else|while))(\w+)\s+)(?<a>\w+)\s*\((?<params>[^\)]*)\)\s*(\n|$)");

        /// <summary>
        /// Matches an 'asm' instruction.
        /// Groups: 'v'
        /// </summary>
        readonly Regex rxAsm = new Regex(@"(\n|^)\s*asm\s*\((?<v>[^#].*)\s*\)\s*;?\s*(\n|$)");

        //==============================================================================
        /// <summary>
        /// Classify a line of code.
        /// </summary>
        LineClass GetCodeLineClass(string code)
        {
            // Convert commands ...
            if (code.StartsWith("//")) return LineClass.Comment;

            lineMatch = rxGoTo.Match(code);
            if (lineMatch.Success) return LineClass.GoTo;

            lineMatch = rxAsm.Match(code);
            if (lineMatch.Success) return LineClass.Asm;

            lineMatch = rxPreprocessorDefine.Match(code);
            if (lineMatch.Success) return LineClass.PreprocessorDefine;

            lineMatch = rxStackStart.Match(code);
            if (lineMatch.Success) return LineClass.StackStart;

            lineMatch = rxStackEnd.Match(code);
            if (lineMatch.Success) return LineClass.StackEnd;

            lineMatch = rxPrint.Match(code);
            if (lineMatch.Success) return LineClass.Print;

            lineMatch = rxWaitFunction.Match(code);
            if (lineMatch.Success) return LineClass.Wait;

            lineMatch = rxIf.Match(code);
            if (lineMatch.Success) return LineClass.If;

            lineMatch = rxElseIf.Match(code);
            if (lineMatch.Success) return LineClass.ElseIf;

            lineMatch = rxElse.Match(code);
            if (lineMatch.Success) return LineClass.Else;

            lineMatch = rxForLoop.Match(code);
            if (lineMatch.Success) return LineClass.ForLoop;

            lineMatch = rxIncrement.Match(code);
            if (lineMatch.Success) return LineClass.Increment;

            lineMatch = rxDecrement.Match(code);
            if (lineMatch.Success) return LineClass.Decrement;

            lineMatch = rxControlEnable.Match(code);
            if (lineMatch.Success) return LineClass.ControlEnable;

            lineMatch = rxReturn.Match(code);
            if (lineMatch.Success) return LineClass.Return;

            lineMatch = rxControlConfig.Match(code);
            if (lineMatch.Success) return LineClass.ControlType;

            lineMatch = rxWriteMem.Match(code);
            if (lineMatch.Success) return LineClass.WriteMem;

            lineMatch = rxAssign_SimpleMemLoad.Match(code);
            if (lineMatch.Success) return LineClass.Assign_SimpleMemLoad;

            lineMatch = rxAssign_SimpleDotProperty.Match(code);
            if (lineMatch.Success) return LineClass.Assign_SimpleDotProperty;

            lineMatch = rxFunctionCall.Match(code);
            if (lineMatch.Success) return LineClass.FunctionCall;

            lineMatch = rxAssign_SimpleComparison.Match(code);
            if (lineMatch.Success) return LineClass.Assign_SimpleComparison;

            lineMatch = rxAssign.Match(code);
            if (lineMatch.Success) return LineClass.Assign;

            return LineClass.Invalid;
        }
    }
}

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
        readonly HashSet<string> unitTypeSet = new HashSet<string>
        {
            "alpha", "antumbra", "arkyid", "atrax", "beta", "bryde", "corvus", "crawler", 
            "dagger", "eclipse", "flare", "fortress", "gamma", "guardian", "horizon", "mace", 
            "mega", "minke", "mono", "nova", "oct", "omura", "poly", "pulsar", "quad", "quasar", 
            "reign", "risso", "scepter", "sei", "spiroct", "toxopid", "vela", "zenith"
        };



        void ParseUnitControl(string l)
        {
            string cmd = lineMatch.GetStr("type").Trim();
            string rest = lineMatch.GetStr("rest").Trim();

            // Turn things like 'boost = true' into 'boost(true)'....
            ParseUnitControl_FormatEqualsIntoFunction(ref rest);
            rest = Regex.Replace(rest, @"\(|\)", e => "");

            // Split params, remove ...
            var psplit = rest.SplitByParamCommas();

           
            // Parse param r-values ...
            for (int i = 0; i < psplit.Count; i++)
                psplit[i] = ParseRval(psplit[i]);


            // Is 'locate' command (complicated)?
            if (cmd == "type" || cmd == "bind")
            {
                ParseUnitControl_Bind(psplit);
                return;
            }

            if (cmd == "locate" || cmd == "find")
            {
                ParseUnitControl_Locate(psplit);
                return;
            }

            // Join all params into spaced-string
            string pval = string.Join(" ", psplit);

            // Create control call ...
            string asm = BuildCode(
                "ucontrol",         // Op
                cmd,                // Control type
                pval                // Parameter(s)
                );
            code.Add(asm);
        }


        /// <summary>
        /// Convert a control assignment into the equivalent function call.
        /// Helps simplify parsing (always function call syntax).
        /// Example: 'boost = true' -> 'boost(true)'
        /// </summary>
        void ParseUnitControl_FormatEqualsIntoFunction(ref string rest)
        {
            // Contains '='? If no, return ...
            if (!rest.Contains("="))
                return;


            rest = "(" + rest.Replace("=", "").Trim() + ")";
            return;
        }


        //========================================================================================

        static readonly HashSet<string> unitLocate_ValidGroups = new HashSet<string>
        {
            "building", "damaged", "ore", "spawn"
        };



        /// <summary>
        /// Parse a 'unit bind' (type of unit) assignment.
        /// </summary>
        void ParseUnitControl_Bind(List<string> psplit)
        {
            // If built-in unit type, make start with '@' ...
            if (!psplit[0].StartsWith("@") && unitTypeSet.Contains(psplit[0]))
                psplit[0] = "@" + psplit[0];

            code.Add(BuildCode(
                "ubind",
                psplit[0]
                ));
        }


        /// <summary>
        /// Parse a 'unit locate' command.
        /// </summary>
        void ParseUnitControl_Locate(List<string> psplit)
        {
            // Group (type to locate) ok?
            if (!unitLocate_ValidGroups.Contains(psplit[0]))
                throw new Exception("Unit locate group unknown");

            // Remove '&' on output
            switch (psplit[0])
            {
                case "building":
                    // type isEnemy ___ &outx &outy &found &building
                    {
                        // Correct num parameters?
                        if (psplit.Count != 7)
                            throw new Exception("Locate param count incorrect.\n\t" +
                                "locate(\"building\", bool isEnemy, int &outX, int& outY, bool& found, string &building");

                        // insert blank parameters
                        psplit.Insert(3, "@copper");

                        string asm = BuildCode(
                            "ulocate",                  // Op
                            string.Join(" ", psplit)    // Parameter(s)
                            );
                        code.Add(asm);
                    }
                    break;

                case "ore":
                    {



                    }
                    break;
            }

            


        }
    }
}

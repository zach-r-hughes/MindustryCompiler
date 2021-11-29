using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mindustry_Compiler
{
    static class MatchGroupExtensions
    {
        public static Group GetWhere(this GroupCollection groups, System.Func<Group, bool> matchFn)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                var g = groups[i];
                if (matchFn(g)) return g;
            }
            return null;
        }


        public static bool HasGroup(this Match match, string groupName) =>
            GetWhere(match.Groups, e => e.Name == groupName) != null;


        public static string GetStr(this Match match, string groupName)
        {
            var group = match.Groups.GetWhere(e => e.Name == groupName);
            if (group != null) return group.Value;
            return "";
        }


        public static int GetAfterIndex(this Group g) =>
            g.Index + g.Value.Length;
    }
}

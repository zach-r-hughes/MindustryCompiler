using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindustry_Compiler
{
    static class StringBuilderExtensions
    {
        public static int IndexOf(this StringBuilder sb, char c)
        {
            for (int i = 0; i < sb.Length; i++)
                if (sb[i] == c)
                    return i;
            return -1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindustry_Compiler
{
    public static class PrefixConverter
    {
        static string reverse(this string s)
        {
            string ret = "";
            for (int i = s.Length; --i >= 0;)
                ret += s[i];
            return ret;
        }

        static bool isOpChar(char c)
        {
            return (!char.IsLetter(c) && !char.IsDigit(c));
        }

        static int getPriority(char C)
        {
            if (C == '-' || C == '+')
                return 1;
            else if (C == '*' || C == '/' || C == '%')
                return 2;
            else if (C == '^')
                return 3;
            return 0;
        }

        static string infixToPostfix(string infix)
        {
            infix = '(' + infix.Trim().Replace(" ", "") + ')';
            int l = infix.Length;
            Stack<char> char_stack = new Stack<char>();
            string output = "";

            for (int i = 0; i < l; i++)
            {

                // If the scanned character is an  
                // operand, add it to output. 
                if (char.IsLetter(infix[i]) || char.IsDigit(infix[i]))
                    output += infix[i];

                // If the scanned character is an 
                // ‘(‘, push it to the stack. 
                else if (infix[i] == '(')
                    char_stack.Push('(');

                // If the scanned character is an 
                // ‘)’, pop and output from the stack  
                // until an ‘(‘ is encountered. 
                else if (infix[i] == ')')
                {

                    while (char_stack.Peek() != '(')
                        output += char_stack.Pop();

                    // Remove '(' from the stack 
                    char_stack.Pop();
                }

                // Operator found  
                else
                {

                    if (isOpChar(char_stack.Peek()))
                    {
                        while (char_stack.Count > 0 && getPriority(infix[i]) <= getPriority(char_stack.Peek()))
                            output += char_stack.Pop();

                        // Push current Operator on stack 
                        char_stack.Push(infix[i]);
                    }
                }
            }
            return output;
        }

        public static string infixToPrefix(string infix_str)
        {
            // Create (and reverse) infix ...
            StringBuilder infix = new StringBuilder(infix_str.reverse());

            /* Reverse String 
             * Replace ( with ) and vice versa 
             * Get Postfix 
             * Reverse Postfix  *  */
            int l = infix.Length;

            // Replace ( with ) and vice versa 
            for (int i = 0; i < l; i++)
            {

                if (infix[i] == '(')
                {
                    infix[i] = ')';
                    i++;
                }
                else if (infix[i] == ')')
                {
                    infix[i] = '(';
                    i++;
                }
            }

            string prefix = infixToPostfix(infix.ToString());

            // Reverse postfix 
            prefix = prefix.reverse();

            return prefix;
        }
    }
}
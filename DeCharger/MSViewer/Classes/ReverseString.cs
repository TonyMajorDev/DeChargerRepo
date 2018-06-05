using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public static class ReverseString
    {
        /// <summary>
        /// Reverse the character order of the string.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static string Reverse(this string sequence)
        {
            // Based on: https://www.dotnetperls.com/reverse-string

            char[] reverse = sequence.ToCharArray();

            Array.Reverse(reverse);

            return new string(reverse);
        }
    }
}

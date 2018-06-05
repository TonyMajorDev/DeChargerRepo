//-----------------------------------------------------------------------
// Copyright 2018 Eli Lilly and Company
//
// Licensed under the Apache License, Version 2.0 (the "License");
//
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Science.Proteomics
{



    public static class AminoAcidHelpers
    {
        public static SortedList<string, double> AminoAcidMass2 = new SortedList<string, double>();
        public static SortedList<char, double> AminoAcids = new SortedList<char, double>();

        //public static SortedList<string, double> ModifiedAminoAcidStrings = new SortedList<string, double>();

        public static SortedList<string, string> ModifiedAminoAcidStrings = new SortedList<string, string>();

        public static SortedList<string, double> ModifiedAminoAcids = new SortedList<string, double>();

        public static string onlymodifications = string.Empty;

        /// <summary>
        /// Validate that the passed in sequence is composed of only valid/known amino acid symbols
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static bool IsValidSequence(this string sequence)
        {
            if (sequence == null) return false;
            return !sequence.Any(c => Char.IsUpper(c) && AminoAcids.ContainsKey(c) == false);
        }

        /// <summary>
        /// Replace I and L amino acids with J.  J represents an I or L at that position.  
        /// </summary>
        /// <param name="aaSequence"></param>
        /// <returns></returns>
        public static string MakeLeucineNeutral(this string aaSequence)
        {
            return aaSequence.Replace('I', 'J').Replace('L', 'J');
        }

        /// <summary>
        /// Adds the distinct reverse items to the passed in list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<string> AddReverse(this IEnumerable<string> list)
        {
            var result = new List<string>();

            result.AddRange(list);
            result.AddRange(list.Select(s => new string(s.Reverse().ToArray())));

            return result.Distinct();
        }

        /// <summary>
        /// Compare if query amino acid is a mass match for the target amino acid
        /// </summary>
        /// <param name="query"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static AminAcidConfidence AminoAcidCompare(this char query, char target)
        {
            if (query == target) return AminAcidConfidence.Sure;
            if (query == 'J' && (target == 'I' || target == 'L')) return AminAcidConfidence.High;

            return AminAcidConfidence.Gap;
        }

        /// <summary>
        /// The sum of all the AminoAcid MonoIsotopic Masses
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static double TotalMass(this string sequence)
        {
            double length = 0;
            foreach (char c in sequence)
            {
                try
                {
                    length += AminoAcidHelpers.AminoAcids[c];
                }
                catch (Exception)
                {
                    return double.NaN;
                }
            }
            return length;
        }


        /// <summary>
        /// This is the Isobaric Sequence where all the Isoleucines and Leucines are replaced with J
        /// </summary>
        public static string ModifiedSequenceforJ(this string sequence)
        {
            if (string.IsNullOrWhiteSpace(sequence)) return string.Empty;

            return  sequence.Replace('I', 'J').Replace('L', 'J');
        }
    }


    /// <summary>
    /// Many confidence levels can provide better way of highlighting
    /// items in the list.
    /// Sure is to say if there is absolute possibility of a sequence tag,
    /// it is determined by saying that there are other sequence tags which 
    /// proves the presence of the current sequence.
    /// Not Sure is to say may be no, just a tiny bit of confidence
    /// Possible to check if there is any evidence for any other mono masses
    /// and hence say if this is good tag
    /// Not Possible is to say no way. No confidence, saying this is worst sequence tag
    /// for match
    /// Gap is a gap with a modification
    /// </summary>
    public enum AminAcidConfidence
    {
        Sure = 12,
        High = 10,
        Medium = 8,
        SearchHit = 7,
        Low = 6,
        Possible = 5,
        NotSure = 4,
        Gap = 3,
        NotPossible = 2,
        Reallybad = 1,
        None = 0,
        BlastTagMatch = -12,  // Blast scores are Negative
        BlastTagMisMatch = -3,
        BlastTagPossible = -6,
        BlastTagHigh = -10
    };
}

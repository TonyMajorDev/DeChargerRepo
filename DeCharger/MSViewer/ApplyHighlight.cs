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

using MassSpectrometry;
using MSViewer.Classes;
using Science.Proteomics;
using SignalProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Visifire.Charts;


namespace MSViewer
{
    public class ApplyHighlighting
    {
        /// <summary>
        /// Sequence Length based on the Amino Acids
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns>Something</returns>
        private static double sequencelength(string sequence)
        {
            double length = 0;

            foreach (char c in sequence)
            {
                if (AminoAcidHelpers.AminoAcids.ContainsKey(c) == false) return double.NaN;
                length += AminoAcidHelpers.AminoAcids[c];
            }

            return length;
        }

        //private static unsafe double sequencelength(string sequence)
        //{
        //    double length = 0;
        //    //for (int i = 0; i < sequence.Length; i++)
        //    //{

        //    ////}
        //    //int strLength = sequence.Length;

        //    fixed (char* pString = sequence)
        //    {
        //        char* pChar = pString;
        //        //for (int i = 0; i < strLength; i++)
        //        while (true)
        //        {
        //            if (*pChar == '\0')
        //                break;
        //            try
        //            {
        //                length += AminoAcidHelpers.AminoAcidMass3[*pChar];
        //            }
        //            catch (Exception ex)
        //            {
        //                return double.NaN;
        //            }
        //            pChar++;

        //            //pString++;
        //        }
        //    }

        //    //foreach (char c in sequence)
        //    //{
        //    //    try
        //    //    {
        //    //        //if (!AminoAcidHelpers.AminoAcidMass3.ContainsKey(c))
        //    //        //    return double.NaN;
        //    //        length += AminoAcidHelpers.AminoAcidMass3[c];
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        return double.NaN;
        //    //    }
        //    //}
        //    return length;
        //}


        //private static unsafe double sequencelength(string sequence)
        //{
        //    double length = 0;
        //    //for (int i = 0; i < sequence.Length; i++)
        //    //{

        //    ////}
        //    int strLength = sequence.Length;

        //    fixed (char* pString = sequence)
        //    {
        //        char* pChar = pString;
        //        //for (int i = 0; i < strLength; i++)
        //        for (int i = 0; ; i++)
        //        {
        //            if (i == strLength) break;
        //            try
        //            {
        //                length += AminoAcidHelpers.AminoAcidMass3[*pChar];
        //            }
        //            catch (Exception ex)
        //            {
        //                return double.NaN;
        //            }
        //            ///lngstrg = lngstrg + *pString;/// *pChar;
        //            pChar++;
                    
        //            //if (*pChar == '\0')
        //                //break;
        //            //pString++;
        //        }
        //    }

        //    //foreach (char c in sequence)
        //    //{
        //    //    try
        //    //    {
        //    //        //if (!AminoAcidHelpers.AminoAcidMass3.ContainsKey(c))
        //    //        //    return double.NaN;
        //    //        length += AminoAcidHelpers.AminoAcidMass3[c];
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        return double.NaN;
        //    //    }
        //    //}
        //    return length;
        //}

        //unsafe char* PtrToStringContent(string s, out GCHandle pin)
        //{
        //    pin = GCHandle.Alloc(s, GCHandleType.Pinned);
        //    return (char*)pin.AddrOfPinnedObject().Add(System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData).ToPointer();
        //}



        //public static Dictionary<string, double> AlltheModifications = new Dictionary<string, double>();

        private static double sequencelengthformodifications(string sequence)
        {
            double length = 0;

            foreach (char c in sequence)
            {
                if (Regex.IsMatch("", ""))
                {
                    foreach (string b in AminoAcidHelpers.ModifiedAminoAcids.Where(a => a.Key.Contains("C")).Select(a => a.Key).ToList())
                    {

                    }
                }
                else
                {

                }
            }
            return length;
        }

        /// <summary>
        /// Sequence Length based on all the match lists. Even the one's which have modification
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static double sequencelengthusingmodifiedaminoacids(string sequence)
        {
            double length = 0;
            string newemptystring = string.Empty;
            try
            {
                int i = 0;
                int j = 0;
                foreach (string a in sequence.Select(a => a.ToString()).ToArray())
                {
                    if (i != j)
                    {
                        j++;
                        i++;
                        j++;
                        continue;
                    }
                    i++;

                    if (Regex.IsMatch(Convert.ToString(sequence[i]), @"^[a-z*]+$"))
                    {
                        length += AminoAcidHelpers.AminoAcidMass2[a + sequence[i]];
                    }
                    else
                    {
                        length += AminoAcidHelpers.AminoAcidMass2[a];
                        j++;
                    }
                }
            }
            catch 
            {
                return double.NaN;
            }
            return length;
        }


        /// <summary>
        /// Find all the indexes for the possible values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        //private static List<int> FindAll(string input, string target)
        //{
        //    int index = 0 - target.Length;
        //    var results = new List<int>();

        //    if (target == "") return results;

        //    do
        //    {
        //        index += target.Length;
        //        index = input.IndexOf(target, index);
        //        if (index >= 0) results.Add(index);

        //    } while (index >= 0);

        //    return results;
        //}

        /// <summary>
        /// Find all the indexes for the possible values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static List<int> FindAll(string input, string target)
        {
            int index = 0 - target.Length;
            var results = new List<int>();

            if (target == "") return results;

            int lengthofinput = input.Length;

            do
            {
                index += target.Length;
                //index = FastIndexOf(input.Substring(index, lengthofinput - index), target);
                index = input.IndexOf(target, index);
                if (index >= 0) results.Add(index);

            } while (index >= 0);

            return results;
        }


        static int FastIndexOf(string source, string pattern)
        {
            if (pattern == null) throw new ArgumentNullException();
            if (pattern.Length == 0) return 0;
            if (pattern.Length == 1) return source.IndexOf(pattern[0]);
            bool found;
            int limit = source.Length - pattern.Length + 1;
            if (limit < 1) return -1;
            // Store the first 2 characters of "pattern"
            char c0 = pattern[0];
            char c1 = pattern[1];
            // Find the first occurrence of the first character
            int first = source.IndexOf(c0, 0, limit);
            while (first != -1)
            {
                // Check if the following character is the same like
                // the 2nd character of "pattern"
                if (source[first + 1] != c1)
                {
                    first = source.IndexOf(c0, ++first, limit - first);
                    continue;
                }
                // Check the rest of "pattern" (starting with the 3rd character)
                found = true;
                for (int j = 2; j < pattern.Length; j++)
                    if (source[first + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                // If the whole word was found, return its index, otherwise try again
                if (found) return first;
                first = source.IndexOf(c0, ++first, limit - first);
            }
            return -1;
        }



        public static List<MatchStartEnds> ApplHighlightingNew(List<FindSequenceTags.SequenceTag> AllSequenceTags, List<string> PairMatch, string BoundText,
            List<string> Allthesqstags, List<MainSequenceTagmatches> Mainsequencematch, List<Cluster> CurrentIons, double ParentMass, ref bool blastpmatch,
            SortedList<char, double> AminoAcidMass = null, bool Dontshowall = false, bool IsBlastp = false, string BlastpTag = null, string BlastpPairMatch = null, List<FindSequenceTags.SequenceTag> NewBlastpMatchTags = null)
        {
            if (AminoAcidMass != null) AminoAcidHelpers.AminoAcids = AminoAcidMass;

            var matchstartends = new List<MatchStartEnds>();

            string matchseq = BoundText;

            string ModifiedSequenceForL = BoundText.Replace('I', 'L');

            List<double> CrntIons = CurrentIons.Select(a => a.MonoMass).ToList();

            CrntIons.Add(0);
            CrntIons.Add(Molecules.Water);
            CrntIons.Add(ParentMass);
            CrntIons.Add(ParentMass - Molecules.Water);

            List<double> ReverseCrntIons = new List<double>();

            ReverseCrntIons = CrntIons.Where(a => a <= ParentMass).Select(a => ParentMass - a).ToList();



            return matchstartends;
        }

        public static List<MatchStartEnds> AssignCoverage(SuperSpectrum spectrum, List<FindSequenceTags.SequenceTag> AllSequenceTags, string proteinSequence)
        {
            //TODO: implement a refactored version of ApplHighlighting 









            return null;
        }



        /// <summary>
        /// Applies highlighting based on the sequence hits of the database with sequence tags from the viewer
        /// </summary>
        public static List<MatchStartEnds> ApplHighlighting(List<FindSequenceTags.SequenceTag> AllSequenceTags, List<string> PairMatch, string BoundText,
            List<string> Allthesqstags, List<MainSequenceTagmatches> Mainsequencematch, List<Cluster> CurrentIons, double ParentMass, ref bool blastpmatch,
            SortedList<char, double> AminoAcidMass = null, bool Dontshowall = false, bool IsBlastp = false, string BlastpTag = null, string BlastpPairMatch = null, List<FindSequenceTags.SequenceTag> NewBlastpMatchTags = null)
        {

            //AlltheModifications.Add("O", 15.99491463);
            //AlltheModifications.Add("Z", 21.9845);

            if (AminoAcidMass != null) AminoAcidHelpers.AminoAcids = AminoAcidMass;

            var AllSqsTags = AllSequenceTags;///.Where(a => a.Sequence.Length >= Properties.Settings.Default.SequenceTagLength).ToList();
            var Allthesequencetags = Allthesqstags;///.Where(a => a.Length >= Properties.Settings.Default.SequenceTagLength).ToList();

            var matchstartends = new List<MatchStartEnds>();

            if (PairMatch != null && PairMatch.Count == 0)
            {
                PairMatch = new List<string>();

                PairMatch.Add(Allthesqstags.OrderByDescending(a => a.Length).First());
            }

            try
            {
                //NumberofTags = 0;
                string matchseq = BoundText;
                string ModifiedSequenceForL = BoundText.Replace('I', 'L');
                List<double> CrntIons = CurrentIons.Select(a => a.MonoMass).ToList();

                CrntIons.Add(0);
                CrntIons.Add(Molecules.Water);
                CrntIons.Add(ParentMass);
                CrntIons.Add(ParentMass - Molecules.Water);

                List<double> ReverseCrntIons = new List<double>();

                ReverseCrntIons = CrntIons.Where(a => a <= ParentMass).Select(a => ParentMass - a).ToList();


                List<string> newPairMatch = new List<string>();

                foreach (string prmtch in PairMatch)
                {
                    if (AllSqsTags.Where(a => a.Sequence == prmtch).Any())
                        newPairMatch.Add(prmtch);
                    else if (AllSqsTags.Where(a => a.ReverseSequence == prmtch).Any())
                        newPairMatch.Add(ReverseString.Reverse(prmtch));
                }

                if (!newPairMatch.Any())
                {
                    foreach (string prmtch in PairMatch)
                    {
                        if (AllSqsTags.Where(a => a.Sequence.Contains(prmtch)).Any())
                        {
                            newPairMatch.Add(prmtch);
                            FindSequenceTags.SequenceTag match = AllSqsTags.Where(a => a.Sequence.Contains(prmtch)).First();
                            int startofsequence = match.Sequence.IndexOf(prmtch);
                            int endofsequence = startofsequence + prmtch.Length;
                            FindSequenceTags.SequenceTag newsequencetag = new FindSequenceTags.SequenceTag();
                            List<FindSequenceTags.AminoAcids> newlistaminoacids = new List<FindSequenceTags.AminoAcids>();
                            foreach (FindSequenceTags.AminoAcids aminoacids in match.IndividualAAs.GetRange(startofsequence, prmtch.Length))
                            {
                                newlistaminoacids.Add(aminoacids);
                            }
                            newsequencetag.Start = newlistaminoacids.Min(a => a.Start);
                            newsequencetag.End = newlistaminoacids.Max(a => a.End);
                            newsequencetag.Index = match.Index + prmtch;
                            newsequencetag.MaxScore = match.MaxScore;
                            newsequencetag.RawSequence = match.RawSequence.Substring(startofsequence, prmtch.Length);
                            newsequencetag.Score = match.Score;
                            newsequencetag.totalScore = match.totalScore;
                            newsequencetag.IndividualAAs = newlistaminoacids;
                            AllSqsTags.Add(newsequencetag);
                        }
                        else if (AllSqsTags.Where(a => a.ReverseSequence.Contains(prmtch)).Any())
                        {
                            newPairMatch.Add(ReverseString.Reverse(prmtch));
                            FindSequenceTags.SequenceTag match = AllSqsTags.Where(a => a.ReverseSequence.Contains(prmtch)).First();
                            int startofsequence = match.ReverseSequence.IndexOf(prmtch);
                            FindSequenceTags.SequenceTag newsequencetag = new FindSequenceTags.SequenceTag();
                            List<FindSequenceTags.AminoAcids> newlistaminoacids = new List<FindSequenceTags.AminoAcids>();
                            match.IndividualAAs.Reverse();
                            foreach (FindSequenceTags.AminoAcids aminoacids in match.IndividualAAs.GetRange(startofsequence, prmtch.Length))
                            {
                                newlistaminoacids.Add(new FindSequenceTags.AminoAcids()
                                {
                                    End = ParentMass - aminoacids.End,
                                    Start = ParentMass - aminoacids.Start,
                                    EndScore = aminoacids.EndScore,
                                    StartScore = aminoacids.StartScore,
                                    Name = aminoacids.Name,
                                    Score = aminoacids.Score
                                });
                            }
                            newsequencetag.IndividualAAs = newlistaminoacids;
                            newsequencetag.Start = newlistaminoacids.Min(a => a.Start);
                            newsequencetag.End = newlistaminoacids.Max(a => a.End);
                            newsequencetag.Index = match.Index + prmtch;
                            newsequencetag.MaxScore = match.MaxScore;
                            newsequencetag.RawSequence = match.RawSequence.Substring(startofsequence, prmtch.Length).ToArray().Reverse().ToString();
                            newsequencetag.Score = match.Score;
                            newsequencetag.totalScore = match.totalScore;
                            AllSqsTags.Add(newsequencetag);
                        }
                    }
                }

                int lengthofmodifiedsequenceforl = ModifiedSequenceForL.Length;
                bool forwardorreverse = false;
                List<double> Monomasses = new List<double>();

                string mainsequencetag = PairMatch.Any() != false ? PairMatch.First() : Allthesequencetags.OrderByDescending(a => a.Length).First();
                List<string> PairMatchwithLforI = PairMatch.Select(a => a.Replace("I", "L")).ToList();
                double startvaluetocompare = 0;
                double endvaluetocompare = 0;
                char[] reversechar = mainsequencetag.ToCharArray();
                matchstartends.Clear();
                Array.Reverse(reversechar);

                int length = BoundText.Length;

                List<string> ReversePairMatches = new List<string>();

                foreach (string s in PairMatchwithLforI)
                {
                    char[] rvrschar = s.ToCharArray();
                    Array.Reverse(rvrschar);
                    ReversePairMatches.Add(new string(rvrschar));
                }

                char[] reverse = ModifiedSequenceForL.ToCharArray();

                Array.Reverse(reverse);

                string reversesequence = new string(reverse);///Also have the reverse sequence

                List<PairMatch> matcheses = new List<PairMatch>();
                ///Find if there are any Sequence tags which match the current Sequence given.
                foreach (string prmatch in PairMatchwithLforI)
                {
                    List<int> indexofallpairmatch = new List<int>();
                    ///Finding if there are any sequencetags which match the entered pair
                    foreach (string pr in Allthesequencetags.Where(a => a.Contains(prmatch)))
                    {
                        ///If its in the forward directin
                        int indexofpairmatch = ModifiedSequenceForL.IndexOf(prmatch);
                        if (indexofpairmatch != -1)
                        {
                            indexofallpairmatch = FindAll(ModifiedSequenceForL, prmatch);
                            foreach (int indexofpairmatc in indexofallpairmatch)
                            {
                                matcheses.Add(new Classes.PairMatch
                                {
                                    PairMtch = prmatch,
                                    Forward = true,
                                    Start = indexofpairmatc,
                                    End = indexofpairmatc + prmatch.Length
                                });
                            }
                        }
                        ///If its in the reverse direction
                        else if (reversesequence.IndexOf(prmatch) != -1)
                        {
                            indexofallpairmatch = FindAll(reversesequence, prmatch);
                            foreach (int indexofreversepairmatch in indexofallpairmatch)
                            {
                                matcheses.Add(new PairMatch
                                {
                                    PairMtch = prmatch,
                                    Forward = false,
                                    Start = length - (indexofreversepairmatch + prmatch.Length),
                                    End = length - (indexofreversepairmatch),
                                });
                            }
                        }
                    }
                    ///Checking if any of the sequence tags are part of the pairmatch
                    foreach (string pr in Allthesequencetags.Where(a => prmatch.Contains(a)))
                    {
                        int indexofpairmatch = ModifiedSequenceForL.IndexOf(pr);
                        if (indexofpairmatch != -1)
                        {
                            indexofallpairmatch = FindAll(ModifiedSequenceForL, pr);
                            foreach (int indexofpairmatc in indexofallpairmatch)
                            {
                                matcheses.Add(new PairMatch
                                {
                                    PairMtch = pr,
                                    Forward = true,
                                    Start = indexofpairmatc,
                                    End = indexofpairmatc + pr.Length
                                });
                            }
                        }
                        else if (reversesequence.IndexOf(prmatch) != -1)
                        {
                            indexofallpairmatch = FindAll(reversesequence, pr);
                            foreach (int indexofreversepairmatch in indexofallpairmatch)
                            {
                                matcheses.Add(new PairMatch
                                {
                                    PairMtch = pr,
                                    Forward = false,
                                    Start = length - (indexofreversepairmatch + pr.Length),
                                    End = length - (indexofreversepairmatch),
                                });
                            }
                        }
                    }
                }
                ///Finding if any of the reversepairmatches are part of the sequence tag list or if any of the
                ///sequence tag list part of the reversepairmatches
                foreach (string prmatch in ReversePairMatches)
                {
                    List<int> indexofallpairmatch = new List<int>();

                    foreach (string pr in Allthesequencetags.Where(a => a.Contains(prmatch)))
                    {
                        int indexofpairmatch = ModifiedSequenceForL.IndexOf(prmatch);
                        if (indexofpairmatch != -1)
                        {
                            indexofallpairmatch = FindAll(ModifiedSequenceForL, pr);
                            foreach (int indexofpairmatc in indexofallpairmatch)
                            {
                                matcheses.Add(new Classes.PairMatch
                                {
                                    PairMtch = pr,
                                    Forward = false,
                                    Start = indexofpairmatc,
                                    End = indexofpairmatc + pr.Length
                                });
                            }
                        }
                        else if (reversesequence.IndexOf(prmatch) != -1)
                        {
                            indexofallpairmatch = FindAll(reversesequence, pr);
                            foreach (int indexofreversepairmatch in indexofallpairmatch)
                            {
                                matcheses.Add(new PairMatch
                                {
                                    PairMtch = pr,
                                    Forward = false,
                                    Start = length - (indexofreversepairmatch + pr.Length),
                                    End = length - (indexofreversepairmatch),
                                });
                            }
                        }
                    }
                    foreach (string pr in Allthesequencetags.Where(a => prmatch.Contains(a)))
                    {
                        int indexofpairmatch = ModifiedSequenceForL.IndexOf(pr);
                        if (indexofpairmatch != -1)
                        {
                            indexofallpairmatch = FindAll(ModifiedSequenceForL, pr);
                            foreach (int indexofpairmatc in indexofallpairmatch)
                            {
                                matcheses.Add(new PairMatch
                                {
                                    PairMtch = pr,
                                    Forward = false,
                                    Start = indexofpairmatc,
                                    End = indexofpairmatc + pr.Length
                                });
                            }
                        }
                        else if (reversesequence.IndexOf(prmatch) != -1)
                        {
                            indexofallpairmatch = FindAll(reversesequence, pr);
                            foreach (int indexofreversepairmatch in indexofallpairmatch)
                            {
                                matcheses.Add(new PairMatch
                                {
                                    PairMtch = pr,
                                    Forward = false,
                                    Start = length - (indexofreversepairmatch + pr.Length),
                                    End = length - (indexofreversepairmatch),
                                });
                            }
                        }
                    }
                }
                foreach (string pr in AllSqsTags.Select(a => a.Sequence).ToList())
                {
                    int indexofpairmatch = ModifiedSequenceForL.IndexOf(pr);
                    List<int> indexofallpairmatch = new List<int>();
                    if (indexofpairmatch != -1)
                    {
                        indexofallpairmatch = FindAll(ModifiedSequenceForL, pr);
                        foreach (int indexofpairmatc in indexofallpairmatch)
                        {
                            matcheses.Add(new PairMatch
                            {
                                PairMtch = pr,
                                Forward = true,
                                Start = indexofpairmatc,
                                End = indexofpairmatc + pr.Length
                            });
                        }
                    }
                    else if (reversesequence.IndexOf(pr) != -1)
                    {
                        indexofallpairmatch = FindAll(reversesequence, pr);
                        foreach (int indexofreversepairmatch in indexofallpairmatch)
                        {
                            matcheses.Add(new PairMatch
                            {
                                PairMtch = pr,
                                Forward = false,
                                Start = length - (indexofreversepairmatch + pr.Length),
                                End = length - (indexofreversepairmatch),
                            });
                        }
                    }
                }

                matcheses = (from m in matcheses
                             group m by new
                             {
                                 m.End,
                                 m.Forward,
                                 m.PairMtch,
                                 m.Start
                             } into mm
                             select new PairMatch()
                             {
                                 End = mm.Key.End,
                                 Forward = mm.Key.Forward,
                                 PairMtch = mm.Key.PairMtch,
                                 Start = mm.Key.Start
                             }).ToList();


                matcheses = matcheses.Where(a => a.PairMtch.Length >= 4).ToList();

                if (matcheses.Count == 0)
                {
                    return new List<MatchStartEnds>();
                }

                List<PairMatch> newmatcheses = new List<PairMatch>();
                int indexofmatchpair = 0;

                for (int iii = 0; iii <= matcheses.Count - 1; iii++)
                {
                    matcheses[iii].Index = iii;
                    indexofmatchpair = iii;
                }
                newmatcheses = matcheses;


                // Find tags that don't match our Protein Sequence and remove them from newmatcheses
                matcheses = matcheses.OrderByDescending(a => a.PairMtch.Length).ToList();
                List<PairMatch> removedmatcheses = new List<PairMatch>();
                int imatchstartends = 1;
                foreach (PairMatch mtse in matcheses)
                {
                    if (removedmatcheses.Contains(mtse))
                    {
                        imatchstartends++;
                        continue;
                    }
                    foreach (PairMatch mt in matcheses.GetRange(imatchstartends, matcheses.Count - imatchstartends))
                    {
                        if (mtse.Start <= mt.Start && mtse.End >= mt.End)
                        {
                            newmatcheses.Remove(matcheses.Where(a => a.Index == mt.Index).First());
                            removedmatcheses.Add(matcheses.Where(a => a.Index == mt.Index).First());
                        }
                    }
                    imatchstartends++;
                }


                // Eliminate tags that don't match our Protein Sequence
                matcheses = newmatcheses.OrderByDescending(a => a.PairMtch.Length).ThenByDescending(a => a.Start).ToList();
                removedmatcheses = new List<PairMatch>();
                imatchstartends = 1;
                foreach (PairMatch mtse in matcheses)
                {
                    if (removedmatcheses.Contains(mtse))
                    {
                        imatchstartends++;
                        continue;
                    }
                    foreach (PairMatch mt in matcheses.GetRange(imatchstartends, matcheses.Count - imatchstartends))
                    {
                        if (mtse.Start >= mt.Start && mtse.Start <= mt.End && mtse.End >= mt.End)
                        {
                            newmatcheses.Remove(matcheses.Where(a => a.Index == mt.Index).First());
                            removedmatcheses.Add(matcheses.Where(a => a.Index == mt.Index).First());
                        }
                    }
                    imatchstartends++;
                }

                matcheses = newmatcheses.OrderByDescending(a => a.PairMtch.Length).ThenByDescending(a => a.Start).ToList();
                removedmatcheses = new List<PairMatch>();
                imatchstartends = 1;

                foreach (PairMatch mtse in matcheses)
                {
                    if (removedmatcheses.Contains(mtse))
                    {
                        imatchstartends++;
                        continue;
                    }
                    foreach (PairMatch mt in matcheses.GetRange(imatchstartends, matcheses.Count - imatchstartends))
                    {
                        if (mtse.Start <= mt.Start && mtse.End >= mt.Start && mtse.End <= mt.End)
                        {
                            newmatcheses.Remove(matcheses.Where(a => a.Index == mt.Index).First());
                            removedmatcheses.Add(matcheses.Where(a => a.Index == mt.Index).First());
                        }
                    }
                    imatchstartends++;
                }

                matcheses = newmatcheses.OrderByDescending(a => a.PairMtch.Length).ThenByDescending(a => a.Start).ToList();

                // add all the necessary details for these tags to the matchstartends collection
                foreach (PairMatch prmatch in matcheses)
                {
                    int indexofpairmatch = ModifiedSequenceForL.IndexOf(prmatch.PairMtch);
                    if (prmatch.Forward)
                    {
                        // foreach (int indexofpairmatc in indexofallpairmatch)
                        {
                            List<double> mms = new List<double>();

                            foreach (var insqstags in AllSqsTags.Where(a => (a.Sequence) == (prmatch.PairMtch)))
                            {
                                mms.AddRange(insqstags.IndividualAAs.Select(a => a.Start).ToList());
                                mms.AddRange(insqstags.IndividualAAs.Select(a => a.End).ToList());
                            }
                            mms = mms.OrderBy(a => a).ToList();
                            if (mms.Count > 0)
                            {
                                matchstartends.Add(new MatchStartEnds
                                {
                                    Index = indexofmatchpair,
                                    confidence = AminAcidConfidence.High,
                                    Start = prmatch.Start,
                                    End = prmatch.End,
                                    SequenceTag = prmatch.PairMtch,
                                    MainSequence = true,
                                    ReverseSequence = false,
                                    MonoMasses = mms,
                                    EndMonoMass = mms.Last(),
                                    StartMonoMass = mms.First(),
                                    ReverseSpectrum = prmatch.Forward,
                                    DontShowall = Dontshowall
                                });
                                indexofmatchpair++;
                            }
                        }
                    }
                    else
                    {
                        {
                            List<double> mms = new List<double>();
                            foreach (var insqstags in AllSqsTags.Where(a => a.Sequence == (prmatch.PairMtch)))
                            {
                                mms.AddRange(insqstags.IndividualAAs.Select(a => a.Start).ToList());
                                mms.AddRange(insqstags.IndividualAAs.Select(a => a.End).ToList());
                            }
                            mms = mms.OrderBy(a => a).ToList();
                            if (mms.Count > 0)
                            {
                                matchstartends.Add(new MatchStartEnds
                                {
                                    Index = indexofmatchpair,
                                    confidence = AminAcidConfidence.High,
                                    Start = prmatch.Start,
                                    End = prmatch.End,
                                    SequenceTag = prmatch.PairMtch,
                                    ReverseSequence = true,
                                    MainSequence = false,
                                    MonoMasses = mms,
                                    EndMonoMass = mms.First() + sequencelength(prmatch.PairMtch),
                                    StartMonoMass = mms.First(),
                                    ReverseSpectrum = prmatch.Forward,
                                    DontShowall = Dontshowall
                                });
                                indexofmatchpair++;
                            }
                        }
                    }
                }


                ///Figure out how well is a sequence tag matches or correlates to other sequences in the list.
                ///Based on it the score of a particular sequence tag is obtained and a delta for the entire sequence
                ///hence can sort it based on the delta.
                #region PairMatching

                List<MatchStartEnds> newmatchstartends = new List<MatchStartEnds>();

                matchstartends = matchstartends.OrderByDescending(a => a.Length).ToList();

                newmatchstartends.AddRange(matchstartends);
                imatchstartends = 1;

                matchstartends = newmatchstartends;///.OrderBy(a => a.Start).ToList();

                matchstartends = matchstartends.OrderBy(a => a.Start).ToList();

                imatchstartends = 1;
                //string mste = matchstartends.First().SequenceTag;

                ///Once all the matches are found within a sequence for the all the tags and also the sequences entered for search
                ///then need to look at the gaps between those tags and if they match perfectly then call it a sequence tag,
                ///if there is a large gap then fill the gap with a number
                List<double> reversecrntIOns = new List<double>();
                CurrentIons ions = new CurrentIons();
                if (matchstartends.Count > 1)
                {
                    foreach (MatchStartEnds mtse in matchstartends)
                    {
                        foreach (MatchStartEnds mt in matchstartends.GetRange(imatchstartends, matchstartends.Count - imatchstartends))
                        {
                            if (mtse == mt) continue;
                            ///Checking if any of the sequence tags is a Palindrome. If not then the direction of the sequence tag is known 
                            if (!(Enumerable.Reverse(mt.SequenceTag).SequenceEqual(mt.SequenceTag) || Enumerable.Reverse(mtse.SequenceTag).SequenceEqual(mtse.SequenceTag)))
                            {
                                if (mtse.MainSequence == mt.MainSequence) ///Finding if both are in the same direction
                                {
                                    if (mtse.MainSequence)/// < mt.Start)
                                    {
                                        if (Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))) <= PPM.CurrentPPMbasedonMatchList(Math.Max(mtse.EndMonoMass, mt.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM))
                                        {
                                            indexofmatchpair++;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                            //FindmatchStartendsintherange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, mt.Start , ions, ParentMass, mtse.End - 1, 
                                            //Findmatchesinrange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, mtse.End, Dontshowall, ions, mtse.End, ParentMass);

                                            //if (ions.currentions.Count > 0)
                                            //{
                                            //    Findmatchesinrange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, Math.Min(mt.Start, mtse.End), Dontshowall, ions.currentions, newmatchstartends.Count, ParentMass);
                                            //}

                                            newmatchstartends.Add(new MatchStartEnds
                                            {
                                                confidence = Math.Abs(mt.Start - mtse.End) == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                                End = Math.Max(mt.Start, mtse.End),
                                                Start = Math.Min(mt.Start, mtse.End),
                                                EndMonoMass = mt.StartMonoMass,
                                                StartMonoMass = mtse.EndMonoMass,
                                                Index = indexofmatchpair,
                                                MainSequence = ions.directionformatchstartends,
                                                ReverseSequence = !ions.directionformatchstartends,
                                                SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                                MonoMasses = ions.currentions,
                                                DontShowall = Dontshowall
                                            });
                                            break;
                                        }
                                        else
                                        {
                                            double lengthofmaxmod = LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)));
                                            ///Checking if the difference between the startmonomass and endmonomass is less than the max possible length for modification
                                            //if (Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) <= lengthofmaxmod)
                                            //{
                                            //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), Math.Abs(mtse.EndMonoMass - mt.StartMonoMass), PPM.CurrentPPMbasedonMatchList(Math.Max(mtse.EndMonoMass, mt.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM));
                                            //    if (modifiedsequence != "")
                                            //    {
                                            //        indexofmatchpair++;
                                            //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                            //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                            //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                            //        newmatchstartends.Add(new MatchStartEnds
                                            //        {
                                            //            confidence = Math.Abs(mt.Start - mtse.End) == 1 ? Confidence.Sure : Confidence.Low,
                                            //            End = Math.Max(mt.Start, mtse.End),
                                            //            Start = Math.Min(mt.Start, mtse.End),
                                            //            EndMonoMass = mt.StartMonoMass,
                                            //            StartMonoMass = mtse.EndMonoMass,
                                            //            Index = indexofmatchpair,
                                            //            MainSequence = ions.directionformatchstartends,
                                            //            ReverseSequence = !ions.directionformatchstartends,
                                            //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                            //            RawSequenceTag = modifiedsequence,
                                            //            MonoMasses = ions.currentions,
                                            //        });
                                            //        break;
                                            //    }
                                            //}
                                            ///Checking if there is any delta mass between the two tags
                                            //if (AlltheModifications.Any(a => a.Value == Math.Round(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 4), 4) ))
                                            //{
                                            //    indexofmatchpair++;
                                            //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                            //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                            //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                            //    newmatchstartends.Add(new MatchStartEnds
                                            //    {
                                            //        confidence = Confidence.Low,
                                            //        End = Math.Max(mt.Start, mtse.End),
                                            //        Start = Math.Min(mtse.End, mt.Start),
                                            //        EndMonoMass = mtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            //        StartMonoMass = mtse.EndMonoMass,
                                            //        Index = indexofmatchpair,
                                            //        MainSequence = ions.directionformatchstartends,
                                            //        ReverseSequence = !ions.directionformatchstartends,
                                            //        MonoMasses = ions.currentions,
                                            //        SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)) + (AlltheModifications.Where(a => a.Value == Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))).First().Key),
                                            //        //Gap = Math.Round(((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))), 4),
                                            //        DontShowall = Dontshowall
                                            //    });
                                            //    break;
                                            //}
                                            //else
                                            if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End)) >= Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))))
                                            {
                                                indexofmatchpair++;
                                                newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                                newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                                newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                                newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                                ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                                newmatchstartends.Add(new MatchStartEnds
                                                {
                                                    confidence = AminAcidConfidence.Gap,
                                                    End = Math.Max(mt.Start, mtse.End),
                                                    Start = Math.Min(mtse.End, mt.Start),
                                                    EndMonoMass = mtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                                    StartMonoMass = mtse.EndMonoMass,
                                                    Index = indexofmatchpair,
                                                    MainSequence = ions.directionformatchstartends,
                                                    ReverseSequence = !ions.directionformatchstartends,
                                                    MonoMasses = ions.currentions,
                                                    SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                                    Gap = Math.Round(((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))), 4),
                                                    DontShowall = Dontshowall
                                                });
                                                break;
                                            }


                                            //if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End)) >= Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))))
                                            //{
                                            //    indexofmatchpair++;
                                            //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                            //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                            //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                            //    newmatchstartends.Add(new MatchStartEnds
                                            //    {
                                            //        confidence = Confidence.Gap,
                                            //        End = Math.Max(mt.Start, mtse.End),
                                            //        Start = Math.Min(mtse.End, mt.Start),
                                            //        EndMonoMass = mtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            //        StartMonoMass = mtse.EndMonoMass,
                                            //        Index = indexofmatchpair,
                                            //        MainSequence = ions.directionformatchstartends,
                                            //        ReverseSequence = !ions.directionformatchstartends,
                                            //        MonoMasses = ions.currentions,
                                            //        SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                            //        Gap = Math.Round(((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))), 4),
                                            //        DontShowall = Dontshowall
                                            //    });
                                            //    break;
                                            //}
                                            ///Checking if the mass difference is less than the max modification and delta mass
                                            //else if (Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) <= (lengthofmaxmod + DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End))))
                                            //{
                                            //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) , DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End)));
                                            //    if (modifiedsequence != "")
                                            //    {
                                            //        indexofmatchpair++;
                                            //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                            //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                            //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                            //        newmatchstartends.Add(new MatchStartEnds
                                            //        {
                                            //            confidence = Confidence.Gap,
                                            //            End = Math.Max(mt.Start, mtse.End),
                                            //            Start = Math.Min(mtse.End, mt.Start),
                                            //            EndMonoMass = mtse.EndMonoMass + sequencelengthformodifications(modifiedsequence),
                                            //            StartMonoMass = mtse.EndMonoMass,
                                            //            Index = indexofmatchpair,
                                            //            MainSequence = ions.directionformatchstartends,
                                            //            ReverseSequence = !ions.directionformatchstartends,
                                            //            MonoMasses = ions.currentions,
                                            //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                            //            Gap = Math.Round(((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))), 4),
                                            //            RawSequenceTag = modifiedsequence,
                                            //        });
                                            //        break;
                                            //    }
                                            //}
                                            else if (ParentMass <= Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))))
                                            {
                                                indexofmatchpair++;
                                                newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                                newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                                ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                                newmatchstartends.Add(new MatchStartEnds
                                                {
                                                    confidence = AminAcidConfidence.NotPossible,
                                                    End = mt.Start,
                                                    Start = mtse.End,
                                                    EndMonoMass = mt.StartMonoMass,
                                                    StartMonoMass = mtse.EndMonoMass,
                                                    Index = indexofmatchpair,
                                                    MainSequence = ions.directionformatchstartends,
                                                    ReverseSequence = !ions.directionformatchstartends,
                                                    SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                                    MonoMasses = ions.currentions,
                                                    DontShowall = Dontshowall
                                                });
                                                break;
                                            }
                                        }
                                    }
                                    else if (!mtse.MainSequence)
                                    {
                                        if ((mtse.End + (mtse.End - mt.Start)) > lengthofmodifiedsequenceforl) continue;
                                        if (Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))) <= PPM.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM))
                                        {
                                            indexofmatchpair++;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                            ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                            //if (ions.currentions.Count > 0)
                                            //{
                                            //    Findmatchesinrange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), ModifiedSequenceForL, Math.Min(mt.Start, mtse.End), Dontshowall, ions.currentions, newmatchstartends.Count, ParentMass);
                                            //}

                                            newmatchstartends.Add(new MatchStartEnds
                                            {
                                                confidence = AminAcidConfidence.Low,
                                                End = Math.Max(mt.Start, mtse.End),
                                                Start = Math.Min(mt.Start, mtse.End),
                                                EndMonoMass = mtse.StartMonoMass,
                                                StartMonoMass = mt.EndMonoMass,
                                                Index = indexofmatchpair,
                                                MainSequence = ions.directionformatchstartends,
                                                ReverseSequence = !ions.directionformatchstartends,
                                                MonoMasses = ions.currentions,
                                                SequenceTag = ReverseString.Reverse(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))),
                                                DontShowall = Dontshowall
                                            });
                                            break;
                                        }
                                        else
                                        {
                                            //double lengthofmaxmod = LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)));
                                            //if (Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) <= lengthofmaxmod)
                                            //{
                                            //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), Math.Abs(mt.EndMonoMass - mtse.StartMonoMass), PPM.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM));
                                            //    if (modifiedsequence != "")
                                            //    {
                                            //        indexofmatchpair++;
                                            //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                            //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                            //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                            //        newmatchstartends.Add(new MatchStartEnds
                                            //        {
                                            //            confidence = Confidence.Low,
                                            //            End = Math.Max(mt.Start, mtse.End),
                                            //            Start = Math.Min(mt.Start, mtse.End),
                                            //            EndMonoMass = mtse.StartMonoMass,
                                            //            StartMonoMass = mt.EndMonoMass,
                                            //            Index = indexofmatchpair,
                                            //            MainSequence = ions.directionformatchstartends,
                                            //            ReverseSequence = !ions.directionformatchstartends,
                                            //            RawSequenceTag = modifiedsequence,
                                            //            MonoMasses = ions.currentions,
                                            //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))
                                            //        });
                                            //        break;
                                            //    }
                                            //}
                                            //if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.Start - mt.End)) >= Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))))
                                            //if (AlltheModifications.Any(a => a.Value == Math.Round(Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))), 4)),4)))
                                            //{
                                            //    indexofmatchpair++;
                                            //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                            //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                            //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                            //    double startrange = Math.Min(mt.EndMonoMass, mtse.StartMonoMass);
                                            //    double endrange = Math.Max(mt.EndMonoMass, mtse.StartMonoMass);



                                            //    //List<MatchStartEnds> mtses = FindMatchingAminoAcids(singleaminoacids.Where(a => a.Start >= startrange && a.End <= endrange).ToList(),
                                            //    //                                                    singleaminoacids.Where(a => a.Start >= (ParentMass - endrange) && a.End <= (ParentMass - startrange)).ToList(),
                                            //    //                                                    ReverseString.ReverseStr(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))),
                                            //    //                                                    startrange, endrange, ParentMass, Math.Min(mtse.End, mt.Start), Math.Max(mtse.End, mt.Start),
                                            //    //                                                    ref indexofmatchpair, Dontshowall, false);
                                            //    //if (mtses.Count > 0)
                                            //    //{
                                            //    //    newmatchstartends.AddRange(mtses);
                                            //    //}
                                            //    //else
                                            //    //{
                                            //    newmatchstartends.Add(new MatchStartEnds
                                            //    {
                                            //        confidence = Confidence.Low,
                                            //        End = Math.Max(mt.Start, mtse.End),
                                            //        Start = Math.Min(mt.Start, mtse.End),
                                            //        EndMonoMass = mt.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            //        StartMonoMass = mt.EndMonoMass,
                                            //        Index = indexofmatchpair,
                                            //        MainSequence = ions.directionformatchstartends,
                                            //        ReverseSequence = !ions.directionformatchstartends,
                                            //        MonoMasses = ions.currentions,
                                            //        SequenceTag = ReverseString.ReverseStr(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))) + (AlltheModifications.Where(a => a.Value == Math.Round(Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))), 4)), 4)).First().Key),
                                            //        //Gap = Math.Round(((Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))), 4),
                                            //        DontShowall = Dontshowall
                                            //    });
                                            //    //}
                                            //    break;
                                            //}
                                            //else 

                                            if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.Start - mt.End)) >= Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))))
                                            {
                                                indexofmatchpair++;
                                                newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                                newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                                newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                                newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                                ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                                double startrange = Math.Min(mt.EndMonoMass, mtse.StartMonoMass);
                                                double endrange = Math.Max(mt.EndMonoMass, mtse.StartMonoMass);

                                                //List<MatchStartEnds> mtses = FindMatchingAminoAcids(singleaminoacids.Where(a => a.Start >= startrange && a.End <= endrange).ToList(),
                                                //                                                    singleaminoacids.Where(a => a.Start >= (ParentMass - endrange) && a.End <= (ParentMass - startrange)).ToList(),
                                                //                                                    ReverseString.ReverseStr(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))),
                                                //                                                    startrange, endrange, ParentMass, Math.Min(mtse.End, mt.Start), Math.Max(mtse.End, mt.Start),
                                                //                                                    ref indexofmatchpair, Dontshowall, false);
                                                //if (mtses.Count > 0)
                                                //{
                                                //    newmatchstartends.AddRange(mtses);
                                                //}
                                                //else
                                                //{
                                                newmatchstartends.Add(new MatchStartEnds
                                                {
                                                    confidence = AminAcidConfidence.Gap,
                                                    End = Math.Max(mt.Start, mtse.End),
                                                    Start = Math.Min(mt.Start, mtse.End),
                                                    EndMonoMass = mt.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                                    StartMonoMass = mt.EndMonoMass,
                                                    Index = indexofmatchpair,
                                                    MainSequence = ions.directionformatchstartends,
                                                    ReverseSequence = !ions.directionformatchstartends,
                                                    MonoMasses = ions.currentions,
                                                    SequenceTag = ReverseString.Reverse(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))),
                                                    Gap = Math.Round(((Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))), 4),
                                                    DontShowall = Dontshowall
                                                });
                                                //}
                                                break;
                                            }
                                            //else if (Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) <= (lengthofmaxmod + DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.Start - mt.End))))
                                            //{
                                            //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), Math.Abs(mt.EndMonoMass - mtse.StartMonoMass), DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.Start - mt.End)));
                                            //    if (modifiedsequence != "")
                                            //    {
                                            //        indexofmatchpair++;
                                            //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                            //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                            //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                            //        newmatchstartends.Add(new MatchStartEnds
                                            //        {
                                            //            confidence = Confidence.Gap,
                                            //            End = Math.Max(mt.Start, mtse.End),
                                            //            Start = Math.Min(mt.Start, mtse.End),
                                            //            EndMonoMass = mt.EndMonoMass + sequencelengthformodifications(modifiedsequence),
                                            //            StartMonoMass = mt.EndMonoMass,
                                            //            Index = indexofmatchpair,
                                            //            MainSequence = ions.directionformatchstartends,
                                            //            ReverseSequence = !ions.directionformatchstartends,
                                            //            MonoMasses = ions.currentions,
                                            //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)),
                                            //            Gap = Math.Round(((Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))), 4),
                                            //            RawSequenceTag = modifiedsequence
                                            //        });
                                            //        break;
                                            //    }
                                            //}
                                            else if (ParentMass <= Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))))
                                            {
                                                indexofmatchpair++;
                                                newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mtse.Start - mt.End);
                                                newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mtse.Start - mt.End);
                                                ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                                newmatchstartends.Add(new MatchStartEnds
                                                {
                                                    confidence = AminAcidConfidence.NotPossible,
                                                    End = Math.Max(mt.Start, mtse.End),
                                                    Start = Math.Min(mt.Start, mtse.End),
                                                    EndMonoMass = mt.StartMonoMass,
                                                    StartMonoMass = mtse.EndMonoMass,
                                                    Index = indexofmatchpair,
                                                    MainSequence = ions.directionformatchstartends,
                                                    ReverseSequence = !ions.directionformatchstartends,
                                                    SequenceTag = ReverseString.Reverse(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))),
                                                    MonoMasses = ions.currentions,
                                                    DontShowall = Dontshowall
                                                });
                                                break;
                                            }
                                        }
                                    }
                                }
                                else ///if(!mt.MainSequence && mtse.MainSequence) ///If they are in the reverse direction
                                {
                                    try
                                    ///if (mtse.ReverseSpectrum)
                                    {
                                        double prntmass = Math.Round(ParentMass, 1);
                                        if ((mtse.End + Math.Abs(mt.Start - mtse.End)) > lengthofmodifiedsequenceforl) continue;
                                        if (Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(mt.EndMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1) <= Math.Round(PPM.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1))
                                        {
                                            indexofmatchpair++;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                            //List<double> forwardions = CrntIons.Where(a => (a.MonoMass >= mtse.EndMonoMass) && (a.MonoMass <= prntmass - mt.EndMonoMass)).Select(a => a.MonoMass).ToList();
                                            //List<double> reverseions = forwardions.Select(a => ParentMass - a).ToList();

                                            ///newmatchstartends.AddRange(FindmatchStartendsintherange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, Math.Min(mt.Start, mtse.End), forwardions ,
                                            ///ParentMass, indexofmatchpair, 0, true, true, reverseions, Dontshowall, Math.Min(mt.Start, mtse.End)));

                                            newmatchstartends.Add(new MatchStartEnds
                                            {
                                                confidence = Math.Abs(mtse.End - mt.Start) == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                                End = Math.Max(mt.Start, mtse.End),
                                                Start = Math.Min(mt.Start, mtse.End),
                                                EndMonoMass = mtse.EndMonoMass + Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1),
                                                StartMonoMass = mtse.EndMonoMass,
                                                Index = indexofmatchpair,
                                                MainSequence = ions.directionformatchstartends,
                                                ReverseSequence = !ions.directionformatchstartends,
                                                SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)),
                                                MonoMasses = ions.currentions,
                                                DontShowall = Dontshowall
                                            });
                                            break;
                                        }
                                        else if (Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1) <= Math.Round(PPM.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1))
                                        {
                                            indexofmatchpair++;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                            //if (ions.currentions.Count > 0)
                                            //{
                                            //    Findmatchesinrange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, Math.Min(mt.Start, mtse.End), Dontshowall, ions.currentions, newmatchstartends.Count, ParentMass);
                                            //}
                                            newmatchstartends.Add(new MatchStartEnds
                                            {
                                                confidence = Math.Abs(mtse.End - mt.Start) == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                                End = Math.Max(mt.Start, mtse.End),
                                                Start = Math.Min(mt.Start, mtse.End),
                                                EndMonoMass = mtse.EndMonoMass + Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1),
                                                StartMonoMass = mtse.EndMonoMass,
                                                Index = indexofmatchpair,
                                                MainSequence = ions.directionformatchstartends,
                                                ReverseSequence = !ions.directionformatchstartends,
                                                SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)),
                                                MonoMasses = ions.currentions,
                                                DontShowall = Dontshowall
                                            });
                                            break;
                                        }
                                        //else if (Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(mt.EndMonoMass - prntmass))), 1) <= LengthofSequencewithMaxModification((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))) + Math.Round(PPM.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1)))
                                        //{
                                        //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), Math.Abs(mtse.EndMonoMass - Math.Abs(mt.EndMonoMass - prntmass)), Math.Round(PPM.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1));
                                        //    if (modifiedsequence != "")
                                        //    {
                                        //        indexofmatchpair++;
                                        //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                        //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                        //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                        //        newmatchstartends.Add(new MatchStartEnds
                                        //        {
                                        //            confidence = Math.Abs(mtse.End - mt.Start) == 1 ? Confidence.Sure : Confidence.Low,
                                        //            End = Math.Max(mt.Start, mtse.End),
                                        //            Start = Math.Min(mt.Start, mtse.End),
                                        //            EndMonoMass = mtse.EndMonoMass + Math.Round(sequencelengthformodifications(modifiedsequence), 1),
                                        //            StartMonoMass = mtse.EndMonoMass,
                                        //            Index = indexofmatchpair,
                                        //            MainSequence = ions.directionformatchstartends,
                                        //            ReverseSequence = !ions.directionformatchstartends,
                                        //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)),
                                        //            RawSequenceTag = modifiedsequence,
                                        //            MonoMasses = ions.currentions, /// FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                        //        });
                                        //        break;
                                        //    }
                                        //}
                                        //else if (Math.Round(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)), 1) <= Math.Round(LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1) + Math.Round(PPM.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1))
                                        //{
                                        //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)), Math.Round(PPM.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1));
                                        //    if (modifiedsequence != "")
                                        //    {
                                        //        indexofmatchpair++;
                                        //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                        //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                        //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                        //        newmatchstartends.Add(new MatchStartEnds
                                        //        {
                                        //            confidence = Math.Abs(mtse.End - mt.Start) == 1 ? Confidence.Sure : Confidence.Low,
                                        //            End = Math.Max(mt.Start, mtse.End),
                                        //            Start = Math.Min(mt.Start, mtse.End),
                                        //            EndMonoMass = mtse.EndMonoMass + Math.Round(sequencelengthformodifications(modifiedsequence), 1),
                                        //            StartMonoMass = mtse.EndMonoMass,
                                        //            Index = indexofmatchpair,
                                        //            MainSequence = ions.directionformatchstartends,
                                        //            ReverseSequence = !ions.directionformatchstartends,
                                        //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)),
                                        //            RawSequenceTag = modifiedsequence,
                                        //            MonoMasses = ions.currentions, /// FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                        //        });
                                        //        break;
                                        //    }
                                        //}

                                        //else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1) >= Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)) //(reverseorforw && 
                                        //else if (AlltheModifications.Any(a => a.Value == Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 4)), 4)))
                                        //{
                                        //    indexofmatchpair++;
                                        //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                        //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                        //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //    ions = FindCrntIonsintheRange((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                        //    double startrange = Math.Min(mtse.EndMonoMass, (prntmass - mt.EndMonoMass));
                                        //    double endrange = Math.Max(mtse.EndMonoMass, (prntmass - mt.EndMonoMass));


                                        //    //List<MatchStartEnds> mtses = FindMatchingAminoAcids(singleaminoacids.Where(a => a.Start >= startrange && a.End <= endrange).ToList(),
                                        //    //                                                    singleaminoacids.Where(a => a.Start >= (ParentMass - endrange) && a.End <= (ParentMass - startrange)).ToList(),
                                        //    //                                                    (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))),
                                        //    //                                                    startrange, endrange, ParentMass, Math.Min(mt.Start, mtse.End), Math.Max(mtse.End, mt.Start),
                                        //    //                                                    ref indexofmatchpair, Dontshowall, true);
                                        //    //if (mtses.Count > 0)
                                        //    //{
                                        //    //    newmatchstartends.AddRange(mtses);
                                        //    //}
                                        //    //else
                                        //    //{
                                        //    newmatchstartends.Add(new MatchStartEnds
                                        //    {
                                        //        confidence = Confidence.Low,
                                        //        End = Math.Max(mt.Start, mtse.End),
                                        //        Start = Math.Min(mt.Start, mtse.End),
                                        //        EndMonoMass = ParentMass - mt.EndMonoMass,
                                        //        StartMonoMass = mtse.EndMonoMass,
                                        //        Index = indexofmatchpair,
                                        //        MainSequence = ions.directionformatchstartends,
                                        //        ReverseSequence = !ions.directionformatchstartends,
                                        //        SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))) + (AlltheModifications.Where(a => a.Value == Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)).First().Key),
                                        //        MonoMasses = ions.currentions, /// FindCrntIonsintheRange((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                        //        //Gap = Math.Round((Math.Abs(mtse.EndMonoMass - Math.Abs(ParentMass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 4),
                                        //        DontShowall = Dontshowall
                                        //    });
                                        //    //}
                                        //    break;
                                        //}
                                        else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1) >= Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)) //(reverseorforw && 
                                        {
                                            indexofmatchpair++;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            ions = FindCrntIonsintheRange((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                            double startrange = Math.Min(mtse.EndMonoMass, (prntmass - mt.EndMonoMass));
                                            double endrange = Math.Max(mtse.EndMonoMass, (prntmass - mt.EndMonoMass));


                                            //List<MatchStartEnds> mtses = FindMatchingAminoAcids(singleaminoacids.Where(a => a.Start >= startrange && a.End <= endrange).ToList(),
                                            //                                                    singleaminoacids.Where(a => a.Start >= (ParentMass - endrange) && a.End <= (ParentMass - startrange)).ToList(),
                                            //                                                    (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))),
                                            //                                                    startrange, endrange, ParentMass, Math.Min(mt.Start, mtse.End), Math.Max(mtse.End, mt.Start),
                                            //                                                    ref indexofmatchpair, Dontshowall, true);
                                            //if (mtses.Count > 0)
                                            //{
                                            //    newmatchstartends.AddRange(mtses);
                                            //}
                                            //else
                                            //{
                                            newmatchstartends.Add(new MatchStartEnds
                                            {
                                                confidence = AminAcidConfidence.Gap,
                                                End = Math.Max(mt.Start, mtse.End),
                                                Start = Math.Min(mt.Start, mtse.End),
                                                EndMonoMass = ParentMass - mt.EndMonoMass,
                                                StartMonoMass = mtse.EndMonoMass,
                                                Index = indexofmatchpair,
                                                MainSequence = ions.directionformatchstartends,
                                                ReverseSequence = !ions.directionformatchstartends,
                                                SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))),
                                                MonoMasses = ions.currentions, /// FindCrntIonsintheRange((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                                Gap = Math.Round((Math.Abs(mtse.EndMonoMass - Math.Abs(ParentMass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 4),
                                                DontShowall = Dontshowall
                                            });
                                            //}
                                            break;
                                        }
                                        //else if ((DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1) >= Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))///!reverseorforw &&
                                        //else if (AlltheModifications.Any(a => a.Value == Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))
                                        //{
                                        //    indexofmatchpair++;
                                        //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                        //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                        //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a <= mtse.StartMonoMass && a >= mt.EndMonoMass).Select(a => a).ToList(), ParentMass);
                                        //    newmatchstartends.Add(new MatchStartEnds
                                        //    {
                                        //        confidence = Confidence.Low,
                                        //        Start = Math.Min(mtse.End, mt.Start),
                                        //        End = Math.Max(mtse.End, mt.Start),
                                        //        StartMonoMass = ParentMass - mtse.StartMonoMass,
                                        //        EndMonoMass = mt.StartMonoMass,
                                        //        Index = indexofmatchpair,
                                        //        MainSequence = ions.directionformatchstartends,
                                        //        ReverseSequence = !ions.directionformatchstartends,
                                        //        SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))) + (AlltheModifications.Where(a => a.Value == Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)).First().Key),
                                        //        MonoMasses = ions.currentions, ///FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                        //        //Gap = (Math.Round((Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - ParentMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 4)),
                                        //        DontShowall = Dontshowall
                                        //    });
                                        //    break;
                                        //}
                                        else if ((DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1) >= Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))///!reverseorforw &&
                                        {
                                            indexofmatchpair++;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                            newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                            ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a <= mtse.StartMonoMass && a >= mt.EndMonoMass).Select(a => a).ToList(), ParentMass);
                                            newmatchstartends.Add(new MatchStartEnds
                                            {
                                                confidence = AminAcidConfidence.Gap,
                                                Start = Math.Min(mtse.End, mt.Start),
                                                End = Math.Max(mtse.End, mt.Start),
                                                StartMonoMass = ParentMass - mtse.StartMonoMass,
                                                EndMonoMass = mt.StartMonoMass,
                                                Index = indexofmatchpair,
                                                MainSequence = ions.directionformatchstartends,
                                                ReverseSequence = !ions.directionformatchstartends,
                                                SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                                MonoMasses = ions.currentions, ///FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                                Gap = (Math.Round((Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - ParentMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 4)),
                                                DontShowall = Dontshowall
                                            });
                                            break;
                                        }
                                        //else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1) + Math.Round(LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1) >= Math.Round(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)), 1))
                                        //{
                                        //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) + DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1), DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1));
                                        //    if (modifiedsequence != "")
                                        //    {
                                        //        indexofmatchpair++;
                                        //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                        //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                        //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                        //        newmatchstartends.Add(new MatchStartEnds
                                        //        {
                                        //            confidence = Confidence.Gap,
                                        //            Start = Math.Min(mtse.End, mt.Start),
                                        //            End = Math.Max(mtse.End, mt.Start),
                                        //            StartMonoMass = ParentMass - mtse.StartMonoMass,
                                        //            EndMonoMass = mt.StartMonoMass,
                                        //            Index = indexofmatchpair,
                                        //            MainSequence = ions.directionformatchstartends,
                                        //            ReverseSequence = !ions.directionformatchstartends,
                                        //            SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                        //            RawSequenceTag = modifiedsequence,
                                        //            MonoMasses = ions.currentions, ///FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                        //            Gap = (Math.Round((Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - ParentMass)) - Math.Round(sequencelengthformodifications(modifiedsequence), 1)), 4))
                                        //        });
                                        //        break;
                                        //    }
                                        //}
                                        //else if ((DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1) + LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))) >= Math.Round(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)), 1)))
                                        //{
                                        //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) + DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1), DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1));
                                        //    if (modifiedsequence != "")
                                        //    {
                                        //        indexofmatchpair++;
                                        //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                        //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                        //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                        //        newmatchstartends.Add(new MatchStartEnds
                                        //        {
                                        //            confidence = Confidence.Gap,
                                        //            Start = Math.Min(mtse.End, mt.Start),
                                        //            End = Math.Max(mtse.End, mt.Start),
                                        //            StartMonoMass = ParentMass - mtse.StartMonoMass,
                                        //            EndMonoMass = mt.StartMonoMass,
                                        //            Index = indexofmatchpair,
                                        //            MainSequence = ions.directionformatchstartends,
                                        //            ReverseSequence = !ions.directionformatchstartends,
                                        //            SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                        //            RawSequenceTag = modifiedsequence,
                                        //            MonoMasses = ions.currentions, ///FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                        //            Gap = (Math.Round((Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - ParentMass)) - Math.Round(sequencelengthusingmodifiedaminoacids(modifiedsequence), 1)), 4))
                                        //        });
                                        //        break;
                                        //    }
                                        //}
                                        else
                                        {
                                            if (ModifiedSequenceForL.Length < mt.End)
                                            {
                                                if (prntmass <= Math.Abs(Math.Round(Math.Abs(mt.EndMonoMass - mtse.EndMonoMass), 1) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mt.End, Math.Abs(mtse.End - mt.End))), 1)))
                                                {
                                                    if (newmatchstartends.Where(a => a.Index == mt.Index).First().confidence != AminAcidConfidence.Sure)
                                                    {
                                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Possible;
                                                        break;
                                                    }
                                                    if (newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence != AminAcidConfidence.Sure)
                                                    {
                                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Possible;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                            else if (Enumerable.Reverse(mt.SequenceTag).SequenceEqual(mt.SequenceTag) || Enumerable.Reverse(mtse.SequenceTag).SequenceEqual(mtse.SequenceTag))
                            {
                                if (Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mtse.EndMonoMass, mt.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM))
                                {
                                    indexofmatchpair++;
                                    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                    //if (ions.currentions.Count > 0)
                                    //{
                                    //    Findmatchesinrange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, Math.Min(mt.Start, mtse.End), Dontshowall, ions.currentions, newmatchstartends.Count, ParentMass);
                                    //}

                                    newmatchstartends.Add(new MatchStartEnds
                                    {
                                        confidence = Math.Abs(mt.Start - mtse.End) == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                        End = Math.Max(mt.Start, mtse.End),
                                        Start = Math.Min(mt.Start, mtse.End),
                                        EndMonoMass = mt.StartMonoMass,
                                        StartMonoMass = mtse.EndMonoMass,
                                        Index = indexofmatchpair,
                                        MainSequence = ions.directionformatchstartends,
                                        ReverseSequence = !ions.directionformatchstartends,
                                        SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                        MonoMasses = ions.currentions,
                                        DontShowall = Dontshowall
                                    });
                                    break;
                                }
                                else if (Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM))
                                {
                                    indexofmatchpair++;
                                    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                    //if (ions.currentions.Count > 0)
                                    //{
                                    //    Findmatchesinrange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, Math.Min(mt.Start, mtse.End), Dontshowall, ions.currentions, newmatchstartends.Count, ParentMass);
                                    //}

                                    newmatchstartends.Add(new MatchStartEnds
                                    {
                                        confidence = AminAcidConfidence.Low,
                                        End = Math.Max(mt.Start, mtse.End),
                                        Start = Math.Min(mt.Start, mtse.End),
                                        EndMonoMass = mt.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                        StartMonoMass = mt.EndMonoMass,
                                        Index = indexofmatchpair,
                                        MainSequence = ions.directionformatchstartends,
                                        ReverseSequence = !ions.directionformatchstartends,
                                        MonoMasses = ions.currentions, /// FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass), ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                        SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)),
                                        DontShowall = Dontshowall
                                    });
                                    break;
                                }
                                //else if ((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass)) <= LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))) + PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mtse.EndMonoMass, mt.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM))
                                //{
                                //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), (Math.Abs(mtse.EndMonoMass - mt.StartMonoMass)), PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mtse.EndMonoMass, mt.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM));
                                //    if (modifiedsequence != "")
                                //    {
                                //        indexofmatchpair++;
                                //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                //        newmatchstartends.Add(new MatchStartEnds
                                //        {
                                //            confidence = Math.Abs(mt.Start - mtse.End) == 1 ? Confidence.Sure : Confidence.Low,
                                //            End = Math.Max(mt.Start, mtse.End),
                                //            Start = Math.Min(mt.Start, mtse.End),
                                //            EndMonoMass = mt.StartMonoMass,
                                //            StartMonoMass = mtse.EndMonoMass,
                                //            Index = indexofmatchpair,
                                //            MainSequence = ions.directionformatchstartends,
                                //            ReverseSequence = !ions.directionformatchstartends,
                                //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                //            RawSequenceTag = modifiedsequence,
                                //            MonoMasses = ions.currentions, /// FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass)
                                //        });
                                //        break;
                                //    }
                                //}
                                //else if ((Math.Abs(mt.EndMonoMass - mtse.StartMonoMass)) <= LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))) + PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM))
                                //{
                                //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), (Math.Abs(mt.EndMonoMass - mtse.StartMonoMass)), PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.StartMonoMass), Properties.Settings.Default.MatchTolerancePPM));
                                //    if (modifiedsequence != "")
                                //    {
                                //        indexofmatchpair++;
                                //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                //        newmatchstartends.Add(new MatchStartEnds
                                //        {
                                //            confidence = Confidence.Low,
                                //            End = Math.Max(mt.Start, mtse.End),
                                //            Start = Math.Min(mt.Start, mtse.End),
                                //            EndMonoMass = mt.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                //            StartMonoMass = mt.EndMonoMass,
                                //            Index = indexofmatchpair,
                                //            MainSequence = ions.directionformatchstartends,
                                //            ReverseSequence = !ions.directionformatchstartends,
                                //            MonoMasses = ions.currentions, /// FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass), ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)) //// + Convert.ToString(Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mt.End, Math.Abs(mtse.Start - mt.End)))))
                                //        });
                                //        break;
                                //    }
                                //}
                                //try
                                {
                                    double prntmass = Math.Round(ParentMass, 1);

                                    if (Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(mt.EndMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1) <= Math.Round(PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1))
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                        //if (ions.currentions.Count > 0)
                                        //{
                                        //    Findmatchesinrange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, Math.Min(mt.Start, mtse.End), Dontshowall, ions.currentions, newmatchstartends.Count, ParentMass);
                                        //}

                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = Math.Abs(mtse.End - mt.Start) == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                            End = Math.Max(mt.Start, mtse.End),
                                            Start = Math.Min(mt.Start, mtse.End),
                                            EndMonoMass = mtse.EndMonoMass + Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1),
                                            StartMonoMass = mtse.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)),
                                            MonoMasses = ions.currentions,
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    else if (Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1) <= Math.Round(PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1))
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);

                                        //if (ions.currentions.Count > 0)
                                        //{
                                        //    Findmatchesinrange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), ModifiedSequenceForL, Math.Min(mt.Start, mtse.End), Dontshowall, ions.currentions, newmatchstartends.Count, ParentMass);
                                        //}

                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = Math.Abs(mtse.End - mt.Start) == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                            End = Math.Max(mt.Start, mtse.End),
                                            Start = Math.Min(mt.Start, mtse.End),
                                            EndMonoMass = mtse.EndMonoMass + Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1),
                                            StartMonoMass = mtse.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)),
                                            MonoMasses = ions.currentions,
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    //else if (Math.Round((Math.Abs(mtse.EndMonoMass - Math.Abs(mt.EndMonoMass - prntmass))), 1) <= Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1) + Math.Round(PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1))
                                    //{
                                    //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), Math.Abs(mtse.EndMonoMass - Math.Abs(mt.EndMonoMass - prntmass)), Math.Round(PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1));
                                    //    if (modifiedsequence != "")
                                    //    {
                                    //        indexofmatchpair++;
                                    //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);

                                    //        newmatchstartends.Add(new MatchStartEnds
                                    //        {
                                    //            confidence = Math.Abs(mtse.End - mt.Start) == 1 ? Confidence.Sure : Confidence.Low,
                                    //            End = Math.Max(mt.Start, mtse.End),
                                    //            Start = Math.Min(mt.Start, mtse.End),
                                    //            EndMonoMass = mtse.EndMonoMass + Math.Round(sequencelengthformodifications(modifiedsequence), 1),
                                    //            StartMonoMass = mtse.EndMonoMass,
                                    //            Index = indexofmatchpair,
                                    //            MainSequence = ions.directionformatchstartends,
                                    //            ReverseSequence = !ions.directionformatchstartends,
                                    //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)),
                                    //            MonoMasses = ions.currentions,
                                    //            RawSequenceTag = modifiedsequence
                                    //        });
                                    //        break;
                                    //    }
                                    //}
                                    //else if (Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1) <= Math.Round(PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1))
                                    //{
                                    //    string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)), Math.Round(PPMCalc.CurrentPPMbasedonMatchList(Math.Max(mt.EndMonoMass, mtse.EndMonoMass), Properties.Settings.Default.MatchTolerancePPM), 1));
                                    //    if (modifiedsequence != "")
                                    //    {
                                    //        indexofmatchpair++;
                                    //        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass);
                                    //        newmatchstartends.Add(new MatchStartEnds
                                    //        {
                                    //            confidence = Math.Abs(mtse.End - mt.Start) == 1 ? Confidence.Sure : Confidence.Low,
                                    //            End = Math.Max(mt.Start, mtse.End),
                                    //            Start = Math.Min(mt.Start, mtse.End),
                                    //            EndMonoMass = mtse.EndMonoMass + Math.Round(sequencelengthformodifications(modifiedsequence), 1),
                                    //            StartMonoMass = mtse.EndMonoMass,
                                    //            Index = indexofmatchpair,
                                    //            MainSequence = ions.directionformatchstartends,
                                    //            ReverseSequence = !ions.directionformatchstartends,
                                    //            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)),
                                    //            RawSequenceTag = modifiedsequence,
                                    //            MonoMasses = ions.currentions, /// FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions,
                                    //        });
                                    //        break;
                                    //    }
                                    //}
                                    //else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End)) + LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))) <= Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass)))
                                    //else if (AlltheModifications.Any(a => a.Value == Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))))
                                    //{
                                    //    //string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)) , 
                                    //    indexofmatchpair++;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                    //    newmatchstartends.Add(new MatchStartEnds
                                    //    {
                                    //        confidence = Confidence.Low,
                                    //        End = Math.Max(mt.Start, mtse.End),
                                    //        Start = Math.Min(mt.Start, mtse.End),
                                    //        EndMonoMass = mtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                    //        StartMonoMass = mtse.EndMonoMass,
                                    //        Index = indexofmatchpair,
                                    //        MainSequence = ions.directionformatchstartends,
                                    //        ReverseSequence = !ions.directionformatchstartends,
                                    //        MonoMasses = ions.currentions, // FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions, ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                    //        SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)) + (AlltheModifications.Where(a => a.Value == Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))).First().Key),
                                    //        //Gap = Math.Round(((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))), 4),
                                    //        DontShowall = Dontshowall
                                    //    });
                                    //    break;
                                    //}
                                    else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End)) >= Math.Abs(Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass)) - LengthofSequencewithMaxModification(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))))
                                    {
                                        //string modifiedsequence = CombinationofallModifications(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)) , 
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.Gap,
                                            End = Math.Max(mt.Start, mtse.End),
                                            Start = Math.Min(mt.Start, mtse.End),
                                            EndMonoMass = mtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            StartMonoMass = mtse.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            MonoMasses = ions.currentions, // FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions, ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                            Gap = Math.Round(((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))), 4),
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    //else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.Start - mt.End)) >= Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))))
                                    //else if (AlltheModifications.Any(a => a.Value == Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))))
                                    //{
                                    //    indexofmatchpair++;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                    //    newmatchstartends.Add(new MatchStartEnds
                                    //    {
                                    //        confidence = Confidence.Low,
                                    //        End = Math.Max(mt.Start, mtse.End),
                                    //        Start = Math.Min(mt.Start, mtse.End),
                                    //        EndMonoMass = mt.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                    //        StartMonoMass = mt.EndMonoMass,
                                    //        Index = indexofmatchpair,
                                    //        MainSequence = ions.directionformatchstartends, // directionformatchstartends,
                                    //        ReverseSequence = !ions.directionformatchstartends,
                                    //        MonoMasses = ions.currentions, // FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions, ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                    //        SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)) + (AlltheModifications.Where(a => a.Value == Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))).First().Key),
                                    //        Gap = Math.Round(((Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))), 4),
                                    //        DontShowall = Dontshowall
                                    //    });
                                    //    break;
                                    //}
                                    else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.Start - mt.End)) >= Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))))
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.Gap,
                                            End = Math.Max(mt.Start, mtse.End),
                                            Start = Math.Min(mt.Start, mtse.End),
                                            EndMonoMass = mt.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            StartMonoMass = mt.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends, // directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            MonoMasses = ions.currentions, // FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions, ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)),
                                            Gap = Math.Round(((Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))), 4),
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    //else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1) >= Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)) //(reverseorforw && 
                                    //else if (AlltheModifications.Any(a => a.Value == Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)))
                                    //{
                                    //    indexofmatchpair++;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    ions = FindCrntIonsintheRange((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                    //    newmatchstartends.Add(new MatchStartEnds
                                    //    {
                                    //        confidence = Confidence.Low,
                                    //        End = Math.Max(mt.Start, mtse.End),
                                    //        Start = Math.Min(mt.Start, mtse.End),
                                    //        EndMonoMass = ParentMass - mt.EndMonoMass,
                                    //        StartMonoMass = mtse.EndMonoMass,
                                    //        Index = indexofmatchpair,
                                    //        MainSequence = ions.directionformatchstartends,
                                    //        ReverseSequence = !ions.directionformatchstartends,
                                    //        SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))) + (AlltheModifications.Where(a => a.Value == Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)).First().Key),
                                    //        MonoMasses = ions.currentions,
                                    //        //Gap = Math.Round((Math.Abs(mtse.EndMonoMass - Math.Abs(ParentMass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 4),
                                    //        DontShowall = Dontshowall
                                    //    });
                                    //    break;
                                    //}
                                    else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1) >= Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)) //(reverseorforw && 
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        ions = FindCrntIonsintheRange((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.Gap,
                                            End = Math.Max(mt.Start, mtse.End),
                                            Start = Math.Min(mt.Start, mtse.End),
                                            EndMonoMass = ParentMass - mt.EndMonoMass,
                                            StartMonoMass = mtse.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))),
                                            MonoMasses = ions.currentions,
                                            Gap = Math.Round((Math.Abs(mtse.EndMonoMass - Math.Abs(ParentMass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 4),
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    //else if  ((DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1) >= Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))///!reverseorforw &&
                                    //else if (AlltheModifications.Any(a => a.Value == Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))
                                    //{
                                    //    indexofmatchpair++;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a <= mtse.StartMonoMass && a >= mt.EndMonoMass).Select(a => a).ToList(), ParentMass);
                                    //    newmatchstartends.Add(new MatchStartEnds
                                    //    {
                                    //        confidence = Confidence.Low,
                                    //        Start = Math.Min(mtse.End, mt.Start),
                                    //        End = Math.Max(mtse.End, mt.Start),
                                    //        StartMonoMass = ParentMass - mtse.StartMonoMass,
                                    //        EndMonoMass = mt.StartMonoMass,
                                    //        Index = indexofmatchpair,
                                    //        MainSequence = ions.directionformatchstartends,
                                    //        ReverseSequence = !ions.directionformatchstartends,
                                    //        SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))) + (AlltheModifications.Where(a => a.Value == Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)).First().Key),
                                    //        MonoMasses = ions.currentions, ///FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                    //        //Gap = (Math.Round((Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - ParentMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 4)),
                                    //        DontShowall = Dontshowall
                                    //    });
                                    //    break;
                                    //}
                                    else if ((DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1) >= Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))///!reverseorforw &&
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a <= mtse.StartMonoMass && a >= mt.EndMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.Gap,
                                            Start = Math.Min(mtse.End, mt.Start),
                                            End = Math.Max(mtse.End, mt.Start),
                                            StartMonoMass = ParentMass - mtse.StartMonoMass,
                                            EndMonoMass = mt.StartMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            MonoMasses = ions.currentions, ///FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                            Gap = (Math.Round((Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - ParentMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 4)),
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    //else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End)) >= Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))))
                                    //else if (AlltheModifications.Any(a => a.Value == Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))))
                                    //{
                                    //    indexofmatchpair++;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                    //    newmatchstartends.Add(new MatchStartEnds
                                    //    {
                                    //        confidence = Confidence.Low,
                                    //        End = Math.Max(mt.Start, mtse.End),
                                    //        Start = Math.Min(mt.Start, mtse.End),
                                    //        EndMonoMass = mtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                    //        StartMonoMass = mtse.EndMonoMass,
                                    //        Index = indexofmatchpair,
                                    //        MainSequence = ions.directionformatchstartends,
                                    //        ReverseSequence = !ions.directionformatchstartends,
                                    //        MonoMasses = ions.currentions, // FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions, ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                    //        SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)) + (AlltheModifications.Where(a => a.Value == Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))).First().Key),
                                    //        //Gap = Math.Round(((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))), 4),
                                    //        DontShowall = Dontshowall
                                    //    });
                                    //    break;
                                    //}
                                    else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End)) >= Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))))
                                    {

                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.Gap,
                                            End = Math.Max(mt.Start, mtse.End),
                                            Start = Math.Min(mt.Start, mtse.End),
                                            EndMonoMass = mtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            StartMonoMass = mtse.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            MonoMasses = ions.currentions, // FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions, ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                            Gap = Math.Round(((Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))))), 4),
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    //else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.Start - mt.End)) >= Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))))
                                    //else if (AlltheModifications.Any(a => a.Value == Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))))
                                    //{
                                    //    indexofmatchpair++;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                    //    newmatchstartends.Add(new MatchStartEnds
                                    //    {
                                    //        confidence = Confidence.Low,
                                    //        End = Math.Max(mt.Start, mtse.End),
                                    //        Start = Math.Min(mt.Start, mtse.End),
                                    //        EndMonoMass = mt.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                    //        StartMonoMass = mt.EndMonoMass,
                                    //        Index = indexofmatchpair,
                                    //        MainSequence = ions.directionformatchstartends, // directionformatchstartends,
                                    //        ReverseSequence = !ions.directionformatchstartends,
                                    //        MonoMasses = ions.currentions, // FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions, ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                    //        SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)) + (AlltheModifications.Where(a => a.Value == Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))).First().Key),
                                    //        //Gap = Math.Round(((Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))), 4),
                                    //        DontShowall = Dontshowall
                                    //    });
                                    //    break;
                                    //}
                                    else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.Start - mt.End)) >= Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))))
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.Gap,
                                            End = Math.Max(mt.Start, mtse.End),
                                            Start = Math.Min(mt.Start, mtse.End),
                                            EndMonoMass = mt.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            StartMonoMass = mt.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends, // directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            MonoMasses = ions.currentions, // FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a.MonoMass >= mt.EndMonoMass && a.MonoMass <= mtse.StartMonoMass).Select(a => a.MonoMass).ToList(), ParentMass).currentions, ///CrntIons.Where(a => a.MonoMass >= mtse.EndMonoMass && a.MonoMass <= mt.StartMonoMass).Select(a => a.MonoMass).ToList(),
                                            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)),
                                            Gap = Math.Round(((Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start))))), 4),
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    //else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1) >= Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)) //(reverseorforw && 
                                    //else if (AlltheModifications.Any(a => a.Value == Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)))
                                    //{
                                    //    indexofmatchpair++;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    ions = FindCrntIonsintheRange((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                    //    newmatchstartends.Add(new MatchStartEnds
                                    //    {
                                    //        confidence = Confidence.Low,
                                    //        End = Math.Max(mt.Start, mtse.End),
                                    //        Start = Math.Min(mt.Start, mtse.End),
                                    //        EndMonoMass = ParentMass - mt.EndMonoMass,
                                    //        StartMonoMass = mtse.EndMonoMass,
                                    //        Index = indexofmatchpair,
                                    //        MainSequence = ions.directionformatchstartends,
                                    //        ReverseSequence = !ions.directionformatchstartends,
                                    //        SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))) + (AlltheModifications.Where(a => a.Value == Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)).First().Key),
                                    //        MonoMasses = ions.currentions,
                                    //        //Gap = Math.Round((Math.Abs(mtse.EndMonoMass - Math.Abs(ParentMass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 4),
                                    //        DontShowall = Dontshowall
                                    //    });
                                    //    break;
                                    //}
                                    else if (DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mt.Start - mtse.End) + 1) >= Math.Round(Math.Abs(Math.Abs(mtse.EndMonoMass - Math.Abs(prntmass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 1)) //(reverseorforw && 
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        ions = FindCrntIonsintheRange((ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.Gap,
                                            End = Math.Max(mt.Start, mtse.End),
                                            Start = Math.Min(mt.Start, mtse.End),
                                            EndMonoMass = ParentMass - mt.EndMonoMass,
                                            StartMonoMass = mtse.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))),
                                            MonoMasses = ions.currentions,
                                            Gap = Math.Round((Math.Abs(mtse.EndMonoMass - Math.Abs(ParentMass - mt.EndMonoMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End))), 1)), 4),
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    //else if ((DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1) >= Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))///!reverseorforw &&
                                    //else if (AlltheModifications.Any(a => a.Value == Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))
                                    //{
                                    //    indexofmatchpair++;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = Confidence.Sure;
                                    //    newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                    //    ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a <= mtse.StartMonoMass && a >= mt.EndMonoMass).Select(a => a).ToList(), ParentMass);
                                    //    newmatchstartends.Add(new MatchStartEnds
                                    //    {
                                    //        confidence = Confidence.Low,
                                    //        Start = Math.Min(mtse.End, mt.Start),
                                    //        End = Math.Max(mtse.End, mt.Start),
                                    //        StartMonoMass = ParentMass - mtse.StartMonoMass,
                                    //        EndMonoMass = mt.StartMonoMass,
                                    //        Index = indexofmatchpair,
                                    //        MainSequence = ions.directionformatchstartends,
                                    //        ReverseSequence = !ions.directionformatchstartends,
                                    //        SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))) + (AlltheModifications.Where(a => a.Value == Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)).First().Key),
                                    //        MonoMasses = ions.currentions, ///FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                    //        //Gap = (Math.Round((Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - ParentMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 4)),
                                    //        DontShowall = Dontshowall
                                    //    });
                                    //    break;
                                    //}
                                    else if ((DeltaMassforSequenceTagMatching.DeltaMass(Math.Abs(mtse.End - mt.Start) + 1) >= Math.Round(Math.Abs(Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - prntmass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 1)))///!reverseorforw &&
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Sure;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a <= mtse.StartMonoMass && a >= mt.EndMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.Gap,
                                            Start = Math.Min(mtse.End, mt.Start),
                                            End = Math.Max(mtse.End, mt.Start),
                                            StartMonoMass = ParentMass - mtse.StartMonoMass,
                                            EndMonoMass = mt.StartMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            SequenceTag = (ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))),
                                            MonoMasses = ions.currentions, ///FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a.MonoMass <= mtse.StartMonoMass && a.MonoMass >= mt.EndMonoMass).Select(a => a.MonoMass).ToList(), ParentMass),
                                            Gap = (Math.Round((Math.Abs(mt.StartMonoMass - Math.Abs(mtse.StartMonoMass - ParentMass)) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(Math.Abs(mt.Start) - mtse.End))), 1)), 4)),
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    else if (ParentMass <= Math.Abs(Math.Abs(mt.EndMonoMass - mtse.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)))))
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mtse.Start - mt.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mtse.Start - mt.End);
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)), CrntIons.Where(a => a >= mt.EndMonoMass && a <= mtse.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.NotPossible,
                                            End = mt.Start,
                                            Start = mtse.End,
                                            EndMonoMass = mt.StartMonoMass,
                                            StartMonoMass = mtse.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mtse.End - mt.Start)),
                                            MonoMasses = ions.currentions,
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                        //// asdkh kjasdhkj
                                        ///jdaflkajdklaj; dlaksjl kasd; aslkd;
                                    }
                                    else if (ParentMass <= Math.Abs(Math.Abs(mtse.EndMonoMass - mt.StartMonoMass) - sequencelength(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)))))
                                    {
                                        indexofmatchpair++;
                                        newmatchstartends.Where(a => a.Index == mtse.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        newmatchstartends.Where(a => a.Index == mt.Index).First().Numberofextensions = Math.Abs(mt.Start - mtse.End);
                                        ions = FindCrntIonsintheRange(ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)), CrntIons.Where(a => a >= mtse.EndMonoMass && a <= mt.StartMonoMass).Select(a => a).ToList(), ParentMass);
                                        newmatchstartends.Add(new MatchStartEnds
                                        {
                                            confidence = AminAcidConfidence.NotPossible,
                                            End = mt.Start,
                                            Start = mtse.End,
                                            EndMonoMass = mt.StartMonoMass,
                                            StartMonoMass = mtse.EndMonoMass,
                                            Index = indexofmatchpair,
                                            MainSequence = ions.directionformatchstartends,
                                            ReverseSequence = !ions.directionformatchstartends,
                                            SequenceTag = ModifiedSequenceForL.Substring(mtse.End, Math.Abs(mt.Start - mtse.End)),
                                            MonoMasses = ions.currentions,
                                            DontShowall = Dontshowall
                                        });
                                        break;
                                    }
                                    else
                                    {
                                        if (ModifiedSequenceForL.Length < mt.End)
                                        {
                                            if (prntmass <= Math.Abs(Math.Round(Math.Abs(mt.EndMonoMass - mtse.EndMonoMass), 1) - Math.Round(sequencelength(ModifiedSequenceForL.Substring(mt.End, Math.Abs(mtse.End - mt.End))), 1)))
                                            {
                                                if (newmatchstartends.Where(a => a.Index == mt.Index).First().confidence != AminAcidConfidence.Sure)
                                                {
                                                    newmatchstartends.Where(a => a.Index == mt.Index).First().confidence = AminAcidConfidence.Possible;
                                                    break;
                                                }
                                                if (newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence != AminAcidConfidence.Sure)
                                                {
                                                    newmatchstartends.Where(a => a.Index == mtse.Index).First().confidence = AminAcidConfidence.Possible;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        imatchstartends++;
                    }
                }
                else if (matchstartends.Count == 1)
                {
                    matchstartends.First().confidence = AminAcidConfidence.Sure;
                }

                if (newmatchstartends.Count == matchstartends.Count && matchstartends.Count > 0)
                {
                    MatchStartEnds tempmatch = new MatchStartEnds();
                    tempmatch = matchstartends.OrderByDescending(a => a.Length).First();
                    tempmatch.confidence = AminAcidConfidence.Sure;
                    matchstartends.Clear();
                    matchstartends.Add(tempmatch);
                }
                else
                {
                    var matchstartendscountusingst = newmatchstartends.Where(a => a.confidence == AminAcidConfidence.Sure || a.confidence == AminAcidConfidence.Low || a.confidence == AminAcidConfidence.Gap).OrderBy(a => a.Start).ToList().Count;
                    var matchstartendscountusingstend = newmatchstartends.Where(a => a.confidence == AminAcidConfidence.Sure || a.confidence == AminAcidConfidence.Low || a.confidence == AminAcidConfidence.Gap && !(a.StartMonoMass >= ParentMass || a.StartMonoMass <= 0) && !(a.EndMonoMass >= ParentMass || a.EndMonoMass <= 0)).OrderBy(a => a.Start).ToList().Count;
                    if (matchstartendscountusingst == matchstartendscountusingstend)
                    {
                        matchstartends = newmatchstartends.Where(a => a.confidence == AminAcidConfidence.Sure || a.confidence == AminAcidConfidence.Low || a.confidence == AminAcidConfidence.Gap && !(a.StartMonoMass >= ParentMass || a.StartMonoMass <= 0) && !(a.EndMonoMass >= ParentMass || a.EndMonoMass <= 0)).OrderBy(a => a.Start).ToList();
                    }
                    else
                    {
                        matchstartends.Clear();
                        matchstartends.Add(newmatchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).OrderByDescending(a => a.SequenceTag.Length).First());
                    }
                }

                List<double> allthemonomasses = new List<double>();

                foreach (double dt in CrntIons.OrderBy(a => a).Select(a => a).ToList())
                {
                    allthemonomasses.Add(dt);
                }

                foreach (double dt in ReverseCrntIons.OrderByDescending(a => a).ToList())
                {
                    allthemonomasses.Add(dt);
                }
                allthemonomasses.Add(0);
                allthemonomasses.Add(ParentMass);
                allthemonomasses.Add(Molecules.Water);
                bool breakornot = false;
                ///If a gap is found making sure the alignment is perfect and there are monomasses where they are should be
                #endregion

                if (matchstartends.Count == 0)
                {
                    string localpairmatch = PairMatch[0];

                    if (newmatchstartends.Where(a => a.SequenceTag == localpairmatch).Any())
                    {
                        newmatchstartends.Where(a => a.SequenceTag == localpairmatch).First().confidence = AminAcidConfidence.Sure;
                    }
                    else if (newmatchstartends.Where(a => a.SequenceTag == ReverseString.Reverse(localpairmatch)).Any())
                    {
                        newmatchstartends.Where(a => a.SequenceTag == ReverseString.Reverse(localpairmatch)).First().confidence = AminAcidConfidence.Sure;
                    }

                    matchstartends = newmatchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).ToList();
                }


                int boundtextlength = BoundText.Length;

                if (matchstartends.Count > 0)
                {
                    #region Extensions

                    List<MatchStartEnds> removedmtse = new List<MatchStartEnds>();
                    List<MatchStartEnds> newmtse = matchstartends;
                    matchstartends = matchstartends.OrderBy(a => a.Start).ToList();
                    imatchstartends = 1;
                    foreach (MatchStartEnds mtse in matchstartends)
                    {
                        if (removedmtse.Contains(mtse))
                        {
                            imatchstartends++;
                            continue;
                        }
                        foreach (MatchStartEnds mt in matchstartends.GetRange(imatchstartends, matchstartends.Count - imatchstartends))
                        {
                            if (mtse.confidence == AminAcidConfidence.Gap || mt.confidence == AminAcidConfidence.Gap) continue;
                            if (mtse.Start <= mt.Start && mtse.End >= mt.End && mtse.confidence > mt.confidence)
                            {
                                newmtse.Remove(matchstartends.Where(a => a.Index == mt.Index).First());
                                removedmtse.Add(matchstartends.Where(a => a.Index == mt.Index).First());
                            }
                        }
                        imatchstartends++;
                    }

                    matchstartends = newmtse.OrderBy(a => a.Start).ToList();
                    string considersequence = string.Empty;

                    List<String> Othersequencetags = Allthesequencetags.GetRange(0, Allthesequencetags.Count - 1);

                    string ReverseBoundText = string.Empty;

                    char[] reverseboundtextchar = BoundText.ToCharArray();
                    Array.Reverse(reverseboundtextchar);

                    ReverseBoundText = new string(reverseboundtextchar);

                    Cluster startIon = new Cluster();

                    if (startIon == null)
                    {
                        Debug.WriteLine("Failed to find the start.");
                        return new List<MatchStartEnds>();
                    }
                    double Startvalueforions = 0.00;
                    int indexforStartvalueforions = 0;

                    Cluster endIon = new Cluster();
                    if (endIon == null)
                    {
                        Debug.WriteLine("Failed to find the end.");
                        return new List<MatchStartEnds>();
                    }
                    double Endvalueforions = 0.00; // endIon.MonoMass;
                    int indexforEndvalueforions = 0; // CrntIons.IndexOf(endIon);

                    int startindexforboundtext = 0;
                    int endindexforboundtext = 0;
                    string startsequenceforboundtext = string.Empty;
                    string endsequenceforboundtext = string.Empty;

                    matchstartends = matchstartends.GroupBy(a => a.forsort).Select(b => b.First()).ToList();

                    int indexoffirstelement = matchstartends.First().Start;

                    newmatchstartends.Clear();

                    ///Finding sequence matches found which are beyond the ParentMass
                    foreach (MatchStartEnds mse in matchstartends.Skip(1))
                    {
                        if (sequencelength(ModifiedSequenceForL.Substring(indexoffirstelement, mse.Start - indexoffirstelement)) > ParentMass)
                        {
                            newmatchstartends.Add(mse);
                        }
                        else if (sequencelength(ModifiedSequenceForL.Substring(indexoffirstelement, mse.End - indexoffirstelement)) > ParentMass)
                        {
                            newmatchstartends.Add(mse);
                        }
                    }

                    ///Removing any of the matches which are beyond the ParentMass
                    foreach (MatchStartEnds mse in newmatchstartends)
                    {
                        matchstartends.Remove(matchstartends.Where(a => a.Index == mse.Index).First());
                    }

                    imatchstartends = 1;
                    newmatchstartends.Clear();

                    ///Finding any other false positives by removing tags which are same and occur at different places with the same monomass
                    foreach (MatchStartEnds mse in matchstartends)
                    {
                        foreach (MatchStartEnds mt in matchstartends.GetRange(imatchstartends, matchstartends.Count - imatchstartends))
                        {
                            if (mse.SequenceTag == mt.SequenceTag && mse.StartMonoMass == mt.StartMonoMass)
                            {
                                newmatchstartends.Add(mt);
                            }
                        }
                        imatchstartends++;
                    }
                    ///Removing any other false positives by removing tags which are same and occur at different places with the same monomass
                    foreach (MatchStartEnds mse in newmatchstartends)
                    {
                        if (matchstartends.Where(a => a.Index == mse.Index).Any())
                            matchstartends.Remove(matchstartends.Where(a => a.Index == mse.Index).First());
                        else
                        {
                            matchstartends.Clear();
                            return matchstartends;
                        }
                    }

                    newmatchstartends.Clear();

                    matchstartends = matchstartends.OrderBy(a => a.Start).ToList();

                    newmatchstartends.AddRange(matchstartends);
                    //imatchstartends = 1;
                    //matchstartends = newmatchstartends;

                    MatchStartEnds newmtses = new MatchStartEnds();

                    if (newmatchstartends.Count >= 1)
                    {
                        newmtses = newmatchstartends.First();
                        bool deletenextitem = false;
                        int indexcurrentmtse = 1;
                        foreach (var mtse in matchstartends.Skip(1))
                        {
                            if (deletenextitem)
                            {
                                newmatchstartends.Remove(newmatchstartends[indexcurrentmtse]);
                                continue;
                            }
                            if (mtse.End == newmtses.Start || mtse.Start == newmtses.End)
                            {
                                newmtses = mtse;
                            }
                            else
                            {
                                newmatchstartends.Remove(newmatchstartends[indexcurrentmtse]);
                                deletenextitem = true;
                                continue;
                            }
                            indexcurrentmtse++;
                        }
                    }


                    matchstartends = newmatchstartends.ToList();

                    try
                    {
                        foreach (MatchStartEnds mtse in matchstartends)
                        {
                            mainsequencetag = mtse.SequenceTag;
                            if (!AllSqsTags.Where(a => a.Sequence == (mainsequencetag)).Any())
                                continue;

                            if (ModifiedSequenceForL.Contains(mainsequencetag))
                            {
                                List<double> mms = new List<double>();
                                foreach (var insqstags in AllSqsTags.Where(a => a.Sequence == (mainsequencetag)).ToList())
                                {
                                    foreach (var ist in insqstags.IndividualAAs.Select(a => a.Start).ToList())
                                    {
                                        mms.Add(Math.Round(ist, 1));
                                    }
                                    foreach (var ist in insqstags.IndividualAAs.Select(a => a.End).ToList())
                                    {
                                        mms.Add(Math.Round(ist, 1));
                                    }
                                }
                                Monomasses.AddRange(mms);
                            }
                            else if (reversesequence.Contains(mainsequencetag))
                            {
                                int Length = ModifiedSequenceForL.Length;
                                List<double> mms = new List<double>();
                                foreach (var insqstags in AllSqsTags.Where(a => a.Sequence == (mainsequencetag)))
                                {
                                    foreach (var ist in insqstags.IndividualAAs.Select(a => a.Start).ToList())
                                    {
                                        mms.Add(Math.Round(ist, 1));
                                    }
                                    foreach (var ist in insqstags.IndividualAAs.Select(a => a.End).ToList())
                                    {
                                        mms.Add(Math.Round(ist, 1));
                                    }
                                }
                                Monomasses.AddRange(mms);
                            }
                        }

                        forwardorreverse = true;

                        startindexforboundtext = matchstartends.First().Start;
                        endindexforboundtext = matchstartends.Last().End;
                        startsequenceforboundtext = ModifiedSequenceForL.Substring(0, startindexforboundtext);
                        endsequenceforboundtext = ModifiedSequenceForL.Substring(endindexforboundtext, ModifiedSequenceForL.Length - endindexforboundtext);

                        startvaluetocompare = Math.Round(matchstartends.First().StartMonoMass, 1);
                        endvaluetocompare = Math.Round(matchstartends.First().EndMonoMass, 1);

                        if (CrntIons.Any(a => Math.Round(a, 1) == startvaluetocompare))
                        {
                            Startvalueforions = CrntIons.First(a => Math.Round(a, 1) == startvaluetocompare);
                            indexforStartvalueforions = CrntIons.IndexOf(CrntIons.Where(a => Math.Round(a, 1) == startvaluetocompare).First());
                        }
                        else if (CrntIons.Any(a => Math.Abs(Math.Round(a, 1) - startvaluetocompare) <= 0.1))
                        {
                            Startvalueforions = CrntIons.Where(a => Math.Abs(Math.Round(a, 1) - startvaluetocompare) <= 0.1).First();
                            indexforStartvalueforions = CrntIons.IndexOf(CrntIons.Where(a => Math.Abs(Math.Round(a, 1) - startvaluetocompare) <= 0.1).First());
                        }
                        else
                        {
                            Startvalueforions = 0;
                        }
                        //Startvalueforions = CrntIons.Any(a => Math.Round(a, 1) == startvaluetocompare) ? 
                        //Startvalueforions = CrntIons.Where(a => Math.Round(a, 1) == startvaluetocompare).First();
                        //indexforStartvalueforions = CrntIons.IndexOf(CrntIons.Where(a => Math.Round(a, 1) == startvaluetocompare).First());
                        if (CrntIons.Any(a => Math.Round(a, 1) == endvaluetocompare))
                        {
                            Endvalueforions = CrntIons.Where(a => Math.Round(a, 1) == endvaluetocompare).First();
                            indexforEndvalueforions = CrntIons.IndexOf(CrntIons.Where(a => Math.Round(a, 1) == endvaluetocompare).First());
                        }
                        else if (CrntIons.Any(a => Math.Abs(Math.Round(a, 1) - endvaluetocompare) <= 0.1))
                        {
                            Endvalueforions = CrntIons.Where(a => Math.Abs(Math.Round(a, 1) - endvaluetocompare) <= 0.1).First();
                            indexforEndvalueforions = CrntIons.IndexOf(CrntIons.Where(a => Math.Abs(Math.Round(a, 1) - endvaluetocompare) <= 0.1).First());
                        }
                        else
                        {
                            Endvalueforions = 0;
                        }
                        //Endvalueforions = CrntIons.Where(a => Math.Round(a, 1) == endvaluetocompare).First();
                        //indexforEndvalueforions = CrntIons.IndexOf(CrntIons.Where(a => Math.Round(a, 1) == endvaluetocompare).First());

                        List<double> reversecurntIons = new List<double>();

                        double tempcrntions = 0.0;
                        ///Need to check if the first sequence is in the forward or the reverse direction.
                        ///Based on which the rest of the sequence is calculated.
                        if (matchstartends.First().MainSequence)
                        {
                            ///Find the place from which we need to use the reverseions
                            tempcrntions = ParentMass - Startvalueforions;
                            //reversecurntIons = ReverseCrntIons.Where(a => a >= tempcrntions).ToList();
                            //reversecurntIons.Add(0);
                            reversecurntIons = CrntIons.Where(a => a >= tempcrntions && a < ParentMass).Select(b => b).ToList();///All the ions which are greater than the currentparentmass minus the curntion
                            reversecurntIons.AddRange(ReverseCrntIons.Where(a => a >= tempcrntions).ToList()); ///Also add all the CrntIons which are greater than the tempcrntions
                            //reversecurntIons.Add(ParentMass);
                            reversecurntIons = reversecurntIons.OrderBy(a => a).ToList();  ///Order all the ions so we can use them later for finding ions
                            startvaluetocompare = 0;
                            ///Add all the matches which are found from the method
                            List<double> tempfcrntions = new List<double>();
                            //tempfcrntions.Add(ParentMass);
                            tempfcrntions.AddRange(CrntIons.Where(a => a <= Startvalueforions).Select(b => b).Reverse().ToList());
                            //tempfcrntions.Add(0);
                            //tempfcrntions.Reverse();
                            matchstartends.AddRange(FindmatchStartendsintherange(ReverseString.Reverse(startsequenceforboundtext), ModifiedSequenceForL, startindexforboundtext - 1, tempfcrntions, ParentMass, indexofmatchpair, Startvalueforions, true, true, reversecurntIons, Dontshowall));
                        }
                        else if (!matchstartends.First().MainSequence)
                        {
                            tempcrntions = ParentMass - Endvalueforions;
                            reversecurntIons = (ReverseCrntIons.Where(a => a <= tempcrntions).ToList());
                            //reversecurntIons = CrntIons.Where(a => a <= tempcrntions && a > 0).Select(b => b).ToList();
                            //reversecurntIons.AddRange(ReverseCrntIons.Where(a => a <= tempcrntions).ToList());
                            //reversecurntIons.Add(ParentMass);
                            //reversecurntIons.Add(0);
                            reversecurntIons = reversecurntIons.OrderBy(a => a).ToList();
                            List<double> tempfcrntions = new List<double>();
                            //tempfcrntions.Add(ParentMass);
                            tempfcrntions.AddRange(CrntIons.Where(a => a >= Endvalueforions && a <= ParentMass).Select(b => b).ToList());
                            tempfcrntions = tempfcrntions.OrderBy(a => a).ToList();
                            //tempfcrntions.Add(0);
                            matchstartends.AddRange(FindmatchStartendsintherange(ReverseString.Reverse(startsequenceforboundtext), ModifiedSequenceForL, startindexforboundtext - 1, tempfcrntions, ParentMass, indexofmatchpair, Endvalueforions, false, true, reversecurntIons, Dontshowall));
                        }


                        matchstartends = matchstartends.OrderBy(a => a.Start).ToList();

                        startvaluetocompare = Math.Round(matchstartends.Last().StartMonoMass, 1);
                        endvaluetocompare = Math.Round(matchstartends.Last().EndMonoMass, 1);

                        Startvalueforions = CrntIons.Where(a => Math.Round(a, 1) == startvaluetocompare).Any() ? CrntIons.Where(a => Math.Round(a, 1) == startvaluetocompare).First() : 0;
                        //indexforStartvalueforions = CrntIons.IndexOf(CrntIons.Where(a => Math.Round(a.MonoMass, 1) == startvaluetocompare).First());
                        Endvalueforions = CrntIons.Where(a => Math.Round(a, 1) == endvaluetocompare).Any() ? CrntIons.Where(a => Math.Round(a, 1) == endvaluetocompare).First() : 0;
                        //indexforEndvalueforions = CrntIons.Where(a => Math.Round(a.MonoMass, 1) == endvaluetocompare).Any() ? CrntIons.IndexOf(CrntIons.Where(a => Math.Round(a.MonoMass, 1) == endvaluetocompare).First()) : 0;

                        if (matchstartends.Last().MainSequence)
                        {
                            ///Find the place from which we need to use the reverseions
                            List<double> tempfcrntions = new List<double>(); /// CrntIons.Where(a => a.MonoMass >= Endvalueforions && a.MonoMass <= ParentMass).Select(b => b.MonoMass).ToList();
                            //tempfcrntions.Add(ParentMass);
                            tempfcrntions.AddRange(CrntIons.Where(a => a >= Endvalueforions && a <= ParentMass).Select(b => b).ToList());
                            //tempfcrntions.Add(0);
                            tempfcrntions = tempfcrntions.OrderByDescending(a => a).ToList();
                            //tempfcrntions.Reverse();

                            tempcrntions = ParentMass - Endvalueforions;
                            //reversecurntIons.Add(0);
                            //reversecurntIons = ReverseCrntIons.Where(a => a <= tempcrntions).ToList(); ///Also add all the CrntIons which are less than the tempcrntions
                            reversecurntIons = CrntIons.Where(a => a <= tempcrntions && a > 0).Select(b => b).ToList();///All the ions which are greater than the currentparentmass minus the curntion
                            reversecurntIons.AddRange(ReverseCrntIons.Where(a => a <= tempcrntions).ToList()); ///Also add all the CrntIons which are less than the tempcrntions
                            //reversecurntIons.Add(ParentMass);
                            reversecurntIons = reversecurntIons.OrderBy(a => a).ToList();  ///Order all the ions so we can use them later for finding ions
                            startvaluetocompare = 0;
                            ///Add all the matches which are found from the method
                            matchstartends.AddRange(FindmatchStartendsintherange(endsequenceforboundtext, ModifiedSequenceForL, endindexforboundtext - 1, tempfcrntions, ParentMass, indexofmatchpair, Endvalueforions, true, false, reversecurntIons, Dontshowall));
                        }
                        else if (!matchstartends.Last().MainSequence)
                        {
                            ///Find the place from which we need to use the reverseions
                            tempcrntions = ParentMass - Startvalueforions;
                            //reversecurntIons.Add(0);
                            //reversecurntIons = CrntIons.Where(a => a >= tempcrntions && a < ParentMass).Select(b => b).ToList();///All the ions which are greater than the currentparentmass minus the curntion
                            //reversecurntIons.AddRange(ReverseCrntIons.Where(a => a >= tempcrntions).ToList()); ///Also add all the CrntIons which are less than the tempcrntions
                            reversecurntIons = ReverseCrntIons.Where(a => a >= tempcrntions).ToList(); ///Also add all the CrntIons which are less than the tempcrntions
                            //reversecurntIons.Add(ParentMass);
                            reversecurntIons = reversecurntIons.OrderBy(a => a).ToList();  ///Order all the ions so we can use them later for finding ions
                            startvaluetocompare = 0;
                            List<double> tempfcrntions = new List<double>();
                            //tempfcrntions.Add(ParentMass);
                            tempfcrntions.AddRange(CrntIons.Where(a => a <= Startvalueforions).Select(b => b).ToList());
                            tempfcrntions = tempfcrntions.OrderBy(a => a).ToList();
                            //tempfcrntions.Add(0);
                            ///Add all the matches which are found from the method
                            matchstartends.AddRange(FindmatchStartendsintherange(endsequenceforboundtext, ModifiedSequenceForL, endindexforboundtext - 1, tempfcrntions, ParentMass, indexofmatchpair, Startvalueforions, false, false, reversecurntIons, Dontshowall));
                        }
                    }

                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception: " + ex.Message);
                    }

                    #endregion



                    matchstartends = matchstartends.OrderBy(a => a.Start).ToList();

                    Monomasses = matchstartends.Select(a => a.StartMonoMass).ToList();
                    Monomasses.AddRange(matchstartends.Select(a => a.EndMonoMass).ToList());

                    Monomasses = Monomasses.GroupBy(a => a).ToList().Select(a => a.First()).ToList();

                    Monomasses = Monomasses.OrderBy(a => a).ToList();

                    int st = matchstartends.First().Start;
                    int en = matchstartends.Last().End;

                    if (en == BoundText.Length)
                    {
                        en = en - 1;
                    }

                    double startmass = Math.Round(Monomasses.Last() - sequencelength(BoundText.Replace('I', 'L').Substring(st, en - st + 1)), 3);

                    double endmass = Math.Round(Monomasses.Last(), 3);

                    if (startmass < 0)
                    {
                        if (matchstartends.Any())
                        {
                            foreach (MatchStartEnds mse in matchstartends.Where(m => m.MonoMasses != null && m.MonoMasses.Any()))
                            {
                                st = mse.Start;
                                if (mse.MonoMasses == null) break;
                                startmass = Math.Round(mse.MonoMasses.Last() - sequencelength(BoundText.Replace('I', 'L').Substring(st, en - st + 1)), 3);
                                if (startmass > 0)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    int mass = 0;
                    if (!forwardorreverse)
                    {
                        mass = st;
                        st = en;
                        en = mass;
                    }

                    int stst = st;
                    int enen = en;
                }

                allthemonomasses.Add(ParentMass);
                allthemonomasses.Add(ParentMass - Molecules.Water);
                allthemonomasses.Add(0);
                allthemonomasses.Add(Molecules.Water);

                ///Based on the indices loop through all the 
                ///characters in the sequence to find if they are
                ///a part of the sequence.
                //matchstartends = matchstartends.OrderBy(a => a.Start).ToList();
                if (matchstartends.Count > 0)
                {
                    matchstartends.AddRange(FindEdges(matchstartends, ParentMass, length, ModifiedSequenceForL, ref allthemonomasses, Dontshowall));

                    int firststart = matchstartends.Select(a => a.Start).Min();
                    int lastend = matchstartends.Select(a => a.End).Max();

                    matchstartends.Add(new MatchStartEnds
                    {
                        Start = 0,
                        End = firststart,
                        confidence = AminAcidConfidence.NotPossible,
                        DontShowall = Dontshowall
                    });

                    matchstartends.Add(new MatchStartEnds
                    {
                        Start = lastend,
                        End = boundtextlength,
                        confidence = AminAcidConfidence.NotPossible,
                        DontShowall = Dontshowall
                    });
                    #region Checkforaminoacids
                    {
                        newmatchstartends = new List<MatchStartEnds>();

                        bool directions;
                        MatchStartEnds firstmtses = new MatchStartEnds();
                        int countforhighlight = 0;
                        bool breakornots = false;
                        if (!matchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).OrderByDescending(a => a.Length).First().MainSequence)/// (!matchstartends.Where(a => a.confidence == Confidence.Sure).OrderBy(a => a.End).ThenBy(a => a.Length).First().MainSequence)
                        {
                            newmatchstartends = matchstartends.Where(a => a.SequenceTag != null).OrderBy(a => a.End).ThenBy(a => a.Start).ThenBy(a => a.Length).ToList();
                            firstmtses = newmatchstartends.Where(a => a.SequenceTag != null).First();
                            countforhighlight = firstmtses.Start;
                            directions = false;
                        }
                        else
                        {
                            newmatchstartends = matchstartends.Where(a => a.SequenceTag != null).OrderByDescending(a => a.End).ThenByDescending(a => a.Start).ThenBy(a => a.Length).ToList();
                            firstmtses = newmatchstartends.Where(a => a.SequenceTag != null).First();
                            countforhighlight = firstmtses.End;
                            directions = true;
                        }

                        double currentends = firstmtses.EndMonoMass;
                        double currentstarts = 0;

                        bool reversedirection = directions;

                        int localcount = 0;
                        int modificationcount = 0;
                        foreach (MSViewer.MatchStartEnds mtse in newmatchstartends)
                        {
                            if (breakornots)
                                break;
                            if (mtse.confidence == AminAcidConfidence.Sure)
                            {
                                if (mtse.MainSequence == directions)
                                {
                                    if (directions)
                                        localcount = mtse.End - modificationcount;
                                    else
                                        localcount = mtse.Start + modificationcount;
                                    bool rawornot = false;
                                    string localsequence = string.Empty;
                                    if (AllSqsTags.Where(a => a.Sequence == mtse.SequenceTag).Any())
                                    {
                                        localsequence = AllSqsTags.Where(a => a.Sequence == mtse.SequenceTag).First().RawSequence;
                                        if (Regex.IsMatch(Convert.ToString(localsequence), @"^[a-z*]+$"))
                                            rawornot = true;
                                    }
                                    if (!rawornot)
                                    {
                                        currentstarts = currentends - sequencelength(mtse.SequenceTag);
                                    }
                                    else
                                    {
                                        currentstarts = currentends - sequencelengthusingmodifiedaminoacids(localsequence);
                                    }
                                    if (allthemonomasses.Where(a => Math.Abs(a - currentstarts) <= PPMCalc.CurrentPPM(currentstarts) + 0.2).ToList().Count == 0)
                                    {
                                        matchstartends.Where(a => a.Index == mtse.Index).First().breakornot = true;
                                        breakornots = true;
                                        break;
                                    }
                                    if (allthemonomasses.Where(a => Math.Abs(a - currentends) <= PPMCalc.CurrentPPM(currentends) + 0.2).ToList().Count == 0)
                                    {
                                        matchstartends.Where(a => a.Index == mtse.Index).First().breakornot = true;
                                        breakornots = true;
                                        break;
                                    }
                                    currentends = currentstarts;
                                }
                                else
                                {
                                    if (directions)
                                        localcount = mtse.End - modificationcount;
                                    else
                                        localcount = mtse.Start + modificationcount;
                                    bool rawornot = false;
                                    string localsequence = string.Empty;
                                    if (AllSqsTags.Where(a => a.Sequence == mtse.SequenceTag).Any())
                                    {
                                        localsequence = AllSqsTags.Where(a => a.Sequence == mtse.SequenceTag).First().RawSequence;
                                        if (Regex.IsMatch(Convert.ToString(localsequence), @"^[a-z*]+$"))
                                            rawornot = true;
                                    }
                                    if (!rawornot)
                                    {
                                        currentstarts = currentends - sequencelength(mtse.SequenceTag);
                                    }
                                    else
                                    {
                                        currentstarts = currentends - sequencelengthusingmodifiedaminoacids(localsequence);
                                    }
                                    if (allthemonomasses.Where(a => Math.Abs(a - currentstarts) <= PPMCalc.CurrentPPM(currentstarts) + 0.2).ToList().Count == 0)
                                    {
                                        matchstartends.Where(a => a.Index == mtse.Index).First().breakornot = true;
                                        breakornots = true;
                                        break;
                                    }
                                    if (allthemonomasses.Where(a => Math.Abs(a - currentends) <= PPMCalc.CurrentPPM(currentends) + 0.2).ToList().Count == 0)
                                    {
                                        matchstartends.Where(a => a.Index == mtse.Index).First().breakornot = true;
                                        breakornots = true;
                                        break;
                                    }
                                    currentends = currentstarts;
                                }
                            }
                            else if (mtse.confidence == AminAcidConfidence.Low)
                            {
                                if (mtse.MainSequence == directions)
                                {
                                    currentstarts = currentends - sequencelength(mtse.SequenceTag);
                                    if (allthemonomasses.Where(a => Math.Abs(a - currentstarts) <= PPMCalc.CurrentPPM(currentstarts) + 0.2).ToList().Count == 0)
                                    {
                                        matchstartends.Where(a => a.Index == mtse.Index).First().breakornot = true;
                                        breakornots = true;
                                        break;
                                    }
                                    if (allthemonomasses.Where(a => Math.Abs(a - currentends) <= PPMCalc.CurrentPPM(currentends) + 0.2).ToList().Count == 0)
                                    {
                                        matchstartends.Where(a => a.Index == mtse.Index).First().breakornot = true;
                                        breakornots = true;
                                        break;
                                    }
                                    currentends = currentstarts;
                                }
                                else
                                {
                                    currentstarts = currentends - sequencelength(mtse.SequenceTag);
                                    if (allthemonomasses.Where(a => Math.Abs(a - currentstarts) <= PPMCalc.CurrentPPM(currentstarts) + 0.2).ToList().Count == 0)
                                    {
                                        matchstartends.Where(a => a.Index == mtse.Index).First().breakornot = true;
                                        breakornots = true;
                                        break;
                                    }
                                    if (allthemonomasses.Where(a => Math.Abs(a - currentends) <= PPMCalc.CurrentPPM(currentends) + 0.2).ToList().Count == 0)
                                    {
                                        matchstartends.Where(a => a.Index == mtse.Index).First().breakornot = true;
                                        breakornots = true;
                                        break;
                                    }
                                    currentends = currentstarts;
                                }
                            }
                            else if (mtse.confidence == AminAcidConfidence.Gap)
                            {
                                currentstarts = currentends - sequencelength(mtse.SequenceTag) - mtse.Gap;
                                currentends = currentstarts;
                            }
                            else if (mtse.confidence == AminAcidConfidence.NotSure)
                            {
                                currentstarts = currentends - sequencelength(mtse.SequenceTag) - mtse.Gap;
                                currentends = currentstarts;
                            }
                        }
                        if (breakornots)
                        {
                            if (matchstartends.Any(a => a.confidence == AminAcidConfidence.Sure))
                            {
                                List<MatchStartEnds> newmmttsss = new List<MatchStartEnds>();
                                var newmatchstartendddss = matchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).OrderByDescending(a => a.SequenceTag.Length).First();

                                double startmnnmss = newmatchstartendddss.StartMonoMass;
                                double endmnnmss = newmatchstartendddss.EndMonoMass;

                                int sequencestart = newmatchstartendddss.Start;
                                int sequenceend = newmatchstartendddss.End;

                                string beforenewmatchstartendddss = string.Empty;
                                string afternewmatchstartendddss = string.Empty;

                                if (newmatchstartendddss.ReverseSequence)
                                {
                                    for (int i = newmatchstartendddss.End; i < length; i++)
                                    {
                                        beforenewmatchstartendddss = beforenewmatchstartendddss + BoundText[i].ToString();

                                        startmnnmss = startmnnmss - sequencelength(BoundText[i].ToString());


                                        if (i == length - 1)
                                        {
                                            if (startmnnmss <= 0)
                                            {
                                                newmmttsss.Add(new MatchStartEnds()
                                                    {
                                                        confidence = AminAcidConfidence.NotSure,
                                                        End = i - 1,
                                                        Start = newmatchstartendddss.End,
                                                        EndMonoMass = newmatchstartendddss.StartMonoMass,
                                                        Gap = startmnnmss + sequencelength(BoundText[i].ToString()),
                                                        StartMonoMass = 0,
                                                        Index = 19191919,
                                                        MainSequence = true,
                                                        ReverseSequence = false,
                                                        SequenceTag = ModifiedSequenceForL.Substring(newmatchstartendddss.End, length - newmatchstartendddss.End - 1),
                                                        DontShowall = Dontshowall
                                                    });
                                                break;
                                            }
                                            else
                                            {
                                                newmmttsss.Add(new MatchStartEnds()
                                                {
                                                    confidence = AminAcidConfidence.NotSure,
                                                    End = i,
                                                    Start = newmatchstartendddss.End,
                                                    EndMonoMass = newmatchstartendddss.StartMonoMass,
                                                    Gap = startmnnmss,
                                                    StartMonoMass = 0,
                                                    Index = 19119,
                                                    MainSequence = true,
                                                    ReverseSequence = false,
                                                    SequenceTag = ModifiedSequenceForL.Substring(newmatchstartendddss.End, length - newmatchstartendddss.End),
                                                    DontShowall = Dontshowall
                                                });
                                                break;
                                            }
                                        }
                                        if (startmnnmss <= 0)
                                        {
                                            newmmttsss.Add(new MatchStartEnds()
                                            {
                                                confidence = AminAcidConfidence.NotSure,
                                                End = i - 1,
                                                Start = newmatchstartendddss.End,
                                                EndMonoMass = newmatchstartendddss.StartMonoMass,
                                                Gap = startmnnmss + sequencelength(BoundText[i].ToString()),
                                                StartMonoMass = 0,
                                                Index = 10101,
                                                MainSequence = true,
                                                ReverseSequence = false,
                                                SequenceTag = ModifiedSequenceForL.Substring(newmatchstartendddss.End, length - newmatchstartendddss.End),
                                                DontShowall = Dontshowall
                                            });
                                            break;
                                        }
                                    }
                                    newmmttsss.Add(newmatchstartendddss);

                                    for (int i = newmatchstartendddss.Start - 1; i >= 0; i--)
                                    {
                                        afternewmatchstartendddss = BoundText[i] + afternewmatchstartendddss;

                                        endmnnmss = sequencelength(BoundText[i].ToString()) + endmnnmss;


                                        if (i == 0)
                                        {
                                            if (endmnnmss >= ParentMass)
                                            {
                                                newmmttsss.Add(new MatchStartEnds()
                                                {
                                                    confidence = AminAcidConfidence.NotSure,
                                                    Start = 1,
                                                    StartMonoMass = newmatchstartendddss.EndMonoMass,
                                                    End = newmatchstartendddss.Start,
                                                    EndMonoMass = ParentMass,
                                                    Index = 19191,
                                                    Gap = ParentMass - endmnnmss + sequencelength(BoundText[i].ToString()),
                                                    MainSequence = true,
                                                    ReverseSequence = false,
                                                    DontShowall = Dontshowall,
                                                    SequenceTag = ModifiedSequenceForL.Substring(1, newmatchstartendddss.Start - 1)
                                                });
                                                break;
                                            }
                                            else
                                            {
                                                newmmttsss.Add(new MatchStartEnds()
                                                {
                                                    confidence = AminAcidConfidence.NotSure,
                                                    Start = 0,
                                                    StartMonoMass = newmatchstartendddss.EndMonoMass,
                                                    End = newmatchstartendddss.Start,
                                                    EndMonoMass = ParentMass,
                                                    Index = 1010201,
                                                    Gap = ParentMass - endmnnmss,
                                                    MainSequence = true,
                                                    ReverseSequence = false,
                                                    DontShowall = Dontshowall,
                                                    SequenceTag = ModifiedSequenceForL.Substring(0, newmatchstartendddss.Start)
                                                });
                                                break;
                                            }
                                        }

                                        if (endmnnmss >= ParentMass)
                                        {
                                            newmmttsss.Add(new MatchStartEnds()
                                            {
                                                confidence = AminAcidConfidence.NotSure,
                                                Start = i + 1,
                                                StartMonoMass = newmatchstartendddss.EndMonoMass,
                                                End = newmatchstartendddss.Start,
                                                EndMonoMass = ParentMass,
                                                Index = 1919191919,
                                                Gap = ParentMass - endmnnmss + sequencelength(BoundText[i].ToString()),
                                                MainSequence = true,
                                                ReverseSequence = false,
                                                DontShowall = Dontshowall,
                                                SequenceTag = ModifiedSequenceForL.Substring(i + 1, newmatchstartendddss.Start - i - 1)
                                            });
                                            break;
                                        }
                                    }

                                    firststart = newmmttsss.MinBy(a => a.End).End;

                                    lastend = newmmttsss.MaxBy(a => a.End).End;

                                    newmmttsss.Add(new MatchStartEnds()
                                    {
                                        Start = 0,
                                        End = firststart,
                                        confidence = AminAcidConfidence.NotPossible,
                                        DontShowall = Dontshowall
                                    });

                                    newmmttsss.Add(new MatchStartEnds()
                                    {
                                        Start = lastend,
                                        End = boundtextlength,
                                        confidence = AminAcidConfidence.NotPossible,
                                        DontShowall = Dontshowall
                                    });

                                    newmmttsss = newmmttsss.OrderBy(a => a.Start).ToList();
                                    return newmmttsss;
                                }

                                if (startmnnmss != 0 && newmatchstartendddss.Start != 0)
                                {
                                    for (int i = newmatchstartendddss.Start; i >= 0; i--)
                                    {
                                        beforenewmatchstartendddss = BoundText[i].ToString() + beforenewmatchstartendddss;

                                        startmnnmss = startmnnmss - sequencelength(BoundText[i].ToString());

                                        if (i == 0)
                                        {
                                            if (startmnnmss <= 0)
                                            {
                                                newmmttsss.Add(new MatchStartEnds()
                                                {
                                                    confidence = AminAcidConfidence.NotSure,
                                                    End = newmatchstartendddss.Start,
                                                    Start = i,
                                                    EndMonoMass = newmatchstartendddss.StartMonoMass,
                                                    Gap = startmnnmss,
                                                    StartMonoMass = 0,
                                                    Index = 19191919,
                                                    MainSequence = newmatchstartendddss.MainSequence,
                                                    ReverseSequence = !newmatchstartendddss.MainSequence,
                                                    SequenceTag = ModifiedSequenceForL.Substring(1, newmatchstartendddss.Start - 1),
                                                    DontShowall = Dontshowall
                                                });
                                                break;
                                            }
                                            else
                                            {
                                                newmmttsss.Add(new MatchStartEnds()
                                                {
                                                    confidence = AminAcidConfidence.NotSure,
                                                    End = newmatchstartendddss.Start,
                                                    Start = i,
                                                    EndMonoMass = newmatchstartendddss.StartMonoMass,
                                                    Gap = startmnnmss,
                                                    StartMonoMass = 0,
                                                    Index = 19119,
                                                    MainSequence = newmatchstartendddss.MainSequence,
                                                    ReverseSequence = !newmatchstartendddss.MainSequence,
                                                    SequenceTag = ModifiedSequenceForL.Substring(0, newmatchstartendddss.Start),
                                                    DontShowall = Dontshowall
                                                });
                                                break;
                                            }
                                        }

                                        if (startmnnmss <= 0)
                                        {
                                            newmmttsss.Add(new MatchStartEnds()
                                            {
                                                confidence = AminAcidConfidence.NotSure,
                                                End = newmatchstartendddss.Start,
                                                Start = i + 1,
                                                EndMonoMass = newmatchstartendddss.StartMonoMass,
                                                Gap = startmnnmss + sequencelength(BoundText[i].ToString()),
                                                StartMonoMass = 0,
                                                Index = 10101,
                                                MainSequence = newmatchstartendddss.MainSequence,
                                                ReverseSequence = !newmatchstartendddss.MainSequence,
                                                SequenceTag = ModifiedSequenceForL.Substring(i + 1, newmatchstartendddss.Start - (i)),
                                                DontShowall = Dontshowall
                                            });
                                            break;
                                        }

                                    }
                                }
                                newmmttsss.Add(newmatchstartendddss);
                                if (endmnnmss != ParentMass && newmatchstartendddss.End != BoundText.Length)
                                {
                                    for (int i = newmatchstartendddss.End; i < BoundText.Length; i++)
                                    {
                                        afternewmatchstartendddss = afternewmatchstartendddss + BoundText[i];

                                        endmnnmss = sequencelength(BoundText[i].ToString()) + endmnnmss;

                                        if (i == BoundText.Length - 1)
                                        {
                                            if (endmnnmss >= ParentMass)
                                            {
                                                newmmttsss.Add(new MatchStartEnds()
                                                {
                                                    confidence = AminAcidConfidence.NotSure,
                                                    Start = newmatchstartendddss.End,
                                                    StartMonoMass = newmatchstartendddss.EndMonoMass,
                                                    End = i - 1,
                                                    EndMonoMass = ParentMass,
                                                    Index = 19191,
                                                    Gap = ParentMass - endmnnmss,
                                                    MainSequence = newmatchstartendddss.MainSequence,
                                                    ReverseSequence = !newmatchstartendddss.MainSequence,
                                                    DontShowall = Dontshowall,
                                                    SequenceTag = ModifiedSequenceForL.Substring(newmatchstartendddss.End - 1, ModifiedSequenceForL.Length - (newmatchstartendddss.End - 1))
                                                });
                                                break;
                                            }
                                            else
                                            {
                                                newmmttsss.Add(new MatchStartEnds()
                                                {
                                                    confidence = AminAcidConfidence.NotSure,
                                                    Start = newmatchstartendddss.End,
                                                    StartMonoMass = newmatchstartendddss.EndMonoMass,
                                                    End = i,
                                                    EndMonoMass = ParentMass,
                                                    Index = 823891,
                                                    Gap = ParentMass - endmnnmss,
                                                    MainSequence = newmatchstartendddss.MainSequence,
                                                    ReverseSequence = !newmatchstartendddss.MainSequence,
                                                    DontShowall = Dontshowall,
                                                    SequenceTag = ModifiedSequenceForL.Substring(newmatchstartendddss.End, ModifiedSequenceForL.Length - newmatchstartendddss.End)
                                                });
                                                break;
                                            }
                                        }

                                        if (endmnnmss >= ParentMass)
                                        {
                                            newmmttsss.Add(new MatchStartEnds()
                                            {
                                                confidence = AminAcidConfidence.NotSure,
                                                Start = newmatchstartendddss.End,
                                                StartMonoMass = newmatchstartendddss.EndMonoMass,
                                                End = i - 1,
                                                EndMonoMass = ParentMass,
                                                Index = 19191,
                                                Gap = ParentMass - endmnnmss,
                                                MainSequence = newmatchstartendddss.MainSequence,
                                                ReverseSequence = !newmatchstartendddss.MainSequence,
                                                DontShowall = Dontshowall,
                                                SequenceTag = ModifiedSequenceForL.Substring(newmatchstartendddss.End - 1, ModifiedSequenceForL.Length - (newmatchstartendddss.End - 1))
                                            });
                                            break;
                                        }
                                    }
                                }
                                firststart = newmmttsss.MinBy(a => a.End).End;

                                lastend = newmmttsss.MaxBy(a => a.End).End;

                                newmmttsss.Add(new MatchStartEnds()
                                {
                                    Start = 0,
                                    End = firststart,
                                    confidence = AminAcidConfidence.NotPossible,
                                    DontShowall = Dontshowall
                                });

                                newmmttsss.Add(new MatchStartEnds()
                                {
                                    Start = lastend,
                                    End = boundtextlength,
                                    confidence = AminAcidConfidence.NotPossible,
                                    DontShowall = Dontshowall
                                });

                                newmmttsss = newmmttsss.OrderBy(a => a.Start).ToList();
                                return newmmttsss;
                            }
                            else
                            {
                                if (matchstartends.Where(a => a.SequenceTag == PairMatch.First()).Any() && matchstartends.Where(a => a.SequenceTag == PairMatch.First()).First().breakornot)
                                {
                                    newmatchstartends.Clear();
                                    newmatchstartends.Add(matchstartends.Where(a => a.SequenceTag == PairMatch.First()).First());
                                    newmatchstartends[0].breakornot = false;
                                    matchstartends = newmatchstartends;
                                }
                                else if (!matchstartends.Where(a => a.SequenceTag != null).OrderByDescending(a => a.SequenceTag.Length).First().MainSequence)
                                {
                                    newmatchstartends = matchstartends.Where(a => a.SequenceTag != null).OrderByDescending(a => a.End).ThenByDescending(a => a.Start).ThenBy(a => a.Length).ToList();
                                }
                                else
                                {
                                    newmatchstartends = matchstartends.Where(a => a.SequenceTag != null).OrderBy(a => a.Start).OrderBy(a => a.End).ThenBy(a => a.Length).ToList();
                                }
                                bool trueorfalse = false;
                                List<MatchStartEnds> tempmatchstartends = new List<MatchStartEnds>();
                                tempmatchstartends.AddRange(newmatchstartends);
                                int localii = 0;
                                foreach (MatchStartEnds mtse in newmatchstartends)
                                {
                                    if (trueorfalse)
                                    {
                                        tempmatchstartends.Remove(matchstartends.Where(a => a.forsort == mtse.forsort).First());
                                        //matchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.NotSure;
                                        continue;
                                    }
                                    if (mtse.breakornot)
                                    {
                                        trueorfalse = true;

                                        //matchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.NotSure;
                                        tempmatchstartends.Remove(matchstartends.Where(a => a.forsort == mtse.forsort).First());

                                        if (localii > 0 && tempmatchstartends[localii - 1].confidence == AminAcidConfidence.Gap)
                                        {
                                            tempmatchstartends.Remove(matchstartends.Where(a => a.forsort == tempmatchstartends[localii - 1].forsort).First());
                                        }
                                    }
                                    localii++;
                                }
                                if (tempmatchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).Any())
                                {
                                    matchstartends = tempmatchstartends;
                                    if (matchstartends.Where(a => a.Start == 0 && a.End == 0).Any())
                                    {
                                        matchstartends.Remove(matchstartends.Where(a => a.Start == 0 && a.End == 0).First());
                                    }

                                    if (matchstartends.Where(a => a.Start == lengthofmodifiedsequenceforl && a.End == lengthofmodifiedsequenceforl).Any())
                                    {
                                        matchstartends.Remove(matchstartends.Where(a => a.Start == lengthofmodifiedsequenceforl && a.End == lengthofmodifiedsequenceforl).First());
                                    }

                                    matchstartends.AddRange(FindEdges(matchstartends, ParentMass, length, ModifiedSequenceForL, ref allthemonomasses, Dontshowall));

                                    firststart = matchstartends.Select(a => a.Start).Min();
                                    lastend = matchstartends.Select(a => a.End).Max();

                                    matchstartends.Add(new MatchStartEnds
                                    {
                                        Start = 0,
                                        End = firststart,
                                        confidence = AminAcidConfidence.NotPossible,
                                        DontShowall = Dontshowall
                                    });

                                    matchstartends.Add(new MatchStartEnds
                                    {
                                        Start = lastend,
                                        End = boundtextlength,
                                        confidence = AminAcidConfidence.NotPossible,
                                        DontShowall = Dontshowall
                                    });
                                }
                                else
                                {
                                    newmatchstartends = matchstartends.Where(a => a.SequenceTag != null).OrderByDescending(a => a.Start).ThenByDescending(a => a.End).ThenBy(a => a.Length).ToList();
                                    tempmatchstartends.Clear();
                                    tempmatchstartends.AddRange(newmatchstartends);
                                    foreach (MatchStartEnds mtse in newmatchstartends)
                                    {
                                        if (trueorfalse)
                                        {
                                            tempmatchstartends.Remove(matchstartends.Where(a => a.Index == mtse.Index).First());
                                            //matchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.NotSure;
                                            continue;
                                        }
                                        if (mtse.breakornot)
                                        {
                                            trueorfalse = true;
                                            //matchstartends.Where(a => a.Index == mtse.Index).First().confidence = Confidence.NotSure;
                                            tempmatchstartends.Remove(matchstartends.Where(a => a.Index == mtse.Index).First());
                                        }
                                    }
                                    if (tempmatchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).Any())
                                    {
                                        matchstartends = tempmatchstartends;
                                        if (matchstartends.Where(a => a.Start == 0 && a.End == 0).Any())
                                        {
                                            matchstartends.Remove(matchstartends.Where(a => a.Start == 0 && a.End == 0).First());
                                        }

                                        if (matchstartends.Where(a => a.Start == lengthofmodifiedsequenceforl && a.End == lengthofmodifiedsequenceforl).Any())
                                        {
                                            matchstartends.Remove(matchstartends.Where(a => a.Start == lengthofmodifiedsequenceforl && a.End == lengthofmodifiedsequenceforl).First());
                                        }

                                        matchstartends.AddRange(FindEdges(matchstartends, ParentMass, length, ModifiedSequenceForL, ref allthemonomasses, Dontshowall));

                                        firststart = matchstartends.Select(a => a.Start).Min();
                                        lastend = matchstartends.Select(a => a.End).Max();

                                        matchstartends.Add(new MatchStartEnds
                                        {
                                            Start = 0,
                                            End = firststart,
                                            confidence = AminAcidConfidence.NotPossible,
                                            DontShowall = Dontshowall
                                        });

                                        matchstartends.Add(new MatchStartEnds
                                        {
                                            Start = lastend,
                                            End = boundtextlength,
                                            confidence = AminAcidConfidence.NotPossible,
                                            DontShowall = Dontshowall
                                        });
                                    }
                                    else
                                    {
                                        matchstartends.Clear();
                                        foreach (string prmatch in PairMatch)
                                        {
                                            int indexofpairmatch = ModifiedSequenceForL.IndexOf(prmatch);
                                            List<double> lls = new List<double>();

                                            if (!AllSqsTags.Any(a => a.Sequence == prmatch)) continue;

                                            lls.AddRange(AllSqsTags.Where(a => a.Sequence == prmatch).First().IndividualAAs.Select(a => a.End).ToList());
                                            lls.AddRange(AllSqsTags.Where(a => a.Sequence == prmatch).First().IndividualAAs.Select(a => a.Start).ToList());

                                            if (indexofpairmatch > 0)
                                            {
                                                matchstartends.Add(new MatchStartEnds
                                                {
                                                    Start = indexofpairmatch,
                                                    End = indexofpairmatch + prmatch.Length,
                                                    SequenceTag = prmatch,
                                                    confidence = AminAcidConfidence.Sure,
                                                    StartMonoMass = lls.Min(),
                                                    EndMonoMass = lls.Max(),
                                                    MainSequence = true,
                                                    MonoMasses = lls.GroupBy(a => a).Select(b => b.First()).ToList(),
                                                    DontShowall = Dontshowall
                                                });
                                            }
                                            else
                                            {
                                                indexofpairmatch = reversesequence.IndexOf(prmatch);
                                                matchstartends.Add(new MatchStartEnds
                                                {
                                                    Start = length - (indexofpairmatch + prmatch.Length),
                                                    End = length - indexofpairmatch,
                                                    SequenceTag = prmatch,
                                                    confidence = AminAcidConfidence.Sure,
                                                    StartMonoMass = lls.Min(),
                                                    EndMonoMass = lls.Max(),
                                                    MainSequence = false,
                                                    MonoMasses = lls.GroupBy(a => a).Select(b => b.First()).ToList(),
                                                    DontShowall = Dontshowall
                                                });
                                            }
                                        }

                                        matchstartends.AddRange(FindEdges(matchstartends, ParentMass, length, ModifiedSequenceForL, ref allthemonomasses));

                                        int start = matchstartends.Select(a => a.Start).Min();
                                        int end = matchstartends.Select(a => a.End).Max();
                                        if (!(start == 0 && end == ModifiedSequenceForL.Length))
                                        {
                                            matchstartends.Add(new MatchStartEnds
                                            {
                                                Start = 0,
                                                End = start,
                                                confidence = AminAcidConfidence.Reallybad,
                                                DontShowall = Dontshowall
                                            });
                                            matchstartends.Add(new MatchStartEnds
                                            {
                                                Start = end,
                                                End = length,
                                                confidence = AminAcidConfidence.Reallybad,
                                                DontShowall = Dontshowall
                                            });
                                        }
                                        matchstartends = matchstartends.OrderBy(a => a.Start).ToList();
                                        end = matchstartends.First().End;
                                        List<MatchStartEnds> tempmtse = new List<MatchStartEnds>();
                                        foreach (MatchStartEnds mtse in matchstartends.Skip(1))
                                        {
                                            if (end == mtse.Start)
                                            {
                                                end = mtse.End;
                                            }
                                            else
                                            {
                                                tempmtse.Add(new MatchStartEnds
                                                {
                                                    Start = end,
                                                    End = mtse.Start,
                                                    confidence = AminAcidConfidence.Reallybad,
                                                    DontShowall = Dontshowall
                                                });
                                                end = mtse.End;
                                            }
                                        }
                                        matchstartends.AddRange(tempmtse);
                                        return matchstartends;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (IsBlastp)
                {
                    blastpmatch = true;

                    FindSequenceTags.SequenceTag sqstg = new FindSequenceTags.SequenceTag();
                    int firststart = 0;
                    int lastend = 0;
                    if (AllSqsTags.Where(a => a.Sequence == BlastpPairMatch).Any())
                    {
                        sqstg = AllSqsTags.Where(a => a.Sequence == BlastpPairMatch).First();
                    }
                    else if (AllSqsTags.Any(a => a.Sequence.Contains(BlastpPairMatch)))
                    {
                        sqstg = AllSqsTags.Where(a => a.Sequence.Contains(BlastpPairMatch)).First();
                    }
                    else if (AllSqsTags.Where(a => a.Sequence == ReverseString.Reverse(BlastpPairMatch)).Any())
                    {
                        sqstg = AllSqsTags.Where(a => a.Sequence == ReverseString.Reverse(BlastpPairMatch)).First();
                    }
                    else if (AllSqsTags.Any(a => a.Sequence.Contains(ReverseString.Reverse(BlastpPairMatch))))
                    {
                        sqstg = AllSqsTags.Where(a => a.Sequence.Contains(ReverseString.Reverse(BlastpPairMatch))).First();
                    }


                    matchstartends.AddRange(ApplyHighlightforBlastPwithnoSequences(BoundText, BlastpTag, sqstg, CrntIons, ParentMass, Dontshowall));

                    matchstartends.AddRange(FindEdges(matchstartends, ParentMass, length, ModifiedSequenceForL, ref allthemonomasses, Dontshowall));

                    firststart = matchstartends.Select(a => a.Start).Min();
                    lastend = matchstartends.Select(a => a.End).Max();
                    matchstartends.Add(new MatchStartEnds
                    {
                        Start = 0,
                        End = firststart,
                        confidence = AminAcidConfidence.NotPossible,
                        DontShowall = Dontshowall
                    });

                    matchstartends.Add(new MatchStartEnds
                    {
                        Start = lastend,
                        End = boundtextlength,
                        confidence = AminAcidConfidence.NotPossible,
                        DontShowall = Dontshowall
                    });

                    return matchstartends;
                }
                else
                {
                    int icount = 0;
                    foreach (string prmatch in PairMatch)
                    {
                        int indexofpairmatch = ModifiedSequenceForL.IndexOf(prmatch);
                        List<double> lls = new List<double>();

                        if (!(AllSqsTags.Any(a => a.Sequence.Contains(prmatch)) || AllSqsTags.Any(a => a.Sequence.Contains(ReverseString.Reverse(prmatch))))) continue;

                        if (AllSqsTags.Any(a => a.Sequence.Contains(prmatch)))
                        {
                            lls.AddRange(AllSqsTags.Where(a => a.Sequence.Contains(prmatch)).First().IndividualAAs.Select(a => a.End).ToList());
                            lls.AddRange(AllSqsTags.Where(a => a.Sequence.Contains(prmatch)).First().IndividualAAs.Select(a => a.Start).ToList());
                        }

                        if (AllSqsTags.Any(a => a.Sequence.Contains(ReverseString.Reverse(prmatch))))
                        {
                            lls.AddRange(AllSqsTags.Where(a => a.Sequence.Contains(ReverseString.Reverse(prmatch))).First().IndividualAAs.Select(a => a.End).ToList());
                            lls.AddRange(AllSqsTags.Where(a => a.Sequence.Contains(ReverseString.Reverse(prmatch))).First().IndividualAAs.Select(a => a.Start).ToList());
                        }

                        //lls.AddRange(AllSqsTags.Where(a => a.Sequence == prmatch).First().IndividualAAs.Select(a => a.End).ToList());
                        //lls.AddRange(AllSqsTags.Where(a => a.Sequence == prmatch).First().IndividualAAs.Select(a => a.Start).ToList());

                        if (indexofpairmatch >= 0)
                        {
                            icount++;
                            matchstartends.Add(new MatchStartEnds
                            {
                                Start = indexofpairmatch,
                                End = indexofpairmatch + prmatch.Length,
                                SequenceTag = prmatch,
                                confidence = AminAcidConfidence.Sure,
                                StartMonoMass = lls.Min(),
                                EndMonoMass = lls.Max(), /// lls.Min() + sequencelength(prmatch),/// lls.Max(),
                                MainSequence = true,
                                MonoMasses = lls.GroupBy(a => a).Select(b => b.First()).ToList(),
                                DontShowall = Dontshowall
                            });
                        }
                        else
                        {
                            indexofpairmatch = reversesequence.IndexOf(prmatch);
                            if (indexofpairmatch >= 0)
                            {
                                icount++;
                                matchstartends.Add(new MatchStartEnds
                                {
                                    Start = length - (indexofpairmatch + prmatch.Length),
                                    End = length - indexofpairmatch,
                                    SequenceTag = prmatch,
                                    confidence = AminAcidConfidence.Sure,
                                    StartMonoMass = lls.Min(),
                                    EndMonoMass = lls.Max(), ///   lls.Min() + sequencelength(prmatch),/// lls.Max(),
                                    MainSequence = false,
                                    MonoMasses = lls.GroupBy(a => a).Select(b => b.First()).ToList(),
                                    DontShowall = Dontshowall
                                });
                            }
                        }
                    }

                    if (icount == 0)
                    {
                        matchstartends.Add(new MatchStartEnds
                        {
                            Start = 0,
                            End = length,
                            SequenceTag = BoundText,
                            confidence = AminAcidConfidence.NotPossible,
                            DontShowall = Dontshowall
                        });
                        return matchstartends;
                    }

                    matchstartends.AddRange(FindEdges(matchstartends, ParentMass, length, ModifiedSequenceForL, ref allthemonomasses));

                    int start = matchstartends.Select(a => a.Start).Min();
                    int end = matchstartends.Select(a => a.End).Max();
                    matchstartends.Add(new MatchStartEnds
                    {
                        Start = 0,
                        End = start,
                        confidence = AminAcidConfidence.Reallybad,
                        DontShowall = Dontshowall
                    });
                    matchstartends.Add(new MatchStartEnds
                    {
                        Start = end,
                        End = length,
                        confidence = AminAcidConfidence.Reallybad,
                        DontShowall = Dontshowall
                    });

                    matchstartends = matchstartends.OrderBy(a => a.Start).ToList();
                    end = matchstartends.First().End;
                    List<MatchStartEnds> tempmtse = new List<MatchStartEnds>();
                    foreach (MatchStartEnds mtse in matchstartends.Skip(1))
                    {
                        if (end == mtse.Start)
                        {
                            end = mtse.End;
                        }
                        else
                        {
                            tempmtse.Add(new MatchStartEnds
                            {
                                Start = end,
                                End = mtse.Start,
                                confidence = AminAcidConfidence.Reallybad,
                                DontShowall = Dontshowall
                            });
                            end = mtse.End;
                        }
                    }
                    matchstartends.AddRange(tempmtse);
                }
                matchstartends = matchstartends.OrderBy(a => a.Start).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }

            return matchstartends;
        }

        public static void Findmatches()
        {
            List<FindSequenceTags.SequenceTag> singleaminoacids = new List<FindSequenceTags.SequenceTag>();
            List<FindSequenceTags.SequenceTag> reversesingleaminoacids = new List<FindSequenceTags.SequenceTag>();

            singleaminoacids.Add(new FindSequenceTags.SequenceTag()
            {
                Start = 1000.20,
                End = 1071.23711,
                RawSequence = "A",
                NumberofAA = 1,
                Index = "1",
            });

            reversesingleaminoacids.Add(new FindSequenceTags.SequenceTag()
            {
                Start = 4373.99939,
                End = 4445.0365,
                RawSequence = "A",
                NumberofAA = 1,
                Index = "8",
            });

            singleaminoacids.Add(new FindSequenceTags.SequenceTag()
            {
                Start = 1120.077,
                End = 1251.11749,
                RawSequence = "M",
                NumberofAA = 1,
                Index = "2",
            });

            reversesingleaminoacids.Add(new FindSequenceTags.SequenceTag()
            {
                Start = 4194.11901,
                End = 4325.1595,
                RawSequence = "M",
                NumberofAA = 1,
                Index = "9",
            });

            singleaminoacids.Add(new FindSequenceTags.SequenceTag()
            {
                Start = 1330.0042,
                End = 1444.04713,
                RawSequence = "N",
                NumberofAA = 1,
                Index = "3",
            });


            //reversesingleaminoacids.Add(new FindSequenceTags.SequenceTag()
            //{
            //    Start = 4001.18937,
            //    End = 4115.2323,
            //    RawSequence = "N",
            //    NumberofAA = 1,
            //    Index = "10",
            //});

            //singleaminoacids.Add(new FindSequenceTags.SequenceTag()
            //{
            //    Start = 1444.04713,
            //    End = 1600.14824,
            //    RawSequence = "R",
            //    NumberofAA = 1,
            //    Index = "4",
            //});

            //reversesingleaminoacids.Add(new FindSequenceTags.SequenceTag()
            //{
            //    Start = 3845.08826, // 1444.04713,
            //    End = 4001.18937,
            //    RawSequence = "R",
            //    NumberofAA = 1,
            //    Index = "11",
            //});


            //singleaminoacids.Add(new FindSequenceTags.SequenceTag()
            //{
            //    Start = 1600.14824,
            //    End = 1713.2323,
            //    RawSequence = "L",
            //    NumberofAA = 1,
            //    Index = "5",
            //});

            //reversesingleaminoacids.Add(new FindSequenceTags.SequenceTag()
            //{
            //    Start = 3732.0042, // 1600.14824,
            //    End = 3845.08826,
            //    RawSequence = "L",
            //    NumberofAA = 1,
            //    Index = "12",
            //});

            singleaminoacids.Add(new FindSequenceTags.SequenceTag()
            {
                Start = 1713.2323,
                End = 1770.25376,
                RawSequence = "G",
                NumberofAA = 1,
                Index = "6"
            });

            //reversesingleaminoacids.Add(new FindSequenceTags.SequenceTag()
            //{
            //    Start = 3674.98274, /// 1713.2323,
            //    End = 3732.0042,
            //    RawSequence = "G",
            //    NumberofAA = 1,
            //    Index = "13"
            //});

            //singleaminoacids.Add(new FindSequenceTags.SequenceTag()
            //{
            //    Start = 1770.25376,
            //    End = 1901.29425,
            //    RawSequence = "M",
            //    NumberofAA = 1,
            //    Index = "7"
            //});

            reversesingleaminoacids.Add(new FindSequenceTags.SequenceTag()
            {
                Start = 3543.94225,
                End = 3674.98274, ///1901.29425,
                RawSequence = "M",
                NumberofAA = 1,
                Index = "14"
            });

            string Sequence = "AMNRLGM";

            double startmonomass = 1000.20;

            double endmonomass = 2000.20512;

            double Parentmass = 5445.2365;

            int start = 3;
            int end = 10;

            int indexofpairmatch = 20;
            bool DontShowall = false;

            List<MatchStartEnds> mtses = FindMatchingAminoAcids(singleaminoacids, reversesingleaminoacids, Sequence, startmonomass, endmonomass, Parentmass, start, end, ref indexofpairmatch, DontShowall, true);

            foreach (var mt in mtses)
            {

            }
        }

        /// <summary>
        /// Comparing the sequence with the single amino acids.
        /// </summary>
        /// <param name="singleaminoacids">All the amino acids with in the range</param>
        /// <param name="Sequence"> Part of the Sequence which needs to be matched </param>
        /// <param name="startmonomass"> Startmonomass of the range </param>
        /// <param name="endmonomass"> Endmonomass of the range </param>
        /// <param name="ParentMass"> ParentMass </param>
        /// <returns></returns>
        static List<MatchStartEnds> FindMatchingAminoAcids(List<FindSequenceTags.SequenceTag> singleaminoacids, List<FindSequenceTags.SequenceTag> reversesingleaminoacids, string Sequence, double startmonomass, double endmonomass, double ParentMass, int start, int end, ref int indexofpairmatch, bool Dontshowall, bool direction) ///, Confidence confidence)
        {
            List<MatchStartEnds> mtse = new List<MatchStartEnds>();

            Dictionary<int, CompareAminoAcids> cmpaasdi = new Dictionary<int, CompareAminoAcids>(); ///Dictionary for comparision

            List<double> allmonomasses = new List<double>();

            allmonomasses.AddRange(singleaminoacids.Select(a => a.Start).ToList());
            allmonomasses.AddRange(singleaminoacids.Select(a => a.End).ToList());

            allmonomasses.AddRange(reversesingleaminoacids.Select(a => a.Start).ToList());
            allmonomasses.AddRange(reversesingleaminoacids.Select(a => a.End).ToList());

            allmonomasses.Add(startmonomass);
            allmonomasses.Add(endmonomass);

            singleaminoacids.AddRange(reversesingleaminoacids);

            double tempstartmonomass = startmonomass;
            double tempendmonomass = endmonomass;
            int tempstart = start;
            int tempend = end;
            int numberofaminoacids = 0;
            foreach (string s in string.Join(",", Sequence.ToCharArray()).Split(',')) ///Adding all the values of the sequence to the compareaminoacid dictionary.
            {
                cmpaasdi.Add(numberofaminoacids, (new CompareAminoAcids()
                {
                    startmonomass = Math.Round(tempstartmonomass, 3),
                    start = tempstart,
                    Aminoacid = s,
                    Index = numberofaminoacids
                }));
                numberofaminoacids++;
                tempstartmonomass = tempstartmonomass + sequencelength(s);
                tempstart++;
            }

            Dictionary<int, CompareAminoAcids> reversecmpaasdi = new Dictionary<int, CompareAminoAcids>();

            int reversenumberofaminoacids = 0;
            foreach (var cmp in cmpaasdi)///Adding all the values of the sequence to the reverse compareaminoacid dictionary.
            {
                reversecmpaasdi.Add(reversenumberofaminoacids, (new CompareAminoAcids()
                {
                    startmonomass = Math.Round(ParentMass - cmp.Value.endmonomass, 3),
                    Aminoacid = cmp.Value.Aminoacid,
                    Index = cmp.Value.Index,
                    start = cmp.Value.start
                }));
                reversenumberofaminoacids++;
            }
            int tempnumberofaminoacids = 0;

            List<int> removedkeys = new List<int>();
            tempstart = start;
            string temporarystring = string.Empty;

            ///Looping through all the values of Amino Acids in the forward direction
            while (true)
            {
                var cmp = cmpaasdi[tempnumberofaminoacids]; //Getting a value of each amino acid
                if (singleaminoacids.Where(a => a.Sequence == cmp.Aminoacid && (Math.Abs(a.Start - cmp.startmonomass) < PPM.CurrentPPMbasedonMatchList(Math.Max(a.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM))).Any()) //Comparing the current Amino Acid with any present amino acids
                {
                    indexofpairmatch++;
                    var smp = singleaminoacids.Where(a => a.Sequence == cmp.Aminoacid && (Math.Abs(a.Start - cmp.startmonomass) < PPM.CurrentPPMbasedonMatchList(Math.Max(a.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM))).First();

                    reversecmpaasdi.Remove(cmp.Index); //If it's found in the forward direction then remove it from the reverse direction. Don't need to compare
                    removedkeys.Add(cmp.Index); // If found in the forward direction adding it to a list of indices
                    cmpaasdi.Remove(cmp.Index);
                    List<double> monomasses = new List<double>();
                    monomasses.Add(cmp.startmonomass);
                    monomasses.Add(cmp.endmonomass);

                    ///Creating a MatchStartend for the amino acid found.
                    mtse.Add(new MatchStartEnds()
                    {
                        confidence = AminAcidConfidence.Sure,
                        Start = tempstart,
                        End = tempstart + 1,
                        StartMonoMass = smp.Start,
                        EndMonoMass = smp.End,
                        Index = indexofpairmatch,
                        MainSequence = true,
                        ReverseSequence = false,
                        MonoMasses = monomasses,
                        SequenceTag = cmp.Aminoacid,
                        DontShowall = Dontshowall
                    });
                    tempstart++;
                    temporarystring = string.Empty;
                }
                else if (singleaminoacids.Where(a => a.Sequence == cmp.Aminoacid && Math.Abs((Math.Abs(a.Start - cmp.startmonomass) - PPM.CurrentPPMbasedonMatchList(Math.Max(a.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM))) < 100).Any()) ///If there is a gap
                {
                    indexofpairmatch++;

                    var smp = singleaminoacids.Where(a => a.Sequence == cmp.Aminoacid && Math.Abs((Math.Abs(a.Start - cmp.startmonomass) - PPM.CurrentPPMbasedonMatchList(Math.Max(a.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM))) < 100).First();
                    double Gap = Math.Abs(Math.Abs(smp.Start - cmp.startmonomass)); /// - PPM.CurrentPPMbasedonMatchList(Math.Max(smp.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM)));

                    cmpaasdi.Where(a => a.Key != tempnumberofaminoacids && a.Value.start > cmp.start).ToDictionary(kp => kp.Key, kp => kp.Value.startmonomass = kp.Value.startmonomass + Gap); //Adding the Gap to other AminoAcids
                    cmpaasdi.Remove(cmp.Index);
                    reversecmpaasdi.Remove(cmp.Index); //If it's found in the forward direction then remove it from the reverse direction. Don't need to compare
                    removedkeys.Add(cmp.Index); // If found in the forward direction adding it to a list of indices

                    reversecmpaasdi.Where(a => a.Key != tempnumberofaminoacids).ToDictionary(kp => kp.Key, kp => kp.Value.startmonomass = kp.Value.startmonomass - Gap); //Adding the Gap to other AminoAcids

                    List<double> monomasses = new List<double>();
                    monomasses.Add(cmp.startmonomass);
                    monomasses.Add(cmp.endmonomass);

                    ///Creating another MatchStartEnd for the AminoAcid
                    mtse.Add(new MatchStartEnds()
                    {
                        confidence = AminAcidConfidence.Sure,
                        Start = tempstart,
                        End = tempstart + 1,
                        StartMonoMass = smp.Start,
                        EndMonoMass = smp.End, // cmp.startmonomass + Gap, /// smp.End,
                        Index = indexofpairmatch,
                        MainSequence = true,
                        ReverseSequence = false,
                        MonoMasses = monomasses,
                        SequenceTag = cmp.Aminoacid,
                        DontShowall = Dontshowall,
                        Gap = Math.Round(Gap, 3)
                    });

                    //mtse.Add(new MatchStartEnds()
                    //{

                    //});

                    temporarystring = string.Empty;

                    ///Creating a MatchStartEnd for the Gap
                    //mtse.Add(new MatchStartEnds()
                    //{
                    //    confidence = Confidence.Gap,
                    //    Start = tempstart,
                    //    End = tempstart + 1,
                    //    StartMonoMass = cmp.startmonomass,
                    //    EndMonoMass = cmp.startmonomass + Gap,
                    //    Index = indexofpairmatch,
                    //    MainSequence = true,
                    //    ReverseSequence = false,
                    //    MonoMasses = monomasses,
                    //    SequenceTag = Convert.ToString(Gap),
                    //    DontShowall = Dontshowall,
                    //    Gap = Gap
                    //});

                    tempstart++;
                }
                else
                {
                    temporarystring = temporarystring + cmp.Aminoacid;
                    tempstart++;
                }
                tempnumberofaminoacids++; ///Increasing the Dictionary value
                if (tempnumberofaminoacids == numberofaminoacids) ///When the Dictionary value is same as the number of Amino Acids then break
                    break;
            }

            tempnumberofaminoacids = 0; //Resetting the value for Dictionary
            tempstart = start;
            ///Comparing the values in the reverse direction
            while (true)
            {
                if (removedkeys.Where(a => a == tempnumberofaminoacids).Any()) //If the AminoAcid is already found in the forward direction then don't look at the list.
                {
                    tempnumberofaminoacids++;
                    tempstart++;
                    if (tempnumberofaminoacids == reversenumberofaminoacids)
                        break;
                    continue;
                }
                var cmp = reversecmpaasdi[tempnumberofaminoacids];
                if (reversesingleaminoacids.Where(a => a.Sequence == cmp.Aminoacid && (Math.Abs(a.Start - cmp.startmonomass) < PPM.CurrentPPMbasedonMatchList(Math.Max(a.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM))).Any())
                {
                    indexofpairmatch++;
                    var smp = reversesingleaminoacids.Where(a => a.Sequence == cmp.Aminoacid && (Math.Abs(a.Start - cmp.startmonomass) < PPM.CurrentPPMbasedonMatchList(Math.Max(a.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM))).First();

                    List<double> monomasses = new List<double>();

                    removedkeys.Add(cmp.Index); //Removing the Key if found
                    cmpaasdi.Remove(cmp.Index);
                    monomasses.Add(cmp.startmonomass);
                    monomasses.Add(cmp.endmonomass);

                    mtse.Add(new MatchStartEnds()
                    {
                        confidence = AminAcidConfidence.Sure,
                        Start = tempstart,
                        End = tempstart + 1,
                        StartMonoMass = smp.Start,
                        EndMonoMass = smp.End,
                        Index = indexofpairmatch,
                        MainSequence = false,
                        ReverseSequence = true,
                        MonoMasses = monomasses,
                        SequenceTag = cmp.Aminoacid,
                        DontShowall = Dontshowall
                    });
                    tempstart++;
                }
                else if (reversesingleaminoacids.Where(a => a.Sequence == cmp.Aminoacid && Math.Abs((Math.Abs(a.Start - cmp.startmonomass) - PPM.CurrentPPMbasedonMatchList(Math.Max(a.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM))) < 100).Any())
                {
                    indexofpairmatch++;

                    var smp = reversesingleaminoacids.Where(a => a.Sequence == cmp.Aminoacid && Math.Abs((Math.Abs(a.Start - cmp.startmonomass) - PPM.CurrentPPMbasedonMatchList(Math.Max(a.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM))) < 100).First();
                    double Gap = Math.Abs(Math.Abs(smp.Start - cmp.startmonomass)); /// - PPM.CurrentPPMbasedonMatchList(Math.Min(smp.Start, cmp.startmonomass), Properties.Settings.Default.MatchTolerancePPM));

                    reversecmpaasdi.Where(a => a.Key != tempnumberofaminoacids && a.Value.start < cmp.start).ToDictionary(kp => kp.Key, kp => kp.Value.startmonomass = kp.Value.startmonomass - Gap);

                    removedkeys.Add(cmp.Index); //Removing the Key if found
                    cmpaasdi.Remove(cmp.Index);

                    List<double> monomasses = new List<double>();
                    monomasses.Add(cmp.startmonomass);
                    monomasses.Add(cmp.endmonomass);
                    mtse.Add(new MatchStartEnds()
                    {
                        confidence = AminAcidConfidence.Gap,
                        Start = tempstart,
                        End = tempstart + 1,
                        StartMonoMass = smp.Start,
                        EndMonoMass = cmp.endmonomass,
                        Index = indexofpairmatch,
                        MainSequence = false,
                        ReverseSequence = true,
                        MonoMasses = monomasses,
                        SequenceTag = cmp.Aminoacid,
                        DontShowall = Dontshowall,
                        Gap = Math.Round(Gap)
                    });

                    //mtse.Add(new MatchStartEnds()
                    //{
                    //    confidence = Confidence.Gap,
                    //    Start = tempstart,
                    //    End = tempstart + 1,
                    //    StartMonoMass = cmp.endmonomass - Gap,
                    //    EndMonoMass = cmp.endmonomass,
                    //    Index = indexofpairmatch,
                    //    MainSequence = false,
                    //    ReverseSequence = true,
                    //    MonoMasses = monomasses,
                    //    SequenceTag = Convert.ToString(Gap),
                    //    DontShowall = Dontshowall,
                    //    Gap = Gap
                    //});


                    tempstart++;
                }
                else
                {
                    tempstart++;
                }
                tempnumberofaminoacids++;
                if (tempnumberofaminoacids == reversenumberofaminoacids)
                    break;
            }

            tempnumberofaminoacids = cmpaasdi.First().Value.Index;
            string currentsequence = cmpaasdi.First().Value.Aminoacid;
            tempstartmonomass = cmpaasdi.First().Value.startmonomass;
            tempendmonomass = cmpaasdi.First().Value.endmonomass;
            //bool createanewcmpaas = false;

            //List<CompareAminoAcids> newcmpaasdi = new List<CompareAminoAcids>();
            Dictionary<int, CompareAminoAcids> newcmpaasdi = new Dictionary<int, CompareAminoAcids>();
            //int i = 2;
            int cmpaasdicount = cmpaasdi.Count;
            int currentindex = 0;
            string tempstring = string.Empty;
            int tstart = 0;
            int tend = 0;
            double tstartmonomass = 0;
            double tendmonomass = 0;

            List<int> keys = cmpaasdi.Select(a => a.Key).ToList();

            if (cmpaasdi.Count == 1)
            {
                newcmpaasdi.Add(currentindex, new CompareAminoAcids()
                {
                    Index = currentindex,
                    Aminoacid = cmpaasdi[0].Aminoacid,
                    start = cmpaasdi[0].start,
                    startmonomass = cmpaasdi[0].startmonomass
                });
            }
            else if (cmpaasdi.Any())
            {
                tstart = cmpaasdi.First().Value.start;
                tend = cmpaasdi.First().Value.end;
                tempstring = cmpaasdi.First().Value.Aminoacid;
                tstartmonomass = cmpaasdi.First().Value.startmonomass;
                tendmonomass = cmpaasdi.First().Value.endmonomass;

                //Filling in the gaps if there are any first by grouping the gaps
                foreach (var cmp in cmpaasdi)
                {
                    if (currentindex + 1 < cmpaasdi.Count && cmp.Value.end == cmpaasdi[keys[currentindex + 1]].start)
                    {
                        tempstring = tempstring + cmpaasdi[keys[currentindex + 1]].Aminoacid;
                        tend = cmpaasdi[keys[currentindex + 1]].end;
                        tendmonomass = cmpaasdi[keys[currentindex + 1]].endmonomass;
                    }
                    else
                    {
                        if (currentindex == 0)
                        {
                            newcmpaasdi.Add(currentindex, new CompareAminoAcids()
                            {
                                Index = currentindex,
                                Aminoacid = cmp.Value.Aminoacid,
                                start = cmp.Value.start,
                                startmonomass = cmp.Value.startmonomass
                            });
                        }
                        else if (cmp.Value.start == cmpaasdi[keys[currentindex - 1]].end)
                        {
                            newcmpaasdi.Add(currentindex, new CompareAminoAcids()
                            {
                                Index = currentindex,
                                Aminoacid = tempstring,
                                start = cmp.Value.end - tempstring.Length,
                                startmonomass = cmp.Value.endmonomass - sequencelength(tempstring)
                            });
                            if (currentindex + 1 < cmpaasdicount)
                            {
                                tempstring = cmpaasdi[keys[currentindex + 1]].Aminoacid;
                            }
                        }
                        else
                        {
                            newcmpaasdi.Add(currentindex, new CompareAminoAcids()
                            {
                                Index = currentindex,
                                Aminoacid = cmp.Value.Aminoacid,
                                start = cmp.Value.start,
                                startmonomass = cmp.Value.startmonomass
                            });
                            if (currentindex + 1 < cmpaasdicount)
                            {
                                tempstring = cmpaasdi[keys[currentindex + 1]].Aminoacid;
                            }
                            //tstartmonomass = cmp.Value.startmonomass;
                            //tstart = cmp.Value.start;
                            //tend = cmp.Value.end;
                            //tempstring = cmp.Value.Aminoacid;
                        }
                    }
                    currentindex++;
                }
            }


            //Creating new MatchStartEnds 
            foreach (var cmp in newcmpaasdi)
            {
                ///Checking if there are any sequences before the current sequence
                if (mtse.Where(a => a.End == cmp.Value.start).Any())
                {
                    var firstmatch = mtse.Where(a => a.End == cmp.Value.start).First();
                    //If the match mtse is in maindirection
                    if (firstmatch.MainSequence)
                    {
                        if (((firstmatch.EndMonoMass - cmp.Value.startmonomass) <= PPM.CurrentPPMbasedonMatchList(Math.Max(firstmatch.EndMonoMass, cmp.Value.startmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                confidence = cmp.Value.Aminoacid.Length == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                Start = cmp.Value.start,
                                End = cmp.Value.end,
                                StartMonoMass = cmp.Value.startmonomass,
                                EndMonoMass = cmp.Value.endmonomass,
                                SequenceTag = cmp.Value.Aminoacid,
                                Index = indexofpairmatch,
                                DontShowall = Dontshowall,
                                MainSequence = true,
                                ReverseSequence = false
                            });
                        }
                        else if (!((firstmatch.EndMonoMass - cmp.Value.startmonomass) <= PPM.CurrentPPMbasedonMatchList(Math.Max(firstmatch.EndMonoMass, cmp.Value.startmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                confidence = AminAcidConfidence.Gap,
                                Start = cmp.Value.start,
                                End = cmp.Value.end,
                                StartMonoMass = cmp.Value.startmonomass,
                                EndMonoMass = firstmatch.EndMonoMass + (cmp.Value.startmonomass - firstmatch.EndMonoMass),
                                Gap = (cmp.Value.startmonomass - firstmatch.EndMonoMass),
                                SequenceTag = cmp.Value.Aminoacid, /// Convert.ToString(cmp.Value.startmonomass - firstmatch.EndMonoMass),
                                Index = indexofpairmatch,
                                DontShowall = Dontshowall,
                                MainSequence = true,
                                ReverseSequence = false
                            });
                        }
                    }
                    //If the match mtse is not in the main direction then we need to subtract the parent mass
                    else
                    {
                        if ((((ParentMass - firstmatch.StartMonoMass) - cmp.Value.startmonomass) <= PPM.CurrentPPMbasedonMatchList(Math.Max((ParentMass - firstmatch.StartMonoMass), cmp.Value.startmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                confidence = cmp.Value.Aminoacid.Length == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                Start = cmp.Value.start,
                                End = cmp.Value.end,
                                StartMonoMass = cmp.Value.startmonomass,
                                EndMonoMass = cmp.Value.endmonomass,
                                SequenceTag = cmp.Value.Aminoacid,
                                Index = indexofpairmatch,
                                DontShowall = Dontshowall,
                                MainSequence = true,
                                ReverseSequence = false
                            });
                        }
                        ///If the gap between the two sequences is not within the tolerance.
                        else if (!(((ParentMass - firstmatch.StartMonoMass) - cmp.Value.startmonomass) <= PPM.CurrentPPMbasedonMatchList(Math.Max((ParentMass - firstmatch.StartMonoMass), cmp.Value.startmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                confidence = AminAcidConfidence.Gap,
                                Start = cmp.Value.start,
                                End = cmp.Value.end,
                                StartMonoMass = cmp.Value.startmonomass, /// ParentMass - firstmatch.StartMonoMass,
                                EndMonoMass = (ParentMass - firstmatch.StartMonoMass) + (cmp.Value.startmonomass - (ParentMass - firstmatch.StartMonoMass)),
                                Gap = (cmp.Value.startmonomass - (ParentMass - firstmatch.StartMonoMass)),
                                SequenceTag = cmp.Value.Aminoacid, /// Convert.ToString(cmp.Value.startmonomass - (ParentMass - firstmatch.StartMonoMass)),
                                Index = indexofpairmatch,
                                DontShowall = Dontshowall,
                                MainSequence = true,
                                ReverseSequence = false
                            });
                        }
                    }
                }
                ///If there are no sequences before checking if there are any sequences after the current sequence
                else if (mtse.Where(a => a.Start == cmp.Value.end).Any())
                {
                    var firstmatch = mtse.Where(a => a.Start == cmp.Value.end).First();
                    //If the match mtse is in maindirection
                    if (firstmatch.MainSequence)
                    {
                        if (((cmp.Value.endmonomass - firstmatch.StartMonoMass) <= PPM.CurrentPPMbasedonMatchList(Math.Max(firstmatch.StartMonoMass, cmp.Value.endmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                confidence = cmp.Value.Aminoacid.Length == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                Start = cmp.Value.start,
                                End = cmp.Value.end,
                                StartMonoMass = cmp.Value.startmonomass,
                                EndMonoMass = cmp.Value.endmonomass,
                                SequenceTag = cmp.Value.Aminoacid,
                                Index = indexofpairmatch,
                                DontShowall = Dontshowall,
                                MainSequence = true,
                                ReverseSequence = false
                            });
                        }
                        else if (!((cmp.Value.endmonomass - firstmatch.StartMonoMass) <= PPM.CurrentPPMbasedonMatchList(Math.Max(firstmatch.StartMonoMass, cmp.Value.endmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                confidence = AminAcidConfidence.Gap,
                                Start = cmp.Value.start,
                                End = cmp.Value.end,
                                StartMonoMass = cmp.Value.startmonomass,
                                EndMonoMass = cmp.Value.endmonomass + (firstmatch.StartMonoMass - cmp.Value.endmonomass), /// - firstmatch.EndMonoMass),
                                Gap = (firstmatch.StartMonoMass - cmp.Value.endmonomass),
                                SequenceTag = cmp.Value.Aminoacid, /// + Convert.ToString(firstmatch.StartMonoMass - cmp.Value.endmonomass),
                                Index = indexofpairmatch,
                                DontShowall = Dontshowall,
                                MainSequence = true,
                                ReverseSequence = false
                            });
                        }
                    }
                    //If the match mtse is not in the main direction then we need to subtract the parent mass
                    else
                    {
                        if (((cmp.Value.endmonomass - (ParentMass - firstmatch.EndMonoMass)) <= PPM.CurrentPPMbasedonMatchList(Math.Max((ParentMass - firstmatch.EndMonoMass), cmp.Value.endmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                confidence = cmp.Value.Aminoacid.Length == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low,
                                Start = cmp.Value.start,
                                End = cmp.Value.end,
                                StartMonoMass = cmp.Value.startmonomass,
                                EndMonoMass = cmp.Value.endmonomass,
                                SequenceTag = cmp.Value.Aminoacid,
                                Index = indexofpairmatch,
                                DontShowall = Dontshowall,
                                MainSequence = true,
                                ReverseSequence = false
                            });
                        }
                        else if (!((cmp.Value.endmonomass - (ParentMass - firstmatch.EndMonoMass)) <= PPM.CurrentPPMbasedonMatchList(Math.Max((ParentMass - firstmatch.EndMonoMass), cmp.Value.endmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                confidence = AminAcidConfidence.Gap,
                                Start = cmp.Value.start,
                                End = cmp.Value.end,
                                StartMonoMass = cmp.Value.startmonomass,
                                EndMonoMass = cmp.Value.endmonomass + (cmp.Value.endmonomass - (ParentMass - firstmatch.EndMonoMass)),
                                Gap = (cmp.Value.endmonomass - (ParentMass - firstmatch.EndMonoMass)),
                                SequenceTag = cmp.Value.Aminoacid, /// Convert.ToString(cmp.Value.endmonomass - (ParentMass - firstmatch.EndMonoMass)),
                                Index = indexofpairmatch,
                                DontShowall = Dontshowall,
                                MainSequence = true,
                                ReverseSequence = false
                            });
                        }
                    }
                }
                //if (mtse.Where(a => (a.StartMonoMass - cmp.Value.endmonomass) < PPM.CurrentPPMbasedonMatchList(Math.Max(a.StartMonoMass, cmp.Value.endmonomass), Properties.Settings.Default.MatchTolerancePPM)).Any() && mtse.Where(a => (a.EndMonoMass - cmp.Value.endmonomass) < PPM.CurrentPPMbasedonMatchList(Math.Max(a.StartMonoMass, cmp.Value.endmonomass), Properties.Settings.Default.MatchTolerancePPM)).Any())
                //{
                //    var firstmatch = mtse.Where(a => (a.StartMonoMass - cmp.Value.endmonomass) < PPM.CurrentPPMbasedonMatchList(Math.Max(a.StartMonoMass, cmp.Value.endmonomass), Properties.Settings.Default.MatchTolerancePPM)).First();
                //    mtse.Add(new MatchStartEnds()
                //    {
                //    });
                //}
            }

            //if (cmpaasdi.Count > 1)
            //{
            //    foreach (var cmp in cmpaasdi.Skip(1))
            //    {
            //        if (tempnumberofaminoacids + 1 == cmp.Value.Index)
            //        {
            //            createanewcmpaas = true;
            //            tempnumberofaminoacids = cmp.Value.Index;
            //            currentsequence += cmp.Value.Aminoacid;/// +currentsequence;
            //            tempendmonomass = cmp.Value.endmonomass;

            //            if (i == cmpaasdicount)
            //            {
            //                newcmpaasdi.Add(i + 1, new CompareAminoAcids()
            //                {
            //                    Index = i,
            //                    Aminoacid = currentsequence,
            //                    startmonomass = cmp.Value.startmonomass,
            //                    start = i
            //                });
            //            }
            //        }
            //        else if (createanewcmpaas)
            //        {
            //            createanewcmpaas = false;
            //            tempnumberofaminoacids = cmp.Value.Index;
            //            newcmpaasdi.Add(i, new CompareAminoAcids()
            //            {
            //                Index = i,
            //                Aminoacid = currentsequence,
            //                startmonomass = tempstartmonomass,
            //                start = i
            //            });
            //            tempstartmonomass = cmp.Value.startmonomass;
            //            currentsequence = cmp.Value.Aminoacid;

            //            if (i == cmpaasdicount)
            //            {
            //                newcmpaasdi.Add(i + 1, new CompareAminoAcids()
            //                {
            //                    Index = i,
            //                    Aminoacid = cmp.Value.Aminoacid,
            //                    startmonomass = cmp.Value.startmonomass,
            //                    start = i
            //                });
            //            }
            //        }
            //        else
            //        {
            //            newcmpaasdi.Add(i, new CompareAminoAcids()
            //            {
            //                Index = i,
            //                Aminoacid = cmp.Value.Aminoacid,
            //                startmonomass = cmp.Value.startmonomass,
            //                start = i
            //            });
            //        }
            //        i++;
            //        //if (!(allmonomasses.Where(a => Math.Abs(a - cmp.Value.startmonomass) < PPM.CurrentPPMbasedonMatchList(cmp.Value.startmonomass, Properties.Settings.Default.MatchTolerancePPM)).Any()))
            //        //{

            //        //    mtse.Add(new MatchStartEnds()
            //        //    {
            //        //        confidence = Confidence.Gap,

            //        //    });
            //        //}
            //    }
            //    foreach (var cmpaasd in newcmpaasdi)
            //    {

            //    }
            //}
            //if (cmpaasdi.Count == 1)
            //{

            //}

            mtse = mtse.OrderBy(a => a.Start).ThenBy(a => a.StartMonoMass).ToList();


            if (mtse.Where(a => a.Start == start).Any())
            {
                double tempsstartmonomass = mtse.Where(a => a.Start == start).First().StartMonoMass;
                if (!(Math.Abs(tempsstartmonomass - startmonomass) <= PPM.CurrentPPMbasedonMatchList(Math.Max(startmonomass, tempsstartmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                {
                    int startindex = mtse.FindIndex(a => a.Start == start);
                    mtse[startindex].confidence = AminAcidConfidence.Gap;
                    mtse[startindex].StartMonoMass = startmonomass;
                    mtse[startindex].Gap = startmonomass - tempsstartmonomass; ///mtse.Where(a => a.Start == start).First().StartMonoMass;
                }
            }

            if (mtse.Where(a => a.End == end).Any())
            {
                double tempsendmonomass = mtse.Where(a => a.End == end).First().EndMonoMass;
                if (!(Math.Abs(tempsendmonomass - endmonomass) <= PPM.CurrentPPMbasedonMatchList(Math.Max(endmonomass, tempsendmonomass), Properties.Settings.Default.MatchTolerancePPM)))
                {
                    int endindex = mtse.FindIndex(a => a.End == end);
                    mtse[endindex].confidence = AminAcidConfidence.Gap;
                    mtse[endindex].EndMonoMass = endmonomass;
                    mtse[endindex].Gap = endmonomass - tempsendmonomass; ///mtse.Where(a => a.End == end).First().EndMonoMass;
                }
            }

            if (!direction)
            {
                List<MatchStartEnds> rvrmtse = new List<MatchStartEnds>();
                mtse.Reverse();
                foreach (var m in mtse)
                {
                    rvrmtse.Add(new MatchStartEnds()
                    {
                        Start = start,
                        breakornot = m.breakornot,
                        confidence = m.confidence,
                        DontShowall = m.DontShowall,
                        End = start + m.SequenceTag.Length,
                        EndMonoMass = m.EndMonoMass,
                        SequenceTag = ReverseString.Reverse(m.SequenceTag),
                        StartMonoMass = m.StartMonoMass,
                        Gap = m.Gap,
                        Index = m.Index,
                        MainSequence = m.MainSequence,
                        MonoMasses = m.MonoMasses,
                        Numberofextensions = m.Numberofextensions,
                        RawSequenceTag = m.RawSequenceTag,
                        ReverseSequence = m.ReverseSequence,
                        ReverseSpectrum = m.ReverseSpectrum
                    });
                    start = start + m.SequenceTag.Length;
                }
                mtse.Clear();
                mtse.AddRange(rvrmtse);
                //var rvrmtse = (from m in mtse
                //               select new MatchStartEnds()
                //               {
                //                    End = tempend - m.End,

                //               }).ToList();
            }

            //mtse = mtse.OrderBy(a => a.Start).ThenBy(a => a.StartMonoMass).ToList();

            return mtse;
        }

        class CompareAminoAcids
        {
            public double startmonomass { get; set; }

            public string Aminoacid { get; set; }

            public int Index { get; set; }

            public int start { get; set; }

            public double endmonomass
            {
                get
                {
                    return startmonomass + sequencelength(Aminoacid);
                }
            }

            public int end
            {
                get
                {
                    return start + Aminoacid.Length;
                }
            }
        }

        /// <summary>
        /// Finding the extensions at the edges of the sequence.
        /// If there are any they are found and also the distance between
        /// the currently found and the parent mass or zero mass is found.
        /// </summary>
        /// <param name="matchstartends"></param>
        /// <param name="ParentMass"></param>
        /// <param name="length"></param>
        /// <param name="ModifiedSequenceForL"></param>
        /// <returns></returns>
        static List<MatchStartEnds> FindEdges(List<MatchStartEnds> matchstartends, double ParentMass, int length, string ModifiedSequenceForL, ref List<double> allthemonomasses, bool Dontshowall = false)
        {
            List<MatchStartEnds> mtse = new List<MatchStartEnds>();

            if (!matchstartends.Any())
            {
                mtse.Add(new MatchStartEnds()
                {
                    Start = 0,
                    End = ModifiedSequenceForL.Length,
                    confidence = AminAcidConfidence.NotPossible,
                    DontShowall = Dontshowall
                });
                return mtse;
            }

            //asdhjkas; hasl; acsjkdasll;

            MatchStartEnds firstmtse = new MatchStartEnds();
            matchstartends = matchstartends.Where(a => a.SequenceTag != null).OrderBy(a => a.End).ThenBy(a => a.Length).ToList();
            firstmtse = matchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).OrderByDescending(a => a.Length).First();
            int stfrstmtse = firstmtse.Start;
            int edfrstmtse = firstmtse.End;
            double startgap = matchstartends.Where(a => a.Start <= firstmtse.Start).Select(a => a.Gap).Sum();
            double endgap = matchstartends.Where(a => a.End >= firstmtse.End).Select(a => a.Gap).Sum();
            bool direction = firstmtse.MainSequence;
            allthemonomasses.Add(Molecules.Water);
            int startforsequences = matchstartends.Select(a => a.Start).Min(); ///Getting the min of all the matchstartends for the start position
            int endforsequences = matchstartends.Select(a => a.End).Max(); ///Getting the max of all the matchstartends for the end position


            ///If its the forward direction
            if (direction)
            {
                double startfirstmtse = firstmtse.StartMonoMass - sequencelength(ModifiedSequenceForL.Substring(startforsequences, firstmtse.Start - startforsequences)) - startgap;
                double endfirstmtse = firstmtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(firstmtse.End, endforsequences - firstmtse.End)) + endgap;
                if (endfirstmtse > ParentMass || startfirstmtse < 0)
                {
                    startfirstmtse = ParentMass - (firstmtse.EndMonoMass - sequencelength(ModifiedSequenceForL.Substring(startforsequences, firstmtse.Start - startforsequences)) - startgap);
                    endfirstmtse = ParentMass - firstmtse.StartMonoMass + (sequencelength(ModifiedSequenceForL.Substring(firstmtse.End, endforsequences - firstmtse.End)) + endgap);
                }
                double tempendfirstmtse = endfirstmtse;
                double tempstartfirstmtse = startfirstmtse;
                int strt = startforsequences;
                int enddd = endforsequences;
                string tempstring = string.Empty;

                MatchStartEnds lasstmttse = new MatchStartEnds();
                lasstmttse = matchstartends.Last();

                foreach (char c in Enumerable.Reverse(ModifiedSequenceForL.Substring(0, startforsequences)))
                {
                    startfirstmtse = startfirstmtse - sequencelength(Convert.ToString(c));
                    strt--;
                    if (startfirstmtse <= 0)
                    {
                        if (startforsequences == strt - 1)
                        {
                            double newstartfirstmtssee = startfirstmtse + sequencelength(Convert.ToString(c));

                            if (newstartfirstmtssee < 0.1)
                                break;

                            mtse.Add(new MatchStartEnds()
                            {
                                StartMonoMass = 0,
                                EndMonoMass = newstartfirstmtssee,
                                Start = strt + 1,
                                End = strt + 1,
                                confidence = AminAcidConfidence.Gap,
                                Gap = Math.Round(newstartfirstmtssee, 3),
                                SequenceTag = string.Empty,
                                MainSequence = true,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = 0,
                                NumberofYellowPeaks = 0
                            });
                            break;
                        }
                        if (Math.Round(startfirstmtse + sequencelength(Convert.ToString(c)), 4) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Round(startfirstmtse + sequencelength(Convert.ToString(c)), 4), Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add(Math.Round(startfirstmtse + sequencelength(Convert.ToString(c)), 4));
                            mtse.Add(new MatchStartEnds()
                            {
                                StartMonoMass = 0,
                                EndMonoMass = tempstartfirstmtse,
                                Start = strt + 1,
                                End = startforsequences,
                                confidence = tempstring.Length == 1 ? AminAcidConfidence.High : AminAcidConfidence.Low,
                                Gap = 0,
                                SequenceTag = tempstring,
                                MainSequence = true,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        mtse.Add(new MatchStartEnds()
                        {
                            StartMonoMass = 0,
                            EndMonoMass = tempstartfirstmtse,
                            Start = strt + 1,
                            End = startforsequences,
                            confidence = AminAcidConfidence.NotSure,
                            Gap = Math.Round(startfirstmtse + sequencelength(Convert.ToString(c)), 4),
                            SequenceTag = tempstring,
                            MainSequence = true,
                            DontShowall = Dontshowall
                        });
                        break;
                    }
                    if (strt == 0)
                    {
                        tempstring = Convert.ToString(c) + tempstring;
                        if (startfirstmtse <= PPMCalc.CurrentPPMbasedonMatchList(startfirstmtse, Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add(Math.Round(startfirstmtse + sequencelength(Convert.ToString(c)), 4));
                            mtse.Add(new MatchStartEnds()
                            {
                                StartMonoMass = 0,
                                EndMonoMass = tempstartfirstmtse,
                                Start = strt,
                                End = startforsequences,
                                confidence = AminAcidConfidence.Low,
                                Gap = (startfirstmtse > 0 ? (Math.Round(startfirstmtse, 1)) : Math.Round(startfirstmtse + sequencelength(Convert.ToString(c)), 4)),
                                SequenceTag = tempstring,
                                MainSequence = true,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        mtse.Add(new MatchStartEnds()
                        {
                            StartMonoMass = 0,
                            EndMonoMass = tempstartfirstmtse,
                            Start = strt,
                            End = startforsequences,
                            confidence = AminAcidConfidence.NotSure,
                            Gap = (startfirstmtse > 0 ? (Math.Round(startfirstmtse, 1)) : Math.Round(startfirstmtse + sequencelength(Convert.ToString(c)), 4)),
                            SequenceTag = tempstring,
                            MainSequence = true,
                            DontShowall = Dontshowall
                        });
                        break;
                    }
                    tempstring = Convert.ToString(c) + tempstring;
                }
                tempstring = string.Empty;
                foreach (char c in ModifiedSequenceForL.Substring(endforsequences, length - endforsequences))
                {
                    endfirstmtse = endfirstmtse + sequencelength(Convert.ToString(c));
                    enddd++;
                    if (endfirstmtse >= ParentMass)
                    {
                        if (enddd - 1 == endforsequences)
                        {
                            double newendfirstmtse = endfirstmtse - sequencelength(Convert.ToString(c));

                            if (ParentMass - newendfirstmtse < 0.1)
                                break;

                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = Math.Round(ParentMass - Molecules.Water, 3),
                                StartMonoMass = newendfirstmtse, ///endfirstmtse - sequencelength(Convert.ToString(c)),
                                Start = endforsequences,
                                End = enddd - 1,
                                confidence = AminAcidConfidence.Gap,
                                Gap = Math.Round(ParentMass - newendfirstmtse - Molecules.Water, 3),
                                SequenceTag = tempstring,
                                MainSequence = true,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = 0,
                                NumberofYellowPeaks = 0
                            });
                            break;
                        }
                        if (tempstring == "")
                            tempstring = Convert.ToString(c);
                        if (Math.Abs(Math.Round(ParentMass - (endfirstmtse - sequencelength(Convert.ToString(c))), 4)) <= PPMCalc.CurrentPPMbasedonMatchList((Math.Round(ParentMass - (endfirstmtse - sequencelength(Convert.ToString(c))), 4)), Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add((Math.Round((endfirstmtse - sequencelength(Convert.ToString(c))), 4)));
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = ParentMass,
                                StartMonoMass = tempendfirstmtse,
                                Start = endforsequences,
                                End = enddd - 2,
                                confidence = AminAcidConfidence.NotSure,
                                Gap = sequencelength(tempstring.Last().ToString()),
                                SequenceTag = tempstring.Substring(0, tempstring.Length - 1),
                                MainSequence = true,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1,
                            });
                            break;
                        }
                        else if (Math.Abs(Math.Round(ParentMass - (endfirstmtse - sequencelength(Convert.ToString(c))), 4) - Molecules.Water) <= 0.2)
                        {
                            allthemonomasses.Add((Math.Round((endfirstmtse - sequencelength(Convert.ToString(c))), 4)));
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = ParentMass - Molecules.Water,
                                StartMonoMass = tempendfirstmtse,
                                Start = endforsequences,
                                End = enddd - 1,
                                confidence = AminAcidConfidence.Low,
                                Gap = 0,
                                SequenceTag = tempstring,
                                MainSequence = true,
                                DontShowall = Dontshowall,
                                CTerminus = ModifiedSequenceForL.Contains(tempstring) ? true : false,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        mtse.Add(new MatchStartEnds()
                        {
                            EndMonoMass = ParentMass,
                            StartMonoMass = tempendfirstmtse,
                            Start = endforsequences,
                            End = enddd - 1,
                            confidence = AminAcidConfidence.NotSure,
                            Gap = (Math.Round(ParentMass - (endfirstmtse - sequencelength(Convert.ToString(c))), 4)),
                            SequenceTag = tempstring,
                            MainSequence = true,
                            DontShowall = Dontshowall
                        });
                        break;
                    }
                    if (enddd == length)
                    {
                        tempstring = tempstring + Convert.ToString(c);
                        if (Math.Abs(endfirstmtse - ParentMass) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Max(endfirstmtse, ParentMass), Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add(endfirstmtse);
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = ParentMass,
                                StartMonoMass = tempendfirstmtse,
                                Start = endforsequences,
                                End = enddd - 1,
                                confidence = AminAcidConfidence.NotSure,
                                Gap = sequencelength(tempstring.Last().ToString()) - Molecules.Water,
                                SequenceTag = tempstring.Substring(0, tempstring.Length - 1),
                                MainSequence = true,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        else if (Math.Abs(Math.Abs(endfirstmtse - ParentMass) - Molecules.Water) <= 0.2)
                        {
                            allthemonomasses.Add(endfirstmtse);
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = ParentMass - Molecules.Water,
                                StartMonoMass = tempendfirstmtse,
                                Start = endforsequences,
                                End = enddd,
                                confidence = AminAcidConfidence.Low,
                                Gap = 0,
                                SequenceTag = tempstring,
                                MainSequence = true,
                                DontShowall = Dontshowall,
                                CTerminus = ModifiedSequenceForL.Contains(tempstring) ? true : false,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        mtse.Add(new MatchStartEnds()
                        {
                            EndMonoMass = ParentMass,
                            StartMonoMass = tempendfirstmtse,
                            Start = endforsequences,
                            End = enddd,
                            confidence = AminAcidConfidence.NotSure,
                            Gap = ParentMass - endfirstmtse,
                            SequenceTag = tempstring,
                            MainSequence = true,
                            DontShowall = Dontshowall
                        });
                        break;
                    }
                    tempstring = tempstring + Convert.ToString(c);
                }
            }
            else if (!direction && firstmtse.End < ModifiedSequenceForL.Length)
            {
                //double endfirstmtse = ParentMass - firstmtse.StartMonoMass;/// -sequencelength(ModifiedSequenceForL.Substring(firstmtse.End, endforsequences - firstmtse.End)) - endgap;

                //double newendgap = firstmtse.StartMonoMass - sequencelength(ModifiedSequenceForL.Substring(firstmtse.End, ModifiedSequenceForL.Length - firstmtse.End)) - endgap;
                double endfirstmtse = 0;
                double startfirstmtse = 0;

                //double newstartgap = firstmtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(startforsequences, ModifiedSequenceForL.Length - startforsequences)) + startgap;
                //if (newendgap > 0)
                //{
                //    endfirstmtse = firstmtse.StartMonoMass - sequencelength(ModifiedSequenceForL.Substring(firstmtse.End, endforsequences - firstmtse.End)) - endgap + newendgap;
                //}
                //else
                //{
                endfirstmtse = firstmtse.StartMonoMass - sequencelength(ModifiedSequenceForL.Substring(firstmtse.End, endforsequences - firstmtse.End)) - endgap;
                //}
                //if (newstartgap)
                startfirstmtse = firstmtse.EndMonoMass + sequencelength(ModifiedSequenceForL.Substring(startforsequences, firstmtse.Start - startforsequences)) + startgap;
                double tempvalue = startfirstmtse;
                double tempstartvalue = endfirstmtse;
                int strt = startforsequences;
                int enddd = endforsequences;
                string tempstring = string.Empty;
                if (startforsequences == 0)
                {
                    if (ParentMass - startfirstmtse > 0.1)
                    {
                        mtse.Add(new MatchStartEnds()
                        {
                            EndMonoMass = ParentMass,
                            StartMonoMass = startfirstmtse,
                            Start = 0,
                            End = 0,
                            confidence = AminAcidConfidence.Gap,
                            Gap = Math.Round(ParentMass - startfirstmtse, 3),
                            SequenceTag = String.Empty,
                            MainSequence = false,
                            DontShowall = Dontshowall,
                            NumberofYellowPeaks = 0,
                            NumberofGreenPeaks = 0
                        });
                    }
                }
                foreach (char c in Enumerable.Reverse(ModifiedSequenceForL.Substring(0, startforsequences)))
                {
                    strt--;
                    startfirstmtse = startfirstmtse + sequencelength(Convert.ToString(c));
                    if (startfirstmtse >= ParentMass)
                    {
                        if (strt + 1 == startforsequences)
                        {
                            double newstartmonomass = startfirstmtse - sequencelength(Convert.ToString(c));

                            if (ParentMass - newstartmonomass < 0.1)
                                break;

                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = ParentMass,
                                StartMonoMass = newstartmonomass,
                                Start = strt + 1,
                                End = startforsequences,
                                confidence = AminAcidConfidence.Gap,
                                Gap = Math.Round(ParentMass - newstartmonomass, 3),
                                SequenceTag = string.Empty,
                                MainSequence = false,
                                DontShowall = Dontshowall,
                                NumberofYellowPeaks = 0,
                                NumberofGreenPeaks = 0
                            });

                            break;
                        }
                        if (Math.Abs(Math.Round(ParentMass - (startfirstmtse - sequencelength(Convert.ToString(c))), 4)) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Round(ParentMass - (startfirstmtse - sequencelength(Convert.ToString(c))), 4), Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = ParentMass,
                                StartMonoMass = tempvalue,
                                Start = strt + 1,
                                End = startforsequences,
                                confidence = AminAcidConfidence.Low,
                                Gap = 0,
                                SequenceTag = tempstring,
                                MainSequence = false,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        //else if (Math.Abs((Math.Round(ParentMass - (startfirstmtse - sequencelength(Convert.ToString(c))), 4)) - Molecules.Water) <= 0.2)
                        //{
                        //    //mtse.Add(new MatchStartEnds()
                        //    //{
                        //    //    EndMonoMass = ParentMass - Molecules.Water,
                        //    //    StartMonoMass = tempvalue,
                        //    //    Start = strt + 1,
                        //    //    End = startforsequences,
                        //    //    confidence = Confidence.Low,
                        //    //    Gap = 0,
                        //    //    SequenceTag = tempstring,
                        //    //    MainSequence = false,
                        //    //    DontShowall = Dontshowall,
                        //    //    CTerminus = ModifiedSequenceForL.Contains(tempstring) ? true : false,
                        //    //    NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                        //    //    NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                        //    //});

                        //    mtse.Add(new MatchStartEnds()
                        //    {
                        //        EndMonoMass = ParentMass - Molecules.Water,
                        //        StartMonoMass = tempvalue,
                        //        Start = strt + 1,
                        //        End = startforsequences,
                        //        confidence = Confidence.NotSure,
                        //        Gap = Molecules.Water,
                        //        SequenceTag = tempstring,
                        //        MainSequence = false,
                        //        DontShowall = Dontshowall,
                        //        CTerminus = ModifiedSequenceForL.Contains(tempstring) ? true : false,
                        //        NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                        //        NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                        //    });

                        //    break;
                        //}
                        mtse.Add(new MatchStartEnds()
                        {
                            EndMonoMass = ParentMass,
                            StartMonoMass = tempvalue,
                            Start = strt + 1,
                            End = startforsequences,
                            confidence = AminAcidConfidence.NotSure,
                            Gap = (Math.Round(ParentMass - (startfirstmtse - sequencelength(Convert.ToString(c))), 4)),
                            SequenceTag = tempstring,
                            MainSequence = false,
                            DontShowall = Dontshowall
                        });
                        break;
                    }
                    if (strt == 0)
                    {
                        tempstring = tempstring + Convert.ToString(c);
                        if (Math.Abs(ParentMass - startfirstmtse) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Max(ParentMass, startfirstmtse), Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add(startfirstmtse);
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = ParentMass,
                                StartMonoMass = tempvalue,
                                Start = strt,
                                End = startforsequences,
                                confidence = AminAcidConfidence.Low,
                                Gap = 0,
                                SequenceTag = tempstring,
                                MainSequence = false,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        mtse.Add(new MatchStartEnds()
                        {
                            EndMonoMass = ParentMass,
                            StartMonoMass = tempvalue,
                            Start = strt,
                            End = startforsequences,
                            confidence = AminAcidConfidence.NotSure,
                            Gap = ParentMass - startfirstmtse,
                            SequenceTag = tempstring,
                            MainSequence = false,
                            DontShowall = Dontshowall
                        });
                        break;
                    }
                    tempstring = tempstring + Convert.ToString(c);
                }
                tempstring = string.Empty;
                if (endforsequences == length)
                {
                    if (endfirstmtse - Molecules.Water > 0.1)
                    {
                        mtse.Add(new MatchStartEnds()
                        {
                            EndMonoMass = Math.Round((endfirstmtse - Molecules.Water), 3),
                            StartMonoMass = Molecules.Water,
                            Start = endforsequences,
                            End = enddd - 1,
                            confidence = AminAcidConfidence.Gap,
                            Gap = Math.Round((endfirstmtse - Molecules.Water), 3),
                            SequenceTag = String.Empty,
                            MainSequence = false,
                            DontShowall = Dontshowall,
                            NumberofGreenPeaks = 0,
                            NumberofYellowPeaks = 0
                        });
                    }
                }
                foreach (char c in ModifiedSequenceForL.Substring(endforsequences, length - endforsequences))
                {
                    enddd++;
                    endfirstmtse = endfirstmtse - sequencelength(Convert.ToString(c));
                    if (endfirstmtse <= 0)
                    {
                        if (enddd - 1 == endforsequences)
                        {
                            if (Math.Round(((endfirstmtse + sequencelength(Convert.ToString(c))) - Molecules.Water), 3) < 0.1)
                                break;
                            mtse.Add(new MatchStartEnds()
                                {
                                    EndMonoMass = Math.Round(((endfirstmtse + sequencelength(Convert.ToString(c))) - Molecules.Water), 3),
                                    StartMonoMass = Molecules.Water,
                                    Start = endforsequences,
                                    End = enddd - 1,
                                    confidence = AminAcidConfidence.Gap,
                                    Gap = Math.Round(((endfirstmtse + sequencelength(Convert.ToString(c))) - Molecules.Water), 3),
                                    SequenceTag = String.Empty,
                                    MainSequence = false,
                                    DontShowall = Dontshowall,
                                    NumberofGreenPeaks = 0,
                                    NumberofYellowPeaks = 0
                                });
                            break;
                        }

                        if (Math.Round(endfirstmtse + sequencelength(Convert.ToString(c)), 4) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Round(endfirstmtse + sequencelength(Convert.ToString(c)), 4), Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add(Math.Round(endfirstmtse + sequencelength(Convert.ToString(c)), 4));
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = tempstartvalue,
                                StartMonoMass = 0,
                                Start = endforsequences,
                                End = enddd - 2,
                                confidence = AminAcidConfidence.NotSure,
                                Gap = sequencelength(tempstring.First().ToString()) - Molecules.Water,
                                SequenceTag = tempstring.Substring(1, tempstring.Length - 1),
                                MainSequence = false,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        else if (Math.Abs(Math.Round(endfirstmtse + sequencelength(Convert.ToString(c)), 4) - Molecules.Water) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Max(Math.Round(endfirstmtse + sequencelength(Convert.ToString(c)), 4), Molecules.Water), Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add(Math.Abs(Math.Round(endfirstmtse + sequencelength(Convert.ToString(c)), 4) - Molecules.Water));
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = tempstartvalue,
                                StartMonoMass = Molecules.Water,
                                Start = endforsequences,
                                End = enddd - 1,
                                confidence = AminAcidConfidence.Low,
                                Gap = 0,
                                SequenceTag = tempstring,
                                MainSequence = false,
                                DontShowall = Dontshowall,
                                CTerminus = ModifiedSequenceForL.Contains(tempstring) ? true : false,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        mtse.Add(new MatchStartEnds()
                        {
                            EndMonoMass = tempstartvalue,
                            StartMonoMass = 0,
                            Start = endforsequences,
                            End = enddd - 1,
                            confidence = AminAcidConfidence.NotSure,
                            Gap = Math.Round(endfirstmtse + sequencelength(Convert.ToString(c)), 4),
                            SequenceTag = tempstring,
                            MainSequence = false,
                            DontShowall = Dontshowall
                        });
                        break;
                    }
                    if (enddd == length)
                    {
                        tempstring = Convert.ToString(c) + tempstring;
                        if (Math.Abs(endfirstmtse) <= PPMCalc.CurrentPPMbasedonMatchList(endfirstmtse, Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add(endfirstmtse);
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = tempstartvalue,
                                StartMonoMass = endfirstmtse,
                                Start = endforsequences,
                                End = enddd,
                                confidence = AminAcidConfidence.Low,
                                Gap = 0,
                                SequenceTag = tempstring,
                                MainSequence = false,
                                DontShowall = Dontshowall,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        else if (Math.Abs(Math.Abs(endfirstmtse) - Molecules.Water) <= PPMCalc.CurrentPPMbasedonMatchList(Math.Max(endfirstmtse, Molecules.Water), Properties.Settings.Default.MatchTolerancePPM) + Molecules.tolerance)
                        {
                            allthemonomasses.Add(endfirstmtse);
                            mtse.Add(new MatchStartEnds()
                            {
                                EndMonoMass = tempstartvalue,
                                StartMonoMass = endfirstmtse,
                                Start = endforsequences,
                                End = enddd,
                                confidence = AminAcidConfidence.Low,
                                Gap = 0,
                                SequenceTag = tempstring,
                                MainSequence = false,
                                DontShowall = Dontshowall,
                                CTerminus = ModifiedSequenceForL.Contains(tempstring) ? true : false,
                                NumberofGreenPeaks = tempstring.Length == 1 ? 2 : 0,
                                NumberofYellowPeaks = tempstring.Length == 1 ? 0 : 1
                            });
                            break;
                        }
                        mtse.Add(new MatchStartEnds()
                        {
                            EndMonoMass = tempstartvalue,
                            StartMonoMass = 0,
                            Start = endforsequences,
                            End = enddd,
                            confidence = AminAcidConfidence.NotSure,
                            Gap = endfirstmtse,
                            SequenceTag = tempstring,
                            MainSequence = false,
                            DontShowall = Dontshowall
                        });
                        break;
                    }
                    tempstring = Convert.ToString(c) + tempstring;
                }
            }
            return mtse;
        }

        static CurrentIons FindCrntIonsintheRange(string seqgap, List<double> CrntIons = null, double ParentMass = 0, List<double> ReverseCrntIons = null)
        {
            CurrentIons ions = new CurrentIons();
            List<double> RvrsCrntIons = new List<double>();
            List<double> newCrntIons = new List<double>();
            List<double> reverseCrnIons = new List<double>();
            RvrsCrntIons = CrntIons.Select(a => ParentMass - a).ToList();
            string localString = String.Empty;
            int start = 0;
            int end = 1;
            foreach (char seq in seqgap)
            {
                localString = localString + Convert.ToString(seq);
                if (CrntIons != null)
                {
                    foreach (double crnt in CrntIons)
                    {
                        if (start >= CrntIons.Count || end >= CrntIons.Count) break;
                        if (sequencelength(localString) - Math.Abs(CrntIons[end] - CrntIons[start]) < -1)
                        {
                            start = end + 1;
                            break;
                        }
                        else if (Math.Abs(sequencelength(localString) - Math.Abs(CrntIons[end] - CrntIons[start])) < PPMCalc.CurrentPPMbasedonMatchList(CrntIons[start], Properties.Settings.Default.MatchTolerancePPM))
                        {
                            newCrntIons.Add(CrntIons[end]);
                            newCrntIons.Add(CrntIons[start]);
                            start = end;
                            end++;
                        }
                        else
                        {
                            start++;
                        }
                    }
                }
                else
                {
                    CrntIons = new List<double>();
                }
            }
            start = 0;
            end = 1;
            localString = string.Empty;
            CrntIons.Reverse();
            foreach (char seq in seqgap)
            {
                localString = localString + Convert.ToString(seq);
                if (reverseCrnIons != null)
                {
                    foreach (double crnt in CrntIons)
                    {
                        if (start >= CrntIons.Count || end >= CrntIons.Count) break;
                        if (sequencelength(localString) - Math.Abs(CrntIons[end] - CrntIons[start]) < -1)
                        {
                            start = end + 1;
                            break;
                        }
                        else if (Math.Abs(sequencelength(localString) - Math.Abs(CrntIons[end] - CrntIons[start])) < PPMCalc.CurrentPPMbasedonMatchList(CrntIons[start], Properties.Settings.Default.MatchTolerancePPM))
                        {
                            reverseCrnIons.Add(CrntIons[end]);
                            reverseCrnIons.Add(CrntIons[start]);
                            start = end;
                            end = end + 1;
                            break;
                        }
                        else
                        {
                            start++;
                        }
                    }
                }
                else
                {
                    reverseCrnIons = new List<double>();
                }
            }
            if (newCrntIons.Count >= reverseCrnIons.Count)
            {
                //directionformatchstartends = true;
                ions.currentions = newCrntIons;
                ions.directionformatchstartends = true;
                //return newCrntIons;
            }
            else
            {
                //directionformatchstartends = false;
                ions.currentions = reverseCrnIons;
                ions.directionformatchstartends = false;
                //return reverseCrnIons;
            }
            return ions;
        }

        class CurrentIons
        {
            public List<Double> currentions { get; set; }
            public bool directionformatchstartends { get; set; }
        }

        /// <summary>
        /// A generalized method for finding the extensions of sequence based on different parameters
        /// </summary>
        /// <param name="seqgap">This is sequence which needs to be compared with all the Ions </param>
        /// <param name="ModifiedSequenceForL">This is the original sequence which is used to find the index of a particular string found in the sequence</param>
        /// <param name="localindex"></param>
        /// <param name="CrntIons">All the Ions</param>
        /// <param name="ParentMass"> Parent Mass </param>
        /// <param name="indexofmatchpair">Index of the Pair Match which gives an ID to each of the Matches</param>
        /// <param name="monomassforreverse">Monomass</param>
        /// <param name="direction">Which direction is the sequence oriented which is not used hence needs to be removed</param>
        /// <param name="beforeorafter"> To check if the sequence is in front of the sequence or at the end</param>
        /// <param name="ReverseCrntIons">All the possible reversecrntions using information from both ends</param>
        /// <returns></returns>
        static List<MatchStartEnds> FindmatchStartendsintherange(string seqgap, string ModifiedSequenceForL, int localindex, List<double> CrntIons, double ParentMass,
                                                                        int indexofmatchpair, double monomassforreverse, bool direction, bool beforeorafter = false, List<double> ReverseCrntIons = null, bool Dontshowall = false, int minvalue = 0)
        {
            if (seqgap.Length == 1)
            {
                return new List<MatchStartEnds>();
            }

            List<double> RvrsCrntIons = new List<double>();
            List<MatchStartEnds> newMatchStartEnds = new List<MatchStartEnds>(); /// Find the matches in the forward direction.
            List<MatchStartEnds> reverseMatchStartEnds = new List<MatchStartEnds>(); ///Find the matches in the reverse direction.
            //StringBuilder localString = new StringBuilder();
            string localString = string.Empty; /// localstring
            int start = 0; ///Start index 
            int end = 1;   /// End index
            int templocalindex = 0;

            if (beforeorafter)
            {
                localindex = seqgap.Length;
            }
            else
            {
                localindex = ModifiedSequenceForL.IndexOf(seqgap) + 1;
            }

            if (seqgap == "" && !beforeorafter)
            {
                if (ParentMass - monomassforreverse > 0.1)
                {
                    newMatchStartEnds.Add(new MatchStartEnds()
                    {
                        Start = ModifiedSequenceForL.Length,
                        End = ModifiedSequenceForL.Length,
                        confidence = AminAcidConfidence.Gap, ///Confidence.Low,
                        SequenceTag = "",
                        ReverseSequence = false,
                        Gap = Math.Round(ParentMass - monomassforreverse, 4) - Molecules.Water,
                        MainSequence = true,
                        StartMonoMass = monomassforreverse,
                        EndMonoMass = ParentMass,
                        MonoMasses = new List<double>(),/// CrntIons.GetRange(start, Math.Abs(start - end)).ToList(),
                        Index = indexofmatchpair,
                        DontShowall = Dontshowall,
                        NumberofYellowPeaks = 0,
                        NumberofGreenPeaks = 0,
                        ForwardforHighlighting = beforeorafter,
                        ForwardIons = true
                    });
                }
            }

            //CrntIons = CrntIons.OrderByDescending(a => a).ToList();
            templocalindex = localindex;
            double tempcrntion = ParentMass - monomassforreverse;

            if (!beforeorafter)
            {
                end = CrntIons.Count - 1;
                start = end - 1;
            }
            int yellowpeaks = 0;
            StringBuilder strbldrseq = new StringBuilder();
            //CrntIons = CrntIons.OrderBy(a => a).ToList();
            foreach (char seq in seqgap)
            {
                strbldrseq.Append(seq);

                //if (!beforeorafter)
                //{
                //    //localString.Append(seq);
                //    localString = localString + Convert.ToString(seq);
                //}
                //else
                //{
                //    //string s = localString.ToString();
                //    //localString.Clear();
                //    //localString.Append(seq + s);
                //    localString = Convert.ToString(seq) + localString;
                //}

                if (CrntIons != null)
                {
                    //string s = localString.ToString();
                    foreach (double crnt in CrntIons)
                    {
                        if (beforeorafter)
                        {
                            if (start >= CrntIons.Count || end >= CrntIons.Count) break;
                        }
                        else
                        {
                            if (start <= minvalue || end <= minvalue) break;
                        }

                        if (Math.Abs(sequencelength(strbldrseq.ToString()) - Math.Abs(CrntIons[end] - CrntIons[start])) < PPMCalc.CurrentPPMbasedonMatchList(CrntIons[start], Properties.Settings.Default.MatchTolerancePPM) + 0.1)
                        {
                            yellowpeaks = yellowpeaks + 1;
                        }

                        if (sequencelength(strbldrseq.ToString()) - Math.Abs(CrntIons[end] - CrntIons[start]) < -1)
                        {
                            break;
                        }
                        else if (Math.Abs(sequencelength(strbldrseq.ToString()) - Math.Abs(CrntIons[end] - CrntIons[start])) < (PPMCalc.CurrentPPMbasedonMatchList(CrntIons[start], Properties.Settings.Default.MatchTolerancePPM) + 0.1))
                        {
                            int indexofmforl = 0;
                            indexofmforl = ModifiedSequenceForL.IndexOf((beforeorafter ? ReverseString.Reverse(strbldrseq.ToString()) : strbldrseq.ToString()), localindex - 1);
                            //indexofmforl = ModifiedSequenceForL.IndexOf(localString, localindex - 1);
                            indexofmatchpair++;
                            if (!beforeorafter)
                            {
                                localindex += strbldrseq.Length;    ///localString.Length;
                            }
                            newMatchStartEnds.Add(new MatchStartEnds()
                            {
                                Start = indexofmforl,
                                End = indexofmforl + strbldrseq.Length,/// localString.Length,
                                confidence = strbldrseq.Length == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low, ///Confidence.Low,
                                //confidence = localString.Length == 1 ? Confidence.Sure : Confidence.Low, ///Confidence.Low,
                                SequenceTag = beforeorafter ? ReverseString.Reverse(strbldrseq.ToString()) : strbldrseq.ToString(), ///localString,
                                ReverseSequence = false,
                                MainSequence = true,
                                StartMonoMass = Math.Min(CrntIons[start], CrntIons[end]),
                                EndMonoMass = Math.Max(CrntIons[start], CrntIons[end]),
                                MonoMasses = CrntIons.GetRange(start, Math.Abs(start - end)).ToList(),
                                Index = indexofmatchpair,
                                DontShowall = Dontshowall,
                                NumberofYellowPeaks = yellowpeaks,
                                NumberofGreenPeaks = 2,
                                ForwardforHighlighting = beforeorafter,
                                ForwardIons = true
                            });
                            strbldrseq.Clear();
                            //localString = string.Empty; //.Clear();
                            yellowpeaks = 0;
                            if (beforeorafter)
                            {
                                start = end;
                                end++;
                            }
                            else
                            {
                                end = start;
                                start--;
                            }
                            break;
                        }
                        ///This line of code was used to find the gaps with delta. Need to change some code to make this working again.
                        else
                        {
                            if (beforeorafter)
                            {
                                end++;
                            }
                            else
                            {
                                start--;
                            }
                        }
                    }
                }
                else
                {
                    CrntIons = new List<double>();
                }

                if (beforeorafter)
                {
                    localindex--;
                }
            }
            start = 0;
            end = 1;
            yellowpeaks = 0;

            localString = string.Empty; ////.Clear();
            localindex = templocalindex;

            if (!beforeorafter)
            {
                end = ReverseCrntIons.Count - 1;
                start = end - 1;
                ReverseCrntIons.Reverse();
            }
            strbldrseq.Clear();/// = new StringBuilder();
            foreach (char seq in seqgap)
            {
                bool canaddyellowpeaks = true;

                strbldrseq.Append(seq);

                //if (!beforeorafter)
                //{
                //    strbldrseq.Append(seq);
                //    //localString.Append(seq);
                //    //localString = localString + Convert.ToString(seq);
                //    //canaddyellowpeaks = true;
                //}
                //else
                //{
                //    //string s = localString.ToString();
                //    //localString.Clear();
                //    //localString.Append(seq + s);
                //    strbldrseq.Append(seq);
                //    //strbldrseq.ToString();
                //    //localString = Convert.ToString(seq) + localString;
                //    //canaddyellowpeaks = true;
                //}

                if (ReverseCrntIons != null)
                {
                    //string s = localString.ToString();
                    foreach (double crnt in ReverseCrntIons)
                    {
                        if (beforeorafter)
                        {
                            if (start >= ReverseCrntIons.Count || end >= ReverseCrntIons.Count) break;
                        }
                        else
                        {
                            if (start <= minvalue || end <= minvalue) break;
                        }

                        if ((Math.Abs(sequencelength(strbldrseq.ToString()) - Math.Abs(ReverseCrntIons[end] - ReverseCrntIons[start])) < PPMCalc.CurrentPPMbasedonMatchList(ReverseCrntIons[start], Properties.Settings.Default.MatchTolerancePPM) + 0.1) && canaddyellowpeaks)
                        {
                            yellowpeaks = yellowpeaks + 1;
                            canaddyellowpeaks = false;
                        }
                        if (sequencelength(strbldrseq.ToString()) - Math.Abs(ReverseCrntIons[end] - ReverseCrntIons[start]) < -1)
                        {
                            break;
                        }
                        else if (Math.Abs(sequencelength(strbldrseq.ToString()) - Math.Abs(ReverseCrntIons[end] - ReverseCrntIons[start])) < (PPMCalc.CurrentPPMbasedonMatchList(ReverseCrntIons[start], Properties.Settings.Default.MatchTolerancePPM) + 0.1))
                        {
                            int indexofmforl = 0;
                            indexofmforl = ModifiedSequenceForL.IndexOf((beforeorafter ? ReverseString.Reverse(strbldrseq.ToString()) : strbldrseq.ToString()), localindex - 1);
                            indexofmatchpair++;
                            if (!beforeorafter)
                            {
                                localindex += strbldrseq.Length; ///localString.Length;
                            }

                            reverseMatchStartEnds.Add(new MatchStartEnds()
                            {
                                Start = indexofmforl,
                                End = indexofmforl + strbldrseq.Length,/// localString.Length,
                                confidence = strbldrseq.Length == 1 ? AminAcidConfidence.Sure : AminAcidConfidence.Low, ///localString.Length == 1 ? Confidence.Sure : Confidence.Low,
                                SequenceTag = (beforeorafter ? ReverseString.Reverse(strbldrseq.ToString()) : strbldrseq.ToString()), ///localString,
                                ReverseSequence = false,
                                MainSequence = true,
                                StartMonoMass = Math.Min(ReverseCrntIons[start], ReverseCrntIons[end]),
                                EndMonoMass = Math.Max(ReverseCrntIons[start], ReverseCrntIons[end]),
                                MonoMasses = ReverseCrntIons.GetRange(start, Math.Abs(start - end)).ToList(),
                                Index = indexofmatchpair,
                                DontShowall = Dontshowall,
                                NumberofYellowPeaks = yellowpeaks,
                                NumberofGreenPeaks = 2,
                                ForwardforHighlighting = !beforeorafter,
                                ForwardIons = false
                            });
                            yellowpeaks = 0;
                            strbldrseq.Clear();
                            ///localString = "";///.Clear();
                            if (beforeorafter)
                            {
                                start = end;
                                end++;
                            }
                            else
                            {
                                end = start;
                                start--;
                            }
                            break;
                        }
                        else
                        {
                            if (beforeorafter)
                            {
                                end++;
                            }
                            else
                            {
                                start--;
                            }
                        }
                    }
                }
                else
                {
                    CrntIons = new List<double>();
                }
                if (beforeorafter) localindex--;
            }

            //if (newMatchStartEnds.Count == 0 && reverseMatchStartEnds.Count == 0)
            //{
            //}

            if (newMatchStartEnds.Count >= reverseMatchStartEnds.Count) return newMatchStartEnds;

            return reverseMatchStartEnds;
        }

        static List<MatchStartEnds> FindmatchStartendsintherangeformodifications(double lengthofmodifiedsequence, string seqgap, string ModifiedSequenceForL, int localindex, List<double> CrntIons, double ParentMass,
                                                                        int indexofmatchpair, double monomassforreverse, bool direction, bool beforeorafter = false, List<double> ReverseCrntIons = null)
        {
            return null;
        }


        /// <summary>
        /// Given the protein sequence and blast tag, find the alignments between each other
        /// </summary>
        /// <param name="Sequence">Protein reference sequence</param>
        /// <param name="BlastpTag">Similar tag from protein</param>
        /// <param name="PairMatchTag">de novo tag?</param>
        /// <param name="MonoMasses"></param>
        /// <param name="ParentMass"></param>
        /// <param name="Dontshowall"></param>
        /// <returns></returns>
        public static List<MatchStartEnds> ApplyHighlightforBlastPwithnoSequences(string Sequence, string BlastpTag, FindSequenceTags.SequenceTag PairMatchTag, List<double> MonoMasses, double ParentMass, bool Dontshowall = false)
        {
            List<MatchStartEnds> mtse = new List<MatchStartEnds>();

            string isobaricProteinSequence = Sequence.Replace('I', 'L');  // ModifiedSequenceforL
            string isobaricBlastPTag = BlastpTag.Replace("I", "L");
            string isobaricPairMatch = PairMatchTag.Sequence.Replace("I", "L");
            bool reversed = false;

            var tagMatches = new List<StringPositions>();
            var protMatches = new List<StringPositions>();

            int length = isobaricProteinSequence.Length;

            // Orient tag and protein to same direction.  The protein is the reference, so we should align to that.  
            if (isobaricProteinSequence.IndexOf(isobaricBlastPTag) == -1)
            {
                // reverse the tags to match the protein sequence direction
                isobaricBlastPTag = isobaricBlastPTag.Reverse();
                isobaricPairMatch = isobaricPairMatch.Reverse();
                reversed = true;
            }

            if (isobaricProteinSequence.IndexOf(isobaricBlastPTag) > -1)
            {
                // Find common amino acid tags between blasttag, de novo tag, and protein reference
                tagMatches.AddRange(PartialSequenceMatches.MatchesForBlastp(isobaricPairMatch, isobaricBlastPTag).OrderBy(a => a.start).ToList());
                protMatches.AddRange(PartialSequenceMatches.MatchesForBlastp(isobaricProteinSequence, isobaricBlastPTag).OrderBy(a => a.start).ToList());
            }
            else
            {
                // This should never happen -- Blastp Tag was not found in the protein sequence.  
                System.Diagnostics.Debug.WriteLine("Warning: BlastP tag should have been in the protein but was not!  ");
                return mtse;
            }

            if (tagMatches.Any() == false || protMatches.Any() == false) return mtse;

            // Loop through the matches to generate our "Match Start/Ends"
            foreach (var aTagMatch in tagMatches)
            {
                foreach (var aProtMatch in protMatches)
                {
                    mtse.Add(new MatchStartEnds
                    {
                        StartMonoMass = reversed ? PairMatchTag.IndividualAAs[PairMatchTag.Sequence.Length - 1 - aTagMatch.start].Start : PairMatchTag.IndividualAAs[aTagMatch.start].Start,
                        Start = aTagMatch.start,
                        End = aTagMatch.end,
                        confidence = AminAcidConfidence.Sure,
                        DontShowall = Dontshowall,
                        SequenceTag = aTagMatch.match,
                        EndMonoMass = reversed ? PairMatchTag.IndividualAAs[PairMatchTag.Sequence.Length - aTagMatch.end].End : PairMatchTag.IndividualAAs[aTagMatch.end - 1].End
                    });
                }
            }

            return mtse;
        }

        public static List<MatchStartEnds> ApplyHighlightforBlastPwithnoSequences(string Sequence, string BlastpTag, string PairMatch, bool Dontshowall = false)
        {
            List<MatchStartEnds> mtse = new List<MatchStartEnds>();
            string ModifiedSequenceforL = Sequence.Replace('I', 'L');
            PairMatch = PairMatch.Replace("I", "L");

            List<MSViewer.Classes.StringPositions> allmatches = new List<StringPositions>();
            int indexofblastp = 0;
            int length = ModifiedSequenceforL.Length;

            if (ModifiedSequenceforL.IndexOf(BlastpTag) > -1)
            {
                allmatches = PartialSequenceMatches.MatchesForBlastp(BlastpTag, PairMatch).OrderBy(a => a.start).ToList();
                indexofblastp = ModifiedSequenceforL.IndexOf(BlastpTag);
            }
            else if (ModifiedSequenceforL.IndexOf(ReverseString.Reverse(BlastpTag)) > -1)
            {
                allmatches = PartialSequenceMatches.MatchesForBlastp(ReverseString.Reverse(BlastpTag), ReverseString.Reverse(PairMatch)).OrderBy(a => a.start).ToList();
                indexofblastp = ModifiedSequenceforL.IndexOf(ReverseString.Reverse(BlastpTag));
            }

            mtse.Add(new MatchStartEnds
            {
                Start = 0,
                End = indexofblastp,
                confidence = AminAcidConfidence.NotPossible,
                DontShowall = Dontshowall
            });

            int lengthtillnow = 0;
            int length1 = 0;
            if (allmatches.Any())
            {
                lengthtillnow = indexofblastp + (allmatches.First().end - allmatches.First().start) + allmatches.First().start;
                length1 = indexofblastp + (allmatches.First().end - allmatches.First().start) + allmatches.First().start;

                if (allmatches.First().start != 0)
                {
                    mtse.Add(new MatchStartEnds
                    {
                        Start = indexofblastp,
                        End = indexofblastp + allmatches.First().start,
                        confidence = AminAcidConfidence.Gap,
                        DontShowall = Dontshowall
                    });
                }

                mtse.Add(new MatchStartEnds
                {
                    Start = indexofblastp + allmatches.First().start,
                    End = indexofblastp + allmatches.First().end,
                    confidence = AminAcidConfidence.Sure,
                    DontShowall = Dontshowall,
                    SequenceTag = allmatches.First().match
                });
            }
            int count = 0;

            for (int i = 1; i < allmatches.Count; i++)
            {
                if (allmatches[i - 1].end == allmatches[i].start)
                {
                    mtse.Add(new MatchStartEnds
                    {
                        Start = indexofblastp + allmatches[i].start,
                        End = indexofblastp + allmatches[i].end,
                        confidence = AminAcidConfidence.Sure,
                        DontShowall = Dontshowall,
                        SequenceTag = allmatches[i].match
                    });
                }
                else
                {
                    try
                    {
                        count = lengthtillnow;
                        mtse.Add(new MatchStartEnds
                        {
                            Start = count,
                            End = indexofblastp + allmatches[i].start,
                            confidence = AminAcidConfidence.Gap,
                            DontShowall = Dontshowall,
                        });
                        mtse.Add(new MatchStartEnds
                        {
                            Start = indexofblastp + allmatches[i].start,
                            End = indexofblastp + allmatches[i].end,
                            confidence = AminAcidConfidence.Sure,
                            DontShowall = Dontshowall,
                            SequenceTag = allmatches[i].match
                        });
                    }
                    catch (Exception)
                    {

                    }
                }
                lengthtillnow = indexofblastp + allmatches[i].end;
            }

            if (BlastpTag.Length != lengthtillnow)
            {
                mtse.Add(new MatchStartEnds
                {
                    Start = lengthtillnow,
                    End = indexofblastp + BlastpTag.Length,
                    confidence = AminAcidConfidence.Gap,
                    DontShowall = Dontshowall
                });
            }

            mtse.Add(new MatchStartEnds
            {
                Start = indexofblastp + BlastpTag.Length,
                End = length,
                confidence = AminAcidConfidence.NotPossible,
                DontShowall = Dontshowall
            });

            return mtse;
        }

        public static List<MatchStartEnds> ApplyHighlightwithnoSequences(string Sequence, string PairMatch, bool Dontshowall = false)
        {
            List<MatchStartEnds> mtse = new List<MatchStartEnds>();

            string ModifiedSequenceforL = Sequence.Replace('I', 'L');

            int indexofpairmatch = ModifiedSequenceforL.IndexOf(PairMatch);
            int length = ModifiedSequenceforL.Length;

            if (indexofpairmatch > -1)
            {
                mtse.Add(new MatchStartEnds
                {
                    Start = indexofpairmatch,
                    End = indexofpairmatch + PairMatch.Length,
                    confidence = AminAcidConfidence.Sure,
                    MainSequence = true,
                    DontShowall = Dontshowall
                });
                mtse.Add(new MatchStartEnds
                {
                    Start = 0,
                    End = indexofpairmatch,
                    confidence = AminAcidConfidence.NotPossible,
                    DontShowall = Dontshowall
                });
                mtse.Add(new MatchStartEnds
                {
                    Start = indexofpairmatch + PairMatch.Length,
                    End = length,
                    confidence = AminAcidConfidence.NotPossible,
                    DontShowall = Dontshowall
                });
            }
            else
            {
                char[] reverse = ModifiedSequenceforL.ToCharArray();
                Array.Reverse(reverse);
                string reversesequence = new string(reverse);
                indexofpairmatch = reversesequence.IndexOf(PairMatch);
                if (indexofpairmatch > -1)
                {
                    mtse.Add(new MatchStartEnds
                    {
                        Start = length - (indexofpairmatch + PairMatch.Length),
                        End = length - indexofpairmatch,
                        confidence = AminAcidConfidence.Sure,
                        MainSequence = true,
                        DontShowall = Dontshowall
                    });
                    mtse.Add(new MatchStartEnds
                    {
                        Start = 0,
                        End = length - (indexofpairmatch + PairMatch.Length),
                        confidence = AminAcidConfidence.NotPossible,
                        DontShowall = Dontshowall
                    });
                    mtse.Add(new MatchStartEnds
                    {
                        Start = length - indexofpairmatch,
                        End = length,
                        confidence = AminAcidConfidence.NotPossible,
                        DontShowall = Dontshowall
                    });
                }
                else
                {
                    mtse.Add(new MatchStartEnds
                    {
                        Start = 0,
                        End = length,
                        confidence = AminAcidConfidence.NotPossible,
                        DontShowall = Dontshowall
                    });
                }
            }
            return mtse;
        }

        public static List<MatchStartEnds> ApplyHighlightwithnoPairMatch(string Sequence, bool Dontshowall)
        {
            List<MatchStartEnds> mtse = new List<MatchStartEnds>();
            mtse.Add(new MatchStartEnds
            {
                Start = 0,
                End = Sequence.Length,
                confidence = AminAcidConfidence.NotPossible,
                MainSequence = true,
                DontShowall = Dontshowall
            });
            return mtse;
        }

        ///<summary>
        ///Goal of the this method is to go through all the possible permutations of all the Amino Acids which have a modification.
        ///All the combinations without any repetition. This is necessary to fast up the process, as there might be many modifications.
        ///For finding all the combinations which can have modification creating five separate strings which changes based on the modification necessary
        ///For example if the string is A B C D E
        ///The following can be the modifications.
        ///The ones which have only one modification are:
        /// Ae   B   C   D   E
        /// A    Be  C   D   E
        /// A    B   Ce  D   E
        /// A    B   C   De  E
        /// A    B   C   D   Ee
        ///The ones which have two modifications are:
        ///Ae   Be   C    D    E
        ///Ae   B    Ce   D    E
        ///Ae   B    C    De   E
        ///Ae   B    C    D    Ee
        ///A    Be   Ce   D    E
        ///A    Be   C    De   E
        ///A    Be   C    D    Ee
        ///A    B    Ce   De   E
        ///A    B    Ce   D    Ee
        ///A    B    C    De   Ee
        ///Three Modifications are:
        ///Ae   Be   Ce   D    E
        ///Ae   Be   C    De   E
        ///Ae   Be   C    D    Ee
        ///A    Be   Ce   De   E
        ///A    Be   Ce   D    Ee
        ///A    B    Ce   De   Ee
        ///Four Modifications are:
        ///Ae   Be   Ce   De   E
        ///Ae   B    Ce   De   Ee
        ///A    Be   Ce   De   Ee
        ///Five Modifications are:
        ///Ae   Be   Ce   De   Ee
        ///This needs a string that is stationary and another string which moves
        ///In this case firststring is stationary and thirdstring is the one which moves.
        ///begin is one before firststring
        ///secondstring is between firststring and thirdstring. As thirdstring moves the gap is filled by the second string.
        ///restofstring is the one after the thirdstring.
        ///<param name="sequence">This is the sequence for which different modifications need to be found </param>
        ///<param name="compareproteinlength">Protein length that needs to be compared with modified sequence </param>
        ///</summary>
        public static string CombinationofallModifications(string sequence, double compareproteinlength, double additionalmodification)
        {
            SortedList<int, string> positions = new SortedList<int, string>();
            int lengthofsequence = sequence.Length;

            string Modifiedstring = string.Empty; //Has just the amino acids which can be modified.
            string unmodifiedstring = string.Empty; //Has just the amino acids which cannot be modified.
            //Goal of the current project is to go through all the possible permutations of all the Amino Acids which have a modification.
            //All the combinations without any repetition. This is necessary to fast up the process, as there might be many modifications.
            int xing = 0;
            double d = 0;
            double maxaminoacidsmodmass = 0; ///Gets the maximum modification that can occur for the current sequence.
            //Looping through all the strings in the sequence to add the parts which have only modified aminoacids and unmodified aminoacids.
            foreach (string s in sequence.Select(a => a.ToString()).ToArray())
            {
                AminoAcidHelpers.ModifiedAminoAcids.TryGetValue(s, out d);
                ///Using trygetvalue for performance
                ///if (AminoAcidHelpers.ModifiedAminoAcids.Where(a => a.Key == s).Any())
                ///http://stackoverflow.com/questions/16101795/why-is-it-faster-to-check-if-dictionary-contains-the-key-rather-than-catch-the
                if (d != 0)
                {
                    Modifiedstring += s;
                    maxaminoacidsmodmass += d;
                }
                else
                {
                    unmodifiedstring += s;
                    positions.Add(xing, s);
                }
                xing++;
            }
            double lengthforunmodifiedstring = sequencelength(unmodifiedstring); ///Length of the Unmodified string which is later used for calculating the total sequence length.

            int lengthofmodifiedstring = Modifiedstring.Length;
            int count = 0;
            string finallyfinalstring = string.Empty;///Final string which is obtained after reconstructing the sequence with modified amino acids.
            //Getting the combinations where only one of the amino acids is modified.
            foreach (char s in Modifiedstring)
            {
                double lengthofmodifiedsequence = 0;
                string x = Modifiedstring.Substring(0, count) + AminoAcidHelpers.ModifiedAminoAcidStrings[Convert.ToString(s)] + Modifiedstring.Substring(count + 1, lengthofmodifiedstring - (count + 1)); //.Where(a => a.Contains(s)).First()
                lengthofmodifiedsequence = sequencelength(Modifiedstring.Substring(0, count)) + AminoAcidHelpers.ModifiedAminoAcids[AminoAcidHelpers.ModifiedAminoAcidStrings[Convert.ToString(s)]] + sequencelength(Modifiedstring.Substring(count + 1, lengthofmodifiedstring - (count + 1)));///.Where(a => a.Contains(s)).First()
                lengthofmodifiedsequence += lengthforunmodifiedstring;
                int lengthofstringmodified = x.Length - 1;
                //finallyfinalstring = string.Empty;
                if (lengthofmodifiedsequence - compareproteinlength > maxaminoacidsmodmass + additionalmodification)
                    return "";
                if (Math.Abs(lengthofmodifiedsequence - compareproteinlength) <= additionalmodification)
                {
                    return reconstructthestring(lengthofsequence, positions, x, lengthofstringmodified);
                }
                count++;
            }

            double length = 0;
            double locallength = 0;
            count = 0;
            string begin = string.Empty;
            string firststring = string.Empty;
            string secondstring = string.Empty;
            string thirdstring = string.Empty;
            string restofstring = string.Empty;
            string finalstring = string.Empty;
            for (int i = 1; i <= Modifiedstring.Length - 1; i++)///Outer loop. This one determines how many values need to be taken
            {
                for (int j = 0; j <= Modifiedstring.Length - i - 1; j++)///This is loop determines the begin and the firststring
                {
                    length = 0;
                    begin = Modifiedstring.Substring(0, j);
                    length = sequencelength(begin);
                    firststring = string.Empty;
                    int modificationslength = 1;
                    foreach (char s in Modifiedstring.Substring(j, i))
                    {
                        string m = AminoAcidHelpers.ModifiedAminoAcidStrings[Convert.ToString(s)];///.Where(a => a.Contains(s)).First();
                        firststring += m;
                        length += AminoAcidHelpers.ModifiedAminoAcids[m];
                        modificationslength++;
                    }
                    int second = 0;
                    for (int m = 1; m <= Modifiedstring.Length - (j + i); m++)///Inner loop which determines the secondstring, thirdstring and restofthestring
                    {
                        locallength = 0;
                        secondstring = Modifiedstring.Substring(second, count);
                        locallength += sequencelength(secondstring);
                        string m1 = AminoAcidHelpers.ModifiedAminoAcidStrings[Convert.ToString(Modifiedstring.ElementAt((m + i + j - 1)))];///.Where(a => a.Contains(Modifiedstring.ElementAt((m + i + j - 1)))).First();
                        thirdstring = m1;
                        locallength += AminoAcidHelpers.ModifiedAminoAcids[m1];
                        restofstring = Modifiedstring.Substring((m + i + j), lengthofmodifiedstring - (m + j + i));
                        locallength += sequencelength(restofstring);
                        finalstring = begin + firststring + secondstring + thirdstring + restofstring;
                        int lengthofstringmodified = finalstring.Length - 1;
                        if (((locallength + length + lengthforunmodifiedstring) - compareproteinlength) > maxaminoacidsmodmass + additionalmodification)
                        {
                            return "";
                        }
                        if (((locallength + length + lengthforunmodifiedstring) - compareproteinlength) < additionalmodification)
                        {
                            return reconstructthestring(lengthofsequence, positions, finalstring, lengthofstringmodified);
                        }
                        second = i + j;
                        count++;
                    }
                    count = 0;
                }
            }
            return finallyfinalstring;
        }

        /// <summary>
        /// Once a match is found based on the sequence length. The sequence is reconstructed.
        /// </summary>
        /// <param name="lengthofsequence">This is length of original sequence</param>
        /// <param name="positions">These are positions of unmodified sequences</param>
        /// <param name="finalstring">This is the final string</param>
        /// <param name="lengthofstringmodified">This is the length of modified string</param>
        /// <returns></returns>
        static string reconstructthestring(int lengthofsequence, SortedList<int, string> positions, string finalstring, int lengthofstringmodified)
        {
            string finallyfinalstring = string.Empty;
            int currentcount = 0;
            string emptystring = "";
            for (int k = 0; k < lengthofsequence; k++)
            {
                positions.TryGetValue(k, out emptystring);
                if (emptystring != null) ///if (positions.Where(a => a.Key == k).Any())
                {
                    finallyfinalstring += positions[k];
                }
                else
                {
                    if (lengthofstringmodified > currentcount && AminoAcidHelpers.onlymodifications.Contains(finalstring.Substring(currentcount + 1, 1)))
                    {
                        finallyfinalstring += finalstring.Substring(currentcount, 2);
                        currentcount++;
                        currentcount++;
                    }
                    else
                    {
                        finallyfinalstring += finalstring.Substring(currentcount, 1);
                        currentcount++;
                    }
                }
            }
            return finallyfinalstring;
        }

        /// <summary>
        /// Find the maximum modification possible for the current sequence even before generating all the combinations
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        static double LengthofSequencewithMaxModification(string sequence)
        {
            double length = 0;
            double d = 0;
            foreach (char s in sequence)
            {
                AminoAcidHelpers.ModifiedAminoAcids.TryGetValue(Convert.ToString(s), out d);
                if (d != 0) ///AminoAcidHelpers.ModifiedAminoAcids.Where(a => a.Key == s).Any()) //If the Amino Acid has a possible modification then use a different list
                {
                    length += AminoAcidHelpers.ModifiedAminoAcids[AminoAcidHelpers.ModifiedAminoAcidStrings[Convert.ToString(s)]]; ///.Where(a => a.Contains(s)).First()
                }
                else   ///If the Amino Acid have no modification then use the regular sequence
                {
                    length += AminoAcidHelpers.AminoAcidMass2[Convert.ToString(s)];
                }
            }
            //foreach (string s in sequence.Select(a => a.ToString()).ToArray())
            //{
            //    AminoAcidHelpers.ModifiedAminoAcids.TryGetValue(s, out d);
            //    if (d != 0) ///AminoAcidHelpers.ModifiedAminoAcids.Where(a => a.Key == s).Any()) //If the Amino Acid has a possible modification then use a different list
            //    {
            //        length += AminoAcidHelpers.ModifiedAminoAcids[AminoAcidHelpers.ModifiedAminoAcidStrings[s]]; ///.Where(a => a.Contains(s)).First()
            //    }
            //    else   ///If the Amino Acid have no modification then use the regular sequence
            //    {
            //        length += AminoAcidHelpers.AminoAcidMass2[s];
            //    }
            //}
            return length;
        }

    }
}

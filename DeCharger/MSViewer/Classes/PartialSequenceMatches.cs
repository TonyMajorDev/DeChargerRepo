using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class PartialSequenceMatches
    {
        /// <summary>
        /// Finds all the partial matches when two strings are compared. Both the strings need to be in the same direction
        /// </summary>
        /// <param name="realtag">This is the original tag</param>
        /// <param name="blasttag">This is the tag from the BlastP</param>
        /// <returns></returns>
        public static List<StringPositions> Matches(string realtag, string blasttag)
        {
            List<StringPositions> partialmatches = new List<StringPositions>();

            //blasttag = blasttag.Trim();


            string buffer = realtag; //Need to modify the realtag to get the correct longest substrings

            StringPositions partialmatch = null;

            while (buffer.Trim().Length > 2 && blasttag.Length > 2) //If the realtag is less than 2 or blasttag length is zero then break
            {

                partialmatch = new StringPositions();
                partialmatch.match = LongestCommonSubstring(buffer, blasttag); //Finding the longest common substring
                partialmatch.start = buffer.IndexOf(partialmatch.match);
                partialmatch.end = partialmatch.start + partialmatch.match.Length;

                if (string.IsNullOrWhiteSpace(partialmatch.match)) break; // no more matches

                if (partialmatch.match.Length == 0)
                    break;

                if (partialmatch.match.Length >= 3) //Break when there is no match found or partialmatch is really small
                {
                    //Only add to our collection if it is at least 3 amino acids in length
                    partialmatches.Add(partialmatch);
                }

                blasttag = blasttag.Substring(blasttag.IndexOf(partialmatch.match) + partialmatch.match.Length); //Removing the part which is already matched so won't compare it again
                buffer = buffer.ReplaceFirst(partialmatch.match, new string(' ', partialmatch.match.Length));  //Removing the part which is already matched so won't compare it again

            }

            return partialmatches;
        }

        /// <summary>
        /// Part of the sequence which matches to the pairmatch with the orientation.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="blastedtag"></param>
        /// <param name="querystart"></param>
        /// <param name="queryend"></param>
        /// <param name="sequencestart"></param>
        /// <param name="sequenceend"></param>
        /// <param name="blasttag"></param>
        /// <returns></returns>
        /// 
        public static bool TotalPartialMatchesforBlastp(string sequence, string blastedtag, int querystart, int queryend, int sequencestart, int sequenceend, string blasttag = null)
        {
            try
            {

                //if (blastedtag == blasttag)
                //    return true;

                int commonlength = queryend - querystart;
                sequence = sequence.Replace("I", "L");
                //string blastedtagstart = blastedtag.Substring(0, querystart - 1);
                string blastedtagstart = blastedtag.Substring(0, queryend);

                string blastedtagend = blastedtag.Substring(queryend, blastedtag.Length - queryend);

                //string sequencestarts = sequence.Substring(0, sequencestart);
                string sequencestarts = sequence.Substring(0, sequenceend);

                string sequenceends = sequence.Substring(sequenceend, sequence.Length - sequenceend);
                ///Forward direction
                if (CompareSequences(blastedtagstart, sequencestarts, blastedtagend, sequenceends, queryend, sequenceend, blastedtag))
                {
                    return true;/// CompareSequences(blastedtagstart, sequencestarts, blastedtagend, sequenceends, queryend, sequenceend, blastedtag);
                }
                ///Reverse Direction
                else
                {
                    commonlength = queryend - querystart;

                    sequence = sequence.Replace("I", "L");

                    blastedtag = ReverseString.Reverse(blastedtag);

                    //string blastedtagstart = blastedtag.Substring(0, querystart - 1);
                    blastedtagstart = blastedtag.Substring(0, queryend);

                    blastedtagend = blastedtag.Substring(queryend, blastedtag.Length - queryend);

                    //string sequencestarts = sequence.Substring(0, sequencestart);
                    sequencestarts = (sequence).Substring(0, sequenceend);

                    sequenceends = (sequence).Substring(sequenceend, sequence.Length - sequenceend);

                    return CompareSequences(blastedtagstart, sequencestarts, blastedtagend, sequenceends, queryend, sequenceend, blastedtag);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //    int count = 0;

        //    int i = Math.Min(blastedtagstart.Length, sequencestarts.Length);

        //    int consecutivecharcount = i;

        //    int l = 0;

        //    int k = blastedtagstart.Length - 1;
        //    int m = sequencestarts.Length - 1;

        //    for (int j = i - 1; j >= 0; j--)
        //    {
        //        if (blastedtagstart[k] == sequencestarts[m])
        //        {
        //            count++;
        //            if (consecutivecharcount == j + 1)
        //            {
        //                l++;
        //            }
        //            else
        //            {
        //                l = 0;
        //            }
        //            consecutivecharcount = j;
        //        }
        //        else
        //        {
        //            l = 0;
        //            consecutivecharcount = j;
        //        }
        //        k--;
        //        m--;
        //    }

        //    if (l >= 4)
        //        return true;

        //    i = Math.Min(blastedtagend.Length, sequenceends.Length);

        //    l = 0;

        //    consecutivecharcount = 0;

        //    k = queryend;// blastedtagend.Length;
        //    m = sequenceend;/// sequenceends.Length;

        //    for (int j = 0; j < i; j++)
        //    {
        //        if (sequenceends[j] == blastedtagend[j])
        //        {
        //            count++;
        //            if (consecutivecharcount == j - 1)
        //            {
        //                l++;
        //            }
        //            else
        //            {
        //                l = 0;
        //            }
        //            consecutivecharcount = j;
        //        }
        //        else
        //        {
        //            l = 0;
        //            consecutivecharcount = j;
        //        }
        //        k++;
        //        m++;
        //    }

        //    if (l > 4)
        //    {
        //        return true;
        //    }

        //    if ((count / blastedtag.Length) * 100 >= 80)
        //    {
        //        return true;
        //    }

        //    return false;
        //}
        //catch (Exception ex)
        //{
        //    return false;
        //}


        /// <summary>
        /// Compares two sequences based on a criteria for similarity
        /// </summary>
        /// <param name="blastedtagstart"></param>
        /// <param name="sequencestarts"></param>
        /// <param name="blastedtagend"></param>
        /// <param name="sequenceends"></param>
        /// <param name="queryend"></param>
        /// <param name="sequenceend"></param>
        /// <param name="blastedtag"></param>
        /// <returns>True for met criteria. And False for did not meet criteria</returns>
        public static bool CompareSequences(string blastedtagstart, string sequencestarts, string blastedtagend, string sequenceends, int queryend, int sequenceend, string blastedtag, int mincmnstringlength = 4, double minAminoAcidMatchPercentage = 80)
        {
            try
            {
                int count = 0;

                int i = Math.Min(blastedtagstart.Length, sequencestarts.Length);

                int consecutivecharcount = i;

                int l = 0;

                int k = blastedtagstart.Length - 1;
                int m = sequencestarts.Length - 1;

                bool ll = false;

                

                for (int j = i - 1; j >= 0; j--)
                {
                    if (blastedtagstart[k] == sequencestarts[m])
                    {
                        count++;
                        if (consecutivecharcount == j + 1)
                        {
                            l++;
                        }
                        else
                        {
                            l = 0;
                        }
                        if (l == mincmnstringlength)
                            ll = true;
                        consecutivecharcount = j;
                    }
                    else
                    {
                        l = 0;
                        consecutivecharcount = j;
                    }
                    k--;
                    m--;
                }

                if (ll)
                    return true;

                i = Math.Min(blastedtagend.Length, sequenceends.Length);

                l = 0;

                consecutivecharcount = 0;

                k = queryend;// blastedtagend.Length;
                m = sequenceend;/// sequenceends.Length;

                for (int j = 0; j < i; j++)
                {
                    if (sequenceends[j] == blastedtagend[j])
                    {
                        count++;
                        if (consecutivecharcount == j - 1)
                        {
                            l++;
                        }
                        else
                        {
                            l = 0;
                        }
                        if (l == mincmnstringlength)
                            ll = true;
                        consecutivecharcount = j;
                    }
                    else
                    {
                        l = 0;
                        consecutivecharcount = j;
                    }
                    k++;
                    m++;
                }

                if (ll)
                {
                    return true;
                }

                if ((count / blastedtag.Length) * 100 >= minAminoAcidMatchPercentage)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static string[] CommonString(string left, string right)
        {
            List<string> result = new List<string>();
            string[] rightArray = right.Split();
            string[] leftArray = left.Split();

            result.AddRange(rightArray.Where(r => leftArray.Any(l => l.StartsWith(r))));

            // must check other way in case left array contains smaller words than right array
            result.AddRange(leftArray.Where(l => rightArray.Any(r => r.StartsWith(l))));

            return result.Distinct().ToArray();
        }



        public static List<StringPositions> MatchesForBlastp(string realtag, string blasttag)
        {
            List<StringPositions> partialmatches = new List<StringPositions>();
            string buffer = realtag; //Need to modify the realtag to get the correct longest substrings

            string partialmatch = string.Empty;

            while (realtag.Length > 2 && blasttag.Length != 0) //If the realtag is less than 2 or blasttag length is zero then break
            {
                partialmatch = LongestCommonSubstring(realtag, blasttag).Trim(); //Finding the longest common substring
                if (partialmatch == string.Empty || partialmatch.Length < 2) //Break when there is no match found or partialmatch is really small
                {
                    break;
                }
                //int blastindex = buffer.IndexOf(blasttag);
                int index = buffer.IndexOf(partialmatch); //index of the partial match
                if (index < 0) break;
                int partialmatchlength = partialmatch.Length; //Length of the partial match
                partialmatches.Add(new StringPositions() ///Add the obtained match to the list
                {
                    match = partialmatch,
                    start = index,
                    end = index + partialmatchlength,  // this is off by +1, but changing now, would cause a cascade of changes, so leaving for now...
                });
                //partialmatches.Add(new StringPositions() ///Add the obtained match to the list
                //{
                //    match = partialmatch,
                //    start = blastindex,
                //    end = blastindex + partialmatchlength
                //});
                realtag = (realtag.Substring(0, realtag.IndexOf(partialmatch)) + " " + realtag.Substring(realtag.IndexOf(partialmatch) + partialmatchlength)).Trim(); //Removing the part which is already matched so won't compare it again
                blasttag = (blasttag.Substring(0, blasttag.IndexOf(partialmatch)) + " " + blasttag.Substring(blasttag.IndexOf(partialmatch) + partialmatchlength)).Trim();//Removing the part which is already matched so won't compare it again
                //if (buffer.IndexOf(partialmatch) < 0) break;
                buffer = buffer.Substring(0, buffer.IndexOf(partialmatch)) + new string(' ', partialmatchlength) + buffer.Substring(buffer.IndexOf(partialmatch) + partialmatchlength); //Filling in with emptystring where it matches so won't get the wrong index when a new comparision is made
            }
            return partialmatches;
        }

        /// <summary>
        /// Finds the longest substring when two strings are compared.
        /// https://fuzzystring.codeplex.com/SourceControl/latest#FuzzyString/FuzzyString/LongestCommonSubstring.cs
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string LongestCommonSubstring(string source, string target)
        {
            if (String.IsNullOrEmpty(source) || String.IsNullOrEmpty(target)) { return null; }

            int[,] L = new int[source.Length, target.Length];
            int maximumLength = 0;
            int lastSubsBegin = 0;
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                for (int j = 0; j < target.Length; j++)
                {
                    if (source[i] != target[j])
                    {
                        L[i, j] = 0;
                    }
                    else
                    {
                        if ((i == 0) || (j == 0))
                            L[i, j] = 1;
                        else
                            L[i, j] = 1 + L[i - 1, j - 1];

                        if (L[i, j] > maximumLength)
                        {
                            maximumLength = L[i, j];
                            int thisSubsBegin = i - L[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {//if the current LCS is the same as the last time this block ran
                                stringBuilder.Append(source[i]);
                            }
                            else //this block resets the string builder if a different LCS is found
                            {
                                lastSubsBegin = thisSubsBegin;
                                stringBuilder.Length = 0; //clear it
                                stringBuilder.Append(source.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Simple class which has the match, start and end
    /// </summary>
    public class StringPositions
    {
        public string match { get; set; }
        public int start { get; set; }
        public int end { get; set; }

    }

}
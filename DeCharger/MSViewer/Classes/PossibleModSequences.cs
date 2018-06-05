using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalProcessing;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using MassSpectrometry;
using MSViewer.Classes;
using Science.Proteomics;

namespace MSViewer.Classes
{
    /// <summary>
    /// Class to find all the modifications
    /// </summary>
    public class PossibleModSequences
    {
        /// <summary>
        /// Find the total aminoacid mass of the sequence.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static double sequencelength(string sequence)
        {
            double length = 0;
            foreach (char c in sequence)
            {
                try
                {
                    length = length + AminoAcidHelpers.AminoAcids[c];
                }
                catch (Exception ex)
                {
                    return double.NaN;
                }
            }
            return Math.Round(length, 4);
        }

        /// <summary>
        /// Sorting all the mods based on a criteria.
        /// Sorting the elements based on an order. First preference is given to the bandyionpercent.
        /// Then sort by descending intensity. Highest intensity peaks are given next priority.
        /// Total number of known mods are given priority over unknown mods.
        /// Then all the mods found.
        /// MedianofMedianbandYion errors. The sequence which has less error spread is given priority.
        /// </summary>
        /// <param name="Allmods"></param>
        /// <returns></returns>
        public static List<PossibleMods> SortModsbyPriority(List<PossibleMods> Allmods, int count, bool KnownMods)
        {
            if (Allmods != null && Allmods.Count != 0) /// If there are any mods
            {
                Allmods = Allmods.Any(a => !a.hasremainingstartgap) ? Allmods.Where(a => !a.hasremainingstartgap).ToList() : Allmods;

                double maxbandyionpercent = Allmods.Where(a => a.BandYIonPercent != null).MaxBy(a => a.BandYIonPercent).BandYIonPercent; /// Get the MaxBandYIon Percent

                //double averageerror = Allmods.Where(a => a.AverageError != null).MaxBy(a => a.AverageError).AverageError; ///Get the best average error which is only within 15% range of the maxbandyion percent. 
                double averageerror = 0;

                if (maxbandyionpercent != 0)
                {
                    averageerror = Allmods.Where(a => a.AverageError != null && ((((maxbandyionpercent - a.BandYIonPercent) / maxbandyionpercent) * 100) <= 25)).MaxBy(a => a.AverageError).AverageError; ///Get the best average error which is only within 25% range of the maxbandyion percent. 
                }
                else
                {
                    averageerror = Allmods.Where(a => a.AverageError != null).MaxBy(a => a.AverageError).AverageError;
                }

                double bandyionpercent = Allmods.Where(a => a.AverageError == averageerror).First().BandYIonPercent;

                List<PossibleMods> tempAllmods = new List<PossibleMods>();

                Allmods = Allmods.Where(a => (a.BandYIonPercent > bandyionpercent) || (Math.Abs(a.AverageError - averageerror) < 0.9)).ToList(); /// Get the mods within average error

                if (!Allmods.Any())
                {
                    return new List<PossibleMods>();
                }

                if (Allmods.Any(a => a.CurrentGap <= 1.0))
                {
                    return Allmods
                        .OrderByDescending(a => a.CurrentGap <= 1.0)
                        .OrderBy(a => a.hasremainingstartgap)
                        .OrderByDescending(a => a.TotalKnownMods)      ///Total number of known mods are given priority over unknown mods.
                       .ThenByDescending(a => a.TotalNumberofModifiedMatchAminoAcids).
                        ThenByDescending(a => a.BandYIonPercent).    ///Sorting the elements based on an order. First preference is given to the bandyionpercent.
                        ThenByDescending(a => a.Intensity).           ///Then sort by descending intensity. Highest intensity peaks are given next priority.
                        ThenByDescending(a => a.Currentposition).
                        ThenByDescending(a => a.Currentpositionatend).
                        ThenBy(a => a.NumberofMods).                  ///Less number of mods would be a better explanation of a modifications.
                        ThenByDescending(a => a.TotalWaterLosses).    ///Then by TotalWaterLosses. More waterlosses means better score.
                        ThenByDescending(a => a.TotalAmmoniaLosses).  ///Then by TotalAmoniaLosses. More Aminolosses means better score.
                        //ThenBy(a => a.MedianofMedianbandyionerrors).  ///MedianofMedianbandYion errors. The sequence which has less error spread is given priority.
                        Take(Math.Min(count, Allmods.Count)).ToList();   ///Only taking a limited number at a time.

                }
                else
                {
                    return Allmods
                       .OrderBy(a => a.hasremainingstartgap)
                       .OrderByDescending(a => a.TotalKnownMods).      ///Total number of known mods are given priority over unknown mods.
                       ThenByDescending(a => a.TotalNumberofModifiedMatchAminoAcids).
                       ThenByDescending(a => a.BandYIonPercent).    ///Sorting the elements based on an order. First preference is given to the bandyionpercent.
                       ThenByDescending(a => a.Intensity).           ///Then sort by descending intensity. Highest intensity peaks are given next priority.
                       ThenByDescending(a => a.Currentposition).
                       ThenByDescending(a => a.Currentpositionatend).
                       ThenBy(a => a.NumberofMods).                  ///Less number of mods would be a better explanation of a modifications.
                       ThenByDescending(a => a.TotalWaterLosses).    ///Then by TotalWaterLosses. More waterlosses means better score.
                       ThenByDescending(a => a.TotalAmmoniaLosses).  ///Then by TotalAmoniaLosses. More Aminolosses means better score.
                        //ThenBy(a => a.MedianofMedianbandyionerrors).  ///MedianofMedianbandYion errors. The sequence which has less error spread is given priority.
                       Take(Math.Min(count, Allmods.Count)).ToList();   ///Only taking a limited number at a time.
                }

            }
            else  /// If there are No mods then return an empty list
            {
                return new List<PossibleMods>();
            }
        }

        /// <summary>
        /// Checks all the possible modifications.
        /// Loops through all the AminoAcids in the sequence and generates all the modifications
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="startmonomass"></param>
        /// <param name="endmonomass"></param>
        /// <param name="monomasslist"></param>
        /// <param name="gap"></param>
        /// <param name="parentmass"></param>
        /// <param name="isend"></param>
        /// <param name="originalsequence"></param>
        /// <param name="isforward"></param>
        /// <param name="startp"></param>
        /// <param name="endp"></param>
        /// <param name="islast"></param>
        /// <param name="aminoacids"></param>
        /// <returns></returns>
        //public static string CheckforMods3(string sequence, double initialstartmonomass, double initialendmonomass, double finalstartmonomass, double finalendmonomass, Dictionary<double, double> monomasslist, double startgap, double endgap, double parentmass, bool isend, string originalsequence, bool isforward, int initialstartp, int initialendp, int finalstartp, int finalendp, bool islast, SortedList<char, double> aminoacids, bool HasKnownMods, ref List<PossibleMods> psdms)
        //{
        //    string sequencewithmods = string.Empty;

        //    string sequencewithoutmods = RemoveAllMods(sequence);

        //    bool hasinitialgap = (initialstartmonomass != initialendmonomass); // If both monomasses are same then there is no gap in the beginning of the sequence. 

        //    bool hasfinalgap = (finalstartmonomass != finalendmonomass); // If both monomasses are same then there is no gap in the end of the sequence.
        //    double tempgap = Math.Round(startgap, 4);

        //    bool breakthegap = false;

        //    bool noinitialgap = false;

        //    if (initialstartp == initialendp)
        //    {
        //        noinitialgap = true;
        //    }


        //    if (!hasfinalgap) // If it doesn't have a gap in the end it should change the value of the finalstartp
        //    {
        //        finalstartp = sequence.Length - 1;
        //        breakthegap = true;

        //    }

        //    if (!hasinitialgap) // If there is no gap in the start it should change the value of the startp
        //    {
        //        initialstartp = finalstartp;
        //        tempgap = Math.Round(endgap, 4);
        //        initialstartmonomass = finalstartmonomass;
        //        //initialendp = finalendp;
        //        breakthegap = true;
        //    }

        //    if (sequencewithoutmods.Length != sequence.Length && hasfinalgap) // If the SequenceWithoutMods is not same the OriginalSequence which means there are mods and need to change the Indexes for InitialGap and FinalGap
        //    {
        //        int tempfinalend = finalendp;
        //        int tempfinalstartp = finalstartp;

        //        finalendp = finalendp + (sequence.Length - sequencewithoutmods.Length);
        //        finalstartp = finalstartp + (sequence.Length - sequencewithoutmods.Length);
        //        if (finalstartp > sequence.Length)
        //        {
        //            finalendp = tempfinalend;
        //            finalstartp = tempfinalstartp;
        //        }
        //    }

        //    List<string> sequencewithmodslist = new List<string>();

        //    var firstallmonos = monomasslist.Where(a => (a.Key <= initialendmonomass + 1.0) && (a.Key >= initialstartmonomass + 1.0)).OrderBy(a => a.Key).Select(a => a.Key).ToList(); //Only look for amino acids within the range.

        //    var lastallmonons = monomasslist.Where(a => (a.Key >= finalstartmonomass + 1.0) && (a.Key <= finalendmonomass + 1.0)).OrderBy(a => a.Key).Select(a => a.Key).ToList();

        //    lastallmonons.AddRange(lastallmonons.Select(a => (parentmass - a)).ToList()); //Add the reverse amino acids

        //    firstallmonos.AddRange(firstallmonos.Select(a => (parentmass - a)).ToList()); //Add the reverse amino acids

        //    lastallmonons.Add(0);
        //    lastallmonons.Add(Molecules.Water);
        //    lastallmonons.Add(parentmass);
        //    lastallmonons.Add(parentmass - Molecules.Water);


        //    firstallmonos.Add(0);
        //    firstallmonos.Add(Molecules.Water);
        //    firstallmonos.Add(parentmass);
        //    firstallmonos.Add(parentmass - Molecules.Water);

        //    double currentmonomass = initialstartmonomass;
        //    double currentgap = startgap;
        //    double reversegap = startgap;
        //    double currentsequencelength = 0.0;
        //    List<PossibleMods> possiblelist = new List<PossibleMods>();
        //    List<double> medianoferrors = new List<double>();

        //    int count = 0;
        //    int reversecount = 0;

        //    int monomasscount = 0;
        //    int reversemonomasscount = 0;

        //    int tempcount = 0;

        //    bool firstterm = true;
        //    string tempsequencewithmods = string.Empty;

        //    int sequencecount = 0;
        //    bool lastterm = false;
        //    int tempmonomasscount = 0;
        //    int Allmodscount = 0;

        //    List<PossibleMods> Allmods = new List<PossibleMods>();
        //    string aa = string.Empty;
        //    List<PossibleMods> temppossiblelist = new List<PossibleMods>();
        //    string previousstring = string.Empty;

        //    previousstring = sequence.Substring(0, initialstartp);

        //    double maxmodmass = Convert.ToDouble(App.AllValidationModifications.MaxBy(a => a.Mass).Mass);

        //    double maxgap = currentgap;

        //    double maxaminoacidmass = aminoacids.MaxBy(a => a.Value).Value;

        //    List<CalculateBandYIons.BYIons> bandyionlist = new List<CalculateBandYIons.BYIons>();

        //    firstallmonos = firstallmonos.OrderBy(a => a).ToList();

        //    List<double> XvalueMonos = new List<double>();

        //    XvalueMonos = monomasslist.Select(a => a.Key).ToList();

        //    List<double> tempallmonomasses = new List<double>();

        //    tempallmonomasses = firstallmonos;

        //    string tempvalidationsequence = string.Empty;

        //    string fixedstring = string.Empty;

        //    bool finalend = false;
        //    bool nofinalgap = false;
        //    bool afterstartgap = false;
        //    //int tempfinalstartp = finalstartp;
        //    bool lookatfinalgap = true;
        //    if (initialstartp == initialendp) ///If there is no initial amino acids then need to change
        //    {
        //        if (finalstartp == finalendp) /// If there is no initial amino acids and no final amino acids then need to change
        //        {
        //            if (hasinitialgap && hasfinalgap)
        //            {
        //                previousstring = sequence.Substring(0, 1) + "[" + startgap + "]" + sequence.Substring(1, finalendp - 1) + "[" + endgap + "]";
        //            }
        //            else if (hasfinalgap)
        //            {
        //                previousstring = sequence + "[" + endgap + "]";
        //            }
        //            noinitialgap = true;
        //        }
        //        else
        //        {
        //            initialstartp = finalstartp;
        //            lookatfinalgap = false;
        //            if (hasinitialgap) ///If it has initial gap 
        //            {
        //                previousstring = sequence.Substring(0, 1) + "[" + startgap + "]" + sequence.Substring(1, initialstartp - 1);
        //                tempgap = endgap;
        //            }
        //            else
        //            {
        //                previousstring = sequence.Substring(0, initialstartp);
        //            }
        //            noinitialgap = true;
        //        }
        //        //previousstring = sequence
        //    }
        //    string temporarystring = string.Empty;
        //    //int tempfinalstartp = 
        //    for (int i = initialstartp; i < sequence.Length; i++)
        //    {
        //        aa = sequence[i].ToString();

        //        temppossiblelist.Clear();
        //        if (i == initialstartp && hasinitialgap && !noinitialgap)
        //        {
        //            previousstring = previousstring + aa;
        //        }

        //        currentmonomass = initialstartmonomass + CalculateBandYIons.sequencelengthwithmodifications(previousstring);

        //        int numberofmods = NumberofMods(previousstring);

        //        temppossiblelist.Add(new PossibleMods()
        //        {
        //            Sequence = previousstring,
        //            CurrentGap = tempgap,
        //            hasremainingstartgap = false,
        //            CurrentMonomass = currentmonomass,
        //            HasMonoMass = false,
        //            ModGap = 0,
        //            MonosCount = 0,
        //            StartMonomass = initialstartmonomass,
        //            NumberofMods = numberofmods
        //        });

        //        if (i == 0 && isend)
        //        {
        //            if (isforward && initialstartp == 0) //If the sequence is forward and this is the first AminoAcid. Nterm
        //            {
        //                possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist, false, true, false, ((initialendp - initialstartp == 1) ? false : true));
        //            }
        //            else
        //            {
        //                possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist);
        //            }
        //        }
        //        else if (i == sequence.Length - 1 && isend && !isforward && initialstartp == 0) //If the sequence is reverse and first AminoAcid. Nterm . If it has only one term then don't 
        //        {
        //            possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist, false, true, true, ((initialendp - initialstartp == 1) ? false : true));
        //        }
        //        else
        //        {
        //            possiblelist = PossiblemodSequences(tempallmonomasses, aa, false, temppossiblelist);
        //        }

        //        /// Going through the list and producing the results
        //        /// If the initial sequence is as follows with gaps at the beginning and one at the end
        //        /// (ABCD)[20.09]EFGHIJKL(MNOPQRST)[18.09]
        //        /// (A)[20.09]BCDEFGHIJKL(M[18.09]NOPQRST) 1st possiblility
        //        /// (A)[20.09]BCDEFGHIJKL(MN[18.09]OPQRST) 2nd possiblility
        //        /// (A)[20.09]BCDEFGHIJKL(MNO[18.09]PQRST) 3rd possiblility
        //        /// (A)[20.09]BCDEFGHIJKL(MNOP[18.09]PQRST) 4th possiblility
        //        /// ....
        //        /// ....
        //        /// ....
        //        /// (AB)[20.09]CDEFGHIJKL(M[18.09]NOPQRST) 2nd loop 1st possibility
        //        /// (AB)[20.09]CDEFGHIJKL(MN[18.09]OPQRST) 2nd loop 2nd possibility
        //        /// (AB)[20.09]CDEFGHIJKL(MNO[18.09]PQRST) 2nd loop 3rd possibility
        //        /// ......
        //        /// ......
        //        /// ......
        //        /// (ABCD)[20.09]EFGHIJKL(MNOPQRST)[18.09]  Final possibility

        //        temppossiblelist.Clear(); //Clearing the temporary list
        //        foreach (var possiblemod in possiblelist) ///Going through the list of items produced.
        //        {
        //            string currenttemporarystring = string.Empty;
        //            if (possiblemod.CurrentGap > 0.1) //If there is any gap then we should add that at the end of the sequence
        //            {
        //                currenttemporarystring = "(" + possiblemod.Sequence + ")" + "[" + possiblemod.CurrentGap + "]" + sequence.Substring(i + 1, finalstartp - (i + 1));
        //            }
        //            else
        //            {
        //                currenttemporarystring = possiblemod.Sequence + sequence.Substring(i + 1, finalstartp - (i + 1));
        //            }

        //            currenttemporarystring = possiblemod.Sequence + (possiblemod.CurrentGap > 0 ? ("[" + possiblemod + "]") : "") + sequence.Substring(i + 1, finalstartp - (i + 1)); ///Completing the resting of the sequence.

        //            foreach (var sqs in sequence.Substring(finalstartp, finalendp - finalstartp))
        //            {
        //                currenttemporarystring = currenttemporarystring + Convert.ToString(sqs);
        //                if (hasfinalgap && (finalendp != finalstartp) && lookatfinalgap)
        //                {
        //                    temppossiblelist.Add(new PossibleMods()
        //                    {
        //                        Sequence = currenttemporarystring,
        //                        CurrentGap = endgap,
        //                        hasremainingstartgap = true,
        //                        CurrentMonomass = finalstartmonomass,
        //                        HasMonoMass = false,
        //                        ModGap = 0,
        //                        MonosCount = 0,
        //                        NumberofMods = numberofmods
        //                    });
        //                    possiblelist = PossiblemodSequences(lastallmonons, Convert.ToString(sqs), true, temppossiblelist, false, true, true, ((initialendp - initialstartp == 1) ? false : true));
        //                }
        //            }
        //        }
        //        //temppossiblelist.Add(new PossibleMods()
        //        //{
        //        //    Sequence = previousstring + 
        //        //});

        //        //if (initialendp != 0 && !noinitialgap)
        //        //{
        //        //    for (int j = i + 1; j < sequence.Length; j++)
        //        //    {

        //        //        if (j == initialendp)
        //        //        {
        //        //            foreach (var pbs in possiblelist)
        //        //            {
        //        //                if (pbs.CurrentGap > 0.1)
        //        //                {
        //        //                    pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (hasfinalgap ? (sequence.Substring(j, finalstartp - (j))) : (sequence.Substring(j, sequence.Length - j)));
        //        //                    pbs.NumberofMods = pbs.NumberofMods + 1;
        //        //                }
        //        //                else
        //        //                {
        //        //                    pbs.Sequence = pbs.Sequence + (hasfinalgap ? (sequence.Substring(j, finalstartp - (j))) : (sequence.Substring(j, sequence.Length - j)));
        //        //                }
        //        //                pbs.CurrentMonomass = finalstartmonomass;
        //        //                pbs.CurrentGap = endgap;
        //        //            }
        //        //            j = finalstartp;
        //        //            tempallmonomasses = lastallmonons;
        //        //            if (breakthegap) break;
        //        //            //if (!hasinitialgap) break;
        //        //        }
        //        //        //if (j > initialendp && j < finalstartp)
        //        //        //{
        //        //        //    break;
        //        //        //}
        //        //        string bb = sequence[j].ToString();

        //        //        if (isend && j == sequence.Length - 1)
        //        //        {
        //        //            possiblelist = PossiblemodSequences(tempallmonomasses, bb, true, possiblelist, true);
        //        //        }
        //        //        else
        //        //        {
        //        //            possiblelist = PossiblemodSequences(tempallmonomasses, bb, false, possiblelist);
        //        //        }

        //        //        if (j == initialendp - 1)
        //        //        {
        //        //            foreach (var pbs in possiblelist)
        //        //            {
        //        //                if (pbs.CurrentGap > 0.1)
        //        //                {
        //        //                    pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (hasfinalgap ? (sequence.Substring(j + 1, finalstartp - (j + 1))) : (sequence.Substring(j + 1, finalstartp - (j)))); /// +"From here";
        //        //                    pbs.NumberofMods = pbs.NumberofMods + 1;
        //        //                }
        //        //                else
        //        //                {
        //        //                    pbs.Sequence = pbs.Sequence + (hasfinalgap ? (sequence.Substring(j + 1, finalstartp - (j + 1))) : (sequence.Substring(j + 1, finalstartp - (j))));
        //        //                }
        //        //                pbs.CurrentMonomass = finalstartmonomass;
        //        //                pbs.CurrentGap = endgap;
        //        //            }
        //        //            j = finalstartp - 1;
        //        //            tempallmonomasses = lastallmonons;
        //        //            if (breakthegap) break;
        //        //        }
        //        //    }
        //        //}
        //        //else if (!noinitialgap)
        //        //{
        //        //    foreach (var pbs in possiblelist)
        //        //    {
        //        //        int lengthvalue = RemoveAllMods(pbs.Sequence).Length;
        //        //        if (pbs.CurrentGap > 0.1 && initialendp != 0)
        //        //        {
        //        //            pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (sequence.Substring(lengthvalue, sequence.Length - lengthvalue));
        //        //            pbs.CurrentGap = 0;
        //        //            pbs.NumberofMods = pbs.NumberofMods + 1;
        //        //        }
        //        //        else if (pbs.CurrentGap > 0.1)
        //        //        {
        //        //            pbs.Sequence = pbs.Sequence + (sequence.Substring(lengthvalue, sequence.Length - lengthvalue));
        //        //            pbs.CurrentGap = endgap;
        //        //        }
        //        //        pbs.CurrentMonomass = finalstartmonomass;
        //        //    }
        //        //}
        //        //else if (noinitialgap)
        //        //{
        //        //    foreach (var pbs in possiblelist)
        //        //    {
        //        //        if (pbs.CurrentGap > 0.1)
        //        //        {
        //        //            pbs.Sequence = pbs.Sequence + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (sequence.Substring(i + 1, sequence.Length - (i + 1)));
        //        //            pbs.CurrentGap = 0;
        //        //            pbs.NumberofMods = pbs.NumberofMods + 1;
        //        //        }
        //        //        else
        //        //        {
        //        //            pbs.Sequence = pbs.Sequence + sequence.Substring(i + 1, sequence.Length - (i + 1));
        //        //            //pbs.ModGap = 0;
        //        //            //jhkljlk jklj; jlkj l;
        //        //            //pbs.CurrentGap = endgap;
        //        //        }
        //        //        pbs.CurrentMonomass = finalstartmonomass;
        //        //    }
        //        //}
        //        if (!finalend)
        //            tempallmonomasses = firstallmonos;
        //        possiblelist = possiblelist.Where(a => Math.Abs(a.ModGap) <= Math.Abs(tempgap)).ToList();

        //        //hkjhkjh jnjkljlkll; hkj hjkh;

        //        List<double> bandyionerrors = new List<double>();
        //        double bandyionerrormedian = 0.0;
        //        for (int ii = 0; ii < possiblelist.Count; ii++)
        //        {
        //            if (!isforward)
        //            {
        //                possiblelist[ii].Sequence = reversesequencewithmods(possiblelist[ii].Sequence);
        //            }
        //            //If there is any gap need to include that in the sequence
        //            if (Math.Abs(possiblelist[ii].CurrentGap) > 0.1)
        //            {
        //                if (islast)
        //                {
        //                    possiblelist[ii].Sequence = possiblelist[ii].Sequence + "[" + Math.Round(possiblelist[ii].CurrentGap, 4) + "]"; /// AddModtoSequenceAttheend(possiblelist[ii].Sequence, Math.Round(possiblelist[ii].CurrentGap, 2), finalstartp, finalendp);                            
        //                }
        //                else
        //                {
        //                    possiblelist[ii].Sequence = AddModtoSequence(possiblelist[ii].Sequence, Math.Round(possiblelist[ii].CurrentGap, 2));
        //                }
        //                possiblelist[ii].CurrentGap = 0;
        //            }
        //            ////
        //            if (isforward)
        //            {
        //                //bandyionlist = CalculateBandYIons.CalculateBandYIon(originalsequence.Substring(0, initialstartp) + (possiblelist[ii].Sequence) + originalsequence.Substring(initialendp, originalsequence.Length - (initialendp)), XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
        //                bandyionlist = CalculateBandYIons.CalculateBandYIon(possiblelist[ii].Sequence, XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
        //            }
        //            else
        //            {
        //                //bandyionlist = CalculateBandYIons.CalculateBandYIon(originalsequence.Substring(0, initialstartp) + (possiblelist[ii].Sequence) + originalsequence.Substring(initialendp, originalsequence.Length - (initialendp)), XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
        //                bandyionlist = CalculateBandYIons.CalculateBandYIon(possiblelist[ii].Sequence, XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
        //            }
        //            possiblelist[ii].BandYIonPercent = Math.Round(((double)bandyionlist.Count(a => a.yioncolor || a.bioncolor)
        //                //(0.25) * (double)bandyionlist.Count(a => a.bh2oioncolor || a.yh2oioncolor) +
        //                //(0.25) * (double)bandyionlist.Count(a => a.bnh3ioncolor || a.ynh3ioncolor))
        //                                                /
        //                                                (double)bandyionlist.Count) * 100, 2);

        //            possiblelist[ii].BIonPercent = Math.Round(((double)bandyionlist.Count(a => a.bioncolor) / (double)bandyionlist.Count) * 100, 2);

        //            possiblelist[ii].YIonPercent = Math.Round(((double)bandyionlist.Count(a => a.yioncolor) / (double)bandyionlist.Count) * 100, 2);

        //            possiblelist[ii].Intensity = bandyionlist.Sum(a => a.Intensity);

        //            possiblelist[ii].TotalAmmoniaLosses = bandyionlist.Where(a => a.bnh3ioncolor).Count();

        //            possiblelist[ii].TotalWaterLosses = bandyionlist.Where(a => a.bh2oioncolor).Count();

        //            possiblelist[ii].ModNorCTerm = ((possiblelist[ii].Sequence.Substring(1, 1).Contains("[") && isend && initialstartp == 0) //If there is a modification at the n-term
        //                                             || (possiblelist[ii].Sequence.EndsWith("]") && initialstartp != 0 && isend))            //If there is a modification at the c-term
        //                                             ? true : false;                                                                   //Setting the norcerterm to be true

        //            bandyionerrors = bandyionlist.Where(a => a.bionerror != 100).Select(a => a.bionerror).ToList();
        //            bandyionerrors.AddRange(bandyionlist.Where(a => a.yionerror != 100).Select(a => a.yionerror).ToList());

        //            List<double> bandyionerrorsppm = new List<double>();

        //            bandyionerrorsppm = bandyionlist.Where(a => a.bionerror != 100).Select(a => a.bionPPM).ToList();

        //            bandyionerrorsppm.AddRange(bandyionlist.Where(a => a.yionerror != 100).Select(a => a.yionPPM).ToList());

        //            bandyionerrorsppm = bandyionerrorsppm.Where(a => a != 0).ToList();

        //            bandyionerrors = bandyionerrors.Where(a => a != 0).ToList();

        //            if (bandyionerrors.Any())
        //            {
        //                bandyionerrormedian = bandyionerrors.Median();
        //                bandyionerrormedian = bandyionerrorsppm.Median();
        //                double minmedianvalue = bandyionerrormedian * 0.25;
        //                double maxmedianvalue = bandyionerrormedian * 2.00;

        //                bandyionerrors = bandyionerrorsppm.Select(a => Math.Abs(a - bandyionerrormedian)).Where(a => a != 0.0).ToList();

        //                possiblelist[ii].NumberofErrorsNearMedian = ((double)bandyionerrorsppm.Where(a => a >= minmedianvalue && a <= maxmedianvalue).Count() / (double)bandyionerrorsppm.Count) * 100;

        //                possiblelist[ii].MedianofErrors = bandyionerrormedian;

        //                bandyionerrorsppm = bandyionerrorsppm.Select(a => Math.Abs(a - bandyionerrormedian)).Where(a => a != 0.0).ToList();
        //                possiblelist[ii].MedianofMedianErrorslist = bandyionerrors;
        //                possiblelist[ii].MedianofMedianErrorslist = bandyionerrorsppm;
        //                //bandyionerrors = bandyionerrors.Where(a => a != 0).Select(a => Math.Abs(a - bandyionerrormedian)).ToList();
        //                if (bandyionerrors.Any())
        //                {
        //                    var medianofmedians = bandyionerrors.Median();
        //                    possiblelist[ii].MedianofMedianbandyionerrors = Math.Round(medianofmedians, 5);/// Math.Round(bandyionerrors.Select(a => Math.Abs(a - bandyionerrormedian)).Median(), 5);
        //                    //possiblelist[ii].BandYIonPercentDividebyMedianofMedianbandyionerrors = possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofMedianbandyionerrors;
        //                    possiblelist[ii].BandYIonPercentDividebyMedianofMedianbandyionerrors = possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofErrors;

        //                    possiblelist[ii].RootmeanSquareError = RootMeanSquareError(possiblelist[ii].MedianofMedianErrorslist);

        //                    possiblelist[ii].AverageError = possiblelist[ii].MedianofMedianErrorslist.Average();

        //                    possiblelist[ii].AverageError = (possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofMedianErrorslist.Average());

        //                    //possiblelist[ii].AverageError = possiblelist[ii].BandYIonPercent * possiblelist[ii].AverageError;
        //                }
        //            }
        //        }

        //        if (!(bandyionerrors.Any()))
        //        {
        //            bandyionerrors.Add(0.0);
        //        }

        //        Allmods.AddRange(possiblelist);
        //        Allmods = SortModsbyPriority(Allmods, 50, HasKnownMods);
        //        possiblelist = new List<PossibleMods>();

        //        if (i == initialendp - 1)
        //        {
        //            if (i == 0)
        //            {
        //                previousstring = previousstring + "[" + startgap + "]" + sequence.Substring(1, finalstartp);
        //            }
        //            else
        //            {
        //                previousstring = previousstring + aa + "[" + startgap + "]" + sequence.Substring(i + 1, finalstartp - (i + 1) + 1);
        //            }
        //            i = finalstartp;
        //            if (startgap >= 1.0)
        //            {
        //                afterstartgap = true;
        //            }
        //            tempgap = endgap;
        //            tempallmonomasses = lastallmonons;
        //            finalend = true;
        //            continue;
        //        }
        //        if (i != 0)
        //        {
        //            previousstring = previousstring + aa;
        //        }
        //    }

        //    List<SequenceandMedianofmedianerrors> mm = new List<SequenceandMedianofmedianerrors>();
        //    if (Allmods.Any())
        //    {
        //        Allmods = Allmods.OrderByDescending(a => a.BandYIonPercentDividebyMedianofMedianbandyionerrors).ToList();

        //        Allmods = Allmods.GroupBy(a => a.Sequence).Select(a => a.Firstt()).ToList();

        //        for (int i = 0; i < Allmods.Count; i++)
        //        {
        //            mm.Add(new SequenceandMedianofmedianerrors
        //            {
        //                Mod = Allmods[i].Sequence,
        //                MedianofMedian = Allmods[i].MedianofMedianbandyionerrors,
        //                MedianofMedianErrorsdivision = Allmods[i].BandYIonPercentDividebyMedianofMedianbandyionerrors
        //            });
        //        }
        //    }

        //    if (Allmods.Any())
        //    {
        //        if (!isforward)
        //        {
        //            var firstallmods = SortModsbyPriority(Allmods, 50, HasKnownMods).Firstt();
        //            sequencewithmods = firstallmods.Sequence;
        //        }
        //        else
        //        {
        //            var firstallmods = SortModsbyPriority(Allmods, 50, HasKnownMods).Firstt();
        //            sequencewithmods = firstallmods.Sequence;
        //        }
        //        psdms = SortModsbyPriority(Allmods, 50, HasKnownMods);
        //    }
        //    else
        //    {
        //        sequencewithmods = string.Empty;
        //    }

        //    return sequencewithmods;
        //}


        /// <summary>
        /// Checks all the possible modifications.
        /// Loops through all the AminoAcids in the sequence and generates all the modifications
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="startmonomass"></param>
        /// <param name="endmonomass"></param>
        /// <param name="monomasslist"></param>
        /// <param name="gap"></param>
        /// <param name="parentmass"></param>
        /// <param name="isend"></param>
        /// <param name="originalsequence"></param>
        /// <param name="isforward"></param>
        /// <param name="startp"></param>
        /// <param name="endp"></param>
        /// <param name="islast"></param>
        /// <param name="aminoacids"></param>
        /// <returns></returns>
        //public static string CheckforMods4(string sequence, double initialstartmonomass, double initialendmonomass, double finalstartmonomass, double finalendmonomass, Dictionary<double, double> monomasslist, double startgap, double endgap, double parentmass, bool isend, string originalsequence, bool isforward, int initialstartp, int initialendp, int finalstartp, int finalendp, bool islast, SortedList<char, double> aminoacids, bool HasKnownMods, ref List<PossibleMods> psdms)
        //{
        //    string sequencewithmods = string.Empty;

        //    string sequencewithoutmods = RemoveAllMods(sequence);

        //    bool hasinitialgap = (initialstartmonomass != initialendmonomass); // If both monomasses are same then there is no gap in the beginning of the sequence. 

        //    bool hasfinalgap = (finalstartmonomass != finalendmonomass); // If both monomasses are same then there is no gap in the end of the sequence.
        //    double tempgap = Math.Round(startgap, 4);

        //    bool breakthegap = false;

        //    bool noinitialgap = false;

        //    if (initialstartp == initialendp)
        //    {
        //        noinitialgap = true;
        //    }


        //    if (!hasfinalgap) // If it doesn't have a gap in the end it should change the value of the finalstartp
        //    {
        //        finalstartp = sequence.Length - 1;
        //        breakthegap = true;

        //    }

        //    if (!hasinitialgap) // If there is no gap in the start it should change the value of the startp
        //    {
        //        initialstartp = finalstartp;
        //        tempgap = Math.Round(endgap, 4);
        //        initialstartmonomass = finalstartmonomass;
        //        //initialendp = finalendp;
        //        breakthegap = true;
        //    }

        //    if (sequencewithoutmods.Length != sequence.Length && hasfinalgap) // If the SequenceWithoutMods is not same the OriginalSequence which means there are mods and need to change the Indexes for InitialGap and FinalGap
        //    {
        //        int tempfinalend = finalendp;
        //        int tempfinalstartp = finalstartp;

        //        finalendp = finalendp + (sequence.Length - sequencewithoutmods.Length);
        //        finalstartp = finalstartp + (sequence.Length - sequencewithoutmods.Length);
        //        if (finalstartp > sequence.Length)
        //        {
        //            finalendp = tempfinalend;
        //            finalstartp = tempfinalstartp;
        //        }
        //    }

        //    List<string> sequencewithmodslist = new List<string>();

        //    var firstallmonos = monomasslist.Where(a => (a.Key <= initialendmonomass + 1.0) && (a.Key >= initialstartmonomass + 1.0)).OrderBy(a => a.Key).Select(a => a.Key).ToList(); //Only look for amino acids within the range.

        //    var lastallmonons = monomasslist.Where(a => (a.Key >= finalstartmonomass + 1.0) && (a.Key <= finalendmonomass + 1.0)).OrderBy(a => a.Key).Select(a => a.Key).ToList();

        //    lastallmonons.AddRange(lastallmonons.Select(a => (parentmass - a)).ToList()); //Add the reverse amino acids

        //    firstallmonos.AddRange(firstallmonos.Select(a => (parentmass - a)).ToList()); //Add the reverse amino acids

        //    lastallmonons.Add(0);
        //    lastallmonons.Add(Molecules.Water);
        //    lastallmonons.Add(parentmass);
        //    lastallmonons.Add(parentmass - Molecules.Water);


        //    firstallmonos.Add(0);
        //    firstallmonos.Add(Molecules.Water);
        //    firstallmonos.Add(parentmass);
        //    firstallmonos.Add(parentmass - Molecules.Water);

        //    double currentmonomass = initialstartmonomass;
        //    double currentgap = startgap;
        //    double reversegap = startgap;
        //    double currentsequencelength = 0.0;
        //    List<PossibleMods> possiblelist = new List<PossibleMods>();
        //    List<double> medianoferrors = new List<double>();

        //    int count = 0;
        //    int reversecount = 0;

        //    int monomasscount = 0;
        //    int reversemonomasscount = 0;

        //    int tempcount = 0;

        //    bool firstterm = true;
        //    string tempsequencewithmods = string.Empty;

        //    int sequencecount = 0;
        //    bool lastterm = false;
        //    int tempmonomasscount = 0;
        //    int Allmodscount = 0;

        //    List<PossibleMods> Allmods = new List<PossibleMods>();
        //    string aa = string.Empty;
        //    List<PossibleMods> temppossiblelist = new List<PossibleMods>();
        //    string previousstring = string.Empty;

        //    previousstring = sequence.Substring(0, initialstartp);

        //    double maxmodmass = Convert.ToDouble(App.AllValidationModifications.MaxBy(a => a.Mass).Mass);

        //    double maxgap = currentgap;

        //    double maxaminoacidmass = aminoacids.MaxBy(a => a.Value).Value;

        //    List<CalculateBandYIons.BYIons> bandyionlist = new List<CalculateBandYIons.BYIons>();

        //    firstallmonos = firstallmonos.OrderBy(a => a).ToList();

        //    List<double> XvalueMonos = new List<double>();

        //    XvalueMonos = monomasslist.Select(a => a.Key).ToList();

        //    List<double> tempallmonomasses = new List<double>();

        //    tempallmonomasses = firstallmonos;

        //    string tempvalidationsequence = string.Empty;

        //    bool finalend = false;
        //    bool nofinalgap = false;
        //    bool afterstartgap = false;
        //    if (initialstartp == initialendp) ///If there is no initial amino acids then need to change
        //    {
        //        if (finalstartp == finalendp) /// If there is no initial amino acids and no final amino acids then need to change
        //        {
        //            if (hasinitialgap && hasfinalgap)
        //            {
        //                previousstring = sequence.Substring(0, 1) + "[" + startgap + "]" + sequence.Substring(1, finalendp - 1) + "[" + endgap + "]";
        //            }
        //            else if (hasfinalgap)
        //            {
        //                previousstring = sequence + "[" + endgap + "]";
        //            }
        //            noinitialgap = true;
        //        }
        //        else
        //        {
        //            initialstartp = finalstartp;
        //            if (hasinitialgap)
        //            {
        //                previousstring = sequence.Substring(0, 1) + "[" + startgap + "]" + sequence.Substring(1, initialstartp - 1);
        //                tempgap = endgap;
        //            }
        //            else
        //            {
        //                previousstring = sequence.Substring(0, initialstartp);
        //            }
        //            noinitialgap = true;
        //        }
        //        //previousstring = sequence
        //    }
        //    for (int i = initialstartp; i < sequence.Length; i++)
        //    {
        //        aa = sequence[i].ToString();

        //        temppossiblelist.Clear();
        //        if (i == initialstartp && hasinitialgap && !noinitialgap)
        //        {
        //            previousstring = previousstring + aa;
        //        }
        //        //jlkjlk j
        //        currentmonomass = initialstartmonomass + CalculateBandYIons.sequencelengthwithmodifications(previousstring);

        //        int numberofmods = NumberofMods(previousstring);

        //        temppossiblelist.Add(new PossibleMods()
        //        {
        //            Sequence = previousstring,
        //            CurrentGap = tempgap,
        //            hasremainingstartgap = afterstartgap,
        //            CurrentMonomass = currentmonomass,
        //            HasMonoMass = false,
        //            ModGap = 0,
        //            MonosCount = 0,
        //            StartMonomass = initialstartmonomass,
        //            NumberofMods = numberofmods
        //        });

        //        if (i == 0 && isend)
        //        {
        //            if (isforward && initialstartp == 0) //If the sequence is forward and this is the first AminoAcid. Nterm
        //            {
        //                possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist, false, true, false, ((initialendp - initialstartp == 1) ? false : true));
        //            }
        //            else
        //            {
        //                possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist);
        //            }
        //        }
        //        else if (i == sequence.Length - 1 && isend && !isforward && initialstartp == 0) //If the sequence is reverse and first AminoAcid. Nterm . If it has only one term then don't 
        //        {
        //            possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist, false, true, true, ((initialendp - initialstartp == 1) ? false : true));
        //        }
        //        else
        //        {
        //            possiblelist = PossiblemodSequences(tempallmonomasses, aa, false, temppossiblelist);
        //        }

        //        if (initialendp != 0 && !noinitialgap)
        //        {
        //            for (int j = i + 1; j < sequence.Length; j++)
        //            {
        //                if (j == initialendp)
        //                {
        //                    foreach (var pbs in possiblelist)
        //                    {
        //                        if (pbs.CurrentGap > 0.1)
        //                        {
        //                            pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (hasfinalgap ? (sequence.Substring(j, finalstartp - (j))) : (sequence.Substring(j, sequence.Length - j)));
        //                            pbs.NumberofMods = pbs.NumberofMods + 1;
        //                        }
        //                        else
        //                        {
        //                            pbs.Sequence = pbs.Sequence + (hasfinalgap ? (sequence.Substring(j, finalstartp - (j))) : (sequence.Substring(j, sequence.Length - j)));
        //                        }
        //                        pbs.CurrentMonomass = finalstartmonomass;
        //                        pbs.CurrentGap = endgap;
        //                    }
        //                    j = finalstartp;
        //                    tempallmonomasses = lastallmonons;
        //                    if (breakthegap) break;
        //                    //if (!hasinitialgap) break;
        //                }
        //                //if (j > initialendp && j < finalstartp)
        //                //{
        //                //    break;
        //                //}
        //                string bb = sequence[j].ToString();

        //                if (isend && j == sequence.Length - 1)
        //                {
        //                    possiblelist = PossiblemodSequences(tempallmonomasses, bb, true, possiblelist, true);
        //                }
        //                else
        //                {
        //                    possiblelist = PossiblemodSequences(tempallmonomasses, bb, false, possiblelist);
        //                }

        //                if (j == initialendp - 1)
        //                {
        //                    foreach (var pbs in possiblelist)
        //                    {
        //                        if (pbs.CurrentGap > 0.1)
        //                        {
        //                            pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (hasfinalgap ? (sequence.Substring(j + 1, finalstartp - (j + 1))) : (sequence.Substring(j + 1, finalstartp - (j)))); /// +"From here";
        //                            pbs.NumberofMods = pbs.NumberofMods + 1;
        //                        }
        //                        else
        //                        {
        //                            pbs.Sequence = pbs.Sequence + (hasfinalgap ? (sequence.Substring(j + 1, finalstartp - (j + 1))) : (sequence.Substring(j + 1, finalstartp - (j))));
        //                        }
        //                        pbs.CurrentMonomass = finalstartmonomass;
        //                        pbs.CurrentGap = endgap;
        //                    }
        //                    j = finalstartp - 1;
        //                    tempallmonomasses = lastallmonons;
        //                    if (breakthegap) break;
        //                }
        //            }
        //        }
        //        else if (!noinitialgap)
        //        {
        //            foreach (var pbs in possiblelist)
        //            {
        //                int lengthvalue = RemoveAllMods(pbs.Sequence).Length;
        //                if (pbs.CurrentGap > 0.1 && initialendp != 0)
        //                {
        //                    pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (sequence.Substring(lengthvalue, sequence.Length - lengthvalue));
        //                    pbs.CurrentGap = 0;
        //                    pbs.NumberofMods = pbs.NumberofMods + 1;
        //                }
        //                else if (pbs.CurrentGap > 0.1)
        //                {
        //                    pbs.Sequence = pbs.Sequence + (sequence.Substring(lengthvalue, sequence.Length - lengthvalue));
        //                    pbs.CurrentGap = endgap;
        //                }
        //                pbs.CurrentMonomass = finalstartmonomass;
        //            }
        //        }
        //        else if (noinitialgap)
        //        {
        //            foreach (var pbs in possiblelist)
        //            {
        //                if (pbs.CurrentGap > 0.1)
        //                {
        //                    pbs.Sequence = pbs.Sequence + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (sequence.Substring(i + 1, sequence.Length - (i + 1)));
        //                    pbs.CurrentGap = 0;
        //                    pbs.NumberofMods = pbs.NumberofMods + 1;
        //                }
        //                else
        //                {
        //                    pbs.Sequence = pbs.Sequence + sequence.Substring(i + 1, sequence.Length - (i + 1));
        //                    //pbs.ModGap = 0;
        //                    //jhkljlk jklj; jlkj l;
        //                    //pbs.CurrentGap = endgap;
        //                }
        //                pbs.CurrentMonomass = finalstartmonomass;
        //            }
        //        }
        //        if (!finalend)
        //            tempallmonomasses = firstallmonos;
        //        possiblelist = possiblelist.Where(a => Math.Abs(a.ModGap) <= Math.Abs(tempgap)).ToList();

        //        Allmods = filteredpossiblemods(possiblelist, Allmods, HasKnownMods, XvalueMonos, parentmass, aminoacids, monomasslist, bandyionlist, islast, isforward, isend, initialstartp);

        //        possiblelist = new List<PossibleMods>();

        //        if (i == initialendp - 1)
        //        {
        //            if (i == 0)
        //            {
        //                previousstring = previousstring + "[" + startgap + "]" + sequence.Substring(1, finalstartp);
        //            }
        //            else
        //            {
        //                previousstring = previousstring + aa + "[" + startgap + "]" + sequence.Substring(i + 1, finalstartp - (i + 1) + 1);
        //            }
        //            i = finalstartp;
        //            if (startgap >= 1.0)
        //            {
        //                afterstartgap = true;
        //            }
        //            tempgap = endgap;
        //            tempallmonomasses = lastallmonons;
        //            finalend = true;
        //            continue;
        //        }
        //        if (i != 0)
        //        {
        //            previousstring = previousstring + aa;
        //        }
        //    }

        //    List<SequenceandMedianofmedianerrors> mm = new List<SequenceandMedianofmedianerrors>();
        //    if (Allmods.Any())
        //    {
        //        Allmods = Allmods.OrderByDescending(a => a.BandYIonPercentDividebyMedianofMedianbandyionerrors).ToList();

        //        Allmods = Allmods.GroupBy(a => a.Sequence).Select(a => a.Firstt()).ToList();

        //        for (int i = 0; i < Allmods.Count; i++)
        //        {
        //            mm.Add(new SequenceandMedianofmedianerrors
        //            {
        //                Mod = Allmods[i].Sequence,
        //                MedianofMedian = Allmods[i].MedianofMedianbandyionerrors,
        //                MedianofMedianErrorsdivision = Allmods[i].BandYIonPercentDividebyMedianofMedianbandyionerrors
        //            });
        //        }
        //    }

        //    if (Allmods.Any())
        //    {
        //        if (!isforward)
        //        {
        //            var firstallmods = SortModsbyPriority(Allmods, 50, HasKnownMods).Firstt();
        //            sequencewithmods = firstallmods.Sequence;
        //        }
        //        else
        //        {
        //            var firstallmods = SortModsbyPriority(Allmods, 50, HasKnownMods).Firstt();
        //            sequencewithmods = firstallmods.Sequence;
        //        }
        //        psdms = SortModsbyPriority(Allmods, 50, HasKnownMods);
        //    }
        //    else
        //    {
        //        sequencewithmods = string.Empty;
        //    }

        //    return sequencewithmods;
        //}


        /// <summary>
        /// Checks all the possible modifications.
        /// Loops through all the AminoAcids in the sequence and generates all the modifications
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="startmonomass"></param>
        /// <param name="endmonomass"></param>
        /// <param name="monomasslist"></param>
        /// <param name="gap"></param>
        /// <param name="parentmass"></param>
        /// <param name="isend"></param>
        /// <param name="originalsequence"></param>
        /// <param name="isforward"></param>
        /// <param name="startp"></param>
        /// <param name="endp"></param>
        /// <param name="islast"></param>
        /// <param name="aminoacids"></param>
        /// <returns></returns>
        public static string CheckforMods2(string sequence, double initialstartmonomass, double initialendmonomass, double finalstartmonomass, double finalendmonomass, Dictionary<double, double> monomasslist, double startgap, double endgap, double parentmass, bool isend, string originalsequence, bool isforward, int initialstartp, int initialendp, int finalstartp, int finalendp, bool islast, SortedList<char, double> aminoacids, bool HasKnownMods, ref List<PossibleMods> psdms, string activation = "CID")
        {
            string sequencewithmods = string.Empty;

            string sequencewithoutmods = RemoveAllMods(sequence);

            bool hasinitialgap = (initialstartmonomass != initialendmonomass); // If both monomasses are same then there is no gap in the beginning of the sequence. 

            bool hasfinalgap = (finalstartmonomass != finalendmonomass); // If both monomasses are same then there is no gap in the end of the sequence.
            double tempgap = Math.Round(startgap, 4);

            bool breakthegap = false;

            bool noinitialgap = false;

            if (initialstartp == initialendp)
            {
                noinitialgap = true;
            }


            if (!hasfinalgap) // If it doesn't have a gap in the end it should change the value of the finalstartp
            {
                finalstartp = sequence.Length - 1;
                breakthegap = true;

            }

            if (!hasinitialgap) // If there is no gap in the start it should change the value of the startp
            {
                //initialstartp = finalstartp;
                tempgap = Math.Round(endgap, 4);
                initialstartmonomass = finalstartmonomass;
                //initialendp = finalendp;
                breakthegap = true;
            }

            if (sequencewithoutmods.Length != sequence.Length && hasfinalgap) // If the SequenceWithoutMods is not same the OriginalSequence which means there are mods and need to change the Indexes for InitialGap and FinalGap
            {
                int tempfinalend = finalendp;
                int tempfinalstartp = finalstartp;

                finalendp = finalendp + (sequence.Length - sequencewithoutmods.Length);
                finalstartp = finalstartp + (sequence.Length - sequencewithoutmods.Length);
                if (finalendp > sequence.Length)
                {
                    finalendp = tempfinalend;
                    finalstartp = tempfinalstartp;
                }
            }

            List<string> sequencewithmodslist = new List<string>();

            var firstallmonos = monomasslist.Where(a => (a.Key <= initialendmonomass + 1.0) && (a.Key >= initialstartmonomass + 1.0)).OrderBy(a => a.Key).Select(a => a.Key).ToList(); //Only look for amino acids within the range.

            var lastallmonons = monomasslist.Where(a => (a.Key >= finalstartmonomass + 1.0) && (a.Key <= finalendmonomass + 1.0)).OrderBy(a => a.Key).Select(a => a.Key).ToList();

            lastallmonons.AddRange(lastallmonons.Select(a => (parentmass - a)).ToList()); //Add the reverse amino acids

            firstallmonos.AddRange(firstallmonos.Select(a => (parentmass - a)).ToList()); //Add the reverse amino acids

            lastallmonons.Add(0);
            lastallmonons.Add(Molecules.Water);
            lastallmonons.Add(parentmass);
            lastallmonons.Add(parentmass - Molecules.Water);


            firstallmonos.Add(0);
            firstallmonos.Add(Molecules.Water);
            firstallmonos.Add(parentmass);
            firstallmonos.Add(parentmass - Molecules.Water);

            double currentmonomass = initialstartmonomass;
            double currentgap = startgap;
            double reversegap = startgap;
            double currentsequencelength = 0.0;
            List<PossibleMods> possiblelist = new List<PossibleMods>();
            List<double> medianoferrors = new List<double>();

            int count = 0;
            int reversecount = 0;

            int monomasscount = 0;
            int reversemonomasscount = 0;

            int tempcount = 0;

            bool firstterm = true;
            string tempsequencewithmods = string.Empty;

            int sequencecount = 0;
            bool lastterm = false;
            int tempmonomasscount = 0;
            int Allmodscount = 0;

            List<PossibleMods> Allmods = new List<PossibleMods>();
            string aa = string.Empty;
            List<PossibleMods> temppossiblelist = new List<PossibleMods>();
            string previousstring = string.Empty;

            previousstring = sequence.Substring(0, initialstartp);

            double maxmodmass = Convert.ToDouble(App.AllValidationModifications.MaxBy(a => a.Mass).Mass);

            double maxgap = currentgap;

            double maxaminoacidmass = aminoacids.MaxBy(a => a.Value).Value;

            List<CalculateBYCZIons.BYCZIons> bandyionlist = new List<CalculateBYCZIons.BYCZIons>();

            firstallmonos = firstallmonos.OrderBy(a => a).ToList();

            List<double> XvalueMonos = new List<double>();

            XvalueMonos = monomasslist.Select(a => a.Key).ToList();

            List<double> tempallmonomasses = new List<double>();

            tempallmonomasses = firstallmonos;

            string tempvalidationsequence = string.Empty;

            bool finalend = false;
            bool nofinalgap = false;
            bool afterstartgap = false;

            int tempinitialstartp = initialstartp;

            int tempinitialendp = initialendp;

            if (initialstartp == initialendp) ///If there is no initial amino acids then need to change
            {
                if (finalstartp == finalendp) /// If there is no initial amino acids and no final amino acids then need to change
                {
                    if (hasinitialgap && hasfinalgap)
                    {
                        previousstring = sequence.Substring(0, 1) + "[" + startgap + "]" + sequence.Substring(1, finalendp - 1) + "[" + endgap + "]";
                    }
                    else if (hasfinalgap)
                    {
                        previousstring = sequence + "[" + endgap + "]";
                    }
                    noinitialgap = true;
                }
                else
                {

                    if (hasinitialgap)
                    {
                        previousstring = "[" + startgap + "]" + sequence.Substring(0, 1) + sequence.Substring(1, (finalstartp) - 1);
                        tempinitialstartp = finalstartp = finalstartp + Convert.ToString(startgap).Length + 2;
                        tempinitialendp = finalendp = finalendp + Convert.ToString(startgap).Length + 2;
                        sequence = "[" + startgap + "]" + sequence;
                        tempgap = endgap;
                    }
                    else
                    {
                        previousstring = sequence.Substring(0, finalstartp);
                        tempinitialstartp = finalstartp;
                        tempinitialendp = finalendp;
                        //previousstring = sequence.Substring(0, initialstartp);
                    }
                    noinitialgap = true;
                }
                //previousstring = sequence
            }
            List<PossibleMods> psmdslst = new List<PossibleMods>();

            for (int i = tempinitialstartp; i < tempinitialendp; i++)
            {
                aa = sequence[i].ToString();

                temppossiblelist.Clear();
                if (i == initialstartp && hasinitialgap && !noinitialgap)
                {
                    previousstring = previousstring + aa;
                }
                //jlkjlk j
                currentmonomass = initialstartmonomass + CalculateBYCZIons.sequencelengthwithmodifications(previousstring);

                int numberofmods = NumberofMods(previousstring);

                temppossiblelist.Add(new PossibleMods()
                {
                    Sequence = previousstring,
                    CurrentGap = tempgap,
                    hasremainingstartgap = afterstartgap,
                    CurrentMonomass = currentmonomass,
                    HasMonoMass = false,
                    ModGap = 0,
                    MonosCount = 0,
                    StartMonomass = initialstartmonomass,
                    NumberofMods = numberofmods,
                    Currentposition = i
                });

                //jdklas dsaj; kasljdkljasdkl; 
                /// hkj hjkh 

                if (i == 0 && isend)
                {
                    if (isforward && initialstartp == 0) //If the sequence is forward and this is the first AminoAcid. Nterm
                    {
                        possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist, false, true, false, ((initialendp - initialstartp == 1) ? false : true));
                    }
                    else
                    {
                        possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist);
                    }
                }
                else if (i == sequence.Length - 1 && isend && !isforward && initialstartp == 0) //If the sequence is reverse and first AminoAcid. Nterm . If it has only one term then don't 
                {
                    possiblelist = PossiblemodSequences(tempallmonomasses, aa, true, temppossiblelist, false, true, true, ((initialendp - initialstartp == 1) ? false : true));
                }
                else
                {
                    possiblelist = PossiblemodSequences(tempallmonomasses, aa, false, temppossiblelist);
                }

                if (initialendp != 0 && !noinitialgap)
                {
                    if (hasfinalgap)
                    {
                        for (int j = initialendp; j < sequence.Length; j++)
                        {
                            if (j == initialendp)
                            {
                                foreach (var pbs in possiblelist)
                                {
                                    if (pbs.CurrentGap > 0.1)
                                    {
                                        pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (hasfinalgap ? (sequence.Substring(i + 1, finalstartp - (i + 1))) : (sequence.Substring(i + 1, sequence.Length - i + 1)));
                                        pbs.modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4;
                                        pbs.NumberofMods = pbs.NumberofMods + 1;
                                    }
                                    else
                                    {
                                        pbs.Sequence = pbs.Sequence + (hasfinalgap ? (sequence.Substring(i + 1, finalstartp - (i + 1))) : (sequence.Substring((i + 1), sequence.Length - (i + 1))));
                                    }
                                    pbs.CurrentMonomass = finalstartmonomass;
                                    pbs.CurrentGap = endgap;
                                }
                                //j = finalstartp;
                                tempallmonomasses = lastallmonons;
                                j = finalstartp;
                                if (breakthegap) break;
                                //if (!hasinitialgap) break;
                            }
                            string bb = sequence[j].ToString();

                            if (isend && j == sequence.Length - 1)
                            {
                                possiblelist = PossiblemodSequences(tempallmonomasses, bb, true, possiblelist, true);
                                foreach (var psbmd in possiblelist)
                                {
                                    PossibleMods psdd = new PossibleMods();

                                    if (Math.Abs(psbmd.CurrentGap) > 0.1)
                                    {
                                        //psmdslst.Add(psbmd);

                                        psmdslst.Add(new PossibleMods()
                                        {
                                            Sequence = psbmd.Sequence.Substring(0, finalstartp + psbmd.modificationlength)
                                                        + "("
                                                        + psbmd.Sequence.Substring(finalstartp + psbmd.modificationlength, psbmd.Sequence.Length - (finalstartp + psbmd.modificationlength)) +
                                                        ")" +
                                                        "[" + Math.Round(psbmd.CurrentGap, 4) + "]"
                                                        + sequence.Substring(j + 1, sequence.Length - (j + 1)),
                                            CurrentGap = 0,
                                            hasremainingstartgap = afterstartgap,
                                            CurrentMonomass = psbmd.CurrentMonomass,
                                            HasMonoMass = psbmd.HasMonoMass,
                                            ModGap = psbmd.ModGap,
                                            MonosCount = psbmd.MonosCount,
                                            StartMonomass = psbmd.StartMonomass,
                                            NumberofMods = psbmd.NumberofMods,
                                            modificationlength = psbmd.modificationlength + Convert.ToString(Math.Round(psbmd.CurrentGap, 4)).Length + 4,
                                            Currentposition = psbmd.Currentposition,
                                            Currentpositionatend = j,
                                            TotalKnownMods = psbmd.TotalKnownMods,
                                        });
                                        //psbmd.Sequence = psbmd.Sequence.Substring(0, finalstartp) + "(" + psbmd.Sequence.Substring(finalstartp, psbmd.Sequence.Length - finalstartp) + ")" + "[" + psbmd.CurrentGap + "]";
                                    }
                                    else
                                    {
                                        //psbmd.Sequence = psbmd.Sequence + sequence.Substring(j, sequence.Length - j);
                                        psmdslst.Add(new PossibleMods()
                                        {
                                            Sequence = psbmd.Sequence + sequence.Substring(j, sequence.Length - j), /// psbmd.Sequence.Substring(0, finalstartp) + "(" + psbmd.Sequence.Substring(finalstartp, psbmd.Sequence.Length - finalstartp) + ")" + "[" + psbmd.CurrentGap + "]",
                                            CurrentGap = 0,
                                            hasremainingstartgap = afterstartgap,
                                            CurrentMonomass = psbmd.CurrentMonomass,
                                            HasMonoMass = psbmd.HasMonoMass,
                                            ModGap = psbmd.ModGap,
                                            MonosCount = psbmd.MonosCount,
                                            StartMonomass = psbmd.StartMonomass,
                                            NumberofMods = psbmd.NumberofMods,
                                            Currentposition = psbmd.Currentposition,
                                            Currentpositionatend = j,
                                            TotalKnownMods = psbmd.TotalKnownMods,
                                        });
                                    }
                                }
                            }
                            else
                            {
                                possiblelist = PossiblemodSequences(tempallmonomasses, bb, false, possiblelist);
                                foreach (var psbmd in possiblelist)
                                {
                                    PossibleMods psdd = new PossibleMods();

                                    if (psbmd.CurrentGap > 0)
                                    {
                                        //psmdslst.Add(psbmd);

                                        psmdslst.Add(new PossibleMods()
                                        {
                                            Sequence = psbmd.Sequence.Substring(0, finalstartp + psbmd.modificationlength)
                                                        + "("
                                                        + psbmd.Sequence.Substring(finalstartp + psbmd.modificationlength, psbmd.Sequence.Length - (finalstartp + psbmd.modificationlength)) +
                                                        ")" +
                                                        "[" + Math.Round(psbmd.CurrentGap, 4) + "]"
                                                        + sequence.Substring(j + 1, sequence.Length - (j + 1)),
                                            CurrentGap = 0,
                                            hasremainingstartgap = afterstartgap,
                                            CurrentMonomass = psbmd.CurrentMonomass,
                                            HasMonoMass = psbmd.HasMonoMass,
                                            ModGap = psbmd.ModGap,
                                            MonosCount = psbmd.MonosCount,
                                            StartMonomass = psbmd.StartMonomass,
                                            NumberofMods = psbmd.NumberofMods,
                                            modificationlength = psbmd.modificationlength + Convert.ToString(Math.Round(psbmd.CurrentGap, 4)).Length + 4,
                                            Currentposition = psbmd.Currentposition,
                                            Currentpositionatend = j,
                                            TotalKnownMods = psbmd.TotalKnownMods,
                                        });
                                        //psbmd.Sequence = psbmd.Sequence.Substring(0, finalstartp) + "(" + psbmd.Sequence.Substring(finalstartp, psbmd.Sequence.Length - finalstartp) + ")" + "[" + psbmd.CurrentGap + "]";
                                    }
                                    else
                                    {
                                        //psbmd.Sequence = psbmd.Sequence + sequence.Substring(j, sequence.Length - j);
                                        psmdslst.Add(new PossibleMods()
                                        {
                                            Sequence = psbmd.Sequence + sequence.Substring(j, sequence.Length - j), /// psbmd.Sequence.Substring(0, finalstartp) + "(" + psbmd.Sequence.Substring(finalstartp, psbmd.Sequence.Length - finalstartp) + ")" + "[" + psbmd.CurrentGap + "]",
                                            CurrentGap = 0,
                                            hasremainingstartgap = afterstartgap,
                                            CurrentMonomass = psbmd.CurrentMonomass,
                                            HasMonoMass = psbmd.HasMonoMass,
                                            ModGap = psbmd.ModGap,
                                            MonosCount = psbmd.MonosCount,
                                            StartMonomass = psbmd.StartMonomass,
                                            NumberofMods = psbmd.NumberofMods,
                                            Currentposition = psbmd.Currentposition,
                                            Currentpositionatend = j,
                                            TotalKnownMods = psbmd.TotalKnownMods
                                        });
                                    }
                                }
                            }

                            if (j == initialendp - 1)
                            {
                                foreach (var pbs in possiblelist)
                                {
                                    if (pbs.CurrentGap > 0.1)
                                    {
                                        psmdslst.Add(new PossibleMods()
                                            {
                                                Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (hasfinalgap ? (sequence.Substring(j + 1, finalstartp - (j + 1))) : (sequence.Substring(j + 1, finalstartp - (j)))),
                                                modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4,
                                                NumberofMods = pbs.NumberofMods + 1,
                                                CurrentGap = 0,
                                                hasremainingstartgap = afterstartgap,
                                                CurrentMonomass = pbs.CurrentMonomass,
                                                HasMonoMass = pbs.HasMonoMass,
                                                ModGap = pbs.ModGap,
                                                MonosCount = pbs.MonosCount,
                                                StartMonomass = pbs.StartMonomass,
                                                Currentposition = pbs.Currentposition,
                                                Currentpositionatend = j,
                                                TotalKnownMods = pbs.TotalKnownMods
                                            });

                                        //pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (hasfinalgap ? (sequence.Substring(j + 1, finalstartp - (j + 1))) : (sequence.Substring(j + 1, finalstartp - (j)))); /// +"From here";
                                        //pbs.modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4;
                                        //pbs.NumberofMods = pbs.NumberofMods + 1;
                                    }
                                    else
                                    {
                                        psmdslst.Add(new PossibleMods()
                                        {
                                            Sequence = pbs.Sequence + (hasfinalgap ? (sequence.Substring(j + 1, finalstartp - (j + 1))) : (sequence.Substring(j + 1, finalstartp - (j)))),
                                            modificationlength = pbs.modificationlength,
                                            NumberofMods = pbs.NumberofMods + 1,
                                            CurrentGap = 0,
                                            hasremainingstartgap = afterstartgap,
                                            CurrentMonomass = pbs.CurrentMonomass,
                                            HasMonoMass = pbs.HasMonoMass,
                                            ModGap = pbs.ModGap,
                                            MonosCount = pbs.MonosCount,
                                            StartMonomass = pbs.StartMonomass,
                                            Currentposition = pbs.Currentposition,
                                            Currentpositionatend = j,
                                            TotalKnownMods = pbs.TotalKnownMods
                                        });
                                        //pbs.Sequence = pbs.Sequence + (hasfinalgap ? (sequence.Substring(j + 1, finalstartp - (j + 1))) : (sequence.Substring(j + 1, finalstartp - (j))));
                                    }
                                    pbs.CurrentMonomass = finalstartmonomass;
                                    pbs.CurrentGap = endgap;
                                }
                                j = finalstartp - 1;
                                tempallmonomasses = lastallmonons;
                                if (breakthegap) break;
                            }
                        }
                    }
                    else if (!hasfinalgap)
                    {
                        foreach (var pbs in possiblelist)
                        {
                            if (pbs.CurrentGap > 0.1)
                            {
                                psmdslst.Add(new PossibleMods()
                                {
                                    Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + sequence.Substring(i + 1, sequence.Length - (i + 1)),
                                    modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4,
                                    CurrentGap = 0,
                                    hasremainingstartgap = afterstartgap,
                                    CurrentMonomass = pbs.CurrentMonomass,
                                    HasMonoMass = pbs.HasMonoMass,
                                    ModGap = pbs.ModGap,
                                    MonosCount = pbs.MonosCount,
                                    StartMonomass = pbs.StartMonomass,
                                    NumberofMods = pbs.NumberofMods + 1,
                                    Currentposition = pbs.Currentposition,
                                    Currentpositionatend = 0,
                                    TotalKnownMods = pbs.TotalKnownMods
                                });
                                //pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (hasfinalgap ? (sequence.Substring(i + 1, finalstartp - (i + 1))) : (sequence.Substring(i + 1, sequence.Length - i + 1)));
                                //pbs.modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4;
                                //pbs.NumberofMods = pbs.NumberofMods + 1;
                            }
                            else
                            {
                                psmdslst.Add(new PossibleMods()
                                {
                                    Sequence = pbs.Sequence + sequence.Substring((i + 1), sequence.Length - (i + 1)),
                                    modificationlength = pbs.modificationlength,
                                    CurrentGap = 0,
                                    hasremainingstartgap = afterstartgap,
                                    CurrentMonomass = pbs.CurrentMonomass,
                                    HasMonoMass = pbs.HasMonoMass,
                                    ModGap = pbs.ModGap,
                                    MonosCount = pbs.MonosCount,
                                    StartMonomass = pbs.StartMonomass,
                                    NumberofMods = pbs.NumberofMods + 1,
                                    Currentposition = pbs.Currentposition,
                                    Currentpositionatend = 0,
                                    TotalKnownMods = pbs.TotalKnownMods
                                });

                                //pbs.Sequence = pbs.Sequence + (hasfinalgap ? (sequence.Substring(i + 1, finalstartp - (i + 1))) : (sequence.Substring((i + 1), sequence.Length - (i + 1))));
                            }
                            //pbs.CurrentMonomass = finalstartmonomass;
                            //pbs.CurrentGap = endgap;
                        }
                        endgap = startgap;
                        //j = finalstartp;
                        //tempallmonomasses = lastallmonons;
                    }
                }
                else if (!noinitialgap)
                {
                    foreach (var pbs in possiblelist)
                    {
                        int lengthvalue = RemoveAllMods(pbs.Sequence).Length;
                        if (pbs.CurrentGap > 0.1 && initialendp != 0)
                        {
                            psmdslst.Add(new PossibleMods()
                            {
                                Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (sequence.Substring(lengthvalue, sequence.Length - lengthvalue)),
                                modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4,
                                NumberofMods = pbs.NumberofMods + 1,
                                CurrentGap = 0,
                                hasremainingstartgap = afterstartgap,
                                CurrentMonomass = pbs.CurrentMonomass,
                                HasMonoMass = pbs.HasMonoMass,
                                ModGap = pbs.ModGap,
                                MonosCount = pbs.MonosCount,
                                StartMonomass = pbs.StartMonomass,
                                Currentposition = pbs.Currentposition,
                                Currentpositionatend = i,
                                TotalKnownMods = pbs.TotalKnownMods
                            });

                            //pbs.Sequence = "(" + pbs.Sequence + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (sequence.Substring(lengthvalue, sequence.Length - lengthvalue));
                            //pbs.modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4;
                            //pbs.CurrentGap = 0;
                            //pbs.NumberofMods = pbs.NumberofMods + 1;
                        }
                        else if (pbs.CurrentGap > 0.1)
                        {
                            psmdslst.Add(new PossibleMods()
                            {
                                Sequence = pbs.Sequence + (sequence.Substring(lengthvalue, sequence.Length - lengthvalue)),
                                modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4,
                                NumberofMods = pbs.NumberofMods + 1,
                                CurrentGap = endgap,
                                hasremainingstartgap = afterstartgap,
                                CurrentMonomass = pbs.CurrentMonomass,
                                HasMonoMass = pbs.HasMonoMass,
                                ModGap = pbs.ModGap,
                                MonosCount = pbs.MonosCount,
                                StartMonomass = pbs.StartMonomass,
                                Currentposition = pbs.Currentposition,
                                Currentpositionatend = i,
                                TotalKnownMods = pbs.TotalKnownMods
                            });

                            //pbs.Sequence = pbs.Sequence + (sequence.Substring(lengthvalue, sequence.Length - lengthvalue));
                            //pbs.CurrentGap = endgap;
                        }
                        pbs.CurrentMonomass = finalstartmonomass;
                    }
                }
                else if (noinitialgap)
                {
                    foreach (var pbs in possiblelist)
                    {
                        if (Math.Abs(pbs.CurrentGap) > 0.1)
                        {
                            psmdslst.Add(new PossibleMods()
                            {
                                Sequence = pbs.Sequence.Substring(0, finalstartp) + "(" + pbs.Sequence.Substring(finalstartp, i + 1 - finalstartp) + ")" + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (sequence.Substring(i + 1, sequence.Length - (i + 1))),
                                modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4,
                                CurrentGap = 0,
                                NumberofMods = pbs.NumberofMods + 1,
                                CurrentMonomass = pbs.CurrentMonomass,
                                HasMonoMass = pbs.HasMonoMass,
                                hasremainingstartgap = afterstartgap,
                                ModGap = pbs.ModGap,
                                MonosCount = pbs.MonosCount,
                                StartMonomass = pbs.StartMonomass,
                                Currentposition = pbs.Currentposition,
                                Currentpositionatend = i,
                                TotalKnownMods = pbs.TotalKnownMods
                            });

                            //pbs.Sequence = pbs.Sequence + ("[" + Math.Round(pbs.CurrentGap, 4) + "]") + (sequence.Substring(i + 1, sequence.Length - (i + 1)));
                            //pbs.modificationlength = pbs.modificationlength + Convert.ToString(Math.Round(pbs.CurrentGap, 4)).Length + 4;
                            //pbs.CurrentGap = 0;
                            //pbs.NumberofMods = pbs.NumberofMods + 1;
                        }
                        else
                        {
                            psmdslst.Add(new PossibleMods()
                            {
                                Sequence = pbs.Sequence + sequence.Substring(i + 1, sequence.Length - (i + 1)),
                                modificationlength = pbs.modificationlength,
                                CurrentGap = 0,
                                NumberofMods = pbs.NumberofMods,
                                CurrentMonomass = pbs.CurrentMonomass,
                                HasMonoMass = pbs.HasMonoMass,
                                hasremainingstartgap = afterstartgap,
                                ModGap = pbs.ModGap,
                                MonosCount = pbs.MonosCount,
                                StartMonomass = pbs.StartMonomass,
                                Currentposition = pbs.Currentposition,
                                Currentpositionatend = i,
                                TotalKnownMods = pbs.TotalKnownMods
                            });
                            //pbs.Sequence = pbs.Sequence + sequence.Substring(i + 1, sequence.Length - (i + 1));
                        }
                        pbs.CurrentMonomass = finalstartmonomass;
                    }
                }
                if (!finalend)
                    tempallmonomasses = firstallmonos;

                possiblelist = psmdslst;

                possiblelist = possiblelist.Where(a => Math.Abs(a.ModGap) <= Math.Abs(endgap)).ToList();

                Allmods.AddRange(filteredpossiblemods(possiblelist, HasKnownMods, XvalueMonos, parentmass, aminoacids, monomasslist, bandyionlist, islast, isforward, isend, initialstartp, activation));

                possiblelist = new List<PossibleMods>();

                psmdslst = new List<PossibleMods>();

                if (i == initialendp - 1)
                {
                    if (i == 0)
                    {
                        previousstring = previousstring + "[" + startgap + "]" + sequence.Substring(1, finalstartp);
                    }
                    else
                    {
                        previousstring = previousstring + aa + "[" + startgap + "]" + sequence.Substring(i + 1, finalstartp - (i + 1) + 1);
                    }
                    i = finalstartp;
                    if (startgap >= 1.0)
                    {
                        afterstartgap = true;
                    }
                    tempgap = endgap;
                    tempallmonomasses = lastallmonons;
                    finalend = true;
                    continue;
                }
                if (i != 0)
                {
                    previousstring = previousstring + aa;
                }
            }

            List<SequenceandMedianofmedianerrors> mm = new List<SequenceandMedianofmedianerrors>();
            if (Allmods.Any())
            {
                Allmods = Allmods.OrderByDescending(a => a.BandYIonPercentDividebyMedianofMedianbandyionerrors).ThenByDescending(a => a.Currentposition).ToList();

                Allmods = Allmods.GroupBy(a => a.Sequence).Select(a => a.Firstt()).ToList();

                for (int i = 0; i < Allmods.Count; i++)
                {
                    mm.Add(new SequenceandMedianofmedianerrors
                    {
                        Mod = Allmods[i].Sequence,
                        MedianofMedian = Allmods[i].MedianofMedianbandyionerrors,
                        MedianofMedianErrorsdivision = Allmods[i].BandYIonPercentDividebyMedianofMedianbandyionerrors
                    });
                }
            }

            if (Allmods.Any())
            {
                if (!isforward)
                {
                    var firstallmods = SortModsbyPriority(Allmods, 50, HasKnownMods).Firstt();
                    sequencewithmods = firstallmods.Sequence;
                }
                else
                {
                    var firstallmods = SortModsbyPriority(Allmods, 50, HasKnownMods).Firstt();
                    sequencewithmods = firstallmods.Sequence;
                }
                if (Allmods.Any(a => a.TotalKnownMods >= 1))
                {
                    psdms = SortModsbyPriority(Allmods, 10, HasKnownMods);
                }
                else
                {
                    psdms.Clear();
                    psdms = SortModsbyPriority(Allmods, 10, HasKnownMods);
                }
            }
            else
            {
                sequencewithmods = string.Empty;
            }

            return sequencewithmods;
        }


        /// <summary>
        /// Filter the mods
        /// </summary>
        /// <param name="possiblelist"></param>
        /// <param name="Allmods"></param>
        /// <param name="HasKnownMods"></param>
        /// <param name="XvalueMonos"></param>
        /// <param name="parentmass"></param>
        /// <param name="aminoacids"></param>
        /// <param name="monomasslist"></param>
        /// <param name="bandyionlist"></param>
        /// <param name="islast"></param>
        /// <param name="isforward"></param>
        /// <param name="isend"></param>
        /// <param name="initialstartp"></param>
        /// <returns></returns>
        static List<PossibleMods> filteredpossiblemods(List<PossibleMods> possiblelist, bool HasKnownMods, List<double> XvalueMonos, double parentmass, SortedList<char, double> aminoacids, Dictionary<double, double> monomasslist, List<CalculateBYCZIons.BYCZIons> bandyionlist, bool islast, bool isforward, bool isend, int initialstartp, string activation = "CID")
        {
            List<double> bandyionerrors = new List<double>();
            double bandyionerrormedian = 0.0;
            double bandyionpercentage = 0.0;
            List<PossibleMods> Allmods = new List<PossibleMods>();
            Dictionary<int, double> listofbandyionpercentage = new Dictionary<int, double>();

            int psbcount = possiblelist.Count;

            XvalueMonos.Add(parentmass - Molecules.Water);

            XvalueMonos = XvalueMonos.OrderBy(a => a).ToList();

            ///Looping through all the possible mods
            for (int ii = 0; ii < psbcount; ii++)
            {
                if (!isforward)
                {
                    possiblelist[ii].Sequence = reversesequencewithmods(possiblelist[ii].Sequence);
                }
                //If there is any gap need to include that in the sequence
                if (Math.Abs(possiblelist[ii].CurrentGap) > 0.1)
                {
                    if (islast)
                    {
                        possiblelist[ii].Sequence = possiblelist[ii].Sequence + "[" + Math.Round(possiblelist[ii].CurrentGap, 4) + "]"; /// AddModtoSequenceAttheend(possiblelist[ii].Sequence, Math.Round(possiblelist[ii].CurrentGap, 2), finalstartp, finalendp);                            
                    }
                    else
                    {
                        possiblelist[ii].Sequence = AddModtoSequence(possiblelist[ii].Sequence, Math.Round(possiblelist[ii].CurrentGap, 2));
                    }
                    possiblelist[ii].CurrentGap = 0;
                }
                if (isforward)
                {
                    //bandyionlist = CalculateBandYIons.CalculateBandYIon(originalsequence.Substring(0, initialstartp) + (possiblelist[ii].Sequence) + originalsequence.Substring(initialendp, originalsequence.Length - (initialendp)), XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
                    bandyionlist = CalculateBYCZIons.CalculateBYCZIon(possiblelist[ii].Sequence, XvalueMonos, parentmass, activation, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);  ///To do: Include real activation type
                    //bandyionlist = CalculateBandYIons.CalculateBandYIon(possiblelist[ii].Sequence, XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
                }
                else
                {
                    //bandyionlist = CalculateBandYIons.CalculateBandYIon(originalsequence.Substring(0, initialstartp) + (possiblelist[ii].Sequence) + originalsequence.Substring(initialendp, originalsequence.Length - (initialendp)), XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
                    bandyionlist = CalculateBYCZIons.CalculateBYCZIon(possiblelist[ii].Sequence, XvalueMonos, parentmass, activation, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);   ///To do: Include real activation type
                }
                possiblelist[ii].BandYIonPercent = Math.Round(((double)bandyionlist.Count(a => a.yioncolor || a.bioncolor)
                    //(0.25) * (double)bandyionlist.Count(a => a.bh2oioncolor || a.yh2oioncolor) +
                    //(0.25) * (double)bandyionlist.Count(a => a.bnh3ioncolor || a.ynh3ioncolor))
                                                    /
                                                    (double)bandyionlist.Count) * 100, 2);


                ///Remove the Possible mods based on the score. Only having the top score possible modifications. 
                //if (listofbandyionpercentage.Any())
                //{
                //    if (listofbandyionpercentage.Any(a => a.Value < possiblelist[ii].BandYIonPercent))
                //    {
                //        //List<int> removeitems = new List<int>();
                //        //foreach (var lst in listofbandyionpercentage.Where(a => a.Value < possiblelist[ii].BandYIonPercent))
                //        //{
                //        //    removeitems.Add(lst.Key);
                //        //    //possiblelist.RemoveAt(lst.Key);
                //        //    //listofbandyionpercentage.Remove(lst.Key);
                //        //}
                //        //for (int i = 0; i < removeitems.Count; i++)
                //        //{
                //        //    possiblelist.RemoveAt(removeitems[i]);
                //        //    listofbandyionpercentage.Remove(removeitems[i]);
                //        //}
                //        listofbandyionpercentage.Add(ii, possiblelist[ii].BandYIonPercent);
                //    }
                //    else if (listofbandyionpercentage.Any(a => a.Value == possiblelist[ii].BandYIonPercent))
                //    {
                //        listofbandyionpercentage.Add(ii, possiblelist[ii].BandYIonPercent);
                //    }
                //}
                //else
                //{
                //    listofbandyionpercentage.Add(ii, possiblelist[ii].BandYIonPercent);
                //}

                possiblelist[ii].BIonPercent = Math.Round(((double)bandyionlist.Count(a => a.bioncolor) / (double)bandyionlist.Count) * 100, 2);

                possiblelist[ii].YIonPercent = Math.Round(((double)bandyionlist.Count(a => a.yioncolor) / (double)bandyionlist.Count) * 100, 2);

                possiblelist[ii].Intensity = bandyionlist.Sum(a => a.Intensity);

                possiblelist[ii].TotalAmmoniaLosses = bandyionlist.Where(a => a.bnh3ioncolor).Count();

                possiblelist[ii].TotalWaterLosses = bandyionlist.Where(a => a.bh2oioncolor).Count();

                possiblelist[ii].ModNorCTerm = ((possiblelist[ii].Sequence.Substring(1, 1).Contains("[") && isend && initialstartp == 0) //If there is a modification at the n-term
                                                 || (possiblelist[ii].Sequence.EndsWith("]") && initialstartp != 0 && isend))            //If there is a modification at the c-term
                                                 ? true : false;                                                                   //Setting the norcerterm to be true

                //possiblelist[ii].Currentposition = ii;

                bandyionerrors = bandyionlist.Where(a => a.bionerror != 100).Select(a => a.bionerror).ToList();
                bandyionerrors.AddRange(bandyionlist.Where(a => a.yionerror != 100).Select(a => a.yionerror).ToList());

                List<double> bandyionerrorsppm = new List<double>();

                bandyionerrorsppm = bandyionlist.Where(a => a.bionerror != 100).Select(a => a.bionPPM).ToList();

                bandyionerrorsppm.AddRange(bandyionlist.Where(a => a.yionerror != 100).Select(a => a.yionPPM).ToList());

                bandyionerrorsppm = bandyionerrorsppm.Where(a => a != 0).ToList();

                bandyionerrors = bandyionerrors.Where(a => a != 0).ToList();

                if (bandyionerrors.Any())
                {
                    bandyionerrormedian = bandyionerrors.Median();
                    bandyionerrormedian = bandyionerrorsppm.Median();
                    double minmedianvalue = bandyionerrormedian * 0.25;
                    double maxmedianvalue = bandyionerrormedian * 2.00;

                    bandyionerrors = bandyionerrorsppm.Select(a => Math.Abs(a - bandyionerrormedian)).Where(a => a != 0.0).ToList();

                    possiblelist[ii].NumberofErrorsNearMedian = ((double)bandyionerrorsppm.Where(a => a >= minmedianvalue && a <= maxmedianvalue).Count() / (double)bandyionerrorsppm.Count) * 100;

                    possiblelist[ii].MedianofErrors = bandyionerrormedian;

                    bandyionerrorsppm = bandyionerrorsppm.Select(a => Math.Abs(a - bandyionerrormedian)).Where(a => a != 0.0).ToList();
                    possiblelist[ii].MedianofMedianErrorslist = bandyionerrors;
                    possiblelist[ii].MedianofMedianErrorslist = bandyionerrorsppm;
                    //bandyionerrors = bandyionerrors.Where(a => a != 0).Select(a => Math.Abs(a - bandyionerrormedian)).ToList();
                    if (bandyionerrors.Any())
                    {
                        var medianofmedians = bandyionerrors.Median();
                        possiblelist[ii].MedianofMedianbandyionerrors = Math.Round(medianofmedians, 5);/// Math.Round(bandyionerrors.Select(a => Math.Abs(a - bandyionerrormedian)).Median(), 5);
                        //possiblelist[ii].BandYIonPercentDividebyMedianofMedianbandyionerrors = possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofMedianbandyionerrors;
                        possiblelist[ii].BandYIonPercentDividebyMedianofMedianbandyionerrors = possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofErrors;

                        possiblelist[ii].RootmeanSquareError = RootMeanSquareError(possiblelist[ii].MedianofMedianErrorslist);

                        possiblelist[ii].AverageError = possiblelist[ii].MedianofMedianErrorslist.Average();

                        possiblelist[ii].AverageError = (possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofMedianErrorslist.Average());

                        //possiblelist[ii].AverageError = possiblelist[ii].BandYIonPercent * possiblelist[ii].AverageError;
                    }
                }
                bandyionpercentage = possiblelist[ii].BandYIonPercent;
            }


            //int count = possiblelist.Count;

            possiblelist = possiblelist.OrderByDescending(a => a.BandYIonPercent).ToList(); //.ThenByDescending(a => a.Currentposition)

            //for (int i = 0; i < psbcount; i++)
            //{
            //    if (!(listofbandyionpercentage.Any(a => a.Key == i)))
            //    {
            //        if (possiblelist.Any(a => a.Currentposition == i))
            //        {
            //            possiblelist.Remove(possiblelist.First(a => a.Currentposition == i));
            //        }
            //    }
            //}

            if (!(bandyionerrors.Any()))
            {
                bandyionerrors.Add(0.0);
            }

            Allmods.Clear();

            //Allmods.Add(possiblelist.First());
            Allmods = SortModsbyPriority(possiblelist, 50, HasKnownMods);

            /// 
            /// asdhjkas jkdh
            /// ..
            /// 
            // 

            //adsklj asd;
            return Allmods;
        }




        ///// <summary>
        ///// Checks all the possible modifications.
        ///// Loops through all the AminoAcids in the sequence and generates all the modifications
        ///// </summary>
        ///// <param name="sequence"></param>
        ///// <param name="startmonomass"></param>
        ///// <param name="endmonomass"></param>
        ///// <param name="monomasslist"></param>
        ///// <param name="gap"></param>
        ///// <param name="parentmass"></param>
        ///// <param name="isend"></param>
        ///// <param name="originalsequence"></param>
        ///// <param name="isforward"></param>
        ///// <param name="startp"></param>
        ///// <param name="endp"></param>
        ///// <param name="islast"></param>
        ///// <param name="aminoacids"></param>
        ///// <returns></returns>
        //public static string CheckforMods(string sequence, double startmonomass, double endmonomass, Dictionary<double, double> monomasslist, double gap, double parentmass, bool isend, string originalsequence, bool isforward, int startp, int endp, bool islast, SortedList<char, double> aminoacids, bool HasKnownMods, ref List<PossibleMods> psdms)
        //{
        //    string sequencewithmods = string.Empty;

        //    List<string> sequencewithmodslist = new List<string>();

        //    var allmonos = monomasslist.Where(a => (a.Key <= endmonomass + 1.0) && (a.Key >= startmonomass + 1.0)).OrderBy(a => a.Key).Select(a => a.Key).ToList(); //Only look for amino acids within the range.

        //    allmonos.AddRange(allmonos.Select(a => (parentmass - a)).ToList()); //Add the reverse amino acids

        //    allmonos.Add(0);
        //    allmonos.Add(Molecules.Water);
        //    allmonos.Add(parentmass);
        //    allmonos.Add(parentmass - Molecules.Water);

        //    double currentmonomass = startmonomass;
        //    double currentgap = gap;
        //    double reversegap = gap;
        //    double currentsequencelength = 0.0;
        //    List<PossibleMods> possiblelist = new List<PossibleMods>();

        //    List<double> medianoferrors = new List<double>();

        //    int count = 0;
        //    int reversecount = 0;

        //    int monomasscount = 0;
        //    int reversemonomasscount = 0;

        //    int tempcount = 0;

        //    bool firstterm = true;
        //    string tempsequencewithmods = string.Empty;

        //    int sequencecount = 0;
        //    bool lastterm = false;
        //    int tempmonomasscount = 0;
        //    int Allmodscount = 0;

        //    List<PossibleMods> Allmods = new List<PossibleMods>();

        //    string aa = string.Empty;
        //    List<PossibleMods> temppossiblelist = new List<PossibleMods>();
        //    string previousstring = string.Empty;

        //    double maxmodmass = Convert.ToDouble(App.AllValidationModifications.MaxBy(a => a.Mass).Mass);

        //    double maxgap = currentgap;

        //    double maxaminoacidmass = aminoacids.MaxBy(a => a.Value).Value;

        //    List<CalculateBandYIons.BYIons> bandyionlist = new List<CalculateBandYIons.BYIons>();

        //    allmonos = allmonos.OrderBy(a => a).ToList();

        //    List<double> XvalueMonos = new List<double>();

        //    XvalueMonos = monomasslist.Select(a => a.Key).ToList();

        //    for (int i = 0; i < sequence.Length; i++)
        //    {
        //        aa = sequence[i].ToString();

        //        temppossiblelist.Clear();

        //        if (i == 0)
        //            previousstring = previousstring + aa;

        //        currentmonomass = startmonomass + sequencelength(previousstring);

        //        temppossiblelist.Add(new PossibleMods()
        //        {
        //            Sequence = previousstring,
        //            CurrentGap = gap,
        //            CurrentMonomass = currentmonomass,
        //            HasMonoMass = false,
        //            ModGap = 0,
        //            MonosCount = 0,
        //            StartMonomass = startmonomass
        //        });

        //        if (i == 0 && isend)
        //        {
        //            if (isforward && startp == 0) //If the sequence is forward and this is the first AminoAcid. Nterm
        //            {
        //                possiblelist = PossiblemodSequences(allmonos, aa, true, temppossiblelist, false, true);
        //            }
        //            else
        //            {
        //                possiblelist = PossiblemodSequences(allmonos, aa, true, temppossiblelist);
        //            }
        //        }
        //        else if (i == sequence.Length - 1 && isend && !isforward && startp == 0) //If the sequence is reverse and first AminoAcid. Nterm
        //        {
        //            possiblelist = PossiblemodSequences(allmonos, aa, true, temppossiblelist, false, true, true);
        //        }
        //        else
        //        {
        //            possiblelist = PossiblemodSequences(allmonos, aa, false, temppossiblelist);
        //        }

        //        for (int j = i + 1; j < sequence.Length; j++)
        //        {
        //            string bb = sequence[j].ToString();
        //            if (isend && j == sequence.Length - 1)
        //            {
        //                possiblelist = PossiblemodSequences(allmonos, bb, true, possiblelist, true);
        //            }
        //            else
        //            {
        //                possiblelist = PossiblemodSequences(allmonos, bb, false, possiblelist);
        //            }
        //        }

        //        possiblelist = possiblelist.Where(a => Math.Abs(a.ModGap) <= currentgap).ToList();

        //        List<double> bandyionerrors = new List<double>();
        //        double bandyionerrormedian = 0.0;
        //        for (int ii = 0; ii < possiblelist.Count; ii++)
        //        {
        //            if (!isforward)
        //            {
        //                possiblelist[ii].Sequence = reversesequencewithmods(possiblelist[ii].Sequence);
        //            }
        //            //If there is any gap need to include that in the sequence
        //            if (possiblelist[ii].CurrentGap > 0.1)
        //            {
        //                if (islast)
        //                {
        //                    possiblelist[ii].Sequence = AddModtoSequenceAttheend(possiblelist[ii].Sequence, Math.Round(possiblelist[ii].CurrentGap, 2));
        //                }
        //                else
        //                {
        //                    possiblelist[ii].Sequence = AddModtoSequence(possiblelist[ii].Sequence, Math.Round(possiblelist[ii].CurrentGap, 2));
        //                }
        //            }

        //            if (isforward)
        //            {
        //                bandyionlist = CalculateBandYIons.CalculateBandYIon(originalsequence.Substring(0, startp) + (possiblelist[ii].Sequence) + originalsequence.Substring(endp, originalsequence.Length - (endp)), XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
        //            }
        //            else
        //            {
        //                bandyionlist = CalculateBandYIons.CalculateBandYIon(originalsequence.Substring(0, startp) + (possiblelist[ii].Sequence) + originalsequence.Substring(endp, originalsequence.Length - (endp)), XvalueMonos, parentmass, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, aminoacids, true, monomasslist);
        //            }

        //            possiblelist[ii].BandYIonPercent = Math.Round(((double)bandyionlist.Count(a => a.yioncolor || a.bioncolor)
        //                //(0.25) * (double)bandyionlist.Count(a => a.bh2oioncolor || a.yh2oioncolor) +
        //                //(0.25) * (double)bandyionlist.Count(a => a.bnh3ioncolor || a.ynh3ioncolor))
        //                                                /
        //                                                (double)bandyionlist.Count) * 100, 2);

        //            possiblelist[ii].BIonPercent = Math.Round(((double)bandyionlist.Count(a => a.bioncolor) / (double)bandyionlist.Count) * 100, 2);

        //            possiblelist[ii].YIonPercent = Math.Round(((double)bandyionlist.Count(a => a.yioncolor) / (double)bandyionlist.Count) * 100, 2);

        //            possiblelist[ii].Intensity = bandyionlist.Sum(a => a.Intensity);

        //            possiblelist[ii].TotalAmmoniaLosses = bandyionlist.Where(a => a.bnh3ioncolor).Count();

        //            possiblelist[ii].TotalWaterLosses = bandyionlist.Where(a => a.bh2oioncolor).Count();

        //            possiblelist[ii].ModNorCTerm = ((possiblelist[ii].Sequence.Substring(1, 1).Contains("[") && isend && startp == 0) //If there is a modification at the n-term
        //                                             || (possiblelist[ii].Sequence.EndsWith("]") && startp != 0 && isend))            //If there is a modification at the c-term
        //                                             ? true : false;                                                                   //Setting the norcerterm to be true

        //            bandyionerrors = bandyionlist.Where(a => a.bionerror != 100).Select(a => a.bionerror).ToList();
        //            bandyionerrors.AddRange(bandyionlist.Where(a => a.yionerror != 100).Select(a => a.yionerror).ToList());

        //            List<double> bandyionerrorsppm = new List<double>();

        //            bandyionerrorsppm = bandyionlist.Where(a => a.bionerror != 100).Select(a => a.bionPPM).ToList();

        //            bandyionerrorsppm.AddRange(bandyionlist.Where(a => a.yionerror != 100).Select(a => a.yionPPM).ToList());

        //            bandyionerrorsppm = bandyionerrorsppm.Where(a => a != 0).ToList();

        //            bandyionerrors = bandyionerrors.Where(a => a != 0).ToList();

        //            if (bandyionerrors.Any())
        //            {
        //                bandyionerrormedian = bandyionerrors.Median();
        //                bandyionerrormedian = bandyionerrorsppm.Median();
        //                double minmedianvalue = bandyionerrormedian * 0.25;
        //                double maxmedianvalue = bandyionerrormedian * 2.00;

        //                bandyionerrors = bandyionerrorsppm.Select(a => Math.Abs(a - bandyionerrormedian)).Where(a => a != 0.0).ToList();

        //                possiblelist[ii].NumberofErrorsNearMedian = ((double)bandyionerrorsppm.Where(a => a >= minmedianvalue && a <= maxmedianvalue).Count() / (double)bandyionerrorsppm.Count) * 100;

        //                possiblelist[ii].MedianofErrors = bandyionerrormedian;
        //                //possiblelist[ii].MedianofErrors = 

        //                bandyionerrorsppm = bandyionerrorsppm.Select(a => Math.Abs(a - bandyionerrormedian)).Where(a => a != 0.0).ToList();
        //                possiblelist[ii].MedianofMedianErrorslist = bandyionerrors;
        //                possiblelist[ii].MedianofMedianErrorslist = bandyionerrorsppm;
        //                //bandyionerrors = bandyionerrors.Where(a => a != 0).Select(a => Math.Abs(a - bandyionerrormedian)).ToList();
        //                if (bandyionerrors.Any())
        //                {
        //                    var medianofmedians = bandyionerrors.Median();
        //                    possiblelist[ii].MedianofMedianbandyionerrors = Math.Round(medianofmedians, 5);/// Math.Round(bandyionerrors.Select(a => Math.Abs(a - bandyionerrormedian)).Median(), 5);
        //                    //possiblelist[ii].BandYIonPercentDividebyMedianofMedianbandyionerrors = possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofMedianbandyionerrors;
        //                    possiblelist[ii].BandYIonPercentDividebyMedianofMedianbandyionerrors = possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofErrors;

        //                    possiblelist[ii].RootmeanSquareError = RootMeanSquareError(possiblelist[ii].MedianofMedianErrorslist);

        //                    possiblelist[ii].AverageError = possiblelist[ii].MedianofMedianErrorslist.Average();

        //                    possiblelist[ii].AverageError = (possiblelist[ii].BandYIonPercent / possiblelist[ii].MedianofMedianErrorslist.Average());

        //                    //possiblelist[ii].AverageError = possiblelist[ii].BandYIonPercent * possiblelist[ii].AverageError;
        //                }
        //            }
        //        }

        //        Allmods.AddRange(possiblelist);
        //        Allmods = SortModsbyPriority(Allmods, 50, HasKnownMods);
        //        possiblelist = new List<PossibleMods>();

        //        if (i != 0)
        //        {
        //            previousstring = previousstring + aa;
        //        }
        //    }

        //    List<SequenceandMedianofmedianerrors> mm = new List<SequenceandMedianofmedianerrors>();
        //    if (Allmods.Any())
        //    {
        //        Allmods = Allmods.OrderByDescending(a => a.BandYIonPercentDividebyMedianofMedianbandyionerrors).ToList();

        //        Allmods = Allmods.GroupBy(a => a.Sequence).Select(a => a.First()).ToList();

        //        for (int i = 0; i < Allmods.Count; i++)
        //        {
        //            mm.Add(new SequenceandMedianofmedianerrors
        //            {
        //                Mod = Allmods[i].Sequence,
        //                MedianofMedian = Allmods[i].MedianofMedianbandyionerrors,
        //                MedianofMedianErrorsdivision = Allmods[i].BandYIonPercentDividebyMedianofMedianbandyionerrors
        //            });
        //        }
        //    }

        //    if (Allmods.Any())
        //    {
        //        if (!isforward)
        //        {
        //            var firstallmods = SortModsbyPriority(Allmods, 50, HasKnownMods).First();
        //            sequencewithmods = firstallmods.Sequence;
        //        }
        //        else
        //        {
        //            var firstallmods = SortModsbyPriority(Allmods, 50, HasKnownMods).First();
        //            sequencewithmods = firstallmods.Sequence;
        //        }
        //        psdms = Allmods;
        //    }
        //    else
        //    {
        //        sequencewithmods = string.Empty;
        //    }

        //    return sequencewithmods;
        //}

        public static double RootMeanSquareError(List<double> values)
        {
            double rmse = 0.0;

            for (int i = 0; i < values.Count; i++)
            {
                rmse = rmse + values[i] * values[i];
            }

            rmse = Math.Sqrt(rmse / values.Count);

            return rmse;
        }


        /// <summary>
        /// Reverse the sequence which have the mods.
        /// The mods need to be placed just after the Amino Acid. 
        /// If the sequence doesn't have any mods it needs to just reverse the sequence.
        /// </summary>
        /// <param name="sequencewithmods"></param>
        /// <returns></returns>
        private static string reversesequencewithmods(string sequencewithmods)
        {
            string seq = string.Empty;

            if (!sequencewithmods.Contains("["))
            {
                seq = ReverseString.Reverse(sequencewithmods);
            }
            else
            {
                for (int i = 0; i < sequencewithmods.Length; i++)
                {
                    string tempseq = string.Empty;
                    if ((i != sequencewithmods.Length - 1) && sequencewithmods[i + 1] == '[') /// If the Amino Acid has a Mod it places it after the Amino Acid
                    {
                        tempseq = tempseq + sequencewithmods[i];
                        for (int j = i + 1; j < sequencewithmods.Length; j++)
                        {
                            if (sequencewithmods[j] == ']')
                            {
                                tempseq = tempseq + "]";
                                seq = tempseq + seq;
                                i = j;
                                break;
                            }
                            tempseq = tempseq + sequencewithmods[j];
                        }
                    }
                    else
                    {
                        seq = sequencewithmods[i] + seq;
                    }
                }
            }

            return seq;
        }

        /// <summary>
        /// Adds a modification to the sequence
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static string AddModtoSequence(string sequence, double mod)
        {
            string tempsequence = string.Empty;
            if (sequence.Contains("["))
            {
                for (int i = 0; i < sequence.Length; i++)
                {
                    if (i < sequence.Length - 2)
                    {
                        if (sequence[i + 2] == '[')
                        {
                            tempsequence = "(" + tempsequence + sequence[i] + ")" + "[" + Math.Round(mod, 2) + "]" + sequence.Substring(i + 1, sequence.Length - (i + 1));
                            break;
                        }
                        else
                        {
                            tempsequence = tempsequence + sequence[i];
                        }
                    }
                    else
                    {
                        if (i == sequence.Length - 1)
                        {
                            tempsequence = tempsequence + sequence[i] + "[" + mod + "]";
                        }
                        else
                        {
                            tempsequence = tempsequence + sequence[i];
                        }
                    }
                }
            }
            else
            {
                tempsequence = "(" + sequence + ")" + "[" + Math.Round(mod, 4) + "]";
            }

            return tempsequence;
        }

        /// <summary>
        /// Adds a modification at the end of the sequence
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static string AddModtoSequenceAttheend(string sequence, double mod, int finalstart, int finalend)
        {
            string tempsequence = string.Empty;

            if (sequence.EndsWith("]"))
            {
                tempsequence = "(" + sequence + ")" + "[" + mod + "]";
                return tempsequence;
            }
            if (sequence.Contains("["))
            {
                for (int i = sequence.Length - 1; i >= 0; i--)
                {
                    if (i > 1)
                    {
                        if (sequence[i - 1] == ']')
                        {
                            if (finalstart < finalend)
                            {
                                if (tempsequence.Length < (finalend - finalstart))
                                {

                                    finalstart = FindEndStart(sequence, finalstart);
                                    tempsequence = sequence.Substring(0, i) + insertsequenceatend(sequence.Substring(sequence.Length - (finalend - finalstart), (finalend - finalstart)), 1, finalend - finalstart) + "[" + mod + "]";
                                    break;
                                }
                                //tempsequence = sequence.Substring(0, i) + sequence[i] + insertsequenceatend(sequence.Substring(sequence.Length - (finalend - finalstart), (finalend - finalstart)), sequence.Length - (finalend - finalstart), sequence.Length) + "[" + mod + "]";

                                tempsequence = sequence.Substring(0, i) + sequence[i] + insertsequenceatend(tempsequence, tempsequence.Length - (finalend - finalstart), tempsequence.Length) + "[" + mod + "]";
                                break;
                            }
                            else
                            {
                                tempsequence = sequence.Substring(0, i) + "(" + sequence[i] + tempsequence + ")" + "[" + mod + "]";
                                break;
                            }
                        }
                        else
                        {
                            tempsequence = sequence[i] + tempsequence;
                        }
                    }
                    else
                    {
                        tempsequence = sequence[i] + tempsequence;
                    }
                }
            }
            else
            {
                tempsequence = "(" + sequence + ")" + "[" + mod + "]";
            }
            tempsequence = RemoveUnnecessaryMods(tempsequence);
            return tempsequence;
        }


        /// <summary>
        /// Removing additional modifications which are not supposed to be present.
        /// For example ABC[18.09]18.09] if at all is created need to remove the 18.09] at the end.
        /// This additional modifications are difficult to find the source and it only created more bugs
        /// when finding the source.
        /// Hence, removing the additional mods
        /// </summary>
        /// <param name="initialmod"></param>
        /// <returns></returns>
        static string RemoveUnnecessaryMods(string initialmod)
        {
            string initialstring = string.Empty;
            bool mod = false; // Is true when we have a mod
            Regex regex = new Regex(@"[\d]");
            bool notmod = false; //Is true when we have a bad mod
            for (int i = 0; i <= initialmod.Length - 1; i++)
            {
                if (mod) //If it is a mod then add the value
                {
                    if (initialmod[i] == ']')
                    {
                        mod = false;
                    }
                    initialstring = initialstring + initialmod[i].ToString();
                    continue;
                }

                if (initialmod[i] == '[') /// Initial the mod vaariable
                {
                    mod = true;
                    initialstring = initialstring + initialmod[i].ToString();
                    continue;
                }

                if (notmod) //If not a mod then pass the string value
                {
                    if (initialmod[i] == ']')
                    {
                        notmod = false;
                    }
                    continue;
                }

                if (i != 0 && regex.IsMatch(initialmod[i].ToString())) ///If the match is 
                {
                    if (initialmod[i - 1] != '[')
                    {
                        notmod = true;
                        continue;
                    }
                }

                initialstring = initialstring + initialmod[i].ToString();
            }

            return initialstring;
        }

        /// <summary>
        /// Finds the Start from the end using ( ) [ ]
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        static int FindEndStart(string seq, int start)
        {
            int number = seq.Length - 1;

            int count = start;
            bool vals = false;
            bool valb = false;

            for (int i = seq.Length - 1; i >= 0; i--)
            {
                if (seq[i] == ')')
                {
                    valb = true;
                }
                else if (seq[i] == ']')
                {
                    vals = true;
                }

                if (!vals || !valb)
                {
                    number = number - 1;
                }

                if ((i != 0 && seq[i] == '(') && vals)
                {
                    vals = false;
                }
                else if ((i != 0 && seq[i] == '[') && valb)
                {
                    valb = false;
                }

                if (number == start)
                {
                    number = i;
                    break;
                }

            }
            return number;
        }


        /// <summary>
        /// Find the correct end
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        static string insertsequenceatend(string seq, int start, int end)
        {
            bool con = false;
            string tempseq = string.Empty;
            int i = end - 1;
            for (; i > start - 1; i--)
            {
                if (seq[i] == ']')
                {
                    break;
                }
                else
                {
                    tempseq = seq[i] + tempseq;
                }
            }
            tempseq = seq.Substring(0, i + 1) + "(" + tempseq + ")";
            return tempseq;
        }


        /// <summary>
        /// Finds the modifications based on the monomasslist.
        /// </summary>
        /// <param name="monomasslist"></param>
        /// <param name="aa"></param>
        /// <param name="firstterm"></param>
        /// <param name="pbmslist"></param>
        /// <param name="addmonomass"></param>
        /// <param name="norcterm"></param>
        /// <returns></returns>
        public static List<PossibleMods> PossiblemodSequences(List<double> monomasslist, string aa, bool firstterm, List<PossibleMods> pbmslist, bool addmonomass = false, bool norcterm = false, bool isreverse = false, bool Doesnthaveonlyoneaminoacid = true, bool onlyknownmods = true)
        {
            List<PossibleMods> possiblemods = new List<PossibleMods>();

            List<double> localmonomasses = new List<double>();

            List<double> lclmms = new List<double>();

            double currentmonomass = 0;
            if (pbmslist.Count == 0)
                pbmslist.Add(new PossibleMods());

            string aaa = string.Empty;

            if (!firstterm) // If it is not the first term then don't consider the string
            {
                aaa = aa;
            }
            if (firstterm && isreverse) // If it is a first term and rev then consider the sequence
            {
                aaa = aa;
            }

            double CurrentAminoAcidmassvalue = AminoAcidHelpers.AminoAcidMass2.First(a => (a.Key == aa)).Value;

            //asdlk jasldk lk;

            var matchingaminoacids = AminoAcidHelpers.AminoAcidMass2.Where(a => (a.Key.Contains(aa)) && (a.Key != aa)).ToList(); ///Getting the AminoAcids which are similar to Current AminoAcid but not equal to the current Amino Acid
            if (matchingaminoacids.Count > 0) ///If there are any matchingaminoacids then need to check if there are any modified aminoacids which can fit the gap
            {
                foreach (var maa in matchingaminoacids) ///Looping through the list and checking if there any amino acids which fit the gap
                {
                    foreach (var pbms in pbmslist)
                    {
                        if (Math.Abs(pbms.CurrentGap - (maa.Value - CurrentAminoAcidmassvalue)) <= 0.1)
                        {
                            possiblemods.Add(new PossibleMods()
                            {
                                Sequence = (firstterm ? maa.Key : (pbms.Sequence + maa.Key)),
                                CurrentMonomass = (firstterm) ? (pbms.StartMonomass + Convert.ToDouble(maa.Value)) : (Convert.ToDouble(maa.Value) + pbms.CurrentMonomass),
                                HasMonoMass = true,
                                ModGap = 0,
                                MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(maa.Value) + currentmonomass)) <= 0.1),
                                CurrentGap = 0,
                                AllMonons = localmonomasses,
                                TotalNumberofModifiedMatchAminoAcids = pbms.TotalNumberofModifiedMatchAminoAcids + 1,
                                HasMod = true,
                                StartMonomass = pbms.StartMonomass,
                                NumberofMods = pbms.NumberofMods + 1,
                                TotalKnownMods = pbms.TotalKnownMods + 1,
                                hasremainingstartgap = pbms.hasremainingstartgap,
                                modificationlength = pbms.modificationlength + (maa.Key.Length - 1),
                                Currentposition = pbms.Currentposition
                            });
                        }
                    }
                }
            }

            try
            {
                foreach (var pbms in pbmslist)
                {

                    localmonomasses = new List<double>();
                    if (pbms.Sequence == null)
                    {
                        pbms.CurrentMonomass = 0;
                        pbms.HasMonoMass = false;
                        pbms.ModGap = 0;
                        pbms.MonosCount = 0;
                        pbms.Sequence = string.Empty;
                        pbms.TotalKnownMods = 0;
                        pbms.NumberofMods = 0;
                    }

                    if (pbms.NumberofMods >= Properties.Settings.Default.MaximumModifications)
                    {
                        possiblemods.Add(new PossibleMods()
                        {
                            Sequence = pbms.Sequence + aa,
                            CurrentMonomass = pbms.CurrentMonomass + sequencelength(aaa),
                            HasMonoMass = pbms.HasMonoMass,
                            ModGap = pbms.ModGap,
                            MonosCount = pbms.MonosCount,
                            CurrentGap = pbms.CurrentGap,
                            AllMonons = pbms.AllMonons,
                            HasMod = pbms.HasMod,
                            StartMonomass = pbms.StartMonomass,
                            NumberofMods = pbms.NumberofMods,
                            TotalKnownMods = pbms.TotalKnownMods,
                            hasremainingstartgap = pbms.hasremainingstartgap,
                            modificationlength = pbms.modificationlength,
                            Currentposition = pbms.Currentposition
                        });
                        continue;
                    }

                    List<ModificationList> possiblemodlist = new List<ModificationList>();
                    if (firstterm)
                    {
                        if (norcterm)
                        {
                            if (Doesnthaveonlyoneaminoacid)
                            {

                                possiblemodlist = App.AllValidationModifications.Any(a => (a.AminoAcids.Any(b => b.ToLower() == ("n-term")) || a.AminoAcids.Any(c => c == aa)) && (Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) <= PPM.CurrentPPM(Convert.ToDouble(a.Mass)) + 0.1)) ?
                                                  App.AllValidationModifications.Where(a => (a.AminoAcids.Any(b => b.ToLower() == "n-term") || (a.AminoAcids.Any(c => c == aa))) && (Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) <= PPM.CurrentPPM(Convert.ToDouble(a.Mass)) + 0.1)).ToList() :
                                                  new List<ModificationList>();

                                foreach (var mod in possiblemodlist)
                                {
                                    localmonomasses = monomasslist.Where(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1).ToList();
                                    possiblemods.Add(new PossibleMods()
                                    {
                                        Sequence = pbms.Sequence + aaa + "[" + (mod.Mass.Contains("-") ? "-" : "+") + mod.Abbreviation + "]",
                                        CurrentMonomass = Convert.ToDouble(mod.Mass) + currentmonomass,
                                        HasMonoMass = true,
                                        ModGap = Convert.ToDouble(mod.Mass),
                                        MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1),
                                        CurrentGap = 0, /// pbms.CurrentGap - Convert.ToDouble(mod.Mass),
                                        AllMonons = localmonomasses,
                                        HasMod = true,
                                        StartMonomass = pbms.StartMonomass,
                                        NumberofMods = pbms.NumberofMods + 1,
                                        TotalKnownMods = pbms.TotalKnownMods + 1,
                                        hasremainingstartgap = pbms.hasremainingstartgap,
                                        modificationlength = pbms.modificationlength + mod.Abbreviation.Length + 3, ///Modification length plus the new abbreviation length plus the length of [ ] and - or +
                                        Currentposition = pbms.Currentposition
                                    });
                                }
                            }
                            else
                            {
                                possiblemodlist = App.AllValidationModifications.Any(a => (a.AminoAcids.Any(b => b.ToLower() == ("n-term")) || a.AminoAcids.Any(c => c == aa)) && (Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) <= (PPM.CurrentPPM(Convert.ToDouble(pbms.CurrentMonomass)) + 0.1))) ?
                                                  App.AllValidationModifications.Where(a => (a.AminoAcids.Any(b => b.ToLower() == "n-term") || (a.AminoAcids.Any(c => c == aa))) && (Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) <= (PPM.CurrentPPM(Convert.ToDouble(pbms.CurrentMonomass)) + 0.1))).ToList() :
                                                  new List<ModificationList>();

                                currentmonomass = pbms.CurrentMonomass;

                                if (possiblemodlist.Count > 0)
                                {
                                    foreach (var mod in possiblemodlist)
                                    {
                                        localmonomasses = monomasslist.Where(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1).ToList();
                                        possiblemods.Add(new PossibleMods()
                                        {
                                            Sequence = pbms.Sequence + aaa + "[" + (mod.Mass.Contains("-") ? "-" : "+") + mod.Abbreviation + "]",
                                            CurrentMonomass = Convert.ToDouble(mod.Mass) + currentmonomass,
                                            HasMonoMass = true,
                                            ModGap = Convert.ToDouble(mod.Mass),
                                            MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1),
                                            CurrentGap = 0, /// pbms.CurrentGap - Convert.ToDouble(mod.Mass),
                                            AllMonons = localmonomasses,
                                            HasMod = true,
                                            StartMonomass = pbms.StartMonomass,
                                            NumberofMods = pbms.NumberofMods + 1,
                                            TotalKnownMods = pbms.TotalKnownMods + 1,
                                            hasremainingstartgap = pbms.hasremainingstartgap,
                                            modificationlength = pbms.modificationlength + mod.Abbreviation.Length + 3,  ///Modification length plus the new abbreviation length plus the length of [ ] and - or +
                                            Currentposition = pbms.Currentposition
                                        });
                                    }
                                }
                                else
                                {
                                    currentmonomass = pbms.CurrentMonomass;
                                    localmonomasses = monomasslist.Where(a => Math.Abs(a - (Convert.ToDouble(pbms.CurrentGap) + currentmonomass)) <= 0.1).ToList();
                                    possiblemods.Add(new PossibleMods()
                                    {
                                        Sequence = pbms.Sequence + aaa + "[" + (pbms.CurrentGap) + "]",
                                        CurrentMonomass = Convert.ToDouble(pbms.CurrentGap) + currentmonomass,
                                        HasMonoMass = true,
                                        ModGap = Convert.ToDouble(pbms.CurrentGap),
                                        MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(pbms.CurrentGap) + currentmonomass)) <= 0.1),
                                        CurrentGap = 0,
                                        AllMonons = localmonomasses,
                                        HasMod = true,
                                        StartMonomass = pbms.StartMonomass,
                                        NumberofMods = pbms.NumberofMods,
                                        TotalKnownMods = pbms.TotalKnownMods,
                                        hasremainingstartgap = pbms.hasremainingstartgap,
                                        modificationlength = pbms.modificationlength + Convert.ToString(pbms.CurrentGap).Length + 2,
                                        Currentposition = pbms.Currentposition
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        if (App.AllValidationModifications.Any(a => a.AminoAcids.Any(b => b == aa) && Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) < 0.1))
                        {
                            possiblemodlist = App.AllValidationModifications.Where(a => a.AminoAcids.Any(b => b == aa) && Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) < 0.1).ToList();
                        }
                        //else if (App.AllValidationModifications.Any(a => a.AminoAcids.Any(b => b == aa) && Convert.ToDouble(a.Mass) < pbms.CurrentGap))
                        //{
                        //    possiblemodlist = App.AllValidationModifications.Where(a => a.AminoAcids.Any(b => b == aa) && Convert.ToDouble(a.Mass) < pbms.CurrentGap).ToList();
                        //}
                        else
                        {
                            possiblemodlist = new List<ModificationList>();
                        }
                        currentmonomass = pbms.CurrentMonomass + sequencelength(aaa);
                    }
                    if (addmonomass)
                    {
                        aaa = aa;
                        currentmonomass = pbms.CurrentMonomass + sequencelength(aaa);
                    }

                    List<double> lstvalidmodvalues = new List<double>();
                    lstvalidmodvalues = App.AllValidationModifications.Select(a => Convert.ToDouble(a.Mass)).ToList();

                    List<double> possibleunknownmods = new List<double>();
                    List<double> localmonos = monomasslist.Where(a => (a <= currentmonomass + pbms.CurrentGap + 0.1) && (a >= currentmonomass + 0.1)).ToList();
                    //if (localmonos.Any(a => Math.Abs(a - (currentmonomass + sequencelength(aa))) < pbms.CurrentGap))
                    if (!onlyknownmods && Doesnthaveonlyoneaminoacid)
                    {
                        foreach (double monomass in localmonos) ///.Where(a => Math.Abs(a - (currentmonomass + sequencelength(aa))) < pbms.CurrentGap).ToList())
                        {
                            double mm = monomass - currentmonomass;
                            if (Math.Abs(pbms.CurrentGap - mm) >= pbms.CurrentGap) continue;

                            if (lstvalidmodvalues.Any(a => Math.Abs(a - mm) <= PPM.CurrentPPMbasedonMatchList(mm, Properties.Settings.Default.MassTolerancePPM))) continue;

                            possiblemods.Add(new PossibleMods()
                            {
                                Sequence = pbms.Sequence + aaa + "[" + (Math.Round(mm, 4)) + "]",
                                CurrentMonomass = mm + currentmonomass,
                                HasMonoMass = true,
                                ModGap = mm,
                                MonosCount = pbms.MonosCount + 1,
                                CurrentGap = pbms.CurrentGap - mm,
                                AllMonons = localmonomasses,
                                HasMod = true,
                                StartMonomass = pbms.StartMonomass,
                                NumberofMods = pbms.NumberofMods + 1,
                                TotalKnownMods = pbms.TotalKnownMods,
                                hasremainingstartgap = pbms.hasremainingstartgap,
                                modificationlength = pbms.modificationlength + Convert.ToString(Math.Round(mm, 4)).Length + 2,
                                Currentposition = pbms.Currentposition
                            });
                        }
                    }
                    bool verified = false;
                    // If there are any mods
                    if (possiblemodlist.Count > 0)
                    {
                        bool anymods = false;
                        //Check if any of mods fit any of the monomasses
                        foreach (var mod in possiblemodlist)
                        {
                            if (monomasslist.Any(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1))
                            {
                                localmonomasses = monomasslist.Where(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1).ToList();
                                possiblemods.Add(new PossibleMods()
                                {
                                    Sequence = pbms.Sequence + aaa + "[" + (mod.Mass.Contains("-") ? "-" : "+") + mod.Abbreviation + "]",
                                    CurrentMonomass = Convert.ToDouble(mod.Mass) + currentmonomass,
                                    HasMonoMass = true,
                                    ModGap = Convert.ToDouble(mod.Mass),
                                    MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1),/// 1,
                                    CurrentGap = pbms.CurrentGap - Convert.ToDouble(mod.Mass),
                                    AllMonons = localmonomasses,
                                    HasMod = true,
                                    StartMonomass = pbms.StartMonomass,
                                    NumberofMods = pbms.NumberofMods + 1,
                                    TotalKnownMods = pbms.TotalKnownMods + 1,
                                    hasremainingstartgap = pbms.hasremainingstartgap,
                                    modificationlength = pbms.modificationlength + mod.Abbreviation.Length + 3,
                                    Currentposition = pbms.Currentposition
                                });
                                anymods = true;
                            }
                            //Apart from the mods check if the current aminoacid fits any of the monomasses
                            if (!verified)
                            {
                                if (monomasslist.Any(a => Math.Abs(a - currentmonomass) <= 0.1))
                                {
                                    localmonomasses = (monomasslist.Where(a => Math.Abs(a - currentmonomass) <= 0.1).ToList());
                                    possiblemods.Add(new PossibleMods()
                                    {
                                        Sequence = pbms.Sequence + aaa,
                                        CurrentMonomass = currentmonomass,
                                        HasMonoMass = true,
                                        ModGap = 0,
                                        MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - currentmonomass) <= 0.1),
                                        CurrentGap = pbms.CurrentGap,
                                        AllMonons = localmonomasses,
                                        HasMod = false,
                                        StartMonomass = pbms.StartMonomass,
                                        NumberofMods = pbms.NumberofMods,
                                        TotalKnownMods = pbms.TotalKnownMods,
                                        hasremainingstartgap = pbms.hasremainingstartgap,
                                        modificationlength = pbms.modificationlength,
                                        Currentposition = pbms.Currentposition
                                    });
                                    verified = true;
                                }
                            }
                            // If there are no mods present and there are no monomasses which fit the amino acid.
                            if (!verified)
                            {
                                if (!anymods)
                                {
                                    if (!(monomasslist.Any(a => Math.Abs(a - currentmonomass) <= 0.1)))
                                    {
                                        possiblemods.Add(new PossibleMods()
                                        {
                                            Sequence = pbms.Sequence + aaa,
                                            CurrentMonomass = currentmonomass,
                                            HasMonoMass = false,
                                            ModGap = 0,
                                            MonosCount = pbms.MonosCount,
                                            CurrentGap = pbms.CurrentGap,
                                            HasMod = false,
                                            StartMonomass = pbms.StartMonomass,
                                            TotalKnownMods = pbms.TotalKnownMods,
                                            NumberofMods = pbms.NumberofMods,
                                            hasremainingstartgap = pbms.hasremainingstartgap,
                                            modificationlength = pbms.modificationlength,
                                            Currentposition = pbms.Currentposition
                                        });
                                        verified = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (!verified) // If no possible modifications
                    {
                        if (monomasslist.Any(a => Math.Abs(a - currentmonomass) <= 0.1)) // Check if any of the monomasses fit the amino acid
                        {
                            localmonomasses = monomasslist.Where(a => Math.Abs(a - currentmonomass) <= 0.1).ToList();
                            possiblemods.Add(new PossibleMods()
                            {
                                Sequence = pbms.Sequence + aaa,
                                CurrentMonomass = currentmonomass,
                                HasMonoMass = true,
                                ModGap = 0,
                                MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - currentmonomass) <= 0.1),
                                CurrentGap = pbms.CurrentGap,
                                HasMod = false,
                                StartMonomass = pbms.StartMonomass,
                                NumberofMods = pbms.NumberofMods,
                                TotalKnownMods = pbms.TotalKnownMods,
                                hasremainingstartgap = pbms.hasremainingstartgap,
                                modificationlength = pbms.modificationlength,
                                Currentposition = pbms.Currentposition
                            });
                            verified = true;
                        }
                        else  // If no monomass fit the amino acid
                        {
                            possiblemods.Add(new PossibleMods()
                            {
                                Sequence = pbms.Sequence + aaa,
                                CurrentMonomass = currentmonomass,
                                HasMonoMass = false,
                                ModGap = 0,
                                MonosCount = pbms.MonosCount,
                                CurrentGap = pbms.CurrentGap,
                                HasMod = false,
                                StartMonomass = pbms.StartMonomass,
                                TotalKnownMods = pbms.TotalKnownMods,
                                NumberofMods = pbms.NumberofMods,
                                hasremainingstartgap = pbms.hasremainingstartgap,
                                modificationlength = pbms.modificationlength,
                                Currentposition = pbms.Currentposition
                            });
                            verified = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return possiblemods;
        }


        /// <summary>
        /// Finds the modifications based on the monomasslist.
        /// </summary>
        /// <param name="monomasslist"></param>
        /// <param name="aa"></param>
        /// <param name="firstterm"></param>
        /// <param name="pbmslist"></param>
        /// <param name="addmonomass"></param>
        /// <param name="norcterm"></param>
        /// <returns></returns>
        public static List<PossibleMods> PossiblemodSequencesOld(List<double> monomasslist, string aa, bool firstterm, List<PossibleMods> pbmslist, bool addmonomass = false, bool norcterm = false, bool isreverse = false, bool Doesnthaveonlyoneaminoacid = true, bool onlyknownmods = true)
        {
            List<PossibleMods> possiblemods = new List<PossibleMods>();

            List<double> localmonomasses = new List<double>();

            List<double> lclmms = new List<double>();

            double currentmonomass = 0;
            if (pbmslist.Count == 0)
                pbmslist.Add(new PossibleMods());

            string aaa = string.Empty;

            if (!firstterm) // If it is not the first term then don't consider the string
            {
                aaa = aa;
            }
            if (firstterm && isreverse) // If it is a first term and rev then consider the sequence
            {
                aaa = aa;
            }
            try
            {
                foreach (var pbms in pbmslist)
                {
                    localmonomasses = new List<double>();
                    if (pbms.Sequence == null)
                    {
                        pbms.CurrentMonomass = 0;
                        pbms.HasMonoMass = false;
                        pbms.ModGap = 0;
                        pbms.MonosCount = 0;
                        pbms.Sequence = string.Empty;
                        pbms.TotalKnownMods = 0;
                        pbms.NumberofMods = 0;
                    }

                    if (pbms.NumberofMods >= Properties.Settings.Default.MaximumModifications)
                    {
                        possiblemods.Add(new PossibleMods()
                        {
                            Sequence = pbms.Sequence + aa,
                            CurrentMonomass = pbms.CurrentMonomass + sequencelength(aaa),
                            HasMonoMass = pbms.HasMonoMass,
                            ModGap = pbms.ModGap,
                            MonosCount = pbms.MonosCount,
                            CurrentGap = pbms.CurrentGap,
                            AllMonons = pbms.AllMonons,
                            HasMod = pbms.HasMod,
                            StartMonomass = pbms.StartMonomass,
                            NumberofMods = pbms.NumberofMods,
                            TotalKnownMods = pbms.TotalKnownMods,
                            hasremainingstartgap = pbms.hasremainingstartgap
                        });
                        continue;
                    }

                    List<ModificationList> possiblemodlist = new List<ModificationList>();
                    if (firstterm)
                    {
                        if (norcterm)
                        {
                            if (Doesnthaveonlyoneaminoacid)
                            {

                                possiblemodlist = App.AllValidationModifications.Any(a => (a.AminoAcids.Any(b => b.ToLower() == ("n-term")) || a.AminoAcids.Any(c => c == aa)) && Convert.ToDouble(a.Mass) < pbms.CurrentGap) ?
                                                  App.AllValidationModifications.Where(a => (a.AminoAcids.Any(b => b.ToLower() == "n-term") || (a.AminoAcids.Any(c => c == aa))) && Convert.ToDouble(a.Mass) < pbms.CurrentGap).ToList() :
                                                  new List<ModificationList>();

                                foreach (var mod in possiblemodlist)
                                {
                                    localmonomasses = monomasslist.Where(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1).ToList();
                                    possiblemods.Add(new PossibleMods()
                                    {
                                        Sequence = pbms.Sequence + aaa + "[" + (mod.Mass.Contains("-") ? "-" : "+") + mod.Abbreviation + "]",
                                        CurrentMonomass = Convert.ToDouble(mod.Mass) + currentmonomass,
                                        HasMonoMass = true,
                                        ModGap = Convert.ToDouble(mod.Mass),
                                        MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1),
                                        CurrentGap = pbms.CurrentGap - Convert.ToDouble(mod.Mass),
                                        AllMonons = localmonomasses,
                                        HasMod = true,
                                        StartMonomass = pbms.StartMonomass,
                                        NumberofMods = pbms.NumberofMods + 1,
                                        TotalKnownMods = pbms.TotalKnownMods + 1,
                                        hasremainingstartgap = pbms.hasremainingstartgap
                                    });
                                }
                            }
                            else
                            {
                                possiblemodlist = App.AllValidationModifications.Any(a => (a.AminoAcids.Any(b => b.ToLower() == ("n-term")) || a.AminoAcids.Any(c => c == aa)) && (Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) < (PPM.CurrentPPM(Convert.ToDouble(pbms.CurrentMonomass)) + 0.1))) ?
                                                  App.AllValidationModifications.Where(a => (a.AminoAcids.Any(b => b.ToLower() == "n-term") || (a.AminoAcids.Any(c => c == aa))) && (Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) < (PPM.CurrentPPM(Convert.ToDouble(pbms.CurrentMonomass)) + 0.1))).ToList() :
                                                  new List<ModificationList>();

                                currentmonomass = pbms.CurrentMonomass;

                                if (possiblemodlist.Count > 0)
                                {
                                    foreach (var mod in possiblemodlist)
                                    {
                                        localmonomasses = monomasslist.Where(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1).ToList();
                                        possiblemods.Add(new PossibleMods()
                                        {
                                            Sequence = pbms.Sequence + aaa + "[" + (mod.Mass.Contains("-") ? "-" : "+") + mod.Abbreviation + "]",
                                            CurrentMonomass = Convert.ToDouble(mod.Mass) + currentmonomass,
                                            HasMonoMass = true,
                                            ModGap = Convert.ToDouble(mod.Mass),
                                            MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1),
                                            CurrentGap = pbms.CurrentGap - Convert.ToDouble(mod.Mass),
                                            AllMonons = localmonomasses,
                                            HasMod = true,
                                            StartMonomass = pbms.StartMonomass,
                                            NumberofMods = pbms.NumberofMods + 1,
                                            TotalKnownMods = pbms.TotalKnownMods + 1,
                                            hasremainingstartgap = pbms.hasremainingstartgap
                                        });
                                    }
                                }
                                else
                                {
                                    currentmonomass = pbms.CurrentMonomass;
                                    localmonomasses = monomasslist.Where(a => Math.Abs(a - (Convert.ToDouble(pbms.CurrentGap) + currentmonomass)) <= 0.1).ToList();
                                    possiblemods.Add(new PossibleMods()
                                    {
                                        Sequence = pbms.Sequence + aaa + "[" + (pbms.CurrentGap) + "]",
                                        CurrentMonomass = Convert.ToDouble(pbms.CurrentGap) + currentmonomass,
                                        HasMonoMass = true,
                                        ModGap = Convert.ToDouble(pbms.CurrentGap),
                                        MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(pbms.CurrentGap) + currentmonomass)) <= 0.1),
                                        CurrentGap = 0,
                                        AllMonons = localmonomasses,
                                        HasMod = true,
                                        StartMonomass = pbms.StartMonomass,
                                        NumberofMods = pbms.NumberofMods,
                                        TotalKnownMods = pbms.TotalKnownMods,
                                        hasremainingstartgap = pbms.hasremainingstartgap
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        if (App.AllValidationModifications.Any(a => a.AminoAcids.Any(b => b == aa) && Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) < 0.1))
                        {
                            possiblemodlist = App.AllValidationModifications.Where(a => a.AminoAcids.Any(b => b == aa) && Math.Abs(Convert.ToDouble(a.Mass) - pbms.CurrentGap) < 0.1).ToList();

                        }
                        else if (App.AllValidationModifications.Any(a => a.AminoAcids.Any(b => b == aa) && Convert.ToDouble(a.Mass) < pbms.CurrentGap))
                        {
                            possiblemodlist = App.AllValidationModifications.Where(a => a.AminoAcids.Any(b => b == aa) && Convert.ToDouble(a.Mass) < pbms.CurrentGap).ToList();
                        }
                        else
                        {
                            possiblemodlist = new List<ModificationList>();
                        }
                        currentmonomass = pbms.CurrentMonomass + sequencelength(aaa);
                    }
                    if (addmonomass)
                    {
                        aaa = aa;
                        currentmonomass = pbms.CurrentMonomass + sequencelength(aaa);
                    }

                    List<double> lstvalidmodvalues = new List<double>();
                    lstvalidmodvalues = App.AllValidationModifications.Select(a => Convert.ToDouble(a.Mass)).ToList();

                    List<double> possibleunknownmods = new List<double>();
                    List<double> localmonos = monomasslist.Where(a => (a <= currentmonomass + pbms.CurrentGap + 0.1) && (a >= currentmonomass + 0.1)).ToList();
                    //if (localmonos.Any(a => Math.Abs(a - (currentmonomass + sequencelength(aa))) < pbms.CurrentGap))
                    if (!onlyknownmods && Doesnthaveonlyoneaminoacid)
                    {
                        foreach (double monomass in localmonos) ///.Where(a => Math.Abs(a - (currentmonomass + sequencelength(aa))) < pbms.CurrentGap).ToList())
                        {
                            double mm = monomass - currentmonomass;
                            if (Math.Abs(pbms.CurrentGap - mm) >= pbms.CurrentGap) continue;

                            if (lstvalidmodvalues.Any(a => Math.Abs(a - mm) <= PPM.CurrentPPMbasedonMatchList(mm, Properties.Settings.Default.MassTolerancePPM))) continue;

                            possiblemods.Add(new PossibleMods()
                            {
                                Sequence = pbms.Sequence + aaa + "[" + (Math.Round(mm, 4)) + "]",
                                CurrentMonomass = mm + currentmonomass,
                                HasMonoMass = true,
                                ModGap = mm,
                                MonosCount = pbms.MonosCount + 1,
                                CurrentGap = pbms.CurrentGap - mm,
                                AllMonons = localmonomasses,
                                HasMod = true,
                                StartMonomass = pbms.StartMonomass,
                                NumberofMods = pbms.NumberofMods + 1,
                                TotalKnownMods = pbms.TotalKnownMods,
                                hasremainingstartgap = pbms.hasremainingstartgap
                            });
                        }
                    }
                    bool verified = false;
                    // If there are any mods
                    if (possiblemodlist.Count > 0)
                    {
                        bool anymods = false;
                        //Check if any of mods fit any of the monomasses
                        foreach (var mod in possiblemodlist)
                        {
                            if (monomasslist.Any(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1))
                            {
                                localmonomasses = monomasslist.Where(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1).ToList();
                                possiblemods.Add(new PossibleMods()
                                {
                                    Sequence = pbms.Sequence + aaa + "[" + (mod.Mass.Contains("-") ? "-" : "+") + mod.Abbreviation + "]",
                                    CurrentMonomass = Convert.ToDouble(mod.Mass) + currentmonomass,
                                    HasMonoMass = true,
                                    ModGap = Convert.ToDouble(mod.Mass),
                                    MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - (Convert.ToDouble(mod.Mass) + currentmonomass)) <= 0.1),/// 1,
                                    CurrentGap = pbms.CurrentGap - Convert.ToDouble(mod.Mass),
                                    AllMonons = localmonomasses,
                                    HasMod = true,
                                    StartMonomass = pbms.StartMonomass,
                                    NumberofMods = pbms.NumberofMods + 1,
                                    TotalKnownMods = pbms.TotalKnownMods + 1,
                                    hasremainingstartgap = pbms.hasremainingstartgap
                                });
                                anymods = true;
                            }
                            //Apart from the mods check if the current aminoacid fits any of the monomasses
                            if (!verified)
                            {
                                if (monomasslist.Any(a => Math.Abs(a - currentmonomass) <= 0.1))
                                {
                                    localmonomasses = (monomasslist.Where(a => Math.Abs(a - currentmonomass) <= 0.1).ToList());
                                    possiblemods.Add(new PossibleMods()
                                    {
                                        Sequence = pbms.Sequence + aaa,
                                        CurrentMonomass = currentmonomass,
                                        HasMonoMass = true,
                                        ModGap = 0,
                                        MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - currentmonomass) <= 0.1),
                                        CurrentGap = pbms.CurrentGap,
                                        AllMonons = localmonomasses,
                                        HasMod = false,
                                        StartMonomass = pbms.StartMonomass,
                                        NumberofMods = pbms.NumberofMods,
                                        TotalKnownMods = pbms.TotalKnownMods,
                                        hasremainingstartgap = pbms.hasremainingstartgap
                                    });
                                    verified = true;
                                }
                            }
                            // If there are no mods present and there are no monomasses which fit the amino acid.
                            if (!verified)
                            {
                                if (!anymods)
                                {
                                    if (!(monomasslist.Any(a => Math.Abs(a - currentmonomass) <= 0.1)))
                                    {
                                        possiblemods.Add(new PossibleMods()
                                        {
                                            Sequence = pbms.Sequence + aaa,
                                            CurrentMonomass = currentmonomass,
                                            HasMonoMass = false,
                                            ModGap = 0,
                                            MonosCount = pbms.MonosCount,
                                            CurrentGap = pbms.CurrentGap,
                                            HasMod = false,
                                            StartMonomass = pbms.StartMonomass,
                                            TotalKnownMods = pbms.TotalKnownMods,
                                            NumberofMods = pbms.NumberofMods,
                                            hasremainingstartgap = pbms.hasremainingstartgap
                                        });
                                        verified = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (!verified) // If no possible modifications
                    {
                        if (monomasslist.Any(a => Math.Abs(a - currentmonomass) <= 0.1)) // Check if any of the monomasses fit the amino acid
                        {
                            localmonomasses = monomasslist.Where(a => Math.Abs(a - currentmonomass) <= 0.1).ToList();
                            possiblemods.Add(new PossibleMods()
                            {
                                Sequence = pbms.Sequence + aaa,
                                CurrentMonomass = currentmonomass,
                                HasMonoMass = true,
                                ModGap = 0,
                                MonosCount = pbms.MonosCount + monomasslist.Count(a => Math.Abs(a - currentmonomass) <= 0.1),
                                CurrentGap = pbms.CurrentGap,
                                HasMod = false,
                                StartMonomass = pbms.StartMonomass,
                                NumberofMods = pbms.NumberofMods,
                                TotalKnownMods = pbms.TotalKnownMods,
                                hasremainingstartgap = pbms.hasremainingstartgap
                            });
                            verified = true;
                        }
                        else  // If no monomass fit the amino acid
                        {
                            possiblemods.Add(new PossibleMods()
                            {
                                Sequence = pbms.Sequence + aaa,
                                CurrentMonomass = currentmonomass,
                                HasMonoMass = false,
                                ModGap = 0,
                                MonosCount = pbms.MonosCount,
                                CurrentGap = pbms.CurrentGap,
                                HasMod = false,
                                StartMonomass = pbms.StartMonomass,
                                TotalKnownMods = pbms.TotalKnownMods,
                                NumberofMods = pbms.NumberofMods,
                                hasremainingstartgap = pbms.hasremainingstartgap
                            });
                            verified = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return possiblemods;
        }

        /// <summary>
        /// Class to represent modifications
        /// </summary>
        public class PossibleMods
        {
            public bool hasremainingstartgap { get; set; }

            public int Currentposition { get; set; }

            int _modificationlength = 0;

            public int TotalNumberofModifiedMatchAminoAcids { get; set; }

            public int Currentpositionatend { get; set; }

            /// <summary>
            /// Modification length based on the current modification
            /// </summary>
            public int modificationlength
            {
                get
                {
                    return _modificationlength;
                }
                set
                {
                    _modificationlength = value;
                }
            }

            /// <summary>
            /// AllMonos found in the current sequence
            /// </summary>
            public List<double> AllMonons { get; set; }
            /// <summary>
            /// Total Intensity
            /// </summary>
            public double Intensity { get; set; }
            /// <summary>
            /// Total BandYIonPercent based on the currentmonomasses
            /// </summary>
            public double BandYIonPercent { get; set; }
            /// <summary>
            /// BIonPercent
            /// </summary>
            public double BIonPercent { get; set; }
            /// <summary>
            /// YIonPercent
            /// </summary>
            public double YIonPercent { get; set; }
            /// <summary>
            /// Total sequence so far
            /// </summary>
            public string Sequence { get; set; }
            /// <summary>
            /// The current monomass
            /// </summary>
            public double CurrentMonomass { get; set; }
            /// <summary>
            /// Checks if the current sequence has a mono
            /// </summary>
            public bool HasMonoMass { get; set; }
            /// <summary>
            /// Total monos count
            /// </summary>
            public int MonosCount { get; set; }
            /// <summary>
            /// Total Modification gap
            /// </summary>
            public double ModGap { get; set; }
            /// <summary>
            /// If true it means the current sequence has a mod
            /// </summary>
            public bool HasMod { get; set; }
            /// <summary>
            /// Total gap so far
            /// </summary>
            public double CurrentGap { get; set; }
            /// <summary>
            /// The startmonomass
            /// </summary>
            public double StartMonomass { get; set; }
            /// <summary>
            /// Total Number of modifications
            /// </summary>
            public int NumberofMods
            {
                get;
                set;
            }
            /// <summary>
            /// Total Number of known mods in the current sequence
            /// </summary>
            public int TotalKnownMods
            {
                get;
                set;
            }
            /// <summary>
            /// Total number of Water losses
            /// </summary>
            public int TotalWaterLosses
            {
                get;
                set;
            }
            /// <summary>
            /// Total number of Ammonia losses
            /// </summary>
            public int TotalAmmoniaLosses
            {
                get;
                set;
            }
            /// <summary>
            /// Finds the Median Error by substracting the Median of the bandyionerrors with the ActualError
            /// </summary>
            public double MedianofMedianbandyionerrors
            {
                get;
                set;
            }
            /// <summary>
            /// If there is any mod at the end it should set as true
            /// </summary>
            public bool ModNorCTerm
            {
                get;
                set;
            }

            public double MedianofErrors
            {
                get;
                set;
            }

            public double BandYIonPercentDividebyMedianofMedianbandyionerrors
            {
                get;
                set;
            }

            public List<double> MedianofMedianErrorslist
            {
                get;
                set;
            }

            public double RootmeanSquareError
            {
                get;
                set;
            }


            public double NumberofErrorsNearMedian
            {
                get;
                set;
            }

            public double AverageError
            {
                get;
                set;
            }

            public override string ToString()
            {
                return "Sequence: " + Sequence + " Average Error: " + AverageError + " TotalKnownMods: " + TotalKnownMods + " BandYIonPercent: " + BandYIonPercent;
            }

            public double AverageErrorPPM
            {
                get;
                set;
            }

        }

        public class SequenceandMedianofmedianerrors
        {
            public string Mod
            {
                get;
                set;
            }

            public double MedianofMedian
            {
                get;
                set;
            }


            public double MedianofMedianErrorsdivision
            {
                get;
                set;
            }
        }

        public class ValidatePrt
        {
            public MatchStartEnds AnchorTag
            {
                get;
                set;
            }

            public string blasttag
            {
                get;
                set;
            }

            public bool reverseblast
            {
                get;
                set;
            }

            public bool anchortagblast
            {
                get;
                set;
            }

            public string RawSequence
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Finds the Anchortag for the given Protein
        /// </summary>
        /// <param name="Protein"></param>
        /// <returns></returns>
        public static ValidatePrt FindAnchorTag(SearchResult Protein, ref MatchStartEnds AnchorTag)
        {
            string rawsequence = string.Empty;
            var sequencetoshow = Protein.matchstartends.OrderBy(a => a.Start).Where(a => a.confidence != AminAcidConfidence.NotPossible && a.confidence != AminAcidConfidence.Reallybad).ToList();
            //bool sequencedirection = sequencetoshow.First().MainSequence;
            ValidatePrt anchrtag = new ValidatePrt();
            var selecteditem = Protein;
            //anchrtag.AnchorTag = new MatchStartEnds();
            //anchrtag.AnchorTag.SequenceTag = Protein.AnchorTag.Sequence;
            //anchrtag.AnchorTag.StartMonoMass = Protein.AnchorTag.Start;
            //anchrtag.AnchorTag.EndMonoMass = Protein.AnchorTag.End;
            //anchrtag.AnchorTag.Start = Protein.AnchorTag.StartStart;
            //anchrtag.AnchorTag.End = Protein.AnchorTag.EndEnd;
            try
            {
                if (selecteditem.AnchorTag == null)
                {
                    return new ValidatePrt();
                }
            }
            catch (Exception)
            {
                return new ValidatePrt();
            }
            List<GapswithIndex> gaps = new List<GapswithIndex>();

            var gps = selecteditem.matchstartends.Where(a => a.confidence == AminAcidConfidence.Gap).Select(a => a).ToList();
            anchrtag.AnchorTag = AnchorTag;
            string sequencewithgaps = string.Empty;

            double initgap = 0;

            double fnlgap = 0;
            bool reverseblast = false;
            bool anchortagblast = false;
            //MatchStartEnds AnchorTag = new MatchStartEnds();
            string blasttagfortopprotein = selecteditem.AnchorTag.Sequence != null ? selecteditem.AnchorTag.Sequence : "";
            string blasttag = selecteditem.BlastTag != null ? selecteditem.BlastTag : "";
            if (blasttag != "" && selecteditem != null)
            {
                if (selecteditem.Sequence.Replace('I', 'L').Contains(blasttag))
                {
                    if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && (a.SequenceTag.Replace('I', 'L') == blasttag)).Any())
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && (a.SequenceTag.Replace('I', 'L') == blasttag)).First();
                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L') == anchrtag.AnchorTag.SequenceTag)).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L') == anchrtag.AnchorTag.SequenceTag)).Firstt().RawSequence;

                        }
                        else if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag)))).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag)))).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                    }
                    else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && (a.SequenceTag.Replace('I', 'L').Contains(blasttag))).Any())
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && (a.SequenceTag.Replace('I', 'L').Contains(blasttag))).First();
                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains(anchrtag.AnchorTag.SequenceTag))).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains(anchrtag.AnchorTag.SequenceTag))).Firstt().RawSequence;
                        }
                        else if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag)))).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag)))).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                    }
                    else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && (a.SequenceTag.Replace('I', 'L') == (ReverseString.Reverse(blasttag)))).Any())
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && (a.SequenceTag.Replace('I', 'L') == (ReverseString.ReverseStr(blasttag)))).First();
                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L') == ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag))).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L') == ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag))).Firstt().RawSequence;
                        }
                        else if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L') == (anchrtag.AnchorTag.SequenceTag))).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L') == (anchrtag.AnchorTag.SequenceTag))).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                        anchrtag.reverseblast = true;
                    }
                    else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && (a.SequenceTag.Replace('I', 'L').Contains(ReverseString.Reverse(blasttag)))).Any())
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && (a.SequenceTag.Replace('I', 'L').Contains(ReverseString.ReverseStr(blasttag)))).First();
                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag)))).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag)))).Firstt().RawSequence;
                        }
                        else if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains((anchrtag.AnchorTag.SequenceTag)))).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && (a.Sequence.Replace('I', 'L').Contains((anchrtag.AnchorTag.SequenceTag)))).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                        anchrtag.reverseblast = true;
                    }

                    if (anchrtag.AnchorTag == null)
                    {
                        anchrtag.AnchorTag = new MatchStartEnds();
                    }

                    if (anchrtag.AnchorTag.StartMonoMass == 0 && anchrtag.AnchorTag.EndMonoMass == 0)
                    {
                        //if (selecteditem.Sequence.Replace("I", "L").Contains(selecteditem.BlastedTagForTopProtein))
                        //{
                        if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == (selecteditem.BlastedTagForTopProtein)).Any())
                        {
                            //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == (selecteditem.BlastedTagForTopProtein)).First();
                            if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == (anchrtag.AnchorTag.SequenceTag)).Any())
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == (anchrtag.AnchorTag.SequenceTag)).Firstt().RawSequence;
                            }
                            else
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                            }
                            anchrtag.blasttag = selecteditem.BlastedTagForTopProtein;
                        }
                        else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L').Contains(selecteditem.BlastedTagForTopProtein)).Any())
                        {
                            //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L').Contains(selecteditem.BlastedTagForTopProtein)).First();
                            if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L').Contains(anchrtag.AnchorTag.SequenceTag)).Any())
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L').Contains(anchrtag.AnchorTag.SequenceTag)).Firstt().RawSequence;
                            }
                            else
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                            }
                            anchrtag.blasttag = selecteditem.BlastedTagForTopProtein;
                        }
                        else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && selecteditem.BlastedTagForTopProtein.Contains(a.SequenceTag.Replace('I', 'L'))).Any())
                        {
                            //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && selecteditem.BlastedTagForTopProtein.Contains(a.SequenceTag.Replace('I', 'L'))).First();
                            if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && anchrtag.AnchorTag.SequenceTag.Contains(a.Sequence.Replace('I', 'L'))).Any())
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && anchrtag.AnchorTag.SequenceTag.Contains(a.Sequence.Replace('I', 'L'))).Firstt().RawSequence;
                            }
                            else
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                            }
                            anchrtag.blasttag = selecteditem.BlastedTagForTopProtein;
                        }
                        //}
                        //else if (selecteditem.Sequence.Replace("I", "L").Contains(ReverseString.ReverseStr(selecteditem.BlastedTagForTopProtein)))
                        //{
                        else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == ReverseString.Reverse(selecteditem.BlastedTagForTopProtein)).Any())
                        {
                            //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == ReverseString.ReverseStr(selecteditem.BlastedTagForTopProtein)).First();
                            if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag)).Any())
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag)).Firstt().RawSequence;
                            }
                            else
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                            }
                            anchrtag.blasttag = selecteditem.BlastedTagForTopProtein;
                        }
                        else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L').Contains(ReverseString.Reverse(selecteditem.BlastedTagForTopProtein))).Any())
                        {
                            //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L').Contains(ReverseString.ReverseStr(selecteditem.BlastedTagForTopProtein))).First();
                            if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag))).Any())
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(anchrtag.AnchorTag.SequenceTag))).Firstt().RawSequence;
                            }
                            else
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;

                            }
                            anchrtag.blasttag = selecteditem.BlastedTagForTopProtein;
                        }
                        else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && selecteditem.BlastedTagForTopProtein.Contains(ReverseString.Reverse(a.SequenceTag.Replace('I', 'L')))).Any())
                        {
                            //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && selecteditem.BlastedTagForTopProtein.Contains(ReverseString.ReverseStr(a.SequenceTag.Replace('I', 'L')))).First();
                            if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && anchrtag.AnchorTag.SequenceTag.Contains(ReverseString.Reverse(a.Sequence.Replace('I', 'L')))).Any())
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && anchrtag.AnchorTag.SequenceTag.Contains(ReverseString.Reverse(a.Sequence.Replace('I', 'L')))).Firstt().RawSequence;
                            }
                            else
                            {
                                anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                            }
                            anchrtag.blasttag = selecteditem.BlastedTagForTopProtein;
                        }
                        //}
                    }
                }
                else if (selecteditem.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(selecteditem.BlastTag)))
                {
                    if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == selecteditem.BlastTag).Any())
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == selecteditem.BlastTag).First();

                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == anchrtag.AnchorTag.SequenceTag).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == anchrtag.AnchorTag.SequenceTag).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                    }
                    else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Length >= Properties.Settings.Default.SequenceTagLength && selecteditem.BlastTag.Contains(a.SequenceTag.Replace('I', 'L'))).Any()) ///.Contains(selecteditem.BlastTag) 
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Length >= Properties.Settings.Default.SequenceTagLength && selecteditem.BlastTag.Contains(a.SequenceTag.Replace('I', 'L'))).First(); ///.Contains(selecteditem.BlastTag)


                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Length >= Properties.Settings.Default.SequenceTagLength && anchrtag.AnchorTag.SequenceTag.Contains(a.Sequence.Replace('I', 'L'))).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Length >= Properties.Settings.Default.SequenceTagLength && anchrtag.AnchorTag.SequenceTag.Contains(a.Sequence.Replace('I', 'L'))).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                    }
                    else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L').Contains(selecteditem.BlastTag)).Any())
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L').Contains(selecteditem.BlastTag)).First();
                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L').Contains(anchrtag.AnchorTag.SequenceTag)).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L').Contains(anchrtag.AnchorTag.SequenceTag)).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                    }

                    if (AnchorTag.StartMonoMass == 0 && AnchorTag.EndMonoMass == 0)
                    {
                        if (selecteditem.Sequence.Replace("I", "L").Contains(ReverseString.Reverse(selecteditem.BlastedTagForTopProtein)))
                        {
                            if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == (selecteditem.BlastedTagForTopProtein)).Any())
                            {
                                //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == (selecteditem.BlastedTagForTopProtein)).First();
                                if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == (anchrtag.AnchorTag.SequenceTag)).Any())
                                {
                                    anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == (anchrtag.AnchorTag.SequenceTag)).Firstt().RawSequence;
                                }
                                else
                                {
                                    anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                                }
                            }
                            else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Length >= Properties.Settings.Default.SequenceTagLength && selecteditem.BlastedTagForTopProtein.Contains(a.SequenceTag.Replace('I', 'L'))).Any()) ///.Contains(selecteditem.BlastTag) 
                            {
                                //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Length >= Properties.Settings.Default.SequenceTagLength && selecteditem.BlastedTagForTopProtein.Contains(a.SequenceTag.Replace('I', 'L'))).First(); ///.Contains(selecteditem.BlastTag)
                                if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Length >= Properties.Settings.Default.SequenceTagLength && anchrtag.AnchorTag.SequenceTag.Contains(a.Sequence.Replace('I', 'L'))).Any())
                                {
                                    anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Length >= Properties.Settings.Default.SequenceTagLength && anchrtag.AnchorTag.SequenceTag.Contains(a.Sequence.Replace('I', 'L'))).Firstt().RawSequence;
                                }
                                else
                                {
                                    anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                                }
                            }
                        }
                        else if (selecteditem.Sequence.Replace("I", "L").Contains(selecteditem.BlastedTagForTopProtein))
                        {
                            if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == (selecteditem.BlastedTagForTopProtein)).Any())
                            {
                                //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == (selecteditem.BlastedTagForTopProtein)).First();
                                if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == (anchrtag.AnchorTag.SequenceTag)).Any())
                                {
                                    anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == (anchrtag.AnchorTag.SequenceTag)).Firstt().RawSequence;
                                }
                                else
                                {
                                    anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                                }
                            }
                            else if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Length >= Properties.Settings.Default.SequenceTagLength && selecteditem.BlastedTagForTopProtein.Contains(a.SequenceTag.Replace('I', 'L'))).Any()) ///.Contains(selecteditem.BlastTag) 
                            {
                                //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Length >= Properties.Settings.Default.SequenceTagLength && selecteditem.BlastedTagForTopProtein.Contains(a.SequenceTag.Replace('I', 'L'))).First(); ///.Contains(selecteditem.BlastTag)

                                if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Length >= Properties.Settings.Default.SequenceTagLength && anchrtag.AnchorTag.SequenceTag.Contains(a.Sequence.Replace('I', 'L'))).Any())
                                {
                                    anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Length >= Properties.Settings.Default.SequenceTagLength && anchrtag.AnchorTag.SequenceTag.Contains(a.Sequence.Replace('I', 'L'))).Firstt().RawSequence;
                                }
                                else
                                {
                                    anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                                }
                            }
                        }
                    }

                }
            }
            else if (blasttagfortopprotein != "" && blasttagfortopprotein != null)
            {
                if (selecteditem.Sequence.Replace('I', 'L').Contains(blasttagfortopprotein))
                {
                    if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == blasttagfortopprotein).Any())
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == blasttagfortopprotein).First();
                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == anchrtag.AnchorTag.SequenceTag).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == anchrtag.AnchorTag.SequenceTag).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                        anchrtag.anchortagblast = true;
                    }
                    else
                    {
                        anchrtag.AnchorTag = selecteditem.matchstartends.Any(a => a.confidence == AminAcidConfidence.Sure) ? selecteditem.matchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).OrderByDescending(a => a.Length).Firstt() : selecteditem.matchstartends.First(a => a.confidence == AminAcidConfidence.NotPossible);
                        //anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.co != null && a.Sequence.Replace('I', 'L')).First().RawSequence;
                        anchrtag.anchortagblast = true;
                    }
                }
                else if (selecteditem.Sequence.Replace('I', 'L').Contains(ReverseString.Reverse(blasttagfortopprotein)))
                {
                    if (selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == blasttagfortopprotein).Any())
                    {
                        //anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null && a.SequenceTag.Replace('I', 'L') == blasttagfortopprotein).First();

                        if (selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == anchrtag.AnchorTag.SequenceTag).Any())
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = selecteditem.AllsqsTags.Where(a => a.Sequence != null && a.Sequence.Replace('I', 'L') == anchrtag.AnchorTag.SequenceTag).Firstt().RawSequence;
                        }
                        else
                        {
                            anchrtag.AnchorTag.RawSequenceTag = anchrtag.RawSequence = string.Empty;
                        }
                        anchrtag.anchortagblast = true;
                    }
                    else
                    {
                        anchrtag.AnchorTag = selecteditem.matchstartends.Any(a => a.confidence == AminAcidConfidence.Sure) ? selecteditem.matchstartends.Where(a => a.confidence == AminAcidConfidence.Sure).OrderByDescending(a => a.Length).Firstt() : selecteditem.matchstartends.First(a => a.confidence == AminAcidConfidence.NotPossible);
                        anchrtag.anchortagblast = true;
                    }
                }
            }

            if (anchrtag.AnchorTag == null || (anchrtag.AnchorTag.Start == 0 && anchrtag.AnchorTag.End == 0))
            {
                anchrtag.AnchorTag = selecteditem.matchstartends.Where(a => a.SequenceTag != null).OrderByDescending(a => a.SequenceTag.Length).Where(a => a.confidence == AminAcidConfidence.Sure).Any() ?
                            selecteditem.matchstartends.Where(a => a.SequenceTag != null).OrderByDescending(a => a.SequenceTag.Length).Where(a => a.confidence == AminAcidConfidence.Sure).Firstt() :
                            selecteditem.matchstartends.Where(a => a.SequenceTag != null).OrderByDescending(a => a.SequenceTag.Length).Firstt();

                if (anchrtag.blasttag != null && anchrtag.blasttag != "")
                {
                    if (ReverseString.Reverse(anchrtag.blasttag).Contains(anchrtag.AnchorTag.SequenceTag))
                    {
                        anchrtag.reverseblast = true;
                    }
                }

                if (anchrtag.AnchorTag.Start == 0 && anchrtag.AnchorTag.End == 0)
                {
                    return new ValidatePrt();
                }
            }
            return anchrtag;
        }

        public static string ReverseRawString(string seq)
        {
            string reversestring = string.Empty;
            Regex regex = new Regex("[a-z]");

            for (int i = 0; i < seq.Length; i++)
            {
                if (i != seq.Length - 1 && regex.IsMatch(Convert.ToString(seq[i + 1])))
                {
                    reversestring = Convert.ToString(seq[i]) + Convert.ToString(seq[i + 1]) + reversestring;
                    i = i + 1;
                }
                else
                {
                    reversestring = seq[i] + reversestring;
                }
            }
            return reversestring;
        }

        /// <summary>
        /// Extracts the Sequence from the SequenceSearch with delta masses and gaps at the end.
        /// </summary>
        /// <param name="Protein"></param>
        public static string ValidateProteinSequence(SearchResult Protein, SortedList<char, double> aminoacids, double parentmass, Dictionary<double, double> mnmasslist, bool HaveKnowMods, ref List<PossibleMods> psdms, bool fromautoscan = false, string activation = "CID")
        {
            string Sequencetext = string.Empty;
            psdms = new List<PossibleMods>();
            try
            {
                if (Math.Abs(Protein.DeltaMassVersusProtein) < 0.1 || fromautoscan)
                {
                    return ReturnConstructedProteinSequence(Protein);
                }

                var boundtext = Protein.Sequence;

                for (int i = 0; i < Protein.InternalMT.Count; i++)
                {
                    if (Protein.InternalMT[i].SequenceTag != null && Protein.InternalMT[i].SequenceTag != string.Empty)
                    {
                        Protein.InternalMT[i].SequenceTag = boundtext.Substring(Protein.InternalMT[i].Start, Protein.InternalMT[i].End - Protein.InternalMT[i].Start);
                        //Protein.InternalMT[i].SequenceTag = Protein.InternalMT[i].MainSequence ? (boundtext.Substring(Protein.InternalMT[i].Start, Protein.InternalMT[i].End - Protein.InternalMT[i].Start)) : ReverseString.Reverse(boundtext.Substring(Protein.InternalMT[i].Start, Protein.InternalMT[i].End - Protein.InternalMT[i].Start));
                    }
                }

                for (int i = 0; i < Protein.matchstartends.Count; i++)
                {
                    if (Protein.matchstartends[i].SequenceTag != null && Protein.InternalMT[i].SequenceTag != string.Empty)
                    {
                        Protein.matchstartends[i].SequenceTag = boundtext.Substring(Protein.matchstartends[i].Start, Protein.matchstartends[i].End - Protein.matchstartends[i].Start);
                        //Protein.matchstartends[i].SequenceTag = Protein.matchstartends[i].MainSequence ? (boundtext.Substring(Protein.matchstartends[i].Start, Protein.matchstartends[i].End - Protein.matchstartends[i].Start)) : ReverseString.Reverse(boundtext.Substring(Protein.matchstartends[i].Start, Protein.matchstartends[i].End - Protein.matchstartends[i].Start));
                    }
                }


                //for (int i = 0; i < Protein.matchstartends.Count; i++)
                //{
                //    Protein.matchstartends[i]
                //}

                double Mass = 0;
                Mass = parentmass;
                var notsureaminoacids = Protein.InternalMT.Where(a => a.confidence == AminAcidConfidence.NotSure).ToList();
                string coveredsequence = Protein.Sequence;
                var sequencetoshow = Protein.matchstartends.OrderBy(a => a.Start).Where(a => a.confidence != AminAcidConfidence.NotPossible && a.confidence != AminAcidConfidence.Reallybad).ToList();
                if (sequencetoshow.Count == 0) return "";

                Dictionary<double, double> monomasslist = new Dictionary<double, double>();

                foreach (var mnmass in mnmasslist.OrderByDescending(a => a.Key))
                {
                    monomasslist.Add(mnmass.Key, mnmass.Value);
                }

                var selecteditem = new SearchResult();

                //var AnchorTag = Protein.InternalMT.Where(a => a.confidence == Confidence.Sure).OrderByDescending(a => a.SequenceTag.Length).Firstt();

                var AnchorTag = Protein.InternalMT.Any(a => a.SequenceTag == Protein.AnchorTag.Sequence) ? Protein.InternalMT.First(a => a.SequenceTag == Protein.AnchorTag.Sequence) : Protein.InternalMT.Where(a => a.confidence == AminAcidConfidence.Sure).OrderByDescending(a => a.SequenceTag.Length).Firstt();

                try
                {
                    if (Protein.AnchorTag == null)
                    {
                        if (AnchorTag != null)
                        {
                            Protein.AnchorTag = new FindSequenceTags.SequenceTag();
                            Protein.AnchorTag.RawSequence = AnchorTag.SequenceTag;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
                catch (Exception)
                {
                    if (AnchorTag != null)
                    {
                        Protein.AnchorTag = new FindSequenceTags.SequenceTag();
                        Protein.AnchorTag.RawSequence = AnchorTag.SequenceTag;
                    }
                    else
                    {
                        return "";
                    }
                }

                selecteditem.AnchrTag = new FindSequenceTags.SequenceTag();

                selecteditem.AnchrTag = Protein.AnchorTag;

                selecteditem.AnchrTag = Protein.AnchorTag != null ? Protein.AnchorTag : new FindSequenceTags.SequenceTag();

                selecteditem.End = Protein.End;
                selecteditem.Start = Protein.Start;

                //selecteditem.BlastTag = Protein.BlastTag != null ? Protein.BlastTag : "";
                selecteditem.Sequence = Protein.Sequence != null ? Protein.Sequence : "";
                selecteditem.matchstartends = Protein.matchstartends != null ? Protein.matchstartends : new List<MatchStartEnds>();

                selecteditem.ValidatedSequence = Protein.ValidatedSequence != null ? Protein.ValidatedSequence : "";

                List<GapswithIndex> gaps = new List<GapswithIndex>();

                var gps = selecteditem.matchstartends.Where(a => a.confidence == AminAcidConfidence.Gap).Select(a => a).ToList();

                string sequencewithgaps = string.Empty;

                double initgap = 0;

                double fnlgap = 0;
                bool reverseblast = false;
                bool anchortagblast = false;

                string blasttagfortopprotein = selecteditem.AnchrTag.Sequence != null ? selecteditem.AnchrTag.Sequence : "";
                string blasttag = selecteditem.BlastTag != null ? selecteditem.BlastTag : "";
                int originalsequencetaglength = selecteditem.Sequence.Length;
                ValidatePrt vldprt = new ValidatePrt();
                vldprt = FindAnchorTag(Protein, ref AnchorTag);

                Regex rglwr = new Regex("[a-z]");

                AnchorTag = vldprt.AnchorTag;
                if (!vldprt.AnchorTag.MainSequence)
                {
                    AnchorTag.SequenceTag = rglwr.IsMatch(vldprt.AnchorTag.SequenceTag) ? ReverseRawString(vldprt.AnchorTag.SequenceTag) : ReverseString.Reverse(vldprt.AnchorTag.SequenceTag);
                }
                if (vldprt.RawSequence != null && vldprt.RawSequence != "")
                {
                    AnchorTag.SequenceTag = vldprt.AnchorTag.MainSequence ? vldprt.RawSequence : (rglwr.IsMatch(vldprt.RawSequence) ? ReverseRawString(vldprt.RawSequence) : ReverseString.Reverse(vldprt.RawSequence));
                    AnchorTag.RawSequenceTag = vldprt.AnchorTag.MainSequence ? vldprt.RawSequence : (rglwr.IsMatch(vldprt.RawSequence) ? ReverseRawString(vldprt.RawSequence) : ReverseString.Reverse(vldprt.RawSequence));
                }
                reverseblast = vldprt.reverseblast;
                blasttag = vldprt.blasttag != null ? vldprt.blasttag : (selecteditem.BlastTag != null ? selecteditem.BlastTag : string.Empty);
                anchortagblast = vldprt.anchortagblast;
                reverseblast = vldprt.reverseblast;

                int endAnchorTag = AnchorTag.End;
                int startAnchorTag = AnchorTag.Start;

                int gpscount = gps.Count;
                int gpcount = 0;

                string selecteditemSequence = selecteditem.Sequence;

                if (AnchorTag.RawSequenceTag != null && AnchorTag.RawSequenceTag != "")
                {
                    selecteditemSequence = selecteditem.Sequence.Substring(0, AnchorTag.Start) + AnchorTag.SequenceTag + selecteditem.Sequence.Substring(AnchorTag.End, selecteditem.Sequence.Length - AnchorTag.End);
                    selecteditem.End = selecteditem.End + (selecteditemSequence.Length - originalsequencetaglength);
                    AnchorTag.End = AnchorTag.End + (selecteditemSequence.Length - originalsequencetaglength);
                }

                if (gpscount > 0)
                {
                    int end = 0;
                    int start = 0;
                    start = selecteditem.Start;
                    end = selecteditem.End;
                    foreach (var gp in gps)
                    {
                        gpcount = gpcount + 1;

                        gaps.Add(new GapswithIndex
                        {
                            end = gp.End,
                            start = gp.Start,
                            gap = gp.Gap
                        });

                        if (gp.End <= startAnchorTag)
                        {
                            initgap += gp.Gap;
                        }
                        else if (gp.End > startAnchorTag)
                        {
                            fnlgap += gp.Gap;
                        }
                        //hs gjsl
                        //alglib sdjas j
                        if (App.AllValidationModifications.Any(a => Math.Abs(Convert.ToDouble(a.Mass) - gp.Gap) <= 0.1))
                        {

                            string ama = selecteditemSequence.Substring(gp.Start, 1);

                            if (App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - gp.Gap)).AminoAcids.Any(b => b == ama))
                            {
                                sequencewithgaps += selecteditemSequence.Substring(start - 1, gp.Start - (start - 1)) +
                                                     "(" + selecteditemSequence.Substring(gp.Start, gp.End - (gp.Start)) +
                                                    "["
                                                    + (App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - gp.Gap)).Mass.Contains("-") ?
                                                      ("-" + App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - gp.Gap)).Abbreviation) :
                                                      ("+" + App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - gp.Gap)).Abbreviation))
                                                    + "]" + ")";
                            }
                            else
                            {
                                sequencewithgaps += selecteditemSequence.Substring(start - 1, gp.Start - (start - 1)) + "(" + selecteditemSequence.Substring(gp.Start, gp.End - (gp.Start)) + "[" + (gp.Gap > 0 ? ("+" + gp.Gap) : Convert.ToString(gp.Gap)) + "]" + ")";
                            }
                        }
                        else if (Math.Abs(gp.Gap) > 0.1)
                        {
                            sequencewithgaps += selecteditemSequence.Substring(start - 1, gp.Start - (start - 1)) + "(" + selecteditemSequence.Substring(gp.Start, gp.End - (gp.Start)) + "[" + (gp.Gap > 0 ? ("+" + gp.Gap) : Convert.ToString(gp.Gap)) + "]" + ")";
                        }
                        else
                        {
                            sequencewithgaps += selecteditemSequence.Substring(start - 1, gp.Start - (start - 1)) + (selecteditemSequence.Substring(gp.Start, gp.End - (gp.Start)).Length > 0 ? "(" + selecteditemSequence.Substring(gp.Start, gp.End - (gp.Start)) + ")" : "");
                        }
                        if (gpcount < gpscount)
                        {
                            start = gp.End + 1;
                            end = gp.End + 1;
                        }
                        else
                        {
                            start = gp.End;
                            end = gp.End;
                        }
                    }

                    if (end != selecteditem.End)
                    {
                        sequencewithgaps += selecteditemSequence.Substring(end, selecteditem.End - end - 1);
                    }
                }
                else
                {
                    try
                    {
                        sequencewithgaps = selecteditemSequence.Substring(selecteditem.Start - 1, selecteditem.End - selecteditem.Start);
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }

                int beforeanchortaggaplength = 0; //Need to add this length to the length before the AnchorTag for gaps

                int afteranchortaggaplength = 0;//Need to add this length to the length after the AnchorTag for gaps

                if (gps.Any())
                {
                    if (gps.Any(a => a.End <= AnchorTag.Start))
                    {
                        foreach (var item in gps.Where(a => a.End <= AnchorTag.Start).ToList())
                        {
                            beforeanchortaggaplength += Convert.ToString(item.Gap).Length + ((item.Gap > 0) ? 1 : 0) + 4;
                        }
                    }

                    if (gps.Any(a => a.Start >= AnchorTag.End))
                    {
                        foreach (var item in gps.Where(a => a.Start >= AnchorTag.Start).ToList())
                        {
                            afteranchortaggaplength += Convert.ToString(item.Gap).Length + ((item.Gap > 0) ? 1 : 0) + 4;
                        }
                    }

                }


                double initialgap = 0;
                double finalgap = 0;
                string firstgapstring = string.Empty;
                string lastgapstring = string.Empty;

                MatchStartEnds firstchangeaamtse = new MatchStartEnds();
                MatchStartEnds lastchangeaamtse = new MatchStartEnds();
                selecteditemSequence = selecteditemSequence.Replace("I", "L");
                string initialstring = string.Empty;
                string finalstring = string.Empty;
                int finalstring1 = 0;
                int finalstring2 = 0;
                //HashSet jkhkj 
                int initialstring1 = 0;
                int initialstring2 = 0;

                int currentsqslength = RemoveAllMods(sequencewithgaps).Length;

                initialstring = sequencewithgaps.Substring(0, AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength);


                if (AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength > sequencewithgaps.Length)
                {
                    finalstring = sequencewithgaps.Substring(AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength, sequencewithgaps.Length - (AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength));
                }
                else
                {
                    finalstring = sequencewithgaps.Substring(AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength, sequencewithgaps.Length - (AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength));
                }

                if (!AnchorTag.MainSequence)
                {
                    initialgap = (parentmass - AnchorTag.EndMonoMass) - CalculateBYCZIons.sequencelengthwithmodifications(sequencewithgaps.Substring(0, AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength));

                    if ((AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength) > sequencewithgaps.Length)
                    {
                        finalgap = AnchorTag.StartMonoMass - CalculateBYCZIons.sequencelengthwithmodifications(sequencewithgaps.Substring((AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength), sequencewithgaps.Length - (AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength)));
                    }
                    else
                    {
                        finalgap = AnchorTag.StartMonoMass - CalculateBYCZIons.sequencelengthwithmodifications(sequencewithgaps.Substring(AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength, sequencewithgaps.Length - (AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength)));
                    }
                }
                else
                {
                    initialgap = AnchorTag.StartMonoMass - CalculateBYCZIons.sequencelengthwithmodifications(sequencewithgaps.Substring(0, AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength));

                    if ((AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength) > sequencewithgaps.Length)
                    {
                        finalgap = (parentmass - AnchorTag.EndMonoMass) - CalculateBYCZIons.sequencelengthwithmodifications(sequencewithgaps.Substring(AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength, sequencewithgaps.Length - (AnchorTag.Start - (Protein.Start - 1) + beforeanchortaggaplength)));
                    }
                    else
                    {
                        finalgap = (parentmass - AnchorTag.EndMonoMass) - CalculateBYCZIons.sequencelengthwithmodifications(sequencewithgaps.Substring(AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength, sequencewithgaps.Length - (AnchorTag.SequenceTag.Length + 1 + (AnchorTag.Start - Protein.Start) + beforeanchortaggaplength)));
                    }
                }
                if (notsureaminoacids.Any())
                {
                    firstchangeaamtse = notsureaminoacids.Any(a => a.End <= AnchorTag.Start) ? notsureaminoacids.First(a => a.End <= AnchorTag.Start) : new MatchStartEnds();
                    if (firstchangeaamtse.SequenceTag != null)
                    {
                        //firstchangeaamtse.SequenceTag = firstchangeaamtse.SequenceTag.Replace("I", "L");
                        initialgap = firstchangeaamtse.Gap;
                    }
                    if (firstchangeaamtse.Start == 0 && firstchangeaamtse.End == 0)
                    {
                        initialgap = 0;
                    }
                    firstgapstring = notsureaminoacids.Any(a => a.End <= AnchorTag.Start) ? notsureaminoacids.First(a => a.End <= AnchorTag.Start).SequenceTag : string.Empty;
                    //firstgapstring = notsureaminoacids.Any(a => a.End <= AnchorTag.Start) ? notsureaminoacids.First(a => a.End <= AnchorTag.Start).SequenceTag.Replace("I", "L") : string.Empty;
                    lastchangeaamtse = notsureaminoacids.Any(a => a.Start >= AnchorTag.End) ? notsureaminoacids.First(a => a.Start >= AnchorTag.Start) : new MatchStartEnds();
                    if (lastchangeaamtse.SequenceTag != null)
                    {
                        //lastchangeaamtse.SequenceTag = lastchangeaamtse.SequenceTag.Replace("I", "L");
                        finalgap = lastchangeaamtse.Gap;
                    }
                    if (lastchangeaamtse.Start == 0 && lastchangeaamtse.End == 0)
                    {
                        finalgap = 0;
                    }
                    lastgapstring = notsureaminoacids.Any(a => a.Start >= AnchorTag.End) ? notsureaminoacids.First(a => a.Start >= AnchorTag.Start).SequenceTag : string.Empty;
                    //lastgapstring = notsureaminoacids.Any(a => a.Start >= AnchorTag.End) ? notsureaminoacids.First(a => a.Start >= AnchorTag.Start).SequenceTag.Replace("I", "L") : string.Empty;
                }

                if (Math.Round(finalgap, 0) != 0)
                {
                    if (Math.Abs(finalgap - Molecules.Water) < 0.1)
                    {
                        finalgap = 0;
                    }
                    else
                    {
                        finalgap = finalgap - Molecules.Water;
                    }
                }
                string ChangeableAminoAcids = string.Empty;

                string finalModification = string.Empty;
                string initialModification = string.Empty;

                int startgapaminoacids = 0;
                int endgapaminoacids = 0;
                int startgapendaminoacids = 0;
                if (App.AllValidationModifications.Any(a => Math.Abs(Convert.ToDouble(a.Mass) - finalgap) <= PPM.CurrentPPM(finalgap)))
                {
                    if (App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - finalgap)).AminoAcids.Any(a => a == lastgapstring))
                    {
                        finalModification = App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - finalgap)).Name;
                        finalgap = 0;
                    }
                    else
                    {
                        finalModification = Convert.ToString(finalgap);
                    }

                    string newst = string.Join(",", lastgapstring.ToArray());
                    endgapaminoacids = lastchangeaamtse.Start;

                    List<string> result = newst.Split(',').ToList();

                    foreach (var item in result)
                    {
                        if (App.AllValidationModifications.Any(a => a.Name == finalModification) && App.AllValidationModifications.Where(a => a.Name == finalModification).Firstt().AminoAcids.Any(a => a == item))
                        {
                            ChangeableAminoAcids = App.AllValidationModifications.Where(a => a.Name == finalModification).Firstt().AminoAcids.First(a => a == item);
                            break;
                        }
                        endgapaminoacids = endgapaminoacids + 1;
                    }
                }

                bool modchecked = false;
                string newsequencewithgaps = string.Empty;

                bool firstmodchecked = false;
                bool lastmodchecked = false;
                int indexoffirstchangemtse = 0;
                bool reversesequncefirstchangemtse = false;
                if ((firstgapstring != null && firstgapstring != "") || (lastgapstring != null && lastgapstring != ""))
                {
                    if (firstgapstring != null && firstgapstring != "")
                    {
                        string sqwgps = RemoveAllMods(sequencewithgaps);///.Replace("I", "L");
                        string tempsequencewithgaps = sequencewithgaps;////.Replace("I", "L");
                        //sequencewithgaps = sequencewithgaps.Replace("I", "L");
                        if (tempsequencewithgaps.Contains(firstchangeaamtse.SequenceTag)) ///Checking if there is the gap in the forward direction.
                        {
                            if (firstchangeaamtse.SequenceTag != null)
                            {
                                indexoffirstchangemtse = tempsequencewithgaps.IndexOf((firstchangeaamtse.SequenceTag));

                                if (indexoffirstchangemtse > firstchangeaamtse.Start) /// If the index is greater than the current changemtse start
                                {
                                    indexoffirstchangemtse = tempsequencewithgaps.IndexOf(ReverseString.Reverse(firstchangeaamtse.SequenceTag));
                                }
                            }
                            else
                            {
                                firstchangeaamtse.SequenceTag = string.Empty;
                            }
                            int indexoflastchangemtse = 0;
                            if (lastchangeaamtse.SequenceTag != null)
                            {
                                indexoflastchangemtse = tempsequencewithgaps.LastIndexOf((lastchangeaamtse.SequenceTag));
                                if (indexoflastchangemtse == -1)
                                {
                                    indexoflastchangemtse = tempsequencewithgaps.LastIndexOf(ReverseString.Reverse(lastchangeaamtse.SequenceTag));
                                }
                            }
                            else
                            {
                                lastchangeaamtse.SequenceTag = string.Empty;
                            }
                            psdms = new List<PossibleMods>();
                            firstgapstring = CheckforMods2(sequencewithgaps, firstchangeaamtse.StartMonoMass, firstchangeaamtse.EndMonoMass, lastchangeaamtse.StartMonoMass, lastchangeaamtse.EndMonoMass, monomasslist, initialgap, finalgap, parentmass,
                                                            true, sequencewithgaps, true, indexoffirstchangemtse, indexoffirstchangemtse + firstchangeaamtse.SequenceTag.Length, indexoflastchangemtse, indexoflastchangemtse + lastchangeaamtse.SequenceTag.Length, true, aminoacids, HaveKnowMods, ref psdms);

                            return firstgapstring;
                        }
                        else if (sqwgps.Contains(ReverseString.Reverse(firstchangeaamtse.SequenceTag)))
                        {
                            indexoffirstchangemtse = sequencewithgaps.IndexOf(ReverseString.Reverse(firstchangeaamtse.SequenceTag));
                            reversesequncefirstchangemtse = true;

                            if (firstchangeaamtse.SequenceTag != null)
                            {
                                indexoffirstchangemtse = sequencewithgaps.LastIndexOf(ReverseString.Reverse(firstchangeaamtse.SequenceTag));
                            }
                            else
                            {
                                firstchangeaamtse.SequenceTag = string.Empty;
                            }

                            int indexoflastchangemtse = 0;

                            if (lastchangeaamtse.SequenceTag != null)
                            {
                                indexoflastchangemtse = sequencewithgaps.LastIndexOf(lastchangeaamtse.SequenceTag);
                                if (indexoflastchangemtse == -1)
                                {
                                    indexoflastchangemtse = sequencewithgaps.LastIndexOf(ReverseString.Reverse(lastchangeaamtse.SequenceTag));
                                }
                            }
                            else
                            {
                                lastchangeaamtse.SequenceTag = string.Empty;
                            }
                            psdms = new List<PossibleMods>();

                            firstgapstring = CheckforMods2(sequencewithgaps, firstchangeaamtse.StartMonoMass, firstchangeaamtse.EndMonoMass, lastchangeaamtse.StartMonoMass, lastchangeaamtse.EndMonoMass, monomasslist, initialgap, finalgap, parentmass,
                                                            true, sequencewithgaps, true, indexoffirstchangemtse, indexoffirstchangemtse + firstchangeaamtse.SequenceTag.Length, indexoflastchangemtse, indexoflastchangemtse + lastchangeaamtse.SequenceTag.Length, true, aminoacids, HaveKnowMods, ref psdms);
                            return firstgapstring;
                        }
                        modchecked = true;
                        firstmodchecked = true;
                    }
                    if (lastgapstring != null && lastgapstring != "")
                    {
                        string sqwgps = RemoveAllMods(sequencewithgaps);///.Replace("I", "L");
                        ///adh ash
                        /// daslkjdl
                        /// 
                        string tempsequencewithgaps = sequencewithgaps;///.Replace("I", "L");
                        //sequencewithgaps = sequencewithgaps.Replace("I", "L");
                        if (tempsequencewithgaps.Contains(lastchangeaamtse.SequenceTag))
                        {
                            int indexoflastchangemtse = tempsequencewithgaps.LastIndexOf((lastchangeaamtse.SequenceTag));
                            double startmmss = firstchangeaamtse.StartMonoMass;
                            double endmmss = firstchangeaamtse.EndMonoMass;
                            if (firstchangeaamtse.SequenceTag == null)
                            {
                                firstchangeaamtse.SequenceTag = string.Empty;
                            }
                            if (reversesequncefirstchangemtse && (firstchangeaamtse.SequenceTag != "" && firstchangeaamtse.SequenceTag != null))
                            {
                                startmmss = parentmass - firstchangeaamtse.EndMonoMass;
                                endmmss = parentmass - firstchangeaamtse.StartMonoMass;
                            }
                            psdms = new List<PossibleMods>();
                            lastgapstring = CheckforMods2(sequencewithgaps, startmmss, endmmss, lastchangeaamtse.StartMonoMass, lastchangeaamtse.EndMonoMass, monomasslist, initialgap, finalgap, parentmass,
                                                           true, sequencewithgaps, true, indexoffirstchangemtse, indexoffirstchangemtse + firstchangeaamtse.SequenceTag.Length, indexoflastchangemtse, indexoflastchangemtse + lastchangeaamtse.SequenceTag.Length, true, aminoacids, HaveKnowMods, ref psdms, activation);

                            return lastgapstring;
                        }
                        else if (sequencewithgaps.Contains(ReverseString.Reverse(lastchangeaamtse.SequenceTag)))
                        {
                            int indexoflastchangemtse = sequencewithgaps.LastIndexOf(ReverseString.Reverse(lastchangeaamtse.SequenceTag));

                            double startmmss = firstchangeaamtse.StartMonoMass;
                            double endmmss = firstchangeaamtse.EndMonoMass;

                            if (firstchangeaamtse.SequenceTag == null)
                            {
                                firstchangeaamtse.SequenceTag = string.Empty;
                            }
                            if (reversesequncefirstchangemtse && (firstchangeaamtse.SequenceTag != "" && firstchangeaamtse.SequenceTag != null))
                            {
                                startmmss = parentmass - firstchangeaamtse.EndMonoMass;
                                endmmss = parentmass - firstchangeaamtse.StartMonoMass;
                            }

                            psdms = new List<PossibleMods>();

                            lastgapstring = CheckforMods2(sequencewithgaps, startmmss, endmmss, lastchangeaamtse.StartMonoMass, lastchangeaamtse.EndMonoMass, monomasslist, initialgap, finalgap, parentmass,
                                                            true, sequencewithgaps, true, indexoffirstchangemtse, indexoffirstchangemtse + firstchangeaamtse.SequenceTag.Length, indexoflastchangemtse, indexoflastchangemtse + lastchangeaamtse.SequenceTag.Length, true, aminoacids, HaveKnowMods, ref psdms);

                            return lastgapstring;
                        }
                        modchecked = true;
                        lastmodchecked = true;
                    }
                }
                else if (App.AllValidationModifications.Any(a => Math.Abs(Convert.ToDouble(a.Mass) - initialgap) <= (PPM.CurrentPPM(initialgap) + 0.1)))
                {
                    bool skipcalc = false;
                    if (App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - initialgap)).AminoAcids.Any(a => a == firstgapstring))
                    {
                        initialModification = App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - initialgap)).Name;
                        initialgap = 0;
                    }
                    else if (App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - initialgap)).AminoAcids.Any(a => a.ToLower() == "n-term"))
                    {
                        initialModification = App.AllValidationModifications.MinBy(a => Math.Abs(Convert.ToDouble(a.Mass) - initialgap)).Name;
                        initialgap = 0;
                        startgapaminoacids = 1;
                        startgapendaminoacids = firstgapstring.Length + 1;
                        skipcalc = true;
                    }
                    else
                    {
                        initialModification = Convert.ToString(initialgap);
                    }
                    if (!skipcalc)
                    {
                        string newst = string.Join(",", firstgapstring.ToArray());
                        startgapaminoacids = firstchangeaamtse.Start;
                        startgapendaminoacids = firstchangeaamtse.End;
                        List<string> result = newst.Split(',').ToList();
                        foreach (var item in result)
                        {
                            if (App.AllValidationModifications.Any(a => a.Name == initialModification) && App.AllValidationModifications.Where(a => a.Name == initialModification).Firstt().AminoAcids.Any(a => a == item))
                            {
                                ChangeableAminoAcids = App.AllValidationModifications.Where(a => a.Name == initialModification).Firstt().AminoAcids.First(a => a == item);
                                break;
                            }
                            startgapaminoacids = startgapaminoacids + 1;
                            startgapendaminoacids = startgapendaminoacids + 1;
                        }
                    }
                }

                if (firstmodchecked && lastmodchecked)
                {
                    int startindex = RemoveAllMods(firstgapstring).Length;
                    int endindex = RemoveAllMods(lastgapstring).Length;
                    Sequencetext = firstgapstring + sequencewithgaps.Substring(startindex, sequencewithgaps.Length - (startindex + endindex)) + lastgapstring;
                }
                else if (firstmodchecked)
                {
                    int startindex = RemoveAllMods(firstgapstring).Length;
                    if (finalgap != 0)
                    {
                        Sequencetext = firstgapstring + sequencewithgaps.Substring(startindex, sequencewithgaps.Length - (startindex)) + "[" + Math.Round(finalgap, 3) + "]";
                    }
                    else
                    {
                        Sequencetext = firstgapstring + sequencewithgaps.Substring(startindex, sequencewithgaps.Length - (startindex));
                    }
                }
                else if (lastmodchecked)
                {
                    int endindex = RemoveAllMods(lastgapstring).Length;
                    if (initialgap != 0)
                    {
                        Sequencetext = sequencewithgaps.Substring(0, 1) + "[" + Math.Round(initialgap, 3) + "]" + sequencewithgaps.Substring(1, sequencewithgaps.Length - endindex - 1) + lastgapstring;
                    }
                    else
                    {
                        Sequencetext = sequencewithgaps.Substring(0, sequencewithgaps.Length - endindex) + lastgapstring;
                    }
                }
                else
                {
                    if (initialgap + (finalgap) <= 0.1)
                    {
                        initialgap = finalgap = 0;
                    }

                    if (Math.Abs(initialgap) > 0.1)
                    {
                        string lastsequencewithgaps = sequencewithgaps.Substring(sequencewithgaps.Length - lastgapstring.Length, lastgapstring.Length);
                        string firstsequencewithgaps = sequencewithgaps.Substring(0, firstgapstring.Length);

                        Sequencetext =
                            sequencewithgaps.Substring(0, firstgapstring.Length + 1) +
                            (((Math.Abs(initialgap) > 0.1) && (firstsequencewithgaps.Length > 0)) ? "(" : "") + firstsequencewithgaps +
                                       (((Math.Abs(initialgap) > 0.1) && (firstsequencewithgaps.Length > 0)) ? ")" : "") + (Math.Abs(initialgap) > 0.1 ? "[" + Math.Round(initialgap, 4) + "]" : "") +
                                       sequencewithgaps.Substring(firstgapstring.Length != 0 ? firstgapstring.Length : 1, sequencewithgaps.Length - (firstgapstring.Length != 0 ? firstgapstring.Length : 1) - lastgapstring.Length) +
                                         (((Math.Abs(finalgap) > 0.1) && (lastsequencewithgaps.Length > 0)) ? "(" : "") + lastsequencewithgaps + (((Math.Abs(finalgap) > 0.1) && (lastsequencewithgaps.Length > 0)) ? ")" : "") +
                                         (Math.Abs(finalgap) > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "") +
                                        (finalModification != "" ? "[" +
                                        (App.AllValidationModifications.First(a => a.Name == finalModification).Mass.Contains("-") ?
                                        "-" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation :
                                        "+" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation)
                                        + "]" : "");
                    }
                    else if (initialModification != "")
                    {
                        string firstsequencwithgaps = sequencewithgaps.Substring(0, startgapaminoacids != 0 ? startgapaminoacids - 1 : 0) +
                                        sequencewithgaps.Substring(startgapaminoacids != 0 ? startgapaminoacids - 1 : 0, startgapendaminoacids - startgapaminoacids);
                        string lastsequencwithgaps = sequencewithgaps.Substring(sequencewithgaps.Length - (endgapaminoacids + startgapaminoacids), endgapaminoacids + startgapaminoacids);

                        if (firstsequencwithgaps != "")
                        {
                            Sequencetext = (((initialModification != "") && (firstsequencwithgaps.Length > 0)) ? "(" : "") + firstsequencwithgaps +
                                            (((initialModification != "") && (firstsequencwithgaps.Length > 0)) ? ")" : "")
                                            + (initialModification != "" ? "[" +
                                            (App.AllValidationModifications.First(a => a.Name == initialModification).Mass.Contains("-") ?
                                            "-" + App.AllValidationModifications.First(a => a.Name == initialModification).Abbreviation :
                                            "+" + App.AllValidationModifications.First(a => a.Name == initialModification).Abbreviation)
                                            + "]" : "")
                                            + sequencewithgaps.Substring(startgapendaminoacids != 0 ? startgapendaminoacids - 1 : 0, sequencewithgaps.Length - (startgapendaminoacids + endgapaminoacids))
                                            + (((finalModification != "") && (lastsequencwithgaps.Length > 0)) ? "(" : "") + lastsequencwithgaps
                                            + (((finalModification != "") && (lastsequencwithgaps.Length > 0)) ? ")" : "") +
                                            (Math.Abs(finalgap) > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "") +
                                            (finalModification != "" ? "[" +
                                            (App.AllValidationModifications.First(a => a.Name == finalModification).Mass.Contains("-") ?
                                            "-" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation :
                                            "+" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation)
                                            + "]" : "");
                        }
                        else
                        {

                            string firstsequencetext = (((initialModification != "") && (firstsequencwithgaps.Length > 0)) ? "(" : "") + sequencewithgaps.Substring(0, 1) +
                                                        (((initialModification != "") && (firstsequencwithgaps.Length > 0)) ? ")" : "");

                            string secondsequencetext = initialModification != "" ?
                                                                                    (
                                                                                    (
                                                                                         (App.AllValidationModifications.Any(a => a.Name == initialModification) ?

                                                                                                        (
                                                                                                            App.AllValidationModifications.First(a => a.Name == initialModification).Mass.Contains("-") ?
                                                                                                                ("[" + "-" + App.AllValidationModifications.First(a => a.Name == initialModification).Abbreviation + "]") :
                                                                                                                ("[" + "+" + App.AllValidationModifications.First(a => a.Name == initialModification).Abbreviation + "]")
                                                                                                        )

                                                                                                            :
                                                                                                        (
                                                                                                            ""
                                                                                                        )
                                                                                         )

                                                                                    )
                                                                                    )
                                                                                  :
                                                                                    "";

                            string thirdsequencetext = sequencewithgaps.Substring(1, sequencewithgaps.Length - (startgapendaminoacids + endgapaminoacids) - 1);

                            string fourthsequencetext = (((finalModification != "") && (lastsequencwithgaps.Length > 0)) ? "(" : "") + lastsequencwithgaps
                                            + (((finalModification != "") && (lastsequencwithgaps.Length > 0)) ? ")" : "") +
                                            (Math.Abs(finalgap) > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "");

                            string fifthsequencetext = finalModification != "" ? "[" +

                                    (
                                      App.AllValidationModifications.Any(a => a.Name == finalModification) ?
                                            (
                                                (App.AllValidationModifications.First(a => a.Name == finalModification).Mass.Contains("-") ?
                                                 ("[-" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation + "]") :
                                                ("+" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation + "]")
                                                )
                                                ) :
                                                ("")
                                    )
                                            + "]"

                                            : "";

                            Sequencetext = firstsequencetext + secondsequencetext + thirdsequencetext + fourthsequencetext + fifthsequencetext;
                        }
                    }
                    else
                    {
                        if (reverseblast)
                        {
                            int startingpar = 0;
                            int endingpar = 0;

                            if (notsureaminoacids.Any())
                            {
                                if (notsureaminoacids.Count == 1)
                                {
                                    startingpar = notsureaminoacids.OrderBy(a => a.Start).Firstt().Start;
                                    endingpar = notsureaminoacids.OrderBy(a => a.End).Firstt().End;
                                    Sequencetext = initialstring + AnchorTag.SequenceTag + finalstring + (finalgap > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "");
                                }
                                else
                                {
                                    startingpar = notsureaminoacids.OrderBy(a => a.Start).Firstt().End;
                                    endingpar = notsureaminoacids.OrderBy(a => a.Start).Last().Start;
                                    string firstsequencwithgaps = sequencewithgaps.Substring(sequencewithgaps.Length - endingpar, endingpar);


                                    Sequencetext = sequencewithgaps.Substring(0, startingpar) +
                                                    sequencewithgaps.Substring(startingpar, sequencewithgaps.Length - endingpar - startingpar) +
                                                    (((Math.Abs(finalgap) > 0.1) && (firstsequencwithgaps.Length > 0)) ? "(" : "") + firstsequencwithgaps +
                                                    (((Math.Abs(finalgap) > 0.1) && (firstsequencwithgaps.Length > 0)) ? ")" : "") + (Math.Abs(finalgap) > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "") +
                                                    (finalModification != "" ? "[" +
                                                    (App.AllValidationModifications.First(a => a.Name == finalModification).Mass.Contains("-") ?
                                                    "-" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation :
                                                    "+" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation)
                                                    + "]" : "");
                                }
                            }
                            else
                            {
                                string firstsequencwithgaps = sequencewithgaps.Substring(sequencewithgaps.Length - endingpar, endingpar);


                                Sequencetext = sequencewithgaps.Substring(0, startingpar) +
                                                sequencewithgaps.Substring(startingpar, sequencewithgaps.Length - endingpar - startingpar) +
                                                (((Math.Abs(finalgap) > 0.1) && (firstsequencwithgaps.Length > 0)) ? "(" : "") + firstsequencwithgaps +
                                                (((Math.Abs(finalgap) > 0.1) && (firstsequencwithgaps.Length > 0)) ? ")" : "") + (Math.Abs(finalgap) > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "") +
                                                (finalModification != "" ? "[" +
                                                (App.AllValidationModifications.First(a => a.Name == finalModification).Mass.Contains("-") ?
                                                "-" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation :
                                                "+" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation)
                                                + "]" : "");
                            }
                        }
                        else
                        {
                            int startingpar = 0;
                            int endingpar = 0;
                            if (notsureaminoacids.Any())
                            {
                                if (notsureaminoacids.Count == 1)
                                {
                                    startingpar = notsureaminoacids.OrderBy(a => a.Start).Firstt().Start;
                                    endingpar = notsureaminoacids.OrderBy(a => a.End).Firstt().End;
                                    Sequencetext = initialstring + AnchorTag.SequenceTag + finalstring + (finalgap > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "");
                                }
                                else
                                {
                                    startingpar = notsureaminoacids.OrderBy(a => a.Start).Firstt().End;
                                    endingpar = notsureaminoacids.OrderBy(a => a.Start).Last().Start;


                                    string firstsequencwithgaps = sequencewithgaps.Substring(sequencewithgaps.Length - endingpar, endingpar);


                                    Sequencetext = sequencewithgaps.Substring(0, startingpar) +
                                                    sequencewithgaps.Substring(startingpar, sequencewithgaps.Length - endingpar - startingpar) +
                                                    (((Math.Abs(finalgap) > 0.1) && (firstsequencwithgaps.Length > 0)) ? "(" : "") + firstsequencwithgaps +
                                                    (((Math.Abs(finalgap) > 0.1) && (firstsequencwithgaps.Length > 0)) ? ")" : "") + (Math.Abs(finalgap) > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "") +
                                                    (finalModification != "" ? "[" +
                                                    (App.AllValidationModifications.First(a => a.Name == finalModification).Mass.Contains("-") ?
                                                    "-" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation :
                                                    "+" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation)
                                                    + "]" : "");
                                }
                            }
                            else
                            {
                                string firstsequencwithgaps = sequencewithgaps.Substring(sequencewithgaps.Length - endingpar, endingpar);


                                Sequencetext = sequencewithgaps.Substring(0, startingpar) +
                                                sequencewithgaps.Substring(startingpar, sequencewithgaps.Length - endingpar - startingpar) +
                                                (((Math.Abs(finalgap) > 0.1) && (firstsequencwithgaps.Length > 0)) ? "(" : "") + firstsequencwithgaps +
                                                (((Math.Abs(finalgap) > 0.1) && (firstsequencwithgaps.Length > 0)) ? ")" : "") + (Math.Abs(finalgap) > 0.1 ? "[" + Math.Round(finalgap, 4) + "]" : "") +
                                                (finalModification != "" ? "[" +
                                                (App.AllValidationModifications.First(a => a.Name == finalModification).Mass.Contains("-") ?
                                                "-" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation :
                                                "+" + App.AllValidationModifications.First(a => a.Name == finalModification).Abbreviation)
                                                + "]" : "");
                            }
                        }
                    }
                }
                return Sequencetext;
            }
            catch (Exception ex)
            {
                return ReturnConstructedProteinSequence(Protein);
                //string finalsequence = string.Empty;
                //int i = 0;
                //MatchStartEnds lstmtse = new MatchStartEnds();
                //lstmtse = Protein.InternalMT.OrderBy(a => a.Start).Last();
                //int internalmtlength = 0;
                //if ((lstmtse.SequenceTag == null || lstmtse.SequenceTag == "") && lstmtse.Gap == 0)
                //{
                //    internalmtlength = Protein.InternalMT.Count - 1;
                //}
                //else
                //{
                //    internalmtlength = Protein.InternalMT.Count;
                //}

                //foreach (var item in Protein.InternalMT.OrderBy(a => a.Start))
                //{
                //    if (item.SequenceTag != null)
                //    {
                //        if (item.Gap > 0.1)
                //        {
                //            if (i == internalmtlength - 1) // If it is the last item then should subtract the water
                //            {
                //                finalsequence = finalsequence + "(" + (item.MainSequence ? item.SequenceTag : ReverseString.Reverse(item.SequenceTag)) + ")" + ("[" + Convert.ToString(Math.Round(item.Gap - Molecules.Water, 4)) + "]");
                //            }
                //            else
                //            {
                //                finalsequence = finalsequence + "(" + (item.MainSequence ? item.SequenceTag : ReverseString.Reverse(item.SequenceTag)) + ")" + ("[" + Convert.ToString(Math.Round(item.Gap, 4)) + "]");
                //            }
                //            //finalsequence = finalsequence + "(" + item.SequenceTag + ")" + ("[" + Convert.ToString(item.Gap) + "]");
                //        }
                //        else
                //        {
                //            finalsequence = finalsequence + (item.MainSequence ? item.SequenceTag : ReverseString.Reverse(item.SequenceTag));// item.SequenceTag;
                //        }
                //    }
                //    i = i + 1;
                //}

                //return finalsequence;

                //string stacktrace = ex.StackTrace;
                //stacktrace += ("\nSequence Tag: " + Protein.Sequence + "\t Scan Number: " + "" + "\t Description: " + Protein.Description);
                //App.Laststacktrace = stacktrace;
                //Action throwException = () => { throw ex; };
                //Dispatcher.CurrentDispatcher.BeginInvoke(throwException);
                //return Sequencetext;
            }
        }

        /// <summary>
        /// Returns the Constructed Sequence based on the Protein Sequence which doesn't calculate any mods
        /// </summary>
        /// <param name="Protein"></param>
        /// <returns></returns>
        static string ReturnConstructedProteinSequence(SearchResult Protein)
        {
            string finalsequence = string.Empty;
            int i = 0;
            MatchStartEnds lstmtse = new MatchStartEnds();
            lstmtse = Protein.InternalMT.OrderByDescending(a => a.confidence == AminAcidConfidence.Gap).ThenBy(a => a.Start).Last();

            int internalmtlength = Protein.InternalMT.Where(a => a.SequenceTag != null).Any() ? Protein.InternalMT.Where(a => a.SequenceTag != null).Count() : 0;
            var tempinternalmt = Protein.InternalMT.Where(a => a.SequenceTag != null).OrderByDescending(a => a.confidence == AminAcidConfidence.Gap).OrderBy(a => a.Start).ToList();

            string tempsequence = Protein.Sequence;

            foreach (var item in tempinternalmt)
            {
                if (item.SequenceTag != null)
                {
                    if (item.Gap > 0.1)
                    {
                        if (i == internalmtlength - 1) // If it is the last item then should subtract the water
                        {
                            if (item.Start == item.End)
                            {
                                finalsequence = finalsequence + ("[" + Convert.ToString(Math.Round(item.Gap - Molecules.Water, 4)) + "]");
                            }
                            else
                            {
                                finalsequence = finalsequence + "(" + tempsequence.Substring(item.Start, item.End - item.Start) + ")" + ("[" + Convert.ToString(Math.Round(item.Gap - Molecules.Water, 4)) + "]");
                            }
                            //finalsequence = finalsequence + "(" + (item.MainSequence ? tempsequence.Substring(item.Start, item.End - item.Start) : ReverseString.Reverse(tempsequence.Substring(item.Start, item.End - item.Start))) + ")" + ("[" + Convert.ToString(Math.Round(item.Gap - Molecules.Water, 4)) + "]");
                        }
                        else
                        {
                            if (item.Start == item.End)
                            {
                                finalsequence = finalsequence + ("[" + Convert.ToString(Math.Round(item.Gap, 4)) + "]");
                            }
                            else
                            {
                                finalsequence = finalsequence + "(" + tempsequence.Substring(item.Start, item.End - item.Start) + ")" + ("[" + Convert.ToString(Math.Round(item.Gap, 4)) + "]");
                            }
                            //finalsequence = finalsequence + "(" + (item.MainSequence ? tempsequence.Substring(item.Start, item.End - item.Start) : ReverseString.Reverse(tempsequence.Substring(item.Start, item.End - item.Start))) + ")" + ("[" + Convert.ToString(Math.Round(item.Gap, 4)) + "]");
                        }
                        //finalsequence = finalsequence + "(" + item.SequenceTag + ")" + ("[" + Convert.ToString(item.Gap) + "]");
                    }
                    else
                    {
                        finalsequence = finalsequence + tempsequence.Substring(item.Start, item.End - item.Start);// item.SequenceTag;
                    }
                }
                i = i + 1;
            }
            
            return finalsequence;
        }

        public class GapswithIndex
        {
            public int start
            {
                get;
                set;
            }

            public int end
            {
                get;
                set;
            }

            public double gap
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Remove all the modifications from the sequence.
        /// </summary>
        /// <param name="sequencewithmods"></param>
        /// <returns></returns>
        public static string RemoveAllMods(string sequencewithmods)
        {
            string sequence = sequencewithmods.Replace("(", "").Replace(")", "");

            string tempseq = string.Empty;

            if (!sequence.Contains("["))
            {
                return sequence;
            }

            foreach (var item in sequence.Split(new char[] { '[', ']' }))
            {
                if (App.AllValidationModifications.Any(a => (a.Abbreviation == item) || ("+" + a.Abbreviation == item) || ("-" + a.Abbreviation == item)))
                {

                }
                else if (Regex.IsMatch(item, @"[0-9\[\]\+\-\.]"))
                {

                }
                else
                {
                    tempseq = tempseq + item;
                }
            }

            return tempseq;
        }

        public static int NumberofMods(string sequence)
        {
            string[] splitstring = { "[", "]" };

            string[] splitsequence = sequence.Split(splitstring, StringSplitOptions.RemoveEmptyEntries);

            Regex regexnumbers = new Regex(@"\d+");

            int count = 0;

            for (int i = 0; i < splitsequence.Length; i++)
            {
                if (regexnumbers.IsMatch(splitsequence[i]))
                {
                    count = count + 1;
                }
            }
            return count;
        }

    }
}

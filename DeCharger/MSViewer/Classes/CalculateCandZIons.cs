using Science.Proteomics;
using SignalProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//using static MSViewer.Classes.CalculateBandYIons;

namespace MSViewer.Classes
{
    public class CalculateCandZIons
    {

        public static SortedList<char, double> Aminoacids = new SortedList<char, double>();

        public static SortedList<string, double> AminoAcidMass2 = new SortedList<string, double>();

        private static double sequencelengthwithmods(string sequence)
        {
            double length = 0;

            length = AminoAcidMass2.ContainsKey(sequence) ? AminoAcidMass2[sequence] : double.NaN;

            return Math.Round(length, 4);
        }

        private static double sequencelength(string sequence)
        {
            double length = 0;

            foreach (char c in sequence)
            {
                if (Aminoacids.ContainsKey(c))
                {
                    length = length + Aminoacids[c]; //AminoAcidHelpers.AminoAcidMass3[c];
                }
                else
                {
                    return double.NaN;
                }
            }

            return Math.Round(length, 4);
        }

        public static double LocalParentMass = 0.0;

        /// <summary>
        /// Find the length of a sequence with modification
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static double sequencelengthwithmodifications(string sequence)
        {
            double length = 0;
            string[] splitstring = { "[", "]" };

            string[] sequences = sequence.Replace("(", "").Replace(")", "").Trim().Split(splitstring, StringSplitOptions.RemoveEmptyEntries);

            Regex rgx = new Regex(@"^[0-9\.\+\-]+$");

            Regex rgxlwralpha = new Regex(@"[a-z]");
            foreach (var item in sequences)
            {
                if (rgx.IsMatch(item))
                {
                    if (item.Contains("+"))
                    {
                        string sq = item;
                        sq = sq.Remove(sq.IndexOf("+"), 1);
                        if (rgx.IsMatch(sq))
                        {
                            length = length + Convert.ToDouble(sq);
                        }
                    }
                    else if (item.Contains("-"))
                    {
                        string sq = item;
                        sq = sq.Remove(sq.IndexOf("-"), 1);
                        if (rgx.IsMatch(sq))
                        {
                            length = length - Convert.ToDouble(sq);
                        }
                    }
                    else
                    {
                        length = length + Convert.ToDouble(item);
                    }
                }
                else
                {
                    if (App.AllValidationModifications.Any(a => "+" + a.Abbreviation == item))
                    {
                        length = length + Math.Abs(Convert.ToDouble(App.AllValidationModifications.First(a => "+" + a.Abbreviation == item).Mass));
                    }
                    else if (App.AllValidationModifications.Any(a => "-" + a.Abbreviation == item))
                    {
                        length = length - Math.Abs(Convert.ToDouble(App.AllValidationModifications.First(a => "-" + a.Abbreviation == item).Mass));
                    }
                    else if (App.AllValidationModifications.Any(a => a.Abbreviation == item))
                    {
                        length = length + Math.Abs(Convert.ToDouble(App.AllValidationModifications.First(a => a.Abbreviation == item).Mass));
                    }
                    else
                    {
                        if (rgxlwralpha.IsMatch(item))
                        {
                            for (int i = 0; i < item.Length; i++)
                            {
                                if ((i != item.Length - 1) && (rgxlwralpha.IsMatch(Convert.ToString(item[i + 1]))))
                                {
                                    length = length + sequencelengthwithmods(Convert.ToString(item[i]) + Convert.ToString(item[i + 1]));
                                    i = i + 1;
                                }
                                else
                                {
                                    length = length + sequencelength(Convert.ToString(item[i]));
                                }
                            }
                        }
                        else
                        {
                            length = length + sequencelength(item);
                        }
                    }
                }
            }
            return length;
        }


        /// <summary>
        /// Calcaulate all the b and y ions using a given sequence.
        /// And compare it to the Monomasses already present. 
        /// </summary>
        /// <param name="Sequence"></param>
        /// <param name="Monomasses"></param>
        /// <returns></returns>
        public static List<CZIons> CalculateCandZIon(string Sequence, List<double> Monomasses, double ParentMass, List<ModificationList> AllValidationModifications = null, double MatchTolerance = 0.00, SortedList<char, double> aa = null, bool isauto = false, Dictionary<double, double> CurrentMonomasses = null)
        {
            Aminoacids = aa == null ? AminoAcidHelpers.AminoAcidMass3 : aa;

            Monomasses = Monomasses.OrderBy(a => a).ToList();

            if (AllValidationModifications == null)
            {
                AllValidationModifications = App.AllValidationModifications;
            }
            if (MatchTolerance == 0.00)
            {
                MatchTolerance = Properties.Settings.Default.MatchTolerancePPM;
            }

            List<CZIons> AllIons = new List<CZIons>();

            double ion = new double();
            ion = Molecules.Ammonia;
            //if (!isauto)
            {
                if (ParentMass == 0.0)
                {
                    ParentMass = LocalParentMass;
                }

                Monomasses.Add(ParentMass);

                Monomasses = Monomasses.Where(a => a <= ParentMass).OrderBy(a => a).ToList();
            }


            string[] splitstring = { "[", "]" };

            List<string> sequences = Sequence.Replace("(", "").Replace(")", "").Trim().Split(splitstring, StringSplitOptions.RemoveEmptyEntries).ToList();

            int count = 1;

            Regex regex = new Regex(@"^[0-9\.\+\-]+$");

            Regex regexlwrcs = new Regex(@"[a-z]+$");

            //bool number = false;

            string tempseq = string.Empty;

            try
            {
                for (int i = 0; i < sequences.Count; i++)
                {
                    string seq = sequences[i];

                    if (i < sequences.Count)
                    {
                        if ((regex.IsMatch(seq)) || AllValidationModifications.Any(a => (a.Abbreviation == sequences[i]) || ("+" + a.Abbreviation == sequences[i]) || ("-" + a.Abbreviation == sequences[i])))
                        {
                            continue;
                        }
                    }

                    {
                        {
                            bool breaksequence = false;
                            for (int j = 0; j < seq.Length; j++) /// This is the individual AminoAcids
                            {
                                if (j == seq.Length - 1)
                                {
                                    for (int ii = i + 1; ii < sequences.Count; ii++)
                                    {
                                        if ((regex.IsMatch(sequences[ii])) || AllValidationModifications.Any(a => (a.Abbreviation == sequences[ii]) || ("+" + a.Abbreviation == sequences[ii]) || ("-" + a.Abbreviation == sequences[ii])))
                                        {
                                            breaksequence = true;
                                            break;
                                        }
                                        else
                                        {
                                            breaksequence = false;
                                            break;
                                        }
                                    }
                                }

                                if (breaksequence)
                                {
                                    for (int ii = i + 1; ii < sequences.Count; ii++)
                                    {
                                        {
                                            if (AllValidationModifications.Any(a => a.Abbreviation == sequences[ii]))
                                            {
                                                tempseq = "+" + sequences[ii] + " " + tempseq;
                                                ion = ion + Convert.ToDouble(AllValidationModifications.First(a => a.Abbreviation == sequences[ii]).Mass);
                                            }
                                            else if (AllValidationModifications.Any(a => "+" + a.Abbreviation == sequences[ii]))
                                            {
                                                tempseq = sequences[ii] + " " + tempseq;
                                                ion = ion + Convert.ToDouble(AllValidationModifications.First(a => "+" + a.Abbreviation == sequences[ii]).Mass);
                                            }
                                            else if (AllValidationModifications.Any(a => "-" + a.Abbreviation == sequences[ii]))
                                            {
                                                tempseq = sequences[ii] + " " + tempseq;
                                                ion = ion - Math.Abs(Convert.ToDouble(AllValidationModifications.First(a => "-" + a.Abbreviation == sequences[ii]).Mass));
                                            }
                                            else if (regex.IsMatch(sequences[ii]))
                                            {
                                                if (sequences[ii].StartsWith("+"))
                                                {
                                                    ion = ion + Convert.ToDouble(sequences[ii]);
                                                    tempseq += "+ " + sequences[ii].Remove(sequences[ii].IndexOf("+"), 1) + " ";
                                                }
                                                else if (sequences[ii].StartsWith("-"))
                                                {
                                                    ion = ion - Math.Abs(Convert.ToDouble(sequences[ii]));
                                                    tempseq += "- " + sequences[ii].Remove(sequences[ii].IndexOf("-"), 1) + " ";
                                                }
                                                else
                                                {
                                                    ion = ion + Convert.ToDouble(sequences[ii]);
                                                    tempseq += "+ " + sequences[ii] + " ";
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (j != seq.Length - 1)
                                {
                                    if (regexlwrcs.IsMatch(Convert.ToString(seq[j + 1])))
                                    {
                                        ion = ion + sequencelengthwithmods((Convert.ToString(seq[j]) + Convert.ToString(seq[j + 1])));
                                        tempseq = Convert.ToString(seq[j]) + Convert.ToString(seq[j + 1]) + " " + tempseq;
                                        j = j + 1;
                                    }
                                    else
                                    {
                                        ion = ion + sequencelength(Convert.ToString(seq[j]));
                                        tempseq = seq[j] + " " + tempseq;
                                    }
                                }
                                else
                                {
                                    ion = ion + sequencelength(Convert.ToString(seq[j]));
                                    tempseq = seq[j] + " " + tempseq;
                                }
                                //if (isauto)
                                //{

                                //    double bionerror = Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100;

                                //    AllIons.Add(new BYIons()
                                //    {
                                //        Bion = ion,
                                //        //Yion = 0,
                                //        //AminoAcid = tempseq.TrimEnd(),
                                //        //BionNumber = count,
                                //        //BorYIon = count,
                                //        bioncolor = bionerror == 100 ? false : (((PPM.CalculatePPM(ion, bionerror) <= Properties.Settings.Default.FragementIonTolerance) && (PPM.CalculatePPM(ion, bionerror) >= -(Properties.Settings.Default.FragementIonTolerance)))), /// Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)),
                                //        Intensity = CurrentMonomasses.Any(a => Math.Abs(a.Key - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? CurrentMonomasses.First(a => Math.Abs(a.Key - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Value : 0,
                                //        bnh3ioncolor = Monomasses.Any(a => Math.Abs(a - (ion - Molecules.Ammonia)) <= PPM.CurrentPPMbasedonMatchList((ion - Molecules.Ammonia), MatchTolerance)),
                                //        bh2oioncolor = Monomasses.Any(a => Math.Abs(a - (ion - Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion - Molecules.Water), MatchTolerance)),
                                //        bionerror = bionerror,/// Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100,
                                //        //bh2oion = Math.Round(ion - Molecules.Water, 5),
                                //        //bh2oioncolor = Monomasses.Any(a => Math.Abs(a - (ion - Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion - Molecules.Water), MatchTolerance)),
                                //        //bionerror = Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100,
                                //        //BionDescription = "B" + Convert.ToString(count) + " " + PPM.CalculatePPM(ion, Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100),
                                //        //BionDaltonDescription = "B" + Convert.ToString(count) + " " + (Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100)
                                //    });
                                //}
                                //else
                                //ion = ion + Molecules.Ammonia;
                                {
                                    double bionerror = Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100;

                                    AllIons.Add(new CZIons()
                                    {
                                        Cion = ion,
                                        Zion = 0,
                                        AminoAcid = tempseq.TrimEnd(),
                                        CionNumber = count,
                                        CorZIon = count,
                                        Intensity = CurrentMonomasses != null ? (CurrentMonomasses.Any(a => Math.Abs(a.Key - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? CurrentMonomasses.First(a => Math.Abs(a.Key - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Value : 0) : 0,
                                        cioncolor = bionerror == 100 ? false : ((Math.Abs(PPM.CalculatePPM(ion, bionerror)) < Properties.Settings.Default.FragementIonTolerance)), ///Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)),
                                        //bioncolor = bionerror == 100 ? false : (((PPM.CalculatePPM(ion, bionerror) < Properties.Settings.Default.FragementIonTolerance) && (PPM.CalculatePPM(ion, bionerror) > -(Properties.Settings.Default.FragementIonTolerance)))), ///Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)),
                                        ch2oion = Math.Round(ion - Molecules.Water, 5),
                                        ch2oioncolor = Monomasses.Any(a => Math.Abs(a - (ion - Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion - Molecules.Water), MatchTolerance)),
                                        cionerror = bionerror,/// Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100,
                                        CionDescription = "B" + Convert.ToString(count) + " " + PPM.CalculatePPM(ion, Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100),
                                        CionDaltonDescription = "B" + Convert.ToString(count) + " " + (Monomasses.Any(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - ion) <= PPM.CurrentPPMbasedonMatchList(ion, MatchTolerance)).Select(a => a - ion).First() : 100),
                                        cnh3ion = Math.Round(ion - Molecules.Ammonia, 5),
                                        cnh3ioncolor = Monomasses.Any(a => Math.Abs(a - (ion - Molecules.Ammonia)) <= PPM.CurrentPPMbasedonMatchList((ion - Molecules.Ammonia), MatchTolerance)),
                                    });
                                }
                                count = count + 1;

                                tempseq = string.Empty;

                            }
                        }
                    }
                }

                count = AllIons.Count - 1;
                //ion = -Molecules.Ammonia;
                ion = 0;
                int c = 1;

                List<CZIons> intensities = new List<CZIons>();

                intensities = AllIons.Where(a => a.cioncolor == true).ToList();

                tempseq = string.Empty;
                for (int i = sequences.Count - 1; i >= 0; i--)
                {
                    string seq = sequences[i];

                    string sq = seq;

                    if (regex.IsMatch(sq))
                    {
                        if (sq.Contains("+"))
                        {
                            sq = sq.Remove(sq.IndexOf("+"), 1);
                            ion = ion + Convert.ToDouble(sq);
                        }
                        else if (sq.Contains("-"))
                        {
                            sq = sq.Remove(sq.IndexOf("-"), 1);
                            ion = ion - Math.Abs(Convert.ToDouble(sq));
                        }
                        else
                        {
                            ion = ion + Convert.ToDouble(sq);
                        }
                    }
                    else
                    {
                        if (seq.Contains("-"))
                        {
                            if (AllValidationModifications.Any(a => ("-" + a.Abbreviation) == seq))
                            {
                                ion = ion - Math.Abs(Convert.ToDouble(AllValidationModifications.Where(a => "-" + a.Abbreviation == seq).First().Mass));
                            }
                        }
                        else if (seq.Contains("+"))
                        {
                            if (AllValidationModifications.Any(a => ("+" + a.Abbreviation) == seq))
                            {
                                ion = ion + Convert.ToDouble(AllValidationModifications.Where(a => "+" + a.Abbreviation == seq).First().Mass);
                            }
                        }
                        else if (AllValidationModifications.Any(a => a.Abbreviation == seq))
                        {
                            ion = ion + Convert.ToDouble(AllValidationModifications.Where(a => a.Abbreviation == seq).First().Mass);
                        }
                        else
                        {
                            string[] alltheaminoacids = seq.Select(a => a.ToString()).ToArray();
                            double modmass = 0.0;
                            string tempaminoacids = string.Empty;
                            bool hasaminoacidsmod = false;
                            foreach (string AminoAcid in alltheaminoacids.Reverse())
                            {
                                if (regexlwrcs.IsMatch(AminoAcid))
                                {
                                    tempaminoacids = AminoAcid;
                                    hasaminoacidsmod = true;
                                    continue;
                                }
                                tempaminoacids = AminoAcid + tempaminoacids;

                                ion = ion + (hasaminoacidsmod ? sequencelengthwithmods(tempaminoacids) : sequencelength(AminoAcid));

                                double Zion = Math.Round(ion, 4); /// + Molecules.Water, 4) - Molecules.Ammonia;

                                //AllIons[count].yioncolor = Monomasses.Any(a => Math.Abs(a - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance));
                                AllIons[count].Zion = Zion;
                                //AllIons[count].in
                                double yionerror = Monomasses.Any(a => Math.Abs(a - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)).Select(a => a - (ion + Molecules.Water)).First() : 100;

                                //if (isauto)
                                //{
                                //    //if (AllIons[count].yioncolor)
                                //    {
                                //        AllIons[count].Intensity += CurrentMonomasses.Any(a => Math.Abs(a.Key - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)) ?
                                //                                    CurrentMonomasses.First(a => Math.Abs(a.Key - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)).Value : 0;
                                //        AllIons[count].yh2oioncolor = Monomasses.Any(a => Math.Abs(a - (ion)) <= PPM.CurrentPPMbasedonMatchList((ion), MatchTolerance));
                                //        AllIons[count].ynh3ioncolor = Monomasses.Any(a => Math.Abs(a - (ion + Molecules.Water - Molecules.Ammonia)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water - Molecules.Ammonia), MatchTolerance));
                                //        AllIons[count].yionerror = yionerror;/// Monomasses.Any(a => Math.Abs(a - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)).Select(a => a - (ion + Molecules.Water)).First() : 100;
                                //        AllIons[count].yioncolor = yionerror == 100 ? false : (((PPM.CalculatePPM(Yion, yionerror) <= Properties.Settings.Default.FragementIonTolerance) && (PPM.CalculatePPM(Yion, yionerror) >= -(Properties.Settings.Default.FragementIonTolerance)))); //Checking if the PPM ion tolerance is between the limits
                                //        //CurrentMonomasses.ContainsKey(AllIons[count].Yion) ? CurrentMonomasses[AllIons[count].Yion] : 0;
                                //        //AllIons[count].Intensity += CurrentMonomasses.ContainsKey(AllIons[count].Yion) ? CurrentMonomasses[AllIons[count].Yion] : 0;
                                //    }
                                //}
                                //if (!isauto)
                                {
                                    AllIons[count].Zion = Zion;/// Math.Round(ion + Molecules.Water, 4);
                                    AllIons[count].ZionNumber = -c;

                                    AllIons[count].Intensity += CurrentMonomasses != null ? (CurrentMonomasses.Any(a => Math.Abs(a.Key - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)) ?
                                                                CurrentMonomasses.First(a => Math.Abs(a.Key - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)).Value : 0) : 0;
                                    AllIons[count].NewZIonNumber = c;
                                    AllIons[count].zh2oion = Math.Round(ion, 4);
                                    AllIons[count].znh3ion = Math.Round(ion + Molecules.Water - Molecules.Ammonia, 4);
                                    AllIons[count].zh2oioncolor = Monomasses.Any(a => Math.Abs(a - (ion)) <= PPM.CurrentPPMbasedonMatchList((ion), MatchTolerance));
                                    AllIons[count].znh3ioncolor = Monomasses.Any(a => Math.Abs(a - (ion + Molecules.Water - Molecules.Ammonia)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water - Molecules.Ammonia), MatchTolerance));
                                    AllIons[count].zionerror = yionerror;/// Monomasses.Any(a => Math.Abs(a - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)) ? Monomasses.Where(a => Math.Abs(a - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)).Select(a => a - (ion + Molecules.Water)).First() : 100;

                                    AllIons[count].zioncolor = yionerror == 100 ? false : ((Math.Abs(PPM.CalculatePPM(Zion, yionerror)) <= Properties.Settings.Default.FragementIonTolerance));

                                    //AllIons[count].yioncolor = yionerror == 100 ? false : (((PPM.CalculatePPM(Yion, yionerror) <= Properties.Settings.Default.FragementIonTolerance) && (PPM.CalculatePPM(Yion, yionerror) >= -(Properties.Settings.Default.FragementIonTolerance))));

                                    if (Monomasses.Any(a => Math.Abs(a - (ion + Molecules.Water)) <= PPM.CurrentPPMbasedonMatchList((ion + Molecules.Water), MatchTolerance)))
                                    {
                                        AllIons[count].ZionDescription = "Y" + Convert.ToString(c) + " " + Convert.ToString(AllIons[count].zionPPM);
                                        AllIons[count].ZionDaltonDescription = "Y" + Convert.ToString(c) + " " + Convert.ToString(AllIons[count].zionerror);
                                    }
                                }
                                c = c + 1;
                                count = count - 1;
                                tempaminoacids = string.Empty;
                                hasaminoacidsmod = false;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return AllIons;
        }

        public class CZIons
        {

            public double Cion
            {
                get;
                set;
            }

            public double Intensity
            {
                get;
                set;
            }

            public double Zion
            {
                get;
                set;
            }

            public string AminoAcid
            {
                get;
                set;
            }


            public double cionPPM
            {
                get
                {
                    return PPM.CalculatePPM(Cion, cionerror);
                }
            }


            public double zionPPM
            {
                get
                {
                    return PPM.CalculatePPM(Zion, zionerror);
                }
            }


            public int CionNumber
            {
                get;
                set;
            }

            public int ZionNumber
            {
                get;
                set;
            }

            public bool cioncolor
            {
                get;
                set;
            }

            public bool zioncolor
            {
                get;
                set;
            }


            public bool ch2oioncolor
            {
                get;
                set;
            }

            public double ch2oion
            {
                get;
                set;
            }

            public bool zh2oioncolor
            {
                get;
                set;
            }

            public double zh2oion
            {
                get;
                set;
            }

            public int NewZIonNumber
            {
                get;
                set;
            }

            public bool znh3ioncolor
            {
                get;
                set;
            }

            public double znh3ion
            {
                get;
                set;
            }

            public bool cnh3ioncolor
            {
                get;
                set;
            }

            public double cnh3ion
            {
                get;
                set;
            }

            public double cionerror
            {
                get;
                set;
            }

            public double zionerror
            {
                get;
                set;
            }

            public double CorZIon
            {
                get;
                set;
            }

            public string ZionDescription
            {
                get;
                set;
            }

            public string CionDescription
            {
                get;
                set;
            }

            public string CionDaltonDescription
            {
                get;
                set;
            }

            public string ZionDaltonDescription
            {
                get;
                set;
            }

            public string cziondescription
            {
                get
                {
                    return CionDescription + "Yion = " + Zion + "   Bion = " + Cion + "      Amino Acid :" + AminoAcid + "   Bion Error = " + cionerror + "   Yion Error = " + zionerror;
                }
            }

            public override string ToString()
            {
                return cziondescription;
            }
        }

    }
}

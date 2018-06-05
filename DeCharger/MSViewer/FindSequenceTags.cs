using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalProcessing;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace MSViewer
{
    public static class FindSequenceTags
    {
        /// <summary>
        /// Finds all the sequences possible for all monomasses
        /// available.
        /// The longest possible sequence is called as a Sequence Tag
        /// Hence finding for the longest sequence rather than individual 
        /// smaller sequences.
        /// But if there is any sequence with just one Amino Acid with none other
        /// present closer to it then consider it as a Sequence Tag
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        public static List<Sequences> FindnewSequencetags(List<InitialAA> values, double maxmassaa, double? parentmass = 0)
        {
            return null;
        }

        /// <summary>
        /// Finds the sequence tags based on the given Amino Acids using
        /// the start and end points
        /// </summary>
        /// <param name="values"></param>
        /// <param name="maxmassaa"></param>
        /// <returns></returns>
        public static List<SequenceTag> FindSequenceTag(List<InitialAA> values)
        {
            try
            {
                List<SequenceTag> sequences = new List<SequenceTag>();
                List<SequenceTag> correctedsequences = new List<SequenceTag>();

                values = values.OrderBy(a => a.Start).ToList();
                var sameval = values.GroupBy(a => a.NameIndex).Select(b => b.First()).ToList();
                findprlltags(sameval, sequences);

                foreach (SequenceTag s in sequences)
                {
                    s.Start = Math.Round(s.IndividualAAs[0].Start, 2);
                    s.Score = (float)Math.Round((s.totalScore / s.IndividualAAs.Count), 4);
                    correctedsequences.Add(s);
                }
                var xcx = correctedsequences.GroupBy(a => a.Sequence).Select(b => b.First()).ToList();

                return xcx.OrderByDescending(a => a.IndividualAAs.Count).ThenBy(b => b.Score).Where(a => a.Sequence.Length >= 3).ToList();
            }
            catch (Exception ex)
            {
                App.Laststacktrace = ex.StackTrace;
                Action throwException = () => { throw ex; };
                Dispatcher.CurrentDispatcher.BeginInvoke(throwException);
                return new List<SequenceTag>();
            }
        }



        /// <summary>
        /// Finds the sequence tags based on the given Amino Acids using
        /// the start and end points. Including the Tags which are one amino acid long.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="maxmassaa"></param>
        /// <returns></returns>
        public static List<SequenceTag> FindAllSequenceTags(List<InitialAA> values)
        {
            try
            {
                List<SequenceTag> sequences = new List<SequenceTag>();
                List<SequenceTag> correctedsequences = new List<SequenceTag>();

                values = values.OrderBy(a => a.Start).ToList();
                var sameval = values.GroupBy(a => a.NameIndex).Select(b => b.First()).ToList();
                findprlltags(sameval, sequences);

                foreach (SequenceTag s in sequences)
                {
                    s.Start = Math.Round(s.IndividualAAs[0].Start, 2);
                    s.Score = (float)Math.Round((s.totalScore / s.IndividualAAs.Count), 4);
                    correctedsequences.Add(s);
                }
                var xcx = new List<FindSequenceTags.SequenceTag>();// correctedsequences.GroupBy(a => a.Sequence).Select(b => b.First()).ToList();

                foreach (var item in correctedsequences.GroupBy(a => a.Sequence).Select(b => b.First()).ToList())
                {
                    item.Index = item.Index + "-" + item.RawSequence + "-";
                    xcx.Add(item);
                }

                foreach (var smvl in sameval)
                {
                    List<AminoAcids> amcs = new List<AminoAcids>();
                    amcs.Add(new AminoAcids()
                    {
                        Score = smvl.Delta + 50 / ((smvl.StartDelta + smvl.EndDelta) / 2),
                        EndScore = smvl.EndDelta,
                        StartScore = smvl.StartDelta,
                        Name = smvl.Name,
                        Start = smvl.Start,
                        End = smvl.End
                    });

                    xcx.Add(new SequenceTag()
                    {
                        Index = "-" + smvl.Index + "-" + smvl.Name + "-",
                        IndividualAAs = amcs,
                        End = Math.Round(smvl.End, 4),
                        Start = Math.Round(smvl.Start, 4),
                        NumberofAA = 1,
                        MaxScore = smvl.Delta,
                        RawSequence = smvl.Name,
                        totalScore = smvl.Delta,
                        Score = (smvl.StartDelta == 0 && smvl.EndDelta == 0) ? 0 : smvl.Delta + 50 / ((smvl.StartDelta + smvl.EndDelta) / 2)
                    });
                }

                var samesequences = xcx.GroupBy(a => a.Index).Select(a => a.First()).ToList();

                return samesequences.OrderByDescending(a => a.IndividualAAs.Count).ThenBy(b => b.Score).ToList();  ///.Where(a => a.Sequence.Length >= Properties.Settings.Default.SequenceTagLength)
            }
            catch (Exception ex)
            {
                App.Laststacktrace = ex.StackTrace;
                Action throwException = () => { throw ex; };
                Dispatcher.CurrentDispatcher.BeginInvoke(throwException);
                return new List<SequenceTag>();
            }
        }

        //public static List<InitialAA> RemoveModifications(List<InitialAA> values, bool direction, List<double> modifications)
        //{
        //    List<InitialAA> newvalues = new List<InitialAA>();

        //    List<InitialAA> vals = new List<InitialAA>();
        //    vals.AddRange(values.OrderBy(a => a.Start).ToList());

        //    foreach (var va in vals)
        //    {
        //        //modifications.Where(a => a == vals.Select(b => b.Start - va.))
        //    }

        //    return newvalues;
        //}

        public static void findprlltags(List<InitialAA> values, List<SequenceTag> sequences)
        {
            try
            {
                int h = 0;
                List<double> start = new List<double>();
                List<SequenceTag> startse = new List<SequenceTag>();
                List<double> end = new List<double>();
                double singleend = new double();

                foreach (InitialAA val in values)
                {
                    var m = from SequenceTag sq in sequences
                            where sq.Index.Contains("-" + val.Index + "-")
                            select sq;
                    if (m.Any()) { h++; continue; }

                    List<SequenceTag> seq = new List<SequenceTag>();
                    List<AminoAcids> aas = new List<AminoAcids>();

                    aas.Add(new AminoAcids
                    {
                        End = val.End,
                        Name = val.Name,
                        Start = val.Start,
                        Score = val.Delta + 50 / ((val.StartDelta + val.EndDelta) / 2),
                        StartScore = val.StartDelta,
                        EndScore = val.EndDelta,
                        AverageMonoMassIntensities = val.AverageMonoMassIntensity,
                        SumofMonoMassIntensities = val.SumofMonoMassIntensities
                    });

                    seq.Add(new SequenceTag
                    {
                        End = val.End,
                        Start = val.Start,
                        totalScore = val.Delta,
                        RawSequence = val.Name,
                        MaxScore = val.Delta,
                        NumberofAA = 1,
                        Index = "-" + val.Index + "-",
                        IndividualAAs = aas
                    });
                    end.Add(val.End);
                    singleend = val.End;
                    foreach (InitialAA v in values.GetRange(h, values.Count - h))
                    {
                        if (v.Index == val.Index) continue;
                        if (end.Where(a => a == v.Start).Any())
                        {
                            if (!(start.Where(a => a == v.Start).Any()))
                            {
                                var xx = (seq.Where(a => a.End == v.Start).ToList());
                                foreach (var z in xx)
                                {
                                    int x = seq.IndexOf(z);
                                    end.Add(v.End);
                                    start.Add(v.Start);
                                    AminoAcids aa = new AminoAcids();
                                    aa.End = v.End;
                                    aa.Start = v.Start;
                                    aa.EndScore = v.EndDelta;
                                    aa.Name = v.Name;
                                    aa.StartScore = v.StartDelta;
                                    aa.AverageMonoMassIntensities = v.AverageMonoMassIntensity;
                                    aa.SumofMonoMassIntensities = v.SumofMonoMassIntensities;
                                    if (v.StartDelta == 0 && v.EndDelta == 0)
                                        aa.Score = v.Delta;
                                    else
                                        aa.Score = v.Delta + 50 / ((v.StartDelta + v.EndDelta) / 2);
                                    List<AminoAcids> aaa = new List<AminoAcids>();
                                    aaa.AddRange(seq[x].IndividualAAs);
                                    startse.Add(new SequenceTag
                                    {
                                        RawSequence = seq[x].Sequence,
                                        Start = v.Start,
                                        Index = seq[x].Index,
                                        IndividualAAs = aaa,
                                        totalScore = seq[x].Score + (float)aa.Score,
                                        Score = seq[x].Score + (float)aa.Score,
                                        End = v.End
                                    });
                                    seq[x].Start = v.Start;
                                    seq[x].End = v.End;
                                    seq[x].RawSequence += v.Name;
                                    seq[x].totalScore = (float)aa.Score + startse[startse.Count - 1].totalScore;
                                    seq[x].Index += "-" + v.Index + "-";
                                    seq[x].IndividualAAs.Add(aa);
                                }
                            }
                            else
                            {
                                var ses = startse.Where(b => b.Start == v.Start).ToList();
                                foreach (var se in ses)
                                {
                                    if (se.realEnd != v.Start) continue;
                                    SequenceTag sequence = new SequenceTag();
                                    AminoAcids aa = new AminoAcids();
                                    aa.End = v.End;
                                    aa.Start = v.Start;
                                    aa.EndScore = v.EndDelta;
                                    aa.Name = v.Name;
                                    if (v.StartDelta == 0 && v.EndDelta == 0)
                                        aa.Score = v.Delta;
                                    else
                                        aa.Score = v.Delta + 50 / ((v.StartDelta + v.EndDelta) / 2);
                                    aa.StartScore = v.StartDelta;
                                    aa.AverageMonoMassIntensities = v.AverageMonoMassIntensity;
                                    aa.SumofMonoMassIntensities = v.SumofMonoMassIntensities;
                                    seq.Add(new SequenceTag
                                    {
                                        Index = se.Index + "-" + v.Index + "-",
                                        Start = se.Start,
                                        RawSequence = se.Sequence + v.Name,
                                        End = v.End,
                                        totalScore = se.totalScore + (float)aa.Score,
                                        Score = se.totalScore + (float)aa.Score
                                    });

                                    int count = seq.Count - 1;

                                    end.Add(v.End);
                                    start.Add(v.Start);
                                    List<AminoAcids> aaa = new List<AminoAcids>();
                                    aaa.AddRange(se.IndividualAAs);
                                    startse.Add(new SequenceTag
                                    {
                                        RawSequence = se.Sequence + v.Name,
                                        Start = se.Start,
                                        Index = v.Index,
                                        IndividualAAs = aaa,
                                        End = v.End
                                    });
                                    int cnt = startse.Count - 1;
                                    startse[cnt].IndividualAAs.Add(aa);
                                    seq[count].IndividualAAs = new List<AminoAcids>();
                                    seq[count].IndividualAAs = startse[cnt].IndividualAAs;
                                    seq[count].totalScore = v.Delta + seq[count].totalScore;
                                }
                            }
                        }
                    }
                    sequences.AddRange(seq);
                    end.Clear();
                    start.Clear();
                    startse.Clear();
                    h++;
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Exception in findprlltags(): " + ex.Message);
            }
        }



        ///// <summary>
        ///// In order for the algorithm to work, creating a recursive function which does
        ///// searching for the rest of the values and sees if there is any sequence present
        ///// </summary>
        ///// <param name="values">This comes is the rest of the values which need to be searched for.</param>
        ///// <param name="sequence">This is the sequence which is already created from the previous method. </param>
        ///// <returns></returns>
        //private static SequenceTag findinnertags(List<InitialAA> values, SequenceTag sequence, double Startpoint, double end, List<double> startpoints = null, List<double> endpoints = null)
        //{
        //    try
        //    {
        //        int i = 0;
        //        foreach (InitialAA value in values)
        //        {
        //            if (i != 0)
        //            {
        //                var valueswithsamestartpoint = (from InitialAA vv in values
        //                                                //from v in vv.StartValues
        //                                                where end == vv.Start
        //                                                select vv).ToList();
        //                if (end == value.Start && valueswithsamestartpoint.Count == 1)
        //                {
        //                    end = value.End;
        //                    sequence.End = value.End;
        //                    sequence.totalScore = sequence.totalScore + value.Delta + 50 / ((value.StartDelta + value.EndDelta) / 2);
        //                    sequence.Sequence = sequence.Sequence + value.Name;
        //                    sequence.NumberofAA = sequence.NumberofAA + 1;
        //                    sequence.Index = sequence.Index + "-" + value.Index + "-";
        //                    List<string> aaName = new List<string>();
        //                    List<double> aaStart = new List<double>();
        //                    List<double> aaEnd = new List<double>();
        //                    List<double> aaScore = new List<double>();
        //                    List<double> aaStartScore = new List<double>();
        //                    List<double> aaEndScore = new List<double>();
        //                    foreach (AminoAcids a in sequence.IndividualAAs)
        //                    {
        //                        aaName.Add(a.Name);
        //                        aaStart.Add(a.Start);
        //                        aaEnd.Add(a.End);
        //                        aaScore.Add(a.Score);
        //                        aaStartScore.Add(a.StartScore);
        //                        aaEndScore.Add(a.EndScore);
        //                    }
        //                    int k = 0;
        //                    sequence.IndividualAAs = new List<AminoAcids>();
        //                    foreach (string a in aaName)
        //                    {
        //                        sequence.IndividualAAs.Add(new AminoAcids
        //                        {
        //                            End = aaEnd[k],
        //                            Start = aaStart[k],
        //                            Name = aaName[k],
        //                            Score = aaScore[k],
        //                            StartScore = aaStartScore[k],
        //                            EndScore = aaEndScore[k]
        //                        });
        //                        k++;
        //                    }
        //                    sequence.IndividualAAs.Add(new AminoAcids
        //                    {
        //                        Name = value.Name,
        //                        End = value.End,
        //                        Start = value.Start,
        //                        Score = value.Delta + 50 / ((value.StartDelta + value.EndDelta) / 2),
        //                        StartScore = value.StartDelta,
        //                        EndScore = value.EndDelta
        //                    });
        //                    if (sequence.MaxScore < value.Delta)
        //                        sequence.MaxScore = value.Delta + 50 / ((value.StartDelta + value.EndDelta) / 2);
        //                }
        //                else if (end == value.Start && valueswithsamestartpoint.Count() > 1)
        //                {
        //                    //bool removeornot = true;
        //                    foreach (InitialAA va in valueswithsamestartpoint)
        //                    {
        //                        SequenceTag sq = new SequenceTag();
        //                        sq.Sequence = sequence.Sequence;
        //                        sq.End = sequence.End;
        //                        sq.Index = sequence.Index;
        //                        sq.Ions = sequence.Ions;
        //                        sq.MaxScore = sequence.MaxScore;
        //                        sq.totalScore = sequence.totalScore;
        //                        sq.Start = sequence.Start;
        //                        sq.NumberofAA = sequence.NumberofAA + 1;
        //                        List<string> aaName = new List<string>();
        //                        List<double> aaStart = new List<double>();
        //                        List<double> aaEnd = new List<double>();
        //                        List<double> aaScore = new List<double>();
        //                        List<double> aaStartScore = new List<double>();
        //                        List<double> aaEndScore = new List<double>();
        //                        foreach (AminoAcids a in sequence.IndividualAAs)
        //                        {
        //                            aaName.Add(a.Name);
        //                            aaEnd.Add(a.End);
        //                            aaStart.Add(a.Start);
        //                            aaScore.Add(a.Score);
        //                            aaStartScore.Add(a.StartScore);
        //                            aaEndScore.Add(a.EndScore);
        //                        }
        //                        int k = 0;
        //                        sq.IndividualAAs = new List<AminoAcids>();
        //                        foreach (string a in aaName)
        //                        {
        //                            sq.IndividualAAs.Add(new AminoAcids
        //                            {
        //                                Start = aaStart[k],
        //                                End = aaEnd[k],
        //                                Name = aaName[k],
        //                                Score = aaScore[k],
        //                                EndScore = aaEndScore[k],
        //                                StartScore = aaStartScore[k]
        //                            });
        //                            k++;
        //                        }
        //                        sq.Sequence = sq.Sequence + va.Name;
        //                        sq.Index = sq.Index + "-" + va.Index + "-";
        //                        sq.IndividualAAs.Add(new AminoAcids
        //                        {
        //                            Start = va.Start,
        //                            End = va.End,
        //                            Name = va.Name,
        //                            Score = va.Delta + 50 / ((va.StartDelta + va.EndDelta) / 2),
        //                            StartScore = va.StartDelta,
        //                            EndScore = va.EndDelta
        //                        });

        //                        sq = findinnertags(values.GetRange(i + 1, values.Count() - i - 1), sq, va.Start, va.End);
        //                        sequences.Add(new SequenceTag
        //                        {
        //                            Index = sq.Index,
        //                            Sequence = sq.Sequence,
        //                            totalScore = sq.totalScore,
        //                            Ions = sq.Ions,
        //                            MaxScore = sq.MaxScore,
        //                            End = sq.End,
        //                            Start = sq.Start,
        //                            NumberofAA = sq.NumberofAA,
        //                            IndividualAAs = sq.IndividualAAs
        //                        });
        //                    }
        //                    end = sequences.Last().End;
        //                    break;
        //                }
        //            }
        //            else if (end == value.Start)
        //            {
        //                end = value.End;
        //                sequence.End = value.End;
        //                sequence.NumberofAA = sequence.NumberofAA + 1;
        //                sequence.totalScore = sequence.totalScore + value.Delta + 50 / ((value.StartDelta + value.EndDelta) / 2);
        //                sequence.Sequence = sequence.Sequence + value.Name;
        //                sequence.Index = sequence.Index + "-" + value.Index + "-";
        //                List<string> aaName = new List<string>();
        //                List<double> aaStart = new List<double>();
        //                List<double> aaEnd = new List<double>();
        //                List<double> aaScore = new List<double>();
        //                List<double> aaStartScore = new List<double>();
        //                List<double> aaEndScore = new List<double>();
        //                foreach (AminoAcids a in sequence.IndividualAAs)
        //                {
        //                    aaName.Add(a.Name);
        //                    aaEnd.Add(a.End);
        //                    aaStart.Add(a.Start);
        //                    aaScore.Add(a.Score);
        //                    aaStartScore.Add(a.StartScore);
        //                    aaEndScore.Add(a.EndScore);
        //                }
        //                int k = 0;
        //                sequence.IndividualAAs = new List<AminoAcids>();
        //                foreach (string a in aaName)
        //                {
        //                    sequence.IndividualAAs.Add(new AminoAcids
        //                    {
        //                        Start = aaStart[k],
        //                        End = aaEnd[k],
        //                        Name = aaName[k],
        //                        Score = aaScore[k],
        //                        StartScore = aaStartScore[k],
        //                        EndScore = aaEndScore[k]
        //                    });
        //                    k++;
        //                }
        //                sequence.IndividualAAs.Add(new AminoAcids
        //                {
        //                    Name = value.Name,
        //                    End = value.End,
        //                    Start = value.Start,
        //                    Score = value.Delta + 50 / ((value.StartDelta + value.EndDelta) / 2),
        //                    StartScore = value.StartDelta,
        //                    EndScore = value.EndDelta
        //                });
        //                if (sequence.MaxScore < value.Delta)
        //                    sequence.MaxScore = value.Delta;
        //            }

        //            i++;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Action throwException = () => { throw ex; };
        //        Dispatcher.CurrentDispatcher.BeginInvoke(throwException);
        //        return new SequenceTag();
        //    }
        //    return sequence;
        //}
        /// <summary>
        /// The SequenceTag class shows different sequences available for 
        /// AminoAcids obtained using the MonoMasses
        /// </summary>
        public class SequenceTag : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            string _sequence = null;

            /// The end sequence
            public string Sequence
            {
                get
                {
                    // remove all lower case letters
                    _sequence = _sequence ?? new string(RawSequence.Where(c => char.IsUpper(c)).ToArray());
                    return _sequence;

                    // This Regex was getting called A LOT from a Linq statement, so I am trying something faster. 
                    //var rgx = new Regex("[^A-Z]");
                    //return rgx.Replace(RawSequence, "");
                }
            }

            string _revSeq = null;

            public string ReverseSequence
            {
                get
                {
                    _revSeq = _revSeq ?? new string(this.Sequence.Reverse().ToArray());

                    return _revSeq;
                }

            }

            string _rawSequence = null;

            public string RawSequence
            {
                get
                {
                    if (_rawSequence == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        _rawSequence = _rawSequence ?? new string(this.Sequence.Reverse().ToArray());
                    }
                    return _rawSequence;
                }
                set
                {
                    if (_rawSequence != value)
                    {
                        _rawSequence = value;

                        // clear the cached values if this changes
                        _sequence = null;
                        _revSeq = null;
                    }
                }
            }

            /// <summary>
            /// The Average delta is the Score
            /// </summary>
            public float Score
            {
                get;
                //{
                //    return totalScore / IndividualAAs.Count();
                //}
                set;
                //{
                //    Score = value;
                //}
            }

            /// <summary>
            /// The totalScore of all the Amino Acids
            /// </summary>
            public float totalScore
            {
                get;
                set;
            }

            /// <summary>
            /// All the Ions present in the current sequence
            /// </summary>
            public List<Cluster> Ions
            {
                get;
                set;
            }
            public double realEnd
            {
                get
                {
                    return IndividualAAs.Last().End;
                }
            }

            /// <summary>
            /// The Start point for the sequence
            /// </summary>
            public double Start
            {
                get;
                set;
            }

            public int StartStart
            {
                get;
                set;
            }

            /// <summary>
            /// The End point for the sequence
            /// </summary>
            public double End
            {
                get;
                set;
            }

            public int EndEnd
            {
                get;
                set;
            }

            /// <summary>
            /// The MaxScore determines the maximum possible delta
            /// </summary>
            public float MaxScore
            {
                get;
                set;
            }


            /// <summary>
            /// The Index is the key for a particular Amino Acid in the sequence.
            /// </summary>
            public string Index
            {
                get;
                set;
            }

            /// <summary>
            /// Total number of AminoAcids present
            /// </summary>
            public int NumberofAA
            {
                get;
                set;
            }
            private bool check;

            public string DatabaseHits
            {
                get
                {
                    if (BlastHits.HasValue && !DatabaseHitsValue.HasValue)
                        return BlastHits == 1 ? ">= 1" : Convert.ToString(BlastHits);
                    else if (DatabaseHitsValue.HasValue)
                        return DatabaseHitsValue >= 100 ? "> 100" : Convert.ToString(DatabaseHitsValue);
                    else
                        return null;
                }
            }

            public int? BlastHits
            {
                get;
                set;
            }


            public int? DatabaseHitsValue
            {
                get;
                set;
            }

            /// <summary>
            /// The individual Amino Acids present within a sequence.
            /// </summary>
            public List<AminoAcids> IndividualAAs
            {
                get;
                set;
            }

            string _visibility = "No";

            public string Visibility
            {
                get { return _visibility; }
                set { _visibility = value; }
            }

            public string BlastTag
            {
                get;
                set;
            }

            public bool VerifyBlastTag
            {
                get
                {
                    if (BlastTag != null && Sequence != null)
                    {
                        if (Sequence.Length >= 30)
                        {
                            if ((Sequence.Length - BlastTag.Length) < 10)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (Sequence.Length < 30 & Sequence.Length >= 20)
                        {
                            if ((Sequence.Length - BlastTag.Length) < 7)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (Sequence.Length < 20 & Sequence.Length >= 10)
                        {
                            if ((Sequence.Length - BlastTag.Length) < 4)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (Sequence.Length < 10 & Sequence.Length >= 6)
                        {
                            if ((Sequence.Length - BlastTag.Length) < 1)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (Sequence.Length < 6)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }


            /// <summary>
            /// This is used to set the check boxes for viewing the sequences
            /// </summary>
            public bool Checked
            {
                get
                {
                    return check;
                }
                set
                {
                    if (check == value)
                    {
                        return;
                    }
                    check = value;
                    if (this.PropertyChanged != null)
                    {
                        OnPropertyChanged("Checked");
                    }
                }
            }

            public bool CheckedYellow
            {
                get
                {
                    return check;
                }
                set
                {
                    if (check == value)
                    {
                        return;
                    }
                    check = value;
                    if (this.PropertyChanged != null)
                    {
                        OnPropertyChanged("Checked");
                    }
                }
            }
            //public enum BackGroundColor
            //{
            //    Green = 0,
            //    Yellow = 1,
            //    None = 2
            //}


            //public BackGroundColor bkgcolor
            //{
            //    get;
            //    set;
            //}

            public string bkgcolor
            {
                get;
                set;
            }
            /// <summary>
            /// Sum of all the monomassintensities
            /// </summary>
            public double SumofMonoMassIntensity
            {
                get
                {
                    if (IndividualAAs.Count > 0)
                    {
                        return Math.Round(IndividualAAs.Sum(a => a.SumofMonoMassIntensities), 4);
                    }
                    return 0;
                }
            }

            public double AverageMonoMassIntensity
            {
                get
                {
                    if (IndividualAAs.Count > 0)
                    {
                        return Math.Round(IndividualAAs.Average(a => a.AverageMonoMassIntensities), 4);
                    }
                    return 0;
                }
            }

            private void OnPropertyChanged(string info)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(info));
                }
            }
            public override string ToString() { return "Seq: " + Sequence + "; Start: " + Start.ToString("0.00"); }
        }
        public class AminoAcids
        {
            public string Name
            {
                get;
                set;
            }
            public double Start
            {
                get;
                set;
            }
            public double End
            {
                get;
                set;
            }
            public double StartScore
            {
                get;
                set;
            }
            public double EndScore
            {
                get;
                set;
            }
            public double Score
            {
                get;
                set;
            }

            public double SumofMonoMassIntensities
            {
                get;
                set;
            }

            public double AverageMonoMassIntensities
            {
                get;
                set;
            }
        }

        public class SimilarAminoAcids
        {
            public string Index
            {
                get;
                set;
            }
            public List<string> Pairs
            {
                get;
                set;
            }
        }

        public class SimilarStartandEnd
        {
            public float Start
            {
                get;
                set;
            }
            public float End
            {
                get;
                set;
            }
            public int Index
            {
                get;
                set;
            }
        }


    }
}

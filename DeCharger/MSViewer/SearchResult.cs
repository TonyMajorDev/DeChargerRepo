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
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;
using MSViewer.Classes;
using Science.Proteomics;
using MoreLinq;
using SignalProcessing;
using MassSpectrometry;
using System.ComponentModel;
using System.Collections.ObjectModel;
//using Newtonsoft.Json;

namespace MSViewer
{
    [XmlType("SearchResult")]
    [Serializable]
    public class SearchResult //: INotifyPropertyChanged
    {
        //public SearchResult()
        //{
        //    Curations.CollectionChanged += Curations_CollectionChanged;

        //}

        //private void Curations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    OnPropertyChanged("Date_Time_Curation");
        //    OnPropertyChanged("LastCuration");
        //}

        //public event PropertyChangedEventHandler PropertyChanged;

        //public void OnPropertyChanged(string pName)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(pName));
        //    }
        //}

        /// <summary>
        /// Set some of the properties based on the spectrum info
        /// </summary>
        /// <param name="spec"></param>
        public void ApplySpectrumDetails(SuperSpectrum spec)
        {
            this.ScanNumbers = string.Join(",", spec.ScanNumbers);
            this.ParentZ = spec.ParentIon.Z;
            this.Parentz_s = string.Join(",", spec.ParentIons.Select(pi => pi.Z)); // Convert.ToString(spec.ParentIon.Z);
            this.ScanTitle = spec.Title;
            this.RetentionTime = spec.RetentionTime.ToString(); // Convert.ToString(MainPointProvider.RetentionTime(aScan.ScanNumber))
        }

        public override string ToString()
        {
            return ScanNumbers + "\t  ParentMass: " + ParentMass.ToString() + "\t ParentZ: " + ParentZ.ToString() + "\t BlastTagforTopProtein: " + BlastTagforTopProtein + "\t Description: " + Description + "\t Sequence: " + Sequence + "\t Accession: " + Accession;
            //return "TagHits = " + this.TagHits + ", Seq = " + Sequence.Substring(0, Math.Min(Sequence.Length, 10)) + "..., Acc = " + Accession;
        }
        double _evalue = 0;

        //[XmlElement("BlastpAlignmentLength")]
        //[XmlIgnore]
        //public int BlastpAlignmentLength
        //{
        //    get
        //    {
        //        if (IsBlastResult)
        //            return QueryTag 
        //        else
        //            return QueryTag.Length;
        //    }
        //}

        string _username = string.Empty;

        public bool ConfirmedSequencetab { get; set; }

        public string RawSequence
        {
            get;
            set;
        }

        public bool DuplicateConfirmedSequence
        {
            get;
            set;
        }

        [XmlElement("ScanTitle")]
        public string ScanTitle
        {
            get;
            set;
        }


        /// <summary>
        /// Retention Time in minutes
        /// </summary>
        [XmlElement("RetentionTime")]
        public string RetentionTime
        {
            get;
            set;
        }


        //[XmlElement("User_Name")]
        //public String User_Name
        //{
        //    // should be LastCuration.Username ??
        //    get;
        //    set;
        //    //get
        //    //{
        //    //    User usr = new User();
        //    //    return usr.DisplayName;
        //    //    //Only return when there is a Username
        //    //    //if (_username != null && _username != string.Empty)
        //    //    //{
        //    //    //    User usr = new User();
        //    //    //    return usr.DisplayName;
        //    //    //}
        //    //    //return _username;
        //    //}
        //    //set
        //    //{
        //    //    //If the username is not set yet then
        //    //    if (_username == string.Empty)
        //    //    {
        //    //        User usr = new User();
        //    //        _username = usr.DisplayName;
        //    //    }
        //    //    //else
        //    //    //{
        //    //    //    _username = value;
        //    //    //}
        //    //}
        //}

        //string _userID = string.Empty;

        // <summary>
        // UserID for the user who generated the current file
        // </summary>
        //[XmlElement("User_ID")]
        //public string User_ID
        //{
        //    // should be LastCuration.UserID ??

        //    get;
        //    set;
        //    //get
        //    //{
        //    //    User usr = new User();
        //    //    return usr.UserID;
        //    //    //return _userID;
        //    //    //If the UserID is not null get that
        //    //    //if (_userID != null && _userID != string.Empty)
        //    //    //{
        //    //    //    return _userID;
        //    //    //}
        //    //    //return string.Empty;
        //    //}
        //    //set
        //    //{
        //    //    if (_userID == string.Empty)
        //    //    {
        //    //        User usr = new User();
        //    //        _userID = usr.UserID;
        //    //    }
        //    //}
        //    //set
        //    //{
        //    //    //If the UserID is already set then just using that. If not otherwise set the value
        //    //    if (_userID == string.Empty)
        //    //    {
        //    //        User usr = new User();
        //    //        _userID = usr.UserID;
        //    //    }
        //    //    else
        //    //    {
        //    //        _username = value;
        //    //    }
        //    //}

        //}

        //[XmlElement("Date_time_Curation")]
        //public DateTime Date_Time_Curation
        //{
        //    get { return LastCuration == null ? DateTime.MinValue : LastCuration.CreatedDate; }
        //}


        /// <summary>
        /// Expect value (length normalized bit score from Blast)
        /// </summary>
        [XmlElement("ExpectValue")]
        public double ExpectValue
        {
            get
            {
                return _evalue;
            }
            set
            {
                _evalue = value;
            }
        }

        public Curation LastCuration
        {
            get { return Curations.OrderByDescending(x => x.CreatedDate).Where(c => c.IsValid).FirstOrDefault(); }
        }

        //public bool TopHit
        //{
        //    get;
        //    set;
        //}

        double _averageerrors;

        double _bandyionpercent;

        //bool _isconfirmed;

        //[XmlElement("IsConfirmed")]
        public bool IsConfirmed
        {
            get
            {
                //return _isconfirmed;
                return LastCuration != null;
            }
            //set
            //{
            //    _isnotconfirmed = !value;
            //    _isconfirmed = value;
            //}
        }

        /// <summary>
        /// This is the opposite of IsConfirmed and is a helper function for binding to the IsEnabled state in XAML
        /// </summary>
        public bool IsNotConfirmed
        {
            get
            {
                return !IsConfirmed;
            }
        }


        //bool _isnotconfirmed;
        //[XmlElement("IsNotConfirmed")]
        //public int IsNotConfirmed
        //{
        //    get
        //    {
        //        return (int)!IsConfirmed;
        //        //if (IsNotConfirmedBool)
        //        //{
        //        //    return 0;
        //        //}
        //        //return 1;
        //    }
        //}

        //[XmlElement("IsNotConfirmedBool")]
        //public bool IsNotConfirmedBool
        //{
        //    get
        //    {
        //        return !_isconfirmed;
        //    }
        //}

        [XmlElement("Curations")]
        public ObservableCollection<Curation> Curations = new ObservableCollection<Curation>();

        //[XmlElement("Curations")]
        //public List<Curation> Curations
        //{
        //    get
        //    {
        //        return _curations;
        //    }
        //}

        /// <summary>
        /// CurrentBandYIon Percent.
        /// If there is a value then it shows, if not it sets to 10000
        /// </summary>
        [XmlElement("BandYIonPercent")]
        public double BandYIonPercent
        {
            get
            {
                if (_bandyionpercent != 0.0)
                    return _bandyionpercent;
                else
                    return 0;
            }
            set
            {
                _bandyionpercent = value;
            }
        }

        [XmlElement("AverageErrors")]
        public double AverageErrors
        {
            get
            {
                if (_averageerrors != 0.0)
                    return _averageerrors;

                else
                {
                    List<double> allmonomass = new List<double>();

                    if (CurntIons != null)
                    {
                        allmonomass = CurntIons.Select(a => a.MonoMass).ToList();
                    }

                    if (Sequence != null && allmonomass != null && App.AllValidationModifications != null && AminoAcidHelpers.AminoAcids != null && CurntIons != null && ValidatedSequence != "")
                    {
                        Dictionary<double, double> currentmonomass = new Dictionary<double, double>();

                        CurntIons = CurntIons.GroupBy(a => a.MonoMass).Select(a => a.First()).ToList();

                        for (int i = 0; i < CurntIons.Count; i++)
                        {
                            //if (currentmonomass.ContainsKey(CurntIons[i].MonoMass))
                            //{
                            //    continue;
                            //}
                            currentmonomass.Add(CurntIons[i].MonoMass, Convert.ToDouble(CurntIons[i].Intensity));
                        }

                        ///To do: Get the activation type populated here.
                        var bandyions = CalculateBYCZIons.CalculateBYCZIon(ValidatedSequence, allmonomass, ParentMass, string.Empty, App.AllValidationModifications, Properties.Settings.Default.MatchTolerancePPM, AminoAcidHelpers.AminoAcids, true, currentmonomass);
                        List<double> bandyionerrorsppm = bandyions.Where(a => a.bionerror != 100).Select(a => a.bionPPM).ToList();
                        bandyionerrorsppm.AddRange(bandyions.Where(a => a.yionerror != 100).Select(a => a.yionPPM).ToList());

                        if (bandyionerrorsppm.Count == 0) return 0;
                        int bandyioncolorcount = bandyions.Where(a => a.bioncolor || a.yioncolor).Count();

                        _bandyionpercent = Math.Round((((double)bandyioncolorcount / (double)bandyions.Count) * 100), 4);

                        bandyionerrorsppm = bandyionerrorsppm.Where(a => a != 0.0).ToList();

                        if (bandyionerrorsppm.Count == 0) return 0;
                        double medianppm = bandyionerrorsppm.Median();

                        bandyionerrorsppm = bandyionerrorsppm.Select(a => Math.Abs(a - medianppm)).Where(a => a != 0.0).ToList();

                        if (bandyionerrorsppm.Any())
                        {
                            _averageerrors = bandyionerrorsppm.Average();
                        }
                        else
                        {
                            _averageerrors = 0.001;
                        }
                        return Math.Round(_averageerrors, 3);

                    }
                    return 0;
                }
            }

            set
            {
                _averageerrors = value;
            }
        }



        string _validatedsequence;

        [XmlElement("ValidatedSequence")]
        public string ValidatedSequence
        {
            get
            {
                if (_validatedsequence != null && _validatedsequence != "")
                    return _validatedsequence;
                else
                    return "";
            }
            set
            {
                _validatedsequence = value;
            }
        }

        [XmlElement("AllMonomass")]
        public List<double> AllMonomass { get; set; }
        /// <summary>
        /// Saves all the monomasses and their respective intensities for a particular scan
        /// </summary>
        //[XmlElement("MonomasseswithIntensities")]
        [XmlIgnore]
        //[JsonIgnore]
        public Dictionary<double, double> MonomasseswithIntensities { get; set; }

        public string IsIdentifiedYesNo { get { return IsAutoScanIdentified ? "Yes" : "No"; } }

        [XmlElement()]
        public bool IsAutoScanIdentified { get; set; }

        [XmlElement()]
        public string Accession { get; set; }

        [XmlElement()]
        public string Description { get; set; }

        [XmlElement("Species")]
        public string Species { get; set; }

        string sequence = string.Empty;
        
        /// <summary>
        /// The Protein sequence resulting from the query
        /// </summary>
        [XmlElement("Sequence")]
        public string Sequence
        {
            get
            {
                return sequence;
            }

            set
            {
                sequence = value;
                leucineNeutralSequence = value.MakeLeucineNeutral();
            }
        }

        string leucineNeutralSequence;

        /// <summary>
        /// Amino Acid sequence of the protein match with all Leucines and Isoleucines represented as J
        /// </summary>
        [XmlIgnore]
        public string IsobaricSequence
        {
            get
            {
                return leucineNeutralSequence;
            }

        }

        [XmlElement("PairMatch")]
        public List<string> PairMatch { get; set; }

        [XmlElement("MyNumber")]
        public int MyNumber { get; set; }

        //List<String> _allthesequencetags;

        //[XmlElement("Allthesequencetags")]
        [XmlIgnore]
        public IEnumerable<string> Allthesequencetags
        {
            get
            {
                return allsqstags.Select(t => t.Sequence);
            }

            //get
            //{
            //    if (_allthesequencetags != null)
            //    {
            //        return _allthesequencetags.GroupBy(a => a).First().ToList();
            //    }
            //    else if (AllsqsTags != null && AllsqsTags.Count > 0)
            //    {
            //        return AllsqsTags.Select(a => a.RawSequence).GroupBy(a => a).First().ToList();
            //    }
            //    return new List<string>();
            //}
            //set
            //{
            //    _allthesequencetags = value;
            //}
        }

        /// <summary>
        /// Calculated Protein Mass from AminoAcid Protein Sequence
        /// </summary>
        [XmlIgnore]
        public double Mass
        {
            get
            {
                //TODO: should this be the mass of the "Covered Sequence" ????

                return this.Sequence.TotalMass();
            }
        }

        [XmlIgnore]
        public double DeltaMass
        {
            get
            {
                if (LastCuration != null) return LastCuration.Delta;

                return Math.Abs(this.ParentMass - Mass);  //  spec.ParentIon.MonoMass - aHit.Protein.Mass),
            }
        }

        [XmlElement("TagCount")]
        public int TagCount { get; set; }

        [XmlElement("ScanNumbers")]
        public string ScanNumbers { get; set; } // Scan numbers

        [XmlElement("ParentZ")]
        public int ParentZ { get; set; }  //Z

        string tagfortopprotein;

        //public double PercentBYIonCoverage
        //{
        //    get
        //    {
        //        double totalpercent = 0.0;
        //        if (CurntIons != null && Sequence != null && ParentMass != null)
        //        {
        //            List<double> CrntIons = CurntIons.Select(a => a.MonoMass).ToList();

        //            var allbandyions = CalculateBandYIons.CalculateBandYIon(Sequence, CrntIons, ParentMass);
        //            double bandhionscount = allbandyions.Where(a => a.yioncolor || a.bioncolor).Count();
        //            totalpercent = Math.Round((bandhionscount / allbandyions.Count) * 100, 2);
        //        }
        //        return totalpercent;
        //    }
        //}

        string _coverage;

        [XmlElement("Coverage")]
        public string Coverage
        {
            get
            {
                string color = "White";
                //if (DeltaMass != 0)
                {
                    if (DeltaMassVersusProtein <= 1.0)
                    {
                        color = "Green";
                    }
                    else if (DeltaMassVersusProtein > 1.0 && DeltaMassVersusProtein <= 80.00)
                    {
                        //color = "Yellow";
                        color = "Orange";
                    }
                    else if (DeltaMassVersusProtein > 80.00)
                    {
                        color = "Red";
                    }
                }
                return color;

            }
            set
            {
                value = _coverage;
            }
        }


        //public bool RedCoverage
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        //public bool GreenCoverage
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}

        //public bool YellowCoverage
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}

        [XmlElement("AppendedScanNumber")]
        public int AppendedScanNumber
        {
            get
            {
                if (ScanNumbers != null)
                {
                    char[] splitchar = { ',' };
                    return Convert.ToInt32(ScanNumbers.Split(splitchar, StringSplitOptions.RemoveEmptyEntries)[0]);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Start position of alignment on subject sequence (1-based)
        /// </summary>
        [XmlElement("BlastTagStart")]
        public int BlastTagStart { get; set; }

        /// <summary>
        /// End position of alignment on subject sequence (1-based)
        /// </summary>
        [XmlElement("BlastTagEnd")]
        public int BlastTagEnd { get; set; }

        /// <summary>
        /// Start position of alignment on query sequence (1-based)
        /// </summary>
        [XmlElement("BlastQueryStart")]
        public int BlastQueryStart { get; set; }

        /// <summary>
        /// End position of alignment on query sequence (1-based)
        /// </summary>
        [XmlElement("BlastQueryEnd")]
        public int BlastQueryEnd { get; set; }

        [XmlElement("Checked")]
        public bool Checked { get; set; }


        /// <summary>
        /// This is the original query tag used for the blast search
        /// </summary>
        [XmlElement("BlastedTagForTopProtein")]
        public string BlastedTagForTopProtein { get; set; }

        /// <summary>
        /// The sequence tag used as a query to obtain the protein match
        /// </summary>
        [XmlIgnore]
        public string QueryTag
        {
            get
            {
                return BlastedTagForTopProtein ?? TagForTopProtein ?? PairMatch.FirstOrDefault();
            }
        }

        /// <summary>
        /// the longest continuous run of matching amino acids between the query and the matching protein
        /// </summary>
        [XmlIgnore]
        public string LongestMatch
        {
            get
            {
                return PartialSequenceMatches.LongestCommonSubstring(Sequence.Replace('I', 'L'), QueryTag);
            }
        }

        /// <summary>
        /// Is the protein match the result of a BLAST search
        /// </summary>
        [XmlIgnore]
        public bool IsBlastResult
        {
            get
            {
                return BlastedTagForTopProtein != null;
            }
        }

        /// <summary>
        /// The QueryTag used for an exact match search
        /// </summary>
        [XmlElement("TagForTopProtein")]
        public string TagForTopProtein { get; set; }


        /// <summary>
        /// 80% match for a good hit.
        /// Calculate this before calculating hit score.
        /// Levenstein distance calculation for percent match. How many Edits ? 80 %
        /// </summary>
        [XmlElement("VerifyBlastTag")]
        public bool VerifyBlastTag
        {
            get
            {
                if (BlastTag != null && (TagForTopProtein != null || PairMatch != null))
                {
                    string temp = TagForTopProtein == null ? PairMatch.First() : TagForTopProtein;

                    if (BlastTag == string.Empty)
                        return true;

                    //if (BlastTag.Length != 0)/// && (BlastTag.Length >= 4) || ((BlastTag.Length / temp.Length) * 100 >= 80))
                    //{
                    //    //PartialSequenceMatches.TotalPartialMatchesforBlastp(Sequence, BlastTag, BlastTagStart, BlastTagEnd);
                    //    return true;
                    //}
                    //if (BlastTag.Length != 0 && ((BlastTag.Length >= 4) || (PartialSequenceMatches.TotalPartialMatchesforBlastp(Sequence, PairMatch.First(), BlastQueryStart, BlastQueryEnd, BlastTagStart, BlastTagEnd, BlastTag))))///((BlastTag.Length / temp.Length) * 100 >= 80)))
                    if (BlastTag.Length != 0 && ((PartialSequenceMatches.TotalPartialMatchesforBlastp(Sequence, PairMatch.First(), BlastQueryStart, BlastQueryEnd, BlastTagStart, BlastTagEnd, BlastTag))))  ///((BlastTag.Length / temp.Length) * 100 >= 80)))
                    {
                        return true;
                        //if (PartialSequenceMatches.TotalPartialMatchesforBlastp(Sequence, PairMatch.First(), BlastQueryStart, BlastQueryEnd, BlastTagStart, BlastTagEnd, BlastTag))
                        //{
                        //return true;
                        //PartialSequenceMatches.TotalPartialMatchesforBlastp(Sequence, BlastTag, BlastTagStart, BlastTagEnd);
                        //}
                        //else
                        //{
                        //return false;
                        //}
                    }
                    //if (temp.Length >= 30)
                    //{
                    //    if ((temp.Length - BlastTag.Length) < 10)
                    //    {
                    //        return true;
                    //    }
                    //    else
                    //    {
                    //        return false;
                    //    }
                    //}
                    //else if (temp.Length < 30 & temp.Length >= 20)
                    //{
                    //    if ((temp.Length - BlastTag.Length) < 7)
                    //    {
                    //        return true;
                    //    }
                    //    else
                    //    {
                    //        return false;
                    //    }
                    //}
                    //else if (temp.Length < 20 & temp.Length >= 10)
                    //{
                    //    if ((temp.Length - BlastTag.Length) < 4)
                    //    {
                    //        return true;
                    //    }
                    //    else
                    //    {
                    //        return false;
                    //    }
                    //}
                    //else if (temp.Length < 10 & temp.Length >= 6)
                    //{
                    //    if ((temp.Length - BlastTag.Length) < 1)
                    //    {
                    //        return true;
                    //    }
                    //    else
                    //    {
                    //        return false;
                    //    }
                    //}
                    //else if (temp.Length < 6)
                    //{
                    //    return true;
                    //}
                }
                else if (BlastTag == null)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Tag Start (1-based.  Works for Blast or exact match)
        /// </summary>
        [XmlIgnore]
        public int tStart
        {
            get
            {
                return this.IsBlastResult ? BlastTagStart : this.Sequence.ModifiedSequenceforJ().IndexOf(QueryTag) + 1;
            }
        }

        /// <summary>
        /// Tag End (1-based.  Works for Blast or exact match)
        /// </summary>
        [XmlIgnore]
        public int tEnd
        { 
            get
            {
                return tStart + QueryTag.Length - 1 ;
            }
        }

        [XmlElement("DontShowall")]
        public bool DontShowall { get; set; }
        string _ParentZString = string.Empty;
        bool changePairMatch { get; set; }


        /// <summary>
        /// This is the portion of the original query that matches the protein
        /// </summary>
        //[XmlElement("BlastTag")]
        public string BlastTag
        {
            get
            {
                // Query (de novo tag from spectrum) = NPCSALLSSPMTA
                //                     Protein Match =  PSSSIISSPMT
                //                          BlastTag =  PCSALLSSPMT

                if (BlastQueryStart == BlastQueryEnd) return null;

                return BlastedTagForTopProtein.Substring(BlastQueryStart - 1, (BlastQueryEnd - BlastQueryStart) + 1);

                //return Sequence.Substring(BlastTagStart, BlastTagEnd - BlastTagStart).Replace("I", "L");
            }  
        }


        /// <summary>
        /// Number of Amino Acids from the QueryTag that match when aligned to the protein
        /// </summary>
        [XmlIgnore]
        public int MatchCount
        {
            get
            {
                var result = 0;
                var position = BlastTagStart - 1;
                var neutralBlastTag = BlastTag.MakeLeucineNeutral();

                foreach (var aa in neutralBlastTag)
                    if (aa == IsobaricSequence[position++]) result++;

                return result;
            }
        }

        [XmlElement("Parentz_s")]
        public string Parentz_s
        {
            get
            {
                if (_ParentZString == string.Empty)
                    return ParentZ.ToString();
                else
                    return _ParentZString;
            }

            set
            {
                _ParentZString = value;
            }
        }

        [XmlElement("DirectionofTagforTopProtein")]
        public bool DirectionofTagforTopProtein { get; set; }

        //[XmlIgnore]
        //[XmlElement("allspectrums")]
        public List<SpectrumInfo> allspectrums = new List<SpectrumInfo>();

        [XmlElement("DontchangePairMatch")]
        public bool DontchangePairMatch { get; set; }


        /// <summary>
        /// Delta Mass between Protein Parent and Sequence using a tag as an anchor
        /// </summary>
        [XmlElement("DeltaMassVersusProtein")]
        public double DeltaMassVersusProtein
        {
            get
            {
                if (Sequence.IsValidSequence() == false) return double.NaN;

                if (ParentMass == 0)
                    return 0;
                if (InternalMT.Count != 0)
                {
                    InternalMT = InternalMT.GroupBy(a => a.SequenceTag + Convert.ToString(a.Start)).Select(a => a.First()).ToList();
                }
                string totalseq = string.Join("", InternalMT.Where(a => a.confidence != AminAcidConfidence.NotPossible && a.confidence != AminAcidConfidence.Reallybad).Select(a => a.SequenceTag).ToList());

                var alltags = InternalMT.Where(a => a.SequenceTag != null && a.SequenceTag != "");

                double tempdeltamassversusprotein = 0;

                if ((ParentMass - totalseq.TotalMass()) < Molecules.Water)
                {
                    if (Math.Abs((ParentMass - totalseq.TotalMass()) - Molecules.Water) < 0.1)
                    {
                        tempdeltamassversusprotein = 0;
                    }
                    else
                    {
                        tempdeltamassversusprotein = ParentMass - totalseq.TotalMass();
                    }
                }
                else
                {
                    tempdeltamassversusprotein = ParentMass - (totalseq.TotalMass() + Molecules.Water);
                }
                if (Math.Abs(tempdeltamassversusprotein) < 0.1)
                    tempdeltamassversusprotein = 0;

                return tempdeltamassversusprotein; ///The delta mass is the difference between the ParentMass and (TotalSequenceLength + WaterMolecule)
            }
        }

        FindSequenceTags.SequenceTag _at;


        [XmlElement("AnchorTag")]
        //[XmlIgnore]
        public FindSequenceTags.SequenceTag AnchorTag
        {
            //get; set;
            get
            {
                if (_at != null)
                    return _at;
                //if (!(AllsqsTags.Count > 0)) return new FindSequenceTags.SequenceTag();
                //TODO: Just pick the longest + highest scoring tag from AllsqsTags instead?  
                if (PairMatch != null && PairMatch.Count != 0)
                {
                    if (AllsqsTags.Where(t => t.Sequence == PairMatch[0] || t.ReverseSequence == PairMatch[0]).Any())
                    {
                        if (AllsqsTags.Any(t => t.Sequence == PairMatch[0] || t.ReverseSequence == PairMatch[0]))
                        {
                            _at = AllsqsTags.Where(t => t.Sequence == PairMatch[0] || t.ReverseSequence == PairMatch[0]).OrderByDescending(s => s.Score).First();
                        }
                    }

                    if (BlastTag != string.Empty && BlastTag != null)
                    {
                        if (AllsqsTags.Any(t => t.Sequence.Contains(PairMatch[0]) || t.ReverseSequence.Contains(PairMatch[0])))
                        {
                            _at = AllsqsTags.Where(t => t.Sequence.Contains(PairMatch[0]) || t.ReverseSequence.Contains(PairMatch[0])).OrderByDescending(s => s.Score).First(); //PairMatch;
                        }
                        else if (InternalMT.Where(a => a.SequenceTag != null && a.SequenceTag != string.Empty).Any(t => t.SequenceTag.Contains(PairMatch[0]) || ReverseString.Reverse(t.SequenceTag).Contains(PairMatch[0])))
                        {
                            var at = InternalMT.Where(a => a.SequenceTag != null && a.SequenceTag != string.Empty).Where(t => t.SequenceTag.Contains(PairMatch[0]) || ReverseString.Reverse(t.SequenceTag).Contains(PairMatch[0])).OrderByDescending(s => s.SequenceTag.Length).First(); //PairMatch;
                            _at = new FindSequenceTags.SequenceTag();
                            _at.BlastTag = at.SequenceTag;
                            _at.RawSequence = at.SequenceTag;
                        }
                    }

                    return _at;
                }
                else
                {
                    return new FindSequenceTags.SequenceTag();
                }
                //This might return 0 tags...
                //return AllsqsTags.Where(t => t.DatabaseHitsValue > 0 && t.DatabaseHitsValue < 200).OrderByDescending(t => t.Sequence.Length).OrderByDescending(s => s.Score).FirstOrDefault();
            }
            set
            {
                _at = value;
            }
        }

        [XmlElement("AnchrTag")]
        //[XmlIgnore]
        public FindSequenceTags.SequenceTag AnchrTag
        {
            get;
            set;
        }


        //[XmlIgnore]
        [XmlElement("CurntIons")]
        public List<Cluster> CurntIons { get; set; }

        [XmlElement("TagFound")]
        public bool TagFound { get; set; }


        //[XmlIgnore]
        [XmlElement("RvrseCurntIons")]
        public List<Cluster> RvrseCurntIons { get; set; }

        [XmlElement("PPM")]
        public double PPM
        {
            get
            {
                if (this.ParentMass != 0)
                    return Math.Abs(PPMCalc.CalculatePPM(this.ParentMass, this.DeltaMassVersusProtein));
                else
                    return 0;
            }
        }

        //[XmlElement("AllsqsTags")]
        [XmlIgnore]
      //  [JsonIgnore]
        public List<FindSequenceTags.SequenceTag> AllsqsTags
        {
            get
            {
                return allsqstags;
            }
            set
            {
                allsqstags = value;
            }
        }

        [XmlElement("Notagsforhighlight")]
        public bool Notagsforhighlight { get; set; }

        class MatchStartEnds
        {
            public int Start { get; set; }
            public int End { get; set; }
        }

        List<FindSequenceTags.SequenceTag> allsqstags = new List<FindSequenceTags.SequenceTag>();

        //public MSViewer.ProteinTagMatchResult MatchResult 
        //{ 
        //    get
        //    {
        //        return new ProteinTagMatchResult() { Sequence = this.Sequence, TagMatches = this.matchstartends };
        //    }
        //}

        [XmlElement("SequenceID")]
        public string SequenceID
        {
            set;
            get;
        }

        /// <summary>
        /// Monoisotopic Parent Mass
        /// </summary>
        [XmlElement("ParentMass")]
        public double ParentMass { get; set; }

        [XmlElement("sequencesfordbmatching")]
        //[XmlIgnore]
        public List<MSViewer.MainSequenceTagmatches> sequencesfordbmatching { get; set; }

        //public static int countmatchstartends;

        bool BlastPMatch = false;

        public List<MSViewer.MatchStartEnds> ComputeProteinTagMatches()
        {
            var matches = new List<MSViewer.MatchStartEnds>();
            BlastPMatch = false;
            if (Notagsforhighlight)
            {
                matches.Add(new MSViewer.MatchStartEnds
                {
                    Start = 0,
                    End = Sequence.Length,
                    confidence = AminAcidConfidence.NotSure
                });
            }
            else
            {
                if (AllsqsTags != null && Sequence != null && PairMatch != null && Allthesequencetags != null && CurntIons != null && ParentMass > 0 && AllsqsTags.Count > 0 && Sequence.IsValidSequence()) //&& sequencesfordbmatching != null
                {
                    var allTheTags = new List<string>(Allthesequencetags);

                    CurntIons = CurntIons.OrderBy(a => a.MonoMass).ToList();
                    if (BlastTag != string.Empty && BlastTag != null)
                    {
                        if (changePairMatch)
                        {
                            DontchangePairMatch = false;
                        }

                        //BlastTag = BlastTag.Replace("I", "L");
                        var partialblasttagmatch = PartialMatch(PairMatch[0]);

                        AllsqsTags.AddRange(partialblasttagmatch); ///PartialMatch(BlastTag)
                        
                        allTheTags.AddRange(partialblasttagmatch.Select(a => a.RawSequence));

                        //AllsqsTags = AllsqsTags.OrderByDescending(a => a.Sequence.Length).ToList();


                        matches = ApplyHighlighting.ApplHighlighting(AllsqsTags, new List<string> { BlastTag }, Sequence, allTheTags, sequencesfordbmatching, CurntIons, ParentMass, ref BlastPMatch, null, DontShowall, true, BlastTag, PairMatch[0]);
                        //PairMatch.Add(BlastTag);
                        //if (!DontchangePairMatch)
                        //{
                        //    changePairMatch = true;
                        //    PairMatch.Clear();
                        //    PairMatch.Add(BlastTag);
                        //}

                        //_mt = ApplyHighlighting.ApplHighlighting(AllsqsTags, PairMatch, Sequence, Allthesequencetags, sequencesfordbmatching, CurntIons, ParentMass, null, DontShowall);
                        //AllsqsTags.AddRange(PartialMatch());
                    }
                    //if (BlastTag != string.Empty && BlastTag != null)
                    //{
                    //    List<string> pm = new List<string>();
                    //    pm.Add(BlastTag);
                    //    _mt = ApplyHighlighting.ApplHighlighting(AllsqsTags, pm, Sequence, Allthesequencetags, sequencesfordbmatching, CurntIons, ParentMass, null, DontShowall);
                    //}
                    else
                    {
                        matches = ApplyHighlighting.ApplHighlighting(AllsqsTags, PairMatch, Sequence, allTheTags, sequencesfordbmatching, CurntIons, ParentMass, ref BlastPMatch, null, DontShowall);
                    }

                    //BlastTagforTopProtein = 

                    if (BlastTag == null || BlastTag == string.Empty)
                    {
                        string firstpairmatch = PairMatch.Any() ? PairMatch.First() : string.Empty;
                        string reversefirstpairmatch = ReverseString.Reverse(firstpairmatch);

                        //if (Sequence.Contains(firstpairmatch))
                        //{
                        //    BlastTag = firstpairmatch;
                        //}
                        //else if (Sequence.Contains(reversefirstpairmatch))
                        //{
                        //    BlastTag = reversefirstpairmatch;
                        //}
                    }
                    //if (BlastTag == null)
                    //{
                    //    BlastTag = string.Empty;
                    //}
                    //else if (BlastTag == String.Empty)
                    //{

                    //}
                    //_mt = ApplyHighlighting.ApplHighlighting(AllsqsTags, PairMatch, Sequence, Allthesequencetags, sequencesfordbmatching, CurntIons, ParentMass, null, DontShowall);
                }
                else if (PairMatch != null && PairMatch.Any()) /// && PairMatch.First() != "")
                {
                    if (BlastTag != null && BlastTag != "")
                    {
                        //BlastTag = BlastTag.Replace("I", "L");
                        matches = ApplyHighlighting.ApplyHighlightforBlastPwithnoSequences(Sequence, BlastTag, PairMatch[0], DontShowall);
                        //matches = ApplyHighlighting.ApplyHighlightwithnoSequences(Sequence, (BlastTag != null && BlastTag != "") ? BlastTag : PairMatch[0], DontShowall);
                    }
                    else
                    {
                        //matches = ApplyHighlighting.ApplyHighlightwithnoSequences(Sequence, (BlastTag != null && BlastTag != "") ? BlastTag : PairMatch[0], DontShowall);
                        matches = ApplyHighlighting.ApplyHighlightwithnoSequences(Sequence, PairMatch[0], DontShowall);
                    }
                }
                else
                {
                    _tagHits = 0;
                }
            }

            return matches;
        }

        List<FindSequenceTags.SequenceTag> PartialMatch(string PairMatch)
        {
            List<FindSequenceTags.SequenceTag> partialm = new List<FindSequenceTags.SequenceTag>();
            try
            {
                List<StringPositions> partialmatches = new List<StringPositions>();

                partialmatches = PartialSequenceMatches.Matches(PairMatch, BlastTag);

                if (partialmatches.Count < 1)
                {
                    partialmatches = PartialSequenceMatches.Matches(PairMatch, BlastTag.Reverse());
                    PairMatch = PairMatch.Reverse();
                }


                MSViewer.FindSequenceTags.SequenceTag match = null;

                if (AllsqsTags.Any(a => a.Sequence == (PairMatch)))
                {
                    match = AllsqsTags.First(a => a.Sequence == (PairMatch));
                }
                else if (AllsqsTags.Any(a => a.Sequence == PairMatch.Reverse()))
                {
                    match = AllsqsTags.First(a => a.Sequence == PairMatch.Reverse());
                }
                else if (AllsqsTags.Any(a => a.Sequence.Contains(PairMatch)))
                {
                    match = AllsqsTags.First(a => a.Sequence.Contains(PairMatch));
                }
                else if (AllsqsTags.Where(a => a.Sequence.Contains(PairMatch.Reverse())).Any())
                {
                    match = AllsqsTags.Where(a => a.Sequence.Contains(PairMatch.Reverse())).First();
                }
                else
                {
                    return partialm;
                }
                foreach (var pm in partialmatches)
                {
                    if (match.Sequence.IndexOf(pm.match) > -1)
                    {
                        pm.start = match.Sequence.IndexOf(pm.match);
                        pm.end = match.Sequence.IndexOf(pm.match) + pm.match.Length - 1;
                        List<FindSequenceTags.AminoAcids> aas = new List<FindSequenceTags.AminoAcids>();
                        if (match.IndividualAAs != null)
                        {
                            aas.AddRange(match.IndividualAAs.OrderBy(a => a.End).ThenBy(a => a.Start));
                        }
                        partialm.Add(new FindSequenceTags.SequenceTag()
                        {
                            Start = aas.GetRange(pm.start, 1).First().Start,
                            End = aas.GetRange(pm.end, 1).First().End,
                            RawSequence = pm.match,
                            Score = match.Score,
                            IndividualAAs = aas != new List<MSViewer.FindSequenceTags.AminoAcids>() ? aas.GetRange(pm.start, pm.end - pm.start + 1) : new List<MSViewer.FindSequenceTags.AminoAcids>(),
                            Index = match.Index + pm.start + pm.end + pm.match,
                            MaxScore = match.MaxScore,
                            DatabaseHitsValue = match.DatabaseHitsValue,
                            NumberofAA = pm.match.Length,
                            totalScore = match.totalScore,
                            Ions = match.Ions != null ? match.Ions.GetRange(pm.start, pm.end - pm.start + 1) : new List<SignalProcessing.Cluster>()
                        });
                    }
                    else if (match.Sequence.Reverse().IndexOf(pm.match) > -1)
                    {
                        pm.match = pm.match.Reverse();
                        pm.start = match.Sequence.IndexOf(pm.match);
                        pm.end = match.Sequence.IndexOf(pm.match) + pm.match.Length - 1;
                        List<FindSequenceTags.AminoAcids> aas = new List<FindSequenceTags.AminoAcids>();
                        if (match.IndividualAAs != null)
                        {
                            aas.AddRange(match.IndividualAAs.OrderBy(a => a.End).ThenBy(a => a.Start));
                        }
                        partialm.Add(new FindSequenceTags.SequenceTag()
                        {
                            Start = aas.GetRange(pm.start, 1).First().Start,
                            End = aas.GetRange(pm.end, 1).First().End,
                            RawSequence = pm.match,
                            Score = match.Score,
                            IndividualAAs = aas != new List<MSViewer.FindSequenceTags.AminoAcids>() ? aas.GetRange(pm.start, pm.end - pm.start + 1) : new List<MSViewer.FindSequenceTags.AminoAcids>(),
                            Index = match.Index + pm.start + pm.end + pm.match,
                            MaxScore = match.MaxScore,
                            DatabaseHitsValue = match.DatabaseHitsValue,
                            NumberofAA = pm.match.Length,
                            totalScore = match.totalScore,
                            Ions = match.Ions != null ? match.Ions.GetRange(pm.start, pm.end - pm.start + 1) : new List<SignalProcessing.Cluster>()
                        });
                    }
                }
            }
            catch (Exception)
            {
                int a = 100;
                int b = a + 10;
                double sq = Math.Sqrt(a + b);
            }
            return partialm;
        }


        List<MSViewer.MatchStartEnds> _mt = null;

        //[XmlElement("InternalMT")]
        [XmlIgnore]
        //[JsonIgnore]
        public List<MSViewer.MatchStartEnds> InternalMT
        {
            get
            {
                if (_mt == null)
                {
                    _mt = ComputeProteinTagMatches();
                }
                //if (_mt.Count != 0)
                //{
                //    _mt = _mt.Where(a => a.SequenceTag != null).GroupBy(a => Convert.ToString(a.Start) + a.SequenceTag).First().ToList();
                //}
                return _mt;
            }
            set
            {
                _mt = value;
            }
        }

        [XmlElement("matchstartends")]
        //[XmlIgnore]
        public List<MSViewer.MatchStartEnds> matchstartends
        {
            get
            {
                return _mt;
            }
            set
            {
                _mt = value;
            }
        }

        List<MSViewer.MatchStartEnds> _bpmt;

        /// <summary>
        /// Blast tag compared with the actual tag used for Blast
        /// Creating matchstartends for them
        /// </summary>
        [XmlElement("BlastpPartialMatch")]
        public List<MSViewer.MatchStartEnds> BlastpPartialMatch
        {
            get
            {
                if (_bpmt == null) computeblastpartialtagmatches(); ///ComputeBlastPartialTagMatches();
                return _bpmt;
            }
            set
            {
                _bpmt = value;
            }
        }

        [XmlElement("BlastTagforTopProtein")]
        public string BlastTagforTopProtein
        {
            get;
            set;
        }

        [XmlElement("ActivationType")]
        public string ActivationType
        {
            get;
            set;
        }



        /// <summary>
        /// Aligns the QueryTag to the Protein in this search result to compute amino acid match scores
        /// </summary>
        /// <returns>0 = no match, 5 = perfect match</returns>
        public Dictionary<int, AminAcidConfidence> ComputeQueryTagScores()
        {
            var scores = new Dictionary<int, AminAcidConfidence>();

            if (string.IsNullOrWhiteSpace(QueryTag)) return scores;

            //var mts = new List<MSViewer.MatchStartEnds>();

            //align the query and protein target

            for (int i = 0; i < this.QueryTag.Length; i++)  // enumerate over the 
            {
                var queryAA = QueryTag[i];
                
                if ((BlastQueryStart - 1 > i) || (BlastQueryEnd - 1 < i))
                {
                    // we are outside the bounds of the tag, so this AA is an automatic mismatch
                    scores.Add(i, AminAcidConfidence.Gap);
                }
                else 
                {
                    var proteinAA = Sequence[(BlastTagStart - BlastQueryStart) + i];

                    scores.Add(i, queryAA.AminoAcidCompare(proteinAA));
                }
            }

            //TODO: mark gaps as Blue if they are a mass match




            return scores;
        }


        private void computeblastpartialtagmatches()
        {
            List<MSViewer.MatchStartEnds> mts = new List<MSViewer.MatchStartEnds>();

            ///If there are no Tagfortopprotein and blasttag then don't even bother to start highlighting
            if (TagForTopProtein != null && BlastTag != null && TagForTopProtein != "" && BlastTag != "")// && BlastedTagForTopProtein != null && BlastedTagForTopProtein != "")
            {
                if (BlastedTagForTopProtein == null || BlastedTagForTopProtein == "")
                {
                    BlastedTagForTopProtein = TagForTopProtein;
                }

                var partialmatches = PartialSequenceMatches.Matches(BlastedTagForTopProtein, BlastTag); ///Finding the partial matches in the forward direction

                var reversepartialmatches = PartialSequenceMatches.Matches(BlastedTagForTopProtein.Reverse(), BlastTag); //Find the partial matches in reverse direction
                //reversepartialmatches.Reverse();


                int end = 0;
                int lengthofpairmatch = BlastedTagForTopProtein.Length; ///Length of the tagfortopprotein
                if (partialmatches.Any()) //If there are any tags in the forward direction
                {
                    if (reversepartialmatches.Any()) //Also checking if there are any tags in the reverse direction. If there are tags in both the directions then we need to compare the lengths of the longest tags
                    {
                        if (partialmatches.First().match.Length >= reversepartialmatches.First().match.Length) ///Comparing the lengths
                        {
                            _bpmt = new List<MSViewer.MatchStartEnds>();
                            ///Iterate through the partialmatches
                            foreach (var pmatch in partialmatches)
                            {
                                var startposition = Math.Min(end, pmatch.start);
                                var endposition = Math.Min(end, pmatch.start) + Math.Abs(end - pmatch.start);

                                if (end > pmatch.start)
                                {
                                    if (BlastTagforTopProtein == null) BlastTagforTopProtein = BlastedTagForTopProtein;
                                    return;
                                }



                                _bpmt.Add(new MSViewer.MatchStartEnds() ///Blastmismatch before the match
                                {
                                    confidence = AminAcidConfidence.BlastTagMisMatch,
                                    Start = end,
                                    End = pmatch.start,
                                    SequenceTag = BlastedTagForTopProtein.Substring(startposition, endposition - startposition)/// (startposition, endposition)/// (end, pmatch.start - end)
                                });
                                _bpmt.Add(new MSViewer.MatchStartEnds() ///Blastmatch which is the actual match
                                {
                                    confidence = AminAcidConfidence.BlastTagMatch,
                                    Start = pmatch.start,
                                    End = pmatch.end,
                                    SequenceTag = pmatch.match
                                });

                                end = pmatch.end;
                            }
                            if (end != lengthofpairmatch)  //Once all the Pairmatches are found then we
                            {
                                _bpmt.Add(new MSViewer.MatchStartEnds()
                                {
                                    confidence = AminAcidConfidence.BlastTagMisMatch,
                                    Start = end,
                                    End = lengthofpairmatch,
                                    SequenceTag = BlastedTagForTopProtein.Substring(end, lengthofpairmatch - end)
                                });
                            }
                            BlastTagforTopProtein = BlastedTagForTopProtein;
                            //BlastTagforTopProtein = TagForTopProtein;
                        }
                        else ///If the reverse tag is longer than the forward tag, then we use the reverse partial matches
                        {
                            _bpmt = new List<MSViewer.MatchStartEnds>();
                            string ReverseTagForTopProtein = BlastedTagForTopProtein.Reverse(); //ReverseString.ReverseStr
                            int length = ReverseTagForTopProtein.Length;

                            if ((length == reversepartialmatches.Select(a => a.end).Max()) && (reversepartialmatches.Select(a => a.start).Min() == 0))
                            {
                                List<StringPositions> rvrpms = new List<StringPositions>();

                                //reversepartialmatches.Reverse();

                                foreach (var rvrs in reversepartialmatches)
                                {
                                    rvrpms.Add(new StringPositions()
                                    {
                                        start = length - rvrs.end,
                                        end = length - rvrs.start,
                                        match = rvrs.match.Reverse()
                                    });
                                }

                                reversepartialmatches.Clear();
                                reversepartialmatches.AddRange(rvrpms);
                            }

                            foreach (var pmatch in reversepartialmatches)
                            {
                                _bpmt.Add(new MSViewer.MatchStartEnds()
                                {
                                    confidence = AminAcidConfidence.BlastTagMisMatch,
                                    Start = end,
                                    End = pmatch.start,
                                    SequenceTag = (ReverseTagForTopProtein.Substring(end, pmatch.start - end))
                                });
                                _bpmt.Add(new MSViewer.MatchStartEnds()
                                {
                                    confidence = AminAcidConfidence.BlastTagMatch,
                                    Start = pmatch.start,
                                    End = pmatch.end,
                                    SequenceTag = (pmatch.match)
                                });
                                end = pmatch.end;
                            }
                            if (end != lengthofpairmatch)
                            {
                                _bpmt.Add(new MSViewer.MatchStartEnds()
                                {
                                    confidence = AminAcidConfidence.BlastTagMisMatch,
                                    Start = end,
                                    End = lengthofpairmatch,
                                    SequenceTag = (ReverseTagForTopProtein.Substring(end, lengthofpairmatch - end))
                                });
                            }
                            BlastTagforTopProtein = BlastedTagForTopProtein.Reverse();
                            // BlastTagforTopProtein = ReverseString.ReverseStr(TagForTopProtein);  ///ReverseString.ReverseStr
                            //_bpmt.Reverse();
                        }
                    }
                    else
                    {
                        _bpmt = new List<MSViewer.MatchStartEnds>();
                        foreach (var pmatch in partialmatches)
                        {
                            if (!(pmatch.start - end < 0))
                            {
                                _bpmt.Add(new MSViewer.MatchStartEnds()
                                {
                                    confidence = AminAcidConfidence.BlastTagMisMatch,
                                    Start = end,
                                    End = pmatch.start,
                                    SequenceTag = BlastedTagForTopProtein.Substring(end, pmatch.start - end)
                                });
                                _bpmt.Add(new MSViewer.MatchStartEnds()
                                {
                                    confidence = AminAcidConfidence.BlastTagMatch,
                                    Start = pmatch.start,
                                    End = pmatch.end,
                                    SequenceTag = pmatch.match
                                });
                            }
                            end = pmatch.end;
                        }
                        if (end != lengthofpairmatch)
                        {
                            _bpmt.Add(new MSViewer.MatchStartEnds()
                            {
                                confidence = AminAcidConfidence.BlastTagMisMatch,
                                Start = end,
                                End = lengthofpairmatch,
                                SequenceTag = BlastedTagForTopProtein.Substring(end, lengthofpairmatch - end)
                            });
                        }
                        BlastTagforTopProtein = BlastedTagForTopProtein;
                        //BlastTagforTopProtein = TagForTopProtein;
                    }
                }
                else if (reversepartialmatches.Any())
                {
                    _bpmt = new List<MSViewer.MatchStartEnds>();
                    string ReverseTagForTopProtein = BlastedTagForTopProtein.Reverse();  //ReverseString.ReverseStr


                    int length = ReverseTagForTopProtein.Length;

                    if ((length == reversepartialmatches.Select(a => a.end).Max()) && (reversepartialmatches.Select(a => a.start).Min() == 0))
                    {
                        List<StringPositions> rvrpms = new List<StringPositions>();

                        //reversepartialmatches.Reverse();

                        foreach (var rvrs in reversepartialmatches)
                        {
                            rvrpms.Add(new StringPositions()
                            {
                                start = length - rvrs.end,
                                end = length - rvrs.start,
                                match = rvrs.match.Reverse()
                            });
                        }

                        reversepartialmatches.Clear();
                        reversepartialmatches.AddRange(rvrpms);
                    }

                    foreach (var pmatch in reversepartialmatches)
                    {
                        if (!(pmatch.start - end < 0))
                        {
                            _bpmt.Add(new MSViewer.MatchStartEnds()
                            {
                                confidence = AminAcidConfidence.BlastTagMisMatch,
                                Start = end,
                                End = pmatch.start,
                                SequenceTag = (ReverseTagForTopProtein.Substring(end, pmatch.start - end))
                            });
                            _bpmt.Add(new MSViewer.MatchStartEnds()
                            {
                                confidence = AminAcidConfidence.BlastTagMatch,
                                Start = pmatch.start,
                                End = pmatch.end,
                                SequenceTag = (pmatch.match)
                            });
                            end = pmatch.end;
                        }
                    }
                    if (end != lengthofpairmatch)
                    {
                        _bpmt.Add(new MSViewer.MatchStartEnds()
                        {
                            confidence = AminAcidConfidence.BlastTagMisMatch,
                            Start = end,
                            End = lengthofpairmatch,
                            SequenceTag = (ReverseTagForTopProtein.Substring(end, lengthofpairmatch - end))
                        });
                    }
                    BlastTagforTopProtein = BlastedTagForTopProtein.Reverse();
                    //BlastTagforTopProtein = ReverseString.ReverseStr(TagForTopProtein);  ///ReverseString.ReverseStr
                    //_bpmt.Reverse();
                }
                else
                {
                    BlastTagforTopProtein = BlastTag;
                    _bpmt = new List<MSViewer.MatchStartEnds>();
                    _bpmt.Add(new MSViewer.MatchStartEnds()
                    {
                        confidence = AminAcidConfidence.BlastTagMatch,
                        Start = 0,
                        End = BlastTag.Length,
                        SequenceTag = BlastTag,
                    });
                }
            }
            else
            {
                BlastTagforTopProtein = BlastedTagForTopProtein;
                //BlastTagforTopProtein = TagForTopProtein;
            }
        }


        //private void ComputeBlastPartialTagMatches()
        //{
        //    List<MSViewer.MatchStartEnds> mts = new List<MSViewer.MatchStartEnds>();

        //    if (TagForTopProtein != null && BlastTag != null && TagForTopProtein != "" && BlastTag != "")
        //    {
        //        ///If the Blasttag and Tag used for blasting are in the same direction
        //        if (TagForTopProtein.IndexOf(BlastTag) > -1)
        //        {
        //            _bpmt = new List<MSViewer.MatchStartEnds>();

        //            _bpmt.Add(new MSViewer.MatchStartEnds()
        //            {
        //                confidence = Confidence.BlastTagMisMatch,
        //                Start = 0,
        //                End = TagForTopProtein.IndexOf(BlastTag),
        //                SequenceTag = TagForTopProtein.Substring(0, TagForTopProtein.IndexOf(BlastTag))
        //            });
        //            _bpmt.Add(new MSViewer.MatchStartEnds()
        //            {
        //                confidence = Confidence.BlastTagMatch,
        //                Start = TagForTopProtein.IndexOf(BlastTag),
        //                End = TagForTopProtein.IndexOf(BlastTag) + BlastTag.Length,
        //                SequenceTag = BlastTag
        //            });
        //            _bpmt.Add(new MSViewer.MatchStartEnds()
        //            {
        //                confidence = Confidence.BlastTagMisMatch,
        //                Start = TagForTopProtein.IndexOf(BlastTag) + BlastTag.Length,
        //                End = TagForTopProtein.Length,
        //                SequenceTag = TagForTopProtein.Substring(TagForTopProtein.IndexOf(BlastTag) + BlastTag.Length, TagForTopProtein.Length - (TagForTopProtein.IndexOf(BlastTag) + BlastTag.Length))
        //            });
        //        }
        //        ///If the blasttag and the tag used for blasting are in the reverse direction
        //        else if (TagForTopProtein.IndexOf(ReverseString.ReverseStr(BlastTag)) > -1)
        //        {
        //            _bpmt = new List<MSViewer.MatchStartEnds>();
        //            string rvrBlastTag = ReverseString.ReverseStr(BlastTag);
        //            _bpmt.Add(new MSViewer.MatchStartEnds()
        //            {
        //                confidence = Confidence.BlastTagMisMatch,
        //                Start = TagForTopProtein.IndexOf(rvrBlastTag) + rvrBlastTag.Length,
        //                End = TagForTopProtein.Length,
        //                SequenceTag = ReverseString.ReverseStr(TagForTopProtein.Substring(TagForTopProtein.IndexOf(rvrBlastTag) + rvrBlastTag.Length, TagForTopProtein.Length - (TagForTopProtein.IndexOf(rvrBlastTag) + BlastTag.Length)))
        //            });
        //            _bpmt.Add(new MSViewer.MatchStartEnds()
        //            {
        //                confidence = Confidence.BlastTagMatch,
        //                Start = TagForTopProtein.IndexOf(rvrBlastTag),
        //                End = TagForTopProtein.IndexOf(rvrBlastTag) + rvrBlastTag.Length,
        //                SequenceTag = ReverseString.ReverseStr(rvrBlastTag)
        //            });
        //            _bpmt.Add(new MSViewer.MatchStartEnds()
        //            {
        //                confidence = Confidence.BlastTagMisMatch,
        //                Start = 0,
        //                End = TagForTopProtein.IndexOf(rvrBlastTag),
        //                SequenceTag = ReverseString.ReverseStr(TagForTopProtein.Substring(0, TagForTopProtein.IndexOf(rvrBlastTag)))
        //            });
        //        }
        //        ///If part of the blast tag is part of the tag used for blasting then index is of less help.
        //        else
        //        {
        //            var partialmatches = PartialSequenceMatches.Matches(TagForTopProtein, BlastTag);

        //            if (partialmatches.Count > 0)
        //            {
        //                //bool reverseblast;
        //                //if (partialmatches.Count == 0)
        //                //{
        //                //    partialmatches = PartialSequenceMatches.Matches(ReverseString.ReverseStr(TagForTopProtein), BlastTag);
        //                //    if (partialmatches.Count == 0)
        //                //        return;
        //                //    reverseblast = true;
        //                //}

        //                _bpmt = new List<MSViewer.MatchStartEnds>();
        //                var pmatch = partialmatches.First();
        //                _bpmt.Add(new MSViewer.MatchStartEnds()
        //                {
        //                    confidence = Confidence.BlastTagMisMatch,
        //                    Start = 0,
        //                    End = pmatch.start,
        //                    SequenceTag = TagForTopProtein.Substring(0, pmatch.start)
        //                });
        //                _bpmt.Add(new MSViewer.MatchStartEnds()
        //                {
        //                    confidence = Confidence.BlastTagMatch,
        //                    Start = pmatch.start,
        //                    End = pmatch.end,
        //                    SequenceTag = pmatch.match
        //                });
        //                _bpmt.Add(new MSViewer.MatchStartEnds()
        //                {
        //                    confidence = Confidence.BlastTagMisMatch,
        //                    Start = pmatch.end,
        //                    End = TagForTopProtein.Length,
        //                    SequenceTag = TagForTopProtein.Substring(pmatch.end, TagForTopProtein.Length - (pmatch.end))
        //                });
        //            }
        //            else
        //            {
        //                partialmatches = PartialSequenceMatches.Matches(ReverseString.ReverseStr(TagForTopProtein), BlastTag);

        //                if (partialmatches.Count > 0)
        //                {
        //                    var pmatch = partialmatches.First();
        //                    _bpmt = new List<MSViewer.MatchStartEnds>();
        //                    _bpmt.Add(new MSViewer.MatchStartEnds()
        //                    {
        //                        confidence = Confidence.BlastTagMisMatch,
        //                        Start = pmatch.end,
        //                        End = TagForTopProtein.Length,
        //                        SequenceTag = ReverseString.ReverseStr(TagForTopProtein.Substring(pmatch.end, TagForTopProtein.Length - (pmatch.end)))
        //                    });
        //                    _bpmt.Add(new MSViewer.MatchStartEnds()
        //                    {
        //                        confidence = Confidence.BlastTagMatch,
        //                        Start = pmatch.start,
        //                        End = pmatch.end,
        //                        SequenceTag = ReverseString.ReverseStr(pmatch.match)
        //                    });
        //                    _bpmt.Add(new MSViewer.MatchStartEnds()
        //                    {
        //                        confidence = Confidence.BlastTagMisMatch,
        //                        Start = 0,
        //                        End = pmatch.start,
        //                        SequenceTag = ReverseString.ReverseStr(TagForTopProtein.Substring(0, pmatch.start))
        //                    });
        //                }
        //            }
        //        }
        //    }
        //}

        double _tagHits = -1;

        double _yellowandgreentagHits = -1;

        [XmlElement("YellowandGreenTagHits")]
        public double YellowandGreenTagHits
        {
            get
            {
                if (Notagsforhighlight)
                    return 0;
                if (_yellowandgreentagHits != -1) return _yellowandgreentagHits;

                if (!this.InternalMT.Any())
                    return 0;

                if (this.InternalMT.Where(a => (a.confidence == AminAcidConfidence.Reallybad)).Any())
                {
                    var mt = this.InternalMT.Where(a => a.Length != 0);

                    //_yellowandgreentagHits = mt.Where(a => a.confidence == Confidence.Sure).Select(a => a.Length + 1).Sum();
                    //_yellowandgreentagHits = mt.Where(a => a.confidence == Confidence.Sure).Select(a => a.Length).Sum() + 1;

                    //int end = 0;
                    //int length = 0;
                    //foreach (var a in mt.Where(a => a.confidence == Confidence.Sure).OrderBy(a => a.Start))
                    //{
                    //    length = a.End - a.Start + 1;
                    //    if (a.Start == end && end != 0)
                    //    {
                    //        length = length - 1;
                    //    }
                    //    end = a.End;
                    //}
                    //_yellowandgreentagHits = length;
                    _yellowandgreentagHits = mt.Where(a => a.confidence == AminAcidConfidence.Sure).Select(a => a.Length).Sum() + 1;

                    //_yellowandgreentagHits = _yellowandgreentagHits + (mt.Any(a => a.CTerminus) ? 1.01 : 0);

                    //_tagHits = 2 * (mt.Where(a => a.confidence == Confidence.Sure).OrderByDescending(a => a.Length).First().Length + 1);
                    if (mt.Where(a => a.confidence == AminAcidConfidence.Low).Any())
                    {
                        //_yellowandgreentagHits += (mt.Where(a => a.confidence == Confidence.Low).DistinctBy(x => x.SequenceTag).Select(a => 0.67 * a.NumberofYellowPeaks + 1).Sum()); /// 0.67 per yellow stick as it is approximately equal to the sequence. And 2 is for the ends as there are monomasses on either end.
                        _yellowandgreentagHits += (mt.Where(a => a.confidence == AminAcidConfidence.Low).DistinctBy(x => x.SequenceTag).Select(a => 0.67 * a.NumberofYellowPeaks).Sum()); /// 0.67 per yellow stick as it is approximately equal to the sequence. And 2 is for the ends as there are monomasses on either end.
                        //_tagHits += (mt.Where(a => a.confidence == Confidence.Low).DistinctBy(x => x.SequenceTag).Select(a => 2.0 / (double)a.Length).Sum());
                    }
                    _yellowandgreentagHits = _yellowandgreentagHits + 1;
                }
                else
                {
                    var mt = this.InternalMT.Where(a => a.Length != 0);
                    //double lowconfidenttaghitlength = (mt.Where(a => a.confidence == Confidence.Low).DistinctBy(x => x.SequenceTag).Select(a => (0.67 * a.NumberofYellowPeaks)).Sum());/// 0.67 per yellow stick as it is approximately equal to the sequence. And 2 is for the ends as there are monomasses on either end.
                    //double lowconfidenttaghitlength = (mt.Where(a => a.confidence == Confidence.Low).DistinctBy(x => x.Start).Select(a => 0.50 * a.NumberofYellowPeaks).Sum());/// 0.67 per yellow stick as it is approximately equal to the sequence. And 2 is for the ends as there are monomasses on either end.

                    _yellowandgreentagHits = 0;

                    //int end = 0;
                    //int length = 0;
                    //foreach (var a in mt.Where(a => a.confidence == Confidence.Sure).OrderBy(a => a.Start))
                    //{
                    //    length = a.End - a.Start + 1;
                    //    if (a.Start == end && end != 0)
                    //    {
                    //        length = length - 1;
                    //    }
                    //    end = a.End;
                    //}
                    //_yellowandgreentagHits = length;

                    //double lowconfidenttaghitlength = (mt.Where(a => a.confidence == Confidence.Low).DistinctBy(x => x.SequenceTag).Select(a => 0.67 * a.NumberofYellowPeaks + 1).Sum());/// 0.67 per yellow stick as it is approximately equal to the sequence. And 2 is for the ends as there are monomasses on either end.
                    double lowconfidenttaghitlength = (mt.Where(a => a.confidence == AminAcidConfidence.Low).DistinctBy(x => x.SequenceTag).Select(a => 0.67 * a.NumberofYellowPeaks).Sum());/// 0.67 per yellow stick as it is approximately equal to the sequence. And 2 is for the ends as there are monomasses on either end.


                    _yellowandgreentagHits = (mt.Where(a => a.confidence == AminAcidConfidence.Sure).DistinctBy(x => x.SequenceTag).Select(a => a.Length).Sum() + 1) + lowconfidenttaghitlength;
                    //_yellowandgreentagHits = (mt.Where(a => a.confidence == Confidence.Sure).DistinctBy(x => x.SequenceTag).Select(a => a.Length).Sum() + 1) + lowconfidenttaghitlength;

                    //List<MSViewer.MatchStartEnds> suretags = mt.Where(a => a.confidence == Confidence.Sure).DistinctBy(x => x.Start).ToList();
                    //foreach (var item in suretags)
                    //{
                    //    _yellowandgreentagHits += item.SequenceTag.Length;
                    //}



                    //_yellowandgreentagHits += lowconfidenttaghitlength;
                    //_yellowandgreentagHits = (mt.Where(a => a.confidence == Confidence.Sure).DistinctBy(x => x.SequenceTag).Select(a => a.Length).Sum()) + lowconfidenttaghitlength;
                    //_yellowandgreentagHits = (mt.Where(a => a.confidence == Confidence.Sure).DistinctBy(x => x.SequenceTag).Select(a => a.Length).Sum() + 1) + lowconfidenttaghitlength;
                    //_yellowandgreentagHits = (mt.Where(a => a.confidence == Confidence.Sure).DistinctBy(x => x.Start).Select(a => a.Length).Sum()) + lowconfidenttaghitlength;
                    //_yellowandgreentagHits += (mt.Any(a => a.CTerminus) ? 1.01 : 0);
                    _yellowandgreentagHits = _yellowandgreentagHits + 1;
                }
                if (BlastPMatch)
                {
                    _yellowandgreentagHits = 2.1;
                }
                return _yellowandgreentagHits;
            }

            set
            {
                _yellowandgreentagHits = value;
            }
        }


        /// <summary>
        /// Get the number of amino acid hits from the database with the
        /// sequence tags
        /// </summary>
        [XmlElement("TagHits")]
        public double TagHits
        {
            get
            {
                //App app = (App)Application.Current;

                if (Notagsforhighlight)
                    return 0;

                if (_tagHits != -1) return _tagHits;

                if (!this.InternalMT.Any())
                    return 0;

                if (this.InternalMT.Where(a => (a.confidence == AminAcidConfidence.Reallybad)).Any())
                {
                    var mt = this.InternalMT.Where(a => a.Length != 0);



                    _tagHits = 2 * (mt.Where(a => a.confidence == AminAcidConfidence.Sure).OrderByDescending(a => a.Length).First().Length + 1);
                    if (mt.Where(a => a.confidence == AminAcidConfidence.Low).Any())
                    {
                        _tagHits += (mt.Where(a => a.confidence == AminAcidConfidence.Low).DistinctBy(x => x.SequenceTag).Select(a => 2.0 / (double)a.Length).Sum());
                    }
                }
                else
                {
                    var mt = this.InternalMT.Where(a => a.Length != 0);
                    double lowconfidenttaghitlength = (mt.Where(a => a.confidence == AminAcidConfidence.Low).DistinctBy(x => x.SequenceTag).Select(a => 2.0 / (double)a.Length).Sum());
                    _tagHits = 2 * (mt.Where(a => a.confidence == AminAcidConfidence.Sure).DistinctBy(x => x.SequenceTag).Select(a => a.Length).Sum() + 1) + lowconfidenttaghitlength;
                }
                if (BlastPMatch)
                {
                    _tagHits = 2.1;
                }
                return _tagHits;
            }
        }

        [XmlElement("CoveredSequence")]
        public string CoveredSequence
        {
            get
            {
                if (Sequence != "" && Sequence != null)
                {
                    if (this.Start == 0)
                        return "";
                    return this.Sequence.Substring(this.Start - 1, this.End - this.Start);
                }
                else
                    return "";
            }
        }




        int start = -1;

        /// <summary>
        /// This is the position of the start of the covered sequence
        /// </summary>
        [XmlElement("Start")]
        public int Start
        {
            //TODO: This property name is ambiguous and need to be calculated from other properties within the SearchResult

            get
            {
                if (this.InternalMT == null)
                    return 0;

                if (Notagsforhighlight)
                    return 0;

                if (start != -1) return start;

                if (this.InternalMT.Where(a => a.confidence == AminAcidConfidence.Low || a.confidence == AminAcidConfidence.Possible || a.confidence == AminAcidConfidence.Sure || a.confidence == AminAcidConfidence.NotSure).Any())
                {
                    start = this.InternalMT.Where(a => a.confidence == AminAcidConfidence.Low || a.confidence == AminAcidConfidence.Possible || a.confidence == AminAcidConfidence.Sure || a.confidence == AminAcidConfidence.NotSure).Select(a => a.Start).Min() + 1;
                }
                else
                {
                    start = this.InternalMT.Any() ? this.InternalMT.Where(a => a.confidence == AminAcidConfidence.NotPossible || a.confidence == AminAcidConfidence.Reallybad).Select(a => a.Start).Min() + 1 : 0;
                }

                return start;
            }
            set
            {
                start = value;
            }
        }



        int end = -1;

        [XmlElement("End")]
        public int End
        {
            //TODO: This property name is ambiguous and need to be calculated from other properties within the SearchResult

            get
            {
                if (this.InternalMT == null)
                    return 0;

                if (Notagsforhighlight)
                    return 0;

                if (end != -1) return end;

                if (this.InternalMT.Where(a => a.confidence == AminAcidConfidence.Low || a.confidence == AminAcidConfidence.Possible || a.confidence == AminAcidConfidence.Sure || a.confidence == AminAcidConfidence.NotSure).Any())
                {
                    end = this.InternalMT.Where(a => a.confidence == AminAcidConfidence.Low || a.confidence == AminAcidConfidence.Possible || a.confidence == AminAcidConfidence.Sure || a.confidence == AminAcidConfidence.NotSure).Select(a => a.End).Max() + 1;
                }
                else
                {
                    end = this.InternalMT.Any() ? this.InternalMT.Where(a => a.confidence == AminAcidConfidence.NotPossible || a.confidence == AminAcidConfidence.Reallybad).Select(a => a.End).Max() + 1 : 0;
                }

                return end;
            }
            set
            {
                end = value;
            }
        }

        int _endend = -1;


        /// <summary>
        /// This is the position of the end of the covered sequence
        /// </summary>
        [XmlIgnore]
        public int endend
        {
            //TODO: This property name is ambiguous and need to be calculated from other properties within the SearchResult

            get
            {
                return End - 1;
            }
        }

        /// <summary>
        /// Combining the Accession and Scan number to identify the correct 
        /// </summary>
        [XmlElement("AccessionandScanNumber")]
        public string AccessionandScanNumber
        {
            get
            {
                if (Accession != null && ScanNumbers != null && Accession != "" && ScanNumbers != "")
                {
                    return Accession + ScanNumbers;
                }
                return "";
            }
        }

        [XmlElement("AccessionandTagstartends")]
        public string AccessionandTagstartends
        {
            get
            {
                if (Accession != null && BlastTagforTopProtein != null && Accession != "" && YellowandGreenTagHits != 0 && BlastTagforTopProtein != "")
                {
                    return Accession + Convert.ToString(YellowandGreenTagHits) + BlastedTagForTopProtein + BlastedTagForTopProtein + Convert.ToString(Start) + Convert.ToString(End);
                }
                else if (Accession != null && Accession != "" && YellowandGreenTagHits != 0)
                {
                    return Accession + Convert.ToString(YellowandGreenTagHits);
                }
                return "";
            }
        }

    }

    public class Curation
    {
        //Note: UserID is guaranteed to always be available, but if someone is working offline, or is not in Lilly, the UserName and UserEmail will be null!  

        public string UserID { get; set; }
        public string UserName { get; set; }

        //public string ScanNumbers { get; set; }

        public string UserEmail { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ValidatedSequence { get; set; }

        public string Accession { get; set; }

        public string Description { get; set; }

        public double Delta
        {
            get
            {
                return ParentMass - (CalculateBYCZIons.sequencelengthwithmodifications(ValidatedSequence) + Molecules.Water);
            }
        }

        public double ParentMass { get; set; }

        public double BandYIonPercent { get; set; }

        /// <summary>
        /// A Deleted result
        /// </summary>
        public bool IsValid { get; set; }

    }

    public class MainSequenceTagmatches
    {
        public double Start { get; set; }
        public double End { get; set; }
        public double MassDiff { get; set; }
        public string Seq { get; set; }

        public override string ToString()
        {
            return "Tag: " + Seq;
        }
    }


}

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

using SignalProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MassSpectrometry
{
    /// <summary>
    /// A Spectrum object that can be an individual or a merged spectrum
    /// </summary>
    public class SuperSpectrum : PeakList
    {


        public IMSProvider ParentProvider = null;

        public int[] ScanNumbers { get; }

        public string Title { get; }

        public SpectrumInfo ParentInfo { get; }

        /// <summary>
        /// Retention time (minutes) of the spectrum.  Average of the component spectra if there are multiple scans merged together
        /// </summary>
        public float RetentionTime
        {
            get { return RetentionTimes.Average(); }
        }

        /// <summary>
        /// Retention times of all the component spectra
        /// </summary>
        public float[] RetentionTimes { get; }

        public int MSLevel { get; }

        private List<SpectrumInfo> parentList = new List<SpectrumInfo>();

        public SuperSpectrum(IMSProvider provider, params int[] scanNumbers)
        {
            ParentProvider = provider;
            ScanNumbers = scanNumbers;

            if (scanNumbers.Any() == false || provider == null) return;

            if (scanNumbers.All(s => ParentProvider.MsLevel(s) == SpecMsLevel.MSMS))
            {
                // All scans are MS2, so let's find the parent ion ?
                //ParentProvider.GetParentInfo

                foreach (var aScanNum in ScanNumbers)
                    parentList.Add(provider.GetParentInfo(aScanNum));

                MSLevel = 2;

                //aScan.ParentZ.HasValue
            }
            else
            {
                // treat as an MS1 spectrum because some or all scans are NOT MS2
                MSLevel = 1;
            }

            if (scanNumbers.Count() == 1)
                Title = provider.Title(scanNumbers.First());
            else
                Title = "Merged MS" + MSLevel + " Spectrum.  Scan Numbers: " + string.Join(",", ScanNumbers);

            if (ParentIon != null) Title += " " + ParentIon.MonoMass.ToString("0.000");  // append the parent mass to the title for MS2 scans?  

            // Average of the retention times
            RetentionTimes = ScanNumbers.Select(sn => provider.RetentionTime(sn)).ToArray();

//            provider
        }

        /// <summary>
        /// A space delimited list of all activations in all caps
        /// </summary>
        public string Activation
        {
            get
            {
                if (this.MSLevel == 1 && Spectrum.MS1ActivationAlwaysEmpty) return string.Empty;

                if (string.IsNullOrWhiteSpace(Spectrum.ActivationOverride) == false) return Spectrum.ActivationOverride;

                List<string> ActivationList = new List<string>();
                
                // Populate the collections with the decharged ions
                foreach (var aScanNumber in this.ScanNumbers)
                    ActivationList.Add(this.ParentProvider.GetActivationType(aScanNumber).Trim().ToUpper());

                return string.Join(" ", ActivationList.Distinct());
            }
        }

        public IEnumerable<Cluster> ParentIons
        {
            get
            {
                return parentList.Select(p => p.ParentIon);
            }

        }

        public Cluster ParentIon
        {
            get 
            {
                // Select the best scoring parent ion as the parent for all the scans.  
                return parentList.MaxBy(p => p.ParentIon.Score).ParentIon;
            }
        }

        //public List<Cluster> VirtualIons
        //{
        //    get
        //    {
        //        //TODO: based on the activation, generate all the possible variations of the ions?  


        //        return Ions;
        //    }
            
        //}

        private List<Cluster> ions = null;

        public List<Cluster> Ions
        {
            get
            {
                //base.AddRange();

                if (ions != null) return ions; //= ParentProvider.DetectIons(this.ScanNumber);

                ions = new List<Cluster>();

                // Populate the collections with the decharged ions
                foreach (var aScanNumber in this.ScanNumbers)
                {
                    var minScore = (this.ParentProvider.MsLevel(aScanNumber) == SpecMsLevel.MSMS) ? 100 : 300;

                    

                    // Gather all the ions from all the scans
                    if (ParentIon != null)
                    {
                        ions.AddRange(ParentProvider.DetectIons(aScanNumber, 1, ParentIon.Z).Where(ion => ion.Score > minScore));  // only run charge detector up to Parent Charge
                    }
                    else
                    {
                        ions.AddRange(ParentProvider.DetectIons(aScanNumber).Where(ion => ion.Score > minScore));
                    }
                }

                // Now, consolidate all those ions where they are the same ion
                ions = SignalProcessor.ConsolidateIons(ions, false);

                //base.AddRange(ions.Peaks)

                return ions;
            }

            //set
            //{
            //    ions = value;
            //    this.Clear();

            //    // Keep the peaks in sync...
            //    foreach (var anIon in ions) this.AddRange(anIon.Peaks);
            //}
        }

    }

    public class Spectrum : PeakList
    {
        internal static bool MS1ActivationAlwaysEmpty = true;

        /// <summary>
        /// Replace Activation with this value
        /// </summary>
        public static string ActivationOverride = string.Empty;


        public override string ToString()
        {
            return "Scan Num = " + this.ScanNumber + ", " + this.Ions.Count + " ions, act = " + this.Activation ?? "N/A" + ", " + this.Title;
        }


        public Spectrum() { }

        public Spectrum(IEnumerable<ClusterPeak> peaks)
            : base(peaks)
        {

        }

        public IMSProvider ParentProvider = null;

        private int msLevel = 0;

        public int MSLevel
        {
            get
            {
                if (msLevel == 0) return ParentIon == null ? 1 : 2;
                return msLevel;
            }

            set
            {
                msLevel = value;
            }
        }


        private string activation = string.Empty;

        public string Activation
        {
            get
            {
                if (this.MSLevel == 1 && Spectrum.MS1ActivationAlwaysEmpty) return string.Empty;

                if (string.IsNullOrWhiteSpace(Spectrum.ActivationOverride) == false) return Spectrum.ActivationOverride;

                return activation;
            }
            
            set
            {
                activation = value.Trim().ToUpper();
            }

        }

        public double? ParentMZ
        {
            get
            {
                if (ParentIon != null) return ParentIon.MonoMZ;
                return null;
            }
            set
            {
                ParentIon.MonoMZ = value.Value;
            }
        }

        public int? ParentCharge
        {
            get
            {
                if (ParentIon != null) return ParentIon.Z;
                return null;
            }

        }

        public double? ParentMass
        {
            get
            {
                if (ParentIon != null) return ParentIon.MonoMass;
                return null;
            }
            set
            {
                if (value != null)
                {
                    ParentIon.MonoMass = value.Value;
                }
            }
        }

        public List<Cluster> Ions
        {
            get
            {
                return ParentProvider.DetectIons(this.ScanNumber);
            }

        }

        public int ScanNumber { get; set; }

        public string Title { get; set; }

        public Cluster ParentIon
        {
            get;
            set;
        }

    }
}

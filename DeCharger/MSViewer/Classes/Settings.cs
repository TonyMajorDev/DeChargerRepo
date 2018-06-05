using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MSViewer.Classes
{
    /// <summary>
    /// Class to represent the Current Settings for the MSViewer
    /// </summary>
    [XmlType("CurrentSettings")]
    [Serializable]
    public class Settings
    {
        public int MinPeak
        {
            get;
            set;
        }

        public bool BinnedLabels
        {
            get;
            set;
        }

        public bool HighBin
        {
            get;
            set;
        }

        public bool Labels
        {
            get;
            set;
        }

        public bool ShowMassTips
        {
            get;
            set;
        }

        public bool ShowOnlyFTMS
        {
            get;
            set;
        }

        public bool JumpToXIC
        {
            get;
            set;
        }

        public bool ShowThermo
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get;
            set;
        }

        public double MassTolerancePPMs
        {
            get;
            set;
        }

        public double MassTolerancePPM
        {
            get;
            set;
        }

        public int SequenceTagLength
        {
            get;
            set;
        }

        public bool SortByDBHits
        {
            get;
            set;
        }

        public bool SortByScore
        {
            get;
            set;
        }

        public bool SequenceDetectionMode
        {
            get;
            set;
        }

        public double MatchTolerancePPM
        {
            get;
            set;
        }

        public double MatchTolerancePPMs
        {
            get;
            set;
        }

        public string MatchList
        {
            get;
            set;
        }

        public string Genus
        {
            get;
            set;
        }

        public bool SearchAllSpecies
        {
            get;
            set;
        }

        public bool ShowHitsWithNoProteinID
        {
            get;
            set;
        }

        public string ModifiedAminoAcids
        {
            get;
            set;
        }

        public bool UseBlast
        {
            get;
            set;
        }

        public bool UseThoroughSearch
        {
            get;
            set;
        }

        public bool AgilentAgreed
        {
            get;
            set;
        }


        public string Emails
        {
            get;
            set;
        }

        public string MatchListProfile
        {
            get;
            set;
        }

        public int MinAgilentThresholdMS1
        {
            get;
            set;
        }

        public int MinAgilentThresholdMS2
        {
            get;
            set;
        }

        public string ConnectionString_dev
        {
            get;
            set;
        }

        public string ConnectionString_prd
        {
            get;
            set;
        }

        public bool CountBlastHits
        {
            get;
            set;
        }

        public string AgilentEULA
        {
            get;
            set;
        }

        public string Species
        {
            get;
            set;
        }

        public string ConnectionStr_dev
        {
            get;
            set;
        }

        public string ConnectionStr_prd
        {
            get;
            set;
        }

        public StringCollection MatchTolerancePPMBasedonFileType
        {
            get;
            set;
        }

        public StringCollection MassTolerancePPMBasedonFileType
        {
            get;
            set;
        }

        public StringCollection MinThresholdMS1
        {
            get;
            set;
        }

        public StringCollection MinThresholdMS2
        {
            get;
            set;
        }

        public bool PPMErrorPlot
        {
            get;
            set;
        }

        public bool DaltonErrorPlot
        {
            get;
            set;
        }

        public double PPMErrorAxisMax
        {
            get;
            set;
        }

        public double PPMErrorAxisMin
        {
            get;
            set;
        }

        public string ValidationModificationsList
        {
            get;
            set;
        }

        public string ValidationModificationsListAminoAcids
        {
            get;
            set;
        }

        public string UnimodModifications
        {
            get;
            set;
        }

        public bool HaveKnownMods
        {
            get;
            set;
        }

        public int MaximumModifications
        {
            get;
            set;
        }

        public int FragementIonTolerance
        {
            get;
            set;
        }

        public bool UseHybridIntensities
        {
            get;
            set;
        }

    }
}

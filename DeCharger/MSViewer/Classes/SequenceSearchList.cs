using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using MSViewer.Properties;
using System.Collections.ObjectModel;

namespace MSViewer.Classes
{
    [XmlRoot("SearchResults")]
    [XmlInclude(typeof(SearchResult))]
    public class SearchSummary
    {
        [XmlArray("SearchResults")]
        [XmlArrayItem("SearchResult")]
        public ObservableCollection<SearchResult> SearchResults = new ObservableCollection<SearchResult>();

        ///
        /// Disable the Save button if the results are loaded from XML.
        /// 

        /// <summary>
        /// Change this to include in the Sequence Search Class
        /// </summary>
        [XmlElement("UserName")]
        public string UserName
        {
            get;
            set;
        }

        //int _version;

        [XmlElement("VersionNumber")]
        public int VersionNumber
        {
            get;
            set;
        }

        /// <summary>
        /// User ID
        /// </summary>
        [XmlElement("UserID")]
        public string UserID
        {
            get;
            set;
        }


        /// <summary>
        /// User Domain
        /// </summary>
        [XmlElement("UserDomain")]
        public string UserDomain
        {
            get;
            set;
        }


        [XmlElement("CurrentSettings")]
        public Settings CurrentSettings
        {
            get;
            set;
        }


        /// <summary>
        /// The name of the raw data file originally used to generate this Sequence Search Result List
        /// </summary>
        [XmlElement("SpectralDataFilename")]
        public string SpectralDataFilename
        {
            get;
            set;
        }

        [XmlElement]
        public Science.Blast.clsFileType.MSFileType InstrumentType { get; set; }


        /// <summary>
        /// SHA-256 of the original source data file of spcetra (like the RAW file or mzML) 
        /// For new results, append with a version number. ??
        /// </summary>
        [XmlElement("SpectralDataFileHash")]
        public string SpectralDataFileHash
        {
            get;
            set;
        }


        ///
        /// Location of the RAW file.
        ///


        /// <summary>
        /// Auto scan runtime.
        /// </summary>
        public string AutoScanRuntime
        {
            get;
            set;
        }

        /// <summary>
        /// Machine Name. From Environment Class.
        /// </summary>
        [XmlElement]
        public string MachineName
        {
            get;
            set;
        }

        [XmlElement]
        public DateTime SearchStartTime { get;  set; }

        [XmlElement]
        public DateTime SearchEndTime { get;  set; }


        /// <summary>
        /// Date and Time of Search Execution
        /// </summary>
        //[XmlElement("CurrentDateTime")]


        public void AddSequenceSearch(SearchResult sequencesearch)
        {
            SearchResults.Add(sequencesearch);
        }


        //public string SpectralDataFilename
        //{ 
        //    get; 
        //    set;
        //}
    }
}

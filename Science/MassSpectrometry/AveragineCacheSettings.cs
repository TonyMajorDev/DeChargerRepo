//
// Copyright (c) Andy Wright Ltd 2022, All rights reserved
// Please see the license file accompanying this software for acceptable use
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MassSpectrometry
{
    /// <summary>
    /// Class to share and serialize Averagine Cache Settings
    /// </summary>

    [Serializable]
    public class AveragineCacheSettings
    {
        #region Private members and constants

        private static AveragineCacheSettings _Instance;
        private string programDataPath;
        private string cacheFileSettingsPath;
        private static XmlSerializer xmlSerializer = new XmlSerializer(typeof(AveragineCacheSettings));

        #endregion Private members and constants

        #region Constructors

        /// <summary>
        /// Constructor with no parameters
        /// </summary>
        public AveragineCacheSettings()
        {
        }

        /// <summary>
        /// Constructor that takes a boolean parameter - the paramter is never used, but we use this constructor to create the instance
        /// </summary>
        /// <param name="IsNew"></param>
        public AveragineCacheSettings(bool IsNew)
        {
            // Initialize
            // Paths
            programDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"DeCharger");

            // Check to see if we have a folder
            if (!Directory.Exists(programDataPath))
                Directory.CreateDirectory(programDataPath);

            cacheFileSettingsPath = System.IO.Path.Combine(programDataPath, "Averagine.xml");

            AveragineDataFiles = new ObservableCollection<string>();

            // Serializer
            //xmlSerializer = new XmlSerializer(typeof(AveragineCacheSettings));

            try
            {
                Deserialize();
            }
            catch (Exception)
            {
                // Read the files
                ReadAveragineDataFiles();
                // Select the first file as the selected file
                SelectedCacheFile = AveragineDataFiles[0];
                // Write the file
                Serialize();
            }
        }

        /// <summary>
        /// Static instance of this class - persist the settings throughout the application
        /// </summary>
        public static AveragineCacheSettings Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new AveragineCacheSettings(true);
                return _Instance;
            }
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<string> AveragineDataFiles { get; set; }

        public string SelectedCacheFile { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Read the local folder to see what files we have
        /// </summary>
        public void ReadAveragineDataFiles()
        {
            // Where are the files - .\Files
            DirectoryInfo di = new DirectoryInfo(@".\files");

            // Create an array of fileInfos
            FileInfo[] cacheFileInfos = di.GetFiles("*.dat");

            // Add the names to the current instance
            // Clear the list
            if (AveragineDataFiles != null)
                AveragineDataFiles.Clear();
            // Iterate the collection
            foreach (FileInfo cacheFileInfo in cacheFileInfos)
            {
                // Add the filename
                AveragineDataFiles.Add(cacheFileInfo.Name);
            }
        }

        public void Save()
        {
            Serialize();
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Serialize the class
        /// </summary>
        private void Serialize()
        {
            // Serialize the instance as xml
            using (var writer = XmlWriter.Create(cacheFileSettingsPath))
            {
                xmlSerializer.Serialize(writer, this);
            }
        }

        /// <summary>
        /// Deserialize the class
        /// </summary>
        private void Deserialize()
        {
            // XMLReader to read in the exising config file
            XmlReader xmlReader = XmlReader.Create(cacheFileSettingsPath);

            // Local class to deserialize to
            var settings = (AveragineCacheSettings)xmlSerializer.Deserialize(xmlReader);

            // Load the current CacheFileList
            this.AveragineDataFiles = settings.AveragineDataFiles;

            // Load the selectedCacheFile
            this.SelectedCacheFile = settings.SelectedCacheFile;
        }

        private void LoadDefaults()
        {
        }

        #endregion Private Methods
    }
}
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace MSViewer
{
    public class DeChargerViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string pName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(pName));
            }
        }

        //public void UpdateState()
        //{
        //    IsSaveEnabled = SearchResults.Any();
        //}

        //bool _saveEnabled;

        //public bool IsSaveEnabled
        //{
        //    get { return _saveEnabled; }
        //    set
        //    {
        //        _saveEnabled = value;
        //        OnPropertyChanged("IsSaveEnabled");
        //    }
        //}

        private RelayActionCommand saveCommand;

        public RelayActionCommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                    saveCommand = new RelayActionCommand() { CanExecuteAction = n => (SearchResults != null && SearchResults.Any()) };

                return saveCommand;
            }

            set
            {
                saveCommand = value;
            }
        }

        private RelayActionCommand saveAsCommand;


        public RelayActionCommand SaveAsCommand
        {
            get
            {
                if (saveAsCommand == null)
                    saveAsCommand = new RelayActionCommand() { CanExecuteAction = n => (SearchResults != null && SearchResults.Any()) };

                return saveAsCommand;
            }

            set
            {
                saveAsCommand = value;
            }
        }

        private RelayActionCommand configCommand;

        public RelayActionCommand ConfigCommand
        {
            get
            {
                if (configCommand == null)
                    configCommand = new RelayActionCommand() { CanExecuteAction = n => true };

                return configCommand;
            }

            set
            {
                configCommand = value;
            }
        }

        public ObservableCollection<SearchResult> SearchResults { get; set; }


        /// <summary>
        /// This is the start Time of a search performed in this application session, not a previously loaded result
        /// </summary>
        public DateTime? SearchStartTime
        {
            get;
            set;
        }

        /// <summary>
        /// This is the end Time of a search performed in this application session, not a previously loaded result
        /// </summary>
        public DateTime? SearchEndTime
        {
            get;
            set;
        }

        public ICollectionView ConfirmedSequencesView { get; private set; }

        public SearchResult LastSelectedItem
        {
            // Based on: https://stackoverflow.com/questions/5372517/how-to-delete-selected-row-from-a-datagridwith-context-menu-in-silverlight

            get { return selectedItem; }
            set
            {
                selectedItem = value;
                //OnPropertyChanged("IsDeletable");
            }
        }
        private SearchResult selectedItem;

        //public System.Windows.Visibility IsDeletable
        //{
        //    get
        //    {
        //        if (SelectedItem.AppendedScanNumber == 2)
        //        {
        //            return System.Windows.Visibility.Collapsed;
        //        }

        //        return System.Windows.Visibility.Visible;
        //    }
        //}

        //public ListCollectionView ProteinView;
        public ICollectionView ProteinView { get; private set; }
        public ICollectionView DenovoView { get; private set; }


        /// <summary>
        /// Current activation Type
        /// </summary>
        public string ActivationType
        {
            get;
            set;
        }

        // Based on: http://social.msdn.microsoft.com/Forums/en-US/915db4e8-0ccf-4c5b-97d3-b8898fcf4bac/filtering-observable-collection-using-collectionview?forum=wpf
        //proteinView.Filter += (item) =>
        //    {
        //        // This is the filter to make the Protein list only show results with a Protein ID
        //        return !string.IsNullOrWhiteSpace((item as SearchResult).Description);
        //    };

        //private ObservableCollection<SearchResult> confirmedSequences;
        //public ObservableCollection<SearchResult> ConfirmedSequences
        //{
        //    get { return confirmedSequences; }
        //    set { confirmedSequences = value; OnPropertyChanged("ConfirmedSequences"); }
        //}

        //private void RefreshConfirmedSequences()
        //{
        //    // Based on: https://social.msdn.microsoft.com/Forums/en-US/f39feb69-d837-4fac-84e2-64d4978ea797/observablecollection-filter?forum=winappswithcsharp

        //    //  Verify the Main Collection is valid, if not return
        //    //  Filter data
        //    var fc = from aResult in SearchResults
        //             where aResult.ValidatedSequence != string.Empty
        //             select aResult;

        //    ConfirmedSequences = new ObservableCollection<SearchResult>(fc);
        //}

        /// <summary>
        /// Reset the ViewModel to a blank/new/empty state
        /// </summary>
        public void Clear()
        {
            CurrentFilesLoaded.Clear();
            SpectralDataFilename = null;

            ClearResults();
        }

        /// <summary>
        /// Reset only the search results of the ViewModel to a blank/new/empty state
        /// </summary>
        public void ClearResults()
        {
            this.SearchResults.Clear();
            //CurrentFilesLoaded.Clear();

            // Remove any previously loaded search results, since we are now loading a new one.  
            if (CurrentFilesLoaded.Contains(WorkspaceFilename)) CurrentFilesLoaded.Remove(WorkspaceFilename);

            WorkspaceFilename = null;

            this.SearchStartTime = null;
            this.SearchEndTime = null;

            //txtblkSequenceSearchResult


        }

        /// <summary>
        /// All files currently loaded into the application context (Fasta, RAW, SSR, whatever)
        /// Strictly the bare filenames (no directories???)
        /// </summary>
        public ObservableCollection<string> CurrentFilesLoaded = new ObservableCollection<string>();

        /// <summary>
        /// Is the Raw spectral data file currently loaded?  
        /// </summary>
        public bool SpectralDataIsAvailable
        {
            get
            {
                return CurrentFilesLoaded.Contains(SpectralDataFilename);
            }
        }
        

        /// <summary>
        /// The Currently Loaded datafile source for the spectra (like a Thermo Raw file or Agilent .D or mzML)
        /// </summary>
        public string SpectralDataFilename
        {
            get;
            set;
        }

        /// <summary>
        /// The SHA256 Hash of the raw data file that was used as the source for the spectral data of the search results
        /// </summary>
        public string SpectralDataFileHash
        {
            get;
            set;
        }

        /// <summary>
        /// The Currently loaded Sequence Search Result (if any)
        /// </summary>
        public string WorkspaceFilename
        {
            get;
            set;
        }

        /// <summary>
        /// The Currently loaded Sequence Search Result Directory
        /// </summary>
        public string WorkspaceDirectory
        {
            get;
            set;
        }

        public double RetentionTime
        {
            get;
            set;
        }

        /// <summary>
        /// The filetype of the Spectral Data file
        /// </summary>
        public Science.Blast.clsFileType.MSFileType CurrentFileType
        {
            get;
            set;
        }

        /// <summary>
        /// Use a local FASTA file as the protein search database
        /// </summary>
        public bool UseFasta { get; set; }

        public DeChargerViewModel()
        {
            SearchResults = new ObservableCollection<SearchResult>();

            ProteinView = CollectionViewSource.GetDefaultView(SearchResults);
            ProteinView.Filter = p => (p as SearchResult).IsAutoScanIdentified;
            ProteinView.SortDescriptions.Add(new SortDescription("IsConfirmed", ListSortDirection.Ascending));
            ProteinView.SortDescriptions.Add(new SortDescription("YellowandGreenTagHits", ListSortDirection.Descending));

            DenovoView = (CollectionView)new CollectionViewSource { Source = SearchResults }.View;
            DenovoView.Filter = p => (p as SearchResult).IsAutoScanIdentified == false;

            ConfirmedSequencesView = (CollectionView)new CollectionViewSource { Source = SearchResults }.View;
            ConfirmedSequencesView.Filter = p => (p as SearchResult).Curations.Any(x => x.IsValid);            
        }

    }
}


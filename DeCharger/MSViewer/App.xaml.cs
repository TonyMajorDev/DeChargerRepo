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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using MSViewer.Controls;
using MSViewer.Classes;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
//using CommonClasses;


namespace MSViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        public static System.Diagnostics.EventLog Log;


        private void Application_DispatcherUnhandledException(object sender,
                               System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

        }

        // [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public App()
        {
            if (EventLog.SourceExists("Application"))
                Log = new EventLog("Application", Environment.MachineName, "Application");

            //NBug.Settings.HandleProcessCorruptedStateExceptions = true;
            //AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            Application.Current.DispatcherUnhandledException += Dispatcher_UnhandledException;
        }

        public static string Laststacktrace
        {
            get;
            set;
        }

        public static string CurrentScanActivationType
        {
            get;
            set;
        }


        /// <summary>
        /// Property which gets the current visibility of BYIons based on the Current Activation Type
        /// </summary>
        public static Visibility DisplayBYIons
        {
            get
            {
                if (CurrentScanActivationType != null)
                {
                    if (CurrentScanActivationType.ToUpper().Contains("HCD")) ///If the activation type is HCD then should check if there is any other activation type
                    {
                        if (CurrentScanActivationType.ToUpper().Contains(",")) ///If it has another activation type then should
                        {
                            return Visibility.Visible;
                        }
                        return Visibility.Collapsed; /// If the activation type is only HCD then should not show BYIons
                    }
                    return Visibility.Visible; ///If the activation type is not HCD should display BYIons
                }
                return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Property which gets the current visibility of CZIons based on the Current Activation Type
        /// </summary>
        public static Visibility DisplayCZIons
        {
            get
            {
                if (CurrentScanActivationType != null)
                {
                    if (CurrentScanActivationType.ToUpper().Contains("HCD")) ///If the activation type is HCD then should check if there is any other activation type
                    {
                        if (CurrentScanActivationType.ToUpper().Contains(",")) ///If it has another activation type then should
                        {
                            return Visibility.Collapsed;
                        }
                        return Visibility.Visible; /// If the activation type is only HCD then should not show BYIons
                    }
                    return Visibility.Collapsed; ///If the activation type is not HCD should display BYIons
                }
                return Visibility.Collapsed;
            }
        }

        public static bool EnabledCZIons
        {
            get
            {
                if (CurrentScanActivationType != null)
                {
                    if (CurrentScanActivationType.ToUpper().Contains("HCD")) ///If the activation type is HCD then should check if there is any other activation type
                    {
                        if (CurrentScanActivationType.ToUpper().Contains(",")) ///If it has another activation type then should
                        {
                            return false;
                        }
                        return true; /// If the activation type is only HCD then should not show BYIons
                    }
                    return false; ///If the activation type is not HCD should display BYIons
                }
                return false;
            }
        }

        public static string AssemblyLocation
        {
            get;
            set;
        }

        public static string ScanNumber
        {
            get;
            set;
        }

        public static SearchSummary SequenceSearchListSSRXML
        {
            get;
            set;
        }

        public static double SSRXMLMatchTolerancePPM
        {
            get;
            set;
        }

        public static double SSRXMLMassTolerancePPM
        {
            get;
            set;
        }


        public static bool SaveorNot
        {
            get;
            set;
        }

        public static bool LoadSpecies
        {
            get;
            set;
        }

        public static List<Species> DistinctSpecies
        {
            get
            {
                return distinctspecies;
            }
            set
            {
                distinctspecies = value;

            }
        }

        public static clsParentInfo ParentDetails
        {
            get;
            set;
        }

        public static string ActivationType
        {
            get;
            set;
        }


        public static string AppVersion
        {
            get;
            set;
        }

        public bool PPmerrorplot
        {
            get;
            set;
        }

        public static bool PPMErrorPlot
        {
            get;
            set;
        }

        public static List<ModificationList> AllValidationModifications
        {
            get;
            set;
        }

        //public static List<ModificationsListAminoAcid> AllValidationModificationAminoAcids
        //{
        //    get;
        //    set;
        //}

        //public static Dictionary<ModificationList, List<string>> AllValidationModificationAminoAcids
        //{
        //    get;
        //    set;
        //}

        //public enum MSFileType
        //{
        //    Agilent,
        //    Raw
        //}

        private static List<Species> distinctspecies;

        private static List<FASTA> lstfasta;

        public static List<FASTA> lstFasta
        {
            get
            {
                return lstfasta;
            }
            set
            {
                lstfasta = value;
            }
        }


        public static ModificationList EditMod
        {
            get;
            set;
        }


        public static string FASTAName
        {
            get;
            set;
        }

        public static StringBuilder FirstTwentyFasta
        {
            get;
            set;
        }

        public static string email
        {
            get;
            set;
        }

        public static string timeduration
        {
            get;
            set;
        }

        public static StringDictionary strDictionary
        {
            get;
            set;
        }

        public static List<Science.Chemistry.Modifications> uniMods
        {
            get;
            set;
        }


        void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Window er = new Window();
            er.Height = 350;
            er.Width = 550;
            er.ResizeMode = ResizeMode.NoResize;
            er.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            er.Title = "Fatal Error: Please help us improve DeCharger";
            er.Activate();
            //er.Owner = MainWindow;

            //TODO: log this error...

            //er.Content = new ErrorReport("Error Message :" + e.Exception.Message.ToString(), App.Laststacktrace);
            er.Show();
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Exception ex = e.ExceptionObject as Exception;

                try
                {
                    if (App.Log != null) App.Log.WriteEntry("Fatal DeCharger Exception: " + ex.Message + "\n\n" + ex.StackTrace + "\n-----------------\n" + (ex.InnerException != null ? (ex.InnerException.Message + "\n\n" + ex.InnerException.StackTrace) : ""), EventLogEntryType.Error);
                }
                catch { }
                

                MessageBox.Show(ex.Message, "Uncaught Thread Exception",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        public static bool IsBeta
        {
            get
            {
                return Assembly.GetEntryAssembly().FullName.ToUpper().Contains("BETA");
            }
        }

        public static bool IsAlpha
        {
            get
            {
                return Assembly.GetEntryAssembly().FullName.ToUpper().Contains("ALPHA");
            }
        }



        public static string XMLFileName
        {
            get;
            set;
        }

        public static string FileName
        {
            get;
            set;
        }

        public static Science.Blast.clsFileType.MSFileType CurrentFileType
        {
            get;
            set;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Args = e.Args;
        }

        public static string[] Args
        {
            get;
            set;
        }

    }
}

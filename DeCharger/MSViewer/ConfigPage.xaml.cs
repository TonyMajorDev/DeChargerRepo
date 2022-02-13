//
// Copyright (c) Andy Wright Ltd 2022, All rights reserved
// Please see the license file accompanying this software for acceptable use
//
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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Data.SqlClient;
using MSViewer.Classes;
using System.Windows.Data;
using System.IO;
using System.Xml.Linq;
using MassSpectrometry;

namespace MSViewer
{
    public partial class ConfigPage : Window
    {
        // private static IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

        private List<Species> AllSpecies = new List<Species>();

        public ConfigPage()
        {
            InitializeComponent();

            Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;

            //MinThresholdAgilentMS1 = Properties.Settings.Default.MinAgilentThresholdMS1;
            //MinThresholdAgilentMS2 = Properties.Settings.Default.MinAgilentThresholdMS2;

            cmbInstrumentType.Items.Clear();

            //Populate Instrument List
            //TODO: Populate this from an Enum maintained in the Mass Spectrometry Library ...  Or maybe leave it like this to allow for user added profiles in the future?
            foreach (var anInstrument in Properties.Settings.Default.Instruments.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                cmbInstrumentType.Items.Add(new ComboBoxItem() { Content = anInstrument });
            }

            if (cmbInstrumentType.Items.Count > 0) cmbInstrumentType.SelectedIndex = 0;  //

            cmbIsotopeTable.Items.Clear();

            ////Pick isotope cache to match molecule analyzed
            //foreach (var anIsotopeTable in Properties.Settings.Default.IsotopeTable.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    cmbIsotopeTable.Items.Add(new ComboBoxItem() { Content = anIsotopeTable });
            //}

            // ADW: Get the elenents from averagine.xml file in ProgramData\DeCharger
            AveragineCacheSettings averagineCacheSettings = AveragineCacheSettings.Instance;
            foreach (string dat in averagineCacheSettings.AveragineDataFiles)
            {
                cmbIsotopeTable.Items.Add(dat);
            }

            // ADW:  Select the entry in the combo matching the selected value of the selected value from averagine.xml
            cmbIsotopeTable.SelectedIndex = cmbIsotopeTable.Items.IndexOf(averagineCacheSettings.SelectedCacheFile);

            //switch (MainWindow.MainViewModel.CurrentFileType)
            //{
            //    case Science.Blast.clsFileType.MSFileType.Agilent:
            //        cmbInstrumentType.SelectedIndex = 0;
            //        txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]);
            //        txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]);
            //        txtMinThresholdMS1.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS1);
            //        txtMinThresholdMS2.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS2);

            //        break;
            //    case Science.Blast.clsFileType.MSFileType.Thermo:
            //        cmbInstrumentType.SelectedIndex = 1;
            //        txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]);
            //        txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]);
            //        txtMinThresholdMS1.Text = Convert.ToString(Properties.Settings.Default.MinThermoThresholdMS1);
            //        txtMinThresholdMS2.Text = Convert.ToString(Properties.Settings.Default.MinThermoThresholdMS2);

            //        break;
            //    default:
            //        break;
            //}

            if (Properties.Settings.Default.SortByDBHits)
                rdbtnSortbyDBHits.IsChecked = true;
            else
                rdbtnSortbyScore.IsChecked = true;
            //Visibility vsb = new Visibility();
            //vsb = System.Windows.Visibility.Visible;

            if (Properties.Settings.Default.PPMErrorPlot)
            {
                rdbtnErrorPlotPPM.IsChecked = true;
                Properties.Settings.Default.DaltonErrorPlot = false;
                //rdbtnErrorPlotAMU.IsChecked = false;
                App.PPMErrorPlot = true;
                App app = (App)Application.Current;
                app.PPmerrorplot = true;
            }
            else
            {
                rdbtnErrorPlotAMU.IsChecked = true;
                Properties.Settings.Default.DaltonErrorPlot = true;
                App.PPMErrorPlot = false;
                App app = (App)Application.Current;
                app.PPmerrorplot = false;
            }
            AllSpecies = App.DistinctSpecies.GroupBy(a => a.Genus).Select(a => a.First()).OrderBy(a => a.IsSelected == false).ToList();

            lstSpecies.ItemsSource = AllSpecies;
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // if the ConnectionString property changed and it's not the same as what is set in the database connection, update the db object immediately
            if (e.PropertyName == "ConnectionString" && (this.Owner as MainWindow).db.ConnectionString != Properties.Settings.Default.ConnectionString)
            {
                // this will recreate the db object
                (this.Owner as MainWindow).InitializeDatabaseConnection();

                System.Diagnostics.Debug.WriteLine("Called LoadSpecies");
            }

            //throw new NotImplementedException();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //TODO: make this more dynamic and savable!

                switch (this.cboAssumedCharge.SelectedIndex)
                {
                    case 3:
                        ChargeDetector.AssumedCharges = new int[0];
                        break;

                    case 2:
                        ChargeDetector.AssumedCharges = new int[] { 1 };
                        break;

                    case 1:
                        ChargeDetector.AssumedCharges = new int[] { 1, 2 };
                        break;

                    default:
                        ChargeDetector.AssumedCharges = new int[] { 1, 2, 3 };
                        break;
                }

                if (rdbtnSortbyDBHits.IsChecked.Value)
                {
                    Properties.Settings.Default.SortByDBHits = true;
                    Properties.Settings.Default.SortByScore = false;
                }
                else
                {
                    Properties.Settings.Default.SortByDBHits = false;
                    Properties.Settings.Default.SortByScore = true;
                }

                if (rdbtnErrorPlotPPM.IsChecked.Value)
                {
                    Properties.Settings.Default.PPMErrorPlot = true;
                    Properties.Settings.Default.DaltonErrorPlot = false;
                    Properties.Settings.Default.PPMErrorAxisMax = Properties.Settings.Default.FragementIonTolerance;
                    Properties.Settings.Default.PPMErrorAxisMin = -Properties.Settings.Default.FragementIonTolerance;
                    App app = (App)Application.Current;
                    app.PPmerrorplot = true;
                    App.PPMErrorPlot = true;
                }
                else
                {
                    Properties.Settings.Default.PPMErrorPlot = false;
                    Properties.Settings.Default.DaltonErrorPlot = true;
                    Properties.Settings.Default.PPMErrorAxisMax = 0.1;
                    Properties.Settings.Default.PPMErrorAxisMin = -0.1;
                    App app = (App)Application.Current;
                    app.PPmerrorplot = false;
                    App.PPMErrorPlot = false;
                }

                //if (MainWindow.MainViewModel.CurrentFileType == Science.Blast.clsFileType.MSFileType.Thermo)
                //{
                //    Properties.Settings.Default.MinAgilentThresholdMS1 = MinThresholdAgilentMS1;
                //    Properties.Settings.Default.MinAgilentThresholdMS2 = MinThresholdAgilentMS2;
                //}

                switch (MainWindow.MainViewModel.CurrentFileType)
                {
                    case Science.Blast.clsFileType.MSFileType.Agilent:
                        (Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]) = txtMonoMatchTolerance.Text;
                        (Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]) = txtMassTolerance.Text;
                        //(Properties.Settings.Default.MinAgilentThresholdMS1) = (txtMonoMatchTolerance_Copy.Text != "" ? Convert.ToInt32(txtMonoMatchTolerance_Copy.Text) : 0);
                        //(Properties.Settings.Default.MinAgilentThresholdMS2) = (txtMonoMatchTolerance_Copy1.Text != "" ? Convert.ToInt32(txtMonoMatchTolerance_Copy1.Text) : 0);
                        cmbInstrumentType.SelectedIndex = 0;
                        break;

                    case Science.Blast.clsFileType.MSFileType.Thermo:
                        (Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]) = txtMonoMatchTolerance.Text;
                        (Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]) = txtMassTolerance.Text;
                        cmbInstrumentType.SelectedIndex = 1;
                        break;

                    default:
                        break;
                }

                Properties.Settings.Default.MatchTolerancePPM = Convert.ToDouble(txtMonoMatchTolerance.Text);
                Properties.Settings.Default.MassTolerancePPM = Convert.ToDouble(txtMassTolerance.Text);

                Properties.Settings.Default.Save();
                App.SaveorNot = true;

                if (!chkSearchAllSpecies.IsChecked.Value)
                {
                    savespecies();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.Source);
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void savespecies()
        {
            Properties.Settings.Default.Genus = "";
            var selectedspecies = AllSpecies.Where(a => a.IsSelected).Select(a => a.Genus).ToList();
            for (int i = 0; i < App.DistinctSpecies.Count; i++)
            {
                if (selectedspecies.Where(a => a == App.DistinctSpecies[i].Genus).Any())
                {
                    App.DistinctSpecies[i].IsSelected = true;
                }
                else
                {
                    App.DistinctSpecies[i].IsSelected = false;
                }
            }
            foreach (Species dsp in App.DistinctSpecies.Where(a => a.IsSelected == true).ToList())
            {
                Properties.Settings.Default.Genus += "\t" + dsp.Genus;
            }
            Properties.Settings.Default.Save();
        }

        private void chkSearchAllSpecies_CheckChange(object sender, RoutedEventArgs e)
        {
            lstSpecies.IsEnabled = !chkSearchAllSpecies.IsChecked.Value;
            if (!chkSearchAllSpecies.IsChecked.Value)
            {
            }
        }

        private void cmbInstrumentType_Selected(object sender, SelectionChangedEventArgs e)
        {
            Binding aBinding = null;

            switch (Convert.ToString(((System.Windows.Controls.ContentControl)(cmbInstrumentType.SelectedValue)).Content))
            {
                case "Thermo Orbi":  // User changed the dropdown combo to Thermo Instrument

                    // Change databindings to save to Thermo settings
                    aBinding = new Binding("MinThermoThresholdMS1");
                    aBinding.Source = Properties.Settings.Default;
                    aBinding.Mode = BindingMode.TwoWay;
                    BindingOperations.SetBinding(txtMinThresholdMS1, TextBox.TextProperty, aBinding);

                    aBinding = new Binding("MinThermoThresholdMS2");
                    aBinding.Source = Properties.Settings.Default;
                    aBinding.Mode = BindingMode.TwoWay;
                    BindingOperations.SetBinding(txtMinThresholdMS2, TextBox.TextProperty, aBinding);

                    txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]);
                    txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]);
                    Properties.Settings.Default.MatchTolerancePPM = Convert.ToDouble(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]);
                    Properties.Settings.Default.MassTolerancePPM = Convert.ToDouble(Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]);

                    break;

                case "Agilent QTOF":  // User changed the dropdown combo to Agilent Instrument

                    // Change databindings to save to Agilent settings
                    aBinding = new Binding("MinAgilentThresholdMS1");
                    aBinding.Source = Properties.Settings.Default;
                    aBinding.Mode = BindingMode.TwoWay;
                    BindingOperations.SetBinding(txtMinThresholdMS1, TextBox.TextProperty, aBinding);

                    aBinding = new Binding("MinAgilentThresholdMS2");
                    aBinding.Source = Properties.Settings.Default;
                    aBinding.Mode = BindingMode.TwoWay;
                    BindingOperations.SetBinding(txtMinThresholdMS2, TextBox.TextProperty, aBinding);

                    txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]);
                    txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]);
                    Properties.Settings.Default.MatchTolerancePPM = Convert.ToDouble(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]);
                    Properties.Settings.Default.MassTolerancePPM = Convert.ToDouble(Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]);

                    //MainWindow.MainViewModel.CurrentFileType = Science.Blast.clsFileType.MSFileType.Agilent;
                    break;
                    //default:
                    //    switch (MainWindow.MainViewModel.CurrentFileType)
                    //    {
                    //        case Science.Blast.clsFileType.MSFileType.Agilent:
                    //            txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]);
                    //            txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]);
                    //            Properties.Settings.Default.MatchTolerancePPM = Convert.ToDouble(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]);
                    //            Properties.Settings.Default.MassTolerancePPM = Convert.ToDouble(Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]);
                    //            txtMinThresholdMS1.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS1);
                    //            txtMinThresholdMS2.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS2);
                    //            break;
                    //        case Science.Blast.clsFileType.MSFileType.Thermo:
                    //            txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]);
                    //            txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]);
                    //            Properties.Settings.Default.MatchTolerancePPM = Convert.ToDouble(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]);
                    //            Properties.Settings.Default.MassTolerancePPM = Convert.ToDouble(Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]);
                    //            txtMinThresholdMS1.Text = "0";
                    //            txtMinThresholdMS2.Text = "0";
                    //            MinThresholdAgilentMS1 = Properties.Settings.Default.MinAgilentThresholdMS1;
                    //            MinThresholdAgilentMS2 = Properties.Settings.Default.MinAgilentThresholdMS2;
                    //            break;
                    //    }
                    //    break;
            }
        }

        private void cmbIsotopeTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AveragineCacheSettings averagineCacheSettings = AveragineCacheSettings.Instance;
            averagineCacheSettings.SelectedCacheFile = cmbIsotopeTable.SelectedItem.ToString();
            averagineCacheSettings.Save();
        }

        //private void cmbInstrumentType_Selected(object sender, SelectionChangedEventArgs e)
        //{
        //}
    }
}
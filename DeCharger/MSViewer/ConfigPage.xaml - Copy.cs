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

namespace MSViewer
{
    public partial class ConfigPage : Window
    {
        // private static IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

        List<Species> AllSpecies = new List<Species>();

        int MinThresholdAgilentMS1 = 0;
        int MinThresholdAgilentMS2 = 0;

        public ConfigPage()
        {
            InitializeComponent();

            MinThresholdAgilentMS1 = Properties.Settings.Default.MinAgilentThresholdMS1;
            MinThresholdAgilentMS2 = Properties.Settings.Default.MinAgilentThresholdMS2;

            switch (App.CurrentFileType)
            {
                case Science.Blast.clsFileType.MSFileType.Agilent:
                    txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]);
                    txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]);
                    txtMonoMatchTolerance_Copy.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS1);
                    txtMonoMatchTolerance_Copy1.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS2);
                    cmbInstrumentType.SelectedIndex = 0;
                    break;
                case Science.Blast.clsFileType.MSFileType.Thermo:
                    txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]);
                    txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]);
                    txtMonoMatchTolerance_Copy.Text = "0";
                    txtMonoMatchTolerance_Copy1.Text = "0";
                    cmbInstrumentType.SelectedIndex = 1;
                    break;
                default:
                    break;
            }

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

        void savespecies()
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

        private void cmbInstrumentType_Selected(object sender, RoutedEventArgs e)
        {
            switch (Convert.ToString(((System.Windows.Controls.ContentControl)(cmbInstrumentType.SelectedValue)).Content))
            {
                case "Thermo Orbi":
                    App.CurrentFileType = Science.Blast.clsFileType.MSFileType.Thermo;
                    txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]);
                    txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]);
                    txtMonoMatchTolerance_Copy.Text = "0";
                    txtMonoMatchTolerance_Copy1.Text = "0";
                    MinThresholdAgilentMS1 = Properties.Settings.Default.MinAgilentThresholdMS1;
                    MinThresholdAgilentMS2 = Properties.Settings.Default.MinAgilentThresholdMS2;
                    break;
                case "Agilent QTOF":
                    txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]);
                    txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]);
                    txtMonoMatchTolerance_Copy.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS1);
                    txtMonoMatchTolerance_Copy1.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS2);
                    App.CurrentFileType = Science.Blast.clsFileType.MSFileType.Agilent;
                    break;
                default:
                    switch (App.CurrentFileType)
                    {
                        case Science.Blast.clsFileType.MSFileType.Agilent:
                            txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]);
                            txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]);
                            txtMonoMatchTolerance_Copy.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS1);
                            txtMonoMatchTolerance_Copy1.Text = Convert.ToString(Properties.Settings.Default.MinAgilentThresholdMS2);
                            break;
                        case Science.Blast.clsFileType.MSFileType.Thermo:
                            txtMonoMatchTolerance.Text = Convert.ToString(Properties.Settings.Default.MatchTolerancePPMBasedonFileType[1]);
                            txtMassTolerance.Text = Convert.ToString(Properties.Settings.Default.MassTolerancePPMBasedonFileType[1]);
                            txtMonoMatchTolerance_Copy.Text = "0";
                            txtMonoMatchTolerance_Copy1.Text = "0";
                            MinThresholdAgilentMS1 = Properties.Settings.Default.MinAgilentThresholdMS1;
                            MinThresholdAgilentMS2 = Properties.Settings.Default.MinAgilentThresholdMS2;
                            break;
                    }
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {

                //TODO: make this more dynamic and savable!
                MainWindow parentwindow = new MainWindow();

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
                    Properties.Settings.Default.PPMErrorAxisMax = 20;
                    Properties.Settings.Default.PPMErrorAxisMin = -20;
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


                if (App.CurrentFileType == Science.Blast.clsFileType.MSFileType.Thermo)
                {
                    Properties.Settings.Default.MinAgilentThresholdMS1 = MinThresholdAgilentMS1;
                    Properties.Settings.Default.MinAgilentThresholdMS2 = MinThresholdAgilentMS2;
                }

                switch (App.CurrentFileType)
                {
                    case Science.Blast.clsFileType.MSFileType.Agilent:
                        (Properties.Settings.Default.MatchTolerancePPMBasedonFileType[0]) = txtMonoMatchTolerance.Text;
                        (Properties.Settings.Default.MassTolerancePPMBasedonFileType[0]) = txtMassTolerance.Text;
                        (Properties.Settings.Default.MinAgilentThresholdMS1) = (txtMonoMatchTolerance_Copy.Text != "" ? Convert.ToInt32(txtMonoMatchTolerance_Copy.Text) : 0);
                        (Properties.Settings.Default.MinAgilentThresholdMS2) = (txtMonoMatchTolerance_Copy1.Text != "" ? Convert.ToInt32(txtMonoMatchTolerance_Copy1.Text) : 0);
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
        }
    }
}


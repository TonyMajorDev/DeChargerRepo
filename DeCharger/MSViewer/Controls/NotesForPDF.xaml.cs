using Microsoft.Win32;
using MSViewer.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSViewer.Controls
{
    /// <summary>
    /// Interaction logic for NotesForPDF.xaml
    /// </summary>
    public partial class NotesForPDF : UserControl
    {
        public NotesForPDF()
        {
            InitializeComponent();
        }

        ObservableCollection<SearchResult> pubresults = new ObservableCollection<SearchResult>();
        string tmduration = string.Empty;

        public NotesForPDF(ObservableCollection<SearchResult> publicresults, string timeduration)
        {
            InitializeComponent();
            pubresults = publicresults;
            tmduration = timeduration;
        }

        private void btnSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog Savelocation = new SaveFileDialog();
            Savelocation.DefaultExt = ".pdf";
            Savelocation.Filter = "PDF (.pdf)|*.pdf";
            bool? result = Savelocation.ShowDialog();
            if (result == true)
            {
                string filename = "\"" + Savelocation.FileName;    ///string.Join("_", Savelocation.FileName.Split(' '));
                GenerateHTML.SaveaPDF(pubresults, filename, tmduration, txtNotes.Text); // Savelocation.FileName); Convert.ToString(DateTime.Now.Subtract(genTime).TotalMinutes) + " Seconds: " + Convert.ToString(DateTime.Now.Subtract(genTime).Seconds)
            }

            Window window = Window.GetWindow(this);
            window.Close();
        }


    }
}

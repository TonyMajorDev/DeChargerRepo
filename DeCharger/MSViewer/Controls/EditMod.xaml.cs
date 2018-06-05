using MSViewer.Classes;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for EditMod.xaml
    /// </summary>
    public partial class EditMod : UserControl
    {
        public EditMod()
        {
            InitializeComponent();
        }

        ModificationList md = new ModificationList();

        public EditMod(string ModName, string ModAbbrv, string ModMass)
        {
            InitializeComponent();
            txtmdName.Text = md.Name = ModName;
            txtmdAbbrv.Text = md.Abbreviation = ModAbbrv;
            txtmdMass.Text = md.Mass = ModMass;
            App.EditMod = md;
        }

        private void btnedmdOK_Click(object sender, RoutedEventArgs e)
        {
            md.Name = txtmdName.Text;
            md.Abbreviation = txtmdAbbrv.Text;
            md.Mass = txtmdMass.Text;
            App.EditMod = md;
            CloseWindow();
        }

        void CloseWindow()
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }
    }
}

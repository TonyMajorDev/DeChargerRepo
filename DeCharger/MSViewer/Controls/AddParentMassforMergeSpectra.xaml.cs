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
    /// Interaction logic for AddParentMass.xaml
    /// </summary>
    public partial class AddParentMassforMergeSpectra : UserControl
    {
        public AddParentMassforMergeSpectra()
        {
            InitializeComponent();
        }

        public class MergeParentMass
        {
            public static double ParentMass
            {
                get;
                set;
            }

            public static int ParentZ
            {
                get;
                set;
            }

            public static bool SetorNot
            {
                get;
                set;
            }
        }

        private void btnmrgParentMsOK_Click(object sender, RoutedEventArgs e)
        {
            MergeParentMass.ParentMass = txtbxmrgParentMass.Text != "" ? Convert.ToDouble(txtbxmrgParentMass.Text) : 0;
            MergeParentMass.ParentZ = txtbxmrgParentZ.Text != "" ? Convert.ToInt32(txtbxmrgParentZ.Text) : 0;
            MergeParentMass.SetorNot = true;

            Window window = Window.GetWindow(this);
            window.Close();
        }
    }
}

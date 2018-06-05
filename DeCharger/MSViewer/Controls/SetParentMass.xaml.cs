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
using MassSpectrometry;
using System.Text.RegularExpressions;

namespace MSViewer.Controls
{
    /// <summary>
    /// Interaction logic for SetParentMass.xaml
    /// </summary>
    public partial class SetParentMass : UserControl
    {
        public SetParentMass()
        {
            InitializeComponent();
            txtMZ.Focus();
        }


        public SetParentMass(string sqstg)
        {
            InitializeComponent();
            txtMZ.Focus();
            SetParentM.SequenceTag = sqstg;
        }


        public SetParentMass(int ParentZ, double ParentMass, double ParentMZ)
        {
            InitializeComponent();
            txtCharge.Text = Convert.ToString(ParentZ);
            txtMass.Text = Convert.ToString(ParentMass);
            txtMZ.Text = Convert.ToString(ParentMZ);
            txtMZ.Focus();
        }

        clsParentInfo currentparent = new clsParentInfo();

        public NonStaticParentMass CurrentParent = new NonStaticParentMass();

        bool changemass = false;
        bool changeMz = false;

        public class SetParentM
        {
            public static bool SaveParentOrNot { get; set; }

            public static string SequenceTag {get; set;}
        }

        public class NonStaticParentMass
        {
            public double ParentMZ
            {
                get;
                set;
            }

            public int Charge
            {
                get;
                set;
            }

            public double Mass
            {
                get;
                set;
            }
        }

        public class ParentMass
        {
            public static double ParentMZ
            {
                get;
                set;
            }

            public static int Charge
            {
                get;
                set;
            }

            public static double Mass
            {
                get;
                set;
            }
        }


        bool fromokay = false;
        private void btnokay_Click(object sender, RoutedEventArgs e)
        {
            SetParentM.SaveParentOrNot = true;
            ParentMass.Charge = CurrentParent.Charge;
            ParentMass.Mass = CurrentParent.Mass;
            ParentMass.ParentMZ = CurrentParent.ParentMZ;
            fromokay = true;
            CloseWindow();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            SetParentM.SaveParentOrNot = false;
            fromokay = false;
            CloseWindow();
        }

        void CloseWindow()
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }

        private void txtMZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetValues();
            changemass = true;
            changeMz = false;
            frommz = true;
            SetMass();
        }

        private void txtCharge_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetValues();
            changemass = true;
            changeMz = false;
            frommz = true;
            SetMass();
        }

        void SetMass()
        {
            if (txtMass != null && changemass && !setonlyparent)
            {
                CurrentParent.Mass = MassSpecExtensions.ToMass(CurrentParent.ParentMZ, CurrentParent.Charge);
                txtMass.Text = Convert.ToString(Math.Round(CurrentParent.Mass, 4));
                changemass = false;
            }
        }

        bool frommass = false;
        bool frommz = false;

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]-+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtMass_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetValues();
            changeMz = true;
            changemass = false;
            frommass = true;
            SetMZ();
            setonlyparent = false;
        }

        bool setonlyparent = false;

        void SetMZ()
        {
            if (txtMZ != null && changeMz)
            {
                setonlyparent = true;
                CurrentParent.ParentMZ = MassSpecExtensions.ToMZ(CurrentParent.Mass, CurrentParent.Charge);
                txtMZ.Text = Convert.ToString(Math.Round(CurrentParent.ParentMZ, 4));
            }
        }

        public void SetValues()
        {
            if (txtMZ != null)
            {
                if (txtMZ.Text != "")
                {
                    CurrentParent.ParentMZ = Convert.ToDouble(txtMZ.Text);
                }
            }
            if (txtCharge != null)
            {
                if (txtCharge.Text != "")
                {
                    CurrentParent.Charge = Convert.ToInt32(txtCharge.Text);
                }
            }
            if (txtMass != null)
            {
                if (txtMass.Text != "")
                {
                    CurrentParent.Mass = Convert.ToDouble(txtMass.Text);
                }
            }
        }

        private void txtMass_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9.]$");
            return reg.IsMatch(str);
        }

    }
}

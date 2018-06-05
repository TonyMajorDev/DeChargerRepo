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
    /// Interaction logic for AgilentAgreement.xaml
    /// </summary>
    public partial class AgilentAgreement : UserControl
    {
        public AgilentAgreement()
        {
            InitializeComponent();
        }

        public AgilentAgreement(string Agreement)
        {
            InitializeComponent();
            btnAgilentAgreementNo.Focus();
            txtAgilentAgreement.Text = Agreement;
            //txtAgilentAgreement.VerticalOffset
        }

        private void btnAgilentAgreementNo_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AgilentAgreed = false;
            CloseWindow();
        }

        void CloseWindow()
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }

        private void btnAgilentAgreementYes_Click(object sender, RoutedEventArgs e)
        {
            //if (IsScrolledToEnd(txtAgilentAgreement))
            //{
                Properties.Settings.Default.AgilentAgreed = true;
                CloseWindow();
            //}
            //else
            //{
            //    MessageBox.Show(Window.GetWindow(this), "Please read the entire agreement.  ", "Read entire agreement, please.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //}
        }

        //public static bool IsScrolledToEnd(TextBox textBox)
        //{
        //    return textBox.VerticalOffset + textBox.ViewportHeight == textBox.ExtentHeight;
        //}
    }


}

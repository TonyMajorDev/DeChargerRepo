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
    /// Interaction logic for TextBoxSubmit.xaml
    /// </summary>
    public partial class TextBoxSubmit : UserControl
    {
        public TextBoxSubmit()
        {
            InitializeComponent();
        }

        public TextBoxSubmit(string Message)
        {
            InitializeComponent();
            lblMessage.Content = Message;
            returnval = ReturnValue.returnvalue;
            txtTextbox.Focus();
        }

        public ReturnValue ReturnValue;

        public static string returnval { get; set; }


        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            if (txtTextbox.Text == "")
            {
                MessageBox.Show("Nothing was specified.  Please enter a valid value, or cancel.  ", "Invalid Entry", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
                
            ReturnValue.returnvalue = returnval = txtTextbox.Text;
            CloseWindow();
        }

        //private void txtCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    CloseWindow();
        //}

        void CloseWindow()
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }
    }

    public class ReturnValue
    {
        public static string returnvalue { get; set; }
    }
}

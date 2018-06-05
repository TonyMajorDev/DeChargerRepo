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
    /// Interaction logic for Prompt.xaml
    /// </summary>
    public partial class Prompt : UserControl
    {
        public Prompt()
        {
            InitializeComponent();
        }

        public Prompt(BooleanValues content, string messagecontent)
        {
            InitializeComponent();

            switch (content)
            {
                case BooleanValues.YesorNo:
                    btnFirst.Content = "Yes";
                    btnSecond.Content = "No";
                    break;
                case BooleanValues.TrueorFalse:
                    btnFirst.Content = "True";
                    btnSecond.Content = "False";
                    break;
                case BooleanValues.OKorCancel:
                    btnFirst.Content = "OK";
                    btnSecond.Content = "Cancel";
                    break;
                default:
                    break;
            }


            lblMessage.Content = messagecontent;
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            TrueorFalse.trueorfalse = true;
            CloseWindow();
        }

        private void btnSecond_Click(object sender, RoutedEventArgs e)
        {
            TrueorFalse.trueorfalse = false;
            CloseWindow();
        }


        void CloseWindow()
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }
    }

    public enum BooleanValues
    {
        YesorNo, TrueorFalse, OKorCancel
    }

    public class TrueorFalse
    {
        public static bool trueorfalse { get; set; }
    }

}

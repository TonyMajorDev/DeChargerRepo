using Science.Chemistry;
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
using System.Windows.Shapes;

namespace MSViewer.Controls
{
    /// <summary>
    /// Interaction logic for AllUnimodModifications.xaml
    /// </summary>
    public partial class AllUnimodModifications : Window
    {
        public AllUnimodModifications()
        {
            InitializeComponent();
        }

        List<Modifications> allmods = new List<Modifications>();
        string description = string.Empty;
        string interimname = string.Empty;
        string monomass = string.Empty;

        bool btndoubleclk = false;

        public AllUnimodModifications(List<Modifications> mods)
        {
            InitializeComponent();
            dtgridunimods.ItemsSource = mods.OrderBy(a => Math.Abs(a.Monomass));
            allmods = mods;
        }

        private Action<Modifications> callback;
        //private List<Science.Chemistry.Modifications> list;

        /// <summary>
        /// Window with modification list and action as parameters
        /// </summary>
        /// <param name="mods">List of Modifications</param>
        /// <param name="action">Action to pass</param>
        public AllUnimodModifications(List<Modifications> mods, Action<Modifications> action)
        {
            InitializeComponent();
            Closed += AllUnimodModifications_Closed;
            btndoubleclk = false;
            callback = action;
            dtgridunimods.ItemsSource = mods.OrderBy(a => (a.Monomass));
            allmods = mods;
        }

        void AllUnimodModifications_Closed(object sender, EventArgs e)
        {
            if (dtgridunimods.SelectedItem != null && dtgridunimods.SelectedItem is Modifications && btndoubleclk)
            {
                var selectedmod = (dtgridunimods.SelectedItem as Modifications);
                Modifications mods = new Modifications();
                mods = selectedmod;
                callback(mods);
                btndoubleclk = false;
                Close();
            }
        }


        private void txtInterimName_TextChanged(object sender, TextChangedEventArgs e)
        {
            interimname = ((System.Windows.Controls.TextBox)(sender)).Text;
            filtermods();
        }

        private void txtDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            description = ((System.Windows.Controls.TextBox)(sender)).Text;
            filtermods();
        }

        private void txtMonomass_TextChanged(object sender, TextChangedEventArgs e)
        {
            monomass = ((System.Windows.Controls.TextBox)(sender)).Text;
            filtermods();
        }

        /// <summary>
        /// Filter mods based on the description, interimname or monomass
        /// </summary>
        private void filtermods()
        {
            var searchboundsequences = (from mds in allmods
                                        let IsNameFilterOff = string.IsNullOrWhiteSpace(interimname)
                                        let IsDescFilterOff = string.IsNullOrWhiteSpace(description)
                                        let IsMassFilterOff = string.IsNullOrWhiteSpace(monomass)
                                        where (IsNameFilterOff || ApplyFilter(mds.InterimName, interimname))
                                        && (IsDescFilterOff || ApplyFilter(mds.Description, description))
                                        && (IsMassFilterOff || ApplyFilterfornumbers(Convert.ToString(mds.Monomass), monomass))
                                        select mds);
            Dispatcher.Invoke((Action)(() =>
                {
                    dtgridunimods.ItemsSource = searchboundsequences.OrderBy(a => a.Monomass).ToList();
                }));
        }

        /// <summary>
        /// Check if the target is contained in the source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool ApplyFilter(string source, string target)
        {
            return source.ToLower().Contains(target.ToLower());
        }

        /// <summary>
        /// Checking if the number starts with a particular number.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool ApplyFilterfornumbers(string source, string target)
        {
            return source.StartsWith(target) || source.StartsWith("-" + target);
        }


        /// <summary>
        /// On a double click adding the modification to the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtgridunimods_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btndoubleclk = true;
            AllUnimodModifications_Closed(null, null);
        }


        /// <summary>
        /// When the add modification button is pressed it needs to add the modification to the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddModification_Click(object sender, RoutedEventArgs e)
        {
            if (dtgridunimods.SelectedItems.Count > 0)
            {
                callback(dtgridunimods.SelectedItem as Modifications);
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class ShareConfiguration : INotifyPropertyChanged
    {
        #region Name

        private string name = String.Empty;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        #endregion

        #region NetworkLocation

        private string netLocation = String.Empty;


        public string NetworkLocation
        {
            get { return netLocation; }
            set
            {
                netLocation = value;
                OnPropertyChanged("NetworkLocation");
            }
        }

        #endregion

        #region INotifyPropertyChanged event

        ///<summary>
        ///Occurs when a property value changes.
        ///</summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for
        /// a given property.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}

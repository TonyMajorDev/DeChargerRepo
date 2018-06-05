using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class ModificationList
    {
        public string Name
        {
            get;
            set;
        }

        public string Mass
        {
            get;
            set;
        }

        public string Abbreviation
        {
            get;
            set;
        }

        public List<string> AminoAcids
        {
            get;
            set;
        }
    }
}

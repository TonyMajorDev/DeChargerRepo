using SignalProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public static class SaveCurrentScanInfo
    {
        public static List<FindSequenceTags.SequenceTag> currentTags
        {
            get;
            set;
        }

        public static List<Cluster> ions
        {
            get;
            set;
        }

        public static double? ParentMass
        {
            get;
            set;
        }
    }
}

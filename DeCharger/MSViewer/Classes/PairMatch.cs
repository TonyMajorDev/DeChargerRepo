using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class PairMatch
    {
        public string PairMtch { get; set; }
        public bool Forward { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Index { get; set; }

        public override string ToString()
        {
            return (Forward ? "> " : "< ") + Start + " - " + End + ", " + PairMtch;
        }
    }
}

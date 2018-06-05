using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer
{
    public class DeltaMassforSequenceTagMatching
    {
        public static double DeltaMass(double numberofaminoacids = 0)
        {
            return Math.Min(70, 10 * numberofaminoacids);
        }
    }
}

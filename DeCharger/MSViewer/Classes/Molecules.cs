using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class Molecules
    {
        public const double Water = 18.01002; //Using the Monoisotopic mass instead of the average mass 18.010565;
        public const double tolerance = 0.2;
        public const double Ammonia = 17.02655;
        public const double Electron = 0.00054858;
        public const double Terminuses = Water - Electron;
        public const double Validationtolerance = 0.1;
        public const double Nitrogen = 14.003074; //Monoisotopic mass of Nitrogen.
        public const double Hydrogen = 1.007825035; //Monoisotopic mass of Hydrogen.
        //public const double Watertolerance = 0.2;
    }
}

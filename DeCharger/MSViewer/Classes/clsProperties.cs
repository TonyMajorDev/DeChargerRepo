using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class clsProperties
    {
        public int PPM
        {
            get;
            set;
        }

        public int Tolerance
        {
            get;
            set;
        }

        public int MinSequenceTagLength
        {
            get;
            set;
        }

        public Databases databases
        {
            get;
            set;
        }

        public enum Databases
        {
            FASTA,
            SQLDatabase
        }

        public int MinPeakIntensity
        {
            get;
            set;
        }

        public ExactOrBlastp exactorblastp
        {
            get;
            set;
        }

        public enum ExactOrBlastp
        {
            Blastp,
            Exact
        }

        public ThoroughOrRapid thoroughorrapid
        {
            get;
            set;
        }

        public enum ThoroughOrRapid
        {
            Thorough,
            Rapid
        }

        public string VersionofProgram
        {
            get;
            set;
        }

        public StringBuilder ContentsofFasta
        {
            get;
            set;
        }

    }
}

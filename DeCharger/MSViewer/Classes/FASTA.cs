using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Science.Proteomics;

namespace MSViewer.Classes
{
    public class FASTA
    {
        public string Accession { get; set; }

        public string Description { get; set; }

        public string Species { get; set; }

        public double Mass { get; set; }

        string sequence;

        public string Sequence
        {
            get
            {
                return sequence;
            }
            set
            {
                sequence = value;
            }
        }


        public string SequenceID { get; set; }

        string _isoSequence = null;
        /// <summary>
        /// This is the Isobaric Sequence where all the Isoleucines are replaced with Leucine
        /// </summary>
        public string ModifiedSequenceforL
        {
            get
            {
                if (Sequence == null) return string.Empty;

                _isoSequence = _isoSequence ?? Sequence.Replace('I', 'L');

                return _isoSequence;
            }
        }

        string _isoSequenceJ = null;

        /// <summary>
        /// This is the Isobaric Sequence where all the Isoleucines and Leucines are replaced with J
        /// </summary>
        public string ModifiedSequenceforJ
        {
            get
            {
                if (Sequence == null) return string.Empty;

                _isoSequenceJ = _isoSequenceJ ?? Sequence.ModifiedSequenceforJ();

                return _isoSequenceJ;
            }
        }

    }
}

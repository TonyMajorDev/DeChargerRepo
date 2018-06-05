using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonClasses
{
    public class FASTA
    {
        public string Accession { get; set; }

        public string Description { get; set; }

        public string Species { get; set; }

        public double Mass { get; set; }

        public string Sequence { get; set; }


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
    }
}

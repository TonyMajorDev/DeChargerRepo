//-----------------------------------------------------------------------
// Copyright 2018 Eli Lilly and Company
//
// Licensed under the Apache License, Version 2.0 (the "License");
//
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using Science.Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer
{
    //public class ProteinTagMatchResult
    //{
    //    public string Sequence { get; set; }
    //    public List<MatchStartEnds> TagMatches { get; set; }
    //}


    public class MatchStartEnds
    {
        public int Index { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string SequenceTag { get; set; }
        public bool MainSequence { get; set; }
        public bool ReverseSequence { get; set; }
        public AminAcidConfidence confidence { get; set; }
        public double StartMonoMass { get; set; }
        public double EndMonoMass { get; set; }
        public List<double> MonoMasses { get; set; }
        public bool ReverseSpectrum { get; set; }
        public int Numberofextensions { get; set; }
        public bool breakornot { get; set; }
        public string RawSequenceTag { get; set; }

        public int NumberofYellowPeaks { get; set; }

        public int NumberofGreenPeaks { get; set; }

        public bool CTerminus { get; set; } /// Checks if a sequence has a C-Terminus
        /// <summary>
        /// Use this to figure out which direction is the sequence.
        /// If its towards the Parent then it is Forward.
        /// If its towards the Zero peak then it is Reverse.
        /// </summary>
        public bool ForwardforHighlighting { get; set; }

        bool forwardions;

        public bool ForwardIons
        {
            get
            {
                return forwardions;
            }
            set
            {
                value = forwardions;
            }
        }

        public bool ReverseIons
        {
            get
            {
                if (forwardions != null)
                    return !forwardions;
                else
                    return false;

            }
        }

        public bool DontShowall { get; set; }

        public override string ToString()
        {
            return Start.ToString() + " - " + End.ToString() + ": " + SequenceTag;
        }

        public int Length
        {
            get
            {
                if (Start != null && End != null)
                    return Math.Abs(Start - End);
                else
                    return 0;
            }
        }
        public string forsort
        {
            get
            {
                if (SequenceTag != null && StartMonoMass != null)
                {
                    return Convert.ToString(Start) + Convert.ToString(End) + SequenceTag;
                }
                else
                    return string.Empty;
            }
        }

        public double Gap
        {
            get;
            set;
        }

        ///// <summary>
        ///// Many confidence levels can provide better way of highlighting
        ///// items in the list.
        ///// Sure is to say if there is absolute possibility of a sequence tag,
        ///// it is determined by saying that there are other sequence tags which 
        ///// proves the presence of the current sequence.
        ///// Not Sure is to say may be no, just a tiny bit of confidence
        ///// Possible to check if there is any evidence for any other mono masses
        ///// and hence say if this is good tag
        ///// Not Possible is to say no way. No confidence, saying this is worst sequence tag
        ///// for match
        ///// Gap is a gap with a modification
        ///// </summary>
        //public enum Confidence
        //{
        //    High = 10,
        //    Medium = 8,
        //    Low = 6,
        //    Sure = 12,
        //    Possible = 4,
        //    NotSure = 3,
        //    NotPossible = 1,
        //    Gap = 2,
        //    None = 0,
        //    Reallybad = -1,
        //    SearchHit = -2,
        //    BlastTagMatch = -3,
        //    BlastTagMisMatch = -4,
        //    BlastTagPossible = -5
        //};
    }
}

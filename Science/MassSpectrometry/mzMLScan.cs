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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MassSpectrometry
{
    public class mzMLScan
    {
        public mzMLScan()
        {
            PrecursorDetails = new List<IonPeak>();
        }

        public int ScanNumber { get; set; }

        public System.Xml.Linq.XElement Details { get; set; }

        public double[] mzPoints { get; set; }

        public double[] intensityPoints { get; set; }

        public float RT { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Scan Mode (also called activation ?)
        /// </summary>
        public string ScanMode { get; set; }

        public string Polarity { get; set; }

        public int MsLevel { get; set; }

        public double MzStart { get; set; }

        public double MzStop { get; set; }

        private float ionCurrent = -1;

        public float IonCurrent
        {
            get
            {
                if (ionCurrent == -1) ionCurrent = (float)this.intensityPoints.Sum();
                return ionCurrent;
            }
        }

        public List<IonPeak> PrecursorDetails { get; set; }
    }

    public class IonPeak
    {
        public double MZ { get; set; }
        public int? Z { get; set; }

        public float Intensity { get; set; }
    }
}

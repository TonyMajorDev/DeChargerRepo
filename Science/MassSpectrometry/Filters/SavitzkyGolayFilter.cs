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
using System.Text;
using System.ComponentModel;

namespace SignalProcessing.Filters
{
    public class SavitzkyGolayFilter : IFilter
    {
        private static Dictionary<int, int> Hvalues;
        private static Dictionary<int, int[]> Avalues;

        public event ProgressChangedEventHandler ProgressChanged;

        protected virtual void FilterProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChanged(this, e); 
        }

        public SavitzkyGolayFilter()
        {

        }

        public void Process(PointSet aScan, ProcessParameters parameters)
        {
            // Initialize AH values
            initializeAHValues();

            int numOfDataPoints = 5;

            if (parameters != null && parameters.ContainsKey("DataPointsUsed"))
                numOfDataPoints = (int)parameters["DataPointsUsed"];

            int[] aVals = Avalues[numOfDataPoints];
            int h = Hvalues[numOfDataPoints];

            int marginSize = (numOfDataPoints + 1) / 2 - 1;
            float sumOfInts;

            var masses = new double[aScan.Keys.Count];
            aScan.Keys.CopyTo(masses, 0);
            var intensities = new float[aScan.Values.Count];
            aScan.Values.CopyTo(intensities, 0);
            
            var newIntensities = new float[masses.Length];

            for (int spectrumInd = marginSize; spectrumInd < (masses.Length - marginSize); spectrumInd++)
            {
                sumOfInts = aVals[0] * intensities[spectrumInd];

                for (int windowInd = 1; windowInd <= marginSize; windowInd++)
                    sumOfInts += aVals[windowInd] * (intensities[spectrumInd + windowInd] + intensities[spectrumInd - windowInd]);

                sumOfInts = sumOfInts / h;

                if (sumOfInts < 0) sumOfInts = 0;
                
                newIntensities[spectrumInd] = sumOfInts;
            }

            // Replace the original values with the smoothed values...
            for (int i = 0; i < aScan.Count; i++)
                aScan[masses[i]] = newIntensities[i];
        }

        /// <summary> Initialize Avalues and Hvalues
        /// These are actually constants, but it is difficult to define them as static final
        /// </summary>
        private void initializeAHValues()
        {
            if (Avalues == null || Hvalues == null)
            {
                Avalues = new Dictionary<int, int[]>();
                Hvalues = new Dictionary<int, int>();

                int[] a5Ints = new int[] { 17, 12, -3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; Avalues.Add(5, a5Ints);
                int[] a7Ints = new int[] { 7, 6, 3, -2, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; Avalues.Add(7, a7Ints);
                int[] a9Ints = new int[] { 59, 54, 39, 14, -21, 0, 0, 0, 0, 0, 0, 0, 0 }; Avalues.Add(9, a9Ints);
                int[] a11Ints = new int[] { 89, 84, 69, 44, 9, -36, 0, 0, 0, 0, 0, 0, 0 }; Avalues.Add(11, a11Ints);
                int[] a13Ints = new int[] { 25, 24, 21, 16, 9, 0, -11, 0, 0, 0, 0, 0, 0 }; Avalues.Add(13, a13Ints);
                int[] a15Ints = new int[] { 167, 162, 147, 122, 87, 42, -13, -78, 0, 0, 0, 0, 0 }; Avalues.Add(15, a15Ints);
                int[] a17Ints = new int[] { 43, 42, 39, 34, 27, 18, 7, -6, -21, 0, 0, 0, 0 }; Avalues.Add(17, a17Ints);
                int[] a19Ints = new int[] { 269, 264, 249, 224, 189, 144, 89, 24, -51, -136, 0, 0, 0 }; Avalues.Add(19, a19Ints);
                int[] a21Ints = new int[] { 329, 324, 309, 284, 249, 204, 149, 84, 9, -76, -171, 0, 0 }; Avalues.Add(21, a21Ints);
                int[] a23Ints = new int[] { 79, 78, 75, 70, 63, 54, 43, 30, 15, -2, -21, -42, 0 }; Avalues.Add(23, a23Ints);
                int[] a25Ints = new int[] { 467, 462, 447, 422, 387, 343, 287, 222, 147, 62, -33, -138, -253 }; Avalues.Add(25, a25Ints);

                int h5Int = 35; Hvalues.Add(5, h5Int);
                int h7Int = 21; Hvalues.Add(7, h7Int);
                int h9Int = 231; Hvalues.Add(9, h9Int);
                int h11Int = 429; Hvalues.Add(11, h11Int);
                int h13Int = 143; Hvalues.Add(13, h13Int);
                int h15Int = 1105; Hvalues.Add(15, h15Int);
                int h17Int = 323; Hvalues.Add(17, h17Int);
                int h19Int = 2261; Hvalues.Add(19, h19Int);
                int h21Int = 3059; Hvalues.Add(21, h21Int);
                int h23Int = 805; Hvalues.Add(23, h23Int);
                int h25Int = 5175; Hvalues.Add(25, h25Int);
            }
        }

    }
}

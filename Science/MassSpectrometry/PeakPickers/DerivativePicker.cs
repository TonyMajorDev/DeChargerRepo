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
using MassSpectrometry;

namespace SignalProcessing.PeakPickers
{
    public class DerivativePicker : IPeakPicker
    {
        public event ProgressChangedEventHandler ProgressChanged;

        public PeakList Detect(PointSet aScan, ProcessParameters parameters)
        {
            PeakList returnList = new PeakList();

            for (int i = 1; i < (aScan.Count - 1); i++)
            {
                if ((aScan[aScan.Keys[i]] > aScan[aScan.Keys[i - 1]]) && (aScan[aScan.Keys[i]] > aScan[aScan.Keys[i + 1]]))
                    returnList.Add(new ClusterPeak() { MZ = aScan.Keys[i], Intensity = aScan.Values[i], RawApex = new SortedList<double, float>() { { aScan.Keys[i-1], aScan.Values[i-1] }, { aScan.Keys[i], aScan.Values[i] }, { aScan.Keys[i + 1], aScan.Values[i + 1] } } });

                // when integer percentage changes, fire an update event...
                if ((i/aScan.Count) > 0)
                    if ((100 % (i/aScan.Count)) == 0)
                        PeakPickerProgressChanged(new ProgressChangedEventArgs(((i/aScan.Count)*100), null));
            }

            returnList.Sort();
            
            return returnList;
        }

        private readonly List<int> _peaks = new List<int>();

        public int[] GetPeaks(PointSet aScan, int windowSize)
        {
            if (windowSize < 2) windowSize = 2;
            
            _peaks.Clear();
            for (var i = 0; i < aScan.Count; i++)
            {
                var isPeak = true;
                for (var j = 0; j < windowSize; j++)
                {
                    var k = i + j - windowSize / 2;
                    if (k >= 0 && k < aScan.Count && k != i)
                    {
                        if (aScan.Values[k] >= aScan.Values[i])
                        {
                            isPeak = false;
                        }
                    }
                }
                if (isPeak)
                {
                    _peaks.Add(i);
                }
            }

            return _peaks.ToArray();
        }



        public int[] GetPeaks(byte[] buffer, int windowSize)
        {
            if (windowSize < 2)
            {
                windowSize = 2;
            }
            _peaks.Clear();
            for (var i = 0; i < buffer.Length; i++)
            {
                var isPeak = true;
                for (var j = 0; j < windowSize; j++)
                {
                    var k = i + j - windowSize / 2;
                    if (k >= 0 && k < buffer.Length && k != i)
                    {
                        if (buffer[k] >= buffer[i])
                        {
                            isPeak = false;
                        }
                    }
                }
                if (isPeak)
                {
                    _peaks.Add(i);
                }
            }

            return _peaks.ToArray();
        }
        


        protected virtual void PeakPickerProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChanged(this, e);
        }
    }
}

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

namespace SignalProcessing.PeakPickers
{
    public class BillauerPicker
    {
        // Based on: http://www.billauer.co.il/peakdet.html

        int detect_peak(
        double[] data, /* the data */
        int data_count, /* row count of data */
        int[] emi_peaks, /* emission peaks will be put here */
        int num_emi_peaks, /* number of emission peaks found */
        int max_emi_peaks, /* maximum number of emission peaks */
        int[] absop_peaks, /* absorption peaks will be put here */
        int num_absop_peaks, /* number of absorption peaks found */
        int max_absop_peaks, /* maximum number of absorption peaks
                                            */
        double delta, /* delta used for distinguishing peaks */
        bool emi_first /* should we search emission peak first of
                                     absorption peak first? */
        )
        {
            int i;
            double mx;
            double mn;
            int mx_pos = 0;
            int mn_pos = 0;
            bool is_detecting_emi = emi_first;


            mx = data[0];
            mn = data[0];

            num_emi_peaks = 0;
            num_absop_peaks = 0;

            for (i = 1; i < data_count; ++i)
            {
                if (data[i] > mx)
                {
                    mx_pos = i;
                    mx = data[i];
                }
                if (data[i] < mn)
                {
                    mn_pos = i;
                    mn = data[i];
                }

                if (is_detecting_emi &&
                        data[i] < mx - delta)
                {
                    if (num_emi_peaks >= max_emi_peaks) /* not enough spaces */
                        return 1;

                    emi_peaks[num_emi_peaks] = mx_pos;
                    ++(num_emi_peaks);

                    is_detecting_emi = false;

                    i = mx_pos - 1;

                    mn = data[mx_pos];
                    mn_pos = mx_pos;
                }
                else if ((!is_detecting_emi) &&
                        data[i] > mn + delta)
                {
                    if (num_absop_peaks >= max_absop_peaks)
                        return 2;

                    absop_peaks[num_absop_peaks] = mn_pos;
                    ++(num_absop_peaks);

                    is_detecting_emi = true;

                    i = mn_pos - 1;

                    mx = data[mn_pos];
                    mx_pos = mn_pos;
                }
            }

            return 0;
        }



//        public static int Detect(PointSet aScan, ProcessParameters parameters) //[maxtab, mintab]=peakdet(v, delta, x)
//        {
//            //%PEAKDET Detect peaks in a vector
//            //%        [MAXTAB, MINTAB] = PEAKDET(V, DELTA) finds the local
//            //% maxima and minima("peaks") in the vector V.
//            //% MAXTAB and MINTAB consists of two columns.Column 1
//            //% contains indices in V, and column 2 the found values.
//            //%
//            //% With[MAXTAB, MINTAB] = PEAKDET(V, DELTA, X) the indices
//            //%        in MAXTAB and MINTAB are replaced with the corresponding
//            //% X - values.
//            //%
//            //% A point is considered a maximum peak if it has the maximal
//            //% value, and was preceded(to the left) by a value lower by
//            //% DELTA.

//            //% Eli Billauer, 3.4.05(Explicitly not copyrighted).
//            //% This function is released to the public domain; Any use is allowed.

//maxtab = [];
//mintab = [];

//v = v(:); % Just in case this wasn't a proper vector

//if nargin< 3
//  x = (1:length(v))';
//else 
//  x = x(:);
//  if length(v)~= length(x)
//    error('Input vectors v and x must have same length');
//        end
//      end
  
//if (length(delta(:)))>1
//  error('Input argument DELTA must be a scalar');
//        end

//if delta <= 0
//  error('Input argument DELTA must be positive');
//        end

//        mn = Inf; mx = -Inf;
//mnpos = NaN; mxpos = NaN;

//lookformax = 1;

//for i=1:length(v)
//  this = v(i);
//  if this > mx, mx = this; mxpos = x(i); end
//  if this < mn, mn = this; mnpos = x(i); end
  
//  if lookformax
//    if this < mx-delta
//      maxtab = [maxtab ; mxpos mx];
//        mn = this; mnpos = x(i);
//        lookformax = 0;
//    end  
//  else
//    if this > mn+delta
//      mintab = [mintab ; mnpos mn];
//        mx = this; mxpos = x(i);
//        lookformax = 1;
//    end
//  end
//end

//    }
    }




}

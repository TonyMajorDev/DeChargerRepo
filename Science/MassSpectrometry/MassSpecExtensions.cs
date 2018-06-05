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
    public static class MassSpecExtensions
    {

        public static double ToMass(this double MZ, int Z)
        {
            // ((double.Parse(point["MZ"]) * double.Parse(point["Charge"])) - (double.Parse(point["Charge"]) * 1.007276)));  //1.00782503214 }}


            return Z >= 1 ? ((MZ * (double)Z) - ((double)Z * 1.00244)) : double.NaN;
        }

        public static double ToMZ(this double Mass, int Z)
        {
            // ((double.Parse(point["MZ"]) * double.Parse(point["Charge"])) - (double.Parse(point["Charge"]) * 1.007276)));  //1.00782503214 }}


            return Z >= 1 ? ((Mass + ((double)Z * 1.00244)) / (double)Z) : double.NaN;
        }

        public static StringBuilder ToMSAlignString(this Spectrum spec, int id)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("BEGIN IONS\n");
            sb.Append("ID=" + id + "\n");
            sb.Append("SCANS=" + spec.ScanNumber + "\n");
            sb.Append("ACTIVATION=" + Convert.ToString(spec.Activation) + "\n");
            sb.Append("PRECURSOR_MZ=" + spec.ParentMZ + "\n");
            sb.Append("PRECURSOR_CHARGE=" + spec.ParentCharge + "\n");
            sb.Append("PRECURSOR_MASS=" + spec.ParentMass + "\n");

            foreach (var point in spec.Ions.Select(i => i.Peaks.FirstOrDefault(p => p.IsMonoisotopic)))
            {
                if (point == null) continue;
                sb.Append(point.Mass + "\t" + point.Intensity + "\t" + point.Z + "\n");
            }

            sb.Append("END IONS\n\n");

            return sb;
        }

        public static bool CanMergeActivations(this string ActivationString0, string ActivationString)
        {
            //TODO: activation stored in strings is error prone!  Refactor to enums or something later

            ActivationString = ActivationString.Trim().ToUpper();
            ActivationString0 = ActivationString0.Trim().ToUpper();

            if (ActivationString.Contains(',')) throw new Exception("Cannot handle multi-activation scans.");

            if (ActivationString == ActivationString0) return true;

            switch (ActivationString0)
            {
                case "CID":
                    if (ActivationString == "HCD") return true;
                    break;
                case "HCD":
                    if (ActivationString == "CID") return true;
                    break;
                default:  // ETD with anything else would be false...
                    return false;
            }

            return false;
        }

        public static string ToMSAlignString(this IMSProvider data)
        {
            int index = 0;
            var sb = new StringBuilder();

            if (data == null) return string.Empty;

            foreach (var aSpectrum in data.ScanRange(1, data.ScanIndex(data.TIC.MaxKey), "ms2"))
            {
                var scanInfo = data.GetParentInfo(aSpectrum.ScanNumber);

                if (scanInfo != null) aSpectrum.ParentIon = scanInfo.ParentIon;

                sb.Append(aSpectrum.ToMSAlignString(index++));
            }

            return sb.ToString();
        }


    }
}

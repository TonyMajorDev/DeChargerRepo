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

// Based on Project Morpheus
// https://github.com/cwenger/Morpheus/blob/master/Morpheus/mzML/TandemMassSpectra.mzML.cs

namespace MassSpectrometry
{
    public class MassTolerance
    {
        public double Value { get; private set; }

        public MassToleranceUnits Units { get; private set; }

        public MassTolerance(double value, MassToleranceUnits units)
        {
            Value = value;
            Units = units;
        }

        public static double CalculateMassError(double experimental, double theoretical, MassToleranceUnits massErrorUnits)
        {
            if(massErrorUnits == MassToleranceUnits.Da)
            {
                return experimental - theoretical;
            }
            else if(massErrorUnits == MassToleranceUnits.ppm)
            {
                return (experimental - theoretical) / theoretical * 1e6;
            }
            else
            {
                return double.NaN;
            }
        }

        public static double operator +(double left, MassTolerance right)
        {
            if(right.Units == MassToleranceUnits.Da)
            {
                return left + right.Value;
            }
            else if(right.Units == MassToleranceUnits.ppm)
            {
                return left + left * right.Value / 1e6;
            }
            else
            {
                return double.NaN;
            }
        }

        public static double operator -(double left, MassTolerance right)
        {
            if(right.Units == MassToleranceUnits.Da)
            {
                return left - right.Value;
            }
            else if(right.Units == MassToleranceUnits.ppm)
            {
                return left - left * right.Value / 1e6;
            }
            else
            {
                return double.NaN;
            }
        }
    }
}

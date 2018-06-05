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
    public static class PPMCalc
    {
        public static double MassTolerancePPM = 5;

        public static double CurrentPPM(double Mass)
        {
            return CurrentPPM(Mass, MassTolerancePPM);
        }

        public static double MaxPPM(double Mass1, double Mass2)
        {
            return MaxPPM(Mass1, Mass2, MassTolerancePPM);
        }

        public static double CurrentPPM(double Mass, double tolerance)
        {
            return (tolerance / 1e6d) * (Mass);
        }

        public static double MaxPPM(double Mass1, double Mass2, double tolerance)
        {
            return Math.Max((tolerance / 1e6d) * (Mass1), (tolerance / 1e6d) * (Mass2));
        }

        public static double CurrentPPMbasedonMatchList(double Mass, double masstolerance)
        {
            return CurrentPPM(Mass, masstolerance);
        }

        public static double CalculatePPM(double TheoreticalParentMass, double DeltaMass)
        {
            return (DeltaMass * 1e6d) / TheoreticalParentMass;
        }

    }
}

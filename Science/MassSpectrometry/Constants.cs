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
    public static class Constants
    {
        public const double PROTON_MASS = 1.007276466879;
        public const double C12_C13_MASS_DIFFERENCE = 1.0033548378;
        public const double WATER_MONOISOTOPIC_MASS = 18.0105646942;
        public const double WATER_AVERAGE_MASS = 18.01528;
        public const double PEPTIDE_N_TERMINAL_MONOISOTOPIC_MASS = 1.0078250321;
        public const double PEPTIDE_N_TERMINAL_AVERAGE_MASS = 1.00794;
        public const double PEPTIDE_C_TERMINAL_MONOISOTOPIC_MASS = 17.0027396621;
        public const double PEPTIDE_C_TERMINAL_AVERAGE_MASS = 17.00734;
    }
}

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
using System.Threading.Tasks;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Ionic.Zlib;

// Based on Project Morpheus
// https://github.com/cwenger/Morpheus/blob/master/Morpheus/mzML/TandemMassSpectra.mzML.cs

namespace MassSpectrometry
{
    public partial class TandemMassSpectra
    {
        private const bool GET_PRECURSOR_MZ_AND_INTENSITY_FROM_MS1 = true;
        private const bool ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE = false;
        private const bool HARMONIC_CHARGE_DETECTION = false;

        public void Load(string mzmlFilepath, int minimumAssumedPrecursorChargeState, int maximumAssumedPrecursorChargeState,
            double absoluteThreshold, double relativeThresholdPercent, int maximumNumberOfPeaks,
            bool assignChargeStates, bool deisotope, MassTolerance isotopicMZTolerance, int maximumThreads)
        {
            //OnReportTaskWithoutProgress(EventArgs.Empty);

            XmlDocument mzML_temp = new XmlDocument();
            mzML_temp.Load(mzmlFilepath);
            XPathNavigator mzML = mzML_temp.CreateNavigator();

            XmlNamespaceManager xnm = new XmlNamespaceManager(mzML.NameTable);
            xnm.AddNamespace("mzML", mzML_temp.DocumentElement.NamespaceURI);

            Dictionary<string, XPathNodeIterator> referenceable_param_groups = new Dictionary<string, XPathNodeIterator>();
            foreach (XPathNavigator referenceable_param_group in mzML.Select("//mzML:mzML/mzML:referenceableParamGroupList/mzML:referenceableParamGroup", xnm))
            {
                referenceable_param_groups.Add(referenceable_param_group.GetAttribute("id", string.Empty), referenceable_param_group.SelectChildren(XPathNodeType.All));
            }

            ParallelOptions parallel_options = new ParallelOptions();
            parallel_options.MaxDegreeOfParallelism = maximumThreads;

            Dictionary<string, SpectrumNavigator> ms1s = null;
            if (GET_PRECURSOR_MZ_AND_INTENSITY_FROM_MS1)
            {
                ms1s = new Dictionary<string, SpectrumNavigator>();
//#if NON_MULTITHREADED
                foreach(XPathNavigator spectrum_navigator in mzML.Select("//mzML:mzML/mzML:run/mzML:spectrumList/mzML:spectrum", xnm).Cast<XPathNavigator>())
//#else
//                Parallel.ForEach(mzML.Select("//mzML:mzML/mzML:run/mzML:spectrumList/mzML:spectrum", xnm).Cast<XPathNavigator>(), parallel_options, spectrum_navigator =>
//#endif
                {
                    string scan_id = spectrum_navigator.GetAttribute("id", string.Empty);
                    int ms_level = -1;

                    foreach (XPathNavigator spectrum_child_navigator in spectrum_navigator.SelectChildren(XPathNodeType.All))
                    {
                        if (spectrum_child_navigator.Name.Equals("cvParam", StringComparison.OrdinalIgnoreCase))
                        {
                            if (spectrum_child_navigator.GetAttribute("name", string.Empty).Equals("ms level", StringComparison.OrdinalIgnoreCase))
                            {
                                ms_level = int.Parse(spectrum_child_navigator.GetAttribute("value", string.Empty));
                            }
                        }
                        else if (spectrum_child_navigator.Name.Equals("referenceableParamGroupRef", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (XPathNavigator navigator in referenceable_param_groups[spectrum_child_navigator.GetAttribute("ref", string.Empty)])
                            {
                                if (navigator.Name.Equals("cvParam", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (navigator.GetAttribute("name", string.Empty).Equals("ms level", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ms_level = int.Parse(navigator.GetAttribute("value", string.Empty));
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (ms_level == 1)
                    {
                        lock (ms1s)
                        {
                            ms1s.Add(scan_id, new SpectrumNavigator(spectrum_navigator, xnm));
                        }
                    }
                }
//#if !NON_MULTITHREADED
//                );
//#endif
            }

            int num_spectra = int.Parse(mzML.SelectSingleNode("//mzML:mzML/mzML:run/mzML:spectrumList", xnm).GetAttribute("count", string.Empty));

            //OnReportTaskWithProgress(EventArgs.Empty);
            object progress_lock = new object();
            int spectra_processed = 0;
            int old_progress = 0;

//#if NON_MULTITHREADED
            foreach(XPathNavigator spectrum_navigator in mzML.Select("//mzML:mzML/mzML:run/mzML:spectrumList/mzML:spectrum", xnm).Cast<XPathNavigator>())
//#else
//            Parallel.ForEach(mzML.Select("//mzML:mzML/mzML:run/mzML:spectrumList/mzML:spectrum", xnm).Cast<XPathNavigator>(), parallel_options, spectrum_navigator =>
//#endif
            {
                int spectrum_index = int.Parse(spectrum_navigator.GetAttribute("index", string.Empty));
                int spectrum_number = spectrum_index + 1;
                string spectrum_id = spectrum_navigator.GetAttribute("id", string.Empty);
                string spectrum_title = null;
                int ms_level = -1;
                int polarity = 0;
                double retention_time_minutes = double.NaN;
                string precursor_scan_id = null;
                double precursor_mz = double.NaN;
                int charge = 0;
                double precursor_intensity = double.NaN;
                string fragmentation_method = "collision-induced dissociation";
                double[] mz = null;
                double[] intensity = null;

                foreach (XPathNavigator spectrum_child_navigator in spectrum_navigator.SelectChildren(XPathNodeType.All))
                {
                    if (spectrum_child_navigator.Name.Equals("cvParam", StringComparison.OrdinalIgnoreCase))
                    {
                        if (spectrum_child_navigator.GetAttribute("name", string.Empty).Equals("ms level", StringComparison.OrdinalIgnoreCase))
                        {
                            ms_level = int.Parse(spectrum_child_navigator.GetAttribute("value", string.Empty));
                        }
                        else if (spectrum_child_navigator.GetAttribute("name", string.Empty).Equals("positive scan", StringComparison.OrdinalIgnoreCase))
                        {
                            polarity = 1;
                        }
                        else if (spectrum_child_navigator.GetAttribute("name", string.Empty).Equals("negative scan", StringComparison.OrdinalIgnoreCase))
                        {
                            polarity = -1;
                        }
                        else if (spectrum_child_navigator.GetAttribute("name", string.Empty).Equals("spectrum title", StringComparison.OrdinalIgnoreCase))
                        {
                            spectrum_title = spectrum_child_navigator.GetAttribute("value", string.Empty);
                        }
                    }
                    else if (spectrum_child_navigator.Name.Equals("referenceableParamGroupRef", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XPathNavigator navigator in referenceable_param_groups[spectrum_child_navigator.GetAttribute("ref", string.Empty)])
                        {
                            if (navigator.Name.Equals("cvParam", StringComparison.OrdinalIgnoreCase))
                            {
                                if (navigator.GetAttribute("name", string.Empty).Equals("ms level", StringComparison.OrdinalIgnoreCase))
                                {
                                    ms_level = int.Parse(navigator.GetAttribute("value", string.Empty));
                                }
                                else if (navigator.GetAttribute("name", string.Empty).Equals("positive scan", StringComparison.OrdinalIgnoreCase))
                                {
                                    polarity = 1;
                                }
                                else if (navigator.GetAttribute("name", string.Empty).Equals("negative scan", StringComparison.OrdinalIgnoreCase))
                                {
                                    polarity = -1;
                                }
                            }
                        }
                    }
                    else if (spectrum_child_navigator.Name.Equals("scanList", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XPathNavigator navigator in spectrum_child_navigator.Select("mzML:scan/mzML:cvParam", xnm))
                        {
                            if (navigator.GetAttribute("name", string.Empty).Equals("scan start time", StringComparison.OrdinalIgnoreCase))
                            {
                                retention_time_minutes = double.Parse(navigator.GetAttribute("value", string.Empty), CultureInfo.InvariantCulture);
                                if (navigator.GetAttribute("unitName", string.Empty).StartsWith("s", StringComparison.OrdinalIgnoreCase))
                                {
                                    retention_time_minutes = TimeSpan.FromSeconds(retention_time_minutes).TotalMinutes;
                                }
                            }
                        }
                    }
                    else if (spectrum_child_navigator.Name.Equals("precursorList", StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator precursor_node = spectrum_child_navigator.SelectSingleNode("mzML:precursor", xnm);
                        precursor_scan_id = precursor_node.GetAttribute("spectrumRef", string.Empty);
                        foreach (XPathNavigator navigator in precursor_node.Select("mzML:selectedIonList/mzML:selectedIon/mzML:cvParam", xnm))
                        {
                            if (navigator.GetAttribute("name", string.Empty).Equals("selected ion m/z", StringComparison.OrdinalIgnoreCase))
                            {
                                precursor_mz = double.Parse(navigator.GetAttribute("value", string.Empty), CultureInfo.InvariantCulture);
                            }
                            else if (navigator.GetAttribute("name", string.Empty).Equals("charge state", StringComparison.OrdinalIgnoreCase))
                            {
                                charge = int.Parse(navigator.GetAttribute("value", string.Empty));
                                if (polarity < 0)
                                {
                                    charge = -charge;
                                }
                            }
                            else if (navigator.GetAttribute("name", string.Empty).Equals("peak intensity", StringComparison.OrdinalIgnoreCase))
                            {
                                precursor_intensity = double.Parse(navigator.GetAttribute("value", string.Empty), CultureInfo.InvariantCulture);
                            }
                        }
                        XPathNavigator navigator2 = spectrum_child_navigator.SelectSingleNode("mzML:precursor/mzML:activation/mzML:cvParam", xnm);
                        if (navigator2 != null)
                        {
                            fragmentation_method = navigator2.GetAttribute("name", string.Empty);
                        }
                    }
                    else if (spectrum_child_navigator.Name.Equals("binaryDataArrayList", StringComparison.OrdinalIgnoreCase))
                    {
                        ReadDataFromSpectrumNavigator(spectrum_child_navigator.Select("mzML:binaryDataArray/*", xnm), out mz, out intensity);
                    }
                    if (ms_level == 1)
                    {
                        break;
                    }
                }

                if (ms_level >= 2)
                {
                    if (GET_PRECURSOR_MZ_AND_INTENSITY_FROM_MS1 && precursor_scan_id != null)
                    {
                        SpectrumNavigator ms1;
                        if (ms1s.TryGetValue(precursor_scan_id, out ms1))
                        {
                            double[] ms1_mz;
                            double[] ms1_intensity;
                            ms1.GetSpectrum(out ms1_mz, out ms1_intensity);
                            int index = -1;
                            for (int i = ms1_mz.GetLowerBound(0); i <= ms1_mz.GetUpperBound(0); i++)
                            {
                                if (index < 0 || Math.Abs(ms1_mz[i] - precursor_mz) < Math.Abs(ms1_mz[index] - precursor_mz))
                                {
                                    index = i;
                                }
                            }
                            precursor_mz = ms1_mz[index];
                            precursor_intensity = ms1_intensity[index];
                        }
                    }

                    if (mz != null && intensity != null && mz.Length > 0 && intensity.Length > 0)
                    {
                        List<MSPeak> peaks = new List<MSPeak>(mz.Length);
                        for (int i = 0; i < mz.Length; i++)
                        {
                            peaks.Add(new MSPeak(mz[i], intensity[i], 0, polarity));
                        }

                        //peaks = FilterPeaks(peaks, absoluteThreshold, relativeThresholdPercent, maximumNumberOfPeaks);

                        if (peaks.Count > 0)
                        {
                            peaks.Sort(MSPeak.AscendingMZComparison);
                            for (int c = (ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE || charge == 0 ? minimumAssumedPrecursorChargeState : charge);
                                c <= (ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE || charge == 0 ? maximumAssumedPrecursorChargeState : charge); c++)
                            {
                                List<MSPeak> new_peaks = peaks;
                                //if (assignChargeStates)
                                //{
                                //    new_peaks = AssignChargeStates(new_peaks, c, polarity, isotopicMZTolerance);
                                //    if (deisotope)
                                //    {
                                //        new_peaks = Deisotope(new_peaks, c, polarity, isotopicMZTolerance);
                                //    }
                                //}

                                double precursor_mass = MSPeak.MassFromMZ(precursor_mz, c);

                                MassSpectrometry.Spectrum spectrum = new MassSpectrometry.Spectrum();

                                foreach (var aPeak in peaks)
                                    spectrum.Add(new SignalProcessing.ClusterPeak() { MZ = aPeak.MZ, Intensity = (float)aPeak.Intensity });

                                //TandemMassSpectrum spectrum = new TandemMassSpectrum(mzmlFilepath, spectrum_number, spectrum_id, spectrum_title, retention_time_minutes, fragmentation_method, precursor_mz, precursor_intensity, c, precursor_mass, new_peaks);
                                //lock (this)
                                //{
                                //    Add(spectrum);
                                //}
                            }
                        }
                    }
                }
                else if (ms_level == 1)
                {
                    SpectrumNavigator ms1;

                    //double[] ms1_mz;
                    //double[] ms1_intensity;

                    if (ms1s.TryGetValue(spectrum_id, out ms1))
                    {
                        ms1.GetSpectrum(out mz, out intensity);

                    if (mz != null && intensity != null && mz.Length > 0 && intensity.Length > 0)
                    {
                        List<MSPeak> peaks = new List<MSPeak>(mz.Length);
                        for (int i = 0; i < mz.Length; i++)
                        {
                            peaks.Add(new MSPeak(mz[i], intensity[i], 0, polarity));
                        }

                        //peaks = FilterPeaks(peaks, absoluteThreshold, relativeThresholdPercent, maximumNumberOfPeaks);

                        if (peaks.Count > 0)
                        {
                            peaks.Sort(MSPeak.AscendingMZComparison);
                            //for (int c = (ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE || charge == 0 ? minimumAssumedPrecursorChargeState : charge);
                            //    c <= (ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE || charge == 0 ? maximumAssumedPrecursorChargeState : charge); c++)
                           // {
                                List<MSPeak> new_peaks = peaks;
                                //if (assignChargeStates)
                                //{
                                //    new_peaks = AssignChargeStates(new_peaks, c, polarity, isotopicMZTolerance);
                                //    if (deisotope)
                                //    {
                                //        new_peaks = Deisotope(new_peaks, c, polarity, isotopicMZTolerance);
                                //    }
                                //}

                             //   double precursor_mass = MSPeak.MassFromMZ(precursor_mz, c);

                                MassSpectrometry.Spectrum spectrum = new MassSpectrometry.Spectrum();

                                foreach (var aPeak in peaks)
                                    spectrum.Add(new SignalProcessing.ClusterPeak() { MZ = aPeak.MZ, Intensity = (float)aPeak.Intensity });

                                //TandemMassSpectrum spectrum = new TandemMassSpectrum(mzmlFilepath, spectrum_number, spectrum_id, spectrum_title, retention_time_minutes, fragmentation_method, precursor_mz, precursor_intensity, c, precursor_mass, new_peaks);
                                //lock (this)
                                //{
                                //    Add(spectrum);
                                //}
                            }
                        }
                    }

                }

                //lock (progress_lock)
                //{
                //    spectra_processed++;
                //    int new_progress = (int)((double)spectra_processed / num_spectra * 100);
                //    if (new_progress > old_progress)
                //    {
                //        OnUpdateProgress(new ProgressEventArgs(new_progress));
                //        old_progress = new_progress;
                //    }
                //}
            }
//#if !NON_MULTITHREADED
//            );
//#endif
        }


        //private static List<MSPeak> AssignChargeStates(IList<MSPeak> peaks, int maxAbsoluteCharge, int polarity, MassTolerance isotopicMZTolerance)
        //{
        //    List<MSPeak> new_peaks = new List<MSPeak>();

        //    for (int i = 0; i < peaks.Count; i++)
        //    {
        //        int j = i + 1;
        //        List<int> charges = new List<int>();
        //        while (j < peaks.Count)
        //        {
        //            if (peaks[j].MZ > (peaks[i].MZ + Constants.C12_C13_MASS_DIFFERENCE) + isotopicMZTolerance)
        //            {
        //                break;
        //            }

        //            for (int c = polarity * maxAbsoluteCharge; polarity > 0 ? c >= 1 : c <= -1; c -= polarity)
        //            {
        //                // remove harmonic charges, e.g. don't consider peak as a +2 (0.5 Th spacing) if it could be a +4 (0.25 Th spacing)
        //                if (HARMONIC_CHARGE_DETECTION)
        //                {
        //                    bool harmonic = false;
        //                    foreach (int c2 in charges)
        //                    {
        //                        if (c2 % c == 0)
        //                        {
        //                            harmonic = true;
        //                            break;
        //                        }
        //                    }
        //                    if (harmonic)
        //                    {
        //                        continue;
        //                    }
        //                }

        //                if (Math.Abs(MassTolerance.CalculateMassError(peaks[j].MZ, peaks[i].MZ + Constants.C12_C13_MASS_DIFFERENCE / c, isotopicMZTolerance.Units)) <= isotopicMZTolerance.Value)
        //                {
        //                    new_peaks.Add(new MSPeak(peaks[i].MZ, peaks[i].Intensity, c, polarity));
        //                    charges.Add(c);
        //                }
        //            }

        //            j++;
        //        }
        //        if (charges.Count == 0)
        //        {
        //            new_peaks.Add(new MSPeak(peaks[i].MZ, peaks[i].Intensity, 0, polarity));
        //        }
        //    }

        //    return new_peaks;
        //}

        //private static List<MSPeak> Deisotope(IEnumerable<MSPeak> peaks, int maxAbsoluteCharge, int polarity, MassTolerance isotopicMZTolerance)
        //{
        //    List<MSPeak> new_peaks = new List<MSPeak>(peaks);

        //    int p = new_peaks.Count - 1;
        //    while (p >= 1)
        //    {
        //        int q = p - 1;
        //        bool removed = false;
        //        while (q >= 0)
        //        {
        //            if (new_peaks[p].MZ > (new_peaks[q].MZ + Constants.C12_C13_MASS_DIFFERENCE) + isotopicMZTolerance)
        //            {
        //                break;
        //            }

        //            if (new_peaks[p].Intensity < new_peaks[q].Intensity)
        //            {
        //                if (polarity == 0)
        //                {
        //                    if (Math.Abs(MassTolerance.CalculateMassError(new_peaks[p].MZ, new_peaks[q].MZ + Constants.C12_C13_MASS_DIFFERENCE, isotopicMZTolerance.Units)) <= isotopicMZTolerance.Value)
        //                    {
        //                        new_peaks.RemoveAt(p);
        //                        removed = true;
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    for (int c = polarity; polarity > 0 ? c <= maxAbsoluteCharge : c >= -maxAbsoluteCharge; c += polarity)
        //                    {
        //                        if (Math.Abs(MassTolerance.CalculateMassError(new_peaks[p].MZ, new_peaks[q].MZ + Constants.C12_C13_MASS_DIFFERENCE / Math.Abs(c), isotopicMZTolerance.Units)) <= isotopicMZTolerance.Value)
        //                        {
        //                            new_peaks.RemoveAt(p);
        //                            removed = true;
        //                            break;
        //                        }
        //                    }
        //                }
        //                if (removed)
        //                {
        //                    break;
        //                }
        //            }

        //            q--;
        //        }

        //        p--;
        //    }

        //    return new_peaks;
        //}
    }

}

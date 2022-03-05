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
using Science.Chemistry;
using SignalProcessing;
using System.Diagnostics;
using System.ComponentModel;

namespace SignalProcessing
{

    public delegate void PercentChangeEventHandler(float percent, string title);
    
    
    public static class SignalProcessor
    {
        /// <summary>
        /// Renaming and changing the value of the NeutronMass(1.00244) to ProtonMass(1.00727) based on Mike's recommendation.
        /// </summary>
        public const double ProtonMass = 1.00244;///1.00727;///1.00244;///1.008664916;///1.00244;  //1.00244; //0.99803;

        public static event PercentChangeEventHandler OnProgressChange;

        public static bool FireChangeEvents = true;

        public static object ActiveProgressTarget = null;

        public static Agilent.MassSpectrometry.DataAnalysis.IonPolarity IonPolarity;

        public static bool CancelConsolidation = false;

        /// <summary>
        /// Consolidate Ions that are the same but differ in charge and scan number
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static List<Cluster> ConsolidateIons(List<Cluster> source, bool fireEvents = false, bool byCharge = true, int anchorCharge = int.MinValue)
        {
            CancelConsolidation = false;

            var alreadyGrouped = new List<Cluster>();
            var consolidatedIons = new List<Cluster>();

            float total = source.Count();
            float index = 0;

            var clusterComparer = new ClusterEqualityComparer();

            foreach (var anIon in source.OrderByDescending(i => i.Intensity))
            {
                if (CancelConsolidation) return new List<Cluster>();

                if (OnProgressChange != null && FireChangeEvents && fireEvents) OnProgressChange((index++ / total), "Merging Ions...");

                if (alreadyGrouped.Contains(anIon)) continue;
                var currentGroup = new List<Cluster>() { anIon };

                if (byCharge)
                {
                    foreach (var anotherIon in source.OrderByDescending(i => i.Score))  // consider sorting by descending i.Peaks.Count
                    {
                        //TODO: This line is VERY expensive.  Need to improve performance here!  
                        if (alreadyGrouped.Contains(anotherIon, clusterComparer) || currentGroup.Contains(anotherIon, clusterComparer)) continue;
                        if (Cluster.SimilarClusters(anIon, anotherIon)) currentGroup.Add(anotherIon);
                    }
                }
                else
                {
                    foreach (var anotherIon in source.OrderByDescending(i => i.Score))  // consider sorting by descending i.Peaks.Count
                    {
                        //TODO: This line is VERY expensive.  Need to improve performance here!  
                        if (alreadyGrouped.Contains(anotherIon, clusterComparer) || currentGroup.Contains(anotherIon, clusterComparer)) continue;
                        if (Cluster.SimilarClustersIncludingCharge(anIon, anotherIon)) currentGroup.Add(anotherIon);
                    }
                }

                Cluster aConsolidatedIon = null;

                if (currentGroup.Count > 1)
                {
                    // The first isotope pattern has the highe
                    double[] weightedValueSums = new double[currentGroup[0].Peaks.Count];
                    double[] weightSums = new double[currentGroup[0].Peaks.Count];
                    double[] intensitySums = new double[currentGroup[0].Peaks.Count];
                    double[] averageMasses = new double[currentGroup[0].Peaks.Count];
                    double[] denomenatorCount = new double[currentGroup[0].Peaks.Count];
                    List<int> chargeStates = new List<int>();

                    foreach (var aCluster in currentGroup)
                    {
                        int indexOffset = Convert.ToInt32(aCluster.Peaks[0].Mass - currentGroup[0].Peaks[0].Mass);

                        foreach (var aPeak in aCluster.Peaks)
                        {
                            if (aPeak.Index + indexOffset < 0 || aPeak.Index + indexOffset > weightSums.Length - 1) continue;  // skip out of range values

                            weightedValueSums[aPeak.Index + indexOffset] += aPeak.Mass * Math.Max(aPeak.Intensity, 0.1d);   // Consider multiplying by another factor when this is a core peak to give it more weight?
                            weightSums[aPeak.Index + indexOffset] += Math.Max(aPeak.Intensity, 0.1d);
                            //averageMasses[aPeak.Index + indexOffset] += aPeak.Mass;
                            //denomenatorCount[aPeak.Index + indexOffset]++;
                        }

                        chargeStates.Add(aCluster.Z);
                    }

                    aConsolidatedIon = new Cluster()
                    {
                        IsoPattern = currentGroup[0].IsoPattern,
                        Z = anchorCharge != int.MinValue ? anchorCharge : currentGroup[0].Z,
                        ConsolidatedCharges = chargeStates.ToArray()
                    };

                    aConsolidatedIon.Peaks = new ListOfClusterPeak();

                    for (int i = 0; i < weightedValueSums.Length; i++)
                    {
                        aConsolidatedIon.Peaks.Add(new ClusterPeak()
                        {
                            Index = i,
                            Intensity = (float)weightSums[i],
                            Mass = weightSums[i] <= 0 ? 0 : weightedValueSums[i] / weightSums[i],
                            Parent = (aConsolidatedIon)
                        });
                    }

                    aConsolidatedIon.Score = currentGroup.Select(g => g.Score).Max();  // use the maximum score of all the consolidated ions.  Consider a sum of the scores?  

                    //// Find best fit for masses to calculate deltas
                    //foreach (var aPeak in aConsolidatedIon.Peaks)
                    //{


                    //}


                    //// Label Core Peaks
                    //foreach (var aPeak in aConsolidatedIon.Peaks)
                    //{
                    //    //aPeak.Delta = aPeak.Mass

                    //}


                    // take the current group and consolidate the ions into 1 composite ion
                    //currentGroup.First().

                    aConsolidatedIon.SetMonoAndCorePeaks();

                    //if (anchorCharge != int.MinValue) aConsolidatedIon.MonoMZ = aConsolidatedIon.MonoMass.ToMZ(anchorCharge);  
                }
                else if (currentGroup.Count == 1)
                {
                    aConsolidatedIon = currentGroup[0];
                }

                //                aConsolidatedIon.FindCorePeaks();



                //aConsolidatedIon.FindMonoMzWithMin();  // this will also set the MonoOffset and the scale for the cluster

                // Find CorePeaks - peaks close to that predicted and have actual intensity of at least 30% of max intensity predicted

                ClusterPeak previousPeak = null;
                var peakSpaces = new List<double>();

                // Back calculate Proton mass from the core peaks differences
                foreach (var aPeak in aConsolidatedIon.Peaks.Where(p => p.IsCorePeak))
                {
                    if (previousPeak == null)
                    {
                        previousPeak = aPeak;
                    }
                    else
                    {
                        peakSpaces.Add((aPeak.MZ - previousPeak.MZ) / (aPeak.Index - previousPeak.Index));
                        previousPeak = aPeak;
                    }
                }

                if (!peakSpaces.Any())
                    continue;


                // Back Calculated Proton Mass -- the Dynamic Proton!
                var proton = peakSpaces.Median() * (double)aConsolidatedIon.Z;

                // Take the largest intensity from the predicted pattern, 
                // back calculate the monomass from all other peaks with an intensity that was close to that predicted using a weighted average 

                // Calculate MonoMZ from all the Core Peaks and take the Median or average


                //var Monos = new List<double>();

                //foreach (var aPeak in aConsolidatedIon.Peaks.Where(p => p.IsCorePeak))
                //{
                //    Monos.Add(aPeak.MZ - ((proton / (double)aConsolidatedIon.Z) * (aPeak.Index - aConsolidatedIon.MonoOffset)));
                //}

                //aConsolidatedIon._monoMZ = Monos.Median();

                //Proton Estimate
                //Debug.WriteLine("Proton = " + proton.ToString("0.00000") + ", Mass = " + aConsolidatedIon.MonoMass.ToString("0.0000"));
                //Debug.WriteLine(proton.ToString("0.00000") + "\t" + aConsolidatedIon.MonoMass.ToString("0.0000"));

                consolidatedIons.Add(aConsolidatedIon);


                //CurrentMonoMasses.Add(new DataPoint() { XValue = anIon.MonoMass, YValue = anIon.Intensity, ZValue = anIon.Z, Score = anIon.Score });

                //CurrentMonoMasses.Add(new DataPoint() { XValue = CurrentSpectrum.ParentMass - anIon.MonoMass, YValue = anIon.Intensity, ZValue = anIon.Z, Score = anIon.Score });

                //CurrentMonos.Add(new NewMonos()
                //{
                //    Index = j,
                //    XValue = anIon.MonoMass,
                //    Score = anIon.Score
                //});

                //CurrentMonos.Add(new NewMonos()
                //{
                //    Index = j,
                //    XValue = CurrentSpectrum.ParentMass - anIon.MonoMass,
                //    Score = anIon.Score
                //});

                //if (anIon.SecondaryMonoMass != null)
                //{
                //    //SecondaryCurrentMonos.Add(new NewMonos()
                //    //{
                //    //    Index = j,
                //    //    XValue = (CurrentSpectrum.ParentMass - anIon.SecondaryMonoMass.Value),
                //    //    Score = anIon.Score
                //    //});
                //    SecondaryCurrentMonos.Add(new NewMonos()
                //    {
                //        Index = j,
                //        XValue = (anIon.SecondaryMonoMass.Value),
                //        Score = anIon.Score
                //    });
                //}
                //else
                //{
                //    SecondaryCurrentMonos.Add(new NewMonos()
                //    {
                //        Index = j,
                //        XValue = 0,
                //        Score = anIon.Score
                //    });
                //}

                alreadyGrouped.AddRange(currentGroup);

            }

            return consolidatedIons;
        }


        public static List<Point> CalledProfile(Cluster ion)
        {

            //foreach (var aPeak in targetIon.IsoPattern)
            //{


            //}

            // Bin the signal and pattern
            double binSize = 0.006;

            var pattern = new PointSet();
            double index = 0;

            bool pastMax = false;
            double max = ion.IsoPattern.Max();

            foreach (var aPeak in ion.IsoPattern)
            {
                if (!pastMax || aPeak > 0.02) pattern.Add(ion.MonoMass + (index++ * ProtonMass), (float)aPeak);
                if (aPeak == max) pastMax = true;
            }

            //pattern = new PointSet(ion.Masses, ion.IsoPattern);

            var patternBins = pattern.RangeSum(pattern.Keys[0], pattern.Keys[pattern.Count - 1], binSize);


            var gaussian = GenerateGaussian(30000, (float)binSize);

            // scale to 1
            gaussian = gaussian.Select(v => v / gaussian.Max()).ToArray();

            //System.Diagnostics.Debug.WriteLine("Gaussian");
            //string peaks = string.Empty;

            //foreach (var peak in gaussian)
            //    peaks += peak.ToString() + "\n";

            //Console.WriteLine(peaks);

            //Console.WriteLine(peaks);

            //  Corrr1d outputs an array of ((Input1[].Count + Input2[].Count) - 1)

            //double[] patternResult;
            //double[] signalResult;

            // Cross-correlate gaussian with signal and pattern
            //.corrr1d(signalBins.Select(kvp => (double)kvp.Value).ToArray(), signalBins.Count(), gaussian, gaussian.Length, out signalResult);
            //.corrr1d(patternBins.Select(kvp => (double)kvp.Value).ToArray(), patternBins.Count(), gaussian, gaussian.Length, out patternResult);

            //var predicted = SimpleCrossCorrelation(ion.IsoPattern.Select(i => i * ion.IsoScale).ToArray(), gaussian);
            var patternResult = SimpleCrossCorrelation(patternBins.Select(kvp => (double)kvp.Value).ToArray(), gaussian);


            //var result = new SortedDictionary<double, float>();
            //int index = 0;


            //foreach (var anIntensity in predicted)
            //{
            //    result.Add(index++ * binSize + ion.MonoMass, (float)anIntensity);
            //}

            //return result;

            //x = patternResult;
            //start = signal.First().Key + massOffset;
            var start = ion.MonoMass;
            var resultFit = new List<Point>();
            double i = -10;
            var multiplier = max * ion.IsoScale;

            foreach (var v in patternResult)
                resultFit.Add(new Point() { X = start + (i++ * binSize), Y = (float)(v * multiplier) });

            return resultFit;
        }


        public static void FindMonoMass1(List<Point> data, out string result, out List<Point> resultFit, out List<Point> processedSignal)
        {
            System.Diagnostics.Debug.WriteLine("Executing FindMonoMass");

            double mass = data.First().X;

            var theoIon = GenerateAveragine(mass);

            var pattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(theoIon, 1f);

            string resultStr = string.Empty;

            foreach (var peak in pattern)
                resultStr += peak.Key.ToString() + ", " + peak.Value.ToString() + "\n";

            System.Diagnostics.Debug.WriteLine(resultStr);

            var signal = new PointSet();

            foreach (var aPoint in data.Skip(1))
            {
                signal.Add(aPoint.X, aPoint.Y);
            }

            //foreach (var aPoint in points.Split('\n').Where(s => !string.IsNullOrEmpty(s) && s.Contains(',')))
            //    signal.Add(double.Parse(aPoint.Split(',')[0]), float.Parse(aPoint.Split(',')[1]));

            //System.Diagnostics.Debug.WriteLine("Observed Raw Isotope Signal");
            //foreach (var peak in signal)
            //    System.Diagnostics.Debug.WriteLine(peak.Key.ToString() + "\t" + peak.Value.ToString());


            // Bin the signal and pattern
            double binSize = 0.006;

            //System.Diagnostics.Debug.WriteLine("Theoretical Raw Isotope Pattern");
            //foreach (var peak in pattern)
            //    System.Diagnostics.Debug.WriteLine(peak.Key.ToString() + "\t" + peak.Value.ToString());

            var patternBins = pattern.RangeSum(pattern.Keys[0], pattern.Keys[pattern.Count - 1], binSize);
            var signalBins = signal.RangeSum(signal.Keys[0], signal.Keys[signal.Count - 1], binSize);

            //System.Diagnostics.Debug.WriteLine("Binned Observed Isotope Signal");
            //foreach (var peak in signalBins.Select(kvp => (double)kvp.Value))
            //    System.Diagnostics.Debug.WriteLine(peak.ToString());

            //System.Diagnostics.Debug.WriteLine("Binned Theoretical Raw Isotope Pattern");
            //foreach (var peak in patternBins.Select(kvp => (double)kvp.Value))
            //    System.Diagnostics.Debug.WriteLine(peak.ToString());


            var gaussian = GenerateGaussian(30000, (float)binSize);

            // scale to 1
            gaussian = gaussian.Select(v => v / gaussian.Max()).ToArray();

            //System.Diagnostics.Debug.WriteLine("Gaussian");
            //string peaks = string.Empty;

            //foreach (var peak in gaussian)
            //    peaks += peak.ToString() + "\n";

            //Console.WriteLine(peaks);

            //Console.WriteLine(peaks);

            //  Corrr1d outputs an array of ((Input1[].Count + Input2[].Count) - 1)

            //double[] patternResult;
            //double[] signalResult;

            // Cross-correlate gaussian with signal and pattern
            //.corrr1d(signalBins.Select(kvp => (double)kvp.Value).ToArray(), signalBins.Count(), gaussian, gaussian.Length, out signalResult);
            //.corrr1d(patternBins.Select(kvp => (double)kvp.Value).ToArray(), patternBins.Count(), gaussian, gaussian.Length, out patternResult);

            var signalResult = SimpleCrossCorrelation(signalBins.Select(kvp => (double)kvp.Value).ToArray(), gaussian);
            var patternResult = SimpleCrossCorrelation(patternBins.Select(kvp => (double)kvp.Value).ToArray(), gaussian);


            //Console.WriteLine("Observed Signal + Gaussian\n");

            //peaks = "Observed Signal + Gaussian\n";

            //foreach (var peak in signalResult)
            //    peaks += peak.ToString() + "\n";

            //Console.WriteLine(peaks);

            //peaks = "Theoretical + Gaussian\n";

            ////Console.WriteLine("Theoretical + Gaussian");
            //foreach (var peak in patternResult)
            //    peaks += peak.ToString() + "\n";

            //Console.WriteLine(peaks);

            //double[] finalResult;

            //.corrr1d(signalResult, signalResult.Length, patternResult, patternResult.Length, out finalResult);

            var finalResult = SimpleCrossCorrelation(signalResult, patternResult);

            //peaks = "Final Cross Correlation for offset\n";
            ////Console.WriteLine("Final Cross Correlation for offset\n");
            //foreach (var peak in finalResult)
            //    peaks += peak.ToString() + "\n";

            //Console.WriteLine(peaks);

            //var patternBins = pattern.Range(pattern.MinKey, pattern.MaxKey, (int)((pattern.MaxKey - pattern.MinKey) / binSize));

            // Cross Correlate!
            //var signal = new List<double>();
            //var pattern = new List<double>();

            //foreach (var aLine in System.IO.File.ReadAllLines("2SINES3.csv"))
            //foreach (var aLine in System.IO.File.ReadAllLines("GausCross.csv"))
            //{
            //    if (aLine.Split(',').Count() > 1 && aLine.Split(',')[0] != string.Empty) signal.Add(double.Parse(aLine.Split(',')[0]));
            //    if (aLine.Split(',').Count() > 1 && aLine.Split(',')[1] != string.Empty) pattern.Add(double.Parse(aLine.Split(',')[1]));
            //}


            //DoubleVector harr_kernel = new DoubleVector(1, 1, -1, -1);
            //DoubleVector data = new DoubleVector(1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1);

            //double[] result;

            //.corrr1d(signal.Select(kvp => (double)kvp.Value).ToArray(), signal.Count, patternBins.Select(kvp => (double)kvp.Value).ToArray(), patternBins.Count(), out result);
            //.corrr1d(data.ToArray(), 11, harr_kernel.ToArray(), 4, out result);


            //foreach (var aPoint in result)
            //    Debug.WriteLine(aPoint);

            // Return X value with Maximal Y value!  This should be the offset of the 2 signals
            var offset = IndexOfMax(finalResult);

            var predictedBinOffset = (theoIon.MonoIsotopicMass - pattern.First().Key) / binSize;
            var massOffset = (((IndexOfMax(finalResult) + 1) - patternResult.Length) + predictedBinOffset) * binSize;

            System.Diagnostics.Debug.WriteLine(signal.First().Key);

            var x = signalResult.Skip(10);
            double start = data[1].X + (binSize / 2); // pattern.First().Key;
            processedSignal = new List<Point>();
            int i = 0;

            foreach (var v in x)
                processedSignal.Add(new Point() { X = start + (i++ * binSize), Y = (float)v });


            x = patternResult;
            start = signal.First().Key + massOffset;
            resultFit = new List<Point>();
            i = -10;
            var multiplier = data.Max(p => p.Y);

            foreach (var v in x)
                resultFit.Add(new Point() { X = start + (i++ * binSize), Y = (float)v * multiplier });

            //resultFit = signalResult.Select(p => new point() { X = 1, Y = (float)p }).ToList();

            result = (signal.First().Key + massOffset).ToString() + "," + finalResult.Max().ToString("#0");

            //return theoIon.MonoIsotopicMass - (offset * binSize);

            //return 10d;
        }

        private static SortedDictionary<int, double[]> AveragineCache = new SortedDictionary<int, double[]>();


        private static object lockvar = new object();

        public static double? SecondMono = new double?();

        public static double FindMonoMz(this Cluster cluster)
        {
            //System.Diagnostics.Debug.WriteLine("Executing FindMono");

            var corePeaks = cluster.Peaks.Where(p => p.IsCorePeak);

            if (!corePeaks.Any()) return 0;

            double mass = corePeaks.OrderByDescending(p => p.Intensity).First().MZ * cluster.Z;

            double[] isoPattern;

            //TODO: Fix AverageCache  exception: Collection was modified after the enumerator was instantiated.

            lock (lockvar)
            {
                var approximateMass = (int)(Math.Round(mass * 0.01)) * 100;

                var ip = AveragineCache.Where(m => m.Key == approximateMass).FirstOrDefault();

                if (ip.Key != 0)
                {
                    isoPattern = ip.Value;
                }
                else
                {

                    //var start = DateTime.Now;

                    var theoIon = GenerateAveragine(approximateMass);

                    var pattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(theoIon, 1f);

                    isoPattern = pattern.Select(p => (double)p.Value).ToArray();

                    //var stop = DateTime.Now;

                    //var newTotal = stop.Subtract(start).TotalMilliseconds;

                    //start = DateTime.Now;



                    //var theoString = GenerateAveragineString(mass);

                    //isoPattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(theoString);


                    //stop = DateTime.Now;

                    //var oldTotal = stop.Subtract(start).TotalMilliseconds;

                    //Debug.WriteLine("New (PNNL): " + newTotal + " ms vs. Old (Mercury): " + oldTotal + " ms");


                    //string resultStr = "Predicted Points\n";

                    //foreach (var peak in pattern)
                    //    resultStr += peak.Key.ToString() + ", " + peak.Value.ToString() + "\n";

                    //System.Diagnostics.Debug.WriteLine(resultStr);


                    //resultStr = "Observed Points\n";

                    //foreach (var peak in cluster.Peaks)
                    //    resultStr += peak.MZ.ToString() + ", " + peak.Intensity.ToString() + "\n";

                    //System.Diagnostics.Debug.WriteLine(resultStr);            


                    AveragineCache.Add(approximateMass, isoPattern);
                }

            }

            cluster.IsoPattern = isoPattern;

            var signalResult = SimpleCrossCorrelation(cluster.Peaks.Select(p => (double)p.Intensity).ToArray(), isoPattern);

            //var peaks = "Cross Correlation\n";

            //foreach (var peak in signalResult)
            //    peaks += peak.ToString() + "\n";

            //Console.WriteLine(peaks);

            //var predictedMonoIndex = signalResult.MaxIndex() - pattern.Count;
            var predictedMonoIndex = (signalResult.MaxIndex() - isoPattern.Length) + 1;

            var predictedSecondaryMonoIndex = (signalResult.SecondaryIndex() - isoPattern.Length) + 1;


            // Find cluster peak with the best score.  Take the index of that peak, and subtract the predictedMonoIndex.  
            // Then multiply by ZSpace and subtract that from the MZ of the best scoring peak.

            var bestPeak = cluster.Peaks.Where(p => p.IsCorePeak).OrderBy(p => p.Delta).FirstOrDefault();
            int bestPeakIndex = bestPeak.Index;

            //var secondbestPeak = cluster.Peaks.Where(p => p.IsCorePeak).OrderBy(p => p.Delta).Skip(1).FirstOrDefault();
            //int secondbestPeakIndex = secondbestPeak.Index;

            var peak = cluster.Peaks.OrderBy(p => p.Delta);

            var mono = cluster.Peaks[bestPeakIndex].MZ - ((bestPeakIndex - predictedMonoIndex) * (ProtonMass / (double)cluster.Z));

            if (signalResult[signalResult.MaxIndex()] * 0.7 <= signalResult[signalResult.SecondaryIndex()])
                SecondMono = cluster.Peaks[bestPeakIndex].MZ - ((bestPeakIndex - predictedSecondaryMonoIndex) * (ProtonMass / (double)cluster.Z));

            //cluster.Peaks[0].IsoIndex = 42;


            return mono;
        }



        public static double FindMonoMzWithMin(this Cluster cluster)
        {
            //System.Diagnostics.Debug.WriteLine("Executing FindMono");

            //double mass = cluster.Peaks.Where(p => p.IsCorePeak).OrderByDescending(p => p.Intensity).First().MZ * cluster.Z;
            double mass = cluster.Peaks.MaxBy(p => p.Mass).Mass;  //.Where(p => p.IsCorePeak).OrderByDescending(p => p.Intensity).First().MZ * cluster.Z;

            double[] isoPattern;

            //TODO: Fix AverageCache  exception: Collection was modified after the enumerator was instantiated.

            lock (lockvar)
            {

                var approximateMass = (int)(Math.Round(mass * 0.01)) * 100;

                var ip = AveragineCache.Where(m => m.Key == approximateMass).FirstOrDefault();

                if (ip.Key != 0)
                {
                    isoPattern = ip.Value;
                }
                else
                {

                    //var start = DateTime.Now;

                    var theoIon = GenerateAveragine(mass);

                    var pattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(theoIon, 1f);

                    isoPattern = pattern.Select(p => (double)p.Value).ToArray();

                    //var stop = DateTime.Now;

                    //var newTotal = stop.Subtract(start).TotalMilliseconds;

                    //start = DateTime.Now;



                    //var theoString = GenerateAveragineString(mass);

                    //isoPattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(theoString);


                    //stop = DateTime.Now;

                    //var oldTotal = stop.Subtract(start).TotalMilliseconds;

                    //Debug.WriteLine("New (PNNL): " + newTotal + " ms vs. Old (Mercury): " + oldTotal + " ms");


                    //string resultStr = "Predicted Points\n";

                    //foreach (var peak in pattern)
                    //    resultStr += peak.Key.ToString() + ", " + peak.Value.ToString() + "\n";

                    //System.Diagnostics.Debug.WriteLine(resultStr);


                    //resultStr = "Observed Points\n";

                    //foreach (var peak in cluster.Peaks)
                    //    resultStr += peak.MZ.ToString() + ", " + peak.Intensity.ToString() + "\n";

                    //System.Diagnostics.Debug.WriteLine(resultStr);            


                    AveragineCache.Add(approximateMass, isoPattern);
                }

            }

            cluster.IsoPattern = isoPattern;

            // Use Top 10 peaks in the iso pattern
            int isoStart = Math.Max(0, cluster.IsoPattern.MaxIndex() - 5);
            int isoEnd = Math.Min(cluster.IsoPattern.Length - 1, cluster.IsoPattern.MaxIndex() + 5);

            //var ratio = cluster.Peaks.Max(p => p.Intensity);

            var bestAlignmentIndex = 0;
            var bestIntensityScaleIndex = 0;
            var bestDifferential = double.MinValue;
            var bestScale = 0f;

            for (int p = 0; p < (cluster.Peaks.Count - (isoEnd - isoStart)) - isoStart; p++)
            {
                //int count = 0;

                for (int i = isoStart; i <= isoEnd; i++)
                {
                    // i is the index of the peak to use for scaling
                    //cluster.Peaks[p]

                    if (isoPattern[i] < 0.20) continue;  // But, don't scale the entire pattern to a relatively weak peak...

                    float scale = (float)(cluster.Peaks[p + i].Intensity / isoPattern[i]);

                    var differential = 0d;

                    for (int j = isoStart; j <= isoEnd; j++)
                    {
                        // Now we enumerate over the peaks with peak i as the fit scale

                        var predictedIntensity = isoPattern[j] * scale;

                        if (predictedIntensity > cluster.Peaks[j + p].Intensity)
                            differential += cluster.Peaks[j + p].Intensity - ((predictedIntensity - cluster.Peaks[j + p].Intensity) * 2);  // penalty for missing signal -- this will amplify the difference
                        else
                            differential += predictedIntensity;
                    }

                    //differential /= scale;

                    if (differential > bestDifferential)
                    {
                        // we have a better fit! 
                        bestAlignmentIndex = p;
                        bestIntensityScaleIndex = i;
                        bestDifferential = differential;
                        bestScale = scale;
                    }


                    //Debug.WriteLine("Peak " + count++ + ": " + (aPeak.Intensity / i).ToString("0.00"));

                    //ratio = (float)Math.Min(aPeak.Intensity / i, ratio);
                }

                //Debug.WriteLine("Min ratio is: " + ratio);
            }

            Debug.Print("MonoMass is " + cluster.Peaks[bestAlignmentIndex].Mass);

            //            cluster.MonoMZ = cluster.Peaks[bestAlignmentIndex].;

            cluster.IsoScale = bestScale;
            cluster.MonoOffset = bestAlignmentIndex;

            //TODO: Calculate MonoMZ (the basis for MonoMass)

            // Back calculate the Mono by using an intensity weighted average of the Mono calculation of each of the core peaks.


            return cluster.Peaks[bestAlignmentIndex].MZ;

            //var ratio = cluster.Peaks.Max(p => p.Intensity);

            //foreach (var aPeak in cluster.Peaks)
            //{
            //    int count = 0;

            //    foreach (var i in isoPattern)
            //    {
            //        Debug.WriteLine("Peak " + count++ + ": " + (aPeak.Intensity / i).ToString("0.00"));

            //        ratio = (float)Math.Min(aPeak.Intensity / i, ratio);


            //    }

            //    Debug.WriteLine("Min ratio is: " + ratio);

            //}


            //return 0d;
        }



        public static double FindTest()
        {
            //System.Diagnostics.Debug.WriteLine("Executing FindMono");

            var cluster = new Cluster() { Z = 3 };
            cluster.Peaks = new ListOfClusterPeak();

            cluster.Peaks.Add(new ClusterPeak() { MZ = 268.47418, Intensity = 100f, Delta = 0.001f, IsCorePeak = true, Parent = (cluster), Index = 0 });
            cluster.Peaks.Add(new ClusterPeak() { MZ = 268.80845, Intensity = 26.15f, Delta = 0.002f, IsCorePeak = true, Parent = (cluster), Index = 1 });
            cluster.Peaks.Add(new ClusterPeak() { MZ = 269.14172, Intensity = 9.54f, Delta = 0.001f, IsCorePeak = true, Parent = (cluster), Index = 2 });
            cluster.Peaks.Add(new ClusterPeak() { MZ = 269.47538, Intensity = 2.46f, Delta = 0.003f, Parent = (cluster), Index = 3 });
            cluster.Peaks.Add(new ClusterPeak() { MZ = 269.80909, Intensity = 0.52f, Delta = 0.003f, Parent = (cluster), Index = 4 });

            double mass = cluster.Peaks.Where(p => p.IsCorePeak).OrderByDescending(p => p.Intensity).First().MZ * cluster.Z;

            var theoIon = GenerateAveragine(mass);

            var pattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(theoIon, 1f);

            //string resultStr = "Predicted Points\n";

            //foreach (var peak in pattern)
            //    resultStr += peak.Key.ToString() + ", " + peak.Value.ToString() + "\n";

            //System.Diagnostics.Debug.WriteLine(resultStr);


            //resultStr = "Observed Points\n";

            //foreach (var peak in cluster.Peaks)
            //    resultStr += peak.MZ.ToString() + ", " + peak.Intensity.ToString() + "\n";

            //System.Diagnostics.Debug.WriteLine(resultStr);            


            var signalResult = SimpleCrossCorrelation(cluster.Peaks.Select(p => (double)p.Intensity).ToArray(), pattern.Select(p => (double)p.Value).ToArray());

            //var peaks = "Cross Correlation\n";

            //foreach (var peak in signalResult)
            //    peaks += peak.ToString() + "\n";

            //Console.WriteLine(peaks);

            var predictedMonoIndex = (signalResult.MaxIndex() - pattern.Count) + 1;

            // Find cluster peak with the best score.  Take the index of that peak, and subtract the predictedMonoIndex.  
            // Then multiply by ZSpace and subtract that from the MZ of the best scoring peak.

            var bestPeakIndex = cluster.Peaks.OrderBy(p => p.Delta).FirstOrDefault().Index;

            var result = cluster.Peaks[bestPeakIndex].MZ - ((bestPeakIndex - predictedMonoIndex) * (ProtonMass / (double)cluster.Z));

            return result;
        }



        private static double[] SimpleCrossCorrelation(double[] input1, double[] input2)
        { 
            double[] pattern, data, result;

            if (input1.Length > input2.Length)
            {
                data = input1;
                pattern = input2;
            }
            else
            {
                data = input2;
                pattern = input1;
            }

            var dataList = new List<double>();
            dataList.AddRange(new double[pattern.Length]);
            dataList.AddRange(data);
            dataList.AddRange(new double[pattern.Length*2]);

            data = dataList.ToArray();

            result = new double[data.Length - pattern.Length];

            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < pattern.Length; j++)
                    result[i] += data[i+j] * pattern[j];
            }

            return result;

        }


     
        //public static List<int> Bucketize(this IEnumerable<KeyValuePair<double, float>> source, int totalBuckets)
        //{
        //    //Based on: http://stackoverflow.com/questions/2387916/looking-for-a-histogram-binning-algorithm-for-decimal-data

        //    var min = double.MaxValue;
        //    var max = double.MinValue;
        //    var buckets = new List<float>();

        //    foreach (var value in source)
        //    {
        //        min = Math.Min(min, value);
        //        max = Math.Max(max, value);
        //    }
        //    var bucketSize = (max - min) / totalBuckets;
        //    foreach (var value in source)
        //    {
        //        int bucketIndex = 0;
        //        if (bucketSize > 0.0)
        //        {
        //            bucketIndex = (int)((value - min) / bucketSize);
        //            if (bucketIndex == totalBuckets)
        //            {
        //                bucketIndex--;
        //            }
        //        }
        //        buckets[bucketIndex]++;
        //    }
        //    return buckets;
        //} 





        private static double GetSigma()
        {
            return GetSigma(30000);
        }

        private static double GetSigma(int resolution)
        {
            double m = 1000;  // value suggested by Rick and Mike

            double fwhm = m / resolution;

            // Based on: http://mathworld.wolfram.com/GaussianFunction.html
            return fwhm / 2.35482;  // (2 * Math.Sqrt(2 * Math.Log(2)));
        }

        private static double[] GenerateGaussian()
        {
            return GenerateGaussian(30000, 0.05f);
        }

        private static double[] GenerateGaussian(int resolution, float binSize)
        {
            var points = new List<double>();

            var s = GetSigma(resolution);

            //System.Diagnostics.Debug.WriteLine("Gaussian internal");

            for (int i = -10; i <= 10; i++)
            {
                var x = i * binSize;

                var g = (Math.Exp((x * x) / (-2 * s * s)) / (s * Math.Sqrt(2 * Math.PI)));

                points.Add(g);

                //System.Diagnostics.Debug.WriteLine(x.ToString() + "\t " + g.ToString());

            }

            //System.Diagnostics.Debug.WriteLine(" End Gaussian internal");

            return points.ToArray();
        }

        private static int IndexOfMax(double[] result)
        {
            int maxIndex = 0;

            for (int i = 0; i < result.Length; i++)
                if (result[i] > result[maxIndex]) maxIndex = i;

            return maxIndex;
        }

        private static string GenerateAveragineString(double mw)
        {
            float AveragineMW = 111.1254f;
            float AverageC = 4.9384f;
            float AverageH = 7.7583f;
            float AverageN = 1.3577f;
            float AverageO = 1.4773f;
            float AverageS = 0.0417f;

            var roundNumAveragine = (int)Math.Round(mw / AveragineMW, 0);

            var CNum = Math.Round(AverageC * roundNumAveragine, 0).ToString("0");
            var HNum = Math.Round(AverageH * roundNumAveragine, 0).ToString("0");
            var NNum = Math.Round(AverageN * roundNumAveragine, 0).ToString("0");
            var ONum = Math.Round(AverageO * roundNumAveragine, 0).ToString("0");
            var SNum = Math.Round(AverageS * roundNumAveragine, 0).ToString("0");

            // Example: C644 H1012 N177 O193 S5
            var formula = (CNum != "0" ? "C" + CNum : "")
                      + (HNum != "0" ? "H" + HNum : "")
                      + (NNum != "0" ? "N" + NNum : "")
                      + (ONum != "0" ? "O" + ONum : "")
                      + (SNum != "0" ? "S" + SNum : "");

            //return new Ion(formula, -1);
            return formula;
        }

        private static IIon GenerateAveragine(double mw)
        {
            float AveragineMW = 111.1254f;
            float AverageC = 4.9384f;
            float AverageH = 7.7583f;
            float AverageN = 1.3577f;
            float AverageO = 1.4773f;
            float AverageS = 0.0417f;

            var roundNumAveragine = (int)Math.Round(mw / AveragineMW, 0);

            // Example: C(644) H(1012) N(177) O(193) S(5)
            var formula = "C(" + Math.Round(AverageC * roundNumAveragine, 0)
                      + ") H(" + Math.Round(AverageH * roundNumAveragine, 0)
                      + ") N(" + Math.Round(AverageN * roundNumAveragine, 0)
                      + ") O(" + Math.Round(AverageO * roundNumAveragine, 0)
                      + ") S(" + Math.Round(AverageS * roundNumAveragine, 0) + ")";

            return new Ion(formula, -1);
            //return new Molecule(formula);
        }

        //[OperationContract, WebGet]
        public static string IsoTest()
        {
            //var ion = new Ion("C(3) H(8) N(1) O(2)", -1);

            //var ion = new Ion("C(647) H(1016) N(178) O(194) S(5)", -1);
            var ion = new Ion("C(644) H(1012) N(177) O(193) S(5)", -1);


            //var ion = new Ion("C(80) H(112) N(16) O(16)", -1);
            var m = new Molecule("C(80) H(112) N(16) O(16)");

            var result = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(ion);

            string resultStr = string.Empty;

            foreach (var peak in result)
                resultStr += peak.Key.ToString() + ", " + peak.Value.ToString() + "\n";

            return resultStr;
        }
        //[WebInvoke(RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare, Method = "POST", UriTemplate = "searchCustomer")]
        //public string MatchPattern(PointSet points)
        //{
        //    return "OK";
        //}


        public static int MaxIndex<T>(this IEnumerable<T> source)
        {
            IComparer<T> comparer = Comparer<T>.Default;
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Empty sequence");
                }
                int maxIndex = 0;
                T maxElement = iterator.Current;
                int index = 0;
                while (iterator.MoveNext())
                {
                    index++;
                    T element = iterator.Current;
                    if (comparer.Compare(element, maxElement) > 0)
                    {
                        maxElement = element;
                        maxIndex = index;
                    }
                }
                return maxIndex;
            }
        }

        public static int SecondaryIndex<T>(this IEnumerable<T> source)
        {
            IComparer<T> comparer = Comparer<T>.Default;
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    throw new InvalidOperationException("Empty sequence");
                }

                int maxIndex = 0;
                T maxElement = iterator.Current;
                int secondaryIndex = 0;
                T secondaryElement = iterator.Current;
                int index = 0;

                while (iterator.MoveNext())
                {
                    index++;
                    T element = iterator.Current;
                    if (comparer.Compare(element, maxElement) > 0)
                    {
                        secondaryElement = maxElement;
                        secondaryIndex = maxIndex;
                        maxElement = element;
                        maxIndex = index;
                    }
                }

                return secondaryIndex;
            }
        }

    }

    public class Point
    {
        //[DataMember] 
        public double X { get; set; }
        //[DataMember] 
        public float Y { get; set; }
    }

    public struct Range
    {
        public double Start { get; set; }
        public double End { get; set; }
    }

    public static class PPM
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
            return CurrentPPM(Mass, masstolerance) + 0.2;
        }

        public static double CalculatePPM(double TheoreticalParentMass, double DeltaMass)
        {
            return (DeltaMass * 1e6d) / TheoreticalParentMass;
        }

    }

    public class ProgressChangeEventArgs : EventArgs
    {
        private float _percent;

        public ProgressChangeEventArgs(float percent)
        {
            _percent = percent;
        }

        public float GetPercent()
        {
            return _percent;
        }
    }


    //[ServiceContract]
    //public class point
    //{
    //    //    [OperationContract]
    //    public double X { get; set; }
    //    //    [OperationContract]
    //    public float Y { get; set; }

    //    public override string ToString()
    //    {
    //        return X.ToString() + ", " + Y.ToString();
    //    }
    //}

}

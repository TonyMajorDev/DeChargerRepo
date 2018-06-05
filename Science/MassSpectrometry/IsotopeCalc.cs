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

#region copyright
/**
 * Project Title: Isotopic distribution Calc
 * Description: Isotopic Distribution Calculator
 * Copyright: Copyright (C) 2006/2007
 * Company: Eli Lilly and Company
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Linq;

using Pattern = System.Collections.Generic.List<MassSpectrometry.Peak>;
using FreqData = System.Collections.Generic.List<double>;
using Composition = System.Collections.Generic.List<MassSpectrometry.Iso_abundancy>;

using Kiss_fft_cfg = MassSpectrometry.Kiss_fft_state;
using Science.Chemistry;
using SignalProcessing;

namespace MassSpectrometry
{
    public static class IsotopeCalc
    {
        //static MwtWinDll.MolecularWeightCalculator mMwtWin = new MwtWinDll.MolecularWeightCalculator();

        const double TWOPI = 2 * Math.PI;
        const double HALFPI = Math.PI / 2;
        static double ELECTRON_MASS = new Atom("e").Isotopes[0].Mass;

        // Calculate integer average molecular weight
        private static long Calc_intMW(IIon theIon)
        {
            return (long)(theIon.AverageMass + 0.5d);
        }


        private static double Calc_variance(IIon theIon)
        {
            double molvar = 0;
            foreach (var atom in theIon.ElementalComposition)
            {
                double avemass = 0;
                foreach (Science.Chemistry.Atom.Isotope anIsotope in atom.Key.Isotopes)
                    avemass += anIsotope.Mass * anIsotope.Abundance;

                double variance = 0;
                foreach (Science.Chemistry.Atom.Isotope anIsotope in atom.Key.Isotopes)
                    variance += (anIsotope.Mass - avemass) * (anIsotope.Mass - avemass) * anIsotope.Abundance;

                molvar += atom.Value * variance;
            }

            return molvar; // the sum of variances of atoms
        }

        private static int Calc_massrange(double molvar)
        {
            int tmp = (int)(Math.Sqrt(1 + molvar) * 20);

            if ((tmp & (tmp - 1)) == 0)
                return tmp; // already a power of 2

            // round up to the nearest power of 2
            int result;
            for (result = 1; tmp != 0; tmp >>= 1)
            {
                result <<= 1;
            }
            return result;
        }

        private static void Calc_freq(kiss_fft_cpx[] data, long npoints, int mass_range, long mass_shift, IIon anIon)
        {
            for (int i = 0; i < npoints; i++)
            {
                double freq;
                if (i <= npoints / 2)
                    // first half of Frequency Domain (+)masses
                    freq = (double)i / mass_range;
                else
                    // second half of Frequency Domain (-)masses
                    freq = (double)(i - npoints) / mass_range;
                double r = 1;
                double theta = 0;
                foreach (var atom in anIon.ElementalComposition)
                {
                    double real = 0, imag = 0;

                    foreach (var isotope in atom.Key.Isotopes)
                    {
                        double X = TWOPI * (int)Math.Round(isotope.Mass) * freq;  //TODO: Verify that this cast to int is what we want!
                        real += isotope.Abundance * Math.Cos(X);
                        imag += isotope.Abundance * Math.Sin(X);
                    }
                    // Convert to polar coordinates, r then theta
                    double tempr = Math.Sqrt(real * real + imag * imag);
                    r *= Math.Pow(tempr, (double)atom.Value);
                    if (real > 0)
                    {
                        theta += atom.Value * Math.Atan(imag / real);
                    }
                    else if (real < 0)
                    {
                        theta += atom.Value * (Math.Atan(imag / real) + Math.PI);
                    }
                    else if (imag > 0)
                    {
                        theta += atom.Value * HALFPI;
                    }
                    else
                    {
                        theta += atom.Value * -HALFPI;
                    }
                }

                // Convert back to real:imag coordinates and store
                double a = r * Math.Cos(theta);
                double b = r * Math.Sin(theta);
                double c = Math.Cos(TWOPI * mass_shift * freq);
                double d = Math.Sin(TWOPI * mass_shift * freq);
                data[i].r = a * c - b * d;
                data[i].i = b * c + a * d;
            }
        }

        // TODO: check whether the data sums to one,
        // it would be enough to normalize once.
        private static void Filter_data(kiss_fft_cpx[] data, long npoints, long intMW, double limit, Pattern result)
        {
            // Normalize the intensity to a probability distribution
            double sumarea = 0;
            for (int i = 0; i < npoints; ++i)
                if (data[i].r < 0)
                    data[i].r = 0; // get rid of negative values
                else
                    sumarea += data[i].r;
            for (int i = 0; i < npoints; ++i)
                data[i].r = data[i].r / sumarea;

            // Find out which values can be discarded as too small
            int start, end;
            for (start = (int)(npoints / 2 + 1); start < npoints; start++)
                if (data[start].r >= limit)
                    break;
            for (end = (int)(npoints / 2); end >= 0; end--)
                if (data[end].r >= limit)
                    break;

            result.Clear();

            for (int i = start + 1; i < npoints; i++)
            {
                Peak p = new Peak();
                p.comp = new List<Iso_abundancy>();
                p.int_mass = (int)(i - npoints + intMW); // integer masses are enough
                p.mass = 0;
                p.rel_area = data[i].r;
                result.Add(p);
            }
            for (int i = 0; i < end; i++)
            {
                Peak p = new Peak();
                p.comp = new List<Iso_abundancy>();
                p.int_mass = (int)(i + intMW);
                p.mass = 0;
                p.rel_area = data[i].r;
                result.Add(p);
            }

            // renormalize
            sumarea = 0;
            for (int i = 0; i < result.Count; i++)
            {
                sumarea += result[i].rel_area;
            }
            for (int i = 0; i < result.Count; i++)
            {
                Peak tmp = new Peak();
                tmp = result[i];
                tmp.rel_area = result[i].rel_area / sumarea;
                result[i] = tmp;
            }
        }

        private static void Calc_pattern(Pattern result, IIon theIon, double limit)
        {
            int ptsperamu = 1;

            long intMW = Calc_intMW(theIon);
            double molvar = Calc_variance(theIon);
            int mass_range = Calc_massrange(molvar);

            long npoints = mass_range * ptsperamu;
            kiss_fft_cpx[] freq_data = new kiss_fft_cpx[npoints];
            kiss_fft_cpx[] peak_data = new kiss_fft_cpx[npoints];

            Calc_freq(freq_data, npoints, mass_range, -intMW, theIon);

            Kiss_fft_cfg cfg = kiss_fft_c.Kiss_fft_alloc((int)npoints, 0);
            kiss_fft_c.Kiss_fft(cfg, freq_data, ref peak_data);
            //this.DebugPrint(peak_data);
            Filter_data(peak_data, npoints, intMW, limit, result);
            //this.DebugPrint(result);

        }

        //Just for debugging, added by Mz
        private static void DebugPrint(kiss_fft_cpx[] peak_data)
        {
            for (int i = 0; i < peak_data.Length; i++)
            {
                Debug.WriteLine("[Item index] : " + i);
                Debug.WriteLine(peak_data[i].i);
                Debug.WriteLine(peak_data[i].r);
            }
        }

        //Just for debugging, added by Mz
        private static void DebugPrint(Pattern result)
        {
            for (int i = 0; i < result.Count; i++)
            {
                Debug.WriteLine("[Item index] : " + i);
                Debug.WriteLine(result[i].int_mass);
                Debug.WriteLine(result[i].mass);
                Debug.WriteLine(result[i].rel_area);
            }
        }

        // this is an O(n + m) algo, where n is product size
        // and m is parent size
        private static void Calc_composition(Pattern product, Pattern parent,
                      Atom anAtom, Atom.Isotope anIsotope,
                      double weight)
        {
            int i = 0;
            int j = 0;
            while (true)
            {
                if (i == product.Count || j == parent.Count)
                {
                    break;
                }
                int prod_mass = product[i].int_mass;
                int par_mass = parent[j].int_mass;
                if (prod_mass < par_mass)
                {
                    i++;
                }
                else if (prod_mass > par_mass)
                {
                    j++;
                }
                else
                {
                    Iso_abundancy ia = new Iso_abundancy();
                    ia.Element = anAtom;
                    ia.iso_index = anAtom.GetIndexOfIsotope(anIsotope);
                    // steps 4, 5 and 6. divide by parent peaks and multiply
                    ia.weight = product[i].rel_area / parent[j].rel_area * weight;
                    parent[j].comp.Add(ia);
                    j++;
                }
            }
        }



        ///// <summary>
        ///// Calculate the Theoretical Isotopic Distribution of this Ion and return a single point per peak (histogram)
        ///// </summary>
        ///// <param name="theMolecule">This should be a string in the form of "C425 H667 N117 O127 S4"</param>
        ///// <returns>Array of intensities</returns>
        //public static double[] CalcIsotopePeaks(this string theMolecule)
        //{
        //  Note: PNNL library was too slow for large ions, went back to FFT based algorithm.

        //    //TODO: Switch to new PNNL based library: 
        //    //s = "C425 H667 N117 O127 S4";

        //    //blnAddProtonChargeCarrier = false;
        //    //intChargeState = 1;
        //    //Console.WriteLine("Isotopic abundance test with Charge=" + intChargeState + "; do not add a proton charge carrier");


        //    return mMwtWin.ComputeIsotopicAbundances(theMolecule);
        //}



        /// <summary>
        /// Calculate the Theoretical Isotopic Distribution of this Ion and return a single point per peak (histogram)
        /// </summary>
        /// <param name="theIon"></param>
        /// <returns>A Theoretical MS1 Spectrum (with relative % intesities)</returns>
        public static PointSet CalcIsotopePeaks(this Science.Chemistry.IIon theIon, float maxScale = 100f)
        {
            var points = new PointSet();

            // Convert result to List<Peak> peakPattern.

            var peakPattern = Calculate(theIon, 0d);

            // find the maximum
            double max_area = 0;

            foreach (var aPeak in peakPattern)
                if (max_area < aPeak.rel_area) max_area = aPeak.rel_area;

            if (max_area == 0) throw new Exception("Failed to compute an isotope pattern.  "); // return; // empty pattern

            double print_limit = Math.Pow((double)10, -7) / 2;

            foreach (var aPeak in peakPattern)
            {
                var val = aPeak.rel_area / max_area * maxScale;
                if (val >= print_limit) points.Add(aPeak.mass, (float)val);
            }

            //            return new MS1Spectrum(points, theIon.Charge, theIon.MonoIsotopicMassToCharge, "m/z (amu)", "Intensity (relative)");


            return points;
        }

        /// <summary>
        /// Calculate the Theoretical Isotopic Distribution of this Ion and return gaussian peaks based on the resolving Power specified
        /// </summary>
        /// <param name="theIon">The Ion object</param>
        /// <param name="resolvingPower">The FWHM based resolving power (Resolving Power = Mass/deltaMass = Mass/FWHM)</param>
        /// <returns></returns>
        public static PointSet CalcIsotopePeaks(this Science.Chemistry.IIon theIon, int resolvingPower)
        {
            // What is resolution?  http://fiehnlab.ucdavis.edu/projects/Seven_Golden_Rules/Mass_Resolution/
            // Gaussian Peak Function: http://en.wikipedia.org/wiki/Gaussian_function
            // Resolution and Resolving Power: http://courses.cm.utexas.edu/jbrodbelt/ch390l/pdf/resolution.pdf

            var peakPattern = Calculate(theIon, 0d);

            // find the maximum
            double max_area = 0;

            foreach (var aPeak in peakPattern)
                if (max_area < aPeak.rel_area) max_area = aPeak.rel_area;

            if (max_area == 0) throw new Exception("Failed to compute an isotope pattern.  "); // return; // empty pattern

            double print_limit = Math.Pow((double)10, -7) / 2;

            double pointSpacing = 0.02;

            double xRangeMax = peakPattern.LastOrDefault(delegate(MassSpectrometry.Peak val) { return !double.IsNaN(val.mass); }).mass + 2;
            double xRangeMin = Math.Max(peakPattern.LastOrDefault(delegate(MassSpectrometry.Peak val) { return !double.IsNaN(val.mass); }).mass - 1, 0);


            // replaced FindLast: http://stackoverflow.com/questions/435729/findlast-on-ienumerable
            //  double xRangeMax = peakPattern.FindLast(delegate(MassSpectrometry.Peak val) { return !double.IsNaN(val.mass); }).mass + 2;
            //  double xRangeMin = Math.Max(peakPattern.Find(delegate(MassSpectrometry.Peak val) { return !double.IsNaN(val.mass); }).mass - 1, 0);


            var points = new PointSet();

            // Generate points with 0 intensity
            for (double x = xRangeMin; x <= (xRangeMax - pointSpacing); x = x + pointSpacing)
                points.Add(Math.Round(x, 3), 0);

            // Add the apex of all isotope peaks to avoid chopped off peak tops
            foreach (var aPeak in peakPattern) if (!double.IsNaN(aPeak.mass)) points.Add(aPeak.mass, 0);

            var masses = new List<double>(points.Keys);

            foreach (var aPeak in peakPattern)
            {
                if (!double.IsNaN(aPeak.mass))
                {
                    // http://mathworld.wolfram.com/GaussianFunction.html

                    double fwhm = 1 / (resolvingPower / aPeak.mass);

                    double sigma = fwhm / (2 * Math.Sqrt(2 * Math.Log(2)));

                    double height = aPeak.rel_area / max_area * 100;
                    if (height >= print_limit)
                    {
                        //Add the cooresponding points for this peak (gaussian function)
                        foreach (var mass in masses)
                            points[mass] += (float)(height * Math.Exp(-1 * (Math.Pow((mass - aPeak.mass), 2) / (2 * sigma * sigma))));
                    }
                }
            }

            //TODO: filter out all the 0's
            for (int i = 1; i < masses.Count - 1; i++)
            {
                if (points[masses[i - 1]] < 0.01 && points[masses[i]] < 0.01 && points[masses[i + 1]] < 0.01)
                    points[masses[i]] = float.NaN; // tag the points to remove with NaN
            }

            //TODO: Remove the NaN points
            //(points as IDictionary<double, double>).RemoveAll(delegate(KeyValuePair<double, double> val) { return double.IsNaN(val.Value); });


            // return new MS1Spectrum(points, theIon.Charge, theIon.MonoIsotopicMassToCharge, "m/z (amu)", "Intensity (relative)");
            return points;
        }

        private static Pattern Calculate(Science.Chemistry.IIon theIon, double limit)
        {
            var parent = new Pattern();

            // calculate the elemental compositions
            Calc_pattern(parent, theIon, limit);// step 1. compute the parent

            Pattern product = new Pattern();
            Ion tempIon = new Ion(theIon.Formula, theIon.Charge);

            foreach (var atom in theIon.ElementalComposition)
            {
                tempIon[atom.Key]--;
                Calc_pattern(product, tempIon, limit);  // step 2. compute the product 
                tempIon[atom.Key]++;

                //Pattern elem_data = _ad[elem_index];
                foreach (Science.Chemistry.Atom.Isotope anIsotope in atom.Key.Isotopes)
                {
                    if (anIsotope.Name != "Electron")
                    {
                        //Peak p = elem_data[j];
                        Pattern tmp = new Pattern(product);
                        for (int k = 0; k < tmp.Count; k++)
                        {
                            Peak tmpPeak = new Peak();
                            tmpPeak.comp = new List<Iso_abundancy>();
                            tmpPeak.int_mass = tmp[k].int_mass + (int)Math.Round(anIsotope.Mass);
                            tmpPeak.mass = tmp[k].mass;
                            tmpPeak.rel_area = tmp[k].rel_area;
                            tmp[k] = tmpPeak;
                        }
                        Calc_composition(tmp, parent, atom.Key, anIsotope, anIsotope.Abundance * atom.Value);
                    }
                }
            }

            // calculate the accurate mass and use
            // error correction method 2            
            for (int i = 0; i < parent.Count; i++)
            {
                Peak p = parent[i];
                p.mass = 0;
                double int_mass = 0;
                for (int j = 0; j < p.comp.Count; j++)
                {


                    Iso_abundancy ia = p.comp[j];
                    int_mass += ((int)Math.Round(ia.Element.Isotopes[ia.iso_index].Mass)) * ia.weight;
                    p.mass += ia.Element.Isotopes[ia.iso_index].Mass * ia.weight;
                }
                p.mass *= p.int_mass;
                p.mass /= int_mass;
                if (theIon.Charge < 0)
                    p.mass = p.mass / Math.Abs(theIon.Charge) - ELECTRON_MASS;
                else if (theIon.Charge > 0)
                    p.mass = p.mass / Math.Abs(theIon.Charge) + ELECTRON_MASS;
                parent[i] = p;
            }

            return parent;
        }

        private static void Print_pattern(Pattern result, int digits)
        {
            // find the maximum
            double max_area = 0;
            for (int i = 0; i < result.Count; i++)
            {
                if (max_area < result[i].rel_area)
                {
                    max_area = result[i].rel_area;
                }
            }
            if (max_area == 0)
                return; // empty pattern

            double print_limit = Math.Pow((double)10, -digits) / 2;
            for (int i = 0; i < result.Count; i++)
            {
                Peak p = result[i]; ;
                double mass = p.mass;
                int int_mass = p.int_mass;
                double rel_area = p.rel_area;
                double val = rel_area / max_area * 100;
                if (val >= print_limit)
                {
                    string outStr = int_mass + " " + mass.ToString("f" + digits.ToString()) + " " + val.ToString("f" + digits.ToString());
                    string outStr2 = string.Empty;

                    // print the isotopic composition
                    Composition c = p.comp;
                    for (int j = 0; j < c.Count; j++)
                    {
                        Iso_abundancy ia = c[j];
                        outStr2 += " " + ia.Element.Symbol + (int)Math.Round(ia.Element.Isotopes[ia.iso_index].Mass) + ":" + ia.weight.ToString("f" + digits.ToString());
                    }
                    Console.Out.Write(outStr + outStr2 + "\n");
                }
            }
        }

        private static string FixedDecimal(double c, int digits)
        {
            string digit = string.Empty;
            for (int i = 0; i < digits; i++)
            {
                digit += "#";
            }
            return c.ToString("#." + digit);
        }
    }

    internal struct kiss_fft_cpx
    {
        public double r;
        public double i;
    }

    internal class ConstVar
    {
        public const int MAXFACTORS = 32;
    }
    internal struct Kiss_fft_state
    {
        public int nfft;
        public int inverse;
        public int[] factors;
        public List<kiss_fft_cpx> twiddles;
    }

    // composition
    internal struct Iso_abundancy
    {
        public Atom Element;
        public int iso_index;
        public double weight;
    }

    internal struct Peak
    {
        public double mass;
        public int int_mass;
        public double rel_area;
        public List<Iso_abundancy> comp;
    }
}

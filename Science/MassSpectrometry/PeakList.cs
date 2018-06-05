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
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Specialized;
using Science.Chemistry;
using MassSpectrometry;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Web.Script.Serialization;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
//using Newtonsoft.Json;

namespace SignalProcessing
{

    public class PeakList : List<ClusterPeak>
    {
        // Default sort order is by mass
        public PeakList() { }

        public PeakList(IEnumerable<ClusterPeak> peaks) : base(peaks) { }

        public bool IsSortedByMass { get; private set; }

        public bool IsSortedByMZ { get; private set; }


        //public SortedList<double, float> as 

        // User-defined conversion from Digit to double 
        public static implicit operator SortedList<double, float>(PeakList input)
        {
            return new SortedList<double, float>(input.ToDictionary(k => k.MZ, v => v.Intensity));
        }

        //  User-defined conversion from double to Digit 
        //public static implicit operator Digit(double d)
        //{
        //    return new Digit(d);
        //}

        public new void Clear()
        {
            base.Clear();
            IsSortedByMass = false;
            IsSortedByMZ = false;
        }



        public void SortByMass()
        {
            if (IsSortedByMass) return;

            this.Sort(ClusterPeak.sortByMass());
            IsSortedByMass = true;
            IsSortedByMZ = false;
        }

        public void SortByMZ()
        {
            if (IsSortedByMZ) return;

            this.Sort(ClusterPeak.sortByMassToCharge());
            IsSortedByMass = false;
            IsSortedByMZ = true;
        }



        public ClusterPeak GetItemWithMaxYValueForXRange(double start, double end, bool useHybrid = false)
        {
            var window = this.Range(start, end);

            if (window == null) return null;

            return (window.Any() ? window.MaxBy(p => useHybrid ? p.HybridIntensity : p.Intensity) : null);
        }

        public float GetMaxYValueForXRange(double zoomMin, double zoomMax, bool useHybrid = false)
        {
            var window = this.Range(zoomMin, zoomMax);

            if (window.Any())
                return window.Max(p => useHybrid ? p.HybridIntensity : p.Intensity);
            else
                return 0;
        }

        //internal System.Collections.IEnumerable Range(double min, double max, int p)
        //{
        //    throw new NotImplementedException();
        //}

        public IEnumerable<ClusterPeak> Range(double axisMin, double axisMax, float intensityThreshold, bool useHybrid = false)
        {
            return this.Range(axisMin, axisMax).Where(p => (useHybrid ? p.HybridIntensity : p.Intensity) > intensityThreshold);
        }

        /// <summary>
        /// Get the range of elements between the start and end specified (inclusive of the endpoints)
        /// </summary>
        /// <param name="startkey">Starting Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="endkey">Ending Key value (will automatically use the closest if the specified value does not exist</param>
        /// <returns>An Enumerable of the Key/Value Pairs</returns>
        internal IEnumerable<ClusterPeak> Range(double startMass, double endMass)
        {
            return this.Range(startMass, endMass, true);
        }



        /// <summary>
        /// Get the range of elements between the start and end specified
        /// </summary>
        /// <param name="startkey">Starting Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="endkey">Ending Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="includeEnds">Whether or not to include the start and end keys in the range returned</param>
        /// <returns>An Enumerable of the Key/Value Pairs</returns>
        public IEnumerable<ClusterPeak> Range(double startkey, double endkey, bool includeEnds = true)
        {
            if (this.IsSortedByMZ)
                return MZRange(startkey, endkey, includeEnds);
            else if (this.IsSortedByMass)
                return MassRange(startkey, endkey, includeEnds);

            // Unsorted, sort first
            this.SortByMass();
            return MassRange(startkey, endkey, includeEnds);

            return null;
        }

        private IEnumerable<ClusterPeak> MassRange(double startkey, double endkey, bool includeEnds)
        {
            var startIndex = this.BinarySearch(new ClusterPeak() { Mass = Math.Min(startkey, endkey) });
            var endIndex = this.BinarySearch(new ClusterPeak() { Mass = Math.Max(startkey, endkey) });

            if (startIndex == endIndex) return new List<ClusterPeak>();  // empty list

            startIndex = startIndex < 0 ? (this[~startIndex].Mass < startkey ? ~startIndex + 1 : ~startIndex) : (includeEnds ? startIndex : startIndex + 1);
            endIndex = endIndex < 0 ? ((~endIndex > this.Count - 1 || this[~endIndex].Mass > endkey) ? ~endIndex - 1 : ~endIndex) : (includeEnds ? endIndex : endIndex - 1);

            // bounds check
            endIndex = Math.Min(endIndex, this.Count - 1);
            startIndex = Math.Max(startIndex, 0);

            if (endIndex < startIndex) return null;  // no point in range

            var results = this.GetRange(startIndex, (endIndex - startIndex) + 1);

            return results;
        }

        private IEnumerable<ClusterPeak> MZRange(double startkey, double endkey, bool includeEnds)
        {

            var startIndex = this.BinarySearch(new ClusterPeak() { MZ = Math.Min(startkey, endkey) }, ClusterPeak.sortByMassToCharge());
            var endIndex = this.BinarySearch(new ClusterPeak() { MZ = Math.Max(startkey, endkey) }, ClusterPeak.sortByMassToCharge());

            startIndex = startIndex < 0 ? (this[~startIndex].MZ < startkey ? ~startIndex + 1 : ~startIndex) : (includeEnds ? startIndex : startIndex + 1);
            endIndex = endIndex < 0 ? (this[~endIndex].MZ > endkey ? ~endIndex - 1 : ~endIndex) : (includeEnds ? endIndex : endIndex - 1);

            // bounds check
            endIndex = Math.Min(endIndex, this.Count - 1);
            startIndex = Math.Max(startIndex, 0);

            if (endIndex < startIndex) return null;  // no point in range

            var results = this.GetRange(startIndex, (endIndex - startIndex) + 1);

            return results;
        }

        /// <summary>
        /// Get the range of elements between the start and end specified (inclusive of the endpoints)
        /// </summary>
        /// <param name="startkey">Starting Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="endkey">Ending Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="numBins">Number of bins</param>
        /// <returns>An Enumerable of the Key/Value Pairs</returns>
        public IEnumerable<ClusterPeak> Range(double startkey, double endkey, int numBins, float valueMin = 0)
        {
            if (this.IsSortedByMZ)
                return MZRange(startkey, endkey, numBins, valueMin);
            else if (this.IsSortedByMass)
                return MassRange(startkey, endkey, numBins, valueMin);

            // Unsorted, sort first
            this.SortByMass();
            return MassRange(startkey, endkey, numBins, valueMin);

            return null;
        }


        private IEnumerable<ClusterPeak> MZRange(double startkey, double endkey, int numBins, float valueMin = 0)
        {
            var window = this.Range(startkey, endkey).OrderBy(p => p.MZ);  // The Order method call should not be necessary here... but sometimes the values are not sorted....
            var result = new PeakList();

            if (window == null) return result;

            var mostIntensePointInBin = window.First();

            double binsize = (endkey - startkey) / numBins;
            double binlimit = mostIntensePointInBin.Mass + binsize;

            foreach (var aPeak in window)
            {
                if (aPeak.MZ > binlimit)
                {
                    if (mostIntensePointInBin.HybridIntensity > valueMin) result.Add(mostIntensePointInBin);
                    mostIntensePointInBin = aPeak;
                    while (binlimit < aPeak.MZ) binlimit += binsize;
                }
                else
                {
                    mostIntensePointInBin = (aPeak.HybridIntensity > mostIntensePointInBin.HybridIntensity) ? aPeak : mostIntensePointInBin;
                }
            }

            result.Add(mostIntensePointInBin);

            return result;
        }

        private IEnumerable<ClusterPeak> MassRange(double startkey, double endkey, int numBins, float valueMin = 0)
        {
            var window = this.Range(startkey, endkey).OrderBy(p => p.Mass);   // The Order method call should not be necessary here... but sometimes the values are not sorted....
            var result = new PeakList();

            if (window == null || !window.Any()) return result;

            var mostIntensePointInBin = window.First();

            double binsize = (endkey - startkey) / numBins;
            double binlimit = mostIntensePointInBin.Mass + binsize;

            foreach (var aPeak in window)
            {
                if (aPeak.Mass > binlimit)
                {
                    if (mostIntensePointInBin.HybridIntensity > valueMin) result.Add(mostIntensePointInBin);
                    mostIntensePointInBin = aPeak;
                    while (binlimit < aPeak.Mass) binlimit += binsize;
                }
                else
                {
                    mostIntensePointInBin = (aPeak.HybridIntensity > mostIntensePointInBin.HybridIntensity) ? aPeak : mostIntensePointInBin;
                }
            }

            result.Add(mostIntensePointInBin);

            return result;
        }


        //public IEnumerable<ClusterPeak> Range(double startkey, double endkey, bool includeEnds)
        //{
        //    //using the Binary Search, find the closest value and use that!
        //    double? actualStartKey = startkey, actualEndKey = endkey;

        //    if (!this.ContainsKey(startkey)) actualStartKey = this.FindClosestInRange(startkey, startkey, endkey);
        //    if (!this.ContainsKey(endkey)) actualEndKey = this.FindClosestInRange(endkey, startkey, endkey);

        //    if (actualStartKey.HasValue || actualEndKey.HasValue)
        //    {
        //        actualStartKey = actualStartKey ?? actualEndKey;
        //        actualEndKey = actualEndKey ?? actualStartKey;

        //        return this.Range(this.IndexOfKey(actualStartKey.Value), this.IndexOfKey(actualEndKey.Value), includeEnds);
        //    }

        //    return null;  // there are no points in that range!

        //    //this.GetViewBetween(
        //}

        //private bool ContainsKey(double startkey)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Find the closest X value to the target passed in using a binary search.  
        /// </summary>
        /// <param name="target">Target X Value</param>
        /// <returns>Closest X Value</returns>
        public ClusterPeak FindClosestInRange(double target, double start, double end)
        {
            if (this.IsSortedByMZ)
                return FindClosestInRangeByMZ(target, start, end);
            else if (this.IsSortedByMass)
                return FindClosestInRangeByMass(target, start, end);

            // Unsorted, sort first
            this.SortByMass();
            return FindClosestInRangeByMass(target, start, end);

            return null;
        }


        /// <summary>
        /// Find the closest X value to the target passed in using a binary search.  
        /// </summary>
        /// <param name="target">Target X Value</param>
        /// <returns>Closest X Value</returns>
        public ClusterPeak FindClosestInRangeByMZ(double target, double start, double end)
        {
            var index = this.BinarySearch(new ClusterPeak() { MZ = target }, ClusterPeak.sortByMassToCharge());

            ClusterPeak closest = null;

            if (index >= 0)
            {
                closest = this[index];  // exact match found
            }
            else if (index < 0)
            {
                closest = this[~index];  // closest

                if (closest.MZ > target && ~index > 0 && (target - this[~index - 1].MZ < closest.MZ - target) && this[~index - 1].MZ > start)
                    closest = this[~index - 1];  // closer
                else if (closest.MZ < target && ~index < this.Count && (this[~index + 1].MZ - target < target - closest.MZ) && this[~index + 1].MZ < end)
                    closest = this[~index + 1];  // closer
            }

            if (closest.MZ > start && closest.MZ < end)
                return closest;
            else
                return null;
        }

        /// <summary>
        /// Find the closest X value to the target passed in using a binary search.  
        /// </summary>
        /// <param name="target">Target X Value</param>
        /// <returns>Closest X Value</returns>
        public ClusterPeak FindClosestInRangeByMass(double target, double start, double end)
        {
            var index = this.BinarySearch(new ClusterPeak() { Mass = target }, ClusterPeak.sortByMass());

            ClusterPeak closest = null;

            if (index >= 0)
            {
                closest = this[index];  // exact match found
            }
            else if (index < 0)
            {
                closest = this[~index];  // closest

                if (closest.Mass > target && ~index > 0 && (target - this[~index - 1].Mass < closest.Mass - target) && this[~index - 1].Mass > start)
                    closest = this[~index - 1];  // closer
                else if (closest.Mass < target && ~index < this.Count && (this[~index + 1].Mass - target < target - closest.Mass) && this[~index + 1].Mass < end)
                    closest = this[~index + 1];  // closer
            }

            if (closest.Mass > start && closest.Mass < end)
                return closest;
            else
                return null;
        }







    }




    public class ListOfClusterPeak : List<ClusterPeak>, IXmlSerializable
    {
        public ListOfClusterPeak() : base() { }

        public ListOfClusterPeak(IEnumerable<ClusterPeak> peaks) : base(peaks)
        { }


        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //reader.ReadContentAsBase64()



            //if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "ListOfClusterPeak")
            {
                byte[] buffer = new byte[19];



                //_name = reader["Name"];
                //_enabled = Boolean.Parse(reader["Enabled"]);
                //_color = Color.FromArgb(Int32.Parse(reader["Color"]));

                //  if (reader.ReadToDescendant("Peaks"))
                {

                    //while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "ClusterPeak")

                    //TODO: read all the peak array binary data at once and loop through to read all the peaks
                    ///while (reader.ReadContentAsBase64(buffer, 0, 19) == 19)
                    while (reader.ReadElementContentAsBase64(buffer, 0, 19) == 19)
                    {
                        //  read Base64 from XML
                        // Parse the byte array into values

                        //reader.ReadContentAsBase64(buffer, 0, 19);

                        var p = new ClusterPeak();
                        p.SetFromBytes(buffer);
                        this.Add(p);

                        //MyEvent evt = new MyEvent();

                        //evt.ReadXml(reader);
                        //_events.Add(evt);
                    }
                }

                //reader.Read();
            }
        }

        public void WriteXml(XmlWriter writer)
        {

            foreach (var aPeak in this)
            {
                writer.WriteBase64(aPeak.ToBytes(), 0, 19);
            }


            //writer.WriteAttributeString("Name", _name);
            //writer.WriteAttributeString("Enabled", _enabled.ToString());
            //writer.WriteAttributeString("Color", _color.ToArgb().ToString());

            //foreach (MyEvent evt in _events)
            //{
            //    writer.WriteStartElement("MyEvent");
            //    evt.WriteXml(writer);
            //    writer.WriteEndElement();
            //}
        }
    }

    [Serializable]
    public class ClusterPeak : IComparable<ClusterPeak>
    {
        Cluster _parent;

        // Based on: http://support.microsoft.com/kb/320727        
        //[NonSerialized]
        [XmlIgnore]
        public Cluster Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }

        public int Index;

        double _mass = -1;
        public double Mass
        {
            get
            {
                if (_mass == -1 && _mz > -1 && this.Z > -1)
                    _mass = (_mz * _z) - (_z * SignalProcessor.ProtonMass);

                return _mass;
            }

            set
            {
                _mass = value;
            }
        }

        private double _mz = -1;
        public double MZ
        {
            get
            {
                if (_mz == -1 && _mass > -1)
                {
                    if (_z > -1)
                        _mz = (_mass + (_z * SignalProcessor.ProtonMass)) / _z;
                    else
                        _mz = (_mass + (Z * SignalProcessor.ProtonMass)) / Z;
                }

                return _mz;
            }

            set
            {
                _mz = value;
            }
        }

        private double _z = -1;
        public int Z
        {
            get
            {
                if (_z > 0)
                    return (int)_z;
                else if (_mass > -1 && _mz > -1)
                    _z = _mass / (_mz - SignalProcessor.ProtonMass);
                else if (this.Parent != null && this.Parent.Z > -1)
                    _z = this.Parent.Z;

                return (int)_z;
            }
            set
            {
                _z = value;
            }
        }

        public int IsoIndex
        {
            get
            {
                return (Parent == null || double.IsNaN(Parent.MonoMZ)) ? -1 : Convert.ToInt32((this.MZ - Parent.MonoMZ) / (SignalProcessor.ProtonMass / (double)Parent.Z));
            }
        }

        /// <summary>
        /// Intensity value with an isotopic filter applied to non-core peaks
        /// </summary>
        public float HybridIntensity
        {
            get
            {
                if (Parent == null) return Intensity;
                if (IsCorePeak) return Intensity;
                if (MZ < Parent.MonoMZ) return 0;
                if (IsoIndex >= Parent.IsoPattern.Count()) return 0;
                return Math.Min(Intensity, (float)Parent.IsoPattern[IsoIndex] * Parent.IsoScale);  // use a calculated Intensity value for peaks that are more intense than expected.  
            }
        }

        public byte[] ToBytes()
        {
            /// Format:
            /// Each ClusterPeak contains:
            /// byte 0   - Index as byte (0-255) 
            /// byte 1-4 - Delta as single precision float
            /// byte 5-12 - Mass as double precision double
            /// MZ is skipped (calculated by ClusterPeak Class from Mass and Z
            /// byte 13 - Charge Z as sbyte (-128 - 128) (set to 0 if null)
            /// byte 14-17 - Intensity as single precision float
            /// byte 18 - IsCorePeak as byte (true/false)
            /// 

            var result = new List<byte>(19);

            result.Add((byte)this.Index);
            result.AddRange(BitConverter.GetBytes(this.Delta));
            result.AddRange(BitConverter.GetBytes(this.Mass));
            result.Add((byte)this.Z);
            result.AddRange(BitConverter.GetBytes(this.Intensity));
            result.AddRange(BitConverter.GetBytes(this.IsCorePeak));

            return result.ToArray();
        }

        public void SetFromBytes(byte[] buffer)
        {
            /// Format:
            /// Each ClusterPeak contains:
            /// byte 0   - Index as byte (0-255) 
            /// byte 1-4 - Delta as single precision float
            /// byte 5-12 - Mass as double precision double
            /// MZ is skipped (calculated by ClusterPeak Class from Mass and Z
            /// byte 13 - Charge Z as sbyte (-128 - 128) (set to 0 if null)
            /// byte 14-17 - Intensity as single precision float
            /// byte 18 - IsCorePeak as byte (true/false)
            /// 

            this.Index = unchecked((byte)buffer[0]);
            this.Delta = BitConverter.ToSingle(buffer, 1);
            this.Mass = BitConverter.ToDouble(buffer, 5);
            this.Z = unchecked((sbyte)buffer[13]);
            this.Intensity = BitConverter.ToSingle(buffer, 14);
            this.IsCorePeak = buffer[18] != 0;
        }


        public float Intensity { get; set; }
        public string Description
        {
            get
            {
                //return string.Empty;

                //return "Mass = " + this.Mass.ToString("0.0000") + "\nMZ = " + MZ.ToString("0.0000") + " *" + this.Z.ToString() + "\nCorePeak? " + (IsCorePeak ? "Yes" : "No") + "\nIsMono? " + (IsMonoisotopic ? "Yes" : "No");  // + "\nDelta? " + ((this.Delta < 0.001) ? "0" : ((this.Delta > 1) ? ">1" : this.Delta.ToString("0.000"))); 
                return "Mass = " + this.Mass.ToString("0.0000") + "\nMZ = " + MZ.ToString("0.0000") + " *" + this.Z.ToString() + "\nCorePeak? " + (IsCorePeak ? "Yes" : "No") + "\nIntensity? " + this.Intensity + "\nIsMono? " + (IsMonoisotopic ? "Yes" : "No") + (this.Parent != null ? ("\nScore = " + this.Parent.Score.ToString("0")) : "") +
                        ((this.Parent != null && this.Parent.ConsolidatedCharges != null && this.Parent.ConsolidatedCharges.Distinct().Count() > 1) ? ("\nAll Z=" + string.Join(",", this.Parent.ConsolidatedCharges.Distinct().OrderBy(p => p))) : "");

                //return "Mass = " + this.Mass.ToString("0.0000") + "\nMZ = " + MZ.ToString("0.0000") + " *" + this.Z.ToString() + "\nCorePeak? " + (IsCorePeak ? "Yes" : "No") + "\nHyIntensity? " + this.HybridIntensity + "\nIntensity? " + this.Intensity + "\nIsMono? " + (IsMonoisotopic ? "Yes" : "No") + (this.Parent != null ? ("\nScore = " + this.Parent.Score.ToString("0")) : "") +
                //        ((this.Parent != null && this.Parent.ConsolidatedCharges != null && this.Parent.ConsolidatedCharges.Distinct().Count() > 1) ? ("\nAll Z=" + string.Join(",", this.Parent.ConsolidatedCharges.Distinct().OrderBy(p => p))) : "");            
            }
        }

        public string ShortDescription
        {
            get
            {
                //return string.Empty;

                //return "Mass = " + this.Mass.ToString("0.0000") + "\nMZ = " + MZ.ToString("0.0000") + " *" + this.Z.ToString() + "\nCorePeak? " + (IsCorePeak ? "Yes" : "No") + "\nIsMono? " + (IsMonoisotopic ? "Yes" : "No");  // + "\nDelta? " + ((this.Delta < 0.001) ? "0" : ((this.Delta > 1) ? ">1" : this.Delta.ToString("0.000"))); 
                return "Mass = " + this.Mass.ToString("0.0000") + "\nMZ = " + MZ.ToString("0.0000") + " *" + this.Z.ToString() + "\nIntensity? " + this.Intensity + "\nIsMono? " + (IsMonoisotopic ? "Yes" : "No") +
                        ((this.Parent != null && this.Parent.ConsolidatedCharges != null && this.Parent.ConsolidatedCharges.Distinct().Count() > 1) ? ("\nAll Z=" + string.Join(",", this.Parent.ConsolidatedCharges.Distinct().OrderBy(p => p))) : "");
            }
        }

        public float Delta;

        /// <summary>
        /// This is the closest peak to the Mono m/z that was fit from a theoretical isotope pattern
        /// </summary>
        public bool IsMonoisotopic
        {
            get
            {
                return (Parent == null || double.IsNaN(Parent.MonoMZ)) ? false : (Math.Abs(Parent.MonoMZ - this.MZ) < ((1d / Parent.Z) * 0.25));
            }
        }

        //private bool? _isCorePeak = null;
        public bool IsCorePeak
        {


            //get
            //{
            //    if (!_isCorePeak.HasValue)
            //    {
            //        // Find CorePeaks - peaks close to the apex of the predicted pattern and have actual intensity of at least 30% of max intensity predicted                    
            //        if (this.Parent != null && this.Parent.IsoPattern != null && this.Parent.IsoScale > 0 && (this.Parent.IsoPattern.Length > this.Index - this.Parent.MonoOffset) && (0 <= this.Index - this.Parent.MonoOffset))
            //        {
            //            var predictedIntensity = this.Parent.IsoPattern[this.Index - this.Parent.MonoOffset] * this.Parent.IsoScale;
            //            _isCorePeak = this.Parent.IsoPattern[this.Index - this.Parent.MonoOffset] > 0.2 && this.Intensity > predictedIntensity * 0.8 && this.Intensity < predictedIntensity * 1.2;
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    }

            //    return _isCorePeak.Value;
            //}

            //set { _isCorePeak = value; }

            get;
            set;
        }


        public override string ToString() { return "mz" + MZ.ToString("0.000") + ", z" + this.Z.ToString() + ", m" + this.Mass.ToString("0.000") + ", i" + this.Intensity.ToString("0") + (this.IsCorePeak ? " +" : ""); }


        // Method to return IComparer object for sort helper.        
        public static IComparer<ClusterPeak> sortByMass()
        {
            return new ByMass();
        }

        // Method to return IComparer object for sort helper.        
        public static IComparer<ClusterPeak> sortByMassToCharge()
        {
            return new ByMassToCharge();
        }

        private class ByMass : IComparer<ClusterPeak>
        {
            public int Compare(ClusterPeak x, ClusterPeak y)
            {
                return x.Mass.CompareTo(y.Mass);
            }
        }

        private class ByMassToCharge : IComparer<ClusterPeak>
        {
            public int Compare(ClusterPeak x, ClusterPeak y)
            {
                return x.MZ.CompareTo(y.MZ);
            }
        }

        public int CompareTo(ClusterPeak other)
        {
            return this.Mass.CompareTo(other.Mass);
        }

    }

    public class ClusterSimilarityComparer : IEqualityComparer<Cluster>
    {
        public bool Equals(Cluster x, Cluster y)
        {
            var diff = Math.Abs(x.MonoMass - y.MonoMass);

            return ((diff < 5) && ((diff - Math.Floor(diff)) < 0.06));
        }

        public int GetHashCode(Cluster obj)
        {
            return obj.GetHashCode();
        }
    }

    class ClusterEqualityComparer : IEqualityComparer<Cluster>
    {
        public bool Equals(Cluster b1, Cluster b2)
        {
            return (b1.Score == b2.Score && b1.Peaks.Count == b2.Peaks.Count && b1.MonoMZ == b2.MonoMZ && b1.Z == b2.Z);
        }


        public int GetHashCode(Cluster obj)
        {
            int hCode = (int)obj.MonoMZ * 1000000 + obj.Z * 10000000;
            return hCode.GetHashCode();

        }
    }

    [XmlType("Cluster")]
    [Serializable]
    public class Cluster
    {
        public static int option;

        public int Z { get; set; }

        //[OnDeserialized]
        //internal void OnSerializedMethod(StreamingContext context)
        //{
        //    // Setting this as parent property for Child object

        //    //Child.Parent = this;
        //}


        //[XmlIgnore]
        public ListOfClusterPeak Peaks { get; set; }
        public float IsoScale { get; set; }

        [XmlIgnore]
        public float SecondaryIsoScale { get; set; }
        public float Score { get; set; }

        public string Activation { get; set; }

        public int[] ConsolidatedCharges { get; set; }

        [XmlIgnore]
        public bool SetMonoMassforunittests { get; set; }

        [XmlIgnore]
        public double MonoMassforunittests { get; set; }

        double _monomass;

        public double MonoMass
        {
            get
            {
                if (_monomass != null && _monomass != 0.0)
                {
                    return _monomass;
                }

                if (!SetMonoMassforunittests)
                {
                    if (this.MonoMZ < 0)
                        Debug.Print("BAD!");

                    return (this.MonoMZ * (double)Z) - ((double)Z * SignalProcessor.ProtonMass);
                }
                else
                {
                    return MonoMassforunittests;
                }
            }
            set
            {
                _monomass = value;
            }
        }

        double? _secondarymonomass = 0;

        [XmlIgnore]
        //[JsonIgnore]
        public double? SecondaryMonoMass
        {
            get
            {
                if (_secondarymonomass != null && _secondarymonomass != 0)
                    return _secondarymonomass;

                return (this.SecondMonoMZ * (double)Z) - ((double)Z * SignalProcessor.ProtonMass);
            }
            set
            {
                _secondarymonomass = value;
            }
        }
        //public double SecondaryMono { get { return } }

        float _intensity;

        [XmlIgnore]
        public float Intensity
        {
            get
            {
                if (_intensity != null && _intensity != 0)
                    return _intensity;
                return Peaks.Where(p => p.Delta < 0.01).Sum(p => p.Intensity);
            }
            set
            {
                _intensity = value;
            }
        }

        string _description = null;

        [XmlIgnore]
        public string Description
        {
            get
            {
                return _description ?? "MZ = " + MonoMZ.ToString("0.0000") + "\nZ = " + Z.ToString();
            }

            set
            {
                _description = value;
            }
        }

        private double[] _isoPattern = null;

        [XmlIgnore]
        //[JsonIgnore]
        public double[] IsoPattern
        {
            get
            {
                if (_isoPattern != null)
                {
                    return _isoPattern;
                }
                else if (_xmlisoPattern != string.Empty)
                {
                    return GetDoubles(Convert.FromBase64String(XMLisoPattern));
                }
                else
                {
                    return _isoPattern;
                }
            }
            set
            {
                _isoPattern = value;

            }
        }


        private string _xmlisoPattern = string.Empty;

        //[XmlElement("IsoPattern")]
        public string XMLisoPattern
        {
            get
            {
                if (_xmlisoPattern != string.Empty)
                {
                    return _xmlisoPattern;
                }
                else
                {
                    return Convert.ToBase64String(GetBytes(_isoPattern));
                }
            }
            set
            {
                _xmlisoPattern = value;
            }
        }

        /// <summary>
        /// Convert array of double to array of bytes
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public byte[] GetBytes(double[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }


        /// <summary>
        /// Convert array of bytes to array of doubles
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public double[] GetDoubles(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(double))
                .Select(offset => BitConverter.ToDouble(bytes, offset * sizeof(double)))
                .ToArray();
        }


        private double? _SmonoMZ = -1;

        private int _monoOffset = int.MaxValue;
        public int MonoOffset
        {
            get
            {
                return _monoOffset;
            }

            set
            {
                _monoOffset = value;
            }
        }

        private int _secondaryMonoOffset = int.MaxValue;

        [XmlIgnore]
        //[JsonIgnore]
        public int SecondaryMonoOffset
        {
            get
            {
                return _secondaryMonoOffset;
            }

            set
            {
                _secondaryMonoOffset = value;
            }
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

        [XmlIgnore]
        //[JsonIgnore]

        private static IDictionary<int, double[]> AveragineCache; 

        private static object lockvar = new object();

        /// <summary>
        /// Round to the tens place
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int RoundOff(double i)
        {
            return ((int)Math.Round(i / 10.0)) * 10;
        }


        private ConcurrentDictionary<int, double[]> LoadAveragineCache(int maxMass = 60000, int interval = 10)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var avgCacheFilename = System.IO.Path.Combine(appData, "avgCache.dat");

            FileStream fs;
            ConcurrentDictionary<int, double[]> cache = null;

            if (File.Exists(avgCacheFilename))
            {
                //Deserilaize object            
                using (fs = new FileStream(avgCacheFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var Debf = new BinaryFormatter();
                    cache = Debf.Deserialize(fs) as ConcurrentDictionary<int, double[]>;
                }
            }


            if (cache == null)
            {
                // populate cache (takes 6-20 seconds, depending on CPU!)
                var prlop = new ParallelOptions();
                prlop.MaxDegreeOfParallelism = Environment.ProcessorCount;

                cache = new ConcurrentDictionary<int, double[]>();

                Parallel.For(1, prlop.MaxDegreeOfParallelism + 1, prlop, index => {
                    for (int i = index; i <= maxMass/interval; i += prlop.MaxDegreeOfParallelism)
                    {
                        var ion = GenerateAveragine(i * interval);
                        var pattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(ion, 1f);
                        var isoPattern = pattern.Select(p => (double)p.Value).ToArray();
                        cache.TryAdd(i * interval, isoPattern);
                    }
                });


                //cache = new SortedList<int, double[]>();

                //for (int i = 10; i <= 30000; i += 10)
                //{
                //    var ion = GenerateAveragine(i);

                //    var pattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(ion, 1f);

                //    var isoPattern = pattern.Select(p => (double)p.Value).ToArray();
                //    cache.Add(i, isoPattern);
                //}

                //Serilaize object -- Save the cache for quick load next time.  

                using (fs = new FileStream(avgCacheFilename, FileMode.Create))
                {
                    try
                    {
                        var bf = new BinaryFormatter();
                        bf.Serialize(fs, cache);
                    }
                    catch (SerializationException ex)
                    {
                        // failing to serialize is not fatal, just slows startup performance.  So don't throw anything.  
                        Debug.Print(ex.Message);
                    }
                }
            }

            return cache;
        }


        /// <summary>
        /// Finds the Monoisotopic M/Z and Core Peaks and sets the Cluster properties accordingly.  
        /// </summary>
        /// <returns></returns>
        public double SetMonoAndCorePeaks()
        {



            //System.Diagnostics.Debug.WriteLine("Executing FindMono");

            //double mass = cluster.Peaks.Where(p => p.IsCorePeak).OrderByDescending(p => p.Intensity).First().MZ * cluster.Z;

            if (this.Peaks.Count <= 2) return this.MonoMZ;

            double mass = this.Peaks.Skip(2).First().Mass;  //.Where(p => p.IsCorePeak).OrderByDescending(p => p.Intensity).First().MZ * cluster.Z;

            AveragineCache = AveragineCache ?? LoadAveragineCache();

            double[] isoPattern;

            //TODO: Fix AverageCache  exception: Collection was modified after the enumerator was instantiated.




            // Retrieve or generate predicted Averagine isotope pattern for this mass

                // Lookup value in Averagine Cache
                //var ip = ;

                if (AveragineCache.ContainsKey(RoundOff(mass)))
                {
                    //var ip = AveragineCache.Where(m => m.Key == RoundOff(mass)).First();

                    isoPattern = AveragineCache[RoundOff(mass)];
                }
                else
                {

                    // To prevent using a ton of memory for the averaginecache, we have to trim it when it has a signicant number of values
                    //if (AveragineCache.Count > 100) AveragineCache = new SortedDictionary<double, double[]>(AveragineCache.Skip(AveragineCache.Count - 50).ToDictionary(k => k, v => v));
                    //if (AveragineCache.Count > 100) AveragineCache.Clear();

                    //var start = DateTime.Now;

                    var theoIon = GenerateAveragine(RoundOff(mass));

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


                    AveragineCache.Add(RoundOff(mass), isoPattern);
                }

            

            this.IsoPattern = isoPattern;

            // Find the Monoisotopic peak

            // Use Top 10 peaks in the iso pattern
            int isoStart = Math.Max(0, this.IsoPattern.MaxIndex() - 5);
            int isoEnd = Math.Min(this.IsoPattern.Length - 1, this.IsoPattern.MaxIndex() + 5);

            //var ratio = cluster.Peaks.Max(p => p.Intensity);

            var bestAlignmentIndex = 0;
            var nextBestAlignmentIndex = 0;
            var bestIntensityScaleIndex = 0;
            var bestDifferential = double.MinValue;
            var nextBestDifferential = double.MinValue;
            var bestScale = 0f;
            var nextBestScale = 0f;

            for (int p = 0; p < (this.Peaks.Count - (isoEnd - isoStart)) - isoStart; p++)
            {
                //int count = 0;

                for (int i = isoStart; i <= isoEnd; i++)
                {
                    // i is the index of the peak to use for scaling
                    //cluster.Peaks[p]

                    if (isoPattern[i] < 0.20) continue;  // But, don't scale the entire pattern to a relatively weak peak...

                    float scale = (float)(this.Peaks[p + i].Intensity / isoPattern[i]);

                    if (scale <= 0) continue;

                    var differential = 0d;

                    for (int j = isoStart; j <= isoEnd; j++)
                    {
                        // Now we enumerate over the peaks with peak i as the fit scale

                        var predictedIntensity = isoPattern[j] * scale;

                        if (predictedIntensity > this.Peaks[j + p].Intensity)
                            differential += this.Peaks[j + p].Intensity - ((predictedIntensity - this.Peaks[j + p].Intensity) * 2);  // penalty for missing signal -- this will amplify the difference
                        else
                            differential += predictedIntensity;
                    }

                    //differential /= scale;

                    if (differential > bestDifferential)
                    {
                        // we have a better fit! 

                        if (bestAlignmentIndex != p)
                        {
                            // save the previous best if it resulted in a different mono mass pick
                            nextBestAlignmentIndex = bestAlignmentIndex;
                            nextBestScale = bestScale;
                            nextBestDifferential = bestDifferential;
                        }

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

            //Debug.Print("MonoMass is close to " + this.Peaks[bestAlignmentIndex].Mass);

            //            cluster.MonoMZ = cluster.Peaks[bestAlignmentIndex].;

            this.IsoScale = bestScale;
            this.MonoOffset = bestAlignmentIndex;

            if (nextBestDifferential - bestDifferential < 1)
            {
                this.SecondaryMonoOffset = nextBestAlignmentIndex;
                this.SecondaryIsoScale = nextBestScale;
            }





            if (this.Peaks.Count <= 0) return this.Peaks[bestAlignmentIndex].MZ;

            // Calculate an Accurate Monoisotopic Mass value by making use of all info available -- Intensity fit (through core peak flag), Weight by core peak            
            // Back calculate the Mono by using an intensity weighted average of the Mono calculation of each of the core peaks.
            var weightedMassSum = 0d;
            var weightSum = 0d;

            int offset = 0;
            float clusterScale = 0;

            for (byte i = 2; i > 0; i--)
            {
                if (i == 1)
                {
                    offset = this.MonoOffset;
                    clusterScale = this.IsoScale;
                }
                else
                {
                    offset = this.SecondaryMonoOffset;
                    clusterScale = this.SecondaryIsoScale;
                }

                weightedMassSum = 0d;
                weightSum = 0d;


                // Find the Core Peaks of the Isotope Pattern and MonoMass by weighted average
                foreach (var aPeak in this.Peaks)
                {
                    aPeak.IsCorePeak = false;

                    // Find CorePeaks - peaks close to the apex of the predicted pattern and have actual intensity of at least 30% of max intensity predicted                    
                    if (this.IsoPattern != null && clusterScale > 0 && (this.IsoPattern.Length > aPeak.Index - offset) && (0 <= aPeak.Index - offset))
                    {
                        //TODO: include a mass tolerance criteria as well
                        var predictedIntensity = this.IsoPattern[aPeak.Index - offset] * clusterScale;
                        aPeak.IsCorePeak = this.IsoPattern[aPeak.Index - offset] > 0.2 && aPeak.Intensity > predictedIntensity * 0.5 && ((aPeak.Intensity < predictedIntensity * 2) || (aPeak.Delta < (PPM.CurrentPPM(aPeak.Mass) / 10) && aPeak.Intensity < predictedIntensity * 4));

                        // calculate weighted average figures
                        if (aPeak.IsCorePeak)
                        {
                            weightedMassSum += (aPeak.MZ - ((SignalProcessor.ProtonMass / (double)aPeak.Z) * (aPeak.Index - offset))) * aPeak.Intensity;
                            weightSum += aPeak.Intensity;
                            aPeak.Parent = (this);
                        }
                    }
                    //else
                    //{

                    ////    return false;
                    //}
                }

                if (i == 1)
                    _monoMZ = weightedMassSum / weightSum;
                else
                    _SmonoMZ = weightedMassSum / weightSum;
            }

            return this._monoMZ;
        }

        public void FindCorePeaks()
        {
            if (this.Peaks.Count <= 0) return;

            _monoMZ = this.FindMonoMzWithMin();



            foreach (var aPeak in this.Peaks)
            {

                // Find CorePeaks - peaks close to the apex of the predicted pattern and have actual intensity of at least 30% of max intensity predicted                    
                if (this.IsoPattern != null && this.IsoScale > 0 && (this.IsoPattern.Length > aPeak.Index - this.MonoOffset) && (0 <= aPeak.Index - this.MonoOffset))
                {
                    //TODO: include a mass tolerance criteria as well
                    var predictedIntensity = this.IsoPattern[aPeak.Index - this.MonoOffset] * this.IsoScale;
                    aPeak.IsCorePeak = this.IsoPattern[aPeak.Index - this.MonoOffset] > 0.2 && aPeak.Intensity > predictedIntensity * 0.5 && aPeak.Intensity < predictedIntensity * 1.5;
                }
                //else
                //{
                //    return false;
                //}
            }

            //var minRatio = this.IsoPattern.

            //// find predicted apex of pattern, then find peaks that fit the model best
            //foreach (var anIsoPeak in this.Peaks.Where(p => this.IsoPattern[p.IsoIndex] >  )
            //{

            //}





            //this.MonoOffset



        }


        public double _monoMZ = -1;
        public double MonoMZ
        {
            get
            {
                if (_monoMZ == -1 && Peaks != null && Peaks.Count > 2) _monoMZ = this.SetMonoAndCorePeaks(); //this.FindMonoMzWithMin(); //this.FindMono();
                return _monoMZ;
            }
            set
            {
                _monoMZ = value;
            }
        }

        double? _secondmonomz = 0;

        [XmlIgnore]
        //[JsonIgnore]
        public double? SecondMonoMZ
        {
            get
            {
                if (_secondmonomz != null && _secondmonomz != 0)
                    return _secondmonomz;

                if (_SmonoMZ == -1 && Peaks != null && Peaks.Count > 2) _SmonoMZ = SignalProcessor.SecondMono;
                return _SmonoMZ;
            }
            set
            {
                _secondmonomz = value;
            }
        }

        //private double FindMono()
        //{
        //    throw new NotImplementedException();
        //}

        public override string ToString() { return MonoMZ.ToString() + ", " + Z.ToString() + ", Score = " + Score.ToString() + ", MonoMass = " + MonoMass.ToString(); }

        public static bool SimilarClusters(Cluster x, Cluster y)
        {
            foreach (var aPeak in x.Peaks.Where(p => p.IsCorePeak))
            {
                if (y.Peaks.Where(q => q.IsCorePeak && Math.Abs(q.Mass - aPeak.Mass) < PPM.CurrentPPM(aPeak.Mass)).Any()) return true;
            }

            return false;
        }

        public static bool SimilarClustersIncludingCharge(Cluster x, Cluster y)
        {
            foreach (var aPeak in x.Peaks.Where(p => p.IsCorePeak))
            {
                if (y.Peaks.Where(q => q.IsCorePeak && q.Z == aPeak.Z && Math.Abs(q.Mass - aPeak.Mass) < PPM.CurrentPPM(aPeak.Mass)).Any()) return true;
            }

            return false;
        }
    }
}

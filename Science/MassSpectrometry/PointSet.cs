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

namespace SignalProcessing
{
    /// <summary>
    /// A Collection of points that is optimized for storing and manipulating Mass Spectra and Chromatograms
    /// </summary>
    public class PointSet : SortedList<double, float>, INotifyCollectionChanged
    {
        public bool IsPopulated { get; private set; }

        public PointSet(IEnumerable<double> keys, IEnumerable<float> values)
        {
            Populate(keys, values);
        }

        public PointSet(IEnumerable<double> keys, IEnumerable<double> values)
        {
            Populate(keys, values);
        }

        //public PointSet(double[] keys, float[] values)
        //{

        //    var valueEnumerator = values.GetEnumerator();
        //    valueEnumerator.Reset();
        //    valueEnumerator.MoveNext();

        //    foreach (double aKey in keys)
        //    {
        //        this.AddWithoutEvents(aKey, (float)valueEnumerator.Current);
        //        valueEnumerator.MoveNext();
        //    }

        //    RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
        //}

        //public PointSet(double[] keys, double[] values)
        //{

        //    var valueEnumerator = values.GetEnumerator();
        //    valueEnumerator.Reset();
        //    valueEnumerator.MoveNext();

        //    foreach (double aKey in keys)
        //    {
        //        this.AddWithoutEvents(aKey, (float)valueEnumerator.Current);
        //        valueEnumerator.MoveNext();
        //    }

        //    RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
        //}

        public PointSet()
            : base()
        {
            IsPopulated = false;
        }

        public void Populate(IEnumerable<double> keys, IEnumerable<double> values)
        {

            var valueEnumerator = values.GetEnumerator();
            valueEnumerator.MoveNext();
            //valueEnumerator.Reset();

            foreach (double aKey in keys)
            {
                this.AddWithoutEvents(aKey, (float)valueEnumerator.Current);
                valueEnumerator.MoveNext();
            }

            IsPopulated = true;

            RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
        }

        public void Populate(IEnumerable<double> keys, IEnumerable<float> values)
        {

            var valueEnumerator = values.GetEnumerator();
            valueEnumerator.MoveNext();
            //valueEnumerator.Reset();

            foreach (double aKey in keys)
            {
                this.AddWithoutEvents(aKey, valueEnumerator.Current);
                valueEnumerator.MoveNext();
            }

            IsPopulated = true;

            RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
        }

        public new void Add(double key, float value)
        {
            //if (double.IsNaN(key) || float.IsNaN(value) || base.ContainsKey(key)) return;
            if (base.ContainsKey(key)) return;

            AddWithoutEvents(key, value);

            //base.Add(key, value);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
            //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,  ));
        }



        public void AddWithoutEvents(double key, float value)
        {
            if (double.IsNaN(key) || float.IsNaN(value)) return;

            if (base.ContainsKey(key))
                base[key] += value;
            else
                base.Add(key, value);

            //RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
            //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,  ));
        }



        public event NotifyCollectionChangedEventHandler CollectionChanged;

        //protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        //{
        //    if (CollectionChanged != null)
        //    {
        //        CollectionChanged(this, e);
        //    }
        //}

        internal void RaiseCollectionChanged(NotifyCollectionChangedAction action)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
        }

        private string m_XAxisUnits = "X";
        private string m_YAxisUnits = "Y";
        private string m_Title = "X-Y Plot";

        public virtual string XAxisUnits
        {
            get { return m_XAxisUnits; }
            set { m_XAxisUnits = value; }
        }

        public virtual string YAxisUnits
        {
            get { return m_YAxisUnits; }
            set { m_YAxisUnits = value; }
        }

        public virtual string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        //public static PointSet operator +(PointSet a, PointSet b)
        //{
        //    PointSet temp = new PointSet();
        //    foreach


        //}

        public IEnumerable<KeyValuePair<double, float>> Range(double startkey, double endkey, int numBins)
        {
            return Range(startkey, endkey, numBins, 0);
        }


        /// <summary>
        /// Get the range of elements between the start and end specified (inclusive of the endpoints)
        /// </summary>
        /// <param name="startkey">Starting Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="endkey">Ending Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="numBins">Number of bins</param>
        /// <returns>An Enumerable of the Key/Value Pairs</returns>
        public IEnumerable<KeyValuePair<double, float>> Range(double startkey, double endkey, int numBins, float valueMin = 0)
        {
            var window = this.Range(startkey, endkey);
            var result = new List<KeyValuePair<double, float>>();

            if (window == null) return result;

            var mostIntensePointInBin = window.First();

            double binsize = (endkey - startkey) / numBins;
            double binlimit = mostIntensePointInBin.Key + binsize;

            foreach (var aPoint in window)
            {
                if (aPoint.Key > binlimit)
                {
                    if (mostIntensePointInBin.Value > valueMin) result.Add(mostIntensePointInBin);
                    mostIntensePointInBin = aPoint;
                    while (binlimit < aPoint.Key) binlimit += binsize;
                }
                else
                {
                    mostIntensePointInBin = (aPoint.Value > mostIntensePointInBin.Value) ? aPoint : mostIntensePointInBin;
                }
            }

            result.Add(mostIntensePointInBin);

            return result;
        }

        /// <summary>
        /// Get the range of elements between the start and end specified (inclusive of the endpoints)
        /// </summary>
        /// <param name="startkey">Starting Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="endkey">Ending Key value (will automatically use the closest if the specified value does not exist</param>
        /// <returns>An Enumerable of the Key/Value Pairs</returns>
        public IEnumerable<KeyValuePair<double, float>> Range(double startkey, double endkey)
        {
            return this.Range(startkey, endkey, true);
        }

        /// <summary>
        /// Get the range of elements between the start and end specified
        /// </summary>
        /// <param name="startkey">Starting Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="endkey">Ending Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="includeEnds">Whether or not to include the start and end keys in the range returned</param>
        /// <returns>An Enumerable of the Key/Value Pairs</returns>
        public IEnumerable<KeyValuePair<double, float>> Range(double startkey, double endkey, bool includeEnds)
        {
            //using the Binary Search, find the closest value and use that!
            double? actualStartKey = startkey, actualEndKey = endkey;

            try
            {
                if (!this.ContainsKey(startkey))
                {
                    actualStartKey = this.FindClosestInRange(startkey, startkey, endkey);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(this.Count + " Points in scan " + this.Title + " Message1: " + ex.Message + ", startkey = " + startkey + ", endkey = " + endkey + ", keys: " + string.Join(", ", this.Where(k => k.Key >= startkey).Select(kvp => kvp.Key.ToString())));

            }

            try
            {
                if (!this.ContainsKey(endkey)) actualEndKey = this.FindClosestInRange(endkey, startkey, endkey);
            }
            catch (Exception ex)
            {
                throw new Exception(this.Count + " Points in scan " + this.Title + " Message2: " + ex.Message + ", startkey = " + startkey + ", endkey = " + endkey + ", keys: " + string.Join(", ", this.Where(k => k.Key >= startkey).Select(kvp => kvp.Key.ToString())));
            }

            if (actualStartKey.HasValue || actualEndKey.HasValue)
            {
                actualStartKey = actualStartKey ?? actualEndKey;
                actualEndKey = actualEndKey ?? actualStartKey;
                return this.Range(this.IndexOfKey(actualStartKey.Value), this.IndexOfKey(actualEndKey.Value), includeEnds);
            }


            return new List<KeyValuePair<double, float>>();  // there are no points in that range!
        }


        /// <summary>
        /// Get the range of elements between the start and end specified
        /// </summary>
        /// <param name="startIndex">Starting Index</param>
        /// <param name="endIndex">Ending Index</param>
        /// <param name="includeEnds">Whether or not to include the start and end keys in the range returned</param>
        /// <returns>An Enumerable of the Key/Value Pairs</returns>
        private IEnumerable<KeyValuePair<double, float>> Range(int startIndex, int endIndex, bool includeEnds)
        {
            if (this.Count <= 0) throw new Exception("Empty Pointset");

            int increment = Math.Sign(endIndex - startIndex);

            if (startIndex == endIndex && includeEnds)
            {
                yield return new KeyValuePair<double, float>(this.Keys[startIndex], this[this.Keys[startIndex]]);
            }
            else
            {
                for (int i = startIndex; i != (endIndex + increment); i += increment)
                    if (includeEnds || (i != startIndex && i != endIndex)) yield return new KeyValuePair<double, float>(this.Keys[i], this[this.Keys[i]]);
            }
            //TODO: There must be a better way than to create a NEW KeyValuePair!  
        }

        /// <summary>
        /// Get the range of elements between the start and end specified (inclusive of the endpoints) and sums the intensities within the bins
        /// Note: bins with a 0 intensity are included!
        /// </summary>
        /// <param name="startkey">Starting Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="endkey">Ending Key value (will automatically use the closest if the specified value does not exist</param>
        /// <param name="numBins">Number of bins</param>
        /// <returns>An Enumerable of the Key/Value Pairs</returns>
        public IEnumerable<KeyValuePair<double, float>> RangeSum(double start, double end, double binSize)
        {
            var window = this.Range(start, end);
            var result = new Dictionary<double, Single>();

            if (window == null) return result;

            var mostIntensePointInBin = window.First();

            double binlimit = start + binSize;
            int binCount = (int)((end - start) / binSize);

            var points = window.GetEnumerator();

            foreach (var aBin in Enumerable.Range(0, binCount))
            {
                result.Add(aBin * binSize, 0);

                while (points.Current.Key < (start + ((aBin + 1) * binSize)))
                {
                    result[aBin * binSize] += points.Current.Value;  // add to the current bin
                    if (!points.MoveNext()) break;
                }

            }

            return result;
        }

        private float _LastMaxValue = float.NaN;

        public float MaxValue
        {
            get
            {
                if (float.IsNaN(_LastMaxValue)) _LastMaxValue = this.Values.Max();

                return _LastMaxValue;
            }
        }

        public string Text
        {
            get
            {
                return string.Join("\n\r", this.Select(kvp => kvp.Key.ToString() + ", " + kvp.Value.ToString()));
            }
        }

        /// <summary>
        /// Gets the Max Key without scanning the whole list!  We can do this because we know the list is sorted.
        /// </summary>
        /// <returns></returns>
        public double MaxKey
        {
            get { return this.Keys[this.Count - 1]; }
        }

        /// <summary>
        /// Gets the Min Key without scanning the whole list!  We can do this because we know the list is sorted.
        /// </summary>
        /// <returns></returns>
        public double MinKey
        {
            get { return this.Keys[0]; }
        }


        /// <summary>
        /// Get the Max Y value of all points between the specified start and end
        /// </summary>
        /// <param name="start">The starting X Value</param>
        /// <param name="end">The ending X Value</param>
        /// <returns>The Y Value itself</returns>
        public double? GetMaxYValueForXRange(double start, double end)
        {
            var key = GetKeyOfMaxYValueForXRange(start, end);

            if (!key.HasValue) return null;

            return this[key.Value];
        }

        /// <summary>
        /// Get the X Value associated with the Max Y value of all points between the specified start and end
        /// </summary>
        /// <param name="start">The starting X Value</param>
        /// <param name="end">The ending X Value</param>
        /// <returns>The X Key or null if not found</returns>
        public double? GetKeyOfMaxYValueForXRange(double start, double end)
        {
            double? startkey = start, endkey = end;

            // if the start and end are not in the collection, get the closest value that is within the range
            if (!this.ContainsKey(start)) startkey = this.FindClosestInRange(start, start, end);
            if (!this.ContainsKey(end)) endkey = this.FindClosestInRange(end, start, end);

            if (!startkey.HasValue || !endkey.HasValue) return null;

            //if (!this.ContainsKey(start)) start = this.FindClosestInRange(start, start, end);
            //if (!this.ContainsKey(end)) end = this.FindClosestInRange(end, start, end);

            //double max = this[start];
            double keyOfMax = startkey.Value;

            foreach (var aPoint in this.Range(startkey.Value, endkey.Value))
                if (this[keyOfMax] < aPoint.Value) keyOfMax = aPoint.Key;

            return keyOfMax;
        }

        /// <summary>
        /// Calculate the Area under the Curve between the start and end X axis values.  
        /// Note: This is tolerant to reversal of start and end points
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public double CalculateArea(double start, double end)
        {
            // Calculate Area under the Curve and add to the PeakAreas list

            var rangePoints = from p in this
                              where p.Key >= Math.Min(start, end) && p.Key <= Math.Max(start, end)
                              orderby p.Key ascending
                              select p;

            if (rangePoints.Count() <= 0) return 0;

            var lastpoint = rangePoints.First();
            double area = 0;

            foreach (var aPoint in rangePoints.Skip(1))
            {

                area += Math.Min(aPoint.Value, lastpoint.Value) * Math.Abs(aPoint.Key - lastpoint.Key);  // rectangle area
                area += 0.5 * (Math.Max(aPoint.Value, lastpoint.Value) - Math.Min(aPoint.Value, lastpoint.Value)) * Math.Abs(aPoint.Key - lastpoint.Key);  // triangle area

                lastpoint = aPoint;
            }

            return area;
        }


        /// <summary>
        /// Find the closest X value to the target passed in using a binary search.  
        /// </summary>
        /// <param name="target">Target X Value</param>
        /// <returns>Closest X Value</returns>
        public double FindClosest(double target)
        {
            // http://www.brpreiss.com/books/opus6/html/page452.html

            if (this.ContainsKey(target)) return target;

            int left = 0, middle = 0, right = this.Count - 1;

            while (left <= right)
            {
                middle = (left + right) / 2;
                if (target > this.Keys[middle])
                    left = middle + 1;
                else if (target < this.Keys[middle])
                    right = middle - 1;
                else
                    return this.Keys[middle];
            }

            // Validate bounds!
            left = Math.Min(left, (this.Keys.Count - 1));
            right = Math.Max(right, 0);

            if (Math.Abs(target - this.Keys[left]) > Math.Abs(target - this.Keys[right]))
                return this.Keys[right];
            else
                return this.Keys[left];
        }


        /// <summary>
        /// Find the closest X value to the target passed in using a binary search.  
        /// </summary>
        /// <param name="target">Target X Value</param>
        /// <returns>Closest X Value</returns>
        public double? FindClosestInRange(double target, double start, double end)
        {
            // http://www.brpreiss.com/books/opus6/html/page452.html
            try
            {
                if (this.ContainsKey(target)) return target;

                if (this.Keys.Any() == false) return null;

                if (target < start || target > end) throw new IndexOutOfRangeException();

                int left = 0, middle = 0, right = this.Count - 1;

                while (left <= right)
                {
                    middle = (left + right) / 2;
                    if (target > this.Keys[middle])
                        left = middle + 1;
                    else if (target < this.Keys[middle])
                        right = middle - 1;
                    else
                        return this.Keys[middle];
                }

                // Validate bounds!
                left = Math.Min(left, (this.Keys.Count - 1));
                right = Math.Max(right, 0);

                if (this.Keys[right] >= start && this.Keys[right] <= end && this.Keys[left] >= start && this.Keys[left] <= end)
                {
                    // both points are in the specified range, so pick the closest to the target
                    if (Math.Abs(target - this.Keys[left]) > Math.Abs(target - this.Keys[right]))
                        return this.Keys[right];
                    else
                        return this.Keys[left];
                }
                else if (this.Keys[right] >= start && this.Keys[right] <= end)
                {
                    // left is out of range, so there must only be a single point in range...
                    return this.Keys[right];
                }
                else if (this.Keys[left] >= start && this.Keys[left] <= end)
                {
                    // right is out of range, so there must only be a single point in range...
                    return this.Keys[left];
                }
                else
                {
                    // no points found in range at all
                    return null;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Find Closest In Range" + ex.Message + "  " + ex.StackTrace + "  " + ex.InnerException);
            }


        }
    }

    public static class EnumerableExtensions
    {
        public static PointSet ToPointSet<TSource>(this IEnumerable<TSource> source, global::System.Func<TSource, double> keySelector, global::System.Func<TSource, float> elementSelector, string title = "X-Y Plot")
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }
            var dictionary = new PointSet();
            foreach (TSource local in source.AsEnumerable())
            {
                dictionary.Add(keySelector(local), elementSelector(local));
            }

            dictionary.Title = title;

            return dictionary;
        }

        public static void Sum(this PointSet target, IList<KeyValuePair<double, float>> source)
        {
            foreach (var aPoint in source)
                target.TrySum(aPoint.Key, aPoint.Value);

            target.RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
            //return target;
        }
    }

    public static class Extensions
    {
        //public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> lookup, TKey key, TValue value)
        //{
        //    if (!lookup.ContainsKey(key)) lookup.Add(key, value);
        //}

        public static void TrySum(this PointSet lookup, double key, float value)
        {
            if (!lookup.ContainsKey(key))
                lookup.AddWithoutEvents(key, value);
            else
                lookup[key] += value;
        }

    }
}

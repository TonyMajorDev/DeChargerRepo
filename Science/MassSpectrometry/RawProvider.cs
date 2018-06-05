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
using SignalProcessing;
using System.IO;
using ThermoFisher.Foundation.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;

namespace MassSpectrometry
{

    /// <summary>
    /// Thermo RAW data file format handler
    /// </summary>
    public class RawProvider
    {
        internal FileInfo _filePath = null;
        internal RawFile _rawFile = null;
        internal SpectrumCollection _scans = null;

        public RawProvider(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                _filePath = new FileInfo(FilePath);
            }
        }

        public string WhyBadFile 
        { 
            get 
            { 
                if (_filePath == null) return "Path to data file is not set.  ";
                _filePath.Refresh();
                if (!_filePath.Exists) return "File does not exist.  ";
                
                if (_rawFile == null) 
                {
                    _rawFile = new RawFile(_filePath.FullName);
                    _rawFile.SelectInstrument(InstrumentType.MassSpec, 1);
                    if (_scans != null) _scans.Clear();
                }

                if (!_rawFile.IsThereMSData) return "There is no MS Data.  The file is likely bad.  " + ((_filePath.Length == 0) ? "Also, the file size is 0 bytes.  " : string.Empty);

                return string.Empty;

                //var x = new ThermoFisher.Foundation.IO.XcaliburRangeSelector();

                
            }
        }

        public SpectrumCollection Scans
        {
            get
            {
                if (_scans == null) _scans = new SpectrumCollection(this);

                return _scans;
            }
        }

        /// <summary>
        /// Free up RAM by deleting the points from the cache.  
        /// </summary>
        public void ClearPoints()
        {
            foreach (Spectrum aScan in _scans) aScan.Clear();
        }

        public RunInfo Info
        {
            get
            {
                if (_rawFile == null)
                {
                    _rawFile = new RawFile(_filePath.FullName);
                    _rawFile.SelectInstrument(InstrumentType.MassSpec, 1);
                    Scans.Clear();
                }

                return new RunInfo(_rawFile.GetRunHeader());
            }
        }


        /// <summary>
        /// Executes a spectrum average using the Thermo Foundation
        /// </summary>
        /// <param name="startScan"></param>
        /// <param name="endScan"></param>
        /// <param name="MsOrderFilter"></param>
        /// <param name="startMass"></param>
        /// <param name="endMass"></param>
        /// <returns></returns>
        public PointSet Average(int startScan, int endScan, int MsOrderFilter = 1, double startMass = double.NaN, double endMass = double.NaN)
        {

            var y = new ThermoFisher.Foundation.IO.SpecListAverager();

            var sf = new ScanFilter() { Detector = ScanEventBase.DetectorType.Valid, MassAnalyzer = ScanEventBase.MassAnalyzerType.MassAnalyzerFTMS, MSOrder = ScanEventBase.MSOrderType.MS };

            var list = SpecList.FromSpecList(_rawFile, startScan, endScan, sf, true, true, true);

            y.AddScansToArray(list);
            y.AverageScans(_rawFile, false, 2, XcaliburToleranceUnit.XUnitsPpm);

            var result3 = y.GetAverageLabelData();

            if (double.IsNaN(startMass) || double.IsNaN(endMass))
                return new PointSet(result3.MassArray, result3.IntensityArray);

            var spec = new PointSet();

            for (int i = 0; i < result3.Length; i++)
            {
                if (result3.MassArray[i] > startMass && result3.MassArray[i] < endMass)
                {
                    spec.AddWithoutEvents(result3.MassArray[i], (float)result3.IntensityArray[i]);
                }
            }

            return spec;

        }

        
        /// <summary>
        /// Generate an XIC
        /// </summary>
        /// <param name="startMZ"></param>
        /// <param name="endMZ"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public Chromatogram ExtractIon(double startMZ, double endMZ, float startTime = 0, float endTime = float.MaxValue)
        {
            if (_rawFile == null)
            {
                _rawFile = new RawFile(_filePath.FullName);
                _rawFile.SelectInstrument(InstrumentType.MassSpec, 1);
            }

            var settings = new ChromatogramSettings() { Trace = ChromatogramSettings.TraceType.MassRange };

            settings.MassRangeCount = 1;
            settings.SetMassRange(0, new ThermoFisher.Foundation.IO.Range(startMZ, endMZ));

            settings.Filter = new ScanFilter() { MSOrder = ScanEventBase.MSOrderType.MS };

            var chromData = this._rawFile.GetChromatogramData(new[] { settings });

            return new Chromatogram(chromData.PositionsArray[0], chromData.IntensitiesArray[0]);
        }

        public Chromatogram ExtractIon(List<SignalProcessing.Range> mzRangeList, float startTime = 0, float endTime = float.MaxValue)
        {
            if (_rawFile == null)
            {
                _rawFile = new RawFile(_filePath.FullName);
                _rawFile.SelectInstrument(InstrumentType.MassSpec, 1);
            }

            var settings = new ChromatogramSettings() { Trace = ChromatogramSettings.TraceType.MassRange };

            settings.MassRangeCount = mzRangeList.Count;

            for (int i = 0; i < mzRangeList.Count; i++ )
                settings.SetMassRange(i, new ThermoFisher.Foundation.IO.Range(mzRangeList[i].Start, mzRangeList[i].End));

            settings.Filter = new ScanFilter() { MSOrder = ScanEventBase.MSOrderType.MS };

            var chromData = this._rawFile.GetChromatogramData(new[] { settings });

            return new Chromatogram(chromData.PositionsArray[0], chromData.IntensitiesArray[0]);
        }


        public void ReleaseFile()
        {
            try
            {
                this._rawFile.Close();
            }
            catch { }
        }
    }

    public partial class RunInfo
    {
        internal RunInfo()
        { }

        internal RunInfo(RUNHEADERINFO info)
        {
            StartTime = info.StartTime;
            EndTime = info.EndTime;
            HighMass = info.HighMass;
            LowMass = info.LowMass;
            FirstScanNum = info.FirstSpectrum;
            LastScanNum = info.LastSpectrum;
        }

    }

    
    
    
    public class Chromatogram : PointSet 
    {
        Chromatogram()
        {
            this.XAxisUnits = "Retention Time (min)";
            this.YAxisUnits = "Intensity";
        }

        public Chromatogram(double[] rt, double[] intensity) : base(rt, intensity) { }
    }

    public class ThermoSpectrum : Spectrum
    {
        static object lockObject = new object();

        SpecPointCollection _pointCache = null;
        RawProvider _parent = null;
        //public Spectrum(RawProvider parent, IEnumerable<double> keys, IEnumerable<double> values) : base(keys, values) { _parent = parent; }
        public ThermoSpectrum(RawProvider parent)
        { 
            _parent = parent; 
        }

        public string Info { get; set; }

        public virtual string OriginFilename { get; internal set; }

        public virtual string ScanType { get; internal set; }

        //public virtual double? TriggerMZ { get; internal set; }

        //public virtual int? ScanNumber { get; internal set; }

        public virtual double? StartMass { get; internal set; }

        public virtual double? EndMass { get; internal set; }

        public virtual double? StartTime { get; internal set; }

        public virtual double? EndTime { get; internal set; }

        //public string Title { get; internal set; }

        /// <summary>
        /// Total Ion Current
        /// </summary>
        public virtual float? IonCurrent { get; internal set; }

        public virtual float? BasePeakIntensity { get; internal set; }

        public virtual float? BasePeakMass { get; internal set; }

        //public virtual string Activation { get; set; }

        //public virtual int? ParentCharge { get; set; }

        //public virtual double? ParentMass { get; set; }

        public SpecPointCollection Points 
        { 
            get
            {
                // All code was moved into the "GetPoints" method because the HandleProcessCorruptedStateExceptions attribute can only be used on a method, not a property :(
                return GetPoints();
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private SpecPointCollection GetPoints()
        {

                if (_pointCache == null)
                {

                    // Added lock to prevent AccessViolationException on the GetScan Method call
                    lock (lockObject)
                    {
                        string errorMsg = null;
                        int tries = 10;

                        while (TryGetPoints(out errorMsg) == false && tries > 0) tries--;

                        //if (tries != 10) throw new Exception("Tried " + (5 - tries).ToString() + " times.  " + errorMsg);

                        if (tries <= 0 && string.IsNullOrWhiteSpace(errorMsg) == false) throw new Exception(errorMsg);
                    }
                }

                return _pointCache;
        }

        private Boolean TryGetPoints(out string errorMessage)
        {
            errorMessage = null;

            try
            {

                // Hypothesis: This section must be marked as unsafe because the Thermo Foundation will, on certain files, randomly do unsafe (but apparently intentional) memory access.  
                // If the section is not marked as unsafe, reading the points may sometimes throw this exception: Attempted to read or write protected memory.  This is oftem an indication that other memory is corrupt.  
                unsafe
                {
                    var s = _parent._rawFile.GetScan(this.ScanNumber.Value);
                    //var s = _parent._rawFile.GetScan(this.ScanNumber.Value);


                    var labelPeaks = s.GetLabelPeaks();

                    if (labelPeaks.Length > 0)
                    {
                        _pointCache = new SpecPointCollection(labelPeaks); // s.GetLabelPeaks().Cast<SpecPoint>().ToArray();
                    }
                    else
                    {
                        // fallback method for populating scan data
                        //Debug.WriteLine("Using Fallback Method for populating Scan Data");

                        var stats = _parent._rawFile.GetScanStatsForScanNumber(this.ScanNumber.Value);

                        var s2 = _parent._rawFile.GetScanFromScanNumber(this.ScanNumber.Value, stats);

                        _pointCache = new SpecPointCollection(s2.PositionsArray, s2.IntensitiesArray);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                //if ex.Message.Contains("Attempted to read or write protected memory");

                errorMessage = "Caught a Fatal exception when reading the Raw File on scan# " + this.ScanNumber.Value + ", Message: " + ex.Message;

                return false;

                // the rethrown exception should be a catchable one, unlike the dreaded AccessViolationException
                //throw new Exception("Caught a Fatal exception when reading the Raw File on scan# " + this.ScanNumber.Value + ", Message: " + ex.Message, ex);
            }

        }

        internal Dictionary<string, object> extras = new Dictionary<string, object>();
       
        public Dictionary<string, object> ExtraValues
        {
            get { return extras; }
        }        
    }

    //public class MergeSpectra
    //{
    //    public int? Scan_Number;
    //    public string Title;
    //    public double? RetentionTime;
    //    public Cluster Ion;
    //    public int TimeSort;
    //    public double Mass;
    //    /// <summary>
    //    /// Also known as Ion Current
    //    /// </summary>
    //    public float Intensity;  
    //}



    public class SpecPoint
    {
        private static byte zeroByte = 0;
        
        //LABELPEAK _source = null;

        internal LABELPEAK Source { get; set; }

        public float Baseline { get { return (Source == null ? 0 : Source.Baseline); } }
        public byte Charge { get { return (Source == null ? zeroByte : Source.Charge); } }
        public byte Flags { get { return (Source == null ? zeroByte : Source.Flags); } }

        internal float _Int = float.NaN;       
        public float Intensity 
        { 
            get 
            {
                if (Source != null)
                    return Source.Intensity;
                else
                    return _Int;
            } 
        }

        internal double _MZ = double.NaN;
        public double MZ 
        { 
            get 
            {
                if (Source != null)
                    return Source.Mass;
                else
                    return _MZ;
            } 
        }
        public float Noise { get { return (Source == null ? 0 : Source.Noise); } }
        public float Resolution { get { return (Source == null ? 0 : Source.Resolution); } }
        
        /// <summary>
        /// Mass (Calculated from MZ and Charge state)
        /// </summary>
        public double Mass 
        { 
            get 
            { 
                
                //var m = (this.Charge) >= 1 ? ((this.MZ * (double)this.Charge) - ((double)this.Charge * 1.0072764668813)) : double.NaN;

                //if (m > 4477.05 && m < 4477.06) 
                //    Debug.WriteLine("MZ = " + this.MZ.ToString() + ", Z = " + this.Charge.ToString());

                return this.MZ.ToMass(this.Charge);
            } 
        }   // ((double.Parse(point["MZ"]) * double.Parse(point["Charge"])) - (double.Parse(point["Charge"]) * 1.007276)));  //1.00782503214 }}
        //((o.MZ * o.Charge) - (o.Charge * 1.00782503214))



        public string Description
        {
            get
            {
                return "Mass = " + Convert.ToString(Math.Round(Mass, 4)) + "\nMZ = " + Convert.ToString(Math.Round(MZ, 4)) + " * " + Convert.ToString(Charge);
            }
        }
    }





    public class SpecPointCollection : IEnumerable<SpecPoint>
    {

        private LABELPEAK[] _source = null;
        private double[] _mz = null;
        private double[] _intensity = null;


        internal SpecPointCollection(LABELPEAK[] source)
        {
            _source = source;
        }

        internal SpecPointCollection(double[] mz, double[] intensity)
        {
            _mz = mz;
            _intensity = intensity;
        }

        public SpecPoint this[int index]
        {
            get
            {
                if (_source != null)
                    return new SpecPoint() { Source = _source[index] };
                else
                    return new SpecPoint() { _MZ = _mz[index], _Int = (float)_intensity[index] };
            }
        }

        public IEnumerator<SpecPoint> GetEnumerator()
        {
            if (_source != null)
            {
                foreach (var point in _source)
                    yield return new SpecPoint() { Source = point };
            }
            else
            {
                for (int i = 0; i < _mz.Length; i++ )
                    yield return new SpecPoint() { _MZ = _mz[i], _Int = (float)_intensity[i] };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

    public class SpectrumCollection : ICollection<ThermoSpectrum>
    {
        RawProvider _parent = null;
        private SortedList<int, ThermoSpectrum> _scanCache = new SortedList<int,ThermoSpectrum>();
        
        

        internal SpectrumCollection(RawProvider parent)
        {
            _parent = parent;

            
        }

        public bool EnableExtraInfo = false;

        Dictionary<int, double> TimeLookup = new Dictionary<int, double>();

        public int ScanIndex(double time)
        {

            
            if (_parent._rawFile == null)
            {
                _parent._rawFile = new RawFile(_parent._filePath.FullName);
                _parent._rawFile.SelectInstrument(InstrumentType.MassSpec, 1);
                this.Clear();
            }

            // populate the cache
            if (TimeLookup.Any() == false) 
                for (int i = _parent._rawFile.FirstSpectrum; i <= _parent._rawFile.LastSpectrum; i++)
                    TimeLookup.Add(i, _parent._rawFile.GetScanStatsForScanNumber(i).StartTime);
                
            // return the nearest time value
            return TimeLookup.MinBy(i => Math.Abs(time - i.Value)).Key;
        }


        public ThermoSpectrum this[int ScanNum]
        {
            get
            {
                var resultSpectrum = new ThermoSpectrum(_parent); //, s2.PositionsArray, s2.IntensitiesArray); //(.Select(i => (float)i).ToArray());
                try
                {
                    if (_parent._rawFile == null)
                    {
                        _parent._rawFile = new RawFile(_parent._filePath.FullName);
                        _parent._rawFile.SelectInstrument(InstrumentType.MassSpec, 1);
                        this.Clear();
                    }

                    if (_scanCache.ContainsKey(ScanNum)) return _scanCache[ScanNum];

                    var stats = _parent._rawFile.GetScanStatsForScanNumber(ScanNum);

                    if (stats == null) return null;  // This is null when the ScanNum is invalid...

                    //var s2 = _parent._rawFile.GetScanFromScanNumber(ScanNum, stats);

                    //var x = s2.PositionsArray;
                    //var y = s2.IntensitiesArray;

                    //var resultSpectrum = new Spectrum(new double[] { 100, 200 }, new double[] { 500, 1000 }); //(.Select(i => (float)i).ToArray());

                    //var s = _parent._rawFile.GetScan(ScanNum);

                    //resultSpectrum.Points = new SpecPointCollection(s.GetLabelPeaks()); // s.GetLabelPeaks().Cast<SpecPoint>().ToArray();



                    resultSpectrum.Info = resultSpectrum.Title = _parent._rawFile.GetFilterForScanNumber(ScanNum).ToString();
                    resultSpectrum.OriginFilename = _parent._filePath.Name;
                    resultSpectrum.ScanNumber = ScanNum;
                    resultSpectrum.StartTime = stats.StartTime;
                    resultSpectrum.StartMass = stats.LowMass;
                    resultSpectrum.EndMass = stats.HighMass;
                    resultSpectrum.BasePeakIntensity = (float)stats.BasePeakIntensity;
                    resultSpectrum.BasePeakMass = (float)stats.BasePeakMass;
                    resultSpectrum.Activation = (((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum)))).Activations.Any() ? Convert.ToString((((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum)))).Activations[0]) : "";
                    resultSpectrum.IonCurrent = (float)stats.TIC;
                    resultSpectrum.TriggerMZ = ((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum))).Masses.Any() ? (double?)((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum))).Masses[0] : (double?)null;
                    var sType = ((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum))).MSOrder.ToString();
                    resultSpectrum.ScanType = (sType == "MS") ? "MS1" : sType;

                    // Populate extra values
                    resultSpectrum.extras.Clear();

                    if (EnableExtraInfo)
                    {
                        try
                        {
                            foreach (string aKey in _parent._rawFile.GetTrailerExtraLabelsForScanNumber(ScanNum))
                                resultSpectrum.extras.Add(aKey, _parent._rawFile.GetTrailerExtraValueForScanNumber(ScanNum, aKey));
                        }
                        catch { }
                    }

                    // Prevent the scan cache from causing outofmemory exceptions
                    if (_scanCache.Count > 200) _scanCache.Clear();

                    _scanCache.Add(ScanNum, resultSpectrum);

                    return resultSpectrum;
                }
                catch (Exception ex)
                {
                    throw ex;

                    return resultSpectrum;
                }
            }
        }

        public IEnumerator<ThermoSpectrum> GetEnumerator()
        {
            if (_parent._rawFile == null)
            {
                _parent._rawFile = new RawFile(_parent._filePath.FullName);
                _parent._rawFile.SelectInstrument(InstrumentType.MassSpec, 1);
                this.Clear();
            }

            int currentScan = _parent._rawFile.FirstSpectrum;

            do
            {
                //for (int i = _rawFile.FirstSpectrum; i <= _rawFile.LastSpectrum; )
                //    yield return i;
                if (currentScan == -1) 
                    yield break;
                else
                    yield return this[currentScan];
                currentScan = _parent._rawFile.GetNextScanIndex(currentScan);
            }
            while (currentScan <= _parent._rawFile.LastSpectrum);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #region ICollection<Spectrum> Members

        public void Add(ThermoSpectrum item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _scanCache.Clear();
        }

        public bool Contains(ThermoSpectrum item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ThermoSpectrum[] myArr, int index)
        {
            foreach (var i in this)
            {
                myArr.SetValue(i, index);
                index++;
            }
        }

        public int Count
        {
            get { return _parent._rawFile.SpectraCount; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(ThermoSpectrum item)
        {
            throw new NotImplementedException();
        }

        #endregion
    }



}

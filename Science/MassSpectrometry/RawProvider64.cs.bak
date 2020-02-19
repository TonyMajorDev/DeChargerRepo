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
//using ThermoFisher.Foundation.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data;
using System.Threading;
//using ThermoFisher.CommonCore.RawFileReader;


public partial class RMSProvider
    {

        /// <summary>
        /// Thermo RAW data file format handler
        /// </summary>
        public class RawProvider64
        {
            internal FileInfo _filePath = null;
            internal IRawDataPlus _rawFile = null;
            internal SpectrumCollection _scans = null;

            public RawProvider64(string FilePath)
            {
                if (File.Exists(FilePath))
                {
                    _filePath = new FileInfo(FilePath);
                }
            }

            private void InitRawFile()
            {
                if (_rawFile == null)
                {
                //Thread.Sleep(100);  // Thermo seems to need a few milliseconds to recognize that the file is there...? 

                //var x = new RawFile();
                //x.FileName = _filePath.FullName;
                //_rawFile = RawFileReaderFactory.ReadFile(_filePath.FullName);

                //RawFile

                //var x = new ThreadSafeRawFileAccess(RawFileReaderAdapter.FileFactory(_filePath.FullName));

                //_rawFile = x.CreateThreadAccessor();


                // According to Shofstahl, Jim <jim.shofstahl@thermofisher.com>, the File not found exception that this line can throw is to be ignored.  
                // I don't like that, but I have to live with it, and it does not cause any issues except when debugging, so I recommend
                // adding an Exception Condition to not break here in Visual Studio: https://blogs.msdn.microsoft.com/devops/2016/03/31/break-on-exceptions-thrown-only-from-specific-modules-in-visual-studio-15-preview/


                _rawFile = RawFileReaderAdapter.FileFactory(_filePath.FullName); // new RawFile(_filePath.FullName);


                _rawFile.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device.MS, 1);
                    if (_scans != null) _scans.Clear();
                }
            }

            public string WhyBadFile
            {
                get
                {
                    try
                    {
                        if (_filePath == null) return "Path to data file is not set.  ";
                        _filePath.Refresh();
                        if (!_filePath.Exists) return "File does not exist.  ";

                        InitRawFile();

                        if (!_rawFile.HasMsData) return "There is no MS Data.  The file is likely bad.  " + ((_filePath.Length == 0) ? "Also, the file size is 0 bytes.  " : string.Empty);
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }

                //var x = new ThermoFisher.Foundation.IO.XcaliburRangeSelector();

                    return string.Empty;
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

            public RunInfo Info
            {
                get
                {
                    if (_rawFile == null)
                    {
                        _rawFile = RawFileReaderAdapter.FileFactory(_filePath.FullName); // new RawFile(_filePath.FullName);
                        _rawFile.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device.MS, 1);
                        Scans.Clear();
                    }

                    return new RunInfo(_rawFile.RunHeader); //  .GetRunHeader());
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

                // Create the mass options object that will be used when averaging the scans
                var options = _rawFile.DefaultMassOptions();

                options.ToleranceUnits = ToleranceUnits.ppm;
                options.Tolerance = 2;  //TODO: pass in tolerance!!!!  This hardcoded value is not acceptable.  

                var scans = new List<int>();

                for (int scanNumber = startScan; scanNumber <= endScan; scanNumber++)
                {
                    // Get the scan filter for the spectrum
                    var scanFilter = _rawFile.GetFilterForScanNumber(scanNumber);

                    if (string.IsNullOrEmpty(scanFilter.ToString())) continue;

                    if (scanFilter.MassAnalyzer == ThermoFisher.CommonCore.Data.FilterEnums.MassAnalyzerType.MassAnalyzerFTMS
                        && scanFilter.MSOrder == ThermoFisher.CommonCore.Data.FilterEnums.MSOrderType.Ms
                        && scanFilter.Detector == ThermoFisher.CommonCore.Data.FilterEnums.DetectorType.Valid)
                        scans.Add(scanNumber);
                }

                // This example uses a different method to get the same average spectrum that was calculated in the
                // previous portion of this method.  Instead of passing the start and end scan, a list of scans will
                // be passed to the GetAveragedMassSpectrum function.

                var spec = new PointSet();

                var averageScan = _rawFile.AverageScans(scans, options);

                if (averageScan.HasCentroidStream)
                {
                    if (double.IsNaN(startMass) || double.IsNaN(endMass))
                        return new PointSet(averageScan.CentroidScan.Masses, averageScan.CentroidScan.Intensities);

                    for (int i = 0; i < averageScan.CentroidScan.Length; i++)
                    {
                        if (averageScan.CentroidScan.Masses[i] > startMass && averageScan.CentroidScan.Masses[i] < endMass)
                        {
                            spec.AddWithoutEvents(averageScan.CentroidScan.Masses[i], (float)averageScan.CentroidScan.Intensities[i]);
                        }
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
            public MassSpectrometry.Chromatogram ExtractIon(double startMZ, double endMZ, float startTime = 0, float endTime = float.MaxValue)
            {
                return this.ExtractIon(new List<SignalProcessing.Range>() { new SignalProcessing.Range() { Start = startMZ, End = endMZ } }, startTime, endTime);
            }


            public MassSpectrometry.Chromatogram ExtractIon(List<SignalProcessing.Range> mzRangeList, float startTime = 0, float endTime = float.MaxValue)
            {
                if (_rawFile == null)
                {
                    _rawFile = RawFileReaderAdapter.FileFactory(_filePath.FullName); // new RawFile(_filePath.FullName);
                    _rawFile.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device.MS, 1);
                }

                // Define the settings for getting the Base Peak chromatogram
                var settings = new ChromatogramTraceSettings(TraceType.MassRange);

                settings.MassRangeCount = mzRangeList.Count;

                for (int i = 0; i < mzRangeList.Count; i++)
                    settings.SetMassRange(i, new ThermoFisher.CommonCore.Data.Business.Range(mzRangeList[i].Start, mzRangeList[i].End));

                //TODO: Not filtering by MS1...  I don't know yet if that is a problem...
                //settings.Filter = new ScanFilter() { MSOrder = ScanEventBase.MSOrderType.MS };

                // Get the chromatogram from the RAW file. 
                var data = _rawFile.GetChromatogramData(new IChromatogramSettings[] { settings }, _rawFile.ScanNumberFromRetentionTime(startTime), _rawFile.ScanNumberFromRetentionTime(endTime));

                // Split the data into the chromatograms
                var chromData = ChromatogramSignal.FromChromatogramData(data);

                return new MassSpectrometry.Chromatogram(chromData[0].Times.ToArray(), chromData[0].Intensities.ToArray());
            }


            public void ReleaseFile()
            {
                try
                {
                    this._rawFile.Dispose();
                }
                catch { }
            }


        }

        public class RunInfo
        {
            public RunInfo(ThermoFisher.CommonCore.Data.Business.RunHeader info)
            {
                StartTime = info.StartTime;
                EndTime = info.EndTime;
                HighMass = info.HighMass;
                LowMass = info.LowMass;
                FirstScanNum = info.FirstSpectrum;
                LastScanNum = info.LastSpectrum;
            }

            public double StartTime { get; private set; }
            public double EndTime { get; private set; }
            public double HighMass { get; private set; }
            public double LowMass { get; private set; }
            public int FirstScanNum { get; private set; }
            public int LastScanNum { get; private set; }
        }



        [Obsolete("TheroSpectrum is deprecated because instrument specific classes violate the design principles, please use Class SuperSpectrum instead?")]
        public class ThermoSpectrum
        {
            static object lockObject = new object();

            SpecPointCollection _pointCache = null;
            RawProvider64 _parent = null;
            //public Spectrum(RawProvider64 parent, IEnumerable<double> keys, IEnumerable<double> values) : base(keys, values) { _parent = parent; }
            public ThermoSpectrum(RawProvider64 parent)
            {
                _parent = parent;
            }

            public string Info { get; set; }

            public virtual string OriginFilename { get; internal set; }

            public virtual string ScanType { get; internal set; }

            public virtual double? ParentMZ { get; internal set; }

            public virtual int? ScanNumber { get; internal set; }

            public virtual double? StartMass { get; internal set; }

            public virtual double? EndMass { get; internal set; }

            public virtual double? StartTime { get; internal set; }

            public virtual double? EndTime { get; internal set; }

            public string Title { get; internal set; }

            /// <summary>
            /// Total Ion Current
            /// </summary>
            public virtual float? IonCurrent { get; internal set; }

            public virtual float? BasePeakIntensity { get; internal set; }

            public virtual float? BasePeakMass { get; internal set; }

            ///public virtual ActivationType[] Activation { get; set; } ///Due the number of activation types available it's better to have a enum type than a string

            ///Using Activation type
            public virtual string Activation
            {
                get;
                set;
            }

            public virtual int? ParentCharge { get; set; }

            public virtual double? ParentMass { get; set; }

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

            private Boolean TryGetPoints(out string errorMessage, bool centroidOnly = true)
            {
                errorMessage = null;

                try
                {
                    // Get the scan statistics from the RAW file for this scan number
                    //var scanStatistics = _parent._rawFile.GetScanStatsForScanNumber(this.ScanNumber.Value);

                    var scanStatistics = _parent._rawFile.GetScanStatsForScanNumber(this.ScanNumber.Value);

                    // Check to see if the scan has centroid data or profile data.  Depending upon the
                    // type of data, different methods will be used to read the data.  While the ReadAllSpectra
                    // method demonstrates reading the data using the Scan.FromFile method, generating the
                    // Scan object takes more time and memory to do, so that method isn't optimum.
                    if (centroidOnly || scanStatistics.IsCentroidScan)
                    {
                        // Get the centroid (label) data from the RAW file for this scan
                        var centroidStream = _parent._rawFile.GetCentroidStream(this.ScanNumber.Value, false);

                        if (centroidStream.Masses != null)
                        {
                            var labelPeaks = centroidStream.GetLabelPeaks();

                            _pointCache = new SpecPointCollection(labelPeaks); // s.GetLabelPeaks().Cast<SpecPoint>().ToArray();


                            //Console.WriteLine("Spectrum (centroid/label) {0} - {1} points", this.ScanNumber.Value, centroidStream.Length);

                            // Print the spectral data (mass, intensity, charge values).  Not all of the information in the high resolution centroid 
                            // (label data) object is reported in this example.  Please check the documentation for more information about what is
                            // available in high resolution centroid (label) data.
                            //for (int i = 0; i < centroidStream.Length; i++)
                            //{
                            //    Console.WriteLine("  {0} - {1:F4}, {2:F0}, {3:F0}", i, centroidStream.Masses[i], centroidStream.Intensities[i], centroidStream.Charges[i]);
                            //}
                        }

                    }

                    if (_pointCache == null)
                    {
                        // Get the segmented (low res and profile) scan data
                        var segmentedScan = _parent._rawFile.GetSegmentedScanFromScanNumber(this.ScanNumber.Value, scanStatistics);

                        //Console.WriteLine("Spectrum (normal data) {0} - {1} points", this.ScanNumber.Value, segmentedScan.Positions.Length);

                        _pointCache = new SpecPointCollection(segmentedScan.Positions, segmentedScan.Intensities);
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

            internal LabelPeak Source { get; set; }

            public float Baseline { get { return (Source == null ? 0 : (float)Source.Baseline); } }
            public byte Charge { get { return (Source == null ? zeroByte : (byte)Source.Charge); } }
            public byte Flags { get { return (Source == null ? zeroByte : (byte)Source.Flag); } }

            internal float _Int = float.NaN;
            public float Intensity
            {
                get
                {
                    if (Source != null)
                        return (float)Source.Intensity;
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
            public float Noise { get { return (Source == null ? 0 : (float)Source.Noise); } }
            public float Resolution { get { return (Source == null ? 0 : (float)Source.Resolution); } }

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

                    return MassSpectrometry.MassSpecExtensions.ToMass(this.MZ, this.Charge);
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


            private LabelPeak[] _source = null;
            private double[] _mz = null;
            private double[] _intensity = null;


            internal SpecPointCollection(LabelPeak[] source)
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
                    for (int i = 0; i < _mz.Length; i++)
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
            public static int MaxCacheSize = 1000;

            RawProvider64 _parent = null;
            private SortedList<int, ThermoSpectrum> _scanCache = new SortedList<int, ThermoSpectrum>();

            internal SpectrumCollection(RawProvider64 parent)
            {
                _parent = parent;
            }

            public bool EnableExtraInfo = true;

            Dictionary<int, double> TimeLookup = new Dictionary<int, double>();

            [HandleProcessCorruptedStateExceptions]
            public int ScanIndex(double time)
            {


                if (_parent._rawFile == null)
                {
                    _parent._rawFile = RawFileReaderAdapter.FileFactory(_parent._filePath.FullName); // new RawFile(_filePath.FullName);
                    _parent._rawFile.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device.MS, 1);
                    this.Clear();
                }

                // populate the cache
                if (TimeLookup.Any() == false)
                    for (int i = _parent._rawFile.RunHeader.FirstSpectrum; i <= _parent._rawFile.RunHeader.LastSpectrum; i++)
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
                            _parent._rawFile = RawFileReaderAdapter.FileFactory(_parent._filePath.FullName); // new RawFile(_filePath.FullName);
                            _parent._rawFile.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device.MS, 1);

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

                        var currentFilter = _parent._rawFile.GetFilterForScanNumber(ScanNum);

                        resultSpectrum.Info = resultSpectrum.Title = currentFilter.ToString();
                        resultSpectrum.OriginFilename = _parent._filePath.Name;
                        resultSpectrum.ScanNumber = ScanNum;
                        resultSpectrum.StartTime = stats.StartTime;
                        resultSpectrum.StartMass = stats.LowMass;
                        resultSpectrum.EndMass = stats.HighMass;
                        resultSpectrum.BasePeakIntensity = (float)stats.BasePeakIntensity;
                        resultSpectrum.BasePeakMass = (float)stats.BasePeakMass;
                        //ScanEventBase.ActivationType[] acttype = { };
                        ///resultSpectrum.Activation = (((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum)))).Activations.Any() ? (((((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum)))).Activations)) : acttype; ///

                        //resultSpectrum.Activation = (((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum)))).Activations.Any() ? Convert.ToString(string.Join(",", (((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum)))).Activations)) : ""; ///

                        resultSpectrum.Activation = string.Empty;

                        if (currentFilter.MSOrder != ThermoFisher.CommonCore.Data.FilterEnums.MSOrderType.Ms)
                        {
                            //TODO: Warning -- this only works for single activation!!!  
                            switch (currentFilter.GetActivation(0))
                            {
                                case ThermoFisher.CommonCore.Data.FilterEnums.ActivationType.ElectronTransferDissociation:
                                    resultSpectrum.Activation += "ETD ";
                                    break;
                                case ThermoFisher.CommonCore.Data.FilterEnums.ActivationType.HigherEnergyCollisionalDissociation:
                                    resultSpectrum.Activation += "HCD ";
                                    break;
                                case ThermoFisher.CommonCore.Data.FilterEnums.ActivationType.UltraVioletPhotoDissociation:
                                    resultSpectrum.Activation += "UVPD ";
                                    break;
                                case ThermoFisher.CommonCore.Data.FilterEnums.ActivationType.ElectronCaptureDissociation:
                                    resultSpectrum.Activation += "ECD ";
                                    break;
                                default:
                                    resultSpectrum.Activation += "CID ";
                                    break;
                            }
                        }


                        //resultSpectrum.Activation = (((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum)))).Activations.Any() ? Convert.ToString((((ThermoFisher.Foundation.IO.ScanEventBase)(_parent._rawFile.GetFilterForScanNumber(ScanNum)))).Activations[0]) : ""; /// Only uses the first activation type. Doesn't work for multiple activation types.

                        resultSpectrum.IonCurrent = (float)stats.TIC;

                        //TODO: warning -- this line ignores other parent ions that may have also been included!!!   
                        resultSpectrum.ParentMZ = currentFilter.MassCount > 0 ? (double?)currentFilter.GetMass(0) : (double?)null;

                        resultSpectrum.ScanType = (currentFilter.MSOrder == ThermoFisher.CommonCore.Data.FilterEnums.MSOrderType.Ms) ? "MS1" : currentFilter.MSOrder.ToString().ToUpper();

                        // Populate extra values
                        resultSpectrum.extras.Clear();

                        if (EnableExtraInfo)
                        {
                            // Get the Trailer Extra data fields present in the RAW file
                            var trailerFields = _parent._rawFile.GetTrailerExtraHeaderInformation();

                            try
                            {
                                for (int i = 0; i < trailerFields.Length; i++)
                                    resultSpectrum.extras.Add(trailerFields[i].Label, _parent._rawFile.GetTrailerExtraValue(ScanNum, i));
                            }
                            catch { }
                        }

                        if (_scanCache.Count > MaxCacheSize) _scanCache.Clear();  // on large files, this we using too much ram, so clear it every so often...

                        _scanCache.Add(ScanNum, resultSpectrum);

                        return resultSpectrum;
                    }
                    catch (Exception ex)
                    {
                        throw ex;

                        //return resultSpectrum;
                    }
                }
            }

            public IEnumerator<ThermoSpectrum> GetEnumerator()
            {
                if (_parent._rawFile == null)
                {

                    // This throws an exception that is not fatal, but I do not like this...  this cannot be right
                    _parent._rawFile = RawFileReaderAdapter.FileFactory(_parent._filePath.FullName); // new RawFile(_filePath.FullName);
                    _parent._rawFile.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device.MS, 1);
                    this.Clear();
                }

                int currentScan = _parent._rawFile.RunHeader.FirstSpectrum;

                do
                {
                    //for (int i = _rawFile.FirstSpectrum; i <= _rawFile.LastSpectrum; )
                    //    yield return i;
                    if (currentScan == -1)
                        yield break;
                    else
                        yield return this[currentScan];

                    currentScan++; // currentScan = _parent._rawFile.GetNextScanIndex(currentScan);  
                }
                while (currentScan <= _parent._rawFile.RunHeader.LastSpectrum);
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
                get { return _parent._rawFile.RunHeader.LastSpectrum < 1 ? 0 : (_parent._rawFile.RunHeader.LastSpectrum - _parent._rawFile.RunHeader.FirstSpectrum) + 1; }
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



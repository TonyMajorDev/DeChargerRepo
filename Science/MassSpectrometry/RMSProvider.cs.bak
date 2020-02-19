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
using System.Net;

using System.IO;
using Ionic.Zip;
using System.Linq;
using System.Collections.Generic;
using SignalProcessing;
using MassSpectrometry;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Threading;
//using static MassSpectrometry.RMSProvider;

/// <summary>
/// An RAW data file provider that eposes the IMSProvider interface
/// </summary>
public partial class RMSProvider : IMSProvider
{
    RawProvider64 rms;

    public static bool UseAlternateChargeDetect = false;

    public static bool CanReadFormat(FileSystemInfo fileOrFolder)
    {
        return fileOrFolder != null && fileOrFolder is FileInfo && fileOrFolder.Exists && fileOrFolder.Name.ToLower().EndsWith(".raw");
    }

    public RMSProvider(FileInfo rmsFile)
    {
        Source = rmsFile;
        Source.Refresh();

        _sourceHash = Source.Sha256Hash();  // We can't generate a hash after the file is opened by RawProvider64, so get that now.  

        Debug.Assert(Source.Exists, "File is Missing! ");

        rms = new RawProvider64(Source.FullName);

        //var selection =
        //    from z in rms
        //    where z.FileName == "index.txt"
        //    select z;

        //var zmsMemory = new MemoryStream();

        //selection.First().Extract(zmsMemory);

        //zmsMemory.Position = 0;
        //var sr = new StreamReader(zmsMemory);
        //var header = sr.ReadLine();

        //zmsIndex = new Dictionary<int, ZMSIndexLine>();

        //while (!sr.EndOfStream)
        //{
        //    var z = new ZMSIndexLine(sr.ReadLine());
        //    zmsIndex.Add(z.ScanNumber, z);
        //}
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
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return rms.Average(startScan, endScan, MsOrderFilter, startMass, endMass);
    }


    /// <summary>
    /// File name (without folder) of source of data
    /// </summary>
    public string Filename
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            return Source.Name; 
        }
    }

    public int ScanIndex(double retentionTime)
    {
        // Find closest point
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return rms.Scans.ScanIndex(retentionTime);

        //rms.Scans.EnableExtraInfo = false;

        //rms.

        //    var r = from s in (rms.Scans as ICollection<ThermoSpectrum>)
        //            orderby Math.Abs(s.StartTime.Value - retentionTime)
        //            select s.ScanNumber.Value;

        ////TODO: The performance at this line is TERRIBLE.  do something simpler to get the scan index!!!
        //var r2 = r.First();

        //    return r2;
    }

    public SpecPointCollection ThermoPoints(int ScanNumber)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        var points = (from s in rms.Scans.Where(i => i.ScanNumber == ScanNumber)
                      orderby s
                      select s);

        if (points.Any())
            return points.First().Points;

        return null;
    }



    public string Title(int scan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        var theSpectrum = rms.Scans[scan];

        return theSpectrum == null ? string.Empty : theSpectrum.Info;
    }

    public double? ParentMZ(int scan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return rms.Scans[scan].ParentMZ;
    }

    public double? ParentMass(int scan)
    {
        return rms.Scans[scan].ParentMass;
    }

    public int? ParentCharge(int scan)
    {
        return rms.Scans[scan].ParentCharge;
    }


    public string ScanType(int scan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return rms.Scans[scan].ScanType;
    }

    public float RetentionTime(int scan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return (float)rms.Scans[scan].StartTime.Value;
    }

    public bool ScanExists(int scan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return rms.Scans[scan] != null;
    }

    public bool Contains(int scan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return rms.Scans.Any(s => s.ScanNumber.Value == scan);
    }

    int _maxscan = 0;

    public int MaxScan
    {
        get
        {
            if (_maxscan != 0)
            {
                return _maxscan;
            }
            else
            {
                _maxscan = rms.Scans.MaxBy(a => a.ScanNumber).ScanNumber.Value;
            }
            return _maxscan;
        }
    }

    public int? NextScan(int currentScan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        int crntscan = currentScan + 1;

        if (crntscan == MaxScan)
        {
            return crntscan;
        }

        for (int i = crntscan; i < MaxScan; i++)
        {
            if (rms.Scans[crntscan] != null)
            {
                if (crntscan != rms.Scans[crntscan].ScanNumber)
                {
                    throw new Exception("Inconsistent scan number, this should not happen");
                }
                return crntscan;
            }
            crntscan = crntscan + 1;
        }
        return null;
        ///The Performance of this Linq expression is terrible.
        //return (from s in rms.Scans.Select(i => i.ScanNumber)
        //        where s > currentScan
        //        orderby s
        //        select s).Cast<int?>().FirstOrDefault();
    }

    public int? PreviousScan(int currentScan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (currentScan == 1)
        {
            return null;
        }
        int crntscan = currentScan - 1;
        for (int i = crntscan; i > 0; i--)
        {
            if (rms.Scans[crntscan] != null)
            {
                if (crntscan != rms.Scans[crntscan].ScanNumber)
                {
                    throw new Exception("Inconsistent scan number, this should not happen");
                }
                return crntscan;
            }
        }
        return null;
        ///The Performance of this Linq expression is terrible.
        //return (from s in rms.Scans.Select(i => i.ScanNumber)
        //        where s < currentScan
        //        orderby s descending
        //        select s).Cast<int?>().FirstOrDefault();
    }

    public PointSet TIC
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            if (!rms.Scans.Any()) return new PointSet();

            if (rms.Scans.Where(s => s.Info.Contains("Full ms ")).Any())
            {
                return rms.Scans.Where(s => s.Info.Contains("Full ms ")).ToPointSet(k => k.StartTime.Value, v => v.IonCurrent.Value);
            }
            else
            {
                return rms.Scans.ToPointSet(k => k.StartTime.Value, v => v.IonCurrent.Value);
            }
        }
    }


    public PointSet BPM
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            if (!rms.Scans.Any()) return new PointSet();

            if (rms.Scans.Where(s => s.Info.Contains("Full ms ")).Any())
            {
                return rms.Scans.Where(s => s.Info.Contains("Full ms ")).ToPointSet(k => k.StartTime.Value, v => v.BasePeakMass.Value);
            }
            else
            {
                return rms.Scans.ToPointSet(k => k.StartTime.Value, v => v.BasePeakMass.Value);
            }
        }
    }

    public PointSet BPI
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);


            if (!rms.Scans.Any()) return new PointSet();

            if (rms.Scans.Where(s => s.Info.Contains("Full ms ")).Any())
            {
                return rms.Scans.Where(s => s.Info.Contains("Full ms ")).ToPointSet(k => k.StartTime.Value, v => v.BasePeakIntensity.Value);
            }
            else
            {
                return rms.Scans.ToPointSet(k => k.StartTime.Value, v => v.BasePeakIntensity.Value);
            }
        }
    }

    public List<SpectrumInfo> GetInitialParentInfo()
    {
        lock (this)
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            List<SpectrumInfo> mspec = new List<SpectrumInfo>();
            try
            {
                if (!rms.Scans.Any())
                {
                    return new List<SpectrumInfo>();
                }
            }
            catch (Exception ex)
            {
                throw new SystemException("The minimum required Thermo Foundation version is 3.0. Please update the update the Thermo Foundation to continue.  \n" + ex.Message);
            }


            try
            {
                SpectrumCollection scan = rms.Scans;
                foreach (var sc in scan)
                {
                    // if (sc.ScanType == "MS2")
                    {
                        mspec.Add(new SpectrumInfo
                        {
                            RetentionTime = sc.StartTime,
                            ScanNumber = sc.ScanNumber.Value,
                            Title = sc.Title,
                            Intensity = sc.IonCurrent.Value,
                            RelativeIntensity = sc.IonCurrent.Value,
                            MsLevel = sc.ScanType == "MS2" ? SpecMsLevel.MSMS : SpecMsLevel.MS
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return new List<SpectrumInfo>();
            }
            return mspec;
        }
    }

    public void SetParentInfo(SpectrumInfo sInfo, bool useProductScan = false)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (sInfo.ParentZ.HasValue && sInfo.ParentMass.HasValue) return;

        sInfo.ParentIon = DetectParentIon(sInfo.ScanNumber);
    }

    /// <summary>
    /// When Parent should be found even when it is found use this method.
    /// </summary>
    /// <param name="sInfo"></param>
    /// <returns></returns>
    public SpectrumInfo ForceParentInfo(SpectrumInfo sInfo)
    {
        SpectrumInfo spinfo = new SpectrumInfo();
        spinfo.ScanNumber = sInfo.ScanNumber;

        spinfo.RelativeIntensity = sInfo.RelativeIntensity;
        spinfo.RetentionTime = sInfo.RetentionTime;
        spinfo.Title = sInfo.Title;
        spinfo.MsLevel = sInfo.MsLevel;
        if (sInfo.MsLevel == SpecMsLevel.MS)
        {
            sInfo = spinfo;
            return spinfo;
        }

        spinfo.ParentIon = DetectParentIon(sInfo.ScanNumber);

        if (spinfo.ParentIon != null)
        {
            spinfo.ParentZ = spinfo.ParentIon.Z;
            spinfo.ParentMass = spinfo.ParentIon.MonoMass;
        }
        else
        {
            spinfo.ParentZ = sInfo.ParentZ;
            spinfo.ParentMass = sInfo.ParentMass;
        }
        sInfo = spinfo;
        return spinfo;
    }

    //public SpectrumInfo ForceParentInfo(SpectrumInfo sInfo)
    //{
    //    return 
    //}

    /// <summary>
    /// Gets the spectrum info with parent detected
    /// </summary>
    /// <param name="scanNumber"></param>
    /// <returns></returns>
    public Cluster DetectParentIon(int scanNumberOfMs2)
    {

        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        //sInfo.ParentIon = null;

        Debug.Print("Detecting Parent of " + scanNumberOfMs2);

        if (this.ScanType(scanNumberOfMs2) != "MS2") return null;

        //if (sInfo.ParentZ.HasValue && sInfo.ParentMass.HasValue) return;

        int detectedCharge = 0;

        // Find the Parent Scan by going backwards
        var parentScanNumber = this.PreviousScan(scanNumberOfMs2);

        while (parentScanNumber.HasValue && (this.ScanType(parentScanNumber.Value) != "MS1"))
            parentScanNumber = this.PreviousScan(parentScanNumber.Value);

        if (!parentScanNumber.HasValue) return null;

        var parentmz = this.ParentMZ(scanNumberOfMs2);

        if (parentmz.HasValue)
        {
            // get surrounding MS1 scans to merge together

            var beforeMS = this.PreviousScan(parentScanNumber.Value);
            var afterMS = this.NextScan(parentScanNumber.Value);

            while (beforeMS.HasValue && (this.ScanType(beforeMS.Value) != "MS1"))
                beforeMS = this.PreviousScan(beforeMS.Value);

            while (afterMS.HasValue && (this.ScanType(afterMS.Value) != "MS1"))
                afterMS = this.NextScan(afterMS.Value);

            List<Cluster> ionCandidatesDetected, pIons = new List<Cluster>();

            bool usingThermoMerge = false;

            if (usingThermoMerge)
            {
                // Use Thermo API for merging scans
                var result = this.Average(beforeMS.Value, afterMS.Value, 1, parentmz.Value - 2d, parentmz.Value + 2d);
                ionCandidatesDetected = new ChargeDetector(result).DetectChargeStates(MassTolerance);

                //ionCandidatesDetected = ionCandidatesDetected.Where(p => p.Peaks.Where(x => x.IsCorePeak && (Math.Abs(parentmz.Value - x.MZ) < (PPMCalc.MaxPPM(x.MZ, parentmz.Value) + 0.1))).Any()); // 0.01

                if (!ionCandidatesDetected.Any() && !pIons.Any()) return null;
            }
            else
            {
                //if (!afterMS.HasValue && !beforeMS.HasValue) return null;  // This should be way more flexible and tolerant to MS scans that are not available

                List<Cluster> parentIons = new List<Cluster>();

                // Use peak detected scans with limited m/z range to improve performance

                try
                {
                    // detect ions in the precursor scan in the target m/z range to get JUST the Charge state
                    var p1 = this[parentScanNumber.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                    pIons = new ChargeDetector(p1).DetectChargeStates(MassTolerance);


                    //var detectedCharge = pIons.Where(i => i.)

                    // filter out ions that don't include the peak that was the target of fragmentation
                    pIons = pIons.Where(p => p.Peaks.Where(x => x.IsCorePeak && (Math.Abs(parentmz.Value - x.MZ) < (PPMCalc.MaxPPM(x.MZ, parentmz.Value) + 0.1))).Any()).OrderByDescending(y => y.Score).ToList();

                    //                    var ion = pIons.Where(p => p.Peaks.Where(x => x.IsCorePeak && (Math.Abs(parentmz.Value - x.MZ) < (PPMCalc.MaxPPM(x.MZ, parentmz.Value) + 0.1))).Any()).MaxBy(x => x.Score * x.Z); // 0.01

                    if (!pIons.Any()) return null;

                    // If there is a clear high quality ion detected without contention, don't bother trying to detect with other charge variants.  
                    if ((pIons.Count == 1 && pIons.MaxBy(i => i.Score).Score > 800) || (pIons.Count > 1 && pIons[0].Score > 800 && (pIons[1].Score - pIons[0].Score) > 400)) return pIons[0];

                    //var ion = pIons.MaxBy(x => x.Score * x.Z); // 0.01  favor higher charge???
                    var ion = pIons.MaxBy(x => x.Score * x.Intensity); // 0.01

                    detectedCharge = ion.Z;
                    var zStart = Math.Max(1, detectedCharge - 5);
                    var zEnd = Math.Min(40, detectedCharge + 5);

                    var points = new PointSet();

                    foreach (var z in Enumerable.Range(zStart, zEnd - zStart))
                    {
                        var indices = new int?[] { parentScanNumber, afterMS, beforeMS };

                        foreach (var specindex in indices.Where(i => i.HasValue).Select(ix => ix.Value))
                        {
                            var a1 = this[specindex].Range(ion.MonoMass.ToMZ(z) - 2d, ion.MonoMass.ToMZ(z) + 2d).ToPointSet(k => k.Key, v => v.Value);

                            foreach (var aPoint in a1)
                                points.AddWithoutEvents(aPoint.Key, aPoint.Value);
                        }
                    }

                    parentIons.AddRange(new ChargeDetector(points).DetectChargeStates(MassTolerance, zStart, zEnd, 0, double.MaxValue, false));

                    //if (afterMS.HasValue)
                    //{



                    //    var a1 = this[afterMS.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                    //    a1.AddWithoutEvents()
                    //    var aIons = new ChargeDetector(a1).DetectChargeStates(MassTolerance);
                    //    parentIons.AddRange(aIons);
                    //}

                    //if (beforeMS.HasValue)
                    //{
                    //    var b1 = this[beforeMS.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                    //    var bIons = new ChargeDetector(b1).DetectChargeStates(MassTolerance);
                    //    parentIons.AddRange(bIons);
                    //}


                    //parentIons.AddRange(pIons);

                    ionCandidatesDetected = SignalProcessor.ConsolidateIons(parentIons.Where(i => ((i.MonoMass > ion.MonoMass - (2f / (float)ion.Z)) && (i.MonoMass < ion.MonoMass + (2f / (float)ion.Z))) || ((i.MonoMass > ion.SecondaryMonoMass - (2f / (float)ion.Z)) && (i.MonoMass < ion.SecondaryMonoMass + (2f / (float)ion.Z)))).ToList(), false, true, detectedCharge);

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + ", scanNum: " + parentScanNumber.Value + ", parentmz: " + parentmz.Value);


                }


            }



            // Added a bias toward ions with more core peaks, and added selection bias for clusters that include the parent M/Z
            if (ionCandidatesDetected.Any())
            {
                var returnionCandidatesDetected = ionCandidatesDetected.MaxBy(c => c.Intensity * (c.Peaks.Where(p => p.IsCorePeak).Count() * .25));
                returnionCandidatesDetected.Activation = rms.Scans[scanNumberOfMs2].Activation;
                return returnionCandidatesDetected;
                //return ionCandidatesDetected.MaxBy(c => c.Intensity * (c.Peaks.Where(p => p.IsCorePeak).Count() * .25));
            }
        }

        return null;
    }

    public List<SpectrumInfo> GetParentScans()
    {

        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        lock (this)
        {
            List<SpectrumInfo> mspec = new List<SpectrumInfo>();

            //  try
            // {
            if (!rms.Scans.Any()) return mspec;

            //SpectrumCollection scan = rms.Scans;
            Cluster cluster = null;

            foreach (var sc in rms.Scans.Where(a => a.ScanType == "").ToList())
            {
                cluster = DetectParentIon(sc.ScanNumber.Value);

                if (cluster != null)
                    mspec.Add(new SpectrumInfo
                    {
                        ParentIon = cluster,
                        Intensity = sc.IonCurrent.Value,
                        RelativeIntensity = sc.IonCurrent.Value,
                        RetentionTime = Math.Round(Convert.ToDouble(sc.StartTime), 3),
                        ScanNumber = sc.ScanNumber.Value,
                        Title = sc.Title,
                        MsLevel = sc.ScanType == "MS2" ? SpecMsLevel.MSMS : SpecMsLevel.MS
                    });
            }

            #region sorting

            var m = mspec.OrderBy(s => s.ParentMass != 0).ThenBy(a => a.RetentionTime);
            mspec = m.SortMergeSpectra();

            #endregion
            //}
            //catch (Exception ex)
            //{
            //    return new List<SpectrumInfo>();
            //}

            return mspec.ToList();
        }
    }


    public SpectrumInfo GetParentInfo(int scanNumber)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

       // lock (this)
       // {
            if (!rms.Scans.Any()) return null;

            //if (this.ParentMZ(scanNumber).HasValue == false) return null;
            if (this.ScanType(scanNumber) != "MS2") return null;

            return new SpectrumInfo
            {
                ParentIon = DetectParentIon(scanNumber),
                Intensity = rms.Scans[scanNumber].IonCurrent.Value,
                RelativeIntensity = rms.Scans[scanNumber].IonCurrent.Value,
                RetentionTime = Math.Round(Convert.ToDouble(rms.Scans[scanNumber].StartTime), 3),
                ScanNumber = rms.Scans[scanNumber].ScanNumber.Value,
                Title = rms.Scans[scanNumber].Title,
                IsParentScan = rms.Scans[scanNumber].ScanType == "MS1" ? true : false,
                MsLevel = rms.Scans[scanNumber].ScanType == "MS1" ? SpecMsLevel.MS : SpecMsLevel.MSMS,
                Activation = rms.Scans[scanNumber].Activation
            };
        //}
    }


    public string GetActivationType(int scanNumber)
    {
        lock (this)
        {
            if (!rms.Scans.Any()) return string.Empty;

            var cluster = DetectParentIon(scanNumber);
            if (cluster != null)
            {
                return rms.Scans[scanNumber].Activation;
            }
            return string.Empty;
        }
    }



    public List<SpectrumInfo> GetParentInfo(CancellationToken cToken = default(CancellationToken), int startScan = 1, int endScan = int.MaxValue)
    {
        //TODO: Implement this by using the new Detect ParentIon Method
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        lock (this)
        {
            List<SpectrumInfo> mspec = new List<SpectrumInfo>();

            //  try
            // {
            if (!rms.Scans.Any()) return mspec;

            SpectrumCollection scan = rms.Scans;
            //int i = scan.Count - 1;
            Cluster cluster = null;

            foreach (var sc in scan)
            {
                if (sc.ScanNumber < startScan || sc.ScanNumber > endScan) continue;

                if (cToken != default(CancellationToken) && cToken.IsCancellationRequested) return mspec;

                cluster = DetectParentIon(sc.ScanNumber.Value);

                mspec.Add(new SpectrumInfo
                {
                    ParentIon = cluster,
                    RetentionTime = sc.StartTime,
                    ScanNumber = sc.ScanNumber.Value,
                    Title = sc.Title,
                    Intensity = sc.IonCurrent.Value,
                    RelativeIntensity = sc.IonCurrent.Value,
                    IsParentScan = sc.ScanType == "MS1" ? true : false,
                    MsLevel = sc.ScanType == "MS1" ? SpecMsLevel.MS : SpecMsLevel.MSMS,
                    Activation = rms.Scans[sc.ScanNumber.Value].Activation,
                    DataSource = this
                });
            }

            #region sorting

            var m = mspec.OrderBy(s => s.ParentMass != 0).ThenBy(a => a.RetentionTime);
            mspec = m.SortMergeSpectra();

            #endregion
            return mspec;
        }
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
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return rms.ExtractIon(startMZ, endMZ, startTime, endTime);
    }

    public Chromatogram ExtractIon(List<Range> mzRangeList, float startTime = 0, float endTime = float.MaxValue)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return rms.ExtractIon(mzRangeList, startTime, endTime);
    }

    public Spectrum NativeScanSum(int[] scanNumbers, bool average = false)
    {
        throw new NotImplementedException();
    }

    public PointSet this[int scan]
    {
        get
        {
            //Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            //var selection =
            //    from z in rms
            //    where z.FileName == scan.ToString() + ".txt"
            //    select z;

            //var scanMemory = new MemoryStream();

            //selection.First().Extract(scanMemory);

            //scanMemory.Position = 0;
            //var sr = new StreamReader(scanMemory);
            //var header = sr.ReadLine();

            //var returnScan = new PointSet();

            //while (!sr.EndOfStream)
            //{
            //    var o = new ZMSLine(sr.ReadLine());
            //    returnScan.Add(o.MZ, o.Intensity);
            //}

            //return returnScan;

            //            return rms.Scans[scan];
            //try
            //{

            return rms.Scans[scan].Points.ToPointSet(k => k.MZ, v => v.Intensity, rms.Scans[scan].Title);
            //}
            //catch
            //{

            //  return new PointSet();
            //}
        }
    }

    /// <summary>
    /// Get a spectrum by the scan number
    /// </summary>
    /// <param name="scan"></param>
    /// <returns>A spectrum object or a null if the scan number is not valid! </returns>
    public Spectrum Scans(int scan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (this.ScanExists(scan) == false) return null;

        if (UseAlternateChargeDetect)
        {
            return TMassScans(scan);
        }
        else
        {
            try
            {

                //TODO: Fill in additional Spectrum properties here
                //if (rms.Scans.Any(x => x.ScanNumber == scan))
                //{
                var spectrum = new Spectrum(rms.Scans[scan].Points.Select(p => new ClusterPeak() { MZ = p.MZ, Intensity = p.Intensity, Z = p.Charge }));

                spectrum.ParentProvider = this;

                spectrum.Activation = rms.Scans[scan].Activation;

                spectrum.ScanNumber = scan;

                return spectrum;
                //}
                //else
                //{
                // this spectum does not exist...
                //    return null;
                //}
            }
            catch (Exception ex)
            {
                Debug.Print("Error reading the scan.  " + ex.Message + "\n" + ex.StackTrace + "\nScan Number: " + scan);

                return null;
            }
        }
    }

    public List<Cluster> DetectIons(int scanNum = 1, int minCharge = 1, int maxCharge = 30, double massRangeMin = 0, double massRangeMax = double.MaxValue)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);


        var cd = new ChargeDetector(this[scanNum]);
        var result = cd.DetectChargeStates(this.MassTolerance, minCharge, maxCharge, massRangeMin, massRangeMax);

        Debug.Print("-----------------------");
        Debug.Print("Detect Ions Called for Scan: " + scanNum + ", z=" + minCharge + " - " + maxCharge + ", peak count: " + this[scanNum].Count);
        Debug.Print("\tFound " + result.Count + " Clusters");
        if (result.Any()) Debug.Print("\tScore Range: " + result.Min(c => c.Score) + " - " + result.Max(c => c.Score));
        Debug.Print("-----------------------");

        return result;
        //return cd.ParallelDetectChargeStates(minCharge, maxCharge);
    }

    //public List<Cluster> GenerateIonsWithAssumedCharge(List<Cluster> ions, int MaxCharge = 1)
    //{


    //}

    /// <summary>
    /// Gets Thermo Mass Scan.  Returns null if scan number passed is not valid.  
    /// </summary>
    /// <param name="scan"></param>
    /// <returns></returns>
    public Spectrum TMassScans(int scan)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (this.Contains(scan) == false) return null;

        var cd = new ChargeDetector(this[scan]);

        //TODO: Fill in additional Spectrum properties here

        //var detectedResults = cd.ParallelDetectChargeStates(2, 40);
        // var detectedResults = cd.DetectChargeStates(2, 40);
        var detectedResults = cd.DetectChargeStates(MassTolerance);




        //var detectedResults = cd.DetectChargeStates(2, 2);

        //return rms.Scans[scan].Points.ToPointSet(k => k.TMass, v => v.Intensity);

        //var resultPoints = new PointSet();

        var resultPoints = new Spectrum();

        foreach (var aCluster in detectedResults)
            resultPoints.AddRange(aCluster.Peaks.Where(p => p.MZ > aCluster.MonoMZ));

        //foreach (var aPeak in aCluster.Peaks.Where(p => p.MZ > aCluster.MonoMZ))
        //    resultPoints.Add(aPeak);
        //resultPoints.Add(aPoint.MZ.ToMass(aCluster.Z), aPoint.Intensity);

        resultPoints.ScanNumber = scan;

        return resultPoints;
    }

    public IEnumerable<Spectrum> ScanRange(int startIndex, int endIndex, string filter)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);


        var includeEnds = true;

        //if (this.Count <= 0) throw new Exception("Empty Pointset");

        int increment = Math.Sign(endIndex - startIndex);

        if (startIndex == endIndex && includeEnds)
        {
            yield return this.Scans(startIndex);
        }
        else
        {
            for (int i = startIndex; i != (endIndex + increment); i += increment)
                if ((includeEnds || (i != startIndex && i != endIndex)) && this.Title(i).Contains(filter)) yield return this.Scans(i);
        }
    }

    public List<SpecPointCollection> ThermoRange(int startvalue, int endvalue, string startFilter = "")
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);


        int increment = Math.Sign(endvalue - startvalue);
        List<SpecPointCollection> thermos = new List<SpecPointCollection>();
        for (int i = startvalue; i != (endvalue + increment); i += increment)
        {
            var points = rms.Scans.Where(a => a.ScanNumber == i && a.ScanType == "MS1");
            if (points.Any() && this.Title(i).StartsWith(startFilter))
                thermos.Add(points.First().Points);
        }
        return thermos;
    }

    public IEnumerable<IEnumerable<Cluster>> DetectIonsForScanRange(int startIndex, int endIndex, string containsFilter = "", string startFilter = "", int minCharge = 1, int maxCharge = 30) //, double massRangeMin = 0, double massRangeMax = double.MaxValue)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        var includeEnds = true;

        //if (this.Count <= 0) throw new Exception("Empty Pointset");

        int increment = Math.Sign(endIndex - startIndex);

        if (startIndex == endIndex && includeEnds)
        {
            yield return this.DetectIons(startIndex, minCharge, maxCharge); //, massRangeMin, massRangeMax);
        }
        else
        {
            for (int i = startIndex; i != (endIndex + increment); i += increment)
                if ((includeEnds || (i != startIndex && i != endIndex)) && this.Title(i).StartsWith(startFilter) && this.Title(i).Contains(containsFilter)) yield return this.DetectIons(i, minCharge, maxCharge); //, massRangeMin, massRangeMax);
        }
    }



    //internal class ZMSLine
    //{
    //    public ZMSLine() { }

    //    public ZMSLine(string lineToParse)
    //    {
    //        var line = lineToParse.Split('\t');

    //        this.MZ = double.Parse(line[0]);
    //        this.Intensity = float.Parse(line[1]);
    //        this.Charge = int.Parse(line[2]);
    //    }


    //    public double MZ { get; set; }
    //    public float Intensity { get; set; }
    //    public int Charge { get; set; }
    //}

    //internal class ZMSIndexLine
    //{
    //    public ZMSIndexLine() { }

    //    public ZMSIndexLine(string lineToParse)
    //    {
    //        var line = lineToParse.Split('\t');

    //        this.ScanNumber = int.Parse(line[0]);
    //        this.RetentionTime = float.Parse(line[1]);
    //        this.TIC = float.Parse(line[2]);
    //        this.BPM = double.Parse(line[3]);
    //        this.BPI = float.Parse(line[4]);
    //        this.Filter = line[5];
    //        //filters.TryAdd(ScanNumber, Filter);
    //    }


    //    public int ScanNumber { get; set; }
    //    public float RetentionTime { get; set; }
    //    public float TIC { get; set; }
    //    public double BPM { get; set; }
    //    public float BPI { get; set; }
    //    public string Filter { get; set; }

    //}


    public double MassTolerance
    {
        get;
        set;
    }


    public FileSystemInfo Source { get; }

    string _sourceHash = null;

    public string SourceHash
    {
        get
        {
            // use a cached value after first call because this could be slow...
            if (_sourceHash == null) _sourceHash = Source.Sha256Hash();
            return _sourceHash;
        }
    }



    public SpecMsLevel MsLevel(int scan)
    {
        return rms.Scans[scan].ScanType == "MS2" ? SpecMsLevel.MSMS : SpecMsLevel.MS;

        //return this[scan].Title.ToLower().Contains("ms2") ? SpecMsLevel.MSMS : SpecMsLevel.MS;

        //throw new NotImplementedException();
    }
}


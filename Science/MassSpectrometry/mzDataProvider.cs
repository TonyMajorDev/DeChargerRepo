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
using System.Collections;
using MassSpectrometry;
using System.ComponentModel;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading;

public class mzDataProvider : IMSProvider
{

    Dictionary<int, int> scanStartIndex;
    //IDictionary<int, string> filterLookup = new Dictionary<int, string>();
    //List<RawExtractorLine> scanLines = new List<RawExtractorLine>();
    IEnumerable<mzDataScan> spectra = null;

    public static bool CanReadFormat(FileSystemInfo fileOrFolder)
    {
        if (fileOrFolder != null && fileOrFolder is FileInfo && fileOrFolder.Name.ToLower().EndsWith(".xml"))
        {
            try
            {
                return XDocument.Load((fileOrFolder as FileInfo).OpenText()).Root.Name == "mzData";
            }
            catch
            {
                return false;
            }
        }

        return false;
    }


    public mzDataProvider(FileStream txtFileStream)
    {
        var xdoc = XDocument.Load(txtFileStream);

        this.Source = new FileInfo(txtFileStream.Name);

        spectra = from aSpectrum in xdoc.Descendants("spectrumList").First().Descendants("spectrum")
                  select new mzDataScan
                  {
                      ScanNumber = int.Parse(aSpectrum.Attribute("id").Value),
                      Details = aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First(),
                      mzPoints = Base64ToArrayPoints(aSpectrum.Descendants("mzArrayBinary").First()),
                      intensityPoints = Base64ToArrayPoints(aSpectrum.Descendants("intenArrayBinary").First()),
                      RT = float.Parse(aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Descendants("cvParam").Where(d => d.Attribute("name").Value == "TimeInMinutes").First().Attribute("value").Value),
                      ScanMode = aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Descendants("cvParam").Where(d => d.Attribute("name").Value == "ScanMode").First().Attribute("value").Value,
                      Polarity = aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Descendants("cvParam").Where(d => d.Attribute("name").Value == "Polarity").First().Attribute("value").Value,
                      MsLevel = int.Parse(aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Attribute("msLevel").Value),
                      MzStart = double.Parse(aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Attribute("mzRangeStart").Value),
                      MzStop = double.Parse(aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Attribute("mzRangeStop").Value),
                      PrecursorDetails = aSpectrum.Descendants("spectrumDesc").First().Descendants("precursorList").Any() ? aSpectrum.Descendants("spectrumDesc").First().Descendants("precursorList").First().Descendants("precursor").First() : null

                      //    Debug.Print("Retention Time: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "TimeInMinutes").First().Attribute("value").Value);
                      //    Debug.Print("Scan Mode: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "ScanMode").First().Attribute("value").Value);
                      //    Debug.Print("Polarity: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "Polarity").First().Attribute("value").Value);
                      //    Debug.Print("MS Level: " + aSpectrum.Details.Attribute("msLevel").Value);
                      //    Debug.Print("m/z start: " + aSpectrum.Details.Attribute("mzRangeStart").Value);
                      //    Debug.Print("m/z stop: " + aSpectrum.Details.Attribute("mzRangeStop").Value);



                  };
    }

    public SpectrumInfo ForceParentInfo(SpectrumInfo sInfo)
    {
        return new SpectrumInfo();
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

    /// <summary>
    /// Filename without folder
    /// </summary>
    public string Filename
    {
        get { return Source.Name; }
    }

    public string GetActivationType(int scanNumber)
    {
        return string.Empty;
        //throw new NotImplementedException();
    }

    public PointSet Average(int startScan, int endScan, int MsOrderFilter = 1, double startMass = double.NaN, double endMass = double.NaN)
    {
        throw new NotImplementedException();
    }

    public PointSet TIC
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            if (!spectra.Any()) return new PointSet();

            if (spectra.Where(s => s.MsLevel == 1).Any())
            {
                return spectra.Where(s => s.MsLevel == 1).ToPointSet(k => k.RT, v => (float)v.intensityPoints.Sum());
            }
            else
            {
                return spectra.ToPointSet(k => k.RT, v => (float)v.intensityPoints.Sum());
            }
        }

    }

    public PointSet BPM
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            if (!spectra.Any()) return new PointSet();

            if (spectra.Where(s => s.MsLevel == 1).Any())
            {
                return spectra.Where(s => s.MsLevel == 1).ToPointSet(k => k.RT, v => (float)v.mzPoints[v.intensityPoints.MaxIndex()]);
                //return rms.Scans.Where(s => s.Info.Contains("Full ms ")).ToPointSet(k => k.StartTime.Value, v => v.BasePeakMass.Value);
            }
            else
            {
                return spectra.ToPointSet(k => k.RT, v => (float)v.mzPoints[v.intensityPoints.MaxIndex()]);
                //return rms.Scans.ToPointSet(k => k.StartTime.Value, v => v.BasePeakMass.Value);
            }
        }

    }

    public PointSet BPI
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            if (!spectra.Any()) return new PointSet();

            if (spectra.Where(s => s.MsLevel == 1).Any())
            {
                return spectra.Where(s => s.MsLevel == 1).ToPointSet(k => k.RT, v => (float)v.intensityPoints.Max());
            }
            else
            {
                return spectra.ToPointSet(k => k.RT, v => (float)v.intensityPoints.Max());
            }
        }

    }

    public PointSet this[int scan]
    {
        get
        {
            //return (from s in spectra
            //        where s.ScanNumber == scan
            //        select s).ToPointSet(k => k.mz, v => v.Intensity);

            //return (from s in scanLines
            //        where s.scan == scan
            //        select s).ToPointSet(k => k.mz, v => v.Intensity);

            return new PointSet(spectra.Where(s => s.ScanNumber == scan).First().mzPoints, spectra.Where(s => s.ScanNumber == scan).First().intensityPoints);

        }
    }

    public Spectrum Scans(int scan)
    {
        var result = new Spectrum();
        var spectrum = spectra.FirstOrDefault(s => s.ScanNumber == scan);
        var pointCount = spectrum.mzPoints.Length;

        for (int i = 0; i < pointCount; i++)
            result.Add(new ClusterPeak() { MZ = spectrum.mzPoints[i], Intensity = (float)spectrum.intensityPoints[i] });

        return result;

        //return new PeakList(from s in spectra
        //                    where s.ScanNumber == scan
        //                    select new ClusterPeak() )


        //return new PeakList(from s in scanLines
        //                    where s.scan == scan
        //                    select new ClusterPeak() { MZ = s.mz, Intensity = s.Intensity });
    }

    public int ScanIndex(double retentionTime)
    {
        return spectra.OrderBy(s => Math.Abs(retentionTime - s.RT)).First().ScanNumber;

        //if (indexes.Any())
        //    return indexes.First().ScanNumber;
        //else
        //    throw new Exception("Scan Index could not be found for Retention Time " + retentionTime);
    }

    //public SpecPointCollection ThermoPoints(int ScanNumber)
    //{
    //    throw new NotImplementedException();
    //}

    public List<SpectrumInfo> GetParentScans()
    {
        throw new NotImplementedException();
    }

    public string Title(int scan)
    {
        var spec = spectra.Where(s => s.ScanNumber == scan).First();

        return "Scan " + spec.ScanNumber + ", " + spec.ScanMode + ", MS" + spec.MsLevel + ", FTMS";
    }

    public double? ParentMZ(int scan)
    {
        throw new NotImplementedException();
    }

    //public List<SpecPointCollection> ThermoRange(int startvalue, int endvalue, string startFilter = "")
    //{
    //    throw new NotImplementedException();
    //}

    public string ScanType(int scan)
    {
        // returns "MS1" or "MS2"

        return "MS" + spectra.Where(s => s.ScanNumber == scan).First().MsLevel;
    }

    public float RetentionTime(int scan)
    {
        if (spectra.Any(s => s.ScanNumber == scan))
        {
            return spectra.Where(s => s.ScanNumber == scan).First().RT;
        }
        else
        {
            throw new Exception("Scan " + scan + " was not found. ");
        }

    }

    public bool Contains(int scan)
    {
        return spectra.Any(s => s.ScanNumber == scan);

        //return filterLookup.ContainsKey(scan);
    }

    public int? NextScan(int currentScan)
    {
        return (from s in spectra
                where s.ScanNumber > currentScan
                orderby s.ScanNumber
                select s.ScanNumber).Cast<int?>().FirstOrDefault();
    }

    public int? PreviousScan(int currentScan)
    {
        return (from s in spectra
                where s.ScanNumber < currentScan
                orderby s.ScanNumber descending
                select s.ScanNumber).Cast<int?>().FirstOrDefault();

    }

    public List<SpectrumInfo> GetParentInfo(CancellationToken cToken = default(CancellationToken), int startScan = 1, int endScan = int.MaxValue)
    {
        throw new NotImplementedException();
    }

    public List<SpectrumInfo> GetInitialParentInfo()
    {
        lock (this)
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            List<SpectrumInfo> mspec = new List<SpectrumInfo>();
            if (!spectra.Any())
            {
                return new List<SpectrumInfo>();
            }

            try
            {
                //SpectrumCollection scan = spectra.Scans;
                foreach (var sc in spectra)
                {
                    //if (sc.MsLevel == 2)
                    // {
                    mspec.Add(new SpectrumInfo
                    {
                        RetentionTime = sc.RT,
                        ScanNumber = sc.ScanNumber,
                        Title = sc.ScanMode,
                        Intensity = (float)sc.intensityPoints.Sum(),
                        RelativeIntensity = (float)sc.intensityPoints.Sum(),
                        MsLevel = (SpecMsLevel)sc.MsLevel
                    });
                    //}
                }
            }
            catch (Exception)
            {
                return new List<SpectrumInfo>();
            }
            return mspec;
        }
    }


    #region IMSProvider Members

    public IEnumerable<Spectrum> ScanRange(int startIndex, int endIndex, string filter)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IMSProvider Members


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

    #endregion

    #region IMSProvider Members


    IEnumerable<IEnumerable<Cluster>> IMSProvider.DetectIonsForScanRange(int startIndex, int endIndex, string containsFilter = "", string startFilter = "", int minCharge = 1, int maxCharge = 30)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IMSProvider Members


    public bool ScanExists(int scan)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IMSProvider Members


    public void SetParentInfo(SpectrumInfo sInfo, bool useProductScan = false)
    {
        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (sInfo.ParentZ.HasValue && sInfo.ParentMass.HasValue) return;

        sInfo.ParentIon = DetectParentIon(sInfo.ScanNumber);

        //throw new NotImplementedException();
    }

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
            }
            else
            {
                //if (!afterMS.HasValue && !beforeMS.HasValue) return null;  // This should be way more flexible and tolerant to MS scans that are not available

                List<Cluster> parentIons = new List<Cluster>();

                // Use peak detected scans with limited m/z range to improve performance

                if (afterMS.HasValue)
                {
                    var a1 = this[afterMS.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                    var aIons = new ChargeDetector(a1).DetectChargeStates(MassTolerance);
                    parentIons.AddRange(aIons);
                }

                if (beforeMS.HasValue)
                {
                    var b1 = this[beforeMS.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                    var bIons = new ChargeDetector(b1).DetectChargeStates(MassTolerance);
                    parentIons.AddRange(bIons);
                }

                var p1 = this[parentScanNumber.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                pIons = new ChargeDetector(p1).DetectChargeStates(MassTolerance);
                parentIons.AddRange(pIons);

                ionCandidatesDetected = SignalProcessor.ConsolidateIons(parentIons);
            }

            if (!ionCandidatesDetected.Any() && !pIons.Any()) return null;

            // Added a bias toward ions with more core peaks, and added selection bias for clusters that include the parent M/Z
            var candidates = ionCandidatesDetected.Where(p => p.Peaks.Where(x => x.IsCorePeak && (Math.Abs(parentmz.Value - x.MZ) < 0.01)).Any());
            if (candidates.Any()) return candidates.MaxBy(c => c.Intensity * (c.Peaks.Where(p => p.IsCorePeak).Count() * .25));

            // If we didn't find any ions in the merge of the 3 scans, let's just look at the original parent MS scan before we give up.  
            //candidates = pIons.Where(p => p.Peaks.Where(x => x.IsCorePeak && (Math.Abs(parentmz.Value - x.MZ) < 0.01)).Any());
            //if (candidates.Any()) return candidates.MaxBy(c => c.Intensity * (c.Peaks.Where(p => p.IsCorePeak).Count() * .25));
        }

        return null;
    }


    #endregion

    #region IMSProvider Members


    public double MassTolerance
    {
        get;
        set;
    }

    #endregion


    public SpectrumInfo GetParentInfo(int scanNumber)
    {
        throw new NotImplementedException();
    }

    private static double[] Base64ToArrayPoints(XElement xElement)
    {
        List<double> results = new List<double>();

        var x = Convert.FromBase64String(xElement.Descendants("data").First().Value);

        int precision = int.Parse(xElement.Descendants("data").First().Attribute("precision").Value);

        int count = int.Parse(xElement.Descendants("data").First().Attribute("length").Value);

        bool IsLittleEndian = xElement.Descendants("data").First().Attribute("endian").Value == "little";

        if (precision == 64)
        {
            for (int i = 0; i < count; i++)
            {
                if (BitConverter.IsLittleEndian == IsLittleEndian)
                {
                    results.Add(BitConverter.ToDouble(x, i * (precision / 8)));
                }
                else
                {
                    // reverse byte order, this likely will perform very badly, but it doesn't matter because it will likely never get called because PCs are always LittleEndian :)
                    Array.Reverse(x, i * (precision / 8), (precision / 8));
                    results.Add(BitConverter.ToDouble(x, i * (precision / 8)));
                }
            }
        }
        else if (precision == 32)
        {
            for (int i = 0; i < count; i++)
            {
                if (BitConverter.IsLittleEndian == IsLittleEndian)
                {
                    results.Add(BitConverter.ToSingle(x, i * (precision / 8)));
                }
                else
                {
                    // reverse byte order, this likely will perform very badly, but it doesn't matter because it will likely never get called because PCs are always LittleEndian :)
                    Array.Reverse(x, i * (precision / 8), (precision / 8));
                    results.Add(BitConverter.ToSingle(x, i * (precision / 8)));
                }
            }
        }
        else
        {
            throw new Exception("Unsupported Numeric Precision Detected: " + precision);
        }

        return results.ToArray();

    }



    public SpecMsLevel MsLevel(int scan)
    {
        return (SpecMsLevel)spectra.Where(s => s.ScanNumber == scan).First().MsLevel;

        //throw new NotImplementedException();
    }

    public Spectrum NativeScanSum(int[] scanNumbers, bool average = false)
    {
        throw new NotImplementedException();
    }
    public Chromatogram ExtractIon(List<Range> targetRange, float startTime = 0, float endTime = float.MaxValue)
    {
        throw new NotImplementedException();
    }
}

//public static class Extensions
//{
//    public static void TryAdd(this IDictionary<int, string> lookup, int key, string value)
//    {
//        if (!lookup.ContainsKey(key)) lookup.Add(key, value);
//    }

//}


////Load xml
//var xdoc = XDocument.Load(input.FullName);

////Run query
//var spectra = from aSpectrum in xdoc.Descendants("spectrumList").First().Descendants("spectrum")
//              select new
//              {
//                  ScanNumber = int.Parse(aSpectrum.Attribute("id").Value),
//                  Details = aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First(),
//                  mzPoints = Base64ToArrayPoints(aSpectrum.Descendants("mzArrayBinary").First()),
//                  intensityPoints = Base64ToArrayPoints(aSpectrum.Descendants("intenArrayBinary").First()),
//                  RT = float.Parse(aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Descendants("cvParam").Where(d => d.Attribute("name").Value == "TimeInMinutes").First().Attribute("value").Value),
//                  ScanMode = aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Descendants("cvParam").Where(d => d.Attribute("name").Value == "ScanMode").First().Attribute("value").Value,
//                  Polarity = aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Descendants("cvParam").Where(d => d.Attribute("name").Value == "Polarity").First().Attribute("value").Value,
//                  MsLevel = int.Parse(aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Attribute("msLevel").Value),
//                  MzStart = double.Parse(aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Attribute("mzRangeStart").Value),
//                  MzStop = double.Parse(aSpectrum.Descendants("spectrumDesc").First().Descendants("spectrumSettings").First().Descendants("spectrumInstrument").First().Attribute("mzRangeStop").Value),
//                  PrecursorDetails = aSpectrum.Descendants("spectrumDesc").First().Descendants("precursorList").Any() ? aSpectrum.Descendants("spectrumDesc").First().Descendants("precursorList").First().Descendants("precursor").First() : null

//                  //    Debug.Print("Retention Time: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "TimeInMinutes").First().Attribute("value").Value);
//                  //    Debug.Print("Scan Mode: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "ScanMode").First().Attribute("value").Value);
//                  //    Debug.Print("Polarity: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "Polarity").First().Attribute("value").Value);
//                  //    Debug.Print("MS Level: " + aSpectrum.Details.Attribute("msLevel").Value);
//                  //    Debug.Print("m/z start: " + aSpectrum.Details.Attribute("mzRangeStart").Value);
//                  //    Debug.Print("m/z stop: " + aSpectrum.Details.Attribute("mzRangeStop").Value);



//              };


////foreach (var aSpectrum in spectra.Take(5))
////{
////    Debug.Print("Scan Number: " + aSpectrum.ScanNumber);
////    Debug.Print("Retention Time: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "TimeInMinutes").First().Attribute("value").Value);
////    Debug.Print("Scan Mode: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "ScanMode").First().Attribute("value").Value);
////    Debug.Print("Polarity: " + aSpectrum.Details.Descendants("cvParam").Where(d => d.Attribute("name").Value == "Polarity").First().Attribute("value").Value);
////    Debug.Print("MS Level: " + aSpectrum.Details.Attribute("msLevel").Value);
////    Debug.Print("m/z start: " + aSpectrum.Details.Attribute("mzRangeStart").Value);
////    Debug.Print("m/z stop: " + aSpectrum.Details.Attribute("mzRangeStop").Value);
////    Debug.Print("Points: ");

////    for (int i = 0; i < aSpectrum.mzPoints.Length; i++)
////        Debug.Print(aSpectrum.mzPoints[i] + ", " + aSpectrum.intensityPoints[i]);

////}



//var output = new FileInfo(input.Name.Replace(".mzdata.xml", ".zip"));

//// Get Number of Spectra
//int SpectraCount = int.Parse(xdoc.Descendants("spectrumList").First().Attribute("count").Value);

////var tempFile = Path.Combine(Path.GetTempPath, result);  //Path.Combine(Application.CommonAppDataPath, Path.GetRandomFileName());
//var tempFile = Path.Combine(output.DirectoryName, Path.ChangeExtension(output.Name, ".txt"));

//try
//{
//    using (System.IO.StreamWriter oWrite = System.IO.File.CreateText(tempFile))
//    {
//        //object varPeakFlags;
//        // object varLabels;

//        //Write Run Header into file
//        oWrite.Write("\r\n\r\nRunHeaderInfo\r\n");
//        oWrite.Write("dataset_id = 0, instrument_id = 0\r\n");
//        oWrite.Write("first_scan = {0}, last_scan = {1}, start_time = {2}, end_time = {3}\r\n", "1", SpectraCount, spectra.First().RT, spectra.Last().RT);
//        oWrite.Write("low_mass = , high_mass = , max_integ_intensity = , sample_volume = 0.000000\r\n");
//        oWrite.Write("sample_amount = 0.000000, injected_volume = 0.000000, vial = 0, inlet = 0\r\n");
//        oWrite.Write("err_flags = 0, sw_rev = 1\r\n");
//        oWrite.Write("Operator = \r\n");
//        oWrite.Write("Acq Date = \r\n");
//        oWrite.Write("comment1 = \r\n");
//        oWrite.Write("comment2 = \r\n");
//        oWrite.Write("acqui_file = \r\n");
//        oWrite.Write("inst_desc = \r\n");
//        oWrite.Write("sample volume units = \r\n");
//        oWrite.Write("sample amount units = \r\n");
//        oWrite.Write("Injected volume units = \r\n");
//        oWrite.Write("Packet Position = \r\n");
//        oWrite.Write("Spectrum Position = \r\n\r\n");

//        Debug.WriteLine("Enumerating over all the scans!  This will take a while! ");

//        // Enumerate over all Scans - index is nScanNumber

//        foreach (var aSpectrum in spectra) // (int nScanNumber = 1; nScanNumber <= SpectraCount; nScanNumber++)
//        {
//            Debug.Print("Processing Scan " + aSpectrum.ScanNumber);

//            string precursor = string.Empty;
//            string scanType = string.Empty;

//            //FTMS + c ESI Full ms [350.00-2000.00]
//            //ITMS + c ESI d Full ms2 532.85@cid35.00 [135.00-1610.00]

//            if (aSpectrum.MsLevel == 2 && aSpectrum.PrecursorDetails != null)
//            {
//                scanType = "Full Scan Type, MS2 Scan";
//                //if (aSpectrum.Info.ToLower().Contains("@")) precursor = aSpectrum.Info.Split('@').FirstOrDefault().Split(' ').LastOrDefault();
//                precursor = aSpectrum.PrecursorDetails.Descendants("ionSelection").First().Descendants("cvParam").First(x => x.Attribute("name").Value == "MassToChargeRatio").Attribute("value").Value;
//            }
//            else if (aSpectrum.MsLevel == 1)
//            {
//                scanType = "Full Scan Type, MS Scan";
//            }
//            else
//            {
//                scanType = "Unknown " + aSpectrum.ScanMode + " Type, MS Scan";
//            }

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
using Agilent.MassSpectrometry.DataAnalysis;
using System.Reflection;

public class AgilentProvider : IMSProvider, IDisposable
{
    private static bool platformOK = false;

    private static void CheckPlatform()
    {
        //var a = Assembly.ReflectionOnlyLoadFrom("MassSpecDataReader.dll");

        if (platformOK == false)
        {
            // Note: the BaseTof.dll is the only platform specific DLL in the set.  All the others seem to be AnyCPU so always report I386.  
            //   That's why I am specifically testing that one DLL.  It is the DECIDER :)
            var a = Assembly.ReflectionOnlyLoadFrom("BaseTof.dll");
            PortableExecutableKinds peKind;
            ImageFileMachine imageFileMachine;
            a.ManifestModule.GetPEKind(out peKind, out imageFileMachine);

            Debug.WriteLine("DLL PATH " + a.Location);

            platformOK = (imageFileMachine == ImageFileMachine.AMD64) == Environment.Is64BitProcess;
        }
        
        // Check if DLL and process are both 64-bit  
        if (platformOK == false)
        {
            throw new Exception("You must run this application on a 64-bit operating system for Agilent file support. ");
        }
    }

    //Dictionary<int, int> scanStartIndex;
    //IDictionary<int, string> filterLookup = new Dictionary<int, string>();
    //List<RawExtractorLine> scanLines = new List<RawExtractorLine>();

    /// <summary>
    /// key is scan number (not the Agilent Scan ID) 
    /// </summary>
    SortedList<int, IMSScanRecord> spectra = null;
    IMsdrDataReader dataAccess = null;
    //RawProvider rms;

    public static int minMS1Counts = 10;
    public static int minMS2Counts = 10;

    public static bool CanReadFormat(FileSystemInfo fileOrFolder)
    {
        //if (Environment.Is64BitProcess == false) throw new Exception("You must run this application on a 64-bit operating system for Agilent file support. "); 
        //CheckPlatform(); 

        var result = fileOrFolder != null && fileOrFolder is DirectoryInfo && fileOrFolder.Exists && fileOrFolder.Name.ToLower().EndsWith(".d");

        // We want to check the platform compatability before we even try to load the raw data file because if it's not compatible, loading the wrong DLL will 
        // throw a BadImageFormatException, which is will force .NET to terminate this process!  To prevent that, we check first!  
        if (result == true) CheckPlatform();

        return result;
    }

    //public class BDFileInformation : IBDAMSScanFileInformation
    //{
    //    bool FileHasMassSpectralData { get; }
    //    double MzScanRangeMinimum { get; }
    //    double MzScanRangeMaximum { get; }
    //    MSScanType ScanTypes { get; }
    //    MSStorageMode SpectraFormat { get; }
    //    IonizationMode IonModes { get; }
    //    DeviceType DeviceType { get; }
    //    MSLevel MSLevel { get; }
    //    ICoreList<double> FragmentorVoltages { get; }
    //    double[] FragmentorVoltage { get; }
    //    ICoreList<double> CollisionEnergies { get; }
    //    double[] CollisionEnergy { get; }
    //    IonPolarity IonPolarity { get; }
    //    IRange[] MRMTransitions { get; }
    //    double[] SIMIons { get; }
    //    int[] ScanMethodNumbers { get; }
    //    long TotalScansPresent { get; }
    //    bool IsFixedCycleLengthDataPresent();
    //    IBDAMSScanTypeInformation GetMSScanTypeInformation(MSScanType scanType);
    //    IBDAMSScanTypeInformation[] GetMSScanTypeInformation();
    //    int ScanTypesInformationCount { get; }
    //    bool Contains(MSScanType key);
    //    IRange MassRange { get; }
    //    bool IsMultipleSpectraPerScanPresent();
    //}

    //public BDAFileInformation 

    public AgilentProvider(DirectoryInfo dataFolder, int minMS1Counts = 10, int minMS2Counts = 10)
    {
        Source = dataFolder;
        Source.Refresh();

        AgilentProvider.minMS1Counts = minMS1Counts;
        AgilentProvider.minMS2Counts = minMS2Counts;
        var fileinfo = new BDAFileInformation();

        //rms = new RawProvider(rmsFilename);
        dataAccess = new MassSpecDataReader();
        //println("MHDA API Version: " + dataAccess.Version);
        dataAccess.OpenDataFile(Source.FullName);

        this.Filename = Source.Name;

        long totalNumScans = dataAccess.MSScanFileInformation.TotalScansPresent;
        spectra = new SortedList<int, IMSScanRecord>();

        for (int i = 0; i < totalNumScans; i++)
        {
            var scanRecord = dataAccess.GetScanRecord(i);
            spectra.Add(i + 1, scanRecord);
        }
    }

    /// <summary>
    /// File name of source of data
    /// </summary>
    public string Filename { get; set; }


    public PointSet Average(int startScan, int endScan, int MsOrderFilter = 1, double startMass = double.NaN, double endMass = double.NaN)
    {
        throw new NotImplementedException();
    }

    public PointSet TIC
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            var chroms = new PointSet();

 //           foreach (var sp in spectra.Where(a => a.Value.MSLevel == MSLevel.MS).ToList())  // filtered for MS1
            foreach (var sp in spectra.ToList())
            {
                    chroms.Add(sp.Value.RetentionTime, (float)(sp.Value.Tic));
            }

            return chroms;


            //if (!spectra.Any()) return new PointSet();

            //if (spectra.Where(s => s.MsLevel == 1).Any())
            //{
            //    return spectra.Where(s => s.MsLevel == 1).ToPointSet(k => k.RT, v => (float)v.intensityPoints.Sum());
            //}
            //else
            //{
            //    return spectra.ToPointSet(k => k.RT, v => (float)v.intensityPoints.Sum());
            //}

            //DataUnit chromunit;
            //DataValueType chromvalueType;

            //long totalNumScans = dataAccess.MSScanFileInformation.TotalScansPresent;
            ////spectra = new SortedList<int, IMSScanRecord>();

            ////for (int i = 0; i < totalNumScans; i++)
            ////{
            ////    var scanRecord = dataAccess.GetScanRecord(i);
            ////    spectra.Add(i + 1, scanRecord);
            ////}

            //IBDAChromFilter filter = new BDAChromFilter();
            //filter.ChromatogramType = ChromType.TotalIon;
            //var chroms = dataAccess.GetChromatogram(filter);




            //var aa = xx.ToList().Count;

            //var bb = aa + 10;
            //var cc = Math.Sqrt(aa + bb);


            //int a = 10;
            //int b = a + 10 + 100;
            //return new PointSet spectra.Where(a => a.Value.MSLevel == MSLevel.MS).Select(a => new PointSet
            //                                                        {
            //                                                            a.Value.RetentionTime,
            //                                                            a.Value.Tic
            //                                                        }).ToList();

            //if (chroms.Any())
            //{
            //    var tic = chroms.First();

            //    return new PointSet(tic.XArray, tic.YArray);
            //}

            //return new PointSet();  // return an empty set if there are no TICs in this file

            //if (chroms.Length > 0)
            //{
            //    chroms[0].GetXAxisInfoChrom(out chromunit, out  chromvalueType);
            //    println("X Axis");
            //    println("======");
            //    println("Data Unit: " + BaseUtilities.GetEnumString(chromunit, true));
            //    println("Data Value type: " + BaseUtilities.GetEnumString(chromvalueType, true));

            //    chroms[0].GetYAxisInfoChrom(out chromunit, out  chromvalueType);
            //    println("Y Axis");
            //    println("======");
            //    println("Data Unit: " + BaseUtilities.GetEnumString(chromunit, true));
            //    println("Data Value type: " + BaseUtilities.GetEnumString(chromvalueType, true));
            //    println(string.Empty);

            //}

        }

    }

    public PointSet BPM
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            var chroms = new PointSet();

            foreach (var sp in spectra.Where(a => a.Value.MSLevel == MSLevel.MS).ToList())
            {
                chroms.Add(sp.Value.RetentionTime, (float)(sp.Value.BasePeakMZ));
            }

            return chroms;

            //IBDAChromFilter filter = new BDAChromFilter();
            //filter.ChromatogramType = ChromType.BasePeak;
            //var chroms = dataAccess.GetChromatogram(filter);

            //if (chroms.Any())
            //{
            //    var tic = chroms.First();

            //    return new PointSet(tic.XArray, tic.YArray);
            //}

            //return new PointSet();  // return an empty set if there are no TICs in this file
        }
    }

    public PointSet BPI
    {
        get
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            var chroms = new PointSet();

            foreach (var sp in spectra.Where(a => a.Value.MSLevel == MSLevel.MS).ToList())
            {
                chroms.Add(sp.Value.RetentionTime, (float)(sp.Value.BasePeakIntensity));
            }

            return chroms;



            //IBDAChromFilter filter = new BDAChromFilter();
            //filter.ChromatogramType = ChromType.BasePeak;
            //var chroms = dataAccess.GetChromatogram(filter);

            //if (chroms.Any())
            //{
            //    var tic = chroms.First();

            //    return new PointSet(tic.XArray, tic.YArray);
            //}

            //return new PointSet();  // return an empty set if there are no TICs in this file
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

            //return new PointSet(spectra.Where(s => s.ScanNumber == scan).First().mzPoints, spectra.Where(s => s.ScanNumber == scan).First().intensityPoints);

            //var result = new PeakList();

            //if this.ScanType == 

            IMsdrPeakFilter filter1 = new MsdrPeakFilter();
            //filter1.AbsoluteThreshold = 10;

            filter1.AbsoluteThreshold = spectra[scan].MSLevel == MSLevel.MSMS ? minMS2Counts : minMS1Counts;

            //filter1.RelativeThreshold = 0.1;
            //filter1.MaxNumPeaks = 0;

            //var rtList = new List<double>();

            //for (int i = Math.Max(scan - 2, 1); i < scan + 2; i++)
            //{
            //    rtList.Add(spectra[i].RetentionTime);
            //}

            //var range = new CenterWidthRange(spectra[scan].RetentionTime, 0.2);

            //var range = new MinMaxRange(spectra[Math.Max(scan - 2, 1)].RetentionTime, spectra[scan + 2].RetentionTime);

            //IBDASpecFilter specFilter = new BDASpecFilter();

            //var x = new List<MinMaxRange>();
            //x.Add(range);

            //specFilter.ScanRange = x.ToArray(); // range;



            //IBDASpecData spec2 = dataAccess.GetSpectrum(range, filter1);//peakFilterOnCentroid=true



            IBDASpecData spec2 = dataAccess.GetSpectrum(spectra[scan].RetentionTime, MSScanType.All, IonPolarity.Mixed, IonizationMode.Unspecified, filter1, true);//peakFilterOnCentroid=true

            //uncomment this section if you want to print out spectrum points
            //double[] mzVals = spec.XArray;
            //float[] aboundanceVals = spec.YArray;
            //for (int j = 0; j < mzVals.Length; j++)
            //{
            //    println(mzVals[j] + ", " + aboundanceVals[j]);                   
            //}



            //var spectrum = spectra.FirstOrDefault(s => s.ScanNumber == scan);
            //var pointCount = spectrum.mzPoints.Length;

            //for (int i = 0; i < spec2.TotalDataPoints; i++)
            //    result.Add(new ClusterPeak() { MZ = spec2.XArray[i], Intensity = spec2.YArray[i] });            



            return new PointSet(spec2.XArray, spec2.YArray);
        }
    }

    public Spectrum Scans(int scanNumber)
    {
        lock (this)
        {
            var result = new Spectrum() { ScanNumber = scanNumber };

            //try
            //{
            IMsdrPeakFilter filter1 = new MsdrPeakFilter();
            //filter1.AbsoluteThreshold = 10;
            filter1.AbsoluteThreshold = spectra[scanNumber].MSLevel == MSLevel.MSMS ? minMS2Counts : minMS1Counts;
            //filter1.RelativeThreshold = 0.1;
            filter1.MaxNumPeaks = 0;

            IBDASpecData spec2 = dataAccess.GetSpectrum(spectra[scanNumber].RetentionTime, MSScanType.All, IonPolarity.Mixed, IonizationMode.Unspecified, filter1, true);//peakFilterOnCentroid=true

            //uncomment this section if you want to print out spectrum points
            //double[] mzVals = spec.XArray;
            //float[] aboundanceVals = spec.YArray;
            //for (int j = 0; j < mzVals.Length; j++)
            //{
            //    println(mzVals[j] + ", " + aboundanceVals[j]);                   
            //}

            //var spectrum = spectra.FirstOrDefault(s => s.ScanNumber == scan);
            //var pointCount = spectrum.mzPoints.Length;

            for (int i = 0; i < spec2.TotalDataPoints; i++)
                result.Add(new ClusterPeak() { MZ = spec2.XArray[i], Intensity = spec2.YArray[i] });
            //}
            //catch (Exception ex1)
            //{
            //    throw new Exception("Getting Spectrum info using the ScanNumber :" + scanNumber);
            //}

            //TODO: Fill in additional Spectrum properties here



            result.ScanNumber = scanNumber;

            return result;
        }
        //return new PeakList(from s in spectra
        //                    where s.ScanNumber == scan
        //                    select new ClusterPeak() )


        //return new PeakList(from s in scanLines
        //                    where s.scan == scan
        //                    select new ClusterPeak() { MZ = s.mz, Intensity = s.Intensity });
    }


    public Spectrum NativeScanSum(int[] scanNumbers, bool average = false)
    {

        IMsdrPeakFilter pf = new MsdrPeakFilter();

        pf.AbsoluteThreshold = 10;

        //var range = new MinMaxRange(1, 2);

        //var x = new List<MinMaxRange>();

        //x.Add(range);

        //IRange r = new MinMaxRange(1, 2);

        IBDASpecFilter sf = new BDASpecFilter();
        sf.AverageSpectrum = average;

        
        sf.ScanIds = scanNumbers.Select(i=> spectra[i].ScanID).ToArray();  // convert scan numbers to Agilent Scan IDs
        //  sf.ScanRange = new[] { new MinMaxRange(0.287, 0.488) };  // scan range is RT (min)?
        sf.SpectrumType = SpecType.TofMassSpectrum;
        //sf.MSLevelFilter = MSLevel.MSMS;


        //Not implemented exception
        //IBDASpecData spec2 = dataAccess.GetSpectrum(r, pf);

        var spec2 = dataAccess.GetSpectrum(sf, pf);

        //for (int i = 0; i < spec2[0].TotalDataPoints; i++)
        //    Debug.Print(spec2[0].XArray[i] + "\t" + spec2[0].YArray[i]);

        return spec2[0].ToSpectrum();
        



    }

    public int ScanIndex(double retentionTime)
    {
        return spectra.OrderBy(s => Math.Abs(s.Value.RetentionTime - retentionTime)).First().Key;

        //Create a chromatogram filter to let the scans that match 
        //the filter criteria to pass through
        //IBDAChromFilter chromFilter = new BDAChromFilter();
        //chromFilter.ChromatogramType = ChromType.TotalIon;
        //chromFilter.DesiredMSStorageType = DesiredMSStorageType.PeakElseProfile; //This means to use peak scans if available, otherwise use profile scans
        //chromFilter.MSLevelFilter = MSLevel.All;
        //chromFilter.MSScanTypeFilter = MSScanType.AllMS;
        //IBDAChromData[] chroms = dataAccess.GetChromatogram(chromFilter);//expect 1 chromatogram because we're not asking it to separate by scan segments, etc.
        //IBDAChromData chrom = chroms[0];

        //double[] retTime = chrom.XArray;

        //return Array.IndexOf(retTime, retTime.OrderBy(s => Math.Abs(retentionTime - s)).First());

        //return (float)retTime[scan];

        //return spectra.OrderBy(s => Math.Abs(retentionTime - s.RT)).First().ScanNumber;


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
        //var spec = spectra.Where(s => s.ScanNumber == scan).First();

        return (spectra[scan].IonPolarity == IonPolarity.Positive ? "+" : "") + System.Enum.GetName(typeof(IonizationMode), spectra[scan].IonizationMode) + " " + ((spectra[scan].MSLevel == MSLevel.MSMS) ? "MS2 " : "MS ") + System.Enum.GetName(typeof(MSScanType), spectra[scan].MSScanType) + " (#" + scan + " - " + spectra[scan].RetentionTime.ToString("0.000") + " min)";

        //return "Scan " + spec.ScanNumber + ", " + spec.ScanMode + ", MS" + spec.MsLevel + ", FTMS";
    }

    public double? ParentMZ(int scan)
    {
        try
        {
            return spectra[scan].MSLevel == MSLevel.MSMS ? spectra[scan].MZOfInterest : (double?)null;


            IBDASpecData spec = dataAccess.GetSpectrum(spectra[scan].RetentionTime, MSScanType.ProductIon, IonPolarity.Mixed, IonizationMode.Unspecified, null);

            int precursorCount = 0;

            double[] precursorIons = spec.GetPrecursorIon(out precursorCount);

            if (precursorCount == 1 && precursorIons != null)
            {
                return precursorIons[0];

                //print("Precursor Ions:");
                //for (int j = 0; j < precursorIons.Length; j++)
                //{
                //    print(precursorIons[j] + " ");
                //}
                //println("");
                //int charge;
                //double intensity;
                //if (spec.GetPrecursorCharge(out charge))
                //    println("  charge: " + charge);
                //else
                //    println("  no charge avaialble");
                //if (spec.GetPrecursorIntensity(out intensity))
                //    println("  intensity: " + intensity);
                //else
                //    println("  no intensity available");
            }
            else
            {
                return null;
                //println("No precursor ions");
            }
        }
        catch (Exception)
        {
            return null;
            //Debug.Print("I am at the exception. Scan Number :" + scan);
            //int a = ex.StackTrace.Length;
            //int b = 20 + a;
            //int c = a + b;
            //throw new Exception(scan.ToString());
            //throw ex;
        }

        //throw new NotImplementedException();
    }

    //public List<SpecPointCollection> ThermoRange(int startvalue, int endvalue, string startFilter = "")
    //{
    //    throw new NotImplementedException();
    //}

    public string ScanType(int scan)
    {
        // returns "MS1" or "MS2"

        return (spectra[scan].MSLevel == MSLevel.MSMS) ? "MS2" : "MS1";

        //return "MS1";

        //return "MS" + spectra.Where(s => s.ScanNumber == scan).First().MsLevel;
    }

    public float RetentionTime(int scan)
    {
        return (float)spectra[scan].RetentionTime;

        //Create a chromatogram filter to let the scans that match 
        //the filter criteria to pass through
        //IBDAChromFilter chromFilter = new BDAChromFilter();
        //chromFilter.ChromatogramType = ChromType.TotalIon;
        //chromFilter.DesiredMSStorageType = DesiredMSStorageType.PeakElseProfile; //This means to use peak scans if available, otherwise use profile scans
        //chromFilter.MSLevelFilter = MSLevel.All;
        //chromFilter.MSScanTypeFilter = MSScanType.AllMS;
        //IBDAChromData[] chroms = dataAccess.GetChromatogram(chromFilter);//expect 1 chromatogram because we're not asking it to separate by scan segments, etc.
        //IBDAChromData chrom = chroms[0];

        //double[] retTime = chrom.XArray;

        //return (float)retTime[scan];

        //long totalNumScans = dataAccess.MSScanFileInformation.TotalScansPresent;
        //for (int i = 0; i < totalNumScans; i++)
        //{
        //    IMSScanRecord scanRecord = dataAccess.GetScanRecord(i);
        //    println("MSSCANRECORD - Scan ID : " + scanRecord.ScanID + " RT = " + scanRecord.RetentionTime.ToString() + " MSLevel = " + scanRecord.MSLevel.ToString());
        //}
    }

    public bool Contains(int scan)
    {
        return spectra.Any(s => s.Value.ScanID == scan);

        //return filterLookup.ContainsKey(scan);
    }

    public int? NextScan(int currentScan)
    {
        //return currentScan + 1;

        return (from s in spectra
                where s.Key > currentScan
                orderby s.Key
                select s.Key).Cast<int?>().FirstOrDefault();
    }

    public int? PreviousScan(int currentScan)
    {
        return (from s in spectra
                where s.Key < currentScan
                orderby s.Key descending
                select s.Key).Cast<int?>().FirstOrDefault();

    }

    public List<SpectrumInfo> GetParentInfo(CancellationToken cToken = default(CancellationToken), int startScan = 1, int endScan = int.MaxValue)
    {
        //TODO: Implement this by using the new Detect ParentIon Method
        //Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        //lock (this)
        //{
        //    List<SpectrumInfo> mspec = new List<SpectrumInfo>();

        //    //  try
        //    // {
        //    if (!spectra.Any()) return mspec;

        //    SpectrumCollection scan = rms.Scans;
        //    //int i = scan.Count - 1;
        //    Cluster cluster = null;

        //    foreach (Spectrum sc in scan)  //scan.Reverse())
        //    {
        //        if (cToken != default(CancellationToken) && cToken.IsCancellationRequested) return mspec;

        //        //if (sc.ScanType == "MS2")
        //        //{
        //        cluster = DetectParentIon(sc.ScanNumber.Value);

        //        if (cluster != null)
        //            mspec.Add(new SpectrumInfo
        //            {
        //                ParentIon = cluster,
        //                RelativeIntensity = sc.IonCurrent.Value,
        //                RetentionTime = Math.Round(Convert.ToDouble(sc.StartTime), 3),
        //                ScanNumber = sc.ScanNumber.Value,
        //                Title = sc.Title,
        //                IsParentScan = sc.ScanType == "MS1" ? true : false,
        //                MsLevel = sc.ScanType == "MS1" ? SpecMsLevel.MS : SpecMsLevel.MSMS
        //            });
        //        else
        //            mspec.Add(new SpectrumInfo
        //            {
        //                RetentionTime = Math.Round(Convert.ToDouble(sc.StartTime), 3),
        //                ScanNumber = sc.ScanNumber.Value,
        //                Title = sc.Title,
        //                //TimeSort = 999999,
        //                RelativeIntensity = sc.IonCurrent.Value,
        //                IsParentScan = sc.ScanType == "MS1" ? true : false,
        //                MsLevel = sc.ScanType == "MS1" ? SpecMsLevel.MS : SpecMsLevel.MSMS
        //            });
        //        //}
        //        //i--;
        //    }

        //    #region sorting

        //    List<SpectrumInfo> m = new List<SpectrumInfo>();
        //    m = mspec.OrderBy(s => s.ParentMass != 0).ThenBy(a => a.RetentionTime).ToList();
        //    mspec = m.SortMergeSpectra();

        //    #endregion
        //    // }
        //    //catch (Exception ex)
        //    //{
        //    //    return new List<SpectrumInfo>();
        //    //}

        //    return mspec.ToList();
        //}


        lock (this)
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            int totalNumScans = (int)dataAccess.MSScanFileInformation.TotalScansPresent;

            if (totalNumScans == 0) return new List<SpectrumInfo>();

            List<SpectrumInfo> mspec = new List<SpectrumInfo>();
            //Cluster cluster = null;

            for (int i = 1; i <= totalNumScans; i++)
            {
                if (cToken != default(CancellationToken) && cToken.IsCancellationRequested) return mspec;

                if (i < startScan || i > endScan) continue;

                //cluster = DetectParentIon(i);

                //IMSScanRecord sc = dataAccess.GetScanRecord(i);
                //if (spectra[i].MSLevel == MSLevel.MSMS)
                // {
                mspec.Add(new SpectrumInfo
                {
                    ParentIon = DetectParentIon(i),
                    RetentionTime = Math.Round(Convert.ToDouble(spectra[i].RetentionTime), 3),
                    ScanNumber = i,
                    IsParentScan = spectra[i].MSLevel == MSLevel.MS, // sc.ScanType == "MS1" ? true : false,
                    Title = Title(i),
                    Intensity = (float)spectra[i].Tic,
                    RelativeIntensity = (float)spectra[i].Tic,
                    MsLevel = (SpecMsLevel)spectra[i].MSLevel,
                    DataSource = this
                });
                //}

                //println("MSSCANRECORD - Scan ID : " + scanRecord.ScanID + " RT = " + scanRecord.RetentionTime.ToString() + " MSLevel = " + scanRecord.MSLevel.ToString());
            }


            return mspec;
        }
        //throw new NotImplementedException();
    }

    public List<SpectrumInfo> GetInitialParentInfo()
    {
        lock (this)
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            long totalNumScans = dataAccess.MSScanFileInformation.TotalScansPresent;

            if (totalNumScans == 0) return new List<SpectrumInfo>();

            List<SpectrumInfo> mspec = new List<SpectrumInfo>();

            for (int i = 1; i <= totalNumScans; i++)
            {
                //IMSScanRecord sc = dataAccess.GetScanRecord(i);
                //if (spectra[i].MSLevel == MSLevel.MSMS)
                //{
                mspec.Add(new SpectrumInfo
                {
                    RetentionTime = spectra[i].RetentionTime,
                    ScanNumber = i,
                    Title = Title(i),
                    RelativeIntensity = (float)spectra[i].Tic,
                    Intensity = (float)spectra[i].Tic,
                    MsLevel = (SpecMsLevel)spectra[i].MSLevel
                });
                //}

                //println("MSSCANRECORD - Scan ID : " + scanRecord.ScanID + " RT = " + scanRecord.RetentionTime.ToString() + " MSLevel = " + scanRecord.MSLevel.ToString());
            }


            return mspec;

            //List<SpectrumInfo> mspec = new List<SpectrumInfo>();
            //if (!spectra.Any())
            //{
            //    return new List<SpectrumInfo>();
            //}

            //try
            //{
            //    //SpectrumCollection scan = spectra.Scans;
            //    foreach (var sc in spectra)
            //    {
            //        if (sc.MsLevel == 2)
            //        {
            //            mspec.Add(new SpectrumInfo
            //            {
            //                RetentionTime = sc.RT,
            //                ScanNumber = sc.ScanNumber,
            //                Title = sc.ScanMode + "FTMS",
            //                RelativeIntensity = (float)sc.intensityPoints.Sum()
            //            });
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    return new List<SpectrumInfo>();
            //}
            //return mspec;
        }
    }


    #region IMSProvider Members

    IEnumerable<Spectrum> IMSProvider.ScanRange(int startIndex, int endIndex, string filter)
    {
        var results = new List<Spectrum>();

        MSLevel newmslevel = new MSLevel();

        if (filter == "ms2")
        {
            newmslevel = MSLevel.MSMS;
            foreach (var aScan in spectra.Where(s => s.Key >= startIndex && s.Key <= endIndex && s.Value.MSLevel == newmslevel))
                results.AddSkipNull(this.Scans(aScan.Key));
        }
        else if (filter == "ms")
        {
            newmslevel = MSLevel.MS;
            foreach (var aScan in spectra.Where(s => s.Key >= startIndex && s.Key <= endIndex && s.Value.MSLevel == newmslevel))
                results.AddSkipNull(this.Scans(aScan.Key));
        }
        else
        {
            newmslevel = MSLevel.All;
            foreach (var aScan in spectra.Where(s => s.Key >= startIndex && s.Key <= endIndex))
                results.AddSkipNull(this.Scans(aScan.Key));
        }

        //foreach (var aScan in spectra.Where(s => s.Key >= startIndex && s.Key <= endIndex && Title(s.Key).Contains(filter)))
        //    results.Add(this.Scans(aScan.Key));

        return results;
    }

    #endregion

    #region IMSProvider Members


    public List<Cluster> DetectIons(int scanNum = 1, int minCharge = 1, int maxCharge = 30, double massRangeMin = 0, double massRangeMax = double.MaxValue)
    {
        try
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
        }
        catch (Exception)
        {
            return new List<Cluster>();
        }

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
        return spectra.ContainsKey(scan);
    }

    #endregion

    #region IMSProvider Members

    public void SetParentInfo(SpectrumInfo sInfo, bool useProductScan = false)
    {
        try
        {
            Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            if (sInfo.ParentZ.HasValue && sInfo.ParentMass.HasValue) return;

            sInfo.ParentIon = DetectParentIon(sInfo.ScanNumber, useProductScan);
        }
        catch (Exception ex)
        {
            //int a = ex.StackTrace.Length;
            //int b = 20 + a;
            //int c = a + b;
            //throw new Exception(Convert.ToString(sInfo.ScanNumber));
            //throw new Exception(scan.ToString());
        }


        //throw new NotImplementedException();
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

    /// <summary>
    /// Gets the spectrum info with parent detected
    /// </summary>
    /// <param name="scanNumber"></param>
    /// <returns></returns>
    public Cluster DetectParentIon(int scanNumberOfMs2, bool useProductScan = false)
    {
        lock (this)
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


            SignalProcessing.PointSet a1 = new PointSet();
            SignalProcessing.PointSet b1 = new PointSet();

            SignalProcessing.PointSet p1 = new PointSet();

            List<Cluster> ionCandidatesDetected = new List<Cluster>(), pIons = new List<Cluster>();

            if (useProductScan)
            {
                // If we have a targeted m/z, see if there is some unfragmented ion left in the ms2 scan to detect charge, otherwise, assume the basepeak is the residual parent
                double targetMZ = parentmz.HasValue ? parentmz.Value : this.spectra[scanNumberOfMs2].BasePeakMZ;

                //TODO: Find the base peak of the MS2 scan.  Take all peaks with at least 50% of that intensity.  Charge detect and assume the largest mass ion is the parent.  
                var basePeakRange = this[scanNumberOfMs2].Range(targetMZ - 3d, targetMZ + 3d).ToPointSet(k => k.Key, v => v.Value);

                ionCandidatesDetected = new ChargeDetector(basePeakRange).DetectChargeStates(MassTolerance);
            }
            else if (parentmz.HasValue)
            {
                //TODO: should we limit the RT range here?  depend on gradient.  Infusion would have no limit...  hmmm.  

                // get surrounding MS1 scans to merge together
                var beforeMS = this.PreviousScan(parentScanNumber.Value);
                var afterMS = this.NextScan(parentScanNumber.Value);

                while (beforeMS.HasValue && (this.ScanType(beforeMS.Value) != "MS1"))
                    beforeMS = this.PreviousScan(beforeMS.Value);

                while (afterMS.HasValue && (this.ScanType(afterMS.Value) != "MS1"))
                    afterMS = this.NextScan(afterMS.Value);



                {
                    List<Cluster> parentIons = new List<Cluster>();

                    // Use peak detected scans with limited m/z range to improve performance

                    if (afterMS.HasValue)
                    {
                        a1 = this[afterMS.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                        if (a1.Count != 0)
                        {
                            var aIons = new ChargeDetector(a1).DetectChargeStates(MassTolerance);
                            parentIons.AddRange(aIons);
                        }
                    }

                    if (beforeMS.HasValue)
                    {
                        b1 = this[beforeMS.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                        if (b1.Count != 0)
                        {
                            var bIons = new ChargeDetector(b1).DetectChargeStates(MassTolerance);
                            parentIons.AddRange(bIons);
                        }
                    }

                    p1 = this[parentScanNumber.Value].Range(parentmz.Value - 2d, parentmz.Value + 2d).ToPointSet(k => k.Key, v => v.Value);
                    if (p1.Count > 0)
                    {
                        pIons = new ChargeDetector(p1).DetectChargeStates(MassTolerance);
                        parentIons.AddRange(pIons);
                        ionCandidatesDetected = SignalProcessor.ConsolidateIons(parentIons);
                    }
                    else
                    {
                        ionCandidatesDetected = new List<Cluster>();
                    }
                }                
            }

            if (!ionCandidatesDetected.Any() && !pIons.Any()) return null;

            // Added a bias toward ions with more core peaks, and added selection bias for clusters that include the parent M/Z

            if (parentmz.HasValue)
            {
                // if parent m/z is an integer, open allowed delta to 1, otherwise, use somthing smaller just to weed out the junk.  
                double delta = parentmz == (int)parentmz ? 1 : 0.1;
                delta = 1;  //temporary fix 
                ionCandidatesDetected = ionCandidatesDetected.Where(p => p.Peaks.Where(x => Math.Abs(parentmz.Value - x.MZ) < delta).Any()).ToList();
            }

            if (ionCandidatesDetected.Any()) return ionCandidatesDetected.MaxBy(c => c.Intensity * (c.Peaks.Where(p => p.IsCorePeak).Count() * .25));

            // If we didn't find any ions in the merge of the 3 scans, let's just look at the original parent MS scan before we give up.  
            //candidates = pIons.Where(p => p.Peaks.Where(x => x.IsCorePeak && (Math.Abs(parentmz.Value - x.MZ) < 0.01)).Any());
            //if (candidates.Any()) return candidates.MaxBy(c => c.Intensity * (c.Peaks.Where(p => p.IsCorePeak).Count() * .25));

            return null;
        }
    }


    #endregion

    #region IMSProvider Members


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
    #endregion


    public SpectrumInfo GetParentInfo(int scanNumber)
    {

        Debug.Print("Method Call to " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        lock (this)
        {
            var scaninfo = dataAccess.GetScanRecord(scanNumber);

            return new SpectrumInfo
            {
                ParentIon = DetectParentIon(scanNumber),
                Intensity = (float)(scaninfo.BasePeakIntensity), /// rms.Scans[scanNumber].IonCurrent.Value,
                RelativeIntensity = (float)(scaninfo.BasePeakIntensity), /// rms.Scans[scanNumber].IonCurrent.Value,
                RetentionTime = scaninfo.RetentionTime, /// Math.Round(Convert.ToDouble(rms.Scans[scanNumber].StartTime), 3),
                ScanNumber = scanNumber, /// rms.Scans[scanNumber].ScanNumber.Value,
                Title = "None",  //rms.Scans[scanNumber].Title,
                IsParentScan = scaninfo.MSLevel == MSLevel.MS ? true : false, /// rms.Scans[scanNumber].ScanType == "MS1" ? true : false,
                MsLevel = scaninfo.MSLevel == MSLevel.MS ? SpecMsLevel.MS : SpecMsLevel.MSMS, /// rms.Scans[scanNumber].ScanType == "MS1" ? SpecMsLevel.MS : SpecMsLevel.MSMS
            };
        }
    }


    public string GetActivationType(int scanNumber)
    {
        IBDASpecData spec2 = dataAccess.GetSpectrum(spectra[scanNumber].RetentionTime, MSScanType.All, IonPolarity.Mixed, IonizationMode.Unspecified);

        //Enum.Parse()

        return (spec2 as BDASpecData).MSOverallScanRecordInformation.FragmentationMode.ToString().ToUpper();


        
        //return string.Empty;

        //lock (this)
        //{
           
        //    var cluster = DetectParentIon(scanNumber);
        //    if (cluster != null)
        //    {

        //        var scaninfo = dataAccess.GetScanRecord(scanNumber);
                
        //        return rms.Scans[scanNumber].Activation;
        //    }
        //    return string.Empty;
        //}
    }

    public void Dispose()
    {
        if (dataAccess != null) dataAccess.CloseDataFile();
    }


    public SpecMsLevel MsLevel(int scan)
    {
        return (SpecMsLevel)spectra[scan].MSLevel;
    }

    public Chromatogram ExtractIon(List<Range> mzRangeList, float startTime = 0, float endTime = float.MaxValue)
    {
        IBDAChromFilter filter = new BDAChromFilter();
        filter.ChromatogramType = ChromType.ExtractedIon;
        filter.MSScanTypeFilter = MSScanType.AllMS;
        IRange[] ranges = new IRange[1];

        ranges[0] = new MinMaxRange(mzRangeList[0].Start, mzRangeList[0].End);//Ex: this is another way to do the same thing
        filter.IncludeMassRanges = ranges;
        IBDAChromData[] chroms = dataAccess.GetChromatogram(filter);

        double[] doubleArray = Array.ConvertAll(chroms[0].YArray, x => (double)x);
        return new Chromatogram(chroms[0].XArray, doubleArray);
    }
}

public static partial class Extensions
{
    public static string Title(this IMSScanRecord record, int scanID = 0)
    {
        return (record.IonPolarity == IonPolarity.Positive ? "+" : "") + System.Enum.GetName(typeof(IonizationMode), record.IonizationMode) + " " + ((record.MSLevel == MSLevel.MSMS) ? "MS2 " : "MS ") + System.Enum.GetName(typeof(MSScanType), record.MSScanType) + " (#" + scanID + " - " + record.RetentionTime.ToString("0.000") + " min)";
    }

    public static Spectrum ToSpectrum(this IBDASpecData spec)
    {
        var result = new Spectrum() { ScanNumber = spec.ScanId };

        //try
        //{
        IMsdrPeakFilter filter1 = new MsdrPeakFilter();
        //filter1.AbsoluteThreshold = 10;
        filter1.AbsoluteThreshold = spec.MSLevelInfo == MSLevel.MSMS ? AgilentProvider.minMS2Counts : AgilentProvider.minMS1Counts;
        //filter1.RelativeThreshold = 0.1;
        filter1.MaxNumPeaks = 0;

        for (int i = 0; i < spec.TotalDataPoints; i++)
            result.Add(new ClusterPeak() { MZ = spec.XArray[i], Intensity = spec.YArray[i] });

        return result;
    }
}


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

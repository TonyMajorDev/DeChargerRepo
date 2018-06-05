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
using System.Threading;

public class TextProvider : IMSProvider
{

    //Dictionary<int, int> scanStartIndex;
    IDictionary<int, string> filterLookup = new Dictionary<int, string>();
    List<RawExtractorLine> scanLines = new List<RawExtractorLine>();

    public TextProvider(FileStream txtFileStream)
    {
        this.Source = new FileInfo(txtFileStream.Name);

        StreamReader reader = new StreamReader(txtFileStream);
        reader.ReadLine(); // throw out the header!

        while (!reader.EndOfStream)
        {
            var tempLine = reader.ReadLine();

            if (tempLine.Trim() != string.Empty && tempLine.Split('\t').Count() >= 9)
            {
                scanLines.Add(new RawExtractorLine(tempLine, ref filterLookup));
            }
        }
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
    /// File name of source of data
    /// </summary>
    public string Filename { get { return this.Source.Name; } }


    public PointSet Average(int startScan, int endScan, int MsOrderFilter = 1, double startMass = double.NaN, double endMass = double.NaN)
    {
        throw new NotImplementedException();
    }

    public string GetActivationType(int scanNumber)
    {
        return string.Empty;
        //throw new NotImplementedException();
    }

    public PointSet TIC
    {
        get
        {
            return null;
        }

    }

    public PointSet BPM
    {
        get
        {
            return null;
        }

    }

    public PointSet BPI
    {
        get
        {
            return null;
        }

    }

    public PointSet this[int scan]
    {
        get
        {
            return (from s in scanLines
                    where s.scan == scan
                    select s).ToPointSet(k => k.mz, v => v.Intensity);
        }
    }

    public Spectrum Scans(int scan)
    {
        return new Spectrum(from s in scanLines
                            where s.scan == scan
                            select new ClusterPeak() { MZ = s.mz, Intensity = s.Intensity });
    }

    public int ScanIndex(double retentionTime)
    {
        throw new NotImplementedException();

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
        return filterLookup[scan];
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
        throw new NotImplementedException();
    }

    public float RetentionTime(int scan)
    {
        throw new NotImplementedException();
    }

    public bool Contains(int scan)
    {
        return filterLookup.ContainsKey(scan);
    }

    public int? NextScan(int currentScan)
    {
        return (from s in filterLookup.Keys
                where s > currentScan
                orderby s
                select s).Cast<int?>().FirstOrDefault();
    }

    public int? PreviousScan(int currentScan)
    {
        return (from s in filterLookup.Keys
                where s < currentScan
                orderby s descending
                select s).Cast<int?>().FirstOrDefault();

    }

    public List<SpectrumInfo> GetParentInfo(CancellationToken cToken = default(CancellationToken), int startScan = 1, int endScan = int.MaxValue)
    {
        throw new NotImplementedException();
    }

    public List<SpectrumInfo> GetInitialParentInfo()
    {
        throw new NotImplementedException();
    }

    internal class RawExtractorLine
    {
        public RawExtractorLine() { }

        public RawExtractorLine(string lineToParse, ref IDictionary<int, string> filters)
        {
            var line = lineToParse.Split('\t');

            scan = int.Parse(line[0]);
            filters.TryAdd(scan, line[1]);
            this.scan = scan;
            this.peak_index = int.Parse(line[2]);
            this.mz = double.Parse(line[3]);
            this.Intensity = float.Parse(line[4]);
            this.Resolution = int.Parse(line[5]);
            this.Baseline = double.Parse(line[6]);
            this.Noise = float.Parse(line[7]);
            this.Charge = int.Parse(line[8]);
            this.Mass = double.Parse(line[9]);
        }


        public int scan { get; set; }
        public string filter { get; set; }
        public int peak_index { get; set; }
        public double mz { get; set; }
        public float Intensity { get; set; }
        public int Resolution { get; set; }
        public double Baseline { get; set; }
        public float Noise { get; set; }
        public int Charge { get; set; }
        public double Mass { get; set; }

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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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


    public SpecMsLevel MsLevel(int scan)
    {
        throw new NotImplementedException();
    }


    public Chromatogram ExtractIon(List<Range> targetRange, float startTime = 0, float endTime = float.MaxValue)
    {
        throw new NotImplementedException();
    }

    public Spectrum NativeScanSum(int[] scanNumbers, bool average = false)
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

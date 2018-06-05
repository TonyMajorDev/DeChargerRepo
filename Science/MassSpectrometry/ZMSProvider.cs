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

public class ZMSProvider : IMSProvider
{
    ZipFile zms;
    Dictionary<int, ZMSIndexLine> zmsIndex;

    public static bool CanReadFormat(FileSystemInfo fileOrFolder)
    {
        return fileOrFolder != null && fileOrFolder is FileInfo && fileOrFolder.Exists && fileOrFolder.Name.ToLower().EndsWith(".zms");
    }

    public ZMSProvider(FileStream zmsFileStream)
    {
        this.Source = new FileInfo(zmsFileStream.Name);

        zms = ZipFile.Read(zmsFileStream);

        var selection =
            from z in zms
            where z.FileName == "index.txt"
            select z;

        var zmsMemory = new MemoryStream();

        selection.First().Extract(zmsMemory);

        zmsMemory.Position = 0;
        var sr = new StreamReader(zmsMemory);
        var header = sr.ReadLine();

        zmsIndex = new Dictionary<int, ZMSIndexLine>();

        while (!sr.EndOfStream)
        {
            var z = new ZMSIndexLine(sr.ReadLine());
            zmsIndex.Add(z.ScanNumber, z);
        }
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

    public SpectrumInfo ForceParentInfo(SpectrumInfo sInfo)
    {
        return new SpectrumInfo();
    }
    public string GetActivationType(int scanNumber)
    {
        return string.Empty;
        //throw new NotImplementedException();
    }
    /// <summary>
    /// File name of source of data
    /// </summary>
    public string Filename { get { return this.Source.Name; } }

    public PointSet Average(int startScan, int endScan, int MsOrderFilter = 1, double startMass = double.NaN, double endMass = double.NaN)
    {
        throw new NotImplementedException();
    }
    public List<SpectrumInfo> GetParentScans()
    {
        throw new NotImplementedException();
    }
    public int ScanIndex(double retentionTime)
    {
        // Find closest point

        return (from s in zmsIndex
                orderby Math.Abs(s.Value.RetentionTime - retentionTime)
                select s.Key).First();
    }

    public string Title(int scan)
    {
        return zmsIndex[scan].Filter;
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


    //public SpecPointCollection ThermoPoints(int ScanNumber)
    //{
    //    throw new NotImplementedException();
    //}

    public float RetentionTime(int scan)
    {
        return zmsIndex[scan].RetentionTime;
    }

    public bool Contains(int scan)
    {
        return zmsIndex.ContainsKey(scan);
    }

    public int? NextScan(int currentScan)
    {
        return (from s in zmsIndex.Values.Select(i => i.ScanNumber)
                where s > currentScan
                orderby s
                select s).Cast<int?>().FirstOrDefault();
    }

    public int? PreviousScan(int currentScan)
    {
        return (from s in zmsIndex.Values.Select(i => i.ScanNumber)
                where s < currentScan
                orderby s descending
                select s).Cast<int?>().FirstOrDefault();

    }

    public PointSet TIC
    {
        get
        {
            if (zmsIndex.Where(s => s.Value.Filter.Contains("Full ms ")).Any())
            {
                return zmsIndex.Where(s => s.Value.Filter.Contains("Full ms ")).ToPointSet(k => k.Value.RetentionTime, v => v.Value.TIC);
            }
            else
            {
                return zmsIndex.ToPointSet(k => k.Value.RetentionTime, v => v.Value.TIC);
            }
        }
    }


    public PointSet BPM
    {
        get
        {
            if (zmsIndex.Where(s => s.Value.Filter.Contains("Full ms ")).Any())
            {
                return zmsIndex.Where(s => s.Value.Filter.Contains("Full ms ")).ToPointSet(k => k.Value.RetentionTime, v => (float)v.Value.BPM);
            }
            else
            {
                return zmsIndex.ToPointSet(k => k.Value.RetentionTime, v => (float)v.Value.BPM);
            }
        }
    }

    public PointSet BPI
    {
        get
        {
            if (zmsIndex.Where(s => s.Value.Filter.Contains("Full ms ")).Any())
            {
                return zmsIndex.Where(s => s.Value.Filter.Contains("Full ms ")).ToPointSet(k => k.Value.RetentionTime, v => v.Value.BPI);
            }
            else
            {
                return zmsIndex.ToPointSet(k => k.Value.RetentionTime, v => v.Value.BPI);
            }
        }
    }

    public PointSet this[int scan]
    {
        get
        {
            var selection =
                from z in zms
                where z.FileName == scan.ToString() + ".txt"
                select z;

            var scanMemory = new MemoryStream();

            selection.First().Extract(scanMemory);

            scanMemory.Position = 0;
            var sr = new StreamReader(scanMemory);
            var header = sr.ReadLine();

            var returnScan = new PointSet();

            while (!sr.EndOfStream)
            {
                var o = new ZMSLine(sr.ReadLine());
                returnScan.Add(o.MZ, o.Intensity);
            }

            return returnScan;
        }
    }

    public Spectrum Scans(int scan)
    {
        var selection =
            from z in zms
            where z.FileName == scan.ToString() + ".txt"
            select z;

        var scanMemory = new MemoryStream();
        var returnScan = new Spectrum() { ScanNumber = scan };

        //TODO: Fill in additional Spectrum properties here

        if (selection.Any())
        {
            selection.First().Extract(scanMemory);

            scanMemory.Position = 0;
            var sr = new StreamReader(scanMemory);
            var header = sr.ReadLine();

            while (!sr.EndOfStream)
            {
                var o = new ZMSLine(sr.ReadLine());
                //returnScan.Add(((o.MZ * o.Charge) - (o.Charge * 1.00782503214)), o.Intensity);
                returnScan.Add(new ClusterPeak() { MZ = o.MZ, Intensity = o.Intensity, Z = o.Charge });
            }
        }

        return returnScan;
    }

    public List<SpectrumInfo> GetParentInfo(CancellationToken cToken = default(CancellationToken), int startScan = 1, int endScan = int.MaxValue)
    {
        throw new NotImplementedException();
    }

    public List<SpectrumInfo> GetInitialParentInfo()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Spectrum> ScanRange(int startIndex, int endIndex, string filter)
    {
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




    internal class ZMSLine
    {
        public ZMSLine() { }

        public ZMSLine(string lineToParse)
        {
            var line = lineToParse.Split('\t');

            this.MZ = double.Parse(line[0]);
            this.Intensity = float.Parse(line[1]);
            this.Charge = int.Parse(line[2]);
        }


        public double MZ { get; set; }
        public float Intensity { get; set; }
        public int Charge { get; set; }
    }

    internal class ZMSIndexLine
    {
        public ZMSIndexLine() { }

        public ZMSIndexLine(string lineToParse)
        {
            var line = lineToParse.Split('\t');

            this.ScanNumber = int.Parse(line[0]);
            this.RetentionTime = float.Parse(line[1]);
            this.TIC = float.Parse(line[2]);
            this.BPM = double.Parse(line[3]);
            this.BPI = float.Parse(line[4]);
            this.Filter = line[5];
            //filters.TryAdd(ScanNumber, Filter);
        }


        public int ScanNumber { get; set; }
        public float RetentionTime { get; set; }
        public float TIC { get; set; }
        public double BPM { get; set; }
        public float BPI { get; set; }
        public string Filter { get; set; }

    }

    public class PointSetList : IEnumerable<PointSet>
    {

        #region IEnumerable<PointSet> Members

        public IEnumerator<PointSet> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    #region IMSProvider Members


    public List<Cluster> DetectIons(int scanNum = 1, int minCharge = 1, int maxCharge = 30, double massRangeMin = 0, double massRangeMax = double.MaxValue)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IMSProvider Members


    public IEnumerable<IEnumerable<Cluster>> DetectIonsForScanRange(int startIndex, int endIndex, string containsFilter = "", string startFilter = "", int minCharge = 1, int maxCharge = 30)
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



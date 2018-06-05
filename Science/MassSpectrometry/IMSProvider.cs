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
using SignalProcessing;
using System.Collections.Generic;
using System.Collections;
using MassSpectrometry;
using System.ComponentModel;
using System.Threading;
using System.IO;

public interface IMSProvider
{

    PointSet BPI { get; }
    PointSet BPM { get; }
    PointSet TIC { get; }
    PointSet this[int scan] { get; }

    /// <summary>
    /// Get Parent Scan Mass and Charge values
    /// </summary>
    //List<SpectrumInfo> GetParentInfo();

    List<SpectrumInfo> GetParentInfo(CancellationToken cToken = default(CancellationToken), int startScan = -1, int endScan = -1);

    SpectrumInfo GetParentInfo(int scanNumber);

    List<SpectrumInfo> GetInitialParentInfo();

    string GetActivationType(int scanNumber);

    /// <summary>
    /// Populates the parent mass and charge information by detection from the preceding MS scans
    /// </summary>
    /// <param name="sInfo"></param>
    void SetParentInfo(SpectrumInfo sInfo, bool useProductScan = false);

    //SpectrumInfo ForceParentInfo(SpectrumInfo sInfo);

    //PointSet MassScans(int scan);
    Spectrum Scans(int scan);
    IEnumerable<Spectrum> ScanRange(int startIndex, int endIndex, string filter);

    List<SpectrumInfo> GetParentScans();

    double MassTolerance { get; set; }

    int ScanIndex(double retentionTime);
    float RetentionTime(int scan);
    string Title(int scan);

    SpecMsLevel MsLevel(int scan);

    bool Contains(int scan);
    int? NextScan(int currentScan);
    int? PreviousScan(int currentScan);
    List<Cluster> DetectIons(int scanNum = 1, int minCharge = 1, int maxCharge = 30, double massRangeMin = 0, double massRangeMax = double.MaxValue);
    double? ParentMZ(int scan);
    string ScanType(int scan);
    bool ScanExists(int scan);
    string Filename { get; }

    /// <summary>
    /// Reference the the MS Data source
    /// </summary>
    FileSystemInfo Source { get; } 

    /// <summary>
    /// SHA256 Hash of the source of the MS Data (file or folder)
    /// </summary>
    string SourceHash { get; }

    //FileInfo SourceFile { get; }

    //SpecPointCollection ThermoPoints(int ScanNumber);

    //List<SpecPointCollection> ThermoRange(int startvalue, int endvalue, string startFilter = "");

    IEnumerable<IEnumerable<Cluster>> DetectIonsForScanRange(int startIndex, int endIndex, string containsFilter = "", string startFilter = "", int minCharge = 1, int maxCharge = 40);

    /// <summary>
    /// Executes a spectrum average using the Thermo Foundation
    /// </summary>
    /// <param name="startScan"></param>
    /// <param name="endScan"></param>
    /// <param name="MsOrderFilter"></param>
    /// <param name="startMass"></param>
    /// <param name="endMass"></param>
    /// <returns></returns>
    PointSet Average(int startScan, int endScan, int MsOrderFilter = 1, double startMass = double.NaN, double endMass = double.NaN);

    Chromatogram ExtractIon(List<Range> targetRange, float startTime = 0, float endTime = float.MaxValue);

    Spectrum NativeScanSum(int[] scanNumbers, bool average = false);


    SpectrumInfo ForceParentInfo(SpectrumInfo aScanInfo);
}


public enum SpecMsLevel : int
{
    All = 0,
    MS = 1,
    MSMS = 2,
}



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
using System.ComponentModel;
using System.Linq;
using System.Text;
using SignalProcessing;
using MassSpectrometry;
using System.Xml.Serialization;

public class MergedSpectrumInfo
{
    private List<SpectrumInfo> siList;
    private IMSProvider p;

    public MergedSpectrumInfo(IMSProvider provider, params SpectrumInfo[] si)
    {
        siList = new List<SpectrumInfo>(si);
        p = provider;
    }


    public IEnumerable<int> ScanNumbers
    {
        get
        {
            return siList.Select(si => si.ScanNumber);
        }
    }

    //public float RelativeIntensity { get; set; }

    /// <summary>
    /// Total Ion current for this spectrum
    /// </summary>
    public float Intensity
    {
        get
        {
            return siList.Sum(si => si.Intensity);
        }
    }

    //public Spectrum GetSpectrum
    //{
    //    get
    //    {
    //        return MergeSpectra(siList);
    //    }
    //}

    //private Spectrum MergeSpectra(IEnumerable<SpectrumInfo> scans)
    //{
    //    if (scans == null || !scans.Any()) return null;

    //    if (scans.Count() == 1) return scans.First().DataSource.Scans(scans.First().ScanNumber);

    //    var result = new Spectrum();

    //    // Populate the collections with the decharged ions
    //    foreach (var aScan in scans)
    //    {
    //        var minScore = (aScan.MsLevel == SpecMsLevel.MSMS) ? 100 : 300;

    //        // Gather all the ions from all the scans
    //        if (aScan.ParentZ.HasValue)
    //        {
    //            result.Ions.AddRange(aScan.DataSource.DetectIons(aScan.ScanNumber, 1, aScan.ParentZ.Value).Where(ion => ion.Score > minScore));  // only run charge detector up to Parent Charge
    //        }
    //        else
    //        {
    //            result.Ions.AddRange(aScan.DataSource.DetectIons(aScan.ScanNumber).Where(ion => ion.Score > minScore));
    //        }
    //    }

    //    // Now, consolidate all those ions where they are the same ion
    //    result.Ions = SignalProcessor.ConsolidateIons(result.Ions, true);

    //    return result;
    //}
}


public class SpectrumInfo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public double TimeSort { get; set; }

    public int ScanNumber
    {
        get;
        set;
    }

    /// <summary>
    /// Expect this to be a string like "ETD" , "CID", "HCD"
    /// </summary>
    public string Activation
    {
        get;
        set;
    }


    public string Title
    {
        get;
        set;
    }

    public SpecMsLevel MsLevel
    {
        get;
        set;
    }

    public double? RetentionTime
    {
        get;
        set;
    }

    double _parentmass = 0;

    public double? ParentMass
    {
        get
        {
            if (_parentmass == 0)
            {
                if (ParentIon != null)
                    return ParentIon.MonoMass;
                else
                    return null;
            }
            else
                return _parentmass;
        }
        set
        {
            _parentmass = value.Value;
        }
    }

    int _parentz = 0;

    public int? ParentZ
    {
        get
        {
            if (_parentz == 0)
            {
                if (ParentIon != null)
                    return ParentIon.Z;
                else
                    return null;
            }
            else
            {
                return _parentz;
            }
        }
        set
        {
            _parentz = value.Value;
        }
    }

    public Cluster ParentIon
    {
        get;
        set;
    }

    public bool IsParentScan
    {
        get;
        set;
    }

    bool isVisible = true;

    public bool IsVisible
    {
        get { return isVisible; }
        set { isVisible = value; }
    }

    private void OnPropertyChanged(string info)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(info));
        }
    }

    public string ScanNumbers
    {
        get;
        set;
    }

    public float RelativeIntensity { get; set; }

    /// <summary>
    /// Total Ion current for this spectrum
    /// </summary>
    public float Intensity { get; set; }

    [XmlIgnore]
    public IMSProvider DataSource { get; set; }

    public bool IsMergeMember { get; set; }

    public override string ToString()
    {
        return "ScanNumber " + Convert.ToString(ScanNumber) + "    ParentMass " + Convert.ToString(ParentMass) + "    RetentionTime " + Convert.ToString(RetentionTime);
    }
}



public static partial class Extensions
{

    /// <summary>
    /// Group all the spectras which are in 
    /// a time range of 2.0 and have mass difference of 
    /// 0.3 
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    public static List<SpectrumInfo> SortMergeSpectra(this IOrderedEnumerable<SpectrumInfo> m)
    {
        PopulateTimeSort(m);
        return m.OrderBy(a => a.TimeSort).ThenBy(a => a.ScanNumber).ToList();
    }



    /// <summary>
    /// Group all the spectras which are in 
    /// a time range of 2.0 and have mass difference of 
    /// 0.3 
    /// </summary>
    /// <param name="m"></param>
    public static void PopulateTimeSort(this IEnumerable<SpectrumInfo> m)
    {
        //try
        //{
        double mass = 0.0;

        var siList = m.OrderByDescending(s => s.ParentIon == null || s.ParentIon.MonoMass == 0).ThenBy(a => a.RetentionTime).ToList();

        var sii = siList.Where(a => (a.ParentIon != null && a.Activation != "") && (a.Activation == "HCD" || a.Activation == "CID")).GroupBy(a => a.Activation).ToList();

        sii.AddRange(siList.Where(a => (a.ParentIon != null && a.Activation != "") && (a.Activation == "ETD")).GroupBy(a => a.Activation).ToList());

        sii.AddRange(siList.Where(a => (a.ParentIon != null && a.Activation != "") && (a.Activation != "ETD" && a.Activation != "HCD" && a.Activation != "CID")).GroupBy(a => a.Activation).ToList());


        foreach (var si in sii)
        {
            foreach (var ms in si)
            {
                if (ms.ParentIon == null)
                {
                    ms.TimeSort = 100000 * ms.ScanNumber;
                    continue;
                }

                mass = ms.ParentIon.MonoMass;

                //foreach (var mm in si) ///.Where(a => a.ParentIon != null)
                {
                    foreach (var mm in siList.Where(a => a.ParentIon != null).Where(a => ((a.RetentionTime - ms.RetentionTime) < 2.0) && (Math.Abs(a.ParentIon.MonoMass - mass) <= 0.3 || Math.Abs(ms.ParentIon.MonoMass - a.ParentIon.MonoMass) <= 0.3))) /// && (a.RetentionTime > ms.RetentionTime)))
                    {
                        //if ((Math.Abs(mm.ParentIon.MonoMass - mass) <= 0.3 || Math.Abs(ms.ParentIon.MonoMass - mm.ParentIon.MonoMass) <= 0.3)) // && mm.TimeSort == int.MaxValue)
                        {
                            mass = mm.ParentIon.MonoMass;
                            //if (mm.RetentionTime - ms.RetentionTime < 2.0)
                            {
                                mm.TimeSort = Convert.ToInt32(ms.ScanNumber) + (0.00001 * mm.ScanNumber);  // The second value is added as a tie breaker by scan number
                                ms.TimeSort = Convert.ToInt32(ms.ScanNumber) + (0.00001 * mm.ScanNumber);
                            }
                        }
                    }
                }
            }
        }
        //}
        //catch (Exception ex)
        //{
        //    throw new Exception("I am here");

        //}


    }
}



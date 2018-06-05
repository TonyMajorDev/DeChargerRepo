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
using System.Text;
using System.ComponentModel;

namespace SignalProcessing
{
    public class ProcessParameters : System.Collections.Generic.Dictionary<string, double>
    {

    }

    //[System.Diagnostics.DebuggerDisplay("{_x}, {_height}")]
    //public class Peak : IComparable<Peak>
    //{
    //    // Basic properties of peak
    //    private double _x;
    //    private double _height;
    //    private double _area;
    //    private double _fwhm;

    //    /// <summary>
    //    /// Peak object
    //    /// </summary>
    //    /// <param name="X">X Coordinate of Apex of peak</param>
    //    /// <param name="height">Height at apex</param>
    //    /// <param name="area"></param>
    //    /// <param name="FWHM"></param>
    //    public Peak(double X, double height, double area, double FWHM)
    //    {
    //        _x = X;
    //        _height = height;
    //        _area = area;
    //        _fwhm = FWHM;
    //    }

    //    public double X
    //    {
    //        get { return _x; }
    //        set { _x = value; }
    //    }

    //    public double Height
    //    {
    //        get { return _height; }
    //        set { _height = value; }
    //    }

    //    public double Area
    //    {
    //        get { return _area; }
    //        set { _area = value; }
    //    }

    //    public double FWHM
    //    {
    //        get { return _fwhm; }
    //        set { _fwhm = value; }
    //    }


    //    #region IComparable<Peak> Members

    //    public int CompareTo(Peak other)
    //    {
    //        return this.X.CompareTo(other.X);
    //    }

    //    #endregion
    //}
    
    //public class PeakList : List<Peak>
    //{

    //    /// <summary>
    //    /// Sort peaks and remove any duplicates
    //    /// </summary>
    //    public void Cleanup()
    //    {
    //        this.Sort();

    //        // Remove duplicates
    //        for (int i = this.Count - 1; i > 0; i--)
    //            if (this[i].X == this[i - 1].X) 
    //                this.RemoveAt(i);
    //    }

    //}

    public delegate void ProgressChangedEventHandler(Object sender, ProgressChangedEventArgs e);
}

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
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.IO;

public class WaitCursor : IDisposable
{
    private Cursor _previousCursor;

    public WaitCursor()
    {
        _previousCursor = Mouse.OverrideCursor;

        Mouse.OverrideCursor = Cursors.Wait;
    }

    #region IDisposable Members

    public void Dispose()
    {
        Mouse.OverrideCursor = _previousCursor;
    }

    #endregion
}

public static partial class Extensions
{
    public static void AddRange<T>(this ObservableCollection<T> oc, IEnumerable<T> dataToAdd)
    {
        //Based on: https://blogs.msdn.microsoft.com/nathannesbit/2009/04/20/addrange-and-observablecollection/

        // We need the starting index later
        //int startingIndex = oc.Count();

        // Add the items directly to the inner collection
        foreach (var data in dataToAdd) oc.Add(data);

        // Now raise the changed events
        //oc.OnPropertyChanged("Count");
        //OnPropertyChanged("Item[]");

        // We have to change our input of new items into an IList since that is what the
        // event args require.

        //var changedItems = new List<T>(dataToAdd);
        //OnCollectionChanged(changedItems, startingIndex);
        //oc.CollectionChanged
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        // http://stackoverflow.com/questions/8809354/replace-first-occurrence-of-pattern-in-a-string
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
    

    /// <summary>
    /// Retrieve the number from the string.  Very tolerant to text mixed in...
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Returns the number if successful and double.NaN if it fails</returns>
    public static double TolerantDoubleParse(this string input)
    {
        // Matches the first numebr with or without leading minus.
        var match = Regex.Match(input, @"-?[0-9]+[\.]?[0-9]*");

        if (match.Success)
        {
            double result = 0;
            if (double.TryParse(match.Value, out result)) return result;
        }

        return double.NaN; // Or any other default value.
    }
}



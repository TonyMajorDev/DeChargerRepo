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

using SignalProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


public static partial class Extensions
{

        public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> lookup, TKey key, TValue value)
        {
            if (!lookup.ContainsKey(key)) lookup.Add(key, value);
        }

        //public static void TrySum<TKey, TValue>(this IDictionary<TKey, TValue> lookup, TKey key, TValue value)
        //{
        //    if (!lookup.ContainsKey(key))
        //        lookup.Add(key, value);
        //    else
        //        lookup[key] += (double?)value;
        //}

        public static void TrySum(this PointSet lookup, double key, float value)
        {
            if (!lookup.ContainsKey(key))
                lookup.Add(key, value);
            else
                lookup[key] += value;
        }

    


    /// <summary>
    /// This is just like the regular Add method, except it checks for null before adding, and if the value is null, the item is simply not added.  
    /// So the null value is skipped, hence the name.  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="aList"></param>
    /// <param name="theValue">Object to be added</param>
    public static void AddSkipNull<T>(this System.Collections.Generic.List<T> aList, T theValue) 
    {
        if (theValue != null) aList.Add(theValue);
    }

    public static double Median(this IEnumerable<double> source)
    {
        if (source.Count() == 0)
        {
            throw new InvalidOperationException("Cannot compute median for an empty set.");
        }

        var sortedList = from number in source
                         orderby number
                         select number;

        int itemIndex = (int)sortedList.Count() / 2;

        if (sortedList.Count() % 2 == 0)
        {
            // Even number of items.
            return (sortedList.ElementAt(itemIndex) + sortedList.ElementAt(itemIndex - 1)) / 2;
        }
        else
        {
            // Odd number of items.
            return sortedList.ElementAt(itemIndex);
        }
    }


    static SortedList<string, IEnumerable<string>> cache = new SortedList<string, IEnumerable<string>>();

    public static IEnumerable<string> ToSymbols(this string sequence)
    {
        if (cache.ContainsKey(sequence)) return cache[sequence];

        if (cache.Count > 100) cache.Clear();  // crude cache size limiting

        var result = new List<string>();

        for (int i = 0; i < sequence.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(sequence[i].ToString()) == false)
            {
                if (i + 1 < sequence.Length && string.IsNullOrWhiteSpace(sequence[i + 1].ToString()) == false && sequence[i + 1].ToString().ToLower() == sequence[i].ToString())
                {
                    result.Add(new string(new char[] { sequence[i], sequence[i + 1] }));  // two letter symbol
                    i++;
                    continue;
                }

                result.Add(sequence[i].ToString()); // single letter symbol

            }

        }

        cache.Add(sequence, result);

        return result;
    }

    //public static double Median(this IEnumerable<double> source)
    //{
    //    var sortedList = from number in source
    //                     orderby number
    //                     select number;

    //    int count = sortedList.Count();
    //    int itemIndex = count / 2;
    //    if (count % 2 == 0) // Even number of items. 
    //        return (sortedList.ElementAt(itemIndex) +
    //                sortedList.ElementAt(itemIndex - 1)) / 2;

    //    // Odd number of items. 
    //    return sortedList.ElementAt(itemIndex);
    //}

    public static double? Median(this IEnumerable<double?> source)
    {
        var sortedList = from number in source.Where(x => x.HasValue).Select(v => v.Value)
                         orderby number
                         select number;

        int count = sortedList.Count();
        int itemIndex = count / 2;
        if (count % 2 == 0) // Even number of items. 
        {
            var avg = (sortedList.ElementAt(itemIndex) + sortedList.ElementAt(itemIndex - 1)) / 2;

            // Return the item closest to the average
            return (Math.Abs(sortedList.ElementAt(itemIndex) - avg) > Math.Abs(sortedList.ElementAt(itemIndex - 1) - avg)) ? sortedList.ElementAt(itemIndex - 1) : sortedList.ElementAt(itemIndex);
        }

        // Odd number of items. 
        return sortedList.ElementAt(itemIndex);
    }

    public static float Median(this IEnumerable<float> source)
    {
        var sortedList = from number in source
                         orderby number
                         select number;

        int count = sortedList.Count();
        int itemIndex = count / 2;
        if (count % 2 == 0) // Even number of items. 
        {
            var avg = (sortedList.ElementAt(itemIndex) + sortedList.ElementAt(itemIndex - 1)) / 2;

            // Return the item closest to the average
            return (Math.Abs(sortedList.ElementAt(itemIndex) - avg) > Math.Abs(sortedList.ElementAt(itemIndex - 1) - avg)) ? sortedList.ElementAt(itemIndex - 1) : sortedList.ElementAt(itemIndex);
        }


        // Odd number of items. 
        return sortedList.ElementAt(itemIndex);
    }

    public static double WeightedAverage<T>(this IEnumerable<T> records, Func<T, double> value, Func<T, double> weight)
    {
        // Based on http://stackoverflow.com/questions/2714639/weighted-average-with-linq

        // usage: double weightedAverage = records.WeightedAverage(record => record.PCR, record => record.LENGTH);

        double weightedValueSum = records.Sum(record => value(record) * weight(record));
        double weightSum = records.Sum(record => weight(record));

        if (weightSum != 0)
            return weightedValueSum / weightSum;
        else
            throw new DivideByZeroException("Your message here");
    }

    /// <summary>
    /// Return a SHA256 from a file
    /// </summary>
    /// <param name="aFile"></param>
    /// <returns>hashcode as a string</returns>
    public static string Sha256Hash(this FileInfo aFile)
    {
        using (var hash = SHA256.Create())
        {
            //return BitConverter.ToString(hash.ComputeHash(aFile.OpenRead())).Replace("-", "");

            // open file as shared so that you avoid the "file in use" error if it is already open
            return BitConverter.ToString(hash.ComputeHash(aFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))).Replace("-", "");
        }
    }

    /// <summary>
    /// Return a SHA256 from a Directory
    /// </summary>
    /// <param name="aDir"></param>
    /// <returns>hashcode as a string</returns>
    public static string Sha256Hash(this DirectoryInfo aDir)
    {
        // based on https://stackoverflow.com/questions/3625658/creating-hash-for-folder

        var files = aDir.GetFiles("*", SearchOption.AllDirectories).OrderBy(p => p.Name).ToArray();

        using (var sha256 = SHA256.Create())
        {
            foreach (var aFile in files)
            {
                // hash path
                byte[] pathBytes = Encoding.UTF8.GetBytes(aFile.Name);  // use relative path here?  
                sha256.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                //byte[] contentBytes = File.ReadAllBytes(aFile.FullName);
                //var s = aFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                //byte[] contentBytes = new byte[aFile.Length];
                //s.Read(contentBytes, 0, contentBytes.Length);

                //sha256.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);

                // Based on: https://stackoverflow.com/questions/20634827/how-to-compute-hash-of-a-large-file-chunk
                using (var fileStream = aFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.Position = 0;
                    long bytesToHash = fileStream.Length;

                    var buf = new byte[128 * 1024]; // chunk buffer
                    while (bytesToHash > 0)
                    {
                        var bytesRead = fileStream.Read(buf, 0, (int)Math.Min(bytesToHash, buf.Length));
                        sha256.TransformBlock(buf, 0, bytesRead, null, 0);
                        bytesToHash -= bytesRead;
                        if (bytesRead == 0)
                            throw new InvalidOperationException("Unexpected end of stream");
                    }
                };
            }

            //Handles empty filePaths case
            sha256.TransformFinalBlock(new byte[0], 0, 0);

            return BitConverter.ToString(sha256.Hash).Replace("-", "");
        }
    }

    ///// <summary>
    ///// Return a SHA256 from a Directory
    ///// </summary>
    ///// <param name="aDir"></param>
    ///// <returns>hashcode as a string</returns>
    //public static string Sha256Hash2(this DirectoryInfo aDir)
    //{
    //    // based on https://stackoverflow.com/questions/3625658/creating-hash-for-folder

    //    // This method uses streams to minimize memory usage.  

    //    var files = aDir.GetFiles("*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();

    //    using (var sha256 = SHA256.Create())
    //    {
    //        foreach (var aFile in files)
    //        {
    //            var fileStream = aFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    //            // Based on: https://blogs.msdn.microsoft.com/shawnfa/2004/02/20/using-the-hashing-transforms-or-how-do-i-compute-a-hash-block-by-block/
    //            // and: https://stackoverflow.com/questions/2124468/possible-to-calculate-md5-or-other-hash-with-buffered-reads

    //            using (var cs = new CryptoStream(fileStream, sha256, CryptoStreamMode.Read))
    //            {
    //                cs.

    //                // hash path
    //                var filenameAsBytes = Encoding.UTF8.GetBytes(aFile.Name); // use relative path here?  
    //                cs.Write(filenameAsBytes, 0, filenameAsBytes.Length);

    //                while (notDoneYet)
    //                {
    //                    buffer = Get32MB();
    //                    cs.Write(buffer, 0, buffer.Length);

    //                    fileStream
    //                }

    //                System.Console.WriteLine(BitConverter.ToString(sha256.Hash));
    //            }

    //            // hash path
    //            //byte[] pathBytes = Encoding.UTF8.GetBytes(aFile.Name);  // use relative path here?  
    //            //sha256.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

    //            // hash contents
    //            byte[] contentBytes = File.ReadAllBytes(aFile.FullName);
    //            //aFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                

    //            //sha256.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);

    //            fileStream.Close();
    //        }

    //        //TODO: test the empty folder case

    //        //Handles empty filePaths case
    //        //sha256.TransformFinalBlock(new byte[0], 0, 0);

    //        return BitConverter.ToString(sha256.Hash).Replace("-", "");
    //    }
    //}

    public static string Sha256Hash(this FileSystemInfo aFileOrDir)
    {
        if (aFileOrDir is FileInfo) return (aFileOrDir as FileInfo).Sha256Hash();
        return (aFileOrDir as DirectoryInfo).Sha256Hash();
    }

}


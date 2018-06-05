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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
//using CommonClasses;

namespace Science.Blast
{
    public class BlastP
    {
        static object LockObject = new object();
         
        /// <summary>
        /// Create a blast database based on sequence and sequenceid
        /// </summary>
        /// <param name="stringbuilder"></param>
        /// <returns></returns>
        public static bool CreateBlastDatabase(StringBuilder stringbuilder)
        {
            lock (LockObject)
            {
                bool output = false;

                try
                {
                    System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
                    //Location is where the assembly is run from 
                    string assemblyLocation = assemblyInfo.Location;
                    //CodeBase is the location of the ClickOnce deployment files
                    Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
                    string ClickOnceLocation = Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Executables\\";

                    //File.WriteAllBytes(ClickOnceLocation + "blastp32.exe", Properties.Resources.blastp32);

                    if (File.Exists(ClickOnceLocation + "indexeddatabase.phr"))
                    {
                        File.Delete(ClickOnceLocation + "indexeddatabase.phr");
                    }
                    if (File.Exists(ClickOnceLocation + "indexeddatabase.pin"))
                    {
                        File.Delete(ClickOnceLocation + "indexeddatabase.pin");
                    }
                    if (File.Exists(ClickOnceLocation + "indexeddatabase.psq"))
                    {
                        File.Delete(ClickOnceLocation + "indexeddatabase.psq");
                    }

                    using (StreamWriter outfile = new StreamWriter(ClickOnceLocation + @"\AllTxtFiles.txt"))
                    {
                        outfile.Write(stringbuilder.ToString());
                    }
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(ClickOnceLocation + "makeblastdb32.exe", " -dbtype prot -out " + ClickOnceLocation + "indexeddatabase" + " -title " + " proteins -in " + " AllTxtFiles.txt ");
                    //Cannot use the Shell
                    startInfo.UseShellExecute = false;
                    //Redirecting the output for use
                    startInfo.RedirectStandardOutput = true;
                    startInfo.WorkingDirectory = ClickOnceLocation;
                    startInfo.CreateNoWindow = true;
                    startInfo.LoadUserProfile = true;
                    using (System.Diagnostics.Process proc = System.Diagnostics.Process.Start(startInfo))
                    {
                        proc.WaitForExit();
                        output = File.Exists(ClickOnceLocation + "indexeddatabase.phr") ? true : false;

                        //TODO: read the output and pass it back in an exception object when the database index failed to be created.  
                        if (output == false) throw new Exception("Failed to create BLAST index.");
                    }
                }
                catch (Exception ex)
                {
                    output = false;
                    throw ex;
                }

                return output;
            }
        }

        //static bool Iwasnothere;
        //static List<clsBlastpResults> lstBlastresults = new List<clsBlastpResults>();


        /// <summary>
        /// Creates the special Blast specific input format for a query
        /// </summary>
        /// <param name="sequences"></param>
        ///// <returns></returns>
        //public Dictionary<Guid, string> BuildQuery(IEnumerable<string> searchTags)
        //{
        //    var hashcodesforblastsequences = new Dictionary<Guid, string>();
        //    StringBuilder stringbuilderlocal = new StringBuilder();
        //    Guid localhashcode = new Guid();
        //    string reversestring = string.Empty;
        //    Guid.NewGuid();

        //    foreach (var s in searchTags)
        //    {
        //        localhashcode = Guid.NewGuid();
        //        hashcodesforblastsequences.Add(localhashcode, s);
        //        stringbuilderlocal.Append(">" + localhashcode + "\n" + s + "\n");
        //        reversestring = ReverseString.Reverse(s);
        //        localhashcode = Guid.NewGuid();
        //        hashcodesforblastsequences.Add(localhashcode, reversestring);
        //        stringbuilderlocal.Append(">" + localhashcode + "\n" + reversestring + "\n");
        //    }

        //    return hashcodesforblastsequences;
        //}



        /// <summary>
        /// Search the blast database for results
        /// </summary>
        /// <param name="query">Special Blast format</param>
        /// <returns></returns>
        public static List<clsBlastpResults> BlastQueryResults(StringBuilder query)
        {
            lock (LockObject)
            {
                DateTime dtbefore = DateTime.Now;
                List<clsBlastpResults> lstBlastresults = new List<clsBlastpResults>();
                double value = 0;
                //if (!Iwasnothere)
                {
                    //Iwasnothere = true;
                    System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
                    //Location is where the assembly is run from
                    string assemblyLocation = assemblyInfo.Location;
                    //CodeBase is the location of the ClickOnce deployment files
                    Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
                    string ClickOnceLocation = Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Executables\\";

                    string queryFilename = Path.GetRandomFileName().Split('.')[0] + ".txt";

                    #region Uncomment_This
                    //using (StreamWriter outfile = new StreamWriter(ClickOnceLocation + @"\Queryfile.txt"))  
                    using (StreamWriter outfile = new StreamWriter(ClickOnceLocation + @"\" + queryFilename))
                    {
                        outfile.Write(query);
                    }
                    #endregion

                    //using (StreamWriter outfile = new StreamWriter(ClickOnceLocation + @"\ReverseQueryfile.txt"))
                    //{
                    //    char[] reverse = query.ToCharArray();
                    //    Array.Reverse(reverse);
                    //    outfile.Write(new string(reverse));
                    //}
                    value = DateTime.Now.Subtract(dtbefore).TotalMilliseconds;
                    Debug.Print("Blastp write search terms in Milliseconds " + value);
                    DateTime dtbefore1 = DateTime.Now;
                    #region forwardsearch
                    //System.Diagnostics.Process proc = null;

                    // Environment.Is64BitProcess

                    // A high E-Value cutoff of 2500 (versus 10) was needed to bring in hits for 4 AA tags.  The Tag Hits are then used to more intelligently order the candidates.  
                    System.Diagnostics.ProcessStartInfo startInfo1 = new System.Diagnostics.ProcessStartInfo(ClickOnceLocation + "blastp32.exe", " -query " + queryFilename + " -evalue 2500 -ungapped -max_target_seqs 100 -comp_based_stats 0 " + " -db " + ClickOnceLocation + "indexeddatabase " + " -task blastp-short -num_threads 4 -outfmt 10"); ///+ "indexeddatabase " + " -task blastp-short -num_threads 4 -outfmt 10");
                    //Cannot use the Shell
                    startInfo1.UseShellExecute = false;
                    //Redirecting the output for use
                    startInfo1.RedirectStandardOutput = true;
                    //Need to specify the working directory for some of the commands to execute
                    startInfo1.WorkingDirectory = ClickOnceLocation;
                    //A command prompt is normally opened to execute. This helps enables to run the commands without opening a window
                    startInfo1.CreateNoWindow = true;
                    startInfo1.LoadUserProfile = true;

                    //proc = System.Diagnostics.Process.Start(startInfo1);


                    string output = string.Empty;
                    DateTime dtbeforestartprocessforforward = DateTime.Now;

                    using (System.Diagnostics.Process proc = System.Diagnostics.Process.Start(startInfo1))
                    {
                        using (StreamReader reader = proc.StandardOutput)
                        {
                            value = DateTime.Now.Subtract(dtbefore1).TotalMilliseconds;
                            Debug.Print("Blastp to start the cmd prompt terms in Milliseconds " + value);
                            dtbeforestartprocessforforward = DateTime.Now;
                            //StringBuilder builder = new StringBuilder();
                            //builder.Append(reader.ReadToEnd());
                            dtbeforestartprocessforforward = DateTime.Now;
                            value = DateTime.Now.Subtract(dtbeforestartprocessforforward).TotalMilliseconds;
                            Debug.Print("Blastp time for reading using stringbuilder the cmd prompt in Milliseconds " + value);
                            dtbeforestartprocessforforward = DateTime.Now;
                            output = reader.ReadToEnd();/// builder.ToString();
                            value = DateTime.Now.Subtract(dtbeforestartprocessforforward).TotalMilliseconds;

                            Debug.Print("Output length is: " + output.Length + " chars");

                            Debug.Print("Blastp time for reading data using string from the cmd prompt in Milliseconds " + value);
                            dtbeforestartprocessforforward = DateTime.Now;
                            string[] newlinestring = { "\n" };
                            string[] lstblastp = output.Split(newlinestring, StringSplitOptions.RemoveEmptyEntries);
                            string[] commastring = { ",", "\r" };
                            foreach (string blastp in lstblastp)
                            {
                                string[] blastpresults = blastp.Split(commastring, StringSplitOptions.RemoveEmptyEntries);
                                lstBlastresults.Add(new clsBlastpResults()
                                {
                                    QuerySequenceID = blastpresults[0],
                                    SubjectSequenceID = blastpresults[1],
                                    PercentageIdenticalMatch = Convert.ToDouble(blastpresults[2]),
                                    AlignmentLength = Convert.ToInt32(blastpresults[3]),
                                    NumberofMismatches = Convert.ToInt32(blastpresults[4]),
                                    GapOpen = Convert.ToInt32(blastpresults[5]),
                                    QueryStart = Convert.ToInt32(blastpresults[6]),
                                    QueryEnd = Convert.ToInt32(blastpresults[7]),
                                    SequenceStart = Convert.ToInt32(blastpresults[8]),
                                    SequenceEnd = Convert.ToInt32(blastpresults[9]),
                                    Expectvalue = Convert.ToDouble(blastpresults[10]),
                                    BitScore = Convert.ToDouble(blastpresults[11]),
                                });
                            }
                        }
                    }
                    value = DateTime.Now.Subtract(dtbeforestartprocessforforward).TotalMilliseconds;
                    Debug.Print("Blastp query the forward direction in Milliseconds " + value);
                    #endregion

                    #region reversesearch

                    ////DateTime dtbefore2 = DateTime.Now;

                    //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(ClickOnceLocation + "blastp.exe", " -query " + "ReverseQueryfile.txt" + " -evalue 200 -max_target_seqs 100 -comp_based_stats 0 " + " -db " + ClickOnceLocation + "indexeddatabase " + " -task blastp-short -num_threads 12 -outfmt 10");
                    ////Cannot use the Shell
                    //startInfo.UseShellExecute = false;
                    ////Redirecting the output for use
                    //startInfo.RedirectStandardOutput = true;
                    ////Need to specify the working directory for some of the commands to execute
                    //startInfo.WorkingDirectory = ClickOnceLocation;
                    ////A command prompt is normally opened to execute. This helps enables to run the commands without opening a window
                    //startInfo.CreateNoWindow = true;
                    //startInfo.LoadUserProfile = true;
                    //output = string.Empty;
                    //using (System.Diagnostics.Process proc = System.Diagnostics.Process.Start(startInfo))
                    //{
                    //    using (StreamReader reader = proc.StandardOutput)
                    //    {
                    //        StringBuilder builder = new StringBuilder();
                    //        builder.Append(reader.ReadToEnd());
                    //        output = builder.ToString();
                    //        string[] newlinestring = { "\n" };
                    //        string[] lstblastp = output.Split(newlinestring, StringSplitOptions.RemoveEmptyEntries);
                    //        string[] commastring = { ",", "\r" };
                    //        foreach (string blastp in lstblastp)
                    //        {
                    //            string[] blastpresults = blastp.Split(commastring, StringSplitOptions.RemoveEmptyEntries);
                    //            lstBlastresults.Add(new clsBlastpResults()
                    //            {
                    //                QuerySequenceID = blastpresults[0],
                    //                SubjectSequenceID = blastpresults[1],
                    //                PercentageIdenticalMatch = Convert.ToDouble(blastpresults[2]),
                    //                AlignmentLength = Convert.ToInt32(blastpresults[3]),
                    //                NumberofMismatches = Convert.ToInt32(blastpresults[4]),
                    //                GapOpen = Convert.ToInt32(blastpresults[5]),
                    //                QueryStart = Convert.ToInt32(blastpresults[6]),
                    //                QueryEnd = Convert.ToInt32(blastpresults[7]),
                    //                SequenceStart = Convert.ToInt32(blastpresults[8]),
                    //                SequenceEnd = Convert.ToInt32(blastpresults[9]),
                    //                Expectvalue = Convert.ToDouble(blastpresults[10]),
                    //                BitScore = Convert.ToDouble(blastpresults[11]),
                    //            });
                    //        }
                    //    }
                    //}
                    ////value = DateTime.Now.Subtract(dtbefore2).TotalMilliseconds;
                    ////Debug.Print("Blastp query the reverse direction in Milliseconds " + value);
                    #endregion


                    File.Delete(ClickOnceLocation + @"\" + queryFilename);

                }
                //value = DateTime.Now.Subtract(dtbefore).TotalMilliseconds;
                //Debug.Print("Blastp time in Milliseconds " + value);
                return lstBlastresults;
            }
        }
    }

    public class clsFileType
    {
        public enum MSFileType
        {
            Agilent,
            Thermo,
            None
        }

        public Dictionary<MSFileType, double> FileType
        {
            get;
            set;
        }
    }


    /// <summary>
    /// All the blastp results in a class
    /// </summary>
    public class clsBlastpResults
    {
        /// <summary>
        /// Query String
        /// </summary>
        public string BlastString
        {
            get;
            set;
        }

        /// <summary>
        /// Actual Protein Sequence
        /// </summary>
        public string Sequence
        {
            get;
            set;
        }

        /// <summary>
        /// SequenceID for the query is specified
        /// </summary>
        public string QuerySequenceID
        {
            get;
            set;
        }

        /// <summary>
        /// Accession of the Protein created by the indexed database.
        /// </summary>
        public string SubjectSequenceID
        {
            get;
            set;
        }

        /// <summary>
        /// How identical is the query string to the database match
        /// </summary>
        public double PercentageIdenticalMatch
        {
            get;
            set;
        }

        /// <summary>
        /// Length of the query which is identical to the matched sequence
        /// </summary>
        public int AlignmentLength
        {
            get;
            set;
        }

        /// <summary>
        /// Difference between the query length and the matched part of the sequence
        /// </summary>
        public int NumberofMismatches
        {
            get;
            set;
        }

        /// <summary>
        /// Number of Gap openings
        /// </summary>
        public int GapOpen
        {
            get;
            set;
        }

        /// <summary>
        /// Start position of the query within the query term which is actually matched to the sequence
        /// </summary>
        public int QueryStart
        {
            get;
            set;
        }

        /// <summary>
        /// End position of the query within the query term which is actually matched to the sequence
        /// </summary>
        public int QueryEnd
        {
            get;
            set;
        }

        public string Parentz_s
        {
            get;
            set;
        }

        /// <summary>
        /// The start position in the Sequence which is matched to the Query
        /// </summary>
        public int SequenceStart
        {
            get;
            set;
        }

        /// <summary>
        /// The end position in the Sequence which is matched to the Query
        /// </summary>
        public int SequenceEnd
        {
            get;
            set;
        }

        /// <summary>
        /// The probability of the query not be a match to the sequence.
        /// The lower the expectvalue, the better is the query a match to the sequence.
        /// </summary>
        public double Expectvalue
        {
            get;
            set;
        }

        /// <summary>
        /// Don't know what this is
        /// </summary>
        public double BitScore
        {
            get;
            set;
        }
    }
}



#region Unused code
/// <summary>
/// Unused code
/// </summary>
//startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
//Get the assembly information
//System.Diagnostics.Process proc1 = System.Diagnostics.Process.Start("cmd", ClickOnceLocation + "blastp.exe " + "-help");
//startInfo.FileName = "blastp.exe";
//startInfo.Arguments = "-help"; //" -query @tempFile -evalue 200 -max_target_seqs 100 -comp_based_stats 0 -db e:\blast_index\bdb -task blastp-short -num_threads 12 -outfmt 10 ";/// /C copy /b Image1.jpg + Archive.rar Image2.jpg";
//startInfo.LoadUserProfile = true;
//string allthevariables = string.Join(",", startInfo.EnvironmentVariables.Values);
//startInfo.Domain = 
//process.StartInfo = startInfo;
//process.Start();
//string output = process.StandardOutput.ReadToEnd();
//process.WaitForExit();
//StreamReader reader = process.StandardOutput;
//string result = reader.ReadToEnd();
//process.Start("cmd", "/C copy c:\\file.txt lpt1");

/// <summary>
/// Gets the blast results based on the database created and the query
/// </summary>
/// <param name="SearchString"></param>
/// <param name="database"></param>
/// <param name="stringbuilder"></param>
/// <param name="threadcount"></param>
/// <returns></returns>
//public static List<clsBlastpResults> GetBlastResuls(BlastCommands blastcmd, string SearchString, StringBuilder stringbuilder, int threadcount = 1)
//{
//    List<clsBlastpResults> Alltheresults = new List<clsBlastpResults>();
//    try
//    {

//        ///Current Assembly information
//        System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();

//        //Location is where the assembly is run from 
//        string assemblyLocation = assemblyInfo.Location;

//        //CodeBase is the location of the ClickOnce deployment files
//        Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);

//        string ClickOnceLocation = Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Executables\\";// Location for the clickonce
//        string filePath = ClickOnceLocation + "blastp.exe"; //Current file path

//        using (StreamWriter outfile = new StreamWriter(ClickOnceLocation + @"\AllTxtFiles.txt"))
//        {
//            outfile.Write(stringbuilder.ToString());
//        }

//        BlastCommands blstcmd = new BlastCommands();
//        blstcmd = BlastCommands.CreateDatabase;
//        ExecuteBlastCommand(blstcmd);

//        Alltheresults.Add(new clsBlastpResults()
//        {
//            BlastString = ExecuteBlastCommand(blstcmd),
//            Sequence = "Nothing"
//        });
//    }
//    catch (Exception ex)
//    {
//        Alltheresults.Add(new clsBlastpResults
//        {
//            Sequence = ex.Message + "\t" + ex.StackTrace,
//            BlastString = ex.Message + "\t" + ex.StackTrace
//        });
//    }
//    return Alltheresults;
//}

/// <summary>
/// Execute all the blast commands based on the enum value
/// </summary>
/// <param name="blstcmds"></param>
//static string ExecuteBlastCommand(BlastCommands blstcmds, string query = "") ///, List<FASTA> lstFASTA)
//{
//    string output = string.Empty;
//    ///Current Assembly information
//    System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();

//    //Location is where the assembly is run from 
//    string assemblyLocation = assemblyInfo.Location;

//    //CodeBase is the location of the ClickOnce deployment files
//    Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);

//    string ClickOnceLocation = Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Executables\\";

//    string filePath = ClickOnceLocation + "blastp.exe"; //Current file path

//    return output;
//}

//public enum BlastCommands
//{
//    CreateDatabase,
//    Query
//}

//string SearchString
//{
//    get;
//    set;
//}

//int ThreadCound
//{
//    get;
//    set;
//}

//FileInfo database
//{
//    get;
//    set;
//}
#endregion

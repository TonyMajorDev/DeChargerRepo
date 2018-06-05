using MSViewer.Controls;
using Science.Proteomics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;

namespace MSViewer.Classes
{
    public class GenerateHTML
    {
        /// <summary>
        /// Copies just the sequence with the highlighting to the clipboard
        /// </summary>
        /// <param name="sequence"></param>
        public static void copyasequence(SearchResult sequence)
        {
            int bytecount = Encoding.UTF8.GetByteCount(sequence.Sequence) + 4607;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Version:1.0");
            sb.AppendLine("StartHTML:0000000105");
            sb.AppendLine("EndHTML:" + bytecount);
            sb.AppendLine("StartFragment:000004607");
            sb.AppendLine("EndFragment:" + (bytecount - 40));
            System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            //Location is where the assembly is run from
            string assemblyLocation = assemblyInfo.Location;
            //CodeBase is the location of the ClickOnce deployment files
            Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
            string ClickOnceLocation = System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Files\\";
            using (StreamReader sr = new StreamReader(ClickOnceLocation + @"\HTMLCopy.txt"))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            sb.AppendLine("<p class=MsoNormal>");
            sb.AppendLine("<b style='mso-bidi-font-weight: normal'>");

            sb.Append(concatinatematchstartends(sequence.Sequence, sequence.matchstartends, false, false, true) + "</b>");
            sb.AppendLine("</p>");
            sb.AppendLine(" <!--EndFragment-->  </body> </html>");

            string allines = sb.ToString();
            //Clipboard.SetData(DataFormats.Html, allines);
            //Clipboard.SetText(sequence.Sequence);
            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, allines, true); ///.SetText(allines, TextDataFormat.Html); ///.SetData(DataFormats.Html, allines, true);
            dataObject.SetData(DataFormats.Text, sequence.Sequence, true); ///.SetText(sequence.Sequence, TextDataFormat.Text);

            Clipboard.SetDataObject(dataObject, true);
        }

        public static void copyHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Version:1.0");
            sb.AppendLine("StartHTML:0000000105");
            sb.AppendLine("EndHTML:" + 4900);
            sb.AppendLine("StartFragment:000004607");
            sb.AppendLine("EndFragment:" + (4900 - 40));
            System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            //Location is where the assembly is run from
            string assemblyLocation = assemblyInfo.Location;
            //CodeBase is the location of the ClickOnce deployment files
            Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
            string ClickOnceLocation = System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Files\\";
            using (StreamReader sr = new StreamReader(ClickOnceLocation + @"\HTMLCopy.txt"))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            sb.AppendLine("<p class=MsoNormal> Scan(s) ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> Mass ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> Z ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span>  Tag ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> Description ");
            sb.AppendLine(" <span style='mso-tab-count: 1'>         </span> Accession ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> Start ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> End ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> tStart ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> tEnd ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> Hits ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> Delta ");
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> Best Matched Sequence ");
            sb.AppendLine(" <span style='mso-tab-count: 1'>         </span> FileName   ");
            sb.AppendLine(" <span style='mso-tab-count: 1'>         </span> Sequence ");
            sb.AppendLine("</p> <p class=MsoNormal> </p> <p class=MsoNormal> </p>");

            sb.AppendLine(" <!--EndFragment-->  </body> </html>");

            string allines = sb.ToString();
            Clipboard.SetData(DataFormats.Html, allines);
        }

        /// <summary>
        /// Returns N/A if the variable part is a null or empty.
        /// Else uses the constant part.
        /// </summary>
        /// <param name="constantpart"></param>
        /// <param name="variablepart"></param>
        /// <returns></returns>
        static string ReturnstringorNA(string constantpart, string variablepart)
        {
            if (variablepart == null || variablepart == "")
            {
                return constantpart + "N/A";
            }
            else
            {
                return constantpart + variablepart;
            }
        }
        /// <summary>
        /// Returns N/A if the variable part is a null or zero.
        /// Else uses the constant part.
        /// </summary>
        /// <param name="constantpart"></param>
        /// <param name="variablepart"></param>
        /// <returns></returns>
        static string ReturnstringorNA(string constantpart, double variablepart)
        {
            if (variablepart == null || variablepart == 0)
            {
                return constantpart + "N/A";
            }
            else
            {
                return constantpart + Convert.ToString(variablepart);
            }
        }
        /// <summary>
        /// Returns N/A if the variable part is a null or zero.
        /// Else uses the constant part.
        /// </summary>
        /// <param name="constantpart"></param>
        /// <param name="variablepart"></param>
        /// <returns></returns>
        static string ReturnstringorNA(string constantpart, int variablepart)
        {
            if (variablepart == null || variablepart == 0)
            {
                return constantpart + "N/A";
            }
            else
            {
                return constantpart + Convert.ToString(variablepart);
            }
        }

        /// <summary>
        /// Copy the entire protein with highlighting
        /// </summary>
        /// <param name="sequence"></param>
        public static void copyaprotein(SearchResult sequence)
        {
            int bytecount = Encoding.UTF8.GetByteCount(sequence.Sequence) + 4607;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Version:1.0");
            sb.AppendLine("StartHTML:0000000105");
            sb.AppendLine("EndHTML:" + bytecount);
            sb.AppendLine("StartFragment:000004607");
            sb.AppendLine("EndFragment:" + (bytecount - 40));
            System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            //Location is where the assembly is run from
            string assemblyLocation = assemblyInfo.Location;
            //CodeBase is the location of the ClickOnce deployment files
            Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
            string ClickOnceLocation = System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Files\\";
            using (StreamReader sr = new StreamReader(ClickOnceLocation + @"\HTMLCopy.txt"))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            string tagforcopy = concatinateblasttags(sequence.BlastTagforTopProtein, sequence.BlastpPartialMatch);
            if (tagforcopy == "" || tagforcopy == null)
            {
                if (sequence.TagForTopProtein != null)
                {
                    tagforcopy = sequence.TagForTopProtein;
                }
                else if (sequence.AnchorTag != null)
                {
                    tagforcopy = sequence.AnchorTag.RawSequence;
                }
                else
                {
                    tagforcopy = "";
                }
                //tagforcopy = sequence.TagForTopProtein != null ? sequence.TagForTopProtein : sequence.AnchorTag.RawSequence;
                sequence.TagForTopProtein = tagforcopy;
            }
            if (sequence.ScanNumbers != null)
            {
                string scannumbers = sequence.ScanNumbers.Contains(",") ? string.Join(" , ", sequence.ScanNumbers.Replace("  ", string.Empty).Split(',')) : sequence.ScanNumbers;

                //sequence.ScanNumbers = sequence.ScanNumbers.Contains(",") ? string.Join(" , ", sequence.ScanNumbers.Split(',')) : sequence.ScanNumbers;

                sb.AppendLine(ReturnstringorNA("<p class=MsoNormal><label style=\"align-content:flex-end;\">", scannumbers) + "</label>");

            }
            else
            {
                sb.AppendLine(ReturnstringorNA("<p class=MsoNormal><label style=\"align-content:flex-end;\">", "N/A") + "</label>");
            }
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span>" + sequence.ParentMass);



            //if (sequence.ParentZ == null || sequence.ParentZ == 0)
            //{
            //    sb.AppendLine("<span style='mso-tab-count: 1'>         </span> " + "N/A");
            //}
            //else
            //{
            sb.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", sequence.ParentZ));
            //}
            //if (tagforcopy == "")
            //{
            //    sb.AppendLine("<span style='mso-tab-count: 1'>         " + "N/A");
            //}
            //else
            //{
            sb.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         ", tagforcopy) + "</span>");
            //}
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> ");//+ sequence.Description);

            string Accession = sequence.Accession;
            var accession = FindAccessions.ReturnLinksforFindingAccessioninUniProt(Accession);
            if (accession.Indexforuni < 0)
            {
                sb.Append(sequence.Description);
            }
            else
            {
                sb.Append(" ");
                sb.Append("<a href='");
                sb.Append("http://www.uniprot.org/uniprot/?query=" + accession.Accession);
                sb.Append("'/>");
                sb.Append(sequence.Description);
                sb.Append(" </a>");
            }
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> " + Accession);
            //if (sequence.Start != null && sequence.Start != 0)
            {
                sb.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span>", sequence.Start));
            }
            //if (sequence.endend != null && sequence.endend != 0)
            {
                sb.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", sequence.End));
            }
            //if (sequence.tStart != null && sequence.tStart != 0)
            {
                sb.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", sequence.tStart));
            }
            //if (sequence.tEnd != null && sequence.tEnd != 0)
            {
                sb.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", sequence.tEnd));
            }
            //if (sequence.YellowandGreenTagHits != null && sequence.YellowandGreenTagHits != 0)
            {
                sb.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", Math.Round(sequence.YellowandGreenTagHits, 2)));
                if (sequence.YellowandGreenTagHits != 0)
                {
                    sb.AppendLine(("<span style='mso-tab-count: 1'>         </span> " + Math.Round(sequence.DeltaMassVersusProtein, 2)));
                }
                else
                {
                    sb.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", Math.Round(sequence.DeltaMassVersusProtein, 2)));
                }
            }
            //sb.AppendLine("<span style='mso-tab-count: 1'>         </span> " + sequence.CoveredSequence != null ? sequence.CoveredSequence : string.Empty);

            if (sequence.CoveredSequence != null)
            {
                string seq = sequence.CoveredSequence;
                if (seq == "")
                {
                    sb.AppendLine("<span style='mso-tab-count: 1'>         </span> " + sequence.Sequence);
                }
                else
                {
                    sb.AppendLine("<span style='mso-tab-count: 1'>         </span> " + sequence.CoveredSequence);
                }
            }
            sb.AppendLine("<span style='mso-tab-count: 1'>         </span> " + MainWindow.MainViewModel.SpectralDataFilename + "   ");
            sb.AppendLine(" <span style='mso-tab-count: 1'>         </span> <b style='mso-bidi-font-weight: normal'>  ");
            sb.Append(concatinatematchstartends(sequence.Sequence, sequence.matchstartends) + "</b>");

            sb.AppendLine("</p>");

            sb.AppendLine(" <!--EndFragment-->  </body> </html>");

            string allines = sb.ToString();
            Clipboard.SetData(DataFormats.Html, allines);
        }


        /// <summary>
        /// Copy the datagrid onto the clipboard
        /// </summary>
        /// <param name="publicresults"></param>
        public static void copyreport(IEnumerable<SearchResult> publicresults)
        {
            try
            {
                App app = (App)Application.Current;
                StringBuilder sb1 = new StringBuilder();
                sb1.AppendLine("<p class=MsoNormal> Scan(s) ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> Mass ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> Z ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span>  Tag ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> Description ");
                sb1.AppendLine(" <span style='mso-tab-count: 1'>         </span> Accession ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> Start ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> End ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> tStart ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> tEnd ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> Hits ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> Delta ");
                sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> Best Matched Sequence ");
                sb1.AppendLine(" <span style='mso-tab-count: 1'>         </span> FileName   "); ///</p> <p class=MsoNormal> </p> <p class=MsoNormal> </p>");
                sb1.AppendLine(" <span style='mso-tab-count: 1'>         </span> Sequence ");//</p> <p class=MsoNormal> </p> <p class=MsoNormal> </p>
                sb1.AppendLine("</p> <p class=MsoNormal> </p> <p class=MsoNormal> </p>");

                foreach (var res in publicresults.OrderBy(a => a.ScanNumbers).ToList())
                {
                    if (res.Sequence == null) continue;
                    string tagforcopy = concatinateblasttags(res.BlastTagforTopProtein, res.BlastpPartialMatch);
                    if (tagforcopy == "" || tagforcopy == null)
                    {
                        tagforcopy = res.TagForTopProtein;
                    }
                    if (res.ScanNumbers != null)
                    {
                        string scannumbers = res.ScanNumbers.Contains(",") ? string.Join(" , ", res.ScanNumbers.Replace("  ", string.Empty).Split(',')) : res.ScanNumbers;
                        sb1.AppendLine("<p class=MsoNormal><label style=\"align-content:flex-end;\">" + scannumbers + "</label>");
                    }
                    else
                    {
                        res.ScanNumbers = "";
                        sb1.AppendLine("<p class=MsoNormal><label style=\"align-content:flex-end;\">" + "N/A" + "</label>");
                    }


                    sb1.AppendLine("<span style='mso-tab-count: 1'>         </span>" + res.ParentMass);

                    sb1.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", res.ParentZ));
                    sb1.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         ", tagforcopy) + "</span>");
                    sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> "); ///+ res.Description
                    sb1.Append(" ");

                    string Accession = res.Accession;
                    var accessions = FindAccessions.ReturnLinksforFindingAccessioninUniProt(Accession);
                    if (accessions.Indexforuni < 0)
                    {
                        sb1.Append(res.Description);
                    }
                    else
                    {
                        sb1.Append("<a href='");
                        sb1.Append("http://www.uniprot.org/uniprot/?query=" + accessions.Accession + "&sort=score");
                        sb1.Append("'/>");
                        sb1.Append(res.Description);
                        sb1.Append(" </a>");
                    }
                    sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> " + Accession);

                    sb1.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span>", res.Start));
                    sb1.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", res.End));
                    sb1.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", res.tStart));
                    sb1.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", res.tEnd));
                    sb1.AppendLine(ReturnstringorNA("<span style='mso-tab-count: 1'>         </span> ", Math.Round(res.YellowandGreenTagHits, 2)));
                    if (res.YellowandGreenTagHits != 0)
                    {
                        sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> " + Math.Round(res.DeltaMassVersusProtein, 2));
                    }
                    else
                    {
                        sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> " + "N/A");
                    }
                    if (res.CoveredSequence != "" && res.CoveredSequence != null)
                    {
                        sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> " + res.CoveredSequence);
                    }
                    else
                    {
                        sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> " + "N/A");
                    }
                    sb1.AppendLine("<span style='mso-tab-count: 1'>         </span> " + MainWindow.MainViewModel.SpectralDataFilename + "   ");
                    sb1.AppendLine(" <span style='mso-tab-count: 1'>         </span> <b style='mso-bidi-font-weight: normal'>  ");
                    if (res.CoveredSequence != "" && res.CoveredSequence != null)
                    {
                        sb1.Append(concatinatematchstartends(res.Sequence, res.matchstartends) + "</b>");
                    }
                    else
                    {
                        sb1.Append(res.Sequence + "</b>");
                    }
                    sb1.AppendLine("</p>");  ///<span style='mso-tab-count: 1'>         </span> <b style='mso-bidi-font-weight: normal'>  <span style='color: red'>" + res.Sequence + " </span>
                }

                sb1.AppendLine(" <!--EndFragment-->  </body> </html>");

                string allines = sb1.ToString();

                int bytecount = Encoding.UTF8.GetByteCount(allines) + 4607;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Version:1.0");
                sb.AppendLine("StartHTML:0000000105");
                sb.AppendLine("EndHTML:" + bytecount);
                sb.AppendLine("StartFragment:000004607");
                sb.AppendLine("EndFragment:" + (bytecount - 40));
                System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
                //Location is where the assembly is run from
                string assemblyLocation = assemblyInfo.Location;
                //CodeBase is the location of the ClickOnce deployment files
                Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
                string ClickOnceLocation = System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Files\\";
                using (StreamReader sr = new StreamReader(ClickOnceLocation + @"\HTMLCopy.txt"))
                {
                    String line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        sb.AppendLine(line);
                    }
                }

                sb.Append(sb1);

                allines = sb.ToString();
                Clipboard.SetData(DataFormats.Html, allines);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\t" + ex.Source + "\t" + ex.StackTrace);
            }
            finally
            {

            }

        }

        /// <summary>
        /// Print the PDF based on the string given to this method
        /// </summary>
        /// <param name="sb"></param>
        static string PrintPDF(StringBuilder sb, string Path = null)
        {
            System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            //Location is where the assembly is run from
            string assemblyLocation = assemblyInfo.Location;
            //CodeBase is the location of the ClickOnce deployment files
            Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
            string ClickOnceLocation = System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Files\\";

            System.IO.File.WriteAllText(System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\HtmlFiles\\SamplePage.html", sb.ToString());// sbattachement.ToString());

            string filename = (MainWindow.MainViewModel.SpectralDataFilename.Length > 40) ? MainWindow.MainViewModel.SpectralDataFilename.Substring(0, 40) : MainWindow.MainViewModel.SpectralDataFilename;

            string sPathToWritePdfTo = "ProtReport_" + Guid.NewGuid().ToString().Substring(0, 6) + "_" + filename + ".pdf";/// DeChargerModel.FileName.Substring(0, 40) + ".pdf";

            ClickOnceLocation = "\"" + System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Executables\\";

            string pdflocation = Path == null ? (ClickOnceLocation + sPathToWritePdfTo) : Path;

            string htmlpagelocation = "\"" + System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\HtmlFiles\\";
            System.Diagnostics.ProcessStartInfo startInfo1 = new System.Diagnostics.ProcessStartInfo(ClickOnceLocation + "wkhtmltopdf32min.exe\"  " + htmlpagelocation + "SamplePage.html\" " + pdflocation + "\""); ///wkhtmltopdf1.exe  ///wkhtmltox-0.12.2.2_msvc2013-win32.exe
            if (File.Exists(System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Executables\\" + sPathToWritePdfTo))
            {
                File.Delete(System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Executables\\" + sPathToWritePdfTo);
                Thread.Sleep(1000);
            }
            //Cannot use the Shell
            startInfo1.UseShellExecute = false;
            //Redirecting the output for use
            startInfo1.RedirectStandardOutput = true;
            //Need to specify the working directory for some of the commands to execute
            //A command prompt is normally opened to execute. This helps enables to run the commands without opening a window
            startInfo1.CreateNoWindow = true;
            startInfo1.LoadUserProfile = true;
            Thread.Sleep(200);

            using (System.Diagnostics.Process proc = System.Diagnostics.Process.Start(startInfo1))
            {
                proc.Close();
            }
            return sPathToWritePdfTo;
        }


        /// <summary>
        /// Creating the Header for the email 
        /// </summary>
        /// <returns></returns>
        static StringBuilder CreateEmailHeader()
        {
            StringBuilder sb = new StringBuilder();
            //StringBuilder sbattachement = new StringBuilder();
            System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            //Location is where the assembly is run from
            string assemblyLocation = assemblyInfo.Location;
            //CodeBase is the location of the ClickOnce deployment files
            Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
            string ClickOnceLocation = System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Files\\";
            using (StreamReader sr = new StreamReader(ClickOnceLocation + "\\CustomEmailHeader.txt"))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            sb.AppendLine("<html> <head> <title> Testing </title> </head> <body  style=\"font-family:Verdana, Geneva, Tahoma, sans-serif; font-size: 13px;\"> ");

            return sb;
        }

        /// <summary>
        /// Creating Body for the email based on the sequence search results
        /// </summary>
        /// <param name="publicresults"></param>
        /// <returns></returns>
        static StringBuilder CreateEmailBody(ObservableCollection<SearchResult> publicresults, string timeduration, string Message = null)
        {
            StringBuilder sb = new StringBuilder();

            System.Reflection.Assembly assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            //Location is where the assembly is run from
            string assemblyLocation = assemblyInfo.Location;
            //CodeBase is the location of the ClickOnce deployment files
            Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
            string ClickOnceLocation = System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Files\\";

            var distinctproteins = publicresults.Where(a => a.Description != null)
                                                .OrderByDescending(a => a.YellowandGreenTagHits)
                                                .GroupBy(a => a.Description.ToUpper())
                                                .Select(a => new
                                                {
                                                    Description = a.First().Description,
                                                    AllAccessions = a.Select(b => b.Accession).ToList(),
                                                    YellowandGreenTagHits = a.First().YellowandGreenTagHits,
                                                    Count = a.Count()
                                                })
                                                .OrderByDescending(a => a.YellowandGreenTagHits)
                                                .ThenByDescending(a => a.Count)
                                                .ThenBy(a => a.Description.ToLower().Contains("uncharacterized"))
                                                .ToList();


            if (Message != null)
            {
                string imgpath = System.IO.Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()) + "\\Icons\\" + "lilly2.png";
                sb.AppendLine("<table><tr> <td  style='width:75%; border:none;'>");
                sb.AppendLine(" </td> <td style='width:25%; border:none;'> ");
                sb.Append("<img src='" + imgpath + "'\\>  </td> </tr> </table> <br /><br />");

                if (Message != "")
                {
                    sb.AppendLine("<b> Notes </b>  <br/>");
                    sb.AppendLine("<i>");
                    sb.AppendLine(Message);
                    sb.AppendLine("</i> <br/><br/><br/>");
                }
            }

            sb.AppendLine("The following proteins were found in file " + MainWindow.MainViewModel.SpectralDataFilename + ":");
            sb.AppendLine("<ol>");

            foreach (var protein in distinctproteins)
            {
                sb.AppendLine("<li>" + protein.Description);
                //if (!DechargerVM.UseFasta)
                {
                    foreach (var accesion in protein.AllAccessions.Distinct())
                    {
                        if (accesion.Contains("SP") || accesion.Contains("GI") || accesion.Contains("RS") || accesion.Contains("SWISS"))
                        {
                            sb.Append(" ");
                            sb.Append("[<a href='");
                            sb.Append("http://www.uniprot.org/uniprot/?query=" + FindAccessions.ReturnLinksforFindingAccessioninUniProt(accesion).Accession + "&sort=score");
                            sb.Append("'>");
                            sb.Append(FindAccessions.Returnaccession(accesion));
                            sb.Append(" </a>]");
                        }
                    }
                }
                sb.Append("</li>");
            }

            sb.AppendLine("</ol>");

            using (StreamReader sr = new StreamReader(ClickOnceLocation + "\\CustomEmailTableHeader.txt"))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            var pub = publicresults.OrderByDescending(a => a.YellowandGreenTagHits).ToList();
            int totalcount = pub.Count;
            bool trueorfalse = true;
            int i = 1;

            foreach (var res in pub)
            {
                if (trueorfalse)
                {
                    sb.AppendLine("<tr>");
                    trueorfalse = false;
                }
                else
                {
                    sb.AppendLine("<tr style=\"background:#F2F2F2\">");
                    trueorfalse = true;
                }

                if (res.Sequence == null)
                {
                    i++;
                    continue;
                }

                sb.AppendLine("<td width=\"22%\" valign=top>");
                sb.Append("<p style=\"font-family:Verdana, Geneva, Tahoma, sans-serif;\">" + res.Description + "</p></td>");

                sb.AppendLine(" <td width=\"7%\" valign=top>");
                sb.Append("<p class=\"tcoloumn\">" + Math.Round(res.YellowandGreenTagHits, 2) + "</p></td>");

                sb.AppendLine("<td width=\"8%\" valign=top>");
                sb.Append("<p class=\"tcoloumn tdwordwrap\" style=\"width:30px;\">" + res.ScanNumbers + "</p></td>");

                sb.AppendLine("<td width=\"6%\" valign=top>");
                sb.Append("<p class=\"tcoloumn\">" + Math.Round(res.ParentMass, 2) + "</p></td>");

                sb.AppendLine("<td width=\"8%\" valign=top>");
                sb.Append("<p class=\"tcoloumn tdwordwrap\" style=\"width:25px;\">" + res.Parentz_s + "</p></td>");

                string TagforEmail = concatinateblasttags(res.BlastTagforTopProtein, res.BlastpPartialMatch);

                if (TagforEmail == "" || TagforEmail == null)
                {
                    TagforEmail = res.TagForTopProtein;
                }

                sb.AppendLine("<td width=\"6%\" valign=top>");
                sb.Append("<p class=\"tcoloumn tdwordwrap\" style=\"width:60px;\">" + TagforEmail + "</p></td>");

                sb.AppendLine("<td width=\"25%\" valign=top>");
                sb.Append("<p class=\"tcoloumn tdwordwrap\" style=\"width:200px;\">");
                sb.Append(concatinatematchstartends(res.Sequence, res.matchstartends, true, true));
                sb.Append("</p></td>");

                sb.AppendLine("<td width=\"6%\" valign=top>");
                sb.Append("<p class=\"tcoloumn\">" + res.Accession + "</p></td>");

                sb.AppendLine("<td width=\"6%\" valign=top>");
                sb.Append("<p class=\"tcoloumn\">" + Math.Round(res.DeltaMassVersusProtein, 2) + "</p></td>");

                sb.AppendLine("<td width=\"3%\" valign=top>");
                sb.Append("<p class=\"tcoloumn\">" + res.Start + "</p></td>");

                sb.AppendLine("<td width=\"2%\" valign=top>");
                sb.Append("<p class=\"tcoloumn\">" + res.End + "</p></td>");

                sb.AppendLine("<td width=\"4%\" valign=top>");
                sb.Append("<p class=\"tcoloumn\">" + res.tStart + "</p></td>");

                sb.AppendLine("<td width=\"3%\" valign=top>");
                sb.Append("<p class=\"tcoloumn\">" + res.tEnd + "</p></td>");

                sb.Append("</tr>");

                i++;
            }

            sb.AppendLine("</table>");

            sb.AppendLine("<br />");

            string fullversion = string.Empty;
            fullversion = App.AppVersion;

            sb.AppendLine("Run Duration: " + timeduration + "<br/>");
            sb.AppendLine("Run Date: " + DateTime.Now.ToLongDateString() + "<br/>");
            sb.AppendLine("Generated by: " + Environment.UserName + "<br/><br/><br/>");

            //sb.AppendLine();

            sb.Append("<b> <u> Settings: </u> </b> <br />");
            sb.AppendLine("Mass Tolerance for Ion Detection: " + Properties.Settings.Default.MassTolerancePPM + " PPM<br />");
            sb.AppendLine("Match Tolerance: " + Properties.Settings.Default.MatchTolerancePPM + " PPM <br />");
            sb.AppendLine("Minimum Sequence Tag Length: " + Properties.Settings.Default.SequenceTagLength + "<br />");
            sb.AppendLine("Databases: " + (MainWindow.MainViewModel.UseFasta ? "FASTA" : "SQL Server") + "<br />");
            sb.AppendLine("Min Peak Intensity: " + Properties.Settings.Default.MinPeak + "<br />");
            sb.AppendLine("Blastp or Exact: " + (Properties.Settings.Default.UseBlast ? "Blastp (Mismatches within tag will be shown in" + "<b style='color:red;'>" + " Red </b>)" : "Exact") + "<br />");
            sb.AppendLine("Rapid or Thorough: " + (Properties.Settings.Default.UseThoroughSearch ? "Thorough" : "Rapid") + "<br />");
            sb.AppendLine("Version of the Program: " + fullversion + "<br /> <br/>");
            string[] splitst = { "\t" };

            string[] st = Properties.Settings.Default.Genus.Split(splitst, StringSplitOptions.RemoveEmptyEntries);
            if (!MainWindow.MainViewModel.UseFasta)
            {
                sb.AppendLine("Species: ");
                if (!Properties.Settings.Default.SearchAllSpecies)
                {
                    sb.AppendLine("<ol>");
                    foreach (var species in App.DistinctSpecies.Where(a => a.IsSelected).Select(a => a.Genus).ToList())
                    {
                        sb.AppendLine("<li>" + species + "</li>");
                    }
                    sb.AppendLine("</ol> <br/> <br/>");
                }
                else
                {
                    sb.AppendLine("All Species <br /> <br />");
                }
            }


            if (MainWindow.MainViewModel.UseFasta)
            {
                if (App.lstFasta.Count > 20)
                {
                    sb.AppendLine("<i> Displaying the first 20 out of ");
                    sb.Append(App.lstFasta.Count);
                    sb.Append(" FASTA sequences </i> <br />");
                }
                else
                {
                    sb.Append("<br />");
                }
                sb.AppendLine("<b> FASTA : ");
                string[] splitstringn = { "\n" };
                sb.Append(String.Join(" , ", App.FASTAName.Split(splitstringn, StringSplitOptions.RemoveEmptyEntries)));
                sb.Append("</b><br /> <br />");
                sb.AppendLine("<p style=\"width:1000px;word-wrap: break-word;\">");
                sb.Append(App.FirstTwentyFasta.ToString());
                sb.AppendLine("</p>");
            }

            sb.AppendLine("<p> <hr /> <b> An LRL Informatics Solution </b> </p>");

            sb.AppendLine(" <!--EndFragment-->  </body> </html>");

            return sb;
        }

        public static void SaveaPDF(ObservableCollection<SearchResult> publicresults, string Path, string timeduration, string Message = null)
        {
            StringBuilder sb = new StringBuilder();
            sb = CreateEmailHeader();
            if (Message != null)
            {
                sb.Append(CreateEmailBody(publicresults, timeduration, Message).ToString());
            }
            else
            {
                sb.Append(CreateEmailBody(publicresults, timeduration).ToString());
            }
            PrintPDF(sb, Path);
        }



        static void EmailErrorMessage()
        {
            System.Windows.MessageBox.Show("Can't email.", "No network access.", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        static string concatinateblasttags(string BlastTagforTopProtein, List<MatchStartEnds> BlastpPartialMatch)
        {
            StringBuilder sb = new StringBuilder();

            if (BlastpPartialMatch != null)
            {
                if (string.IsNullOrWhiteSpace(BlastTagforTopProtein)) return string.Empty;

                foreach (var aa in BlastpPartialMatch.Where(t => !string.IsNullOrWhiteSpace(t.SequenceTag)))
                {
                    sb.Append(generatehtmlfortags(aa.SequenceTag, aa.confidence));
                }
            }

            return sb.ToString();
        }

        private class AminoAcidWithConfidence
        {
            public int Index;
            public char Symbol;
            public AminAcidConfidence Score;

            public override string ToString()
            {
                return Symbol.ToString() + Score.ToString();
            }
        }

        /// <summary>
        /// Based on the ProteinSequence and all the tags generate matchstartends which are used to highlight the sequence shown in the grid
        /// </summary>
        /// <param name="ProteinSequence"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        static StringBuilder concatinatematchstartends(string ProteinSequence, List<MSViewer.MatchStartEnds> tgs, bool Dontshowall = false, bool foremailreport = false, bool forsequence = false)
        {
            StringBuilder sb = new StringBuilder();
            var sequenceWithConfidence = new List<AminoAcidWithConfidence>();
            int currentIndex = 0;
            var tags = tgs.ToList();
            // Create an ordered list of all amino acids in the protein with default Confidence of None for that Amino Acid
            foreach (var aChar in ProteinSequence)
                sequenceWithConfidence.Add(new AminoAcidWithConfidence() { Index = currentIndex++, Symbol = aChar, Score = AminAcidConfidence.None });

            if (tags.Where(a => a.DontShowall).Any())
                Dontshowall = true;
            // Assigns the highest confidence value to each amino acid based on the confidence of the passed in tags

            foreach (var aMatch in tags.OrderBy(t => t.confidence))
            {
                if (aMatch.End - aMatch.Start < 1) continue;

                foreach (var aa in sequenceWithConfidence.Where(a => a.Index >= aMatch.Start && a.Index < (aMatch.Start + aMatch.Length)))
                    aa.Score = aMatch.confidence;
            }

            var currentConfidence = sequenceWithConfidence.First().Score;
            var runStart = 0;

            if (Dontshowall && !forsequence)
            {
                List<AminoAcidWithConfidence> grayarea = sequenceWithConfidence.Where(a => a.Score == AminAcidConfidence.NotPossible || a.Score == AminAcidConfidence.Reallybad).ToList(); //Checking if there is any gray area

                if (grayarea.Any()) //If there is any gray area then the following logic should be applied
                {
                    var notgrayarea = sequenceWithConfidence.Where(a => a.Score != AminAcidConfidence.NotPossible && a.Score != AminAcidConfidence.Reallybad).ToList(); //Area which is not gray
                    if (!notgrayarea.Any()) return sb;
                    int startnotgrayarea = notgrayarea.Min(a => a.Index);

                    int endnotgrayarea = notgrayarea.Max(a => a.Index);

                    if (grayarea.Where(a => a.Index < (startnotgrayarea - 30)).Any())
                    {
                        ProteinSequence = "←" + ProteinSequence.Substring(startnotgrayarea - 30);
                        sequenceWithConfidence = sequenceWithConfidence.Where(a => a.Index >= (startnotgrayarea - 30)).ToList();

                        sequenceWithConfidence.Add(new AminoAcidWithConfidence
                        {
                            Index = startnotgrayarea - 31,
                            Score = AminAcidConfidence.NotPossible,
                            Symbol = '←'
                        });
                        sequenceWithConfidence = sequenceWithConfidence.OrderBy(a => a.Index).ToList();
                        List<AminoAcidWithConfidence> seq = new List<AminoAcidWithConfidence>();
                        int cseq = 0;
                        foreach (var se in sequenceWithConfidence)
                        {
                            seq.Add(new AminoAcidWithConfidence
                            {
                                Symbol = se.Symbol,
                                Index = cseq,
                                Score = se.Score
                            });
                            cseq++;
                        }
                        sequenceWithConfidence = seq.ToList();
                        endnotgrayarea = sequenceWithConfidence.Where(a => a.Score != AminAcidConfidence.NotPossible && a.Score != AminAcidConfidence.Reallybad).ToList().Max(a => a.Index);
                        grayarea = sequenceWithConfidence.Where(a => a.Score == AminAcidConfidence.NotPossible || a.Score == AminAcidConfidence.Reallybad).ToList();
                    }

                    if (grayarea.Where(a => a.Index > (endnotgrayarea + 30)).Any())
                    {
                        ProteinSequence = ProteinSequence.Substring(0, endnotgrayarea + 31) + "→";
                        sequenceWithConfidence = sequenceWithConfidence.Where(a => a.Index <= (endnotgrayarea + 30)).ToList();
                        sequenceWithConfidence.Add(new AminoAcidWithConfidence
                        {
                            Index = endnotgrayarea + 31,
                            Score = AminAcidConfidence.NotPossible,
                            Symbol = '→'
                        });
                        List<AminoAcidWithConfidence> seq = new List<AminoAcidWithConfidence>();
                        int cseq = 0;
                        foreach (var se in sequenceWithConfidence)
                        {
                            seq.Add(new AminoAcidWithConfidence
                            {
                                Symbol = se.Symbol,
                                Index = cseq,
                                Score = se.Score
                            });
                            cseq++;
                        }
                        sequenceWithConfidence = seq.ToList();
                    }
                }
            }

            foreach (var aa in sequenceWithConfidence)
            {
                if (aa.Score != currentConfidence)
                {
                    if (foremailreport)
                    {
                        sb.Append(generatehtmlusingtagsforemailreport(ProteinSequence.Substring(runStart, (aa.Index - runStart)), currentConfidence));
                    }
                    else
                    {
                        sb.Append(generatehtmlfortags(ProteinSequence.Substring(runStart, (aa.Index - runStart)), currentConfidence));
                    }
                    runStart = aa.Index;
                    currentConfidence = aa.Score;
                }
            }

            var lastAA = sequenceWithConfidence.Last();
            if (foremailreport)
            {
                sb.Append(generatehtmlusingtagsforemailreport(ProteinSequence.Substring(runStart, (lastAA.Index - runStart) + 1), currentConfidence));
            }
            else
            {
                sb.Append(generatehtmlfortags(ProteinSequence.Substring(runStart, (lastAA.Index - runStart) + 1), currentConfidence));
            }
            return sb;
        }

        /// <summary>
        /// Generate HTML for tags based on the confidence and the sequence tag.
        /// All the spans which are generated here use a particular background color based on the confidence of the sequence tag
        /// </summary>
        /// <param name="SequenceTag">Tag</param>
        /// <param name="confidence">Confidence of the tag</param>
        /// <returns></returns>
        static StringBuilder generatehtmlfortags(string SequenceTag, AminAcidConfidence confidence)
        {
            StringBuilder sb = new StringBuilder();
            switch (confidence)
            {
                case AminAcidConfidence.High:
                    sb.Append("<span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: #7F7F7F; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.Medium:
                    sb.Append("<span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: #7F7F7F; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.Low:
                    sb.Append("<u><span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: #558ED5; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span></u>");
                    break;
                case AminAcidConfidence.Sure:
                    sb.Append("<u><span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: black; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span></u>");
                    break;
                case AminAcidConfidence.Possible:
                    sb.Append("<u><span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: black; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span></u>");
                    break;
                case AminAcidConfidence.NotSure:
                    sb.Append("<span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: #4B4A4A; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.NotPossible:
                    sb.Append("<span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: #CBCBCB; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.Gap:
                    sb.Append("<u><span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: red; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span></u>");
                    break;
                case AminAcidConfidence.None:
                    break;
                case AminAcidConfidence.Reallybad:
                    sb.Append("<span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: #7F7F7F; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.SearchHit:
                    sb.Append("<span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: #7030A0; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.BlastTagMatch:
                    sb.Append("<b><span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: black; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span></b>");
                    break;
                case AminAcidConfidence.BlastTagMisMatch:
                    sb.Append("<b><span style='font-size: 12.0pt; line-height: 115%; font-family: Consolas; mso-fareast-font-family: Calibri; mso-fareast-theme-font: minor-latin; color: red; mso-ansi-language: EN-US; mso-fareast-language: EN-US; mso-bidi-language: AR-SA'>" + SequenceTag + "</span></b>");
                    break;
                default:
                    break;
            }
            return sb;
        }

        /// <summary>
        /// Generate HTML for tags based on the confidence and the sequence tag. This method is for generating email report.
        /// All the spans which are generated here use a particular background color based on the confidence of the sequence tag
        /// </summary>
        /// <param name="SequenceTag">Tag</param>
        /// <param name="confidence">Confidence of the tag</param>
        /// <returns></returns>
        static StringBuilder generatehtmlusingtagsforemailreport(string SequenceTag, AminAcidConfidence confidence)
        {
            StringBuilder sb = new StringBuilder();
            switch (confidence)
            {
                case AminAcidConfidence.High:
                    sb.Append("<span style='font-size:10.0pt;font-family:Consolas;color:#7F7F7F;'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.Medium:
                    sb.Append("<span style='font-size:10.0pt;font-family:Consolas;color:#7F7F7F;'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.Low:
                    sb.Append("<b><span style='font-size:10.0pt;font-family:Consolas;color:white;background:royalblue;'>" + SequenceTag + "</span></b>"); //<span style='font-size:10.0pt;font-family:Consolas;color:white;mso-themecolor:background1;background:royalblue;mso-highlight:royalblue'>
                    break;
                case AminAcidConfidence.Sure:
                    sb.Append("<b><span style='font-size:10.0pt;font-family:Consolas;color:white;background:black;'>" + SequenceTag + "</span></b>");
                    break;
                case AminAcidConfidence.Possible:
                    sb.Append("<b><span style='font-size:10.0pt;font-family:Consolas;color:black;'>" + SequenceTag + "</span></b>");
                    break;
                case AminAcidConfidence.NotSure:
                    sb.Append("<b><span style='font-size:10.0pt;font-family:Consolas;color:black;'>" + SequenceTag + "</span></b>");
                    break;
                case AminAcidConfidence.NotPossible:
                    sb.Append("<span style='font-size:10.0pt;font-family:Consolas;color:#7F7F7F;'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.Gap:
                    sb.Append("<b><span style='font-size:10.0pt;font-family:Consolas;color:white;background:darkred'>" + SequenceTag + "</span></b>");
                    break;
                case AminAcidConfidence.None:
                    break;
                case AminAcidConfidence.Reallybad:
                    sb.Append("<span style='font-size:10.0pt;font-family:Consolas;color:#7F7F7F;'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.SearchHit:
                    sb.Append("<span style='font-size:10.0pt;font-family:Consolas;color:white;background:purple;'>" + SequenceTag + "</span>");
                    break;
                case AminAcidConfidence.BlastTagMatch:
                    sb.Append("<b><span style='font-size: 10.0pt; line-height: 115%; font-family: Consolas; color: black;'>" + SequenceTag + "</span></b>");
                    break;
                case AminAcidConfidence.BlastTagMisMatch:
                    sb.Append("<b><span style='font-size: 10.0pt; line-height: 115%; font-family: Consolas;'>" + SequenceTag + "</span></b>");
                    break;
                default:
                    break;
            }
            return sb;
        }


    }
}

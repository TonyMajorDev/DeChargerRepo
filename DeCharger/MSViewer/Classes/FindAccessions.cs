using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MSViewer.Classes
{
    public class FindAccessions
    {
        /// <summary>
        /// Finds all the accessions.
        /// There is an order of precedence in which it needs to find what is an accession
        /// 1) GI 
        /// 2) SWISS-PROT
        /// 3) RS
        /// If none of the above is present in the accession value then it uses the entire accession
        /// </summary>
        /// <param name="accession"></param>
        /// <returns></returns>
        static Accessions FindAccesion(string accession)
        {
            int index = -1;
            string allaccessions = string.Empty;
            if (!(accession.IndexOf("SP") < 0)) ///Check if there is GI
            {
                index = accession.IndexOf("SP");
                allaccessions = accession.Substring(index + 3, accession.Length - (index + 3)).Split(' ').First();
            }
            else if (!(accession.IndexOf("SWISS-PROT") < 0)) ///If no GI in the accession then use SWISS-PROT
            {
                index = accession.IndexOf("SWISS-PROT");
                allaccessions = accession.Substring(index + 11, accession.Length - (index + 11)).Split(' ').First();
            }
            else if (!(accession.IndexOf("TR") < 0))
            {
                index = accession.IndexOf("TR");
                allaccessions = accession.Substring(index + 3, accession.Length - (index + 3)).Split(' ').First();
            }
            else if (!(accession.IndexOf("GI") < 0))
            {
                index = accession.IndexOf("GI");
                allaccessions = accession.Substring(index + 3, accession.Length - (index + 3)).Split(' ').First();
            }
            //else
            //{
            //    allaccessions = accession.Split(' ')[0];
            //    index = 0;
            //}
            //else if (!(accession.IndexOf("RS") > 0)) ///If no GI in the accession then use RS
            //{
            //    index = accession.IndexOf("RS");
            //    allaccessions = accession.Substring(index + 3, accession.Length - (index + 3)).Split(' ').First();
            //}

            //if (index < 0)  ///If none then parse to find all the other accessions that might be in the value
            //{
            //    string[] splitstring = { ":", " " };
            //    List<string> accessions = accession.Split(splitstring, StringSplitOptions.RemoveEmptyEntries).ToList();
            //    bool trueorfalse = false;
            //    allaccessions = accessions[1];
            //    foreach (string acc in accessions.Skip(2))
            //    {
            //        if (trueorfalse)
            //        {
            //            allaccessions += " or " + acc;
            //        }
            //        trueorfalse = !trueorfalse;
            //    }
            //}

            return new Accessions { Accession = allaccessions, Indexforuni = index };

            //return allaccessions;
        }

        /// <summary>
        /// Copy the accession to clipboard
        /// </summary>
        public static void CopyAccessionToClipBoard(string accession)
        {
            string allaccessions = FindAccesion(accession).Accession;
            Clipboard.SetText(allaccessions);
        }

        /// <summary>
        /// Open a link for the searched Accession in the UniProt database
        /// </summary>
        public static void FindAccessioninUniProt(string accession)
        {
            if (string.IsNullOrWhiteSpace(accession)) return;

            var allaccessions = FindAccessions.FindAccesion(accession);
            if (allaccessions.Indexforuni < 0)
            {
                return;/// System.Diagnostics.Process.Start(allaccessions.Accession);
            }
            else
            {
                System.Diagnostics.Process.Start("http://www.uniprot.org/uniprot/?query=" + allaccessions.Accession + "&sort=score");
            }
        }

        public static string Returnaccession(string accession)
        {
            return FindAccesion(accession).Accession;
        }

        /// <summary>
        /// Return hyperlinks based on the accesions for the uniprot database
        /// </summary>
        /// <param name="accession"></param>
        /// <returns></returns>
        public static Accessions ReturnLinksforFindingAccessioninUniProt(string accession)
        {
            if (string.IsNullOrWhiteSpace(accession)) return new Accessions();


            var allaccessions = FindAccessions.FindAccesion(accession);

            //if (allaccessions.Indexforuni < 0)
            //{
                return allaccessions;
            //}
            //else
            //{
            //    return ("http://www.uniprot.org/uniprot/?query=" + allaccessions.Accession + "&sort=score");
            //}
        }

        public class Accessions
        {
            public string Accession { get; set; }
            public int Indexforuni { get; set; }
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class ExportToCSV
    {
        public static void ExportTOCSV(List<String> Headers, List<List<String>> Values)
        {
            if (!Directory.Exists(@"C:\TEMP\"))
            {
                Directory.CreateDirectory(@"C:\temp\");
            }
            if (!Directory.Exists(@"C:\temp\Exported CSV Files\"))
            {
                Directory.CreateDirectory(@"C:\temp\Exported CSV Files\");
            }
            string csvPath = @"C:\temp\Exported CSV Files\";
            //string csvFileName = csvPath + DateTime.Now.ToString("yyyy-MM-dd-hh.mm.ss.ffffff") + ".csv";
            ListViewToCSV(Headers, Values, csvPath, false);
        }


        public static void ListViewToCSV(List<string> Headers ,List<List<String>> values, string csvPath, bool includeHidden)
        {
            //make header string
            StringBuilder result = new StringBuilder();
            WriteCSVRow(result,  Headers);
            string csvFileName = csvPath + DateTime.Now.ToString("yyyy-MM-dd-hh.mm.ss.ffffff") + ".csv";
            //export data rows
            foreach (var item in values)
            {
                WriteCSVRow(result, item);
            }

            File.WriteAllText(csvFileName, result.ToString());

            System.Diagnostics.Process.Start(csvFileName);
        }

        /// <summary>
        /// Write the values to a CSV file format using a StringBuilder
        /// </summary>
        /// <param name="result"></param>
        /// <param name="column"></param>
        private static void WriteCSVRow(StringBuilder result, List<string> column)
        {
            bool isFirstTime = true;
            foreach (var item in column)
            {
                if (!isFirstTime)
                    result.Append(",");
                isFirstTime = false;
                result.Append(string.Format("\"{0}\"", item));
            }
            result.AppendLine();
        }

    }
}

using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Excel = Microsoft.Office.Interop.Excel;
//using Excel = OfficeOpenXml;

namespace MSViewer.Classes
{
    public class ExportToExcel
    {
        /// <summary>
        /// Exports all the elements to an excel document
        /// </summary>
        /// <param name="Headers"></param>
        /// <param name="AlltheColumns"></param>
        public static void ExportToexcel(List<string> Headers, List<List<string>> AlltheColumns, bool generateBandYIons = false, List<CalculateBYCZIons.BYCZIons> bandyions = null, string currenttext = null, bool createstyle = false)
        {
            return;

            /// http://csharp.net-informations.com/excel/csharp-create-excel.htm
            /// http://www.codeproject.com/Articles/20228/Using-C-to-Create-an-Excel-Document
            //try
            //{
            //    Excel.Application xlApp = new Excel.Application();
            //    if (xlApp == null)
            //    {
            //        return;
            //    }

            //    Excel.Workbook xlWorkBook;
            //    Excel.Worksheet xlWorkSheet;
            //    object misValue = System.Reflection.Missing.Value;
            //    xlWorkBook = xlApp.Workbooks.Add(misValue);
            //    xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                
            //    if (generateBandYIons)
            //    {
            //        CreateWorkSheet(Headers, bandyions, xlWorkSheet, currenttext);
            //    }
            //    else
            //    {
            //        int[] MaxStringlength = new int[AlltheColumns.Select(a => a.Count).First()]; //Find the maximum length of each column to set the column width in the excel document.

            //        for (int i = 0; i < AlltheColumns.Count; i++)
            //        {
            //            for (int j = 0; j < AlltheColumns.Select(a => a.Count).First(); j++)
            //            {
            //                MaxStringlength[j] = AlltheColumns[i][j] != null ? (Math.Max(AlltheColumns[i][j].Length, MaxStringlength[j])) : 0;
            //            }
            //        }

            //        for (int i = 0; i < Headers.Count; i++)
            //        {
            //            MaxStringlength[i] = Math.Max(MaxStringlength[i], Headers[i].Length);
            //        }
            //        if (createstyle)
            //        {
            //            xlWorkSheet = CreateWorkSheet(Headers, AlltheColumns, xlWorkSheet, MaxStringlength, true);
            //        }
            //        else
            //        {
            //            xlWorkSheet = CreateWorkSheet(Headers, AlltheColumns, xlWorkSheet, MaxStringlength);
            //        }
            //    }

            //    xlApp.Visible = true;
            //    releaseObject(xlWorkSheet);
            //    releaseObject(xlWorkBook);
            //    releaseObject(xlApp);
            //}
            //catch (Exception ex)
            //{
            //    string ex1 = ex.Message.ToString();
            //    int ex1i = ex1.Length;
            //    double aa = ex1i + 10;
            //}
        }

        /// <summary>
        /// Create the WorkSheet based on the Headers and Columns given
        /// </summary>
        /// <param name="Headers"></param>
        /// <param name="Columns"></param>
        /// <param name="WorkSheet"></param>
        /// <returns></returns>
        static ExcelWorksheet CreateWorkSheet(List<string> Headers, List<List<string>> Columns, ExcelWorksheet workSheet, int[] ColumnWidths, bool createstyle = false)
        {


            //Excel.Worksheet xlWorkSheet = new Excel.Worksheet();
            for (int i = 0; i < Headers.Count; i++)
            {
                workSheet.SetValue(1, i + 1, Headers[i]);
            }
            if (!createstyle)
            {
                var xlWorkSheet_Range = CreateHeaderStyle(Headers.Count, workSheet);
            }
            else
            {
                var xlWorkSheet_Range = CreateHeaderStyleforMgrSpectra(Headers.Count, workSheet);
            }
            int numberofcolumns = Columns.Select(a => a.Count).First();

            for (int i = 0; i < Columns.Count; i++)
            {
                for (int j = 0; j < numberofcolumns; j++)
                {
                    workSheet.SetValue(i + 2, j + 1, Columns[i][j]);
                }
            }
            if (!createstyle)
            {
                var ColumnxlWorkSheet_Range = CreateColumnsStyle(Columns.Count, numberofcolumns, workSheet, ColumnWidths);
            }
            return workSheet;
        }

        /// <summary>
        /// Styles all the Header elements
        /// </summary>
        /// <param name="Headers">All the header names</param>
        /// <param name="xlWorkSheet"></param>
        /// <returns></returns>
        static ExcelRange CreateHeaderStyleforMgrSpectra(int HeadersCount, ExcelWorksheet xlWorkSheet)
        {
            return null;

            char row = new char();

            //row = FindtheCurrentAlphabet(HeadersCount);
            //ExcelRange xlWorkSheet_range = xlWorkSheet.get_Range("A1", row + "1");
            //xlWorkSheet_range.Font.Bold = true;
            //xlWorkSheet_range.Font.Size = 13;
            //xlWorkSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //xlWorkSheet_range.ColumnWidth = 20;
            //xlWorkSheet_range.Interior.Color = System.Drawing.Color.Honeydew.ToArgb();
            //return xlWorkSheet_range;
        }

        /// <summary>
        /// Create WorkSheet for bandyions 
        /// </summary>
        /// <param name="Headers"></param>
        /// <param name="Columns"></param>
        /// <param name="WorkSheet"></param>
        /// <returns></returns>
        static ExcelWorksheet CreateWorkSheet(List<String> Headers, List<MSViewer.Classes.CalculateBYCZIons.BYCZIons> Columns, ExcelWorksheet WorkSheet, string currenttext)
        {
            return null;

            //for (int i = 0; i < Headers.Count; i++)
            //{
            //    WorkSheet.Cells[1, i + 1] = Headers[i];
            //}
            //var xlWorkSheet_Range = CreateHeaderStyle(Headers.Count, WorkSheet);
            //WorkSheet.Cells[2, 10] = currenttext;
            //for (int i = 0; i < Columns.Count; i++)
            //{
            //    WorkSheet.Cells[i + 2, 1] = Columns[i].bh2oion;
            //    WorkSheet.Cells[i + 2, 2] = Columns[i].Bion;
            //    WorkSheet.Cells[i + 2, 3] = Columns[i].BionNumber;
            //    WorkSheet.Cells[i + 2, 4] = Columns[i].AminoAcid;
            //    WorkSheet.Cells[i + 2, 5] = Math.Abs(Columns[i].YionNumber);
            //    WorkSheet.Cells[i + 2, 6] = Columns[i].Yion;
            //    WorkSheet.Cells[i + 2, 7] = Columns[i].yh2oion;
            //    WorkSheet.Cells[i + 2, 8] = Columns[i].ynh3ion;
            //}
            //string currentfilepath = App.AssemblyLocation; /// FindCurrentAssemblyPath.GetAssemblyPath();
            //WorkSheet.Shapes.AddPicture(currentfilepath + "\\" + "ErrorPlot.jpeg", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 440, 40, 900, 300);
            //WorkSheet.Shapes.AddPicture(currentfilepath + "\\" + "Spectrum.jpeg", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 440, 340, 1200, 300);

            ////WorkSheet.PasteSpecial()

            //var ColumnsxlWorkSheet_Range = CreateColumnsStyleforBandYIons(Columns.Count, Columns, WorkSheet);
            //return WorkSheet;
        }

        /// <summary>
        /// Create Rowstyle based on BandYIons
        /// </summary>
        /// <param name="NumberofRows"></param>
        /// <param name="NumberofColumns"></param>
        /// <param name="xlWorkSheet"></param>
        /// <returns></returns>
        static List<ExcelRange> CreateColumnsStyleforBandYIons(int NumberofRows, List<CalculateBYCZIons.BYCZIons> Columns, ExcelWorksheet xlWorkSheet)
        {
            return new List<ExcelRange>();

            //List<Microsoft.Office.Interop.Excel.Range> xlWorkSheet_rangelist = new List<Excel.Range>();
            //bool color = true;
            //for (int i = 0; i < NumberofRows; i++)
            //{
            //    //xlWorkSheet.Shapes.AddPicture
            //    //int h = 0;
            //    Microsoft.Office.Interop.Excel.Range xlWorkSheet_range1 = xlWorkSheet.get_Range('A' + Convert.ToString(i + 2));
            //    xlWorkSheet_range1.Font.Size = 11;
            //    xlWorkSheet_range1.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //    if (Columns[i].bh2oioncolor)
            //    {
            //        xlWorkSheet_range1.Interior.Color = System.Drawing.Color.Tomato.ToArgb();
            //    }
            //    xlWorkSheet_range1.ColumnWidth = 10.29;
            //    xlWorkSheet_rangelist.Add(xlWorkSheet_range1);

            //    Microsoft.Office.Interop.Excel.Range xlWorkSheet_range2 = xlWorkSheet.get_Range('B' + Convert.ToString(i + 2));
            //    xlWorkSheet_range2.Font.Size = 11;
            //    xlWorkSheet_range2.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //    if (Columns[i].bioncolor)
            //    {
            //        xlWorkSheet_range2.Interior.Color = System.Drawing.Color.Blue.ToArgb();
            //    }
            //    xlWorkSheet_range2.ColumnWidth = 9.29;
            //    xlWorkSheet_rangelist.Add(xlWorkSheet_range2);

            //    Microsoft.Office.Interop.Excel.Range xlWorkSheet_range3 = xlWorkSheet.get_Range('C' + Convert.ToString(i + 2));
            //    xlWorkSheet_range3.Font.Size = 11;
            //    xlWorkSheet_range3.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //    xlWorkSheet_range3.ColumnWidth = 6.86;
            //    xlWorkSheet_rangelist.Add(xlWorkSheet_range3);

            //    Microsoft.Office.Interop.Excel.Range xlWorkSheet_range4 = xlWorkSheet.get_Range('D' + Convert.ToString(i + 2));
            //    xlWorkSheet_range4.Font.Size = 11;
            //    xlWorkSheet_range4.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //    xlWorkSheet_range4.ColumnWidth = 12;
            //    xlWorkSheet_rangelist.Add(xlWorkSheet_range4);


            //    Microsoft.Office.Interop.Excel.Range xlWorkSheet_range5 = xlWorkSheet.get_Range('E' + Convert.ToString(i + 2));
            //    xlWorkSheet_range5.Font.Size = 11;
            //    xlWorkSheet_range5.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //    xlWorkSheet_range5.ColumnWidth = 6.14;
            //    xlWorkSheet_rangelist.Add(xlWorkSheet_range5);


            //    Microsoft.Office.Interop.Excel.Range xlWorkSheet_range6 = xlWorkSheet.get_Range('F' + Convert.ToString(i + 2));
            //    xlWorkSheet_range6.Font.Size = 11;
            //    xlWorkSheet_range6.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //    if (Columns[i].yioncolor)
            //    {
            //        xlWorkSheet_range6.Interior.Color = System.Drawing.Color.Green.ToArgb();
            //    }
            //    xlWorkSheet_range6.ColumnWidth = 9.29;
            //    xlWorkSheet_rangelist.Add(xlWorkSheet_range6);


            //    Microsoft.Office.Interop.Excel.Range xlWorkSheet_range7 = xlWorkSheet.get_Range('G' + Convert.ToString(i + 2));
            //    xlWorkSheet_range7.Font.Size = 11;
            //    xlWorkSheet_range7.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //    if (Columns[i].yh2oioncolor)
            //    {
            //        xlWorkSheet_range7.Interior.Color = System.Drawing.Color.Tomato.ToArgb();
            //    }
            //    xlWorkSheet_range7.ColumnWidth = 9.29;
            //    xlWorkSheet_rangelist.Add(xlWorkSheet_range7);


            //    Microsoft.Office.Interop.Excel.Range xlWorkSheet_range8 = xlWorkSheet.get_Range('H' + Convert.ToString(i + 2));
            //    xlWorkSheet_range8.Font.Size = 11;
            //    xlWorkSheet_range8.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //    if (Columns[i].ynh3ioncolor)
            //    {
            //        xlWorkSheet_range8.Interior.Color = System.Drawing.Color.LightGreen.ToArgb();
            //    }
            //    xlWorkSheet_range8.ColumnWidth = 9.29;
            //    xlWorkSheet_rangelist.Add(xlWorkSheet_range8);
            //}
            //return xlWorkSheet_rangelist;
        }

        /// <summary>
        /// Styles all the Header elements
        /// </summary>
        /// <param name="Headers">All the header names</param>
        /// <param name="xlWorkSheet"></param>
        /// <returns></returns>
        static ExcelRange CreateHeaderStyle(int HeadersCount, ExcelWorksheet xlWorkSheet)
        {
            return null;

            //char row = new char();

            //row = FindtheCurrentAlphabet(HeadersCount);
            //ExcelRange xlWorkSheet_range = xlWorkSheet.get_Range("A1", row + "1");
            //xlWorkSheet_range.Font.Bold = true;
            //xlWorkSheet_range.Font.Size = 13;
            //xlWorkSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //xlWorkSheet_range.Interior.Color = System.Drawing.Color.Honeydew.ToArgb();
            //return xlWorkSheet_range;
        }

        /// <summary>
        /// Styles all the Columns for the excel body elements
        /// </summary>
        /// <param name="AlltheColumns"></param>
        /// <param name="xlWorkSheet"></param>
        /// <returns></returns>
        static List<ExcelRange> CreateColumnsStyle(int NumberofRows, int NumberofColumns, ExcelWorksheet xlWorkSheet, int[] ColumnWidths)
        {
            return null;

            //char row = new char();
            //row = FindtheCurrentAlphabet(NumberofColumns);
            //List<Microsoft.Office.Interop.Excel.Range> xlWorkSheet_rangelist = new List<Excel.Range>();
            //bool color = true;
            //for (int i = 0; i < NumberofRows; i++)
            //{
            //    int h = 0;
            //    for (char j = 'A'; j <= row; j++)
            //    {
            //        Microsoft.Office.Interop.Excel.Range xlWorkSheet_range = xlWorkSheet.get_Range(j + Convert.ToString(i + 2));
            //        xlWorkSheet_range.Font.Size = 11;
            //        xlWorkSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //        xlWorkSheet_range.ColumnWidth = Math.Min(ColumnWidths[h] * 1.16, 120);
            //        if (color) ///For creating an alternating LightGray and White background color for readability
            //        {
            //            xlWorkSheet_range.Interior.Color = System.Drawing.Color.LightGray.ToArgb();
            //        }
            //        xlWorkSheet_rangelist.Add(xlWorkSheet_range);
            //        h = h + 1;
            //    }
            //    color = !color;
            //}
            //return xlWorkSheet_rangelist;
        }

        /// <summary>
        /// Finds the current alphabet based on the Number
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        static char FindtheCurrentAlphabet(int count)
        {
            char currentalphabet = new char();
            int m = 0;
            ///Finding the correct column based on the number of Headers in the String
            for (char i = 'A'; i <= 'Z'; i++)
            {
                currentalphabet = i;
                m = m + 1;
                if (m == count) break;
            }
            return currentalphabet;
        }

        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                return;
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}

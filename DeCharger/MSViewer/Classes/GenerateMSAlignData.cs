using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MassSpectrometry;
using Microsoft.Win32;

namespace MSViewer.Classes
{
    public class GenerateMSAlignData
    {
        public static void GenerateFileforMSAlignData(IMSProvider MainPointProvider, string FileName)
        {
            SaveFileDialog Savelocation = new SaveFileDialog();
            Savelocation.FileName = FileName.Remove(FileName.Length - 4, 4);
            Savelocation.DefaultExt = ".msalign";
            Savelocation.Filter = "MSAlign (.msalign)|*.msalign";
            bool? result = Savelocation.ShowDialog();
            if (result == true)
            {
                System.IO.File.WriteAllText(Savelocation.FileName, MainPointProvider.ToMSAlignString()); 
            }

            //System.IO.File.WriteAllText("", MainPointProvider.ToMSAlignString());
            //MainPointProvider.ToMSAlignString();
        }
    }
}

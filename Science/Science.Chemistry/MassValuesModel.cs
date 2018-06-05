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
using System.Net;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using Ionic.Zip;
using System.Xml;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;

namespace Science.Chemistry
{
    public class MassValuesModel
    {
        // based on: http://www.yoda.arachsys.com/csharp/singleton.html
        static readonly MassValuesModel instance = new MassValuesModel();
 
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static MassValuesModel()
        {
            
        }

        /// <summary>
        /// Reads information from an embedded resource.
        /// In VS, set the type of the file in solution explorer to "Embedded Resource"
        /// <example>
        /// bytes = ReadBytesFromStream("MyTestProgram.SomeDataFile.xml")
        /// </example>
        /// </summary>
        /// <param name="streamName"></param>
        /// <returns></returns>
        private byte[] ReadBytesFromStream(string streamName)
        {
            using (System.IO.Stream stream = this.GetType().Assembly.GetManifestResourceStream(streamName))
            {
                byte[] result = new byte[stream.Length];
                stream.Read(result, 0, (int)stream.Length);
                return result;
            }
        }

        MassValuesModel()
        {
            #region Serialization test
            //// Serialize to XML
            //AminoAcids = new List<AminoAcid>();

            //AminoAcids.Add(new AminoAcid() { Abbreviation = "X", Name = "Test NAme", Symbol = "AAA", Composition = "C6H2O2" });

            //var m2 = new MemoryStream();
            
            //XMLUtility<List<AminoAcid>>.Write(AminoAcids.ToList(), m2);

            //StreamReader reader = new StreamReader(m2);
            //string text = reader.ReadToEnd();
            //System.Diagnostics.Debug.WriteLine(text);

            //m2.Position = 0;

            ////Deserialize from XML stream
            //AminoAcids = new List<AminoAcid>(XMLUtility<List<AminoAcid>>.Read(m2));
            #endregion

            // Based on: http://www.silverlightexamples.net/post/How-to-Get-Files-From-Resources-in-Silverlight-20.aspx

            StreamResourceInfo sr = Application.GetResourceStream(new Uri("Science.Chemistry;component/MassValues.zip", UriKind.Relative));
            var x = sr.Stream;


            //StreamResourceInfo sr2 = Application.GetResourceStream(new Uri("MassValues.zip", UriKind.Relative));
            //var y = sr2.Stream;


            //var b = ReadBytesFromStream("Resources/MassValues.zip");

            //var r = Application.Current.Resources["MassValues.zip"];
            //var a = Assembly.GetExecutingAssembly();

            //var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Resources/MassValues.zip");


            //using (var zip1 = ZipFile.Read(Application.GetResourceStream(new Uri("MassValues.zip", UriKind.Relative)).Stream))
            using (var zip1 = ZipFile.Read(x))
            {
                if (zip1.Any(f => f.FileName == "AminoAcids.xml")) 
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        zip1["AminoAcids.xml"].Extract(m);
                        AminoAcids = new List<AminoAcid>(XMLUtility<List<AminoAcid>>.Read(m));
                    }
                }
                else
                {
                    AminoAcids = new List<AminoAcid>();
                }

                if (zip1.Any(f => f.FileName == "AtomIsotopes.xml"))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        zip1["AtomIsotopes.xml"].Extract(m);
                        AtomIsotopes = new List<AtomIsotope>(XMLUtility<List<AtomIsotope>>.Read(m));
                    }
 
                }
                else
                {
                    AtomIsotopes = new List<AtomIsotope>();
                }

                if (zip1.Any(f => f.FileName == "FragmentTypes.xml"))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        zip1["FragmentTypes.xml"].Extract(m);
                        FragmentTypes = new List<FragmentType>(XMLUtility<List<FragmentType>>.Read(m));
                    }
                }
                else
                {
                    FragmentTypes = new List<FragmentType>();
                }

                if (zip1.Any(f => f.FileName == "InstrumentFragmentXRefs.xml"))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        zip1["InstrumentFragmentXRefs.xml"].Extract(m);
                        InstrumentFragmentXRefs = new List<InstrumentFragmentXRef>(XMLUtility<List<InstrumentFragmentXRef>>.Read(m));
                    }
                }
                else
                {
                    InstrumentFragmentXRefs = new List<InstrumentFragmentXRef>();
                }


                if (zip1.Any(f => f.FileName == "Instruments.xml"))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        zip1["Instruments.xml"].Extract(m);
                        Instruments = new List<Instrument>(XMLUtility<List<Instrument>>.Read(m));
                    }
                }
                else
                {
                    Instruments = new List<Instrument>();
                }
            }
 
        }
 
        public static MassValuesModel Instance
        {
            get
            {
                return instance;
            }
        }

        public List<AminoAcid> AminoAcids = new List<AminoAcid>();
        public List<AtomIsotope> AtomIsotopes = new List<AtomIsotope>();
        public List<FragmentType> FragmentTypes = new List<FragmentType>();
        public List<InstrumentFragmentXRef> InstrumentFragmentXRefs = new List<InstrumentFragmentXRef>();
        public List<Instrument> Instruments = new List<Instrument>();
 
        //public bool SaveChanges()
        //{   
        //    using (ZipFile zip = new ZipFile())
        //    {
        //        using (var m = new MemoryStream())
        //        {
        //            XMLUtility<List<AminoAcid>>.Write(AminoAcids.ToList(), m);
        //            zip.AddEntry("AminoAcids.xml", m);
        //            zip.Save(Path.Combine(App.DataPath, "MassValuesModel.zip"));
        //        }
 
        //        using (var m = new MemoryStream())
        //        {
        //            XMLUtility<List<AtomIsotope>>.Write(AtomIsotopes.ToList(), m);
        //            zip.AddEntry("AtomIsotopes.xml", m);
        //            zip.Save(Path.Combine(App.DataPath, "MassValuesModel.zip"));
        //        }
 
        //        using (var m = new MemoryStream())
        //        {
        //            XMLUtility<List<FragmentType>>.Write(FragmentTypees.ToList(), m);
        //            zip.AddEntry("FragmentTypees.xml", m);
        //            zip.Save(Path.Combine(App.DataPath, "MassValuesModel.zip"));
        //        }
 
        //        using (var m = new MemoryStream())
        //        {
        //            XMLUtility<List<InstrumentFragmentXRef>>.Write(InstrumentFragmentXRefs.ToList(), m);
        //            zip.AddEntry("InstrumentFragmentXRefs.xml", m);
        //            zip.Save(Path.Combine(App.DataPath, "MassValuesModel.zip"));
        //        }
 
        //        using (var m = new MemoryStream())
        //        {
        //            XMLUtility<List<Instrument>>.Write(Instruments.ToList(), m);
        //            zip.AddEntry("Instruments.xml", m);
        //            zip.Save(Path.Combine(App.DataPath, "MassValuesModel.zip"));
        //        }
 
        //    }
 
        //    return true;
        //}
    }
 
    //[Serializable]
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class AminoAcid
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Abbreviation { get; set; }
        public string Composition { get; set; }
    }
 
 
    //[Serializable]
    public class AtomIsotope
    {
        public string Symbol { get; set; }
        public string IsoSymbol { get; set; }
        public string Name { get; set; }
        public double Mass { get; set; }
        public double Abundance { get; set; }
    }


    public class FragmentType
    {
        public int FragmentTypeID { get; set; }
        public string FragmentClass { get; set; }
        public string Label { get; set; }
        public string Composition { get; set; }
        public string MatchExpression { get; set; }
        public string Terminus { get; set; }
        public IEnumerable<Instrument> Instruments
        {
            get
            {
                return from i in MassValuesModel.Instance.Instruments
                       join x in MassValuesModel.Instance.InstrumentFragmentXRefs on i.InstrumentID equals x.InstrumentID
                       where x.FragmentTypeID == this.FragmentTypeID
                       select i;
            }
        }
    }
 
    public partial class InstrumentFragmentXRef
    {
        public int InstrumentID { get; set; }
        public int FragmentTypeID { get; set; }
 
        //[XmlElement("Instrument")]
        //internal string _Instrument { get; set; }
 
        //[XmlIgnore]
        //public Instrument Instrument
        //{
        //    get
        //    {
        //        return MassValuesModel.Instance.Instruments.Where(o => o.ID == this._Instrument).FirstOrDefault();
        //    }
 
        //    set
        //    {
        //        _Instrument = value.ID;
        //    }
        //}
 
        //[XmlElement("Prefix")]
        //internal int _Prefix { get; set; }
 
        //[XmlIgnore]
        //public AminoAcid Prefix
        //{
        //    get
        //    {
        //        return MassValuesModel.Instance.AminoAcides.Where(o => o.ID == this._Prefix).FirstOrDefault();
        //    }
 
        //    set
        //    {
        //        _Prefix = value.ID;
        //    }
        //}
 
        //[XmlElement("Destination")]
        //internal byte? _Destination { get; set; }
 
        //[XmlIgnore]
        //public FragmentType Destination
        //{
        //    get
        //    {
        //        return MassValuesModel.Instance.FragmentTypes.Where(o => o.ID == this._Destination).FirstOrDefault();
        //    }
 
        //    set
        //    {
        //        _Destination = value.ID;
        //    }
        //}
 
        //[XmlElement("Source")]
        //internal byte? _Source { get; set; }
 
        //[XmlIgnore]
        //public FragmentType Source
        //{
        //    get
        //    {
        //        return MassValuesModel.Instance.FragmentTypees.Where(o => o.ID == this._Source).FirstOrDefault();
        //    }
 
        //    set
        //    {
        //        _Source = value.ID;
        //    }
        //}
 
    }
 
    //[Serializable]
    public class Instrument
    {
        public int InstrumentID { get; set; }
        public string InstrumentName { get; set; }

        public IEnumerable<FragmentType> FragmentTypes
        {
            get
            {
                return from f in MassValuesModel.Instance.FragmentTypes
                       join x in MassValuesModel.Instance.InstrumentFragmentXRefs on f.FragmentTypeID equals x.FragmentTypeID
                       where x.InstrumentID == this.InstrumentID
                       select f;
            }
        }
    }
 
    public sealed class XMLUtility<T>
    {
        // Based on http://www.codeproject.com/KB/XML/XMLSupport.aspx
        
        public static T Read(Stream theStream)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            theStream.Position = 0;
            return (T)xs.Deserialize(theStream);
        }
 
        public static void Write(T xmlObject, Stream theStream)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            xs.Serialize(theStream, xmlObject);
            theStream.Position = 0;
        }
    }
}

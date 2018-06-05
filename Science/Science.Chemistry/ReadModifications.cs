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
using System.Xml.Linq;

namespace Science.Chemistry
{
    public static class ReadModifications
    {
        public static List<Modifications> ReadMods()
        {
            List<Modifications> allmods = new List<Modifications>();

            try
            {

                XElement xelement = XElement.Load("unimod_mod.xml"); /// Loading the file

                IEnumerable<XElement> mods = xelement.Elements("modifications"); /// Get all the elements with modifications tag

                IEnumerable<XElement> mod = mods.Elements("mod"); // Get all the elements with mod tag within the modifications

                int i = 0;

                foreach (var md in mod) // Go through all the modifications 
                {
                    IEnumerable<XElement> specificity = md.Elements("specificity");

                    List<String> spefs = new List<String>();

                    foreach (var sp in specificity)
                    {
                        spefs.Add(sp.Attribute("site").Value);
                    }

                    allmods.Add(new Modifications()
                    {
                        InterimName = md.Attribute("title").Value,
                        Description = md.Attribute("full_name").Value,
                        Monomass = Convert.ToDouble(md.Element("delta").Attribute("mono_mass").Value),
                        Sites = spefs
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Message :" +ex.Message + "  Stack trace" + ex.StackTrace);
                
            }
            return allmods;
        }
    }


}

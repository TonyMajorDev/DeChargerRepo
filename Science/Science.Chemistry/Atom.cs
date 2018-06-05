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
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;

namespace Science.Chemistry
{

    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public struct Atom //: IEqualityComparer<Atom> //: IComparable<Atom>, IComparable
    {
        private SortedSet<Isotope> m_Isotopes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string m_symbol;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string m_Name;

        /// <summary>
        /// Construct atoms by passing in a symbol such as "C" or "O" or "13C"
        /// </summary>
        /// <param name="Symbol"></param>
        public Atom(string symbol)
        {
            m_Isotopes = new SortedSet<Isotope>();

            foreach (var anAtom in MassValuesModel.Instance.AtomIsotopes.Where(i => i.Symbol == symbol))
                m_Isotopes.Add(new Isotope(anAtom.Mass, anAtom.Abundance, anAtom.Name));

            if (m_Isotopes.Count == 0)
            {
                foreach (var anAtom in MassValuesModel.Instance.AtomIsotopes.Where(i => i.IsoSymbol == symbol))
                    m_Isotopes.Add(new Isotope(anAtom.Mass, 1, anAtom.Name));
            }

            System.Diagnostics.Debug.Assert(m_Isotopes.Count > 0, "Atom " + symbol + " could not be found in composition database.  ");

            this.m_symbol = symbol;
            this.m_Name = m_Isotopes[0].Name;  //Name Atom with the most abundant isotope
        }

        /// <summary>
        /// This is an adhoc atom to represent and unknown molecule.  It is an atom with a single isotope
        /// of the mass passed into the constructor.  
        /// </summary>
        /// <param name="mass">Monoisotopic mass of the adhoc atom</param>
        public Atom(double mass)
        {
            m_Isotopes = new SortedSet<Isotope>();

            m_Isotopes.Add(new Isotope(mass, 1, "Adhoc Atom - " + (int)mass));

            this.m_symbol = (int)mass + "X";

            this.m_Name = m_Isotopes[0].Name; //Name Atom with the most abundant isotope
        }

        public string Symbol
        {
            get { return m_symbol; }
        }

        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Get an Isotope Array ordered by decreasing abundance
        /// </summary>
        public Isotope[] Isotopes
        {
            get { return m_Isotopes.ToArray(); }
        }

        /// <summary>
        /// Get the Index of the specified isotope within the context of the internal isotope set
        /// </summary>
        /// <param name="anIsotope">The isotope for which the index is being saught</param>
        /// <returns>0 based index of the isotope specified</returns>
        public int GetIndexOfIsotope(Isotope anIsotope)
        {
            return m_Isotopes.IndexOf(anIsotope);
        }

        public struct Isotope : IComparable<Isotope>
        {
            private double m_Mass;
            private double m_Abundance;
            private string m_Name;

            public Isotope(double Mass, double Abundance, string Name)
            {
                m_Mass = Mass;
                m_Abundance = Abundance;
                m_Name = Name;
            }

            public double Mass
            {
                get { return m_Mass; }
            }

            public double Abundance
            {
                get { return m_Abundance; }
            }

            public string Name
            {
                get { return m_Name; }
            }

            #region IComparable<Isotope> Members

            /// <summary>
            /// Allows for sorting by decrease in abundance
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(Isotope other)
            {
                return other.m_Abundance.CompareTo(this.m_Abundance);
            }

            #endregion
        }

        public class AtomComparer : System.Collections.Generic.EqualityComparer<Atom>
        {
            public override bool Equals(Atom x, Atom y)
            {
                return x.Symbol == y.Symbol;
            }

            public override int GetHashCode(Atom obj)
            {
                return obj.Symbol.GetHashCode();
            }
        }
    }

    public class SortedSet<T> : ObservableCollection<T> where T : IComparable<T>
    {
        protected override void InsertItem(int index, T item)
        {
            for (int i = 0; i < this.Count; i++)
            {
                switch (Math.Sign(this[i].CompareTo(item)))
                {
                    case 0:
                        throw new InvalidOperationException("Cannot insert duplicated items");
                    case 1:
                        base.InsertItem(i, item);
                        return;
                    case -1:
                        break;
                }
            }
            base.InsertItem(this.Count, item);
        }
    }
}

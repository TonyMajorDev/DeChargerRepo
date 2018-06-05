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

namespace Science.Chemistry
{

    [System.Diagnostics.DebuggerDisplay("{Formula}")]
    public abstract class MoleculeBase : Dictionary<Atom, int>, IMolecule
    {        
        public MoleculeBase() : this(string.Empty) { }
        
        /// <summary>
        /// UNIMOD format elemental composition
        /// </summary>
        /// <param name="formula"></param>
        public MoleculeBase(string formula): base(new Atom.AtomComparer())
        {
            this.Formula = formula;
        }

        public string SimpleFormula
        {
            get
            {
                string formulaStr = string.Empty;
                foreach (KeyValuePair<Atom, int> anAtom in this)
                {
                    formulaStr += anAtom.Key.Symbol + anAtom.Value + " ";
                }

                return formulaStr.Trim();
            }
        }

        /// <summary>
        /// Elemental Composition represented as a string (such as "C(3) H(2) O N")
        /// </summary>
        public string Formula
        {
            get
            {
                string formulaStr = string.Empty;
                foreach (KeyValuePair<Atom, int> anAtom in this)
                {
                    formulaStr += anAtom.Key.Symbol + (anAtom.Value != 1 ? ("(" + anAtom.Value + ")") : string.Empty) + " ";
                }

                return formulaStr.Trim();
            }

            set
            {
                this.Clear();

                string currentAtomSymbol;
                int currentAtomCount;

                foreach (string anElement in value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    currentAtomSymbol = string.Empty;
                    currentAtomCount = 1;
                    
                    foreach (string elementPart in anElement.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (char.IsNumber(elementPart[elementPart.Length - 1]))
                            currentAtomCount = int.Parse(elementPart);
                        else
                            currentAtomSymbol = elementPart;
                    }

                    if (currentAtomSymbol.Contains("X")) throw new Exception("Molcules cannot be created by formulas that include adhoc atoms (X).  This is because Isotopes are unknown for this atom. ");
                    this.Add(new Atom(currentAtomSymbol), currentAtomCount);
                }
            }
        }


        /// <summary>
        /// Add a number of atoms to this molecule
        /// </summary>
        /// <param name="key">Which atom</param>
        /// <param name="value">number of atoms to add (note: a negative value will subtract atoms)</param>
        public new void Add(Atom key, int value)
        {
            if (this.ContainsKey(key))
                base[key] += value;
            else
                base.Add(key, value);

            if (base[key] == 0) base.Remove(key);  // cleanup atoms with 0 instances
        }

        public void Add(KeyValuePair<Atom, int> theAtom)
        {
            if (this.ContainsKey(theAtom.Key))
                base[theAtom.Key] += theAtom.Value;
            else
                base.Add(theAtom.Key, theAtom.Value);

            if (base[theAtom.Key] == 0) base.Remove(theAtom.Key);  // cleanup atoms with 0 instances        
        }


        #region IMolecule Members

        public virtual double MonoIsotopicMass
        {
            get 
            {
                double tempMass = 0;

                foreach (KeyValuePair<Atom, int> anAtom in this)
                    tempMass += anAtom.Key.Isotopes[0].Mass * anAtom.Value;

                return tempMass;
            }
        }

        public virtual double AverageMass
        {
            get
            {
                double tempMass = 0;

                foreach (KeyValuePair<Atom, int> anAtom in this)
                {
                    double atomMass = 0;
                    foreach (Atom.Isotope anIsotope in anAtom.Key.Isotopes)
                        atomMass += anIsotope.Mass * anIsotope.Abundance;

                    tempMass += atomMass * anAtom.Value;
                }

                return tempMass;
            }
        }

        #endregion


        #region IMolecule Members


        public IEnumerable<KeyValuePair<Atom, int>> ElementalComposition
        {
            get { return this as IEnumerable<KeyValuePair<Atom, int>>; }
        }

        #endregion

    }

    public class Molecule : MoleculeBase
    {
        //public static Molecule m_proton = new Molecule("H e(-1)");
        
        public Molecule(string formula) : base(formula) { }

        public Molecule() : base() { }

        public static Molecule operator +(Molecule A, IMolecule B)
        {
            Molecule temp = A.Clone();

            foreach (KeyValuePair<Atom, int> anAtom in B.ElementalComposition)
                temp.Add(anAtom.Key, anAtom.Value);

            return temp; // new Molecule(A.Formula + " " + B.Formula);
        }

        public static Molecule operator -(Molecule A, IMolecule B)
        {
            Molecule temp = A.Clone();

            foreach (KeyValuePair<Atom, int> anAtom in B.ElementalComposition)
                temp.Add(anAtom.Key, anAtom.Value * -1);

            return temp; // new Molecule(A.Formula + " " + B.Formula);
        }


        /// <summary>
        /// Create a new instance of this molecule with the same formula
        /// </summary>
        /// <returns>The Clone</returns>
        public Molecule Clone()
        {
            Molecule temp = new Molecule();
            
            foreach (KeyValuePair<Atom, int> anAtom in this)
                temp.Add(anAtom.Key, anAtom.Value);
            
            return temp;
        }
    }

    public interface IMolecule
    {
        double MonoIsotopicMass { get; }
        double AverageMass { get; }
        IEnumerable<KeyValuePair<Atom, int>> ElementalComposition { get; }
        string Formula { get; }
        string SimpleFormula { get; }
    }
}

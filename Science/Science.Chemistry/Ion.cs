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
    public interface IIon : IMolecule
    {
        double MonoIsotopicMassToCharge { get; }
        double AverageMassToCharge { get; }
        int Charge { get; }
    }

    public abstract class IonBase : Molecule, IIon
    {
        public IonBase() : this(string.Empty, 1) { }
        
        public IonBase(string formula, int charge)
            : base(formula)
        {
            this.Charge = charge;
        }
        
        public double MonoIsotopicMassToCharge 
        {
            get { return this.MonoIsotopicMass / Math.Abs(this.Charge); }
        }

        public double AverageMassToCharge
        {
            get { return this.AverageMass / Math.Abs(this.Charge); }
        }

        /// <summary>
        /// The Charge state of this Ion
        /// </summary>
        public int Charge
        {
            get
            {
                // The charge is dictated by the number of protons/electrons
                if (this.ContainsKey(new Atom("e")))
                    return this[new Atom("e")] * -1;
                else if (this.ContainsKey(new Atom("p")))
                    return this[new Atom("p")];
                else
                    throw new Exception("An Ion must have either free protons or free electrons. ");
            }
            
            set
            {
                if (this.ContainsKey(new Atom("e")))
                {
                    if (value > 0)
                    {
                        this.Remove(new Atom("e"));
                        this.Add(new Atom("p"), value);
                    }
                    else
                    {
                        this[new Atom("e")] = value * -1;
                    }
                }
                else if (this.ContainsKey(new Atom("p")))
                {
                    if (value < 0)
                    {
                        this.Remove(new Atom("p"));
                        this.Add(new Atom("e"), value * -1);
                    }
                    else
                    {
                        this[new Atom("p")] = value;
                    }
                }
                else if (value > 0)
                {
                    // Add a proton for each positive charge unit
                    this.Add(new Atom("p"), value);
                }
                else if (value < 0)
                {
                    this.Add(new Atom("e"), value * -1);
                }
            }
        }
    }

    public class Ion : IonBase
    {
        private Ion() {}

        public Ion(string formula, int charge) : base(formula, charge) { }
    }
}

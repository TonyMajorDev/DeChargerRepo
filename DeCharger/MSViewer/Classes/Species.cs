using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class Species
    {
        private string specie;
        private bool isselected;
        private int specieid;


        public bool DefaultSpecies
        {
            get;
            set;
        }

        public int SpeciesID
        {
            get
            {
                return specieid;
            }
            set
            {
                specieid = value;
            }
        }


        public string Genus
        {
            get
            {
                return specie;
            }
            set
            {
                specie = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return isselected;
            }
            set
            {
                isselected = value;
            }
        }
    }
}

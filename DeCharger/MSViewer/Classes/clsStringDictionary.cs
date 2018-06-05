using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class clsStringDictionary
    {

        public static string Serialize(StringDictionary data)
        {
            if (data == null) return null; 
            DbConnectionStringBuilder db = new DbConnectionStringBuilder();
            foreach (string key in data.Keys)
            {
                db[key] = data[key];
            }
            return db.ConnectionString;
        }

        public static StringDictionary Deserialize(string data)
        {
            if (data == null) return null;
            DbConnectionStringBuilder db = new DbConnectionStringBuilder();
            StringDictionary lookup = new StringDictionary();
            db.ConnectionString = data;
            foreach (string key in db.Keys)
            {
                lookup[key] = Convert.ToString(db[key]);
            }
            return lookup;
        }
    }
}

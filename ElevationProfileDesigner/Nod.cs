using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

namespace Gxt.ElevationProfileDesigner
{
    
    public class Nod
    {
        
        public DBDictionary NodDictionary { get; set; }

        public Nod()
        {

            //NodDictionary = (DBDictionary)trans.GetObject(Db.NamedObjectsDictionaryId, OpenMode.ForRead);
            
        }

        public void DeleteFromNod()
        {
            throw new NotImplementedException();
        }

        public void WriteToNod()
        {
            throw new NotImplementedException();
        }

        public void ReadFromNod()
        {
            throw new NotImplementedException();
        }

        public void ClearNod()
        {
            throw new NotImplementedException();
        }

        public int NodRecordCount()
        {
            return 0;
        }


    }
}

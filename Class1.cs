using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Gxt.ElevationProfileDesigner;

namespace Gxt
{
    public class Class1
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database database = Application.DocumentManager.MdiActiveDocument.Database;

        [CommandMethod("EPD")]
        public void EPD()
        {
            ProfileGrid profileGrid = new ProfileGrid(520, 5);

            Transaction trans = database.TransactionManager.StartTransaction();

            using (trans)
            {
                BlockTable bt = trans.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                foreach (Entity obj in profileGrid.ProfileGridDBOjbects)
                {
                    btr.AppendEntity(obj);
                    trans.AddNewlyCreatedDBObject(obj, true);
                }

                trans.Commit();
            }
        }
         
    }
}

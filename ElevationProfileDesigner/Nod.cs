using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace Gxt.ElevationProfileDesigner
{
    
    public class Nod
    {
        public DBDictionary AppDictionary { get; set; }
        public int Count { get; set; }

        const string APPDICTIONARYNAME = "profileDesigner";
       
        public Nod()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
            using (trans)
            {
                try
                {
                    AppDictionary = new DBDictionary();
                    DBDictionary NOD = (DBDictionary)trans.GetObject(Application.DocumentManager.MdiActiveDocument.Database.NamedObjectsDictionaryId,
                                                                        OpenMode.ForRead);
                    //try to add new dictonary to the NOD if it fails then create a new one
                    try
                    {
                        Object appDictionaryId = NOD.GetAt(APPDICTIONARYNAME);
                        
                        //no error so create record already exist
                        ed.WriteMessage(APPDICTIONARYNAME + " Already Exists");
                    }
                    catch
                    {
                        NOD.UpgradeOpen();

                        //insert new dictionary in NOD
                        NOD.SetAt(APPDICTIONARYNAME, AppDictionary);
                        trans.AddNewlyCreatedDBObject(AppDictionary, true);
                    }
                    trans.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Error while opening Nod: " + e.Message);
                }
            }
        }


        public void WriteToNod(ProfileObject obj)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

            using (trans)
            {
                try
                {
                    // check to see if our entry is in there, excpetion will be thrown if not so process that
                    // condition in the catch
                    ObjectId entryId = AppDictionary.GetAt(obj.NodKey);

                    //no error so create record already exist
                    ed.WriteMessage(obj.NodKey + "Already Exists");

                }
                catch
                {
                    //no record exist therefore open for write
                    AppDictionary.UpgradeOpen();

                    //create new Xrecord
                    Xrecord xrecord = new Xrecord();
                    //create the resbuf list
                    ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Text, obj.NodKey),
                                                         new TypedValue((int)DxfCode.Int64, obj.DistanceAtCrossing),
                                                         new TypedValue((int)DxfCode.Text, obj.Layer),
                                                         new TypedValue((int)DxfCode.Text, obj.LineType),
                                                         new TypedValue((int)DxfCode.Int32, obj.Depth));
                    //add data to new record
                    xrecord.Data = data;

                    //create the entry to nod
                    AppDictionary.SetAt(obj.NodKey, xrecord);

                    //add to transaction
                    trans.AddNewlyCreatedDBObject(xrecord, true);
                }
                //all ok then commit
                trans.Commit();

                //increment object counter
                this.Count++;
            }
        }

        public List<ProfileObject> ReadFromNod(ProfileObject obj)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

            using (trans)
            {
                try
                {
                    // check to see if our entry is in there, excpetion will be thrown if not so process that
                    // condition in the catch
                    ObjectId entryId = AppDictionary.GetAt(obj.NodKey);

                    //create xrecord
                    Xrecord xrecord = default(Xrecord);

                    //read it from the AppDirectory
                    xrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                    //get the data from xrecord
                    ResultBuffer resbuf = xrecord.Data;
                    TypedValue[] resbufvalue = resbuf.AsArray();

                    ed.WriteMessage(string.Format("\n{0}, {1}, {2}", resbufvalue[0].Value, resbufvalue[1].Value, resbufvalue[2].Value));
                }
                catch
                {
                    ed.WriteMessage(obj.NodKey + "Does not Exists App Dictionary.");
                }
            }
        }

        public void DeleteFromNod()
        {
            throw new NotImplementedException();
        }

        public void ClearNod()
        {
            throw new NotImplementedException();
        }
    }
}

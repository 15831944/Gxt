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
        public static DBDictionary AppDictionary { get; set; }
        public static int Count { get; set; }

        private  const string APPDICTIONARYNAME = "profileDesigner";
       
        public static void CreateAppNod()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
            using (trans)
            {
                

                try
                {
                    
                    DBDictionary NOD = (DBDictionary)trans.GetObject(Application.DocumentManager.MdiActiveDocument.Database.NamedObjectsDictionaryId,
                                                                        OpenMode.ForRead);
                    //try to add new dictonary to the NOD if it fails then create a new one
                    try
                    {
                        ObjectId appDictionaryId = NOD.GetAt(APPDICTIONARYNAME);

                        //no error so dictionary already exist
                        AppDictionary = (DBDictionary)trans.GetObject(appDictionaryId, OpenMode.ForRead);
                        ed.WriteMessage(APPDICTIONARYNAME + " Already Exists");
                    }
                    catch
                    {
                        NOD.UpgradeOpen();
                        AppDictionary = new DBDictionary();
                        
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


        public static void WriteToNod(ProfileObject obj)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

            using (trans)
            {
                
                if (!AppDictionary.IsWriteEnabled)
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

                try
                {
                    //add to transaction, if exist then handle in catch
                    trans.AddNewlyCreatedDBObject(xrecord, true);

                }
                catch
                {
                    //what todo when xrecord already exists
                }
                
                //all ok then commit
                trans.Commit();

                //increment object counter
                Count++;
            }
        }

        public static List<ProfileObject> ReadFromNod()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

            List<ProfileObject> profileObjects = new List<ProfileObject>();

            using (trans)
            {
                try
                {
                    foreach (DBDictionaryEntry entry in AppDictionary)
                    {
                        // check to see if our entry is in there, excpetion will be thrown if not so process that
                        // condition in the catch
                        ObjectId entryId = entry.m_value;
                        //create xrecord
                        Xrecord xrecord = default(Xrecord);

                        //read it from the AppDirectory
                        xrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                        //get the data from xrecord
                        ResultBuffer resbuf = xrecord.Data;
                        TypedValue[] resbufvalue = resbuf.AsArray();

                        profileObjects.Add(new ProfileObject(){
                            NodKey = (string)resbufvalue[0].Value,
                            DistanceAtCrossing = (double)resbufvalue[1].Value,
                            LineType = (string)resbufvalue[2].Value,
                            Layer = (string)resbufvalue[3].Value,
                            Depth = (int)resbufvalue[4].Value
                        });

                        ed.WriteMessage(string.Format("\n{0}, {1}, {2}, {3}, {4}", resbufvalue[0].Value, resbufvalue[1].Value, resbufvalue[2].Value, resbufvalue[3].Value, resbufvalue[4].Value));
                    }
                        
                }
                catch
                {
                    ed.WriteMessage("Does not Exists App Dictionary.");
                }
            }

            return profileObjects;
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

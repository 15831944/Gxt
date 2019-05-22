using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using Gxt.ElevationProfileDesigner;
using System.Net;
using System.Xml.Linq;
using Gxt.Windows;

namespace Gxt
{
    public class Commands
    {
        private const string APPDICTIONARYNAME = "profileDesigner";

        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database database = Application.DocumentManager.MdiActiveDocument.Database;

        [CommandMethod("gd")]
        public void GRID()
        {
            ProfileGrid profileGrid = new ProfileGrid();

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

        [CommandMethod("gl")]
        public void GradeLine()
        {
            Editor ed = doc.Editor;

            PromptEntityOptions peo = new PromptEntityOptions("Select Running Line: ");

            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK)
                return;

            Transaction trans = database.TransactionManager.StartTransaction();
            using (trans)
            {
                Polyline polyline = (Polyline)trans.GetObject(per.ObjectId, OpenMode.ForRead);
                GradeLine grade = new GradeLine(polyline);

                foreach (ProfileObject po in grade.CrossingObjects())
                {
                    WriteToNod(po);
                    //nod.ReadFromNod(po);
                }
            }
        }

        // Asynchronous helper that checks whether a URL exists
        // (i.e. that the URL is valid and can be loaded)
        private async static Task<bool> PageExists(string url)
        {
            // First check whether the URL is valid
            Uri uriResult;

            if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult) || uriResult.Scheme != Uri.UriSchemeHttp)
                return false;

            // Then we try to peform a HEAD request on the page
            // (a WebException will be fired if it doesn't exist)

            try
            {
                using (var client = new HeadClient())
                {
                    await client.DownloadStringTaskAsync(url);
                }

                return true;
            }

            catch (WebException)
            {
                return false;
            }
        }

        [CommandMethod("GXT")]
        public async static void OpenBlog()
        {
            const string url =
              "http://www.gxtltd.com/home";

            // As we're calling an async function, we need to await
            // (and mark the command itself as async)

            if (await PageExists(url))
            {
                // Now that we've validated the URL, we can call the
                // new API in AutoCAD 2015 to load our page
                Application.DocumentWindowCollection.AddDocumentWindow(
                  "GXTLTD", new System.Uri(url)
                );
            }
            else
            {
                // Print a helpful message if the URL wasn't loadable
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                ed.WriteMessage(
                  "\nCould not load url: \"{0}\".", url
                );
            }
        }

        [CommandMethod("EPD")]
        public static void EPD()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            var dialog = new Modal();
            var result = Application.ShowModalWindow(dialog);
            //if (result.Value)
                //Application.ShowAlertDialog("Hello " + dialog.UserName);
        }


        [CommandMethod("TV")]
        public static void TileVertically()
        {
            Application.DocumentWindowCollection.TileVertically();
        }

        [CommandMethod("TH")]
        public static void TileHorizontally()
        {
            Application.DocumentWindowCollection.TileHorizontally();
        }

        
        //public static void CreateAppNod()
        //{
        //    DBDictionary appDictionary;
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
        //    using (trans)
        //    {
        //        try
        //        {
        //            DBDictionary NOD = (DBDictionary)trans.GetObject(Application.DocumentManager.MdiActiveDocument.Database.NamedObjectsDictionaryId,
        //                                                                OpenMode.ForRead);
        //            //try to add new dictonary to the NOD if it fails then create a new one
        //            try
        //            {
        //                ObjectId appDictionaryId = NOD.GetAt(APPDICTIONARYNAME);

        //                //no error so dictionary already exist
        //                appDictionary = (DBDictionary)trans.GetObject(appDictionaryId, OpenMode.ForRead);
        //                ed.WriteMessage(APPDICTIONARYNAME + " Already Exists");
        //            }
        //            catch
        //            {
        //                NOD.UpgradeOpen();
        //                appDictionary = new DBDictionary();

        //                //insert new dictionary in NOD
        //                NOD.SetAt(APPDICTIONARYNAME, appDictionary);
        //                trans.AddNewlyCreatedDBObject(appDictionary, true);
        //            }
        //            trans.Commit();
        //        }
        //        catch (Autodesk.AutoCAD.Runtime.Exception e)
        //        {
        //            ed.WriteMessage("Error while opening Nod: " + e.Message);
        //        }
        //    }
        //}

        //public static void WriteToNod(ProfileObject obj)
        //{
        //    DBDictionary dBDictionary;
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

        //    using (trans)
        //    {
        //        try //try to open Nod
        //        {
        //            DBDictionary NOD = (DBDictionary)trans.GetObject(Application.DocumentManager.MdiActiveDocument.Database.NamedObjectsDictionaryId,
        //                                                                OpenMode.ForRead);

        //            try //try to add new dictonary to the NOD if it fails then return message
        //            {
        //                ObjectId appDictionaryId = NOD.GetAt(APPDICTIONARYNAME);

        //                //no error so dictionary already exist
        //                dBDictionary = (DBDictionary)trans.GetObject(appDictionaryId, OpenMode.ForRead);

        //                if (!dBDictionary.IsWriteEnabled)
        //                    dBDictionary.UpgradeOpen();

        //                //create new Xrecord
        //                Xrecord xrecord = new Xrecord();
        //                //create the resbuf list
        //                ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Text, obj.NodKey),
        //                                                        new TypedValue((int)DxfCode.Int64, obj.DistanceAtCrossing),
        //                                                        new TypedValue((int)DxfCode.Text, obj.Layer),
        //                                                        new TypedValue((int)DxfCode.Text, obj.LineType),
        //                                                        new TypedValue((int)DxfCode.Int32, obj.Depth));
        //                //add data to new record
        //                xrecord.Data = data;

        //                //create the entry to nod
        //                dBDictionary.SetAt(obj.NodKey, xrecord);

        //                try
        //                {
        //                    //add to transaction, if exist then handle in catch
        //                    trans.AddNewlyCreatedDBObject(xrecord, true);

        //                }
        //                catch
        //                {
        //                    //what todo when xrecord already exists
        //                }

        //                //all ok then commit
        //                trans.Commit();
                        
        //            }
        //            catch
        //            {
        //                ed.WriteMessage("Create dictionary NOD");
        //            }
                    
        //        }
        //        catch (Autodesk.AutoCAD.Runtime.Exception e)
        //        {
        //            ed.WriteMessage("Error while opening Nod: " + e.Message);
        //        }
        //    }
        //}


        //public static List<ProfileObject> ReadFromNod()
        //{
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

        //    List<ProfileObject> profileObjects = new List<ProfileObject>();

        //    using (trans)
        //    {
        //        try
        //        {
        //            foreach (DBDictionaryEntry entry in AppDictionary)
        //            {
        //                // check to see if our entry is in there, excpetion will be thrown if not so process that
        //                // condition in the catch
        //                ObjectId entryId = entry.m_value;
        //                //create xrecord
        //                Xrecord xrecord = default(Xrecord);

        //                //read it from the AppDirectory
        //                xrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

        //                //get the data from xrecord
        //                ResultBuffer resbuf = xrecord.Data;
        //                TypedValue[] resbufvalue = resbuf.AsArray();

        //                profileObjects.Add(new ProfileObject()
        //                {
        //                    NodKey = (string)resbufvalue[0].Value,
        //                    DistanceAtCrossing = (double)resbufvalue[1].Value,
        //                    LineType = (string)resbufvalue[2].Value,
        //                    Layer = (string)resbufvalue[3].Value,
        //                    Depth = (int)resbufvalue[4].Value
        //                });

        //                ed.WriteMessage(string.Format("\n{0}, {1}, {2}, {3}, {4}", resbufvalue[0].Value, resbufvalue[1].Value, resbufvalue[2].Value, resbufvalue[3].Value, resbufvalue[4].Value));
        //            }

        //        }
        //        catch
        //        {
        //            ed.WriteMessage("Does not Exists App Dictionary.");
        //        }
        //    }

        //    return profileObjects;
        //}

    }

    class HeadClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);

            if (req.Method == "GET")
                req.Method = "HEAD";

            return req;
        }
    }
}

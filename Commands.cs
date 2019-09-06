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
using Gxt.Forms;


namespace Gxt
{
    public class Commands
    {
        private const string APPDICTIONARYNAME = "profileDesigner";
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = Application.DocumentManager.MdiActiveDocument.Database;

        [CommandMethod("gd")]
        public void ProfileGrid()
        {
            
			PromptPointOptions ppo = new PromptPointOptions("\nSelect profile insertion point: ");
			PromptPointResult ppr = doc.Editor.GetPoint(ppo);

			if (ppr.Status != PromptStatus.OK)
			{
				return;
			}

			ProfileGrid profileGrid = null;
			try
            {
                profileGrid = new ProfileGrid();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                doc.Editor.WriteMessage("error creating grid");
            }

            if (profileGrid != null)
            {
                profileGrid.SaveGrid();
            }
        }

        [CommandMethod("gl")]
        public void GradeLine()
        {
               
            Grade grade = new Grade();
            //grade.Draw();
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
        public void EPD()
        {
            var ed = doc.Editor;
            //var trans = db.TransactionManager.StartTransaction();

            //create the nod dictionary 
            if (CreateAppNod() == true )
            {
				ProfileGrid profileGrid = null;
				Grade gradeLine = null;

				try
				{
					profileGrid = new ProfileGrid();
					gradeLine = new Grade();
					gradeLine.DrawGradeLine(profileGrid.InsertionPoint);
				}

				catch(Autodesk.AutoCAD.Runtime.Exception e)
				{
					ed.WriteMessage("EPD Error: " + e.Message);
				}

				profileGrid.SaveGrid();
			}
        }

        public bool CreateAppNod()
        {
            var ed = doc.Editor;
            var trans = db.TransactionManager.StartTransaction();
            DBDictionary appDictionary;
            
            //check for appnode existance
            using(trans)
            {
                try 
                {
                    DBDictionary nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId,
                                                                        OpenMode.ForRead);

                    //try to add new dictonary to the NOD if it fails then create a new one
                    try
                    {
                        ObjectId appDictionaryId = nod.GetAt(APPDICTIONARYNAME);
                        //if we are here no error so dictionary already exist
                        ed.WriteMessage(APPDICTIONARYNAME + " Already exists opening EPD Modal... ");
                        return false;
                    }
                    catch 
                    {
                        nod.UpgradeOpen(); 

                        appDictionary = new DBDictionary();
                        //insert new app dictionary into NOD
                        nod.SetAt(APPDICTIONARYNAME, appDictionary);
                        //since we have new object
                        trans.AddNewlyCreatedDBObject(appDictionary, true);
                        trans.Commit();
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    ed.WriteMessage("Error loading nod!!: " + e.Message);
                    return false;
                }
            } 
            return true;
        }

        public static void WriteToNod(ProfileObject obj)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction trans = doc.TransactionManager.StartTransaction();
            DBDictionary dBDictionary;

            using (trans)
            {
                DBDictionary nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId,
                                                                        OpenMode.ForRead);

                //try to add new dictonary to the NOD if it fails then create a new one
                try
                {
                    ObjectId appDictionaryId = nod.GetAt(APPDICTIONARYNAME);
                    //if we are here no error so dictionary already exist
                    dBDictionary = (DBDictionary)trans.GetObject(appDictionaryId, OpenMode.ForRead);
                }
                catch
                {
                    ed.WriteMessage(APPDICTIONARYNAME + " App Dictionary does not exist ");
                    return;
                }

                if (!dBDictionary.IsWriteEnabled)
                    dBDictionary.UpgradeOpen();

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
                dBDictionary.SetAt(obj.NodKey, xrecord);

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

            }
        }

        public static List<ProfileObject> ReadFromNod()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction trans = doc.TransactionManager.StartTransaction();
            DBDictionary dBDictionary;

            List<ProfileObject> profileObjects = new List<ProfileObject>();
        
            using (trans)
            {
                try
                {
                    DBDictionary nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId,
                                                                        OpenMode.ForRead);

                    ////try to add new dictonary to the NOD if it fails then create a new one
                    //try
                    //{
                    //    ObjectId appDictionaryId = nod.GetAt(APPDICTIONARYNAME);
                    //    //if we are here no error so dictionary already exist
                    //    dBDictionary = (DBDictionary)trans.GetObject(appDictionaryId, OpenMode.ForRead);
                    //}
                    //catch
                    //{
                    //    ed.WriteMessage(APPDICTIONARYNAME + " AppDic does not exist ");
                    //    return null;
                    //}

                    foreach (DBDictionaryEntry entry in nod)
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
                    ed.WriteMessage("Error trying to open NOD.");
                }
            }

            return profileObjects;
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

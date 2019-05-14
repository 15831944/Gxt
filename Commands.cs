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

                // create a nod dictionary for the profile objects
                Nod nod = new Nod();

                foreach (ProfileObject po in grade.CrossingObjects())
                {
                    nod.WriteToNod(po);
                    nod.ReadFromNod(po);
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
            var dialog = new ModalDialog();
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

        [CommandMethod("Nod")]
        public void CreateNod()
        {
            Nod nod = new Nod();

        }

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

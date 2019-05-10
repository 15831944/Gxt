using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace Gxt.ElevationProfileDesigner
{
    public class GradeLine
    {
        public DBObjectCollection MyProperty { get; set; }
        public Polyline Polyline { get; set; }

        public GradeLine(Polyline polyline)
        {
            this.Polyline = polyline;
        }

        //return a list of Profile objects
        public List<ProfileObject> CrossingObjects()
        {

            List<ProfileObject> profileObjects = new List<ProfileObject>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            //vertice to create selection fence
            Point3dCollection vertices = new Point3dCollection();

            for (int i = 0; i < Polyline.NumberOfVertices; i++)
            {
                vertices.Add(Polyline.GetPoint3dAt(i));
            }

            //get fence selection results
            PromptSelectionResult promptSelectionResult = ed.SelectFence(vertices);
            if (promptSelectionResult.Status != PromptStatus.OK)
            {
                return null;
            }

            Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

            using (trans)
            {
                //iterate troughth selection set and get intersection points
                foreach (SelectedObject selectedObject in promptSelectionResult.Value)
                {
                    Entity ent = (Entity)trans.GetObject(selectedObject.ObjectId, OpenMode.ForRead);

                    if (ent.ObjectId == Polyline.ObjectId)
                        continue;
                    
                    //try and if they intersect the results will be on points variable
                    try
                    {
                        Point3dCollection points = new Point3dCollection();

                        Polyline.IntersectWith(ent, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);

                        //if we are here then all good no error!
                        double distanceAtPoint = Polyline.GetDistAtPoint(points[0]);

                        //create new profile object and add to list
                        profileObjects.Add(new ProfileObject() {
                            NodKey = "epd" + ent.ObjectId,
                            DistanceAtCrossing = distanceAtPoint,
                            LineType = ent.Linetype,
                            Layer = ent.Layer,
                            Depth = 0
                        });

                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception e)
                    {
                        ed.WriteMessage(e.Message + "\n" + ent.BlockName + "Does not Intersect running line.");
                    }
                }
            }
            return profileObjects;
        }

        
        //public void GradeLineCrossings()
        //{
        //    Point3dCollection point3DCollection = new Point3dCollection();
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            
        //    //get all the vertices of the running line to create a selection fence
        //    Point3dCollection vertices = new Point3dCollection();
        //    for (int i = 0; i < Polyline.NumberOfVertices; i++)
        //    {
        //        vertices.Add(Polyline.GetPoint3dAt(i));
        //    }

        //    PromptSelectionResult promptSelectionResult = ed.SelectFence(vertices);

        //    if (promptSelectionResult.Status != PromptStatus.OK)
        //    {
        //        return;
        //    }

        //    Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

        //    //iterate troughth selection set and get intersection points
        //    foreach (SelectedObject selectedObject in promptSelectionResult.Value)
        //    {
        //        Point3dCollection points = new Point3dCollection();
        //        DBDictionary nod;

        //        Entity ent;
        //        double distanceAtPoint = 0;

        //        using (trans)
        //        {
        //            ent = (Entity)trans.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
        //            nod = (DBDictionary)trans.GetObject(Application.DocumentManager.MdiActiveDocument.Database.NamedObjectsDictionaryId, OpenMode.ForRead);

        //            //try and if they intersect the results will be on points variable
        //            try
        //            {
        //                Polyline.IntersectWith(ent, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
        //                distanceAtPoint = Polyline.GetDistAtPoint(points[0]);
        //            }
        //            catch
        //            {
        //                ed.WriteMessage(ent.BlockName + "Does not Intersect running line.");
        //            }

                    
        //        }


        //        nod.UpgradeOpen();

        //        //create a new record
        //        Xrecord xrecord = new Xrecord();

        //        //create the resbuf 
        //        ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int32, distanceAtPoint),
        //                                             new TypedValue((int)DxfCode.Text, ent.Linetype),
        //                                             new TypedValue((int)DxfCode.Text, ent.Layer),
        //                                             new TypedValue((int)DxfCode.Int32, 0));

        //        //add it to the xrecord
        //        xrecord.Data = data;

        //        //create an entry into nod
        //        nod.SetAt("epd" + ent.ObjectId, xrecord);

        //        //add to transaction 
        //        trans.AddNewlyCreatedDBObject(xrecord, true);

        //        trans.Commit();
        //    }
        //}




    }
}


/*  one option is to save in memory..... no good
 *  
 * 
 *  
 * 
 * 
 */
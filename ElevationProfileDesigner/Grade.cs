using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace Gxt.ElevationProfileDesigner
{
    public class Grade
    {
        //public List<ProfileObject> ProfileObjects { get; set; }
        public Polyline Gradeline { get; set; }
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = Application.DocumentManager.MdiActiveDocument.Database;

        public Grade()
        {
            var ed = doc.Editor;
            var trans = db.TransactionManager.StartTransaction();
            
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect running Line: ");
            peo.SetRejectMessage("Line selected invalid!");
            peo.AddAllowedClass(typeof(Polyline), true);

            PromptEntityResult per = ed.GetEntity(peo);

            if (per.Status != PromptStatus.OK)
                return;

            using (trans)
            {
                this.Gradeline = (Polyline)trans.GetObject(per.ObjectId, OpenMode.ForRead);
            }

            //CrossingObjects();
        }

        //create a list of Profile objects
   //     public void CrossingObjects()
   //     {
             
   //         Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
   //         ProfileObjects = new List<ProfileObject>();

   //         //vertice to create selection fence
   //         Point3dCollection vertices = new Point3dCollection();

   //         for (int i = 0; i < Gradeline.NumberOfVertices; i++)
   //         {
   //             vertices.Add(Gradeline.GetPoint3dAt(i));
   //         }

   //         //get fence selection results
   //         PromptSelectionResult promptSelectionResult = ed.SelectFence(vertices);
   //         if (promptSelectionResult.Status != PromptStatus.OK)
   //         {
   //             return;
   //         }

   //         Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

   //         using (trans)
   //         {
   //             //iterate troughth selection set and get intersection points
   //             foreach (SelectedObject selectedObject in promptSelectionResult.Value)
   //             {
   //                 Entity ent = (Entity)trans.GetObject(selectedObject.ObjectId, OpenMode.ForRead);

   //                 if (ent.ObjectId == Gradeline.ObjectId)
   //                     continue;
                    
   //                 //try and if they intersect the results will be on points variable
   //                 try
   //                 {
   //                     Point3dCollection points = new Point3dCollection();

   //                     Gradeline.IntersectWith(ent, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);

   //                     //if we are here then all good no error!
   //                     double distanceAtPoint = Gradeline.GetDistAtPoint(points[0]);

   //                     //create new profile object and add to list
   //                     ProfileObjects.Add(new ProfileObject() {
   //                         NodKey = "epd" + ent.ObjectId.ToString(),
   //                         DistanceAtCrossing = distanceAtPoint,
   //                         LineType = ent.Linetype,
   //                         Layer = ent.Layer,
   //                         Depth = 0
   //                     });

   //                 }
   //                 catch (Autodesk.AutoCAD.Runtime.Exception e)
   //                 {
   //                     ed.WriteMessage(e.Message + "\n" + ent.BlockName + "Does not Intersect running line.");
   //                 }
   //             }
   //         }
   //     }

   //     public void Draw()
   //     {

			//foreach (var obj in ProfileObjects)
			//{

			//}
   //     }

		public void DrawGradeLine(Point3d pt)
		{
			Database db = Application.DocumentManager.MdiActiveDocument.Database;
			Transaction trans = db.TransactionManager.StartTransaction();

			Vector3d vector = pt.GetVectorTo(new Point3d(25, 80, 0));
			pt.TransformBy(Matrix3d.Displacement(vector));

			using (trans)
			{
				BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
				BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

				Polyline gradeLine = new Polyline();
				gradeLine.AddVertexAt(0, new Point2d((pt.X + 300), pt.Y), 0, 1, 1);
				gradeLine.AddVertexAt(1, new Point2d((pt.X + 300), pt.Y), 0, 1, 1);

				btr.AppendEntity(gradeLine);
				trans.AddNewlyCreatedDBObject(gradeLine, true);

				trans.Commit();
			}
		}
    }
}


/*  one option is to save in memory..... no good
 *  
 * 
 *  
 * 
 * 
 */
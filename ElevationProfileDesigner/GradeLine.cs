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
        
        //create a collection of points 
        public Point3dCollection GradeLineCrossings()
        {
            Point3dCollection point3DCollection = new Point3dCollection();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            
            //get all the vertices of the running line to create a selection fence
            Point3dCollection vertices = new Point3dCollection();
            for (int i = 0; i < Polyline.NumberOfVertices; i++)
            {
                vertices.Add(Polyline.GetPoint3dAt(i));
            }

            PromptSelectionResult promptSelectionResult = ed.SelectFence(vertices);

            if (promptSelectionResult.Status != PromptStatus.OK)
            {
                return null;
            }

            //iterate troughth selection set and get intersection points
            foreach (SelectedObject selectedObject in promptSelectionResult.Value)
            {
                Entity ent;
                Point3dCollection points = new Point3dCollection();
                Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

                using (trans)
                {
                    ent = (Entity)trans.GetObject(selectedObject.ObjectId, OpenMode.ForRead);
                }
                //try and if they intersect the results with be on points variable
                try
                {
                    Polyline.IntersectWith(ent, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);

                }
                catch
                {
                    ed.WriteMessage(ent.BlockName + "Does not Intersect running line.");
                }

                


            }

                       
            return new Point3dCollection();
        }
    }
}

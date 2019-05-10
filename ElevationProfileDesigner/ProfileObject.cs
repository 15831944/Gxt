using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace Gxt.ElevationProfileDesigner
{
    public class UserDataClass
    {
        const string myKey = "iamBoss";

        public static void AddProfileObject()

        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            System.Collections.Hashtable ud = doc.UserData;
            ProfileObjectData pod;

            pod = ud[myKey] as ProfileObjectData;
            if (pod == null)
            {
                object obj = ud[myKey];

                if (obj == null)
                {
                    // MyData object not found - first time run
                    //pod = new ProfileObjectData();
                    //ud.Add(myKey, pod);
                }
                else
                {
                    // Found something different instead
                    ed.WriteMessage("Found an object of type \"" + obj.GetType().ToString() + "\" instead of MyData.");
                }
            }

            //if (pod != null)
            //{
            //    ed.WriteMessage("\nCounter value is: " + pod.ObjectType);
            //}
        }
    }

    public class ProfileObjectData
    {
        public string ObjectType { get; set; }
        public Point3d IntersectingPoint { get; set; }
        public double IntersectionLength { get; set; }

        public ProfileObjectData(string objType, Point3d ip, double length)
        {
            ObjectType = objType;
            IntersectingPoint = ip;
            IntersectionLength = length;
        }
    }
}

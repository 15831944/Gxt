using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace Gxt.ElevationProfileDesigner
{
    public class ProfileObject
    {
        public string NodKey { get; set; }
        public double DistanceAtCrossing { get; set; }
        public string LineType { get; set; }
        public string Layer { get; set; }
        public int Depth { get; set; }
    }
}

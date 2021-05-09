using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using GH_IO;
using GH_IO.Serialization;
using Rhino.Render;
using Rhino.Geometry;
using static Objectivism.Util;
using System.Windows.Forms;
using System.Reflection;
namespace Objectivism
{
    /*
     * Originally this class would be changed for v7 version that had extra line for dealing with SubD. 
     * Now this is done with reflection, only one gha is required for both v6 and v7. 
     * It is a bit slow, may choose to fork two versions later.
     */
    static class DeReferenceGeometryUtil
    {
        internal static IGH_GeometricGoo DeReferenceWhereRequired(IGH_GeometricGoo geom)
        {
            if (geom.IsReferencedGeometry)
            {
                geom.LoadGeometry();
                   
                //Really want c# 9 switch expressions
                if (geom is GH_Brep brep) { return new GH_Brep((Brep)brep.Value.Duplicate()); }
                if (geom is GH_Curve curve) { return new GH_Curve((Curve)curve.Value.Duplicate()); }
                if (geom is GH_Mesh mesh) { return new GH_Mesh((Mesh)mesh.Value.Duplicate()); }
                if (geom is GH_Point point) { return new GH_Point(point.Value); }
                if (geom is GH_Surface surface) { return new GH_Surface((Brep)surface.Value.Duplicate()); }
                //if (geom is GH_SubD subd) { return new GH_SubD((SubD)subd.Value.Duplicate()); }

                //If none of the hard coded cases fit use reflection to copy the object
                return DeReferenceWithReflection(geom);
            }
            else
            {
                return geom;
            }
        }

        private static IGH_GeometricGoo DeReferenceWithReflection(IGH_GeometricGoo geom)
        {
            var geomType = geom.GetType();
            var geomInfo = geomType.GetRuntimeProperty("Value");
            if (geomInfo != null)
            {
                var geomVal = geomInfo.GetValue(geom);
                if (geomVal is GeometryBase rhinoGeom)
                {
                    IGH_GeometricGoo newGoo;
                    try
                    {
                        newGoo = (IGH_GeometricGoo)Activator.CreateInstance(geomType, rhinoGeom);
                    }
                    catch { newGoo = geom; }
                    geom = newGoo;
                }

            }
            return geom.DuplicateGeometry();
        }
    }
}

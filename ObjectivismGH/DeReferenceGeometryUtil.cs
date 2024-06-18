using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Reflection;

namespace Objectivism
{
    internal static class DeReferenceGeometryUtil
    {
        /*I am convinced this should be easier to do, just using IGH_GeometricGoo.DuplicateGeometry() didn't work,
        did not seem to be properly duplicated when applying the transforms. This however does work. */
        internal static IGH_GeometricGoo DeReferenceWhereRequired( IGH_GeometricGoo geom )
        {
            if ( geom.IsReferencedGeometry )
            {
                geom.LoadGeometry();

                //As far as I am aware only these geometry types support direct reference from Rhino.   
                if ( geom is GH_Brep brep ) { return new GH_Brep( (Brep) brep.Value.Duplicate() ); }

                if ( geom is GH_Curve curve ) { return new GH_Curve( (Curve) curve.Value.Duplicate() ); }

                if ( geom is GH_Mesh mesh ) { return new GH_Mesh( (Mesh) mesh.Value.Duplicate() ); }

                if ( geom is GH_Point point ) { return new GH_Point( point.Value ); }

                if ( geom is GH_Surface surface ) { return new GH_Surface( (Brep) surface.Value.Duplicate() ); }
                //if (geom is GH_SubD subd) { return new GH_SubD((SubD)subd.Value.Duplicate()); }
                //(Building for Rhino 6, subD workds with the reflection method.)

                //If none of the hard coded cases fit use reflection to copy the object
                return DeReferenceWithReflection( geom );
            }

            return geom;
        }

        private static IGH_GeometricGoo DeReferenceWithReflection( IGH_GeometricGoo geom )
        {
            var geomType = geom.GetType();
            var geomInfo = geomType.GetRuntimeProperty( "Value" );
            if ( geomInfo != null )
            {
                var geomVal = geomInfo.GetValue( geom );
                if ( geomVal is GeometryBase rhinoGeom )
                {
                    IGH_GeometricGoo newGoo;
                    try
                    {
                        newGoo = (IGH_GeometricGoo) Activator.CreateInstance( geomType, rhinoGeom );
                    }
                    //Plugins that implement IGH_GeometricGoo may not have a constructor like above
                    //In this case revert to DuplicateGeometry and hope it is implemented properly. 
                    catch { newGoo = geom.DuplicateGeometry(); }

                    geom = newGoo;
                }
            }

            return geom.DuplicateGeometry();
        }
    }
}
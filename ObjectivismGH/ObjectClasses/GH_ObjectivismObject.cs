using GH_IO;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Render;
using System;

namespace Objectivism
{
    public class GH_ObjectivismObject : GH_GeometricGoo<ObjectivismObject>, IGH_PreviewData, IGH_RenderAwareData,
        GH_ISerializable
    {
        public GH_ObjectivismObject( ObjectivismObject value )
        {
            this.Value = value;
        }

        public GH_ObjectivismObject()
        {
            this.Value = null;
        }

        public GH_ObjectivismObject( GH_ObjectivismObject other )
        {
            this.Value = other.Value;
        }

        public override bool IsValid => true;

        public override bool IsReferencedGeometry => false;
        public override string TypeName => this.Value.TypeName;

        public override string TypeDescription => $"Objectivism object of type: {this.Value.TypeName}";

        public override BoundingBox Boundingbox => this.Value.BoundingBox;

        public override bool Read( GH_IReader reader )
        {
            var value = new ObjectivismObject();
            value.GH_Read( reader );
            this.Value = value;
            return true;
        }

        public override bool Write( GH_IWriter writer )
        {
            try
            {
                this.Value.GH_Write( writer );
                return true;
            }
            catch ( Exception e )
            {
                writer.SetString( "Oh Dear", "An Exception Occured" );
                writer.SetString( "Exception Message", e.Message );
                writer.SetString( "Stack Trace", e.StackTrace );
                writer.SetString( "Exception Type", e.GetType().FullName );
                writer.SetString( "TargetSite", e.TargetSite.ToString() );
                return true;
            }
        }


        public BoundingBox ClippingBox => this.Value.ClippingBox;

        public void DrawViewportWires( GH_PreviewWireArgs args ) => this.Value.DrawViewportWires( args );

        public void DrawViewportMeshes( GH_PreviewMeshArgs args ) => this.Value.DrawViewportMeshes( args );

        public void AppendRenderGeometry( GH_RenderArgs args, RenderMaterial material ) =>
            this.Value.AppendRenderGeometry( args, material );

        public override IGH_Goo Duplicate() => new GH_ObjectivismObject( this.Value );

        public override IGH_GeometricGoo DuplicateGeometry() => new GH_ObjectivismObject( this.Value );

        public override string ToString() => $"{this.Value.TypeName} object";

        public override bool CastFrom( object source )
        {
            if ( source == null ) { return false; }

            if ( source is GH_ObjectivismObject gh_obj )
            {
                this.Value = gh_obj.Value;
                return true;
            }

            if ( source is GH_Goo<ObjectivismObject> goo )
            {
                this.Value = goo.Value;
                return true;
            }

            if ( source is ObjectivismObject obj2 )
            {
                this.Value = obj2;
                return true;
            }

            return false;
        }

        public override BoundingBox GetBoundingBox( Transform xform ) => this.Value.Transform( xform ).BoundingBox;

        public override IGH_GeometricGoo Transform( Transform xform ) =>
            new GH_ObjectivismObject( this.Value.Transform( xform ) );

        public override IGH_GeometricGoo Morph( SpaceMorph xmorph ) =>
            new GH_ObjectivismObject( this.Value.Morph( xmorph ) );

        public override object ScriptVariable() => this.Value.ToDynamic();
    }
}
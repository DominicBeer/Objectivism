using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Render;
using System.Collections.Generic;
using System.Linq;
using static Objectivism.DeReferenceGeometryUtil;
using static Objectivism.Util;


namespace Objectivism
{
    internal enum PropertyAccess { Item, List, Tree }

    public class ObjectProperty : IGH_RenderAwareData, IGH_PreviewData
    {
        public ObjectProperty()
        {
            this.Data = new GH_Structure<IGH_Goo>();
            this.Access = PropertyAccess.Item;
        }

        public ObjectProperty( IGH_Goo item )
        {
            this.Data = new GH_Structure<IGH_Goo>();
            var item2 = this.DeReferenceIfRequired( item );
            this.Data.Append( item2 );
            this.Access = PropertyAccess.Item;
        }

        public ObjectProperty( List<IGH_Goo> list )
        {
            this.Data = new GH_Structure<IGH_Goo>();
            var list2 = list.Select( goo => this.DeReferenceIfRequired( goo ) ).ToList();
            this.Data.AppendRange( list2 );
            this.Access = PropertyAccess.List;
        }

        public ObjectProperty( GH_Structure<IGH_Goo> tree )
        {
            var tree2 = tree.MapTree( this.DeReferenceIfRequired );
            this.Data = tree2;
            this.Access = PropertyAccess.Tree;
        }

        internal ObjectProperty( GH_Structure<IGH_Goo> tree, PropertyAccess access )
        {
            var tree2 = tree.MapTree( this.DeReferenceIfRequired );
            this.Data = tree2;
            this.Access = access;
        }

        public ObjectProperty( ObjectProperty other )
        {
            this.Access = other.Access;
            this.Data = other.Data.MapTree( this.DuplicateUtil );
        }

        public bool PreviewOn { get; internal set; } = true;
        public GH_Structure<IGH_Goo> Data { get; private set; }
        internal PropertyAccess Access { get; private set; }

        public BoundingBox BoundingBox
        {
            get
            {
                if ( this.HasGeometry && this.PreviewOn )
                {
                    var boxes = new List<BoundingBox>();
                    foreach ( var goo in this.Data )
                    {
                        if ( goo is IGH_GeometricGoo geom )
                        {
                            boxes.Add( geom.Boundingbox );
                        }
                    }

                    return UnionBoxes( boxes );
                }

                return default;
            }
        }

        public bool HasGeometry => this.Data.Any( goo => goo is IGH_GeometricGoo );
        public BoundingBox ClippingBox => this.BoundingBox;

        public void DrawViewportWires( GH_PreviewWireArgs args )
        {
            if ( !this.PreviewOn )
            {
                return;
            }

            foreach ( var goo in this.Data )
            {
                if ( goo is IGH_PreviewData previewGoo )
                {
                    previewGoo.DrawViewportWires( args );
                }
            }
        }

        public void DrawViewportMeshes( GH_PreviewMeshArgs args )
        {
            if ( !this.PreviewOn )
            {
                return;
            }

            foreach ( var goo in this.Data )
            {
                if ( goo is IGH_PreviewData previewGoo )
                {
                    previewGoo.DrawViewportMeshes( args );
                }
            }
        }

        public void AppendRenderGeometry( GH_RenderArgs args, RenderMaterial material )
        {
            if ( !this.PreviewOn )
            {
                return;
            }

            foreach ( var goo in this.Data )
            {
                if ( goo is IGH_RenderAwareData renderGoo )
                {
                    renderGoo.AppendRenderGeometry( args, material );
                }
            }
        }

        private IGH_Goo DeReferenceIfRequired( IGH_Goo goo )
        {
            if ( goo is IGH_GeometricGoo geom )
            {
                return DeReferenceWhereRequired( geom );
            }

            return goo;
        }


        private IGH_Goo DuplicateUtil( IGH_Goo goo )
        {
            if ( goo is IGH_GeometricGoo geom )
            {
                return geom.DuplicateGeometry();
            }

            if ( goo != null )
            {
                return goo.Duplicate();
            }

            return goo;
        }

        public bool WriteProp( GH_IWriter writer )
        {
            writer.SetTree( "PropertyDataTree", this.Data );
            writer.SetInt32( "Access", (int) this.Access );
            writer.SetBoolean( "PreviewToggle", this.PreviewOn );
            return true;
        }

        public bool ReadProp( GH_IReader reader )
        {
            this.Data = reader.GetTree( "PropertyDataTree" );
            this.Access = (PropertyAccess) reader.GetInt32( "Access" );
            try
            {
                this.PreviewOn = reader.GetBoolean( "PreviewToggle" );
            }
            catch
            {
                this.PreviewOn = true;
            }

            return true;
        }

        public ObjectProperty Transform( Transform xform )
        {
            var newData = this.Data.MapTree( TransformUtil, xform );
            return new ObjectProperty( newData, this.Access );
        }

        public ObjectProperty Morph( SpaceMorph morph )
        {
            var newData = this.Data.MapTree( MorphUtil, morph );
            return new ObjectProperty( newData, this.Access );
        }

        public static IGH_Goo TransformUtil( IGH_Goo item, Transform xform ) =>
            item is IGH_GeometricGoo geom
                ? geom.DuplicateGeometry().Transform( xform )
                : item;

        public static IGH_Goo MorphUtil( IGH_Goo item, SpaceMorph morph ) =>
            item is IGH_GeometricGoo geom
                ? geom.DuplicateGeometry().Morph( morph )
                : item;
    }
}
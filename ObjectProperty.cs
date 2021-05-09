using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using static Objectivism.Util;
using static Objectivism.DeReferenceGeometryUtil;


namespace Objectivism
{
    enum PropertyAccess {Item, List, Tree}
    public class ObjectProperty : IGH_RenderAwareData, IGH_PreviewData
    {

        public GH_Structure<IGH_Goo> Data { get; private set; }
        internal PropertyAccess Access { get; private set; } 
        public BoundingBox BoundingBox 
        { 
            get
            {
                if (this.HasGeometry)
                {
                    var boxes = Data
                        .Where(goo => goo is IGH_GeometricGoo)
                        .Select(goo => ((IGH_GeometricGoo)goo).Boundingbox);
                    return UnionBoxes(boxes);
                }
                else
                {
                    return default;
                }
            }
        }
        public bool HasGeometry => Data.Any(goo => goo is IGH_GeometricGoo);
        public BoundingBox ClippingBox => BoundingBox;
        
        public ObjectProperty() 
        {
            Data = new GH_Structure<IGH_Goo>();
            Access = PropertyAccess.Item;

        }

        public ObjectProperty(IGH_Goo item)
        {
            Data = new GH_Structure<IGH_Goo>();
            var item2 = DeReferenceIfRequired(item);
            Data.Append(item2);
            Access = PropertyAccess.Item;
        }
        public ObjectProperty(List<IGH_Goo> list)
        {
            Data = new GH_Structure<IGH_Goo>();
            var list2 = list.Select(goo => DeReferenceIfRequired(goo)).ToList();
            Data.AppendRange(list2);
            Access = PropertyAccess.List;
        }

        public ObjectProperty(GH_Structure<IGH_Goo> tree)
        {
            var tree2 = tree.MapTree(DeReferenceIfRequired);
            Data = tree2;
            Access = PropertyAccess.Tree;
        }

        internal ObjectProperty(GH_Structure<IGH_Goo> tree, PropertyAccess access)
        {
            var tree2 = tree.MapTree(DeReferenceIfRequired);
            Data = tree2;
            Access = access;
        }

        public ObjectProperty(ObjectProperty other)
        {
            Access = other.Access;
            Data = other.Data.MapTree(DuplicateUtil);
        }

        private IGH_Goo DeReferenceIfRequired(IGH_Goo goo)
        {
            if(goo is IGH_GeometricGoo geom)
            {
                return DeReferenceWhereRequired(geom);
            }
            return goo;
        }

        

        private IGH_Goo DuplicateUtil(IGH_Goo goo)
        {
            if (goo is IGH_GeometricGoo geom)
            {
                return geom.DuplicateGeometry();
            }
            return goo.Duplicate();
        }

        public bool WriteProp(GH_IWriter writer)
        {
            writer.SetTree("PropertyDataTree", Data);
            writer.SetInt32("Access", (int)Access);
            return true;
        }

        public bool ReadProp(GH_IReader reader)
        {
            Data = reader.GetTree("PropertyDataTree");
            Access = (PropertyAccess)reader.GetInt32("Access");
            return true;
        }

        public ObjectProperty Transform(Transform xform)
        {
            var newData = Data.MapTree(TransformUtil,xform);
            return new ObjectProperty(newData, this.Access);
        }
        public ObjectProperty Morph(SpaceMorph morph)
        {
            var newData = Data.MapTree(MorphUtil, morph);
            return new ObjectProperty(newData, this.Access);
        }

        public static IGH_Goo TransformUtil(IGH_Goo item, Transform xform)
        {
            return item is IGH_GeometricGoo geom
                ? geom.DuplicateGeometry().Transform(xform)
                : item;
        }
        public static IGH_Goo MorphUtil(IGH_Goo item, SpaceMorph morph)
        {
            return item is IGH_GeometricGoo geom
                ? geom.DuplicateGeometry().Morph(morph)
                : item;
        }

        public void AppendRenderGeometry(GH_RenderArgs args, RenderMaterial material)
        {
            Data
                .Where(goo => goo is IGH_RenderAwareData)
                .Select(goo => (IGH_RenderAwareData)goo)
                .ToList()
                .ForEach(goo => goo.AppendRenderGeometry(args, material));
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            Data
                .Where(goo => goo is IGH_PreviewData)
                .Select(goo => (IGH_PreviewData)goo)
                .ToList()
                .ForEach(goo => goo.DrawViewportWires(args));
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            Data
                .Where(goo => goo is IGH_PreviewData)
                .Select(goo => (IGH_PreviewData)goo)
                .ToList()
                .ForEach(goo => goo.DrawViewportMeshes(args));
        }
    }
}

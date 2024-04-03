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
    enum PropertyAccess { Item, List, Tree }
    public class ObjectProperty : IGH_RenderAwareData, IGH_PreviewData
    {
        public bool PreviewOn { get; internal set; } = true;
        public GH_Structure<IGH_Goo> Data { get; private set; }
        internal PropertyAccess Access { get; private set; }
        public BoundingBox BoundingBox
        {
            get
            {
                if (HasGeometry && PreviewOn)
                {
                    var boxes = new List<BoundingBox>();
                    foreach (var goo in Data)
                    {
                        if (goo is IGH_GeometricGoo geom)
                        {
                            boxes.Add(geom.Boundingbox);
                        }
                    }
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
            if (goo is IGH_GeometricGoo geom)
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
            if (goo != null)
            {
                return goo.Duplicate();
            }
            else { return goo; }
        }

        public bool WriteProp(GH_IWriter writer)
        {
            writer.SetTree("PropertyDataTree", Data);
            writer.SetInt32("Access", (int)Access);
            writer.SetBoolean("PreviewToggle", PreviewOn);
            return true;
        }

        public bool ReadProp(GH_IReader reader)
        {
            Data = reader.GetTree("PropertyDataTree");
            Access = (PropertyAccess)reader.GetInt32("Access");
            try
            {
                PreviewOn = reader.GetBoolean("PreviewToggle");
            }
            catch
            {
                PreviewOn = true;
            }
            return true;
        }

        public ObjectProperty Transform(Transform xform)
        {
            var newData = Data.MapTree(TransformUtil, xform);
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
            if (!PreviewOn) return;
            foreach (var goo in Data)
            {
                if (goo is IGH_RenderAwareData renderGoo)
                {
                    renderGoo.AppendRenderGeometry(args, material);
                }
            }
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (!PreviewOn) return;
            foreach (var goo in Data)
            {
                if (goo is IGH_PreviewData previewGoo)
                {
                    previewGoo.DrawViewportWires(args);
                }
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (!PreviewOn) return;
            foreach (var goo in Data)
            {
                if (goo is IGH_PreviewData previewGoo)
                {
                    previewGoo.DrawViewportMeshes(args);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel;
using GH_IO;
using GH_IO.Serialization;
using Rhino.Geometry;
using Rhino.Render;
using System.Windows.Forms;
namespace Objectivism
{
    public class GH_ObjectivismObject : GH_GeometricGoo<ObjectivismObject>,IGH_PreviewData,IGH_RenderAwareData,GH_ISerializable
    {
        public override bool IsValid => true;

        public override bool IsReferencedGeometry => false;
        public override string TypeName => Value.TypeName;

        public override string TypeDescription => $"Objectivism object of type: {Value.TypeName}";


        public BoundingBox ClippingBox => Value.ClippingBox;

        public override BoundingBox Boundingbox => Value.BoundingBox;

        public override IGH_Goo Duplicate()
        {
            return new GH_ObjectivismObject(this.Value);
        }
        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new GH_ObjectivismObject(this.Value);
        }

        public override string ToString()
        {
            return $"{Value.TypeName} object";
        }

        public GH_ObjectivismObject(ObjectivismObject value)
        {
            Value = value;
        }
        public GH_ObjectivismObject()
        {
            Value = null;
        }
        public GH_ObjectivismObject(GH_ObjectivismObject other)
        {
            Value = other.Value;
        }

        public override bool Read(GH_IReader reader)
        {
            var value = new ObjectivismObject();
            value.GH_Read(reader);
            Value = value;
            return true;
        }
        public override bool Write(GH_IWriter writer)
        {
            try
            {
                Value.GH_Write(writer);
                return true;
            }
            catch(Exception e)
            {
                writer.SetString("Oh Dear","An Exception Occured");
                writer.SetString("Exception Message", e.Message);
                writer.SetString("Stack Trace", e.StackTrace);
                writer.SetString("Exception Type", e.GetType().FullName);
                writer.SetString("TargetSite", e.TargetSite.ToString());
                return true;
            }
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            Value.DrawViewportWires(args);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            Value.DrawViewportMeshes(args);
        }

        public void AppendRenderGeometry(GH_RenderArgs args, RenderMaterial material)
        {
            Value.AppendRenderGeometry(args, material);
        }

        public override bool CastFrom(object source)
        {
            if (source == null) { return false; }
            if (source is GH_ObjectivismObject gh_obj)
            {
                this.Value = gh_obj.Value;
                return true;
            }
            if (source is GH_Goo<ObjectivismObject> goo)
            {
                this.Value = goo.Value;
                return true;
            }
            if (source is ObjectivismObject obj2)
            {
                this.Value = obj2;
                return true;
            }
            return false;
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            return Value.Transform(xform).BoundingBox;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            return new GH_ObjectivismObject(Value.Transform(xform));
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            return new GH_ObjectivismObject(Value.Morph(xmorph));
        }
    }
}

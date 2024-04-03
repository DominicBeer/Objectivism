using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Render;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Objectivism
{
    public class ObjectivismObject : IGH_PreviewData, IGH_RenderAwareData
    {
        private List<(string Name, ObjectProperty Property)> properties;
        private Dictionary<string, int> propertyGetter;

        public List<string> AllProperties => this.properties.Select(x => x.Name).ToList();
        public string TypeName { get; private set; }
        private BoundingBox boxCache;
        private bool boxIsCached = false;
        public BoundingBox BoundingBox
        {
            get
            {
                if (boxIsCached)
                {
                    return boxCache;
                }
                var boxes = properties
                    .Select(pair => pair.Property)
                    .Where(p => p.HasGeometry)
                    .Select(p => p.BoundingBox)
                    .ToList();
                boxCache = Util.UnionBoxes(boxes);
                boxIsCached = true;
                return boxCache;
            }
        }

        public BoundingBox ClippingBox => BoundingBox;

        public ObjectivismObject()
        {
            this.TypeName = "Objectivism Object";
        }


        public ObjectivismObject(List<(string Name, ObjectProperty Property)> props, string typeName)
        {
            TypeName = typeName;
            properties = props;
            propertyGetter = props
                .Select((p, i) => (p.Name, i))
                .ToDictionary(t => t.Name, t => t.i);
        }
        public ObjectivismObject(ObjectivismObject obj)
        {
            TypeName = obj.TypeName;
            properties = obj.properties.Select(pair => (pair.Name, new ObjectProperty(pair.Property))).ToList();
            propertyGetter = new Dictionary<string, int>(obj.propertyGetter);
        }

        public bool HasProperty(string name)
        {
            return propertyGetter.ContainsKey(name);
        }

        internal bool HasProperty(string name, PropertyAccess access)
        {
            if (TryGetProperty(name, out var property))
            {
                return property.Access == access;
            }
            return false;
        }

        internal bool Implements(ObjectivismObject template)
        {
            return template.properties.All(prop => this.HasProperty(prop.Name, prop.Property.Access));
        }

        public bool TryGetProperty(string name, out ObjectProperty property)
        {
            property = null;
            if (propertyGetter.ContainsKey(name))
            {
                property = properties[propertyGetter[name]].Property;
                return true;
            }
            return false;
        }

        public ObjectProperty GetProperty(string name)
        {
            if (propertyGetter.ContainsKey(name))
            {
                return properties[propertyGetter[name]].Property;
            }
            else
            {
                return null;
            }
        }


        internal (ObjectivismObject obj, AccessInfo conflicts) AddOrChangeProperties(List<(string name, ObjectProperty newProperty)> changes) =>
            AddOrChangeProperties(changes, this.TypeName);

        internal (ObjectivismObject obj, AccessInfo conflicts) AddOrChangeProperties(List<(string name, ObjectProperty newProperty)> changes, string newName)
        {
            var newObj = new ObjectivismObject(this);
            newObj.TypeName = newName;
            var numberOfExistingProps = newObj.properties.Count;
            AccessInfo accessInfo = new AccessInfo();
            foreach ((string name, var newProp) in changes)
            {

                if (newObj.propertyGetter.ContainsKey(name))
                {
                    var currentAccess = newObj.GetProperty(name).Access;
                    var newAccess = newProp.Access;
                    if (newAccess != currentAccess)

                    {
                        accessInfo.AddConflict(name);
                    }
                    newObj.properties[propertyGetter[name]] = (name, newProp);
                }
                else
                {
                    newObj.properties.Add((name, newProp));
                    newObj.propertyGetter.Add(name, numberOfExistingProps);
                    numberOfExistingProps++;
                }
            }
            return (newObj, accessInfo);
        }
        internal ObjectivismObject AddProperties(List<(string name, ObjectProperty newProperty)> additions)
        {
            var newObj = new ObjectivismObject(this);
            var numberOfExistingProps = newObj.properties.Count;
            foreach (var addition in additions)
            {
                newObj.properties.Add(addition);
                newObj.propertyGetter.Add(addition.name, numberOfExistingProps);
                numberOfExistingProps++;
            }
            return newObj;
        }
        public ObjectivismObject Transform(Transform xform)
        {
            var newObj = new ObjectivismObject(this);
            newObj.properties = newObj.properties
                .Select(p => (p.Name, p.Property.Transform(xform)))
                .ToList();
            return newObj;
        }
        public ObjectivismObject Morph(SpaceMorph xmorph)
        {
            var newObj = new ObjectivismObject(this);
            newObj.properties = newObj.properties
                .Select(p => (p.Name, p.Property.Morph(xmorph)))
                .ToList();
            return newObj;

        }
        public bool GH_Write(GH_IWriter writer)
        {
            writer.SetString("ObjectTypeName", TypeName);

            writer.SetInt32("NumberOfProperties", properties.Count);
            var nameWriter = writer.CreateChunk("Names");
            int i = 0;
            foreach (var pair in properties)
            {
                nameWriter.SetString("Name", i, pair.Name);
                var propWriter = nameWriter.CreateChunk("Prop", i);
                pair.Property.WriteProp(propWriter);
                i++;
            }
            return true;
        }

        public bool GH_Read(GH_IReader reader)
        {
            try
            {
                this.TypeName = reader.GetString("ObjectTypeName");
                this.properties = new List<(string Name, ObjectProperty Property)>();
                this.propertyGetter = new Dictionary<string, int>();
                int count = reader.GetInt32("NumberOfProperties");
                var nameReader = reader.FindChunk("Names");
                for (int i = 0; i < count; i++)
                {
                    string name = nameReader.GetString("Name", i);
                    var prop = new ObjectProperty();
                    var propReader = nameReader.FindChunk("Prop", i);
                    prop.ReadProp(propReader);
                    properties.Add((name, prop));
                    propertyGetter.Add(name, i);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            properties.ForEach(prop => prop.Property.DrawViewportWires(args));
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            properties.ForEach(prop => prop.Property.DrawViewportMeshes(args));
        }

        public void AppendRenderGeometry(GH_RenderArgs args, RenderMaterial material)
        {
            properties.ForEach(prop => prop.Property.AppendRenderGeometry(args, material));
        }

        internal dynamic ToDynamic()
        {
            var eo = new ExpandoObject();
            var eoColl = (ICollection<KeyValuePair<string, object>>)eo;
            foreach (var pair in properties)
            {
                var name = pair.Name.SpacesToUnderscores();
                var prop = pair.Property;
                if (prop.Access == PropertyAccess.Item)
                {
                    var item = prop != null
                        ? ProcessGoo(prop.Data.get_FirstItem(false))
                        : null;
                    eoColl.Add(new KeyValuePair<string, object>(name, item));
                }
                if (prop.Access == PropertyAccess.List)
                {
                    var list = prop != null
                        ? prop.Data.Branches[0].Select(ProcessGoo).ToList()
                        : new List<object>();
                    eoColl.Add(new KeyValuePair<string, object>(name, list));
                }
                if (prop.Access == PropertyAccess.Tree)
                {
                    var tree = prop != null
                        ? prop.Data.ToDataTree(ProcessGoo)
                        : new Grasshopper.DataTree<object>();
                    eoColl.Add(new KeyValuePair<string, object>(name, tree));
                }
            }
            dynamic eoDynamic = eo;
            return eoDynamic;

        }



        private static object ProcessGoo(IGH_Goo goo)
        {
            return goo != null ? goo.ScriptVariable() : null;
        }

        internal class AccessInfo
        {
            List<string> Conflicts;
            public void AddConflict(string propertyName) => Conflicts.Add(propertyName);
            public void BroadcastConflicts(GH_Component comp) => Conflicts.ForEach(conflict => comp.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"{conflict} has its access level changed"));

            public AccessInfo()
            {
                Conflicts = new List<string>();
            }
        }
    }
}

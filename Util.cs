using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GH_IO;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Grasshopper.Kernel;

namespace Objectivism
{
    static class Util
    {
        public static bool SetGoo(this GH_IWriter writer, string itemName, IGH_Goo item)
        {
            var itemWriter = writer.CreateChunk(itemName);
            var tree = new GH_Structure<IGH_Goo>();
            tree.Append(item);
            return tree.Write(itemWriter);
        }

        public static IGH_Goo GetGoo(this GH_IReader reader, string itemName)
        {
            var dataReader = reader.FindChunk(itemName);
            var tree = new GH_Structure<IGH_Goo>();
            tree.Read(dataReader);
            return tree.Branches[0][0];
        }

        public static bool SetList(this GH_IWriter writer,string listName, List<IGH_Goo> list)
        {
            var listWriter = writer.CreateChunk(listName);
            var tree = new GH_Structure<IGH_Goo>();
            tree.AppendRange(list);
            return tree.Write(listWriter);
        }

        public static List<IGH_Goo> GetList(this GH_IReader reader, string listName)
        {
            var listReader = reader.FindChunk(listName);
            var tree = new GH_Structure<IGH_Goo>();
            tree.Read(listReader);
            return tree.Branches[0];
        }

        public static bool SetTree(this GH_IWriter writer, string treeName, GH_Structure<IGH_Goo> tree)
        {
            var treeWriter = writer.CreateChunk(treeName);
            return tree.Write(treeWriter);

        }
        public static GH_Structure<IGH_Goo> GetTree(this GH_IReader reader, string treeName)
        {
            var tree = new GH_Structure<IGH_Goo>();
            var treeReader = reader.FindChunk(treeName);
            tree.Read(treeReader);
            return tree;
        }
        public static BoundingBox UnionBoxes(IEnumerable<BoundingBox> boxes)
        {
            var points = boxes
                .Select(box => box.Min)
                .Concat(boxes
                .Select(box => box.Max));
            return new BoundingBox(points);
        }

        public static GH_Structure<IGH_Goo> MapTree(this GH_Structure<IGH_Goo> tree, Func<IGH_Goo, IGH_Goo> function)
        {
            var tree2 = new GH_Structure<IGH_Goo>(tree, true);
            foreach(var branch in tree2.Branches)
            {
                for(int i = 0; i<branch.Count; i++)
                {
                    branch[i] = function(branch[i]);
                }
            }
            return tree2;
        }
        public static GH_Structure<IGH_Goo> MapTree<T>(this GH_Structure<IGH_Goo> tree, Func<IGH_Goo,T, IGH_Goo> function, T param)
        {
            var tree2 = new GH_Structure<IGH_Goo>(tree, true);
            foreach (var branch in tree2.Branches)
            {
                for (int i = 0; i < branch.Count; i++)
                {
                    branch[i] = function(branch[i], param);
                }
            }
            return tree2;
        }

        public static IEnumerable<(int , T )> Enumerate<T>(this IEnumerable<T> series)
        {
            return series.Select((x, i) => (i, x));
        }

        internal static (string Name, ObjectProperty Property) GetPropertyData(IGH_DataAccess DA, int paramIndex, IGH_Component @this)
        {
            var param = @this.Params.Input[paramIndex];
            ObjectProperty prop;
            var name = param.NickName;
            if (param.Access == GH_ParamAccess.item)
            {
                IGH_Goo item = null;
                if (!DA.GetData(paramIndex, ref item))
                {
                    @this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"{name} has no input and has been assigned null data");
                }
                prop = new ObjectProperty(item);
            }
            else if (param.Access == GH_ParamAccess.list)
            {
                var items = new List<IGH_Goo>();
                DA.GetDataList(paramIndex, items);
                prop = new ObjectProperty(items);
            }
            else //tree access
            {
                DA.GetDataTree(paramIndex, out GH_Structure<IGH_Goo> itemTree);
                prop = new ObjectProperty(itemTree);
            }
            return (name, prop);
        }

        internal static GH_Structure<IGH_Goo> EmptyTree 
        { 
            get 
            {
                var tree = new GH_Structure<IGH_Goo>();
                tree.AppendRange(new List<IGH_Goo>());
                return tree;
            } 
        }
    }
}

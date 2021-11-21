using System;
using System.Collections.Generic;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Grasshopper;
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
        public static BoundingBox UnionBoxes(List<BoundingBox> boxes)
        {
            var points = new List<Point3d>(boxes.Count*2);
            foreach(var box in boxes)
            {
                points.Add(box.Min);
                points.Add(box.Max);
            }
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

        //python style enumerate for use in a foreach loop
        public static IEnumerable<(int , T )> Enumerate<T>(this IEnumerable<T> series)
        {
            return series.Select((x, i) => (i, x));
        }


        public static string SpacesToUnderscores(this string @this)
        {
            return @this.Replace(" ", "_");
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

        internal static object UnwrapGoo(this IGH_Goo goo)
        {
            var goo2 = goo.Duplicate();
            var type = goo2.GetType();
            var propInfo = type.GetProperty("Value");
            if (propInfo != null)
            {
                return propInfo.GetValue(goo2);
            }
            else
            {
                return goo2;
            }
        }

        internal static object PackSubObjects(this object obj)
        {
            if (obj is ObjectivismObject objectivismObject)
            {
                return objectivismObject.ToDynamic();
            }
            else
            {
                return obj;
            }
        }

        internal static DataTree<object> ToDataTree(this GH_Structure<IGH_Goo> gooTree, Func<IGH_Goo, object> deGoo)
        {
            var tree = new DataTree<object>();
            foreach(var path in gooTree.Paths)
            {
                var branch = gooTree[path].Select(deGoo);
                tree.AddRange(branch,path);
            }
            return tree;
        }
    }
}

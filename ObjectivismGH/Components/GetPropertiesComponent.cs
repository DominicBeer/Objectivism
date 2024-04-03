using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism
{
    public class GetPropertiesComponent : GH_Component, IGH_VariableParameterComponent, IHasMultipleTypes
    {
        /// <summary>
        /// Initializes a new instance of the GetPropertiesComponent class.
        /// </summary>
        public GetPropertiesComponent()
          : base("Get Object Properties", "Object.",
              "Retrieve stored properties of an Objectivism object",
              "Sets", "Objectivism")
        {
            this.IconDisplayMode = GH_IconDisplayMode.name;
            this.Message = getGraftMessage();
        }
        private bool GraftItems = false;
        private string getGraftMessage() => GraftItems
            ? "Graft all properties"
            : "Graft lists + trees";
        private HashSet<string> PropertyNames = new HashSet<string>();
        internal List<string> GetUnusedNames() => PropertyNames.Except(Params.Output.Select(p => p.NickName)).ToList();
        internal string NextUnusedName()
        {
            var unusedNames = GetUnusedNames();
            return unusedNames.Count == 0
                ? "No properties to list"
                : unusedNames[0];

        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "O", "Objectivism object to retrieve properties from", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void BeforeSolveInstance()
        {
            UpdateTypeNames();
            if (!JustOneTypeName())
            {
                this.NickName = "MultipleTypes.";
            }
            else
            {
                this.NickName = GetTypeName() + ".";
            }
            this.PropertyNames.Clear();
            accessChecker = new AccessChecker(this);
            base.BeforeSolveInstance();
        }

        private AccessChecker accessChecker;

        private HashSet<string> typeNames = new HashSet<string>();
        public HashSet<string> TypeNames => typeNames;

        private void UpdateTypeNames()
        {
            typeNames.Clear();
            var data = this.Params.Input[0].VolatileData.AllData(true);
            foreach (var goo in data)
            {
                if (goo is GH_ObjectivismObject ghObj)
                {
                    var tn = ghObj.Value.TypeName;
                    typeNames.Add(tn);
                }
            }
        }

        private bool JustOneTypeName() => typeNames.Count <= 1;

        private string GetTypeName()
        {
            var input = (Param_GenericObject)this.Params.Input[0];
            var allObjects = input.PersistentDataCount != 0
                ? input.PersistentData
                : input.VolatileData;
            if (!allObjects.IsEmpty)
            {
                if (allObjects is GH_Structure<IGH_Goo> tree)
                {
                    var first = tree.get_FirstItem(true);
                    if (first != null)
                    {
                        if (first is GH_ObjectivismObject obj)
                        {
                            return obj.Value.TypeName;
                        }
                    }
                }
            }
            return "NoValidType";
        }




        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            IGH_Goo goo = null;
            if (!DA.GetData(0, ref goo))
            {
                return;
            }
            ObjectivismObject obj;
            if (goo is GH_ObjectivismObject ghObj)
            {
                obj = ghObj.Value;
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Can only get properties from ojects built with Objectivism");
                return;
            }
            PropertyNames.UnionWith(obj.AllProperties);



            foreach ((int i, var param) in Params.Output.Enumerate())
            {

                string name = param.NickName;



                var prop = obj.GetProperty(name);
                PropertyAccess access;
                if (prop != null)
                {
                    access = prop.Access;
                    accessChecker.AccessCheck(prop, name);
                }
                else
                {
                    access = accessChecker.BestGuessAccess(name);
                }



                if (access == PropertyAccess.Item)
                {
                    var item = prop != null
                        ? prop.Data.get_FirstItem(false)
                        : null;
                    var path = DA.ParameterTargetPath(i);
                    if (GraftItems)
                    {
                        int[] index = { DA.ParameterTargetIndex(0) };
                        var newPath = new GH_Path(path.Indices.Concat(index).ToArray());
                        var tree = new GH_Structure<IGH_Goo>();
                        tree.Append(item, newPath);
                        DA.SetDataTree(i, tree);
                    }
                    else
                    {
                        var tree = new GH_Structure<IGH_Goo>();
                        tree.Append(item, path);
                        DA.SetDataTree(i, tree);
                    }

                }
                if (access == PropertyAccess.List)
                {
                    var list = prop != null
                        ? prop.Data.Branches[0]
                        : new List<IGH_Goo>();

                    var path = DA.ParameterTargetPath(i);
                    int[] index = { DA.ParameterTargetIndex(0) };
                    var newPath = new GH_Path(path.Indices.Concat(index).ToArray());
                    var tree = new GH_Structure<IGH_Goo>();
                    tree.AppendRange(list, newPath);
                    DA.SetDataTree(i, tree);
                }
                if (access == PropertyAccess.Tree)
                {
                    var tree = prop != null
                        ? prop.Data
                        : Util.EmptyTree;
                    var basePath = DA.ParameterTargetPath(i);
                    var outTree = new GH_Structure<IGH_Goo>();
                    for (int j = 0; j < tree.PathCount; j++)
                    {
                        var branch = tree.Branches[j];
                        var path = tree.Paths[j];
                        int[] index = { DA.ParameterTargetIndex(0) };
                        var newPathIndices = basePath.Indices
                            .Concat(index)
                            .Concat(path.Indices)
                            .ToArray();
                        var newPath = new GH_Path(newPathIndices);
                        outTree.AppendRange(branch, newPath);
                    }
                    DA.SetDataTree(i, outTree);
                }
            }
        }



        protected override void AfterSolveInstance()
        {
            VariableParameterMaintenance();
            foreach (var p in Params.Output)
            {
                if (p is Param_ObjectivismOutput o)
                {
                    o.CommitNickName();
                }
            }
            accessChecker.ThrowWarnings();
            base.AfterSolveInstance();
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Output;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Output;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_ObjectivismOutput();
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            foreach (var param in Params.Output)
            {
                if (param.NickName == string.Empty)
                {
                    param.NickName = NextUnusedName();
                }
                if (param is Param_ObjectivismOutput outputParam)
                {
                    outputParam.AllPropertyNames = this.PropertyNames;
                }
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Recompute", UpdateObjectEventHandler);
            Menu_AppendItem(menu, "Full Explode", FullExplodeEventHandler);
            Menu_AppendItem(menu, "Graft all properties", DoNotGraftItemsEventHandler, true, GraftItems);
        }

        private void DoNotGraftItemsEventHandler(object sender, EventArgs e)
        {
            RecordUndoEvent("Get object properties graft mode");
            GraftItems = !GraftItems;
            this.Message = getGraftMessage();
            ExpireSolution(true);

        }

        private void FullExplodeEventHandler(object sender, EventArgs e)
        {
            RecordUndoEvent("Object full explode");
            var unusedNames = GetUnusedNames();
            foreach (var name in unusedNames)
            {
                var param = new Param_ObjectivismOutput();
                Params.RegisterOutputParam(param);
                param.ExpireSolution(false);
            }
            VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        private void UpdateObjectEventHandler(object sender, EventArgs e)
        {
            ExpireSolution(true);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.objexplode;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9605357b-fad0-4c44-8dda-1cd9ba685fbc"); }
        }

        public override bool Read(GH_IReader reader)
        {
            try
            {
                GraftItems = reader.GetBoolean("GraftItemsToggle");
                Message = getGraftMessage();
            }
            catch
            {
                GraftItems = true;
                Message = getGraftMessage();
            }
            return base.Read(reader);

        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("GraftItemsToggle", GraftItems);
            return base.Write(writer);
        }
    }
}
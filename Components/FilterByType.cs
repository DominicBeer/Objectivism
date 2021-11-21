﻿using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.Windows.Forms;

namespace Objectivism.Components
{
    public class FilterByType : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the FilterByType class.
        /// </summary>
        public FilterByType()
          : base("Filter By Type", "Filter",
              "Filter objects by their type name",
              "Sets", "Objectivism")
        {
        }

        private HashSet<string> TypeNames = new HashSet<string>();
        internal List<string> GetUnusedNames() => TypeNames.Except(Params.Output.Select(p => p.NickName)).ToList();
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
            pManager.AddGenericParameter("Object", "O", "Objectivism object to turn into tree", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            
        }


        private HashSet<string> GetTypeNames()
        {
            var input = (Param_GenericObject)this.Params.Input[0];
            var objs = input.PersistentDataCount != 0
                ? input.PersistentData.WhereIsType<GH_ObjectivismObject>().ToList()
                : input.VolatileData.AllData(false).WhereIsType<GH_ObjectivismObject>().ToList();
            var inputNames = new HashSet<string>(objs.Select(obj => obj.Value.TypeName));
            return inputNames;
        }

        protected override void BeforeSolveInstance()
        {
            this.TypeNames.Clear();
            base.BeforeSolveInstance();
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
            GH_ObjectivismObject obj;
            if (goo is GH_ObjectivismObject ghObj)
            {
                obj = ghObj;
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Can only filter ojects built with Objectivism");
                return;
            }
            TypeNames.Add(obj.Value.TypeName);

            foreach ((int i, var param) in Params.Output.Enumerate())
            {
                string name = param.NickName;
                if(obj.Value.TypeName == name)
                {
                    DA.SetData(i, obj);
                }
                else
                {
                    DA.SetData(i, null);
                }
            }
        }

        protected override void AfterSolveInstance()
        {
            VariableParameterMaintenance();
            foreach (var p in Params.Output)
            {
                if (p is Param_ObjectivismObjectTypeOutput o)
                {
                    o.CommitNickName();
                }
            }
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
            return new Param_ObjectivismObjectTypeOutput();
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
                if (param is Param_ObjectivismObjectTypeOutput outputParam)
                {
                    outputParam.AllPropertyNames = this.TypeNames;
                }
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Update", UpdateObjectEventHandler);
            Menu_AppendItem(menu, "Get All Types", GetAllTypesEventHandler);
        }


        private void GetAllTypesEventHandler(object sender, EventArgs e)
        {
            RecordUndoEvent("GetAllTypes");
            var unusedNames = GetUnusedNames();
            foreach (var name in unusedNames)
            {
                var param = new Param_ObjectivismObjectTypeOutput();
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
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("615e81f0-484b-4b43-91c4-1f0a211c200c"); }
        }
    }
}
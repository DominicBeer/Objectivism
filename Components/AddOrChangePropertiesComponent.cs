using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Objectivism.Util;
using static Objectivism.PropertyRetriever;
using Grasshopper.Kernel.Parameters;
namespace Objectivism
{
    public class AddOrChangePropertiesComponent : GH_Component,IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the ChangePropertiesComponent class.
        /// </summary>
        public AddOrChangePropertiesComponent()
          : base("Add Or Change Properties", "Add/Change",
              "Change the value of a particular property, or add a new property",
              "Sets", "Objectivism")
        {
        }

        private HashSet<string> PropertyNames = new HashSet<string>();
        internal List<string> GetUnusedNames() => PropertyNames.Except(Params.Input.Select(p => p.NickName).Skip(1)).ToList();
        internal string NextUnusedName()
        {
            var unusedNames = GetUnusedNames();
            return unusedNames.Count == 0
                ? defaultNickName + GH_ComponentParamServer.InventUniqueNickname(numbers, StrippedParamNames())
                : unusedNames[0];
        }

        private void UpdatePropertyNames()
        {
            PropertyNames.Clear();
            var data = this.Params.Input[0].VolatileData.AllData(true).ToList();
            foreach( var goo in data)
            {
                if (goo is GH_ObjectivismObject ghObj)
                {
                    var propNames = ghObj.Value.AllProperties;
                    PropertyNames.UnionWith(propNames);
                }
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "O", "Object to make changes to", GH_ParamAccess.item);

            var objParam = new Param_GenericObject();
            objParam.NickName = "O";
            objParam.Name = "Object";
            objParam.Description = "Object to modify";
            objParam.Access = GH_ParamAccess.item;
            objParam.ObjectChanged += ObjectWireChangedHandler;
            /*
            var param = new Param_ExtraObjectProperty();
            param.Name = "PropertyName";
            param.nickNameCache = defaultNickName + "1";
            param.NickName = param.nickNameCache;
            param.Description = description;
            pManager.AddParameter(param);*/
        }

        private void ObjectWireChangedHandler(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
        {
            if (e.Type == GH_ObjectEventType.Sources)
            {
                this.UpdatePropertyNames();
            };
        }

        private readonly string description = "Property to change in object, or add if the property does not exist. Param nickname must correspond to name of property to change/add";
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "O", "Modified object", GH_ParamAccess.item);
        }

        protected override void BeforeSolveInstance()
        {
            if(!JustOneTypeName())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "You are trying to change objects of different types in one component - proceed with caution");
            }
            this.UpdatePropertyNames();
            Params.Input.ForEach(CommitParamNames);
            base.BeforeSolveInstance();
        }

        private bool JustOneTypeName()
        {
            var typeName = "";
            bool firstIter = true;
            var data = this.Params.Input[0].VolatileData.AllData(true);
            foreach( var goo in data)
            {
                if (goo is GH_ObjectivismObject ghObj)
                {
                    var tn = ghObj.Value.TypeName;
                    if(firstIter)
                    {
                        typeName = tn;
                        firstIter = false;
                    }
                    else if (tn != typeName)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void CommitParamNames(IGH_Param param)
        {
            if (param is Param_ExtraObjectProperty p)
            {
                p.CommitNickName();
            }
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
                return;
            }

            var updates = new List<(string Name, ObjectProperty Property)>();

            for(int i = 1; i < Params.Input.Count; i++)
            {
                updates.Add(RetrieveProperties(DA, i, this));
            }
            (var newObj, var accessConflict) = obj.AddOrChangeProperties(updates);
            accessConflict.BroadcastConflicts(this);
            DA.SetData(0, new GH_ObjectivismObject(newObj));

        }


        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index != 0;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index != 0;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new Param_ExtraObjectProperty();
            param.Name = "PropertyToChange";
            param.nickNameCache = string.Empty;
            param.NickName = string.Empty;
            param.Description = description;
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            this.UpdatePropertyNames();
            for (int i = 1; i < Params.Input.Count; i++)
            {
                Params.Input[i].Optional = true;
            }
            foreach (var param in Params.Input)
            {
                if (param is Param_ExtraObjectProperty extraParam)
                {
                    extraParam.AllPropertyNames = this.PropertyNames;
                }
                if (param.NickName == string.Empty)
                {
                    param.NickName = NextUnusedName();
                }
                
            }
        }

        private List<string> StrippedParamNames()
        {
            var variableParams = this.Params.Input.ToList();
            return variableParams
                .Select(p => p.NickName)
                .Where(n => n.StartsWith(defaultNickName) && numbers.Contains(n.ToCharArray()[defaultNickName.Length]))
                .Select(n => n.Replace(defaultNickName, ""))
                .ToList();
        }

        private readonly string defaultNickName = "Property";
        private readonly string numbers = "1234567890";

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Update Object", UpdateObjectEventHandler);
        }

        private void UpdateObjectEventHandler(object sender, EventArgs e)
        {
            this.Params.Input.ForEach(p => p.ExpireSolution(false));
            ExpireSolution(true);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.objchange;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e9a4a775-aaa2-489f-9d8e-6f8f7c1bc36b"); }
        }
    }
}
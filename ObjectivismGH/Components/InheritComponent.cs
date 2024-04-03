using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Objectivism.DataUtil;

namespace Objectivism
{
    public class InheritComponent : GH_Component, IGH_VariableParameterComponent, IHasMultipleTypes
    {
        /// <summary>
        /// Initializes a new instance of the ChangePropertiesComponent class.
        /// </summary>
        public InheritComponent()
          : base("Inherit", "NewTypeName",
              "Create a new object from a template. Add or change properties as required",
              "Sets", "Objectivism")
        {
            NickNameCache = this.NickName;
            this.IconDisplayMode = GH_IconDisplayMode.name;
            this.ObjectChanged += NickNameChangedEventHandler;
            this.Message = "Inherit";
        }

        private string NickNameCache;

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
            foreach (var goo in data)
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
            pManager.AddParameter(param); 
            */
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
            UpdateTypeNames();
            this.UpdatePropertyNames();
            Params.Input.ForEach(CommitParamNames);
            base.BeforeSolveInstance();
        }

        private HashSet<string> typeNames = new HashSet<string>();
        public HashSet<string> TypeNames => typeNames;

        private void UpdateTypeNames()
        {
            typeNames.Clear();
            typeNames.Add(this.NickName);
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

        private void CommitParamNames(IGH_Param param)
        {
            if (param is Param_ExtraObjectProperty p)
            {
                p.CommitNickName();
            }
        }

        public void NickNameChangedEventHandler(object sender, GH_ObjectChangedEventArgs args)
        {
            if (args.Type == GH_ObjectEventType.NickName)
            {
                if (NickName != NickNameCache)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type name (component nickname) changed but object not updated, right click on component and press \"Recompute\"");
                }
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var typeName = this.NickName;
            NickNameCache = NickName;

            if (!DA.TryGetObjectivsmObject(0, out var obj)) return;

            var updates = new List<(string Name, ObjectProperty Property)>();

            for (int i = 1; i < Params.Input.Count; i++)
            {
                updates.Add(RetrieveProperties(DA, i, this));
            }

            (var newObj, var accessConflict) = obj.AddOrChangeProperties(updates, typeName);
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
            Menu_AppendItem(menu, "Recompute", UpdateObjectEventHandler);
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
                return Resources.inherit;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6F1CED3F-D460-4339-A494-D1829342E2C3"); }
        }
    }
}
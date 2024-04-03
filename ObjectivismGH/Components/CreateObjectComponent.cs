using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Objectivism.DataUtil;


namespace Objectivism
{

    internal interface IHasMultipleTypes
    {
        HashSet<string> TypeNames { get; }
    }
    public class CreateObjectComponent : GH_Component, IGH_VariableParameterComponent
    {

        public CreateObjectComponent()
          : base("Create Object", "Object",
              "Encapsulate multiple kinds of data within a single object",
              "Sets", "Objectivism")
        {
            NickNameCache = this.NickName;
            this.IconDisplayMode = GH_IconDisplayMode.name;
            this.ObjectChanged += NickNameChangedEventHandler;
        }

        private string NickNameCache;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            var param1 = new Param_NewObjectProperty();
            param1.NickName = String.Empty;
            param1.Access = GH_ParamAccess.item;
            pManager.AddParameter(param1);
            var param2 = new Param_NewObjectProperty();
            param2.NickName = String.Empty;
            param2.Access = GH_ParamAccess.item;
            pManager.AddParameter(param2);
            VariableParameterMaintenance();
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "O", "Object that is created", GH_ParamAccess.item);
        }

        protected override void BeforeSolveInstance()
        {
            Params.Input.ForEach(CommitParamNames);
            base.BeforeSolveInstance();
        }

        private void CommitParamNames(IGH_Param param)
        {
            if (param is Param_NewObjectProperty p)
            {
                p.CommitNickName();
            }
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var typeName = this.NickName;
            NickNameCache = NickName;
            var data = new List<(string Name, ObjectProperty Property)>();
            for (int i = 0; i < Params.Input.Count; i++)
            {
                data.Add(RetrieveProperties(DA, i, this));
            }
            var obj = new ObjectivismObject(data, typeName);
            var ghObj = new GH_ObjectivismObject(obj);
            DA.SetData(0, ghObj);
        }



        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new Param_NewObjectProperty();
            param.NickName = String.Empty;
            param.Access = GH_ParamAccess.item;
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            var dynamicParams = Params.Input.ToList();
            foreach (var param in dynamicParams)
            {
                param.Optional = true;
            }
            var emptyParams = dynamicParams.Where(p => p.NickName == String.Empty);
            foreach (var param in emptyParams)
            {
                var paramKey = GH_ComponentParamServer.InventUniqueNickname(numbers, StrippedParamNames());
                param.NickName = defaultNickName + paramKey;
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

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Recompute", UpdateObjectEventHandler);
        }

        private void UpdateObjectEventHandler(object sender, EventArgs e)
        {
            this.Params.Input.ForEach(p => p.ExpireSolution(false));
            ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.objcreate;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("312f8eb3-254f-4c22-aead-7918c6cc6699"); }
        }
    }
}

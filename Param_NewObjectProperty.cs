using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

namespace Objectivism
{
    public class Param_NewObjectProperty : Param_GenericObject
    {
        public override Guid ComponentGuid => new Guid("81320c17-4090-470d-b036-95005338c2b1");
        internal string nickNameCache = "";
        public override string TypeName => "Object Property Data";
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public Param_NewObjectProperty() : base()
        {

            Name = "Property";
            nickNameCache = "Prop";
            NickName = "Prop";
            Description = "Property for an Objectivsm Object ";
            ObjectChanged += NickNameChangedEventHandler;
        }



        internal void CommitNickName()
        {
            nickNameCache = NickName;
        }
        public void NickNameChangedEventHandler(object sender, GH_ObjectChangedEventArgs args)
        {
            if(args.Type == GH_ObjectEventType.NickName)
            {
                if(NickName != nickNameCache)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Property input name changed but object not updated, right click on component and press \"Update Object\"");
                }
                else
                {
                    this.ClearRuntimeMessages();
                }
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            bool isItem = Access == GH_ParamAccess.item;
            bool isList = Access == GH_ParamAccess.list;
            bool isTree = Access == GH_ParamAccess.tree;

            var itemButton = Menu_AppendItem(menu, "Item Access", ItemAccessEventHandler, true , isItem);
            var listButton = Menu_AppendItem(menu, "List Access", ListAccessEventHandler, true, isList);
            var treeButton = Menu_AppendItem(menu, "Tree Access", TreeAccessEventHandler, true, isTree);
        }

        public void ItemAccessEventHandler(object sender, EventArgs e)
        {
            if (Access != GH_ParamAccess.item)
            {
                RecordUndoEvent("Change access type");
                Access = GH_ParamAccess.item;
                ExpireSolution(true);
            }
        }
        public void ListAccessEventHandler(object sender, EventArgs e)
        {
            if (Access != GH_ParamAccess.list)
            {
                RecordUndoEvent("Change access type");
                Access = GH_ParamAccess.list;
                ExpireSolution(true);
            }
        }
        public void TreeAccessEventHandler(object sender, EventArgs e)
        {
            if (Access != GH_ParamAccess.tree)
            {
                RecordUndoEvent("Change access type");
                Access = GH_ParamAccess.tree;
                ExpireSolution(true);
            }
        }


    }

}

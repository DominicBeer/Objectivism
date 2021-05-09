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
    public class Param_OutputObjectProperty: Param_GenericObject
    {
        public override Guid ComponentGuid => new Guid("1b625488-3ec8-4189-8d14-6de1b0c9effd");
        internal string nickNameCache = "";
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        internal void CommitNickName() { this.nickNameCache = NickName; }
        public override string TypeName => "Object Property Data";
        internal HashSet<string> AllPropertyNames = new HashSet<string>();
        public Param_OutputObjectProperty() : base()
        {
            Name = "Property";
            nickNameCache = String.Empty;
            NickName = String.Empty;
            Description = "Retrieved Property ";
            Access = GH_ParamAccess.tree;
            ObjectChanged += NickNameChangedEventHandler;
        }

        public void NickNameChangedEventHandler(object sender, GH_ObjectChangedEventArgs args)
        {
            if (args.Type == GH_ObjectEventType.NickName)
            {
                if (NickName != nickNameCache)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Property input name changed but object not updated, right click on component and press \"Update\"");
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
            var button = Menu_AppendItem(menu, "Properties");
            var dropDownButtons = this.AllPropertyNames.Select(n => new ToolStripMenuItem(n, null, PropertyClickEventHandler)).ToArray();
            button.DropDownItems.AddRange(dropDownButtons);
            
        }

        private void PropertyClickEventHandler(object sender, EventArgs e)
        {
            RecordUndoEvent("Change property name");
            if(sender is ToolStripMenuItem button)
            {
                this.NickName = button.Text;
                var parent = this.Attributes.GetTopLevel.DocObject;
                parent.ExpireSolution(true);
            }
        }
    }
}

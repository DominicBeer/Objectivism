using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Objectivism.Forms;
using Objectivism.Parameters;

namespace Objectivism
{
    public class Param_ExtraObjectProperty : Param_GenericObject, IHasPreviewToggle //Item access, property retrieval
    {
        public override Guid ComponentGuid => new Guid("41412f8c-c2d9-45c7-83d0-bd04a10e14fa");
        internal string nickNameCache = "";
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override string TypeName => "Object Property Data";

        public bool PreviewOn { get; private set; } = true;

        internal HashSet<string> AllPropertyNames = new HashSet<string>();
        internal void CommitNickName() { this.nickNameCache = NickName; }
        public Param_ExtraObjectProperty() : base()
        {
            Name = "Extra Property";
            nickNameCache = String.Empty;
            NickName = String.Empty;
            Description = "Property to change/add to object";
            Access = GH_ParamAccess.item;
            ObjectChanged += NickNameChangedEventHandler;
        }

        public void NickNameChangedEventHandler(object sender, GH_ObjectChangedEventArgs args)
        {
            if (args.Type == GH_ObjectEventType.NickName)
            {
                if (NickName != nickNameCache)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Property input name changed but object not updated, right click on component and press \"Recompute\"");
                }
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {

            base.AppendAdditionalMenuItems(menu);

            Menu_AppendSeparator(menu);

            var recomputeButton = Menu_AppendItem(menu, "Recompute", RecomputeHandler);

            Menu_AppendSeparator(menu);

            var toggleButton = Menu_AppendItem(menu, "Preview Geometry", PreviewToggleHandler, true, PreviewOn);

            Menu_AppendSeparator(menu);

            bool isItem = Access == GH_ParamAccess.item;
            bool isList = Access == GH_ParamAccess.list;
            bool isTree = Access == GH_ParamAccess.tree;

            var itemButton = Menu_AppendItem(menu, "Item Access", ItemAccessEventHandler, true, isItem);
            var listButton = Menu_AppendItem(menu, "List Access", ListAccessEventHandler, true, isList);
            var treeButton = Menu_AppendItem(menu, "Tree Access", TreeAccessEventHandler, true, isTree);

            Menu_AppendSeparator(menu);

            var button = Menu_AppendItem(menu, "Properties");
            var dropDownButtons = this.AllPropertyNames.Select(n => new ToolStripMenuItem(n, null, PropertyClickEventHandler)).ToArray();
            button.DropDownItems.AddRange(dropDownButtons);

            Menu_AppendSeparator(menu);
            var changeButton = Menu_AppendItem(menu, "Change Property Name", LaunchChangeDialog, true);

        }

        private void PreviewToggleHandler(object sender, EventArgs e)
        {
            RecordUndoEvent("Change object preview type");
            PreviewOn = !PreviewOn;
            ExpireSolution(true);
        }

        private void RecomputeHandler(object sender, EventArgs e)
        {
            var parent = this.GetParentComponent();
            if (parent != null)
            {
                parent.Params.Input.ForEach(p => p.ExpireSolution(false));
                parent.ExpireSolution(true);
            }
        }

        //When it is updated to cope with multiple types on arrival

        private void LaunchChangeDialog(object sender, EventArgs e)
        {
            var parent = this.GetParentComponent();
            if (parent != null)
            {
                var comp = (IHasMultipleTypes)parent;
                var tn = comp.TypeNames.FirstOrDefault();
                var multiple = comp.TypeNames.Count() != 1;
                var form = new ChangePropertyNameForm(this.NickName, tn, this.OnPingDocument(), multiple);
                form.ShowDialog();
            }

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


        private void PropertyClickEventHandler(object sender, EventArgs e)
        {
            RecordUndoEvent("Change property name");
            if (sender is ToolStripMenuItem button)
            {
                this.NickName = button.Text;
                this.ExpireSolution(true);
            }
        }

        public override bool Read(GH_IReader reader)
        {
            try
            {
                PreviewOn = reader.GetBoolean("PreviewState");
            }
            catch
            {
                PreviewOn = true;
            }
            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("PreviewState", PreviewOn);
            return base.Write(writer);
        }
    }
}

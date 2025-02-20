﻿using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Objectivism.Forms;
using Objectivism.Parameters;
using System;
using System.Windows.Forms;

namespace Objectivism
{
    public class Param_NewObjectProperty : Param_GenericObject, IHasPreviewToggle
    {
        public override Guid ComponentGuid => new Guid("81320c17-4090-470d-b036-95005338c2b1");
        internal string nickNameCache = "";
        public override string TypeName => "Object Property Data";
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public bool PreviewOn { get; private set; } = true;
        public override string Name
        {
            get => NickName;
            set => NickName = value;
        }

        public Param_NewObjectProperty() : base()
        {
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
            if (args.Type == GH_ObjectEventType.NickName)
            {
                if (NickName != nickNameCache)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Property input name changed but object not updated, right click on component and press \"Recompute\"");
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

        private void LaunchChangeDialog(object sender, EventArgs e)
        {
            var parent = this.GetParentComponent();
            if (parent != null)
            {
                var typeName = this.GetParentComponent().NickName;
                var form = new ChangePropertyNameForm(this.NickName, typeName, this.OnPingDocument(), false);
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

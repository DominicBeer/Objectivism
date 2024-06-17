using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Objectivism.Components;
using Objectivism.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism.Parameters
{
    public class Param_ExtraObjectProperty : Param_GenericObject, IHasPreviewToggle //Item access, property retrieval
    {
        internal HashSet<string> AllPropertyNames = new HashSet<string>();
        internal string nickNameCache = "";

        public Param_ExtraObjectProperty()
        {
            this.Name = "Extra Property";
            this.nickNameCache = string.Empty;
            this.NickName = string.Empty;
            this.Description = "Property to change/add to object";
            this.Access = GH_ParamAccess.item;
            this.ObjectChanged += this.NickNameChangedEventHandler;
        }

        public override Guid ComponentGuid => new Guid( "41412f8c-c2d9-45c7-83d0-bd04a10e14fa" );
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override string TypeName => "Object Property Data";

        public bool PreviewOn { get; private set; } = true;
        internal void CommitNickName() => this.nickNameCache = this.NickName;

        public void NickNameChangedEventHandler( object sender, GH_ObjectChangedEventArgs args )
        {
            if ( args.Type == GH_ObjectEventType.NickName )
            {
                if ( this.NickName != this.nickNameCache )
                {
                    this.AddRuntimeMessage( GH_RuntimeMessageLevel.Warning,
                        "Property input name changed but object not updated, right click on component and press \"Recompute\"" );
                }
            }
        }

        public override void AppendAdditionalMenuItems( ToolStripDropDown menu )
        {
            base.AppendAdditionalMenuItems( menu );

            Menu_AppendSeparator( menu );

            var recomputeButton = Menu_AppendItem( menu, "Recompute", this.RecomputeHandler );

            Menu_AppendSeparator( menu );

            var toggleButton =
                Menu_AppendItem( menu, "Preview Geometry", this.PreviewToggleHandler, true, this.PreviewOn );

            Menu_AppendSeparator( menu );

            var isItem = this.Access == GH_ParamAccess.item;
            var isList = this.Access == GH_ParamAccess.list;
            var isTree = this.Access == GH_ParamAccess.tree;

            var itemButton = Menu_AppendItem( menu, "Item Access", this.ItemAccessEventHandler, true, isItem );
            var listButton = Menu_AppendItem( menu, "List Access", this.ListAccessEventHandler, true, isList );
            var treeButton = Menu_AppendItem( menu, "Tree Access", this.TreeAccessEventHandler, true, isTree );

            Menu_AppendSeparator( menu );

            var button = Menu_AppendItem( menu, "Properties" );
            var dropDownButtons = this.AllPropertyNames
                .Select( n => new ToolStripMenuItem( n, null, this.PropertyClickEventHandler ) ).ToArray();
            button.DropDownItems.AddRange( dropDownButtons );

            Menu_AppendSeparator( menu );
            var changeButton = Menu_AppendItem( menu, "Change Property Name", this.LaunchChangeDialog, true );
        }

        private void PreviewToggleHandler( object sender, EventArgs e )
        {
            this.RecordUndoEvent( "Change object preview type" );
            this.PreviewOn = !this.PreviewOn;
            this.ExpireSolution( true );
        }

        private void RecomputeHandler( object sender, EventArgs e )
        {
            var parent = this.GetParentComponent();
            if ( parent != null )
            {
                parent.Params.Input.ForEach( p => p.ExpireSolution( false ) );
                parent.ExpireSolution( true );
            }
        }

        //When it is updated to cope with multiple types on arrival

        private void LaunchChangeDialog( object sender, EventArgs e )
        {
            var parent = this.GetParentComponent();
            if ( parent != null )
            {
                var comp = (IHasMultipleTypes) parent;
                var tn = comp.TypeNames.FirstOrDefault();
                var multiple = comp.TypeNames.Count() != 1;
                var form = new ChangePropertyNameForm( this.NickName, tn, this.OnPingDocument(), multiple );
                form.ShowDialog();
            }
        }

        public void ItemAccessEventHandler( object sender, EventArgs e )
        {
            if ( this.Access != GH_ParamAccess.item )
            {
                this.RecordUndoEvent( "Change access type" );
                this.Access = GH_ParamAccess.item;
                this.ExpireSolution( true );
            }
        }

        public void ListAccessEventHandler( object sender, EventArgs e )
        {
            if ( this.Access != GH_ParamAccess.list )
            {
                this.RecordUndoEvent( "Change access type" );
                this.Access = GH_ParamAccess.list;
                this.ExpireSolution( true );
            }
        }

        public void TreeAccessEventHandler( object sender, EventArgs e )
        {
            if ( this.Access != GH_ParamAccess.tree )
            {
                this.RecordUndoEvent( "Change access type" );
                this.Access = GH_ParamAccess.tree;
                this.ExpireSolution( true );
            }
        }


        private void PropertyClickEventHandler( object sender, EventArgs e )
        {
            this.RecordUndoEvent( "Change property name" );
            if ( sender is ToolStripMenuItem button )
            {
                this.NickName = button.Text;
                this.ExpireSolution( true );
            }
        }

        public override bool Read( GH_IReader reader )
        {
            try
            {
                this.PreviewOn = reader.GetBoolean( "PreviewState" );
            }
            catch
            {
                this.PreviewOn = true;
            }

            return base.Read( reader );
        }

        public override bool Write( GH_IWriter writer )
        {
            writer.SetBoolean( "PreviewState", this.PreviewOn );
            return base.Write( writer );
        }
    }
}
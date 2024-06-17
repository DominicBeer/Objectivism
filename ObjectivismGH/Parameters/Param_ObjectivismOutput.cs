using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism
{
    public class Param_ObjectivismOutput : Param_GenericObject
    {
        internal HashSet<string> AllPropertyNames = new HashSet<string>();
        internal string nickNameCache = "";

        public Param_ObjectivismOutput()
        {
            this.Name = this.OutputType;
            this.nickNameCache = string.Empty;
            this.NickName = string.Empty;
            this.Description = $"Retrieved {this.OutputType} ";
            this.Access = GH_ParamAccess.tree;
            this.ObjectChanged += this.NickNameChangedEventHandler;
        }

        public override Guid ComponentGuid => new Guid( "1b625488-3ec8-4189-8d14-6de1b0c9effd" );
        protected virtual string OutputType => "Property";
        protected virtual string OutputTypePlural => "Properties";
        protected string OutputTyputLC => this.OutputType.ToLower();
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override string TypeName => $"Object {this.OutputType} Data";
        internal void CommitNickName() => this.nickNameCache = this.NickName;

        public void NickNameChangedEventHandler( object sender, GH_ObjectChangedEventArgs args )
        {
            if ( args.Type == GH_ObjectEventType.NickName )
            {
                if ( this.NickName != this.nickNameCache )
                {
                    this.AddRuntimeMessage( GH_RuntimeMessageLevel.Warning,
                        $"{this.OutputType} name changed but component not updated, right click on component and press \"Recompute\"" );
                }
                else
                {
                    this.ClearRuntimeMessages();
                }
            }
        }

        public override void AppendAdditionalMenuItems( ToolStripDropDown menu )
        {
            base.AppendAdditionalMenuItems( menu );

            Menu_AppendSeparator( menu );
            var recomputeButton = Menu_AppendItem( menu, "Recompute", this.RecomputeHandler );
            Menu_AppendSeparator( menu );
            var button = Menu_AppendItem( menu, this.OutputTypePlural );
            var dropDownButtons = this.AllPropertyNames
                .Select( n => new ToolStripMenuItem( n, null, this.PropertyClickEventHandler ) ).ToArray();
            button.DropDownItems.AddRange( dropDownButtons );
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

        private void PropertyClickEventHandler( object sender, EventArgs e )
        {
            this.RecordUndoEvent( $"Change {this.OutputTyputLC} name" );
            if ( sender is ToolStripMenuItem button )
            {
                this.NickName = button.Text;
                var parent = this.Attributes.GetTopLevel.DocObject;
                parent.ExpireSolution( true );
            }
        }
    }

    public class Param_ObjectivismObjectTypeOutput : Param_ObjectivismOutput
    {
        protected override string OutputType => "Type";
        protected override string OutputTypePlural => "Types";
        public override Guid ComponentGuid => new Guid( "55bf273b-08ac-4cd7-a3a2-27bb1228a58c" );
    }
}
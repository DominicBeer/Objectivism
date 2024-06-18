using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism.Parameters
{
    public class Param_ObjectivismOutput : Param_GenericObject
    {
        private readonly HashSet<string> _allPropertyNames = new HashSet<string>();
        private string _nickNameCache;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Param_ObjectivismOutput" /> class with default values.
        ///     Nick name and nick name cache are both set to <see cref="string.Empty" />.
        /// </summary>
        public Param_ObjectivismOutput()
        {
            // TODO: Review, remove virtual member calls. https://discourse.mcneel.com/t/api-param-genericobject-does-not-provide-suitable-constructors/184753
            base.Name = this.OutputType;
            this._nickNameCache = string.Empty;
            this.NickName = string.Empty;
            this.Description = $"Retrieved {this.OutputType} ";
            this.Access = GH_ParamAccess.tree;
            this.ObjectChanged += this.NickNameChangedEventHandler;
        }

        public new virtual string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        public override Guid ComponentGuid => new Guid( "1b625488-3ec8-4189-8d14-6de1b0c9effd" );

        protected virtual string OutputType => "Property";

        protected virtual string OutputTypePlural => "Properties";

        protected string OutputTyputLC => this.OutputType.ToLower();

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public override string TypeName => $"Object {this.OutputType} Data";

        internal void ReplaceAllPropertyNames( IEnumerable<string> names )
        {
            this._allPropertyNames.Clear();
            this._allPropertyNames.UnionWith( names );
        }

        internal void CommitNickName() => this._nickNameCache = this.NickName;

        public void NickNameChangedEventHandler( object sender, GH_ObjectChangedEventArgs args )
        {
            if ( args.Type == GH_ObjectEventType.NickName )
            {
                if ( this.NickName != this._nickNameCache )
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
            var dropDownButtons = this._allPropertyNames
                .Select( n => (ToolStripItem) new ToolStripMenuItem( n, null, this.PropertyClickEventHandler ) )
                .ToArray();
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
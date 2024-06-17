using Grasshopper.Kernel;
using Grasshopper.Kernel.Undo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism.Forms
{
    public partial class ChangePropertyNameForm : Form
    {
        private readonly GH_Document _doc;
        private readonly string _propName;
        private readonly string _typeName;
        private List<IGH_Param> _paramsToChange;
        private int _radioState;

        public ChangePropertyNameForm( string propertyName, string typeName, GH_Document ghDoc, bool multipleTypesOnly )
        {
            this.InitializeComponent();

            this._doc = ghDoc;
            this.WelcomeTextLabel.Text = multipleTypesOnly
                ? $"Change property name \"{propertyName}\" used in multiple types"
                : $"Change property name \"{propertyName}\" belonging to type \"{typeName}\"";
            this._propName = propertyName;
            this._typeName = typeName;

            if ( multipleTypesOnly )
            {
                this.ConnectedTypesButton.Checked = true;
                this.ThisTypeButton.Checked = false;
                this.ThisTypeButton.Enabled = false;
            }

            if ( multipleTypesOnly )
            {
                this.GetParamsOfConnectedTypes();
            }
            else
            {
                this.GetParamsOfThisType();
            }
        }

        private void SetRadio( int i )
        {
            if ( i != this._radioState )
            {
                this._radioState = i;
                if ( i == 0 )
                {
                    this.GetParamsOfThisType();
                }

                if ( i == 1 )
                {
                    this.GetParamsOfConnectedTypes();
                }

                if ( i == 2 )
                {
                    this.GetParamsOfAllTypes();
                }

                this.ThisTypeButton.Checked = i == 0;
                this.ConnectedTypesButton.Checked = i == 1;
                this.AllTypesButton.Checked = i == 2;
            }
        }

        private void GetParamsOfAllTypes()
        {
            var createParams = this._doc.Objects
                .Where( obj => obj is CreateObjectComponent c )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Input
                    .Where( p => p is Param_NewObjectProperty && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var changeParams = this._doc.Objects
                .Where( obj => obj is AddOrChangePropertiesComponent || obj is InheritComponent )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Input
                    .Where( p => p is Param_ExtraObjectProperty && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var propParams = this._doc.Objects
                .Where( obj => obj is GetPropertiesComponent c )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Output
                    .Where( p => p is Param_ObjectivismOutput && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var allParams = createParams.Concat( changeParams ).Concat( propParams );
            var count = allParams.Count();
            this._paramsToChange = allParams.ToList();
            this.InstancesLabel.Text = $"{count} instances found";
            this.Update();
        }

        private void GetParamsOfConnectedTypes()
        {
            var connectedTypes = new HashSet<string> { this._typeName };

            var typeComps =
                new Stack<IHasMultipleTypes>( this._doc.Objects
                    .Where( obj => obj is IHasMultipleTypes )
                    .Select( obj => (IHasMultipleTypes) obj ) );

            connectedTypes = this.FindAllConnectedTypes( connectedTypes, typeComps );

            var createParams = this._doc.Objects
                .Where( obj => obj is CreateObjectComponent c && connectedTypes.Contains( c.NickName ) )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Input
                    .Where( p => p is Param_NewObjectProperty && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var changeParams = this._doc.Objects
                .Where( obj =>
                    obj is AddOrChangePropertiesComponent || obj is InheritComponent )
                .Where( obj =>
                    obj is IHasMultipleTypes c
                    && c.TypeNames.Count != 0
                    && connectedTypes.Contains( c.TypeNames.First() ) )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Input
                    .Where( p => p is Param_ExtraObjectProperty && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var propParams = this._doc.Objects
                .Where( obj =>
                    obj is GetPropertiesComponent c
                    && c.TypeNames.Count != 0
                    && connectedTypes.Contains( c.TypeNames.First() ) )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Output
                    .Where( p => p is Param_ObjectivismOutput && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var allParams = createParams.Concat( changeParams ).Concat( propParams );
            var count = allParams.Count();
            this._paramsToChange = allParams.ToList();
            this.InstancesLabel.Text = $"{count} instances found";
            this.Update();
        }

        private HashSet<string> FindAllConnectedTypes( HashSet<string> connectedTypes, Stack<IHasMultipleTypes> stack )
        {
            if ( stack.Count() == 0 )
            {
                return connectedTypes;
            }

            var thisComp = stack.Pop();
            var intersection = false;
            foreach ( var t in thisComp.TypeNames )
            {
                if ( connectedTypes.Contains( t ) )
                {
                    intersection = true;
                    break;
                }
            }

            if ( intersection )
            {
                connectedTypes.UnionWith( thisComp.TypeNames );
            }

            return this.FindAllConnectedTypes( connectedTypes, stack );
        }

        public void GetParamsOfThisType()
        {
            var createParams = this._doc.Objects
                .Where( obj => obj is CreateObjectComponent c && c.NickName == this._typeName )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Input
                    .Where( p => p is Param_NewObjectProperty && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var changeParams = this._doc.Objects
                .Where( obj =>
                    obj is AddOrChangePropertiesComponent || obj is InheritComponent )
                .Where( obj =>
                    obj is IHasMultipleTypes c
                    && c.TypeNames.Count == 1
                    && c.TypeNames.First() == this._typeName )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Input
                    .Where( p => p is Param_ExtraObjectProperty && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var propParams = this._doc.Objects
                .Where( obj =>
                    obj is GetPropertiesComponent c
                    && c.TypeNames.Count == 1
                    && c.TypeNames.First() == this._typeName )
                .Select( obj => (IGH_Component) obj )
                .Select( c => c.Params.Output
                    .Where( p => p is Param_ObjectivismOutput && p.NickName == this._propName ) )
                .SelectMany( x => x );
            var allParams = createParams.Concat( changeParams ).Concat( propParams );
            var count = allParams.Count();
            this._paramsToChange = allParams.ToList();
            this.InstancesLabel.Text = $"{count} instances found";
            this.Update();
        }


        private void OkButton_Click( object sender, EventArgs e )
        {
            var newName = this.NewNameBox.Text.Trim();
            if ( newName == "" )
            {
                MessageBox.Show( "No new name entered, please enter a name" );
                return;
            }

            var undo = new GH_UndoRecord( "Rename property for doc" );

            foreach ( var p in this._paramsToChange )
            {
                var action = new ChangeNameAction( p, p.NickName, newName );
                undo.AddAction( action );
                p.NickName = newName;
                p.ExpireSolution( false );
            }

            this._paramsToChange.ForEach( p => p.Attributes.ExpireLayout() );
            this._paramsToChange.First().ExpireSolution( true );
            this._doc.UndoUtil.RecordEvent( undo );
            this.Close();
        }


        private void CancelButton_Click( object sender, EventArgs e ) => this.Close();

        private void ThisType_CheckedChanged( object sender, EventArgs e )
        {
            if ( this.ThisTypeButton.Checked )
            {
                this.SetRadio( 0 );
            }
        }

        private void ConnectedTypesButton_CheckedChanged( object sender, EventArgs e )
        {
            if ( this.ConnectedTypesButton.Checked )
            {
                this.SetRadio( 1 );
            }
        }

        private void AllTypesButton_CheckedChanged( object sender, EventArgs e )
        {
            if ( this.AllTypesButton.Checked )
            {
                this.SetRadio( 2 );
            }
        }

        private void Label1_Click( object sender, EventArgs e )
        {
        }
    }


    internal class ChangeNameAction : GH_UndoAction
    {
        private readonly string _newName;
        private readonly string _oldName;
        private readonly IGH_Param _param;

        public ChangeNameAction( IGH_Param param, string oldName, string newName )
        {
            this._param = param;
            this._oldName = oldName;
            this._newName = newName;
        }

        public override bool ExpiresSolution => true;

        protected override void Internal_Redo( GH_Document doc )
        {
            this._param.NickName = this._newName;
            this._param.Attributes.GetTopLevel.ExpireLayout();
            this._param.ExpireSolution( false );
        }

        protected override void Internal_Undo( GH_Document doc )
        {
            this._param.NickName = this._oldName;
            this._param.Attributes.GetTopLevel.ExpireLayout();
            this._param.ExpireSolution( false );
        }
    }
}
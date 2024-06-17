using Grasshopper.Kernel;
using Objectivism.ObjectClasses;
using Objectivism.Parameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Objectivism.Components.Utilities.ComponentExtensions;

namespace Objectivism.Components
{
    internal interface IHasMultipleTypes
    {
        HashSet<string> TypeNames { get; }
    }

    public class CreateObjectComponent : GH_Component, IGH_VariableParameterComponent
    {
        private const string _myDefaultNickname = "Object";
        private const string _defaultNickName = "Property";
        private const string _numbers = "1234567890";

        private string _nickNameCache;

        public CreateObjectComponent()
            : base( "Create Object", _myDefaultNickname,
                "Encapsulate multiple kinds of data within a single object",
                "Sets", "Objectivism" )
        {
            this._nickNameCache = _myDefaultNickname;
            this.IconDisplayMode = GH_IconDisplayMode.name;
            this.ObjectChanged += this.NickNameChangedEventHandler;
        }

        protected override Bitmap Icon => Resources.objcreate;

        public override Guid ComponentGuid => new Guid( "312f8eb3-254f-4c22-aead-7918c6cc6699" );


        public bool CanInsertParameter( GH_ParameterSide side, int index ) => side == GH_ParameterSide.Input;

        public bool CanRemoveParameter( GH_ParameterSide side, int index ) => side == GH_ParameterSide.Input;

        public IGH_Param CreateParameter( GH_ParameterSide side, int index )
        {
            var param = new Param_NewObjectProperty { NickName = string.Empty, Access = GH_ParamAccess.item };
            return param;
        }

        public bool DestroyParameter( GH_ParameterSide side, int index ) => true;

        public void VariableParameterMaintenance()
        {
            var dynamicParams = this.Params.Input.ToList();
            foreach ( var param in dynamicParams )
            {
                param.Optional = true;
            }

            var emptyParams = dynamicParams.Where( p => p.NickName == string.Empty );
            foreach ( var param in emptyParams )
            {
                // TODO: TG: Review. The use of StrippedParamNames() does not make sense to me.

                var paramKey = GH_ComponentParamServer.InventUniqueNickname( _numbers, this.StrippedParamNames() );
                param.NickName = _defaultNickName + paramKey;
            }
        }

        protected override void RegisterInputParams( GH_InputParamManager pManager )
        {
            var param1 = new Param_NewObjectProperty { NickName = string.Empty, Access = GH_ParamAccess.item };
            pManager.AddParameter( param1 );
            var param2 = new Param_NewObjectProperty { NickName = string.Empty, Access = GH_ParamAccess.item };
            pManager.AddParameter( param2 );
            this.VariableParameterMaintenance();
        }

        protected override void RegisterOutputParams( GH_OutputParamManager pManager ) =>
            pManager.AddGenericParameter( "Object", "O", "Object that is created", GH_ParamAccess.item );

        protected override void BeforeSolveInstance()
        {
            this.Params.Input.ForEach( this.CommitParamNames );
            base.BeforeSolveInstance();
        }

        private void CommitParamNames( IGH_Param param )
        {
            if ( param is Param_NewObjectProperty p )
            {
                p.CommitNickName();
            }
        }


        protected override void SolveInstance( IGH_DataAccess daObject )
        {
            var typeName = this.NickName;
            this._nickNameCache = this.NickName;
            var data = new List<(string Name, ObjectProperty Property)>();
            for ( var i = 0; i < this.Params.Input.Count; i++ )
            {
                data.Add( this.GetProperty( daObject, i ) );
            }

            var obj = new ObjectivismObject( data, typeName );
            var ghObj = new GH_ObjectivismObject( obj );
            daObject.SetData( 0, ghObj );
        }

        private List<string> StrippedParamNames()
        {
            var variableParams = this.Params.Input.ToList();
            return variableParams
                .Select( p => p.NickName )
                .Where( n =>
                    n.Length > _defaultNickName.Length
                    && char.IsDigit( n[_defaultNickName.Length] )
                    && n.StartsWith( _defaultNickName, StringComparison.Ordinal ) )
                .Select( n => n.Substring( _defaultNickName.Length ) )
                .ToList();
        }

        public void NickNameChangedEventHandler( object sender, GH_ObjectChangedEventArgs args )
        {
            if ( args.Type == GH_ObjectEventType.NickName )
            {
                if ( this.NickName != this._nickNameCache )
                {
                    this.AddRuntimeMessage( GH_RuntimeMessageLevel.Warning,
                        "Type name (component nickname) changed but object not updated, right click on component and press \"Recompute\"" );
                }
            }
        }

        public override void AppendAdditionalMenuItems( ToolStripDropDown menu ) =>
            Menu_AppendItem( menu, "Recompute", this.UpdateObjectEventHandler );

        private void UpdateObjectEventHandler( object sender, EventArgs e )
        {
            this.Params.Input.ForEach( p => p.ExpireSolution( false ) );
            this.ExpireSolution( true );
        }
    }
}
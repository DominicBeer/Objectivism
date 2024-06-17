using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Objectivism.Components.Utilities;
using Objectivism.ObjectClasses;
using Objectivism.Parameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism.Components
{
    public class AddOrChangePropertiesComponent : GH_Component, IGH_VariableParameterComponent, IHasMultipleTypes
    {
        private readonly string _defaultNickName = "Property";

        private readonly string _description =
            "Property to change in object, or add if the property does not exist. Param nickname must correspond to name of property to change/add";

        private readonly string _numbers = "1234567890";


        private readonly HashSet<string> _propertyNames = new HashSet<string>();

        /// <summary>
        ///     Initializes a new instance of the ChangePropertiesComponent class.
        /// </summary>
        public AddOrChangePropertiesComponent()
            : base( "Add Or Change Properties", "Add/Change",
                "Change the value of a particular property, or add a new property",
                "Sets", "Objectivism" )
        {
        }

        /// <summary>
        ///     Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.objchange;

        /// <summary>
        ///     Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid( "e9a4a775-aaa2-489f-9d8e-6f8f7c1bc36b" );


        public bool CanInsertParameter( GH_ParameterSide side, int index ) =>
            side == GH_ParameterSide.Input && index != 0;

        public bool CanRemoveParameter( GH_ParameterSide side, int index ) =>
            side == GH_ParameterSide.Input && index != 0;

        public IGH_Param CreateParameter( GH_ParameterSide side, int index )
        {
            var param = new Param_ExtraObjectProperty
            {
                Name = "PropertyToChange",
                nickNameCache = string.Empty,
                NickName = string.Empty,
                Description = this._description
            };
            return param;
        }

        public bool DestroyParameter( GH_ParameterSide side, int index ) => true;

        public void VariableParameterMaintenance()
        {
            this.UpdatePropertyNames();
            for ( var i = 1; i < this.Params.Input.Count; i++ )
            {
                this.Params.Input[i].Optional = true;
            }

            foreach ( var param in this.Params.Input )
            {
                if ( param is Param_ExtraObjectProperty extraParam )
                {
                    extraParam.AllPropertyNames = this._propertyNames;
                }

                if ( param.NickName == string.Empty )
                {
                    param.NickName = this.NextUnusedName();
                }
            }
        }

        public HashSet<string> TypeNames { get; } = new HashSet<string>();

        internal List<string> GetUnusedNames() =>
            this._propertyNames.Except( this.Params.Input.Select( p => p.NickName ).Skip( 1 ) ).ToList();

        internal string NextUnusedName()
        {
            // TODO: TG: Review. The use of StrippedParamNames() does not make sense to me.

            var unusedNames = this.GetUnusedNames();
            return unusedNames.Count == 0
                ? this._defaultNickName +
                  GH_ComponentParamServer.InventUniqueNickname( this._numbers, this.StrippedParamNames() )
                : unusedNames[0];
        }

        private void UpdatePropertyNames()
        {
            this._propertyNames.Clear();
            var data = this.Params.Input[0].VolatileData.AllData( true ).ToList();
            foreach ( var goo in data )
            {
                if ( goo is GH_ObjectivismObject ghObj )
                {
                    var propNames = ghObj.Value.AllProperties;
                    this._propertyNames.UnionWith( propNames );
                }
            }
        }

        /// <summary>
        ///     Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams( GH_InputParamManager pManager )
        {
            pManager.AddGenericParameter( "Object", "O", "Object to make changes to", GH_ParamAccess.item );

            var objParam = new Param_GenericObject
            {
                NickName = "O", Name = "Object", Description = "Object to modify", Access = GH_ParamAccess.item
            };
            objParam.ObjectChanged += this.ObjectWireChangedHandler;
            /*
            var param = new Param_ExtraObjectProperty();
            param.Name = "PropertyName";
            param.nickNameCache = defaultNickName + "1";
            param.NickName = param.nickNameCache;
            param.Description = description;
            pManager.AddParameter(param);
            */
        }

        private void ObjectWireChangedHandler( IGH_DocumentObject sender, GH_ObjectChangedEventArgs e )
        {
            if ( e.Type == GH_ObjectEventType.Sources )
            {
                this.UpdatePropertyNames();
            }
        }

        /// <summary>
        ///     Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams( GH_OutputParamManager pManager ) =>
            pManager.AddGenericParameter( "Object", "O", "Modified object", GH_ParamAccess.item );

        protected override void BeforeSolveInstance()
        {
            this.UpdateTypeNames();
            if ( !this.JustOneTypeName() )
            {
                this.AddRuntimeMessage( GH_RuntimeMessageLevel.Remark, "Mutliple types detected" );
            }

            this.UpdatePropertyNames();
            this.Params.Input.ForEach( this.CommitParamNames );
            base.BeforeSolveInstance();
        }

        private void UpdateTypeNames()
        {
            this.TypeNames.Clear();
            var data = this.Params.Input[0].VolatileData.AllData( true );
            foreach ( var goo in data )
            {
                if ( goo is GH_ObjectivismObject ghObj )
                {
                    var tn = ghObj.Value.TypeName;
                    this.TypeNames.Add( tn );
                }
            }
        }

        private bool JustOneTypeName() => this.TypeNames.Count <= 1;

        private void CommitParamNames( IGH_Param param )
        {
            if ( param is Param_ExtraObjectProperty p )
            {
                p.CommitNickName();
            }
        }

        /// <summary>
        ///     This is the method that actually does the work.
        /// </summary>
        /// <param name="daObject">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance( IGH_DataAccess daObject )
        {
            if ( !daObject.TryGetObjectivsmObject( 0, out var obj ) )
            {
                return;
            }

            var updates = new List<(string Name, ObjectProperty Property)>();

            for ( var i = 1; i < this.Params.Input.Count; i++ )
            {
                updates.Add( this.GetProperty( daObject, i ) );
            }

            var (newObj, accessConflict) = obj.AddOrChangeProperties( updates );
            accessConflict.BroadcastConflicts( this );
            daObject.SetData( 0, new GH_ObjectivismObject( newObj ) );
        }

        private List<string> StrippedParamNames()
        {
            var variableParams = this.Params.Input.ToList();
            return variableParams
                .Select( p => p.NickName )
                .Where( n =>
                    n.StartsWith( this._defaultNickName ) &&
                    this._numbers.Contains( n.ToCharArray()[this._defaultNickName.Length] ) )
                .Select( n => n.Replace( this._defaultNickName, "" ) )
                .ToList();
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
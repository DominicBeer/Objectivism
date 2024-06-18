using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Objectivism.ObjectClasses;
using Objectivism.Parameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism.Components
{
    [Obsolete( "Obsolete", true )]
    public class FilterByType : GH_Component, IGH_VariableParameterComponent
    {
        private readonly HashSet<string> _typeNames = new HashSet<string>();

        /// <summary>
        ///     Initializes a new instance of the FilterByType class.
        /// </summary>
        public FilterByType()
            : base( "Filter By Type Old", "Filter",
                "Filter objects by their type name",
                "Sets", "Objectivism" )
        {
        }

        public override bool Obsolete => true;
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        ///     Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.GroupByType;

        /// <summary>
        ///     Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid( "615e81f0-484b-4b43-91c4-1f0a211c200c" );

        public bool CanInsertParameter( GH_ParameterSide side, int index ) => side == GH_ParameterSide.Output;

        public bool CanRemoveParameter( GH_ParameterSide side, int index ) => side == GH_ParameterSide.Output;

        public IGH_Param CreateParameter( GH_ParameterSide side, int index ) => new Param_ObjectivismObjectTypeOutput();

        public bool DestroyParameter( GH_ParameterSide side, int index ) => true;

        public void VariableParameterMaintenance()
        {
            foreach ( var param in this.Params.Output )
            {
                if ( param.NickName == string.Empty )
                {
                    param.NickName = this.NextUnusedName();
                }

                if ( param is Param_ObjectivismObjectTypeOutput outputParam )
                {
                    outputParam.ReplaceAllPropertyNames( this._typeNames );
                }
            }
        }

        internal List<string> GetUnusedNames() =>
            this._typeNames.Except( this.Params.Output.Select( p => p.NickName ) ).ToList();

        internal string NextUnusedName()
        {
            var unusedNames = this.GetUnusedNames();
            return unusedNames.Count == 0
                ? "No properties to list"
                : unusedNames[0];
        }

        /// <summary>
        ///     Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams( GH_InputParamManager pManager ) =>
            pManager.AddGenericParameter( "Object", "O", "Objectivism object to turn into tree", GH_ParamAccess.item );

        /// <summary>
        ///     Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams( GH_OutputParamManager pManager )
        {
        }

        protected override void BeforeSolveInstance()
        {
            this._typeNames.Clear();
            base.BeforeSolveInstance();
        }

        /// <summary>
        ///     This is the method that actually does the work.
        /// </summary>
        /// <param name="daObject">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance( IGH_DataAccess daObject )
        {
            IGH_Goo goo = null;
            if ( !daObject.GetData( 0, ref goo ) )
            {
                return;
            }

            GH_ObjectivismObject obj;
            if ( goo is GH_ObjectivismObject ghObj )
            {
                obj = ghObj;
            }
            else
            {
                this.AddRuntimeMessage( GH_RuntimeMessageLevel.Error, "Can only filter ojects built with Objectivism" );
                return;
            }

            this._typeNames.Add( obj.Value.TypeName );

            foreach ( var (i, param) in this.Params.Output.Enumerate() )
            {
                var name = param.NickName;
                daObject.SetData( i, obj.Value.TypeName == name ? obj : null );
            }
        }

        protected override void AfterSolveInstance()
        {
            this.VariableParameterMaintenance();
            foreach ( var p in this.Params.Output )
            {
                if ( p is Param_ObjectivismObjectTypeOutput o )
                {
                    o.CommitNickName();
                }
            }

            base.AfterSolveInstance();
        }

        public override void AppendAdditionalMenuItems( ToolStripDropDown menu )
        {
            Menu_AppendSeparator( menu );
            Menu_AppendItem( menu, "Get All Types", this.GetAllTypesEventHandler );
        }


        private void GetAllTypesEventHandler( object sender, EventArgs e )
        {
            this.RecordUndoEvent( "GetAllTypes" );
            var unusedNames = this.GetUnusedNames();
            for ( var i = 0; i < unusedNames.Count; ++i )
            {
                var param = new Param_ObjectivismObjectTypeOutput();
                this.Params.RegisterOutputParam( param );
                param.ExpireSolution( false );
            }

            this.VariableParameterMaintenance();
            this.Params.OnParametersChanged();
            this.ExpireSolution( true );
        }
    }
}
﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism.Components
{
    public class FilterByTypeOBSOLETE : GH_Component, IGH_VariableParameterComponent
    {
        private readonly HashSet<string> _typeNames = new HashSet<string>();

        /// <summary>
        ///     Initializes a new instance of the FilterByType class.
        /// </summary>
        public FilterByTypeOBSOLETE()
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
                    outputParam.AllPropertyNames = this._typeNames;
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


        private HashSet<string> GetTypeNames()
        {
            var input = (Param_GenericObject) this.Params.Input[0];
            var objs = input.PersistentDataCount != 0
                ? input.PersistentData.WhereIsType<GH_ObjectivismObject>().ToList()
                : input.VolatileData.AllData( false ).WhereIsType<GH_ObjectivismObject>().ToList();
            var inputNames = new HashSet<string>( objs.Select( obj => obj.Value.TypeName ) );
            return inputNames;
        }

        protected override void BeforeSolveInstance()
        {
            this._typeNames.Clear();
            base.BeforeSolveInstance();
        }

        /// <summary>
        ///     This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance( IGH_DataAccess DA )
        {
            IGH_Goo goo = null;
            if ( !DA.GetData( 0, ref goo ) )
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
                if ( obj.Value.TypeName == name )
                {
                    DA.SetData( i, obj );
                }
                else
                {
                    DA.SetData( i, null );
                }
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
            foreach ( var name in unusedNames )
            {
                var param = new Param_ObjectivismObjectTypeOutput();
                this.Params.RegisterOutputParam( param );
                param.ExpireSolution( false );
            }

            this.VariableParameterMaintenance();
            this.Params.OnParametersChanged();
            this.ExpireSolution( true );
        }

        private void UpdateObjectEventHandler( object sender, EventArgs e ) => this.ExpireSolution( true );
    }
}
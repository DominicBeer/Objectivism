using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
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
    public class GetPropertiesComponent : GH_Component, IGH_VariableParameterComponent, IHasMultipleTypes
    {
        private readonly HashSet<string> _propertyNames = new HashSet<string>();
        private AccessChecker _accessChecker;
        private bool _graftItems;

        /// <summary>
        ///     Initializes a new instance of the GetPropertiesComponent class.
        /// </summary>
        public GetPropertiesComponent()
            : base( "Get Object Properties", "Object.",
                "Retrieve stored properties of an Objectivism object",
                "Sets", "Objectivism" )
        {
            this.IconDisplayMode = GH_IconDisplayMode.name;
            this.Message = this.GetGraftMessage();
        }

        /// <summary>
        ///     Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.objexplode;

        /// <summary>
        ///     Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid( "9605357b-fad0-4c44-8dda-1cd9ba685fbc" );

        public bool CanInsertParameter( GH_ParameterSide side, int index ) => side == GH_ParameterSide.Output;

        public bool CanRemoveParameter( GH_ParameterSide side, int index ) => side == GH_ParameterSide.Output;

        public IGH_Param CreateParameter( GH_ParameterSide side, int index ) => new Param_ObjectivismOutput();

        public bool DestroyParameter( GH_ParameterSide side, int index ) => true;

        public void VariableParameterMaintenance()
        {
            foreach ( var param in this.Params.Output )
            {
                if ( param.NickName == string.Empty )
                {
                    param.NickName = this.NextUnusedName();
                }

                if ( param is Param_ObjectivismOutput outputParam )
                {
                    outputParam.ReplaceAllPropertyNames( this._propertyNames );
                }
            }
        }

        public HashSet<string> TypeNames { get; } = new HashSet<string>();

        private string GetGraftMessage() =>
            this._graftItems
                ? "Graft all properties"
                : "Graft lists + trees";

        internal List<string> GetUnusedNames() =>
            this._propertyNames.Except( this.Params.Output.Select( p => p.NickName ) ).ToList();

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
            pManager.AddGenericParameter( "Object", "O", "Objectivism object to retrieve properties from",
                GH_ParamAccess.item );

        /// <summary>
        ///     Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams( GH_OutputParamManager pManager )
        {
        }

        protected override void BeforeSolveInstance()
        {
            this.UpdateTypeNames();
            if ( !this.JustOneTypeName() )
            {
                this.NickName = "MultipleTypes.";
            }
            else
            {
                this.NickName = this.GetTypeName() + ".";
            }

            this._propertyNames.Clear();
            this._accessChecker = new AccessChecker( this );
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

        private string GetTypeName()
        {
            var input = (Param_GenericObject) this.Params.Input[0];
            var allObjects = input.PersistentDataCount != 0
                ? input.PersistentData
                : input.VolatileData;
            if ( !allObjects.IsEmpty )
            {
                if ( allObjects is GH_Structure<IGH_Goo> tree )
                {
                    var first = tree.get_FirstItem( true );
                    if ( first != null )
                    {
                        if ( first is GH_ObjectivismObject obj )
                        {
                            return obj.Value.TypeName;
                        }
                    }
                }
            }

            return "NoValidType";
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

            ObjectivismObject obj;
            if ( goo is GH_ObjectivismObject ghObj )
            {
                obj = ghObj.Value;
            }
            else
            {
                this.AddRuntimeMessage( GH_RuntimeMessageLevel.Error,
                    "Can only get properties from ojects built with Objectivism" );
                return;
            }

            this._propertyNames.UnionWith( obj.AllProperties );


            foreach ( var (i, param) in this.Params.Output.Enumerate() )
            {
                var name = param.NickName;


                var prop = obj.GetProperty( name );
                PropertyAccess access;
                if ( prop != null )
                {
                    access = prop.Access;
                    this._accessChecker.AccessCheck( prop, name );
                }
                else
                {
                    access = this._accessChecker.BestGuessAccess( name );
                }


                if ( access == PropertyAccess.Item )
                {
                    var item = prop?.Data.get_FirstItem( false );
                    var path = daObject.ParameterTargetPath( i );
                    if ( this._graftItems )
                    {
                        int[] index = { daObject.ParameterTargetIndex( 0 ) };
                        var newPath = new GH_Path( path.Indices.Concat( index ).ToArray() );
                        var tree = new GH_Structure<IGH_Goo>();
                        tree.Append( item, newPath );
                        daObject.SetDataTree( i, tree );
                    }
                    else
                    {
                        var tree = new GH_Structure<IGH_Goo>();
                        tree.Append( item, path );
                        daObject.SetDataTree( i, tree );
                    }
                }

                if ( access == PropertyAccess.List )
                {
                    var list = prop != null
                        ? prop.Data.Branches[0]
                        : new List<IGH_Goo>();

                    var path = daObject.ParameterTargetPath( i );
                    int[] index = { daObject.ParameterTargetIndex( 0 ) };
                    var newPath = new GH_Path( path.Indices.Concat( index ).ToArray() );
                    var tree = new GH_Structure<IGH_Goo>();
                    tree.AppendRange( list, newPath );
                    daObject.SetDataTree( i, tree );
                }

                if ( access == PropertyAccess.Tree )
                {
                    var tree = prop != null
                        ? prop.Data
                        : Util.EmptyTree;
                    var basePath = daObject.ParameterTargetPath( i );
                    var outTree = new GH_Structure<IGH_Goo>();
                    for ( var j = 0; j < tree.PathCount; j++ )
                    {
                        var branch = tree.Branches[j];
                        var path = tree.Paths[j];
                        int[] index = { daObject.ParameterTargetIndex( 0 ) };
                        var newPathIndices = basePath.Indices
                            .Concat( index )
                            .Concat( path.Indices )
                            .ToArray();
                        var newPath = new GH_Path( newPathIndices );
                        outTree.AppendRange( branch, newPath );
                    }

                    daObject.SetDataTree( i, outTree );
                }
            }
        }


        protected override void AfterSolveInstance()
        {
            this.VariableParameterMaintenance();
            foreach ( var p in this.Params.Output )
            {
                if ( p is Param_ObjectivismOutput o )
                {
                    o.CommitNickName();
                }
            }

            this._accessChecker.ThrowWarnings();
            base.AfterSolveInstance();
        }

        public override void AppendAdditionalMenuItems( ToolStripDropDown menu )
        {
            Menu_AppendSeparator( menu );
            Menu_AppendItem( menu, "Recompute", this.UpdateObjectEventHandler );
            Menu_AppendItem( menu, "Full Explode", this.FullExplodeEventHandler );
            Menu_AppendItem( menu, "Graft all properties", this.DoNotGraftItemsEventHandler, true, this._graftItems );
        }

        private void DoNotGraftItemsEventHandler( object sender, EventArgs e )
        {
            this.RecordUndoEvent( "Get object properties graft mode" );
            this._graftItems = !this._graftItems;
            this.Message = this.GetGraftMessage();
            this.ExpireSolution( true );
        }

        private void FullExplodeEventHandler( object sender, EventArgs e )
        {
            this.RecordUndoEvent( "Object full explode" );
            var unusedNames = this.GetUnusedNames();
            for ( var i = 0; i < unusedNames.Count; ++i )
            {
                var param = new Param_ObjectivismOutput();
                this.Params.RegisterOutputParam( param );
                param.ExpireSolution( false );
            }

            this.VariableParameterMaintenance();
            this.Params.OnParametersChanged();
            this.ExpireSolution( true );
        }

        private void UpdateObjectEventHandler( object sender, EventArgs e ) => this.ExpireSolution( true );

        public override bool Read( GH_IReader reader )
        {
            try
            {
                this._graftItems = reader.GetBoolean( "GraftItemsToggle" );
                this.Message = this.GetGraftMessage();
            }
            catch
            {
                this._graftItems = true;
                this.Message = this.GetGraftMessage();
            }

            return base.Read( reader );
        }

        public override bool Write( GH_IWriter writer )
        {
            writer.SetBoolean( "GraftItemsToggle", this._graftItems );
            return base.Write( writer );
        }
    }
}
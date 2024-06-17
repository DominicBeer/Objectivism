using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Objectivism.Components
{
    public class ObjectToTree : GH_Component
    {
        private AccessChecker _accessChecker;
        private HashSet<string> _propNamesSet;

        private List<string> _propNamesStore;

        /// <summary>
        ///     Initializes a new instance of the ObjectToTree class.
        /// </summary>
        public ObjectToTree()
            : base( "Object To Tree", "ToTree",
                "Turn an objectivism object into a tree. Also returns a mirror tree of the property names",
                "Sets", "Objectivism" )
        {
        }

        /// <summary>
        ///     Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.ObjToTree;

        /// <summary>
        ///     Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid( "b717a0eb-95ae-4e64-8dbb-7ad39f6fcda0" );

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
            pManager.AddGenericParameter( "Tree", "T", "Object as tree, root branch paths correspond to properties",
                GH_ParamAccess.tree );
            pManager.AddTextParameter( "Property Names", "P", "Property names in tree of same structure as output tree",
                GH_ParamAccess.tree );
        }

        protected override void BeforeSolveInstance()
        {
            this._propNamesStore = new List<string>();
            this._propNamesSet = new HashSet<string>();
            this._accessChecker = new AccessChecker( this );
            base.BeforeSolveInstance();
        }

        private void AddToStoreIfRequired( string name )
        {
            if ( !this._propNamesSet.Contains( name ) )
            {
                this._propNamesStore.Add( name );
                this._propNamesSet.Add( name );
            }
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

            obj.AllProperties.ForEach( n => this.AddToStoreIfRequired( n ) );

            var outTree = new GH_Structure<IGH_Goo>();
            var nameTree = new GH_Structure<GH_String>();

            foreach ( var (i, name) in this._propNamesStore.Enumerate() )
            {
                var nameGoo = new GH_String( name );
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
                    var path = new List<int> { i };
                    path.AddRange( DA.ParameterTargetPath( 0 ).Indices );
                    path.Add( DA.ParameterTargetIndex( 0 ) );
                    var newPath = new GH_Path( path.ToArray() );
                    outTree.Append( item, newPath );
                    nameTree.Append( nameGoo, newPath );
                }

                if ( access == PropertyAccess.List )
                {
                    var list = prop != null
                        ? prop.Data.Branches[0]
                        : new List<IGH_Goo>();
                    var path = new List<int> { i };
                    path.AddRange( DA.ParameterTargetPath( 0 ).Indices );
                    path.Add( DA.ParameterTargetIndex( 0 ) );
                    var newPath = new GH_Path( path.ToArray() );
                    outTree.AppendRange( list, newPath );
                    nameTree.Append( nameGoo, newPath );
                }

                if ( access == PropertyAccess.Tree )
                {
                    var tree = prop != null
                        ? prop.Data
                        : Util.EmptyTree;
                    var path = new List<int> { i };
                    path.AddRange( DA.ParameterTargetPath( 0 ).Indices );
                    path.Add( DA.ParameterTargetIndex( 0 ) );
                    for ( var j = 0; j < tree.PathCount; j++ )
                    {
                        var branch = tree.Branches[j];
                        var subPath = tree.Paths[j].Indices;
                        var newPathIndices = path
                            .Concat( subPath )
                            .ToArray();
                        var newPath = new GH_Path( newPathIndices );
                        outTree.AppendRange( branch, newPath );
                        nameTree.Append( nameGoo, newPath );
                    }
                }
            }

            DA.SetDataTree( 0, outTree );
            DA.SetDataTree( 1, nameTree );
        }

        protected override void AfterSolveInstance()
        {
            this._accessChecker.ThrowWarnings();
            base.AfterSolveInstance();
        }
    }
}
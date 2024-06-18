using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Render;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Objectivism.ObjectClasses
{
    public class ObjectivismObject : IGH_PreviewData, IGH_RenderAwareData
    {
        private BoundingBox _boxCache;
        private bool _boxIsCached;
        private List<(string Name, ObjectProperty Property)> _properties;
        private Dictionary<string, int> _propertyGetter;

        public ObjectivismObject()
        {
            this.TypeName = "Objectivism Object";
        }


        public ObjectivismObject( List<(string Name, ObjectProperty Property)> props, string typeName )
        {
            this.TypeName = typeName;
            this._properties = props;
            this._propertyGetter = props
                .Select( ( p, i ) => (p.Name, i) )
                .ToDictionary( t => t.Name, t => t.i );
        }

        public ObjectivismObject( ObjectivismObject obj )
        {
            this.TypeName = obj.TypeName;
            this._properties = obj._properties.Select( pair => (pair.Name, new ObjectProperty( pair.Property )) )
                .ToList();
            this._propertyGetter = new Dictionary<string, int>( obj._propertyGetter );
        }

        public List<string> AllProperties => this._properties.Select( x => x.Name ).ToList();
        public string TypeName { get; private set; }

        public BoundingBox BoundingBox
        {
            get
            {
                if ( this._boxIsCached )
                {
                    return this._boxCache;
                }

                var boxes = this._properties
                    .Select( pair => pair.Property )
                    .Where( p => p.HasGeometry )
                    .Select( p => p.BoundingBox )
                    .ToList();
                this._boxCache = Util.UnionBoxes( boxes );
                this._boxIsCached = true;
                return this._boxCache;
            }
        }

        public BoundingBox ClippingBox => this.BoundingBox;

        public void DrawViewportWires( GH_PreviewWireArgs args ) =>
            this._properties.ForEach( prop => prop.Property.DrawViewportWires( args ) );

        public void DrawViewportMeshes( GH_PreviewMeshArgs args ) =>
            this._properties.ForEach( prop => prop.Property.DrawViewportMeshes( args ) );

        public void AppendRenderGeometry( GH_RenderArgs args, RenderMaterial material ) =>
            this._properties.ForEach( prop => prop.Property.AppendRenderGeometry( args, material ) );

        public bool HasProperty( string name ) => this._propertyGetter.ContainsKey( name );

        internal bool HasProperty( string name, PropertyAccess access )
        {
            if ( this.TryGetProperty( name, out var property ) )
            {
                return property.Access == access;
            }

            return false;
        }

        internal bool Implements( ObjectivismObject template ) =>
            template._properties.All( prop => this.HasProperty( prop.Name, prop.Property.Access ) );

        public bool TryGetProperty( string name, out ObjectProperty property )
        {
            property = null;

            if ( this._propertyGetter.TryGetValue( name, out var propertyIndex ) )
            {
                property = this._properties[propertyIndex].Property;
                return true;
            }

            return false;
        }

        public ObjectProperty GetProperty( string name )
        {
            if ( this._propertyGetter.TryGetValue( name, out var propertyIndex ) )
            {
                return this._properties[propertyIndex].Property;
            }

            return null;
        }


        internal (ObjectivismObject obj, AccessInfo conflicts) AddOrChangeProperties(
            List<(string name, ObjectProperty newProperty)> changes ) =>
            this.AddOrChangeProperties( changes, this.TypeName );

        internal (ObjectivismObject obj, AccessInfo conflicts) AddOrChangeProperties(
            List<(string name, ObjectProperty newProperty)> changes, string newName )
        {
            var newObj = new ObjectivismObject( this ) { TypeName = newName };
            var numberOfExistingProps = newObj._properties.Count;
            var accessInfo = new AccessInfo();
            foreach ( var (name, newProp) in changes )
            {
                if ( newObj._propertyGetter.ContainsKey( name ) )
                {
                    var currentAccess = newObj.GetProperty( name ).Access;
                    var newAccess = newProp.Access;
                    if ( newAccess != currentAccess )

                    {
                        accessInfo.AddConflict( name );
                    }

                    newObj._properties[this._propertyGetter[name]] = (name, newProp);
                }
                else
                {
                    newObj._properties.Add( (name, newProp) );
                    newObj._propertyGetter.Add( name, numberOfExistingProps );
                    numberOfExistingProps++;
                }
            }

            return (newObj, accessInfo);
        }

        internal ObjectivismObject AddProperties( List<(string name, ObjectProperty newProperty)> additions )
        {
            var newObj = new ObjectivismObject( this );
            var numberOfExistingProps = newObj._properties.Count;
            foreach ( var addition in additions )
            {
                newObj._properties.Add( addition );
                newObj._propertyGetter.Add( addition.name, numberOfExistingProps );
                numberOfExistingProps++;
            }

            return newObj;
        }

        public ObjectivismObject Transform( Transform xform )
        {
            var newObj = new ObjectivismObject( this );
            newObj._properties = newObj._properties
                .Select( p => (p.Name, p.Property.Transform( xform )) )
                .ToList();
            return newObj;
        }

        public ObjectivismObject Morph( SpaceMorph xmorph )
        {
            var newObj = new ObjectivismObject( this );
            newObj._properties = newObj._properties
                .Select( p => (p.Name, p.Property.Morph( xmorph )) )
                .ToList();
            return newObj;
        }

        public bool GH_Write( GH_IWriter writer )
        {
            writer.SetString( "ObjectTypeName", this.TypeName );

            writer.SetInt32( "NumberOfProperties", this._properties.Count );
            var nameWriter = writer.CreateChunk( "Names" );
            var i = 0;
            foreach ( var pair in this._properties )
            {
                nameWriter.SetString( "Name", i, pair.Name );
                var propWriter = nameWriter.CreateChunk( "Prop", i );
                pair.Property.WriteProp( propWriter );
                i++;
            }

            return true;
        }

        public bool GH_Read( GH_IReader reader )
        {
            try
            {
                this.TypeName = reader.GetString( "ObjectTypeName" );
                this._properties = new List<(string Name, ObjectProperty Property)>();
                this._propertyGetter = new Dictionary<string, int>();
                var count = reader.GetInt32( "NumberOfProperties" );
                var nameReader = reader.FindChunk( "Names" );
                for ( var i = 0; i < count; i++ )
                {
                    var name = nameReader.GetString( "Name", i );
                    var prop = new ObjectProperty();
                    var propReader = nameReader.FindChunk( "Prop", i );
                    prop.ReadProp( propReader );
                    this._properties.Add( (name, prop) );
                    this._propertyGetter.Add( name, i );
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal dynamic ToDynamic()
        {
            var eo = new ExpandoObject();
            var eoColl = (ICollection<KeyValuePair<string, object>>) eo;
            foreach ( var pair in this._properties )
            {
                var name = pair.Name.SpacesToUnderscores();
                var prop = pair.Property;

                // TODO: Review null handling.
                // I've added the following null test. The original code is in the `else` branch. The original code
                // checks prop.Access, then tests prop for null. If prop is null, prop.Access will throw. If
                // empty List or DataTree are required, PropertyAccess must be stored separately.

                if ( prop == null )
                {
                    eoColl.Add( new KeyValuePair<string, object>( name, null ) );
                }
                else
                {
                    if ( prop.Access == PropertyAccess.Item )
                    {
                        var item = prop != null
                            ? ProcessGoo( prop.Data.get_FirstItem( false ) )
                            : null;
                        eoColl.Add( new KeyValuePair<string, object>( name, item ) );
                    }

                    if ( prop.Access == PropertyAccess.List )
                    {
                        var list = prop != null
                            ? prop.Data.Branches[0].Select( ProcessGoo ).ToList()
                            : new List<object>();
                        eoColl.Add( new KeyValuePair<string, object>( name, list ) );
                    }

                    if ( prop.Access == PropertyAccess.Tree )
                    {
                        var tree = prop != null
                            ? prop.Data.ToDataTree( ProcessGoo )
                            : new DataTree<object>();
                        eoColl.Add( new KeyValuePair<string, object>( name, tree ) );
                    }
                }
            }

            dynamic eoDynamic = eo;
            return eoDynamic;
        }


        private static object ProcessGoo( IGH_Goo goo ) => goo?.ScriptVariable();

        internal class AccessInfo
        {
            private readonly List<string> _conflicts = new List<string>();

            public void AddConflict( string propertyName ) => this._conflicts.Add( propertyName );

            public void BroadcastConflicts( GH_Component comp ) => this._conflicts.ForEach( conflict =>
                comp.AddRuntimeMessage( GH_RuntimeMessageLevel.Warning, $"{conflict} has its access level changed" ) );
        }
    }
}
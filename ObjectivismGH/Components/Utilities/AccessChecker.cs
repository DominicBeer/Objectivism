using Grasshopper.Kernel;
using Objectivism.ObjectClasses;
using System.Collections.Generic;

namespace Objectivism.Components.Utilities
{
    internal class AccessChecker
    {
        private readonly Dictionary<string, PropertyAccess> _accessRecorder;

        private readonly IGH_Component _hostRef;
        private readonly HashSet<string> _warningsToThrow;

        public AccessChecker( IGH_Component @this )
        {
            this._accessRecorder = new Dictionary<string, PropertyAccess>();
            this._warningsToThrow = new HashSet<string>();
            this._hostRef = @this;
        }

        public void AccessCheck( ObjectProperty prop, string name )
        {
            if ( this._accessRecorder.ContainsKey( name ) )
            {
                if ( this._accessRecorder[name] != prop.Access )
                {
                    this._warningsToThrow.Add( name );
                }
            }
            else
            {
                this._accessRecorder.Add( name, prop.Access );
            }
        }

        public void ThrowWarnings()
        {
            foreach ( var name in this._warningsToThrow )
            {
                this._hostRef.AddRuntimeMessage( GH_RuntimeMessageLevel.Warning,
                    $"Access not consistent for {name} property. Output data tree may be messy and not consistent" );
            }
        }

        public PropertyAccess BestGuessAccess( string name )
        {
            if ( this._accessRecorder.ContainsKey( name ) )
            {
                return this._accessRecorder[name];
            }

            try
            {
                var data = this._hostRef.Params.Input[0].VolatileData.AllData( true );
                foreach ( var goo in data )
                {
                    if ( goo is GH_ObjectivismObject ghObj )
                    {
                        if ( ghObj.Value.HasProperty( name ) )
                        {
                            return ghObj.Value.GetProperty( name ).Access;
                        }
                    }
                }

                return PropertyAccess.Item;
            }
            catch
            {
                return PropertyAccess.Item;
            }
        }
    }
}
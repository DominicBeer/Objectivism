using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Objectivism.ObjectClasses;

namespace Objectivism.Components.Utilities
{
    internal static class DataAccessExtensions
    {
        public static bool TryGetObjectivsmObject( this IGH_DataAccess daObject, int paramIndex,
            out ObjectivismObject obj )
        {
            obj = null;
            IGH_Goo goo = null;
            if ( !daObject.GetData( paramIndex, ref goo ) )
            {
                return false;
            }

            if ( goo is GH_ObjectivismObject ghObj )
            {
                obj = ghObj.Value;
                return true;
            }

            return false;
        }
    }
}
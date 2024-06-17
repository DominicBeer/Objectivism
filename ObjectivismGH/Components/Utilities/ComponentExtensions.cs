using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Objectivism.ObjectClasses;
using Objectivism.Parameters;
using System.Collections.Generic;

namespace Objectivism.Components.Utilities
{
    internal static class ComponentExtensions
    {
        public static (string Name, ObjectProperty Property) GetProperty( this IGH_Component component,
            IGH_DataAccess daObject, int paramIndex )
        {
            var previewOn = true;
            var param = component.Params.Input[paramIndex];

            if ( param is IHasPreviewToggle hasPreviewToggle )
            {
                previewOn = hasPreviewToggle.PreviewOn;
            }

            ObjectProperty prop;
            var name = param.NickName;
            if ( param.Access == GH_ParamAccess.item )
            {
                IGH_Goo item = null;
                if ( !daObject.GetData( paramIndex, ref item ) )
                {
                    component.AddRuntimeMessage( GH_RuntimeMessageLevel.Remark,
                        $"{name} has no input and has been assigned null data" );
                }

                prop = new ObjectProperty( item );
            }
            else if ( param.Access == GH_ParamAccess.list )
            {
                var items = new List<IGH_Goo>();
                daObject.GetDataList( paramIndex, items );
                prop = new ObjectProperty( items );
            }
            else //tree access
            {
                daObject.GetDataTree( paramIndex, out GH_Structure<IGH_Goo> itemTree );
                prop = new ObjectProperty( itemTree );
            }

            prop.PreviewOn = previewOn;
            return (name, prop);
        }
    }
}
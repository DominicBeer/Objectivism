using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Objectivism.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Objectivism
{
    static class PropertyRetriever
    {
        internal static (string Name, ObjectProperty Property) RetrieveProperties(IGH_DataAccess DA, int paramIndex, IGH_Component @this)
        {
            bool previewOn = true;
            var param = @this.Params.Input[paramIndex];
            if(param is IHasPreviewToggle hasPreviewToggle)
            {
                previewOn = hasPreviewToggle.PreviewOn;
            }

            ObjectProperty prop;
            var name = param.NickName;
            if (param.Access == GH_ParamAccess.item)
            {
                IGH_Goo item = null;
                if (!DA.GetData(paramIndex, ref item))
                {
                    @this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"{name} has no input and has been assigned null data");
                }
                prop = new ObjectProperty(item);
            }
            else if (param.Access == GH_ParamAccess.list)
            {
                var items = new List<IGH_Goo>();
                DA.GetDataList(paramIndex, items);
                prop = new ObjectProperty(items);
            }
            else //tree access
            {
                DA.GetDataTree(paramIndex, out GH_Structure<IGH_Goo> itemTree);
                prop = new ObjectProperty(itemTree);
            }
            prop.PreviewOn = previewOn;
            return (name, prop);
        }
    }
}

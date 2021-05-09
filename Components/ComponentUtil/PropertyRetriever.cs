using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Objectivism
{
    static class PropertyRetriever
    {
        internal static (string Name, ObjectProperty Property) RetrieveProperties(IGH_DataAccess DA, int paramIndex, IGH_Component @this)
        {
            var param = @this.Params.Input[paramIndex];
            ObjectProperty prop;
            var name = param.NickName;
            if (param.Access == GH_ParamAccess.item)
            {
                IGH_Goo item = null;
                if (!DA.GetData(paramIndex, ref item))
                {
                    @this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"{name} has no input and has been assigned null data");
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
            return (name, prop);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
namespace Objectivism
{
    class AccessChecker
    {
        private Dictionary<string, PropertyAccess> accessRecorder;
        private HashSet<string> warningsToThrow;

        private IGH_Component hostRef;

        public AccessChecker(IGH_Component @this)
        {
            this.accessRecorder = new Dictionary<string, PropertyAccess>();
            this.warningsToThrow = new HashSet<string>();
            this.hostRef = @this;
        }

        public void AccessCheck(ObjectProperty prop, string name)
        {
            if (accessRecorder.ContainsKey(name))
            {
                if (accessRecorder[name] != prop.Access)
                {
                    warningsToThrow.Add(name);
                }
            }
            else
            {
                accessRecorder.Add(name, prop.Access);
            }
        }

        public void ThrowWarnings()
        {
            foreach(var name in warningsToThrow)
            {
                hostRef.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Access not consistent for {name} property. Output data tree may be messy and not consistent");
            }
        }

        public PropertyAccess BestGuessAccess(string name)
        {
            if(accessRecorder.ContainsKey(name))
            {
                return accessRecorder[name];
            }
            else
            {

                try
                {
                    var data = hostRef.Params.Input[0].VolatileData.AllData(true);
                    foreach (var goo in data)
                    {
                        if (goo is GH_ObjectivismObject ghObj)
                        {
                            if (ghObj.Value.HasProperty(name))
                            {
                                return ghObj.Value.GetProperty(name).Access;
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
}

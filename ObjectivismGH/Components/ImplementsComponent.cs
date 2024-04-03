using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Objectivism.Components
{
    public class ImplementsComponent : GH_Component, IHasMultipleTypes
    {
        /// <summary>
        /// Initializes a new instance of the Implements class.
        /// </summary>
        public ImplementsComponent()
          : base("Implements", "Implements",
              "Tests if an object implements all the properties of a template object. An object implements the template if it has all the properties of the template, with name and access level matching",
              "Sets", "Objectivism")
        {
            TypeNames = new HashSet<string>();
        }

        protected override void BeforeSolveInstance()
        {
            TypeNames.Clear();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Template", "T", "Template object", GH_ParamAccess.item);
            pManager.AddGenericParameter("Object", "O", "Subject object", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Implements", "I", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.TryGetObjectivsmObject(0, out var template)) return;
            if (!DA.TryGetObjectivsmObject(1, out var subject)) return;

            TypeNames.Add(template.TypeName);
            TypeNames.Add(subject.TypeName);

            DA.SetData(0, subject.Implements(template));
        }

        protected override System.Drawing.Bitmap Icon => Resources.implements;

        public override Guid ComponentGuid => new Guid("B77AF544-BB27-4BFB-8032-09993C3EEE4C");

        public HashSet<string> TypeNames { get; private set; }
    }
}

using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Objectivism
{
    public class ObjectivismInfo : GH_AssemblyInfo
    {
        public override string Name => "Objectivism";

        public override Bitmap Icon =>
            //Return a 24x24 pixel bitmap to represent this GHA library.
            Resources.ObjectivismLogoSmall;

        public override string Description =>
            //Return a short string describing the purpose of this GHA library.
            "Objectivism allows you to create objects in Grasshopper, enabling better management of data";

        public override Guid Id => new Guid( "3bc8585d-aa6b-49b6-8c92-ff1f0e14b3b4" );

        public override string AuthorName =>
            //Return a string identifying you or your company.
            "Dominic Beer";

        public override string AuthorContact =>
            //Return a string representing your preferred contact details.
            "";
    }
}
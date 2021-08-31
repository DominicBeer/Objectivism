using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Objectivism
{
    public class ObjectivismInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Objectivism";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Resources.ObjectivismLogoSmall;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("3bc8585d-aa6b-49b6-8c92-ff1f0e14b3b4");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Dominic Beer";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}

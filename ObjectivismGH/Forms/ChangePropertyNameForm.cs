using Grasshopper.Kernel;
using Grasshopper.Kernel.Undo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Objectivism.Forms
{
    public partial class ChangePropertyNameForm : Form
    {
        private readonly GH_Document _doc;
        private List<IGH_Param> _paramsToChange;
        private readonly string _propName;
        private readonly string _typeName;
        private int radioState = 0;
        private void setRadio(int i)
        {
            if (i != radioState)
            {
                radioState = i;
                if (i == 0)
                {
                    GetParamsOfThisType();
                }
                if (i == 1)
                {
                    GetParamsOfConnectedTypes();
                }
                if (i == 2)
                {
                    GetParamsOfAllTypes();
                }
                ThisTypeButton.Checked = i == 0;
                ConnectedTypesButton.Checked = i == 1;
                AllTypesButton.Checked = i == 2;
            }

        }

        private void GetParamsOfAllTypes()
        {
            var createParams =
                _doc.Objects
                .Where(obj => obj is CreateObjectComponent c)
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Input
                    .Where(p => p is Param_NewObjectProperty && p.NickName == _propName))
                .SelectMany(x => x);
            var changeParams =
                _doc.Objects
                .Where(obj => obj is AddOrChangePropertiesComponent || obj is InheritComponent)
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Input
                    .Where(p => p is Param_ExtraObjectProperty && p.NickName == _propName))
                .SelectMany(x => x);
            var propParams =
                _doc.Objects
                .Where(obj => obj is GetPropertiesComponent c)
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Output
                    .Where(p => p is Param_ObjectivismOutput && p.NickName == _propName))
                .SelectMany(x => x);
            var allParams = createParams.Concat(changeParams).Concat(propParams);
            var count = allParams.Count();
            _paramsToChange = allParams.ToList();
            this.InstancesLabel.Text = $"{count} instances found";
            this.Update();

        }

        private void GetParamsOfConnectedTypes()
        {
            var connectedTypes = new HashSet<string> { _typeName };

            var typeComps =
                new Stack<IHasMultipleTypes>(
                    _doc.Objects
                    .Where(obj => obj is IHasMultipleTypes)
                    .Select(obj => (IHasMultipleTypes)obj));

            connectedTypes = FindAllConnectedTypes(connectedTypes, typeComps);

            var createParams =
                _doc.Objects
                .Where(obj => obj is CreateObjectComponent c && connectedTypes.Contains(c.NickName))
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Input
                    .Where(p => p is Param_NewObjectProperty && p.NickName == _propName))
                .SelectMany(x => x);
            var changeParams =
                _doc.Objects
                .Where(obj =>
                    obj is AddOrChangePropertiesComponent || obj is InheritComponent)
                .Where(obj =>
                    obj is IHasMultipleTypes c
                    && c.TypeNames.Count != 0
                    && connectedTypes.Contains(c.TypeNames.First()))
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Input
                    .Where(p => p is Param_ExtraObjectProperty && p.NickName == _propName))
                .SelectMany(x => x);
            var propParams =
                _doc.Objects
                .Where(obj =>
                    obj is GetPropertiesComponent c
                    && c.TypeNames.Count != 0
                    && connectedTypes.Contains(c.TypeNames.First()))
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Output
                    .Where(p => p is Param_ObjectivismOutput && p.NickName == _propName))
                .SelectMany(x => x);
            var allParams = createParams.Concat(changeParams).Concat(propParams);
            var count = allParams.Count();
            _paramsToChange = allParams.ToList();
            this.InstancesLabel.Text = $"{count} instances found";
            this.Update();
        }

        private HashSet<string> FindAllConnectedTypes(HashSet<string> connectedTypes, Stack<IHasMultipleTypes> stack)
        {
            if (stack.Count() == 0)
            {
                return connectedTypes;
            }
            var thisComp = stack.Pop();
            var intersection = false;
            foreach (var t in thisComp.TypeNames)
            {
                if (connectedTypes.Contains(t))
                {
                    intersection = true;
                    break;
                }
            }
            if (intersection)
            {
                connectedTypes.UnionWith(thisComp.TypeNames);
            }
            return FindAllConnectedTypes(connectedTypes, stack);
        }

        public ChangePropertyNameForm(string propertyName, string typeName, GH_Document ghDoc, bool multipleTypesOnly)
        {
            InitializeComponent();

            _doc = ghDoc;
            WelcomeTextLabel.Text = multipleTypesOnly
                ? $"Change property name \"{propertyName}\" used in multiple types"
                : $"Change property name \"{propertyName}\" belonging to type \"{typeName}\"";
            _propName = propertyName;
            _typeName = typeName;

            if (multipleTypesOnly)
            {
                this.ConnectedTypesButton.Checked = true;
                this.ThisTypeButton.Checked = false;
                this.ThisTypeButton.Enabled = false;
            }
            if (multipleTypesOnly)
            {
                GetParamsOfConnectedTypes();
            }
            else
            {
                GetParamsOfThisType();
            }



        }

        public void GetParamsOfThisType()
        {
            var createParams =
                _doc.Objects
                .Where(obj => obj is CreateObjectComponent c && c.NickName == _typeName)
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Input
                    .Where(p => p is Param_NewObjectProperty && p.NickName == _propName))
                .SelectMany(x => x);
            var changeParams =
                _doc.Objects
                .Where(obj =>
                    obj is AddOrChangePropertiesComponent || obj is InheritComponent)
                .Where(obj =>
                    obj is IHasMultipleTypes c
                    && c.TypeNames.Count == 1
                    && c.TypeNames.First() == _typeName)
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Input
                    .Where(p => p is Param_ExtraObjectProperty && p.NickName == _propName))
                .SelectMany(x => x);
            var propParams =
                _doc.Objects
                .Where(obj =>
                    obj is GetPropertiesComponent c
                    && c.TypeNames.Count == 1
                    && c.TypeNames.First() == _typeName)
                .Select(obj => (IGH_Component)obj)
                .Select(c => c.Params.Output
                    .Where(p => p is Param_ObjectivismOutput && p.NickName == _propName))
                .SelectMany(x => x);
            var allParams = createParams.Concat(changeParams).Concat(propParams);
            var count = allParams.Count();
            _paramsToChange = allParams.ToList();
            this.InstancesLabel.Text = $"{count} instances found";
            this.Update();

        }



        private void OkButton_Click(object sender, EventArgs e)
        {
            var newName = this.NewNameBox.Text.Trim();
            if (newName == "")
            {
                MessageBox.Show("No new name entered, please enter a name");
                return;
            }
            var undo = new GH_UndoRecord("Rename property for doc");

            foreach (var p in _paramsToChange)
            {
                var action = new ChangeNameAction(p, p.NickName, newName);
                undo.AddAction(action);
                p.NickName = newName;
                p.ExpireSolution(false);
            }
            _paramsToChange.ForEach(p => p.Attributes.ExpireLayout());
            _paramsToChange.First().ExpireSolution(true);
            _doc.UndoUtil.RecordEvent(undo);
            this.Close();
        }



        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ThisType_CheckedChanged(object sender, EventArgs e)
        {
            if (ThisTypeButton.Checked == true)
                setRadio(0);
        }

        private void ConnectedTypesButton_CheckedChanged(object sender, EventArgs e)
        {
            if (ConnectedTypesButton.Checked == true)
                setRadio(1);
        }

        private void AllTypesButton_CheckedChanged(object sender, EventArgs e)
        {
            if (AllTypesButton.Checked == true)
                setRadio(2);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }


    class ChangeNameAction : GH_UndoAction
    {
        private readonly IGH_Param param;
        private readonly string oldName;
        private readonly string newName;
        public override bool ExpiresSolution => true;
        protected override void Internal_Redo(GH_Document doc)
        {
            param.NickName = newName;
            param.Attributes.GetTopLevel.ExpireLayout();
            param.ExpireSolution(false);
        }

        protected override void Internal_Undo(GH_Document doc)
        {
            param.NickName = oldName;
            param.Attributes.GetTopLevel.ExpireLayout();
            param.ExpireSolution(false);
        }

        public ChangeNameAction(IGH_Param param, string oldName, string newName)
        {
            this.param = param;
            this.oldName = oldName;
            this.newName = newName;
        }
    }


}

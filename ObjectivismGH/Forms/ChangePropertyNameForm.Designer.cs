namespace Objectivism.Forms
{
    partial class ChangePropertyNameForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OkButton = new System.Windows.Forms.Button();
            this.FormCancelButton = new System.Windows.Forms.Button();
            this.WelcomeTextLabel = new System.Windows.Forms.Label();
            this.InstancesLabel = new System.Windows.Forms.Label();
            this.NewNameLabel = new System.Windows.Forms.Label();
            this.NewNameBox = new System.Windows.Forms.TextBox();
            this.ThisTypeButton = new System.Windows.Forms.RadioButton();
            this.ConnectedTypesButton = new System.Windows.Forms.RadioButton();
            this.AllTypesButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(590, 317);
            this.OkButton.Margin = new System.Windows.Forms.Padding(4);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(213, 42);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "Ok";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // FormCancelButton
            // 
            this.FormCancelButton.Location = new System.Drawing.Point(369, 317);
            this.FormCancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.FormCancelButton.Name = "FormCancelButton";
            this.FormCancelButton.Size = new System.Drawing.Size(213, 42);
            this.FormCancelButton.TabIndex = 1;
            this.FormCancelButton.Text = "Cancel";
            this.FormCancelButton.UseVisualStyleBackColor = true;
            this.FormCancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // WelcomeTextLabel
            // 
            this.WelcomeTextLabel.AutoSize = true;
            this.WelcomeTextLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WelcomeTextLabel.Location = new System.Drawing.Point(21, 23);
            this.WelcomeTextLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WelcomeTextLabel.Name = "WelcomeTextLabel";
            this.WelcomeTextLabel.Size = new System.Drawing.Size(143, 20);
            this.WelcomeTextLabel.TabIndex = 2;
            this.WelcomeTextLabel.Text = "WELCOME TEXT";
            // 
            // InstancesLabel
            // 
            this.InstancesLabel.AutoSize = true;
            this.InstancesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InstancesLabel.Location = new System.Drawing.Point(21, 220);
            this.InstancesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.InstancesLabel.Name = "InstancesLabel";
            this.InstancesLabel.Size = new System.Drawing.Size(140, 20);
            this.InstancesLabel.TabIndex = 2;
            this.InstancesLabel.Text = "x instances found";
            // 
            // NewNameLabel
            // 
            this.NewNameLabel.AutoSize = true;
            this.NewNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewNameLabel.Location = new System.Drawing.Point(22, 263);
            this.NewNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NewNameLabel.Name = "NewNameLabel";
            this.NewNameLabel.Size = new System.Drawing.Size(103, 20);
            this.NewNameLabel.TabIndex = 3;
            this.NewNameLabel.Text = "New name : ";
            // 
            // NewNameBox
            // 
            this.NewNameBox.Location = new System.Drawing.Point(157, 260);
            this.NewNameBox.Name = "NewNameBox";
            this.NewNameBox.Size = new System.Drawing.Size(646, 26);
            this.NewNameBox.TabIndex = 4;
            // 
            // ThisTypeButton
            // 
            this.ThisTypeButton.AutoSize = true;
            this.ThisTypeButton.Checked = true;
            this.ThisTypeButton.Location = new System.Drawing.Point(26, 108);
            this.ThisTypeButton.Name = "ThisTypeButton";
            this.ThisTypeButton.Size = new System.Drawing.Size(130, 24);
            this.ThisTypeButton.TabIndex = 5;
            this.ThisTypeButton.TabStop = true;
            this.ThisTypeButton.Text = "Just this type";
            this.ThisTypeButton.UseVisualStyleBackColor = true;
            this.ThisTypeButton.CheckedChanged += new System.EventHandler(this.ThisType_CheckedChanged);
            // 
            // ConnectedTypesButton
            // 
            this.ConnectedTypesButton.AutoSize = true;
            this.ConnectedTypesButton.Location = new System.Drawing.Point(26, 143);
            this.ConnectedTypesButton.Name = "ConnectedTypesButton";
            this.ConnectedTypesButton.Size = new System.Drawing.Size(257, 24);
            this.ConnectedTypesButton.TabIndex = 6;
            this.ConnectedTypesButton.TabStop = true;
            this.ConnectedTypesButton.Text = "This type and connected types";
            this.ConnectedTypesButton.UseVisualStyleBackColor = true;
            this.ConnectedTypesButton.CheckedChanged += new System.EventHandler(this.ConnectedTypesButton_CheckedChanged);
            // 
            // AllTypesButton
            // 
            this.AllTypesButton.AutoSize = true;
            this.AllTypesButton.Location = new System.Drawing.Point(26, 178);
            this.AllTypesButton.Name = "AllTypesButton";
            this.AllTypesButton.Size = new System.Drawing.Size(94, 24);
            this.AllTypesButton.TabIndex = 7;
            this.AllTypesButton.TabStop = true;
            this.AllTypesButton.Text = "All types";
            this.AllTypesButton.UseVisualStyleBackColor = true;
            this.AllTypesButton.CheckedChanged += new System.EventHandler(this.AllTypesButton_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(21, 66);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(438, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Change this property name for components operating on: ";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // ChangePropertyNameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(843, 398);
            this.ControlBox = false;
            this.Controls.Add(this.AllTypesButton);
            this.Controls.Add(this.ConnectedTypesButton);
            this.Controls.Add(this.ThisTypeButton);
            this.Controls.Add(this.NewNameBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NewNameLabel);
            this.Controls.Add(this.InstancesLabel);
            this.Controls.Add(this.WelcomeTextLabel);
            this.Controls.Add(this.FormCancelButton);
            this.Controls.Add(this.OkButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangePropertyNameForm";
            this.Text = "Change Property Name";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button FormCancelButton;
        private System.Windows.Forms.Label WelcomeTextLabel;
        private System.Windows.Forms.Label InstancesLabel;
        private System.Windows.Forms.Label NewNameLabel;
        private System.Windows.Forms.TextBox NewNameBox;
        private System.Windows.Forms.RadioButton ThisTypeButton;
        private System.Windows.Forms.RadioButton ConnectedTypesButton;
        private System.Windows.Forms.RadioButton AllTypesButton;
        private System.Windows.Forms.Label label1;
    }
}
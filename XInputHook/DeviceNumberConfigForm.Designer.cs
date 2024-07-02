namespace XInputHook {
    partial class DeviceNumberConfigForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceNumberConfigForm));
            keyboardsListBox = new ListBox();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            keyboardUpButton = new Button();
            keyboardDownButton = new Button();
            mouseDownButton = new Button();
            mouseUpButton = new Button();
            miceListBox = new ListBox();
            label1 = new Label();
            label2 = new Label();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // keyboardsListBox
            // 
            keyboardsListBox.FormattingEnabled = true;
            keyboardsListBox.ItemHeight = 15;
            keyboardsListBox.Location = new Point(11, 32);
            keyboardsListBox.Name = "keyboardsListBox";
            keyboardsListBox.Size = new Size(302, 154);
            keyboardsListBox.TabIndex = 0;
            keyboardsListBox.SelectedIndexChanged += KeyboardSelectedIndexChanged;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 377);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(418, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(0, 17);
            // 
            // keyboardUpButton
            // 
            keyboardUpButton.Location = new Point(319, 82);
            keyboardUpButton.Name = "keyboardUpButton";
            keyboardUpButton.Size = new Size(89, 23);
            keyboardUpButton.TabIndex = 2;
            keyboardUpButton.Text = "Move Up";
            keyboardUpButton.UseVisualStyleBackColor = true;
            keyboardUpButton.Click += KeyboardUpButtonPressed;
            // 
            // keyboardDownButton
            // 
            keyboardDownButton.Location = new Point(319, 111);
            keyboardDownButton.Name = "keyboardDownButton";
            keyboardDownButton.Size = new Size(89, 23);
            keyboardDownButton.TabIndex = 3;
            keyboardDownButton.Text = "Move Down";
            keyboardDownButton.UseVisualStyleBackColor = true;
            keyboardDownButton.Click += KeyboardDownButtonPressed;
            // 
            // mouseDownButton
            // 
            mouseDownButton.Location = new Point(320, 291);
            mouseDownButton.Name = "mouseDownButton";
            mouseDownButton.Size = new Size(89, 23);
            mouseDownButton.TabIndex = 6;
            mouseDownButton.Text = "Move Down";
            mouseDownButton.UseVisualStyleBackColor = true;
            mouseDownButton.Click += MouseDownButtonPressed;
            // 
            // mouseUpButton
            // 
            mouseUpButton.Location = new Point(320, 262);
            mouseUpButton.Name = "mouseUpButton";
            mouseUpButton.Size = new Size(89, 23);
            mouseUpButton.TabIndex = 5;
            mouseUpButton.Text = "Move Up";
            mouseUpButton.UseVisualStyleBackColor = true;
            mouseUpButton.Click += MouseUpButtonPressed;
            // 
            // miceListBox
            // 
            miceListBox.FormattingEnabled = true;
            miceListBox.ItemHeight = 15;
            miceListBox.Location = new Point(12, 212);
            miceListBox.Name = "miceListBox";
            miceListBox.Size = new Size(302, 154);
            miceListBox.TabIndex = 4;
            miceListBox.SelectedIndexChanged += MouseSelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 14);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 7;
            label1.Text = "Keyboards";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 194);
            label2.Name = "label2";
            label2.Size = new Size(33, 15);
            label2.TabIndex = 8;
            label2.Text = "Mice";
            // 
            // DeviceNumberConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(418, 399);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(mouseDownButton);
            Controls.Add(mouseUpButton);
            Controls.Add(miceListBox);
            Controls.Add(keyboardDownButton);
            Controls.Add(keyboardUpButton);
            Controls.Add(statusStrip1);
            Controls.Add(keyboardsListBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "DeviceNumberConfigForm";
            Text = "Device Number Configuration";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox keyboardsListBox;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
        private Button keyboardUpButton;
        private Button keyboardDownButton;
        private Button mouseDownButton;
        private Button mouseUpButton;
        private ListBox miceListBox;
        private Label label1;
        private Label label2;
    }
}
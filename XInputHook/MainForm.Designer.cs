namespace XInputHook {
    partial class MainForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            panel1 = new Panel();
            tableLayoutPanel2 = new TableLayoutPanel();
            button2 = new Button();
            runButton = new Button();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            button3 = new Button();
            panel2 = new Panel();
            outputTextBox = new TextBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            programTextBox = new TextBox();
            button8 = new Button();
            profileComboBox = new ComboBox();
            label4 = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            argumentsTextBox = new TextBox();
            workingDirectoryTextBox = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            button1 = new Button();
            run_Button = new Button();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip = new ContextMenuStrip(components);
            exitContextMenuItem = new ToolStripMenuItem();
            openFileDialog = new OpenFileDialog();
            panel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panel2.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            contextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(tableLayoutPanel2);
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(tableLayoutPanel4);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(8);
            panel1.Size = new Size(816, 450);
            panel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 9;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel2.Controls.Add(button2, 0, 0);
            tableLayoutPanel2.Controls.Add(runButton, 8, 0);
            tableLayoutPanel2.Controls.Add(button4, 1, 0);
            tableLayoutPanel2.Controls.Add(button5, 4, 0);
            tableLayoutPanel2.Controls.Add(button6, 4, 0);
            tableLayoutPanel2.Controls.Add(button7, 2, 0);
            tableLayoutPanel2.Controls.Add(button3, 6, 0);
            tableLayoutPanel2.Dock = DockStyle.Bottom;
            tableLayoutPanel2.Location = new Point(8, 404);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(800, 38);
            tableLayoutPanel2.TabIndex = 19;
            // 
            // button2
            // 
            button2.Dock = DockStyle.Fill;
            button2.Location = new Point(3, 3);
            button2.Name = "button2";
            button2.Size = new Size(102, 32);
            button2.TabIndex = 14;
            button2.Text = "New Profile";
            button2.UseVisualStyleBackColor = true;
            button2.Click += NewProfileButtonClicked;
            // 
            // runButton
            // 
            runButton.Dock = DockStyle.Fill;
            runButton.Location = new Point(691, 3);
            runButton.Name = "runButton";
            runButton.Size = new Size(106, 32);
            runButton.TabIndex = 13;
            runButton.Text = "Run";
            runButton.UseVisualStyleBackColor = true;
            runButton.Click += RunButtonClicked;
            // 
            // button4
            // 
            button4.Dock = DockStyle.Fill;
            button4.Location = new Point(111, 3);
            button4.Name = "button4";
            button4.Size = new Size(102, 32);
            button4.TabIndex = 15;
            button4.Text = "Delete Profile";
            button4.UseVisualStyleBackColor = true;
            button4.Click += DeleteProfileButtonClicked;
            // 
            // button5
            // 
            button5.Dock = DockStyle.Fill;
            button5.Location = new Point(455, 3);
            button5.Name = "button5";
            button5.Size = new Size(102, 32);
            button5.TabIndex = 16;
            button5.Text = "Edit Script";
            button5.UseVisualStyleBackColor = true;
            button5.Click += EditScriptButtonClicked;
            // 
            // button6
            // 
            button6.Dock = DockStyle.Fill;
            button6.Location = new Point(347, 3);
            button6.Name = "button6";
            button6.Size = new Size(102, 32);
            button6.TabIndex = 17;
            button6.Text = "Quick Config";
            button6.UseVisualStyleBackColor = true;
            button6.Click += QuickConfigButtonPressed;
            // 
            // button7
            // 
            button7.Dock = DockStyle.Fill;
            button7.Location = new Point(219, 3);
            button7.Name = "button7";
            button7.Size = new Size(102, 32);
            button7.TabIndex = 18;
            button7.Text = "Duplicate Profile";
            button7.UseVisualStyleBackColor = true;
            button7.Click += DuplicateProfileButtonClicked;
            // 
            // button3
            // 
            button3.Dock = DockStyle.Fill;
            button3.Location = new Point(563, 3);
            button3.Name = "button3";
            button3.Size = new Size(102, 32);
            button3.TabIndex = 19;
            button3.Text = "Edit Device No.";
            button3.UseVisualStyleBackColor = true;
            button3.Click += EditDeviceNumbersButtonClicked;
            // 
            // panel2
            // 
            panel2.Controls.Add(outputTextBox);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(8, 127);
            panel2.Name = "panel2";
            panel2.Padding = new Padding(8);
            panel2.Size = new Size(800, 271);
            panel2.TabIndex = 14;
            // 
            // outputTextBox
            // 
            outputTextBox.Dock = DockStyle.Fill;
            outputTextBox.Location = new Point(8, 8);
            outputTextBox.Margin = new Padding(8);
            outputTextBox.Multiline = true;
            outputTextBox.Name = "outputTextBox";
            outputTextBox.ReadOnly = true;
            outputTextBox.ScrollBars = ScrollBars.Vertical;
            outputTextBox.Size = new Size(784, 255);
            outputTextBox.TabIndex = 15;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(tableLayoutPanel3, 1, 1);
            tableLayoutPanel4.Controls.Add(profileComboBox, 1, 0);
            tableLayoutPanel4.Controls.Add(label4, 0, 2);
            tableLayoutPanel4.Controls.Add(label1, 0, 0);
            tableLayoutPanel4.Controls.Add(label2, 0, 1);
            tableLayoutPanel4.Controls.Add(label3, 0, 3);
            tableLayoutPanel4.Controls.Add(argumentsTextBox, 1, 2);
            tableLayoutPanel4.Controls.Add(workingDirectoryTextBox, 1, 3);
            tableLayoutPanel4.Dock = DockStyle.Top;
            tableLayoutPanel4.Location = new Point(8, 8);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 4;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.Size = new Size(800, 119);
            tableLayoutPanel4.TabIndex = 12;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.Controls.Add(programTextBox, 0, 0);
            tableLayoutPanel3.Controls.Add(button8, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(112, 32);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(685, 29);
            tableLayoutPanel3.TabIndex = 18;
            // 
            // programTextBox
            // 
            programTextBox.Dock = DockStyle.Fill;
            programTextBox.Location = new Point(0, 3);
            programTextBox.Margin = new Padding(0, 3, 0, 3);
            programTextBox.Name = "programTextBox";
            programTextBox.Size = new Size(607, 23);
            programTextBox.TabIndex = 5;
            programTextBox.Leave += ProfileFieldFocusLeave;
            // 
            // button8
            // 
            button8.Dock = DockStyle.Fill;
            button8.Location = new Point(610, 3);
            button8.Margin = new Padding(3, 3, 0, 3);
            button8.Name = "button8";
            button8.Size = new Size(75, 23);
            button8.TabIndex = 16;
            button8.Text = "...";
            button8.UseVisualStyleBackColor = true;
            button8.Click += ProgramPickButtonClicked;
            // 
            // profileComboBox
            // 
            profileComboBox.Dock = DockStyle.Fill;
            profileComboBox.FormattingEnabled = true;
            profileComboBox.Location = new Point(112, 3);
            profileComboBox.Name = "profileComboBox";
            profileComboBox.Size = new Size(685, 23);
            profileComboBox.TabIndex = 0;
            profileComboBox.SelectedValueChanged += ProfileComboBoxSelectedValueChanged;
            profileComboBox.Leave += ProfileFieldFocusLeave;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Location = new Point(3, 71);
            label4.Name = "label4";
            label4.Size = new Size(66, 15);
            label4.TabIndex = 4;
            label4.Text = "Arguments";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 7);
            label1.Name = "label1";
            label1.Size = new Size(41, 15);
            label1.TabIndex = 1;
            label1.Text = "Profile";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(3, 39);
            label2.Name = "label2";
            label2.Size = new Size(53, 15);
            label2.TabIndex = 2;
            label2.Text = "Program";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(3, 100);
            label3.Name = "label3";
            label3.Size = new Size(103, 15);
            label3.TabIndex = 3;
            label3.Text = "Working Directory";
            // 
            // argumentsTextBox
            // 
            argumentsTextBox.Dock = DockStyle.Fill;
            argumentsTextBox.Location = new Point(112, 67);
            argumentsTextBox.Name = "argumentsTextBox";
            argumentsTextBox.Size = new Size(685, 23);
            argumentsTextBox.TabIndex = 6;
            argumentsTextBox.Leave += ProfileFieldFocusLeave;
            // 
            // workingDirectoryTextBox
            // 
            workingDirectoryTextBox.Dock = DockStyle.Fill;
            workingDirectoryTextBox.Location = new Point(112, 96);
            workingDirectoryTextBox.Name = "workingDirectoryTextBox";
            workingDirectoryTextBox.Size = new Size(685, 23);
            workingDirectoryTextBox.TabIndex = 7;
            workingDirectoryTextBox.Leave += ProfileFieldFocusLeave;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 5;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tableLayoutPanel1.Controls.Add(button1, 0, 0);
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.Size = new Size(200, 100);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Dock = DockStyle.Fill;
            button1.Location = new Point(3, 3);
            button1.Name = "button1";
            button1.Size = new Size(94, 94);
            button1.TabIndex = 14;
            button1.Text = "New Profile";
            button1.UseVisualStyleBackColor = true;
            // 
            // run_Button
            // 
            run_Button.Dock = DockStyle.Fill;
            run_Button.Location = new Point(53, 3);
            run_Button.Name = "run_Button";
            run_Button.Size = new Size(144, 94);
            run_Button.TabIndex = 13;
            run_Button.Text = "Run";
            run_Button.UseVisualStyleBackColor = true;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "XInputHook";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += OnTrayDoubleClicked;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { exitContextMenuItem });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.ShowImageMargin = false;
            contextMenuStrip.Size = new Size(69, 26);
            // 
            // exitContextMenuItem
            // 
            exitContextMenuItem.Name = "exitContextMenuItem";
            exitContextMenuItem.Size = new Size(68, 22);
            exitContextMenuItem.Text = "Exit";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(816, 450);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            Text = "XInputHook";
            FormClosed += OnFormClosed;
            Resize += OnFormResized;
            panel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            contextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private TableLayoutPanel tableLayoutPanel4;
        private ComboBox profileComboBox;
        private Label label4;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox programTextBox;
        private TextBox argumentsTextBox;
        private TextBox workingDirectoryTextBox;
        private TextBox outputTextBox;
        private TableLayoutPanel tableLayoutPanel1;
        private Button button1;
        private Button run_Button;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button2;
        private Button runButton;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button3;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem exitContextMenuItem;
        private TableLayoutPanel tableLayoutPanel3;
        private Button button8;
        private OpenFileDialog openFileDialog;
    }
}

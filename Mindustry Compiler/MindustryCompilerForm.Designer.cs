
namespace Mindustry_Compiler
{
    partial class MindustryCompilerForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MindustryCompilerForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkCompileOnFocus = new System.Windows.Forms.CheckBox();
            this.panel7 = new System.Windows.Forms.Panel();
            this.btnCompile = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.panel8 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.btnEditSource = new System.Windows.Forms.Button();
            this.pnlCompilerMsg = new System.Windows.Forms.Panel();
            this.txtCompileMsg = new System.Windows.Forms.TextBox();
            this.pnlMsgResize = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtAsm = new System.Windows.Forms.TextBox();
            this.tmrGameFocused = new System.Windows.Forms.Timer(this.components);
            this.fswSource = new System.IO.FileSystemWatcher();
            this.panel1.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel6.SuspendLayout();
            this.pnlCompilerMsg.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fswSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.chkCompileOnFocus);
            this.panel1.Controls.Add(this.panel7);
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(486, 61);
            this.panel1.TabIndex = 0;
            // 
            // chkCompileOnFocus
            // 
            this.chkCompileOnFocus.AutoSize = true;
            this.chkCompileOnFocus.Checked = true;
            this.chkCompileOnFocus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCompileOnFocus.ForeColor = System.Drawing.Color.Black;
            this.chkCompileOnFocus.Location = new System.Drawing.Point(12, 35);
            this.chkCompileOnFocus.Name = "chkCompileOnFocus";
            this.chkCompileOnFocus.Size = new System.Drawing.Size(128, 17);
            this.chkCompileOnFocus.TabIndex = 3;
            this.chkCompileOnFocus.Text = "Copy on Game Focus";
            this.chkCompileOnFocus.UseVisualStyleBackColor = true;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.btnCompile);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel7.Location = new System.Drawing.Point(381, 28);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(105, 33);
            this.panel7.TabIndex = 8;
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(5, 3);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(94, 23);
            this.btnCompile.TabIndex = 4;
            this.btnCompile.Text = "Compile Now";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.txtPath);
            this.panel4.Controls.Add(this.panel8);
            this.panel4.Controls.Add(this.panel6);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(0, 4, 8, 2);
            this.panel4.Size = new System.Drawing.Size(486, 28);
            this.panel4.TabIndex = 7;
            // 
            // txtPath
            // 
            this.txtPath.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtPath.Location = new System.Drawing.Point(10, 6);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(371, 20);
            this.txtPath.TabIndex = 6;
            // 
            // panel8
            // 
            this.panel8.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel8.Location = new System.Drawing.Point(0, 4);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(10, 22);
            this.panel8.TabIndex = 1;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.button1);
            this.panel6.Controls.Add(this.btnEditSource);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel6.Location = new System.Drawing.Point(381, 4);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(97, 22);
            this.panel6.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Transparent;
            this.button1.Location = new System.Drawing.Point(39, -1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(60, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Open...";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnEditSource
            // 
            this.btnEditSource.BackgroundImage = global::Mindustry_Compiler.Properties.Resources.edit_icon1;
            this.btnEditSource.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnEditSource.Location = new System.Drawing.Point(5, -6);
            this.btnEditSource.Margin = new System.Windows.Forms.Padding(0);
            this.btnEditSource.Name = "btnEditSource";
            this.btnEditSource.Size = new System.Drawing.Size(32, 32);
            this.btnEditSource.TabIndex = 7;
            this.btnEditSource.UseVisualStyleBackColor = true;
            this.btnEditSource.Click += new System.EventHandler(this.btnEditSource_Click);
            // 
            // pnlCompilerMsg
            // 
            this.pnlCompilerMsg.BackColor = System.Drawing.Color.Transparent;
            this.pnlCompilerMsg.Controls.Add(this.txtCompileMsg);
            this.pnlCompilerMsg.Controls.Add(this.pnlMsgResize);
            this.pnlCompilerMsg.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlCompilerMsg.Location = new System.Drawing.Point(10, 559);
            this.pnlCompilerMsg.Name = "pnlCompilerMsg";
            this.pnlCompilerMsg.Size = new System.Drawing.Size(476, 177);
            this.pnlCompilerMsg.TabIndex = 2;
            // 
            // txtCompileMsg
            // 
            this.txtCompileMsg.BackColor = System.Drawing.SystemColors.Control;
            this.txtCompileMsg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCompileMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCompileMsg.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCompileMsg.Location = new System.Drawing.Point(0, 6);
            this.txtCompileMsg.Multiline = true;
            this.txtCompileMsg.Name = "txtCompileMsg";
            this.txtCompileMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCompileMsg.Size = new System.Drawing.Size(476, 171);
            this.txtCompileMsg.TabIndex = 0;
            this.txtCompileMsg.WordWrap = false;
            // 
            // pnlMsgResize
            // 
            this.pnlMsgResize.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pnlMsgResize.Cursor = System.Windows.Forms.Cursors.SizeNS;
            this.pnlMsgResize.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMsgResize.Location = new System.Drawing.Point(0, 0);
            this.pnlMsgResize.Name = "pnlMsgResize";
            this.pnlMsgResize.Size = new System.Drawing.Size(476, 6);
            this.pnlMsgResize.TabIndex = 1;
            this.pnlMsgResize.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlMsgResize_MouseMove);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.Transparent;
            this.panel5.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel5.Location = new System.Drawing.Point(486, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(10, 736);
            this.panel5.TabIndex = 6;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 61);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(10, 675);
            this.panel3.TabIndex = 9;
            // 
            // txtAsm
            // 
            this.txtAsm.BackColor = System.Drawing.Color.Gainsboro;
            this.txtAsm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAsm.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAsm.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.txtAsm.HideSelection = false;
            this.txtAsm.Location = new System.Drawing.Point(10, 61);
            this.txtAsm.Multiline = true;
            this.txtAsm.Name = "txtAsm";
            this.txtAsm.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAsm.Size = new System.Drawing.Size(476, 498);
            this.txtAsm.TabIndex = 10;
            this.txtAsm.WordWrap = false;
            this.txtAsm.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtAsm_MouseDown);
            // 
            // tmrGameFocused
            // 
            this.tmrGameFocused.Enabled = true;
            this.tmrGameFocused.Interval = 250;
            this.tmrGameFocused.Tick += new System.EventHandler(this.tmrGameFocused_Tick);
            // 
            // fswSource
            // 
            this.fswSource.EnableRaisingEvents = true;
            this.fswSource.SynchronizingObject = this;
            this.fswSource.Changed += new System.IO.FileSystemEventHandler(this.fswSource_Changed);
            // 
            // MindustryCompilerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(496, 736);
            this.Controls.Add(this.txtAsm);
            this.Controls.Add(this.pnlCompilerMsg);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel5);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MindustryCompilerForm";
            this.Text = "Mindustry Compiler";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MindustryCompilerForm_FormClosing);
            this.Load += new System.EventHandler(this.MindustryCompilerForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.pnlCompilerMsg.ResumeLayout(false);
            this.pnlCompilerMsg.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fswSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnlCompilerMsg;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel7;
        public System.Windows.Forms.CheckBox chkCompileOnFocus;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtAsm;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Timer tmrGameFocused;
        private System.IO.FileSystemWatcher fswSource;
        private System.Windows.Forms.TextBox txtCompileMsg;
        private System.Windows.Forms.Panel pnlMsgResize;
        private System.Windows.Forms.Button btnEditSource;
    }
}


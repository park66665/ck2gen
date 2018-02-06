namespace CrusaderKingsStoryGen
{
    partial class ModSelect
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
            this.moveUp = new System.Windows.Forms.Button();
            this.moveDown = new System.Windows.Forms.Button();
            this.inactiveMods = new System.Windows.Forms.ListBox();
            this.remove = new System.Windows.Forms.Button();
            this.add = new System.Windows.Forms.Button();
            this.activeMods = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // moveUp
            // 
            this.moveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveUp.Location = new System.Drawing.Point(552, 12);
            this.moveUp.Name = "moveUp";
            this.moveUp.Size = new System.Drawing.Size(75, 23);
            this.moveUp.TabIndex = 0;
            this.moveUp.Text = "Move Up";
            this.moveUp.UseVisualStyleBackColor = true;
            this.moveUp.Click += new System.EventHandler(this.moveUp_Click);
            // 
            // moveDown
            // 
            this.moveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveDown.Location = new System.Drawing.Point(552, 41);
            this.moveDown.Name = "moveDown";
            this.moveDown.Size = new System.Drawing.Size(75, 23);
            this.moveDown.TabIndex = 0;
            this.moveDown.Text = "Move Down";
            this.moveDown.UseVisualStyleBackColor = true;
            this.moveDown.Click += new System.EventHandler(this.moveDown_Click);
            // 
            // inactiveMods
            // 
            this.inactiveMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inactiveMods.FormattingEnabled = true;
            this.inactiveMods.Location = new System.Drawing.Point(13, 12);
            this.inactiveMods.Name = "inactiveMods";
            this.inactiveMods.Size = new System.Drawing.Size(219, 433);
            this.inactiveMods.Sorted = true;
            this.inactiveMods.TabIndex = 1;
            this.inactiveMods.SelectedIndexChanged += new System.EventHandler(this.inactiveMods_SelectedIndexChanged);
            this.inactiveMods.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.inactiveMods_MouseDoubleClick);
            // 
            // remove
            // 
            this.remove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remove.Location = new System.Drawing.Point(238, 59);
            this.remove.Name = "remove";
            this.remove.Size = new System.Drawing.Size(83, 23);
            this.remove.TabIndex = 0;
            this.remove.Text = "< Remove";
            this.remove.UseVisualStyleBackColor = true;
            this.remove.Click += new System.EventHandler(this.remove_Click);
            // 
            // add
            // 
            this.add.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.add.Location = new System.Drawing.Point(238, 88);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(83, 23);
            this.add.TabIndex = 0;
            this.add.Text = "Add >";
            this.add.UseVisualStyleBackColor = true;
            this.add.Click += new System.EventHandler(this.add_Click);
            // 
            // activeMods
            // 
            this.activeMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activeMods.FormattingEnabled = true;
            this.activeMods.Location = new System.Drawing.Point(327, 12);
            this.activeMods.Name = "activeMods";
            this.activeMods.Size = new System.Drawing.Size(219, 433);
            this.activeMods.TabIndex = 1;
            this.activeMods.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // ModSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 471);
            this.Controls.Add(this.activeMods);
            this.Controls.Add(this.inactiveMods);
            this.Controls.Add(this.add);
            this.Controls.Add(this.remove);
            this.Controls.Add(this.moveDown);
            this.Controls.Add(this.moveUp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModSelect";
            this.Text = "Select Mods";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button moveUp;
        private System.Windows.Forms.Button moveDown;
        private System.Windows.Forms.ListBox inactiveMods;
        private System.Windows.Forms.Button remove;
        private System.Windows.Forms.Button add;
        internal System.Windows.Forms.ListBox activeMods;
    }
}
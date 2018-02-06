namespace CrusaderKingsStoryGen
{
    partial class CreateBookmark
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
            this.bookMarkTitle = new System.Windows.Forms.TextBox();
            this.bookMarkDescription = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bookMarkTitle
            // 
            this.bookMarkTitle.Location = new System.Drawing.Point(12, 12);
            this.bookMarkTitle.Name = "bookMarkTitle";
            this.bookMarkTitle.Size = new System.Drawing.Size(259, 20);
            this.bookMarkTitle.TabIndex = 0;
            // 
            // bookMarkDescription
            // 
            this.bookMarkDescription.Location = new System.Drawing.Point(12, 38);
            this.bookMarkDescription.Multiline = true;
            this.bookMarkDescription.Name = "bookMarkDescription";
            this.bookMarkDescription.Size = new System.Drawing.Size(260, 190);
            this.bookMarkDescription.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(197, 234);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Ok";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // CreateBookmark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.bookMarkDescription);
            this.Controls.Add(this.bookMarkTitle);
            this.Name = "CreateBookmark";
            this.Text = "Create Bookmark";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.TextBox bookMarkTitle;
        public System.Windows.Forms.TextBox bookMarkDescription;
    }
}
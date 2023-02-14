
namespace GCPPhotoConversion
{
    partial class Form1
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
            this.btnGetData = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnGetList = new System.Windows.Forms.Button();
            this.btnDownloadPhotos = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnGetData
            // 
            this.btnGetData.Location = new System.Drawing.Point(60, 62);
            this.btnGetData.Name = "btnGetData";
            this.btnGetData.Size = new System.Drawing.Size(147, 23);
            this.btnGetData.TabIndex = 0;
            this.btnGetData.Text = "Compile Download List";
            this.btnGetData.UseVisualStyleBackColor = true;
            this.btnGetData.Click += new System.EventHandler(this.btnGetData_Click);
            // 
            // btnGetList
            // 
            this.btnGetList.Location = new System.Drawing.Point(60, 114);
            this.btnGetList.Name = "btnGetList";
            this.btnGetList.Size = new System.Drawing.Size(147, 23);
            this.btnGetList.TabIndex = 1;
            this.btnGetList.Text = "Get List to download";
            this.btnGetList.UseVisualStyleBackColor = true;
            this.btnGetList.Click += new System.EventHandler(this.btnGetList_Click);
            // 
            // btnDownloadPhotos
            // 
            this.btnDownloadPhotos.Location = new System.Drawing.Point(60, 171);
            this.btnDownloadPhotos.Name = "btnDownloadPhotos";
            this.btnDownloadPhotos.Size = new System.Drawing.Size(147, 23);
            this.btnDownloadPhotos.TabIndex = 2;
            this.btnDownloadPhotos.Text = "Download Photos";
            this.btnDownloadPhotos.UseVisualStyleBackColor = true;
            this.btnDownloadPhotos.Click += new System.EventHandler(this.btnDownloadPhotos_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnDownloadPhotos);
            this.Controls.Add(this.btnGetList);
            this.Controls.Add(this.btnGetData);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnGetData;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnGetList;
        private System.Windows.Forms.Button btnDownloadPhotos;
    }
}


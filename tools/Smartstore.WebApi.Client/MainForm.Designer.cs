﻿namespace Smartstore.WebApi.Client
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Smartstore.WebApi.Client.Properties.Settings settings1 = new Smartstore.WebApi.Client.Properties.Settings();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.chkIEEE754Compatible = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.callApi = new System.Windows.Forms.Button();
            this.cboPath = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cboMethod = new System.Windows.Forms.ComboBox();
            this.txtResponse = new System.Windows.Forms.RichTextBox();
            this.clear = new System.Windows.Forms.Button();
            this.txtSecretKey = new System.Windows.Forms.TextBox();
            this.txtPublicKey = new System.Windows.Forms.TextBox();
            this.cboContent = new System.Windows.Forms.ComboBox();
            this.btnDeletePath = new System.Windows.Forms.Button();
            this.btnDeleteContent = new System.Windows.Forms.Button();
            this.txtRequest = new System.Windows.Forms.RichTextBox();
            this.btnDeleteQuery = new System.Windows.Forms.Button();
            this.cboQuery = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblRequest = new System.Windows.Forms.TextBox();
            this.lblResponse = new System.Windows.Forms.TextBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.btnFileOpen = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnDeleteHeaders = new System.Windows.Forms.Button();
            this.cboHeaders = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cboFileUpload = new System.Windows.Forms.ComboBox();
            this.txtVersion = new System.Windows.Forms.TextBox();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.btnDeleteFileUpload = new System.Windows.Forms.Button();
            this.txtProxyPort = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chkIEEE754Compatible
            // 
            this.chkIEEE754Compatible.AutoSize = true;
            this.chkIEEE754Compatible.Location = new System.Drawing.Point(153, 91);
            this.chkIEEE754Compatible.Name = "chkIEEE754Compatible";
            this.chkIEEE754Compatible.Size = new System.Drawing.Size(119, 17);
            this.chkIEEE754Compatible.TabIndex = 50;
            this.chkIEEE754Compatible.Text = "IEEE754Compatible";
            this.chkIEEE754Compatible.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(516, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Public-Key";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Secret-Key";
            // 
            // callApi
            // 
            this.callApi.AutoSize = true;
            this.callApi.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.callApi.Location = new System.Drawing.Point(12, 203);
            this.callApi.Name = "callApi";
            this.callApi.Size = new System.Drawing.Size(173, 33);
            this.callApi.TabIndex = 8;
            this.callApi.Text = "Execute";
            this.callApi.UseVisualStyleBackColor = true;
            this.callApi.Click += new System.EventHandler(this.CallApi_Click);
            // 
            // cboPath
            // 
            this.cboPath.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cboPath.FormattingEnabled = true;
            this.cboPath.Location = new System.Drawing.Point(551, 9);
            this.cboPath.Name = "cboPath";
            this.cboPath.Size = new System.Drawing.Size(571, 22);
            this.cboPath.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Store URL";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 139);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "JSON Body";
            // 
            // cboMethod
            // 
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.FormattingEnabled = true;
            this.cboMethod.Items.AddRange(new object[] {
            "GET",
            "POST",
            "PUT",
            "PATCH",
            "DELETE"});
            this.cboMethod.Location = new System.Drawing.Point(370, 9);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(89, 21);
            this.cboMethod.TabIndex = 3;
            this.cboMethod.SelectionChangeCommitted += new System.EventHandler(this.CboMethod_changeCommitted);
            // 
            // txtResponse
            // 
            this.txtResponse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResponse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtResponse.DetectUrls = false;
            this.txtResponse.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtResponse.HideSelection = false;
            this.txtResponse.Location = new System.Drawing.Point(465, 262);
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.ReadOnly = true;
            this.txtResponse.Size = new System.Drawing.Size(676, 587);
            this.txtResponse.TabIndex = 10;
            this.txtResponse.Text = "";
            this.txtResponse.WordWrap = false;
            // 
            // clear
            // 
            this.clear.AutoSize = true;
            this.clear.Location = new System.Drawing.Point(191, 203);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(99, 33);
            this.clear.TabIndex = 9;
            this.clear.Text = "Clear";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // txtSecretKey
            // 
            this.txtSecretKey.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtSecretKey.Location = new System.Drawing.Point(84, 36);
            this.txtSecretKey.Name = "txtSecretKey";
            this.txtSecretKey.Size = new System.Drawing.Size(225, 20);
            this.txtSecretKey.TabIndex = 1;
            // 
            // txtPublicKey
            // 
            this.txtPublicKey.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtPublicKey.Location = new System.Drawing.Point(84, 10);
            this.txtPublicKey.Name = "txtPublicKey";
            this.txtPublicKey.Size = new System.Drawing.Size(225, 20);
            this.txtPublicKey.TabIndex = 0;
            // 
            // cboContent
            // 
            this.cboContent.FormattingEnabled = true;
            this.cboContent.Location = new System.Drawing.Point(84, 136);
            this.cboContent.Name = "cboContent";
            this.cboContent.Size = new System.Drawing.Size(1038, 21);
            this.cboContent.TabIndex = 7;
            // 
            // btnDeletePath
            // 
            this.btnDeletePath.AutoSize = true;
            this.btnDeletePath.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnDeletePath.Location = new System.Drawing.Point(1124, 9);
            this.btnDeletePath.Name = "btnDeletePath";
            this.btnDeletePath.Size = new System.Drawing.Size(20, 22);
            this.btnDeletePath.TabIndex = 8;
            this.btnDeletePath.Text = "x";
            this.btnDeletePath.UseVisualStyleBackColor = true;
            this.btnDeletePath.Click += new System.EventHandler(this.BtnDeletePath_Click);
            // 
            // btnDeleteContent
            // 
            this.btnDeleteContent.AutoSize = true;
            this.btnDeleteContent.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnDeleteContent.Location = new System.Drawing.Point(1124, 136);
            this.btnDeleteContent.Name = "btnDeleteContent";
            this.btnDeleteContent.Size = new System.Drawing.Size(20, 22);
            this.btnDeleteContent.TabIndex = 9;
            this.btnDeleteContent.Text = "x";
            this.btnDeleteContent.UseVisualStyleBackColor = true;
            this.btnDeleteContent.Click += new System.EventHandler(this.BtnDeleteContent_Click);
            // 
            // txtRequest
            // 
            this.txtRequest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtRequest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRequest.DetectUrls = false;
            this.txtRequest.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtRequest.HideSelection = false;
            this.txtRequest.Location = new System.Drawing.Point(12, 262);
            this.txtRequest.Name = "txtRequest";
            this.txtRequest.ReadOnly = true;
            this.txtRequest.Size = new System.Drawing.Size(447, 587);
            this.txtRequest.TabIndex = 20;
            this.txtRequest.Text = "";
            this.txtRequest.WordWrap = false;
            // 
            // btnDeleteQuery
            // 
            this.btnDeleteQuery.AutoSize = true;
            this.btnDeleteQuery.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnDeleteQuery.Location = new System.Drawing.Point(1124, 35);
            this.btnDeleteQuery.Name = "btnDeleteQuery";
            this.btnDeleteQuery.Size = new System.Drawing.Size(20, 22);
            this.btnDeleteQuery.TabIndex = 9;
            this.btnDeleteQuery.Text = "x";
            this.btnDeleteQuery.UseVisualStyleBackColor = true;
            this.btnDeleteQuery.Click += new System.EventHandler(this.BtnDeleteQuery_Click);
            // 
            // cboQuery
            // 
            this.cboQuery.FormattingEnabled = true;
            this.cboQuery.Location = new System.Drawing.Point(551, 35);
            this.cboQuery.Name = "cboQuery";
            this.cboQuery.Size = new System.Drawing.Size(571, 21);
            this.cboQuery.TabIndex = 6;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(510, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 13);
            this.label8.TabIndex = 24;
            this.label8.Text = "Query";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(323, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "Method";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(323, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 13);
            this.label7.TabIndex = 29;
            this.label7.Text = "Version";
            // 
            // lblRequest
            // 
            this.lblRequest.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblRequest.Location = new System.Drawing.Point(12, 242);
            this.lblRequest.Name = "lblRequest";
            this.lblRequest.ReadOnly = true;
            this.lblRequest.Size = new System.Drawing.Size(447, 14);
            this.lblRequest.TabIndex = 31;
            this.lblRequest.Text = "Request";
            // 
            // lblResponse
            // 
            this.lblResponse.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblResponse.Location = new System.Drawing.Point(465, 242);
            this.lblResponse.Name = "lblResponse";
            this.lblResponse.ReadOnly = true;
            this.lblResponse.Size = new System.Drawing.Size(676, 14);
            this.lblResponse.TabIndex = 32;
            this.lblResponse.Text = "Response";
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(22, 166);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(58, 13);
            this.lblFile.TabIndex = 34;
            this.lblFile.Text = "File upload";
            // 
            // btnFileOpen
            // 
            this.btnFileOpen.AutoSize = true;
            this.btnFileOpen.Location = new System.Drawing.Point(1075, 161);
            this.btnFileOpen.Name = "btnFileOpen";
            this.btnFileOpen.Size = new System.Drawing.Size(69, 24);
            this.btnFileOpen.TabIndex = 35;
            this.btnFileOpen.Text = "Open file";
            this.btnFileOpen.UseVisualStyleBackColor = true;
            this.btnFileOpen.Click += new System.EventHandler(this.BtnFileOpen_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnDeleteHeaders
            // 
            this.btnDeleteHeaders.AutoSize = true;
            this.btnDeleteHeaders.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnDeleteHeaders.Location = new System.Drawing.Point(1124, 61);
            this.btnDeleteHeaders.Name = "btnDeleteHeaders";
            this.btnDeleteHeaders.Size = new System.Drawing.Size(20, 22);
            this.btnDeleteHeaders.TabIndex = 9;
            this.btnDeleteHeaders.Text = "x";
            this.btnDeleteHeaders.UseVisualStyleBackColor = true;
            this.btnDeleteHeaders.Click += new System.EventHandler(this.BtnDeleteHeaders_Click);
            // 
            // cboHeaders
            // 
            this.cboHeaders.FormattingEnabled = true;
            this.cboHeaders.Location = new System.Drawing.Point(551, 61);
            this.cboHeaders.Name = "cboHeaders";
            this.cboHeaders.Size = new System.Drawing.Size(571, 21);
            this.cboHeaders.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(500, 64);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 13);
            this.label10.TabIndex = 46;
            this.label10.Text = "Headers";
            // 
            // cboFileUpload
            // 
            this.cboFileUpload.FormattingEnabled = true;
            this.cboFileUpload.Location = new System.Drawing.Point(84, 163);
            this.cboFileUpload.Name = "cboFileUpload";
            this.cboFileUpload.Size = new System.Drawing.Size(965, 21);
            this.cboFileUpload.TabIndex = 48;
            // 
            // txtVersion
            // 
            settings1.ApiContent = "";
            settings1.ApiHeaders = "";
            settings1.ApiPaths = "";
            settings1.ApiProxyPort = "";
            settings1.ApiPublicKey = "";
            settings1.ApiQuery = "";
            settings1.ApiSecretKey = "";
            settings1.ApiUrl = "";
            settings1.ApiVersion = "";
            settings1.FileUpload = "";
            settings1.SettingsKey = "";
            this.txtVersion.DataBindings.Add(new System.Windows.Forms.Binding("Text", settings1, "ApiVersion", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtVersion.Location = new System.Drawing.Point(370, 36);
            this.txtVersion.Name = "txtVersion";
            this.txtVersion.Size = new System.Drawing.Size(89, 21);
            this.txtVersion.TabIndex = 4;
            // 
            // txtUrl
            // 
            this.txtUrl.DataBindings.Add(new System.Windows.Forms.Binding("Text", settings1, "ApiUrl", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtUrl.Location = new System.Drawing.Point(84, 62);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(225, 21);
            this.txtUrl.TabIndex = 2;
            // 
            // btnDeleteFileUpload
            // 
            this.btnDeleteFileUpload.AutoSize = true;
            this.btnDeleteFileUpload.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnDeleteFileUpload.Location = new System.Drawing.Point(1051, 162);
            this.btnDeleteFileUpload.Name = "btnDeleteFileUpload";
            this.btnDeleteFileUpload.Size = new System.Drawing.Size(20, 22);
            this.btnDeleteFileUpload.TabIndex = 49;
            this.btnDeleteFileUpload.Text = "x";
            this.btnDeleteFileUpload.UseVisualStyleBackColor = true;
            this.btnDeleteFileUpload.Click += new System.EventHandler(this.BtnDeleteFileUpload_Click);
            // 
            // txtProxyPort
            // 
            this.txtProxyPort.DataBindings.Add(new System.Windows.Forms.Binding("Text", settings1, "ApiProxyPort", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtProxyPort.Location = new System.Drawing.Point(84, 89);
            this.txtProxyPort.Name = "txtProxyPort";
            this.txtProxyPort.Size = new System.Drawing.Size(63, 21);
            this.txtProxyPort.TabIndex = 51;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(23, 92);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(58, 13);
            this.label11.TabIndex = 52;
            this.label11.Text = "Proxy Port";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 861);
            this.Controls.Add(this.txtProxyPort);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.chkIEEE754Compatible);
            this.Controls.Add(this.btnDeleteFileUpload);
            this.Controls.Add(this.cboFileUpload);
            this.Controls.Add(this.btnDeleteHeaders);
            this.Controls.Add(this.cboHeaders);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btnFileOpen);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.lblResponse);
            this.Controls.Add(this.lblRequest);
            this.Controls.Add(this.txtVersion);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnDeleteQuery);
            this.Controls.Add(this.cboQuery);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtRequest);
            this.Controls.Add(this.btnDeleteContent);
            this.Controls.Add(this.btnDeletePath);
            this.Controls.Add(this.cboContent);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.txtResponse);
            this.Controls.Add(this.cboMethod);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboPath);
            this.Controls.Add(this.callApi);
            this.Controls.Add(this.txtSecretKey);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPublicKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1170, 900);
            this.MinimumSize = new System.Drawing.Size(1170, 900);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSecretKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button callApi;
        private System.Windows.Forms.TextBox txtPublicKey;
        private System.Windows.Forms.ComboBox cboPath;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cboMethod;
        private System.Windows.Forms.RichTextBox txtResponse;
        private System.Windows.Forms.Button clear;
        private System.Windows.Forms.ComboBox cboContent;
        private System.Windows.Forms.Button btnDeletePath;
        private System.Windows.Forms.Button btnDeleteContent;
        private System.Windows.Forms.RichTextBox txtRequest;
        private System.Windows.Forms.Button btnDeleteQuery;
        private System.Windows.Forms.ComboBox cboQuery;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtVersion;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox lblRequest;
        private System.Windows.Forms.TextBox lblResponse;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.Button btnFileOpen;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnDeleteHeaders;
        private System.Windows.Forms.ComboBox cboHeaders;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cboFileUpload;
        private System.Windows.Forms.Button btnDeleteFileUpload;
        private System.Windows.Forms.CheckBox chkIEEE754Compatible;
        private System.Windows.Forms.TextBox txtProxyPort;
        private System.Windows.Forms.Label label11;
    }
}
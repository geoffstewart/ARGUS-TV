/*
 * Created by SharpDevelop.
 * User: geoff
 * Date: 11/15/2014
 * Time: 8:38 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace ArgusTV.GuideImporter.JSONService
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.username = new System.Windows.Forms.TextBox();
			this.password = new System.Windows.Forms.MaskedTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.updateChannelsCheckBox = new System.Windows.Forms.CheckBox();
			this.channelFormat = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.postalCode = new System.Windows.Forms.TextBox();
			this.country = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.saveButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.Days = new System.Windows.Forms.ComboBox();
			this.ColumnNameType = new System.Windows.Forms.ColumnHeader();
			this.ColumnNameLocation = new System.Windows.Forms.ColumnHeader();
			this.reloadLineupsButton = new System.Windows.Forms.Button();
			this.saveLineupsButton = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.addsLeftValueLabel = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.allLineupsList = new System.Windows.Forms.ListBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.getLogosButton = new System.Windows.Forms.Button();
			this.logoPath = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// username
			// 
			this.username.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.username.Location = new System.Drawing.Point(139, 43);
			this.username.Name = "username";
			this.username.Size = new System.Drawing.Size(234, 22);
			this.username.TabIndex = 0;
			// 
			// password
			// 
			this.password.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.password.Location = new System.Drawing.Point(139, 71);
			this.password.Name = "password";
			this.password.Size = new System.Drawing.Size(234, 22);
			this.password.TabIndex = 1;
			this.password.UseSystemPasswordChar = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(50, 46);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(83, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Username:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(40, 60);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Password:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.updateChannelsCheckBox);
			this.groupBox1.Controls.Add(this.channelFormat);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.postalCode);
			this.groupBox1.Controls.Add(this.country);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.saveButton);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.Days);
			this.groupBox1.Location = new System.Drawing.Point(12, 13);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(705, 152);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Base Configuration";
			// 
			// updateChannelsCheckBox
			// 
			this.updateChannelsCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.updateChannelsCheckBox.Location = new System.Drawing.Point(127, 116);
			this.updateChannelsCheckBox.Name = "updateChannelsCheckBox";
			this.updateChannelsCheckBox.Size = new System.Drawing.Size(171, 24);
			this.updateChannelsCheckBox.TabIndex = 3;
			this.updateChannelsCheckBox.Text = "Update Channels?";
			this.updateChannelsCheckBox.UseVisualStyleBackColor = true;
			// 
			// channelFormat
			// 
			this.channelFormat.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.channelFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.channelFormat.FormattingEnabled = true;
			this.channelFormat.Location = new System.Drawing.Point(127, 87);
			this.channelFormat.Name = "channelFormat";
			this.channelFormat.Size = new System.Drawing.Size(269, 24);
			this.channelFormat.TabIndex = 2;
			this.channelFormat.UseWaitCursor = true;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(0, 90);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(120, 16);
			this.label7.TabIndex = 9;
			this.label7.Text = "Channel Format:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// postalCode
			// 
			this.postalCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.postalCode.Location = new System.Drawing.Point(527, 56);
			this.postalCode.Name = "postalCode";
			this.postalCode.Size = new System.Drawing.Size(156, 22);
			this.postalCode.TabIndex = 5;
			// 
			// country
			// 
			this.country.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.country.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.country.FormattingEnabled = true;
			this.country.Location = new System.Drawing.Point(527, 27);
			this.country.Name = "country";
			this.country.Size = new System.Drawing.Size(156, 24);
			this.country.TabIndex = 4;
			this.country.UseWaitCursor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(424, 58);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(97, 16);
			this.label4.TabIndex = 6;
			this.label4.Text = "Postal Code:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(535, 117);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(148, 23);
			this.saveButton.TabIndex = 7;
			this.saveButton.Text = "Save Config";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(421, 87);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Days of data:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(457, 30);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(64, 16);
			this.label5.TabIndex = 5;
			this.label5.Text = "Country:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// Days
			// 
			this.Days.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.Days.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Days.FormattingEnabled = true;
			this.Days.Items.AddRange(new object[] {
			"2",
			"4",
			"13"});
			this.Days.Location = new System.Drawing.Point(527, 84);
			this.Days.Name = "Days";
			this.Days.Size = new System.Drawing.Size(46, 24);
			this.Days.TabIndex = 6;
			// 
			// reloadLineupsButton
			// 
			this.reloadLineupsButton.Location = new System.Drawing.Point(534, 35);
			this.reloadLineupsButton.Name = "reloadLineupsButton";
			this.reloadLineupsButton.Size = new System.Drawing.Size(148, 23);
			this.reloadLineupsButton.TabIndex = 9;
			this.reloadLineupsButton.Text = "Reload Lineups from SD";
			this.reloadLineupsButton.UseVisualStyleBackColor = true;
			this.reloadLineupsButton.Click += new System.EventHandler(this.reloadLineupsButton_Click);
			// 
			// saveLineupsButton
			// 
			this.saveLineupsButton.Location = new System.Drawing.Point(534, 64);
			this.saveLineupsButton.Name = "saveLineupsButton";
			this.saveLineupsButton.Size = new System.Drawing.Size(148, 23);
			this.saveLineupsButton.TabIndex = 2;
			this.saveLineupsButton.Text = "Save Lineups";
			this.saveLineupsButton.UseVisualStyleBackColor = true;
			this.saveLineupsButton.Click += new System.EventHandler(this.SaveLineupsButtonClick);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(549, 203);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(133, 32);
			this.label6.TabIndex = 3;
			this.label6.Text = "Remaining \'adds\' \r\nleft today:";
			// 
			// addsLeftValueLabel
			// 
			this.addsLeftValueLabel.AutoSize = true;
			this.addsLeftValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.addsLeftValueLabel.Location = new System.Drawing.Point(667, 219);
			this.addsLeftValueLabel.Name = "addsLeftValueLabel";
			this.addsLeftValueLabel.Size = new System.Drawing.Size(0, 16);
			this.addsLeftValueLabel.TabIndex = 4;
			this.addsLeftValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.addsLeftValueLabel);
			this.groupBox2.Controls.Add(this.allLineupsList);
			this.groupBox2.Controls.Add(this.saveLineupsButton);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.reloadLineupsButton);
			this.groupBox2.Location = new System.Drawing.Point(13, 171);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(704, 261);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Lineup Selection";
			// 
			// allLineupsList
			// 
			this.allLineupsList.FormattingEnabled = true;
			this.allLineupsList.Location = new System.Drawing.Point(25, 36);
			this.allLineupsList.Name = "allLineupsList";
			this.allLineupsList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.allLineupsList.Size = new System.Drawing.Size(482, 199);
			this.allLineupsList.TabIndex = 5;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.getLogosButton);
			this.groupBox3.Controls.Add(this.logoPath);
			this.groupBox3.Controls.Add(this.label8);
			this.groupBox3.Location = new System.Drawing.Point(13, 445);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(704, 54);
			this.groupBox3.TabIndex = 6;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Channel Logos";
			// 
			// getLogosButton
			// 
			this.getLogosButton.Location = new System.Drawing.Point(534, 20);
			this.getLogosButton.Name = "getLogosButton";
			this.getLogosButton.Size = new System.Drawing.Size(148, 23);
			this.getLogosButton.TabIndex = 10;
			this.getLogosButton.Text = "Get Logos";
			this.getLogosButton.UseVisualStyleBackColor = true;
			this.getLogosButton.Click += new System.EventHandler(this.getLogosClick);
			// 
			// logoPath
			// 
			this.logoPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.logoPath.Location = new System.Drawing.Point(126, 21);
			this.logoPath.Name = "logoPath";
			this.logoPath.Size = new System.Drawing.Size(381, 22);
			this.logoPath.TabIndex = 7;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(76, 24);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(43, 16);
			this.label8.TabIndex = 11;
			this.label8.Text = "Path:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(744, 511);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.password);
			this.Controls.Add(this.username);
			this.Controls.Add(this.groupBox1);
			this.Name = "MainForm";
			this.Text = "Next-Generation SchedulesDirect Plugin";
			this.Load += new System.EventHandler(this.MainFormLoad);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

			this.Load += new System.EventHandler(this.MainFormLoad);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
        private System.Windows.Forms.TextBox username;
        private System.Windows.Forms.MaskedTextBox password;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox Days;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox postalCode;
        private System.Windows.Forms.ComboBox country;
        private System.Windows.Forms.ColumnHeader ColumnNameType;
        private System.Windows.Forms.ColumnHeader ColumnNameLocation;
        private System.Windows.Forms.Label addsLeftValueLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button saveLineupsButton;
        private System.Windows.Forms.Button reloadLineupsButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox channelFormat;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListBox allLineupsList;
        private System.Windows.Forms.CheckBox updateChannelsCheckBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button getLogosButton;
        private System.Windows.Forms.TextBox logoPath;
	}
	
}

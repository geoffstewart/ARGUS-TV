using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArgusTV.GuideImporter.JSONService;
using ArgusTV.GuideImporter.JSONService.Entities;
using System.IO;
using ArgusTV.Common.Logging;

namespace ArgusTV.GuideImporter.JSONService
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private readonly Dictionary<string, string> countries = new Dictionary<string, string>();
		private string defaultLogoPath = "";
		
		public MainForm(string installationPath)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			Application.ThreadException += Application_ThreadException;
			
			string fileName = Path.Combine(installationPath, "Countries.config");
			countries = CountryListStore.Load(fileName);
			defaultLogoPath = installationPath;
			
		}
		void MainFormLoad(object sender, EventArgs e)
		{
			this.username.Text = ConfigInstance.Current.SDUserName;
			this.password.Text = ConfigInstance.Current.SDPassword;
			this.Days.SelectedText = ConfigInstance.Current.NrOfDaysToImport.ToString();
			this.country.DataSource = new BindingSource(this.countries,null);
			this.country.DisplayMember = "Value";
			this.country.ValueMember = "Key";
			this.country.SelectedValue = ConfigInstance.Current.SDCountry;
			this.postalCode.Text = ConfigInstance.Current.SDPostalCode;
			this.channelFormat.DataSource = ConfigInstance.Current.ChannelNameFormats;
			this.channelFormat.SelectedItem = ConfigInstance.Current.ChannelNameFormat;
			this.updateChannelsCheckBox.Checked = ConfigInstance.Current.UpdateChannelNames;
			this.logoPath.Text = defaultLogoPath;
		}
			
		

		private void saveButton_Click(object sender, EventArgs e) 
		{
			ConfigInstance.Current.SDUserName = this.username.Text;
			ConfigInstance.Current.SDPassword = this.password.Text;
			ConfigInstance.Current.NrOfDaysToImport = Int32.Parse(this.Days.Text);
			ConfigInstance.Current.SDCountry = this.country.SelectedValue.ToString();
			ConfigInstance.Current.SDPostalCode = this.postalCode.Text;
			ConfigInstance.Current.ChannelNameFormat = this.channelFormat.Text;
			ConfigInstance.Current.UpdateChannelNames = this.updateChannelsCheckBox.Checked;
			
			ConfigInstance.Save();
						
		}

		private void reloadLineupsButton_Click(Object sender, EventArgs e) 
		{
			this.reloadLineupsButton.Enabled = false;
			
			saveButton_Click(sender, e);
			
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest(ConfigInstance.Current.SDUserName, ConfigInstance.Current.SDPassword);
			
			// get all available lineups
			List<HeadendResponseInstance.Lineup> allLineups =
				wc.getLineupsFromHeadends(tr, ConfigInstance.Current.SDCountry, ConfigInstance.Current.SDPostalCode);

			this.allLineupsList.DataSource = new List<LineupFactory.GuiLineup>();
			
			// get the assigned headends
			List<AssignedLineupsResponse.Lineup> assignedLineups = wc.getAssignedLineups(tr);
			
			List<LineupFactory.GuiLineup> allGuiLineups = LineupFactory.makeGuiLineupList(allLineups);
			List<LineupFactory.GuiLineup> assignedGuiLineups = LineupFactory.makeGuiLineupList(assignedLineups);
			
			this.allLineupsList.DataSource = allGuiLineups;
			this.allLineupsList.DisplayMember = "displayName";
			this.allLineupsList.ValueMember = "uri";
		
			// clear the default selected item
			allLineupsList.ClearSelected();
			
			for (int i = 0; i < allLineupsList.Items.Count; i++) {
				
				foreach (LineupFactory.GuiLineup lu in assignedGuiLineups) {
					if (lu.uri.Equals(((LineupFactory.GuiLineup)allLineupsList.Items[i]).uri)) {
						this.allLineupsList.SetSelected(i,true);
					}
				}
				
			}
			this.reloadLineupsButton.Enabled = true;
		}
		
		void getLogosClick(object sender, EventArgs e) {
			
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest(ConfigInstance.Current.SDUserName, ConfigInstance.Current.SDPassword);
			
			List<LineupInfoResponse> llir =  wc.getAssignedLineupInfoList(tr);
			
			Dictionary<string, string> logoUrls = new Dictionary<string, string>();
			

			foreach (LineupInfoResponse lir in llir) {
				// make a real map from the map "array"
				Dictionary<string,string> channelMap = new Dictionary<string, string>();
				foreach (LineupInfoResponse.ChannelMap cm in lir.map) {
					if (!channelMap.ContainsKey(cm.stationID)) {
						channelMap.Add(cm.stationID, cm.channel);
					}
				}
				
				foreach (LineupInfoResponse.Station station in lir.stations) {
					if (station.logo != null && station.logo.URL != null) {
						string channelName = ChannelFactory.makeChannelName(station, channelMap, ConfigInstance.Current.ChannelNameFormat);
						if (!logoUrls.ContainsKey(channelName)) {
							logoUrls.Add(channelName,station.logo.URL);
						}
					}
				}
			}
			
			foreach (string channelName in logoUrls.Keys) {
				System.Net.WebClient httpWebClient = new System.Net.WebClient();
				
				try {
					httpWebClient.DownloadFile(
						logoUrls[channelName], 
						Path.Combine(this.logoPath.Text, channelName + ".png"));
				} catch (Exception ex) {
					Logger.Error("Could not download logo for channel {0}: {1}", channelName, ex.StackTrace);
					
				}
				
			}
			
			
		}
		

		
		
		void SaveLineupsButtonClick(object sender, EventArgs e)
		{
			// 
			this.saveButton.Enabled = false;
			
			// get currently assigned lineups
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest(ConfigInstance.Current.SDUserName, ConfigInstance.Current.SDPassword);
			List<AssignedLineupsResponse.Lineup> assignedLineups = wc.getAssignedLineups(tr);
			
			List<string> lineupsToAssign = new List<string>();
			List<string> lineupsToDelete = new List<string>();
			
			
			// find lineups that were added
			foreach (object i in this.allLineupsList.SelectedItems) 
			{
				LineupFactory.GuiLineup lu = (LineupFactory.GuiLineup)i;
				
				bool skipAdd = false;
				foreach (AssignedLineupsResponse.Lineup alu in assignedLineups) {
					if (alu.uri.Equals(lu.uri)) {
						skipAdd = true;
						break;
					}
				}
				
				if (!skipAdd) {
					lineupsToAssign.Add(lu.uri);
				}
			}
			
			// find lineups that were deleted
			foreach (AssignedLineupsResponse.Lineup alu in assignedLineups) {
				bool skipDelete = false;
				foreach (object  i in this.allLineupsList.SelectedItems) {
					LineupFactory.GuiLineup lu = (LineupFactory.GuiLineup)i;
					
					if (lu.uri.Equals(alu.uri)) {
						// assigned lineup is still selected
						skipDelete = true;
						break;
					}
					
				}
				
				if (!skipDelete) {
					lineupsToDelete.Add(alu.uri);
				}
			}
			
			// add the lineups that need adding
			foreach (string uriToAdd in lineupsToAssign) {
				AlterLineupsResponse alr = wc.addLineup(tr, uriToAdd);
				
				this.addsLeftValueLabel.Text = alr.changesRemaining.ToString();
				this.addsLeftValueLabel.Refresh();
			}
			
			// delete the lineups that need deleting
			foreach (string uriToAdd in lineupsToDelete) {
				AlterLineupsResponse alr = wc.deleteLineup(tr, uriToAdd);
				
				this.addsLeftValueLabel.Text = alr.changesRemaining.ToString();
				this.addsLeftValueLabel.Refresh();
			}
			
			this.saveButton.Enabled = true;
		}
		
		
		private void loadCountries() 
		{
			countries.Add("CAN", "Canada");
			countries.Add("USA", "USA");
			countries.Add("MEX", "Mexico");
			countries.Add("MEX", "Mexico");
		}

		
		void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			if (e.Exception is WebClientException) {
				WebClientException wce = (WebClientException)e.Exception;
				string message = string.Format("Error from SchedulesDirect JSON webservice: \nCode: {0}\nMessage: {1}\nServer: {2}",
				                               wce.Error.code, 
				                               wce.Error.message,
				                               wce.Error.serverID);
				Logger.Error(message);
				MessageBox.Show(message,
				                "SD WebClient Error", 
				                MessageBoxButtons.OK, 
				                MessageBoxIcon.Hand, 
				                MessageBoxDefaultButton.Button1, 
				                MessageBoxOptions.DefaultDesktopOnly,
				                false);
				this.saveButton.Enabled = true;
				this.reloadLineupsButton.Enabled = true;
			} else {
				Logger.Error("Unexpected error: {0}", e.Exception.Message);
				MessageBox.Show(e.Exception.Message,
				                "Unexpected Error",
				                MessageBoxButtons.OK, 
				                MessageBoxIcon.Hand, 
				                MessageBoxDefaultButton.Button1, 
				                MessageBoxOptions.DefaultDesktopOnly,
				                false);
				                
			}
		}

	}
}

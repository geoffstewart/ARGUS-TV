
using System;
using System.IO;
using ArgusTV.GuideImporter.Interfaces;
using ArgusTV.Common.Logging;
using ArgusTV.Common;
using ArgusTV.DataContracts;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using ArgusTV.GuideImporter.JSONService.Entities;
using log4net.Config;
using log4net;

namespace ArgusTV.GuideImporter.JSONService
{
	/// <summary>
	/// Description of SchedulesDirectJsonPlugin.
	/// </summary>
	public class SchedulesDirectJsonPlugin : IGuideImportPlugin
	{
		const string _pluginName = "SchedulesDirectJSONService";
		const string _pluginDesc = "Next-generation plugin for schedulesdirect.org (USA, Canada, Mexico, etc.)";
		const int _programBlockSize = 4000;
		private string _pluginInstallationPath;
		
		public SchedulesDirectJsonPlugin() {
			LogManager.GetLogger("ArgusTV").Logger.Repository.Threshold = log4net.Core.Level.Info;
		}
		#region INamedPlugin members
        public string Name { 
			get	{ return _pluginName; }
		}

        public string Description { 
			get { return _pluginDesc; }
		}

        public string InstallationPath { 
			get { return _pluginInstallationPath; }
			set {
				_pluginInstallationPath = value;
                ConfigInstance.Load(ConfigFileName);			
			}
		}

        public string ConfigFileName { 
			get { return Path.Combine(InstallationPath, "ArgusTV.GuideImporter.JSONService.dll.config"); }
		}
		#endregion


		public void ShowConfigurationDialog(System.Windows.Forms.Form parentDialog)
		{
			MainForm mf = new MainForm(this.InstallationPath);
			mf.ShowDialog(parentDialog);
		}
		
		public bool IsConfigured()
		{
			bool configured = (!String.IsNullOrEmpty(ConfigInstance.Current.SDUserName) && 
			                   !String.IsNullOrEmpty(ConfigInstance.Current.SDPassword) &&
			                   !String.IsNullOrEmpty(ConfigInstance.Current.SDCountry) &&
			                   !String.IsNullOrEmpty(ConfigInstance.Current.SDPostalCode));
			return configured;
		}

		/// <summary>
		/// Return a list of channels for the GUI to display
		/// </summary>
		/// <param name="reload"></param>
		/// <param name="progressCallback"></param>
		/// <param name="feedbackCallback"></param>
		/// <returns></returns>
		public List<ImportGuideChannel> GetAllImportChannels(
				bool reload, 
				ProgressCallback progressCallback, 
				FeedbackCallback feedbackCallback)
		{
			
			List<ImportGuideChannel> ligc = new List<ImportGuideChannel>();
			
			if (reload)
            {
				try {
	                GiveFeedback(feedbackCallback, "Calling SchedulesDirect JSON WebService ...");
	                WebClient wc = WebClient.getInstance();
	                
	                TokenRequest tr = new TokenRequest(ConfigInstance.Current.SDUserName, ConfigInstance.Current.SDPassword);
	                
	                List<AssignedLineupsResponse.Lineup> lineups = wc.getAssignedLineups(tr);
	                
	                GiveFeedback(feedbackCallback, "Got the lineups.... " + lineups.Count + " lineup assigned");
	                
	                foreach (AssignedLineupsResponse.Lineup lu in lineups) {
	                	GiveFeedback(feedbackCallback, "Get channels for " + lu.name);
	                	LineupInfoResponse liur = wc.getLineupInfo(tr, lu.uri);
	                	GiveFeedback(feedbackCallback, "Got a bunch of channels: " + liur.stations.Count);
	                	List<ImportGuideChannel> localLigc = ChannelFactory.makeImportChannels(liur, ConfigInstance.Current.ChannelNameFormat);
	                	ligc.AddRange(localLigc);
	                }
	                
	                GuideChannelStore.Save(AvailableChannelsConfigFile, ligc);
				} catch (Exception ex) {
					Logger.Error("Had a problem importing channels: {0}\n{1}", ex.Message, ex.StackTrace);
					throw;
				}
                
			} else {
                // read from file
				List<ImportGuideChannel> availableGuideChannels = GuideChannelStore.Load(AvailableChannelsConfigFile);
				ligc.AddRange(availableGuideChannels);
			}
			
            return ligc;
		}

		/// <summary>
		/// No implementation required
		/// </summary>
		/// <param name="feedbackCallback"></param>
		/// <param name="keepImportServiceAliveCallback"></param>
		public void PrepareImport(
			FeedbackCallback feedbackCallback, 
			KeepImportServiceAliveCallback keepImportServiceAliveCallback)
		{
			// nothing to do
		}

		/// <summary>
		/// Do the actual guide update
		/// </summary>
		/// <param name="skipChannels"></param>
		/// <param name="importDataCallback"></param>
		/// <param name="progressCallback"></param>
		/// <param name="feedbackCallback"></param>
		/// <param name="keepImportServiceAliveCallback"></param>
		public void Import(
			List<ImportGuideChannel> skipChannels, 
			ImportDataCallback importDataCallback, 
			ProgressCallback progressCallback, 
			FeedbackCallback feedbackCallback, 
			KeepImportServiceAliveCallback keepImportServiceAliveCallback)
		{
		
			try {
				GiveFeedback(feedbackCallback,"Staring SD JSON guide data import...");
				
				DateTime importStart = DateTime.Now;
				
				if (progressCallback != null)
	            {
	                progressCallback(0);
	            }
	            
	            keepImportServiceAliveCallback();
	            
	            // get station list
	            List<string> stationIdList = getStationIdList(skipChannels);
	            
	            Logger.Info("Getting guide data for {0} channels", stationIdList.Count);
	            
	            WebClient wc = WebClient.getInstance();
	            
	            TokenRequest tr = new TokenRequest(ConfigInstance.Current.SDUserName, ConfigInstance.Current.SDPassword);
	
				// make sure SD site is online            
	            StatusResponse status = wc.getStatus(tr);
	            if (status != null && status.systemStatus != null) {
	            	string st = status.systemStatus[0].status.ToLower();
	            	if (!"online".Equals(st)) {
	            		Logger.Error("The SD server is not online: {0} - {1) - {2}", status.serverID, status.systemStatus[0].status, status.systemStatus[0].details);
	            		throw new SystemException("The SD Server is not online.  See log for details");
	            	}
	            }
	            
	            // get lineup info
	            GiveFeedback(feedbackCallback, "Getting channel information...");
	            List<LineupInfoResponse> llir = wc.getAssignedLineupInfoList(tr);
	            // map is keyed by stationId or externalId... the number that SD uses to identify the channel
	            Dictionary<string,ImportGuideChannel> digc = ChannelFactory.makeImportChannelMap(llir, ConfigInstance.Current.ChannelNameFormat);
	            
	            // SD asks that no more than 5000 stations be queried at once
	            // ignore this for now as not many will have 5000+ channels
	            GiveFeedback(feedbackCallback, "Getting schedules...");
	            Logger.Info("Get schedules for {0} days", ConfigInstance.Current.NrOfDaysToImport);
	            List<SchedulesResponse> lsr = wc.getSchedules(tr, stationIdList, ConfigInstance.Current.NrOfDaysToImport);
	            
	
	            	
	        	// 1) Get all programs at once
	        	
	        	
	            // make a global HashSet of program IDs
	            HashSet<string> globalProgramSet = new HashSet<string>();
	            foreach (SchedulesResponse sr in lsr) {
	            	foreach (SchedulesResponse.Program pg in sr.programs) {
	            		// we only get a programID once, even if it's on multiple schedules
	        			globalProgramSet.Add(pg.programID);
	            	}
	            }
	            
	            Logger.Info("There are {0} programs in this timeframe", globalProgramSet.Count);
	            
	            // take the programs in chunks of _programBlockSize... value below 5000
	            // SD won't allow a query of more than 5000 programs at once
	            int progLoops = globalProgramSet.Count / _programBlockSize;
	            int progsInFinalLoop = globalProgramSet.Count % _programBlockSize;
	            
	            if (progsInFinalLoop > 0) {
	            	progLoops++;
	            }
	            
	            // this map will contain *all* program responses across all stations
	            Dictionary<string,ProgramResponseInstance> globalProgramResponseMap = new Dictionary<string, ProgramResponseInstance>();
	            
	            // keep a list for navigating in for loop
	            List<string> globalProgramList = new List<string>();
	            globalProgramList.AddRange(globalProgramSet);
	            
	            GiveFeedback(feedbackCallback,"About to get program information for all programs...");
	                         
	            for (int i = 0; i < progLoops; i++) {
	            	// is this the last loop?
	            	int blockSize = _programBlockSize;
	            	if (i == progLoops - 1) {
	            		blockSize = progsInFinalLoop;
	            	}
	            	string message = string.Format("Getting program information for items {0} to {1}", 
	            	            i * _programBlockSize + 1, 
	            	            (i * _programBlockSize) + blockSize);
	            	GiveFeedback(feedbackCallback, message);
	            	
	            	List<string> programSubset = globalProgramList.GetRange(i * _programBlockSize, blockSize);
	            	
	            	List<ProgramResponseInstance> lpri = wc.getPrograms(tr, programSubset);
	            	
	            	foreach (ProgramResponseInstance pri in lpri) {
	            		globalProgramResponseMap.Add(pri.programID, pri);
	            	}
	            	
	            }
	            
	            // tackle each station at a time... 
	            int count = 1;
	            foreach (SchedulesResponse sr in lsr) {
		            
	            	keepImportServiceAliveCallback();
	            	
	            	List<GuideProgram> lgp = new List<GuideProgram>();
	            	try {
	            		// take the schedules and programs and create the guide programs to put into Argus
	            		// in a try block to catch issues with programfactory
	            		lgp = ProgramFactory.makeGuidePrograms(globalProgramResponseMap, sr);
	            	} catch (Exception ex) {
	            		Logger.Error("Could not extract program information: {0}", ex.StackTrace);
						throw;
	            	}
	            	ImportGuideChannel currentGuideChannel = digc[sr.stationID];
	            	
	            	double percentDone = 100 * ((double)count/lsr.Count);
	            	
	            	int percentInt = Convert.ToInt32(percentDone);
	            	
	            	if (progressCallback != null)
	            	{
	            		progressCallback(percentInt);
	            	}
	            	
	            	string programMessage =
	            		string.Format("Processing station: {0}, ID: {1}, Program Count: {2}, {3} done", 
	            		              digc[sr.stationID].ChannelName, 
	            		              sr.stationID, 
	            		              sr.programs.Count,
	            		              string.Format("{0}%",percentInt));
	            	
	            	GiveFeedback(feedbackCallback, programMessage);
	            	
	            	// save the programs in the Argus db
	            	importDataCallback(
	            		currentGuideChannel, 
	            		ChannelType.Television, 
	            		lgp.ToArray(), 
	            		ConfigInstance.Current.UpdateChannelNames);
	            	
	
	            	count++;
	            }
		            
	            
	            DateTime endTime = DateTime.Now;
	            
	            TimeSpan ts = endTime.Subtract(importStart);
	            int seconds = ts.Seconds;
	           	int minutes = ts.Minutes;
	           	int hours = ts.Hours;
	            
	            Logger.Info("Import complete.  Took {0} hours, {1} minutes and {2} seconds", hours, minutes, seconds);
	            Logger.Info("Channels processed: {0}", lsr.Count);
	            
				GiveFeedback(feedbackCallback,"Completed SD JSON guide data import.");
			} catch (Exception ex) {
				Logger.Error("There was an error importing the guide data: {0}/n{1}", ex.Message, ex.StackTrace);
				throw;
			}
			
		}
		

		
		
		private List<string> getStationIdList(List<ImportGuideChannel> skipChannels)
		{
			List<ImportGuideChannel> allChannels = GuideChannelStore.Load(AvailableChannelsConfigFile);
			
			Dictionary<string, ImportGuideChannel> allChannelsMap = new Dictionary<string,ImportGuideChannel>();
			foreach (ImportGuideChannel igc in allChannels) {
			
				allChannelsMap.Add(igc.ExternalId, igc);
			}
			
			// remove skipped channels
			foreach (ImportGuideChannel igc in skipChannels) {
				allChannelsMap.Remove(igc.ExternalId);
			}
			
			List<string> channelsToImport = new List<string>();
			
			channelsToImport.AddRange(allChannelsMap.Keys);
			
			return channelsToImport;
			
		}
		private void GiveFeedback(FeedbackCallback feedbackCallback, string message)
        {
            Logger.Info(FormatForLogger(message));

            if(feedbackCallback != null)
            {
                feedbackCallback(message);
            }
        }
		
        private string FormatForLogger(string message)
        {
            return String.Format("{0} {1}", Name, message);
        }
                
        private string AvailableChannelsConfigFile
        {
            get
            {
                return Path.Combine(InstallationPath, "AvailableChannels.config");
            }
        }
        
        private string ProgramMD5ConfigFile
        {
            get
            {
                return Path.Combine(InstallationPath, "ProgramMD5Cache.config");
            }
        }
	}
	
	
	
	//            // I have 2 implementations for the import
//            // 1) get all the programs at once in blocks of 4000, then process them
//            // 2) get the programs on a per station basis
//            if (ConfigInstance.Current.GetAllProgramsAtOnce) {
//            } else {
//            	
//            	// 2) Get programs on a per-channel basis
//            	
//            	
//            	int programCount = 0;
//            	int count = 1;
//            	// get programs one station at a time
//	            foreach (SchedulesResponse sr in lsr) {
//	            
//	            	keepImportServiceAliveCallback();
//	            	
//		            HashSet<string> programIdList = new HashSet<string>();
//		            foreach (SchedulesResponse.Program pg in sr.programs) {
//		            	programIdList.Add(pg.programID);
//		            }
//		            GiveFeedback(feedbackCallback, "Getting program information...");
//		            
//		            programCount += programIdList.Count;
//		            
//		            // get the program information
//	            	List<ProgramResponseInstance> lpri = wc.getPrograms(tr, programIdList);
//	            	
//	            	string programMessage =
//	            		string.Format("Station: {0}, ID: {1}, Program Count: {2}", digc[sr.stationID].ChannelName, sr.stationID, lpri.Count);
//	            	
//	            	GiveFeedback(feedbackCallback, programMessage);
//	            	
//	            	List<GuideProgram> lgp = new List<GuideProgram>();
//	            	try {
//	            		// take the schedules and programs and create the guide programs to put into Argus
//	            		// in a try block to catch issues with programfactory
//	            		lgp = ProgramFactory.makeGuidePrograms(lpri, sr);
//	            	} catch (Exception ex) {
//	            		Logger.Error("Could not extract program information: {0}", ex.StackTrace);
//						throw;
//	            	}
//	            	ImportGuideChannel currentGuideChannel = digc[sr.stationID];
//	            	
//	            	
//	            	// save the programs in the Argus db
//	            	importDataCallback(
//	            		currentGuideChannel, 
//	            		ChannelType.Television, 
//	            		lgp.ToArray(), 
//	            		ConfigInstance.Current.UpdateChannelNames);
//	            	
//	            	if (progressCallback != null)
//	            	{
//	            		int percentDone = (100 * count/lsr.Count);
//	            		Logger.Info("Continuing import... {0}% complete", percentDone);
//	            		progressCallback(percentDone);
//	            	}
//	            	count++;
//	            }
//            	Logger.Info("Processed {0} programs", programCount);
//            }

	
}

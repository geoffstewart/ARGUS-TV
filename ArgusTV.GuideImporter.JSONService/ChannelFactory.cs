
using System;
using System.Collections.Generic;
using ArgusTV.GuideImporter.Interfaces;
using ArgusTV.GuideImporter.JSONService.Entities;
using ArgusTV.Common.Logging;

namespace ArgusTV.GuideImporter.JSONService
{
	/// <summary>
	/// Description of ChannelFactory.
	/// </summary>
	public class ChannelFactory
	{
		
		public static List<ImportGuideChannel> makeImportChannels(LineupInfoResponse lir, string channelNameFormat) 
		{
			List<ImportGuideChannel> ligc = new List<ImportGuideChannel>();
			Dictionary<string, ImportGuideChannel> migc = new Dictionary<string, ImportGuideChannel>();
			
			// make a real map from the map "array"
//			Dictionary<string,string> channelMap = new Dictionary<string, string>();
//			foreach (LineupInfoResponse.ChannelMap cm in lir.map) {
//				if (!channelMap.ContainsKey(cm.stationID)) {
//					if (lir.metadata.transport != null && lir.metadata.transport.ToLower().Equals("antenna")) {
//						// Over the air channels have a uhfVhf channel and/or atscMajor/atsc minor
//						//
//						// example:
//						// sometimes, uvfVhf is only present, sometimes just atsc stuff is present
//						//{
//						//		"stationID": "24504",
//            			//		"uhfVhf": 9,
//            			// 		"atscMajor": 9,
//            			//		"atscMinor": 9
//        				//},
//        				
//        				
//        				if (cm.atscMinor != null && cm.atscMajor != null && cm.uhfVhf != null) {
//        					// all three options are there
//        					if (ConfigInstance.Current.AntennaUseUhfVhfChannel) {
//        						channelMap.Add(cm.stationID, cm.uhfVhf);
//	        				} else {
//	        					channelMap.Add(cm.stationID, cm.atscMajor + "." + cm.atscMinor);
//	        				}
//        				} else if (cm.uhfVhf != null) {
//    						channelMap.Add(cm.stationID, cm.uhfVhf);
//        				} else if (cm.atscMinor != null && cm.atscMajor != null) {
//        					channelMap.Add(cm.stationID, cm.atscMajor + "." + cm.atscMinor);
//        				} else {
//        					Logger.Info("There was a problem picking a Logical Channel Number for station ID: {0}", cm.stationID);
//        					
//        				}
//					} else {
//						channelMap.Add(cm.stationID, cm.channel);
//					}
//				}
//			}
			
			Dictionary<string,string> channelMap = makeChannelMap(lir);

			foreach (LineupInfoResponse.Station station in lir.stations) {
				ImportGuideChannel igc = new ImportGuideChannel();
				igc.ExternalId = station.stationID;
				igc.ChannelName = makeChannelName(station, channelMap, channelNameFormat);
				if (!migc.ContainsKey(station.stationID)) {
					migc.Add(station.stationID, igc);
				}
				string lcn = channelMap[station.stationID];
				if (lcn != null && lcn.Contains(".")) {
					string[] lcnArray = lcn.Split('.');
					igc.LogicalChannelNumber = Int32.Parse(lcnArray[0]);
				} else {
					igc.LogicalChannelNumber = Int32.Parse(lcn);
				}
			}
			ligc.AddRange(migc.Values);
			return ligc;
			
		}
		
		public static Dictionary<string, string> makeChannelMap(LineupInfoResponse lir) {
			Dictionary<string,string> channelMap = new Dictionary<string, string>();
			
			foreach (LineupInfoResponse.ChannelMap cm in lir.map) {
				if (!channelMap.ContainsKey(cm.stationID)) {
					if (lir.metadata.transport != null && lir.metadata.transport.ToLower().Equals("antenna")) {
						// Over the air channels have a uhfVhf channel and/or atscMajor/atsc minor
						//
						// example:
						// sometimes, uvfVhf is only present, sometimes just atsc stuff is present
						//{
						//		"stationID": "24504",
            			//		"uhfVhf": 9,
            			// 		"atscMajor": 9,
            			//		"atscMinor": 9
        				//},
        				
        				
        				if (cm.atscMinor != null && cm.atscMajor != null && cm.uhfVhf != null) {
        					// all three options are there
        					if (ConfigInstance.Current.AntennaUseUhfVhfChannel) {
        						channelMap.Add(cm.stationID, cm.uhfVhf);
	        				} else {
	        					channelMap.Add(cm.stationID, cm.atscMajor + "." + cm.atscMinor);
	        				}
        				} else if (cm.uhfVhf != null) {
    						channelMap.Add(cm.stationID, cm.uhfVhf);
        				} else if (cm.atscMinor != null && cm.atscMajor != null) {
        					channelMap.Add(cm.stationID, cm.atscMajor + "." + cm.atscMinor);
        				} else {
        					Logger.Info("There was a problem picking a Logical Channel Number for station ID: {0}", cm.stationID);
        					
        				}
					} else {
						channelMap.Add(cm.stationID, cm.channel);
					}
				}
			}
			
			return channelMap;
		}
		
		
		public static Dictionary<string, ImportGuideChannel> makeImportChannelMap(LineupInfoResponse lir, string channelNameFormat) {
			Dictionary<string, ImportGuideChannel> digc = new Dictionary<string, ImportGuideChannel>();
			
			List<ImportGuideChannel> ligc = makeImportChannels(lir, channelNameFormat);
			
			foreach (ImportGuideChannel igc in ligc) {
				digc.Add(igc.ExternalId, igc);
			}
			
			return digc;
		}
		
		public static Dictionary<string, ImportGuideChannel> makeImportChannelMap(List<LineupInfoResponse> llir, string channelNameFormat) {
			Dictionary<string, ImportGuideChannel> digc = new Dictionary<string, ImportGuideChannel>();
			
			foreach (LineupInfoResponse lir in llir) {
				Dictionary<string, ImportGuideChannel> localDigc = makeImportChannelMap(lir, channelNameFormat);
				
				foreach (string stationId in localDigc.Keys) {
					if (!digc.ContainsKey(stationId)) {
						digc.Add(stationId, localDigc[stationId]);
					}
				}
			}
			
			return digc;
		}
		
		public static string makeChannelName(LineupInfoResponse.Station station, Dictionary<string, string> channelMap, string channelNameFormat) 
		{
			string localCmf = (string)channelNameFormat.Clone();
			
	      	localCmf = localCmf.Replace("{Callsign}", station.callsign);
            localCmf = localCmf.Replace("{Name}", station.name);
            string lcn = channelMap[station.stationID];
            if (lcn == null) {
            	Logger.Info("The channel map has no Logical Channel Number for stationId: {0}", station.stationID);
            } else {
            	Logger.Info("Replace LCN {0} in map for stationId: {1}", lcn, station.stationID);
            	int lcnNum;
            	bool worked = Int32.TryParse(lcn, out lcnNum);
            	if (worked) {
            		localCmf = localCmf.Replace("{LogicalChannelNumber}", lcnNum.ToString());
            	} else {
            		//lcn may have a dot in it
            		localCmf = localCmf.Replace("{LogicalChannelNumber}", lcn);
            	}
            }
            
            return localCmf;
		}
		
		public ChannelFactory()
		{
		}
	}
}

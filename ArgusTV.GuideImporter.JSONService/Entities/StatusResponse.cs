/*
 * Created by SharpDevelop.
 * User: geoff
 * Date: 11/15/2014
 * Time: 9:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of StatusResponse.
	/// {
	//    "account": {
	//        "expires": "2014-06-28T05:16:29Z",
	//        "messages": [],
	//        "maxLineups": 16,
	//        "nextSuggestedConnectTime": "2014-05-20T16:32:12Z"
	//    },
	//    "lineups": [
	//        {
	//            "ID": "USA-OTA-60030",
	//            "modified": "2014-05-17T15:47:42Z",
	//            "uri": "/20140530/lineups/USA-OTA-60030"
	//        },
	//        {
	//            "ID": "USA-OTA-60654",
	//            "modified": "2014-05-15T17:41:11Z",
	//            "uri": "/20140530/lineups/USA-OTA-60654"
	//        },
	//        {
	//            "ID": "USA-PA37765-QAM",
	//            "modified": "2014-05-06T14:50:02Z",
	//            "uri": "/20140530/lineups/USA-PA37765-QAM"
	//        }
	//    ],
	//    "lastDataUpdate": "2014-05-19T14:44:34Z",
	//    "notifications": [],
	//    "systemStatus": [
	//        {
	//            "date": "2012-12-17T16:24:47Z",
	//            "status": "Online",
	//            "details": "All servers running normally."
	//        }
	//    ],
	//    "serverID": "AWS-SD-web.1",
	//    "code": 0
	//}
	/// </summary>
	public class StatusResponse
	{
		public class Account
		{
		    public string expires { get; set; }
		    public List<object> messages { get; set; }
		    public int maxLineups { get; set; }
		    public string nextSuggestedConnectTime { get; set; }
		}
		
		public class Lineup
		{
		    public string ID { get; set; }
		    public string modified { get; set; }
		    public string uri { get; set; }
		}
		
		public class SystemStatus
		{
		    public string date { get; set; }
		    public string status { get; set; }
		    public string details { get; set; }
		}
	
	
	    public Account account { get; set; }
	    public List<Lineup> lineups { get; set; }
	    public string lastDataUpdate { get; set; }
	    public List<object> notifications { get; set; }
	    public List<SystemStatus> systemStatus { get; set; }
	    public string serverID { get; set; }
	    public int code { get; set; }
	}
}

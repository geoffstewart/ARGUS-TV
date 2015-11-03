
using System;
using System.Collections.Generic;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of LineupInfoResponse.
	/// </summary>
	public class LineupInfoResponse
	{
		public class ChannelMap
		{
		    public string stationID { get; set; }
		    public string channel { get; set; }
		    public string uhfVhf { get; set; }
            public string atscMajor { get; set; }
            public string atscMinor { get; set; }
		}
		
		public class Logo
		{
		    public string URL { get; set; }
		    public int height { get; set; }
		    public int width { get; set; }
		    public string md5 { get; set; }
		}
		
		public class Broadcaster
		{
		    public string city { get; set; }
		    public string state { get; set; }
		    public string postalcode { get; set; }
		    public string country { get; set; }
		}
		
		public class Station
		{
		    public string stationID { get; set; }
		    public string name { get; set; }
		    public string callsign { get; set; }
		    public string affiliate { get; set; }
		    public List<string> broadcastLanguage { get; set; }
		    public List<string> descriptionLanguage { get; set; }
		    public Broadcaster broadcaster { get; set; }
		    public Logo logo { get; set; }
		}
		
		public class Metadata
		{
		    public string lineup { get; set; }
		    public string modified { get; set; }
		    public string transport { get; set; }
		}


	    public List<ChannelMap> map { get; set; }
	    public List<Station> stations { get; set; }
	    public Metadata metadata { get; set; }

		public LineupInfoResponse()
		{
		}
	}
}


using System;
using System.Collections.Generic;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of SchedulesRequest.
	/// </summary>
	public class SchedulesRequestInstance
	{
		public string stationID { get; set; }
    	public List<string> date { get; set; }
		
    	public SchedulesRequestInstance()
		{
		}
	}
}

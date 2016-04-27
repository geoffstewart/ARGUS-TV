
using System;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of SchedulesRequest.
	/// </summary>
	public class SchedulesRequestInstance
	{
		public string stationID { get; set; }
    	public int days { get; set; }
		
    	public SchedulesRequestInstance()
		{
		}
	}
}


using System;
using System.Collections.Generic;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of SchedulesResponse.
	/// </summary>
	public class SchedulesResponse
	{
		public class Rating
		{
		    public string body { get; set; }
		    public string code { get; set; }
		}
		
		public class Multipart
		{
		    public int partNumber { get; set; }
		    public int totalParts { get; set; }
		}
		
		public class Program
		{
		    public string programID { get; set; }
		    public string airDateTime { get; set; }
		    public int duration { get; set; }
		    public string md5 { get; set; }
		    public List<string> audioProperties { get; set; }
		    public List<Rating> ratings { get; set; }
		    public bool? @new { get; set; }
		    public bool? signed { get; set; }
		    public string liveTapeDelay { get; set; }
		    public bool? cableInTheClassroom { get; set; }
		    public bool? catchup { get; set; }
	     	public bool? continued { get; set; } 
	     	public bool? educational { get; set; }
	     	public bool? joinedInProgress { get; set; }
	     	public bool? leftInProgress { get; set; }
	     	public bool? premiere { get; set; }
	     	public bool? programBreak { get; set; }
	     	public bool? repeat { get; set; }
	     	public bool? subjectToBlackout { get; set; }
	     	public bool? timeApproximate { get; set; }
	     	public string isPremiereOrFinale { get; set; }
     	    public Multipart multipart { get; set; }
		}
		
		public class Metadata
		{
		    public string modified { get; set; }
		    public string md5 { get; set; }
		    public string startDate { get; set; }
		    public string endDate { get; set; }
		    public int days { get; set; }
		}
	    public string stationID { get; set; }
	    public List<Program> programs { get; set; }
	    public Metadata metadata { get; set; }
	
		public SchedulesResponse()
		{
		}
	}
}


using System;
using System.Collections.Generic;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of ProgramResponseInstance.
	/// </summary>
	public class ProgramResponseInstance
	{
		public class Title
		{
		    public string title120 { get; set; }
		}
		
		public class EventDetails
		{
		    public string subType { get; set; }
		}
		
		public class Description1000
		{
		    public string descriptionLanguage { get; set; }
		    public string description { get; set; }
		}
		public class Description100
		{
		    public string descriptionLanguage { get; set; }
		    public string description { get; set; }
		}
		public class Descriptions
		{
		    public List<Description1000> description1000 { get; set; }
		    public List<Description100> description100 { get; set; }
		}
		
		public class Tribune
		{
		    public int season { get; set; }
		    public int episode { get; set; }
		}
		
		public class Metadata
		{
		    public Tribune Tribune { get; set; }
		}
		
		public class Cast
		{
		    public string personId { get; set; }
		    public string nameId { get; set; }
		    public string name { get; set; }
		    public string role { get; set; }
		    public string billingOrder { get; set; }
		}
		
		public class Crew
		{
		    public string personId { get; set; }
		    public string nameId { get; set; }
		    public string name { get; set; }
		    public string role { get; set; }
		    public string billingOrder { get; set; }
		}
		
		
	    public string programID { get; set; }
	    public List<Title> titles { get; set; }
	    public EventDetails eventDetails { get; set; }
	    public Descriptions descriptions { get; set; }
	    public string originalAirDate { get; set; }
	    public List<string> genres { get; set; }
	    public string episodeTitle150 { get; set; }
	    public List<Metadata> metadata { get; set; }
	    public List<Cast> cast { get; set; }
	    public List<Crew> crew { get; set; }
	    public string showType { get; set; }
	    public bool hasImageArtwork { get; set; }
	    public string md5 { get; set; }

		public ProgramResponseInstance()
		{
		}
	}
}


using System;
using System.Collections.Generic;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of LineupsResponse.
	/// </summary>
	public class AssignedLineupsResponse
	{
		public class Lineup
		{
		    public string name { get; set; }
		    public string type { get; set; }
		    public string location { get; set; }
		    public string uri { get; set; }

		}
				
	    public string serverID { get; set; }
	    public string datetime { get; set; }
	    public List<Lineup> lineups { get; set; }
		
	}
}

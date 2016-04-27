
using System;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of AssignLineupsResponse.
	/// </summary>
	public class AlterLineupsResponse
	{
		public string response { get; set; }
	    public int code { get; set; }
	    public string serverID { get; set; }
	    public string message { get; set; }
	    public int changesRemaining { get; set; }
	    public string datetime { get; set; }
	    
		public AlterLineupsResponse()
		{
		}
	}
}

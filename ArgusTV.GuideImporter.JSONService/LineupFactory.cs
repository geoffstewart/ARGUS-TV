
using System;
using System.Collections.Generic;
using ArgusTV.GuideImporter.JSONService.Entities;

namespace ArgusTV.GuideImporter.JSONService
{
	/// <summary>
	/// Convert various webservice lineup structures to a single GUI version of a Lineup
	/// </summary>
	public class LineupFactory
	{
		
		public class GuiLineup 
		{
			public string name {get; set;}
			public string uri {get; set;}
			public string displayName
			{
				get 
		    	{
		    		return name + " - " + uri;
		    	}
			}
			public GuiLineup(string n, string u) {
				this.name = n;
				this.uri = u;
			}
		}
		
		public static List<GuiLineup> makeGuiLineupList(List<AssignedLineupsResponse.Lineup> lineups) 
		{
			List<GuiLineup> guiLineups = new List<GuiLineup>();
			
			foreach (AssignedLineupsResponse.Lineup lu in lineups) {
				GuiLineup gl = new GuiLineup(lu.name, lu.uri);
				guiLineups.Add(gl);
			}
			return guiLineups;
		}
		
		public static List<GuiLineup> makeGuiLineupList(List<HeadendResponseInstance.Lineup> lineups) 
		{
			List<GuiLineup> guiLineups = new List<GuiLineup>();
			
			foreach (HeadendResponseInstance.Lineup lu in lineups) {
				GuiLineup gl = new GuiLineup(lu.name, lu.uri);
				guiLineups.Add(gl);
			}
			return guiLineups;
		}
		
		public LineupFactory()
		{
		}
	}
}

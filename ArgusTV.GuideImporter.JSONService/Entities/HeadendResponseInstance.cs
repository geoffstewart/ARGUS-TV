﻿
using System;
using System.Collections.Generic;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of HeadendResponseInstance.
	/// </summary>
	public class HeadendResponseInstance
	{
		public class Lineup
		{
		    public string name { get; set; }
		    public string uri { get; set; }
		}
		
		public string type { get; set; }
	    public string location { get; set; }
	    public List<Lineup> lineups { get; set; }
	    
		public HeadendResponseInstance()
		{
		}
	}
}

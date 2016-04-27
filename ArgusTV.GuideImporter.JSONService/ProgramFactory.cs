
using System;
using System.Collections.Generic;
using ArgusTV.DataContracts;
using ArgusTV.GuideImporter.JSONService.Entities;
using ArgusTV.Common.Logging;
using Newtonsoft.Json;

namespace ArgusTV.GuideImporter.JSONService
{
	/// <summary>
	/// Description of ProgramFactory.
	/// </summary>
	public class ProgramFactory
	{
		
		public static List<GuideProgram> makeGuidePrograms(
			List<ProgramResponseInstance> lpri, 
			SchedulesResponse sr) {
			
			// make a map for program reference
			Dictionary<string, ProgramResponseInstance> dpri = new Dictionary<string, ProgramResponseInstance>();
			foreach (ProgramResponseInstance pri in lpri) {
				dpri.Add(pri.programID, pri);
			}
			
			List<GuideProgram> lgp = makeGuidePrograms(dpri, sr);
			
			return lgp;
		}
		
		public static List<GuideProgram> makeGuidePrograms(
			Dictionary<string, ProgramResponseInstance> dpri,
			SchedulesResponse sr) {
			
			List<String> errors = new List<String>();
			List<GuideProgram> lgp = new List<GuideProgram>();
			
			foreach (SchedulesResponse.Program srp in sr.programs) {
				if (dpri.ContainsKey(srp.programID)) {
					try {
						GuideProgram gp = makeGuideProgram(sr, srp, dpri[srp.programID]);
						lgp.Add(gp);
					} catch (Exception ex) {
						string json = JsonConvert.SerializeObject(srp);
						errors.Add(json);
					}
				}
			}
			
			if (errors.Count > 0) {
				Logger.Error("There were {0} programs that could not be processed.  Continuing without them.", errors.Count);
				foreach (string json in errors) {
					Logger.Error("Program failed in factory:\n{0}", json);
				}
			}
			
			return lgp;
			
		}
		
		public static GuideProgram makeGuideProgram(
			SchedulesResponse sr,
			SchedulesResponse.Program srp,
			ProgramResponseInstance pri)
		{
			try {
				var gp = new GuideProgram();
				
				
				gp.Title = makeTitle(pri);
				gp.SubTitle = pri.episodeTitle150;
				gp.Description = makeDescription(pri);
				DateTime startTime = DateTime.Parse(srp.airDateTime);
				gp.StartTime = startTime;
				gp.StopTime = startTime.AddSeconds(srp.duration);
				gp.EpisodeNumber = makeEpisodeNumber(pri);
				gp.SeriesNumber = makeSeriesNumber(pri);
				gp.EpisodeNumberDisplay = makeEpisodeNumberDisplay(pri);
				gp.Rating = makeRatings(srp);
				// srp.Premiere is for miniseries or movies
				if (srp.premiere.HasValue) {
					gp.IsPremiere = (true == srp.premiere.Value);
				}
				// so use srp.new to determine if it's a repeat... srp.repeat is only for sporting events
				gp.IsRepeat = true;  // default to a repeat
				if (srp.@new.HasValue) {
					gp.IsRepeat = !srp.@new.Value;
				}
				gp.PreviouslyAiredTime = DateTime.Parse(srp.airDateTime);
				gp.Actors = makeActors(pri);
				gp.Category = makeCategory(pri);
				gp.Directors = makeDirectors(pri);
				
				return gp;
			} catch (Exception ex) {
				Logger.Error("Error while creating guide program instances: {0}\n{1}",ex.Message, ex.StackTrace);
				throw;
			}
		}
		
		private static string makeCategory(ProgramResponseInstance pri) {
			string cat = "";
			
			if (pri.genres == null) {
				return cat;
			}
		 	foreach (string genre in pri.genres) {
				cat += genre;
				cat += ",";
		 	}
			
			if (cat.Length > 0) {
				// remove trailing comma
				cat = cat.Remove(cat.Length - 1);
			}
			
			return cat;
		}
		private static string[] makeActors(ProgramResponseInstance pri) {
			List<string> actors = new List<string>();
			
			if (pri.cast == null) {
				return actors.ToArray();
			}
			foreach (ProgramResponseInstance.Cast cast in pri.cast) {
				actors.Add(cast.name);
			}
			return actors.ToArray();
			
		}
		private static string[] makeDirectors(ProgramResponseInstance pri) {
			List<string> directors = new List<string>();
			
			if (pri.crew == null) {
				return directors.ToArray();
			}
			foreach (ProgramResponseInstance.Crew crew in pri.crew) {
				if (crew.role.ToLower().StartsWith("direct")) {
					directors.Add(crew.name);
				}
			}
			return directors.ToArray();
			
		}
		private static string makeRatings(SchedulesResponse.Program srp) {
			string rating = "";
			
			if (srp.ratings != null && srp.ratings.Count > 0) {
				// just take the first one
				rating = srp.ratings[0].code;
			}
			return rating;
		}
		private static int makeSeriesNumber(ProgramResponseInstance pri) {
			int sn = 0;
			if (pri.metadata != null && pri.metadata.Count > 0) {
				// just take the first metadata
				ProgramResponseInstance.Tribune tri = pri.metadata[0].Tribune;
				sn = tri.season;
			}
			return sn;
		}
		private static string makeEpisodeNumberDisplay(ProgramResponseInstance pri) {
			string en = "";
			if (pri.metadata != null && pri.metadata.Count > 0) {
				// just take the first one
				ProgramResponseInstance.Tribune tri = pri.metadata[0].Tribune;
				en = String.Format("S{0:D2}E{1:D2}", tri.season, tri.episode);
			}
			return en;
		}
		
		private static int makeEpisodeNumber(ProgramResponseInstance pri) {
			int en = 0;
			if (pri.metadata != null && pri.metadata.Count > 0) {
				// just take the first one
				ProgramResponseInstance.Tribune tri = pri.metadata[0].Tribune;
				en = tri.episode;
			}
			return en;
		}
		
		private static string makeDescription(ProgramResponseInstance pri) 
		{
			if (pri.descriptions != null) {
				if (pri.descriptions.description1000 != null && pri.descriptions.description1000.Count > 0) {
					var ldesc = pri.descriptions.description1000;
					return ldesc[0].description;
				} else if (pri.descriptions.description100 != null && pri.descriptions.description100.Count > 0) {
					var ldesc = pri.descriptions.description100;
					return ldesc[0].description;
				} else {
					return "";
				}
				
			} else {
				// no description
				return "";
			}
		}
		private static string makeTitle(ProgramResponseInstance pri) {
			// titles is supposed to be mandatory
			if (pri.titles != null && pri.titles.Count == 1) 
			{
				ProgramResponseInstance.Title t = pri.titles[0];
				return t.title120;
			} else {
				Logger.Warn("Could not find title for program: {0}", pri.programID);
				return "Unknown title";
			}
		}
		
		public ProgramFactory()
		{
		}
	}
}

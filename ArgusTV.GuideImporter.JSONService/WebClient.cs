using System;
using System.Net;
using ArgusTV.GuideImporter.JSONService.Entities;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ArgusTV.Common.Logging;
using System.Collections.Generic;

namespace ArgusTV.GuideImporter.JSONService
{
	/// <summary>
	/// Description of WebClient.
	/// 
	/// Singleton to access the JSON webservice of SchedulesDirect.
	/// </summary>
	public class WebClient
	{
		const string baseUrlWithoutVersion = "https://json.schedulesdirect.org";
		const string baseUrl = baseUrlWithoutVersion + "/20141201/";
		const string userAgent = "argustv/1.0.0";
		const double tokenValidFor = -23; // really 24 hours, but remove case where it's close and make it 23
		
		private static WebClient _wc = null;
		private string _token = null;
		private DateTime tokenCreated {get; set;}
		
		public string Token {
			get
			{
				return this._token;
			}
		}
		
		public static WebClient getInstance() {
			if (_wc == null) {
				_wc = new WebClient();
			}
			return _wc;
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="token"></param>
		/// <param name="method"></param>
		/// <param name="url"></param>
		/// <param name="postData"></param>
		/// <returns></returns>
		private List<T> executeHttpRequestWithList<T>(string token, string method, string url, Object postData = null) {
			
			string jsonResp = callHttp(token, method, url, postData);
			
		
			// each item is on a line and is a valid JSON object
			// but, the whole response is NOT a valid JSON object.
			// Their attempt to handle a large response
			jsonResp = jsonResp.Trim();
			
			string[] jsonLines = jsonResp.Split('\n');
			
			List<T> listt = new List<T>();
			
			foreach (string jsonLine in jsonLines) {
				T pri = JsonConvert.DeserializeObject<T>(jsonLine);
				listt.Add(pri);
			}
			
			return listt;			
			
		}
		
		private T executeHttpRequest<T>(string token, string method, string url, Object postData = null) {
			
			string jsonResp = callHttp(token, method, url, postData);
			
			T resp = JsonConvert.DeserializeObject<T>(jsonResp);
			
		
			return resp;
			
			
		}
		
		private string callHttp(string token, string method, string url, Object postData = null) 
		{
			HttpWebRequest wr = (HttpWebRequest) WebRequest.Create(url);
				
			wr.Headers.Add("token", token);
			wr.UserAgent = userAgent;
			wr.Method = method;
			wr.Headers.Add("Accept-Encoding", "deflate");
//			wr.KeepAlive = false; // otherwise, there are intermittent errors with connections closing
			
			if (postData != null) {
				string json = JsonConvert.SerializeObject(postData,Formatting.Indented);
				
				if ("Debug".Equals(ConfigInstance.Current.LogLevel)) {
					Logger.Verbose("JSON Post Data: \n{0}", json);
				}
				
				Stream ds = wr.GetRequestStream();
				
				byte[] byteArray = Encoding.UTF8.GetBytes (json);
				ds.Write(byteArray, 0, byteArray.Length);
				ds.Close();
			}
			
			WebResponse response;
			bool error = false;
			try {
				if ("Debug".Equals(ConfigInstance.Current.LogLevel)) {
					Logger.Verbose("About to call {0} to {1}", method, url);
				}
				
				response = wr.GetResponse();
			} catch (System.Net.WebException wex) {
				// We get here when a login fails... but also if the HTTP connection was lost
				// if there's data, it's a normal process... if not, proceed with retry logic below
				Logger.Info("Got an HTTP error {0}\n{1}", wex.Message, wex.StackTrace);
				response = wex.Response;
				error = true;
				
				if (response == null) {
					// something went wrong with the HTTP connection... retry
					Logger.Error("There was no data in the error response... retrying");
					
					for (int i = 0; i < 3; i++) {
						try {
							response = wr.GetResponse();
							// we got here, so all good... break!
							Logger.Info("Retry number {0} worked... continuing", i);
							break;
						} catch (System.Net.WebException wex2) {
							Logger.Error("Retry {0} or 4 failed. Exception: {1}", i, wex2.StackTrace);
						}
					}
					
					// still here?  throw the exception
					Logger.Error("All retries failed.  Stopping");
					throw;
				}
			}
			
			Stream respStream = response.GetResponseStream();
			
			MemoryStream ms = new MemoryStream();
			respStream.CopyTo(ms);
			byte[] respBytes = ms.ToArray();
			String jsonResp = Encoding.UTF8.GetString(respBytes);
			
			if ("Debug".Equals(ConfigInstance.Current.LogLevel)) {
				Logger.Verbose("JSON Response: \n{0}", jsonResp);
			} else {
				Logger.Info("Got a reply of {0} bytes", respBytes.Length);
			}
			ms.Dispose();
			
			if (error) {
				Logger.Error("Got the following error response: \n{0}", jsonResp);
				ErrorResponse eResp = JsonConvert.DeserializeObject<ErrorResponse>(jsonResp);
				throw new WebClientException(eResp);
			}
			
			return jsonResp;
		}
		
		// send a request (if necessary) to get an authentication token from SD.org
		public string getToken(TokenRequest tr) {
			
			// if we already have a token, don't get a new one
			if (tokenStillValid()) {
				return this._token;
			}
			TokenResponse tresp = executeHttpRequest<TokenResponse>("", "POST", baseUrl + "token", tr);
		
			if (tresp.code == 0) {
				this._token = tresp.token;
				this.tokenCreated = DateTime.Now;
			} else {
				Logger.Error("Could not get token. Got this response: {0}", tresp.message);
				
			}

			return this._token;
		}
		
		private bool tokenStillValid() {
			if (this._token == null) {
				return false;
			}
			DateTime uhNow = DateTime.Now;
			if (uhNow.AddHours(tokenValidFor) > this.tokenCreated) {
				// token has expired
				return false;
			}
			
			return true;
			
		}
		
		// verify the status of the SD webservice before communicating with it
		public StatusResponse getStatus(TokenRequest tr) {
			StatusResponse sresp = new StatusResponse();
			
			string token = getToken(tr);
			
			sresp = executeHttpRequest<StatusResponse>(token, "GET", baseUrl + "status");
							
			return sresp;
			
		}
		
		// get choices for lineups available for a country and postal code
		/// <summary>
		///  requires a list of headends
		/// </summary>
		/// <param name="country"></param>
		/// <param name="postalcode"></param>
		/// <returns></returns>
		public List<HeadendResponseInstance> getHeadends(TokenRequest tr, string country, string postalcode) {
			List<HeadendResponseInstance> retList = new List<HeadendResponseInstance>();
			
			string token = getToken(tr);

			string url = baseUrl + "headends?country=" + country + "&postalcode=" + postalcode;
			
			retList = executeHttpRequest<List<HeadendResponseInstance>>(token, "GET", url);
							
			
			return retList;
		}
		
		
		/// <summary>
		/// Alternative of getHeadends if all you want is the lineup options
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="country"></param>
		/// <param name="postalcode"></param>
		/// <returns></returns>
		public List<HeadendResponseInstance.Lineup> getLineupsFromHeadends(TokenRequest tr, string country, string postalcode) 
		{
			List<HeadendResponseInstance.Lineup> lineups = new List<HeadendResponseInstance.Lineup>();
			
			List<HeadendResponseInstance> dic = getHeadends(tr, country, postalcode);
			
			foreach(var he in dic) {
				foreach(var lu in he.lineups) {
					Logger.Info("This is a lineup: {0} - {1}", lu.name, lu.uri);
					lineups.Add(lu);
				}
			}
			return lineups;
		}
		
		/// <summary>
		/// see what lineups are assigned to a user
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>
		public List<AssignedLineupsResponse.Lineup> getAssignedLineups(TokenRequest tr) {
			
			List<AssignedLineupsResponse.Lineup> lineups = new List<AssignedLineupsResponse.Lineup>();
			
			string token = getToken(tr);
			
			string url = baseUrl + "lineups";
			
			// special case here... catch exception for no assigned lineups
			try 
			{
				AssignedLineupsResponse lur = executeHttpRequest<AssignedLineupsResponse>(token, "GET", url);
							
				if (lur != null) {
					lineups = lur.lineups;
				}
			} catch (WebClientException wce) {
				if (wce.Error.code == 4102) {
					// not a error... just no assigned lineups
					return lineups;
				}
				throw;
			}
				
			
			return lineups;
		}
		
		public List<LineupInfoResponse> getAssignedLineupInfoList(TokenRequest tr) 
		{
			
			List<LineupInfoResponse> llir = new List<LineupInfoResponse>();
			
			List<AssignedLineupsResponse.Lineup> lineups = getAssignedLineups(tr);
			
			foreach (AssignedLineupsResponse.Lineup lu in lineups) {
				LineupInfoResponse lir = getLineupInfo(tr, lu.uri);
				llir.Add(lir);
			}
			
			return llir;
		}
		
		
		/// <summary>
		/// add a lineup to a user
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="lineupRelativeUrl"></param>
		/// <returns></returns>
		public AlterLineupsResponse addLineup(TokenRequest tr, string lineupRelativeUrl) {
			AlterLineupsResponse alr = new AlterLineupsResponse();
			
			string token = getToken(tr);
			
			// assuming lineupRelativeUrl starts with /
			string url = baseUrlWithoutVersion + lineupRelativeUrl;
			
			alr = executeHttpRequest<AlterLineupsResponse>(token, "PUT", url);
			
			if (alr.code != 0) {
				Logger.Error("Unable to assign lineup: {0}", alr.message);
			} else {
				Logger.Info("Lineup Added: {0}", lineupRelativeUrl);
			}
			
			return alr;
		}
		
		/// <summary>
		/// remove a lineup from being assigned to a user
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="lineupRelativeUrl"></param>
		/// <returns></returns>
		public AlterLineupsResponse deleteLineup(TokenRequest tr, string lineupRelativeUrl) {
			AlterLineupsResponse alr = new AlterLineupsResponse();
			
			string token = getToken(tr);
			
			// assuming lineupRelativeUrl starts with /
			string url = baseUrlWithoutVersion + lineupRelativeUrl;
			
			alr = executeHttpRequest<AlterLineupsResponse>(token, "DELETE", url);
			
			if (alr.code != 0) {
				Logger.Error("Unable to delete lineup: {0}", alr.message);
			} else {
				Logger.Info("Lineup Removed");
			}
				
			return alr;
		}
		
		
		public LineupInfoResponse getLineupInfo(TokenRequest tr, string lineupRelativeUrl) {
			LineupInfoResponse lir = new LineupInfoResponse();
			
			string token = getToken(tr);
			
			// assuming lineupRelativeUrl starts with /
			string url = baseUrlWithoutVersion + lineupRelativeUrl;
			
			lir = executeHttpRequest<LineupInfoResponse>(token, "GET", url);
				
			
			
			return lir;
			
		}
		
		/// <summary>
		/// Get a list of programsIds available for the stations passed in.
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="stationIdList"></param>
		/// <param name="days"></param>
		/// <returns></returns>
		public List<SchedulesResponse> getSchedules(TokenRequest tr, List<string> stationIdList, int days) {
			
			List<SchedulesResponse> lsr = new List<SchedulesResponse>();
			
			string token = getToken(tr);
			
			List<SchedulesRequestInstance> req = new List<SchedulesRequestInstance>();
			
			foreach (string stationId in stationIdList) {
				SchedulesRequestInstance sri = new SchedulesRequestInstance();
				sri.stationID = stationId;
				// don't set date... days is no longer part of structure
//				sri.days = days;
				
				req.Add(sri);
			}
			
			lsr = executeHttpRequest<List<SchedulesResponse>>(token, "POST", baseUrl + "schedules", req);
				
			return lsr;
		}
		
		/// <summary>
		/// The response of "programs" is not a valid JSON object, but a series of lines, each 
		/// of which is a valid JSON object.  For that reason, the HTTP works is embedded in this 
		/// method, too.
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="programIds"></param>
		/// <returns></returns>
		public List<ProgramResponseInstance> getPrograms(TokenRequest tr, ICollection<string> programIds) {
			List<ProgramResponseInstance> lpri = new List<ProgramResponseInstance>();
			
			string token = getToken(tr);
			
			lpri = executeHttpRequest<List<ProgramResponseInstance>>(token, "POST", baseUrl + "programs", programIds);
			
			return lpri;
			
		}
		
		
	}
}


using System;
using NUnit.Framework;
using ArgusTV.GuideImporter.JSONService.Entities;
using System.Collections.Generic;
using ArgusTV.Common.Logging;

namespace ArgusTV.GuideImporter.JSONService.test
{
	[TestFixture]
	public class WebClientTest
	{
		const string username = "geoffstewart";
		const string password = "600shadow";
		
		[SetUp]
		public void setup() {
			Logger.SetLogFilePath("C:\\Users\\geoff\\Documents\\git\\ARGUS-TV\\ArgusTV.GuideImporter.JSONService\\bin\\Debug",System.Diagnostics.SourceLevels.All);
		}
		
		
		[Test]
		public void TestAAAGetToken()
		{
			/// <summary>
			///  Called AAAGetToken so it gets run first.
			/// </summary>
			WebClient wc = WebClient.getInstance();
			
			Assert.IsNull(wc.Token);
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			string tok = wc.getToken(tr);
			
			Assert.IsNotNull(tok);
			
			string tok2 = wc.getToken(tr);
			
			Assert.AreEqual(tok, tok2);
			
			Logger.Info("Token worked");
		}
		
		[Test]
		public void TestGetStatus()
		{
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			string tok = wc.getToken(tr);
			StatusResponse sr = wc.getStatus(tr);
			
			Assert.NotNull(sr);
		
			Assert.NotNull(sr.systemStatus[0]);
			
			Assert.AreEqual("Online", sr.systemStatus[0].status);
			
		}
		
		[Test]
		public void TestGetHeadends()
		{
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			Dictionary<string, HeadendResponseInstance> headends = wc.getHeadends(tr, "CAN", "K1Z6R6");
			
			Assert.NotNull(headends);
		
			foreach(var he in headends.Values) {
				foreach(var lu in he.lineups) {
//					Logger.Info("This is a lineup: {0} - {1}", lu.name, lu.uri);
					System.Console.WriteLine("This is a lineup: {0} - {1}", lu.name, lu.uri);
				}
			}
			
			
		}
		
		[Test]
		public void TestLineups()
		{
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			List<HeadendResponseInstance.Lineup> lus = wc.getLineupsFromHeadends(tr, "CAN", "K1Z6R6");
			
			Assert.NotNull(lus);
		
			foreach(var lu in lus) {
				System.Console.WriteLine("This is a lineup: {0} - {1}", lu.name, lu.uri);
			}
			
			
		}
		
		[Test]
		public void TestAAGetAssignedLineups() {
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			List<AssignedLineupsResponse.Lineup> alr = wc.getAssignedLineups(tr);
			Assert.NotNull(alr);
		
			Assert.AreEqual(0, alr.Count);
			
			
		}
		
		[Test]
		public void TestAddLineupDeleteLineup() {
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			AlterLineupsResponse alr = wc.addLineup(tr, "/20140530/lineups/CAN-0005410-X");
			Assert.NotNull(alr);
		
			Assert.AreEqual(0, alr.code);
			
			List<AssignedLineupsResponse.Lineup> alrl = wc.getAssignedLineups(tr);
			Assert.NotNull(alrl);
		
			Assert.AreEqual(1, alrl.Count);
			
//			alr = wc.deleteLineup(tr, "/20140530/lineups/CAN-0005410-X");
//			Assert.NotNull(alr);
//		
//			Assert.AreEqual(0, alr.code);
		}
		
		[Test]
		public void TestGetLineupInfo() {
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			LineupInfoResponse lir = wc.getLineupInfo(tr, "/20140530/lineups/CAN-0005410-X");
			
			Assert.NotNull(lir);
			
		}
		
		[Test]
		public void TestGetSchedules() {
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			List<string> sil = new List<string>();
			sil.Add("10116");
			sil.Add("10127");
			
			List<SchedulesResponse> sr = wc.getSchedules(tr,sil, 2);
			
			Assert.NotNull(sr);
		}
		
		[Test]
		public void TestGetPrograms() {
			WebClient wc = WebClient.getInstance();
			
			TokenRequest tr = new TokenRequest();
			tr.username = username;
			tr.password = password;
			
			List<string> programIds = new List<string>();
			programIds.Add("EP016996020017");
			programIds.Add("EP007473660419");
			programIds.Add("EP015084700082");
			programIds.Add("EP012599310004");
			
			List<ProgramResponseInstance> lpri = wc.getPrograms(tr,programIds);
			
			Assert.NotNull(lpri);
			
			Assert.AreEqual(4, lpri.Count);
		}
	}
}

/*
 * Created by SharpDevelop.
 * User: geoff
 * Date: 11/15/2014
 * Time: 9:14 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of TokenResponse.
	/// {
	//    "code": 0,
	//    "message": "OK",
	//    "serverID": "AWS-SD-web.1",
	//    "token": "f3fca79989cafe7dead71beefedc812b"
	//   }
	/// </summary>
	public class TokenResponse
	{
	  	public int code { get; set; }
	    public string message { get; set; }
	    public string serverID { get; set; }
	    public string token { get; set; }
	}
}

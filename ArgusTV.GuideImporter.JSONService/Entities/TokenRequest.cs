/*
 */
using System;


namespace ArgusTV.GuideImporter.JSONService.Entities
{
	/// <summary>
	/// Description of TokenRequest.
	/// 
	/// example
	/// {"username":"rkulagow@rocketmail.com", "password":"sha1hexpassword"}
	/// 
	/// </summary>
	public class TokenRequest
	{
		
	    public string username { get; set; }
	    private string _password;
	    public string password { 
	    	get
	    	{
	    		return _password;
	    	}
	    	set
	    	{
	    		this._password = Util.Hash(value);
	    		this._password = this._password.ToLower();
	    	}
	    }
	    
	    public TokenRequest() {
	    	
	    }
	    public TokenRequest(string u, string p) {
	    	this.username = u;
	    	this.password = p;
	    }
	}
}

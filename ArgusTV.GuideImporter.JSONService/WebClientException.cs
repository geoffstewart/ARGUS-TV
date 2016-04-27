
using System;
using ArgusTV.GuideImporter.JSONService.Entities;

namespace ArgusTV.GuideImporter.JSONService
{
	/// <summary>
	/// Description of WebClientException.
	/// </summary>
	public class WebClientException : SystemException
	{
		public ErrorResponse Error { get; set; }
		
		public WebClientException(ErrorResponse er) {
			Error = er;
		}
		public WebClientException()
		{
		}
	}
}

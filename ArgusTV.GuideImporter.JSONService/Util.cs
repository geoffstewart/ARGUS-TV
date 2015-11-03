
using System;
using System.Security.Cryptography;
using System.Text;

namespace ArgusTV.GuideImporter.JSONService
{
	/// <summary>
	/// Description of Util.
	/// </summary>
	public class Util
	{
		
		public static string GetString(byte[] bytes)
		{
		    char[] chars = new char[bytes.Length / sizeof(char)];
		    System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		    return new string(chars);
		}
		
		public static string Hash(string input)
		{
		    using (SHA1Managed sha1 = new SHA1Managed())
		    {
		        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
		        var sb = new StringBuilder(hash.Length * 2);
		
		        foreach (byte b in hash)
		        {
		            // can be "x2" if you want lowercase
		            sb.Append(b.ToString("X2"));
		        }
		
		        return sb.ToString();
		    }
		}

	}
}

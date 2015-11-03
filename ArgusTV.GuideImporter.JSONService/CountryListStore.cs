
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace ArgusTV.GuideImporter.JSONService
{
	internal static class CountryListStore
    {
        #region Serialization

        public static Dictionary<string, string> Load(string fileName)
        {
        	Dictionary<string,string> countries = new Dictionary<string, string>();
            if (File.Exists(fileName))
            {
                using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
                {
                    XPathDocument xpathDocument = new XPathDocument(reader);
                    XPathNavigator navigator = xpathDocument.CreateNavigator();
                    XPathNodeIterator iterator = navigator.Select("/Countries/Country");
                    while (iterator.MoveNext())
                    {
                        string name = iterator.Current.GetAttribute("Name", String.Empty);
                        string code = iterator.Current.GetAttribute("Code", String.Empty);
                        countries.Add(code, name);
                    }
                }
            }
            return countries;
        }

     
        #endregion
    }
}

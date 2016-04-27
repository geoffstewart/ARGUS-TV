
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using ArgusTV.GuideImporter.Interfaces;
using ArgusTV.GuideImporter.JSONService.Entities;

namespace ArgusTV.GuideImporter.JSONService
{
	internal static class ProgramMD5Store
    {
        #region Serialization

        public static Dictionary<string, string> Load(string fileName)
        {
        	Dictionary<string, string> programsMD5Map = new Dictionary<string, string>();
            if (File.Exists(fileName))
            {
                using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
                {
                    XPathDocument xpathDocument = new XPathDocument(reader);
                    XPathNavigator navigator = xpathDocument.CreateNavigator();
                    XPathNodeIterator iterator = navigator.Select("/Programs/Program");
                    while (iterator.MoveNext())
                    {
                        string programId = iterator.Current.GetAttribute("ProgramId", String.Empty);
                        string md5 = iterator.Current.GetAttribute("MD5", String.Empty);
                        if (!programsMD5Map.ContainsKey(programId)) {
                        	programsMD5Map.Add(programId, md5);
                        }
                    }
                }
            }
            return programsMD5Map;
        }

        public static void Save(string fileName, Dictionary<string, string> programMD5Map)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }

            using (XmlTextWriter xmlWriter = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Programs");
                foreach (string programId in programMD5Map.Keys)
                {
                    xmlWriter.WriteStartElement("Program");
                    xmlWriter.WriteAttributeString("ProgramId", programId);
                    xmlWriter.WriteAttributeString("MD5", programMD5Map[programId]);
                    xmlWriter.WriteEndElement(); 
                }
                xmlWriter.WriteEndElement();
            }
        }
        
        public static void Save(string fileName, List<SchedulesResponse> lsr) {
        	Dictionary<string, string> programMD5Map = new Dictionary<string, string>();
        	
        	foreach (SchedulesResponse sr in lsr) {
        		foreach (SchedulesResponse.Program program in sr.programs) {
        			if (!programMD5Map.ContainsKey(program.programID)) {
        				programMD5Map.Add(program.programID, program.md5);
        			}
        		}
        	}
        	
        	Save(fileName, programMD5Map);
        }
        #endregion
    }
}

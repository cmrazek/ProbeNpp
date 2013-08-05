using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProbeNpp
{
	public static class XmlUtil
	{
		public static string Serialize(object obj)
		{
			try
			{
				var sb = new StringBuilder();
				var xmlSettings = new XmlWriterSettings { OmitXmlDeclaration = true };
				var xmlWriter = XmlWriter.Create(sb, xmlSettings);

				var serializer = new XmlSerializer(obj.GetType());
				serializer.Serialize(xmlWriter, obj);

				return sb.ToString();
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(NppSharp.OutputStyle.Error, ex.ToString());
				return string.Empty;
			}
		}

		public static T Deserialize<T>(string xml)
		{
			try
			{
				var stringReader = new StringReader(xml);
				var xmlReader = XmlReader.Create(stringReader);
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(xmlReader);
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(NppSharp.OutputStyle.Error, ex.ToString());
				return default(T);
			}
			
		}
	}
}

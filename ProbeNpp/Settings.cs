using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Xml;
using System.IO;
using System.Reflection;
using NppSharp;

namespace ProbeNpp
{
	public partial class Settings
	{
		private ProbeNppPlugin _plugin = null;
		private SettingsGroup _root = new SettingsGroup();

		public class SettingsGroup
		{
		}

		public Settings(ProbeNppPlugin plugin)
		{
			try
			{
				if (plugin == null) throw new ArgumentNullException("plugin is null");
				_plugin = plugin;
			}
			catch (Exception ex)
			{
				if (_plugin != null) Plugin.Output.WriteLine(OutputStyle.Error, "Exception when initializing settings object: {0}", ex);
			}
		}

		public SettingsGroup Root
		{
			get { return _root; }
		}

		public void Save()
		{
			StringBuilder xmlSb = new StringBuilder();
			using (TextWriter xmlTW = new StringWriter(xmlSb))
			{
				XmlWriterSettings xmlSettings = new XmlWriterSettings();
				xmlSettings.Indent = true;
				xmlSettings.CloseOutput = false;
				using (XmlWriter xml = XmlTextWriter.Create(xmlTW, xmlSettings))
				{
					xml.WriteStartDocument();
					xml.WriteStartElement("ProbeNppConfig");

					foreach (FieldInfo fi in this.GetType().GetFields())
					{
						if (fi.FieldType.IsSubclassOf(typeof(SettingsGroup)))
						{
							SettingsGroup groupObj = (SettingsGroup)fi.GetValue(this);
							SaveGroup(xml, fi, groupObj);
						}
					}

					xml.WriteEndElement();	// ProbeNppConfig
					xml.WriteEndDocument();
				}
			}

			string configFileName = Path.Combine(_plugin.ConfigDir, "ProbeNppConfig.xml");
			File.WriteAllText(configFileName, xmlSb.ToString(), Encoding.Unicode);
		}

		private void SaveGroup(XmlWriter xml, FieldInfo groupFi, SettingsGroup groupObj)
		{
			xml.WriteStartElement(groupFi.Name);

			foreach (FieldInfo fi in groupFi.FieldType.GetFields())
			{
				if (!fi.IsPublic) continue;

				xml.WriteStartElement(fi.Name);

				try
				{
					object value = fi.GetValue(groupObj);
					if (value != null) SaveItem(xml, fi, value);
				}
				catch (Exception ex)
				{
					_plugin.Output.WriteLine(OutputStyle.Error, "Exception when getting value for {0}.{1}: {2}", groupFi.Name, fi.Name, ex);
				}

				xml.WriteEndElement();	// fi.Name
			}

			xml.WriteEndElement();	// groupFi.Name
		}

		public void Load()
		{
			string configFileName = Path.Combine(_plugin.ConfigDir, "ProbeNppConfig.xml");
			if (File.Exists(configFileName))
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(configFileName);

				XmlNode rootNode = xmlDoc.SelectSingleNode("ProbeNppConfig");
				if (rootNode == null) return;

				foreach (XmlNode node in rootNode.ChildNodes)
				{
					if (node.NodeType != XmlNodeType.Element) continue;
					XmlElement groupElement = (XmlElement)node;

					FieldInfo fi = this.GetType().GetField(groupElement.Name);
					if (fi == null)
					{
						_plugin.Output.WriteLine(OutputStyle.Warning, "Couldn't find a field to match the element '{0}'.", groupElement.Name);
					}
					else
					{
						LoadGroup(groupElement, fi, (SettingsGroup)fi.GetValue(this));
					}
				}
			}
		}

		private void LoadGroup(XmlElement groupElement, FieldInfo groupFi, SettingsGroup groupObj)
		{
			foreach (XmlNode node in groupElement.ChildNodes)
			{
				if (node.NodeType != XmlNodeType.Element) continue;
				XmlElement fieldElement = (XmlElement)node;

				FieldInfo fi = groupFi.FieldType.GetField(fieldElement.Name);
				if (fi == null)
				{
					_plugin.Output.WriteLine(OutputStyle.Warning, "Couldn't find a field to match element '{0}' in group '{1}'.", fieldElement.Name, groupFi.Name);
				}
				else
				{
					LoadItem(fieldElement, fi, groupObj);
				}
			}
		}

		private void SaveItem(XmlWriter xml, FieldInfo fi, object value)
		{
			Type type = fi.FieldType;

			if (type.IsValueType)
			{
				xml.WriteString(value.ToString());
			}
			else if (type == typeof(string))
			{
				xml.WriteString((string)value);
			}
			else
			{
				_plugin.Output.WriteLine(OutputStyle.Warning, "Unable to determine how to write settings field {0} (type {1})", fi.Name, type.ToString());
			}
		}

		private void LoadItem(XmlElement fieldElement, FieldInfo fi, SettingsGroup groupObj)
		{
			Type type = fi.FieldType;

			if (type.IsValueType)
			{
				string str = fieldElement.InnerText;
				if (str == null) str = "";

				object val = ConvertStringToType(str, fi.FieldType);
				if (val != null) fi.SetValue(groupObj, val);
			}
			else if (type == typeof(string))
			{
				fi.SetValue(groupObj, fieldElement.InnerText);
			}
			else
			{
				_plugin.Output.WriteLine(OutputStyle.Warning, "Unable to determine how to load settings field {0} (type {1})", fi.Name, type.ToString());
			}
		}

		private object ConvertStringToType(string str, Type type)
		{
			object val = null;

			if (type.IsEnum)
			{
				try 
				{
					return Enum.Parse(type, str);
				}
				catch (Exception)
				{
					return 0;
				}
			}
			else
			{
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
						{
							Boolean v;
							if (Boolean.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.Char:
						{
							Char v;
							if (Char.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.SByte:
						{
							SByte v;
							if (SByte.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.Byte:
						{
							Byte v;
							if (Byte.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.Int16:
						{
							Int16 v;
							if (Int16.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.UInt16:
						{
							UInt16 v;
							if (UInt16.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.Int32:
						{
							Int32 v;
							if (Int32.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.UInt32:
						{
							UInt32 v;
							if (UInt32.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.Int64:
						{
							Int64 v;
							if (Int64.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.UInt64:
						{
							UInt64 v;
							if (UInt64.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.Single:
						{
							Single v;
							if (Single.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.Double:
						{
							Double v;
							if (Double.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.Decimal:
						{
							Decimal v;
							if (Decimal.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.DateTime:
						{
							DateTime v;
							if (DateTime.TryParse(str, out v)) val = v;
						}
						break;
					case TypeCode.String:
						val = str;
						break;
				}
			}

			return val;
		}


		
	}
}

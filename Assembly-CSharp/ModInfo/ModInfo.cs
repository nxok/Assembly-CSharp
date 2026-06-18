using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.WpfDesign.XamlDom;
using XMLData;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace ModInfo
{
	public class ModInfo : IXMLData
	{
		public DataItem<string> Name
		{
			get
			{
				return this.pName;
			}
			set
			{
				this.pName = value;
			}
		}

		public DataItem<string> Description
		{
			get
			{
				return this.pDescription;
			}
			set
			{
				this.pDescription = value;
			}
		}

		public DataItem<string> Author
		{
			get
			{
				return this.pAuthor;
			}
			set
			{
				this.pAuthor = value;
			}
		}

		public DataItem<string> Version
		{
			get
			{
				return this.pVersion;
			}
			set
			{
				this.pVersion = value;
			}
		}

		public DataItem<string> Website
		{
			get
			{
				return this.pWebsite;
			}
			set
			{
				this.pWebsite = value;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pName;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pDescription;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pAuthor;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pVersion;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pWebsite;

		public static class Parser
		{
			public static ModInfo Parse(XElement _elem, Dictionary<PositionXmlElement, DataItem<ModInfo>> _updateLater)
			{
				ModInfo modInfo = new ModInfo();
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (XElement xelement in _elem.Elements())
				{
					if (!ModInfo.Parser.knownAttributesMultiplicity.ContainsKey(xelement.Name.LocalName))
					{
						string text = "Unknown element \"";
						XName name = xelement.Name;
						throw new UnexpectedElementException(text + ((name != null) ? name.ToString() : null) + "\" found while parsing ModInfo", ((IXmlLineInfo)xelement).LineNumber);
					}
					string localName = xelement.Name.LocalName;
					if (!(localName == "Name"))
					{
						if (!(localName == "Description"))
						{
							if (!(localName == "Author"))
							{
								if (!(localName == "Version"))
								{
									if (localName == "Website")
									{
										ModInfo.Parser.ParseFieldAttributeWebsite(modInfo, dictionary, xelement);
									}
								}
								else
								{
									ModInfo.Parser.ParseFieldAttributeVersion(modInfo, dictionary, xelement);
								}
							}
							else
							{
								ModInfo.Parser.ParseFieldAttributeAuthor(modInfo, dictionary, xelement);
							}
						}
						else
						{
							ModInfo.Parser.ParseFieldAttributeDescription(modInfo, dictionary, xelement);
						}
					}
					else
					{
						ModInfo.Parser.ParseFieldAttributeName(modInfo, dictionary, xelement);
					}
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in ModInfo.Parser.knownAttributesMultiplicity)
				{
					int num = (dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : 0);
					if ((keyValuePair.Value.hasMin && num < keyValuePair.Value.min) || (keyValuePair.Value.hasMax && num > keyValuePair.Value.max))
					{
						string[] array = new string[6];
						array[0] = "Element has incorrect number of \"";
						array[1] = keyValuePair.Key;
						array[2] = "\" attribute instances, found ";
						array[3] = num.ToString();
						array[4] = ", expected ";
						int num2 = 5;
						Range<int> value = keyValuePair.Value;
						array[num2] = ((value != null) ? value.ToString() : null);
						throw new IncorrectAttributeOccurrenceException(string.Concat(array), ((IXmlLineInfo)_elem).LineNumber);
					}
				}
				return modInfo;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void ParseFieldAttributeName(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
			{
				string text = null;
				if (_elem != null)
				{
					text = ParserUtils.ParseStringAttribute(_elem, "value", true, null);
				}
				string text2;
				try
				{
					text2 = stringParser.Parse(text);
				}
				catch (Exception ex)
				{
					throw new InvalidValueException("Could not parse attribute \"Name\" value \"" + text + "\"", (_elem != null) ? ((IXmlLineInfo)_elem).LineNumber : (-1), ex);
				}
				DataItem<string> dataItem = new DataItem<string>("Name", text2);
				_entry.pName = dataItem;
				if (_elem != null)
				{
					if (!_foundAttributes.ContainsKey("Name"))
					{
						_foundAttributes["Name"] = 0;
					}
					int num = _foundAttributes["Name"];
					_foundAttributes["Name"] = num + 1;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void ParseFieldAttributeDescription(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
			{
				string text = null;
				if (_elem != null)
				{
					text = ParserUtils.ParseStringAttribute(_elem, "value", true, null);
				}
				string text2;
				try
				{
					text2 = stringParser.Parse(text);
				}
				catch (Exception ex)
				{
					throw new InvalidValueException("Could not parse attribute \"Description\" value \"" + text + "\"", (_elem != null) ? ((IXmlLineInfo)_elem).LineNumber : (-1), ex);
				}
				DataItem<string> dataItem = new DataItem<string>("Description", text2);
				_entry.pDescription = dataItem;
				if (_elem != null)
				{
					if (!_foundAttributes.ContainsKey("Description"))
					{
						_foundAttributes["Description"] = 0;
					}
					int num = _foundAttributes["Description"];
					_foundAttributes["Description"] = num + 1;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void ParseFieldAttributeAuthor(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
			{
				string text = null;
				if (_elem != null)
				{
					text = ParserUtils.ParseStringAttribute(_elem, "value", true, null);
				}
				string text2;
				try
				{
					text2 = stringParser.Parse(text);
				}
				catch (Exception ex)
				{
					throw new InvalidValueException("Could not parse attribute \"Author\" value \"" + text + "\"", (_elem != null) ? ((IXmlLineInfo)_elem).LineNumber : (-1), ex);
				}
				DataItem<string> dataItem = new DataItem<string>("Author", text2);
				_entry.pAuthor = dataItem;
				if (_elem != null)
				{
					if (!_foundAttributes.ContainsKey("Author"))
					{
						_foundAttributes["Author"] = 0;
					}
					int num = _foundAttributes["Author"];
					_foundAttributes["Author"] = num + 1;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void ParseFieldAttributeVersion(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
			{
				string text = null;
				if (_elem != null)
				{
					text = ParserUtils.ParseStringAttribute(_elem, "value", true, null);
				}
				string text2;
				try
				{
					text2 = stringParser.Parse(text);
				}
				catch (Exception ex)
				{
					throw new InvalidValueException("Could not parse attribute \"Version\" value \"" + text + "\"", (_elem != null) ? ((IXmlLineInfo)_elem).LineNumber : (-1), ex);
				}
				DataItem<string> dataItem = new DataItem<string>("Version", text2);
				_entry.pVersion = dataItem;
				if (_elem != null)
				{
					if (!_foundAttributes.ContainsKey("Version"))
					{
						_foundAttributes["Version"] = 0;
					}
					int num = _foundAttributes["Version"];
					_foundAttributes["Version"] = num + 1;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void ParseFieldAttributeWebsite(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
			{
				string text = null;
				if (_elem != null)
				{
					text = ParserUtils.ParseStringAttribute(_elem, "value", true, null);
				}
				string text2;
				try
				{
					text2 = stringParser.Parse(text);
				}
				catch (Exception ex)
				{
					throw new InvalidValueException("Could not parse attribute \"Website\" value \"" + text + "\"", (_elem != null) ? ((IXmlLineInfo)_elem).LineNumber : (-1), ex);
				}
				DataItem<string> dataItem = new DataItem<string>("Website", text2);
				_entry.pWebsite = dataItem;
				if (_elem != null)
				{
					if (!_foundAttributes.ContainsKey("Website"))
					{
						_foundAttributes["Website"] = 0;
					}
					int num = _foundAttributes["Website"];
					_foundAttributes["Website"] = num + 1;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static readonly Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Name",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Description",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Author",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Version",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Website",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}

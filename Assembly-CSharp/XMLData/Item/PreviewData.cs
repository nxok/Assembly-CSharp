using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item
{
	public class PreviewData : IXMLData
	{
		public DataItem<int> Zoom
		{
			get
			{
				return this.pZoom;
			}
			set
			{
				this.pZoom = value;
			}
		}

		public DataItem<Vector2> Pos
		{
			get
			{
				return this.pPos;
			}
			set
			{
				this.pPos = value;
			}
		}

		public DataItem<Vector3> Rot
		{
			get
			{
				return this.pRot;
			}
			set
			{
				this.pRot = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			return new List<IDataItem>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pZoom;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<Vector2> pPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<Vector3> pRot;

		public static class Parser
		{
			public static PreviewData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "PreviewData");
				Type type = Type.GetType(typeof(PreviewData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				PreviewData previewData = (PreviewData)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing Preview", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!PreviewData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing Preview", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string text2 = positionXmlElement.Name;
						if (!(text2 == "Zoom"))
						{
							if (!(text2 == "Pos"))
							{
								if (text2 == "Rot")
								{
									Vector3 vector;
									try
									{
										vector = Vector3Parser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex);
									}
									DataItem<Vector3> dataItem = new DataItem<Vector3>("Rot", vector);
									previewData.pRot = dataItem;
								}
							}
							else
							{
								Vector2 vector2;
								try
								{
									vector2 = Vector2Parser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex2)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex2);
								}
								DataItem<Vector2> dataItem2 = new DataItem<Vector2>("Pos", vector2);
								previewData.pPos = dataItem2;
							}
						}
						else
						{
							int num;
							try
							{
								num = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception ex3)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, ex3);
							}
							DataItem<int> dataItem3 = new DataItem<int>("Zoom", num);
							previewData.pZoom = dataItem3;
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						Dictionary<string, int> dictionary2 = dictionary;
						text2 = positionXmlElement.Name;
						int num2 = dictionary2[text2];
						dictionary2[text2] = num2 + 1;
					}
				}
				if (!dictionary.ContainsKey("Zoom"))
				{
					int num3;
					try
					{
						num3 = intParser.Parse("0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0\" for attribute \"Zoom\" could not be parsed", -1);
					}
					DataItem<int> dataItem4 = new DataItem<int>("Zoom", num3);
					previewData.pZoom = dataItem4;
					dictionary["Zoom"] = 1;
				}
				if (!dictionary.ContainsKey("Pos"))
				{
					Vector2 vector3;
					try
					{
						vector3 = Vector2Parser.Parse("0,0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0,0\" for attribute \"Pos\" could not be parsed", -1);
					}
					DataItem<Vector2> dataItem5 = new DataItem<Vector2>("Pos", vector3);
					previewData.pPos = dataItem5;
					dictionary["Pos"] = 1;
				}
				if (!dictionary.ContainsKey("Rot"))
				{
					Vector3 vector4;
					try
					{
						vector4 = Vector3Parser.Parse("0,0,0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0,0,0\" for attribute \"Rot\" could not be parsed", -1);
					}
					DataItem<Vector3> dataItem6 = new DataItem<Vector3>("Rot", vector4);
					previewData.pRot = dataItem6;
					dictionary["Rot"] = 1;
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in PreviewData.Parser.knownAttributesMultiplicity)
				{
					int num4 = (dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : 0);
					if ((keyValuePair.Value.hasMin && num4 < keyValuePair.Value.min) || (keyValuePair.Value.hasMax && num4 > keyValuePair.Value.max))
					{
						throw new IncorrectAttributeOccurrenceException(string.Concat(new string[]
						{
							"Element has incorrect number of \"",
							keyValuePair.Key,
							"\" attribute instances, found ",
							num4.ToString(),
							", expected ",
							keyValuePair.Value.ToString()
						}), _elem.LineNumber);
					}
				}
				return previewData;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Zoom",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Pos",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Rot",
					new Range<int>(true, 1, true, 1)
				}
			};
		}
	}
}

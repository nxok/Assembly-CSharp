using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item
{
	[Preserve]
	public class ArmorData : IXMLData
	{
		public DataItem<float> Melee
		{
			get
			{
				return this.pMelee;
			}
			set
			{
				this.pMelee = value;
			}
		}

		public DataItem<float> Bullet
		{
			get
			{
				return this.pBullet;
			}
			set
			{
				this.pBullet = value;
			}
		}

		public DataItem<float> Puncture
		{
			get
			{
				return this.pPuncture;
			}
			set
			{
				this.pPuncture = value;
			}
		}

		public DataItem<float> Blunt
		{
			get
			{
				return this.pBlunt;
			}
			set
			{
				this.pBlunt = value;
			}
		}

		public DataItem<float> Explosive
		{
			get
			{
				return this.pExplosive;
			}
			set
			{
				this.pExplosive = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			return new List<IDataItem>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pMelee;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pBullet;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pPuncture;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pBlunt;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pExplosive;

		public static class Parser
		{
			public static ArmorData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ArmorData");
				Type type = Type.GetType(typeof(ArmorData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				ArmorData armorData = (ArmorData)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing Armor", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!ArmorData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing Armor", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string text2 = positionXmlElement.Name;
						if (!(text2 == "Melee"))
						{
							if (!(text2 == "Bullet"))
							{
								if (!(text2 == "Puncture"))
								{
									if (!(text2 == "Blunt"))
									{
										if (text2 == "Explosive")
										{
											float num;
											try
											{
												num = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<float> dataItem = new DataItem<float>("Explosive", num);
											armorData.pExplosive = dataItem;
										}
									}
									else
									{
										float num2;
										try
										{
											num2 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<float> dataItem2 = new DataItem<float>("Blunt", num2);
										armorData.pBlunt = dataItem2;
									}
								}
								else
								{
									float num3;
									try
									{
										num3 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
									DataItem<float> dataItem3 = new DataItem<float>("Puncture", num3);
									armorData.pPuncture = dataItem3;
								}
							}
							else
							{
								float num4;
								try
								{
									num4 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex4)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex4);
								}
								DataItem<float> dataItem4 = new DataItem<float>("Bullet", num4);
								armorData.pBullet = dataItem4;
							}
						}
						else
						{
							float num5;
							try
							{
								num5 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception ex5)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, ex5);
							}
							DataItem<float> dataItem5 = new DataItem<float>("Melee", num5);
							armorData.pMelee = dataItem5;
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						Dictionary<string, int> dictionary2 = dictionary;
						text2 = positionXmlElement.Name;
						int num6 = dictionary2[text2];
						dictionary2[text2] = num6 + 1;
					}
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in ArmorData.Parser.knownAttributesMultiplicity)
				{
					int num7 = (dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : 0);
					if ((keyValuePair.Value.hasMin && num7 < keyValuePair.Value.min) || (keyValuePair.Value.hasMax && num7 > keyValuePair.Value.max))
					{
						throw new IncorrectAttributeOccurrenceException(string.Concat(new string[]
						{
							"Element has incorrect number of \"",
							keyValuePair.Key,
							"\" attribute instances, found ",
							num7.ToString(),
							", expected ",
							keyValuePair.Value.ToString()
						}), _elem.LineNumber);
					}
				}
				return armorData;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Melee",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Bullet",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Puncture",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Blunt",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Explosive",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}

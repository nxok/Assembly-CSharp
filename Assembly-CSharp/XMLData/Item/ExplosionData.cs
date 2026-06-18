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
	public class ExplosionData : IXMLData
	{
		public DataItem<int> BlockDamage
		{
			get
			{
				return this.pBlockDamage;
			}
			set
			{
				this.pBlockDamage = value;
			}
		}

		public DataItem<int> EntityDamage
		{
			get
			{
				return this.pEntityDamage;
			}
			set
			{
				this.pEntityDamage = value;
			}
		}

		public DataItem<int> ParticleIndex
		{
			get
			{
				return this.pParticleIndex;
			}
			set
			{
				this.pParticleIndex = value;
			}
		}

		public DataItem<int> RadiusBlocks
		{
			get
			{
				return this.pRadiusBlocks;
			}
			set
			{
				this.pRadiusBlocks = value;
			}
		}

		public DataItem<int> RadiusEntities
		{
			get
			{
				return this.pRadiusEntities;
			}
			set
			{
				this.pRadiusEntities = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			return new List<IDataItem>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pBlockDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pEntityDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pParticleIndex;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRadiusBlocks;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRadiusEntities;

		public static class Parser
		{
			public static ExplosionData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ExplosionData");
				Type type = Type.GetType(typeof(ExplosionData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				ExplosionData explosionData = (ExplosionData)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing Explosion", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!ExplosionData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing Explosion", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string text2 = positionXmlElement.Name;
						if (!(text2 == "BlockDamage"))
						{
							if (!(text2 == "EntityDamage"))
							{
								if (!(text2 == "ParticleIndex"))
								{
									if (!(text2 == "RadiusBlocks"))
									{
										if (text2 == "RadiusEntities")
										{
											int num;
											try
											{
												num = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<int> dataItem = new DataItem<int>("RadiusEntities", num);
											explosionData.pRadiusEntities = dataItem;
										}
									}
									else
									{
										int num2;
										try
										{
											num2 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<int> dataItem2 = new DataItem<int>("RadiusBlocks", num2);
										explosionData.pRadiusBlocks = dataItem2;
									}
								}
								else
								{
									int num3;
									try
									{
										num3 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
									DataItem<int> dataItem3 = new DataItem<int>("ParticleIndex", num3);
									explosionData.pParticleIndex = dataItem3;
								}
							}
							else
							{
								int num4;
								try
								{
									num4 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
								DataItem<int> dataItem4 = new DataItem<int>("EntityDamage", num4);
								explosionData.pEntityDamage = dataItem4;
							}
						}
						else
						{
							int num5;
							try
							{
								num5 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
							DataItem<int> dataItem5 = new DataItem<int>("BlockDamage", num5);
							explosionData.pBlockDamage = dataItem5;
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
				foreach (KeyValuePair<string, Range<int>> keyValuePair in ExplosionData.Parser.knownAttributesMultiplicity)
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
				return explosionData;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"BlockDamage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"EntityDamage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ParticleIndex",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RadiusBlocks",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RadiusEntities",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}

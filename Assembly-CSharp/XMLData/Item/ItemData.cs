using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item
{
	[Preserve]
	public class ItemData : IXMLParserBase, IXMLData
	{
		public string Name
		{
			get
			{
				return this.pName;
			}
		}

		public int Id
		{
			get
			{
				return this.pId;
			}
		}

		public DataItem<bool> Active
		{
			get
			{
				return this.pActive;
			}
			set
			{
				this.pActive = value;
			}
		}

		public DataItem<bool> AlwaysActive
		{
			get
			{
				return this.pAlwaysActive;
			}
			set
			{
				this.pAlwaysActive = value;
			}
		}

		public DataItem<int> FuelValue
		{
			get
			{
				return this.pFuelValue;
			}
			set
			{
				this.pFuelValue = value;
			}
		}

		public DataItem<string> ImageEffectOnActive
		{
			get
			{
				return this.pImageEffectOnActive;
			}
			set
			{
				this.pImageEffectOnActive = value;
			}
		}

		public DataItem<PartsData> PartTypes
		{
			get
			{
				return this.pPartTypes;
			}
			set
			{
				this.pPartTypes = value;
			}
		}

		public DataItem<string> PlaySoundOnActive
		{
			get
			{
				return this.pPlaySoundOnActive;
			}
			set
			{
				this.pPlaySoundOnActive = value;
			}
		}

		public DataItem<int> Weight
		{
			get
			{
				return this.pWeight;
			}
			set
			{
				this.pWeight = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			List<IDataItem> list = new List<IDataItem>();
			if (_recursive && this.pPartTypes != null)
			{
				list.AddRange(this.pPartTypes.Value.GetDisplayValues(true));
			}
			for (int i = 0; i < this.pAction.Length; i++)
			{
				if (_recursive && this.pAction[i] != null)
				{
					list.AddRange(this.pAction[i].Value.GetDisplayValues(true));
				}
			}
			if (_recursive && this.pArmor != null)
			{
				list.AddRange(this.pArmor.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pPreview != null)
			{
				list.AddRange(this.pPreview.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pAttributes != null)
			{
				list.AddRange(this.pAttributes.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pExplosion != null)
			{
				list.AddRange(this.pExplosion.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pUMA != null)
			{
				list.AddRange(this.pUMA.Value.GetDisplayValues(true));
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, ItemClass> pElementMap = new Dictionary<string, ItemClass>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static ItemClass[] pElementIndexed;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string pName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int pId;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pAlwaysActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pPlaySoundOnActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pImageEffectOnActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pMeshfile;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pMaterial;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pHoldType;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pCanhold;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pStacknumber;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDegradation;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pDegradationBreaksAfter;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pFuelValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pCritChance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pGroup;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDamageEntityMin;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDamageEntityMax;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSmell;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDropScale;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pCustomIcon;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pCustomIconTint;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<EPartType> pPartType;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pWeight;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pCandrop;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pUserHidden;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pLightSource;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pThrowableDecoy;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pFuseTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ItemClass> pMoldTarget;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<EquipmentSlots> pEquipSlot;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pActivateObject;

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemData.DataItemArrayRepairTools pRepairTools;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pRepairTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRepairAmount;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundDestroy;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundIdle;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundJammed;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<PartsData> pPartTypes;

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemData.DataItemArrayAction pAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ArmorData> pArmor;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<PreviewData> pPreview;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<AttributesData> pAttributes;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ExplosionData> pExplosion;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<UMAData> pUMA;

		public class DataItemArrayRepairTools
		{
			public DataItem<string> this[int index]
			{
				get
				{
					if (index >= this.pRepairTools.Length)
					{
						throw new ArgumentOutOfRangeException("index", "index " + index.ToString() + " greater/equal than array length " + this.pRepairTools.Length.ToString());
					}
					return this.pRepairTools[index];
				}
				set
				{
					if (index >= this.pRepairTools.Length)
					{
						throw new ArgumentOutOfRangeException("index", "index " + index.ToString() + " greater/equal than array length " + this.pRepairTools.Length.ToString());
					}
					this.pRepairTools[index] = value;
				}
			}

			public int Length
			{
				get
				{
					return this.pRepairTools.Length;
				}
			}

			public DataItemArrayRepairTools(int _size)
			{
				this.pRepairTools = new DataItem<string>[_size];
			}

			public DataItemArrayRepairTools(DataItem<string>[] _init)
			{
				this.pRepairTools = _init;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public DataItem<string>[] pRepairTools;
		}

		public class DataItemArrayAction
		{
			public DataItem<ItemAction> this[int index]
			{
				get
				{
					if (index >= this.pAction.Length)
					{
						throw new ArgumentOutOfRangeException("index", "index " + index.ToString() + " greater/equal than array length " + this.pAction.Length.ToString());
					}
					return this.pAction[index];
				}
				set
				{
					if (index >= this.pAction.Length)
					{
						throw new ArgumentOutOfRangeException("index", "index " + index.ToString() + " greater/equal than array length " + this.pAction.Length.ToString());
					}
					this.pAction[index] = value;
				}
			}

			public int Length
			{
				get
				{
					return this.pAction.Length;
				}
			}

			public DataItemArrayAction(int _size)
			{
				this.pAction = new DataItem<ItemAction>[_size];
			}

			public DataItemArrayAction(DataItem<ItemAction>[] _init)
			{
				this.pAction = _init;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public DataItem<ItemAction>[] pAction;
		}

		public static class Parser
		{
			public static ItemClass Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = ParserUtils.ParseStringAttribute(_elem, "name", true, null);
				int num = ParserUtils.ParseIntAttribute(_elem, "id", true, 0) + ItemData.Parser.idOffset;
				string text2 = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ItemClass");
				Type type = Type.GetType(typeof(ItemData.Parser).Namespace + "." + text2);
				if (type == null)
				{
					type = Type.GetType(text2);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text2 + "\" not found", _elem.LineNumber);
					}
				}
				ItemClass itemClass = (ItemClass)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				itemClass.pName = text;
				itemClass.pId = num;
				List<DataItem<string>> list = new List<DataItem<string>>();
				List<DataItem<ItemAction>> list2 = new List<DataItem<ItemAction>>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing Item", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!ItemData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing Item", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string text3 = positionXmlElement.Name;
						uint num2 = <PrivateImplementationDetails>.ComputeStringHash(text3);
						if (num2 <= 2292684416U)
						{
							if (num2 <= 955858345U)
							{
								if (num2 <= 529077071U)
								{
									if (num2 <= 175614239U)
									{
										if (num2 != 91525164U)
										{
											if (num2 == 175614239U)
											{
												if (text3 == "Action")
												{
													ItemAction itemAction = ItemActionData.Parser.Parse(positionXmlElement, _updateLater);
													DataItem<ItemAction> dataItem = new DataItem<ItemAction>("Action", itemAction);
													list2.Add(dataItem);
												}
											}
										}
										else if (text3 == "Group")
										{
											string text4;
											try
											{
												text4 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<string> dataItem2 = new DataItem<string>("Group", text4);
											itemClass.pGroup = dataItem2;
										}
									}
									else if (num2 != 372600678U)
									{
										if (num2 != 461563084U)
										{
											if (num2 == 529077071U)
											{
												if (text3 == "Active")
												{
													bool flag;
													try
													{
														flag = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
													DataItem<bool> dataItem3 = new DataItem<bool>("Active", flag);
													itemClass.Active = dataItem3;
												}
											}
										}
										else if (text3 == "Candrop")
										{
											bool flag2;
											try
											{
												flag2 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<bool> dataItem4 = new DataItem<bool>("Candrop", flag2);
											itemClass.pCandrop = dataItem4;
										}
									}
									else if (text3 == "DropScale")
									{
										int num3;
										try
										{
											num3 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<int> dataItem5 = new DataItem<int>("DropScale", num3);
										itemClass.pDropScale = dataItem5;
									}
								}
								else if (num2 <= 531232375U)
								{
									if (num2 != 530817272U)
									{
										if (num2 == 531232375U)
										{
											if (text3 == "DegradationBreaksAfter")
											{
												bool flag3;
												try
												{
													flag3 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
												DataItem<bool> dataItem6 = new DataItem<bool>("DegradationBreaksAfter", flag3);
												itemClass.pDegradationBreaksAfter = dataItem6;
											}
										}
									}
									else if (text3 == "Canhold")
									{
										bool flag4;
										try
										{
											flag4 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex6)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex6);
										}
										DataItem<bool> dataItem7 = new DataItem<bool>("Canhold", flag4);
										itemClass.pCanhold = dataItem7;
									}
								}
								else if (num2 != 759865468U)
								{
									if (num2 != 942707696U)
									{
										if (num2 == 955858345U)
										{
											if (text3 == "FuseTime")
											{
												float num4;
												try
												{
													num4 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex7)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex7);
												}
												DataItem<float> dataItem8 = new DataItem<float>("FuseTime", num4);
												itemClass.pFuseTime = dataItem8;
											}
										}
									}
									else if (text3 == "MoldTarget")
									{
										ItemClass itemClass2;
										try
										{
											itemClass2 = null;
										}
										catch (Exception ex8)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex8);
										}
										DataItem<ItemClass> dataItem9 = new DataItem<ItemClass>("MoldTarget", itemClass2);
										_updateLater.Add(positionXmlElement, dataItem9);
										itemClass.pMoldTarget = dataItem9;
									}
								}
								else if (text3 == "PartType")
								{
									EPartType epartType;
									try
									{
										epartType = EnumParser.Parse<EPartType>(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex9)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex9);
									}
									DataItem<EPartType> dataItem10 = new DataItem<EPartType>("PartType", epartType);
									itemClass.pPartType = dataItem10;
								}
							}
							else if (num2 <= 1432876798U)
							{
								if (num2 <= 1012402666U)
								{
									if (num2 != 960815908U)
									{
										if (num2 == 1012402666U)
										{
											if (text3 == "SoundJammed")
											{
												string text5;
												try
												{
													text5 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex10)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex10);
												}
												DataItem<string> dataItem11 = new DataItem<string>("SoundJammed", text5);
												itemClass.pSoundJammed = dataItem11;
											}
										}
									}
									else if (text3 == "ImageEffectOnActive")
									{
										string text6;
										try
										{
											text6 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex11)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex11);
										}
										DataItem<string> dataItem12 = new DataItem<string>("ImageEffectOnActive", text6);
										itemClass.pMeshfile = dataItem12;
									}
								}
								else if (num2 != 1053088598U)
								{
									if (num2 != 1082430025U)
									{
										if (num2 == 1432876798U)
										{
											if (text3 == "SoundIdle")
											{
												string text7;
												try
												{
													text7 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex12)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex12);
												}
												DataItem<string> dataItem13 = new DataItem<string>("SoundIdle", text7);
												itemClass.pSoundIdle = dataItem13;
											}
										}
									}
									else if (text3 == "CritChance")
									{
										float num5;
										try
										{
											num5 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex13)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex13);
										}
										DataItem<float> dataItem14 = new DataItem<float>("CritChance", num5);
										itemClass.pCritChance = dataItem14;
									}
								}
								else if (text3 == "CustomIconTint")
								{
									string text8;
									try
									{
										text8 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex14)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex14);
									}
									DataItem<string> dataItem15 = new DataItem<string>("CustomIconTint", text8);
									itemClass.pCustomIconTint = dataItem15;
								}
							}
							else if (num2 <= 2079154250U)
							{
								if (num2 != 1534719901U)
								{
									if (num2 == 2079154250U)
									{
										if (text3 == "Meshfile")
										{
											string text9;
											try
											{
												text9 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex15)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex15);
											}
											DataItem<string> dataItem16 = new DataItem<string>("Meshfile", text9);
											itemClass.pMeshfile = dataItem16;
										}
									}
								}
								else if (text3 == "PartTypes")
								{
									PartsData partsData = PartsData.Parser.Parse(positionXmlElement, _updateLater);
									DataItem<PartsData> dataItem17 = new DataItem<PartsData>("PartTypes", partsData);
									itemClass.pPartTypes = dataItem17;
								}
							}
							else if (num2 != 2104753456U)
							{
								if (num2 != 2226667892U)
								{
									if (num2 == 2292684416U)
									{
										if (text3 == "Explosion")
										{
											ExplosionData explosionData = ExplosionData.Parser.Parse(positionXmlElement, _updateLater);
											DataItem<ExplosionData> dataItem18 = new DataItem<ExplosionData>("Explosion", explosionData);
											itemClass.pExplosion = dataItem18;
										}
									}
								}
								else if (text3 == "Armor")
								{
									ArmorData armorData = ArmorData.Parser.Parse(positionXmlElement, _updateLater);
									DataItem<ArmorData> dataItem19 = new DataItem<ArmorData>("Armor", armorData);
									itemClass.pArmor = dataItem19;
								}
							}
							else if (text3 == "HoldType")
							{
								int num6;
								try
								{
									num6 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex16)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex16);
								}
								DataItem<int> dataItem20 = new DataItem<int>("HoldType", num6);
								itemClass.pHoldType = dataItem20;
							}
						}
						else if (num2 <= 3419754368U)
						{
							if (num2 <= 2924404423U)
							{
								if (num2 <= 2570065113U)
								{
									if (num2 != 2517612320U)
									{
										if (num2 == 2570065113U)
										{
											if (text3 == "Weight")
											{
												int num7;
												try
												{
													num7 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex17)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex17);
												}
												DataItem<int> dataItem21 = new DataItem<int>("Weight", num7);
												itemClass.pWeight = dataItem21;
											}
										}
									}
									else if (text3 == "Smell")
									{
										string text10;
										try
										{
											text10 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex18)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex18);
										}
										DataItem<string> dataItem22 = new DataItem<string>("Smell", text10);
										itemClass.pSmell = dataItem22;
									}
								}
								else if (num2 != 2613839563U)
								{
									if (num2 != 2887776777U)
									{
										if (num2 == 2924404423U)
										{
											if (text3 == "RepairTools")
											{
												string text11;
												try
												{
													text11 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception ex19)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, ex19);
												}
												DataItem<string> dataItem23 = new DataItem<string>("RepairTools", text11);
												list.Add(dataItem23);
											}
										}
									}
									else if (text3 == "ThrowableDecoy")
									{
										bool flag5;
										try
										{
											flag5 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex20)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex20);
										}
										DataItem<bool> dataItem24 = new DataItem<bool>("ThrowableDecoy", flag5);
										itemClass.pThrowableDecoy = dataItem24;
									}
								}
								else if (text3 == "CustomIcon")
								{
									string text12;
									try
									{
										text12 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex21)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex21);
									}
									DataItem<string> dataItem25 = new DataItem<string>("CustomIcon", text12);
									itemClass.pCustomIcon = dataItem25;
								}
							}
							else if (num2 <= 3131847555U)
							{
								if (num2 != 2948774643U)
								{
									if (num2 == 3131847555U)
									{
										if (text3 == "Degradation")
										{
											int num8;
											try
											{
												num8 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex22)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex22);
											}
											DataItem<int> dataItem26 = new DataItem<int>("Degradation", num8);
											itemClass.pDegradation = dataItem26;
										}
									}
								}
								else if (text3 == "DamageEntityMax")
								{
									int num9;
									try
									{
										num9 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex23)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex23);
									}
									DataItem<int> dataItem27 = new DataItem<int>("DamageEntityMax", num9);
									itemClass.pDamageEntityMax = dataItem27;
								}
							}
							else if (num2 != 3319162189U)
							{
								if (num2 != 3365715102U)
								{
									if (num2 == 3419754368U)
									{
										if (text3 == "Material")
										{
											string text13;
											try
											{
												text13 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex24)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex24);
											}
											DataItem<string> dataItem28 = new DataItem<string>("Material", text13);
											itemClass.pMaterial = dataItem28;
										}
									}
								}
								else if (text3 == "Stacknumber")
								{
									int num10;
									try
									{
										num10 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex25)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex25);
									}
									DataItem<int> dataItem29 = new DataItem<int>("Stacknumber", num10);
									itemClass.pStacknumber = dataItem29;
								}
							}
							else if (text3 == "DamageEntityMin")
							{
								int num11;
								try
								{
									num11 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex26)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex26);
								}
								DataItem<int> dataItem30 = new DataItem<int>("DamageEntityMin", num11);
								itemClass.pDamageEntityMin = dataItem30;
							}
						}
						else if (num2 <= 3694724757U)
						{
							if (num2 <= 3463560063U)
							{
								if (num2 != 3430864052U)
								{
									if (num2 == 3463560063U)
									{
										if (text3 == "EquipSlot")
										{
											EquipmentSlots equipmentSlots;
											try
											{
												equipmentSlots = EnumParser.Parse<EquipmentSlots>(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex27)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex27);
											}
											DataItem<EquipmentSlots> dataItem31 = new DataItem<EquipmentSlots>("EquipSlot", equipmentSlots);
											itemClass.pEquipSlot = dataItem31;
										}
									}
								}
								else if (text3 == "Attributes")
								{
									AttributesData attributesData = AttributesData.Parser.Parse(positionXmlElement, _updateLater);
									DataItem<AttributesData> dataItem32 = new DataItem<AttributesData>("Attributes", attributesData);
									itemClass.pAttributes = dataItem32;
								}
							}
							else if (num2 != 3519818983U)
							{
								if (num2 != 3646476408U)
								{
									if (num2 == 3694724757U)
									{
										if (text3 == "RepairTime")
										{
											float num12;
											try
											{
												num12 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception ex28)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, ex28);
											}
											DataItem<float> dataItem33 = new DataItem<float>("RepairTime", num12);
											itemClass.pRepairTime = dataItem33;
										}
									}
								}
								else if (text3 == "RepairAmount")
								{
									int num13;
									try
									{
										num13 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception ex29)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, ex29);
									}
									DataItem<int> dataItem34 = new DataItem<int>("RepairAmount", num13);
									itemClass.pRepairAmount = dataItem34;
								}
							}
							else if (text3 == "ActivateObject")
							{
								string text14;
								try
								{
									text14 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex30)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex30);
								}
								DataItem<string> dataItem35 = new DataItem<string>("ActivateObject", text14);
								itemClass.pActivateObject = dataItem35;
							}
						}
						else if (num2 <= 3912538170U)
						{
							if (num2 != 3912144422U)
							{
								if (num2 == 3912538170U)
								{
									if (text3 == "UserHidden")
									{
										bool flag6;
										try
										{
											flag6 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex31)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex31);
										}
										DataItem<bool> dataItem36 = new DataItem<bool>("UserHidden", flag6);
										itemClass.pUserHidden = dataItem36;
									}
								}
							}
							else if (text3 == "SoundDestroy")
							{
								string text15;
								try
								{
									text15 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception ex32)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, ex32);
								}
								DataItem<string> dataItem37 = new DataItem<string>("SoundDestroy", text15);
								itemClass.pSoundDestroy = dataItem37;
							}
						}
						else if (num2 != 4235238322U)
						{
							if (num2 != 4258942199U)
							{
								if (num2 == 4293145386U)
								{
									if (text3 == "LightSource")
									{
										string text16;
										try
										{
											text16 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception ex33)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, ex33);
										}
										DataItem<string> dataItem38 = new DataItem<string>("LightSource", text16);
										itemClass.pLightSource = dataItem38;
									}
								}
							}
							else if (text3 == "Preview")
							{
								PreviewData previewData = PreviewData.Parser.Parse(positionXmlElement, _updateLater);
								DataItem<PreviewData> dataItem39 = new DataItem<PreviewData>("Preview", previewData);
								itemClass.pPreview = dataItem39;
							}
						}
						else if (text3 == "FuelValue")
						{
							int num14;
							try
							{
								num14 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception ex34)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, ex34);
							}
							DataItem<int> dataItem40 = new DataItem<int>("FuelValue", num14);
							itemClass.pFuelValue = dataItem40;
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						Dictionary<string, int> dictionary2 = dictionary;
						text3 = positionXmlElement.Name;
						int num15 = dictionary2[text3];
						dictionary2[text3] = num15 + 1;
					}
				}
				if (!dictionary.ContainsKey("Stacknumber"))
				{
					int num16;
					try
					{
						num16 = intParser.Parse("64");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"64\" for attribute \"Stacknumber\" could not be parsed", -1);
					}
					DataItem<int> dataItem41 = new DataItem<int>("Stacknumber", num16);
					itemClass.pStacknumber = dataItem41;
					dictionary["Stacknumber"] = 1;
				}
				if (!dictionary.ContainsKey("Degradation"))
				{
					int num17;
					try
					{
						num17 = intParser.Parse("0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0\" for attribute \"Degradation\" could not be parsed", -1);
					}
					DataItem<int> dataItem42 = new DataItem<int>("Degradation", num17);
					itemClass.pDegradation = dataItem42;
					dictionary["Degradation"] = 1;
				}
				if (!dictionary.ContainsKey("DegradationBreaksAfter"))
				{
					bool flag7;
					try
					{
						flag7 = boolParser.Parse("true");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"true\" for attribute \"DegradationBreaksAfter\" could not be parsed", -1);
					}
					DataItem<bool> dataItem43 = new DataItem<bool>("DegradationBreaksAfter", flag7);
					itemClass.pDegradationBreaksAfter = dataItem43;
					dictionary["DegradationBreaksAfter"] = 1;
				}
				if (!dictionary.ContainsKey("CritChance"))
				{
					float num18;
					try
					{
						num18 = floatParser.Parse("0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0\" for attribute \"CritChance\" could not be parsed", -1);
					}
					DataItem<float> dataItem44 = new DataItem<float>("CritChance", num18);
					itemClass.pCritChance = dataItem44;
					dictionary["CritChance"] = 1;
				}
				if (!dictionary.ContainsKey("Group"))
				{
					string text17;
					try
					{
						text17 = stringParser.Parse("Miscellaneous");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"Miscellaneous\" for attribute \"Group\" could not be parsed", -1);
					}
					DataItem<string> dataItem45 = new DataItem<string>("Group", text17);
					itemClass.pGroup = dataItem45;
					dictionary["Group"] = 1;
				}
				if (!dictionary.ContainsKey("PartType"))
				{
					EPartType epartType2;
					try
					{
						epartType2 = EnumParser.Parse<EPartType>("None");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"None\" for attribute \"PartType\" could not be parsed", -1);
					}
					DataItem<EPartType> dataItem46 = new DataItem<EPartType>("PartType", epartType2);
					itemClass.pPartType = dataItem46;
					dictionary["PartType"] = 1;
				}
				if (!dictionary.ContainsKey("UserHidden"))
				{
					bool flag8;
					try
					{
						flag8 = boolParser.Parse("false");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"false\" for attribute \"UserHidden\" could not be parsed", -1);
					}
					DataItem<bool> dataItem47 = new DataItem<bool>("UserHidden", flag8);
					itemClass.pUserHidden = dataItem47;
					dictionary["UserHidden"] = 1;
				}
				if (!dictionary.ContainsKey("ThrowableDecoy"))
				{
					bool flag9;
					try
					{
						flag9 = boolParser.Parse("false");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"false\" for attribute \"ThrowableDecoy\" could not be parsed", -1);
					}
					DataItem<bool> dataItem48 = new DataItem<bool>("ThrowableDecoy", flag9);
					itemClass.pThrowableDecoy = dataItem48;
					dictionary["ThrowableDecoy"] = 1;
				}
				if (!dictionary.ContainsKey("EquipSlot"))
				{
					EquipmentSlots equipmentSlots2;
					try
					{
						equipmentSlots2 = EnumParser.Parse<EquipmentSlots>("None");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"None\" for attribute \"EquipSlot\" could not be parsed", -1);
					}
					DataItem<EquipmentSlots> dataItem49 = new DataItem<EquipmentSlots>("EquipSlot", equipmentSlots2);
					itemClass.pEquipSlot = dataItem49;
					dictionary["EquipSlot"] = 1;
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in ItemData.Parser.knownAttributesMultiplicity)
				{
					int num19 = (dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : 0);
					if ((keyValuePair.Value.hasMin && num19 < keyValuePair.Value.min) || (keyValuePair.Value.hasMax && num19 > keyValuePair.Value.max))
					{
						throw new IncorrectAttributeOccurrenceException(string.Concat(new string[]
						{
							"Element has incorrect number of \"",
							keyValuePair.Key,
							"\" attribute instances, found ",
							num19.ToString(),
							", expected ",
							keyValuePair.Value.ToString()
						}), _elem.LineNumber);
					}
				}
				itemClass.pRepairTools = new ItemData.DataItemArrayRepairTools(list.ToArray());
				itemClass.pAction = new ItemData.DataItemArrayAction(list2.ToArray());
				return itemClass;
			}

			public static List<ItemClass> ParseXml(string _filename, string _content, bool _clearFirst = true, bool _validateOnly = false)
			{
				PositionXmlDocument positionXmlDocument = new PositionXmlDocument();
				if (_clearFirst && !_validateOnly)
				{
					ItemData.Parser.Clear();
				}
				try
				{
					using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(_content ?? "")))
					{
						positionXmlDocument.Load(XmlReader.Create(stream));
					}
				}
				catch (XmlException ex)
				{
					Log.Error("Failed parsing " + _filename + ":");
					Log.Exception(ex);
					return null;
				}
				XmlNode documentElement = positionXmlDocument.DocumentElement;
				List<ItemClass> list = new List<ItemClass>();
				Dictionary<PositionXmlElement, DataItem<ItemClass>> dictionary = new Dictionary<PositionXmlElement, DataItem<ItemClass>>();
				foreach (object obj in documentElement.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							Log.Error("Unexpected XML node: " + xmlNode.NodeType.ToString() + " at line " + ((IXmlLineInfo)xmlNode).LineNumber.ToString());
						}
					}
					else if (xmlNode.Name == "Item")
					{
						ItemClass itemClass = ItemData.Parser.Parse((PositionXmlElement)xmlNode, dictionary);
						if (itemClass != null)
						{
							list.Add(itemClass);
						}
					}
					else
					{
						Log.Error(string.Format("Unknown element found: {0} (file {1}, line {2})", xmlNode.Name, _filename, ((IXmlLineInfo)xmlNode).LineNumber));
					}
				}
				if (!_validateOnly)
				{
					ItemData.Parser.FillLists(_filename, list, _clearFirst);
					ItemData.Parser.UpdateXmlRefs(_filename, dictionary);
				}
				return list;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void Clear()
			{
				ItemData.pElementMap.Clear();
				ItemData.pElementIndexed = null;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void FillLists(string _filename, List<ItemClass> _entries, bool _clearFirst)
			{
				int num = -1;
				HashSet<int> hashSet = new HashSet<int>();
				foreach (ItemClass itemClass in _entries)
				{
					if (itemClass.Id > num)
					{
						num = itemClass.Id;
					}
					if (hashSet.Contains(itemClass.Id))
					{
						Log.Error(string.Format("Duplicate index: {0} in {1}", itemClass.Id, _filename));
					}
					hashSet.Add(itemClass.Id);
				}
				if (!_clearFirst && num >= ItemData.pElementIndexed.Length)
				{
					ItemClass[] array = new ItemClass[num + 1];
					Array.Copy(ItemData.pElementIndexed, array, ItemData.pElementIndexed.Length);
					ItemData.pElementIndexed = array;
				}
				else if (_clearFirst)
				{
					ItemData.pElementIndexed = new ItemClass[num + 1];
				}
				foreach (ItemClass itemClass2 in _entries)
				{
					if (ItemData.pElementIndexed[itemClass2.Id] != null)
					{
						Log.Warning(string.Format("Overwriting existing element index: {0} in {1}", itemClass2.Id, _filename));
					}
					ItemData.pElementIndexed[itemClass2.Id] = itemClass2;
					if (ItemData.pElementMap.ContainsKey(itemClass2.Name))
					{
						Log.Warning(string.Format("Overwriting existing element name: {0} in {1}", itemClass2.Name, _filename));
					}
					ItemData.pElementMap[itemClass2.Name] = itemClass2;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void UpdateXmlRefs(string _filename, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				foreach (KeyValuePair<PositionXmlElement, DataItem<ItemClass>> keyValuePair in _updateLater)
				{
					string text = ParserUtils.ParseStringAttribute(keyValuePair.Key, "value", true, null);
					if (!ItemData.pElementMap.ContainsKey(text))
					{
						throw new InvalidValueException(string.Concat(new string[]
						{
							"Element with name \"",
							text,
							"\" for attribute \"",
							keyValuePair.Value.Name,
							"\" not found (referencing an XML entry by name which is not defined)"
						}), keyValuePair.Key.LineNumber);
					}
					keyValuePair.Value.Value = ItemData.pElementMap[text];
				}
			}

			public static int idOffset = 0;

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Active",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ImageEffectOnActive",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Meshfile",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Material",
					new Range<int>(true, 1, true, 1)
				},
				{
					"HoldType",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Canhold",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Stacknumber",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Degradation",
					new Range<int>(true, 1, true, 1)
				},
				{
					"DegradationBreaksAfter",
					new Range<int>(true, 1, true, 1)
				},
				{
					"FuelValue",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CritChance",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Group",
					new Range<int>(true, 1, true, 1)
				},
				{
					"DamageEntityMin",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DamageEntityMax",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Smell",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DropScale",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CustomIcon",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CustomIconTint",
					new Range<int>(true, 0, true, 1)
				},
				{
					"PartType",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Weight",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Candrop",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UserHidden",
					new Range<int>(true, 1, true, 1)
				},
				{
					"LightSource",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ThrowableDecoy",
					new Range<int>(true, 1, true, 1)
				},
				{
					"FuseTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"MoldTarget",
					new Range<int>(true, 0, true, 1)
				},
				{
					"EquipSlot",
					new Range<int>(true, 1, true, 1)
				},
				{
					"ActivateObject",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RepairTools",
					new Range<int>(true, 0, false, 0)
				},
				{
					"RepairTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RepairAmount",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundDestroy",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundIdle",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundJammed",
					new Range<int>(true, 0, true, 1)
				},
				{
					"PartTypes",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Action",
					new Range<int>(true, 0, true, 5)
				},
				{
					"Armor",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Preview",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Attributes",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Explosion",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UMA",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}

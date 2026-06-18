using System;
using System.Collections.Generic;
using SharpEXR.AttributeTypes;

namespace SharpEXR
{
	public class EXRAttribute
	{
		public string Name
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public string Type
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public int Size
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public object Value
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public static bool Read(EXRFile file, IEXRReader reader, out EXRAttribute attribute)
		{
			attribute = new EXRAttribute();
			return attribute.Read(file, reader);
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public bool Read(EXRFile file, IEXRReader reader)
		{
			int maxNameLength = file.Version.MaxNameLength;
			try
			{
				this.Name = reader.ReadNullTerminatedString(maxNameLength);
			}
			catch (Exception ex)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute name: " + ex.Message, ex);
			}
			if (this.Name == "")
			{
				return false;
			}
			try
			{
				this.Type = reader.ReadNullTerminatedString(maxNameLength);
			}
			catch (Exception ex2)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute type for '" + this.Name + "': " + ex2.Message, ex2);
			}
			if (this.Type == "")
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute type for '" + this.Name + "': Cannot be an empty string.");
			}
			this.Size = reader.ReadInt32();
			string type = this.Type;
			uint num = <PrivateImplementationDetails>.ComputeStringHash(type);
			if (num <= 1683634359U)
			{
				if (num <= 987494389U)
				{
					if (num <= 398550328U)
					{
						if (num != 137362080U)
						{
							if (num == 398550328U)
							{
								if (type == "string")
								{
									if (this.Size < 0)
									{
										throw new EXRFormatException(string.Concat(new string[]
										{
											"Invalid or corrupt EXR header attribute '",
											this.Name,
											"' of type string: Invalid Size, was ",
											this.Size.ToString(),
											"."
										}));
									}
									this.Value = reader.ReadString(this.Size);
									return true;
								}
							}
						}
						else if (type == "m33f")
						{
							if (this.Size != 36)
							{
								throw new EXRFormatException(string.Concat(new string[]
								{
									"Invalid or corrupt EXR header attribute '",
									this.Name,
									"' of type m33f: Size must be 36 bytes, was ",
									this.Size.ToString(),
									"."
								}));
							}
							this.Value = new M33F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
							return true;
						}
					}
					else if (num != 668727635U)
					{
						if (num != 754561049U)
						{
							if (num == 987494389U)
							{
								if (type == "compression")
								{
									if (this.Size != 1)
									{
										throw new EXRFormatException(string.Concat(new string[]
										{
											"Invalid or corrupt EXR header attribute '",
											this.Name,
											"' of type compression: Size must be 1 byte, was ",
											this.Size.ToString(),
											"."
										}));
									}
									this.Value = (EXRCompression)reader.ReadByte();
									return true;
								}
							}
						}
						else if (type == "lineOrder")
						{
							if (this.Size != 1)
							{
								throw new EXRFormatException(string.Concat(new string[]
								{
									"Invalid or corrupt EXR header attribute '",
									this.Name,
									"' of type lineOrder: Size must be 1 byte, was ",
									this.Size.ToString(),
									"."
								}));
							}
							this.Value = (LineOrder)reader.ReadByte();
							return true;
						}
					}
					else if (type == "rational")
					{
						if (this.Size != 8)
						{
							throw new EXRFormatException(string.Concat(new string[]
							{
								"Invalid or corrupt EXR header attribute '",
								this.Name,
								"' of type rational: Size must be 8 bytes, was ",
								this.Size.ToString(),
								"."
							}));
						}
						this.Value = new Rational(reader.ReadInt32(), reader.ReadUInt32());
						return true;
					}
				}
				else if (num <= 1055095241U)
				{
					if (num != 997609106U)
					{
						if (num != 1047941963U)
						{
							if (num == 1055095241U)
							{
								if (type == "keycode")
								{
									if (this.Size != 28)
									{
										throw new EXRFormatException(string.Concat(new string[]
										{
											"Invalid or corrupt EXR header attribute '",
											this.Name,
											"' of type keycode: Size must be 28 bytes, was ",
											this.Size.ToString(),
											"."
										}));
									}
									this.Value = new KeyCode(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
									return true;
								}
							}
						}
						else if (type == "box2i")
						{
							if (this.Size != 16)
							{
								throw new EXRFormatException(string.Concat(new string[]
								{
									"Invalid or corrupt EXR header attribute '",
									this.Name,
									"' of type box2i: Size must be 16 bytes, was ",
									this.Size.ToString(),
									"."
								}));
							}
							this.Value = new Box2I(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
							return true;
						}
					}
					else if (type == "box2f")
					{
						if (this.Size != 16)
						{
							throw new EXRFormatException(string.Concat(new string[]
							{
								"Invalid or corrupt EXR header attribute '",
								this.Name,
								"' of type box2f: Size must be 16 bytes, was ",
								this.Size.ToString(),
								"."
							}));
						}
						this.Value = new Box2F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
						return true;
					}
				}
				else if (num != 1257924439U)
				{
					if (num != 1546378161U)
					{
						if (num == 1683634359U)
						{
							if (type == "stringvector")
							{
								if (this.Size == 0)
								{
									this.Value = new List<string>();
									return true;
								}
								if (this.Size < 4)
								{
									throw new EXRFormatException(string.Concat(new string[]
									{
										"Invalid or corrupt EXR header attribute '",
										this.Name,
										"' of type stringvector: Size must be at least 4 bytes or 0 bytes, was ",
										this.Size.ToString(),
										"."
									}));
								}
								List<string> list = new List<string>();
								this.Value = list;
								int i;
								int position;
								for (i = 0; i < this.Size; i += reader.Position - position)
								{
									position = reader.Position;
									string text = reader.ReadString();
									list.Add(text);
								}
								if (i != this.Size)
								{
									throw new EXRFormatException(string.Concat(new string[]
									{
										"Invalid or corrupt EXR header attribute '",
										this.Name,
										"' of type stringvector: Read ",
										i.ToString(),
										" bytes but Size was ",
										this.Size.ToString(),
										"."
									}));
								}
								return true;
							}
						}
					}
					else if (type == "timecode")
					{
						if (this.Size != 8)
						{
							throw new EXRFormatException(string.Concat(new string[]
							{
								"Invalid or corrupt EXR header attribute '",
								this.Name,
								"' of type timecode: Size must be 8 bytes, was ",
								this.Size.ToString(),
								"."
							}));
						}
						this.Value = new TimeCode(reader.ReadUInt32(), reader.ReadUInt32());
						return true;
					}
				}
				else if (!(type == "preview"))
				{
				}
			}
			else if (num <= 2515107422U)
			{
				if (num <= 2113033893U)
				{
					if (num != 1895071941U)
					{
						if (num != 1911849560U)
						{
							if (num == 2113033893U)
							{
								if (type == "v3i")
								{
									if (this.Size != 12)
									{
										throw new EXRFormatException(string.Concat(new string[]
										{
											"Invalid or corrupt EXR header attribute '",
											this.Name,
											"' of type v3i: Size must be 12 bytes, was ",
											this.Size.ToString(),
											"."
										}));
									}
									this.Value = new V3I(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
									return true;
								}
							}
						}
						else if (type == "v2i")
						{
							if (this.Size != 8)
							{
								throw new EXRFormatException(string.Concat(new string[]
								{
									"Invalid or corrupt EXR header attribute '",
									this.Name,
									"' of type v2i: Size must be 8 bytes, was ",
									this.Size.ToString(),
									"."
								}));
							}
							this.Value = new V2I(reader.ReadInt32(), reader.ReadInt32());
							return true;
						}
					}
					else if (type == "v2f")
					{
						if (this.Size != 8)
						{
							throw new EXRFormatException(string.Concat(new string[]
							{
								"Invalid or corrupt EXR header attribute '",
								this.Name,
								"' of type v2f: Size must be 8 bytes, was ",
								this.Size.ToString(),
								"."
							}));
						}
						this.Value = new V2F(reader.ReadSingle(), reader.ReadSingle());
						return true;
					}
				}
				else if (num != 2129811512U)
				{
					if (num != 2495261410U)
					{
						if (num == 2515107422U)
						{
							if (type == "int")
							{
								if (this.Size != 4)
								{
									throw new EXRFormatException(string.Concat(new string[]
									{
										"Invalid or corrupt EXR header attribute '",
										this.Name,
										"' of type int: Size must be 4 bytes, was ",
										this.Size.ToString(),
										"."
									}));
								}
								this.Value = reader.ReadInt32();
								return true;
							}
						}
					}
					else if (type == "m44f")
					{
						if (this.Size != 64)
						{
							throw new EXRFormatException(string.Concat(new string[]
							{
								"Invalid or corrupt EXR header attribute '",
								this.Name,
								"' of type m44f: Size must be 64 bytes, was ",
								this.Size.ToString(),
								"."
							}));
						}
						this.Value = new M44F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
						return true;
					}
				}
				else if (type == "v3f")
				{
					if (this.Size != 12)
					{
						throw new EXRFormatException(string.Concat(new string[]
						{
							"Invalid or corrupt EXR header attribute '",
							this.Name,
							"' of type v3f: Size must be 12 bytes, was ",
							this.Size.ToString(),
							"."
						}));
					}
					this.Value = new V3F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
					return true;
				}
			}
			else if (num <= 3144536282U)
			{
				if (num != 2699759368U)
				{
					if (num != 2797886853U)
					{
						if (num == 3144536282U)
						{
							if (type == "envmap")
							{
								if (this.Size != 1)
								{
									throw new EXRFormatException(string.Concat(new string[]
									{
										"Invalid or corrupt EXR header attribute '",
										this.Name,
										"' of type envmap: Size must be 1 byte, was ",
										this.Size.ToString(),
										"."
									}));
								}
								this.Value = (EnvMap)reader.ReadByte();
								return true;
							}
						}
					}
					else if (type == "float")
					{
						if (this.Size != 4)
						{
							throw new EXRFormatException(string.Concat(new string[]
							{
								"Invalid or corrupt EXR header attribute '",
								this.Name,
								"' of type float: Size must be 4 bytes, was ",
								this.Size.ToString(),
								"."
							}));
						}
						this.Value = reader.ReadSingle();
						return true;
					}
				}
				else if (type == "double")
				{
					if (this.Size != 8)
					{
						throw new EXRFormatException(string.Concat(new string[]
						{
							"Invalid or corrupt EXR header attribute '",
							this.Name,
							"' of type double: Size must be 8 bytes, was ",
							this.Size.ToString(),
							"."
						}));
					}
					this.Value = reader.ReadDouble();
					return true;
				}
			}
			else if (num != 3489061551U)
			{
				if (num != 3499001626U)
				{
					if (num == 3877285684U)
					{
						if (type == "tiledesc")
						{
							if (this.Size != 9)
							{
								throw new EXRFormatException(string.Concat(new string[]
								{
									"Invalid or corrupt EXR header attribute '",
									this.Name,
									"' of type tiledesc: Size must be 9 bytes, was ",
									this.Size.ToString(),
									"."
								}));
							}
							this.Value = new TileDesc(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadByte());
							return true;
						}
					}
				}
				else if (type == "chlist")
				{
					ChannelList channelList = new ChannelList();
					try
					{
						channelList.Read(file, reader, this.Size);
					}
					catch (Exception ex3)
					{
						throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + this.Name + "' of type chlist: " + ex3.Message, ex3);
					}
					this.Value = channelList;
					return true;
				}
			}
			else if (type == "chromaticities")
			{
				if (this.Size != 32)
				{
					throw new EXRFormatException(string.Concat(new string[]
					{
						"Invalid or corrupt EXR header attribute '",
						this.Name,
						"' of type chromaticities: Size must be 32 bytes, was ",
						this.Size.ToString(),
						"."
					}));
				}
				this.Value = new Chromaticities(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				return true;
			}
			this.Value = reader.ReadBytes(this.Size);
			return true;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class SignData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData()
	{
	}

	public static SignData Duplicate(SignData original, string copyName = "")
	{
		SignData signData = new SignData();
		signData.guid = Guid.NewGuid();
		signData.name = (string.IsNullOrEmpty(copyName) ? original.name : copyName);
		signData.layers = null;
		signData.lastModified = DateTime.Now;
		signData.nextPolyId = original.nextPolyId;
		signData.nextGroupId = original.nextGroupId;
		signData.nextTextId = original.nextTextId;
		signData.nextNoiseId = original.nextNoiseId;
		signData.CloneLayersFrom(original);
		return signData;
	}

	public void CloneLayersFrom(SignData source)
	{
		if (source.layers == null)
		{
			Log.Error("Failed to clone layers from source with null layers list.");
			return;
		}
		if (this.layers != null)
		{
			this.layers.Clear();
		}
		else
		{
			this.layers = new List<SignData.SignLayer>();
		}
		foreach (SignData.SignLayer signLayer in source.layers)
		{
			this.layers.Add(signLayer.Clone());
		}
		this.nextPolyId = source.nextPolyId;
		this.nextTextId = source.nextTextId;
		this.nextNoiseId = source.nextNoiseId;
		this.nextGroupId = source.nextGroupId;
		this.lastModified = DateTime.Now;
	}

	public void Write(BinaryWriter bw)
	{
		StreamUtils.Write(bw, this.guid);
		bw.Write(this.name);
		bw.Write(this.lastModified.ToUniversalTime().Ticks);
		bw.Write(this.nextPolyId);
		bw.Write(this.nextTextId);
		bw.Write(this.nextNoiseId);
		bw.Write(this.nextGroupId);
		List<SignData.SignLayer> list = this.layers;
		bw.Write((list != null) ? list.Count : 0);
		if (this.layers != null)
		{
			foreach (SignData.SignLayer signLayer in this.layers)
			{
				signLayer.Write(bw);
			}
		}
	}

	public static SignData Read(BinaryReader br)
	{
		SignData signData = new SignData
		{
			guid = StreamUtils.ReadGuid(br),
			name = br.ReadString(),
			lastModified = new DateTime(br.ReadInt64(), DateTimeKind.Utc)
		};
		signData.nextPolyId = br.ReadInt32();
		signData.nextTextId = br.ReadInt32();
		signData.nextNoiseId = br.ReadInt32();
		signData.nextGroupId = br.ReadInt32();
		int num = br.ReadInt32();
		signData.layers = new List<SignData.SignLayer>(num);
		for (int i = 0; i < num; i++)
		{
			signData.layers.Add(SignData.SignLayer.Read(br));
		}
		return signData;
	}

	public void WriteXml(XmlElement parentElement)
	{
		XmlElement xmlElement = parentElement.AddXmlElement("sign");
		xmlElement.SetAttrib("guid", this.guid.ToString());
		xmlElement.SetAttrib("name", this.name.ToString());
		xmlElement.SetAttrib("modified", this.lastModified.ToString("u", Utils.StandardCulture));
		xmlElement.SetAttribute("next_poly_id", this.nextPolyId.ToString());
		xmlElement.SetAttribute("next_text_id", this.nextTextId.ToString());
		xmlElement.SetAttribute("next_noise_id", this.nextNoiseId.ToString());
		xmlElement.SetAttribute("next_group_id", this.nextGroupId.ToString());
		foreach (SignData.SignLayer signLayer in this.layers)
		{
			signLayer.WriteXml(xmlElement.AddXmlElement("layer"));
		}
	}

	public static SignData ReadXML(XElement signElement)
	{
		SignData.<>c__DisplayClass30_0 CS$<>8__locals1;
		CS$<>8__locals1.signElement = signElement;
		SignData signData = new SignData
		{
			guid = Guid.Parse(CS$<>8__locals1.signElement.GetAttribute("guid")),
			name = CS$<>8__locals1.signElement.GetAttribute("name"),
			lastModified = DateTime.ParseExact(CS$<>8__locals1.signElement.GetAttribute("modified"), "u", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
			layers = new List<SignData.SignLayer>()
		};
		signData.nextPolyId = SignData.<ReadXML>g__ReadNextIdAttribute|30_0("next_poly_id", ref CS$<>8__locals1);
		signData.nextTextId = SignData.<ReadXML>g__ReadNextIdAttribute|30_0("next_text_id", ref CS$<>8__locals1);
		signData.nextNoiseId = SignData.<ReadXML>g__ReadNextIdAttribute|30_0("next_noise_id", ref CS$<>8__locals1);
		signData.nextGroupId = SignData.<ReadXML>g__ReadNextIdAttribute|30_0("next_group_id", ref CS$<>8__locals1);
		foreach (XElement xelement in CS$<>8__locals1.signElement.Elements("layer"))
		{
			signData.layers.Add(SignData.SignLayer.LayerFromXml(xelement));
		}
		return signData;
	}

	public void SetLayerDefaultName(SignData.SignLayer layer)
	{
		SignData.PolygonSignLayer polygonSignLayer = layer as SignData.PolygonSignLayer;
		if (polygonSignLayer != null)
		{
			this.nextPolyId++;
			polygonSignLayer.SetDefaultName(this.nextPolyId);
			return;
		}
		SignData.TextSignLayer textSignLayer = layer as SignData.TextSignLayer;
		if (textSignLayer != null)
		{
			this.nextTextId++;
			textSignLayer.SetDefaultName(this.nextTextId);
			return;
		}
		SignData.NoiseSignLayer noiseSignLayer = layer as SignData.NoiseSignLayer;
		if (noiseSignLayer != null)
		{
			this.nextNoiseId++;
			noiseSignLayer.SetDefaultName(this.nextNoiseId);
			return;
		}
		SignData.GroupSignLayer groupSignLayer = layer as SignData.GroupSignLayer;
		if (groupSignLayer == null)
		{
			return;
		}
		this.nextGroupId++;
		groupSignLayer.SetDefaultName(this.nextGroupId);
	}

	public void UnpackGroups(bool recursive)
	{
		for (int i = this.layers.Count - 1; i >= 0; i--)
		{
			int num;
			this.TryUnpackTopLevelGroup(i, recursive, out num);
		}
	}

	public bool TryUnpackTopLevelGroup(int groupIndex, bool recursive, out int unpackedLayerCount)
	{
		unpackedLayerCount = 0;
		SignData.GroupSignLayer groupSignLayer = this.layers[groupIndex] as SignData.GroupSignLayer;
		if (groupSignLayer != null)
		{
			this.layers.RemoveAt(groupIndex);
			this.UnpackLayers(groupSignLayer.transform, groupSignLayer.layers, this.layers, groupIndex, recursive, ref unpackedLayerCount);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnpackLayers(SignData.SignTransform parentTransform, List<SignData.SignLayer> sourceLayers, List<SignData.SignLayer> destLayers, int destIndex, bool recursive, ref int unpackedLayerCount)
	{
		int i = sourceLayers.Count - 1;
		while (i >= 0)
		{
			SignData.SignLayer signLayer = sourceLayers[i];
			sourceLayers.RemoveAt(i);
			if (!recursive)
			{
				goto IL_0049;
			}
			SignData.GroupSignLayer groupSignLayer = signLayer as SignData.GroupSignLayer;
			if (groupSignLayer == null)
			{
				goto IL_0049;
			}
			this.UnpackLayers(parentTransform * groupSignLayer.transform, groupSignLayer.layers, destLayers, destIndex, recursive, ref unpackedLayerCount);
			IL_006C:
			i--;
			continue;
			IL_0049:
			signLayer.transform = parentTransform * signLayer.transform;
			destLayers.Insert(destIndex, signLayer);
			unpackedLayerCount++;
			goto IL_006C;
		}
	}

	public static SignData GetTestSignData(string text = "Hello World!", float hueOffset = 0f)
	{
		SignData signData = new SignData();
		signData.guid = Guid.NewGuid();
		signData.name = text;
		List<SignData.SignLayer> list = new List<SignData.SignLayer>();
		for (int i = 0; i < 20; i++)
		{
			float num = (float)i / 20f;
			float num2 = 8f + Mathf.Lerp(360f, 0f, num);
			float num3 = (Mathf.Lerp(1f, 0f, num) + hueOffset) % 1f;
			list.Add(new SignData.TextSignLayer("Text Sign Layer", Vector2.zero, num2, SignData.DefaultSignData.textSize, new SignData.SignRenderSettings(Color.HSVToRGB(num3, 1f, 1f), SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), "LiberationSans", text, 0f, 0f, 0f, 0.5f));
		}
		list.Add(new SignData.GroupSignLayer("Group Sign Layer", new Vector2(-1f, 0.3f), 8f, Vector2.one, new SignData.SignRenderSettings(Color.white, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), SignData.DefaultSignData.PolygonGroupLayers, SignData.GroupSignLayer.OffsetTarget.All, 0f, 0f, SignData.GroupSignLayer.ColorMode.Multiply));
		list.Add(new SignData.GroupSignLayer("Group Sign Layer", new Vector2(-0.7f, 0.45f), 8f, 0.5f * Vector2.one, new SignData.SignRenderSettings(Color.white, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), SignData.DefaultSignData.PolygonGroupLayers, SignData.GroupSignLayer.OffsetTarget.All, 0f, 0f, SignData.GroupSignLayer.ColorMode.Multiply));
		list.Add(new SignData.GroupSignLayer("Group Sign Layer", new Vector2(0.5f, -0.5f), 8f, 1.25f * Vector2.one, new SignData.SignRenderSettings(Color.white, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), SignData.DefaultSignData.PolygonGroupLayers, SignData.GroupSignLayer.OffsetTarget.All, 0f, 0f, SignData.GroupSignLayer.ColorMode.Multiply));
		list.Add(new SignData.TextSignLayer("Text Sign Layer", new Vector2(0.025f, -0.025f), 8f, SignData.DefaultSignData.textSize, new SignData.SignRenderSettings(Color.black, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), "LiberationSans", text, 0f, 0f, 1f, 0.25f));
		list.Add(new SignData.TextSignLayer("Text Sign Layer", Vector2.zero, 8f, SignData.DefaultSignData.textSize, new SignData.SignRenderSettings(Color.white, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), "LiberationSans", text, 0f, 0f, 0f, 0.5f));
		list.Add(new SignData.TextSignLayer("Text Sign Layer", Vector2.zero, 8f, SignData.DefaultSignData.textSize, new SignData.SignRenderSettings(Color.HSVToRGB(0f + hueOffset, 1f, 1f), SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), "LiberationSans", text, 0f, 0f, 0f, 0.15f));
		list.Add(new SignData.TextSignLayer("Text Sign Layer", new Vector2(-0.005f, 0.005f), 8f, SignData.DefaultSignData.textSize, new SignData.SignRenderSettings(Color.HSVToRGB(0.05f + hueOffset, 0.9f, 1f), SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), "LiberationSans", text, 0f, 0f, 1f, -0.5f));
		signData.layers = new List<SignData.SignLayer>
		{
			new SignData.GroupSignLayer("Group Sign Layer", Vector2.zero, 0f, 0.55f * Vector2.one, new SignData.SignRenderSettings(Color.white, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), list, SignData.GroupSignLayer.OffsetTarget.All, 0f, 0f, SignData.GroupSignLayer.ColorMode.Multiply)
		};
		signData.lastModified = DateTime.Now;
		return signData;
	}

	public static SignData GetBasicTestSignData(string text = "Hello World!", float hueOffset = 0f)
	{
		return new SignData
		{
			guid = Guid.NewGuid(),
			name = text,
			layers = new List<SignData.SignLayer>
			{
				new SignData.PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 0.85f * Vector2.one, new SignData.SignRenderSettings(Color.HSVToRGB(0.58f + hueOffset, 1f, 1f), SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), 8, 0f, 0f, 0f, 0f, 20f, SignData.PolygonSignLayer.ShapeMode.Normal),
				new SignData.TextSignLayer("Text Sign Layer", Vector2.zero, 8f, 0.55f * SignData.DefaultSignData.textSize, new SignData.SignRenderSettings(Color.HSVToRGB(0.16f + hueOffset, 1f, 1f), SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), "LiberationSans", text, 0f, 0f, 0f, 0.15f)
			},
			lastModified = DateTime.Now
		};
	}

	public static SignData GetErrorSignData()
	{
		return new SignData
		{
			guid = Guid.NewGuid(),
			name = "Error",
			layers = new List<SignData.SignLayer>
			{
				new SignData.PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 3f * Vector2.one, new SignData.SignRenderSettings(new Color(1f, 0f, 1f), SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), 4, 1f, 0f, 0f, 0f, 20f, SignData.PolygonSignLayer.ShapeMode.Normal)
			},
			lastModified = DateTime.Now
		};
	}

	public static SignData GetNewSignData(string signName)
	{
		return new SignData
		{
			guid = Guid.NewGuid(),
			name = signName,
			layers = new List<SignData.SignLayer>(),
			lastModified = DateTime.Now
		};
	}

	public static SignData GetDefaultSignData()
	{
		SignData testSignData = SignData.GetTestSignData("Hello World!", 0f);
		testSignData.guid = SignData.defaultSignDataID.signGuid;
		return testSignData;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int <ReadXML>g__ReadNextIdAttribute|30_0(string attribute, ref SignData.<>c__DisplayClass30_0 A_1)
	{
		string text;
		if (A_1.signElement.TryGetAttribute(attribute, out text))
		{
			return StringParsers.ParseSInt32(text, 0, -1, NumberStyles.Integer);
		}
		return 0;
	}

	public Guid guid;

	public string name;

	public List<SignData.SignLayer> layers;

	public DateTime lastModified;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextPolyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextTextId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextNoiseId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextGroupId;

	public static readonly GlobalSignId defaultSignDataID = new GlobalSignId("[D]", Guid.Empty);

	public struct SignTransform : IEquatable<SignData.SignTransform>
	{
		public bool HasUniformScale
		{
			get
			{
				return Mathf.Approximately(this.scale.x, this.scale.y);
			}
		}

		public SignTransform(Vector2 position, float rotation, Vector2 scale)
		{
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
		}

		public static SignData.SignTransform operator *(SignData.SignTransform parent, SignData.SignTransform child)
		{
			float num = parent.rotation * 0.017453292f;
			Vector2 vector = Vector2.Scale(child.position, parent.scale);
			Vector2 vector2;
			vector2..ctor(vector.x * Mathf.Cos(num) - vector.y * Mathf.Sin(num), vector.x * Mathf.Sin(num) + vector.y * Mathf.Cos(num));
			Vector2 vector3 = parent.position + vector2;
			float num2 = parent.rotation + child.rotation;
			Vector2 vector4 = Vector2.Scale(parent.scale, child.scale);
			return new SignData.SignTransform(vector3, num2, vector4);
		}

		public bool Equals(SignData.SignTransform other)
		{
			return this.position == other.position && Mathf.Approximately(this.rotation, other.rotation) && this.scale == other.scale;
		}

		public override bool Equals(object obj)
		{
			if (obj is SignData.SignTransform)
			{
				SignData.SignTransform signTransform = (SignData.SignTransform)obj;
				return this.Equals(signTransform);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((17 * 23 + this.position.GetHashCode()) * 23 + this.rotation.GetHashCode()) * 23 + this.scale.GetHashCode();
		}

		public static bool operator ==(SignData.SignTransform left, SignData.SignTransform right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SignData.SignTransform left, SignData.SignTransform right)
		{
			return !left.Equals(right);
		}

		public void WriteXml(XmlElement element)
		{
			element.SetAttrib("pos", string.Format("{0}, {1}", this.position.x, this.position.y));
			element.SetAttrib("rot", this.rotation.ToCultureInvariantString());
			element.SetAttrib("scale", string.Format("{0}, {1}", this.scale.x, this.scale.y));
		}

		public void ReadXml(XElement element)
		{
			this.position = StringParsers.ParseVector2(element.GetAttribute("pos"));
			this.rotation = float.Parse(element.GetAttribute("rot"), Utils.StandardCulture);
			this.scale = StringParsers.ParseVector2(element.GetAttribute("scale"));
		}

		public void Write(BinaryWriter bw)
		{
			StreamUtils.Write(bw, this.position);
			bw.Write(this.rotation);
			StreamUtils.Write(bw, this.scale);
		}

		public static SignData.SignTransform Read(BinaryReader br)
		{
			Vector2 vector = StreamUtils.ReadVector2(br);
			float num = br.ReadSingle();
			Vector2 vector2 = StreamUtils.ReadVector2(br);
			return new SignData.SignTransform(vector, num, vector2);
		}

		public Vector2 position;

		public float rotation;

		public Vector2 scale;

		public static readonly SignData.SignTransform Defaults = new SignData.SignTransform(Vector2.zero, 0f, new Vector2(0.3f, 0.3f));

		public static readonly SignData.SignTransform Identity = new SignData.SignTransform(Vector2.zero, 0f, Vector2.one);
	}

	public struct SignRenderSettings
	{
		public SignRenderSettings(Color color, SignData.SignRenderSettings.Mode mode = SignData.SignRenderSettings.Mode.ColorOnly)
		{
			this.color = color;
			this.mode = mode;
		}

		public void WriteXml(XmlElement element)
		{
			element.SetAttrib("color", "#" + ColorUtility.ToHtmlStringRGBA(this.color));
			element.SetAttrib("mode", this.mode.ToString());
		}

		public void ReadXml(XElement element)
		{
			string text = (element.HasAttribute("tint") ? element.GetAttribute("tint") : element.GetAttribute("color"));
			if (!ColorUtility.TryParseHtmlString(text, ref this.color))
			{
				Log.Error("Invalid color string: " + text);
				this.color = Color.white;
			}
			if (element.HasAttribute("mode"))
			{
				string attribute = element.GetAttribute("mode");
				if (!Enum.TryParse<SignData.SignRenderSettings.Mode>(attribute, true, out this.mode))
				{
					Log.Error("Failed to parse SignRenderSettings mode from string: \"" + attribute + "\"");
					this.mode = SignData.SignRenderSettings.Mode.ColorAndMask;
				}
			}
		}

		public void Write(BinaryWriter bw)
		{
			StreamUtils.Write(bw, this.color);
			bw.Write((byte)this.mode);
		}

		public static SignData.SignRenderSettings Read(BinaryReader br)
		{
			Color color = StreamUtils.ReadColor(br);
			SignData.SignRenderSettings.Mode mode = (SignData.SignRenderSettings.Mode)br.ReadByte();
			return new SignData.SignRenderSettings(color, mode);
		}

		public Color color;

		public SignData.SignRenderSettings.Mode mode;

		public enum Mode : byte
		{
			ColorOnly,
			ColorAndMask,
			MaskOnly,
			PunchOut
		}
	}

	public abstract class SignWarp
	{
		public abstract SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public abstract SignData.SignWarp Clone();

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void ReadXml(XElement element);

		public abstract void WriteXml(XmlElement xmlElement);

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void InternalRead(BinaryReader br);

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void InternalWrite(BinaryWriter bw);

		public void Write(BinaryWriter bw)
		{
			bw.Write((byte)this.TypeId);
			this.InternalWrite(bw);
		}

		public static SignData.SignWarp Read(BinaryReader br)
		{
			SignData.SignWarp.WarpType warpType = (SignData.SignWarp.WarpType)br.ReadByte();
			SignData.SignWarp signWarp;
			switch (warpType)
			{
			case SignData.SignWarp.WarpType.Skew:
				signWarp = new SignData.SkewWarp();
				break;
			case SignData.SignWarp.WarpType.Bulge:
				signWarp = new SignData.BulgeWarp();
				break;
			case SignData.SignWarp.WarpType.Twirl:
				signWarp = new SignData.TwirlWarp();
				break;
			case SignData.SignWarp.WarpType.Kaleido:
				signWarp = new SignData.KaleidoWarp();
				break;
			case SignData.SignWarp.WarpType.Perspective:
				signWarp = new SignData.PerspectiveWarp();
				break;
			case SignData.SignWarp.WarpType.Arc:
				signWarp = new SignData.ArcWarp();
				break;
			case SignData.SignWarp.WarpType.Stretch:
				signWarp = new SignData.StretchWarp();
				break;
			case SignData.SignWarp.WarpType.Grid:
				signWarp = new SignData.GridWarp();
				break;
			default:
				throw new Exception(string.Format("Unknown warp type id: {0}", (byte)warpType));
			}
			SignData.SignWarp signWarp2 = signWarp;
			signWarp2.InternalRead(br);
			return signWarp2;
		}

		public static SignData.SignWarp WarpFromXml(XElement element)
		{
			string attribute = element.GetAttribute("type");
			uint num = <PrivateImplementationDetails>.ComputeStringHash(attribute);
			SignData.SignWarp signWarp;
			if (num <= 2239152927U)
			{
				if (num <= 812224709U)
				{
					if (num != 542699362U)
					{
						if (num == 812224709U)
						{
							if (attribute == "ArcWarp")
							{
								signWarp = new SignData.ArcWarp();
								goto IL_015C;
							}
						}
					}
					else if (attribute == "KaleidoWarp")
					{
						signWarp = new SignData.KaleidoWarp();
						goto IL_015C;
					}
				}
				else if (num != 1765275369U)
				{
					if (num == 2239152927U)
					{
						if (attribute == "PerspectiveWarp")
						{
							signWarp = new SignData.PerspectiveWarp();
							goto IL_015C;
						}
					}
				}
				else if (attribute == "SkewWarp")
				{
					signWarp = new SignData.SkewWarp();
					goto IL_015C;
				}
			}
			else if (num <= 3251058595U)
			{
				if (num != 2846920778U)
				{
					if (num == 3251058595U)
					{
						if (attribute == "TwirlWarp")
						{
							signWarp = new SignData.TwirlWarp();
							goto IL_015C;
						}
					}
				}
				else if (attribute == "BulgeWarp")
				{
					signWarp = new SignData.BulgeWarp();
					goto IL_015C;
				}
			}
			else if (num != 3544788227U)
			{
				if (num == 4106962192U)
				{
					if (attribute == "StretchWarp")
					{
						signWarp = new SignData.StretchWarp();
						goto IL_015C;
					}
				}
			}
			else if (attribute == "GridWarp")
			{
				signWarp = new SignData.GridWarp();
				goto IL_015C;
			}
			throw new Exception("Unknown warp type: " + attribute);
			IL_015C:
			SignData.SignWarp signWarp2 = signWarp;
			signWarp2.ReadXml(element);
			return signWarp2;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public SignWarp()
		{
		}

		public enum WarpType : byte
		{
			Skew,
			Bulge,
			Twirl,
			Kaleido,
			Perspective,
			Arc,
			Stretch,
			Grid
		}
	}

	public class SkewWarp : SignData.SignWarp
	{
		public SkewWarp()
		{
			this.amount = new Vector2(0f, 0.1f);
			this.rotation = 0f;
		}

		public SkewWarp(Vector2 amount, float rotation)
		{
			this.amount = amount;
			this.rotation = rotation;
		}

		public override SignData.SignWarp Clone()
		{
			return new SignData.SkewWarp(this.amount, this.rotation);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", base.GetType().Name);
			element.SetAttrib("amount", string.Format("{0}, {1}", this.amount.x, this.amount.y));
			element.SetAttrib("rotation", this.rotation.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			this.amount = StringParsers.ParseVector2(element.GetAttribute("amount"));
			this.rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
		}

		public override SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignWarp.WarpType.Skew;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, this.amount);
			bw.Write(this.rotation);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.amount = StreamUtils.ReadVector2(br);
			this.rotation = br.ReadSingle();
		}

		public Vector2 amount;

		public float rotation;
	}

	public class BulgeWarp : SignData.SignWarp
	{
		public BulgeWarp()
		{
			this.offset = Vector2.zero;
			this.amount = 2f;
		}

		public BulgeWarp(Vector2 offset, float amount)
		{
			this.offset = offset;
			this.amount = amount;
		}

		public override SignData.SignWarp Clone()
		{
			return new SignData.BulgeWarp(this.offset, this.amount);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", base.GetType().Name);
			element.SetAttrib("offset", string.Format("{0}, {1}", this.offset.x, this.offset.y));
			element.SetAttrib("amount", this.amount.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			this.offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			this.amount = float.Parse(element.GetAttribute("amount"), Utils.StandardCulture);
		}

		public override SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignWarp.WarpType.Bulge;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, this.offset);
			bw.Write(this.amount);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.offset = StreamUtils.ReadVector2(br);
			this.amount = br.ReadSingle();
		}

		public Vector2 offset;

		public float amount;
	}

	public class TwirlWarp : SignData.SignWarp
	{
		public TwirlWarp()
		{
			this.offset = Vector2.zero;
			this.amount = 0.2f;
			this.frequency = 0.2f;
		}

		public TwirlWarp(Vector2 offset, float amount, float frequency)
		{
			this.offset = offset;
			this.amount = amount;
			this.frequency = frequency;
		}

		public override SignData.SignWarp Clone()
		{
			return new SignData.TwirlWarp(this.offset, this.amount, this.frequency);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", base.GetType().Name);
			element.SetAttrib("offset", string.Format("{0}, {1}", this.offset.x, this.offset.y));
			element.SetAttrib("amount", this.amount.ToCultureInvariantString() ?? "");
			element.SetAttrib("frequency", this.frequency.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			this.offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			this.amount = float.Parse(element.GetAttribute("amount"), Utils.StandardCulture);
			this.frequency = float.Parse(element.GetAttribute("frequency"), Utils.StandardCulture);
		}

		public override SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignWarp.WarpType.Twirl;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, this.offset);
			bw.Write(this.amount);
			bw.Write(this.frequency);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.offset = StreamUtils.ReadVector2(br);
			this.amount = br.ReadSingle();
			this.frequency = br.ReadSingle();
		}

		public Vector2 offset;

		public float amount;

		public float frequency;
	}

	public class KaleidoWarp : SignData.SignWarp
	{
		public KaleidoWarp()
		{
			this.offset = Vector2.zero;
			this.sides = 6;
			this.rotation = 0f;
			this.offsetScale = 1f;
		}

		public KaleidoWarp(Vector2 offset, int sides, float rotation, float offsetScale)
		{
			this.offset = offset;
			this.sides = sides;
			this.rotation = rotation;
			this.offsetScale = offsetScale;
		}

		public override SignData.SignWarp Clone()
		{
			return new SignData.KaleidoWarp(this.offset, this.sides, this.rotation, this.offsetScale);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", base.GetType().Name);
			element.SetAttrib("offset", string.Format("{0}, {1}", this.offset.x, this.offset.y));
			element.SetAttrib("sides", string.Format("{0}", this.sides));
			element.SetAttrib("rotation", this.rotation.ToCultureInvariantString() ?? "");
			element.SetAttrib("offsetScale", this.offsetScale.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			this.offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			this.sides = int.Parse(element.GetAttribute("sides"));
			this.rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
			this.offsetScale = (element.HasAttribute("offsetScale") ? float.Parse(element.GetAttribute("offsetScale"), Utils.StandardCulture) : 1f);
		}

		public override SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignWarp.WarpType.Kaleido;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, this.offset);
			bw.Write(this.sides);
			bw.Write(this.rotation);
			bw.Write(this.offsetScale);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.offset = StreamUtils.ReadVector2(br);
			this.sides = br.ReadInt32();
			this.rotation = br.ReadSingle();
			this.offsetScale = br.ReadSingle();
		}

		public Vector2 offset;

		public int sides;

		public float rotation;

		public float offsetScale;
	}

	public class PerspectiveWarp : SignData.SignWarp
	{
		public PerspectiveWarp()
		{
			this.rotation = new Vector3(0f, 30f, 0f);
			this.strength = 0.1f;
		}

		public PerspectiveWarp(Vector3 rotation, float strength)
		{
			this.rotation = rotation;
			this.strength = strength;
		}

		public override SignData.SignWarp Clone()
		{
			return new SignData.PerspectiveWarp(this.rotation, this.strength);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", base.GetType().Name);
			element.SetAttrib("rotation", string.Format("{0}, {1}, {2}", this.rotation.x, this.rotation.y, this.rotation.z));
			element.SetAttrib("strength", this.strength.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			this.rotation = StringParsers.ParseVector3(element.GetAttribute("rotation"), 0, -1);
			this.strength = float.Parse(element.GetAttribute("strength"), Utils.StandardCulture);
		}

		public override SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignWarp.WarpType.Perspective;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, this.rotation);
			bw.Write(this.strength);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.rotation = StreamUtils.ReadVector3(br);
			this.strength = br.ReadSingle();
		}

		public Vector3 rotation;

		public float strength;
	}

	public class ArcWarp : SignData.SignWarp
	{
		public ArcWarp()
		{
			this.rotation = 0f;
			this.radius = 2f;
			this.width = 0.1f;
		}

		public ArcWarp(float rotation, float radius, float width)
		{
			this.rotation = rotation;
			this.radius = radius;
			this.width = width;
		}

		public override SignData.SignWarp Clone()
		{
			return new SignData.ArcWarp(this.rotation, this.radius, this.width);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", base.GetType().Name);
			element.SetAttrib("rotation", this.rotation.ToCultureInvariantString() ?? "");
			element.SetAttrib("radius", this.radius.ToCultureInvariantString() ?? "");
			element.SetAttrib("width", this.width.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			this.rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
			this.radius = float.Parse(element.GetAttribute("radius"), Utils.StandardCulture);
			this.width = float.Parse(element.GetAttribute("width"), Utils.StandardCulture);
		}

		public override SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignWarp.WarpType.Arc;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write(this.rotation);
			bw.Write(this.radius);
			bw.Write(this.width);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.rotation = br.ReadSingle();
			this.radius = br.ReadSingle();
			this.width = br.ReadSingle();
		}

		public float rotation;

		public float radius;

		public float width;
	}

	public class StretchWarp : SignData.SignWarp
	{
		public StretchWarp()
		{
			this.offset = new Vector2(0f, 0f);
			this.rotation = 0f;
			this.distance = 0f;
			this.width = 0f;
			this.exponent = 0f;
		}

		public StretchWarp(Vector2 offset, float rotation, float distance, float width, float exponent)
		{
			this.offset = offset;
			this.rotation = rotation;
			this.distance = distance;
			this.width = width;
			this.exponent = exponent;
		}

		public override SignData.SignWarp Clone()
		{
			return new SignData.StretchWarp(this.offset, this.rotation, this.distance, this.width, this.exponent);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", base.GetType().Name);
			element.SetAttrib("offset", string.Format("{0}, {1}", this.offset.x, this.offset.y));
			element.SetAttrib("rotation", this.rotation.ToCultureInvariantString() ?? "");
			element.SetAttrib("distance", this.distance.ToCultureInvariantString() ?? "");
			element.SetAttrib("width", this.width.ToCultureInvariantString() ?? "");
			element.SetAttrib("exponent", this.exponent.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			this.offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			this.rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
			this.distance = float.Parse(element.GetAttribute("distance"), Utils.StandardCulture);
			this.width = float.Parse(element.GetAttribute("width"), Utils.StandardCulture);
			this.exponent = float.Parse(element.GetAttribute("exponent"), Utils.StandardCulture);
		}

		public override SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignWarp.WarpType.Stretch;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, this.offset);
			bw.Write(this.rotation);
			bw.Write(this.distance);
			bw.Write(this.width);
			bw.Write(this.exponent);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.offset = StreamUtils.ReadVector2(br);
			this.rotation = br.ReadSingle();
			this.distance = br.ReadSingle();
			this.width = br.ReadSingle();
			this.exponent = br.ReadSingle();
		}

		public Vector2 offset;

		public float rotation;

		public float distance;

		public float width;

		public float exponent;
	}

	public class GridWarp : SignData.SignWarp
	{
		public GridWarp()
		{
			this.mode = SignData.GridWarp.Mode.Rectangle;
			this.offset = Vector2.zero;
			this.rotation = 0f;
			this.scale = 1f;
			this.aspect = 1f;
			this.shift = 0f;
		}

		public GridWarp(SignData.GridWarp.Mode mode, Vector2 offset, float rotation, float scale, float aspect, float shift)
		{
			this.mode = mode;
			this.offset = offset;
			this.rotation = rotation;
			this.scale = scale;
			this.aspect = aspect;
			this.shift = shift;
		}

		public override SignData.SignWarp Clone()
		{
			return new SignData.GridWarp(this.mode, this.offset, this.rotation, this.scale, this.aspect, this.shift);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", base.GetType().Name);
			element.SetAttrib("mode", string.Format("{0}", (int)this.mode));
			element.SetAttrib("offset", string.Format("{0}, {1}", this.offset.x, this.offset.y));
			element.SetAttrib("rotation", this.rotation.ToCultureInvariantString() ?? "");
			element.SetAttrib("scale", this.scale.ToCultureInvariantString() ?? "");
			element.SetAttrib("aspect", this.aspect.ToCultureInvariantString() ?? "");
			element.SetAttrib("shift", this.shift.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			this.mode = (SignData.GridWarp.Mode)int.Parse(element.GetAttribute("mode"));
			this.offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			this.rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
			this.scale = float.Parse(element.GetAttribute("scale"), Utils.StandardCulture);
			this.aspect = (element.HasAttribute("aspect") ? float.Parse(element.GetAttribute("aspect"), Utils.StandardCulture) : 1f);
			this.shift = (element.HasAttribute("shift") ? float.Parse(element.GetAttribute("shift"), Utils.StandardCulture) : 0f);
		}

		public override SignData.SignWarp.WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignWarp.WarpType.Grid;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write((int)this.mode);
			StreamUtils.Write(bw, this.offset);
			bw.Write(this.rotation);
			bw.Write(this.scale);
			bw.Write(this.aspect);
			bw.Write(this.shift);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.mode = (SignData.GridWarp.Mode)br.ReadInt32();
			this.offset = StreamUtils.ReadVector2(br);
			this.rotation = br.ReadSingle();
			this.scale = br.ReadSingle();
			this.aspect = br.ReadSingle();
			this.shift = br.ReadSingle();
		}

		public SignData.GridWarp.Mode mode;

		public Vector2 offset;

		public float rotation;

		public float scale;

		public float aspect;

		public float shift;

		public enum Mode
		{
			Column,
			Rectangle,
			Hex
		}
	}

	public abstract class SignLayer
	{
		public bool HasWarps
		{
			get
			{
				List<SignData.SignWarp> list = this.warps;
				return list != null && list.Count > 0;
			}
		}

		public bool HasTransform
		{
			get
			{
				return this.transform != SignData.SignTransform.Identity;
			}
		}

		public bool HasTransformOrWarps
		{
			get
			{
				return this.HasTransform || this.HasWarps;
			}
		}

		public abstract SignData.SignLayer.LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public abstract string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public abstract SignData.SignLayer Clone();

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void InternalRead(BinaryReader br);

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void InternalWrite(BinaryWriter bw);

		public SignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignData.SignRenderSettings renderSettings, List<SignData.SignWarp> warps)
		{
			this.name = name;
			this.transform.position = position;
			this.transform.rotation = rotation;
			this.transform.scale = scale;
			this.renderSettings = renderSettings;
			this.warps = warps;
		}

		public SignLayer()
		{
			this.name = string.Empty;
			this.transform.position = Vector2.zero;
			this.transform.rotation = 0f;
			this.transform.scale = new Vector2(0.3f, 0.3f);
			this.renderSettings = new SignData.SignRenderSettings(Color.white, SignData.SignRenderSettings.Mode.ColorOnly);
			this.warps = new List<SignData.SignWarp>();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<SignData.SignWarp> CloneWarps()
		{
			List<SignData.SignWarp> list = new List<SignData.SignWarp>();
			foreach (SignData.SignWarp signWarp in this.warps)
			{
				list.Add(signWarp.Clone());
			}
			return list;
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write((byte)this.TypeId);
			this.InternalWrite(bw);
			bw.Write(this.name ?? "");
			this.transform.Write(bw);
			this.renderSettings.Write(bw);
			List<SignData.SignWarp> list = this.warps;
			bw.Write((list != null) ? list.Count : 0);
			if (this.warps != null)
			{
				foreach (SignData.SignWarp signWarp in this.warps)
				{
					signWarp.Write(bw);
				}
			}
		}

		public static SignData.SignLayer Read(BinaryReader br)
		{
			SignData.SignLayer.LayerType layerType = (SignData.SignLayer.LayerType)br.ReadByte();
			SignData.SignLayer signLayer;
			switch (layerType)
			{
			case SignData.SignLayer.LayerType.Group:
				signLayer = new SignData.GroupSignLayer();
				break;
			case SignData.SignLayer.LayerType.Text:
				signLayer = new SignData.TextSignLayer();
				break;
			case SignData.SignLayer.LayerType.Polygon:
				signLayer = new SignData.PolygonSignLayer();
				break;
			case SignData.SignLayer.LayerType.Noise:
				signLayer = new SignData.NoiseSignLayer();
				break;
			default:
				throw new Exception(string.Format("Unknown layer type id: {0}", (byte)layerType));
			}
			SignData.SignLayer signLayer2 = signLayer;
			signLayer2.InternalRead(br);
			signLayer2.name = br.ReadString();
			signLayer2.transform = SignData.SignTransform.Read(br);
			signLayer2.renderSettings = SignData.SignRenderSettings.Read(br);
			int num = br.ReadInt32();
			signLayer2.warps = new List<SignData.SignWarp>(num);
			for (int i = 0; i < num; i++)
			{
				signLayer2.warps.Add(SignData.SignWarp.Read(br));
			}
			return signLayer2;
		}

		public static SignData.SignLayer LayerFromXml(XElement element)
		{
			string attribute = element.GetAttribute("type");
			SignData.SignLayer signLayer;
			if (!(attribute == "TextSignLayer"))
			{
				if (!(attribute == "PolygonSignLayer"))
				{
					if (!(attribute == "GroupSignLayer"))
					{
						if (!(attribute == "NoiseSignLayer"))
						{
							throw new Exception("Unknown layer type: " + attribute);
						}
						signLayer = new SignData.NoiseSignLayer();
					}
					else
					{
						signLayer = new SignData.GroupSignLayer();
					}
				}
				else
				{
					signLayer = new SignData.PolygonSignLayer();
				}
			}
			else
			{
				signLayer = new SignData.TextSignLayer();
			}
			SignData.SignLayer signLayer2 = signLayer;
			signLayer2.ReadXml(element);
			return signLayer2;
		}

		public virtual void WriteXml(XmlElement element)
		{
			element.SetAttribute("name", this.name);
			element.SetAttrib("type", base.GetType().Name);
			this.transform.WriteXml(element);
			this.renderSettings.WriteXml(element);
			foreach (SignData.SignWarp signWarp in this.warps)
			{
				signWarp.WriteXml(element.AddXmlElement("warp"));
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void ReadXml(XElement element)
		{
			element.TryGetAttribute("name", out this.name);
			this.transform.ReadXml(element);
			this.renderSettings.ReadXml(element);
			foreach (XElement xelement in element.Elements("warp"))
			{
				this.warps.Add(SignData.SignWarp.WarpFromXml(xelement));
			}
		}

		public void SetDefaultName(int idx)
		{
			this.name = string.Format(this.defaultName + " " + idx.ToString("00"), Array.Empty<object>());
		}

		public string name;

		public SignData.SignTransform transform;

		public SignData.SignRenderSettings renderSettings;

		public List<SignData.SignWarp> warps;

		public enum LayerType : byte
		{
			Group,
			Text,
			Polygon,
			Noise
		}
	}

	public class GroupSignLayer : SignData.SignLayer
	{
		public override string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "Group";
			}
		}

		public override SignData.SignLayer.LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignLayer.LayerType.Group;
			}
		}

		public GroupSignLayer()
		{
		}

		public GroupSignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignData.SignRenderSettings renderSettings, List<SignData.SignWarp> warps, List<SignData.SignLayer> layers, SignData.GroupSignLayer.OffsetTarget offsetTarget = SignData.GroupSignLayer.OffsetTarget.All, float softnessOffset = 0f, float dilateOffset = 0f, SignData.GroupSignLayer.ColorMode colorMode = SignData.GroupSignLayer.ColorMode.Multiply)
			: base(name, position, rotation, scale, renderSettings, warps)
		{
			this.layers = layers;
			this.offsetTarget = offsetTarget;
			this.softnessOffset = softnessOffset;
			this.dilateOffset = dilateOffset;
			this.colorMode = colorMode;
		}

		public override SignData.SignLayer Clone()
		{
			List<SignData.SignLayer> list = null;
			if (this.layers != null)
			{
				list = new List<SignData.SignLayer>();
				foreach (SignData.SignLayer signLayer in this.layers)
				{
					list.Add(signLayer.Clone());
				}
			}
			return new SignData.GroupSignLayer(this.name, this.transform.position, this.transform.rotation, this.transform.scale, this.renderSettings, base.CloneWarps(), list, this.offsetTarget, this.softnessOffset, this.dilateOffset, this.colorMode);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write((byte)this.offsetTarget);
			bw.Write(this.softnessOffset);
			bw.Write(this.dilateOffset);
			bw.Write((byte)this.colorMode);
			List<SignData.SignLayer> list = this.layers;
			bw.Write((list != null) ? list.Count : 0);
			if (this.layers != null)
			{
				foreach (SignData.SignLayer signLayer in this.layers)
				{
					signLayer.Write(bw);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.offsetTarget = (SignData.GroupSignLayer.OffsetTarget)br.ReadByte();
			this.softnessOffset = br.ReadSingle();
			this.dilateOffset = br.ReadSingle();
			this.colorMode = (SignData.GroupSignLayer.ColorMode)br.ReadByte();
			int num = br.ReadInt32();
			this.layers = new List<SignData.SignLayer>(num);
			for (int i = 0; i < num; i++)
			{
				this.layers.Add(SignData.SignLayer.Read(br));
			}
		}

		public override void WriteXml(XmlElement element)
		{
			base.WriteXml(element);
			if (this.offsetTarget != SignData.GroupSignLayer.OffsetTarget.All)
			{
				element.SetAttrib("offsetTarget", this.offsetTarget.ToString());
			}
			if (this.softnessOffset != 0f)
			{
				element.SetAttrib("softnessOffset", this.softnessOffset.ToCultureInvariantString());
			}
			if (this.dilateOffset != 0f)
			{
				element.SetAttrib("dilateOffset", this.dilateOffset.ToCultureInvariantString());
			}
			if (this.colorMode != SignData.GroupSignLayer.ColorMode.Multiply)
			{
				element.SetAttrib("colorMode", this.colorMode.ToString());
			}
			foreach (SignData.SignLayer signLayer in this.layers)
			{
				signLayer.WriteXml(element.AddXmlElement("layer"));
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			base.ReadXml(element);
			string text;
			SignData.GroupSignLayer.OffsetTarget offsetTarget;
			if (element.TryGetAttribute("offsetTarget", out text) && Enum.TryParse<SignData.GroupSignLayer.OffsetTarget>(text, out offsetTarget))
			{
				this.offsetTarget = offsetTarget;
			}
			else
			{
				this.offsetTarget = SignData.GroupSignLayer.OffsetTarget.All;
			}
			string text2;
			if (element.TryGetAttribute("softnessOffset", out text2))
			{
				this.softnessOffset = float.Parse(text2, Utils.StandardCulture);
			}
			string text3;
			if (element.TryGetAttribute("dilateOffset", out text3))
			{
				this.dilateOffset = float.Parse(text3, Utils.StandardCulture);
			}
			string text4;
			SignData.GroupSignLayer.ColorMode colorMode;
			if (element.TryGetAttribute("colorMode", out text4) && Enum.TryParse<SignData.GroupSignLayer.ColorMode>(text4, out colorMode))
			{
				this.colorMode = colorMode;
			}
			else
			{
				this.colorMode = SignData.GroupSignLayer.ColorMode.Multiply;
			}
			this.layers = new List<SignData.SignLayer>();
			foreach (XElement xelement in element.Elements("layer"))
			{
				this.layers.Add(SignData.SignLayer.LayerFromXml(xelement));
			}
		}

		public List<SignData.SignLayer> layers;

		public SignData.GroupSignLayer.OffsetTarget offsetTarget;

		public float softnessOffset;

		public float dilateOffset;

		public SignData.GroupSignLayer.ColorMode colorMode;

		public enum OffsetTarget : byte
		{
			All,
			Shapes,
			Text,
			Noise
		}

		public enum ColorMode : byte
		{
			Multiply,
			Blend,
			Override
		}
	}

	public class TextSignLayer : SignData.SignLayer
	{
		public override string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "Text";
			}
		}

		public override SignData.SignLayer.LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignLayer.LayerType.Text;
			}
		}

		public TextSignLayer()
		{
			this.name = "Text";
			this.font = "LiberationSans";
			this.text = Localization.Get("lblSignLayerTypeText", false, null);
			this.direction = 0f;
			this.spacing = 1f;
			this.softness = 0f;
			this.dilate = 0f;
		}

		public TextSignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignData.SignRenderSettings renderSettings, List<SignData.SignWarp> warps, string font, string text, float direction, float spacing, float softness, float dilate)
			: base(name, position, rotation, scale, renderSettings, warps)
		{
			this.font = font;
			this.text = text;
			this.direction = direction;
			this.spacing = spacing;
			this.softness = softness;
			this.dilate = dilate;
		}

		public override SignData.SignLayer Clone()
		{
			return new SignData.TextSignLayer(this.name, this.transform.position, this.transform.rotation, this.transform.scale, this.renderSettings, base.CloneWarps(), this.font, this.text, this.direction, this.spacing, this.softness, this.dilate);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write(this.font);
			bw.Write(this.text);
			bw.Write(this.direction);
			bw.Write(this.spacing);
			bw.Write(this.softness);
			bw.Write(this.dilate);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.font = br.ReadString();
			this.text = br.ReadString();
			this.direction = br.ReadSingle();
			this.spacing = br.ReadSingle();
			this.softness = br.ReadSingle();
			this.dilate = br.ReadSingle();
		}

		public override void WriteXml(XmlElement element)
		{
			base.WriteXml(element);
			element.SetAttrib("font", this.font);
			element.SetAttrib("text", this.text);
			element.SetAttrib("direction", this.direction.ToCultureInvariantString());
			element.SetAttrib("spacing", this.spacing.ToCultureInvariantString());
			element.SetAttrib("softness", this.softness.ToCultureInvariantString());
			element.SetAttrib("dilate", this.dilate.ToCultureInvariantString());
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			base.ReadXml(element);
			this.font = element.GetAttribute("font");
			this.text = element.GetAttribute("text");
			this.direction = float.Parse(element.GetAttribute("direction"), Utils.StandardCulture);
			this.spacing = float.Parse(element.GetAttribute("spacing"), Utils.StandardCulture);
			this.softness = float.Parse(element.GetAttribute("softness"), Utils.StandardCulture);
			this.dilate = float.Parse(element.GetAttribute("dilate"), Utils.StandardCulture);
		}

		public string font;

		public string text;

		public float direction;

		public float spacing;

		public float softness;

		public float dilate;
	}

	public class PolygonSignLayer : SignData.SignLayer
	{
		public override string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "Poly";
			}
		}

		public override SignData.SignLayer.LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignLayer.LayerType.Polygon;
			}
		}

		public PolygonSignLayer()
		{
			this.name = "Polygon";
			this.sides = 4;
			this.smoothness = 0f;
			this.starify = 0f;
			this.softness = 0f;
			this.dilate = 0f;
			this.frequency = 5f;
			this.shapeMode = SignData.PolygonSignLayer.ShapeMode.Normal;
		}

		public PolygonSignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignData.SignRenderSettings renderSettings, List<SignData.SignWarp> warps, int sides, float smoothness, float starify, float softness, float dilate, float frequency, SignData.PolygonSignLayer.ShapeMode shapeMode)
			: base(name, position, rotation, scale, renderSettings, warps)
		{
			this.sides = sides;
			this.smoothness = smoothness;
			this.starify = starify;
			this.softness = softness;
			this.dilate = dilate;
			this.frequency = frequency;
			this.shapeMode = shapeMode;
		}

		public override SignData.SignLayer Clone()
		{
			return new SignData.PolygonSignLayer(this.name, this.transform.position, this.transform.rotation, this.transform.scale, this.renderSettings, base.CloneWarps(), this.sides, this.smoothness, this.starify, this.softness, this.dilate, this.frequency, this.shapeMode);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write(this.sides);
			bw.Write(this.smoothness);
			bw.Write(this.starify);
			bw.Write(this.softness);
			bw.Write(this.dilate);
			bw.Write(this.frequency);
			bw.Write((byte)this.shapeMode);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.sides = br.ReadInt32();
			this.smoothness = br.ReadSingle();
			this.starify = br.ReadSingle();
			this.softness = br.ReadSingle();
			this.dilate = br.ReadSingle();
			this.frequency = br.ReadSingle();
			this.shapeMode = (SignData.PolygonSignLayer.ShapeMode)br.ReadByte();
		}

		public override void WriteXml(XmlElement element)
		{
			base.WriteXml(element);
			element.SetAttrib("sides", this.sides.ToString());
			element.SetAttrib("smoothness", this.smoothness.ToCultureInvariantString());
			element.SetAttrib("starify", this.starify.ToCultureInvariantString());
			element.SetAttrib("softness", this.softness.ToCultureInvariantString());
			element.SetAttrib("dilate", this.dilate.ToCultureInvariantString());
			element.SetAttrib("frequency", this.frequency.ToCultureInvariantString());
			element.SetAttrib("shapeMode", this.shapeMode.ToString());
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			base.ReadXml(element);
			this.sides = int.Parse(element.GetAttribute("sides"));
			this.smoothness = float.Parse(element.GetAttribute("smoothness"), Utils.StandardCulture);
			this.starify = float.Parse(element.GetAttribute("starify"), Utils.StandardCulture);
			string text;
			string text2;
			if (element.TryGetAttribute("softness", out text) && element.TryGetAttribute("dilate", out text2))
			{
				this.softness = float.Parse(text, Utils.StandardCulture);
				this.dilate = float.Parse(text2, Utils.StandardCulture);
			}
			else
			{
				this.softness = 0f;
				this.dilate = 0f;
			}
			string text3;
			if (element.TryGetAttribute("frequency", out text3))
			{
				this.frequency = float.Parse(text3, Utils.StandardCulture);
			}
			else
			{
				this.frequency = 5f;
			}
			string text4;
			SignData.PolygonSignLayer.ShapeMode shapeMode;
			if (element.TryGetAttribute("shapeMode", out text4) && Enum.TryParse<SignData.PolygonSignLayer.ShapeMode>(text4, out shapeMode))
			{
				this.shapeMode = shapeMode;
				return;
			}
			this.shapeMode = SignData.PolygonSignLayer.ShapeMode.Normal;
		}

		public int sides;

		public float smoothness;

		public float starify;

		public float softness;

		public float dilate;

		public float frequency;

		public SignData.PolygonSignLayer.ShapeMode shapeMode;

		public enum ShapeMode : byte
		{
			Normal,
			Invert,
			Line,
			Ripple
		}
	}

	public class NoiseSignLayer : SignData.SignLayer
	{
		public override string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "Noise";
			}
		}

		public override SignData.SignLayer.LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return SignData.SignLayer.LayerType.Noise;
			}
		}

		public NoiseSignLayer()
		{
			this.name = "Noise";
			this.seed = 0;
			this.detail = 4;
			this.softness = 0f;
			this.dilate = 0f;
			this.fade = 0f;
		}

		public NoiseSignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignData.SignRenderSettings renderSettings, List<SignData.SignWarp> warps, int seed, int detail, float softness, float dilate, float fade)
			: base(name, position, rotation, scale, renderSettings, warps)
		{
			this.seed = seed;
			this.detail = detail;
			this.softness = softness;
			this.dilate = dilate;
			this.fade = fade;
		}

		public override SignData.SignLayer Clone()
		{
			return new SignData.NoiseSignLayer(this.name, this.transform.position, this.transform.rotation, this.transform.scale, this.renderSettings, base.CloneWarps(), this.seed, this.detail, this.softness, this.dilate, this.fade);
		}

		public override void WriteXml(XmlElement element)
		{
			base.WriteXml(element);
			element.SetAttrib("seed", this.seed.ToString());
			element.SetAttrib("detail", this.detail.ToString());
			element.SetAttrib("softness", this.softness.ToCultureInvariantString());
			element.SetAttrib("dilate", this.dilate.ToCultureInvariantString());
			element.SetAttrib("fade", this.fade.ToCultureInvariantString());
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			base.ReadXml(element);
			this.seed = int.Parse(element.GetAttribute("seed"));
			this.detail = int.Parse(element.GetAttribute("detail"));
			this.softness = float.Parse(element.GetAttribute("softness"), Utils.StandardCulture);
			this.dilate = float.Parse(element.GetAttribute("dilate"), Utils.StandardCulture);
			string text;
			if (element.TryGetAttribute("fade", out text))
			{
				this.fade = float.Parse(text, Utils.StandardCulture);
				return;
			}
			this.fade = 0f;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write(this.seed);
			bw.Write(this.detail);
			bw.Write(this.softness);
			bw.Write(this.dilate);
			bw.Write(this.fade);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			this.seed = br.ReadInt32();
			this.detail = br.ReadInt32();
			this.softness = br.ReadSingle();
			this.dilate = br.ReadSingle();
			this.fade = br.ReadSingle();
		}

		public int seed;

		public int detail;

		public float softness;

		public float dilate;

		public float fade;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static class DefaultSignData
	{
		public static List<SignData.SignLayer> PolygonGroupLayers
		{
			get
			{
				return new List<SignData.SignLayer>
				{
					new SignData.PolygonSignLayer("Polygon Sign Layer", Vector2.zero + new Vector2(0.005f, -0.005f), 0f, 0.25f * Vector2.one, new SignData.SignRenderSettings(Color.black, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), 5, 0.2f, 0.5f, 0f, 0f, 20f, SignData.PolygonSignLayer.ShapeMode.Normal),
					new SignData.PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 0.25f * Vector2.one, new SignData.SignRenderSettings(Color.white, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), 5, 0.2f, 0.5f, 0f, 0f, 20f, SignData.PolygonSignLayer.ShapeMode.Normal),
					new SignData.PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 0.23f * Vector2.one, new SignData.SignRenderSettings(Color.HSVToRGB(0.05f, 1f, 1f), SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), 5, 0.165f, 0.5f, 0f, 0f, 20f, SignData.PolygonSignLayer.ShapeMode.Normal),
					new SignData.PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 0.18f * Vector2.one, new SignData.SignRenderSettings(Color.HSVToRGB(0.13f, 1f, 1f), SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), 5, 0.15f, 0.5f, 0f, 0f, 20f, SignData.PolygonSignLayer.ShapeMode.Normal)
				};
			}
		}

		public static readonly Color color1 = new Color(0.318f, 0.396f, 0.686f);

		public static readonly Color color2 = new Color(0.286f, 0.518f, 1f);

		public static readonly Color color3 = new Color(0.514f, 0.671f, 1f);

		public static readonly Vector2 textSize = 0.65f * Vector2.one;

		public const float textRotation = 8f;

		public const string font1 = "LiberationSans";

		public const string font2 = "AmaticSC";

		public const string text = "Hello World!";

		public const int polygonSides = 5;

		public const float polygonSmoothness = 0.2f;

		public const float polygonStarify = 0.5f;
	}
}

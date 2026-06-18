using System;
using System.Collections.Generic;
using SharpEXR.AttributeTypes;

namespace SharpEXR
{
	public class EXRHeader
	{
		public Dictionary<string, EXRAttribute> Attributes
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public EXRHeader()
		{
			this.Attributes = new Dictionary<string, EXRAttribute>();
		}

		public void Read(EXRFile file, IEXRReader reader)
		{
			EXRAttribute exrattribute;
			while (EXRAttribute.Read(file, reader, out exrattribute))
			{
				this.Attributes[exrattribute.Name] = exrattribute;
			}
		}

		public bool TryGetAttribute<T>(string name, out T result)
		{
			EXRAttribute exrattribute;
			if (!this.Attributes.TryGetValue(name, out exrattribute))
			{
				result = default(T);
				return false;
			}
			if (exrattribute.Value == null)
			{
				result = default(T);
				return !typeof(T).IsClass && !typeof(T).IsInterface && !typeof(T).IsArray;
			}
			if (typeof(T).IsAssignableFrom(exrattribute.Value.GetType()))
			{
				result = (T)((object)exrattribute.Value);
				return true;
			}
			result = default(T);
			return false;
		}

		public bool IsEmpty
		{
			get
			{
				return this.Attributes.Count == 0;
			}
		}

		public int ChunkCount
		{
			get
			{
				int num;
				if (!this.TryGetAttribute<int>("chunkCount", out num))
				{
					throw new EXRFormatException("Invalid or corrupt EXR header: Missing chunkCount attribute.");
				}
				return num;
			}
		}

		public Box2I DataWindow
		{
			get
			{
				Box2I box2I;
				if (!this.TryGetAttribute<Box2I>("dataWindow", out box2I))
				{
					throw new EXRFormatException("Invalid or corrupt EXR header: Missing dataWindow attribute.");
				}
				return box2I;
			}
		}

		public EXRCompression Compression
		{
			get
			{
				EXRCompression exrcompression;
				if (!this.TryGetAttribute<EXRCompression>("compression", out exrcompression))
				{
					throw new EXRFormatException("Invalid or corrupt EXR header: Missing compression attribute.");
				}
				return exrcompression;
			}
		}

		public PartType Type
		{
			get
			{
				PartType partType;
				if (!this.TryGetAttribute<PartType>("type", out partType))
				{
					throw new EXRFormatException("Invalid or corrupt EXR header: Missing type attribute.");
				}
				return partType;
			}
		}

		public ChannelList Channels
		{
			get
			{
				ChannelList channelList;
				if (!this.TryGetAttribute<ChannelList>("channels", out channelList))
				{
					throw new EXRFormatException("Invalid or corrupt EXR header: Missing channels attribute.");
				}
				return channelList;
			}
		}

		public Chromaticities Chromaticities
		{
			get
			{
				foreach (EXRAttribute exrattribute in this.Attributes.Values)
				{
					if (exrattribute.Type == "chromaticities" && exrattribute.Value is Chromaticities)
					{
						return (Chromaticities)exrattribute.Value;
					}
				}
				return EXRHeader.DefaultChromaticities;
			}
		}

		public static readonly Chromaticities DefaultChromaticities = new Chromaticities(0.64f, 0.33f, 0.3f, 0.6f, 0.15f, 0.06f, 0.3127f, 0.329f);
	}
}

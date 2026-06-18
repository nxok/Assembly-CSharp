using System;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
	[CreateAssetMenu(menuName = "MicroSplat/Texture Array Config", order = 1)]
	[ExecuteInEditMode]
	public class TextureArrayConfig : ScriptableObject
	{
		public bool IsScatter()
		{
			return false;
		}

		public bool IsDecal()
		{
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Awake()
		{
			TextureArrayConfig.sAllConfigs.Add(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDestroy()
		{
			TextureArrayConfig.sAllConfigs.Remove(this);
		}

		public static TextureArrayConfig FindConfig(Texture2DArray diffuse)
		{
			for (int i = 0; i < TextureArrayConfig.sAllConfigs.Count; i++)
			{
				if (TextureArrayConfig.sAllConfigs[i].diffuseArray == diffuse)
				{
					return TextureArrayConfig.sAllConfigs[i];
				}
			}
			return null;
		}

		public bool diffuseIsLinear;

		[HideInInspector]
		public bool antiTileArray;

		[HideInInspector]
		public bool emisMetalArray;

		public bool traxArray;

		[HideInInspector]
		public TextureArrayConfig.TextureMode textureMode = TextureArrayConfig.TextureMode.PBR;

		[HideInInspector]
		public TextureArrayConfig.ClusterMode clusterMode;

		[HideInInspector]
		public TextureArrayConfig.PackingMode packingMode;

		[HideInInspector]
		public TextureArrayConfig.PBRWorkflow pbrWorkflow;

		[HideInInspector]
		public int hash;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public static List<TextureArrayConfig> sAllConfigs = new List<TextureArrayConfig>();

		[HideInInspector]
		public Texture2DArray splatArray;

		[HideInInspector]
		public Texture2DArray diffuseArray;

		[HideInInspector]
		public Texture2DArray normalSAOArray;

		[HideInInspector]
		public Texture2DArray smoothAOArray;

		[HideInInspector]
		public Texture2DArray specularArray;

		[HideInInspector]
		public Texture2DArray diffuseArray2;

		[HideInInspector]
		public Texture2DArray normalSAOArray2;

		[HideInInspector]
		public Texture2DArray smoothAOArray2;

		[HideInInspector]
		public Texture2DArray specularArray2;

		[HideInInspector]
		public Texture2DArray diffuseArray3;

		[HideInInspector]
		public Texture2DArray normalSAOArray3;

		[HideInInspector]
		public Texture2DArray smoothAOArray3;

		[HideInInspector]
		public Texture2DArray specularArray3;

		[HideInInspector]
		public Texture2DArray emisArray;

		[HideInInspector]
		public Texture2DArray emisArray2;

		[HideInInspector]
		public Texture2DArray emisArray3;

		public TextureArrayConfig.TextureArrayGroup defaultTextureSettings = new TextureArrayConfig.TextureArrayGroup();

		public List<TextureArrayConfig.PlatformTextureOverride> platformOverrides = new List<TextureArrayConfig.PlatformTextureOverride>();

		public TextureArrayConfig.SourceTextureSize sourceTextureSize;

		[HideInInspector]
		public TextureArrayConfig.AllTextureChannel allTextureChannelHeight = TextureArrayConfig.AllTextureChannel.G;

		[HideInInspector]
		public TextureArrayConfig.AllTextureChannel allTextureChannelSmoothness = TextureArrayConfig.AllTextureChannel.G;

		[HideInInspector]
		public TextureArrayConfig.AllTextureChannel allTextureChannelAO = TextureArrayConfig.AllTextureChannel.G;

		[HideInInspector]
		public List<TextureArrayConfig.TextureEntry> sourceTextures = new List<TextureArrayConfig.TextureEntry>();

		[HideInInspector]
		public List<TextureArrayConfig.TextureEntry> sourceTextures2 = new List<TextureArrayConfig.TextureEntry>();

		[HideInInspector]
		public List<TextureArrayConfig.TextureEntry> sourceTextures3 = new List<TextureArrayConfig.TextureEntry>();

		public enum AllTextureChannel
		{
			R,
			G,
			B,
			A,
			Custom
		}

		public enum TextureChannel
		{
			R,
			G,
			B,
			A
		}

		public enum Compression
		{
			AutomaticCompressed,
			ForceDXT,
			ForcePVR,
			ForceETC2,
			ForceASTC,
			ForceCrunch,
			Uncompressed
		}

		public enum TextureSize
		{
			k4096 = 4096,
			k2048 = 2048,
			k1024 = 1024,
			k512 = 512,
			k256 = 256,
			k128 = 128,
			k64 = 64,
			k32 = 32
		}

		[Serializable]
		public class TextureArraySettings
		{
			public TextureArraySettings(TextureArrayConfig.TextureSize s, TextureArrayConfig.Compression c, FilterMode f, int a = 1)
			{
				this.textureSize = s;
				this.compression = c;
				this.filterMode = f;
				this.Aniso = a;
			}

			public TextureArrayConfig.TextureSize textureSize;

			public TextureArrayConfig.Compression compression;

			public FilterMode filterMode;

			[Range(0f, 16f)]
			public int Aniso = 1;
		}

		public enum PBRWorkflow
		{
			Metallic,
			Specular
		}

		public enum PackingMode
		{
			Fastest,
			Quality
		}

		public enum SourceTextureSize
		{
			Unchanged,
			k32 = 32,
			k256 = 256
		}

		public enum TextureMode
		{
			Basic,
			PBR
		}

		public enum ClusterMode
		{
			None,
			TwoVariations,
			ThreeVariations
		}

		[Serializable]
		public class TextureArrayGroup
		{
			public TextureArrayConfig.TextureArraySettings diffuseSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 1, 1);

			public TextureArrayConfig.TextureArraySettings normalSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 2, 1);

			public TextureArrayConfig.TextureArraySettings smoothSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 1, 1);

			public TextureArrayConfig.TextureArraySettings antiTileSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 1, 1);

			public TextureArrayConfig.TextureArraySettings emissiveSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 1, 1);

			public TextureArrayConfig.TextureArraySettings specularSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 1, 1);

			public TextureArrayConfig.TextureArraySettings traxDiffuseSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 1, 1);

			public TextureArrayConfig.TextureArraySettings traxNormalSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 1, 1);

			public TextureArrayConfig.TextureArraySettings decalSplatSettings = new TextureArrayConfig.TextureArraySettings(TextureArrayConfig.TextureSize.k1024, TextureArrayConfig.Compression.AutomaticCompressed, 1, 1);
		}

		[Serializable]
		public class PlatformTextureOverride
		{
			public TextureArrayConfig.TextureArrayGroup settings = new TextureArrayConfig.TextureArrayGroup();
		}

		[Serializable]
		public class TextureEntry
		{
			public void Reset()
			{
				this.diffuse = null;
				this.height = null;
				this.normal = null;
				this.smoothness = null;
				this.specular = null;
				this.ao = null;
				this.isRoughness = false;
				this.detailNoise = null;
				this.distanceNoise = null;
				this.metal = null;
				this.emis = null;
				this.heightChannel = TextureArrayConfig.TextureChannel.G;
				this.smoothnessChannel = TextureArrayConfig.TextureChannel.G;
				this.aoChannel = TextureArrayConfig.TextureChannel.G;
				this.distanceChannel = TextureArrayConfig.TextureChannel.G;
				this.detailChannel = TextureArrayConfig.TextureChannel.G;
				this.traxDiffuse = null;
				this.traxNormal = null;
				this.traxHeight = null;
				this.traxSmoothness = null;
				this.traxAO = null;
				this.traxHeightChannel = TextureArrayConfig.TextureChannel.G;
				this.traxSmoothnessChannel = TextureArrayConfig.TextureChannel.G;
				this.traxAOChannel = TextureArrayConfig.TextureChannel.G;
				this.splat = null;
			}

			public bool HasTextures(TextureArrayConfig.PBRWorkflow wf)
			{
				if (wf == TextureArrayConfig.PBRWorkflow.Specular)
				{
					return this.diffuse != null || this.height != null || this.normal != null || this.smoothness != null || this.specular != null || this.ao != null;
				}
				return this.diffuse != null || this.height != null || this.normal != null || this.smoothness != null || this.metal != null || this.ao != null;
			}

			public Texture2D diffuse;

			public Texture2D height;

			public TextureArrayConfig.TextureChannel heightChannel = TextureArrayConfig.TextureChannel.G;

			public Texture2D normal;

			public Texture2D smoothness;

			public TextureArrayConfig.TextureChannel smoothnessChannel = TextureArrayConfig.TextureChannel.G;

			public bool isRoughness;

			public Texture2D ao;

			public TextureArrayConfig.TextureChannel aoChannel = TextureArrayConfig.TextureChannel.G;

			public Texture2D emis;

			public Texture2D metal;

			public TextureArrayConfig.TextureChannel metalChannel = TextureArrayConfig.TextureChannel.G;

			public Texture2D specular;

			public Texture2D noiseNormal;

			public Texture2D detailNoise;

			public TextureArrayConfig.TextureChannel detailChannel = TextureArrayConfig.TextureChannel.G;

			public Texture2D distanceNoise;

			public TextureArrayConfig.TextureChannel distanceChannel = TextureArrayConfig.TextureChannel.G;

			public Texture2D traxDiffuse;

			public Texture2D traxHeight;

			public TextureArrayConfig.TextureChannel traxHeightChannel = TextureArrayConfig.TextureChannel.G;

			public Texture2D traxNormal;

			public Texture2D traxSmoothness;

			public TextureArrayConfig.TextureChannel traxSmoothnessChannel = TextureArrayConfig.TextureChannel.G;

			public bool traxIsRoughness;

			public Texture2D traxAO;

			public TextureArrayConfig.TextureChannel traxAOChannel = TextureArrayConfig.TextureChannel.G;

			public Texture2D splat;
		}
	}
}

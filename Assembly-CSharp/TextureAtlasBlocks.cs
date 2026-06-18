using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TextureAtlasBlocks : TextureAtlas
{
	public override bool LoadTextureAtlas(int _idx, MeshDescriptionCollection _tac, bool _bLoadTextures)
	{
		try
		{
			this.LoadTextureAtlasFromMetadata(_idx, _tac);
		}
		catch (Exception ex)
		{
			Log.Error(string.Concat(new string[]
			{
				"Parsing file xml file for texture atlas ",
				_tac.name,
				" (",
				_idx.ToString(),
				"): ",
				ex.Message,
				")"
			}));
			Log.Exception(ex);
			Log.Error("Loading of textures aborted due to errors!");
			return false;
		}
		base.LoadTextureAtlas(_idx, _tac, _bLoadTextures);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadTextureAtlasFromMetadata(int _idx, MeshDescriptionCollection _tac)
	{
		XElement root = new XmlFile(_tac.meshes[_idx].MetaData).XmlDoc.Root;
		int num = 0;
		foreach (XElement xelement in root.Elements("uv"))
		{
			num = Math.Max(num, int.Parse(xelement.GetAttribute("id")));
		}
		this.uvMapping = new UVRectTiling[num + 1];
		foreach (XElement xelement2 in root.Elements("uv"))
		{
			int num2 = int.Parse(xelement2.GetAttribute("id"));
			UVRectTiling uvrectTiling = default(UVRectTiling);
			uvrectTiling.FromXML(xelement2);
			this.uvMapping[num2] = uvrectTiling;
		}
	}

	public override void Cleanup()
	{
		if (this.diffuseTexture != null)
		{
			Resources.UnloadAsset(this.diffuseTexture);
			this.diffuseTexture = null;
		}
		if (this.normalTexture != null)
		{
			Resources.UnloadAsset(this.normalTexture);
			this.normalTexture = null;
		}
		if (this.maskTexture != null)
		{
			Resources.UnloadAsset(this.maskTexture);
			this.maskTexture = null;
		}
		if (this.maskNormalTexture != null)
		{
			Resources.UnloadAsset(this.maskNormalTexture);
			this.maskNormalTexture = null;
		}
		if (this.emissionTexture != null)
		{
			Resources.UnloadAsset(this.emissionTexture);
			this.emissionTexture = null;
		}
		if (this.specularTexture != null)
		{
			Resources.UnloadAsset(this.specularTexture);
			this.specularTexture = null;
		}
		if (this.heightTexture != null)
		{
			Resources.UnloadAsset(this.heightTexture);
			this.heightTexture = null;
		}
		if (this.occlusionTexture != null)
		{
			Resources.UnloadAsset(this.occlusionTexture);
			this.occlusionTexture = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTextureArraySize = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int cTextureBorder = 34;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int cTextureSize = 8192;

	public enum WrapMode
	{
		Mirror,
		Wrap,
		TransparentEdges
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BlockUVRect
	{
		public BlockUVRect(int _textureId, int _x, int _y, int _width, int _height, int _blocksW, int _blocksH, Color _color, bool _bGlobalUV)
		{
			this.textureId = _textureId;
			this.x = _x;
			this.y = _y;
			this.width = _width;
			this.height = _height;
			this.blocksW = _blocksW;
			this.blocksH = _blocksH;
			this.color = _color;
			this.bGlobalUV = _bGlobalUV;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"texId=",
				this.textureId.ToString(),
				" x/y=",
				this.x.ToString(),
				"/",
				this.y.ToString(),
				" w/h=",
				this.width.ToString(),
				"/",
				this.height.ToString(),
				" block W/H=",
				this.blocksW.ToString(),
				"/",
				this.blocksH.ToString()
			});
		}

		public int textureId;

		public int x;

		public int y;

		public int width;

		public int height;

		public int blocksW;

		public int blocksH;

		public Color color;

		public bool bGlobalUV;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class BlocksTexture
	{
		public override string ToString()
		{
			string[] array = new string[7];
			array[0] = "diffuse=";
			int num = 1;
			Texture2D texture2D = this.diffuse;
			array[num] = ((texture2D != null) ? texture2D.ToString() : null);
			array[2] = " normal=";
			int num2 = 3;
			Texture2D texture2D2 = this.normal;
			array[num2] = ((texture2D2 != null) ? texture2D2.ToString() : null);
			array[4] = " count=";
			array[5] = this.blocks.Count.ToString();
			array[6] = "\n";
			string text = string.Concat(array);
			for (int i = 0; i < this.blocks.Count; i++)
			{
				TextureAtlasBlocks.BlockUVRect blockUVRect = this.blocks[i];
				string text2 = text;
				TextureAtlasBlocks.BlockUVRect blockUVRect2 = blockUVRect;
				text = text2 + blockUVRect2.ToString() + "\n";
			}
			return text;
		}

		public List<TextureAtlasBlocks.BlockUVRect> blocks = new List<TextureAtlasBlocks.BlockUVRect>();

		public string textureName;

		public Texture2D diffuse;

		public Texture2D normal;

		public Texture2D specular;

		public Texture2D height;

		public TextureAtlasBlocks.WrapMode wrapMode;

		public int diffuseW;

		public int diffuseH;

		public Color color;
	}
}

using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TextureAtlasTerrain : TextureAtlasBlocks
{
	public TextureAtlasTerrain()
	{
		this.bDestroyTextures = false;
	}

	public override bool LoadTextureAtlas(int _idx, MeshDescriptionCollection _tac, bool _bLoadTextures)
	{
		if (base.LoadTextureAtlas(_idx, _tac, _bLoadTextures))
		{
			this.diffuse = new Texture2D[this.uvMapping.Length];
			this.normal = new Texture2D[this.uvMapping.Length];
			this.specular = new Texture2D[this.uvMapping.Length];
			if (_bLoadTextures)
			{
				for (int i = 0; i < this.uvMapping.Length; i++)
				{
					if (this.uvMapping[i].textureName != null)
					{
						string text = GameIO.RemoveFileExtension(this.uvMapping[i].textureName);
						string fileExtension = GameIO.GetFileExtension(this.uvMapping[i].textureName);
						Texture2D asset = LoadManager.LoadAssetFromAddressables<Texture2D>("TerrainTextures", this.uvMapping[i].textureName, null, null, false, true, false).Asset;
						if (asset == null)
						{
							throw new Exception("TextureAtlasTerrain: couldn't load diffuse texture '" + this.uvMapping[i].textureName + "'");
						}
						Texture2D asset2 = LoadManager.LoadAssetFromAddressables<Texture2D>("TerrainTextures", text + "_n" + fileExtension, null, null, false, true, false).Asset;
						if (asset2 == null)
						{
							throw new Exception(string.Concat(new string[] { "TextureAtlasTerrain: couldn't load normal texture '", text, "_n", fileExtension, "'" }));
						}
						Texture2D asset3 = LoadManager.LoadAssetFromAddressables<Texture2D>("TerrainTextures", text + "_s" + fileExtension, null, null, false, true, false).Asset;
						if (!Application.isEditor)
						{
							if (asset != null && asset.isReadable)
							{
								asset.Apply(false, true);
							}
							if (asset2 != null && asset2.isReadable)
							{
								asset2.Apply(false, true);
							}
							if (asset3 != null && asset3.isReadable)
							{
								asset3.Apply(false, true);
							}
						}
						this.diffuse[i] = asset;
						this.normal[i] = asset2;
						this.specular[i] = asset3;
					}
				}
			}
		}
		return true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		for (int i = 0; i < this.diffuse.Length; i++)
		{
			if (this.diffuse[i] != null)
			{
				LoadManager.ReleaseAddressable<Texture2D>(this.diffuse[i]);
			}
		}
		for (int j = 0; j < this.normal.Length; j++)
		{
			if (this.normal[j] != null)
			{
				LoadManager.ReleaseAddressable<Texture2D>(this.normal[j]);
			}
		}
		for (int k = 0; k < this.specular.Length; k++)
		{
			if (this.specular[k] != null)
			{
				LoadManager.ReleaseAddressable<Texture2D>(this.specular[k]);
			}
		}
	}

	public Texture2D[] diffuse;

	public Texture2D[] normal;

	public Texture2D[] specular;
}

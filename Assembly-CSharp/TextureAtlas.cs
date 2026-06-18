using System;
using UnityEngine;

public class TextureAtlas
{
	public TextureAtlas()
		: this(true)
	{
	}

	public TextureAtlas(bool _bDestroyTextures)
	{
		this.bDestroyTextures = _bDestroyTextures;
	}

	public virtual bool LoadTextureAtlas(int _idx, MeshDescriptionCollection _tac, bool _bLoadTextures)
	{
		if (_bLoadTextures)
		{
			MeshDescription meshDescription = _tac.Meshes[_idx];
			this.diffuseTexture = meshDescription.TexDiffuse;
			this.normalTexture = meshDescription.TexNormal;
			this.specularTexture = meshDescription.TexSpecular;
			this.emissionTexture = meshDescription.TexEmission;
			this.heightTexture = meshDescription.TexHeight;
			this.occlusionTexture = meshDescription.TexOcclusion;
			this.maskTexture = meshDescription.TexMask;
			this.maskNormalTexture = meshDescription.TexMaskNormal;
		}
		return true;
	}

	public virtual void Cleanup()
	{
	}

	public Texture diffuseTexture;

	public Texture normalTexture;

	public Texture specularTexture;

	public Texture emissionTexture;

	public Texture heightTexture;

	public Texture occlusionTexture;

	public Texture2D maskTexture;

	public Texture2D maskNormalTexture;

	public UVRectTiling[] uvMapping = new UVRectTiling[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDestroyTextures;
}

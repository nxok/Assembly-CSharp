using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

public class DynamicUIAtlas : UIAtlas
{
	public event Action AtlasUpdatedEv;

	public void Awake()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		if (this.PrebakedAtlas.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
		{
			this.PrebakedAtlas = this.PrebakedAtlas.Substring(0, this.PrebakedAtlas.Length - 4);
		}
		if (!DynamicUIAtlasTools.ReadPrebakedAtlasDescriptor(this.PrebakedAtlas, out this.origSpriteData, out this.elementWidth, out this.elementHeight, out this.paddingSize))
		{
			Object.Destroy(this);
			return;
		}
		base.spriteMaterial = new Material(this.shader);
		base.spriteList = new List<UISpriteData>();
		this.ResetAtlas();
		base.pixelSize = 1f;
		stopwatch.Stop();
		Log.Out("Atlas load took " + stopwatch.ElapsedMilliseconds.ToString() + " ms");
		if (this.AtlasUpdatedEv != null)
		{
			this.AtlasUpdatedEv();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadBaseTexture()
	{
		Texture2D texture2D;
		if (!DynamicUIAtlasTools.ReadPrebakedAtlasTexture(this.PrebakedAtlas, out texture2D))
		{
			Object.Destroy(this);
			return;
		}
		this.currentTex = new Texture2D(texture2D.width, texture2D.height, texture2D.format, texture2D.mipmapCount > 1);
		NativeArray<byte> rawTextureData = texture2D.GetRawTextureData<byte>();
		NativeArray<byte> rawTextureData2 = this.currentTex.GetRawTextureData<byte>();
		rawTextureData.CopyTo(rawTextureData2);
		DynamicUIAtlasTools.UnloadTex(this.PrebakedAtlas, texture2D);
	}

	public void LoadAdditionalSprites(Dictionary<string, Texture2D> _nameToTex)
	{
		DynamicUIAtlasTools.AddSprites(this.elementWidth, this.elementHeight, this.paddingSize, _nameToTex, ref this.currentTex, base.spriteList);
		base.spriteMaterial.mainTexture = this.currentTex;
		this.currentTex.Apply();
		if (this.AtlasUpdatedEv != null)
		{
			this.AtlasUpdatedEv();
		}
	}

	public void ResetAtlas()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		if (this.currentTex != null)
		{
			Object.Destroy(this.currentTex);
		}
		base.spriteList.Clear();
		this.LoadBaseTexture();
		base.spriteMaterial.mainTexture = this.currentTex;
		this.currentTex.Apply();
		base.spriteList.AddRange(this.origSpriteData);
		stopwatch.Stop();
		Log.Out("Atlas reset took " + stopwatch.ElapsedMilliseconds.ToString() + " ms");
		if (this.AtlasUpdatedEv != null)
		{
			this.AtlasUpdatedEv();
		}
	}

	public void Compress()
	{
		this.currentTex.Compress(true);
		this.currentTex.Apply(false, true);
	}

	public static DynamicUIAtlas Create(GameObject _parent, string _prebakedAtlasResourceName, Shader _shader)
	{
		string text = _prebakedAtlasResourceName;
		int num;
		if ((num = _prebakedAtlasResourceName.IndexOf('?')) >= 0)
		{
			text = text.Substring(num + 1);
		}
		GameObject gameObject = new GameObject(text);
		gameObject.transform.parent = _parent.transform;
		gameObject.SetActive(false);
		DynamicUIAtlas dynamicUIAtlas = gameObject.AddComponent<DynamicUIAtlas>();
		dynamicUIAtlas.PrebakedAtlas = _prebakedAtlasResourceName;
		dynamicUIAtlas.shader = _shader;
		gameObject.SetActive(true);
		return dynamicUIAtlas;
	}

	public Shader shader;

	public string PrebakedAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int elementWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int elementHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int paddingSize;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<UISpriteData> origSpriteData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D currentTex;
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureLoadingManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		TextureLoadingManager.Instance = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Time.time - this.lastTimeChecked < 1f)
		{
			return;
		}
		this.lastTimeChecked = Time.time;
		for (int i = this.runningRequests.Count - 1; i >= 0; i--)
		{
			TextureLoadingManager.AsyncLoadInfo asyncLoadInfo = this.runningRequests[i];
			TextureLoadingManager.TextureInfo textureInfo;
			if (asyncLoadInfo.resRequest.isDone && this.availableTextures.TryGetValue(asyncLoadInfo.fullPath, out textureInfo))
			{
				textureInfo.tex = (Texture)asyncLoadInfo.resRequest.asset;
				textureInfo.bPending = false;
				if (asyncLoadInfo.material)
				{
					asyncLoadInfo.material.SetTexture(asyncLoadInfo.propName, textureInfo.tex);
				}
				this.runningRequests.RemoveAt(i);
			}
		}
	}

	public void Cleanup()
	{
		this.availableTextures.Clear();
		this.runningRequests.Clear();
	}

	public void LoadTexture(Material _m, string _propName, string _assetPath, string _texName, Texture _lowResTexture)
	{
		if (!Application.isPlaying)
		{
			Texture texture = Resources.Load<Texture2D>(_assetPath + _texName);
			_m.SetTexture(_propName, texture);
			return;
		}
		TextureLoadingManager.TextureInfo textureInfo = null;
		string text = _assetPath + _texName;
		if (this.availableTextures.TryGetValue(text, out textureInfo) && !textureInfo.bPending)
		{
			_m.SetTexture(_propName, textureInfo.tex);
			textureInfo.refCounts++;
			return;
		}
		ResourceRequest resourceRequest = Resources.LoadAsync<Texture2D>(text);
		TextureLoadingManager.AsyncLoadInfo asyncLoadInfo = default(TextureLoadingManager.AsyncLoadInfo);
		asyncLoadInfo.resRequest = resourceRequest;
		asyncLoadInfo.propName = _propName;
		asyncLoadInfo.material = _m;
		asyncLoadInfo.fullPath = text;
		asyncLoadInfo.lowResTexture = _lowResTexture;
		this.runningRequests.Add(asyncLoadInfo);
		if (textureInfo == null)
		{
			textureInfo = new TextureLoadingManager.TextureInfo();
			textureInfo.bPending = true;
			textureInfo.refCounts = 1;
			this.availableTextures.Add(text, textureInfo);
			return;
		}
		textureInfo.refCounts++;
	}

	public bool UnloadTexture(string _assetPath, string _texName)
	{
		string text = _assetPath + _texName;
		TextureLoadingManager.TextureInfo textureInfo;
		if (this.availableTextures.TryGetValue(text, out textureInfo))
		{
			textureInfo.refCounts--;
			if (textureInfo.refCounts == 0)
			{
				this.availableTextures.Remove(text);
				Resources.UnloadAsset(textureInfo.tex);
				return true;
			}
		}
		return false;
	}

	public int GetHiResTextureCount()
	{
		return this.availableTextures.Count;
	}

	public static TextureLoadingManager Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, TextureLoadingManager.TextureInfo> availableTextures = new Dictionary<string, TextureLoadingManager.TextureInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<TextureLoadingManager.AsyncLoadInfo> runningRequests = new List<TextureLoadingManager.AsyncLoadInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct AsyncLoadInfo
	{
		public ResourceRequest resRequest;

		public string propName;

		public Material material;

		public string fullPath;

		public Texture lowResTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class TextureInfo
	{
		public bool bPending;

		public int refCounts;

		public Texture tex;
	}
}

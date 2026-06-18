using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiSourceAtlasManager : MonoBehaviour
{
	public INGUIAtlas GetAtlasForSprite(string _spriteName)
	{
		MultiSourceAtlasManager.BaseAtlas baseAtlas;
		if (this.atlasesForSprites.TryGetValue(_spriteName, out baseAtlas))
		{
			return baseAtlas.Atlas;
		}
		if (this.atlases.Count <= 0)
		{
			return null;
		}
		return this.atlases[0].Atlas;
	}

	public void AddAtlas(INGUIAtlas _atlas, GameObject _gameObject, bool _isLoadingInGame)
	{
		MultiSourceAtlasManager.BaseAtlas baseAtlas = new MultiSourceAtlasManager.BaseAtlas
		{
			Parent = _gameObject,
			Atlas = _atlas,
			IsLoadedInGame = _isLoadingInGame
		};
		this.atlases.Add(baseAtlas);
		_atlas.Name = base.name;
		this.recalcSpriteSources();
	}

	public void CleanupAfterGame()
	{
		for (int i = this.atlases.Count - 1; i >= 0; i--)
		{
			MultiSourceAtlasManager.BaseAtlas baseAtlas = this.atlases[i];
			if (baseAtlas.IsLoadedInGame)
			{
				this.atlases.RemoveAt(i);
				Object.Destroy(baseAtlas.Atlas.spriteMaterial.mainTexture);
				if (baseAtlas.Parent != null)
				{
					Object.Destroy(baseAtlas.Parent);
				}
			}
		}
		this.recalcSpriteSources();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recalcSpriteSources()
	{
		foreach (MultiSourceAtlasManager.BaseAtlas baseAtlas in this.atlases)
		{
			foreach (UISpriteData uispriteData in baseAtlas.Atlas.spriteList)
			{
				this.atlasesForSprites[uispriteData.name] = baseAtlas;
			}
		}
	}

	public static MultiSourceAtlasManager Create(GameObject _parent, string _name)
	{
		return new GameObject(_name)
		{
			transform = 
			{
				parent = _parent.transform
			}
		}.AddComponent<MultiSourceAtlasManager>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<MultiSourceAtlasManager.BaseAtlas> atlases = new List<MultiSourceAtlasManager.BaseAtlas>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly Dictionary<string, MultiSourceAtlasManager.BaseAtlas> atlasesForSprites = new Dictionary<string, MultiSourceAtlasManager.BaseAtlas>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class BaseAtlas
	{
		public GameObject Parent;

		public INGUIAtlas Atlas;

		public bool IsLoadedInGame;
	}
}

using System;
using UnityEngine;

public class DynamicUIAtlasAssigner : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		GameObject gameObject = GameObject.Find(this.AtlasPathInScene);
		if (gameObject == null)
		{
			Log.Warning("Could not assign atlas: Atlas object not found");
			Object.Destroy(this);
			return;
		}
		this.atlas = gameObject.GetComponent<DynamicUIAtlas>();
		if (this.atlas == null)
		{
			Log.Warning("Could not assign atlas: Atlas component not found");
			Object.Destroy(this);
			return;
		}
		this.atlas.AtlasUpdatedEv += this.AtlasUpdateCallback;
		this.sprites = base.GetComponents<UISprite>();
		foreach (UISprite uisprite in this.sprites)
		{
			uisprite.atlas = this.atlas;
			if (!string.IsNullOrEmpty(this.OptionalSpriteName))
			{
				uisprite.spriteName = this.OptionalSpriteName;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDestroy()
	{
		if (this.atlas != null)
		{
			this.atlas.AtlasUpdatedEv -= this.AtlasUpdateCallback;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AtlasUpdateCallback()
	{
		if (!string.IsNullOrEmpty(this.OptionalSpriteName))
		{
			foreach (UISprite uisprite in this.sprites)
			{
				uisprite.spriteName = null;
				uisprite.spriteName = this.OptionalSpriteName;
			}
		}
	}

	public string AtlasPathInScene;

	public string OptionalSpriteName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DynamicUIAtlas atlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UISprite[] sprites;
}

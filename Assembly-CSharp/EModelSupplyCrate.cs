using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelSupplyCrate : EModelBase
{
	public override void Init(World _world, Entity _entity, EModelInstanceAssets _assets)
	{
		base.Init(_world, _entity, _assets);
		this.parachute = base.transform.FindInChilds("parachute_supplies", false);
	}

	public override void SetSkinTexture(string _texture)
	{
		base.SetSkinTexture(_texture);
		if (_texture == null || _texture.Length == 0)
		{
			return;
		}
		for (int i = 0; i < this.modelTransformParent.childCount; i++)
		{
			Transform child = this.modelTransformParent.GetChild(i);
			if (child != this.parachute)
			{
				child.GetComponent<Renderer>().material.mainTexture = DataLoader.LoadAsset<Texture2D>(DataLoader.IsInResources(_texture) ? ("Entities/" + _texture) : _texture, false);
				return;
			}
		}
	}

	public Transform parachute;
}

using System;
using UnityEngine;

public class EntityInstanceAssets
{
	public Transform PrefabT
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsLoadComplete
	{
		get
		{
			return this.prefabHandle.IsDone;
		}
	}

	public bool IsLoadSuccessful
	{
		get
		{
			return this.IsLoadComplete && !(this.PrefabT == null);
		}
	}

	public void Load(bool _loadSync, EntityClass _ec, bool isLocalPlayer)
	{
		if (isLocalPlayer)
		{
			this.prefabHandle = LoadManager.LoadAsset<GameObject>("Prefabs/prefabEntityPlayerLocal", null, null, false, true, false);
			this.PrefabT = this.prefabHandle.Asset.transform;
			return;
		}
		this.prefabHandle = LoadManager.LoadAsset<GameObject>(_ec.prefabPath, new Action<GameObject>(this.OnPrefabLoaded), null, false, _loadSync, false);
	}

	public void OnPrefabLoaded(GameObject prefab)
	{
		this.PrefabT = prefab.transform;
	}

	public void WaitForComplete()
	{
		if (!this.prefabHandle.IsDone)
		{
			this.prefabHandle.WaitForComplete();
		}
	}

	public void Release()
	{
		LoadManager.AssetRequestTask<GameObject> assetRequestTask = this.prefabHandle;
		if (assetRequestTask == null)
		{
			return;
		}
		assetRequestTask.Release();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public LoadManager.AssetRequestTask<GameObject> prefabHandle;
}

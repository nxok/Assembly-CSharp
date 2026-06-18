using System;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseAssetsOnDestroy : MonoBehaviour
{
	public void AddAssetHandle(LoadManager.IAssetHandle handle)
	{
		this.assetHandles.Add(handle);
	}

	public void CopyTo(ReleaseAssetsOnDestroy other)
	{
		foreach (LoadManager.IAssetHandle assetHandle in other.assetHandles)
		{
			this.assetHandles.Add(assetHandle.Copy());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		foreach (LoadManager.IAssetHandle assetHandle in this.assetHandles)
		{
			assetHandle.Release();
		}
		this.assetHandles.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<LoadManager.IAssetHandle> assetHandles = new List<LoadManager.IAssetHandle>();
}

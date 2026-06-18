using System;
using System.Collections.Generic;
using PrefabVolumes;

public class PrefabInstanceClientManager
{
	public static PrefabInstanceClientManager Instance
	{
		get
		{
			PrefabInstanceClientManager prefabInstanceClientManager;
			if ((prefabInstanceClientManager = PrefabInstanceClientManager.instance) == null)
			{
				prefabInstanceClientManager = (PrefabInstanceClientManager.instance = new PrefabInstanceClientManager());
			}
			return prefabInstanceClientManager;
		}
	}

	public void Cleanup()
	{
		this.clientPrefabs.Clear();
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			dynamicPrefabDecorator.OnPrefabLoaded -= this.prefabLoadedServer;
			dynamicPrefabDecorator.OnPrefabChanged -= this.prefabChangedServer;
			dynamicPrefabDecorator.OnPrefabRemoved -= this.prefabRemovedServer;
		}
		PrefabEditModeManager.Instance.OnPrefabChanged -= this.prefabChangedServer;
		GameManager.Instance.OnClientSpawned -= this.sendAllPrefabs;
	}

	public void StartAsServer()
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		dynamicPrefabDecorator.OnPrefabLoaded += this.prefabLoadedServer;
		dynamicPrefabDecorator.OnPrefabChanged += this.prefabChangedServer;
		dynamicPrefabDecorator.OnPrefabRemoved += this.prefabRemovedServer;
		PrefabEditModeManager.Instance.OnPrefabChanged += this.prefabChangedServer;
		GameManager.Instance.OnClientSpawned += this.sendAllPrefabs;
	}

	public void StartAsClient()
	{
		this.clientPrefabs.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sendAllPrefabs(ClientInfo _toClient)
	{
		if (_toClient != null)
		{
			this.dynamicPrefabBuffer.Clear();
			GameManager.Instance.GetDynamicPrefabDecorator().GetWorldPrefabs(this.dynamicPrefabBuffer);
			foreach (PrefabInstance prefabInstance in this.dynamicPrefabBuffer)
			{
				_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Added, prefabInstance));
				foreach (PrefabVolumeListAbs prefabVolumeListAbs in prefabInstance.prefab.AllVolumeLists)
				{
					prefabVolumeListAbs.SendAllVolumesToClient(_toClient, prefabInstance.id);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void prefabLoadedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Added, _prefabInstance), false, -1, -1, -1, null, 192, false);
	}

	public void PrefabLoadedClient(int _prefabInstanceId, Vector3i _boundingBoxPosition, Vector3i _boundingBoxSize, string _prefabInstanceName, Vector3i _prefabSize, string _prefabFilename, int _prefabLocalRotation, int _yOffset)
	{
		PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFilename, null, null);
		PrefabInstance prefabInstance = new PrefabInstance(_prefabInstanceId, location, _boundingBoxPosition, 0, null, 0)
		{
			boundingBoxSize = _boundingBoxSize,
			name = _prefabInstanceName,
			prefab = new Prefab
			{
				size = _prefabSize,
				location = location,
				yOffset = _yOffset
			}
		};
		prefabInstance.prefab.SetLocalRotation(_prefabLocalRotation);
		prefabInstance.CreateBoundingBox(false);
		this.clientPrefabs.Add(prefabInstance);
		if (this.clientPrefabs.Count == 1)
		{
			PrefabEditModeManager.Instance.SetGroundLevel(_yOffset);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void prefabChangedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Changed, _prefabInstance), false, -1, -1, -1, null, 192, false);
	}

	public void PrefabChangedClient(int _prefabInstanceId, Vector3i _boundingBoxPosition, Vector3i _boundingBoxSize, string _prefabInstanceName, Vector3i _prefabSize, string _prefabFilename, int _prefabLocalRotation, int _yOffset)
	{
		PrefabInstance prefabInstance = this.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId.ToString());
			return;
		}
		PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFilename, null, null);
		prefabInstance.boundingBoxPosition = _boundingBoxPosition;
		prefabInstance.boundingBoxSize = _boundingBoxSize;
		prefabInstance.name = _prefabInstanceName;
		prefabInstance.prefab.size = _prefabSize;
		prefabInstance.prefab.location = location;
		prefabInstance.prefab.SetLocalRotation(_prefabLocalRotation);
		prefabInstance.prefab.yOffset = _yOffset;
		prefabInstance.CreateBoundingBox(false);
		if (this.clientPrefabs.IndexOf(prefabInstance) == 0)
		{
			PrefabEditModeManager.Instance.SetGroundLevel(_yOffset);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void prefabRemovedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Removed, _prefabInstance), false, -1, -1, -1, null, 192, false);
	}

	public void PrefabRemovedClient(int _prefabInstanceId)
	{
		for (int i = 0; i < this.clientPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.clientPrefabs[i];
			if (prefabInstance.id == _prefabInstanceId)
			{
				this.clientPrefabs.RemoveAt(i);
				using (List<PrefabVolumeListAbs>.Enumerator enumerator = prefabInstance.prefab.AllVolumeLists.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PrefabVolumeListAbs prefabVolumeListAbs = enumerator.Current;
						prefabVolumeListAbs.RemoveVolumes(prefabInstance);
					}
					break;
				}
			}
		}
	}

	public PrefabInstance GetPrefabInstance(int _prefabId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			foreach (PrefabInstance prefabInstance in this.clientPrefabs)
			{
				if (prefabInstance.id == _prefabId)
				{
					return prefabInstance;
				}
			}
			return null;
		}
		if (GameManager.Instance.GetDynamicPrefabDecorator() == null)
		{
			return null;
		}
		return GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(_prefabId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabInstanceClientManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PrefabInstance> clientPrefabs = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PrefabInstance> dynamicPrefabBuffer = new List<PrefabInstance>();
}

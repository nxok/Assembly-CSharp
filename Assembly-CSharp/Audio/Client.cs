using System;
using UnityEngine;

namespace Audio
{
	public class Client : IDisposable
	{
		public Client(int _entityId)
		{
			this.entityId = _entityId;
		}

		public void Dispose()
		{
		}

		public void Play(int playOnEntityId, string soundGroupName, float occlusion, float volumeScale = 1f)
		{
			NetPackageAudio netPackageAudio = NetPackageManager.GetPackage<NetPackageAudio>().Setup(playOnEntityId, soundGroupName, occlusion, true, false, volumeScale);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageAudio, false, this.entityId, -1, -1, null, 192, false);
		}

		public void Play(Vector3 position, string soundGroupName, float occlusion, int entityId = -1, float volumeScale = 1f)
		{
			NetPackageAudio netPackageAudio = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGroupName, occlusion, true, entityId, volumeScale);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageAudio, false, this.entityId, -1, -1, null, 192, false);
		}

		public void Stop(int stopOnEntityId, string soundGroupName)
		{
			NetPackageAudio netPackageAudio = NetPackageManager.GetPackage<NetPackageAudio>().Setup(stopOnEntityId, soundGroupName, 0f, false, false, 1f);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageAudio, false, this.entityId, -1, -1, null, 192, false);
		}

		public void Stop(Vector3 position, string soundGroupName)
		{
			NetPackageAudio netPackageAudio = NetPackageManager.GetPackage<NetPackageAudio>().Setup(position, soundGroupName, 0f, false, -1, 1f);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageAudio, false, this.entityId, -1, -1, null, 192, false);
		}

		public int entityId;
	}
}

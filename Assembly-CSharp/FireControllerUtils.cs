using System;

public static class FireControllerUtils
{
	public static void SpawnParticleEffect(ParticleEffect _pe, int _entityId)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!GameManager.IsDedicatedServer)
			{
				GameManager.Instance.SpawnParticleEffectClient(_pe, _entityId, false, true);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, false, true), false, -1, _entityId, -1, null, 192, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, false, true), false);
	}
}

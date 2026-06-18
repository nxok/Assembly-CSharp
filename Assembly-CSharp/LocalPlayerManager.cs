using System;
using UnityEngine;

public class LocalPlayerManager
{
	public static event Action OnLocalPlayersChanged;

	public static void LocalPlayersChanged()
	{
		Action onLocalPlayersChanged = LocalPlayerManager.OnLocalPlayersChanged;
		if (onLocalPlayersChanged == null)
		{
			return;
		}
		onLocalPlayersChanged();
	}

	public static void Init()
	{
		GameManager.Instance.OnLocalPlayerChanged += LocalPlayerManager.HandleLocalPlayerChanged;
	}

	public static void Destroy()
	{
		GameManager.Instance.OnLocalPlayerChanged -= LocalPlayerManager.HandleLocalPlayerChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void HandleLocalPlayerChanged(EntityPlayerLocal localPlayer)
	{
		if (localPlayer != null)
		{
			return;
		}
		foreach (LocalPlayerUI localPlayerUI in LocalPlayerUI.PlayerUIs)
		{
			if (!localPlayerUI.isPrimaryUI)
			{
				Object.Destroy(localPlayerUI.gameObject);
			}
		}
	}
}

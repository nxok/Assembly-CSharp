using System;
using System.Collections;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
	public EntityPlayerLocal entityPlayerLocal
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public LocalPlayerUI playerUI
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.avatarController = base.GetComponentInChildren<AvatarLocalPlayerController>();
		this.entityPlayerLocal = base.GetComponent<EntityPlayerLocal>();
		this.playerUI = LocalPlayerUI.GetUIForPlayer(this.entityPlayerLocal);
		LocalPlayerCamera.CameraType cameraType = LocalPlayerCamera.CameraType.Main;
		Camera[] componentsInChildren = base.GetComponentsInChildren<Camera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Camera camera = componentsInChildren[i];
			if (i > 0)
			{
				cameraType = LocalPlayerCamera.CameraType.Weapon;
			}
			if (camera.name != "FinalCamera")
			{
				LocalPlayerCamera.AddToCamera(camera, cameraType).SetUI(this.playerUI);
			}
		}
		Transform transform = this.entityPlayerLocal.playerCamera.transform;
		Transform transform2 = transform.Find("ScreenEffectsWithDepth");
		if (transform2 != null)
		{
			this.SetupLocalPlayerVisual(transform2.Find("UnderwaterHaze"));
		}
		Transform transform3 = transform.Find("effect_refract_plane");
		if (transform3 != null)
		{
			transform3.GetComponent<MeshRenderer>().material.SetInt("_ZTest", 8);
			this.SetupLocalPlayerVisual(transform3);
		}
		this.SetupLocalPlayerVisual(transform.Find("effect_underwater_debris"));
		this.SetupLocalPlayerVisual(transform.Find("effect_dropletsParticle"));
		this.SetupLocalPlayerVisual(transform.Find("effect_water_fade"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupLocalPlayerVisual(Transform _transform)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Start()
	{
		this.DispatchLocalPlayersChanged();
		while (this.avatarController == null)
		{
			yield return null;
			this.avatarController = base.GetComponentInChildren<AvatarLocalPlayerController>();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		LocalPlayerManager.OnLocalPlayersChanged += this.HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		LocalPlayerManager.OnLocalPlayersChanged -= this.HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DispatchLocalPlayersChanged()
	{
		LocalPlayerManager.LocalPlayersChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLocalPlayersChanged()
	{
		int num = 0;
		Camera[] componentsInChildren = base.GetComponentsInChildren<Camera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth = -1f + (float)(this.playerUI.playerIndex * 2 + num++) * 0.01f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.DispatchLocalPlayersChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AvatarLocalPlayerController avatarController;
}

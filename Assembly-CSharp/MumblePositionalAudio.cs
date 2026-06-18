using System;
using mumblelib;
using UnityEngine;

public class MumblePositionalAudio : SingletonMonoBehaviour<MumblePositionalAudio>
{
	public static void Init()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance != null)
		{
			return;
		}
		new GameObject("MumbleLink").AddComponent<MumblePositionalAudio>().IsPersistant = true;
	}

	public static void Destroy()
	{
		if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance == null)
		{
			return;
		}
		Object.Destroy(SingletonMonoBehaviour<MumblePositionalAudio>.Instance.gameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setCommonValues()
	{
		if (this.mumbleLink == null || this.player == null)
		{
			return;
		}
		this.mumbleLink.Name = "7 Days To Die";
		this.mumbleLink.Description = "7 Days To Die Positional Audio";
		this.mumbleLink.UIVersion = 2U;
		string text = this.player.entityId.ToString();
		Log.Out("[Mumble] Setting Mumble ID to " + text);
		this.mumbleLink.Identity = text;
		string text2 = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? GamePrefs.GetString(EnumGamePrefs.GameGuidClient) : this.player.world.Guid);
		Log.Out("[Mumble] Setting context to " + text2);
		this.mumbleLink.Context = text2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initShm()
	{
		this.mumbleLink = LinkFileManager.Open();
		this.setCommonValues();
		Log.Out("[Mumble] Shared Memory initialized");
	}

	public void ReinitShm()
	{
		if (this.mumbleLink == null)
		{
			this.initShm();
			return;
		}
		this.setCommonValues();
	}

	public void SetPlayer(EntityPlayerLocal player)
	{
		this.player = player;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.player == null)
		{
			if (this.mumbleLink != null)
			{
				if (!this.contextCleared)
				{
					this.contextCleared = true;
					this.mumbleLink.Context = "";
					this.mumbleLink.Tick();
					return;
				}
				this.mumbleLink.Dispose();
				this.mumbleLink = null;
			}
			return;
		}
		if (this.mumbleLink == null)
		{
			try
			{
				this.initShm();
				this.initErrorLoggedOnce = false;
				this.contextCleared = false;
			}
			catch (Exception ex)
			{
				if (!this.initErrorLoggedOnce)
				{
					this.initErrorLoggedOnce = true;
					Log.Error("[Mumble] Error initializing Mumble link:");
					Log.Exception(ex);
				}
				return;
			}
		}
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime - this.lastUpdateTime < 0.02f)
		{
			return;
		}
		this.lastUpdateTime = unscaledTime;
		if (this.mumbleLink.UIVersion == 0U)
		{
			Log.Warning("[Mumble] Mumble disconnected, reinit");
			this.ReinitShm();
		}
		this.mumbleLink.AvatarPosition = (this.mumbleLink.CameraPosition = this.player.position);
		this.mumbleLink.AvatarForward = (this.mumbleLink.CameraForward = this.player.cameraTransform.forward);
		this.mumbleLink.Tick();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void singletonDestroy()
	{
		if (this.mumbleLink != null)
		{
			if (!this.contextCleared)
			{
				this.contextCleared = true;
				this.mumbleLink.Context = "";
				this.mumbleLink.Tick();
			}
			this.mumbleLink.Dispose();
			this.mumbleLink = null;
			Log.Out("[Mumble] Shared Memory disposed");
		}
		Log.Out("[Mumble] Link destroyed");
	}

	public void printUiVersion()
	{
		if (this.mumbleLink == null)
		{
			Log.Out("[Mumble] MumbleLink == null!");
			return;
		}
		Log.Out(string.Format("[Mumble] UiVersion = {0}", this.mumbleLink.UIVersion));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float updateInterval = 0.02f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ILinkFile mumbleLink;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool initErrorLoggedOnce;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool contextCleared;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastUpdateTime;
}

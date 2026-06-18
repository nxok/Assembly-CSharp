using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class SignTextureManager
{
	public static SignTextureManager Instance
	{
		get
		{
			return SignTextureManager.m_instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignTextureManager()
	{
		this._prioritizer = new SignPrioritizer();
		this._store = new SignTextureStore();
		this._scheduler = new BakeScheduler();
	}

	public bool IsEnabled
	{
		get
		{
			return this._initialised;
		}
	}

	public SignTextureManager.SignTextureQuality CurrentQuality
	{
		get
		{
			return this._qualityLevel;
		}
	}

	public void SetQuality(SignTextureManager.SignTextureQuality quality)
	{
		if (this._qualityLevel == quality)
		{
			return;
		}
		this._qualityLevel = quality;
		this.SelectActiveTiers();
		if (this._initialised)
		{
			if (this._qualityLevel == SignTextureManager.SignTextureQuality.Infinite)
			{
				foreach (SignCanvas signCanvas in this._canvases)
				{
					if (signCanvas != null)
					{
						signCanvas.SetTexture(null);
					}
				}
			}
			this._store.RebuildPools(this._activeTiers);
			this._scheduler.Clear();
			this._requestsDirty = true;
		}
	}

	public void SetTileSize(int tileSize)
	{
		if (tileSize > 0)
		{
			this._tileSize = tileSize;
		}
	}

	public void SetTileSizeForCurrentQuality()
	{
		if (this._activeTiers != null && this._activeTiers.Length != 0)
		{
			this._tileSize = this._activeTiers[0].Resolution;
		}
	}

	public void SetShowProgress(bool showProgress)
	{
		this._showProgress = showProgress;
	}

	public bool Initialize()
	{
		bool flag;
		using (SignTextureManager.s_SignTextureManagerInitialize.Auto())
		{
			this._initialised = false;
			if (GameManager.IsDedicatedServer)
			{
				flag = false;
			}
			else
			{
				this._shader = GlobalAssets.FindShader("Game/SignTech/UI");
				if (this._shader == null)
				{
					Log.Error("Failed to initialize SignTextureManager. Shader not found: Game/SignTech/UI");
					flag = false;
				}
				else
				{
					if (this._material == null)
					{
						this._material = new Material(this._shader)
						{
							hideFlags = 61
						};
					}
					if (this._cmd == null)
					{
						this._cmd = new CommandBuffer
						{
							name = "SignTech_Bake"
						};
					}
					this.SelectActiveTiers();
					this._store.RebuildPools(this._activeTiers);
					this._initialised = true;
					flag = true;
				}
			}
		}
		return flag;
	}

	public void Cleanup(bool preserveRegisteredCanvases = false)
	{
		using (SignTextureManager.s_SignTextureManagerCleanup.Auto())
		{
			foreach (SignCanvas signCanvas in this._canvases)
			{
				if (signCanvas != null)
				{
					signCanvas.SetTexture(null);
				}
			}
			if (!preserveRegisteredCanvases)
			{
				this._canvases.Clear();
				SignTextureManager.s_RegisteredSigns.Value = this._canvases.Count;
			}
			this._scheduler.Clear();
			this._prioritizer.Clear();
			this._store.Clear();
			if (this._material != null)
			{
				Object.DestroyImmediate(this._material);
				this._material = null;
			}
			if (this._cmd != null)
			{
				this._cmd.Release();
				this._cmd = null;
			}
			this._shader = null;
			this._initialised = false;
			this._requestsDirty = true;
		}
	}

	public void Register(SignCanvas canvas)
	{
		if (!this._initialised)
		{
			return;
		}
		if (canvas == null)
		{
			return;
		}
		this._canvases.Add(canvas);
		this._requestsDirty = true;
		SignTextureManager.s_RegisteredSigns.Value = this._canvases.Count;
	}

	public void Deregister(SignCanvas canvas)
	{
		if (!this._initialised)
		{
			return;
		}
		if (canvas == null)
		{
			return;
		}
		this._canvases.Remove(canvas);
		SignTextureManager.s_RegisteredSigns.Value = this._canvases.Count;
	}

	public void Invalidate(SignCanvas canvas)
	{
		using (SignTextureManager.s_SignTextureManagerInvalidate.Auto())
		{
			if (this._initialised)
			{
				if (!(canvas == null))
				{
					if (canvas.DisplaySignId.IsValid)
					{
						GlobalSignId displaySignId = canvas.DisplaySignId;
						foreach (SignCanvas signCanvas in this._canvases)
						{
							if (signCanvas != null && signCanvas.DisplaySignId.Equals(displaySignId))
							{
								signCanvas.SetTexture(null);
							}
						}
						this._store.InvalidateSign(displaySignId);
						this._scheduler.Clear();
						this._requestsDirty = true;
					}
				}
			}
		}
	}

	public void MainThreadUpdate()
	{
		SignTextureManager.<>c__DisplayClass48_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		using (SignTextureManager.s_SignTextureManagerMainThreadUpdate.Auto())
		{
			if (this._initialised)
			{
				if (!(this._material == null) && this._cmd != null)
				{
					if (this._qualityLevel != SignTextureManager.SignTextureQuality.Infinite)
					{
						this._framesSinceRefreshed += 1U;
						CS$<>8__locals1.playerPos = SignTextureManager.GetPlayerPos();
						float sqrMagnitude = (CS$<>8__locals1.playerPos - this._lastUpdatePosition).sqrMagnitude;
						this._requestsDirty |= this._framesSinceRefreshed > 10U && sqrMagnitude > 1f;
						if (this._requestsDirty && this._scheduler.CurrentState == BakeScheduler.BakeJobState.None)
						{
							this.<MainThreadUpdate>g__RefreshRequestQueue|48_0(ref CS$<>8__locals1);
						}
						else
						{
							this._scheduler.TickOnce(this._tileSize, this._material, this._cmd, this._store, this._prioritizer, this._showProgress);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 GetPlayerPos()
	{
		return GameManager.Instance.World.GetPrimaryPlayer().position + new Vector3(0f, 1.5f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectActiveTiers()
	{
		SignTextureManager.TierSpec[] array;
		switch (this._qualityLevel)
		{
		case SignTextureManager.SignTextureQuality.Low:
			array = SignTextureManager.s_tiersLow;
			break;
		case SignTextureManager.SignTextureQuality.Medium:
			array = SignTextureManager.s_tiersMedium;
			break;
		case SignTextureManager.SignTextureQuality.High:
			array = SignTextureManager.s_tiersHigh;
			break;
		case SignTextureManager.SignTextureQuality.Ultra:
			array = SignTextureManager.s_tiersUltra;
			break;
		case SignTextureManager.SignTextureQuality.Infinite:
			array = Array.Empty<SignTextureManager.TierSpec>();
			break;
		default:
			array = SignTextureManager.s_tiersLowest;
			break;
		}
		this._activeTiers = array;
	}

	public void LogDebugInfo(bool saveToDisk)
	{
		this.sb.Clear();
		this.sb.AppendLine("[STM] LogDebugInfo");
		bool flag = true;
		if (!this._initialised)
		{
			flag = false;
			this.sb.AppendLine("Cannot log sign group info: SignTextureManager is not initialized.");
		}
		else if (this._material == null || this._cmd == null)
		{
			flag = false;
			this.sb.AppendLine("Cannot log sign group info: SignTextureManager has null material or command buffer.");
		}
		else if (this._qualityLevel == SignTextureManager.SignTextureQuality.Infinite)
		{
			flag = false;
			this.sb.AppendLine("Cannot log sign group info: SignTextureManager is in Infinite quality mode.");
		}
		if (flag)
		{
			Vector3 playerPos = SignTextureManager.GetPlayerPos();
			float sqrMagnitude = (playerPos - this._lastUpdatePosition).sqrMagnitude;
			this.sb.AppendLine();
			this.sb.AppendLine("**************************************************");
			this.sb.AppendLine("Manager (Enabled)");
			this.sb.AppendLine("**************************************************");
			this.sb.AppendLine();
			this.sb.AppendLine(string.Format("Registered Canvases: {0}", this._canvases.Count));
			this.sb.AppendLine();
			this.sb.AppendLine(string.Format("Bake Job State: {0}", this._scheduler.CurrentState));
			this.sb.AppendLine(string.Format("Player Pos: {0}", playerPos));
			this.sb.AppendLine(string.Format("Last Update Pos: {0}", this._lastUpdatePosition));
			this.sb.AppendLine(string.Format("Sq Dist Since Update: {0} (min: {1}, ready: {2})", sqrMagnitude.ToString("00.000"), 1f, sqrMagnitude > 1f));
			this.sb.AppendLine(string.Format("Frames Since Update: {0} (min: {1}, ready: {2})", this._framesSinceRefreshed, 10U, this._framesSinceRefreshed > 10U));
			this.sb.AppendLine(string.Format("Requests Dirty: {0}", this._requestsDirty));
			this.sb.AppendLine();
			this.sb.AppendLine(string.Format("Tile Size: {0}", this._tileSize));
			this.sb.AppendLine(string.Format("Quality: {0}", this._qualityLevel));
			if (this._activeTiers == null)
			{
				this.sb.AppendLine("Active Tiers: NULL");
			}
			else if (this._activeTiers.Length == 0)
			{
				this.sb.AppendLine("Active Tiers: NONE");
			}
			else
			{
				this.sb.AppendLine("Active Tiers:");
				for (int i = 0; i < this._activeTiers.Length; i++)
				{
					SignTextureManager.TierSpec tierSpec = this._activeTiers[i];
					this.sb.AppendLine(string.Format("\t{0}: {1}px, {2} active, {3} buffer", new object[] { i, tierSpec.Resolution, tierSpec.ActiveCount, tierSpec.BufferCount }));
				}
			}
			this.sb.AppendLine();
			this._store.WriteTextureStoreInfo(this.sb);
			this.sb.AppendLine();
			this._prioritizer.WriteGroupInfo(this.sb, playerPos, this._activeTiers);
			Log.Out(this.sb.ToString());
			if (saveToDisk)
			{
				string text = string.Format("STM_Dump_{0:yyyyMMdd_HHmmss}.txt", DateTime.Now);
				string text2 = Path.Combine(Application.dataPath + "/../", text);
				File.WriteAllText(text2, this.sb.ToString());
				Log.Out("[STM] Full dump saved to " + text2);
			}
		}
		else
		{
			Log.Warning(this.sb.ToString());
		}
		this.sb.Clear();
	}

	[Conditional("SIGN_TEXTURE_DEBUG_LOG_INIT")]
	public static void DebugLog(string message)
	{
		Log.Out("[STM] " + message);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <MainThreadUpdate>g__RefreshRequestQueue|48_0(ref SignTextureManager.<>c__DisplayClass48_0 A_1)
	{
		this._store.BeginFrameMarkAllNotInUse();
		this._tmpRequests.Clear();
		this._prioritizer.RebuildRequests(this._canvases, A_1.playerPos, this._activeTiers, this._store, this._tmpRequests);
		this._store.EvictUnused();
		this._scheduler.ResetRequests(this._tmpRequests);
		this._lastUpdatePosition = A_1.playerPos;
		this._framesSinceRefreshed = 0U;
		this._requestsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string kShaderName = "Game/SignTech/UI";

	public static readonly ProfilerMarker s_SignTextureManagerInitialize = new ProfilerMarker("SignTextureManager.Initialize");

	public static readonly ProfilerMarker s_SignTextureManagerCleanup = new ProfilerMarker("SignTextureManager.Cleanup");

	public static readonly ProfilerMarker s_SignTextureManagerInvalidate = new ProfilerMarker("SignTextureManager.Invalidate");

	public static readonly ProfilerMarker s_SignTextureManagerMainThreadUpdate = new ProfilerMarker("SignTextureManager.MainThreadUpdate");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_RegisteredSigns = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "STM Registered Signs", 3, 2);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureManager m_instance = new SignTextureManager();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureManager.TierSpec[] s_tiersLowest = new SignTextureManager.TierSpec[]
	{
		new SignTextureManager.TierSpec(64, 32, 32),
		new SignTextureManager.TierSpec(128, 32, 16),
		new SignTextureManager.TierSpec(512, 8, 8)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureManager.TierSpec[] s_tiersLow = new SignTextureManager.TierSpec[]
	{
		new SignTextureManager.TierSpec(128, 32, 32),
		new SignTextureManager.TierSpec(512, 8, 8),
		new SignTextureManager.TierSpec(1024, 3, 4)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureManager.TierSpec[] s_tiersMedium = new SignTextureManager.TierSpec[]
	{
		new SignTextureManager.TierSpec(256, 32, 16),
		new SignTextureManager.TierSpec(512, 8, 4),
		new SignTextureManager.TierSpec(1024, 3, 4)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureManager.TierSpec[] s_tiersHigh = new SignTextureManager.TierSpec[]
	{
		new SignTextureManager.TierSpec(256, 32, 16),
		new SignTextureManager.TierSpec(1024, 10, 6),
		new SignTextureManager.TierSpec(2048, 1, 1)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureManager.TierSpec[] s_tiersUltra = new SignTextureManager.TierSpec[]
	{
		new SignTextureManager.TierSpec(512, 32, 16),
		new SignTextureManager.TierSpec(1024, 10, 6),
		new SignTextureManager.TierSpec(2048, 2, 2)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<SignCanvas> _canvases = new HashSet<SignCanvas>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignPrioritizer _prioritizer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignTextureStore _store;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BakeScheduler _scheduler;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SignBakeRequest> _tmpRequests = new List<SignBakeRequest>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader _shader;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material _material;

	[PublicizedFrom(EAccessModifier.Private)]
	public CommandBuffer _cmd;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _initialised;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignTextureManager.SignTextureQuality _qualityLevel = SignTextureManager.SignTextureQuality.Medium;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignTextureManager.TierSpec[] _activeTiers = SignTextureManager.s_tiersMedium;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _tileSize = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _requestsDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _lastUpdatePosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint _framesSinceRefreshed;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint kMinFramesSinceRefreshed = 10U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinSqrDistanceSinceRefreshed = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _showProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder sb = new StringBuilder();

	public enum SignTextureQuality
	{
		Lowest,
		Low,
		Medium,
		High,
		Ultra,
		Infinite
	}

	public readonly struct TierSpec
	{
		public int TotalCount
		{
			get
			{
				return this.ActiveCount + this.BufferCount;
			}
		}

		public TierSpec(int resolution, int activeCount, int bufferCount)
		{
			this.Resolution = resolution;
			this.ActiveCount = activeCount;
			this.BufferCount = bufferCount;
		}

		public readonly int Resolution;

		public readonly int ActiveCount;

		public readonly int BufferCount;
	}
}

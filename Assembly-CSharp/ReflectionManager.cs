using System;
using System.Collections.Generic;
using ShinyScreenSpaceRaytracedReflections;
using UnityEngine;

public class ReflectionManager
{
	public static ReflectionManager Create(EntityPlayerLocal player)
	{
		ReflectionManager reflectionManager = new ReflectionManager();
		reflectionManager.player = player;
		reflectionManager.Init();
		return reflectionManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		Object.Destroy(this.player.gameObject.GetComponentInChildren<PlayerReflectionProbe>().gameObject);
		this.managerObj = new GameObject("ReflectionManager");
		this.managerObj.layer = 2;
		Transform transform = this.managerObj.transform;
		this.hasCopySupport = SystemInfo.copyTextureSupport > 0;
		this.probes = new List<ReflectionManager.Probe>();
		if (this.hasCopySupport)
		{
			for (int i = 0; i < 1; i++)
			{
				ReflectionManager.Probe probe = this.AddProbe(transform, false);
				this.probes.Add(probe);
			}
		}
		this.mainProbe = this.AddProbe(this.player.transform, true);
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality);
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxReflectShadows);
		this.ApplyProbeOptions(@int, @bool);
	}

	public static void ApplyOptions(bool useSimple = false)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality);
		bool flag = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxReflectShadows);
		if (useSimple)
		{
			num = 0;
			flag = false;
		}
		World world = GameManager.Instance.World;
		if (world != null)
		{
			List<EntityPlayerLocal> localPlayers = world.GetLocalPlayers();
			for (int i = localPlayers.Count - 1; i >= 0; i--)
			{
				localPlayers[i].renderManager.reflectionManager.ApplyProbeOptions(num, flag);
			}
		}
	}

	public void Destroy()
	{
		Object.Destroy(this.managerObj);
		this.probes.Clear();
		if (this.mainTex)
		{
			this.mainTex.Release();
			this.mainTex = null;
		}
		Object.Destroy(this.mainProbe.reflectionProbe.gameObject);
		this.mainProbe = null;
		if (this.blendTex)
		{
			this.blendTex.Release();
			this.blendTex = null;
		}
		if (this.captureTex)
		{
			this.captureTex.Release();
			this.captureTex = null;
		}
	}

	public void LightChanged(Vector3 lightPos)
	{
		ReflectionManager.Probe probe = this.mainProbe;
		if (this.probes.Count > 0)
		{
			probe = this.probes[0];
		}
		if ((lightPos - probe.worldPos).sqrMagnitude <= 225f)
		{
			probe.updateTime = 0f;
		}
	}

	public void FrameUpdate()
	{
		if (ReflectionManager.optionsSelected.resolution == 0)
		{
			return;
		}
		int count = this.probes.Count;
		if (this.renderProbe != null)
		{
			ReflectionProbe reflectionProbe = this.renderProbe.reflectionProbe;
			if (this.renderFixTime > 0f)
			{
				this.renderFixTime -= Time.deltaTime;
				if (this.renderFixTime <= 0f)
				{
					reflectionProbe.enabled = false;
					reflectionProbe.refreshMode = 2;
					this.renderProbe = null;
				}
			}
			else if (reflectionProbe.IsFinishedRendering(this.renderId))
			{
				if (this.hasCopySupport && this.renderProbe == this.blendProbe)
				{
					this.blendProbe = null;
				}
				this.renderProbe = null;
			}
			else
			{
				this.renderDuration += Time.deltaTime;
				if (this.renderDuration > 2f)
				{
					this.renderFixTime = 1f;
					reflectionProbe.refreshMode = 1;
				}
			}
		}
		Vector3 position = this.player.position;
		position.y += 1.55f;
		if (this.player.IsCrouching)
		{
			position.y -= 0.2f;
		}
		Vector3 vector = this.player.GetVelocityPerSecond() * ReflectionManager.optionsSelected.playerVel;
		float num = vector.magnitude + 0.15f;
		Vector3 vector2 = position + vector;
		RaycastHit raycastHit;
		if (num > 0.2f && Physics.SphereCast(position - Origin.position, 0.2f, vector, ref raycastHit, num - 0.2f, 1082195968))
		{
			vector2 = position;
		}
		ReflectionManager.Probe probe = this.mainProbe;
		if (count > 0)
		{
			this.SortProbes(vector2);
			probe = this.probes[0];
			if (probe != this.blendProbe && probe != this.renderProbe)
			{
				Graphics.CopyTexture(this.mainTex, this.blendTex);
				this.blendProbe = probe;
				this.blendPer = 0.05f;
				this.blendPos = vector2;
			}
			if (this.blendPer > 0f)
			{
				float magnitude = (vector2 - this.blendPos).magnitude;
				this.blendPos = vector2;
				float deltaTime = Time.deltaTime;
				float num2 = magnitude / 0.75f;
				num2 += 0.8333333f * ReflectionManager.optionsSelected.rateScale * deltaTime;
				num2 *= 1f - Mathf.Pow(this.blendPer, 0.7f);
				this.blendPer += num2;
				this.blendPer += ((this.renderProbe != null) ? (ReflectionManager.optionsSelected.rateRender * ReflectionManager.optionsSelected.rateScale * deltaTime) : 0f);
				if (this.blendPer < 0.95f)
				{
					ReflectionProbe.BlendCubemap(this.blendTex, this.captureTex, this.blendPer, this.mainTex);
				}
				else
				{
					ReflectionProbe.BlendCubemap(this.blendTex, this.captureTex, 1f, this.mainTex);
					this.blendPer = 0f;
				}
			}
		}
		if (this.renderProbe == null)
		{
			ReflectionManager.Probe probe2 = null;
			if (Time.time - probe.updateTime >= 8f)
			{
				probe2 = probe;
			}
			float worldLightLevelInRange = LightManager.GetWorldLightLevelInRange(probe.worldPos, 40f);
			float num3 = worldLightLevelInRange - probe.lightLevel;
			if (num3 < -0.15f || num3 > 0.15f)
			{
				probe2 = probe;
			}
			Vector3 forward = this.player.cameraTransform.forward;
			if (Vector3.Dot(forward, probe.forward) < 0.7f)
			{
				probe2 = probe;
			}
			float sqrMagnitude = (vector2 - probe.worldPos).sqrMagnitude;
			float num4 = 0.3f / ReflectionManager.optionsSelected.rateScale;
			if (sqrMagnitude >= num4 * num4)
			{
				probe2 = probe;
				if (count > 1)
				{
					probe2 = this.probes[count - 1];
				}
			}
			if (probe2 != null)
			{
				probe2.lightLevel = worldLightLevelInRange;
				probe2.updateTime = Time.time;
				probe2.worldPos = vector2;
				probe2.forward = forward;
				probe2.t.position = vector2 - Origin.position;
				ReflectionProbe reflectionProbe2 = probe2.reflectionProbe;
				reflectionProbe2.enabled = true;
				int num5 = this.renderId;
				this.renderId = reflectionProbe2.RenderProbe(this.captureTex);
				if (this.renderId == num5)
				{
					Log.Warning("{0} ReflectionManager #{1}, rid {2}, probe stuck", new object[]
					{
						GameManager.frameCount,
						this.probes.IndexOf(this.renderProbe),
						this.renderId
					});
				}
				this.renderProbe = probe2;
				this.renderDuration = 0f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortProbes(Vector3 pos)
	{
		for (int i = this.probes.Count - 1; i >= 0; i--)
		{
			ReflectionManager.Probe probe = this.probes[i];
			probe.distSq = (pos - probe.worldPos).sqrMagnitude;
		}
		this.probes.Sort(this.sorter);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ReflectionManager.Probe AddProbe(Transform parentT, bool isMain)
	{
		ReflectionManager.Probe probe = new ReflectionManager.Probe();
		GameObject gameObject = new GameObject("RProbe");
		gameObject.layer = 2;
		Transform transform = gameObject.transform;
		probe.t = transform;
		transform.SetParent(parentT, false);
		ReflectionProbe reflectionProbe = gameObject.AddComponent<ReflectionProbe>();
		probe.reflectionProbe = reflectionProbe;
		reflectionProbe.enabled = false;
		reflectionProbe.mode = 1;
		reflectionProbe.refreshMode = 2;
		reflectionProbe.blendDistance = 20f;
		reflectionProbe.center = Vector3.zero;
		reflectionProbe.size = Vector3.zero;
		reflectionProbe.clearFlags = 2;
		if (isMain)
		{
			reflectionProbe.blendDistance = 400f;
			reflectionProbe.size = new Vector3(400f, 400f, 400f);
			reflectionProbe.importance = 10;
			if (this.hasCopySupport)
			{
				reflectionProbe.mode = 2;
			}
		}
		return probe;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyProbeOptions(int quality, bool useShadows)
	{
		if (quality < 0 || quality >= ReflectionManager.optionsData.Length)
		{
			quality = ReflectionManager.optionsData.Length - 1;
		}
		ReflectionManager.optionsSelected = ReflectionManager.optionsData[quality];
		for (int i = this.probes.Count - 1; i >= 0; i--)
		{
			ReflectionManager.Probe probe = this.probes[i];
			this.ApplyProbeOptions(probe, useShadows);
		}
		this.ApplyProbeOptions(this.mainProbe, useShadows);
		if (ReflectionManager.optionsSelected.resolution > 0)
		{
			this.mainProbe.reflectionProbe.enabled = true;
		}
		bool flag = ReflectionManager.optionsSelected.resolution > 0;
		Shader.SetGlobalFloat("_ReflectionsOn", (float)(flag ? 1 : 0));
		if (!flag)
		{
			Shader.EnableKeyword("GAME_NOREFLECTION");
		}
		else
		{
			Shader.DisableKeyword("GAME_NOREFLECTION");
		}
		if (!this.mainTex || this.mainTex.width != ReflectionManager.optionsSelected.resolution)
		{
			if (this.mainTex)
			{
				this.mainTex.Release();
				this.mainTex = null;
			}
			if (flag)
			{
				this.mainTex = this.CreateTexture(false);
				this.mainTex.name = "probeMain";
				this.mainProbe.reflectionProbe.customBakedTexture = this.mainTex;
				if (!this.hasCopySupport)
				{
					this.captureTex = this.mainTex;
				}
			}
		}
		if (this.hasCopySupport)
		{
			if (!this.blendTex || this.blendTex.width != ReflectionManager.optionsSelected.resolution)
			{
				if (this.blendTex)
				{
					this.blendTex.Release();
					this.blendTex = null;
				}
				if (flag)
				{
					this.blendTex = this.CreateTexture(false);
					this.blendTex.name = "probeBlend";
				}
			}
			if (!this.captureTex || this.captureTex.width != ReflectionManager.optionsSelected.resolution)
			{
				if (this.captureTex)
				{
					this.captureTex.Release();
					this.captureTex = null;
				}
				if (flag && this.hasCopySupport)
				{
					this.captureTex = this.CreateTexture(false);
					this.captureTex.name = "probeCap";
				}
			}
		}
		this.ApplyWaterSetting();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyProbeOptions(ReflectionManager.Probe probe, bool useShadows)
	{
		ReflectionProbe reflectionProbe = probe.reflectionProbe;
		reflectionProbe.enabled = false;
		if (ReflectionManager.optionsSelected.resolution == 0)
		{
			return;
		}
		reflectionProbe.nearClipPlane = 0.1f;
		reflectionProbe.farClipPlane = ReflectionManager.optionsSelected.farClip;
		reflectionProbe.shadowDistance = (useShadows ? ReflectionManager.optionsSelected.shadowDist : 0f);
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
		reflectionProbe.intensity = ReflectionManager.optionsSelected.intensity * (useShadows ? 1f : 0.85f) * ((@int > 0) ? 0.91f : 1f);
		reflectionProbe.resolution = ReflectionManager.optionsSelected.resolution;
		reflectionProbe.cullingMask = ReflectionManager.optionsSelected.mask;
		if (ReflectionManager.optionsSelected.rate <= 1)
		{
			reflectionProbe.timeSlicingMode = 1;
			return;
		}
		if (ReflectionManager.optionsSelected.rate <= 2)
		{
			reflectionProbe.timeSlicingMode = 0;
			return;
		}
		reflectionProbe.timeSlicingMode = 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyWaterSetting()
	{
	}

	public void ApplyCameraOptions(Camera camera)
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
		ShinySSRR component = camera.GetComponent<ShinySSRR>();
		if (component)
		{
			component.enabled = @int >= 1;
			component.jitter = 0.2f;
			switch (@int)
			{
			case 1:
				component.ApplyRaytracingPreset(10);
				component.minimumBlur = 0.5f;
				break;
			case 2:
				component.ApplyRaytracingPreset(20);
				component.minimumBlur = 0.35f;
				component.sampleCount = 32;
				component.maxRayLength = 16f;
				break;
			case 3:
				component.ApplyRaytracingPreset(30);
				component.minimumBlur = 0.3f;
				break;
			case 4:
				component.ApplyRaytracingPreset(35);
				component.minimumBlur = 0.2f;
				break;
			}
			component.refineThickness = false;
			component.temporalFilter = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture CreateTexture(bool autoGenMips)
	{
		RenderTextureFormat renderTextureFormat = 0;
		RenderTextureDescriptor renderTextureDescriptor;
		renderTextureDescriptor..ctor(ReflectionManager.optionsSelected.resolution, ReflectionManager.optionsSelected.resolution, renderTextureFormat, 0);
		renderTextureDescriptor.dimension = 4;
		renderTextureDescriptor.useMipMap = true;
		renderTextureDescriptor.autoGenerateMips = autoGenMips;
		RenderTexture renderTexture = new RenderTexture(renderTextureDescriptor);
		renderTexture.Create();
		if (!autoGenMips)
		{
			renderTexture.GenerateMips();
		}
		return renderTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cProbeCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdateAge = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdatePlayerDistance = 0.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdateLightDistance = 15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBlendInPerSec = 0.8333333f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cOffsetY = 1.55f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cIntensityNoShadowsScale = 0.85f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject managerObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public ReflectionManager.Probe mainProbe;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture mainTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ReflectionManager.Probe> probes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasCopySupport;

	[PublicizedFrom(EAccessModifier.Private)]
	public float blendPer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 blendPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public ReflectionManager.Probe blendProbe;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture blendTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture captureTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public ReflectionManager.Probe renderProbe;

	[PublicizedFrom(EAccessModifier.Private)]
	public float renderDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public int renderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public float renderFixTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ReflectionManager.Sorter sorter = new ReflectionManager.Sorter();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSkyLayer = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReflectionManager.Options[] optionsData = new ReflectionManager.Options[]
	{
		default(ReflectionManager.Options),
		new ReflectionManager.Options
		{
			rate = 1,
			rateScale = 0.5f,
			rateRender = 2f,
			playerVel = 0.1f,
			farClip = 30f,
			shadowDist = 25f,
			resolution = 64,
			intensity = 0.55f,
			mask = 268435968
		},
		new ReflectionManager.Options
		{
			rate = 1,
			rateScale = 1f,
			rateRender = 2f,
			playerVel = 0.1f,
			farClip = 90f,
			shadowDist = 30f,
			resolution = 128,
			intensity = 0.65f,
			mask = 276824576
		},
		new ReflectionManager.Options
		{
			rate = 1,
			rateScale = 1.2f,
			rateRender = 2f,
			playerVel = 0.1f,
			farClip = 180f,
			shadowDist = 50f,
			resolution = 256,
			intensity = 0.66f,
			mask = 276824576
		},
		new ReflectionManager.Options
		{
			rate = 2,
			rateScale = 1.6f,
			rateRender = 2f,
			playerVel = 0.08f,
			farClip = 280f,
			shadowDist = 70f,
			resolution = 256,
			intensity = 0.67f,
			mask = 276824576
		},
		new ReflectionManager.Options
		{
			rate = 3,
			rateScale = 12f,
			rateRender = 5f,
			playerVel = 0.01f,
			farClip = 425f,
			shadowDist = 100f,
			resolution = 512,
			intensity = 0.68f,
			mask = 276824576
		},
		new ReflectionManager.Options
		{
			rate = 3,
			rateScale = 12f,
			rateRender = 5f,
			playerVel = 0.01f,
			farClip = 600f,
			shadowDist = 150f,
			resolution = 512,
			intensity = 0.68f,
			mask = 276824576
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReflectionManager.Options optionsSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] waterTransparencyDistances = new int[]
	{
		32, 32, 32, 32, 32, 32, 36, 40, 44, 48,
		52, 56, 60
	};

	public class Probe
	{
		public Transform t;

		public ReflectionProbe reflectionProbe;

		public Vector3 worldPos;

		public Vector3 forward;

		public float distSq;

		public float lightLevel;

		public float updateTime;
	}

	public class Sorter : IComparer<ReflectionManager.Probe>
	{
		public int Compare(ReflectionManager.Probe _p1, ReflectionManager.Probe _p2)
		{
			if (_p1.distSq < _p2.distSq)
			{
				return -1;
			}
			if (_p1.distSq > _p2.distSq)
			{
				return 1;
			}
			return 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Options
	{
		public int rate;

		public float rateScale;

		public float rateRender;

		public float playerVel;

		public float farClip;

		public float shadowDist;

		public int resolution;

		public float intensity;

		public int mask;
	}
}

using System;
using System.Runtime.CompilerServices;
using PI.NGSS;
using UnityEngine;

public class SkyManager : MonoBehaviour
{
	public static Transform SunLightT
	{
		get
		{
			return SkyManager.sunLightT;
		}
	}

	public static bool IsBloodMoonVisible()
	{
		return GameUtils.IsBloodMoonTime(new ValueTuple<int, int>(SkyManager.duskTime - 4, SkyManager.dawnTime + 2), (int)SkyManager.TimeOfDay(), SkyManager.bloodmoonDay, (int)SkyManager.dayCount);
	}

	public static float BloodMoonVisiblePercent()
	{
		int num = (int)SkyManager.dayCount;
		float num2 = SkyManager.TimeOfDay();
		if (num != SkyManager.bloodmoonDay)
		{
			return (float)((num > 1 && num == SkyManager.bloodmoonDay + 1 && num2 <= (float)(SkyManager.dawnTime + 2)) ? 1 : 0);
		}
		float num3 = (float)SkyManager.duskTime - num2;
		if (num3 < 0f)
		{
			return 1f;
		}
		float num4 = 1f - num3 / 4f;
		if (num4 >= 0f)
		{
			return num4;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		GameOptionsManager.ShadowDistanceChanged += this.OnShadowDistanceChanged;
		GameStats.OnChangedDelegates += this.OnGameStatsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ShadowDistanceChanged -= this.OnShadowDistanceChanged;
		GameStats.OnChangedDelegates -= this.OnGameStatsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnShadowDistanceChanged(int optionsShadowDistance)
	{
		int num;
		switch (optionsShadowDistance)
		{
		case 0:
		case 1:
		case 2:
			num = 1024;
			goto IL_0036;
		case 3:
			num = 2048;
			goto IL_0036;
		}
		num = 4096;
		IL_0036:
		SkyManager.sunLight.shadowCustomResolution = num;
		SkyManager.moonLight.shadowCustomResolution = num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameStatsChanged(EnumGameStats _gameState, object _newValue)
	{
		if (_gameState != EnumGameStats.DayLightLength)
		{
			if (_gameState == EnumGameStats.BloodMoonDay)
			{
				SkyManager.bloodmoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
				return;
			}
		}
		else
		{
			ValueTuple<int, int> valueTuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
			SkyManager.duskTime = valueTuple.Item1;
			SkyManager.dawnTime = valueTuple.Item2;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		SkyManager.Reset();
	}

	public static void Loaded(GameObject _obj)
	{
		_obj.SetActive(true);
		_obj.transform.parent = GameManager.Instance.transform;
		SkyManager.skyManager = _obj.GetComponent<SkyManager>();
		SkyManager.Reset();
	}

	public static void Cleanup()
	{
		if (SkyManager.skyManager != null)
		{
			Object.DestroyImmediate(SkyManager.skyManager.gameObject);
		}
	}

	public static void SetSkyEnabled(bool _enabled)
	{
		if (_enabled)
		{
			SkyManager.SetFogDebug(-1f, float.MinValue, float.MinValue);
			SkyManager.mainCamera.backgroundColor = Color.black;
		}
		else
		{
			SkyManager.SetFogDebug(0f, float.MinValue, float.MinValue);
			SkyManager.mainCamera.backgroundColor = new Color(0.44f, 0.48f, 0.52f);
		}
		SkyManager.atmosphereSphere.gameObject.SetActive(_enabled);
		SkyManager.cloudsSphere.gameObject.SetActive(_enabled);
	}

	public static Color GetSkyColor()
	{
		return SkyManager.SkyColor;
	}

	public static void SetSkyColor(Color c)
	{
		SkyManager.SkyColor = c;
	}

	public static void SetGameTime(ulong _time)
	{
		SkyManager.dayCount = _time / 24000f + 1f;
		SkyManager.timeOfDay = _time;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float TimeOfDay()
	{
		return SkyManager.timeOfDay % 24000UL / 1000f;
	}

	public static float GetTimeOfDayAsMinutes()
	{
		return SkyManager.TimeOfDay() / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	public static void SetCloudTextures(Texture _mainTex, Texture _blendTex)
	{
		SkyManager.cloudMainTex = _mainTex;
		SkyManager.cloudBlendTex = _blendTex;
	}

	public static void SetCloudTransition(float t)
	{
		SkyManager.cloudTransition = t;
	}

	public static Color GetFogColor()
	{
		return SkyManager.fogColor;
	}

	public static void SetFogColor(Color c)
	{
		if (SkyManager.fogDebugColor.a > 0f)
		{
			c = SkyManager.fogDebugColor;
		}
		SkyManager.fogColor = c;
	}

	public static float GetFogDensity()
	{
		return SkyManager.fogDensity;
	}

	public static void SetFogDensity(float density)
	{
		if (SkyManager.fogDebugDensity >= 0f)
		{
			density = SkyManager.fogDebugDensity;
		}
		SkyManager.fogDensity = density;
		float num = density - 0.65f;
		if (num < 0f)
		{
			num = 0f;
		}
		SkyManager.fogLightScale = 1f - num * 1.7f;
	}

	public static float GetFogStart()
	{
		return SkyManager.fogStart;
	}

	public static float GetFogEnd()
	{
		return SkyManager.fogEnd;
	}

	public static void SetFogFade(float start, float end)
	{
		float num = 1f;
		World world = GameManager.Instance.World;
		if (world != null)
		{
			num = (world.GetPrimaryPlayer().bPlayingSpawnIn ? 1f : 0.01f);
		}
		SkyManager.fogStart = Mathf.Lerp(SkyManager.fogStart, start, num);
		SkyManager.fogEnd = Mathf.Lerp(SkyManager.fogEnd, end, num);
		if (SkyManager.fogDebugDensity >= 0f)
		{
			if (SkyManager.fogDebugStart > -1000f)
			{
				SkyManager.fogStart = SkyManager.fogDebugStart;
			}
			if (SkyManager.fogDebugEnd > -1000f)
			{
				SkyManager.fogEnd = SkyManager.fogDebugEnd;
			}
		}
	}

	public static void SetFogDebug(float density = -1f, float start = -3.4028235E+38f, float end = -3.4028235E+38f)
	{
		SkyManager.fogDebugDensity = density;
		SkyManager.fogDebugStart = start;
		SkyManager.fogDebugEnd = end;
		SkyManager.SetFogDensity(0f);
	}

	public static void SetFogDebugColor(Color color = default(Color))
	{
		SkyManager.fogDebugColor = color;
		SkyManager.SetFogColor(Color.gray);
	}

	public static void Reset()
	{
		SkyManager.bNeedsReset = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearStatics()
	{
		SkyManager.random = null;
		SkyManager.fogLightScale = 1f;
		SkyManager.weatherLightScale = 1f;
		SkyManager.cloudTransition = 1f;
		SkyManager.cloudMainTexOld = null;
		SkyManager.cloudBlendTex = null;
		SkyManager.parent = null;
		SkyManager.sunLightT = null;
		SkyManager.sunLight = null;
		SkyManager.moonLightT = null;
		SkyManager.moonLight = null;
		SkyManager.moonSpriteT = null;
		SkyManager.mainCamera = null;
		SkyManager.moonSpriteMat = null;
		SkyManager.cloudsSphere = null;
		SkyManager.cloudsSphereMtrl = null;
		SkyManager.atmosphereSphere = null;
		SkyManager.atmosphereMtrl = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		SkyManager.random = GameRandomManager.Instance.CreateGameRandom();
		RenderSettings.fog = true;
		RenderSettings.fogMode = 2;
		this.sunAxis = Vector3.forward;
		this.sunStartV = Quaternion.AngleAxis(20f, Vector3.up) * Vector3.left;
		this.moonStartV = Quaternion.AngleAxis(35f, Vector3.up) * Vector3.right;
		this.starAxis = Vector3.forward;
		if (SkyManager.parent == null)
		{
			SkyManager.parent = GameObject.Find("SkySystem(Clone)");
			if (SkyManager.parent == null)
			{
				return;
			}
		}
		if (SkyManager.sunLightT == null)
		{
			Transform transform = SkyManager.parent.transform.Find("SunLight");
			if (transform != null)
			{
				SkyManager.sunLightT = transform;
			}
		}
		if (SkyManager.sunLight == null && SkyManager.sunLightT != null)
		{
			SkyManager.sunLight = SkyManager.sunLightT.transform.GetComponent<Light>();
		}
		if (SkyManager.moonLightT == null)
		{
			SkyManager.moonLightT = SkyManager.parent.transform.Find("MoonLight");
			if (SkyManager.moonLightT)
			{
				SkyManager.moonLight = SkyManager.moonLightT.GetComponent<Light>();
			}
		}
		SkyManager.GetMaterialAndTransform("MoonSprite", ref SkyManager.moonSpriteT, ref SkyManager.moonSpriteMat);
		SkyManager.GetMaterialAndTransform("CloudsSphere", ref SkyManager.cloudsSphere, ref SkyManager.cloudsSphereMtrl);
		SkyManager.GetMaterialAndTransform("AtmosphereSphere", ref SkyManager.atmosphereSphere, ref SkyManager.atmosphereMtrl);
		MeshFilter component = SkyManager.moonSpriteT.GetComponent<MeshFilter>();
		if (component != null)
		{
			Mesh mesh = component.mesh;
			if (mesh != null)
			{
				mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000000f);
			}
		}
		if (SkyManager.cloudsSphereMtrl)
		{
			SkyManager.cloudsSphereMtrl.SetFloat("_CloudSpeed", this.cloudSpeed);
		}
		if (SkyManager.mainCamera == null)
		{
			this.GetMainCamera();
		}
		this.OnShadowDistanceChanged(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance));
		SkyManager.bloodmoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		ValueTuple<int, int> valueTuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
		SkyManager.duskTime = valueTuple.Item1;
		SkyManager.dawnTime = valueTuple.Item2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetMaterialAndTransform(string objectName, ref Transform trans, ref Material mtrl)
	{
		Transform transform = SkyManager.parent.transform.Find(objectName);
		if (trans == null)
		{
			trans = transform;
		}
		if (mtrl == null)
		{
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			mtrl = component.material;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetMainCamera()
	{
		SkyManager.mainCamera = Camera.main;
	}

	public static bool IsDark()
	{
		if (SkyManager.isAllTimeNight)
		{
			return true;
		}
		if (SkyManager.isAllTimeDay)
		{
			return false;
		}
		float num = SkyManager.TimeOfDay();
		return num < (float)SkyManager.dawnTime || num > (float)SkyManager.duskTime;
	}

	public static float GetDawnTime()
	{
		return (float)SkyManager.dawnTime;
	}

	public static float GetDawnTimeAsMinutes()
	{
		return (float)SkyManager.dawnTime / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	public static float GetDuskTime()
	{
		return (float)SkyManager.duskTime;
	}

	public static float GetDuskTimeAsMinutes()
	{
		return (float)SkyManager.duskTime / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float CalcDayPercent()
	{
		if (SkyManager.isAllTimeNight)
		{
			return 1f;
		}
		float num;
		if (SkyManager.worldRotation < 0.5f)
		{
			num = Mathf.Pow(1f - Utils.FastAbs(0.25f - SkyManager.worldRotation) * 4f, 0.6f);
			num = num * 0.68f + 0.5f;
			if (num > 1f)
			{
				num = 1f;
			}
		}
		else
		{
			num = Mathf.Pow(1f - Utils.FastAbs(0.75f - SkyManager.worldRotation) * 4f, 0.6f);
			num = 0.5f - num * 0.68f;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		return num;
	}

	public static void TriggerLightning(Vector3 _position)
	{
		if (!SkyManager.triggerLightning)
		{
			SkyManager.lightningFlashes = SkyManager.random.RandomRange(2, 6);
			float num = Time.time;
			for (int i = 0; i < SkyManager.lightningFlashes; i++)
			{
				float num2 = SkyManager.random.RandomRange(0.1f, 0.21f);
				num += num2;
				SkyManager.lightningEndTimes[i] = num;
			}
			SkyManager.lightningIndex = -1;
			SkyManager.lightningDir.x = SkyManager.random.RandomFloat * 360f;
			SkyManager.lightningDir.y = (float)SkyManager.random.RandomRange(8, 20);
			SkyManager.triggerLightning = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSunMoonAngles()
	{
		if (!SkyManager.sunLight)
		{
			return;
		}
		if (!SkyManager.moonLight)
		{
			return;
		}
		if (Time.time - SkyManager.worldRotationTime >= 0.2f || SkyManager.bUpdateSunMoonNow)
		{
			bool flag = SkyManager.IsBloodMoonVisible();
			if (SkyManager.isAllTimeNight && !flag)
			{
				SkyManager.worldRotationTarget = 0.75f;
			}
			else if (SkyManager.isAllTimeDay && !flag)
			{
				SkyManager.worldRotationTarget = 0.25f;
			}
			else
			{
				SkyManager.worldRotationTime = Time.time;
				SkyManager.bUpdateSunMoonNow = false;
				float num = SkyManager.TimeOfDay();
				if (num >= (float)SkyManager.dawnTime && num < (float)SkyManager.duskTime)
				{
					SkyManager.worldRotationTarget = (num - (float)SkyManager.dawnTime) / (float)(SkyManager.duskTime - SkyManager.dawnTime);
				}
				else
				{
					float num2 = (float)(24 - SkyManager.duskTime);
					float num3 = num2 + (float)SkyManager.dawnTime;
					if (num < (float)SkyManager.dawnTime)
					{
						SkyManager.worldRotationTarget = (num2 + num) / num3;
					}
					else
					{
						SkyManager.worldRotationTarget = (num - (float)SkyManager.duskTime) / num3;
					}
					SkyManager.worldRotationTarget += 1f;
				}
				SkyManager.worldRotationTarget *= 0.5f;
				SkyManager.worldRotationTarget = Mathf.Clamp01(SkyManager.worldRotationTarget);
			}
		}
		float num4 = SkyManager.worldRotationTarget - SkyManager.worldRotation;
		float num5 = SkyManager.worldRotationTarget;
		if (num4 < -0.5f)
		{
			num5 += 1f;
		}
		else if (num4 > 0.5f)
		{
			num5 -= 1f;
		}
		SkyManager.worldRotation = Mathf.Lerp(SkyManager.worldRotation, num5, 0.05f);
		if (SkyManager.worldRotation < 0f)
		{
			SkyManager.worldRotation += 1f;
		}
		else if (SkyManager.worldRotation >= 1f)
		{
			SkyManager.worldRotation -= 1f;
		}
		SkyManager.dayPercent = SkyManager.CalcDayPercent();
		float num6 = SkyManager.worldRotation * 360f;
		SkyManager.sunDirV = Quaternion.AngleAxis(num6, this.sunAxis) * this.sunStartV;
		SkyManager.moonLightRot = Quaternion.LookRotation(Quaternion.AngleAxis(num6, this.sunAxis) * this.moonStartV);
		float num7 = SkyManager.worldRotation * 360f;
		if (SkyManager.sunIntensity >= 0.001f)
		{
			if (num7 < 14f)
			{
				num7 = 14f;
			}
			if (num7 > 166f)
			{
				num7 = 166f;
			}
			Vector3 eulerAngles = Quaternion.LookRotation(Quaternion.AngleAxis(num7, this.sunAxis) * this.sunStartV).eulerAngles;
			SkyManager.sunLightT.localEulerAngles = eulerAngles;
			SkyManager.sunLight.shadowStrength = 1f;
			SkyManager.sunLight.shadows = ((SkyManager.sunIntensity > 0f) ? 2 : 0);
			SkyManager.moonLight.enabled = false;
		}
		else if (SkyManager.moonLightColor.grayscale > 0f)
		{
			if (num7 < 166f)
			{
				num7 = 166f;
			}
			if (num7 > 346f)
			{
				num7 = 346f;
			}
			Vector3 eulerAngles2 = Quaternion.LookRotation(Quaternion.AngleAxis(num7, this.sunAxis) * this.moonStartV).eulerAngles;
			SkyManager.moonLightT.localEulerAngles = eulerAngles2;
			float num8 = SkyManager.fogLightScale * SkyManager.moonBright;
			float num9 = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness) * 2f;
			num8 *= Utils.FastLerp(0.2f, 1f, num9);
			SkyManager.moonLight.intensity = num8;
			SkyManager.moonLight.color = SkyManager.moonLightColor;
			SkyManager.moonLight.shadowStrength = 1f;
			SkyManager.moonLight.shadows = ((num8 > 0f) ? 2 : 0);
			SkyManager.moonLight.enabled = true;
		}
		else
		{
			SkyManager.moonLight.enabled = false;
		}
		SkyManager.sunMoonDirV = SkyManager.sunDirV;
		if (SkyManager.sunIntensity < 0.001f)
		{
			SkyManager.sunMoonDirV = SkyManager.moonLightRot * Vector3.forward;
		}
		if (!GameManager.IsDedicatedServer && SkyManager.mainCamera)
		{
			if (SkyManager.moonSpriteT)
			{
				Transform transform = SkyManager.mainCamera.transform;
				Vector3 vector = SkyManager.moonLightRot * Vector3.forward * -45000f + transform.position;
				SkyManager.moonSpriteT.SetPositionAndRotation(vector, Quaternion.LookRotation(vector, Vector3.up));
				float num10 = 6857.143f;
				if (SkyManager.IsBloodMoonVisible())
				{
					num10 *= 1.3f;
				}
				SkyManager.moonSpriteT.localScale = new Vector3(num10, num10, num10);
			}
			this.UpdateSunShaftSettings();
		}
		SkyManager.atmosphereSphere.Rotate(this.starAxis, SkyManager.worldRotation * 0.004f);
		if (this.bUpdateShaders && SkyManager.cloudsSphereMtrl)
		{
			SkyManager.cloudsSphereMtrl.SetVector("_SunDir", SkyManager.sunDirV);
			SkyManager.cloudsSphereMtrl.SetVector("_SunMoonDir", SkyManager.sunMoonDirV);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSunShaftSettings()
	{
		Vector3 vector = SkyManager.GetSunDirection();
		if (vector.y <= 0.09f)
		{
			SkyManager.sunShaftSettings.sunShaftIntensity = 0.1f + SkyManager.GetFogDensity() * 0.5f;
			SkyManager.sunShaftSettings.sunColor = SkyManager.GetSunLightColor();
			SkyManager.sunShaftSettings.sunThreshold = new Color(0.87f, 0.74f, 0.65f, 1f);
		}
		else
		{
			vector = SkyManager.GetMoonDirection();
			SkyManager.sunShaftSettings.sunShaftIntensity = 0.06f + SkyManager.GetFogDensity() * 0.85f;
			SkyManager.sunShaftSettings.sunColor = SkyManager.GetMoonLightColor();
			SkyManager.sunShaftSettings.sunThreshold = new Color(0.8f, 0.6f, 0.6f, 1f);
		}
		SkyManager.sunShaftSettings.sunPosition = vector * -100000f;
	}

	public static SunShaftsEffect.SunSettings GetSunShaftSettings()
	{
		return SkyManager.sunShaftSettings;
	}

	public static float GetLuma(Color color)
	{
		return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool VerifyCamera()
	{
		if (GameManager.IsDedicatedServer)
		{
			return true;
		}
		if (SkyManager.mainCamera == null)
		{
			this.GetMainCamera();
			if (SkyManager.mainCamera == null)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool VerifyValidMaterials()
	{
		return !(SkyManager.atmosphereMtrl == null) && !(SkyManager.cloudsSphere == null) && !(SkyManager.atmosphereSphere == null) && !(SkyManager.moonLight == null) && !(SkyManager.sunLight == null) && !(SkyManager.moonSpriteT == null) && !(SkyManager.moonSpriteMat == null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetIfNeeded()
	{
		if (SkyManager.bNeedsReset)
		{
			this.ClearStatics();
			this.Init();
			SkyManager.bNeedsReset = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFogShader()
	{
		this.fogParams.x = SkyManager.GetFogStart();
		this.fogParams.y = SkyManager.GetFogEnd();
		this.fogParams.z = 1f;
		this.fogParams.w = Mathf.Pow(SkyManager.GetFogDensity(), 2f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateShaderGlobals()
	{
		Color color = SkyManager.GetFogColor();
		float num = SkyManager.GetFogDensity();
		num *= num * num;
		RenderSettings.fogDensity = num;
		RenderSettings.fogColor = color;
		Shader.SetGlobalVector("_FogParams", new Vector4(num, this.fogParams.x, this.fogParams.y, 0f));
		Shader.SetGlobalColor("_FogColor", color.linear);
		Shader.SetGlobalVector("SunColor", SkyManager.sunLight.color);
		Shader.SetGlobalVector("FogColor", color);
		Shader.SetGlobalFloat("_HighResViewDistance", (float)GameUtils.GetViewDistance() * 16f);
		Shader.SetGlobalFloat("_DayPercent", SkyManager.dayPercent);
	}

	public void Update()
	{
		if (SkyManager.parent == null)
		{
			this.Init();
			if (SkyManager.parent == null)
			{
				return;
			}
		}
		SkyManager.sMaxSunIntensity = this.maxSunIntensity;
		if (SkyManager.mainCamera)
		{
			float num = SkyManager.fogDensity + 0.001f;
			num *= SkyManager.fogDensity * SkyManager.fogDensity;
			SkyManager.mainCamera.farClipPlane = Utils.FastClamp(6f / num + 400f, 200f, 2800f);
		}
		int num2 = Time.frameCount & 1;
		this.bUpdateShaders = num2 == 0 || SkyManager.triggerLightning;
		if (!this.VerifyCamera())
		{
			return;
		}
		this.ResetIfNeeded();
		this.UpdateSunMoonAngles();
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		float magnitude = (SkyManager.parent.transform.position - SkyManager.mainCamera.transform.position).magnitude;
		if (magnitude < 0.001f)
		{
			this.bMovingSky = false;
			this.targetSkyPosition = SkyManager.mainCamera.transform.position;
		}
		else if (!this.bMovingSky && magnitude > 5f)
		{
			this.bMovingSky = true;
			this.targetSkyPosition = SkyManager.mainCamera.transform.position;
		}
		if (this.bMovingSky)
		{
			SkyManager.parent.transform.position = Vector3.Lerp(SkyManager.parent.transform.position, this.targetSkyPosition, Mathf.Clamp01(Time.deltaTime * 10f));
		}
		this.UpdateFogShader();
		if (!this.bUpdateShaders)
		{
			return;
		}
		if (!this.VerifyValidMaterials())
		{
			return;
		}
		SkyManager.sSunFadeHeight = this.sunFadeHeight;
		this.UpdateShaderGlobals();
		bool flag = SkyManager.IsBloodMoonVisible();
		int num3 = 0;
		Color color = new Color(1f, 0.14f, 0.05f) * 1.5f;
		if (!flag)
		{
			num3 = (int)(SkyManager.dayCount + 5.5f) % 7;
			color = Color.Lerp(Color.white, SkyManager.moonLightColor, 0.2f);
		}
		float num4 = SkyManager.sMoonPhases[num3] * 3.1415927f;
		Vector3 vector;
		vector..ctor(-Mathf.Sin(num4), 0f, Mathf.Cos(num4));
		SkyManager.moonSpriteMat.SetVector("_LightDir", vector);
		SkyManager.moonSpriteMat.SetColor("_Color", color);
		SkyManager.moonBright = SkyManager.sMoonBrights[num3];
		float cloudThickness = WeatherManager.GetCloudThickness();
		if (cloudThickness > 45f && GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness) < 0.49f)
		{
			SkyManager.moonBright *= Utils.FastLerp(1f, 0.15f, (cloudThickness - 45f) / 55f);
		}
		float num5 = ((Mathf.Pow(SkyManager.moonLightColor.grayscale, 0.45f) - 0.5f) * 0.5f + 0.5f) * SkyManager.moonBright;
		float num6 = Mathf.Max(SkyManager.sunIntensity, num5);
		if (WeatherManager.currentWeather != null)
		{
			SkyManager.starIntensity = 1f - WeatherManager.currentWeather.CloudThickness() * 0.01f;
		}
		if (this.bUpdateShaders)
		{
			SkyManager.atmosphereMtrl.SetColor("_SkyColor", SkyManager.GetSkyColor());
			SkyManager.atmosphereMtrl.SetFloat("_Stars", SkyManager.starIntensity);
			SkyManager.atmosphereMtrl.SetVector("_SunDir", SkyManager.sunDirV);
			SkyManager.cloudsSphereMtrl.SetFloat("_CloudTransition", SkyManager.cloudTransition);
			SkyManager.cloudsSphereMtrl.SetFloat("_LightIntensity", num6);
			SkyManager.cloudsSphereMtrl.SetColor("_SkyColor", SkyManager.GetSkyColor());
			SkyManager.cloudsSphereMtrl.SetColor("_SunColor", SkyManager.sunLight.color);
			SkyManager.cloudsSphereMtrl.SetVector("_SunDir", SkyManager.sunDirV);
			SkyManager.cloudsSphereMtrl.SetVector("_SunMoonDir", SkyManager.sunMoonDirV);
			color.a = SkyManager.moonBright * 0.4f;
			SkyManager.cloudsSphereMtrl.SetColor("_MoonColor", color);
		}
		if (SkyManager.cloudMainTex != SkyManager.cloudMainTexOld)
		{
			SkyManager.cloudsSphereMtrl.SetTexture("_CloudMainTex", SkyManager.cloudMainTex);
			SkyManager.cloudMainTexOld = SkyManager.cloudMainTex;
		}
		if (SkyManager.cloudBlendTex != SkyManager.cloudBlendTexOld)
		{
			SkyManager.cloudsSphereMtrl.SetTexture("_CloudBlendTex", SkyManager.cloudBlendTex);
			SkyManager.cloudBlendTexOld = SkyManager.cloudBlendTex;
		}
		if (SkyManager.triggerLightning)
		{
			float deltaTime = Time.deltaTime;
			Light light = SkyManager.moonLight;
			SkyManager.lightningIntensity -= 4f * deltaTime;
			if (SkyManager.lightningIndex < 0 || Time.time >= SkyManager.lightningEndTimes[SkyManager.lightningIndex])
			{
				SkyManager.lightningIndex++;
				SkyManager.lightningIntensity = 0.75f + SkyManager.random.RandomFloat * 0.25f;
				SkyManager.lightningDir.x = SkyManager.lightningDir.x + SkyManager.random.RandomRange(-20f, 20f);
				SkyManager.lightningDirVel.x = SkyManager.random.RandomRange(-50f, 50f);
				SkyManager.lightningDir.y = SkyManager.lightningDir.y + (float)SkyManager.random.RandomRange(-8, 8);
			}
			if (SkyManager.lightningIndex >= SkyManager.lightningFlashes)
			{
				SkyManager.triggerLightning = false;
				SkyManager.cloudsSphereMtrl.SetFloat("_Lightning", 0f);
			}
			else
			{
				SkyManager.lightningIntensity = Utils.FastMax(0f, SkyManager.lightningIntensity);
				SkyManager.cloudsSphereMtrl.SetFloat("_Lightning", SkyManager.lightningIntensity);
				SkyManager.lightningDir += SkyManager.lightningDirVel * deltaTime;
				float num7 = SkyManager.lightningDir.x * 0.017453292f;
				Vector3 vector2;
				vector2.x = Mathf.Sin(num7);
				vector2.y = Mathf.Sin(SkyManager.lightningDir.y * 0.017453292f) * 1.5f;
				vector2.z = Mathf.Cos(num7);
				SkyManager.cloudsSphereMtrl.SetVector("_LightningDir", vector2.normalized);
				Transform transform = GameManager.Instance.World.GetPrimaryPlayer().transform;
				Transform transform2 = light.transform;
				Vector3 vector3 = vector2 * 200f;
				vector3.y = 300f;
				transform2.position = transform.position + vector3;
				transform2.LookAt(transform);
				light.color = Color.white;
				light.intensity = SkyManager.lightningIntensity * 0.9f;
				light.shadows = 1;
				light.shadowStrength = 1f;
				light.enabled = true;
			}
		}
		if (SkyManager.frustumShadows == null)
		{
			SkyManager.frustumShadows = SkyManager.mainCamera.GetComponent<NGSS_FrustumShadows_7DTD>();
		}
		if (SkyManager.frustumShadows != null)
		{
			SkyManager.frustumShadows.mainShadowsLight = ((!SkyManager.IsDark()) ? SkyManager.sunLight : SkyManager.moonLight);
		}
		SkyManager.WasBloodMoon = flag;
	}

	public static float GetSunAngle()
	{
		return SkyManager.sunDirV.y;
	}

	public static Vector3 GetSunDirection()
	{
		return SkyManager.sunDirV;
	}

	public static float GetSunPercent()
	{
		return -SkyManager.sunDirV.y;
	}

	public static Vector3 GetSunLightDirection()
	{
		if (!(SkyManager.sunLight == null))
		{
			return SkyManager.sunLightT.forward;
		}
		return Vector3.down;
	}

	public static float GetSunIntensity()
	{
		return SkyManager.sunIntensity;
	}

	public static Color GetSunLightColor()
	{
		if (SkyManager.sunLight)
		{
			return SkyManager.sunLight.color;
		}
		return Color.black;
	}

	public static void SetSunColor(Color color)
	{
		if (SkyManager.sunLight != null)
		{
			SkyManager.sunLight.color = color;
		}
	}

	public static void SetSunIntensity(float _intensity)
	{
		SkyManager.sunIntensity = _intensity;
		float sunAngle = SkyManager.GetSunAngle();
		if (sunAngle >= -SkyManager.sSunFadeHeight)
		{
			SkyManager.sunIntensity = -sunAngle * 10f * SkyManager.sunIntensity * (float)((sunAngle < 0f) ? 1 : 0);
		}
		SkyManager.sunIntensity = Mathf.Clamp(SkyManager.sunIntensity, 0f, SkyManager.sMaxSunIntensity);
		SkyManager.sunIntensity *= 1.5f;
		if (SkyManager.sunLight)
		{
			SkyManager.sunLight.intensity = SkyManager.sunIntensity * SkyManager.fogLightScale * SkyManager.weatherLightScale;
		}
	}

	public static void SetWeatherLightScale(float _scale)
	{
		SkyManager.weatherLightScale = _scale;
	}

	public static float GetMoonAmbientScale(float add, float mpy)
	{
		return Utils.FastLerp(add + SkyManager.moonBright * mpy, 1f, SkyManager.dayPercent * 3.030303f);
	}

	public static float GetMoonBrightness()
	{
		return SkyManager.moonLightColor.grayscale * SkyManager.moonBright;
	}

	public static Color GetMoonLightColor()
	{
		return SkyManager.moonLightColor;
	}

	public static Vector3 GetMoonDirection()
	{
		return SkyManager.moonLightRot * Vector3.forward;
	}

	public static void SetMoonLightColor(Color color)
	{
		SkyManager.moonLightColor = color;
	}

	public static float GetWorldRotation()
	{
		return SkyManager.worldRotation;
	}

	public static SkyManager skyManager;

	public static float dayCount;

	public static bool indoorFogOn = true;

	public static bool isAllTimeDay = false;

	public static bool isAllTimeNight = false;

	public static Material atmosphereMtrl;

	public static Transform atmosphereSphere;

	public static Material cloudsSphereMtrl;

	public static Transform cloudsSphere;

	public static bool bUpdateSunMoonNow = false;

	public static float sSunFadeHeight = 0.1f;

	public static float dayPercent;

	public static GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float sMaxSunIntensity = 0.7f;

	public float maxSunIntensity = 0.7f;

	public float sunHeight = 0.1f;

	public float moonHeight = 0.095f;

	public float sunFadeHeight = 0.1f;

	public float cloudSpeed = 0.05f;

	public float ShowFogDensity;

	public Color ShowSkyColor;

	public Color ShowFogColor;

	public float maxFarClippingPlane = 1500f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int dawnTime = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int duskTime = 22;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static ulong timeOfDay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int bloodmoonDay = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float cloudTransition = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogLightScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogStart = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogEnd = 80f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogDebugDensity = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogDebugStart;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogDebugEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color fogDebugColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float worldRotationTime = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float worldRotation = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float worldRotationTarget = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float sunIntensity = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float starIntensity = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float weatherLightScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool bNeedsReset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color SkyColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color fogColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Transform sunLightT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Light sunLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector3 sunDirV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector3 sunMoonDirV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly float[] sMoonPhases = new float[] { 0.05f, 0.35f, 0.55f, 0.7f, 1.4f, 1.63f, 1.82f };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly float[] sMoonBrights = new float[] { 1f, 0.65f, 0.45f, 0.25f, 0.4f, 0.6f, 0.9f };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Transform moonLightT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Light moonLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Transform moonSpriteT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Material moonSpriteMat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float moonBright;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color moonLightColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Quaternion moonLightRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture cloudMainTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture cloudMainTexOld;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture cloudBlendTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture cloudBlendTexOld;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameObject parent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Camera mainCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static NGSS_FrustumShadows_7DTD frustumShadows;

	public static bool WasBloodMoon = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWorldRotationUpdateFreq = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cStarRotationSpeed = 0.004f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bUpdateShaders;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector4 fogParams;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 sunAxis;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 sunStartV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 moonStartV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 starAxis;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool triggerLightning;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int lightningFlashes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] lightningEndTimes = new float[] { 1f, 2f, 3f, 4f, 5f };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int lightningIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 lightningDir;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 lightningDirVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float lightningIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static SunShaftsEffect.SunSettings sunShaftSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 targetSkyPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bMovingSky = true;
}

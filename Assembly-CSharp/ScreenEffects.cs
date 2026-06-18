using System;
using System.Collections.Generic;
using UniLinq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ScreenEffects : MonoBehaviour
{
	public static ScreenEffects Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.Init();
		this.cameraRef = base.GetComponent<Camera>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.prevViewProjMatrix = GL.GetGPUProjectionMatrix(this.cameraRef.nonJitteredProjectionMatrix, false) * this.cameraRef.worldToCameraMatrix;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		if (ScreenEffects.Instance != null)
		{
			Log.Warning("ScreenEffects instance already exists!");
		}
		else
		{
			ScreenEffects.Instance = this;
		}
		Material[] array = Resources.LoadAll<Material>("ScreenEffects/");
		for (int i = 0; i < array.Length; i++)
		{
			string name = array[i].name;
			this.loadedEffects.Add(new ScreenEffects.ScreenEffect
			{
				Name = name,
				Material = Object.Instantiate<Material>(array[i]),
				TargetIntensity = 0f,
				Intensity = 0f
			});
		}
		this.SortEffects();
		this.particles = new NativeArray<ParticleSystem.Particle>(64, 4, 1);
	}

	public void ResetEffects()
	{
		Material[] array = Resources.LoadAll<Material>("ScreenEffects/");
		for (int i = 0; i < array.Length; i++)
		{
			string name = array[i].name;
			for (int j = 0; j < this.loadedEffects.Count; j++)
			{
				ScreenEffects.ScreenEffect screenEffect = this.loadedEffects[j];
				if (screenEffect.Name == name)
				{
					if (screenEffect.Material != null)
					{
						Object.Destroy(screenEffect.Material);
					}
					screenEffect.Material = Object.Instantiate<Material>(array[i]);
				}
			}
		}
		this.SortEffects();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortEffects()
	{
		this.loadedEffects = this.loadedEffects.OrderBy((ScreenEffects.ScreenEffect se) => se.Material.renderQueue).ToList<ScreenEffects.ScreenEffect>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		for (int i = 0; i < this.loadedEffects.Count; i++)
		{
			Object.Destroy(this.loadedEffects[i].Material);
		}
		this.particles.Dispose();
		if (ScreenEffects.Instance == this)
		{
			ScreenEffects.Instance = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender()
	{
		Matrix4x4 matrix4x = GL.GetGPUProjectionMatrix(this.cameraRef.nonJitteredProjectionMatrix, false) * this.cameraRef.worldToCameraMatrix;
		Shader.SetGlobalMatrix("_PrevViewProjMatrix", this.prevViewProjMatrix);
		Shader.SetGlobalMatrix("_CurrViewProjMatrix", matrix4x);
		Shader.SetGlobalMatrix("_CurrViewProjMatrix_Inverse", matrix4x.inverse);
		this.prevViewProjMatrix = matrix4x;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		int i = 0;
		while (i < this.activeEffects.Count)
		{
			ScreenEffects.ScreenEffect screenEffect = this.activeEffects[i];
			if (screenEffect == null)
			{
				this.activeEffects.RemoveAt(i);
			}
			else
			{
				float intensity = screenEffect.Intensity;
				if (screenEffect.FadeTime <= 0f)
				{
					screenEffect.Intensity = screenEffect.TargetIntensity;
				}
				else if (screenEffect.TargetIntensity > screenEffect.Intensity)
				{
					screenEffect.Intensity = Utils.FastMin(screenEffect.Intensity + Time.deltaTime / screenEffect.FadeTime, screenEffect.TargetIntensity);
				}
				else if (screenEffect.TargetIntensity < screenEffect.Intensity)
				{
					screenEffect.Intensity = Utils.FastMax(screenEffect.Intensity - Time.deltaTime / screenEffect.FadeTime, screenEffect.TargetIntensity);
				}
				if (screenEffect.Intensity != intensity)
				{
					if (screenEffect.Name == "NightVision")
					{
						world.m_WorldEnvironment.SetNightVision(screenEffect.Intensity);
					}
					if (screenEffect.particleSystems != null)
					{
						for (int j = 0; j < screenEffect.particleSystems.Length; j++)
						{
							ParticleSystem particleSystem = screenEffect.particleSystems[j];
							int num = particleSystem.GetParticles(this.particles);
							for (int k = 0; k < num; k++)
							{
								ParticleSystem.Particle particle = this.particles[k];
								Color32 startColor = particle.startColor;
								startColor.a = (byte)(screenEffect.Intensity * 255f);
								particle.startColor = startColor;
								this.particles[k] = particle;
							}
							particleSystem.SetParticles(this.particles, num);
						}
					}
				}
				if (screenEffect.Intensity == screenEffect.TargetIntensity && screenEffect.Intensity <= 0f)
				{
					this.activeEffects.RemoveAt(i);
					if (screenEffect.particlePrefab)
					{
						Object.Destroy(screenEffect.particlePrefab);
						screenEffect.particlePrefab = null;
					}
				}
				else
				{
					i++;
				}
			}
		}
	}

	public void SetScreenEffect(string _name, float _intensity = 1f, float _fadeTime = 4f)
	{
		ScreenEffects.ScreenEffect screenEffect = this.Find(_name, this.activeEffects);
		if (screenEffect == null)
		{
			if (_intensity <= 0f)
			{
				return;
			}
			screenEffect = this.Find(_name, this.loadedEffects);
			if (screenEffect == null)
			{
				return;
			}
			int renderQueue = screenEffect.Material.renderQueue;
			int num = this.activeEffects.Count;
			for (int i = 0; i < this.activeEffects.Count; i++)
			{
				ScreenEffects.ScreenEffect screenEffect2 = this.activeEffects[i];
				if (screenEffect2 != null && renderQueue <= screenEffect2.Material.renderQueue)
				{
					num = i;
					break;
				}
			}
			this.activeEffects.Insert(num, screenEffect);
			GameObject gameObject = Resources.Load<GameObject>("ScreenEffects/" + _name);
			if (gameObject)
			{
				gameObject = Object.Instantiate<GameObject>(gameObject);
				screenEffect.particlePrefab = gameObject;
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer)
				{
					Transform transform = gameObject.transform;
					transform.SetParent(primaryPlayer.cameraTransform, false);
					transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, 0.12f), Quaternion.identity);
				}
				gameObject.GetComponentsInChildren<ParticleSystem>(this.particleSystems);
				if (this.particleSystems.Count > 0)
				{
					screenEffect.particleSystems = this.particleSystems.ToArray();
					this.particleSystems.Clear();
				}
			}
		}
		screenEffect.TargetIntensity = _intensity;
		screenEffect.FadeTime = _fadeTime;
	}

	public void DisableScreenEffects()
	{
		for (int i = 0; i < this.activeEffects.Count; i++)
		{
			this.DisableScreenEffect(this.activeEffects[i].Name);
		}
	}

	public void DisableScreenEffect(string _name)
	{
		this.SetScreenEffect(_name, 0f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ScreenEffects.ScreenEffect Find(string _name, List<ScreenEffects.ScreenEffect> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			ScreenEffects.ScreenEffect screenEffect = _list[i];
			if (screenEffect != null && screenEffect.Name == _name)
			{
				return screenEffect;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDefaultFadeTime = 4f;

	public List<ScreenEffects.ScreenEffect> loadedEffects = new List<ScreenEffects.ScreenEffect>();

	public List<ScreenEffects.ScreenEffect> activeEffects = new List<ScreenEffects.ScreenEffect>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NativeArray<ParticleSystem.Particle> particles;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Matrix4x4 prevViewProjMatrix;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera cameraRef;

	public class ScreenEffect : PostProcessEffectSubRenderer
	{
		public override void Render(PostProcessRenderContext context)
		{
			this.Material.SetFloat("Intensity", Mathf.Clamp01(this.Intensity));
			context.command.Blit(context.source, context.destination, this.Material);
		}

		public string Name;

		public Material Material;

		public float Intensity;

		public float TargetIntensity;

		public float FadeTime;

		public GameObject particlePrefab;

		public ParticleSystem[] particleSystems;
	}
}

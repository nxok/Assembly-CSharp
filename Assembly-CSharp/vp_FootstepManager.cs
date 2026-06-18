using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FootstepManager : MonoBehaviour
{
	public static vp_FootstepManager[] FootstepManagers
	{
		get
		{
			if (vp_FootstepManager.mIsDirty)
			{
				vp_FootstepManager.mIsDirty = false;
				vp_FootstepManager.m_FootstepManagers = Object.FindObjectsByType<vp_FootstepManager>(0);
				if (vp_FootstepManager.m_FootstepManagers == null)
				{
					vp_FootstepManager.m_FootstepManagers = Resources.FindObjectsOfTypeAll(typeof(vp_FootstepManager)) as vp_FootstepManager[];
				}
			}
			return vp_FootstepManager.m_FootstepManagers;
		}
	}

	public bool IsDirty
	{
		get
		{
			return vp_FootstepManager.mIsDirty;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Player = base.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
		this.m_Camera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
		this.m_Controller = base.transform.root.GetComponentInChildren<vp_FPController>();
		this.m_Audio = base.gameObject.AddComponent<AudioSource>();
	}

	public virtual void SetDirty(bool dirty)
	{
		vp_FootstepManager.mIsDirty = dirty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.m_Camera.BobStepCallback == null)
		{
			vp_FPCamera camera = this.m_Camera;
			camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(this.Footstep));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		vp_FPCamera camera = this.m_Camera;
		camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(this.Footstep));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		vp_FPCamera camera = this.m_Camera;
		camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Remove(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(this.Footstep));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Footstep()
	{
		if (!this.m_Controller.Grounded)
		{
			return;
		}
		if (this.m_Player.GroundTexture.Get() == null && this.m_Player.SurfaceType.Get() == null)
		{
			return;
		}
		if (this.m_Player.SurfaceType.Get() != null)
		{
			this.PlaySound(this.SurfaceTypes[this.m_Player.SurfaceType.Get().SurfaceID]);
			return;
		}
		foreach (vp_FootstepManager.vp_SurfaceTypes vp_SurfaceTypes in this.SurfaceTypes)
		{
			using (List<Texture>.Enumerator enumerator2 = vp_SurfaceTypes.Textures.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current == this.m_Player.GroundTexture.Get())
					{
						this.PlaySound(vp_SurfaceTypes);
						break;
					}
				}
			}
		}
	}

	public virtual void PlaySound(vp_FootstepManager.vp_SurfaceTypes st)
	{
		if (st.Sounds == null || st.Sounds.Count == 0)
		{
			return;
		}
		for (;;)
		{
			this.m_SoundToPlay = st.Sounds[Random.Range(0, st.Sounds.Count)];
			if (this.m_SoundToPlay == null)
			{
				break;
			}
			if (!(this.m_SoundToPlay == this.m_LastPlayedSound) || st.Sounds.Count <= 1)
			{
				goto IL_0068;
			}
		}
		return;
		IL_0068:
		this.m_Audio.pitch = Random.Range(st.RandomPitch.x, st.RandomPitch.y) * Time.timeScale;
		this.m_Audio.clip = this.m_SoundToPlay;
		this.m_Audio.Play();
		this.m_LastPlayedSound = this.m_SoundToPlay;
	}

	public static int GetMainTerrainTexture(Vector3 worldPos, Terrain terrain)
	{
		TerrainData terrainData = terrain.terrainData;
		Vector3 position = terrain.transform.position;
		int num = (int)((worldPos.x - position.x) / terrainData.size.x * (float)terrainData.alphamapWidth);
		int num2 = (int)((worldPos.z - position.z) / terrainData.size.z * (float)terrainData.alphamapHeight);
		float[,,] alphamaps = terrainData.GetAlphamaps(num, num2, 1, 1);
		float[] array = new float[alphamaps.GetUpperBound(2) + 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = alphamaps[0, 0, i];
		}
		float num3 = 0f;
		int num4 = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > num3)
			{
				num4 = j;
				num3 = array[j];
			}
		}
		return num4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static vp_FootstepManager[] m_FootstepManagers;

	public static bool mIsDirty = true;

	public List<vp_FootstepManager.vp_SurfaceTypes> SurfaceTypes = new List<vp_FootstepManager.vp_SurfaceTypes>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPCamera m_Camera;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPController m_Controller;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_Audio;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioClip m_SoundToPlay;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioClip m_LastPlayedSound;

	[Serializable]
	public class vp_SurfaceTypes
	{
		public Vector2 RandomPitch = new Vector2(1f, 1.5f);

		public bool Foldout = true;

		public bool SoundsFoldout = true;

		public bool TexturesFoldout = true;

		public string SurfaceName = "";

		public List<AudioClip> Sounds = new List<AudioClip>();

		public List<Texture> Textures = new List<Texture>();
	}
}

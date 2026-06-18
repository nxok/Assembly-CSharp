using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplashCubes
{
	public WaterSplashCubes()
	{
		if (WaterSplashCubes.instance == null)
		{
			WaterSplashCubes.instance = this;
		}
		WaterSplashCubes.addList = new List<WaterSplashCubes.ParticlePlacement>();
		WaterSplashCubes.removeList = new List<Vector3i>();
		WaterSplashCubes.root = GameObject.Find("WaterSplashes");
		WaterSplashCubes.particleLimiter = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
	}

	public static long MakeKey(int x, int z)
	{
		return (((long)z & 16777215L) << 24) | ((long)x & 16777215L);
	}

	public static object GetSyncRoot()
	{
		return ((ICollection)WaterSplashCubes.splashes.dict).SyncRoot;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject AddParticleEffect(Vector3i pos, BlockFace face, WaterSplashCubes.SplashType type)
	{
		if (WaterSplashCubes.waterFallSplashCubeEffect == null)
		{
			WaterSplashCubes.waterFallSplashCubeEffect = new Object[EnumUtils.Names<WaterSplashCubes.SplashType>().Count];
			WaterSplashCubes.waterFallSplashCubeEffect[0] = Resources.Load("prefabs/WaterFallSplashCube");
			WaterSplashCubes.waterFallSplashCubeEffect[1] = Resources.Load("prefabs/WaterFallSlopeParticles");
			WaterSplashCubes.waterFallSplashCubeEffect[2] = Resources.Load("prefabs/WaterFallAreaParticles");
			WaterSplashCubes.waterFallSplashCubeEffect[3] = Resources.Load("prefabs/WaterFallSplashCube");
		}
		GameObject gameObject = (GameObject)Object.Instantiate(WaterSplashCubes.waterFallSplashCubeEffect[(int)type]);
		WaterSplashCubes.particleCount++;
		switch (face)
		{
		case BlockFace.Top:
			gameObject.transform.Rotate(new Vector3(90f, 0f, 0f));
			break;
		case BlockFace.Bottom:
			gameObject.transform.Rotate(new Vector3(-90f, 0f, 0f));
			break;
		case BlockFace.North:
			gameObject.transform.Rotate(new Vector3(0f, 0f, 0f));
			break;
		case BlockFace.West:
			gameObject.transform.Rotate(new Vector3(0f, -90f, 0f));
			break;
		case BlockFace.South:
			gameObject.transform.Rotate(new Vector3(0f, 180f, 0f));
			break;
		case BlockFace.East:
			gameObject.transform.Rotate(new Vector3(0f, 90f, 0f));
			break;
		}
		gameObject.transform.position = new Vector3((float)pos.x + 0.5f, (float)pos.y + 0.5f, (float)pos.z + 0.5f) - Origin.position;
		gameObject.transform.parent = WaterSplashCubes.root.transform;
		if (WaterSplashCubes.particleLimiter < 1f && WaterSplashCubes.particleCount % Mathf.CeilToInt((1f - WaterSplashCubes.particleLimiter) * 6f) != 0)
		{
			gameObject.SetActive(false);
		}
		return gameObject;
	}

	public static void Update()
	{
		if (WaterSplashCubes.removeList == null)
		{
			return;
		}
		if (WaterSplashCubes.addList == null)
		{
			return;
		}
		List<Vector3i> list = WaterSplashCubes.removeList;
		lock (list)
		{
			for (int i = 0; i < WaterSplashCubes.removeList.Count; i++)
			{
				Vector3i vector3i = WaterSplashCubes.removeList[i];
				int x = vector3i.x;
				int y = vector3i.y;
				int z = vector3i.z;
				long num = WaterSplashCubes.MakeKey(x, z);
				object obj = WaterSplashCubes.GetSyncRoot();
				lock (obj)
				{
					if (WaterSplashCubes.splashes.dict.ContainsKey(num))
					{
						DictionaryList<int, GameObject> dictionaryList = WaterSplashCubes.splashes.dict[num];
						if (dictionaryList.dict.ContainsKey(y))
						{
							Object.DestroyImmediate(dictionaryList.dict[y]);
							dictionaryList.Remove(y);
						}
					}
				}
			}
			WaterSplashCubes.removeList.Clear();
		}
		List<WaterSplashCubes.ParticlePlacement> list2 = WaterSplashCubes.addList;
		lock (list2)
		{
			for (int j = 0; j < WaterSplashCubes.addList.Count; j++)
			{
				WaterSplashCubes.ParticlePlacement particlePlacement = WaterSplashCubes.addList[j];
				int x2 = particlePlacement.pos.x;
				int y2 = particlePlacement.pos.y;
				int z2 = particlePlacement.pos.z;
				long num2 = WaterSplashCubes.MakeKey(x2, z2);
				object obj = WaterSplashCubes.GetSyncRoot();
				lock (obj)
				{
					if (!WaterSplashCubes.splashes.dict.ContainsKey(num2))
					{
						DictionaryList<int, GameObject> dictionaryList2 = new DictionaryList<int, GameObject>();
						dictionaryList2.Add(y2, WaterSplashCubes.AddParticleEffect(particlePlacement.pos, particlePlacement.dir, particlePlacement.type));
						WaterSplashCubes.splashes.Add(num2, dictionaryList2);
					}
					else
					{
						DictionaryList<int, GameObject> dictionaryList3 = WaterSplashCubes.splashes.dict[num2];
						if (!dictionaryList3.dict.ContainsKey(y2))
						{
							dictionaryList3.Add(y2, WaterSplashCubes.AddParticleEffect(particlePlacement.pos, particlePlacement.dir, particlePlacement.type));
						}
					}
				}
			}
			WaterSplashCubes.addList.Clear();
		}
		WaterSplashCubes.CleanUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanUp()
	{
		if (Time.time > WaterSplashCubes.cleanUpTimer + 0.15f)
		{
			WaterSplashCubes.cleanUpTimer = Time.time;
			if (WaterSplashCubes.root == null)
			{
				return;
			}
			if (WaterSplashCubes.root.transform == null)
			{
				return;
			}
			if (GameManager.Instance == null)
			{
				return;
			}
			World world = GameManager.Instance.World;
			if (world == null)
			{
				return;
			}
			bool flag = false;
			if (WaterSplashCubes.currentCleanupIndex >= WaterSplashCubes.root.transform.childCount)
			{
				WaterSplashCubes.currentCleanupIndex = WaterSplashCubes.root.transform.childCount;
				flag = true;
			}
			int num = 0;
			while (num < WaterSplashCubes.root.transform.childCount && num < WaterSplashCubes.currentCleanupIndex)
			{
				Transform child = WaterSplashCubes.root.transform.GetChild(num);
				if (child != null)
				{
					Vector3i vector3i = new Vector3i((int)(child.position.x - 0.5f), (int)(child.position.y - 0.5f), (int)(child.position.z - 0.5f));
					if (!world.IsChunkAreaLoaded(vector3i.x, vector3i.y, vector3i.z))
					{
						WaterSplashCubes.RemoveSplashAt(vector3i.x, vector3i.y, vector3i.z);
					}
					else if (!world.IsWater(vector3i) || !world.IsAir((int)(child.position.x - 0.5f), (int)(child.position.y - 0.5f) + 1, (int)(child.position.z - 0.5f)))
					{
						WaterSplashCubes.RemoveSplashAt(vector3i.x, vector3i.y, vector3i.z);
					}
				}
				num++;
			}
			WaterSplashCubes.currentCleanupIndex += 15;
			if (flag)
			{
				WaterSplashCubes.currentCleanupIndex = 0;
			}
		}
	}

	public static void RemoveSplashAt(int _x, int _y, int _z)
	{
		if (WaterSplashCubes.removeList == null)
		{
			return;
		}
		List<Vector3i> list = WaterSplashCubes.removeList;
		lock (list)
		{
			WaterSplashCubes.removeList.Add(new Vector3i(_x, _y, _z));
		}
	}

	public static void AddSplashAt(int _x, int _y, int _z, BlockFace _dir, WaterSplashCubes.SplashType _type)
	{
		if (WaterSplashCubes.particleLimiter <= 0f)
		{
			return;
		}
		if (WaterSplashCubes.addList == null)
		{
			return;
		}
		List<WaterSplashCubes.ParticlePlacement> list = WaterSplashCubes.addList;
		lock (list)
		{
			WaterSplashCubes.ParticlePlacement particlePlacement = new WaterSplashCubes.ParticlePlacement(new Vector3i(_x, _y, _z), _dir, _type);
			WaterSplashCubes.addList.Add(particlePlacement);
		}
	}

	public static void Clear()
	{
		object syncRoot = WaterSplashCubes.GetSyncRoot();
		lock (syncRoot)
		{
			for (int i = 0; i < WaterSplashCubes.splashes.Count; i++)
			{
				DictionaryList<int, GameObject> dictionaryList = WaterSplashCubes.splashes.list[i];
				for (int j = 0; j < dictionaryList.list.Count; j++)
				{
					Object.DestroyImmediate(dictionaryList.list[j]);
				}
				dictionaryList.Clear();
			}
			WaterSplashCubes.splashes.Clear();
		}
	}

	public static WaterSplashCubes instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryList<long, DictionaryList<int, GameObject>> splashes = new DictionaryList<long, DictionaryList<int, GameObject>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject root;

	public static List<WaterSplashCubes.ParticlePlacement> addList;

	public static List<Vector3i> removeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int checkListNum = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Object[] waterFallSplashCubeEffect = null;

	public static float particleLimiter = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int particleCount = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float cleanUpTimer = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int currentCleanupIndex = 15;

	public enum SplashType
	{
		Splash,
		Slope,
		Area,
		Mist
	}

	public class ParticlePlacement
	{
		public ParticlePlacement(Vector3i _pos, BlockFace _dir, WaterSplashCubes.SplashType _type)
		{
			this.pos = _pos;
			this.dir = _dir;
			this.type = _type;
		}

		public Vector3i pos;

		public BlockFace dir;

		public WaterSplashCubes.SplashType type;
	}
}

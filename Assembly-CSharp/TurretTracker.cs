using System;
using System.Collections.Generic;
using System.IO;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TurretTracker
{
	public static TurretTracker Instance
	{
		get
		{
			return TurretTracker.instance;
		}
	}

	public static void Init()
	{
		TurretTracker.instance = new TurretTracker();
		TurretTracker.instance.Load();
	}

	public void AddTrackedTurret(EntityTurret _turret)
	{
		if (!_turret)
		{
			Log.Error("{0} AddTrackedTurret null", new object[] { base.GetType() });
			return;
		}
		if (this.turretsUnloaded.Contains(_turret.entityId))
		{
			this.turretsUnloaded.Remove(_turret.entityId);
		}
		if (!this.turretsActive.Contains(_turret))
		{
			this.turretsActive.Add(_turret);
			this.TriggerSave();
		}
	}

	public void RemoveTrackedTurret(EntityTurret _turret, EnumRemoveEntityReason _reason)
	{
		this.turretsActive.Remove(_turret);
		if (_reason == EnumRemoveEntityReason.Unloaded)
		{
			this.turretsUnloaded.Add(_turret.entityId);
		}
		this.TriggerSave();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleCount>().Setup(), false, -1, -1, -1, null, 192, false);
	}

	public void TriggerSave()
	{
		this.saveTime = Mathf.Min(this.saveTime, 10f);
	}

	public void Update()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null || world.Players == null || world.Players.Count == 0)
		{
			return;
		}
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		this.saveTime -= Time.deltaTime;
		if (this.saveTime <= 0f && (this.saveThread == null || this.saveThread.HasTerminated()))
		{
			this.saveTime = 120f;
			this.Save();
		}
	}

	public static void Cleanup()
	{
		if (TurretTracker.instance != null)
		{
			TurretTracker.instance.SaveAndClear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveAndClear()
	{
		this.WaitOnSave();
		this.Save();
		this.WaitOnSave();
		this.turretsActive.Clear();
		this.turretsUnloaded.Clear();
		TurretTracker.instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WaitOnSave()
	{
		if (this.saveThread != null)
		{
			this.saveThread.WaitForEnd(30);
			this.saveThread = null;
		}
	}

	public void Load()
	{
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "turrets.dat");
		if (SdFile.Exists(text))
		{
			try
			{
				using (Stream stream = SdFile.OpenRead(text))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(stream);
						this.read(pooledBinaryReader);
					}
				}
			}
			catch (Exception)
			{
				text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "turrets.dat.bak");
				if (SdFile.Exists(text))
				{
					using (Stream stream2 = SdFile.OpenRead(text))
					{
						using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader2.SetBaseStream(stream2);
							this.read(pooledBinaryReader2);
						}
					}
				}
			}
			Log.Out("{0} {1}, loaded {2}", new object[]
			{
				base.GetType(),
				text,
				this.turretsUnloaded.Count
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (this.saveThread == null || !ThreadManager.ActiveThreads.ContainsKey("turretDataSave"))
		{
			Log.Out("{0} saving {1} ({2} + {3})", new object[]
			{
				base.GetType(),
				this.turretsActive.Count + this.turretsUnloaded.Count,
				this.turretsActive.Count,
				this.turretsUnloaded.Count
			});
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				this.write(pooledBinaryWriter);
			}
			this.saveThread = ThreadManager.StartThread("turretDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.SaveThread), null, pooledExpandableMemoryStream, null, false, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SaveThread(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "turrets.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "turrets.dat.bak"), true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		Log.Out("{0} saved {1} bytes", new object[]
		{
			base.GetType(),
			pooledExpandableMemoryStream.Length
		});
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _br)
	{
		if (_br.ReadChar() != 'v' || _br.ReadChar() != 'd' || _br.ReadChar() != 'a' || _br.ReadChar() != '\0')
		{
			Log.Error("{0} file bad signature", new object[] { base.GetType() });
			return;
		}
		if (_br.ReadByte() != 1)
		{
			Log.Error("{0} file bad version", new object[] { base.GetType() });
			return;
		}
		this.turretsUnloaded.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.turretsUnloaded.Add(_br.ReadInt32());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(PooledBinaryWriter _bw)
	{
		_bw.Write('v');
		_bw.Write('d');
		_bw.Write('a');
		_bw.Write(0);
		_bw.Write(1);
		List<int> list = new List<int>();
		this.GetTurrets(list);
		_bw.Write(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			_bw.Write(list[i]);
		}
	}

	public List<int> GetTurretsList()
	{
		List<int> list = new List<int>();
		this.GetTurrets(list);
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetTurrets(List<int> _list)
	{
		for (int i = 0; i < this.turretsActive.Count; i++)
		{
			_list.Add(this.turretsActive[i].entityId);
		}
		for (int j = 0; j < this.turretsUnloaded.Count; j++)
		{
			_list.Add(this.turretsUnloaded[j]);
		}
	}

	public static int GetServerTurretCount()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return TurretTracker.Instance.turretsActive.Count + TurretTracker.Instance.turretsUnloaded.Count;
		}
		return TurretTracker.serverTurretCount;
	}

	public static void SetServerTurretCount(int count)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		TurretTracker.serverTurretCount = count;
	}

	public static bool CanAddMoreTurrets()
	{
		return !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() || TurretTracker.GetServerTurretCount() < 500;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSaveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cChangeSaveDelay = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxTurrets = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int serverTurretCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityTurret> turretsActive = new List<EntityTurret>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> turretsUnloaded = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float saveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TurretTracker instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo saveThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cNameKey = "turrets";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cThreadKey = "turretDataSave";
}

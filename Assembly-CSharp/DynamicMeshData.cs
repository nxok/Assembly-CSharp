using System;
using System.Diagnostics;
using System.Threading;

[DebuggerDisplay("{X},{Z} {StateInfo}")]
public class DynamicMeshData
{
	public string GetByteLengthString()
	{
		if (this.Bytes == null)
		{
			return "null";
		}
		return this.Bytes.Length.ToString();
	}

	public string Path(bool isRegionQueue)
	{
		return DynamicMeshFile.MeshLocation + string.Format("{0},{1}.{2}", this.X, this.Z, isRegionQueue ? "region" : "mesh");
	}

	public bool Exists(bool isRegionQueue)
	{
		return SdFile.Exists(DynamicMeshFile.MeshLocation + string.Format("{0},{1}.{2}", this.X, this.Z, isRegionQueue ? "region" : "mesh"));
	}

	public bool GetLock(string debug)
	{
		DateTime now = DateTime.Now;
		bool flag = false;
		while (!this.TryTakeLock(debug))
		{
			if (DynamicMeshThread.RequestThreadStop)
			{
				Log.Out(this.ToDebugLocation() + " World is unloading so lock attempt failed " + debug);
				return false;
			}
			double totalSeconds = (DateTime.Now - now).TotalSeconds;
			if (!flag && totalSeconds > 5.0)
			{
				flag = true;
				if (DynamicMeshManager.DoLog)
				{
					Log.Out(this.ToDebugLocation() + " Waiting for lock to release: " + this.lastLock);
				}
			}
			if (totalSeconds > 60.0 && Monitor.IsEntered(this._lock))
			{
				Log.Warning(string.Concat(new string[]
				{
					"Forcing lock release to ",
					debug,
					" from ",
					this.lastLock,
					" after ",
					totalSeconds.ToString(),
					" seconds"
				}));
				this.ReleaseLock();
			}
		}
		return true;
	}

	public bool ReleaseLock()
	{
		return this.TryExit("releaseLock");
	}

	public bool TryTakeLock(string debug)
	{
		bool flag = false;
		if (Monitor.IsEntered(this._lock))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Warning(this.ToDebugLocation() + " Lock kept by " + debug);
			}
			this.lastLock = debug;
			return true;
		}
		Monitor.TryEnter(this._lock, ref flag);
		if (flag)
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Warning(this.ToDebugLocation() + " Lock taken by " + debug);
			}
			this.lastLock = debug;
		}
		else if (DynamicMeshManager.DoLog)
		{
			Log.Warning(string.Concat(new string[]
			{
				this.ToDebugLocation(),
				" Lock failed on ",
				debug,
				" : ",
				this.lastLock
			}));
		}
		return flag;
	}

	public bool ThreadHasLock()
	{
		return Monitor.IsEntered(this._lock);
	}

	public bool TryGetBytes(out byte[] bytes, string debug)
	{
		if (this.TryTakeLock(debug))
		{
			bytes = this.Bytes;
			return true;
		}
		bytes = null;
		return false;
	}

	public bool TryExit(string debug)
	{
		if (!Monitor.IsEntered(this._lock))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Warning(this.ToDebugLocation() + " Tried to release lock when not owner " + debug);
			}
			return false;
		}
		while (Monitor.IsEntered(this._lock))
		{
			Monitor.Exit(this._lock);
		}
		if (DynamicMeshManager.DoLog)
		{
			Log.Out(this.ToDebugLocation() + " Lock released " + debug);
		}
		return true;
	}

	public bool IsAvailableToLoad()
	{
		return this.Bytes != null && !this.StateInfo.HasFlag(DynamicMeshStates.MarkedForDelete) && !this.StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating) && !this.StateInfo.HasFlag(DynamicMeshStates.LoadRequired);
	}

	public string ToDebugLocation()
	{
		return string.Format("{0}:{1},{2}", this.IsRegion ? "R" : "C", this.X, this.Z);
	}

	public void ClearUnloadMarks()
	{
		this.StateInfo &= ~DynamicMeshStates.UnloadMark1;
		this.StateInfo &= ~DynamicMeshStates.UnloadMark2;
		this.StateInfo &= ~DynamicMeshStates.UnloadMark3;
	}

	public static DynamicMeshData Create(int x, int z, bool isRegion)
	{
		return new DynamicMeshData
		{
			X = x,
			Z = z,
			IsRegion = isRegion
		};
	}

	public byte[] Bytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	public string lastLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsRegion;

	public DynamicMeshStates StateInfo;

	public int X;

	public int Z;

	public int StreamLength;
}

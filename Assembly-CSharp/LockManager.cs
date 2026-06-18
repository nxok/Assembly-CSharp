using System;
using System.Collections.Generic;
using System.Linq;

public class LockManager
{
	public static LockManager Instance
	{
		get
		{
			if (LockManager._instance == null)
			{
				LockManager._instance = new LockManager();
			}
			return LockManager._instance;
		}
	}

	public void Init()
	{
		if (this.pendingClientLocks.Count > 0 || this.singleLocks.CountKeys > 0 || this.sharedLocks.CountValues > 0)
		{
			Log.Warning("[LockManager] had locked entities from a previous session!");
			this.pendingClientLocks.Clear();
			this.singleLocks.Clear();
			this.sharedLocks.Clear();
		}
		this.playersWithLocks.Clear();
		this.keepOpenTimes.Clear();
		this.lastUpdate = DateTime.UtcNow;
	}

	public void Update()
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if ((DateTime.UtcNow - this.lastUpdate).TotalSeconds <= 2.5)
		{
			return;
		}
		this.lastUpdate = DateTime.UtcNow;
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			List<int> list = null;
			foreach (KeyValuePair<int, DateTime> keyValuePair in this.keepOpenTimes)
			{
				if (keyValuePair.Key != primaryPlayerId && (DateTime.UtcNow - keyValuePair.Value).TotalSeconds > 10.0)
				{
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(keyValuePair.Key);
				}
			}
			foreach (int num in this.playersWithLocks)
			{
				if (!this.keepOpenTimes.ContainsKey(num))
				{
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(num);
				}
			}
			if (list == null)
			{
				return;
			}
			using (List<int>.Enumerator enumerator3 = list.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					int num2 = enumerator3.Current;
					Log.Error(string.Format("[{0}] No keep-open packet received from player {1}.", "LockManager", num2));
					this.ForceUnlockByPlayer(num2);
				}
				return;
			}
		}
		if (this.playersWithLocks.Contains(primaryPlayerId))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageInventoryKeepOpen>().Setup(), false);
		}
	}

	public bool IsLockedByLocalPlayer(ILockTarget _target, ushort _channel = 0)
	{
		if (_target == null)
		{
			return false;
		}
		LockEntry lockEntry = new LockEntry(_target, _channel);
		if (this.pendingClientLocks.Contains(lockEntry))
		{
			return true;
		}
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (primaryPlayerId == -1)
		{
			return false;
		}
		IReadOnlyCollection<int> readOnlyCollection;
		if (_target.IsSharedLock(_channel) && this.sharedLocks.TryGetByValue(lockEntry, out readOnlyCollection))
		{
			using (IEnumerator<int> enumerator = readOnlyCollection.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == primaryPlayerId)
					{
						return true;
					}
				}
			}
			return false;
		}
		int num;
		return !_target.IsSharedLock(_channel) && this.singleLocks.TryGetByValue(lockEntry, out num) && num == primaryPlayerId;
	}

	public bool IsLockedServer(ILockTarget _target, ushort _channel = 0)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Clients may only check local lock states by using IsLockedByLocalPlayer.");
			return false;
		}
		LockEntry lockEntry = new LockEntry(_target, _channel);
		return (_target.IsSharedLock(_channel) && this.sharedLocks.ContainsValue(lockEntry)) || (!_target.IsSharedLock(_channel) && this.singleLocks.ContainsValue(lockEntry));
	}

	public void LockRequestLocal(ILockTarget _target, ILockContext _context = null, ushort _channel = 0)
	{
		if (_target == null)
		{
			Log.Error("[LockManager] target supplied for locking was null.");
			return;
		}
		ReadOnlySpan<ILockTarget> readOnlySpan = new ILockTarget[] { _target };
		this.LockRequestLocal(readOnlySpan, _context, _channel);
	}

	public unsafe void LockRequestLocal(ReadOnlySpan<ILockTarget> _targets, ILockContext _context = null, ushort _channel = 0)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			this.LockRequestServer(_targets, GameManager.Instance.World.GetPrimaryPlayerId(), _context, _channel);
			return;
		}
		if (_targets == null || _targets.Length == 0)
		{
			Log.Error("[LockManager] no targets supplied for locking.");
			return;
		}
		if (this.pendingClientLocks.Count > 0)
		{
			Log.Error("[LockManager] lock request already in progress.");
			return;
		}
		for (int i = 0; i < _targets.Length; i++)
		{
			this.pendingClientLocks.Add(new LockEntry(*_targets[i], _channel));
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageLockRequest>().Setup(_targets, _context, _channel), false);
	}

	public unsafe void LockRequestServer(ReadOnlySpan<ILockTarget> _targets, int _playerId, ILockContext _context = null, ushort _channel = 0)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Clients must only request a lock by using LockRequestLocal.");
			return;
		}
		if (this.singleLocks.ContainsKey(_playerId) || this.sharedLocks.ContainsKey(_playerId) || this.keepOpenTimes.ContainsKey(_playerId))
		{
			Log.Error(string.Format("[{0}] Invalid lock state for player {1} when calling {2}. The player tried requesting a lock despite having an existing lock state.", "LockManager", _playerId, "LockRequestServer"));
			this.ForceUnlockByPlayer(_playerId);
			return;
		}
		bool flag = false;
		string text = null;
		try
		{
			if (_targets.Length > 5)
			{
				text = string.Format("[{0}] Failed to lock targets. Requested {1} exceeds limit {2}.", "LockManager", _targets.Length, 5);
			}
			else
			{
				for (int i = 0; i < _targets.Length; i++)
				{
					ILockTarget lockTarget = *_targets[i];
					LockEntry lockEntry = new LockEntry(lockTarget, _channel);
					if (!lockTarget.IsSharedLock(_channel) && this.singleLocks.ContainsValue(lockEntry))
					{
						text = string.Format("[{0}] Failed to lock {1} on channel {2}. It is already locked by another player.", "LockManager", lockTarget.GetType(), _channel);
						return;
					}
				}
				for (int j = 0; j < _targets.Length; j++)
				{
					ILockTarget lockTarget2 = *_targets[j];
					if (!lockTarget2.CanLockOnServer(_playerId, _context, _channel))
					{
						text = string.Format("[{0}] Failed to lock {1} on channel {2}. Target refused to be locked on server.", "LockManager", lockTarget2.GetType(), _channel);
						return;
					}
				}
				flag = true;
			}
		}
		finally
		{
			if (flag)
			{
				for (int k = 0; k < _targets.Length; k++)
				{
					ILockTarget lockTarget3 = *_targets[k];
					LockEntry lockEntry2 = new LockEntry(lockTarget3, _channel);
					if (lockTarget3.IsSharedLock(_channel))
					{
						this.sharedLocks.Add(_playerId, lockEntry2);
					}
					else
					{
						this.singleLocks.Add(_playerId, lockEntry2);
					}
					this.RefreshPlayerActive(_playerId);
					this.keepOpenTimes[_playerId] = DateTime.UtcNow;
				}
				for (int l = 0; l < _targets.Length; l++)
				{
					_targets[l]->OnLockedServer(true, _playerId, _context, _channel);
				}
			}
			if (GameManager.Instance.World.GetPrimaryPlayerId() != _playerId)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageLockResponse>().Setup(flag, text, _targets, _context, _channel), false, _playerId, -1, -1, null, 192, false);
			}
			else
			{
				this.LockResponse(flag, text, _targets, _context, _channel);
			}
		}
	}

	public unsafe void LockResponse(bool _success, string _errorMsg, ReadOnlySpan<ILockTarget> _targets, ILockContext _context, ushort _channel)
	{
		this.pendingClientLocks.Clear();
		if (!string.IsNullOrEmpty(_errorMsg))
		{
			Log.Warning(_errorMsg);
		}
		if (GameManager.Instance.World == null)
		{
			Log.Error("[LockManager] World not fully initialized.");
			return;
		}
		for (int i = 0; i < _targets.Length; i++)
		{
			if (!(*_targets[i]))
			{
				Log.Error("[LockManager] target supplied for locking was null.");
				return;
			}
		}
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (_success)
		{
			for (int j = 0; j < _targets.Length; j++)
			{
				ILockTarget lockTarget = *_targets[j];
				if (!lockTarget.CanLockLocally(_context, _channel))
				{
					Log.Warning(string.Format("[{0}] Failed to lock {1} on channel {2}. Target refused to be locked locally.", "LockManager", lockTarget.GetType(), _channel));
					return;
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				for (int k = 0; k < _targets.Length; k++)
				{
					ILockTarget lockTarget2 = *_targets[k];
					LockEntry lockEntry = new LockEntry(lockTarget2, _channel);
					if (lockTarget2.IsSharedLock(_channel))
					{
						this.sharedLocks.Add(primaryPlayerId, lockEntry);
					}
					else
					{
						this.singleLocks.Add(primaryPlayerId, lockEntry);
					}
					this.RefreshPlayerActive(primaryPlayerId);
				}
				this.keepOpenTimes[primaryPlayerId] = DateTime.UtcNow;
			}
		}
		for (int l = 0; l < _targets.Length; l++)
		{
			_targets[l]->OnLockedLocal(_success, _context, _channel);
		}
	}

	public void UnlockRequestLocal()
	{
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageLockRequest>().Setup(), false);
			LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.CloseAllOpenModalWindows(null, false);
			return;
		}
		this.UnlockRequestServer(primaryPlayerId, false);
	}

	public void UnlockRequestServer(int _playerId, bool _isForceUnlocked)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Clients must unlock by using UnlockRequestLocal.");
			return;
		}
		bool flag = false;
		string text = null;
		try
		{
			if (this.playersWithLocks.Contains(_playerId))
			{
				bool flag2 = false;
				IReadOnlyCollection<LockEntry> readOnlyCollection;
				if (this.singleLocks.TryGetByKey(_playerId, out readOnlyCollection) && readOnlyCollection != null)
				{
					flag2 = true;
					foreach (LockEntry lockEntry in readOnlyCollection.ToArray<LockEntry>())
					{
						this.singleLocks.RemoveByValue(lockEntry);
						lockEntry.Target.OnUnlockedServer(_playerId, lockEntry.Channel);
					}
					this.singleLocks.RemoveByKey(_playerId);
				}
				IReadOnlyCollection<LockEntry> readOnlyCollection2;
				if (this.sharedLocks.TryGetByKey(_playerId, out readOnlyCollection2) && readOnlyCollection2 != null)
				{
					flag2 = true;
					foreach (LockEntry lockEntry2 in readOnlyCollection2.ToArray<LockEntry>())
					{
						this.sharedLocks.Remove(_playerId, lockEntry2);
						lockEntry2.Target.OnUnlockedServer(_playerId, lockEntry2.Channel);
					}
					this.sharedLocks.RemoveByKey(_playerId);
				}
				this.keepOpenTimes.Remove(_playerId);
				this.RefreshPlayerActive(_playerId);
				if (!flag2)
				{
					text = string.Format("[{0}] Found no targets while trying to unlock for player {1}.", "LockManager", _playerId);
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				text = "[LockManager] Player entity has nothing to unlock.";
			}
		}
		finally
		{
			if (GameManager.Instance.World.GetPrimaryPlayerId() != _playerId)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageLockResponse>().Setup(flag, text, _isForceUnlocked), false, _playerId, -1, -1, null, 192, false);
			}
			else
			{
				LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.CloseAllOpenModalWindows(null, false);
				this.UnlockResponse(flag, text, _isForceUnlocked);
			}
		}
	}

	public void UnlockResponse(bool _success, string _errorMsg, bool _isForceUnlocked = false)
	{
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		this.singleLocks.RemoveByKey(primaryPlayerId);
		this.sharedLocks.RemoveByKey(primaryPlayerId);
		this.keepOpenTimes.Remove(primaryPlayerId);
		this.RefreshPlayerActive(primaryPlayerId);
		if (!_success && !string.IsNullOrEmpty(_errorMsg))
		{
			Log.Warning(_errorMsg);
		}
		if (_isForceUnlocked)
		{
			LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.CloseAllOpenModalWindows(null, false);
			this.pendingClientLocks.Clear();
		}
	}

	public void ForceUnlockAll()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Only servers can use ForceUnlockAll");
			return;
		}
		int[] array = this.playersWithLocks.ToArray<int>();
		for (int i = 0; i < array.Length; i++)
		{
			this.ForceUnlockByPlayer(array[i]);
		}
	}

	public void ForceUnlockByPlayer(int _playerId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Only servers can use ForceUnlockByPlayer");
			return;
		}
		this.UnlockRequestServer(_playerId, true);
	}

	public void ForceUnlockLockTarget(ILockTarget _target)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Only servers can use ForceUnlockLockTarget");
			return;
		}
		if (_target == null)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (LockEntry lockEntry in this.singleLocks.Values)
		{
			int num;
			if (lockEntry.Target == _target && this.singleLocks.TryGetByValue(lockEntry, out num) && !list.Contains(num))
			{
				list.Add(num);
			}
		}
		foreach (LockEntry lockEntry2 in this.sharedLocks.Values)
		{
			IReadOnlyCollection<int> readOnlyCollection;
			if (lockEntry2.Target == _target && this.sharedLocks.TryGetByValue(lockEntry2, out readOnlyCollection))
			{
				foreach (int num2 in readOnlyCollection)
				{
					if (!list.Contains(num2))
					{
						list.Add(num2);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			foreach (int num3 in list)
			{
				this.UnlockRequestServer(num3, true);
			}
			return;
		}
		Log.Warning("[LockManager] Target is not locked.");
	}

	public void ForceUnlockByChunk(HashSetLong chunks)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Only servers can use ForceUnlockByChunk");
			return;
		}
		List<ILockTarget> list = new List<ILockTarget>();
		foreach (long num in chunks)
		{
			foreach (LockEntry lockEntry in this.singleLocks.Values)
			{
				TileEntity tileEntity = lockEntry.Target as TileEntity;
				if (tileEntity != null)
				{
					Chunk chunk = tileEntity.GetChunk();
					if ((chunk == null || chunk.Key == num) && !list.Contains(lockEntry.Target))
					{
						list.Add(lockEntry.Target);
					}
				}
				else
				{
					TEFeatureAbs tefeatureAbs = lockEntry.Target as TEFeatureAbs;
					if (tefeatureAbs != null)
					{
						TileEntityComposite parent = tefeatureAbs.Parent;
						Chunk chunk2 = ((parent != null) ? parent.GetChunk() : null);
						if ((chunk2 == null || chunk2.Key == num) && !list.Contains(lockEntry.Target))
						{
							list.Add(lockEntry.Target);
						}
					}
				}
			}
			foreach (LockEntry lockEntry2 in this.sharedLocks.Values)
			{
				TileEntity tileEntity2 = lockEntry2.Target as TileEntity;
				if (tileEntity2 != null)
				{
					Chunk chunk3 = tileEntity2.GetChunk();
					if ((chunk3 == null || chunk3.Key == num) && !list.Contains(lockEntry2.Target))
					{
						list.Add(lockEntry2.Target);
					}
				}
				else
				{
					TEFeatureAbs tefeatureAbs2 = lockEntry2.Target as TEFeatureAbs;
					if (tefeatureAbs2 != null)
					{
						TileEntityComposite parent2 = tefeatureAbs2.Parent;
						Chunk chunk4 = ((parent2 != null) ? parent2.GetChunk() : null);
						if ((chunk4 == null || chunk4.Key == num) && !list.Contains(lockEntry2.Target))
						{
							list.Add(lockEntry2.Target);
						}
					}
				}
			}
		}
		foreach (ILockTarget lockTarget in list)
		{
			this.ForceUnlockLockTarget(lockTarget);
		}
	}

	public void ProcessKeepOpen(int _playerId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (!this.singleLocks.ContainsKey(_playerId) && !this.sharedLocks.ContainsKey(_playerId))
		{
			Log.Warning("[LockManager] Keep Open request received but player owns no locks.");
			this.keepOpenTimes.Remove(_playerId);
			this.ForceUnlockByPlayer(_playerId);
			return;
		}
		this.keepOpenTimes[_playerId] = DateTime.UtcNow;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshPlayerActive(int playerId)
	{
		if (this.singleLocks.ContainsKey(playerId) || this.sharedLocks.ContainsKey(playerId))
		{
			this.playersWithLocks.Add(playerId);
			return;
		}
		this.playersWithLocks.Remove(playerId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxLocksPerPlayer = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float KeepOpenTimeoutSeconds = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float KeepOpenPeriodSeconds = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LockManager _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> playersWithLocks = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public OneToManyDictionary<int, LockEntry> singleLocks = new OneToManyDictionary<int, LockEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ManyToManyDictionary<int, LockEntry> sharedLocks = new ManyToManyDictionary<int, LockEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<LockEntry> pendingClientLocks = new HashSet<LockEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, DateTime> keepOpenTimes = new Dictionary<int, DateTime>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastUpdate;
}

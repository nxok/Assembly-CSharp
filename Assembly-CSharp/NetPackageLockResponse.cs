using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLockResponse : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public unsafe NetPackageLockResponse Setup(bool _success, string _errorMsg, ReadOnlySpan<ILockTarget> _targets, ILockContext _context, ushort _channel = 0)
	{
		if (_targets == null || _targets.Length == 0)
		{
			Log.Error("[NetPackageLockResponse] No lock targets supplied.");
			return null;
		}
		this.locking = true;
		this.success = _success;
		this.errorMsg = _errorMsg ?? string.Empty;
		this.targets = new ILockTarget[_targets.Length];
		for (int i = 0; i < _targets.Length; i++)
		{
			this.targets[i] = *_targets[i];
		}
		this.context = _context;
		this.channel = _channel;
		return this;
	}

	public NetPackageLockResponse Setup(bool _success, string _errorMsg, bool _isForceUnlocked)
	{
		this.locking = false;
		this.success = _success;
		this.errorMsg = _errorMsg ?? string.Empty;
		this.isForceUnlocked = _isForceUnlocked;
		this.targets = null;
		this.context = null;
		this.channel = 0;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.locking = _br.ReadBoolean();
		this.success = _br.ReadBoolean();
		this.errorMsg = _br.ReadString();
		this.isForceUnlocked = _br.ReadBoolean();
		this.channel = _br.ReadUInt16();
		int num = _br.ReadInt32();
		if (num > 0)
		{
			this.targets = new ILockTarget[num];
			for (int i = 0; i < num; i++)
			{
				this.targets[i] = ILockTarget.ReadIdentifyingInfo(_br);
			}
		}
		string text = _br.ReadString();
		if (string.IsNullOrEmpty(text))
		{
			this.context = null;
			return;
		}
		Type type = Type.GetType(text);
		if (type == null)
		{
			Log.Error("[NetPackageLockResponse] Unknown context type: " + text);
			this.context = null;
			return;
		}
		this.context = (ILockContext)Activator.CreateInstance(type);
		this.context.Read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.locking);
		_bw.Write(this.success);
		_bw.Write(this.errorMsg);
		_bw.Write(this.isForceUnlocked);
		_bw.Write(this.channel);
		ILockTarget[] array = this.targets;
		_bw.Write((array != null) ? array.Length : 0);
		if (this.targets != null)
		{
			for (int i = 0; i < this.targets.Length; i++)
			{
				ILockTarget.WriteIdentifyingInfo(this.targets[i], _bw);
			}
		}
		if (this.context == null)
		{
			_bw.Write(string.Empty);
			return;
		}
		_bw.Write(this.context.GetType().FullName);
		this.context.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!this.locking)
		{
			LockManager.Instance.UnlockResponse(this.success, this.errorMsg, this.isForceUnlocked);
			return;
		}
		LockManager.Instance.LockResponse(this.success, this.errorMsg, this.targets.AsSpan<ILockTarget>(), this.context, this.channel);
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool locking;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool success;

	[PublicizedFrom(EAccessModifier.Private)]
	public string errorMsg = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockTarget[] targets;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockContext context;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort channel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isForceUnlocked;
}

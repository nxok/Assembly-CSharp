using System;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public abstract class NetPackage
{
	public virtual int Channel
	{
		get
		{
			return 0;
		}
	}

	public virtual bool Compress
	{
		get
		{
			return false;
		}
	}

	public virtual bool FlushQueue
	{
		get
		{
			return false;
		}
	}

	public virtual NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.Both;
		}
	}

	public virtual bool AllowedBeforeAuth
	{
		get
		{
			return false;
		}
	}

	public int PackageId
	{
		get
		{
			return NetPackageManager.GetPackageId(base.GetType());
		}
	}

	public ClientInfo Sender { get; set; }

	public virtual bool ReliableDelivery
	{
		get
		{
			return true;
		}
	}

	public abstract void read(PooledBinaryReader _reader);

	public virtual void write(PooledBinaryWriter _writer)
	{
		_writer.Write((ushort)this.PackageId);
	}

	public virtual bool ShouldProcess(World _world, GameManager _callbacks)
	{
		return true;
	}

	public abstract void ProcessPackage(World _world, GameManager _callbacks);

	public virtual void HandleSkipped(World _world, GameManager _callbacks)
	{
		NetPackageManager.FreePackage(this);
	}

	public abstract int GetLength();

	public override string ToString()
	{
		string text;
		if ((text = this.classnameCached) == null)
		{
			text = (this.classnameCached = base.GetType().Name);
		}
		return text;
	}

	public void RegisterSendQueue()
	{
		Interlocked.Increment(ref this.inSendQueuesCount);
	}

	public void SendQueueHandled()
	{
		if (Interlocked.Decrement(ref this.inSendQueuesCount) == 0)
		{
			NetPackageManager.FreePackage(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ValidEntityIdForSender(int _entityId, bool _allowAttachedToEntity = false)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return true;
		}
		if (_entityId == this.Sender.entityId)
		{
			return true;
		}
		if (_allowAttachedToEntity)
		{
			EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.Sender.entityId) as EntityPlayer;
			if (entityPlayer != null && entityPlayer.AttachedToEntity != null && entityPlayer.AttachedToEntity.entityId == _entityId)
			{
				return true;
			}
		}
		Log.Warning(string.Format("Received {0} with invalid entityId {1} from {2}", this.ToString(), _entityId, this.Sender));
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ValidUserIdForSender(PlatformUserIdentifierAbs _userId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return true;
		}
		if (object.Equals(_userId, this.Sender.PlatformId) || object.Equals(_userId, this.Sender.CrossplatformId))
		{
			return true;
		}
		Log.Warning(string.Format("Received {0} with invalid userId {1} from {2}", this.ToString(), _userId, this.Sender));
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public NetPackage()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int inSendQueuesCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string classnameCached;
}

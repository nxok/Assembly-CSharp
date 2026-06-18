using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEncryptionSharedKey : NetPackage
{
	public override bool AllowedBeforeAuth
	{
		get
		{
			return true;
		}
	}

	public NetPackageEncryptionSharedKey Setup(byte[] encryptionKey, byte[] integrityKey)
	{
		this.EncryptionKey = encryptionKey;
		this.IntegrityKey = integrityKey;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.EncryptionKey = new byte[_br.ReadInt32()];
		_br.Read(this.EncryptionKey, 0, this.EncryptionKey.Length);
		this.IntegrityKey = new byte[_br.ReadInt32()];
		_br.Read(this.IntegrityKey, 0, this.IntegrityKey.Length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.EncryptionKey.Length);
		_bw.Write(this.EncryptionKey);
		_bw.Write(this.IntegrityKey.Length);
		_bw.Write(this.IntegrityKey);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthClient.CompleteKeyExchange(this.EncryptionKey, this.IntegrityKey);
	}

	public override int GetLength()
	{
		byte[] encryptionKey = this.EncryptionKey;
		if (encryptionKey == null)
		{
			byte[] integrityKey = this.IntegrityKey;
			int? num = ((integrityKey != null) ? new int?(integrityKey.Length) : null);
			return ((num != null) ? new int?(num.GetValueOrDefault()) : null).GetValueOrDefault();
		}
		return encryptionKey.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] EncryptionKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] IntegrityKey;
}

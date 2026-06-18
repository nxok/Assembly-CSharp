using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEncryptionPublicKey : NetPackage
{
	public override bool AllowedBeforeAuth
	{
		get
		{
			return true;
		}
	}

	public NetPackageEncryptionPublicKey Setup(string keyParamsXml, byte[] hash, byte[] signedHash)
	{
		this.ExchangePublicKeyParamsXml = keyParamsXml;
		this.Hash = hash;
		this.SignedHash = signedHash;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.ExchangePublicKeyParamsXml = _br.ReadString();
		this.Hash = new byte[_br.ReadInt32()];
		_br.Read(this.Hash, 0, this.Hash.Length);
		this.SignedHash = new byte[_br.ReadInt32()];
		_br.Read(this.SignedHash, 0, this.SignedHash.Length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.ExchangePublicKeyParamsXml);
		_bw.Write(this.Hash.Length);
		_bw.Write(this.Hash);
		_bw.Write(this.SignedHash.Length);
		_bw.Write(this.SignedHash);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthServer.SendSharedKey(base.Sender, this.ExchangePublicKeyParamsXml, this.Hash, this.SignedHash);
	}

	public override int GetLength()
	{
		string exchangePublicKeyParamsXml = this.ExchangePublicKeyParamsXml;
		if (exchangePublicKeyParamsXml != null)
		{
			return exchangePublicKeyParamsXml.Length;
		}
		byte[] hash = this.Hash;
		int? num = ((hash != null) ? new int?(hash.Length) : null);
		int? num2 = ((num != null) ? new int?(num.GetValueOrDefault()) : null);
		if (num2 == null)
		{
			byte[] signedHash = this.SignedHash;
			num = ((signedHash != null) ? new int?(signedHash.Length) : null);
			return ((num != null) ? new int?(num.GetValueOrDefault()) : null).GetValueOrDefault();
		}
		return num2.GetValueOrDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string ExchangePublicKeyParamsXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] Hash;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] SignedHash;
}

using System;
using System.IO;
using System.Security.Cryptography;

public class AesEncryptAndMac : IEncryptionModule
{
	public byte[] EncryptionKey
	{
		get
		{
			return this.aes.Key;
		}
	}

	public byte[] IntegrityKey
	{
		get
		{
			return this.hmac.Key;
		}
	}

	public AesEncryptAndMac()
	{
		this.aes = Aes.Create();
		this.aes.GenerateKey();
		this.aes.GenerateIV();
		this.encryptor = this.aes.CreateEncryptor();
		this.decryptor = this.aes.CreateDecryptor();
		this.hmac = new HMACSHA256();
		this.random = RandomNumberGenerator.Create();
	}

	public AesEncryptAndMac(byte[] encryptionKey, byte[] integrityKey)
	{
		this.aes = Aes.Create();
		this.aes.Key = encryptionKey;
		this.aes.GenerateIV();
		this.encryptor = this.aes.CreateEncryptor();
		this.decryptor = this.aes.CreateDecryptor();
		this.hmac = new HMACSHA256(integrityKey);
		this.random = RandomNumberGenerator.Create();
	}

	public unsafe bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		int inputBlockSize = this.encryptor.InputBlockSize;
		Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)inputBlockSize], inputBlockSize);
		PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(false);
		int num = (int)_stream.Length;
		object obj = this.lockObj;
		byte[] hash;
		lock (obj)
		{
			using (CryptoStream cryptoStream = new CryptoStream(pooledExpandableMemoryStream, this.hmac, CryptoStreamMode.Write, true))
			{
				using (CryptoStream cryptoStream2 = new CryptoStream(cryptoStream, this.encryptor, CryptoStreamMode.Write))
				{
					this.random.GetBytes(span);
					cryptoStream2.Write(span);
					StreamUtils.Write(cryptoStream2, num);
					cryptoStream2.Write(_stream.GetBuffer(), 0, num);
				}
			}
			hash = this.hmac.Hash;
		}
		_stream.SetLength(0L);
		_stream.Write(hash);
		_stream.Write(pooledExpandableMemoryStream.GetBuffer(), 0, (int)pooledExpandableMemoryStream.Length);
		_stream.Position = 0L;
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return true;
	}

	public unsafe bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream)
	{
		int num = this.hmac.HashSize / 8;
		Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)num], num);
		_stream.Read(span);
		num = this.decryptor.OutputBlockSize;
		Span<byte> span2 = new Span<byte>(stackalloc byte[(UIntPtr)num], num);
		int num2 = 0;
		object obj = this.lockObj;
		byte[] array;
		byte[] hash;
		lock (obj)
		{
			using (CryptoStream cryptoStream = new CryptoStream(_stream, this.hmac, CryptoStreamMode.Read, true))
			{
				using (CryptoStream cryptoStream2 = new CryptoStream(cryptoStream, this.decryptor, CryptoStreamMode.Read))
				{
					cryptoStream2.Read(span2);
					num2 = StreamUtils.ReadInt32(cryptoStream2);
					array = MemoryPools.poolByte.Alloc(num2);
					cryptoStream2.Read(array, 0, num2);
				}
			}
			hash = this.hmac.Hash;
		}
		if (span.Length != hash.Length)
		{
			Log.Error(string.Format("[EncryptionAgreement] decryption failure, MAC bytes not the same length. Calculated {0}, Remote {1}", this.hmac.Hash.Length, span.Length));
			return false;
		}
		for (int i = 0; i < span.Length; i++)
		{
			if (*span[i] != hash[i])
			{
				Log.Error("[EncryptionAgreement] decryption failure, MAC did not match");
				return false;
			}
		}
		_stream.SetLength(0L);
		_stream.Write(array, 0, num2);
		_stream.Position = 0L;
		MemoryPools.poolByte.Free(array);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Aes aes;

	[PublicizedFrom(EAccessModifier.Private)]
	public ICryptoTransform encryptor;

	[PublicizedFrom(EAccessModifier.Private)]
	public ICryptoTransform decryptor;

	[PublicizedFrom(EAccessModifier.Private)]
	public HMAC hmac;

	[PublicizedFrom(EAccessModifier.Private)]
	public RandomNumberGenerator random;

	[PublicizedFrom(EAccessModifier.Private)]
	public object lockObj = new object();
}

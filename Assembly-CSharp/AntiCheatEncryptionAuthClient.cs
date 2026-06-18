using System;
using System.Security.Cryptography;
using System.Text;

public class AntiCheatEncryptionAuthClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RSA GetSigningKey()
	{
		throw new NotImplementedException();
	}

	public void StartKeyExchange()
	{
		Log.Out("[EncryptionAgreement] checking signing key");
		RSA signingKey;
		try
		{
			signingKey = this.GetSigningKey();
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageKeyExchangeComplete>().Setup(false), false);
			return;
		}
		Log.Out("[EncryptionAgreement] creating key exchange params");
		this.keyExchangeSessionPair = RSA.Create(2048);
		string text = this.keyExchangeSessionPair.ToXmlString(false);
		Log.Out("[EncryptionAgreement] signing params");
		using (SHA512 sha = SHA512.Create())
		{
			byte[] array = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
			RSAPKCS1SignatureFormatter rsapkcs1SignatureFormatter = new RSAPKCS1SignatureFormatter(signingKey);
			rsapkcs1SignatureFormatter.SetHashAlgorithm("SHA512");
			byte[] array2 = rsapkcs1SignatureFormatter.CreateSignature(array);
			Log.Out("[EncryptionAgreement] sending params to server");
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEncryptionPublicKey>().Setup(text, array, array2), false);
			signingKey.Dispose();
		}
	}

	public void CompleteKeyExchange(byte[] protectedEncryptionKey, byte[] protectedIntegrityKey)
	{
		Log.Out("[EncryptionAgreement] received shared keys");
		byte[] array = this.keyExchangeSessionPair.Decrypt(protectedEncryptionKey, RSAEncryptionPadding.Pkcs1);
		byte[] array2 = this.keyExchangeSessionPair.Decrypt(protectedIntegrityKey, RSAEncryptionPadding.Pkcs1);
		AesEncryptAndMac aesEncryptAndMac = new AesEncryptAndMac(array, array2);
		INetConnection[] connectionToServer = SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer();
		for (int i = 0; i < connectionToServer.Length; i++)
		{
			connectionToServer[i].SetEncryptionModule(aesEncryptAndMac);
		}
		Log.Out("[EncryptionAgreement] sending reply");
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageKeyExchangeComplete>().Setup(true), false);
		RSA rsa = this.keyExchangeSessionPair;
		if (rsa != null)
		{
			rsa.Dispose();
		}
		this.keyExchangeSessionPair = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RSA keyExchangeSessionPair;
}

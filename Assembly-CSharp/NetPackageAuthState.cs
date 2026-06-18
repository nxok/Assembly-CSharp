using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAuthState : NetPackage
{
	public override bool FlushQueue
	{
		get
		{
			return true;
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override bool AllowedBeforeAuth
	{
		get
		{
			return true;
		}
	}

	public NetPackageAuthState Setup(string _authStateKey)
	{
		this.stateKey = _authStateKey ?? "";
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.stateKey = _reader.ReadString();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.stateKey);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			Log.Out("Login: " + this.stateKey);
			if (!string.IsNullOrEmpty(this.stateKey))
			{
				string text = Localization.Get(this.stateKey, false, null);
				string text2 = text;
				object platformDisplayName = PlatformManager.NativePlatform.PlatformDisplayName;
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				text = string.Format(text2, platformDisplayName, (crossplatformPlatform != null) ? crossplatformPlatform.PlatformDisplayName : null);
				XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, text, true);
			}
		}
	}

	public override int GetLength()
	{
		return 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string stateKey;
}

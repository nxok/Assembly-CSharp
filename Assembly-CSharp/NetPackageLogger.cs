using System;
using System.IO;
using UnityEngine;

public static class NetPackageLogger
{
	public static void Init()
	{
		if (NetPackageLogger.logFilePathPrefix != null)
		{
			return;
		}
		if (string.IsNullOrEmpty(Application.consoleLogPath))
		{
			NetPackageLogger.logFilePathPrefix = "";
			NetPackageLogger.logEnabled = false;
			return;
		}
		NetPackageLogger.logFilePathPrefix = Path.GetDirectoryName(Application.consoleLogPath) + "/netpackages_";
		NetPackageLogger.logEnabled = GameUtils.GetLaunchArgument("debugpackages") != null;
	}

	public static void BeginLog(bool _asServer)
	{
		if (!NetPackageLogger.logEnabled)
		{
			return;
		}
		NetPackageLogger.opened++;
		if (NetPackageLogger.logFileStream != null)
		{
			return;
		}
		NetPackageLogger.logFileStream = SdFile.CreateText(NetPackageLogger.logFilePathPrefix + (_asServer ? "S_" : "C_") + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv");
		NetPackageLogger.logFileStream.WriteLine("Time,Dir,Src/Tgt,PackageType,Chn,Len,Encrypted,Compressed,Pkg# in Msg,Pkgs in Msg");
	}

	public static void LogPackage(bool _dirIsOut, ClientInfo _clientInfo, NetPackage _packageType, int _channel, int _length, bool _encrypted, bool _compressed, int _pkgNumInMsg, int _pkgsInMsg)
	{
		if (NetPackageLogger.logFileStream == null)
		{
			return;
		}
		string text = ((_clientInfo == null) ? "Server" : _clientInfo.InternalId.CombinedString);
		StreamWriter streamWriter = NetPackageLogger.logFileStream;
		lock (streamWriter)
		{
			NetPackageLogger.logFileStream.WriteLine(string.Format("{0:O},{1},{2},{3},{4},{5},{6},{7},{8},{9}", new object[]
			{
				DateTime.Now,
				_dirIsOut ? "Out" : "In",
				text,
				_packageType.GetType().Name,
				_channel,
				_length,
				_encrypted,
				_compressed,
				_pkgNumInMsg,
				_pkgsInMsg
			}));
		}
	}

	public static void EndLog()
	{
		if (NetPackageLogger.logFileStream == null)
		{
			return;
		}
		if (--NetPackageLogger.opened > 0)
		{
			return;
		}
		NetPackageLogger.logFileStream.Close();
		NetPackageLogger.logFileStream = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int opened;

	[PublicizedFrom(EAccessModifier.Private)]
	public static StreamWriter logFileStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool logEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string logFilePathPrefix;
}

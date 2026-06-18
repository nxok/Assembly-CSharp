using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdProfileNetwork : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "profilenetwork" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		this.doProfileNetwork();
	}

	public void resetData()
	{
		ConsoleCmdProfileNetwork.lastTimeWritten = Time.time;
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ClientInfo clientInfo = list[i];
				this.resetDataOnConnection(clientInfo.netConnection);
			}
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			for (int j = 0; j < 2; j++)
			{
				this.resetDataOnConnection(SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetDataOnConnection(INetConnection[] _con)
	{
		int[] array = new int[NetPackageManager.KnownPackageCount];
		int[] array2 = new int[NetPackageManager.KnownPackageCount];
		int[] array3 = new int[NetPackageManager.KnownPackageCount];
		int[] array4 = new int[NetPackageManager.KnownPackageCount];
		for (int i = 0; i < 2; i++)
		{
			NetConnectionStatistics stats = _con[i].GetStats();
			int num;
			int num2;
			int num3;
			int num4;
			stats.GetStats(0f, out num, out num2, out num3, out num4);
			stats.GetPackageTypes(array, array3, array2, array4, true);
			stats.GetLastPackagesReceived().Clear();
			stats.GetLastPackagesSent().Clear();
		}
	}

	public void doProfileNetwork()
	{
		string text = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? "server" : "client");
		string text2 = string.Format("{0}/network_profiling_{1}_{2:yyyy-MM-dd_HH-mm-ss}.log", GameIO.GetApplicationPath(), text, DateTime.Now);
		float num = Time.time - ConsoleCmdProfileNetwork.lastTimeWritten;
		ConsoleCmdProfileNetwork.lastTimeWritten = Time.time;
		using (StreamWriter streamWriter = new StreamWriter(text2))
		{
			streamWriter.WriteLine("Interval: " + num.ToCultureInvariantString("0.0") + "s\n");
			int[] array = new int[2];
			int[] array2 = new int[2];
			int[] array3 = new int[2];
			int[] array4 = new int[2];
			int[] array5 = new int[NetPackageManager.KnownPackageCount];
			int[] array6 = new int[NetPackageManager.KnownPackageCount];
			int[] array7 = new int[NetPackageManager.KnownPackageCount];
			int[] array8 = new int[NetPackageManager.KnownPackageCount];
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
				for (int i = 0; i < list.Count; i++)
				{
					ClientInfo clientInfo = list[i];
					streamWriter.WriteLine(string.Format("*** Client {0}:\n", clientInfo));
					this.doResultsForConnection(clientInfo.netConnection, streamWriter, array, array2, array3, array4, array6, array8, array5, array7, num);
				}
				streamWriter.WriteLine("\n\nTotal:");
				for (int j = 0; j < 2; j++)
				{
					this.printChannelStats(streamWriter, j, array4[j], array2[j], array3[j], array[j], num);
				}
			}
			else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				streamWriter.WriteLine("*** ToServer\n");
				this.doResultsForConnection(SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer(), streamWriter, array, array2, array3, array4, array6, array8, array5, array7, num);
			}
			streamWriter.WriteLine("\nPackages:");
			streamWriter.WriteLine(string.Format(" {0,3}\t{1,6}\t{2,8}\t{3,6}\t{4,8}\t{5,6}\t{6,8}\t{7,8}\tPackage type name", new object[] { "ID", "CntRX", "SizeRX", "CntTX", "SizeTX", "CntSum", "SizeSum", "SizeAvg" }));
			for (int k = 0; k < array5.Length; k++)
			{
				int num2 = array5[k];
				int num3 = array7[k];
				int num4 = array6[k];
				int num5 = array8[k];
				int num6 = num2 + num4;
				int num7 = num3 + num5;
				int num8 = ((num6 > 0) ? (num7 / num6) : 0);
				streamWriter.WriteLine(string.Format(" {0,3}\t{1,6}\t{2,8}\t{3,6}\t{4,8}\t{5,6}\t{6,8}\t{7,8}\t{8}", new object[]
				{
					k,
					num2,
					num3,
					num4,
					num5,
					num6,
					num7,
					num8,
					NetPackageManager.GetPackageName(k)
				}));
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Network profiling for {0:0.0}s writing to file {1}", num, text2));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void printChannelStats(StreamWriter _file, int _channel, int _pkgsRx, int _bytesRx, int _pkgsTx, int _bytesTx, float _interval)
	{
		if (_channel == 0)
		{
			_file.WriteLine("Stats:");
			_file.WriteLine(string.Format(" {0,3}\t{1,6}\t{2,8}\t{3,6}\t{4,8}\t{5,10}\t{6,10}", new object[] { "Ch", "CntRX", "SizeRX", "CntTX", "SizeTX", "RateRX", "RateTX" }));
		}
		_file.WriteLine(string.Format(" {0,3}\t{1,6}\t{2,8}\t{3,6}\t{4,8}\t{5,10}kb/s\t{6,10}kb/s", new object[]
		{
			_channel,
			_pkgsRx,
			_bytesRx,
			_pkgsTx,
			_bytesTx,
			((float)_bytesRx / (_interval * 1024f)).ToCultureInvariantString("0.0"),
			((float)_bytesTx / (_interval * 1024f)).ToCultureInvariantString("0.0")
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doResultsForConnection(INetConnection[] _con, StreamWriter _outFile, int[] _tBytesSent, int[] _tBytesReceived, int[] _tPackagesSent, int[] _tPackagesReceived, int[] _tPackagesPerTypeSent, int[] _tBytesPerTypeSent, int[] _tPackagesPerTypeReceived, int[] _tBytesPerTypeReceived, float _interval)
	{
		for (int i = 0; i < 2; i++)
		{
			int num;
			int num2;
			int num3;
			int num4;
			_con[i].GetStats().GetStats(0f, out num, out num2, out num3, out num4);
			this.printChannelStats(_outFile, i, num4, num3, num2, num, _interval);
			_tBytesSent[i] += num;
			_tBytesReceived[i] += num3;
			_tPackagesSent[i] += num2;
			_tPackagesReceived[i] += num4;
			_con[i].GetStats().GetPackageTypes(_tPackagesPerTypeReceived, _tBytesPerTypeReceived, _tPackagesPerTypeSent, _tBytesPerTypeSent, true);
		}
		_outFile.WriteLine("");
		for (int j = 0; j < 2; j++)
		{
			string text = this.printPackageSequence(_con[j].GetStats().GetLastPackagesReceived(), "Recv Ch" + j.ToString());
			_outFile.WriteLine(text);
		}
		_outFile.WriteLine("");
		for (int k = 0; k < 2; k++)
		{
			string text2 = this.printPackageSequence(_con[k].GetStats().GetLastPackagesSent(), "Sent on Ch" + k.ToString());
			_outFile.WriteLine(text2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string printPackageSequence(RingBuffer<SNetPackageInfo> _sequence, string _addInfo)
	{
		if (_sequence == null)
		{
			return _addInfo;
		}
		StringBuilder stringBuilder = new StringBuilder();
		_sequence.SetToLast();
		ulong tick = _sequence.Peek().Tick;
		stringBuilder.Append(_addInfo);
		stringBuilder.Append(" ");
		stringBuilder.Append(tick.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Past\tCnt\t Size\tID\n");
		SNetPackageInfo? snetPackageInfo = null;
		int num = 1;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num3 < 30 && num4 < _sequence.Count - 1)
		{
			SNetPackageInfo prev = _sequence.GetPrev();
			if (snetPackageInfo != null)
			{
				if (snetPackageInfo.Value.Tick == prev.Tick && snetPackageInfo.Value.Id == prev.Id)
				{
					num++;
				}
				else
				{
					stringBuilder.Append(this.formatPackage(tick, snetPackageInfo.Value, num, num2));
					num = 1;
					num2 = 0;
					num3++;
				}
			}
			snetPackageInfo = new SNetPackageInfo?(prev);
			num2 += prev.Size;
			num4++;
		}
		if (snetPackageInfo != null)
		{
			stringBuilder.Append(this.formatPackage(tick, snetPackageInfo.Value, num, num2));
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string formatPackage(ulong _baseTick, SNetPackageInfo _lastPackage, int _lastPackageCnt, int _size)
	{
		return string.Format(" {0,3}\t{1,2}\t{2,5}\t{3}\n", new object[]
		{
			_baseTick - _lastPackage.Tick,
			_lastPackageCnt,
			_size,
			NetPackageManager.GetPackageName(_lastPackage.Id)
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Writes network profiling information";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastTimeWritten;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ColWidthType = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ColWidthCount = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ColWidthSize = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ColWidthRate = 10;
}

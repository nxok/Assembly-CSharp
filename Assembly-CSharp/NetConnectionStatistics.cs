using System;
using UnityEngine;

public class NetConnectionStatistics
{
	public void RegisterReceivedPackage(int _packageType, int _length)
	{
		this.statsPackagePerTypeReceived[_packageType]++;
		this.statsBytesPerTypeReceived[_packageType] += _length;
		this.statsLastPackagesRec.Add(new SNetPackageInfo(_packageType, _length));
	}

	public void RegisterReceivedData(int _packageCount, int _netDataSize)
	{
		this.statsBytesReceived += _netDataSize;
		this.statsPackagesReceived += _packageCount;
	}

	public void RegisterSentData(int _packageCount, int _netDataSize)
	{
		this.statsBytesSent += _netDataSize;
		this.statsPackagesSent += _packageCount;
	}

	public void RegisterSentPackage(int _packageType, int _length)
	{
		this.statsPackagePerTypeSent[_packageType]++;
		this.statsBytesPerTypeSent[_packageType] += _length;
		this.statsLastPackagesSent.Add(new SNetPackageInfo(_packageType, _length));
	}

	public void GetPackageTypes(int[] _packagesPerTypeReceived, int[] _bytesPerTypeReceived, int[] _packagesPerTypeSent, int[] _bytesPerTypeSent, bool _reset)
	{
		for (int i = 0; i < _packagesPerTypeReceived.Length; i++)
		{
			_packagesPerTypeReceived[i] += this.statsPackagePerTypeReceived[i];
			_packagesPerTypeSent[i] += this.statsPackagePerTypeSent[i];
			_bytesPerTypeReceived[i] += this.statsBytesPerTypeReceived[i];
			_bytesPerTypeSent[i] += this.statsBytesPerTypeSent[i];
		}
		if (_reset)
		{
			Array.Clear(this.statsPackagePerTypeReceived, 0, this.statsPackagePerTypeReceived.Length);
			Array.Clear(this.statsPackagePerTypeSent, 0, this.statsPackagePerTypeSent.Length);
			Array.Clear(this.statsBytesPerTypeReceived, 0, this.statsBytesPerTypeReceived.Length);
			Array.Clear(this.statsBytesPerTypeSent, 0, this.statsBytesPerTypeSent.Length);
		}
	}

	public RingBuffer<SNetPackageInfo> GetLastPackagesSent()
	{
		return this.statsLastPackagesSent;
	}

	public RingBuffer<SNetPackageInfo> GetLastPackagesReceived()
	{
		return this.statsLastPackagesRec;
	}

	public void GetStats(float _interval, out int _bytesPerSecondSent, out int _packagesPerSecondSent, out int _bytesPerSecondReceived, out int _packagesPerSecondReceived)
	{
		_bytesPerSecondSent = this.statsBytesSent;
		_packagesPerSecondSent = this.statsPackagesSent;
		_bytesPerSecondReceived = this.statsBytesReceived;
		_packagesPerSecondReceived = this.statsPackagesReceived;
		this.resetStats();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetStats()
	{
		this.statsBytesReceived = 0;
		this.statsPackagesReceived = 0;
		this.statsBytesSent = 0;
		this.statsPackagesSent = 0;
		this.lastTimeStatsRequested = Time.time;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeStatsRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int statsBytesSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int statsPackagesSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] statsPackagePerTypeSent = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] statsBytesPerTypeSent = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RingBuffer<SNetPackageInfo> statsLastPackagesSent = new RingBuffer<SNetPackageInfo>(30);

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int statsBytesReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int statsPackagesReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] statsPackagePerTypeReceived = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] statsBytesPerTypeReceived = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RingBuffer<SNetPackageInfo> statsLastPackagesRec = new RingBuffer<SNetPackageInfo>(30);

	[PublicizedFrom(EAccessModifier.Private)]
	public int bytesPerSecondSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int packagesPerSecondSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bytesPerSecondReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public int packagesPerSecondReceived;
}

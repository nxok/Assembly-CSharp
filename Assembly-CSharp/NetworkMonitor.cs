using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;

public class NetworkMonitor
{
	public NetworkMonitor(int _channel, Transform _parent)
	{
		this.channel = _channel;
		this.parent = _parent;
		this.uiLblOverview = _parent.Find("lblOverview").GetComponent<UILabel>();
		Transform transform;
		this.uiLblRecvPkgCnt = (((transform = _parent.Find("lblRecvPkgCnt")) != null) ? transform.GetComponent<UILabel>() : null);
		this.uiLblSentPkgCnt = (((transform = _parent.Find("lblSentPkgCnt")) != null) ? transform.GetComponent<UILabel>() : null);
		this.uiLblRecvPkgSeq = (((transform = _parent.Find("lblRecvPkgSeq")) != null) ? transform.GetComponent<UILabel>() : null);
		this.uiLblSentPkgSeq = (((transform = _parent.Find("lblSentPkgSeq")) != null) ? transform.GetComponent<UILabel>() : null);
		this.uiTxtRecv = (((transform = _parent.Find("texRecv")) != null) ? transform.GetComponent<UITexture>() : null);
		this.uiTxtSent = (((transform = _parent.Find("texSent")) != null) ? transform.GetComponent<UITexture>() : null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		this.initialized = true;
		if (this.uiTxtRecv != null)
		{
			this.graphReceived = new SimpleGraph();
			this.graphReceived.Init(1024, 128, 1f, new float[] { 0.01f, 0.5f, 1f });
			this.uiTxtRecv.mainTexture = this.graphReceived.texture;
		}
		if (this.uiTxtSent != null)
		{
			this.graphSent = new SimpleGraph();
			this.graphSent.Init(1024, 128, 1f, new float[] { 0.01f, 0.5f, 1f });
			this.uiTxtSent.mainTexture = this.graphSent.texture;
		}
	}

	public void Cleanup()
	{
		SimpleGraph simpleGraph = this.graphReceived;
		if (simpleGraph != null)
		{
			simpleGraph.Cleanup();
		}
		SimpleGraph simpleGraph2 = this.graphSent;
		if (simpleGraph2 == null)
		{
			return;
		}
		simpleGraph2.Cleanup();
	}

	public void ResetAllNumbers()
	{
		this.bResetNext = true;
		this.sumBRecv = 0;
		this.totalBRecv = 0;
		this.sumBSent = 0;
		this.totalBSent = 0;
		this.sumPRecv = 0;
		this.totalPRecv = 0;
		this.sumPSent = 0;
		this.totalPSent = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDataForConnection(INetConnection[] _nc)
	{
		if (this.channel >= _nc.Length)
		{
			return;
		}
		if (_nc[this.channel] != null)
		{
			int num;
			int num2;
			int num3;
			int num4;
			_nc[this.channel].GetStats().GetStats(0f, out num, out num2, out num3, out num4);
			this.totalBytesSent += num;
			this.totalBytesReceived += num3;
			this.totalPackagesSent += num2;
			this.totalPackagesReceived += num4;
			_nc[this.channel].GetStats().GetPackageTypes(this.packagesPerTypeReceived, this.bytesPerTypeReceived, this.packagesPerTypeSent, this.bytesPerTypeSent, this.bResetNext);
			this.recSequence = _nc[this.channel].GetStats().GetLastPackagesReceived();
			this.sentSequence = _nc[this.channel].GetStats().GetLastPackagesSent();
		}
		this.bResetNext = false;
	}

	public void Update()
	{
		if (this.Enabled != this.parent.gameObject.activeSelf)
		{
			this.parent.gameObject.SetActive(this.Enabled);
		}
		if (!this.Enabled)
		{
			this.bpsRecv = (this.bpsSent = 0f);
			this.sumPRecv = (this.sumPSent = 0);
			return;
		}
		this.timePassed += Time.deltaTime;
		if (!this.initialized)
		{
			this.Init();
		}
		this.totalBytesSent = 0;
		this.totalBytesReceived = 0;
		this.totalPackagesSent = 0;
		this.totalPackagesReceived = 0;
		Array.Clear(this.packagesPerTypeReceived, 0, this.packagesPerTypeReceived.Length);
		Array.Clear(this.bytesPerTypeReceived, 0, this.bytesPerTypeReceived.Length);
		Array.Clear(this.packagesPerTypeSent, 0, this.packagesPerTypeSent.Length);
		Array.Clear(this.bytesPerTypeSent, 0, this.bytesPerTypeSent.Length);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ClientInfo clientInfo = list[i];
					this.updateDataForConnection(clientInfo.netConnection);
				}
			}
		}
		else
		{
			this.updateDataForConnection(SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer());
		}
		SimpleGraph simpleGraph = this.graphReceived;
		if (simpleGraph != null)
		{
			simpleGraph.Update((float)this.totalBytesReceived / 1024f, Color.green);
		}
		SimpleGraph simpleGraph2 = this.graphSent;
		if (simpleGraph2 != null)
		{
			simpleGraph2.Update((float)this.totalBytesSent / 1024f, Color.green);
		}
		this.sumBRecv += this.totalBytesReceived;
		this.totalBRecv += this.totalBytesReceived;
		this.sumBSent += this.totalBytesSent;
		this.totalBSent += this.totalBytesSent;
		this.sumPRecv += this.totalPackagesReceived;
		this.totalPRecv += this.totalPackagesReceived;
		this.sumPSent += this.totalPackagesSent;
		this.totalPSent += this.totalPackagesSent;
		if (this.timePassed > 1f)
		{
			this.bpsRecv = (float)this.sumBRecv / this.timePassed;
			this.bpsSent = (float)this.sumBSent / this.timePassed;
			this.sumBSent = (this.sumBRecv = 0);
			this.ppsRecv = (float)this.sumPRecv / this.timePassed;
			this.ppsSent = (float)this.sumPSent / this.timePassed;
			this.sumPRecv = (this.sumPSent = 0);
			this.timePassed = 0f;
		}
		this.uiLblOverview.text = string.Format("Overview Channel {0}:\n Recv: {1:0.00} kB/s {2:0} kB\n Sent: {3:0.00} kB/s {4:0} kB\n Recv: {5:0.0} p/s {6:0} p\n Sent: {7:0.0} p/s {8:0} p\n", new object[]
		{
			this.channel,
			this.bpsRecv / 1024f,
			(float)this.totalBRecv / 1024f,
			this.bpsSent / 1024f,
			(float)this.totalBSent / 1024f,
			this.ppsRecv,
			this.totalPRecv,
			this.ppsSent,
			this.totalPSent
		});
		StringBuilder stringBuilder = new StringBuilder();
		this.updatePackageListText(stringBuilder, this.uiLblRecvPkgCnt, this.packagesPerTypeReceived, this.bytesPerTypeReceived, "Rec");
		this.updatePackageListText(stringBuilder, this.uiLblSentPkgCnt, this.packagesPerTypeSent, this.bytesPerTypeSent, "Sent");
		if (this.uiLblRecvPkgSeq != null)
		{
			this.uiLblRecvPkgSeq.text = this.printPackageSequence(this.recSequence, "Rec last 30:");
		}
		if (this.uiLblSentPkgSeq != null)
		{
			this.uiLblSentPkgSeq.text = this.printPackageSequence(this.sentSequence, "Sent last 30:");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePackageListText(StringBuilder _sb, UILabel _label, int[] _packagesPerType, int[] _bytesPerType, string _caption)
	{
		if (_label != null)
		{
			this.sortList.Clear();
			for (int i = 0; i < _packagesPerType.Length; i++)
			{
				NetworkMonitor.SIdCnt sidCnt = new NetworkMonitor.SIdCnt
				{
					Id = i,
					Cnt = _packagesPerType[i],
					Bytes = _bytesPerType[i]
				};
				this.sortList.Add(sidCnt);
			}
			this.sortList.Sort(this.sorter);
			_sb.Length = 0;
			_sb.Append(_caption);
			_sb.Append(":\n");
			int num = 0;
			while (num < 16 && num < this.sortList.Count && this.sortList[num].Cnt != 0)
			{
				_sb.Append(string.Format("{0}. {1}: {2} - {3} B\n", new object[]
				{
					num + 1,
					NetPackageManager.GetPackageName(this.sortList[num].Id),
					this.sortList[num].Cnt,
					this.sortList[num].Bytes
				}));
				num++;
			}
			_label.text = _sb.ToString();
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
		SNetPackageInfo? snetPackageInfo = null;
		int num = 0;
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
					num = 0;
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
		return string.Format(" {0:000} {1}{2} {3} B\n", new object[]
		{
			_baseTick - _lastPackage.Tick,
			(_lastPackageCnt > 0) ? ((_lastPackageCnt + 1).ToString() + "x ") : string.Empty,
			NetPackageManager.GetPackageName(_lastPackage.Id),
			_size
		});
	}

	public bool Enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblOverview;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblRecvPkgCnt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblSentPkgCnt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblRecvPkgSeq;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblSentPkgSeq;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UITexture uiTxtRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UITexture uiTxtSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public SimpleGraph graphReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public SimpleGraph graphSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] packagesPerTypeReceived = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] packagesPerTypeSent = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] bytesPerTypeReceived = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] bytesPerTypeSent = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bResetNext;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBytesSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBytesReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalPackagesSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalPackagesReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public RingBuffer<SNetPackageInfo> recSequence;

	[PublicizedFrom(EAccessModifier.Private)]
	public RingBuffer<SNetPackageInfo> sentSequence;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timePassed;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NetworkMonitor.SIdCnt> sortList = new List<NetworkMonitor.SIdCnt>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly NetworkMonitor.SIdCntSorter sorter = new NetworkMonitor.SIdCntSorter();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Transform parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int channel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sumBRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sumBSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sumPRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sumPSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalPRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalPSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bpsRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bpsSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ppsRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ppsSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxDisplayedPackageTypes = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct SIdCnt
	{
		public int Id;

		public int Cnt;

		public int Bytes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SIdCntSorter : IComparer<NetworkMonitor.SIdCnt>
	{
		public int Compare(NetworkMonitor.SIdCnt _obj1, NetworkMonitor.SIdCnt _obj2)
		{
			return _obj2.Cnt - _obj1.Cnt;
		}
	}
}

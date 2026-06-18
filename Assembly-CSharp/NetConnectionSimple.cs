using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Noemax.GZip;
using UnityEngine.Networking;

public class NetConnectionSimple : NetConnectionAbs
{
	public NetConnectionSimple(int _channel, ClientInfo _clientInfo, INetworkClient _netClient, string _uniqueId, int _reservedHeaderBytes = 0, int _maxPacketSize = 0)
		: base(_channel, _clientInfo, _netClient, _uniqueId)
	{
		this.reservedHeaderBytes = _reservedHeaderBytes;
		this.maxPacketSize = _maxPacketSize;
		if (this.maxPacketSize > 0)
		{
			this.maxPayloadPerPacket = this.maxPacketSize - this.reservedHeaderBytes;
		}
		this.readerBuffer = new byte[4096];
		this.writerBuffer = new byte[4096];
		if (_clientInfo != null)
		{
			if (_channel == 0)
			{
				this.InitStreams(false);
			}
		}
		else
		{
			this.InitStreams(true);
		}
		this.readerThreadInfo = ThreadManager.StartThread("NCS_Reader_" + this.connectionIdentifier, new ThreadManager.ThreadFunctionDelegate(this.taskDeserialize), null, null, true, false);
		this.writerThreadInfo = ThreadManager.StartThread("NCS_Writer_" + this.connectionIdentifier, new ThreadManager.ThreadFunctionDelegate(this.taskSerialize), null, null, true, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitStreams(bool _full)
	{
		if (this.fullConnection)
		{
			return;
		}
		if (_full)
		{
			byte[] array = new byte[2097152];
			this.receiveStreamCompressed = new MemoryStream(array, 0, array.Length, true, true);
			this.receiveStreamCompressed.SetLength(0L);
			byte[] array2 = new byte[2097152];
			this.receiveStreamUncompressed = new MemoryStream(array2, 0, array2.Length, true, true);
			this.receiveZipStream = new DeflateInputStream(this.receiveStreamCompressed, true);
			byte[] array3 = new byte[2097152];
			this.reliableSendStreamUncompressed = new MemoryStream(array3, 0, array3.Length, true, true);
			this.reliableSendStreamWriter = new PooledBinaryWriter();
			this.reliableSendStreamWriter.SetBaseStream(this.reliableSendStreamUncompressed);
			byte[] array4 = new byte[2097152];
			this.unreliableSendStreamUncompressed = new MemoryStream(array4, 0, array4.Length, true, true);
			this.unreliableSendStreamWriter = new PooledBinaryWriter();
			this.unreliableSendStreamWriter.SetBaseStream(this.unreliableSendStreamUncompressed);
			byte[] array5 = new byte[2097152];
			this.sendStreamCompressed = new MemoryStream(array5, 0, array5.Length, true, true);
			this.sendZipStream = new DeflateOutputStream(this.sendStreamCompressed, 3, true);
			this.writerStream = new MemoryStream(new byte[2097152]);
			this.writerStream.SetLength(0L);
			this.fullConnection = true;
			return;
		}
		byte[] array6 = new byte[32768];
		this.receiveStreamCompressed = new MemoryStream(array6, 0, array6.Length, true, true);
		this.receiveStreamCompressed.SetLength(0L);
		byte[] array7 = new byte[32768];
		this.reliableSendStreamUncompressed = new MemoryStream(array7, 0, array7.Length, true, true);
		this.reliableSendStreamWriter = new PooledBinaryWriter();
		this.reliableSendStreamWriter.SetBaseStream(this.reliableSendStreamUncompressed);
		byte[] array8 = new byte[32768];
		this.unreliableSendStreamUncompressed = new MemoryStream(array8, 0, array8.Length, true, true);
		this.unreliableSendStreamWriter = new PooledBinaryWriter();
		this.unreliableSendStreamWriter.SetBaseStream(this.unreliableSendStreamUncompressed);
		this.writerStream = new MemoryStream(new byte[32768]);
		this.writerStream.SetLength(0L);
	}

	public override void Disconnect(bool _kick)
	{
		base.Disconnect(_kick);
		this.readerTriggerEvent.Set();
		this.writerTriggerEvent.Set();
	}

	public override void AddToSendQueue(NetPackage _package)
	{
		_package.RegisterSendQueue();
		object obj = this.writerListLockObj;
		lock (obj)
		{
			this.writerListFilling.Add(_package);
		}
	}

	public override void FlushSendQueue()
	{
		object obj = this.writerListLockObj;
		lock (obj)
		{
			this.writerTriggerEvent.Set();
		}
	}

	public override void AppendToReaderStream(byte[] _data, int _dataSize)
	{
		if (this.bDisconnected)
		{
			return;
		}
		Queue<NetConnectionSimple.RecvBuffer> queue = this.receivedBuffers;
		lock (queue)
		{
			this.receivedBuffers.Enqueue(new NetConnectionSimple.RecvBuffer(_data, _dataSize));
		}
		this.readerTriggerEvent.Set();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void taskDeserialize(ThreadManager.ThreadInfo _threadInfo)
	{
		bool flag = false;
		int num = 0;
		bool flag2 = false;
		bool flag3 = false;
		int num2 = 0;
		int num3 = 0;
		try
		{
			while (!this.bDisconnected && !_threadInfo.TerminationRequested())
			{
				this.readerTriggerEvent.WaitOne(8);
				if (this.bDisconnected)
				{
					break;
				}
				Queue<NetConnectionSimple.RecvBuffer> queue = this.receivedBuffers;
				NetConnectionSimple.RecvBuffer recvBuffer;
				lock (queue)
				{
					if (this.receivedBuffers.Count == 0)
					{
						this.readerTriggerEvent.Reset();
						continue;
					}
					recvBuffer = this.receivedBuffers.Dequeue();
				}
				int num4 = this.reservedHeaderBytes;
				if (!flag)
				{
					num3 = 0;
					num = StreamUtils.ReadInt32(recvBuffer.Data, ref num4);
					this.receiveStreamCompressed.Position = 0L;
					if (num > this.receiveStreamCompressed.Capacity)
					{
						if (this.cInfo != null)
						{
							Log.Error(string.Format("NCSimple_Deserializer (cl={0}, ch={1}) Received message with size {2} > capacity {3}", new object[]
							{
								this.cInfo.InternalId.CombinedString,
								this.channel,
								num,
								this.receiveStreamCompressed.Capacity
							}));
						}
						else
						{
							Log.Error(string.Format("NCSimple_Deserializer (ch={0}) Received message with size {1} > capacity {2}", this.channel, num, this.receiveStreamCompressed.Capacity));
						}
					}
					this.receiveStreamCompressed.SetLength((long)num);
					flag2 = StreamUtils.ReadByte(recvBuffer.Data, ref num4) == 1;
					flag3 = StreamUtils.ReadByte(recvBuffer.Data, ref num4) == 1;
					num2 = (int)StreamUtils.ReadUInt16(recvBuffer.Data, ref num4);
					if (num2 == 0)
					{
						continue;
					}
					flag = true;
				}
				while (num3 < num && num4 < recvBuffer.Size)
				{
					int num5 = recvBuffer.Size - num4;
					this.receiveStreamCompressed.Write(recvBuffer.Data, num4, num5);
					num3 += num5;
					num4 += num5;
				}
				MemoryPools.poolByte.Free(recvBuffer.Data);
				if (num3 >= num)
				{
					this.receiveStreamCompressed.Position = 0L;
					flag = false;
					num3 = 0;
					this.stats.RegisterReceivedData(num2, num);
					if (this.Decrypt(flag3, this.receiveStreamCompressed))
					{
						this.receiveStreamReader.SetBaseStream(this.Decompress(flag2, this.receiveStreamUncompressed, this.receiveZipStream, this.readerBuffer) ? this.receiveStreamUncompressed : this.receiveStreamCompressed);
						for (int i = 0; i < num2; i++)
						{
							int num6 = this.receiveStreamReader.ReadInt32();
							if (NetConnectionSimple.doCrash)
							{
								num6++;
								NetConnectionSimple.doCrash = false;
							}
							long position = this.receiveStreamReader.BaseStream.Position;
							NetPackage netPackage = NetPackageManager.ParsePackage(this.receiveStreamReader, this.cInfo);
							int num7 = (int)(this.receiveStreamReader.BaseStream.Position - position);
							if (num7 != num6)
							{
								string[] array = new string[6];
								array[0] = "Parsed data size (";
								array[1] = num7.ToString();
								array[2] = ") does not match expected size (";
								array[3] = num6.ToString();
								array[4] = ") in ";
								int num8 = 5;
								NetPackage netPackage2 = netPackage;
								array[num8] = ((netPackage2 != null) ? netPackage2.ToString() : null);
								throw new InvalidDataException(string.Concat(array));
							}
							List<NetPackage> receivedPackages = this.receivedPackages;
							lock (receivedPackages)
							{
								this.receivedPackages.Add(netPackage);
							}
							int packageId = netPackage.PackageId;
							this.stats.RegisterReceivedPackage(packageId, num7);
							NetPackageLogger.LogPackage(false, this.cInfo, netPackage, this.channel, num7, flag3, flag2, i + 1, num2);
							if (ConnectionManager.VerboseNetLogging)
							{
								if (this.cInfo != null)
								{
									Log.Out("NCSimple deserialized (cl={3}, ch={0}): {1}, size={2}", new object[]
									{
										this.channel,
										NetPackageManager.GetPackageName(packageId),
										num7,
										this.cInfo.InternalId.CombinedString
									});
								}
								else
								{
									Log.Out("NCSimple deserialized (ch={0}): {1}, size={2}", new object[]
									{
										this.channel,
										NetPackageManager.GetPackageName(packageId),
										num7
									});
								}
							}
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (this.cInfo != null)
			{
				Log.Error(string.Format("NCSimple_Deserializer (cl={0}, ch={1}) Message: {2}", this.cInfo.InternalId.CombinedString, this.channel, ex.Message));
			}
			else
			{
				Log.Error(string.Format("NCSimple_Deserializer (ch={0}) Message: {1}", this.channel, ex.Message));
			}
			Log.Exception(ex);
			if (this.isServer)
			{
				this.Disconnect(true);
			}
			else
			{
				GameUtils.ForceDisconnect();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void taskSerialize(ThreadManager.ThreadInfo _threadInfo)
	{
		this.writerListProcessing.Clear();
		bool flag = false;
		MicroStopwatch microStopwatch = new MicroStopwatch();
		try
		{
			while (!this.bDisconnected && !_threadInfo.TerminationRequested())
			{
				if ((!flag || microStopwatch.ElapsedMilliseconds > 500L) && (this.reliableBufsToSend.Count > 0 || this.unreliableBufsToSend.Count > 0))
				{
					try
					{
						NetworkError networkError = this.SendBuffers();
						flag = networkError > NetworkError.Ok;
						switch (networkError)
						{
						case NetworkError.Ok:
							goto IL_0167;
						case NetworkError.WrongHost:
						case NetworkError.WrongChannel:
							goto IL_00FF;
						case NetworkError.WrongConnection:
							break;
						case NetworkError.NoResources:
							if (this.isServer)
							{
								Log.Warning("NET No resources to send data to client ({0}) on channel {2}, backing off for {1} ms", new object[]
								{
									this.cInfo.ToString(),
									500,
									this.channel
								});
							}
							else
							{
								Log.Warning("NET No resources to send data to server on channel {1}, backing off for {0} ms", new object[] { 500, this.channel });
							}
							microStopwatch.ResetAndRestart();
							goto IL_0167;
						default:
							if (networkError != NetworkError.WrongOperation)
							{
								goto IL_00FF;
							}
							break;
						}
						this.Disconnect(true);
						goto IL_0167;
						IL_00FF:
						if (this.isServer)
						{
							Log.Warning("NET Unexpected result '{2}' trying to send data to client ({0}) on channel {1}", new object[]
							{
								this.cInfo.ToString(),
								this.channel,
								networkError.ToStringCached<NetworkError>()
							});
						}
						else
						{
							Log.Warning("NET Unexpected result '{1}' trying to send data to server on channel {0}", new object[]
							{
								this.channel,
								networkError.ToStringCached<NetworkError>()
							});
						}
						IL_0167:;
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						this.Disconnect(true);
					}
				}
				this.writerTriggerEvent.WaitOne(4);
				if (this.bDisconnected)
				{
					break;
				}
				object obj = this.writerListLockObj;
				int count;
				lock (obj)
				{
					count = this.writerListFilling.Count;
				}
				if (count == 0)
				{
					this.writerTriggerEvent.Reset();
				}
				else
				{
					obj = this.writerListLockObj;
					lock (obj)
					{
						List<NetPackage> list = this.writerListFilling;
						List<NetPackage> list2 = this.writerListProcessing;
						this.writerListProcessing = list;
						this.writerListFilling = list2;
						this.writerListFilling.Clear();
					}
					if (this.writerListProcessing.Count != 0)
					{
						this.reliableSendStreamUncompressed.Position = 0L;
						this.reliableSendStreamUncompressed.SetLength(0L);
						this.unreliableSendStreamUncompressed.Position = 0L;
						this.unreliableSendStreamUncompressed.SetLength(0L);
						int num = 0;
						int num2 = 0;
						int num3 = 0;
						int num4 = 0;
						for (int i = 0; i < this.writerListProcessing.Count; i++)
						{
							NetPackage netPackage = this.writerListProcessing[i];
							bool flag3;
							if (netPackage.ReliableDelivery || !GameManager.unreliableNetPackets)
							{
								flag3 = this.WriteToStream(num3, i, ref num, ref netPackage, ref this.reliableSendStreamWriter, ref this.reliableSendStreamUncompressed);
								num3++;
							}
							else
							{
								long position = this.unreliableSendStreamWriter.BaseStream.Position;
								if (this.cInfo != null && position + (long)netPackage.GetLength() >= (long)this.cInfo.network.GetMaximumPacketSize(this.cInfo, false))
								{
									flag3 = this.WriteToStream(num3, i, ref num, ref netPackage, ref this.reliableSendStreamWriter, ref this.reliableSendStreamUncompressed);
									num3++;
								}
								else
								{
									flag3 = this.WriteToStream(num4, i, ref num2, ref netPackage, ref this.unreliableSendStreamWriter, ref this.unreliableSendStreamUncompressed);
									num4++;
								}
							}
							if (!flag3)
							{
								break;
							}
						}
						if (this.reliableSendStreamUncompressed.Length > 0L)
						{
							this.StreamToBuffer(ref this.reliableSendStreamUncompressed, true, ref num, microStopwatch.ElapsedMilliseconds);
						}
						if (this.unreliableSendStreamUncompressed.Length > 0L)
						{
							this.StreamToBuffer(ref this.unreliableSendStreamUncompressed, false, ref num2, microStopwatch.ElapsedMilliseconds);
						}
					}
				}
			}
		}
		catch (Exception ex2)
		{
			if (this.cInfo != null)
			{
				Log.Error(string.Format("NCSimple_Serializer (cl={0}, ch={1}) Message: {2}", this.cInfo.InternalId.CombinedString, this.channel, ex2.Message));
			}
			else
			{
				Log.Error(string.Format("NCSimple_Serializer (ch={0}) Message: {1}", this.channel, ex2.Message));
			}
			Log.Exception(ex2);
			if (this.isServer)
			{
				this.Disconnect(true);
			}
			else
			{
				GameUtils.ForceDisconnect();
			}
		}
	}

	public int preCompressMaxBufferSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this.allowCompression)
			{
				return 32256;
			}
			return 2097152;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool WriteToStream(int streamIndex, int packageIndex, ref int packagesToSend, ref NetPackage package, ref PooledBinaryWriter writer, ref MemoryStream stream)
	{
		long position = writer.BaseStream.Position;
		if (streamIndex > 0 && position + (long)package.GetLength() >= (long)this.preCompressMaxBufferSize)
		{
			object obj = this.writerListLockObj;
			lock (obj)
			{
				for (int i = this.writerListProcessing.Count - 1; i >= packageIndex; i--)
				{
					this.writerListFilling.Insert(0, this.writerListProcessing[i]);
				}
			}
			return false;
		}
		bool flag;
		try
		{
			writer.Write(-1);
			package.write(writer);
			int num = (int)(writer.BaseStream.Position - position - 4L);
			writer.BaseStream.Position = position;
			writer.Write(num);
			writer.BaseStream.Position = writer.BaseStream.Length;
			packagesToSend++;
			int packageId = package.PackageId;
			this.stats.RegisterSentPackage(packageId, num);
			NetPackageLogger.LogPackage(true, this.cInfo, package, this.channel, num, this.EnableEncryptData(), false, packagesToSend, -1);
			if (ConnectionManager.VerboseNetLogging)
			{
				if (this.cInfo != null)
				{
					Log.Out("NCSimple serialized (cl={3}, ch={0}): {1}, size={2}", new object[]
					{
						this.channel,
						NetPackageManager.GetPackageName(packageId),
						num,
						this.cInfo.InternalId.CombinedString
					});
				}
				else
				{
					Log.Out("NCSimple serialized (ch={0}): {1}, size={2}", new object[]
					{
						this.channel,
						NetPackageManager.GetPackageName(packageId),
						num
					});
				}
			}
			package.SendQueueHandled();
			flag = true;
		}
		catch (Exception ex)
		{
			if (packagesToSend > 0)
			{
				string text;
				if (this.cInfo != null)
				{
					text = string.Format("(cl={0}, ch={1})", this.cInfo.InternalId.CombinedString, this.channel);
				}
				else
				{
					text = string.Format("(ch={0})", this.channel);
				}
				Log.Exception(ex);
				string[] array = new string[10];
				array[0] = "Failed writing ";
				int num2 = 1;
				NetPackage netPackage = package;
				array[num2] = ((netPackage != null) ? netPackage.ToString() : null);
				array[2] = " to client ";
				array[3] = text;
				array[4] = ", requeueing ";
				array[5] = (this.writerListProcessing.Count - packageIndex).ToString();
				array[6] = " packages. Stream index: ";
				array[7] = (streamIndex + 1).ToString();
				array[8] = " - stream size before: ";
				array[9] = position.ToString();
				Log.Warning(string.Concat(array));
				Log.Warning("Packages in stream:");
				int[] array2 = new int[NetPackageManager.KnownPackageCount];
				for (int j = 0; j < packageIndex; j++)
				{
					array2[this.writerListProcessing[j].PackageId]++;
				}
				for (int k = 0; k < array2.Length; k++)
				{
					if (array2[k] > 0)
					{
						Log.Warning("   " + NetPackageManager.GetPackageName(k) + ": " + array2[k].ToString());
					}
				}
				object obj = this.writerListLockObj;
				lock (obj)
				{
					for (int l = this.writerListProcessing.Count - 1; l >= packageIndex; l--)
					{
						this.writerListFilling.Insert(0, this.writerListProcessing[l]);
					}
				}
				stream.SetLength(position);
				this.writerListProcessing.Clear();
				flag = false;
			}
			else
			{
				Log.Exception(ex);
				string[] array3 = new string[7];
				array3[0] = "Failed writing first package: ";
				int num3 = 1;
				NetPackage netPackage2 = package;
				array3[num3] = ((netPackage2 != null) ? netPackage2.ToString() : null);
				array3[2] = " of size ";
				array3[3] = package.GetLength().ToString();
				array3[4] = ". ";
				array3[5] = (this.writerListProcessing.Count - packageIndex).ToString();
				array3[6] = " remaining packages in queue.";
				Log.Warning(string.Concat(array3));
				package.SendQueueHandled();
				flag = false;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StreamToBuffer(ref MemoryStream uncompressedStream, bool sendReliable, ref int packagesToSend, long milliseconds)
	{
		if (uncompressedStream.Length == 0L)
		{
			return;
		}
		this.writerListProcessing.Clear();
		uncompressedStream.Position = 0L;
		long length = uncompressedStream.Length;
		bool flag = this.allowCompression && uncompressedStream.Length > 500L;
		MemoryStream memoryStream = uncompressedStream;
		if (this.Compress(flag, uncompressedStream, this.sendZipStream, this.sendStreamCompressed, this.writerBuffer, packagesToSend))
		{
			memoryStream = this.sendStreamCompressed;
			long length2 = this.sendStreamCompressed.Length;
		}
		bool flag2 = this.Encrypt(memoryStream);
		this.stats.RegisterSentData(packagesToSend, (int)memoryStream.Length);
		StreamUtils.Write(this.writerStream, (int)memoryStream.Length);
		StreamUtils.Write(this.writerStream, flag ? 1 : 0);
		StreamUtils.Write(this.writerStream, flag2 ? 1 : 0);
		StreamUtils.Write(this.writerStream, (ushort)packagesToSend);
		if (memoryStream.Length > (long)this.writerStream.Capacity)
		{
			Log.Error(string.Format("Source stream size ({0}) > writer stream capacity ({1}), packages: {2}, compressed: {3}", new object[]
			{
				memoryStream.Length,
				this.writerStream.Capacity,
				packagesToSend,
				flag
			}));
		}
		StreamUtils.StreamCopy(memoryStream, this.writerStream, this.writerBuffer, true);
		this.writerStream.Position = 0L;
		ArrayListMP<byte> arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, (int)this.writerStream.Length + this.reservedHeaderBytes);
		this.writerStream.Read(arrayListMP.Items, this.reservedHeaderBytes, (int)this.writerStream.Length);
		arrayListMP.Count = (int)(this.writerStream.Length + (long)this.reservedHeaderBytes);
		if (sendReliable)
		{
			this.reliableBufsToSend.AddLast(arrayListMP);
		}
		else
		{
			this.unreliableBufsToSend.AddLast(arrayListMP);
		}
		this.writerStream.SetLength(0L);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetworkError SendBuffers()
	{
		NetworkError networkError;
		if ((networkError = this.sendBuffersFromQueue(this.reliableBufsToSend, true)) != NetworkError.Ok)
		{
			return networkError;
		}
		if ((networkError = this.sendBuffersFromQueue(this.unreliableBufsToSend, false)) != NetworkError.Ok)
		{
			return networkError;
		}
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetworkError sendBuffersFromQueue(LinkedList<ArrayListMP<byte>> _sendQueue, bool _reliableDelivery)
	{
		while (_sendQueue.Count > 0)
		{
			ArrayListMP<byte> arrayListMP = _sendQueue.First.Value;
			_sendQueue.RemoveFirst();
			NetworkError networkError = NetworkError.Ok;
			if (!this.bDisconnected)
			{
				if (this.maxPacketSize > 0 && arrayListMP.Count > this.maxPacketSize)
				{
					arrayListMP = this.splitSendBuffer(arrayListMP, _sendQueue);
				}
				if (this.isServer)
				{
					networkError = this.cInfo.network.SendData(this.cInfo, this.channel, arrayListMP, _reliableDelivery);
				}
				else
				{
					networkError = this.netClient.SendData(this.channel, arrayListMP);
				}
			}
			if (networkError != NetworkError.Ok)
			{
				_sendQueue.AddFirst(arrayListMP);
				return networkError;
			}
		}
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<byte> splitSendBuffer(ArrayListMP<byte> _inBuf, LinkedList<ArrayListMP<byte>> _sendQueue)
	{
		int num = _inBuf.Count - this.reservedHeaderBytes;
		int num2 = num / this.maxPayloadPerPacket;
		if (num2 * this.maxPayloadPerPacket < num)
		{
			num2++;
		}
		for (int i = num2 - 1; i >= 0; i--)
		{
			int num3 = i * this.maxPayloadPerPacket;
			int num4;
			if (i == num2 - 1)
			{
				num4 = num - num3;
			}
			else
			{
				num4 = this.maxPayloadPerPacket;
			}
			ArrayListMP<byte> arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, num4 + this.reservedHeaderBytes);
			Array.Copy(_inBuf.Items, num3 + this.reservedHeaderBytes, arrayListMP.Items, 1, num4);
			arrayListMP.Count = num4 + this.reservedHeaderBytes;
			if (i <= 0)
			{
				return arrayListMP;
			}
			_sendQueue.AddFirst(arrayListMP);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int reservedHeaderBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int maxPacketSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int maxPayloadPerPacket;

	public static bool doCrash;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object writerListLockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetPackage> writerListFilling = new List<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetPackage> writerListProcessing = new List<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ManualResetEvent writerTriggerEvent = new ManualResetEvent(false);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] writerBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo writerThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryWriter reliableSendStreamWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryWriter unreliableSendStreamWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream reliableSendStreamUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream unreliableSendStreamUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream sendStreamCompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public DeflateOutputStream sendZipStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream writerStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<ArrayListMP<byte>> reliableBufsToSend = new LinkedList<ArrayListMP<byte>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<ArrayListMP<byte>> unreliableBufsToSend = new LinkedList<ArrayListMP<byte>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ManualResetEvent readerTriggerEvent = new ManualResetEvent(false);

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo readerThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] readerBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream receiveStreamCompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream receiveStreamUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public DeflateInputStream receiveZipStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly PooledBinaryReader receiveStreamReader = new PooledBinaryReader();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<NetConnectionSimple.RecvBuffer> receivedBuffers = new Queue<NetConnectionSimple.RecvBuffer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int networkErrorCooldownMs = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct RecvBuffer
	{
		public RecvBuffer(byte[] _data, int _size)
		{
			this.Data = _data;
			this.Size = _size;
		}

		public readonly byte[] Data;

		public readonly int Size;
	}
}

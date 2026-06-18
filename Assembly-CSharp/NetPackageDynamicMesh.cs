using System;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDynamicMesh : DynamicMeshServerData, IMemoryPoolableObject
{
	public void Setup(DynamicMeshItem item, byte[] byteArray)
	{
		this.Item = item;
		this.bytes = byteArray;
		this.X = item.WorldPosition.x;
		this.Z = item.WorldPosition.z;
		this.UpdateTime = item.UpdateTime;
		DynamicMeshItem item2 = this.Item;
		this.PresumedLength = ((item2 != null) ? item2.PackageLength : 0);
	}

	public override bool Prechecks()
	{
		return true;
	}

	public override bool FlushQueue
	{
		get
		{
			return true;
		}
	}

	public override void read(PooledBinaryReader reader)
	{
		this.X = reader.ReadInt32();
		this.Z = reader.ReadInt32();
		this.UpdateTime = reader.ReadInt32();
		int num = reader.ReadInt32();
		NetPackageDynamicMesh.Count++;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			string text = string.Concat(new string[]
			{
				DynamicMeshFile.MeshLocation,
				this.X.ToString(),
				",",
				this.Z.ToString(),
				".mesh"
			});
			if (DynamicMeshManager.DoLog)
			{
				Log.Out(string.Format("Reading {0},{1} len {2}", this.X, this.Z, num));
			}
			if (num == 0)
			{
				if (DynamicMeshManager.Instance != null)
				{
					DynamicMeshManager.Instance.ArrangeChunkRemoval(this.X, this.Z);
				}
				this.bytes = null;
				return;
			}
			int num2 = 0;
			this.bytes = DynamicMeshThread.ChunkDataQueue.GetFromPool(num);
			reader.Read(this.bytes, 0, num);
			if (string.IsNullOrWhiteSpace(DynamicMeshFile.MeshLocation))
			{
				this.IsValidUpdate = false;
				return;
			}
			while (num2++ < 10)
			{
				try
				{
					this.IsValidUpdate = DynamicMeshThread.ChunkDataQueue.SaveNetPackageData(this.X, this.Z, this.bytes, this.UpdateTime);
					break;
				}
				catch (Exception)
				{
					Log.Out(string.Concat(new string[]
					{
						"Failed attempt ",
						num2.ToString(),
						" to write mesh ",
						text,
						". Retrying..."
					}));
					Thread.Sleep(1000);
				}
			}
		}
	}

	public override void write(PooledBinaryWriter writer)
	{
		base.write(writer);
		writer.Write(this.X);
		writer.Write(this.Z);
		writer.Write(this.UpdateTime);
		int num = this.PresumedLength;
		long position = writer.BaseStream.Position;
		string text = "start";
		try
		{
			text = "len";
			if (DynamicMeshManager.DoLog)
			{
				Log.Out(string.Format("Sending {0},{1} len {2}", this.X, this.Z, num));
			}
			text = "lencheck";
			if (NetPackageDynamicMesh.MaxLength < num)
			{
				NetPackageDynamicMesh.MaxLength = num;
				if (DynamicMeshManager.DoLog)
				{
					Log.Out("New dyMesh maxLen: " + NetPackageDynamicMesh.MaxLength.ToString());
				}
			}
			NetPackageDynamicMesh.LastLength = num;
			NetPackageDynamicMesh.LastX = this.X;
			NetPackageDynamicMesh.LastZ = this.Z;
			text = "writelen";
			writer.Write(num);
			if (this.bytes != null)
			{
				text = "writebytes";
				if (this.bytes.Length < num)
				{
					text = "writebytecheck";
					Log.Warning("Dymesh byte length was lower than expected. Len was " + num.ToString() + " and bytes were " + this.bytes.Length.ToString());
					num = (this.PresumedLength = this.bytes.Length);
				}
				text = "writenow";
				writer.Write(this.bytes, 0, num);
			}
		}
		catch (Exception ex)
		{
			string text2 = " ERROR MESH EXCEPTION\r\ndyMesh netWrite error: {0}\r\n({1},{2})\r\nLength: {3}\r\nLen: {4}\r\nbytes: {5}\r\nMaxLength: {6}\r\nwriterStartPosition: {7}\r\nwriterLength: {8}\r\nwriterPos: {9}\r\nstep: {10}\r\n";
			object[] array = new object[11];
			array[0] = ex.Message;
			array[1] = this.X;
			array[2] = this.Z;
			array[3] = num;
			array[4] = num;
			int num2 = 5;
			byte[] array2 = this.bytes;
			array[num2] = ((array2 != null) ? array2.Length.ToString() : null) ?? "null";
			array[6] = NetPackageDynamicMesh.MaxLength;
			array[7] = position;
			array[8] = writer.BaseStream.Length;
			array[9] = writer.BaseStream.Position;
			array[10] = text;
			Log.Error(string.Format(text2, array));
			throw ex;
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.Both;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicMeshServer.ClientReadyForNextMesh(this);
			return;
		}
		if (this.IsValidUpdate)
		{
			DynamicMeshManager.AddDataFromServer(this.X, this.Z);
		}
		NetPackageDynamicMesh package = NetPackageManager.GetPackage<NetPackageDynamicMesh>();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
	}

	public override int GetLength()
	{
		if (this.PresumedLength > 0)
		{
			return Math.Min(this.PresumedLength, NetPackageDynamicMesh.MaxMessageSize);
		}
		if (this.bytes != null)
		{
			return 16 + this.bytes.Length;
		}
		return 16;
	}

	public void Reset()
	{
		DynamicMeshServer.SyncRelease(this.Item);
		this.Item = null;
		this.Attempts = 0;
		this.bytes = null;
		this.PresumedLength = 0;
	}

	public void Cleanup()
	{
		this.Reset();
	}

	public override int Channel
	{
		get
		{
			return 1;
		}
	}

	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public static byte[] DelayedMessageBytes = new byte[1];

	public static int MaxMessageSize = 2097152;

	public static int MaxLength = 0;

	public static int LastLength;

	public static int LastZ;

	public static int LastX;

	public static int Count = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshItem Item;

	public int Attempts;

	public int PresumedLength;

	public int UpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] bytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidUpdate;
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DynamicMeshDebugConsoleCmd : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return false;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params == null || _params.Count == 0)
		{
			DynamicMeshManager.Instance.AddChunk(new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition()), true);
			return;
		}
		string text = _params[0].ToLower();
		if (text == "ss")
		{
			DynamicMeshServer.ShowSender = true;
			return;
		}
		if (text == "dolog")
		{
			DynamicMeshManager.DoLog = !DynamicMeshManager.DoLog;
			Log.Out("DyMesh doLog: " + DynamicMeshManager.DoLog.ToString());
			return;
		}
		if (text == "lognet")
		{
			DynamicMeshManager.DoLogNet = !DynamicMeshManager.DoLogNet;
			Log.Out("DoLogNet: " + DynamicMeshManager.DoLogNet.ToString());
			return;
		}
		if (text == "tars")
		{
			Vector3 position = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId].position;
			DynamicMeshManager.ImportVox("tars", position, 502);
			return;
		}
		if (text == "vox")
		{
			string param = this.GetParam(_params, 1);
			Vector3 position2 = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId].position;
			int num;
			int.TryParse(this.GetParam(_params, 2) ?? "502", out num);
			DynamicMeshManager.ImportVox(param, position2, num);
			return;
		}
		if (text == "areaaround" || text == "aa")
		{
			int num2 = ((_params.Count < 2) ? 150 : int.Parse(_params[1]));
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			Vector3 vector;
			if (primaryPlayer == null)
			{
				vector = GameManager.Instance.World.GetPlayers().FirstOrDefault((EntityPlayer d) => d.entityId == _senderInfo.RemoteClientInfo.entityId).position;
			}
			else
			{
				vector = primaryPlayer.position;
			}
			int num3 = (int)vector.x - num2;
			while ((float)num3 < vector.x + (float)num2)
			{
				int num4 = (int)vector.z - num2;
				while ((float)num4 < vector.z + (float)num2)
				{
					DynamicMeshManager.Instance.AddChunk(new Vector3i(num3, 0, num4), true);
					num4 += 16;
				}
				num3 += 16;
			}
			string text2 = "Adding chunks around  ";
			Vector3 vector2 = vector;
			Log.Out(text2 + vector2.ToString() + " rad: " + num2.ToString());
			return;
		}
		if (text == "air")
		{
			if (_params.Count < 1)
			{
				DynamicMeshManager.LogMsg("Specify a radius");
				return;
			}
			int num5 = int.Parse(this.GetParam(_params, 1));
			BlockValue air = BlockValue.Air;
			Vector3i vector3i = new Vector3i(GameManager.Instance.World.Players.list.FirstOrDefault((EntityPlayer d) => d.entityId == _senderInfo.RemoteClientInfo.entityId).GetPosition());
			World world = GameManager.Instance.World;
			for (int i = vector3i.x - num5; i < vector3i.x + num5; i++)
			{
				for (int j = vector3i.z - num5; j < vector3i.z + num5; j++)
				{
					Vector3i vector3i2 = new Vector3i(i, vector3i.y, j);
					if (world.GetBlock(vector3i2).type != 0)
					{
						world.SetBlockRPC(vector3i2, air);
					}
				}
			}
			return;
		}
		else
		{
			if (text == "log")
			{
				string text3 = string.Concat(new string[]
				{
					"Buff : ",
					DynamicMeshManager.Instance.BufferRegionLoadRequests.Count.ToString(),
					"\n Server : ",
					SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer.ToString(),
					" / ",
					SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient.ToString(),
					"\n Thread P/S : ",
					DynamicMeshThread.PrimaryQueue.Count.ToString(),
					" / ",
					DynamicMeshThread.SecondaryQueue.Count.ToString(),
					"\n Packets: ",
					NetPackageDynamicMesh.Count.ToString(),
					"\n ThreadDistance: ",
					DynamicMeshManager.ThreadDistance.ToString(),
					"\n ObserverDistance: ",
					DynamicMeshManager.ObserverDistance.ToString(),
					"\n ItemLoadDistance: ",
					DynamicMeshManager.ItemLoadDistance.ToString(),
					"\n WorldChunks: ",
					GameManager.Instance.World.ChunkCache.chunks.list.Count.ToString(),
					"\n ThreadNext: ",
					DynamicMeshThread.nextChunks.Count.ToString(),
					"\n ThreadQueue: ",
					DynamicMeshThread.Queue,
					"\n RegionUpdates: ",
					DynamicMeshThread.RegionUpdates.Count.ToString(),
					"\n RegionUpdatesDebug: ",
					DynamicMeshThread.RegionUpdatesDebug,
					"\n SyncPackets: ",
					DynamicMeshServer.SyncRequests.Count.ToString(),
					" (",
					DynamicMeshServer.ActiveSyncs.Count.ToString(),
					")"
				});
				foreach (DynamicMeshClientConnection dynamicMeshClientConnection in DynamicMeshServer.ClientData.Values)
				{
					string[] array = new string[5];
					array[0] = text3;
					array[1] = "\n ";
					array[2] = dynamicMeshClientConnection.EntityId.ToString();
					array[3] = ": ";
					array[4] = dynamicMeshClientConnection.ItemsToSend.Values.Sum((ConcurrentQueue<DynamicMeshSyncRequest> d) => d.Count).ToString();
					text3 = string.Concat(array);
				}
				Log.Out("Info: " + text3);
				return;
			}
			if (text == "fog")
			{
				float minValue = float.MinValue;
				float minValue2 = float.MinValue;
				if (_params.Count >= 1)
				{
					float num6 = StringParsers.ParseFloat(_params[1], 0, -1, NumberStyles.Any);
					SkyManager.SetFogDebug(num6, minValue, minValue2);
					Log.Out("Fog " + num6.ToString());
					return;
				}
			}
			else
			{
				if (text == "checkprefabs" || text == "cp" || text == "forcegen")
				{
					DynamicMeshManager.Instance.CheckPrefabs("Console", true);
					return;
				}
				if (text == "resend")
				{
					DynamicMeshServer.ResendPackages = !DynamicMeshServer.ResendPackages;
					Log.Out("Resending dymesh packages: " + DynamicMeshServer.ResendPackages.ToString());
				}
			}
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetParam(List<string> _params, int index)
	{
		if (_params == null)
		{
			return null;
		}
		if (index >= _params.Count)
		{
			return null;
		}
		return _params[index];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetParamAsInt(List<string> _params, int index)
	{
		int num = -9999;
		if (_params == null)
		{
			return num;
		}
		if (index >= _params.Count)
		{
			return num;
		}
		int.TryParse(_params[index], out num);
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			DynamicMeshDebugConsoleCmd.info,
			"zd"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return DynamicMeshDebugConsoleCmd.info;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string info = "Dynamic mesh debug";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool GC_ENABLED = true;
}

using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSimpleChat : NetPackage
{
	public NetPackageSimpleChat Setup(string _msg)
	{
		this.msg = (string.IsNullOrEmpty(_msg) ? string.Empty : _msg);
		return this;
	}

	public NetPackageSimpleChat Setup(string _msg, List<int> _recipientEntityIds)
	{
		this.msg = (string.IsNullOrEmpty(_msg) ? string.Empty : _msg);
		this.recipientEntityIds = _recipientEntityIds;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.msg = _br.ReadString();
		int num = _br.ReadInt32();
		if (num > 0)
		{
			this.recipientEntityIds = new List<int>();
			for (int i = 0; i < num; i++)
			{
				this.recipientEntityIds.Add(_br.ReadInt32());
			}
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.msg);
		_bw.Write((this.recipientEntityIds != null) ? this.recipientEntityIds.Count : 0);
		if (this.recipientEntityIds != null && this.recipientEntityIds.Count > 0)
		{
			for (int i = 0; i < this.recipientEntityIds.Count; i++)
			{
				_bw.Write(this.recipientEntityIds[i]);
			}
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			if (this.recipientEntityIds == null)
			{
				return;
			}
			using (List<int>.Enumerator enumerator = this.recipientEntityIds.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int num = enumerator.Current;
					EntityPlayerLocal entityPlayerLocal = _world.GetEntity(num) as EntityPlayerLocal;
					if (entityPlayerLocal != null)
					{
						LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
						if (null != uiforPlayer && null != uiforPlayer.windowManager)
						{
							XUiC_ChatOutput.AddMessage(uiforPlayer.xui, EnumGameMessages.PlainTextLocal, this.msg, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.Server, GeneratedTextManager.TextFilteringMode.None, GeneratedTextManager.BbCodeSupportMode.Supported);
						}
					}
					else
					{
						ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(num);
						if (clientInfo != null)
						{
							clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSimpleChat>().Setup(this.msg));
						}
					}
				}
				return;
			}
		}
		List<EntityPlayerLocal> localPlayers = _callbacks.World.GetLocalPlayers();
		for (int i = 0; i < localPlayers.Count; i++)
		{
			LocalPlayerUI uiforPlayer2 = LocalPlayerUI.GetUIForPlayer(localPlayers[i]);
			if (null != uiforPlayer2 && null != uiforPlayer2.windowManager)
			{
				XUiC_ChatOutput.AddMessage(uiforPlayer2.xui, EnumGameMessages.PlainTextLocal, this.msg, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.Server, GeneratedTextManager.TextFilteringMode.None, GeneratedTextManager.BbCodeSupportMode.Supported);
			}
		}
	}

	public override int GetLength()
	{
		return 4 + this.msg.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> recipientEntityIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string msg;
}

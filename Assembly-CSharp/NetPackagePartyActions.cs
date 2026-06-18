using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePartyActions : NetPackage
{
	public NetPackagePartyActions Setup(NetPackagePartyActions.PartyActions _operation, int _invitedByEntityID, int _invitedEntityID, int[] _partyMembers = null, string _voiceLobbyId = null)
	{
		this.currentOperation = _operation;
		this.invitedByEntityID = _invitedByEntityID;
		this.invitedEntityID = _invitedEntityID;
		this.partyMembers = _partyMembers;
		this.voiceLobbyId = _voiceLobbyId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.currentOperation = (NetPackagePartyActions.PartyActions)_br.ReadByte();
		this.invitedByEntityID = _br.ReadInt32();
		this.invitedEntityID = _br.ReadInt32();
		this.voiceLobbyId = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.currentOperation);
		_bw.Write(this.invitedByEntityID);
		_bw.Write(this.invitedEntityID);
		_bw.Write(this.voiceLobbyId ?? "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = _world.GetEntity(this.invitedEntityID) as EntityPlayer;
		EntityPlayer entityPlayer2 = _world.GetEntity(this.invitedByEntityID) as EntityPlayer;
		if (entityPlayer == null || entityPlayer2 == null)
		{
			return;
		}
		switch (this.currentOperation)
		{
		case NetPackagePartyActions.PartyActions.SendInvite:
			if (entityPlayer2.HasPendingPartyInvite(this.invitedEntityID))
			{
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.AcceptInvite, this.invitedEntityID, this.invitedByEntityID, null, null), false);
					return;
				}
				if (!entityPlayer2.IsInParty())
				{
					Party.ServerHandleAcceptInvite(entityPlayer, entityPlayer2);
					return;
				}
				if (!entityPlayer.IsInParty())
				{
					Party.ServerHandleAcceptInvite(entityPlayer2, entityPlayer);
					return;
				}
			}
			else if (!entityPlayer.IsInParty())
			{
				entityPlayer.AddPartyInvite(this.invitedByEntityID);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, this.invitedByEntityID, this.invitedEntityID, null, null), false, -1, -1, -1, null, 192, false);
				}
				EntityPlayerLocal entityPlayerLocal = entityPlayer as EntityPlayerLocal;
				if (entityPlayerLocal != null)
				{
					GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttPartyInviteReceived", false, null), entityPlayer2.PlayerDisplayName), null, "party_invite_receive", null, false, false, 0f);
					return;
				}
			}
			break;
		case NetPackagePartyActions.PartyActions.AcceptInvite:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityPlayer.Party == null)
			{
				Party.ServerHandleAcceptInvite(entityPlayer2, entityPlayer);
				return;
			}
			break;
		case NetPackagePartyActions.PartyActions.ChangeLead:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Party.ServerHandleChangeLead(entityPlayer);
				return;
			}
			break;
		case NetPackagePartyActions.PartyActions.LeaveParty:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityPlayer.Party != null)
			{
				Party.ServerHandleLeaveParty(entityPlayer, this.invitedEntityID);
				return;
			}
			break;
		case NetPackagePartyActions.PartyActions.KickFromParty:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityPlayer.Party != null)
			{
				Party.ServerHandleKickParty(this.invitedEntityID);
				return;
			}
			break;
		case NetPackagePartyActions.PartyActions.Disconnected:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityPlayer.Party != null)
			{
				Party.ServerHandleDisconnectParty(entityPlayer);
				return;
			}
			break;
		case NetPackagePartyActions.PartyActions.JoinAutoParty:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Party.ServerHandleAutoJoinParty(entityPlayer);
				return;
			}
			break;
		case NetPackagePartyActions.PartyActions.SetVoiceLobby:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Party.ServerHandleSetVoiceLoby(entityPlayer, this.voiceLobbyId);
			}
			break;
		default:
			return;
		}
	}

	public override int GetLength()
	{
		return 9;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int invitedByEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int invitedEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public string voiceLobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackagePartyActions.PartyActions currentOperation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] partyMembers;

	public enum PartyActions
	{
		SendInvite,
		AcceptInvite,
		ChangeLead,
		LeaveParty,
		KickFromParty,
		Disconnected,
		JoinAutoParty,
		SetVoiceLobby
	}
}

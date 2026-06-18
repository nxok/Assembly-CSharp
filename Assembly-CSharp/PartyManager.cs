using System;
using System.Collections.Generic;

public class PartyManager
{
	public static PartyManager Current
	{
		get
		{
			PartyManager partyManager;
			if ((partyManager = PartyManager.instance) == null)
			{
				partyManager = (PartyManager.instance = new PartyManager());
			}
			return partyManager;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return PartyManager.instance != null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PartyManager()
	{
		this.voice = PartyVoice.Instance;
	}

	public Party CreateParty()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return null;
		}
		Party party = new Party();
		int num = this.nextPartyID + 1;
		this.nextPartyID = num;
		party.PartyID = num;
		Party party2 = party;
		this.partyList.Add(party2);
		return party2;
	}

	public Party CreateClientParty(World world, int partyID, int leaderIndex, int[] partyMembers, string voiceLobbyId)
	{
		Party party = new Party
		{
			PartyID = partyID,
			LeaderIndex = leaderIndex,
			VoiceLobbyId = voiceLobbyId
		};
		this.partyList.Add(party);
		party.UpdateMemberList(world, partyMembers);
		return party;
	}

	public void RemoveParty(Party party)
	{
		if (this.partyList.Contains(party))
		{
			this.partyList.Remove(party);
		}
	}

	public Party GetParty(int partyID)
	{
		for (int i = 0; i < this.partyList.Count; i++)
		{
			if (this.partyList[i].PartyID == partyID)
			{
				return this.partyList[i];
			}
		}
		return null;
	}

	public void Cleanup()
	{
		this.partyList.Clear();
		this.nextPartyID = 0;
	}

	public void Update()
	{
		this.voice.Update();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PartyManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly PartyVoice voice;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextPartyID;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Party> partyList = new List<Party>();
}

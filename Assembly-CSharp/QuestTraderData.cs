using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class QuestTraderData
{
	public QuestTraderData()
	{
	}

	public QuestTraderData(Vector2 traderPOI)
	{
		this.TraderPOI = traderPOI;
	}

	public void AddPOI(int tier, Vector2 poiPosition)
	{
		if (!this.CompletedPOIByTier.ContainsKey(tier))
		{
			this.CompletedPOIByTier.Add(tier, new List<Vector2>());
		}
		if (!this.CompletedPOIByTier[tier].Contains(poiPosition))
		{
			this.CompletedPOIByTier[tier].Add(poiPosition);
		}
		if (this.resetDay == -1)
		{
			this.resetDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
		}
	}

	public void ClearTier(int tier)
	{
		if (tier == -1)
		{
			this.resetDay = -1;
			for (int i = QuestTraderData.resetStartTier; i <= QuestTraderData.fullTierCount; i++)
			{
				if (this.CompletedPOIByTier.ContainsKey(i))
				{
					this.CompletedPOIByTier.Remove(i);
				}
			}
			return;
		}
		if (this.CompletedPOIByTier.ContainsKey(tier))
		{
			this.CompletedPOIByTier.Remove(tier);
		}
	}

	public void CheckReset(EntityPlayer player)
	{
		if (this.resetDay != -1 && GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) - this.resetDay >= 7)
		{
			this.resetDay = -1;
			for (int i = QuestTraderData.resetStartTier; i <= QuestTraderData.fullTierCount; i++)
			{
				if (this.CompletedPOIByTier.ContainsKey(i))
				{
					this.CompletedPOIByTier.Remove(i);
				}
			}
			if (!(player is EntityPlayerLocal))
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNPCQuestList>().SetupClear(player.entityId, this.TraderPOI, -1), false, player.entityId, -1, -1, null, 192, false);
			}
		}
	}

	public List<Vector2> GetTierPOIs(int tier)
	{
		if (this.CompletedPOIByTier.ContainsKey(tier))
		{
			return this.CompletedPOIByTier[tier];
		}
		return null;
	}

	public void Read(BinaryReader _br, byte version)
	{
		this.TraderPOI = StreamUtils.ReadVector2(_br);
		int num = (int)_br.ReadByte();
		this.CompletedPOIByTier.Clear();
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)_br.ReadByte();
			int num3 = _br.ReadInt32();
			if (num3 > 0)
			{
				List<Vector2> list = new List<Vector2>();
				for (int j = 0; j < num3; j++)
				{
					list.Add(StreamUtils.ReadVector2(_br));
				}
				this.CompletedPOIByTier.Add(num2, list);
			}
		}
		int num4 = (int)_br.ReadByte();
		this.TradersSentTo.Clear();
		for (int k = 0; k < num4; k++)
		{
			this.TradersSentTo.Add(StreamUtils.ReadVector2(_br));
		}
		this.resetDay = _br.ReadInt32();
	}

	public void Write(BinaryWriter _bw)
	{
		StreamUtils.Write(_bw, this.TraderPOI);
		_bw.Write((byte)this.CompletedPOIByTier.Count);
		foreach (int num in this.CompletedPOIByTier.Keys)
		{
			_bw.Write((byte)num);
			List<Vector2> list = this.CompletedPOIByTier[num];
			_bw.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				StreamUtils.Write(_bw, list[i]);
			}
		}
		_bw.Write((byte)this.TradersSentTo.Count);
		for (int j = 0; j < this.TradersSentTo.Count; j++)
		{
			StreamUtils.Write(_bw, this.TradersSentTo[j]);
		}
		_bw.Write(this.resetDay);
	}

	public QuestJournal Owner;

	public Vector2 TraderPOI;

	public List<Vector2> TradersSentTo = new List<Vector2>();

	public Dictionary<int, List<Vector2>> CompletedPOIByTier = new Dictionary<int, List<Vector2>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int resetDay = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int resetStartTier = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int fullTierCount = 6;
}

using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageQuestTreasurePoint : NetPackage
{
	public NetPackageQuestTreasurePoint Setup(int _playerId, float _distance, int _offset, int _questCode, int posX = 0, int posY = -1, int posZ = 0, bool _useNearby = false)
	{
		this.playerId = _playerId;
		this.distance = _distance;
		this.offset = _offset;
		this.questCode = _questCode;
		this.position = new Vector3i(posX, posY, posZ);
		this.useNearby = _useNearby;
		this.treasureOffset = Vector3.zero;
		this.ActionType = NetPackageQuestTreasurePoint.QuestPointActions.GetGotoPoint;
		return this;
	}

	public NetPackageQuestTreasurePoint Setup(int _playerId, int _questCode, int _blocksPerReduction, Vector3i _position, Vector3 _treasureOffset)
	{
		this.playerId = _playerId;
		this.distance = 0f;
		this.offset = 0;
		this.questCode = _questCode;
		this.position = _position;
		this.treasureOffset = _treasureOffset;
		this.blocksPerReduction = _blocksPerReduction;
		this.ActionType = NetPackageQuestTreasurePoint.QuestPointActions.GetTreasurePoint;
		return this;
	}

	public NetPackageQuestTreasurePoint Setup(int _questCode, float _distance, int _offset, float _treasureRadius, Vector3 _startPosition, int _playerId, bool _useNearby, int _blocksPerReduction)
	{
		this.playerId = _playerId;
		this.distance = _distance;
		this.offset = _offset;
		this.questCode = _questCode;
		this.treasureRadius = _treasureRadius;
		this.position = new Vector3i(_startPosition);
		this.useNearby = _useNearby;
		this.treasureOffset = Vector3.zero;
		this.blocksPerReduction = _blocksPerReduction;
		this.ActionType = NetPackageQuestTreasurePoint.QuestPointActions.GetTreasurePoint;
		return this;
	}

	public NetPackageQuestTreasurePoint Setup(int _questCode, Vector3i _updatedPosition)
	{
		this.questCode = _questCode;
		this.position = _updatedPosition;
		this.ActionType = NetPackageQuestTreasurePoint.QuestPointActions.UpdateTreasurePoint;
		return this;
	}

	public NetPackageQuestTreasurePoint Setup(int _questCode, int _blocksPerReduction)
	{
		this.questCode = _questCode;
		this.blocksPerReduction = _blocksPerReduction;
		this.ActionType = NetPackageQuestTreasurePoint.QuestPointActions.UpdateBlocksPerReduction;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.ActionType = (NetPackageQuestTreasurePoint.QuestPointActions)_br.ReadByte();
		if (this.ActionType == NetPackageQuestTreasurePoint.QuestPointActions.UpdateTreasurePoint)
		{
			this.questCode = _br.ReadInt32();
			this.position = StreamUtils.ReadVector3i(_br);
			return;
		}
		this.playerId = _br.ReadInt32();
		this.distance = _br.ReadSingle();
		this.offset = _br.ReadInt32();
		this.treasureRadius = _br.ReadSingle();
		this.blocksPerReduction = _br.ReadInt32();
		this.questCode = _br.ReadInt32();
		this.position = StreamUtils.ReadVector3i(_br);
		this.treasureOffset = StreamUtils.ReadVector3(_br);
		this.useNearby = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.ActionType);
		if (this.ActionType == NetPackageQuestTreasurePoint.QuestPointActions.UpdateTreasurePoint)
		{
			_bw.Write(this.questCode);
			StreamUtils.Write(_bw, this.position);
			return;
		}
		_bw.Write(this.playerId);
		_bw.Write(this.distance);
		_bw.Write(this.offset);
		_bw.Write(this.treasureRadius);
		_bw.Write(this.blocksPerReduction);
		_bw.Write(this.questCode);
		StreamUtils.Write(_bw, this.position);
		StreamUtils.Write(_bw, this.treasureOffset);
		_bw.Write(this.useNearby);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (_world.IsRemote())
		{
			Quest quest = GameManager.Instance.World.GetPrimaryPlayer().QuestJournal.FindActiveQuest(this.questCode);
			if (quest != null)
			{
				for (int i = 0; i < quest.Objectives.Count; i++)
				{
					if (quest.CurrentPhase == quest.Objectives[i].Phase)
					{
						if (quest.Objectives[i] is ObjectiveTreasureChest)
						{
							if (this.ActionType == NetPackageQuestTreasurePoint.QuestPointActions.GetTreasurePoint)
							{
								((ObjectiveTreasureChest)quest.Objectives[i]).FinalizePointFromServer(this.blocksPerReduction, this.position, this.treasureOffset);
							}
							else if (this.ActionType == NetPackageQuestTreasurePoint.QuestPointActions.UpdateBlocksPerReduction)
							{
								((ObjectiveTreasureChest)quest.Objectives[i]).CurrentBlocksPerReduction = this.blocksPerReduction;
							}
						}
						else if (quest.Objectives[i] is ObjectiveRandomGoto)
						{
							((ObjectiveRandomGoto)quest.Objectives[i]).FinalizePoint(this.position.x, this.position.y, this.position.z);
						}
					}
				}
			}
			return;
		}
		if (this.ActionType == NetPackageQuestTreasurePoint.QuestPointActions.UpdateTreasurePoint)
		{
			QuestEventManager.Current.SetTreasureContainerPosition(this.questCode, this.position);
			return;
		}
		if (this.ActionType == NetPackageQuestTreasurePoint.QuestPointActions.UpdateBlocksPerReduction)
		{
			QuestEventManager.Current.UpdateTreasureBlocksPerReduction(this.questCode, this.blocksPerReduction);
			return;
		}
		for (int j = 0; j < 15; j++)
		{
			Vector3i vector3i;
			if (QuestEventManager.Current.GetTreasureContainerPosition(this.questCode, this.distance, this.offset, this.treasureRadius, this.position.ToVector3(), this.playerId, this.useNearby, this.blocksPerReduction, out this.blocksPerReduction, out vector3i, out this.treasureOffset))
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(this.playerId, this.questCode, this.blocksPerReduction, vector3i, this.treasureOffset), false, this.playerId, -1, -1, null, 192, false);
				return;
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int offset;

	[PublicizedFrom(EAccessModifier.Private)]
	public float treasureRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i position;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useNearby;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 treasureOffset = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blocksPerReduction;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageQuestTreasurePoint.QuestPointActions ActionType;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum QuestPointActions
	{
		GetGotoPoint,
		GetTreasurePoint,
		UpdateTreasurePoint,
		UpdateBlocksPerReduction
	}
}

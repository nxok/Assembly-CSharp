using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveModifierTrackBlocks : BaseObjectiveModifier
{
	public override void AddHooks()
	{
		base.OwnerObjective.OwnerQuest.TrackingHelper.AddTrackingEntry(this);
		QuestEventManager.Current.BlockChange += this.Current_BlockChange;
	}

	public override void RemoveHooks()
	{
		base.OwnerObjective.OwnerQuest.TrackingHelper.RemoveTrackingEntry(this);
		QuestEventManager.Current.BlockChange -= this.Current_BlockChange;
		NavObjectManager instance = NavObjectManager.Instance;
		for (int i = this.TrackedBlocks.Count - 1; i >= 0; i--)
		{
			instance.UnRegisterNavObject(this.TrackedBlocks[i].NavObject);
			this.TrackedBlocks.RemoveAt(i);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockChange(Block blockOld, Block blockNew, Vector3i blockPos)
	{
		if (blockOld.IndexName == this.blockIndexName)
		{
			for (int i = 0; i < this.TrackedBlocks.Count; i++)
			{
				if (this.TrackedBlocks[i].WorldPos == blockPos)
				{
					NavObjectManager.Instance.UnRegisterNavObject(this.TrackedBlocks[i].NavObject);
					this.TrackedBlocks.RemoveAt(i);
					return;
				}
			}
		}
	}

	public void StartUpdate()
	{
		if (this.localPlayer == null)
		{
			this.localPlayer = base.OwnerObjective.OwnerQuest.OwnerJournal.OwnerPlayer;
		}
		for (int i = 0; i < this.TrackedBlocks.Count; i++)
		{
			this.TrackedBlocks[i].KeepAlive = false;
		}
	}

	public void HandleTrack(Chunk c)
	{
		List<Vector3i> list;
		if (c.IndexedBlocks.TryGetValue(this.blockIndexName, out list))
		{
			foreach (Vector3i vector3i in list)
			{
				if (!c.GetBlockNoDamage(vector3i.x, vector3i.y, vector3i.z).ischild)
				{
					Vector3i vector3i2 = c.ToWorldPos(vector3i);
					if (Vector3.Distance(vector3i2, this.localPlayer.position) < this.trackDistance)
					{
						this.HandleAddTrackedBlock(vector3i2);
					}
				}
			}
		}
	}

	public void EndUpdate()
	{
		NavObjectManager instance = NavObjectManager.Instance;
		for (int i = this.TrackedBlocks.Count - 1; i >= 0; i--)
		{
			if (!this.TrackedBlocks[i].KeepAlive)
			{
				instance.UnRegisterNavObject(this.TrackedBlocks[i].NavObject);
				this.TrackedBlocks.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleAddTrackedBlock(Vector3i pos)
	{
		for (int i = 0; i < this.TrackedBlocks.Count; i++)
		{
			if (pos == this.TrackedBlocks[i].WorldPos)
			{
				this.TrackedBlocks[i].KeepAlive = true;
			}
		}
		this.TrackedBlocks.Add(new ObjectiveModifierTrackBlocks.TrackedBlock(pos, this.navObjectName));
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveModifierTrackBlocks.PropBlockIndexName))
		{
			this.blockIndexName = properties.Values[ObjectiveModifierTrackBlocks.PropBlockIndexName];
		}
		if (properties.Values.ContainsKey(ObjectiveModifierTrackBlocks.PropNavObjectName))
		{
			this.navObjectName = properties.Values[ObjectiveModifierTrackBlocks.PropNavObjectName];
		}
		if (properties.Values.ContainsKey(ObjectiveModifierTrackBlocks.PropTrackDistance))
		{
			this.trackDistance = StringParsers.ParseFloat(properties.Values[ObjectiveModifierTrackBlocks.PropTrackDistance], 0, -1, NumberStyles.Any);
		}
	}

	public override BaseObjectiveModifier Clone()
	{
		return new ObjectiveModifierTrackBlocks
		{
			blockIndexName = this.blockIndexName,
			navObjectName = this.navObjectName,
			trackDistance = this.trackDistance
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockIndexName = "questTracked";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string navObjectName = "quest_resource";

	public float trackDistance = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<ObjectiveModifierTrackBlocks.TrackedBlock> TrackedBlocks = new List<ObjectiveModifierTrackBlocks.TrackedBlock>();

	public static string PropBlockIndexName = "block_index_name";

	public static string PropNavObjectName = "nav_object";

	public static string PropTrackDistance = "track_distance";

	public class TrackedBlock
	{
		public TrackedBlock(Vector3i worldPos, string NavObjectName)
		{
			this.WorldPos = worldPos;
			this.NavObject = NavObjectManager.Instance.RegisterNavObject(NavObjectName, this.WorldPos.ToVector3Center(), "", false, -1, null);
			this.KeepAlive = true;
		}

		public Vector3i WorldPos;

		public NavObject NavObject;

		public bool KeepAlive;
	}
}

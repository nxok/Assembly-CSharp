using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRandomGoto : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			if (base.CurrentValue != 3)
			{
				return BaseObjective.ObjectiveValueTypes.Distance;
			}
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveRallyPointHeadTo", false, null);
		this.SetupIcon();
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	public override void SetupDisplay()
	{
		base.Description = this.keyword;
		this.StatusText = "";
	}

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				if (!this.positionSet)
				{
					return "--";
				}
				return ValueDisplayFormatters.Distance(this.distance);
			}
			else
			{
				if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
				{
					return "";
				}
				if (base.ObjectiveState == BaseObjective.ObjectiveStates.Failed)
				{
					return Localization.Get("failed", false, null);
				}
				return Localization.Get("completed", false, null);
			}
		}
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, this.NavObjectName, -1);
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupIcon()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 GetPosition()
	{
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.Location))
		{
			base.OwnerQuest.Position = this.position;
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.TreasurePoint))
		{
			this.positionSet = true;
			base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Location, base.OwnerQuest.Position);
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		float num = 50f;
		float num2 = 50f;
		if (this.Value != null && this.Value != "")
		{
			if (StringParsers.TryParseFloat(this.Value, out this.distance))
			{
				num2 = (num = this.distance);
			}
			else if (this.Value.Contains("-"))
			{
				string[] array = this.Value.Split('-', StringSplitOptions.None);
				num = StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any);
				num2 = StringParsers.ParseFloat(array[1], 0, -1, NumberStyles.Any);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.distance = GameManager.Instance.World.GetGameRandom().RandomFloat * (num2 - num) + num;
			Vector3i vector3i = ObjectiveRandomGoto.CalculateRandomPoint(ownerPlayer.entityId, this.distance, base.OwnerQuest.ID, false, this.biomeFilterType, this.biomeFilter);
			if (!GameManager.Instance.World.CheckForLevelNearbyHeights((float)vector3i.x, (float)vector3i.z, 5) || GameManager.Instance.World.GetWaterAt((float)vector3i.x, (float)vector3i.z))
			{
				return Vector3.zero;
			}
			World world = GameManager.Instance.World;
			if (vector3i.y > 0 && world.IsPositionInBounds(vector3i) && !world.IsPositionWithinPOI(vector3i, 5))
			{
				this.FinalizePoint(vector3i.x, vector3i.y, vector3i.z);
				return this.position;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(ownerPlayer.entityId, this.distance, 1, base.OwnerQuest.QuestCode, 0, -1, 0, false), false);
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public static Vector3i CalculateRandomPoint(int entityID, float distance, string questID, bool canBeWithinPOI = false, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		World world = GameManager.Instance.World;
		EntityAlive entityAlive = world.GetEntity(entityID) as EntityAlive;
		Vector3 vector;
		vector..ctor(world.GetGameRandom().RandomFloat * 2f + -1f, 0f, world.GetGameRandom().RandomFloat * 2f + -1f);
		vector.Normalize();
		Vector3 vector2 = entityAlive.position + vector * distance;
		int num = (int)vector2.x;
		int num2 = (int)vector2.z;
		int num3 = (int)world.GetHeightAt(vector2.x, vector2.z);
		Vector3i vector3i = new Vector3i(num, num3, num2);
		Vector3 vector3;
		vector3..ctor((float)vector3i.x, (float)vector3i.y, (float)vector3i.z);
		if (!world.IsPositionInBounds(vector3) || (entityAlive is EntityPlayer && !world.CanPlaceBlockAt(vector3i, GameManager.Instance.GetPersistentLocalPlayer(), false)) || (!canBeWithinPOI && world.IsPositionWithinPOI(vector3, 2)))
		{
			return new Vector3i(0, -99999, 0);
		}
		if (!world.CheckForLevelNearbyHeights(vector2.x, vector2.z, 5) || world.GetWaterAt(vector2.x, vector2.z))
		{
			return new Vector3i(0, -99999, 0);
		}
		if (biomeFilterType != BiomeFilterTypes.AnyBiome)
		{
			string[] array = null;
			BiomeDefinition biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)vector2.x, (int)vector2.z);
			if (biomeFilterType == BiomeFilterTypes.OnlyBiome)
			{
				if (biomeAt.m_sBiomeName != biomeFilter)
				{
					return new Vector3i(0, -99999, 0);
				}
			}
			else if (biomeFilterType == BiomeFilterTypes.ExcludeBiome)
			{
				if (array == null)
				{
					array = biomeFilter.Split(',', StringSplitOptions.None);
				}
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (biomeAt.m_sBiomeName == array[i])
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					return new Vector3i(0, -99999, 0);
				}
			}
			else if (biomeFilterType == BiomeFilterTypes.SameBiome)
			{
				BiomeDefinition biomeAt2 = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)entityAlive.position.x, (int)entityAlive.position.z);
				if (biomeAt != biomeAt2)
				{
					return new Vector3i(0, -99999, 0);
				}
			}
		}
		return vector3i;
	}

	public static Vector3i CalculateRandomPositionFromFlatAreas(int entityID, float minDistance, float maxDistance, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		World world = GameManager.Instance.World;
		EntityAlive entityAlive = world.GetEntity(entityID) as EntityAlive;
		if (biomeFilterType == BiomeFilterTypes.SameBiome)
		{
			biomeFilter = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)entityAlive.position.x, (int)entityAlive.position.z).m_sBiomeName;
		}
		List<FlatArea> areasWithinRange = GameManager.Instance.World.FlatAreaManager.GetAreasWithinRange(entityAlive.position, minDistance, maxDistance, eFlatAreaSizeFilter.All, biomeFilterType, biomeFilter.Split(',', StringSplitOptions.None), ChunkProtectionLevel.NearLandClaim);
		if (areasWithinRange.Count > 0)
		{
			return Vector3i.FromVector3Rounded(areasWithinRange[world.GetGameRandom().RandomRange(areasWithinRange.Count)].GetRandomPosition(1f));
		}
		return new Vector3i(0, -99999, 0);
	}

	public void FinalizePoint(int x, int y, int z)
	{
		this.position = new Vector3((float)x, (float)y, (float)z);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Location, this.position);
		base.OwnerQuest.Position = this.position;
		this.positionSet = true;
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, this.NavObjectName, -1);
		base.CurrentValue = 2;
	}

	public override void Update(float deltaTime)
	{
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 1f;
			if (!this.positionSet && base.CurrentValue != 1)
			{
				this.GetPosition() != Vector3.zero;
				this.OnStart();
			}
			switch (base.CurrentValue)
			{
			case 0:
				this.GetPosition() != Vector3.zero;
				return;
			case 1:
				break;
			case 2:
			{
				Entity ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
				if (base.OwnerQuest.NavObject != null && base.OwnerQuest.NavObject.TrackedPosition != this.position)
				{
					base.OwnerQuest.NavObject.TrackedPosition = this.position;
				}
				Vector3 vector = ownerPlayer.position;
				this.distance = Vector3.Distance(vector, this.position);
				if (this.distance < this.completionDistance && base.OwnerQuest.CheckRequirements())
				{
					base.CurrentValue = 3;
					this.Refresh();
					return;
				}
				break;
			}
			case 3:
			{
				if (this.completeWithinRange)
				{
					QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
					return;
				}
				Entity ownerPlayer2 = base.OwnerQuest.OwnerJournal.OwnerPlayer;
				if (base.OwnerQuest.NavObject != null && base.OwnerQuest.NavObject.TrackedPosition != this.position)
				{
					base.OwnerQuest.NavObject.TrackedPosition = this.position;
				}
				Vector3 vector2 = ownerPlayer2.position;
				this.distance = Vector3.Distance(vector2, this.position);
				if (this.distance > this.completionDistance)
				{
					base.CurrentValue = 2;
					this.Refresh();
				}
				break;
			}
			default:
				return;
			}
		}
	}

	public virtual void OnStart()
	{
	}

	public override void Refresh()
	{
		bool flag = base.CurrentValue == 3;
		base.Complete = flag;
		if (base.Complete)
		{
			base.ObjectiveState = BaseObjective.ObjectiveStates.Complete;
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, this.PlayObjectiveComplete, null);
			base.OwnerQuest.RemoveMapObject();
			this.RemoveHooks();
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveRandomGoto objectiveRandomGoto = new ObjectiveRandomGoto();
		this.CopyValues(objectiveRandomGoto);
		objectiveRandomGoto.position = this.position;
		objectiveRandomGoto.positionSet = this.positionSet;
		objectiveRandomGoto.completionDistance = this.completionDistance;
		objectiveRandomGoto.biomeFilter = this.biomeFilter;
		objectiveRandomGoto.biomeFilterType = this.biomeFilterType;
		return objectiveRandomGoto;
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		this.FinalizePoint((int)pos.x, (int)pos.y, (int)pos.z);
		return true;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(ObjectiveRandomGoto.PropDistance, ref this.Value);
		properties.ParseFloat(ObjectiveRandomGoto.PropCompletionDistance, ref this.completionDistance);
		properties.ParseEnum<BiomeFilterTypes>(ObjectiveRandomGoto.PropBiomeFilterType, ref this.biomeFilterType);
		properties.ParseString(ObjectiveRandomGoto.PropBiomeFilter, ref this.biomeFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool positionSet;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float distance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float completionDistance = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string icon = "ui_game_symbol_quest";

	[PublicizedFrom(EAccessModifier.Protected)]
	public BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string biomeFilter = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public new float updateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool completeWithinRange = true;

	public static string PropDistance = "distance";

	public static string PropCompletionDistance = "completion_distance";

	public static string PropBiomeFilterType = "biome_filter_type";

	public static string PropBiomeFilter = "biome_filter";

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum GotoStates
	{
		NoPosition,
		WaitingForPoint,
		TryComplete,
		Completed
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveGoto : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Distance;
		}
	}

	public override bool NeedsNPCSetPosition
	{
		get
		{
			return true;
		}
	}

	public override void SetupObjective()
	{
		if (this.ID == "trader")
		{
			Localization.Get("xuiTrader", false, null);
		}
		this.keyword = string.Format(Localization.Get("ObjectiveGoto_keyword", false, null), Localization.Get(this.locationName, false, null));
		this.distance = StringParsers.ParseFloat(this.Value, 0, -1, NumberStyles.Any);
		this.SetupIcon();
		if (base.OwnerQuest.Active)
		{
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.NavObjectName, -1);
		}
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
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
			if (this.poiNotFound && base.OwnerQuest.Position == Vector3.zero)
			{
				return "NO TRADER";
			}
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				return ValueDisplayFormatters.Distance(this.currentDistance);
			}
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupIcon()
	{
		if (this.ID.EqualsCaseInsensitive("trader"))
		{
			this.icon = "ui_game_symbol_map_trader";
		}
	}

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		return this.GetPosition(ownerNPC, player, usedPOILocations, entityIDforQuests) != Vector3.zero;
	}

	public override void SetPosition(Vector3 POIPosition, Vector3 POISize)
	{
		if (POISize.x > POISize.z)
		{
			this.distanceOffset = POISize.x;
		}
		else
		{
			this.distanceOffset = POISize.z;
		}
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POIPosition, POIPosition);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POISize, POISize);
		this.position = this.GetMidPOIPosition(POIPosition, POISize);
		base.OwnerQuest.Position = this.position;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 GetMidPOIPosition(Vector3 poiPosition, Vector3 poiSize)
	{
		int num = (int)(poiPosition.x + poiSize.x / 2f);
		int num2 = (int)(poiPosition.z + poiSize.z / 2f);
		int num3 = (int)GameManager.Instance.World.GetHeightAt((float)num, (float)num2);
		return new Vector3((float)num, (float)num3, (float)num2);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		EntityAlive entityAlive = ((ownerNPC == null) ? base.OwnerQuest.OwnerJournal.OwnerPlayer : ownerNPC);
		if (entityPlayer == null)
		{
			entityPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		}
		int num = ((ownerNPC == null) ? (-1) : ownerNPC.entityId);
		int num2 = ((entityPlayer == null) ? (-1) : entityPlayer.entityId);
		FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.none;
		if (!string.IsNullOrEmpty(this.ID))
		{
			fastTags = FastTags<TagGroup.Global>.Parse(this.ID);
		}
		Vector3 zero = Vector3.zero;
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.POIPosition) && base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POISize))
		{
			this.position = this.GetMidPOIPosition(this.position, zero);
			base.OwnerQuest.Position = this.position;
			this.positionSet = true;
			if (zero.x > zero.z)
			{
				this.distanceOffset = zero.x;
			}
			else
			{
				this.distanceOffset = zero.z;
			}
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PrefabInstance prefabInstance = null;
			if (prefabInstance == null)
			{
				prefabInstance = base.OwnerQuest.QuestPrefab;
			}
			int num3 = (int)((ownerNPC != null) ? ownerNPC.NPCInfo.QuestFaction : base.OwnerQuest.QuestFaction);
			usedPOILocations = ((entityPlayer != null) ? entityPlayer.QuestJournal.GetTraderList(num3) : null);
			if (prefabInstance == null)
			{
				string text = "";
				if (base.OwnerQuest != null && base.OwnerQuest.QuestClass != null)
				{
					text = base.OwnerQuest.QuestClass.UniqueKey;
				}
				prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(fastTags, new Vector2(entityAlive.position.x, entityAlive.position.z), usedPOILocations, -1, !this.allowCurrentPoi, this.biomeFilterType, this.biomeFilter, text);
				if (prefabInstance == null)
				{
					prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(fastTags, new Vector2(entityAlive.position.x, entityAlive.position.z), usedPOILocations, -1, !this.allowCurrentPoi, BiomeFilterTypes.SameBiome, "", text);
				}
			}
			if (prefabInstance == null)
			{
				return Vector3.zero;
			}
			Vector2 vector;
			vector..ctor((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
			if (vector.x == -0.1f && vector.y == -0.1f)
			{
				Log.Error("ObjectiveGoto: No Trader found.");
				return Vector3.zero;
			}
			int num4 = (int)vector.x;
			int num5 = (int)vector.y;
			int num6 = (int)GameManager.Instance.World.GetHeightAt(vector.x, vector.y);
			this.position = new Vector3((float)num4, (float)num6, (float)num5);
			if (GameManager.Instance.World.IsPositionInBounds(this.position))
			{
				base.OwnerQuest.Position = this.position;
				this.FinalizePoint(new Vector3((float)prefabInstance.boundingBoxPosition.x, (float)prefabInstance.boundingBoxPosition.y, (float)prefabInstance.boundingBoxPosition.z), new Vector3((float)prefabInstance.boundingBoxSize.x, (float)prefabInstance.boundingBoxSize.y, (float)prefabInstance.boundingBoxSize.z));
				base.OwnerQuest.QuestPrefab = prefabInstance;
				base.OwnerQuest.DataVariables.Add("POIName", Localization.Get(base.OwnerQuest.QuestPrefab.location.Name, false, null));
				return this.position;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(num, num2, fastTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.Trader, base.OwnerQuest.QuestClass.DifficultyTier, 0, -1, 0f, 0f, 0f, -1f, this.biomeFilterType, this.biomeFilter), false);
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public void FinalizePoint(Vector3 POIPosition, Vector3 POISize)
	{
		if (POISize.x > POISize.z)
		{
			this.distanceOffset = POISize.x;
		}
		else
		{
			this.distanceOffset = POISize.z;
		}
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POIPosition, POIPosition);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POISize, POISize);
		this.position = this.GetMidPOIPosition(POIPosition, POISize);
		this.positionSet = true;
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.NavObjectName, -1);
		if (GameSparksCollector.CollectGamePlayData && base.OwnerQuest.QuestClass.ID == "quest_whiterivercitizen1")
		{
			GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestStarterTraderDistance, ((int)Vector3.Distance(this.position, base.OwnerQuest.OwnerJournal.OwnerPlayer.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
		}
		base.CurrentValue = 2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_NeedSetup()
	{
		int num = -1;
		if (base.OwnerQuest != null)
		{
			num = base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId;
		}
		if (this.GetPosition(null, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, num) != Vector3.zero)
		{
			this.poiNotFound = false;
			return;
		}
		this.poiNotFound = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
		if (!this.positionSet)
		{
			this.GetPosition(null, null, null, -1);
			return;
		}
		if (this.position.y == 0f)
		{
			this.position.y = (float)((int)GameManager.Instance.World.GetHeightAt(this.position.x, this.position.z));
		}
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (base.OwnerQuest.NavObject != null && base.OwnerQuest.NavObject.TrackedPosition != this.position)
		{
			base.OwnerQuest.NavObject.TrackedPosition = this.position;
		}
		this.currentDistance = Vector3.Distance(ownerPlayer.position, this.position);
		if (this.currentDistance < this.distance + this.distanceOffset && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 3;
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		bool flag = base.CurrentValue == 3;
		base.Complete = flag;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveGoto objectiveGoto = new ObjectiveGoto();
		this.CopyValues(objectiveGoto);
		return objectiveGoto;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveGoto objectiveGoto = (ObjectiveGoto)objective;
		objectiveGoto.distance = this.distance;
		objectiveGoto.biomeFilterType = this.biomeFilterType;
		objectiveGoto.biomeFilter = this.biomeFilter;
		objectiveGoto.locationName = this.locationName;
		objectiveGoto.allowCurrentPoi = this.allowCurrentPoi;
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		this.FinalizePoint(pos, size);
		return true;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveGoto.PropDistance))
		{
			this.Value = properties.Values[ObjectiveGoto.PropDistance];
			this.distance = StringParsers.ParseFloat(this.Value, 0, -1, NumberStyles.Any);
		}
		if (properties.Values.ContainsKey(ObjectiveGoto.PropAllowCurrentPOI))
		{
			this.allowCurrentPoi = StringParsers.ParseBool(properties.Values[ObjectiveGoto.PropAllowCurrentPOI], 0, -1, true);
		}
		properties.ParseString(ObjectiveGoto.PropLocation, ref this.ID);
		properties.ParseEnum<BiomeFilterTypes>(ObjectiveGoto.PropBiomeFilterType, ref this.biomeFilterType);
		properties.ParseString(ObjectiveGoto.PropBiomeFilter, ref this.biomeFilter);
		properties.ParseString(ObjectiveGoto.PropLocationName, ref this.locationName);
	}

	public override void HandleCompleted()
	{
		Vector3 vector;
		if (base.OwnerQuest.OwnerJournal != null && base.OwnerQuest.GetPositionData(out vector, Quest.PositionDataTypes.POIPosition))
		{
			base.OwnerQuest.OwnerJournal.AddTraderPOI(new Vector2(vector.x, vector.z), (int)base.OwnerQuest.QuestFaction);
		}
	}

	public override string ParseBinding(string bindingName)
	{
		string id = this.ID;
		string value = this.Value;
		if (bindingName == "location")
		{
			return this.ID;
		}
		if (!(bindingName == "name"))
		{
			if (!(bindingName == "distance"))
			{
				if (!(bindingName == "direction"))
				{
					if (bindingName == "directionfull")
					{
						if (base.OwnerQuest.QuestGiverID != -1)
						{
							EntityNPC entityNPC = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
							if (entityNPC != null)
							{
								this.position.y = 0f;
								Vector3 vector = entityNPC.position;
								vector.y = 0f;
								return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(this.position.x - vector.x, this.position.z - vector.z)), true);
							}
						}
					}
				}
				else if (base.OwnerQuest.QuestGiverID != -1)
				{
					EntityNPC entityNPC2 = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
					if (entityNPC2 != null)
					{
						this.position.y = 0f;
						Vector3 vector2 = entityNPC2.position;
						vector2.y = 0f;
						return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(this.position.x - vector2.x, this.position.z - vector2.z)), false);
					}
				}
			}
			else if (base.OwnerQuest.QuestGiverID != -1)
			{
				EntityNPC entityNPC3 = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
				if (entityNPC3 != null)
				{
					Vector3 vector3 = entityNPC3.position;
					this.currentDistance = Vector3.Distance(vector3, this.position);
					return ValueDisplayFormatters.Distance(this.currentDistance);
				}
			}
			return "";
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (base.OwnerQuest.DataVariables.ContainsKey("POIName"))
			{
				return base.OwnerQuest.DataVariables["POIName"];
			}
			if (base.OwnerQuest.QuestPrefab == null)
			{
				return "";
			}
			return Localization.Get(base.OwnerQuest.QuestPrefab.location.Name, false, null);
		}
		else
		{
			if (!base.OwnerQuest.DataVariables.ContainsKey("POIName"))
			{
				return "";
			}
			return base.OwnerQuest.DataVariables["POIName"];
		}
	}

	public static string PropAllowCurrentPOI = "allow_current_poi";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool allowCurrentPoi;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool positionSet;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float distance = 20f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float distanceOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string icon = "ui_game_symbol_quest";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string locationVariable = "gotolocation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string biomeFilter = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string locationName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool poiNotFound;

	public static string PropLocation = "location_tag";

	public static string PropDistance = "distance";

	public static string PropBiomeFilterType = "biome_filter_type";

	public static string PropBiomeFilter = "biome_filter";

	public static string PropLocationName = "location_name";
}

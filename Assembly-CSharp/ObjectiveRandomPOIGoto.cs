using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRandomPOIGoto : ObjectiveGoto
{
	public int POITier
	{
		get
		{
			if (this.poiTier != -1)
			{
				return this.poiTier;
			}
			return (int)base.OwnerQuest.QuestClass.DifficultyTier;
		}
		set
		{
			this.poiTier = value;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveRallyPointHeadTo", false, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetupIcon()
	{
		this.icon = "ui_game_symbol_quest";
	}

	public override bool NeedsNPCSetPosition
	{
		get
		{
			return true;
		}
	}

	public override bool PlayObjectiveComplete
	{
		get
		{
			return false;
		}
	}

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		return this.GetPosition(ownerNPC, player, usedPOILocations, entityIDforQuests) != Vector3.zero;
	}

	public override void AddHooks()
	{
		base.AddHooks();
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.NavObjectName, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDistanceOffset(Vector3 POISize)
	{
		if (POISize.x > POISize.z)
		{
			this.distanceOffset = POISize.x;
			return;
		}
		this.distanceOffset = POISize.z;
	}

	public override void SetPosition(Vector3 POIPosition, Vector3 POISize)
	{
		this.SetDistanceOffset(POISize);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POIPosition, POIPosition);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POISize, POISize);
		base.OwnerQuest.Position = POIPosition;
		this.position = POIPosition;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		int num = ((ownerNPC == null) ? (-1) : ownerNPC.entityId);
		int num2 = ((entityPlayer == null) ? (-1) : entityPlayer.entityId);
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.POIPosition))
		{
			Vector3 vector;
			base.OwnerQuest.GetPositionData(out vector, Quest.PositionDataTypes.POISize);
			Vector2 vector2;
			vector2..ctor(this.position.x + vector.x / 2f, this.position.z + vector.z / 2f);
			int num3 = (int)vector2.x;
			int num4 = (int)vector2.y;
			int num5 = (int)GameManager.Instance.World.GetHeightAt(vector2.x, vector2.y);
			this.position = new Vector3((float)num3, (float)num5, (float)num4);
			base.OwnerQuest.Position = this.position;
			this.SetDistanceOffset(vector);
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		EntityAlive entityAlive = entityPlayer;
		if (entityAlive == null)
		{
			entityAlive = ((ownerNPC == null) ? base.OwnerQuest.OwnerJournal.OwnerPlayer : ownerNPC);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PrefabInstance prefabInstance;
			if (ownerNPC != null)
			{
				prefabInstance = DynamicPrefabDecorator.GetRandomPOINearTrader(ownerNPC as EntityTrader, base.OwnerQuest.QuestTags, (byte)this.POITier, usedPOILocations, entityIDforQuests, this.biomeFilterType, this.biomeFilter);
			}
			else
			{
				prefabInstance = DynamicPrefabDecorator.GetRandomPOINearWorldPos(new Vector2(entityAlive.position.x, entityAlive.position.z), 1000, 4000000, base.OwnerQuest.QuestTags, (byte)this.POITier, usedPOILocations, entityIDforQuests, this.biomeFilterType, this.biomeFilter);
			}
			if (prefabInstance == null)
			{
				return Vector3.zero;
			}
			if (prefabInstance != null)
			{
				Vector2 vector3;
				vector3..ctor((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
				if (vector3.x == -0.1f && vector3.y == -0.1f)
				{
					Log.Error("ObjectiveRandomGoto: No POI found.");
					return Vector3.zero;
				}
				int num6 = (int)vector3.x;
				int num7 = (int)GameManager.Instance.World.GetHeightAt(vector3.x, vector3.y);
				int num8 = (int)vector3.y;
				this.position = new Vector3((float)num6, (float)num7, (float)num8);
				if (GameManager.Instance.World.IsPositionInBounds(this.position))
				{
					base.OwnerQuest.Position = this.position;
					base.FinalizePoint(new Vector3((float)prefabInstance.boundingBoxPosition.x, (float)prefabInstance.boundingBoxPosition.y, (float)prefabInstance.boundingBoxPosition.z), new Vector3((float)prefabInstance.boundingBoxSize.x, (float)prefabInstance.boundingBoxSize.y, (float)prefabInstance.boundingBoxSize.z));
					base.OwnerQuest.QuestPrefab = prefabInstance;
					base.OwnerQuest.DataVariables.Add("POIName", Localization.Get(base.OwnerQuest.QuestPrefab.location.Name, false, null));
					if (usedPOILocations != null)
					{
						usedPOILocations.Add(new Vector2((float)prefabInstance.boundingBoxPosition.x, (float)prefabInstance.boundingBoxPosition.z));
					}
					return this.position;
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(num, num2, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI, (byte)this.POITier, 0, -1, 0f, 0f, 0f, -1f, this.biomeFilterType, this.biomeFilter), false);
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public override BaseObjective Clone()
	{
		ObjectiveRandomPOIGoto objectiveRandomPOIGoto = new ObjectiveRandomPOIGoto();
		this.CopyValues(objectiveRandomPOIGoto);
		objectiveRandomPOIGoto.poiTier = this.poiTier;
		objectiveRandomPOIGoto.distance = this.distance;
		objectiveRandomPOIGoto.biomeFilterType = this.biomeFilterType;
		objectiveRandomPOIGoto.biomeFilter = this.biomeFilter;
		objectiveRandomPOIGoto.locationName = this.locationName;
		return objectiveRandomPOIGoto;
	}

	public override string ParseBinding(string bindingName)
	{
		string id = this.ID;
		string value = this.Value;
		if (!(bindingName == "name"))
		{
			if (!(bindingName == "distance"))
			{
				if (bindingName == "direction")
				{
					if (base.OwnerQuest.QuestGiverID != -1)
					{
						EntityNPC entityNPC = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
						if (entityNPC != null)
						{
							this.position.y = 0f;
							Vector3 position = entityNPC.position;
							position.y = 0f;
							return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(this.position.x - position.x, this.position.z - position.z)), false);
						}
					}
				}
			}
			else if (base.OwnerQuest.QuestGiverID != -1)
			{
				EntityNPC entityNPC2 = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
				if (entityNPC2 != null)
				{
					Vector3 position2 = entityNPC2.position;
					this.currentDistance = Vector3.Distance(position2, this.position);
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

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseInt(ObjectiveRandomPOIGoto.PropPOITier, ref this.poiTier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int poiTier = -1;

	public static string PropPOITier = "poi_tier";
}

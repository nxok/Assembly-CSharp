using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class PersistentPlayerData
{
	public PlatformUserIdentifierAbs PrimaryId
	{
		get
		{
			return this.PlayerData.PrimaryId;
		}
	}

	public IPlatformUserData PlatformData
	{
		get
		{
			return this.PlayerData.PlatformData;
		}
	}

	public PlatformUserIdentifierAbs NativeId
	{
		get
		{
			return this.PlayerData.NativeId;
		}
	}

	public EPlayGroup PlayGroup
	{
		get
		{
			return this.PlayerData.PlayGroup;
		}
	}

	public PersistentPlayerData(PlatformUserIdentifierAbs _primaryId, PlatformUserIdentifierAbs _nativeId, AuthoredText _playerName, EPlayGroup _playGroup)
	{
		this.PlayerData = new PlayerData(_primaryId, _nativeId, _playerName, _playGroup);
		this.PlayerName = new PersistentPlayerName(_playerName);
	}

	public void Update(PlatformUserIdentifierAbs _nativeId, AuthoredText _playerName, EPlayGroup _playGroup)
	{
		this.PlayerData = new PlayerData(this.PrimaryId, _nativeId, _playerName, _playGroup);
		this.PlayerName.Update(_playerName);
	}

	public Vector3i MostRecentBackpackPosition
	{
		get
		{
			if (this.backpacksByID.Count == 0)
			{
				return Vector3i.zero;
			}
			this.RefreshSortedBackpacksList();
			return this.backpacksSortedByTimestamp[this.backpacksSortedByTimestamp.Count - 1].Position;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshSortedBackpacksList()
	{
		if (!this.sortedBackpacksDirty)
		{
			return;
		}
		this.backpacksSortedByTimestamp.Clear();
		foreach (KeyValuePair<int, PersistentPlayerData.ProtectedBackpack> keyValuePair in this.backpacksByID)
		{
			this.backpacksSortedByTimestamp.Add(keyValuePair.Value);
		}
		this.backpacksSortedByTimestamp.Sort((PersistentPlayerData.ProtectedBackpack a, PersistentPlayerData.ProtectedBackpack b) => a.Timestamp.CompareTo(b.Timestamp));
		this.sortedBackpacksDirty = false;
	}

	public void AddDroppedBackpack(int backpackEntityId, Vector3i pos, uint timestamp)
	{
		this.backpacksByID[backpackEntityId] = new PersistentPlayerData.ProtectedBackpack(backpackEntityId, pos, timestamp);
		this.sortedBackpacksDirty = true;
		this.RefreshSortedBackpacksList();
		if (this.backpacksByID.Count > 3)
		{
			for (int i = 0; i < this.backpacksSortedByTimestamp.Count - 3; i++)
			{
				int entityID = this.backpacksSortedByTimestamp[i].EntityID;
				if (entityID == backpackEntityId)
				{
					Debug.LogError("AddDroppedBackpack failed: dropped backpack timestamp is older than other tracked backpacks and the tracking limit has been reached.");
				}
				this.TryRemoveDroppedBackpack(entityID);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSetBackpackPosition>().Setup(this.EntityId, this.GetDroppedBackpackPositions()), false, this.EntityId, -1, -1, null, 192, false);
		}
	}

	public bool TryUpdateBackpackPosition(int entityID, Vector3i pos)
	{
		PersistentPlayerData.ProtectedBackpack protectedBackpack;
		if (!this.backpacksByID.TryGetValue(entityID, out protectedBackpack))
		{
			return false;
		}
		this.backpacksByID[entityID] = new PersistentPlayerData.ProtectedBackpack(entityID, pos, protectedBackpack.Timestamp);
		return true;
	}

	public bool TryRemoveDroppedBackpack(int entityID)
	{
		if (this.backpacksByID.Remove(entityID))
		{
			this.sortedBackpacksDirty = true;
			this.RefreshSortedBackpacksList();
			return true;
		}
		return false;
	}

	public void ProcessBackpacks(Action<PersistentPlayerData.ProtectedBackpack> action)
	{
		foreach (PersistentPlayerData.ProtectedBackpack protectedBackpack in this.backpacksByID.Values)
		{
			action(protectedBackpack);
		}
	}

	public void RemoveBackpacks(Predicate<PersistentPlayerData.ProtectedBackpack> shouldRemove)
	{
		this.RefreshSortedBackpacksList();
		for (int i = 0; i < this.backpacksSortedByTimestamp.Count; i++)
		{
			if (shouldRemove(this.backpacksSortedByTimestamp[i]))
			{
				this.TryRemoveDroppedBackpack(this.backpacksSortedByTimestamp[i].EntityID);
			}
		}
	}

	public void ClearDroppedBackpacks()
	{
		this.backpacksByID.Clear();
		this.backpacksSortedByTimestamp.Clear();
		this.sortedBackpacksDirty = true;
	}

	public List<Vector3i> GetDroppedBackpackPositions()
	{
		List<Vector3i> list = new List<Vector3i>();
		foreach (PersistentPlayerData.ProtectedBackpack protectedBackpack in this.backpacksSortedByTimestamp)
		{
			list.Add(protectedBackpack.Position);
		}
		return list;
	}

	public void AddVendingMachinePosition(Vector3i pos)
	{
		if (!this.OwnedVendingMachinePositions.Contains(pos))
		{
			this.OwnedVendingMachinePositions.Add(pos);
		}
	}

	public bool TryRemoveVendingMachinePosition(Vector3i pos)
	{
		return this.OwnedVendingMachinePositions.Remove(pos);
	}

	public bool HasBedrollPos
	{
		get
		{
			return this.BedrollPos.y != int.MaxValue;
		}
	}

	public double OfflineHours
	{
		get
		{
			if (this.EntityId != -1)
			{
				return -1.0;
			}
			return (DateTime.Now - this.LastLogin).TotalHours;
		}
	}

	public double OfflineMinutes
	{
		get
		{
			if (this.EntityId != -1)
			{
				return -1.0;
			}
			return (DateTime.Now - this.LastLogin).TotalMinutes;
		}
	}

	public bool IsAlly(PlatformUserIdentifierAbs other)
	{
		return GameManager.Instance.persistentPlayers.Allies.IsAlly(this.PrimaryId, other);
	}

	public bool IsAlly(PersistentPlayerData other)
	{
		return GameManager.Instance.persistentPlayers.Allies.IsAlly(this.PrimaryId, (other != null) ? other.PrimaryId : null);
	}

	public void AddLandProtectionBlock(Vector3i pos)
	{
		if (this.LPBlocks == null)
		{
			this.LPBlocks = new List<Vector3i>();
		}
		this.LPBlocks.Add(pos);
	}

	public List<Vector3i> GetLandProtectionBlocks()
	{
		if (this.LPBlocks == null)
		{
			this.LPBlocks = new List<Vector3i>();
		}
		return this.LPBlocks;
	}

	public bool GetLandProtectionBlock(out Vector3i _blockPos)
	{
		_blockPos = Vector3i.zero;
		if (this.LPBlocks != null && this.LPBlocks.Count > 0)
		{
			_blockPos = this.LPBlocks[0];
			return true;
		}
		return false;
	}

	public void RemoveLandProtectionBlock(Vector3i pos)
	{
		this.LPBlocks.Remove(pos);
	}

	public void ClearBedroll()
	{
		Entity entity = GameManager.Instance.World.GetEntity(this.EntityId);
		if (entity)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(entity, "sleeping_bag");
			this.BedrollPos.y = int.MaxValue;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.SleepingBag, this.EntityId), false, -1, -1, -1, null, 192, false);
			}
		}
	}

	public void ShowBedrollOnMap()
	{
		if (GameManager.Instance.IsEditMode())
		{
			return;
		}
		if (this.BedrollPos.y != 2147483647)
		{
			Entity entity = GameManager.Instance.World.GetEntity(this.EntityId);
			if (entity)
			{
				NavObject navObject = NavObjectManager.Instance.RegisterNavObject("sleeping_bag", this.BedrollPos.ToVector3(), "", false, -1, null);
				if (navObject != null)
				{
					navObject.OwnerEntity = entity;
				}
			}
		}
	}

	public void AddQuestPosition(int questCode, Quest.PositionDataTypes positionDataType, Vector3 position)
	{
		Vector3i vector3i = World.worldToBlockPos(position);
		foreach (QuestPositionData questPositionData in this.QuestPositions)
		{
			if (questPositionData.questCode == questCode && questPositionData.positionDataType == positionDataType)
			{
				questPositionData.blockPosition = vector3i;
				return;
			}
		}
		if (positionDataType == Quest.PositionDataTypes.TreasureOffset || positionDataType == Quest.PositionDataTypes.POISize || positionDataType == Quest.PositionDataTypes.TraderPosition)
		{
			return;
		}
		this.QuestPositions.Add(new QuestPositionData(questCode, positionDataType, vector3i));
		this.questPositionsChanged = true;
	}

	public void RemovePositionsForQuest(int questCode)
	{
		List<QuestPositionData> list = new List<QuestPositionData>();
		foreach (QuestPositionData questPositionData in this.QuestPositions)
		{
			if (questPositionData.questCode == questCode)
			{
				list.Add(questPositionData);
			}
		}
		foreach (QuestPositionData questPositionData2 in list)
		{
			this.QuestPositions.Remove(questPositionData2);
		}
		this.questPositionsChanged = true;
	}

	public void UpdatePositionFromEntity()
	{
		if (this.EntityId == -1)
		{
			return;
		}
		Entity entity = GameManager.Instance.World.GetEntity(this.EntityId);
		if (!entity)
		{
			return;
		}
		this.Position = new Vector3i(entity.position);
	}

	public void Write(BinaryWriter stream)
	{
		this.UpdatePositionFromEntity();
		this.PrimaryId.ToStream(stream, true);
		this.NativeId.ToStream(stream, true);
		stream.Write((byte)this.PlayGroup);
		AuthoredText.ToStream(this.PlayerName.AuthoredName, stream);
		stream.Write(this.LastLogin.Ticks);
		stream.Write(this.Position.x);
		stream.Write(this.Position.y);
		stream.Write(this.Position.z);
		stream.Write(this.EntityId);
		List<Vector3i> lpblocks = this.LPBlocks;
		stream.Write((lpblocks != null) ? lpblocks.Count : 0);
		stream.Write(this.backpacksByID.Count);
		if (this.LPBlocks != null)
		{
			for (int i = 0; i < this.LPBlocks.Count; i++)
			{
				Vector3i vector3i = this.LPBlocks[i];
				stream.Write(vector3i.x);
				stream.Write(vector3i.y);
				stream.Write(vector3i.z);
			}
		}
		foreach (KeyValuePair<int, PersistentPlayerData.ProtectedBackpack> keyValuePair in this.backpacksByID)
		{
			stream.Write(keyValuePair.Key);
			Vector3i position = keyValuePair.Value.Position;
			stream.Write(position.x);
			stream.Write(position.y);
			stream.Write(position.z);
			stream.Write(keyValuePair.Value.Timestamp);
		}
		stream.Write(this.BedrollPos.x);
		stream.Write(this.BedrollPos.y);
		stream.Write(this.BedrollPos.z);
		stream.Write(this.QuestPositions.Count);
		foreach (QuestPositionData questPositionData in this.QuestPositions)
		{
			questPositionData.Write(stream);
		}
		stream.Write(this.OwnedVendingMachinePositions.Count);
		foreach (Vector3i vector3i2 in this.OwnedVendingMachinePositions)
		{
			stream.Write(vector3i2.x);
			stream.Write(vector3i2.y);
			stream.Write(vector3i2.z);
		}
	}

	public static PersistentPlayerData Read(BinaryReader stream)
	{
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(stream, false, true);
		PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromStream(stream, false, true);
		EPlayGroup eplayGroup = (EPlayGroup)stream.ReadByte();
		PersistentPlayerData persistentPlayerData = new PersistentPlayerData(platformUserIdentifierAbs, platformUserIdentifierAbs2, AuthoredText.FromStream(stream), eplayGroup);
		persistentPlayerData.LastLogin = new DateTime(stream.ReadInt64());
		persistentPlayerData.Position.x = stream.ReadInt32();
		persistentPlayerData.Position.y = stream.ReadInt32();
		persistentPlayerData.Position.z = stream.ReadInt32();
		persistentPlayerData.EntityId = stream.ReadInt32();
		int num = stream.ReadInt32();
		int num2 = stream.ReadInt32();
		if (num > 0)
		{
			persistentPlayerData.LPBlocks = new List<Vector3i>();
			for (int i = 0; i < num; i++)
			{
				persistentPlayerData.LPBlocks.Add(new Vector3i(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32()));
			}
		}
		for (int j = 0; j < num2; j++)
		{
			int num3 = stream.ReadInt32();
			Vector3i vector3i = new Vector3i(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32());
			uint num4 = stream.ReadUInt32();
			persistentPlayerData.AddDroppedBackpack(num3, vector3i, num4);
		}
		persistentPlayerData.BedrollPos.x = stream.ReadInt32();
		persistentPlayerData.BedrollPos.y = stream.ReadInt32();
		persistentPlayerData.BedrollPos.z = stream.ReadInt32();
		int num5 = stream.ReadInt32();
		persistentPlayerData.QuestPositions = new List<QuestPositionData>();
		for (int k = 0; k < num5; k++)
		{
			persistentPlayerData.QuestPositions.Add(QuestPositionData.Read(stream));
		}
		int num6 = stream.ReadInt32();
		persistentPlayerData.OwnedVendingMachinePositions = new List<Vector3i>();
		for (int l = 0; l < num6; l++)
		{
			persistentPlayerData.OwnedVendingMachinePositions.Add(new Vector3i(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32()));
		}
		return persistentPlayerData;
	}

	public void Write(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement("player");
		this.PrimaryId.ToXml(xmlElement, "");
		PlatformUserIdentifierAbs nativeId = this.NativeId;
		if (nativeId != null)
		{
			nativeId.ToXml(xmlElement, "native");
		}
		xmlElement.SetAttrib("playername", this.PlayerName.AuthoredName.Text);
		xmlElement.SetAttrib("playgroup", this.PlayGroup.ToStringCached<EPlayGroup>());
		xmlElement.SetAttrib("lastlogin", this.LastLogin.ToCultureInvariantString());
		xmlElement.SetAttrib("position", string.Format("{0},{1},{2}", this.Position.x, this.Position.y, this.Position.z));
		if ((this.LPBlocks == null || this.LPBlocks.Count == 0) && this.backpacksByID.Count == 0 && this.BedrollPos.y == 2147483647 && this.QuestPositions.Count == 0 && this.OwnedVendingMachinePositions.Count == 0)
		{
			return;
		}
		if (this.LPBlocks != null)
		{
			for (int i = 0; i < this.LPBlocks.Count; i++)
			{
				Vector3i vector3i = this.LPBlocks[i];
				xmlElement.AddXmlElement("lpblock").SetAttrib("pos", string.Format("{0},{1},{2}", vector3i.x, vector3i.y, vector3i.z));
			}
		}
		foreach (KeyValuePair<int, PersistentPlayerData.ProtectedBackpack> keyValuePair in this.backpacksByID)
		{
			int key = keyValuePair.Key;
			Vector3i position = keyValuePair.Value.Position;
			uint timestamp = keyValuePair.Value.Timestamp;
			XmlElement xmlElement2 = xmlElement.AddXmlElement("backpack");
			xmlElement2.SetAttrib("id", string.Format("{0}", key));
			xmlElement2.SetAttrib("pos", string.Format("{0},{1},{2}", position.x, position.y, position.z));
			xmlElement2.SetAttrib("timestamp", string.Format("{0}", timestamp));
		}
		if (this.BedrollPos.y != 2147483647)
		{
			xmlElement.AddXmlElement("bedroll").SetAttrib("pos", string.Format("{0},{1},{2}", this.BedrollPos.x, this.BedrollPos.y, this.BedrollPos.z));
		}
		if (this.QuestPositions != null && this.QuestPositions.Count > 0)
		{
			XmlElement xmlElement3 = xmlElement.AddXmlElement("questpositions");
			foreach (QuestPositionData questPositionData in this.QuestPositions)
			{
				XmlElement xmlElement4 = xmlElement3.AddXmlElement("position");
				xmlElement4.SetAttribute("id", questPositionData.questCode.ToString());
				string text = "positiondatatype";
				int positionDataType = (int)questPositionData.positionDataType;
				xmlElement4.SetAttribute(text, positionDataType.ToString());
				xmlElement4.SetAttrib("pos", string.Format("{0},{1},{2}", questPositionData.blockPosition.x, questPositionData.blockPosition.y, questPositionData.blockPosition.z));
			}
		}
		if (this.OwnedVendingMachinePositions != null && this.OwnedVendingMachinePositions.Count > 0)
		{
			XmlElement xmlElement5 = xmlElement.AddXmlElement("vendingmachinepositions");
			foreach (Vector3i vector3i2 in this.OwnedVendingMachinePositions)
			{
				xmlElement5.AddXmlElement("position").SetAttrib("pos", string.Format("{0},{1},{2}", vector3i2.x, vector3i2.y, vector3i2.z));
			}
		}
	}

	public static PersistentPlayerData ReadXML(XmlElement root, int readVersion, out HashSet<PlatformUserIdentifierAbs> legacyACL)
	{
		legacyACL = null;
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromXml(root, true, null);
		if (platformUserIdentifierAbs == null)
		{
			Log.Error("player-entry has missing or invalid user-identifier attributes: " + root.OuterXml);
			Application.Quit();
			return null;
		}
		if (!root.HasAttribute("playername"))
		{
			Log.Error("player-entry is missing 'playername' attribute: " + root.OuterXml);
			return null;
		}
		AuthoredText authoredText = new AuthoredText(root.GetAttribute("playername"), platformUserIdentifierAbs);
		string text;
		EPlayGroup eplayGroup;
		if (!root.TryGetAttribute("playgroup", out text))
		{
			eplayGroup = EPlayGroup.Unknown;
		}
		else if (!Enum.TryParse<EPlayGroup>(text, out eplayGroup))
		{
			Log.Error("player-entry has missing or malformed 'playgroup' attribute: " + root.OuterXml);
			Application.Quit();
			return null;
		}
		PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromXml(root, true, "native");
		PersistentPlayerData persistentPlayerData = new PersistentPlayerData(platformUserIdentifierAbs, platformUserIdentifierAbs2, authoredText, eplayGroup);
		if (!root.HasAttribute("lastlogin"))
		{
			Log.Error("player-entry is missing 'lastlogin' attribute: " + root.OuterXml);
			Application.Quit();
			return null;
		}
		if (!StringParsers.TryParseDateTime(root.GetAttribute("lastlogin"), out persistentPlayerData.LastLogin) && !DateTime.TryParse(root.GetAttribute("lastlogin"), out persistentPlayerData.LastLogin))
		{
			Log.Error("player-entry has malfored 'lastlogin' attribute: " + root.OuterXml);
			Application.Quit();
			return null;
		}
		if (!root.HasAttribute("position"))
		{
			Log.Error("player-entry is missing 'position' attribute: " + root.OuterXml);
			Application.Quit();
			return null;
		}
		string[] array = root.GetAttribute("position").Split(',', StringSplitOptions.None);
		if (array.Length < 3)
		{
			Log.Error("player-entry has invalid 'position' attribute: " + root.OuterXml);
			Application.Quit();
			return null;
		}
		persistentPlayerData.Position.x = int.Parse(array[0].Trim());
		persistentPlayerData.Position.y = int.Parse(array[1].Trim());
		persistentPlayerData.Position.z = int.Parse(array[2].Trim());
		foreach (object obj in root.ChildNodes)
		{
			XmlNode xmlNode = (XmlNode)obj;
			if (xmlNode.NodeType == XmlNodeType.Element)
			{
				XmlElement xmlElement = (XmlElement)xmlNode;
				if (readVersion == 0 && xmlNode.Name == "acl")
				{
					if (legacyACL == null)
					{
						legacyACL = new HashSet<PlatformUserIdentifierAbs>();
					}
					PlatformUserIdentifierAbs platformUserIdentifierAbs3 = PlatformUserIdentifierAbs.FromXml(xmlElement, true, null);
					if (platformUserIdentifierAbs3 == null)
					{
						Log.Warning("Ignoring malformed acl-entry: " + xmlNode.OuterXml);
					}
					else
					{
						legacyACL.Add(platformUserIdentifierAbs3);
					}
				}
				else if (xmlNode.Name == "lpblock")
				{
					if (!xmlElement.HasAttribute("pos"))
					{
						Log.Warning("Ignoring lpblock-entry because of missing 'pos' attribute: " + xmlNode.OuterXml);
					}
					else
					{
						string[] array2 = xmlElement.GetAttribute("pos").Split(',', StringSplitOptions.None);
						if (array2.Length < 3)
						{
							Log.Warning("Ignoring lpblock-entry because of malformed 'pos' attribute: " + xmlNode.OuterXml);
						}
						else
						{
							Vector3i vector3i = default(Vector3i);
							vector3i.x = int.Parse(array2[0].Trim());
							vector3i.y = int.Parse(array2[1].Trim());
							vector3i.z = int.Parse(array2[2].Trim());
							if (persistentPlayerData.LPBlocks == null)
							{
								persistentPlayerData.LPBlocks = new List<Vector3i>();
							}
							persistentPlayerData.LPBlocks.Add(vector3i);
						}
					}
				}
				else if (xmlNode.Name == "backpack")
				{
					if (!xmlElement.HasAttribute("pos"))
					{
						Log.Warning("Ignoring backpack-entry because of missing 'pos' attribute: " + xmlNode.OuterXml);
					}
					else if (!xmlElement.HasAttribute("id"))
					{
						Log.Warning("Ignoring backpack-entry because of missing 'id' attribute: " + xmlNode.OuterXml);
					}
					else if (!xmlElement.HasAttribute("timestamp"))
					{
						Log.Warning("Ignoring backpack-entry because of missing 'timestamp' attribute: " + xmlNode.OuterXml);
					}
					else
					{
						string[] array3 = xmlElement.GetAttribute("pos").Split(',', StringSplitOptions.None);
						if (array3.Length < 3)
						{
							Log.Warning("Ignoring backpack-entry because of malformed 'pos' attribute: " + xmlNode.OuterXml);
						}
						else
						{
							int num = int.Parse(xmlElement.GetAttribute("id"));
							Vector3i vector3i2 = default(Vector3i);
							vector3i2.x = int.Parse(array3[0].Trim());
							vector3i2.y = int.Parse(array3[1].Trim());
							vector3i2.z = int.Parse(array3[2].Trim());
							uint num2 = uint.Parse(xmlElement.GetAttribute("timestamp"));
							persistentPlayerData.AddDroppedBackpack(num, vector3i2, num2);
						}
					}
				}
				else if (xmlNode.Name == "bedroll")
				{
					string[] array4 = xmlElement.GetAttribute("pos").Split(',', StringSplitOptions.None);
					if (array4.Length < 3)
					{
						Log.Warning("Ignoring bedroll-entry. Invalid 'pos' attribute: " + xmlNode.OuterXml);
					}
					else
					{
						persistentPlayerData.BedrollPos.x = int.Parse(array4[0].Trim());
						persistentPlayerData.BedrollPos.y = int.Parse(array4[1].Trim());
						persistentPlayerData.BedrollPos.z = int.Parse(array4[2].Trim());
					}
				}
				else
				{
					if (xmlNode.Name == "questpositions")
					{
						persistentPlayerData.QuestPositions = new List<QuestPositionData>();
						using (IEnumerator enumerator2 = xmlNode.ChildNodes.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								object obj2 = enumerator2.Current;
								XmlElement xmlElement2 = (XmlElement)obj2;
								if (xmlElement2.Name == "position")
								{
									int num3 = int.Parse(xmlElement2.GetAttribute("id"));
									Vector3i vector3i3 = default(Vector3i);
									int num4 = int.Parse(xmlElement2.GetAttribute("positiondatatype"));
									string[] array5 = xmlElement2.GetAttribute("pos").Split(',', StringSplitOptions.None);
									if (array5.Length < 3)
									{
										Log.Warning("Ignoring bedroll-entry. Invalid 'pos' attribute: " + xmlNode.OuterXml);
									}
									else
									{
										vector3i3.x = int.Parse(array5[0].Trim());
										vector3i3.y = int.Parse(array5[1].Trim());
										vector3i3.z = int.Parse(array5[2].Trim());
										persistentPlayerData.QuestPositions.Add(new QuestPositionData(num3, (Quest.PositionDataTypes)num4, vector3i3));
									}
								}
							}
							continue;
						}
					}
					if (xmlNode.Name == "vendingmachinepositions")
					{
						persistentPlayerData.OwnedVendingMachinePositions = new List<Vector3i>();
						foreach (object obj3 in xmlNode.ChildNodes)
						{
							XmlElement xmlElement3 = (XmlElement)obj3;
							if (xmlElement3.Name == "position")
							{
								Vector3i vector3i4 = default(Vector3i);
								string[] array6 = xmlElement3.GetAttribute("pos").Split(',', StringSplitOptions.None);
								vector3i4.x = int.Parse(array6[0].Trim());
								vector3i4.y = int.Parse(array6[1].Trim());
								vector3i4.z = int.Parse(array6[2].Trim());
								persistentPlayerData.OwnedVendingMachinePositions.Add(vector3i4);
							}
						}
					}
				}
			}
		}
		return persistentPlayerData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxTrackedBackpacks = 3;

	public PlayerData PlayerData;

	public readonly PersistentPlayerName PlayerName;

	public DateTime LastLogin;

	public int EntityId = -1;

	public List<Vector3i> LPBlocks;

	public const int cBedrollUnsetY = 2147483647;

	public Vector3i BedrollPos = new Vector3i(0, int.MaxValue, 0);

	public Vector3i Position;

	public List<QuestPositionData> QuestPositions = new List<QuestPositionData>();

	public bool questPositionsChanged;

	public List<Vector3i> OwnedVendingMachinePositions = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, PersistentPlayerData.ProtectedBackpack> backpacksByID = new Dictionary<int, PersistentPlayerData.ProtectedBackpack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortedBackpacksDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PersistentPlayerData.ProtectedBackpack> backpacksSortedByTimestamp = new List<PersistentPlayerData.ProtectedBackpack>();

	public delegate void PlayerEventHandler(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason);

	public struct ProtectedBackpack
	{
		public ProtectedBackpack(int entityID, Vector3i position, uint timestamp)
		{
			this.EntityID = entityID;
			this.Position = position;
			this.Timestamp = timestamp;
		}

		public readonly int EntityID;

		public readonly Vector3i Position;

		public readonly uint Timestamp;
	}
}

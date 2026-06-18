using System;
using System.Collections.Generic;
using Platform;
using UniLinq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterConstruct : MonoBehaviour
{
	public bool ShowCharacters
	{
		get
		{
			return this.showCharacters;
		}
		set
		{
			this.showCharacters = value;
			this.ShowAllCharacters();
		}
	}

	public bool ShowGear
	{
		get
		{
			return this.showGear;
		}
		set
		{
			this.showGear = value;
			this.ShowAllCharacters();
		}
	}

	public bool ShowHair
	{
		get
		{
			return this.showHair;
		}
		set
		{
			this.showHair = value;
			this.ShowAllCharacters();
		}
	}

	public bool ShowHatHair
	{
		get
		{
			return this.showHatHair;
		}
		set
		{
			this.showHatHair = value;
			this.ShowAllCharacters();
		}
	}

	public bool ShowFacialHair
	{
		get
		{
			return this.showFacialHair;
		}
		set
		{
			this.showFacialHair = value;
			this.ShowAllCharacters();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string EntityClassName(string sex)
	{
		return string.Format("player{0}", sex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		LoadManager.Init();
		PlatformManager.Init();
		this.characterRoot = new GameObject("CharacterRoot");
		this.characterRoot.transform.SetParent(base.transform);
		this.characterRoot.transform.position = new Vector3(0f, 0f, 0f);
		this.gearRoot = new GameObject("GearRoot");
		this.gearRoot.transform.SetParent(base.transform);
		this.gearRoot.transform.position = new Vector3(0f, 0f, 0f);
		this.hairRoot = new GameObject("HairRoot");
		this.hairRoot.transform.SetParent(base.transform);
		this.hairRoot.transform.position = new Vector3(0f, 0f, 0f);
		this.hatHairRoot = new GameObject("HatHairRoot");
		this.hatHairRoot.transform.SetParent(base.transform);
		this.hatHairRoot.transform.position = new Vector3(0f, 0f, 0f);
		this.facialHairRoot = new GameObject("FacialHairRoot");
		this.facialHairRoot.transform.SetParent(base.transform);
		this.facialHairRoot.transform.position = new Vector3(0f, 0f, 0f);
		for (int i = 0; i < this.sexes.Length; i++)
		{
			GameObject gameObject = new GameObject((i == 0) ? "Male" : "Female");
			gameObject.transform.SetParent(this.characterRoot.transform);
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			for (int j = 0; j < this.races.Length; j++)
			{
				for (int k = 0; k < this.variants.Length; k++)
				{
					Archetype archetype = new Archetype("", i == 0, true);
					archetype.Sex = this.sexes[i];
					archetype.Race = this.races[j];
					archetype.Variant = this.variants[k];
					GameObject gameObject2 = Object.Instantiate<GameObject>(this.baseRig, gameObject.transform);
					SDCSUtils.TransformCatalog transformCatalog = new SDCSUtils.TransformCatalog(gameObject2.transform);
					CharacterConstructUtils.CreateViz(archetype, ref gameObject2, ref transformCatalog);
					gameObject2.name = archetype.Race + this.variants[k].ToString("00");
					Vector3 vector;
					vector..ctor((float)(k + i * 4), 0f, (float)j);
					vector -= new Vector3(3.5f, 0f, 0f);
					gameObject2.transform.SetParent(gameObject.transform);
					gameObject2.transform.localPosition = vector;
					gameObject2.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
					this.Create3DTextLabel(gameObject2, this.sexes[i] + "_" + gameObject2.name);
					this.AddClickableCollider(gameObject2);
					this.allCharacters.Add(gameObject2);
				}
			}
		}
		for (int l = 0; l < this.gear.Length; l++)
		{
			this.allGearKeys.Add(this.gear[l]);
			this.gearPathLookup.Add(this.gear[l], string.Concat(new string[]
			{
				"@:Entities/Player/{sex}/Gear/",
				this.gear[l],
				"/gear{sex}",
				this.gear[l],
				"Prefab.prefab"
			}));
			this.headgearPathLookup.Add(this.gear[l], string.Concat(new string[]
			{
				"@:Entities/Player/{sex}/Gear/",
				this.gear[l],
				"/HeadgearMorphMatrix/{race}{variant}/gear",
				this.gear[l],
				"Head.asset"
			}));
		}
		for (int m = 0; m < this.dlcGear.Length; m++)
		{
			this.allGearKeys.Add(this.dlcGear[m]);
			this.gearPathLookup.Add(this.dlcGear[m], string.Concat(new string[]
			{
				"@:DLC/",
				this.dlcGear[m],
				"Cosmetic/Entities/Player/{sex}/Gear/Prefabs/gear{sex}",
				this.dlcGear[m],
				"Prefab.prefab"
			}));
			this.headgearPathLookup.Add(this.dlcGear[m], string.Concat(new string[]
			{
				"@:DLC/",
				this.dlcGear[m],
				"Cosmetic/Entities/Player/{sex}/Gear/HeadgearMorphMatrix/{race}{variant}/gear",
				this.dlcGear[m],
				"{hair}Head.asset"
			}));
		}
		for (int n = 0; n < this.twitchDropGear.Length; n++)
		{
			string text = this.twitchDropGear[n].Replace("StoreGear", "");
			this.allGearKeys.Add(this.twitchDropGear[n]);
			this.gearPathLookup.Add(this.twitchDropGear[n], string.Concat(new string[]
			{
				"@:TwitchDrops/",
				this.twitchDropGear[n],
				"/Entities/Player/{sex}/Gear/Prefabs/gear{sex}",
				text,
				"Prefab.prefab"
			}));
			this.headgearPathLookup.Add(this.twitchDropGear[n], string.Concat(new string[]
			{
				"@:TwitchDrops/",
				this.twitchDropGear[n],
				"/Entities/Player/{sex}/Gear/HeadgearMorphMatrix/{race}{variant}/gear",
				text,
				"{hair}Head.asset"
			}));
		}
		this.CreateGearInstances();
		this.CreateHairInstances();
		this.CreateHatHairInstances();
		this.CreateFacialHairInstances();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateGearInstances()
	{
		foreach (GameObject gameObject in this.allGear)
		{
			Object.Destroy(gameObject);
		}
		this.allGear.Clear();
		foreach (object obj in this.gearRoot.transform)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		for (int i = 0; i < this.sexes.Length; i++)
		{
			GameObject gameObject2 = new GameObject((i == 0) ? "Male" : "Female");
			gameObject2.transform.SetParent(this.gearRoot.transform);
			gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
			for (int j = 0; j < this.allGearKeys.Count; j++)
			{
				Archetype archetype = new Archetype("", i == 0, true);
				archetype.Sex = this.sexes[i];
				archetype.Race = this.races[this.selectedRaceIndex];
				archetype.Variant = this.variants[this.selectedVariantIndex];
				archetype.Equipment = new List<SDCSUtils.SlotData>();
				for (int k = 0; k < CharacterConstruct.baseParts.Length; k++)
				{
					SDCSUtils.SlotData slotData = new SDCSUtils.SlotData();
					slotData.PartName = CharacterConstruct.baseParts[k];
					if (CharacterConstruct.baseParts[k] != "head" || this.headHiders.Contains(this.allGearKeys[j]) || this.noMorphs.Contains(this.allGearKeys[j]))
					{
						slotData.PrefabName = this.gearPathLookup[this.allGearKeys[j]];
						slotData.BaseToTurnOff = ((CharacterConstruct.baseParts[k] == "head" && !this.headHiders.Contains(this.allGearKeys[j])) ? null : CharacterConstruct.baseParts[k]);
					}
					else
					{
						slotData.PrefabName = this.headgearPathLookup[this.allGearKeys[j]];
						slotData.BaseToTurnOff = null;
					}
					archetype.Equipment.Add(slotData);
				}
				GameObject gameObject3 = Object.Instantiate<GameObject>(this.baseRig, gameObject2.transform);
				SDCSUtils.TransformCatalog transformCatalog = new SDCSUtils.TransformCatalog(gameObject3.transform);
				CharacterConstructUtils.CreateViz(archetype, ref gameObject3, ref transformCatalog);
				gameObject3.name = this.allGearKeys[j];
				Vector3 vector;
				vector..ctor((float)(j + 5), 0f, (float)i);
				gameObject3.transform.SetParent(gameObject2.transform);
				gameObject3.transform.localPosition = vector;
				gameObject3.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				this.Create3DTextLabel(gameObject3, archetype.Sex + "_" + this.allGearKeys[j]);
				this.AddClickableCollider(gameObject3);
				this.allGear.Add(gameObject3);
			}
		}
		foreach (GameObject gameObject4 in this.allGear)
		{
			gameObject4.SetActive(this.showGear);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateHairInstances()
	{
		foreach (GameObject gameObject in this.allHair)
		{
			Object.Destroy(gameObject);
		}
		this.allHair.Clear();
		foreach (object obj in this.hairRoot.transform)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		for (int i = 0; i < this.sexes.Length; i++)
		{
			GameObject gameObject2 = new GameObject((i == 0) ? "Male" : "Female");
			gameObject2.transform.SetParent(this.hairRoot.transform);
			gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
			for (int j = -1; j < this.hairs.Length; j++)
			{
				Archetype archetype = new Archetype("", i == 0, true);
				archetype.Sex = this.sexes[i];
				archetype.Race = this.races[this.selectedRaceIndex];
				archetype.Variant = this.variants[this.selectedVariantIndex];
				archetype.Hair = ((j == -1) ? "none" : this.hairs[j]);
				archetype.HairColor = "04 Brown";
				GameObject gameObject3 = Object.Instantiate<GameObject>(this.baseRig, gameObject2.transform);
				SDCSUtils.TransformCatalog transformCatalog = new SDCSUtils.TransformCatalog(gameObject3.transform);
				CharacterConstructUtils.CreateViz(archetype, ref gameObject3, ref transformCatalog);
				gameObject3.name = ((j == -1) ? "none" : archetype.Hair);
				Vector3 vector;
				vector..ctor((float)(j + 6), 0f, (float)(i + 2));
				gameObject3.transform.SetParent(gameObject2.transform);
				gameObject3.transform.localPosition = vector;
				gameObject3.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				this.Create3DTextLabel(gameObject3, (j == -1) ? "none" : (archetype.Sex + "_" + archetype.Hair));
				this.AddClickableCollider(gameObject3);
				this.allHair.Add(gameObject3);
			}
		}
		foreach (GameObject gameObject4 in this.allHair)
		{
			gameObject4.SetActive(this.showHair);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateHatHairInstances()
	{
		foreach (GameObject gameObject in this.allHatHair)
		{
			Object.Destroy(gameObject);
		}
		this.allHatHair.Clear();
		foreach (object obj in this.hatHairRoot.transform)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		for (int i = 0; i < this.sexes.Length; i++)
		{
			GameObject gameObject2 = new GameObject((i == 0) ? "Male" : "Female");
			gameObject2.transform.SetParent(this.hatHairRoot.transform);
			gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
			for (int j = -1; j < this.hairs.Length; j++)
			{
				Archetype archetype = new Archetype("", i == 0, true);
				archetype.Sex = this.sexes[i];
				archetype.Race = this.races[this.selectedRaceIndex];
				archetype.Variant = this.variants[this.selectedVariantIndex];
				archetype.Hair = ((j == -1) ? "none" : this.hairs[j]);
				archetype.HairColor = "04 Brown";
				archetype.Equipment = new List<SDCSUtils.SlotData>();
				SDCSUtils.SlotData slotData = new SDCSUtils.SlotData();
				slotData.PartName = CharacterConstruct.baseParts[0];
				if (this.headHiders.Contains(this.allGearKeys[this.hatHairGearIndex]) || this.noMorphs.Contains(this.allGearKeys[this.hatHairGearIndex]))
				{
					slotData.PrefabName = this.gearPathLookup[this.allGearKeys[this.hatHairGearIndex]];
					slotData.BaseToTurnOff = ((!this.headHiders.Contains(this.allGearKeys[this.hatHairGearIndex])) ? null : CharacterConstruct.baseParts[0]);
				}
				else
				{
					slotData.PrefabName = this.headgearPathLookup[this.allGearKeys[this.hatHairGearIndex]];
					slotData.BaseToTurnOff = null;
				}
				slotData.HairMaskType = SDCSUtils.SlotData.HairMaskTypes.Hat;
				archetype.Equipment.Add(slotData);
				GameObject gameObject3 = Object.Instantiate<GameObject>(this.baseRig, gameObject2.transform);
				SDCSUtils.TransformCatalog transformCatalog = new SDCSUtils.TransformCatalog(gameObject3.transform);
				CharacterConstructUtils.CreateViz(archetype, ref gameObject3, ref transformCatalog);
				gameObject3.name = ((j == -1) ? "none" : (archetype.Hair + "_hat"));
				Vector3 vector;
				vector..ctor((float)(j + 6), 0f, (float)(i + 4));
				gameObject3.transform.SetParent(gameObject2.transform);
				gameObject3.transform.localPosition = vector;
				gameObject3.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				this.Create3DTextLabel(gameObject3, (j == -1) ? "none" : (archetype.Sex + "_" + archetype.Hair));
				this.AddClickableCollider(gameObject3);
				this.allHatHair.Add(gameObject3);
			}
		}
		foreach (GameObject gameObject4 in this.allHatHair)
		{
			gameObject4.SetActive(this.showHatHair);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateFacialHairInstances()
	{
		foreach (GameObject gameObject in this.allFacialHair)
		{
			Object.Destroy(gameObject);
		}
		this.allFacialHair.Clear();
		foreach (object obj in this.facialHairRoot.transform)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		Archetype archetype = new Archetype("", true, true);
		archetype.Sex = this.sexes[0];
		archetype.Race = this.races[this.selectedRaceIndex];
		archetype.Variant = this.variants[this.selectedVariantIndex];
		for (int i = 0; i < this.facialHairIndexes.Length; i++)
		{
			archetype.Hair = this.hairs[0];
			archetype.HairColor = "04 Brown";
			archetype.MustacheName = this.facialHairIndexes[i].ToString();
			archetype.ChopsName = this.facialHairIndexes[i].ToString();
			archetype.BeardName = this.facialHairIndexes[i].ToString();
			GameObject gameObject2 = Object.Instantiate<GameObject>(this.baseRig, this.facialHairRoot.transform);
			SDCSUtils.TransformCatalog transformCatalog = new SDCSUtils.TransformCatalog(gameObject2.transform);
			CharacterConstructUtils.CreateViz(archetype, ref gameObject2, ref transformCatalog);
			gameObject2.name = "FacialHair " + i.ToString("00");
			Vector3 vector;
			vector..ctor((float)(i + 5), 0f, 6f);
			gameObject2.transform.SetParent(this.facialHairRoot.transform);
			gameObject2.transform.localPosition = vector;
			gameObject2.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			this.Create3DTextLabel(gameObject2, "FacialHair " + i.ToString("00"));
			this.AddClickableCollider(gameObject2);
			this.allFacialHair.Add(gameObject2);
		}
		foreach (GameObject gameObject3 in this.allFacialHair)
		{
			gameObject3.SetActive(this.showFacialHair);
		}
	}

	public void RespawnAllGroups()
	{
		this.ShowAllCharacters();
		this.CreateGearInstances();
		this.CreateHairInstances();
		this.CreateHatHairInstances();
		this.CreateFacialHairInstances();
	}

	public void RespawnHatHairGroup()
	{
		this.ShowAllCharacters();
		this.CreateHatHairInstances();
	}

	public string[] GetGearTypes()
	{
		return this.allGearKeys.ToArray();
	}

	public string[] GetRaceTypes()
	{
		return this.races;
	}

	public string[] GetVariantTypes()
	{
		string[] array = new string[this.variants.Length];
		for (int i = 0; i < this.variants.Length; i++)
		{
			array[i] = this.variants[i].ToString();
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Create3DTextLabel(GameObject instance, string labelText)
	{
		GameObject gameObject = new GameObject(instance.name + "_Label");
		gameObject.transform.SetParent(instance.transform);
		gameObject.transform.localPosition = new Vector3(0f, this.labelHeight, 0f);
		GameObject gameObject2 = GameObject.CreatePrimitive(5);
		gameObject2.name = "LabelBackground";
		gameObject2.transform.SetParent(gameObject.transform);
		gameObject2.transform.localPosition = new Vector3(0f, 0f, 0.01f);
		gameObject2.transform.localScale = new Vector3((float)labelText.Length * 0.175f, 0.4f, 1f);
		MeshRenderer component = gameObject2.GetComponent<MeshRenderer>();
		component.material = new Material(Shader.Find("Unlit/Color"));
		component.material.color = new Color(0f, 0f, 0f, 0.7f);
		component.shadowCastingMode = 0;
		component.receiveShadows = false;
		GameObject gameObject3 = new GameObject("Text");
		gameObject3.transform.SetParent(gameObject.transform);
		gameObject3.transform.localPosition = Vector3.zero;
		TextMesh textMesh = gameObject3.AddComponent<TextMesh>();
		textMesh.text = labelText;
		textMesh.fontSize = 30;
		textMesh.characterSize = 0.1f;
		textMesh.alignment = 1;
		textMesh.anchor = 4;
		textMesh.color = this.labelColor;
		MeshRenderer component2 = textMesh.GetComponent<MeshRenderer>();
		component2.shadowCastingMode = 0;
		component2.receiveShadows = false;
		component2.sortingOrder = 1;
		gameObject.transform.localScale = new Vector3(this.labelScale, this.labelScale, this.labelScale);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddClickableCollider(GameObject instance)
	{
		Collider[] componentsInChildren = instance.GetComponentsInChildren<Collider>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("No colliders found in " + instance.name + ". Character won't be clickable.");
			return;
		}
		instance.AddComponent<CharacterClickHandler>().parentScript = this;
		Collider[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.AddComponent<CharacterColliderHelper>().mainInstance = instance;
		}
	}

	public void OnCharacterClicked(GameObject clickedCharacter)
	{
		if (this.selectedCharacter == clickedCharacter)
		{
			this.ShowAllCharacters();
			return;
		}
		this.HideAllExcept(clickedCharacter);
	}

	public void ShowAllCharacters()
	{
		this.selectedCharacter = null;
		this.allCharactersVisible = true;
		foreach (GameObject gameObject in this.allCharacters)
		{
			gameObject.SetActive(this.showCharacters);
		}
		foreach (GameObject gameObject2 in this.allGear)
		{
			gameObject2.SetActive(this.showGear);
		}
		foreach (GameObject gameObject3 in this.allHair)
		{
			gameObject3.SetActive(this.showHair);
		}
		foreach (GameObject gameObject4 in this.allHatHair)
		{
			gameObject4.SetActive(this.showHatHair);
		}
		foreach (GameObject gameObject5 in this.allFacialHair)
		{
			gameObject5.SetActive(this.showFacialHair);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HideAllExcept(GameObject exception)
	{
		this.selectedCharacter = exception;
		this.allCharactersVisible = false;
		foreach (GameObject gameObject in this.allCharacters)
		{
			gameObject.SetActive(gameObject == exception);
		}
		foreach (GameObject gameObject2 in this.allGear)
		{
			gameObject2.SetActive(gameObject2 == exception);
		}
		foreach (GameObject gameObject3 in this.allHair)
		{
			gameObject3.SetActive(gameObject3 == exception);
		}
		foreach (GameObject gameObject4 in this.allHatHair)
		{
			gameObject4.SetActive(gameObject4 == exception);
		}
		foreach (GameObject gameObject5 in this.allFacialHair)
		{
			gameObject5.SetActive(gameObject5 == exception);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.allCharactersVisible)
		{
			this.characterRoot.SetActive(this.showCharacters);
			this.gearRoot.SetActive(this.showGear);
			this.hairRoot.SetActive(this.showHair);
			this.hatHairRoot.SetActive(this.showHatHair);
			this.facialHairRoot.SetActive(this.showFacialHair);
		}
		if (Input.GetMouseButtonDown(0))
		{
			this.CheckMouseClick();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckMouseClick()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (EventSystem.current.IsPointerOverGameObject())
		{
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(ray, ref raycastHit))
		{
			CharacterColliderHelper component = raycastHit.collider.gameObject.GetComponent<CharacterColliderHelper>();
			if (component != null && component.mainInstance != null)
			{
				CharacterClickHandler component2 = component.mainInstance.GetComponent<CharacterClickHandler>();
				if (component2 != null)
				{
					component2.HandleClick();
				}
			}
		}
	}

	public GameObject baseRig;

	public int hairColorIndex;

	public int hatHairGearIndex;

	public int selectedRaceIndex;

	public int selectedVariantIndex;

	public float labelHeight = 2f;

	public float labelScale = 0.1f;

	public Color labelColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool showCharacters = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool showGear;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool showHair;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool showHatHair;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool showFacialHair;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject selectedCharacter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool allCharactersVisible = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] sexes = new string[] { "Male", "Female" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] races = new string[] { "White", "Black", "Asian", "Native" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] variants = new int[] { 1, 2, 3, 4 };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] hairs = new string[]
	{
		"buzzcut", "comb_over", "slicked_back", "slicked_back_long", "pixie_cut", "ponytail", "midpart_karen_messy", "midpart_long", "midpart_mid", "midpart_short",
		"midpart_shoulder", "sidepart_short", "sidepart_mid", "sidepart_long", "cornrows", "mohawk", "flattop_fro", "small_fro", "dreads", "afro_curly"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] facialHairIndexes = new int[] { 1, 2, 3, 4, 5 };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] gear = new string[]
	{
		"Assassin", "Biker", "Commando", "Enforcer", "Farmer", "Fiber", "Fitness", "LumberJack", "Miner", "Nerd",
		"Nomad", "Preacher", "Raider", "Ranger", "Scavenger", "Stealth"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] dlcGear = new string[] { "DarkKnight", "Desert", "Hoarder", "Marauder", "CrimsonWarlord", "Samurai" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] twitchDropGear = new string[] { "Watcher", "PimpHatBlue", "PimpHatPurple" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<string> allGearKeys = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, string> gearPathLookup = new Dictionary<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, string> headgearPathLookup = new Dictionary<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] headHiders = new string[] { "Assassin", "Fiber", "Nomad", "Raider", "Watcher" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] noMorphs = new string[] { "Hoarder", "Marauder" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string[] baseParts = new string[] { "head", "body", "hands", "feet" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject characterRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject gearRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject hairRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject hatHairRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject facialHairRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameObject> allCharacters = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameObject> allGear = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameObject> allHair = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameObject> allHatHair = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameObject> allFacialHair = new List<GameObject>();
}

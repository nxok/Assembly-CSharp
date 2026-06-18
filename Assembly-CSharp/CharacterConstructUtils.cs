using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GearVariants;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public static class CharacterConstructUtils
{
	public static GameObject Stitch(GameObject sourceObj, GameObject parentObj, SDCSUtils.TransformCatalog boneCatalog, Material eyeMat = null)
	{
		GameObject gameObject = Object.Instantiate<GameObject>(sourceObj, parentObj.transform);
		gameObject.name = sourceObj.name;
		gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(CharacterConstructUtils.tempSMRs);
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in CharacterConstructUtils.tempSMRs)
		{
			string name = skinnedMeshRenderer.gameObject.name;
			skinnedMeshRenderer.bones = CharacterConstructUtils.TranslateTransforms(skinnedMeshRenderer.bones, boneCatalog);
			skinnedMeshRenderer.rootBone = CharacterConstructUtils.Find<string, Transform>(boneCatalog, skinnedMeshRenderer.rootBone.name);
			skinnedMeshRenderer.updateWhenOffscreen = true;
			Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				if (sharedMaterials[i] && sharedMaterials[i].HasColor("_Tint"))
				{
					string name2 = sharedMaterials[i].name;
					Material material = null;
					if (name2.Contains("_Body"))
					{
						material = DataLoader.LoadAsset<Material>(CharacterConstructUtils.baseBodyMatLoc, false);
					}
					else if (name2.Contains("_Head"))
					{
						material = DataLoader.LoadAsset<Material>(CharacterConstructUtils.baseHeadMatLoc, false);
					}
					else if (name2.Contains("_Hand"))
					{
						material = DataLoader.LoadAsset<Material>(CharacterConstructUtils.baseHandsMatLoc, false);
					}
					if (material != null && material.HasColor("_Tint"))
					{
						sharedMaterials[i] = new Material(sharedMaterials[i]);
						sharedMaterials[i].SetColor("_Tint", material.GetColor("_Tint"));
					}
				}
			}
			if (name == "eyes" && eyeMat)
			{
				sharedMaterials[0] = eyeMat;
			}
			skinnedMeshRenderer.sharedMaterials = sharedMaterials;
		}
		CharacterConstructUtils.tempSMRs.Clear();
		Transform transform = boneCatalog["Hips"];
		gameObject.GetComponentsInChildren<Cloth>(CharacterConstructUtils.tempCloths);
		foreach (Cloth cloth in CharacterConstructUtils.tempCloths)
		{
			cloth.capsuleColliders = transform.GetComponentsInChildren<CapsuleCollider>();
		}
		CharacterConstructUtils.tempCloths.Clear();
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> CollectRequiredNamesForSlot(Transform root, Transform slotSubRoot)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		GearBoneMap component = root.GetComponent<GearBoneMap>();
		if (component != null)
		{
			IReadOnlyList<Transform> partBones = component.GetPartBones(slotSubRoot.name);
			if (partBones == null || partBones.Count <= 0)
			{
				goto IL_013B;
			}
			using (IEnumerator<Transform> enumerator = partBones.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Transform transform = enumerator.Current;
					if (transform)
					{
						hashSet.Add(transform.name);
					}
				}
				goto IL_013B;
			}
		}
		Debug.LogWarning(string.Concat(new string[] { "[SDCSUtils] No GearBoneMap found on root ", root.name, ", falling back to collecting all bones from SMRs under ", slotSubRoot.name, "." }));
		CharacterConstructUtils._smrBuf.Clear();
		slotSubRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true, CharacterConstructUtils._smrBuf);
		for (int i = 0; i < CharacterConstructUtils._smrBuf.Count; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = CharacterConstructUtils._smrBuf[i];
			Transform[] array = ((skinnedMeshRenderer != null) ? skinnedMeshRenderer.bones : null);
			if (array != null)
			{
				foreach (Transform transform2 in array)
				{
					if (transform2)
					{
						hashSet.Add(transform2.name);
					}
				}
			}
		}
		IL_013B:
		hashSet = CharacterConstructUtils.BuildAllowedWithAncestors(root, hashSet);
		CharacterConstructUtils._hingeBuf.Clear();
		slotSubRoot.GetComponentsInChildren<HingeJoint>(true, CharacterConstructUtils._hingeBuf);
		for (int k = 0; k < CharacterConstructUtils._hingeBuf.Count; k++)
		{
			HingeJoint hingeJoint = CharacterConstructUtils._hingeBuf[k];
			if (hingeJoint)
			{
				hashSet.Add(hingeJoint.transform.name);
				if (hingeJoint.connectedBody)
				{
					hashSet.Add(hingeJoint.connectedBody.transform.name);
				}
			}
		}
		CharacterConstructUtils._bcBuf.Clear();
		root.GetComponentsInChildren<BlendConstraint>(true, CharacterConstructUtils._bcBuf);
		for (int l = 0; l < CharacterConstructUtils._bcBuf.Count; l++)
		{
			BlendConstraint blendConstraint = CharacterConstructUtils._bcBuf[l];
			if (blendConstraint)
			{
				Transform constrainedObject = blendConstraint.data.constrainedObject;
				Transform sourceObjectA = blendConstraint.data.sourceObjectA;
				Transform sourceObjectB = blendConstraint.data.sourceObjectB;
				if (constrainedObject && hashSet.Contains(constrainedObject.name))
				{
					if (sourceObjectA)
					{
						hashSet.Add(sourceObjectA.name);
					}
					if (sourceObjectB)
					{
						hashSet.Add(sourceObjectB.name);
					}
				}
			}
		}
		return CharacterConstructUtils.BuildAllowedWithAncestors(root, hashSet);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Transform> MapSourceByName(Transform sourceOrigin)
	{
		CharacterConstructUtils.<>c__DisplayClass9_0 CS$<>8__locals1;
		CS$<>8__locals1.map = new Dictionary<string, Transform>(StringComparer.Ordinal);
		CharacterConstructUtils.<MapSourceByName>g__Recurse|9_0(sourceOrigin, ref CS$<>8__locals1);
		return CS$<>8__locals1.map;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> BuildAllowedWithAncestors(Transform sourceOrigin, HashSet<string> allowedBones)
	{
		HashSet<string> hashSet = new HashSet<string>(allowedBones, StringComparer.Ordinal);
		Dictionary<string, Transform> dictionary = CharacterConstructUtils.MapSourceByName(sourceOrigin);
		foreach (string text in allowedBones)
		{
			Transform transform;
			if (dictionary.TryGetValue(text, out transform))
			{
				Transform transform2 = transform.parent;
				while (transform2 != null)
				{
					hashSet.Add(transform2.name);
					if (transform2 == sourceOrigin)
					{
						break;
					}
					transform2 = transform2.parent;
				}
			}
		}
		return hashSet;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddRequiredChildren(Transform sourceT, Transform targetT, SDCSUtils.TransformCatalog catalog, HashSet<string> allowedBones, AuxBoneTracker auxBoneTracker = null)
	{
		if (auxBoneTracker == null && catalog != null && catalog["Origin"] != null)
		{
			auxBoneTracker = catalog["Origin"].GetComponent<AuxBoneTracker>();
		}
		for (int i = 0; i < sourceT.childCount; i++)
		{
			Transform child = sourceT.GetChild(i);
			if (allowedBones.Contains(child.name))
			{
				Transform transform = null;
				for (int j = 0; j < targetT.childCount; j++)
				{
					Transform child2 = targetT.GetChild(j);
					if (child2.name == child.name)
					{
						transform = child2;
						break;
					}
				}
				if (!transform)
				{
					transform = Object.Instantiate<GameObject>(child.gameObject, targetT, true).transform;
					transform.name = child.name;
					transform.SetLocalPositionAndRotation(child.localPosition, child.localRotation);
					transform.localScale = child.localScale;
					if (auxBoneTracker && !auxBoneTracker.AuxBoneLookup.ContainsKey(transform.name))
					{
						auxBoneTracker.AuxBoneLookup.Add(transform.name, transform);
					}
				}
				if (!catalog.ContainsKey(transform.name))
				{
					catalog.Add(transform.name, transform);
				}
				CharacterConstructUtils.TransferCharacterJoint(child, transform.gameObject, catalog);
				CharacterConstructUtils.AddRequiredChildren(child, transform, catalog, allowedBones, auxBoneTracker);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupRigConstraints(RigBuilder rigBuilder, Transform sourceRootT, Transform targetRootT, SDCSUtils.TransformCatalog catalog, HashSet<string> allowedBones)
	{
		Transform transform = sourceRootT.parent.Find("RigConstraints");
		if (!transform)
		{
			return;
		}
		string text = "RigConstraints_" + sourceRootT.name;
		Transform transform2 = targetRootT.Find(text);
		if (!transform2)
		{
			transform2 = new GameObject(text).transform;
			transform2.SetParent(targetRootT, false);
		}
		Rig rig = transform2.gameObject.GetOrAddComponent<Rig>();
		if (!rigBuilder.layers.Any((RigLayer l) => l.rig == rig))
		{
			rigBuilder.layers.Add(new RigLayer(rig, true));
		}
		for (int i = transform2.childCount - 1; i >= 0; i--)
		{
			Object.DestroyImmediate(transform2.GetChild(i).gameObject);
		}
		foreach (BlendConstraint blendConstraint in transform.GetComponentsInChildren<BlendConstraint>(true))
		{
			string text2 = ((blendConstraint.data.constrainedObject != null) ? blendConstraint.data.constrainedObject.name : null);
			if (text2 != null && allowedBones.Contains(text2))
			{
				GameObject gameObject = Object.Instantiate<GameObject>(blendConstraint.gameObject, transform2, false);
				gameObject.name = blendConstraint.name;
				BlendConstraint component = gameObject.GetComponent<BlendConstraint>();
				component.data.constrainedObject = ((text2 != null) ? CharacterConstructUtils.Find<string, Transform>(catalog, text2) : null);
				string text3 = ((blendConstraint.data.sourceObjectA != null) ? blendConstraint.data.sourceObjectA.name : null);
				string text4 = ((blendConstraint.data.sourceObjectB != null) ? blendConstraint.data.sourceObjectB.name : null);
				component.data.sourceObjectA = ((text3 != null) ? CharacterConstructUtils.Find<string, Transform>(catalog, text3) : null);
				component.data.sourceObjectB = ((text4 != null) ? CharacterConstructUtils.Find<string, Transform>(catalog, text4) : null);
			}
		}
	}

	public static void MatchRigs(Transform source, Transform target, SDCSUtils.TransformCatalog catalog, HashSet<string> allowedBones)
	{
		Transform transform = source.Find("Origin");
		Transform transform2 = target.Find("Origin");
		if (!transform || !transform2)
		{
			return;
		}
		AuxBoneTracker auxBoneTracker = null;
		if (catalog != null && catalog["Origin"] != null)
		{
			auxBoneTracker = catalog["Origin"].GetComponent<AuxBoneTracker>();
		}
		CharacterConstructUtils.AddRequiredChildren(transform, transform2, catalog, allowedBones, auxBoneTracker);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void TransferCharacterJoint(Transform source, GameObject newBone, SDCSUtils.TransformCatalog transformCatalog)
	{
		CharacterJoint component;
		CharacterJoint characterJoint;
		if ((component = source.GetComponent<CharacterJoint>()) != null && (characterJoint = NGUITools.AddMissingComponent<CharacterJoint>(newBone)) != null)
		{
			Transform transform = CharacterConstructUtils.Find<string, Transform>(transformCatalog, component.connectedBody.name);
			characterJoint.connectedBody = ((transform != null) ? transform.GetComponent<Rigidbody>() : null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform[] TranslateTransforms(Transform[] transforms, SDCSUtils.TransformCatalog transformCatalog)
	{
		for (int i = 0; i < transforms.Length; i++)
		{
			Transform transform = transforms[i];
			if (transform)
			{
				transforms[i] = CharacterConstructUtils.Find<string, Transform>(transformCatalog, transform.name);
			}
			else
			{
				Log.Error("Null transform in bone list");
			}
		}
		return transforms;
	}

	public static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key)
	{
		TValue tvalue;
		source.TryGetValue(key, out tvalue);
		return tvalue;
	}

	public static string baseBodyLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/Common/Meshes/player",
				CharacterConstructUtils.archetype.Sex,
				".fbx"
			});
		}
	}

	public static string baseHeadLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/Heads/",
				CharacterConstructUtils.archetype.Race,
				"/",
				CharacterConstructUtils.archetype.Variant.ToString("00"),
				"/Meshes/player",
				CharacterConstructUtils.archetype.Sex,
				CharacterConstructUtils.archetype.Race,
				CharacterConstructUtils.archetype.Variant.ToString("00"),
				".fbx"
			});
		}
	}

	public static string baseHairLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/Hair/",
				CharacterConstructUtils.archetype.Hair,
				"/HairMorphMatrix/",
				CharacterConstructUtils.archetype.Race,
				CharacterConstructUtils.archetype.Variant.ToString("00")
			});
		}
	}

	public static string baseMustacheLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/FacialHair/Mustache/",
				CharacterConstructUtils.archetype.MustacheName,
				"/HairMorphMatrix/",
				CharacterConstructUtils.archetype.Race,
				CharacterConstructUtils.archetype.Variant.ToString("00")
			});
		}
	}

	public static string baseChopsLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/FacialHair/Chops/",
				CharacterConstructUtils.archetype.ChopsName,
				"/HairMorphMatrix/",
				CharacterConstructUtils.archetype.Race,
				CharacterConstructUtils.archetype.Variant.ToString("00")
			});
		}
	}

	public static string baseBeardLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/FacialHair/Beard/",
				CharacterConstructUtils.archetype.BeardName,
				"/HairMorphMatrix/",
				CharacterConstructUtils.archetype.Race,
				CharacterConstructUtils.archetype.Variant.ToString("00")
			});
		}
	}

	public static string baseHairColorLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/Common/HairColorSwatches";
		}
	}

	public static string baseEyeColorMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/Common/Eyes/Materials/" + CharacterConstructUtils.archetype.EyeColorName + ".mat";
		}
	}

	public static string baseBodyMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/Heads/",
				CharacterConstructUtils.archetype.Race,
				"/",
				CharacterConstructUtils.archetype.Variant.ToString("00"),
				"/Materials/player",
				CharacterConstructUtils.archetype.Sex,
				CharacterConstructUtils.archetype.Race,
				CharacterConstructUtils.archetype.Variant.ToString("00"),
				"_Body.mat"
			});
		}
	}

	public static string baseHeadMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/Heads/",
				CharacterConstructUtils.archetype.Race,
				"/",
				CharacterConstructUtils.archetype.Variant.ToString("00"),
				"/Materials/player",
				CharacterConstructUtils.archetype.Sex,
				CharacterConstructUtils.archetype.Race,
				CharacterConstructUtils.archetype.Variant.ToString("00"),
				"_Head.mat"
			});
		}
	}

	public static string baseHandsMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return string.Concat(new string[]
			{
				"@:Entities/Player/",
				CharacterConstructUtils.archetype.Sex,
				"/Heads/",
				CharacterConstructUtils.archetype.Race,
				"/",
				CharacterConstructUtils.archetype.Variant.ToString("00"),
				"/Materials/player",
				CharacterConstructUtils.archetype.Sex,
				CharacterConstructUtils.archetype.Race,
				CharacterConstructUtils.archetype.Variant.ToString("00"),
				"_Hand.mat"
			});
		}
	}

	public static string baseRigPrefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/Common/BaseRigs/baseRigPrefab.prefab";
		}
	}

	public static RuntimeAnimatorController UIAnimController
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DataLoader.LoadAsset<RuntimeAnimatorController>("@:Entities/Player/Common/AnimControllers/MenuSDCS" + CharacterConstructUtils.archetype.Sex + "Controller" + (CharacterConstructUtils.archetype.IsMale ? ".controller" : ".overrideController"), false);
		}
	}

	public static void CreateViz(Archetype _archetype, ref GameObject baseRigUI, ref SDCSUtils.TransformCatalog boneCatalogUI)
	{
		CharacterConstructUtils.DestroyViz(baseRigUI, false);
		CharacterConstructUtils.archetype = _archetype;
		CharacterConstructUtils.setupRig(ref baseRigUI, ref boneCatalogUI, CharacterConstructUtils.baseRigPrefab, null, CharacterConstructUtils.UIAnimController);
		CharacterConstructUtils.setupBase(baseRigUI, boneCatalogUI, CharacterConstructUtils.baseParts);
		CharacterConstructUtils.setupHairObjects(baseRigUI, boneCatalogUI, _archetype.Hair, _archetype.MustacheName, _archetype.ChopsName, _archetype.BeardName);
		CharacterConstructUtils.setupEquipment(baseRigUI, boneCatalogUI, _archetype.Equipment, true);
		Transform transform = baseRigUI.transform.Find("IKRig");
		if (transform != null)
		{
			transform.GetComponent<Rig>().weight = 0f;
		}
		foreach (HingeJoint hingeJoint in baseRigUI.GetComponentsInChildren<HingeJoint>())
		{
			if (hingeJoint.connectedBody == null)
			{
				Log.Warning("SDCSUtils::CreateVizUI: No connected body for " + hingeJoint.transform.name + "'s HingeJoint! Disabling for UI until this is solved.");
				hingeJoint.gameObject.SetActive(false);
			}
		}
	}

	public static void DestroyViz(GameObject _baseRigUI, bool _keepRig = false)
	{
		if (_baseRigUI)
		{
			Transform transform = _baseRigUI.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.name != "Origin")
				{
					child.GetComponentsInChildren<SkinnedMeshRenderer>(true, CharacterConstructUtils.tempSMRs);
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in CharacterConstructUtils.tempSMRs)
					{
						Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
						if (MeshMorph.IsInstance(sharedMesh))
						{
							Object.Destroy(sharedMesh);
						}
						skinnedMeshRenderer.GetSharedMaterials(CharacterConstructUtils.tempMats);
						Utils.CleanupMaterials<List<Material>>(CharacterConstructUtils.tempMats);
						CharacterConstructUtils.tempMats.Clear();
					}
				}
			}
			if (!_keepRig)
			{
				Object.DestroyImmediate(_baseRigUI);
			}
		}
	}

	public static void SetVisible(GameObject _baseRigUI, bool _visible)
	{
		if (_baseRigUI)
		{
			Transform transform = _baseRigUI.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.name != "Origin")
				{
					child.GetComponentsInChildren<SkinnedMeshRenderer>(true, CharacterConstructUtils.tempSMRs);
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in CharacterConstructUtils.tempSMRs)
					{
						skinnedMeshRenderer.gameObject.SetActive(_visible);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupRig(ref GameObject _rigObj, ref SDCSUtils.TransformCatalog _boneCatalog, string prefabLocation, Transform parent, RuntimeAnimatorController animController)
	{
		if (!_rigObj)
		{
			_rigObj = Object.Instantiate<GameObject>(DataLoader.LoadAsset<GameObject>(prefabLocation, false), parent);
			_boneCatalog = new SDCSUtils.TransformCatalog(_rigObj.transform);
			BoneRenderer[] componentsInChildren = _rigObj.GetComponentsInChildren<BoneRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		else
		{
			CharacterConstructUtils.cleanupEquipment(_rigObj, _boneCatalog);
		}
		Animator component = _rigObj.GetComponent<Animator>();
		if (component && component.runtimeAnimatorController != animController)
		{
			component.runtimeAnimatorController = animController;
		}
		if (!CharacterConstructUtils.archetype.IsMale)
		{
			CapsuleCollider orAddComponent = _boneCatalog["Hips"].gameObject.GetOrAddComponent<CapsuleCollider>();
			orAddComponent.center = new Vector3(0f, 0f, -0.03f);
			orAddComponent.radius = 0.15f;
			orAddComponent.height = 0.375f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void cleanupEquipment(GameObject _rigObj, SDCSUtils.TransformCatalog _boneCatalog)
	{
		SDCSUtils.SlotAllowedBonesCache.Clear();
		RigBuilder component = _rigObj.GetComponent<RigBuilder>();
		if (component)
		{
			List<RigLayer> layers = component.layers;
			for (int i = layers.Count - 1; i >= 0; i--)
			{
				if (layers[i].name != "IKRig")
				{
					layers.RemoveAt(i);
				}
			}
			component.Clear();
		}
		Animator component2 = _rigObj.GetComponent<Animator>();
		if (component2)
		{
			AnimatorJobExtensions.UnbindAllStreamHandles(component2);
		}
		GameUtils.DestroyAllChildrenImmediatelyBut(_rigObj.transform, new List<string> { "Origin", "IKRig" });
		CharacterConstructUtils.SanitizeRig(_rigObj, _boneCatalog);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SanitizeRig(GameObject _rigObj, SDCSUtils.TransformCatalog _boneCatalog)
	{
		AuxBoneTracker auxBoneTracker = null;
		if (_boneCatalog != null && _boneCatalog["Origin"] != null)
		{
			auxBoneTracker = _boneCatalog["Origin"].GetComponent<AuxBoneTracker>();
		}
		if (auxBoneTracker)
		{
			foreach (string text in auxBoneTracker.AuxBoneLookup.Keys)
			{
				if (_boneCatalog.ContainsKey(text) && _boneCatalog[text] != null)
				{
					Object.DestroyImmediate(_boneCatalog[text].gameObject);
					_boneCatalog.Remove(text);
				}
			}
			auxBoneTracker.AuxBoneLookup.Clear();
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Transform> keyValuePair in _boneCatalog)
		{
			if (keyValuePair.Value == null)
			{
				list.Add(keyValuePair.Key);
			}
		}
		foreach (string text2 in list)
		{
			_boneCatalog.Remove(text2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupBase(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string[] baseParts)
	{
		foreach (string text in baseParts)
		{
			GameObject gameObject;
			if (text == "head")
			{
				gameObject = DataLoader.LoadAsset<GameObject>(CharacterConstructUtils.baseHeadLoc, false);
			}
			else if (text == "hands")
			{
				gameObject = DataLoader.LoadAsset<GameObject>(CharacterConstructUtils.baseBodyLoc, false);
			}
			else
			{
				gameObject = DataLoader.LoadAsset<GameObject>(CharacterConstructUtils.baseBodyLoc, false);
			}
			if (gameObject == null)
			{
				return;
			}
			GameObject bodyPartContainingName;
			if (!((bodyPartContainingName = CharacterConstructUtils.getBodyPartContainingName(gameObject.transform, text)) == null))
			{
				bodyPartContainingName.name = text;
				CharacterConstructUtils.Stitch(bodyPartContainingName, _rig, _boneCatalog, DataLoader.LoadAsset<Material>(CharacterConstructUtils.baseEyeColorMatLoc, false));
				if (text == "head")
				{
					Transform transform = _rig.transform;
					Transform transform2 = gameObject.transform;
					CharacterGazeController orAddComponent = transform.FindRecursive("Head").gameObject.GetOrAddComponent<CharacterGazeController>();
					orAddComponent.rootTransform = transform.FindRecursive("Origin");
					orAddComponent.neckTransform = _boneCatalog["Neck"];
					orAddComponent.headTransform = _boneCatalog["Head"];
					orAddComponent.leftEyeTransform = _boneCatalog["LeftEye"];
					orAddComponent.rightEyeTransform = _boneCatalog["RightEye"];
					orAddComponent.eyeSkinnedMeshRenderer = transform.FindRecursive("eyes").GetComponent<SkinnedMeshRenderer>();
					orAddComponent.leftEyeLocalPosition = transform2.FindInChildren("LeftEye").localPosition;
					orAddComponent.rightEyeLocalPosition = transform2.FindInChildren("RightEye").localPosition;
					orAddComponent.eyeLookAtTargetAngle = 35f;
					orAddComponent.eyeRotationSpeed = 30f;
					orAddComponent.twitchSpeed = 25f;
					orAddComponent.headLookAtTargetAngle = 75f;
					orAddComponent.headRotationSpeed = 7f;
					orAddComponent.maxLookAtDistance = 5f;
					EyeLidController orAddComponent2 = transform.FindRecursive("Head").gameObject.GetOrAddComponent<EyeLidController>();
					orAddComponent2.leftTopTransform = _boneCatalog["LeftEyelidTop"];
					orAddComponent2.leftBottomTransform = _boneCatalog["LeftEyelidBot"];
					orAddComponent2.rightTopTransform = _boneCatalog["RightEyelidTop"];
					orAddComponent2.rightBottomTransform = _boneCatalog["RightEyelidBot"];
					orAddComponent2.leftTopLocalPosition = transform2.FindInChildren("LeftEyelidTop").localPosition;
					orAddComponent2.leftBottomLocalPosition = transform2.FindInChildren("LeftEyelidBot").localPosition;
					orAddComponent2.leftTopRotation = transform2.FindInChildren("LeftEyelidTop").localRotation;
					orAddComponent2.leftBottomRotation = transform2.FindInChildren("LeftEyelidBot").localRotation;
					orAddComponent2.rightTopLocalPosition = transform2.FindInChildren("RightEyelidTop").localPosition;
					orAddComponent2.rightBottomLocalPosition = transform2.FindInChildren("RightEyelidBot").localPosition;
					orAddComponent2.rightTopRotation = transform2.FindInChildren("RightEyelidTop").localRotation;
					orAddComponent2.rightBottomRotation = transform2.FindInChildren("RightEyelidBot").localRotation;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupEquipment(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, List<SDCSUtils.SlotData> slotData, bool _ignoreDlcEntitlements)
	{
		if (slotData == null)
		{
			return;
		}
		List<Transform> list = new List<Transform>();
		Transform transform = _rig.transform.Find("Origin");
		if (transform)
		{
			List<Transform> list2 = CharacterConstructUtils.findStartsWith(transform, "RigConstraints");
			int count = list2.Count;
			for (int i = 0; i < count; i++)
			{
				Object.DestroyImmediate(list2[i].gameObject);
			}
		}
		if (CharacterConstructUtils.archetype.Equipment == null)
		{
			CharacterConstructUtils.archetype.Equipment = new List<SDCSUtils.SlotData>();
			foreach (SDCSUtils.SlotData slotData2 in slotData)
			{
				CharacterConstructUtils.archetype.Equipment.Add(slotData2);
			}
		}
		foreach (SDCSUtils.SlotData slotData3 in slotData)
		{
			GameObject gameObject = null;
			if ("head".Equals(slotData3.PartName, StringComparison.OrdinalIgnoreCase) && slotData3.PrefabName != null && slotData3.PrefabName.Contains("HeadGearMorphMatrix", StringComparison.OrdinalIgnoreCase))
			{
				gameObject = CharacterConstructUtils.setupHeadgearMorph(_rig, _boneCatalog, slotData3, _ignoreDlcEntitlements);
			}
			else
			{
				Transform transform2 = CharacterConstructUtils.setupEquipmentSlot(_rig, _boneCatalog, slotData3, list, _ignoreDlcEntitlements);
				if (transform2)
				{
					gameObject = CharacterConstructUtils.Stitch(transform2.gameObject, _rig, _boneCatalog, null);
					if (gameObject != null)
					{
						Morphable componentInChildren = gameObject.GetComponentInChildren<Morphable>();
						if (componentInChildren)
						{
							componentInChildren.MorphHeadgear(CharacterConstructUtils.archetype, _ignoreDlcEntitlements);
						}
					}
				}
			}
			if (gameObject != null)
			{
				ColorSwatchApplicator componentInChildren2 = gameObject.GetComponentInChildren<ColorSwatchApplicator>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.ApplyColorSwatch(CharacterConstructUtils.archetype.HairColor);
				}
			}
		}
		CharacterConstructUtils.setupEquipmentCommon(_rig, _boneCatalog, list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform setupEquipmentSlot(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, SDCSUtils.SlotData wornItem, List<Transform> allGears, bool _ignoreDlcEntitlements)
	{
		string text = CharacterConstructUtils.GetPathForSlotData(wornItem, true);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		GameObject gameObject = DataLoader.LoadAsset<GameObject>(text, _ignoreDlcEntitlements);
		if (gameObject == null && wornItem.PartName == "head")
		{
			text = CharacterConstructUtils.GetPathForSlotData(wornItem, false);
			gameObject = DataLoader.LoadAsset<GameObject>(text, _ignoreDlcEntitlements);
		}
		if (!gameObject)
		{
			Log.Warning(string.Concat(new string[] { "SDCSUtils::", text, " not found for item ", wornItem.PrefabName, "!" }));
			return null;
		}
		string text2 = "";
		for (int i = 0; i < CharacterConstructUtils.archetype.Equipment.Count; i++)
		{
			SDCSUtils.SlotData slotData = CharacterConstructUtils.archetype.Equipment[i];
			if (slotData.PartName.Equals("body", StringComparison.OrdinalIgnoreCase))
			{
				text2 = CharacterConstructUtils.GetPathForSlotData(slotData, false);
				break;
			}
		}
		Transform clothingPartWithName = CharacterConstructUtils.getClothingPartWithName(gameObject, CharacterConstructUtils.getPartNameWithVariant(wornItem.PartName, text, text2));
		if (clothingPartWithName)
		{
			HashSet<string> hashSet = CharacterConstructUtils.CollectRequiredNamesForSlot(gameObject.transform, clothingPartWithName);
			SDCSUtils.SlotAllowedBonesCache.Set(clothingPartWithName, hashSet);
			if (!allGears.Contains(clothingPartWithName))
			{
				allGears.Add(clothingPartWithName);
			}
			string baseToTurnOff = wornItem.BaseToTurnOff;
			if (baseToTurnOff != null && baseToTurnOff.Length > 0)
			{
				foreach (string text3 in wornItem.BaseToTurnOff.Split(',', StringSplitOptions.None))
				{
					Transform transform = _rig.transform.FindInChildren(text3);
					if (transform)
					{
						Object.Destroy(transform.gameObject);
					}
				}
			}
			if (!clothingPartWithName.gameObject.activeSelf)
			{
				clothingPartWithName.gameObject.SetActive(true);
			}
			CharacterConstructUtils.MatchRigs(gameObject.transform, _rig.transform, _boneCatalog, hashSet);
		}
		return clothingPartWithName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getPartNameWithVariant(string partName, string sourceGearPath, string targetBodyPath)
	{
		if (CharacterConstructUtils.archetype == null || CharacterConstructUtils.archetype.Equipment == null)
		{
			return partName;
		}
		string text = string.Empty;
		if (GearVariantMatrixSO.Instance != null)
		{
			text = GearVariantMatrixSO.Instance.GetVariantOrEmpty(CharacterConstructUtils.archetype.Sex, partName, sourceGearPath, targetBodyPath);
		}
		else
		{
			Log.Warning("SDCSUtils::getPartNameWithVariant: No GearVariantMatrixSO instance found!");
		}
		if (string.IsNullOrEmpty(text))
		{
			return partName;
		}
		return partName + "_" + text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupEquipmentCommon(GameObject _rigObj, SDCSUtils.TransformCatalog _boneCatalog, List<Transform> allGears)
	{
		RigBuilder orAddComponent = _rigObj.GetOrAddComponent<RigBuilder>();
		orAddComponent.enabled = false;
		foreach (Transform transform in allGears)
		{
			HashSet<string> hashSet;
			if (!SDCSUtils.SlotAllowedBonesCache.TryGet(transform, out hashSet) || hashSet == null || hashSet.Count == 0)
			{
				Debug.LogWarning("[SDCS] No required leaves cached for slot '" + transform.name + "'. Skipping constraints.");
			}
			else
			{
				CharacterConstructUtils.SetupRigConstraints(orAddComponent, transform, _rigObj.transform, _boneCatalog, hashSet);
			}
		}
		foreach (HingeJoint hingeJoint in _rigObj.GetComponentsInChildren<HingeJoint>())
		{
			if (hingeJoint.connectedBody != null && _boneCatalog.ContainsKey(hingeJoint.connectedBody.transform.name))
			{
				hingeJoint.connectedBody = _boneCatalog[hingeJoint.connectedBody.transform.name].GetComponent<Rigidbody>();
			}
			hingeJoint.autoConfigureConnectedAnchor = true;
		}
		orAddComponent.enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHairObjects(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string hairName, string mustacheName, string chopsName, string beardName)
	{
		HairColorSwatch hairColorSwatch = null;
		if (!string.IsNullOrEmpty(CharacterConstructUtils.archetype.HairColor))
		{
			ScriptableObject scriptableObject = DataLoader.LoadAsset<ScriptableObject>(CharacterConstructUtils.baseHairColorLoc + "/" + CharacterConstructUtils.archetype.HairColor + ".asset", false);
			if (!(scriptableObject == null))
			{
				hairColorSwatch = scriptableObject as HairColorSwatch;
			}
		}
		bool flag = false;
		bool flag2 = true;
		if (CharacterConstructUtils.archetype.Equipment != null && CharacterConstructUtils.archetype.Equipment.Count > 0)
		{
			foreach (SDCSUtils.SlotData slotData in CharacterConstructUtils.archetype.Equipment)
			{
				if (slotData.PartName == "head")
				{
					flag = slotData.HairMaskType == SDCSUtils.SlotData.HairMaskTypes.Hat;
					flag2 = slotData.FacialHairMaskType != SDCSUtils.SlotData.HairMaskTypes.None;
				}
			}
		}
		if (!string.IsNullOrEmpty(hairName))
		{
			if (flag)
			{
				CharacterConstructUtils.setupHair(_rig, _boneCatalog, CharacterConstructUtils.baseHairLoc + "/hair_" + hairName + "_hat.asset", hairName);
			}
			else
			{
				CharacterConstructUtils.setupHair(_rig, _boneCatalog, CharacterConstructUtils.baseHairLoc + "/hair_" + hairName + ".asset", hairName);
			}
		}
		if (flag2)
		{
			if (!string.IsNullOrEmpty(mustacheName))
			{
				CharacterConstructUtils.setupHair(_rig, _boneCatalog, CharacterConstructUtils.baseMustacheLoc + "/hair_facial_mustache" + mustacheName + ".asset", mustacheName);
			}
			if (!string.IsNullOrEmpty(chopsName))
			{
				CharacterConstructUtils.setupHair(_rig, _boneCatalog, CharacterConstructUtils.baseChopsLoc + "/hair_facial_sideburns" + chopsName + ".asset", chopsName);
			}
			if (!string.IsNullOrEmpty(beardName))
			{
				CharacterConstructUtils.setupHair(_rig, _boneCatalog, CharacterConstructUtils.baseBeardLoc + "/hair_facial_beard" + beardName + ".asset", beardName);
			}
		}
		if (hairColorSwatch != null)
		{
			CharacterConstructUtils.ApplySwatchToGameObject(_rig, hairColorSwatch);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplySwatchToGameObject(GameObject targetGameObject, HairColorSwatch hairSwatch)
	{
		if (targetGameObject != null)
		{
			foreach (Renderer renderer in targetGameObject.GetComponentsInChildren<Renderer>(true))
			{
				Material[] array;
				if (Application.isPlaying)
				{
					array = renderer.materials;
				}
				else
				{
					array = renderer.sharedMaterials;
				}
				foreach (Material material in array)
				{
					if (material.shader.name == "Game/SDCS/Hair" && !material.name.Contains("lashes"))
					{
						hairSwatch.ApplyToMaterial(material);
					}
				}
			}
			return;
		}
		Debug.LogWarning("No target GameObject selected.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHair(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string path, string hairName)
	{
		if (string.IsNullOrEmpty(hairName) || hairName == "-1" || hairName == "none")
		{
			return;
		}
		MeshMorph meshMorph = DataLoader.LoadAsset<MeshMorph>(path, false);
		GameObject gameObject = ((meshMorph != null) ? meshMorph.GetMorphedSkinnedMesh() : null);
		if (gameObject == null)
		{
			Log.Warning(string.Concat(new string[] { "SDCSUtils::", path, " not found for hair ", hairName, "!" }));
			return;
		}
		if (!gameObject.gameObject.activeSelf)
		{
			gameObject.gameObject.SetActive(true);
		}
		CharacterConstructUtils.Stitch(gameObject.gameObject, _rig, _boneCatalog, null);
		Object.Destroy(gameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetPathForSlotData(SDCSUtils.SlotData _slotData, bool _headgearShortHairMask = false)
	{
		if (_slotData == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(_slotData.PartName))
		{
			return null;
		}
		if (_slotData.PartName.Equals("head", StringComparison.OrdinalIgnoreCase))
		{
			string text = _slotData.PrefabName;
			if (text.Contains("{sex}"))
			{
				text = text.Replace("{sex}", CharacterConstructUtils.archetype.Sex);
			}
			if (text.Contains("{race}"))
			{
				text = text.Replace("{race}", CharacterConstructUtils.archetype.Race);
			}
			if (text.Contains("{variant}"))
			{
				text = text.Replace("{variant}", CharacterConstructUtils.archetype.Variant.ToString("00"));
			}
			if (text.Contains("{hair}"))
			{
				if (_headgearShortHairMask && (string.IsNullOrEmpty(CharacterConstructUtils.archetype.Hair) || CharacterConstructUtils.shortHairNames.ContainsCaseInsensitive(CharacterConstructUtils.archetype.Hair)))
				{
					text = text.Replace("{hair}", "Bald");
				}
				else
				{
					text = text.Replace("{hair}", "");
				}
			}
			return text;
		}
		if (string.IsNullOrEmpty(_slotData.PrefabName))
		{
			return null;
		}
		return CharacterConstructUtils.parseSexedLocation(_slotData.PrefabName, CharacterConstructUtils.archetype.Sex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject setupHeadgearMorph(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, SDCSUtils.SlotData _slotData, bool _ignoreDlcEntitlements)
	{
		string text = CharacterConstructUtils.GetPathForSlotData(_slotData, true);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		MeshMorph meshMorph = DataLoader.LoadAsset<MeshMorph>(text, _ignoreDlcEntitlements);
		if (meshMorph == null)
		{
			text = CharacterConstructUtils.GetPathForSlotData(_slotData, false);
			meshMorph = DataLoader.LoadAsset<MeshMorph>(text, _ignoreDlcEntitlements);
		}
		GameObject gameObject = ((meshMorph != null) ? meshMorph.GetMorphedSkinnedMesh() : null);
		if (gameObject == null)
		{
			Log.Warning(string.Concat(new string[] { "SDCSUtils::", text, " not found for headgear ", _slotData.PrefabName, "!" }));
			return null;
		}
		if (!gameObject.gameObject.activeSelf)
		{
			gameObject.gameObject.SetActive(true);
		}
		DataLoader.LoadAsset<Material>(CharacterConstructUtils.baseBodyMatLoc, false);
		GameObject gameObject2 = CharacterConstructUtils.Stitch(gameObject.gameObject, _rig, _boneCatalog, null);
		Object.Destroy(gameObject);
		return gameObject2;
	}

	public static bool BasePartsExist(Archetype _archetype)
	{
		CharacterConstructUtils.archetype = _archetype;
		if (!DataLoader.LoadAsset<GameObject>(CharacterConstructUtils.baseBodyLoc, false))
		{
			Log.Error("base body not found at " + CharacterConstructUtils.baseBodyLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<GameObject>(CharacterConstructUtils.baseHeadLoc, false))
		{
			Log.Error("base head not found at " + CharacterConstructUtils.baseHeadLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<Material>(CharacterConstructUtils.baseBodyMatLoc, false))
		{
			Log.Error("body material not found at " + CharacterConstructUtils.baseBodyMatLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<Material>(CharacterConstructUtils.baseHeadMatLoc, false))
		{
			Log.Error("head material not found at " + CharacterConstructUtils.baseHeadMatLoc);
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Transform> findStartsWith(Transform parent, string key)
	{
		List<Transform> list = new List<Transform>();
		foreach (object obj in parent)
		{
			Transform transform = (Transform)obj;
			if (transform.name.StartsWith(key))
			{
				list.Add(transform);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject getBodyPartContainingName(Transform parent, string name)
	{
		foreach (object obj in parent.transform)
		{
			Transform transform = (Transform)obj;
			if (transform.name.ToLower().Contains(name))
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform getClothingPartWithName(GameObject clothingPrefab, string partName)
	{
		foreach (object obj in clothingPrefab.transform)
		{
			Transform transform = (Transform)obj;
			if (transform.name.ToLower() == partName.ToLower())
			{
				return transform;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string parseSexedLocation(string sexedLocation, string sex)
	{
		return sexedLocation.Replace("{sex}", sex);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <MapSourceByName>g__Recurse|9_0(Transform t, ref CharacterConstructUtils.<>c__DisplayClass9_0 A_1)
	{
		A_1.map[t.name] = t;
		for (int i = 0; i < t.childCount; i++)
		{
			CharacterConstructUtils.<MapSourceByName>g__Recurse|9_0(t.GetChild(i), ref A_1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Cloth> tempCloths = new List<Cloth>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Material> tempMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SkinnedMeshRenderer> tempSMRs = new List<SkinnedMeshRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] shortHairNames = new string[] { "buzzcut", "cornrows", "flattop_fro", "mohawk", "small_fro" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SkinnedMeshRenderer> _smrBuf = new List<SkinnedMeshRenderer>(64);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<HingeJoint> _hingeBuf = new List<HingeJoint>(32);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<BlendConstraint> _bcBuf = new List<BlendConstraint>(32);

	[PublicizedFrom(EAccessModifier.Private)]
	public const string ORIGIN = "Origin";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string RIGCON = "RigConstraints";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string IKRIG = "IKRig";

	public const string HEAD = "head";

	public const string EYES = "eyes";

	public const string BEARD = "beard";

	public const string HAIR = "hair";

	public const string BODY = "body";

	public const string HANDS = "hands";

	public const string FEET = "feet";

	public const string HELMET = "helmet";

	public const string TORSO = "torso";

	public const string GLOVES = "gloves";

	public const string BOOTS = "boots";

	public const string SEX_MARKER = "{sex}";

	public const string RACE_MARKER = "{race}";

	public const string VARIANT_MARKER = "{variant}";

	public const string HAIR_MARKER = "{hair}";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Archetype archetype;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] baseParts = new string[] { "head", "body", "hands", "feet" };
}

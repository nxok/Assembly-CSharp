using System;
using System.Collections.Generic;
using UnityEngine;

public static class ProjectileManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static void init()
	{
		ProjectileManager.nextId = 0;
		ProjectileManager.projectiles = new DictionaryKeyValueList<int, Transform>();
		ProjectileManager.keysToRemove = new List<int>();
		ProjectileManager.blockHitParent = new GameObject("WorldProjectileContainer").transform;
		Origin.Add(ProjectileManager.blockHitParent, 0);
	}

	public static void Update()
	{
		if (ProjectileManager.projectiles == null)
		{
			ProjectileManager.init();
		}
		for (int i = 0; i < ProjectileManager.projectiles.keyList.Count; i++)
		{
			int num = ProjectileManager.projectiles.keyList[i];
			Transform transform = ProjectileManager.projectiles.dict[num];
			if (transform == null)
			{
				ProjectileManager.keysToRemove.Add(num);
			}
			else if (transform.GetComponent<ProjectileMoveScript>() != null && !GameManager.Instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(transform.position.x + Origin.position.x), Mathf.CeilToInt(transform.position.y + Origin.position.y), Mathf.CeilToInt(transform.position.z + Origin.position.z)))
			{
				ProjectileManager.keysToRemove.Add(num);
			}
		}
		for (int j = 0; j < ProjectileManager.keysToRemove.Count; j++)
		{
			ProjectileManager.RemoveProjectile(ProjectileManager.keysToRemove[j], false);
		}
		ProjectileManager.keysToRemove.Clear();
	}

	public static void Cleanup()
	{
		ProjectileManager.keysToRemove = new List<int>();
		if (ProjectileManager.projectiles == null)
		{
			return;
		}
		for (int i = 0; i < ProjectileManager.projectiles.keyList.Count; i++)
		{
			ProjectileManager.keysToRemove.Add(ProjectileManager.projectiles.keyList[i]);
		}
		for (int j = 0; j < ProjectileManager.keysToRemove.Count; j++)
		{
			ProjectileManager.RemoveProjectile(ProjectileManager.keysToRemove[j], false);
		}
	}

	public static int AddProjectileItem(Transform _transform = null, int _pId = -1, Vector3 _position = default(Vector3), Vector3 _movementLastFrame = default(Vector3), int _itemValueType = -1)
	{
		if (ProjectileManager.projectiles == null)
		{
			ProjectileManager.init();
		}
		ProjectileManager.Update();
		if (_pId == -1)
		{
			_pId = ProjectileManager.nextId;
			ProjectileManager.nextId++;
		}
		if (_transform == null && _itemValueType != -1)
		{
			_transform = ProjectileManager.instantiateProjectile(_pId, _itemValueType);
		}
		float num = ItemClass.GetForId(_itemValueType).StickyOffset;
		Renderer renderer = _transform.GetComponent<Renderer>();
		if (renderer == null)
		{
			LODGroup component = _transform.GetComponent<LODGroup>();
			if (component != null)
			{
				LOD[] lods = component.GetLODs();
				for (int i = 0; i < component.lodCount; i++)
				{
					Renderer[] renderers = lods[i].renderers;
					if (i == 0)
					{
						renderer = renderers[i];
					}
					for (int j = 0; j < renderers.Length; j++)
					{
						renderers[j].material = DataLoader.LoadAsset<Material>(ItemClass.GetForId(_itemValueType).StickyMaterial, false);
					}
				}
			}
			else
			{
				renderer = _transform.GetComponentInChildren<Renderer>();
			}
		}
		else if (ItemClass.GetForId(_itemValueType).StickyMaterial != null)
		{
			renderer.material = DataLoader.LoadAsset<Material>(ItemClass.GetForId(_itemValueType).StickyMaterial, false);
		}
		Mesh mesh = _transform.GetComponent<Mesh>();
		MeshFilter meshFilter = _transform.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = _transform.GetComponentInChildren<MeshFilter>();
		}
		if (mesh == null)
		{
			SkinnedMeshRenderer componentInChildren = _transform.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren != null)
			{
				mesh = componentInChildren.sharedMesh;
				renderer = componentInChildren;
			}
		}
		if (mesh == null && meshFilter != null)
		{
			mesh = meshFilter.mesh;
		}
		if (renderer != null || (meshFilter != null && meshFilter.mesh != null))
		{
			Bounds bounds = renderer.bounds;
			if (meshFilter != null && meshFilter.mesh != null)
			{
				bounds = meshFilter.mesh.bounds;
			}
			if (mesh != null)
			{
				bounds = mesh.bounds;
			}
			if (!Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
			{
				CapsuleCollider capsuleCollider = _transform.gameObject.AddComponent<CapsuleCollider>();
				capsuleCollider.center = bounds.center;
				float num2 = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
				if (ItemClass.GetForId(_itemValueType).StickyColliderUp == -1)
				{
					if (num2 == bounds.size.x)
					{
						capsuleCollider.direction = 2;
					}
					else if (num2 == bounds.size.y)
					{
						capsuleCollider.direction = 1;
					}
					else if (num2 == bounds.size.z)
					{
						capsuleCollider.direction = 2;
					}
				}
				else
				{
					capsuleCollider.direction = ItemClass.GetForId(_itemValueType).StickyColliderUp;
				}
				capsuleCollider.height = ((ItemClass.GetForId(_itemValueType).StickyColliderLength > 0f) ? ItemClass.GetForId(_itemValueType).StickyColliderLength : num2);
				capsuleCollider.radius = ((ItemClass.GetForId(_itemValueType).StickyColliderRadius > 0f) ? ItemClass.GetForId(_itemValueType).StickyColliderRadius : 0.05f);
				if (num == 0f)
				{
					num = capsuleCollider.height / 2f;
				}
			}
		}
		if (Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
		{
			_transform.parent = Voxel.voxelRayHitInfo.transform;
		}
		else
		{
			_transform.parent = ProjectileManager.blockHitParent;
		}
		_transform.localScale = Vector3.one * 0.75f;
		ProjectileMoveScript component2;
		ThrownWeaponMoveScript component3;
		if ((component2 = _transform.GetComponent<ProjectileMoveScript>()) != null)
		{
			component2.ProjectileID = _pId;
			_transform.name = string.Format("temp_Projectile[item:{0},owner:{1},projectileId:{2}", component2.itemProjectile.GetLocalizedItemName(), GameManager.Instance.World.GetEntity(component2.ProjectileOwnerID).name, component2.ProjectileID.ToString());
			component2.FinalPosition = _position - _transform.forward * num;
			_transform.position = component2.FinalPosition - Origin.position;
		}
		else if ((component3 = _transform.GetComponent<ThrownWeaponMoveScript>()) != null)
		{
			component3.ProjectileID = _pId;
			_transform.name = string.Format("temp_Projectile[item:{0},owner:{1},projectileId:{2}", component3.itemWeapon.GetLocalizedItemName(), GameManager.Instance.World.GetEntity(component3.ProjectileOwnerID).name, component3.ProjectileID.ToString());
			component3.FinalPosition = _position - component3.velocity.normalized * num;
			_transform.position = component3.FinalPosition - Origin.position;
		}
		Transform transform = _transform.Find("Trail");
		if (transform != null)
		{
			transform.gameObject.SetActive(false);
		}
		_transform.tag = "Item";
		_transform.gameObject.layer = 13;
		ProjectileManager.projectiles.Add(_pId, _transform);
		return _pId;
	}

	public static bool RemoveProjectile(int _id, bool revive = false)
	{
		if (ProjectileManager.projectiles == null)
		{
			ProjectileManager.init();
			return false;
		}
		Transform transform;
		if (!ProjectileManager.projectiles.dict.TryGetValue(_id, out transform))
		{
			return false;
		}
		ProjectileManager.projectiles.Remove(_id);
		if (!transform)
		{
			return false;
		}
		if (!revive)
		{
			Object.Destroy(transform.gameObject);
			return true;
		}
		transform.gameObject.layer = 0;
		transform.tag = "Untagged";
		Transform transform2 = transform.Find("Trail");
		if (transform2 != null)
		{
			transform2.gameObject.SetActive(true);
		}
		ProjectileMoveScript projectileMoveScript;
		if (transform.TryGetComponent<ProjectileMoveScript>(ref projectileMoveScript))
		{
			projectileMoveScript.ProjectileID = -1;
			projectileMoveScript.FinalPosition = Vector3.zero;
		}
		ProjectileMoveScript projectileMoveScript2;
		if (transform.TryGetComponent<ProjectileMoveScript>(ref projectileMoveScript2))
		{
			projectileMoveScript2.ProjectileID = -1;
			projectileMoveScript2.FinalPosition = Vector3.zero;
		}
		transform.localScale = Vector3.one;
		transform.SetParent(null);
		CapsuleCollider capsuleCollider;
		if (transform.gameObject.TryGetComponent<CapsuleCollider>(ref capsuleCollider))
		{
			Object.Destroy(capsuleCollider);
		}
		return true;
	}

	public static Transform GetProjectile(int _id)
	{
		Transform transform = null;
		if (ProjectileManager.projectiles.keyList.Contains(_id) && ProjectileManager.projectiles.dict[_id] != null)
		{
			transform = ProjectileManager.projectiles.dict[_id];
		}
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform instantiateProjectile(int _id, int _itemValueType)
	{
		ItemClass forId = ItemClass.GetForId(_itemValueType);
		if (forId == null)
		{
			return null;
		}
		ItemValue itemValue = new ItemValue(forId.Id, false);
		Transform transform = forId.CloneModel(GameManager.Instance.World, itemValue, Vector3.zero, null, BlockShape.MeshPurpose.World, default(TextureFullArray));
		Utils.SetLayerRecursively(transform.gameObject, 13);
		transform.gameObject.SetActive(true);
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int nextId;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryKeyValueList<int, Transform> projectiles;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> keysToRemove;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform blockHitParent;
}

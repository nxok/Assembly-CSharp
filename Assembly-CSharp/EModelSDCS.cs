using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelSDCS : EModelPlayer
{
	public bool isMale
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.archetype == null || this.archetype.Sex == "Male";
		}
	}

	public override Transform NeckTransform
	{
		get
		{
			if (!base.IsFPV)
			{
				return this.boneCatalog["Neck"];
			}
			return this.boneCatalogFP["Neck"];
		}
	}

	public void Awake()
	{
		this.playerEntity = base.transform.GetComponent<EntityPlayerLocal>();
		if (this.playerEntity == null)
		{
			this.playerEntity = base.transform.GetComponent<EntityPlayer>();
		}
	}

	public override void Init(World _world, Entity _entity, EModelInstanceAssets _assets)
	{
		this.entity = _entity;
		this.assets = _assets;
		this.entityClass = EntityClass.list[this.entity.entityClass];
		this.archetype = this.playerEntity.playerProfile.CreateTempArchetype();
		this.ragdollChance = this.entityClass.RagdollOnDeathChance;
		this.bHasRagdoll = this.entityClass.HasRagdoll;
		this.modelTransformParent = EModelBase.FindModel(base.transform);
		base.IsFPV = this.entity is EntityPlayerLocal;
		this.createModel(_world, this.entityClass);
		this.createAvatarController(EntityClass.list[this.entity.entityClass]);
		XUiM_PlayerEquipment.HandleRefreshEquipment += this.XUiM_PlayerEquipment_HandleRefreshEquipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void OnDestroy()
	{
		XUiM_PlayerEquipment.HandleRefreshEquipment -= this.XUiM_PlayerEquipment_HandleRefreshEquipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment playerEquipment)
	{
		if (playerEquipment.Equipment == this.playerEntity.equipment)
		{
			this.UpdateEquipment();
		}
	}

	public void UpdateEquipment()
	{
		Animator animator = this.avatarController.GetAnimator();
		if (animator && (base.IsFPV || animator.enabled))
		{
			this.GenerateMeshes();
		}
	}

	public Transform HeadTransformFP
	{
		get
		{
			if (this.headTransFP == null && this.boneCatalogFP.ContainsKey("Head"))
			{
				this.headTransFP = this.boneCatalogFP["Head"];
			}
			return this.headTransFP;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		base.LateUpdate();
		if (base.IsFPV && this.HeadTransformFP != null)
		{
			foreach (Material material in this.ClipMaterialsFP)
			{
				material.SetVector("_ClipCenter", this.HeadTransformFP.position);
			}
		}
	}

	public override void SwitchModelAndView(bool _bFPV, bool _isMale)
	{
		base.IsFPV = _bFPV;
		this.playerEntity.IsMale = this.isMale;
		this.GenerateMeshes();
		base.SwitchModelAndView(base.IsFPV, _isMale);
		this.meshTransform = this.modelTransform.FindInChildren("Spine1");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createModel(World _world, EntityClass _ec)
	{
		if (this.modelTransformParent == null)
		{
			return;
		}
		if (this.playerModelTransform != null)
		{
			Object.Destroy(this.playerModelTransform.gameObject);
		}
		this.playerModelTransform = this.GenerateMeshes();
		this.playerModelTransform.name = (this.modelName = "player_" + this.archetype.Sex + "Ragdoll");
		this.playerModelTransform.tag = "E_BP_Body";
		this.playerModelTransform.SetParent(this.modelTransformParent, false);
		this.playerModelTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		this.playerModelTransform.gameObject.GetOrAddComponent<AnimationEventBridge>();
		this.updateLightScript = this.playerModelTransform.gameObject.GetOrAddComponent<UpdateLightOnPlayers>();
		this.updateLightScript.IsDynamicObject = true;
		EntityAlive entityAlive = this.entity as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.ReassignEquipmentTransforms();
		}
		this.baseRig.transform.FindInChilds("Origin", false).tag = "E_BP_BipedRoot";
	}

	public Transform GenerateMeshes()
	{
		SDCSUtils.CreateVizTP(this.archetype, ref this.baseRig, ref this.boneCatalog, this.playerEntity, base.IsFPV);
		if (this.playerEntity as EntityPlayerLocal != null)
		{
			SDCSUtils.CreateVizFP(this.archetype, ref this.baseRigFP, ref this.boneCatalogFP, this.playerEntity, base.IsFPV);
		}
		base.ClothSimInit();
		return this.baseRig.transform;
	}

	public override Transform GetHeadTransform()
	{
		if (this.headT == null && this.boneCatalog.ContainsKey("Head"))
		{
			this.headT = this.boneCatalog["Head"];
		}
		return this.headT;
	}

	public override Transform GetPelvisTransform()
	{
		if (this.bipedPelvisTransform == null && this.boneCatalog.ContainsKey("Hips"))
		{
			this.bipedPelvisTransform = this.boneCatalog["Hips"];
		}
		return this.bipedPelvisTransform;
	}

	public Archetype Archetype
	{
		get
		{
			return this.archetype;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetRace(string value)
	{
		this.archetype.Race = value;
		if (SDCSUtils.BasePartsExist(this.archetype))
		{
			this.SwitchModelAndView(base.IsFPV, this.isMale);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetVariant(int value)
	{
		this.archetype.Variant = (int)((byte)value);
		if (SDCSUtils.BasePartsExist(this.archetype))
		{
			this.SwitchModelAndView(base.IsFPV, this.isMale);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetSex(bool value)
	{
		this.archetype.IsMale = value;
		if (SDCSUtils.BasePartsExist(this.archetype))
		{
			this.SwitchModelAndView(base.IsFPV, this.isMale);
		}
	}

	public override void SetVisible(bool _bVisible, bool _isKeepColliders = false)
	{
		bool visible = base.visible;
		if (_bVisible != visible)
		{
			SDCSUtils.SetVisible(this.baseRig, _bVisible);
		}
		base.SetVisible(_bVisible, _isKeepColliders);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityClass entityClass;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayer playerEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject baseRig;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SDCSUtils.TransformCatalog boneCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject baseRigFP;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SDCSUtils.TransformCatalog boneCatalogFP;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform playerModelTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform headT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform headTransFP;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UpdateLightOnPlayers updateLightScript;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Archetype archetype;

	public SDCSUtils.SlotData.HairMaskTypes HairMaskType;

	public SDCSUtils.SlotData.HairMaskTypes FacialHairMaskType;

	public List<Material> ClipMaterialsFP = new List<Material>();
}

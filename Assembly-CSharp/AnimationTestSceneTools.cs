using System;
using UnityEngine;

public class AnimationTestSceneTools : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.anim = base.GetComponent<Animator>();
		this.weaponPrefabCount = this.weaponPrefabs.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyUp(306))
		{
			if (this.isCrouching)
			{
				this.isCrouching = false;
			}
			else
			{
				this.isCrouching = true;
			}
			this.anim.SetBool("IsCrouching", this.isCrouching);
		}
		if (Input.GetKeyUp(32))
		{
			this.locomotionState++;
			if (this.locomotionState >= this.locomotionSpeeds.Length)
			{
				this.locomotionState = 0;
			}
			this.forwardGoal = this.locomotionSpeeds[this.locomotionState];
		}
		if (Input.GetKey(97))
		{
			base.transform.Rotate(0f, this.turnRate * Time.deltaTime * -1f, 0f);
		}
		if (Input.GetKey(100))
		{
			base.transform.Rotate(0f, this.turnRate * Time.deltaTime, 0f);
		}
		if (Input.GetKeyUp(119))
		{
			this.weaponPrefabIndex++;
			this.anim.SetTrigger("ItemHasChangedTrigger");
			if (this.weaponPrefabIndex >= this.weaponPrefabCount)
			{
				this.weaponPrefabIndex = 0;
			}
			this.attachWeapon(this.weaponPrefabIndex);
		}
		if (Input.GetKeyUp(115))
		{
			this.weaponPrefabIndex--;
			this.anim.SetTrigger("ItemHasChangedTrigger");
			if (this.weaponPrefabIndex < 0)
			{
				this.weaponPrefabIndex = this.weaponPrefabCount;
			}
			this.attachWeapon(this.weaponPrefabIndex);
		}
		if (Input.GetKeyUp(114))
		{
			this.anim.SetTrigger("Reload");
		}
		if (Input.GetMouseButtonDown(0))
		{
			this.anim.SetTrigger("WeaponFire");
		}
		if (Input.GetMouseButtonUp(0))
		{
			this.anim.ResetTrigger("WeaponFire");
		}
		if (Input.GetMouseButtonDown(1))
		{
			this.anim.SetTrigger("IsAiming");
		}
		if (Input.GetMouseButtonUp(1))
		{
			this.anim.ResetTrigger("IsAiming");
		}
		if (Input.GetKeyUp(113))
		{
			this.anim.SetTrigger("PowerAttack");
		}
		if (Input.GetKeyUp(101))
		{
			this.anim.SetTrigger("UseItem");
		}
		this.updateYLook();
		this.forward = Mathf.Lerp(this.forward, this.forwardGoal, 0.01f);
		this.anim.SetFloat("Forward", this.forward);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void attachWeapon(int weaponPrefabIndex)
	{
		this.removeAllWeapons();
		if (this.weaponPrefabs[weaponPrefabIndex] != null)
		{
			this.newWeapon = Object.Instantiate<GameObject>(this.weaponPrefabs[weaponPrefabIndex]);
			this.newWeapon.transform.parent = this.weaponJoint.transform;
			this.newWeapon.transform.localPosition = Vector3.zero;
			this.newWeapon.transform.localEulerAngles = Vector3.zero;
		}
		Debug.Log(weaponPrefabIndex);
		this.anim.SetInteger("WeaponHoldType", this.weaponHoldTypes[weaponPrefabIndex]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeAllWeapons()
	{
		this.weaponJointChildrenCount = this.weaponJoint.childCount;
		if (this.weaponJointChildrenCount > 0)
		{
			for (int i = 0; i < this.weaponJointChildrenCount; i++)
			{
				this.existingChild = this.weaponJoint.GetChild(i).gameObject;
				Object.Destroy(this.existingChild);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateYLook()
	{
		Vector3 mousePosition = Input.mousePosition;
		this.mousePosXRatio = mousePosition.x / (float)Screen.width;
		this.mousePosYRatio = mousePosition.y / (float)Screen.height;
		this.mousePosX = (this.mousePosXRatio - 0.5f) * 2f;
		this.mousePosY = (this.mousePosYRatio - 0.5f) * -2f;
		this.anim.SetFloat("YLook", this.mousePosY);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator anim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int layerIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float oneHandMeleeTargetWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float oneHandPistolTargetWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int maxLayers;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float layerWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float turnRate = 200f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int totalModels;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentModel = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int weaponPrefabIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int weaponJointChildrenCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currOneHandMeleeWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currOneHandPistolWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCrouching;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int weaponPrefabCount;

	public GameObject[] weaponPrefabs;

	public int[] weaponHoldTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float[] locomotionSpeeds = new float[] { 0f, 2.08f, 4.2f };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int locomotionState;

	public Transform weaponJoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject existingChild;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject newWeapon;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mousePosXRatio;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mousePosYRatio;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mousePosX;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mousePosY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float forward;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float forwardGoal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float strafe;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float YLook;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float horizontalMax = 4.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float verticalMax = 4.2f;
}

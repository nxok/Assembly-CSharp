using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKController : MonoBehaviour
{
	public unsafe void Start()
	{
		this.animator = base.GetComponent<Animator>();
		Transform transform = base.transform.Find("IKRig");
		if (transform)
		{
			this.rig = transform.GetComponent<Rig>();
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				TwoBoneIKConstraint component = transform.GetChild(i).GetComponent<TwoBoneIKConstraint>();
				if (component)
				{
					TwoBoneIKConstraintData twoBoneIKConstraintData = *component.data;
					Transform target = twoBoneIKConstraintData.target;
					if (target)
					{
						int num = this.NameToIndex(target.name);
						if (num >= 0)
						{
							IKController.Constraint constraint;
							constraint.tbConstraint = component;
							constraint.originalWeight = component.weight;
							constraint.originalTargetT = target;
							this.rigConstraints[num] = constraint;
						}
					}
				}
			}
			this.ModifyRig();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int NameToIndex(string name)
	{
		for (int i = 0; i < IKController.IKNames.Length; i++)
		{
			if (name == IKController.IKNames[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void SetTargets(List<IKController.Target> _targets)
	{
		this.targets = _targets;
	}

	public void Cleanup()
	{
		this.targets = null;
		if (this.rig)
		{
			this.ModifyRig();
		}
	}

	public void ModifyRig()
	{
		if (this.targets == null)
		{
			for (int i = 0; i < 4; i++)
			{
				IKController.Constraint constraint = this.rigConstraints[i];
				if (constraint.originalTargetT)
				{
					TwoBoneIKConstraint tbConstraint = constraint.tbConstraint;
					tbConstraint.weight = constraint.originalWeight;
					tbConstraint.data.target = constraint.originalTargetT;
					Transform transform = tbConstraint.transform;
					transform.position = Vector3.zero;
					transform.rotation = Quaternion.identity;
				}
			}
		}
		else
		{
			Transform transform2 = base.transform;
			for (int j = 0; j < this.targets.Count; j++)
			{
				IKController.Target target = this.targets[j];
				int avatarGoal = target.avatarGoal;
				TwoBoneIKConstraint tbConstraint2 = this.rigConstraints[avatarGoal].tbConstraint;
				if (tbConstraint2)
				{
					Transform transform3 = target.transform;
					if (!transform3)
					{
						transform3 = tbConstraint2.transform;
						Matrix4x4 localToWorldMatrix = transform2.localToWorldMatrix;
						Vector3 vector = localToWorldMatrix.MultiplyPoint(target.position);
						transform3.position = vector;
						Quaternion quaternion = localToWorldMatrix.rotation * Quaternion.Euler(target.rotation);
						transform3.rotation = quaternion;
					}
					tbConstraint2.weight = 1f;
					tbConstraint2.data.target = transform3;
				}
			}
		}
		base.GetComponent<RigBuilder>().Build();
	}

	public void OnAnimatorIK()
	{
		if (!this.animator)
		{
			return;
		}
		if (this.targets == null)
		{
			for (int i = 0; i < 4; i++)
			{
				this.animator.SetIKPositionWeight(i, 0f);
				this.animator.SetIKRotationWeight(i, 0f);
			}
			return;
		}
		Transform transform = base.transform;
		for (int j = 0; j < this.targets.Count; j++)
		{
			IKController.Target target = this.targets[j];
			this.animator.SetIKPositionWeight(target.avatarGoal, 1f);
			this.animator.SetIKRotationWeight(target.avatarGoal, 1f);
			Transform transform2 = target.transform;
			if (!transform2)
			{
				Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
				Vector3 vector = localToWorldMatrix.MultiplyPoint(target.position);
				this.animator.SetIKPosition(target.avatarGoal, vector);
				Quaternion quaternion = localToWorldMatrix.rotation * Quaternion.Euler(target.rotation);
				this.animator.SetIKRotation(target.avatarGoal, quaternion);
			}
			else
			{
				this.animator.SetIKPosition(target.avatarGoal, transform2.position);
				this.animator.SetIKRotation(target.avatarGoal, transform2.rotation);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cTargetTypeCount = 4;

	public static string[] IKNames = new string[] { "IKFootL", "IKFootR", "IKHandL", "IKHandR" };

	public List<IKController.Target> targets;

	public IKController.Constraint[] rigConstraints = new IKController.Constraint[4];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator animator;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rig rig;

	[Serializable]
	public struct Target
	{
		public AvatarIKGoal avatarGoal;

		public Transform transform;

		public Vector3 position;

		public Vector3 rotation;
	}

	[Serializable]
	public struct Constraint
	{
		public TwoBoneIKConstraint tbConstraint;

		public float originalWeight;

		public Transform originalTargetT;
	}
}

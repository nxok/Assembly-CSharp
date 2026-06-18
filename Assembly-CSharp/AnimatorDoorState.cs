using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AnimatorDoorState : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.matchDoorState(animator, stateInfo))
		{
			this.colliders = animator.gameObject.GetComponentsInChildren<Collider>(true);
			this.rules = new EntityCollisionRules[this.colliders.Length];
			for (int i = this.colliders.Length - 1; i >= 0; i--)
			{
				Collider collider = this.colliders[i];
				this.rules[i] = collider.GetComponent<EntityCollisionRules>();
				if (!this.isOpenAnim)
				{
					EntityCollisionRules entityCollisionRules = this.rules[i];
					if (entityCollisionRules && entityCollisionRules.IsAnimPush)
					{
						collider.enabled = false;
					}
				}
			}
			this.EnableColliders(true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool matchDoorState(Animator animator, AnimatorStateInfo stateInfo)
	{
		bool @bool = animator.GetBool(AnimatorDoorState.IsOpenHash);
		this.isOpenAnim = stateInfo.shortNameHash == AnimatorDoorState.OpenHash;
		return @bool == this.isOpenAnim;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.matchDoorState(animator, stateInfo))
		{
			if (this.stopNext)
			{
				this.stopNext = false;
				this.EnableColliders(true);
				if (!this.keepAnimatorEnabled)
				{
					animator.enabled = false;
					return;
				}
			}
			else
			{
				if (animator.IsInTransition(layerIndex))
				{
					return;
				}
				float normalizedTime = stateInfo.normalizedTime;
				if (normalizedTime >= this.collideOnPercent)
				{
					if (!this.collidersEnabled)
					{
						if (this.CheckForObstacles())
						{
							return;
						}
						this.EnableColliders(true);
					}
					else if (this.disableColliderOnObstacleDetection && this.CheckForObstacles())
					{
						this.EnableColliders(false);
					}
					if (normalizedTime >= 1f)
					{
						this.stopNext = true;
						return;
					}
				}
				else if (normalizedTime < 1f && normalizedTime >= this.collideOffPercent)
				{
					if (this.collidersEnabled)
					{
						this.EnableColliders(false);
					}
					if (!this.isOpenAnim)
					{
						this.PushPlayers(normalizedTime);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnableColliders(bool _on)
	{
		this.collidersEnabled = _on;
		if (this.colliders != null)
		{
			int i = this.colliders.Length - 1;
			while (i >= 0)
			{
				EntityCollisionRules entityCollisionRules = this.rules[i];
				if (!entityCollisionRules)
				{
					goto IL_0050;
				}
				if (!entityCollisionRules.IsStatic)
				{
					if (!entityCollisionRules.IsAnimPush)
					{
						goto IL_0050;
					}
					this.colliders[i].enabled = !_on;
				}
				IL_005E:
				i--;
				continue;
				IL_0050:
				this.colliders[i].enabled = _on;
				goto IL_005E;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CheckForObstacles()
	{
		if (this.colliders == null)
		{
			return false;
		}
		Ray ray;
		ray..ctor(Vector3.zero, Vector3.up);
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			Vector3 vector = list[i].position - Origin.position;
			vector.y += 0.35f;
			ray.origin = vector;
			for (int j = this.colliders.Length - 1; j >= 0; j--)
			{
				EntityCollisionRules entityCollisionRules = this.rules[j];
				if (!entityCollisionRules || (!entityCollisionRules.IsStatic && !entityCollisionRules.IsAnimPush))
				{
					Collider collider = this.colliders[j];
					bool enabled = collider.enabled;
					collider.enabled = true;
					RaycastHit raycastHit;
					bool flag = collider.Raycast(ray, ref raycastHit, 0.9f);
					collider.enabled = enabled;
					if (flag)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void PushPlayers(float _normalizedTime)
	{
		if (this.colliders == null)
		{
			return;
		}
		if (_normalizedTime < 0.5f)
		{
			return;
		}
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer = list[i];
			Vector3 vector = entityPlayer.position - Origin.position;
			vector.y += 0.1f;
			float num = entityPlayer.m_characterController.GetRadius();
			num *= num;
			for (int j = this.colliders.Length - 1; j >= 0; j--)
			{
				EntityCollisionRules entityCollisionRules = this.rules[j];
				if (entityCollisionRules && entityCollisionRules.IsAnimPush)
				{
					Collider collider = this.colliders[j];
					Vector3 vector2 = collider.ClosestPoint(vector);
					Vector3 vector3 = vector - vector2;
					vector3.y = 0f;
					float sqrMagnitude = vector3.sqrMagnitude;
					if (sqrMagnitude < num)
					{
						float num2 = 0.002f;
						if (sqrMagnitude == 0f)
						{
							vector3 = collider.transform.forward * -1f;
							if (_normalizedTime >= 0.94f)
							{
								num2 *= 7f;
							}
						}
						vector3 = vector3.normalized * num2;
						entityPlayer.PhysicsPush(vector3, Vector3.zero, true);
					}
				}
			}
		}
	}

	[Conditional("DEBUG_DOOR")]
	public void LogDoor(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} Door {1}", GameManager.frameCount, _format);
		Log.Warning(_format, _args);
	}

	public static readonly int OpenHash = Animator.StringToHash("Open");

	public static readonly int CloseHash = Animator.StringToHash("Close");

	public static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

	public static readonly int OpenTriggerHash = Animator.StringToHash("OpenTrigger");

	[Range(0f, 1f)]
	public float collideOffPercent;

	[Range(0f, 1f)]
	public float collideOnPercent = 0.99f;

	public bool disableColliderOnObstacleDetection;

	public bool keepAnimatorEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityCollisionRules[] rules;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Collider[] colliders;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool collidersEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isOpenAnim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool stopNext;
}

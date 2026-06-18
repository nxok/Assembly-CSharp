using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class GameObjectAnimalAnimation : AvatarController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.parentT = EModelBase.FindModel(base.transform);
		for (int i = this.parentT.childCount - 1; i >= 0; i--)
		{
			this.figureT = this.parentT.GetChild(i);
			if (this.figureT.gameObject.activeSelf)
			{
				break;
			}
		}
		this.anim = this.figureT.GetComponent<Animation>();
		if (this.anim["Idle1"])
		{
			this.anim.Play("Idle1");
		}
		this.attack1AS = this.anim["Attack1"];
		this.attack2AS = this.anim["Attack2"];
	}

	public void SetAlwaysWalk(bool _b)
	{
		this.bAlwaysWalk = _b;
	}

	public override bool IsAnimationAttackPlaying()
	{
		return (this.attack1AS != null && this.attack1AS.enabled) || (this.attack2AS != null && this.attack2AS.enabled);
	}

	public override void StartAnimationAttack()
	{
		this.state = GameObjectAnimalAnimation.State.Attack;
		if (this.attack1AS != null)
		{
			if (this.attack2AS != null)
			{
				if (this.entity.rand.RandomFloat > 0.5f)
				{
					this.anim.Play("Attack1");
					return;
				}
				this.anim.Play("Attack2");
				return;
			}
			else
			{
				this.anim.Play("Attack1");
			}
		}
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		if (this.isDead)
		{
			return;
		}
		this.state = GameObjectAnimalAnimation.State.Pain;
		if (this.anim["Pain"])
		{
			this.anim.Play("Pain");
		}
	}

	public override void StartAnimationJumping()
	{
		if (!this.entity.IsSwimming() && this.anim["Jump"] != null)
		{
			this.state = GameObjectAnimalAnimation.State.Jump;
			this.anim.CrossFade("Jump", 0.2f);
		}
	}

	public override void SetVisible(bool _b)
	{
		if (this.m_bVisible != _b || !this.visInit)
		{
			this.m_bVisible = _b;
			this.visInit = true;
			Transform transform = this.parentT;
			if (transform)
			{
				Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = _b;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		if (!this.m_bVisible)
		{
			return;
		}
		if (this.entity == null)
		{
			return;
		}
		if (this.entity.IsDead())
		{
			if (!this.isDead)
			{
				this.isDead = true;
				this.anim.Stop();
				if (this.anim["Death"])
				{
					this.anim.CrossFade("Death", 0.5f);
				}
			}
			return;
		}
		if (!this.entity.Jumping && (this.attack1AS == null || !this.attack1AS.enabled) && (this.attack2AS == null || !this.attack2AS.enabled) && (this.anim["Death"] == null || !this.anim["Death"].enabled) && (this.anim["Pain"] == null || !this.anim["Pain"].enabled))
		{
			float num = this.lastAbsMotion;
			float num2 = Mathf.Abs(this.entity.position.x - this.entity.lastTickPos[0].x) * 6f;
			float num3 = Mathf.Abs(this.entity.position.z - this.entity.lastTickPos[0].z) * 6f;
			if (!this.entity.isEntityRemote)
			{
				if (Mathf.Abs(num2 - this.lastAbsMotionX) > 0.01f || Mathf.Abs(num3 - this.lastAbsMotionZ) > 0.01f)
				{
					num = Mathf.Sqrt(num2 * num2 + num3 * num3);
					this.lastAbsMotionX = num2;
					this.lastAbsMotionZ = num3;
					this.lastAbsMotion = num;
				}
			}
			else if (num2 > this.lastAbsMotionX || num3 > this.lastAbsMotionZ)
			{
				num = Mathf.Sqrt(num2 * num2 + num3 * num3);
				this.lastAbsMotionX = num2;
				this.lastAbsMotionZ = num3;
				this.lastAbsMotion = num;
			}
			else
			{
				this.lastAbsMotionX *= 0.9f;
				this.lastAbsMotionZ *= 0.9f;
				this.lastAbsMotion *= 0.9f;
			}
			if (this.bAlwaysWalk || num > 0.15f)
			{
				if (this.entity.IsSwimming() && this.anim["Swim"] != null)
				{
					this.state = GameObjectAnimalAnimation.State.Swim;
					if (!this.anim["Swim"].enabled)
					{
						this.anim.Play("Swim");
					}
					this.anim["Swim"].speed = Mathf.Clamp01(num * 2f);
					return;
				}
				if (num >= 1f)
				{
					if (this.state != GameObjectAnimalAnimation.State.Run)
					{
						this.state = GameObjectAnimalAnimation.State.Run;
						AnimationState animationState = this.anim["Run"];
						if (!animationState.enabled)
						{
							this.anim.CrossFade("Run", 0.5f);
						}
						animationState.speed = Utils.FastMin(num, 1.5f);
					}
				}
				else if (this.state != GameObjectAnimalAnimation.State.Run)
				{
					this.state = GameObjectAnimalAnimation.State.Walk;
					AnimationState animationState2 = this.anim["Walk"];
					if (!animationState2.enabled)
					{
						this.anim.CrossFade("Walk", 0.5f);
					}
					animationState2.speed = num * 2f;
				}
				if (this.stepSoundCounter <= 0f)
				{
					this.stepSoundCounter = 0.3f;
					return;
				}
			}
			else
			{
				this.state = GameObjectAnimalAnimation.State.Idle;
				if (this.anim["Idle2"] != null)
				{
					if (!this.anim["Idle1"].enabled && !this.anim["Idle2"].enabled)
					{
						if (this.entity.rand.RandomFloat > 0.5f)
						{
							this.anim.CrossFade("Idle1", 0.5f);
							return;
						}
						this.anim.CrossFade("Idle2", 0.5f);
						return;
					}
				}
				else if (!this.anim["Idle1"].enabled)
				{
					this.anim.CrossFade("Idle1", 0.5f);
				}
			}
		}
	}

	public override Transform GetActiveModelRoot()
	{
		return this.figureT;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimIdle1 = "Idle1";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimIdle2 = "Idle2";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimAttack1 = "Attack1";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimAttack2 = "Attack2";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimPain = "Pain";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimJump = "Jump";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimDeath = "Death";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimRun = "Run";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimWalk = "Walk";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAnimSwim = "Swim";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform parentT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform figureT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public new Animation anim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AnimationState attack1AS;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AnimationState attack2AS;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool visInit;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_bVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDead;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bAlwaysWalk;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastAbsMotionX;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastAbsMotionZ;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastAbsMotion;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stepSoundCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObjectAnimalAnimation.State state;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum State
	{
		None,
		Attack,
		Idle,
		Jump,
		Pain,
		Run,
		Swim,
		Walk
	}
}

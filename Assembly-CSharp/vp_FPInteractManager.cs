using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPInteractManager : MonoBehaviour
{
	public float CrosshairTimeoutTimer { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Player = base.GetComponent<vp_FPPlayerEventHandler>();
		this.m_Camera = base.GetComponentInChildren<vp_FPCamera>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.m_Player != null)
		{
			this.m_Player.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (this.m_Player != null)
		{
			this.m_Player.Unregister(this);
		}
	}

	public virtual void OnStart_Dead()
	{
		this.ShouldFinishInteraction();
	}

	public virtual void LateUpdate()
	{
		if (this.m_Player.Dead.Active)
		{
			return;
		}
		if (this.m_OriginalCrosshair == null && this.m_Player.Crosshair.Get() != null)
		{
			this.m_OriginalCrosshair = this.m_Player.Crosshair.Get();
		}
		this.InteractCrosshair();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Interact()
	{
		if (this.ShouldFinishInteraction())
		{
			return false;
		}
		if (this.m_Player.SetWeapon.Active)
		{
			return false;
		}
		vp_Interactable vp_Interactable = null;
		if (!this.FindInteractable(out vp_Interactable))
		{
			return false;
		}
		if (vp_Interactable.InteractType != vp_Interactable.vp_InteractType.Normal)
		{
			return false;
		}
		if (!vp_Interactable.TryInteract(this.m_Player))
		{
			return false;
		}
		this.ResetCrosshair(false);
		this.m_LastInteractable = vp_Interactable;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ShouldFinishInteraction()
	{
		if (this.m_Player.Interactable.Get() != null)
		{
			this.m_CurrentCrosshairInteractable = null;
			this.ResetCrosshair(true);
			this.m_Player.Interactable.Get().FinishInteraction();
			this.m_Player.Interactable.Set(null);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InteractCrosshair()
	{
		if (this.m_Player.Crosshair.Get() == null)
		{
			return;
		}
		if (this.m_Player.Interactable.Get() != null)
		{
			return;
		}
		vp_Interactable interactable = null;
		if (this.FindInteractable(out interactable))
		{
			if (interactable != this.m_CurrentCrosshairInteractable)
			{
				if (this.CrosshairTimeoutTimer > Time.time && this.m_LastInteractable != null && interactable.GetType() == this.m_LastInteractable.GetType())
				{
					return;
				}
				this.m_CanInteract = true;
				this.m_CurrentCrosshairInteractable = interactable;
				if (interactable.InteractText != "" && !this.m_ShowTextTimer.Active)
				{
					vp_Timer.In(interactable.DelayShowingText, delegate
					{
						this.m_Player.HUDText.Send(interactable.InteractText);
					}, this.m_ShowTextTimer);
				}
				if (interactable.m_InteractCrosshair == null)
				{
					return;
				}
				this.m_Player.Crosshair.Set(interactable.m_InteractCrosshair);
				return;
			}
		}
		else
		{
			this.m_CanInteract = false;
			this.ResetCrosshair(true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool FindInteractable(out vp_Interactable interactable)
	{
		interactable = null;
		RaycastHit raycastHit;
		if (Physics.Raycast(this.m_Camera.Transform.position, this.m_Camera.Transform.forward, ref raycastHit, this.MaxInteractDistance, -538750989))
		{
			if (!this.m_Interactables.TryGetValue(raycastHit.collider, out interactable))
			{
				Dictionary<Collider, vp_Interactable> interactables = this.m_Interactables;
				Collider collider = raycastHit.collider;
				vp_Interactable component;
				interactable = (component = raycastHit.collider.GetComponent<vp_Interactable>());
				interactables.Add(collider, component);
			}
			return !(interactable == null) && (interactable.InteractDistance != 0f || raycastHit.distance < (this.m_Player.IsFirstPerson.Get() ? this.InteractDistance : this.InteractDistance3rdPerson)) && (interactable.InteractDistance <= 0f || raycastHit.distance < interactable.InteractDistance);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ResetCrosshair(bool reset = true)
	{
		if (this.m_OriginalCrosshair == null || this.m_Player.Crosshair.Get() == this.m_OriginalCrosshair)
		{
			return;
		}
		this.m_ShowTextTimer.Cancel();
		if (reset)
		{
			this.m_Player.Crosshair.Set(this.m_OriginalCrosshair);
		}
		this.m_CurrentCrosshairInteractable = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (attachedRigidbody == null || attachedRigidbody.isKinematic)
		{
			return;
		}
		vp_Interactable vp_Interactable = null;
		if (!this.m_Interactables.TryGetValue(hit.collider, out vp_Interactable))
		{
			this.m_Interactables.Add(hit.collider, vp_Interactable = hit.collider.GetComponent<vp_Interactable>());
		}
		if (vp_Interactable == null)
		{
			return;
		}
		if (vp_Interactable.InteractType != vp_Interactable.vp_InteractType.CollisionTrigger)
		{
			return;
		}
		hit.gameObject.SendMessage("TryInteract", this.m_Player, 1);
	}

	public virtual vp_Interactable OnValue_Interactable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_CurrentInteractable;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_CurrentInteractable = value;
		}
	}

	public virtual bool OnValue_CanInteract
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_CanInteract;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_CanInteract = value;
		}
	}

	public float InteractDistance = 2f;

	public float InteractDistance3rdPerson = 3f;

	public float MaxInteractDistance = 25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPCamera m_Camera;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Interactable m_CurrentInteractable;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Texture m_OriginalCrosshair;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Interactable m_LastInteractable;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<Collider, vp_Interactable> m_Interactables = new Dictionary<Collider, vp_Interactable>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Interactable m_CurrentCrosshairInteractable;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_ShowTextTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_CanInteract;
}

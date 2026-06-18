using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class vp_Interactable : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.m_Transform = base.transform;
		if (this.RecipientTags.Count == 0)
		{
			this.RecipientTags.Add("Player");
		}
		if (this.InteractType == vp_Interactable.vp_InteractType.Trigger && base.GetComponent<Collider>() != null)
		{
			base.GetComponent<Collider>().isTrigger = true;
		}
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

	public virtual bool TryInteract(vp_FPPlayerEventHandler player)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerEnter(Collider col)
	{
		if (this.InteractType != vp_Interactable.vp_InteractType.Trigger)
		{
			return;
		}
		foreach (string text in this.RecipientTags)
		{
			if (col.gameObject.tag == text)
			{
				goto IL_004F;
			}
		}
		return;
		IL_004F:
		this.m_Player = col.gameObject.GetComponent<vp_FPPlayerEventHandler>();
		if (this.m_Player == null)
		{
			return;
		}
		this.TryInteract(this.m_Player);
	}

	public virtual void FinishInteraction()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Interactable()
	{
	}

	public vp_Interactable.vp_InteractType InteractType;

	public List<string> RecipientTags = new List<string>();

	public float InteractDistance;

	public Texture m_InteractCrosshair;

	public string InteractText = "";

	public float DelayShowingText = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPController m_Controller;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPCamera m_Camera;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_WeaponHandler m_WeaponHandler;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;

	public enum vp_InteractType
	{
		Normal,
		Trigger,
		CollisionTrigger
	}
}

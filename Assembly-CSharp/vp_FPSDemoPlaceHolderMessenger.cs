using System;
using UnityEngine;

public class vp_FPSDemoPlaceHolderMessenger : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.Player = base.transform.root.GetComponent<vp_FPPlayerEventHandler>();
		if (this.Player == null)
		{
			base.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.Player == null)
		{
			return;
		}
		if (!this.Player.IsFirstPerson.Get() && this.Player.Climb.Active)
		{
			if (!this.m_WasClimbingIn3rdPersonLastFrame)
			{
				this.m_WasClimbingIn3rdPersonLastFrame = true;
				vp_Timer.In(0f, delegate
				{
					this.Player.HUDText.Send("PLACEHOLDER CLIMB ANIMATION");
				}, 3, 1f, null);
			}
		}
		else
		{
			this.m_WasClimbingIn3rdPersonLastFrame = false;
		}
		if (!this.Player.IsFirstPerson.Get() && this.Player.CurrentWeaponIndex.Get() == 4 && this.Player.Attack.Active)
		{
			if (!this.m_WasSwingingMaceIn3rdPersonLastFrame)
			{
				this.m_WasSwingingMaceIn3rdPersonLastFrame = true;
				vp_Timer.In(0f, delegate
				{
					this.Player.HUDText.Send("PLACEHOLDER MELEE ANIMATION");
				}, 3, 1f, null);
				return;
			}
		}
		else
		{
			this.m_WasSwingingMaceIn3rdPersonLastFrame = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPPlayerEventHandler Player;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_WasSwingingMaceIn3rdPersonLastFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_WasClimbingIn3rdPersonLastFrame;
}

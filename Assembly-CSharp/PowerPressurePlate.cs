using System;
using Audio;

public class PowerPressurePlate : PowerTrigger
{
	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.PressurePlate;
		}
	}

	public bool Pressed
	{
		get
		{
			return this.pressed;
		}
		set
		{
			this.pressed = value;
			if (this.pressed && !this.lastPressed)
			{
				Manager.BroadcastPlay(this.Position.ToVector3(), "pressureplate_down", 0f);
			}
			this.lastPressed = this.pressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CheckForActiveChange()
	{
		base.CheckForActiveChange();
		if (!this.pressed && this.lastPressed)
		{
			Manager.BroadcastPlay(this.Position.ToVector3(), "pressureplate_up", 0f);
			if (this.powerTime == 0f)
			{
				this.isActive = false;
				this.HandleDisconnectChildren();
				base.SendHasLocalChangesToRoot();
				this.powerTime = -1f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleSoundDisable()
	{
		base.HandleSoundDisable();
		this.lastPressed = this.pressed;
		this.pressed = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool pressed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastPressed;
}

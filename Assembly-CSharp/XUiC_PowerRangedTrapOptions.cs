using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerRangedTrapOptions : XUiController
{
	public TileEntityPoweredRangedTrap TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
		}
	}

	public XUiC_PowerRangedTrapWindowGroup Owner
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override void Init()
	{
		base.Init();
		this.pnlTargeting = base.GetChildById("pnlTargeting");
		this.btnTargetSelf = base.GetChildById("btnTargetSelf");
		this.btnTargetAllies = base.GetChildById("btnTargetAllies");
		this.btnTargetStrangers = base.GetChildById("btnTargetStrangers");
		this.btnTargetZombies = base.GetChildById("btnTargetZombies");
		if (this.btnTargetSelf != null)
		{
			this.btnTargetSelf.OnPress += this.btnTargetSelf_OnPress;
		}
		if (this.btnTargetAllies != null)
		{
			this.btnTargetAllies.OnPress += this.btnTargetAllies_OnPress;
		}
		if (this.btnTargetStrangers != null)
		{
			this.btnTargetStrangers.OnPress += this.btnTargetStrangers_OnPress;
		}
		if (this.btnTargetZombies != null)
		{
			this.btnTargetZombies.OnPress += this.btnTargetZombies_OnPress;
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTargetSelf_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiV_Button xuiV_Button = this.btnTargetSelf.ViewComponent as XUiV_Button;
		xuiV_Button.Selected = !xuiV_Button.Selected;
		if (xuiV_Button.Selected)
		{
			this.TileEntity.TargetType |= 1;
			return;
		}
		this.TileEntity.TargetType &= -2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTargetAllies_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiV_Button xuiV_Button = this.btnTargetAllies.ViewComponent as XUiV_Button;
		xuiV_Button.Selected = !xuiV_Button.Selected;
		if (xuiV_Button.Selected)
		{
			this.TileEntity.TargetType |= 2;
			return;
		}
		this.TileEntity.TargetType &= -3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTargetStrangers_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiV_Button xuiV_Button = this.btnTargetStrangers.ViewComponent as XUiV_Button;
		xuiV_Button.Selected = !xuiV_Button.Selected;
		if (xuiV_Button.Selected)
		{
			this.TileEntity.TargetType |= 4;
			return;
		}
		this.TileEntity.TargetType &= -5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTargetZombies_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiV_Button xuiV_Button = this.btnTargetZombies.ViewComponent as XUiV_Button;
		xuiV_Button.Selected = !xuiV_Button.Selected;
		if (xuiV_Button.Selected)
		{
			this.TileEntity.TargetType |= 8;
			return;
		}
		this.TileEntity.TargetType &= -9;
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.tileEntity == null)
		{
			return;
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.tileEntity.SetUserAccessing(true);
		this.SetupTargeting();
		this.tileEntity.SetModified();
	}

	public override void OnClose()
	{
		GameManager instance = GameManager.Instance;
		this.tileEntity.ToWorldPos();
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			this.tileEntity.SetUserAccessing(false);
			LockManager.Instance.UnlockRequestLocal();
			this.tileEntity.SetModified();
		}
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTargeting()
	{
		if (this.pnlTargeting != null)
		{
			if (this.btnTargetSelf != null)
			{
				this.btnTargetSelf.OnPress -= this.btnTargetSelf_OnPress;
				((XUiV_Button)this.btnTargetSelf.ViewComponent).Selected = this.TileEntity.TargetSelf;
				this.btnTargetSelf.OnPress += this.btnTargetSelf_OnPress;
			}
			if (this.btnTargetAllies != null)
			{
				this.btnTargetAllies.OnPress -= this.btnTargetAllies_OnPress;
				((XUiV_Button)this.btnTargetAllies.ViewComponent).Selected = this.TileEntity.TargetAllies;
				this.btnTargetAllies.OnPress += this.btnTargetAllies_OnPress;
			}
			if (this.btnTargetStrangers != null)
			{
				this.btnTargetStrangers.OnPress -= this.btnTargetStrangers_OnPress;
				((XUiV_Button)this.btnTargetStrangers.ViewComponent).Selected = this.TileEntity.TargetStrangers;
				this.btnTargetStrangers.OnPress += this.btnTargetStrangers_OnPress;
			}
			if (this.btnTargetZombies != null)
			{
				this.btnTargetZombies.OnPress -= this.btnTargetZombies_OnPress;
				((XUiV_Button)this.btnTargetZombies.ViewComponent).Selected = this.TileEntity.TargetZombies;
				this.btnTargetZombies.OnPress += this.btnTargetZombies_OnPress;
			}
		}
	}

	public Vector3i GetBlockPos()
	{
		if (this.TileEntity != null)
		{
			return this.TileEntity.ToWorldPos();
		}
		return Vector3i.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController pnlTargeting;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetSelf;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetAllies;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetStrangers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetZombies;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredRangedTrap tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;
}

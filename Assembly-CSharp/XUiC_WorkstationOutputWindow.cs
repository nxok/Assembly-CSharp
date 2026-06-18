using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationOutputWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.outputGrid = base.GetChildByType<XUiC_WorkstationOutputGrid>();
		this.controls = base.GetChildByType<XUiC_ContainerStandardControls>();
		if (this.controls != null)
		{
			this.controls.SortPressed = delegate(PackedBoolArray _ignoredSlots)
			{
				ItemStack[] array = StackSortUtil.CombineAndSortStacks(this.outputGrid.GetSlots(), 0, _ignoredSlots);
				this.outputGrid.SetSlots(array);
			};
			this.controls.MoveAllowed = delegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory)
			{
				_parentWindow = this;
				_grid = this.outputGrid;
				_inventory = this.xui.PlayerInventory;
				return true;
			};
			this.controls.MoveAllDone = delegate(bool _allMoved, bool _anyMoved)
			{
				if (_anyMoved)
				{
					Manager.BroadcastPlayByLocalPlayer(this.xui.playerUI.entityPlayer.position + Vector3.one * 0.5f, "UseActions/takeall1");
				}
			};
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.xui.playerUI.windowManager.IsInputActive() && (this.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || this.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
		{
			this.controls.MoveAll();
			this.windowGroup.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationOutputGrid outputGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls controls;
}

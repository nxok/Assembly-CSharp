using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnBlockEditor : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_SpawnBlockEditor.ID = base.WindowGroup.Id;
		this.list = base.GetChildByType<XUiC_StringList>();
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnOk") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnOk_OnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton2 = base.GetChildById("btnCancel") as XUiC_SimpleButton;
		if (xuiC_SimpleButton2 != null)
		{
			xuiC_SimpleButton2.OnPressed += delegate(XUiController _, int _)
			{
				this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_StringList.Entry selectedEntryData = this.list.SelectedEntryData;
		if (selectedEntryData == null)
		{
			return;
		}
		BlockValue block = GameManager.Instance.World.GetBlock(this.blockPos);
		block.meta = (byte)selectedEntryData.Tag;
		GameManager.Instance.World.SetBlockRPC(this.blockPos, block);
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setBlock(Vector3i _blockPos, BlockValue _blockValue, BlockSpawnEntity _block)
	{
		string[] spawnClasses = _block.spawnClasses;
		this.blockPos = _blockPos;
		this.list.ClearList();
		byte b = 0;
		while ((int)b < spawnClasses.Length)
		{
			this.list.AddEntry(spawnClasses[(int)b], b);
			b += 1;
		}
		this.list.SortList();
		this.list.RebuildList(false);
		byte meta = _blockValue.meta;
		if ((int)meta < spawnClasses.Length)
		{
			this.list.SelectByString(spawnClasses[(int)meta]);
		}
	}

	public static void Open(LocalPlayerUI _playerUi, Vector3i _blockPos, BlockValue _blockValue, BlockSpawnEntity _block)
	{
		XUiC_SpawnBlockEditor childByType = _playerUi.xui.FindWindowGroupByName(XUiC_SpawnBlockEditor.ID).GetChildByType<XUiC_SpawnBlockEditor>();
		_playerUi.windowManager.Open(XUiC_SpawnBlockEditor.ID, true);
		childByType.setBlock(_blockPos, _blockValue, _block);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_StringList list;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;
}

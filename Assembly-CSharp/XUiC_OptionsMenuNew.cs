using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsMenuNew : XUiController
{
	[XuiXmlBinding("video_options_simplified")]
	public bool VideoOptionsSimplified
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			OptionsVideoWindowMode optionsVideoWindow = UIOptions.OptionsVideoWindow;
			if (optionsVideoWindow == OptionsVideoWindowMode.Simplified)
			{
				return true;
			}
			if (optionsVideoWindow != OptionsVideoWindowMode.Detailed)
			{
				Log.Error(string.Format("Unknown video options menu {0}", UIOptions.OptionsVideoWindow));
				return false;
			}
			return false;
		}
	}

	[XuiXmlBinding("has_block_list")]
	public bool HasBlockList
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return BlockedPlayerList.Instance != null;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_OptionsMenuNew.ParentSelector = base.GetParentByType<XUiC_WindowSelector>();
		UIOptions.OnOptionsVideoWindowChanged += this.OnVideoOptionsWindowChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		XUiC_OptionsMenuNew.ParentSelector = null;
		UIOptions.OnOptionsVideoWindowChanged -= this.OnVideoOptionsWindowChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoOptionsWindowChanged(OptionsVideoWindowMode _mode)
	{
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GameManager.Instance.Pause(true);
	}

	public override void OnClose()
	{
		base.OnClose();
		GameManager.Instance.Pause(false);
	}

	public static XUiC_WindowSelector ParentSelector;
}

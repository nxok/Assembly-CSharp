using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignStackGrid : XUiController
{
	public int Length
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			this.page = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.signStackControllers = base.GetChildrenByType<XUiC_SignStack>(null);
		this.Length = this.signStackControllers.Length;
		this.IsDirty = false;
	}

	public void SetSignIds(List<GlobalSignId> signIdList, GlobalSignId newSelectedId)
	{
		this.xui.GetChildByType<XUiC_SignGalleryWindow>();
		int count = signIdList.Count;
		this.currentList = signIdList;
		this.selectedId = newSelectedId;
		for (int i = 0; i < this.Length; i++)
		{
			int num = i + this.Length * this.page;
			XUiC_SignStack xuiC_SignStack = this.signStackControllers[i];
			if (num < count)
			{
				xuiC_SignStack.SignId = signIdList[num];
				if (xuiC_SignStack.SignId == this.selectedId)
				{
					xuiC_SignStack.IsSelected = true;
				}
				else if (xuiC_SignStack.IsSelected)
				{
					xuiC_SignStack.IsSelected = false;
				}
			}
			else
			{
				xuiC_SignStack.SignId = GlobalSignId.InvalidId;
				if (xuiC_SignStack.IsSelected)
				{
					xuiC_SignStack.IsSelected = false;
				}
			}
		}
		this.IsDirty = false;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.SetSignIds(this.currentList, this.selectedId);
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int curPageIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int numPages;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SignStack[] signStackControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId selectedId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GlobalSignId> currentList;
}

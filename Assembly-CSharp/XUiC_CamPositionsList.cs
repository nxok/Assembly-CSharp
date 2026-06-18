using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CamPositionsList : XUiC_List<XUiC_CamPositionsList.CamPerspectiveEntry>
{
	public bool ShowAddCamPositionWindow
	{
		get
		{
			return this.showAddCamPositionWindow;
		}
		set
		{
			if (value == this.showAddCamPositionWindow)
			{
				return;
			}
			this.showAddCamPositionWindow = value;
			base.RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("btnAddCamPosition");
		XUiV_Button xuiV_Button = ((childById != null) ? childById.ViewComponent : null) as XUiV_Button;
		if (xuiV_Button != null)
		{
			xuiV_Button.Controller.OnPress += delegate(XUiController _, int _)
			{
				this.ShowAddCamPositionWindow = !this.ShowAddCamPositionWindow;
			};
		}
		base.OnVisiblity += this.OnVisibilityChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnVisiblity -= this.OnVisibilityChanged;
	}

	public void UpdateList()
	{
		int page = base.Page;
		this.RebuildList(false);
		base.Page = page;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		this.perspectives.Load();
		foreach (KeyValuePair<string, CameraPerspectives.Perspective> keyValuePair in this.perspectives.Perspectives)
		{
			string text;
			CameraPerspectives.Perspective perspective;
			keyValuePair.Deconstruct(out text, out perspective);
			CameraPerspectives.Perspective perspective2 = perspective;
			this.allEntries.Add(new XUiC_CamPositionsList.CamPerspectiveEntry(perspective2));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "cam_position_add_open")
		{
			_value = this.ShowAddCamPositionWindow.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public void OnVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			this.RebuildList(false);
		}
	}

	public void Add(string _name, string _comment)
	{
		CameraPerspectives.Perspective perspective = new CameraPerspectives.Perspective(_name, this.xui.playerUI.entityPlayer, _comment);
		this.perspectives.Perspectives.Add(perspective.Name, perspective);
		this.perspectives.Save();
		this.UpdateList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CameraPerspectives perspectives = new CameraPerspectives(false);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showAddCamPositionWindow;

	[Preserve]
	public class CamPerspectiveEntry : XUiListEntry<XUiC_CamPositionsList.CamPerspectiveEntry>
	{
		public CamPerspectiveEntry(CameraPerspectives.Perspective _perspective)
		{
			this.Perspective = _perspective;
		}

		public override int CompareTo(XUiC_CamPositionsList.CamPerspectiveEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return string.Compare(this.Perspective.Name, _otherEntry.Perspective.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			if (string.IsNullOrEmpty(_searchString))
			{
				return true;
			}
			if (!this.Perspective.Name.ContainsCaseInsensitive(_searchString))
			{
				string comment = this.Perspective.Comment;
				return comment != null && comment.ContainsCaseInsensitive(_searchString);
			}
			return true;
		}

		public readonly CameraPerspectives.Perspective Perspective;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class CamPositionsListEntryController : XUiC_List<XUiC_CamPositionsList.CamPerspectiveEntry>.XUiC_ListEntry
	{
		public override void Init()
		{
			base.Init();
			this.parentList = base.GetParentByType<XUiC_CamPositionsList>();
			XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("camButton") as XUiC_SimpleButton;
			if (xuiC_SimpleButton != null)
			{
				xuiC_SimpleButton.OnPressed += delegate(XUiController _, int _)
				{
					base.GetEntry().Perspective.ToPlayer(this.xui.playerUI.entityPlayer);
				};
			}
			XUiController childById = base.GetChildById("btnDelete");
			XUiV_Button xuiV_Button = ((childById != null) ? childById.ViewComponent : null) as XUiV_Button;
			if (xuiV_Button != null)
			{
				xuiV_Button.Controller.OnPress += delegate(XUiController _, int _)
				{
					XUiC_CamPositionsList.CamPerspectiveEntry entry = base.GetEntry();
					this.parentList.perspectives.Perspectives.Remove(entry.Perspective.Name);
					this.parentList.perspectives.Save();
					this.parentList.UpdateList();
				};
			}
		}

		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_CamPositionsList.CamPerspectiveEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Perspective.Name : null) ?? "";
		}

		[XuiXmlBinding("position")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingPosition()
		{
			if (this.entryData != null)
			{
				return ValueDisplayFormatters.WorldPos(this.entryData.Perspective.Position, " ", false);
			}
			return "";
		}

		[XuiXmlBinding("direction")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDirection()
		{
			XUiC_CamPositionsList.CamPerspectiveEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Perspective.Direction.ToCultureInvariantString() : null) ?? "";
		}

		[XuiXmlBinding("comment")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingComment()
		{
			XUiC_CamPositionsList.CamPerspectiveEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Perspective.Comment : null) ?? "";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_CamPositionsList parentList;
	}
}

using System;

public class XUiC_SelectableEntry : XUiController
{
	public bool IsSelected
	{
		get
		{
			return this.isSelected;
		}
		set
		{
			if (value)
			{
				if (this.xui.CurrentSelectedEntry != null)
				{
					this.xui.CurrentSelectedEntry.SelectedChanged(false);
					this.xui.CurrentSelectedEntry.isSelected = false;
				}
			}
			else if (this.xui.CurrentSelectedEntry == this)
			{
				this.xui.CurrentSelectedEntry.SelectedChanged(false);
				this.xui.CurrentSelectedEntry.isSelected = false;
				this.xui.CurrentSelectedEntry = null;
			}
			this.isSelected = value;
			if (this.isSelected)
			{
				this.xui.CurrentSelectedEntry = this;
			}
			this.SelectedChanged(this.isSelected);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SelectedChanged(bool _isSelected)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSelected;
}

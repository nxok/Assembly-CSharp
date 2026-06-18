using System;

public interface IXUiElement
{
	XUiController Controller { get; }

	string GetXuiHierarchy();
}

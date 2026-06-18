using System;
using UnityEngine;

public class XUiV_Empty : XUiView
{
	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return null;
		}
	}

	public XUiV_Empty(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	public override Vector3[] WorldCorners
	{
		get
		{
			return XUiV_Empty.WorldCornersEmpty;
		}
	}

	public static readonly Vector3[] WorldCornersEmpty = new Vector3[4];
}

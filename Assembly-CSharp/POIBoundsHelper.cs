using System;
using System.Collections.Generic;
using UnityEngine;

public class POIBoundsHelper : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (this.SideHelpers != null && this.SideHelpers.Count > 0)
		{
			this.SideHelpers[0].Setup();
			this.material = new Material(this.SideHelpers[0].MeshRenderer.material);
			this.fullAlpha = this.material.color.a;
			this.material.color = new Color(this.material.color.r, this.material.color.g, this.material.color.b, 0f);
			for (int i = 0; i < this.SideHelpers.Count; i++)
			{
				this.SideHelpers[i].Owner = this;
				this.SideHelpers[i].Setup();
				this.SideHelpers[i].MeshRenderer.material = this.material;
				this.SideHelpers[i].MeshRenderer.enabled = false;
			}
		}
	}

	public void SetSidesVisible(bool visible)
	{
		for (int i = 0; i < this.SideHelpers.Count; i++)
		{
			this.SideHelpers[i].MeshRenderer.enabled = visible;
		}
	}

	public void SetPosition(Vector3 position, Vector3 size)
	{
		base.transform.position = position;
		for (int i = 0; i < this.SideHelpers.Count; i++)
		{
			this.SideHelpers[i].SetSize(size);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		switch (this.CurrentState)
		{
		case POIBoundsHelper.WallVisibilityStates.None:
		case POIBoundsHelper.WallVisibilityStates.Visible:
			break;
		case POIBoundsHelper.WallVisibilityStates.Showing:
		{
			float num = Mathf.MoveTowards(this.material.color.a, this.fullAlpha, Time.deltaTime);
			this.material.color = new Color(this.material.color.r, this.material.color.g, this.material.color.b, num);
			if (num == this.fullAlpha)
			{
				this.showTime = 3f;
				this.CurrentState = POIBoundsHelper.WallVisibilityStates.Visible;
				return;
			}
			break;
		}
		case POIBoundsHelper.WallVisibilityStates.ReadyToHide:
			this.showTime -= Time.deltaTime;
			if (this.showTime <= 0f)
			{
				this.CurrentState = POIBoundsHelper.WallVisibilityStates.Hiding;
				return;
			}
			break;
		case POIBoundsHelper.WallVisibilityStates.Hiding:
		{
			float num2 = Mathf.MoveTowards(this.material.color.a, 0f, Time.deltaTime);
			this.material.color = new Color(this.material.color.r, this.material.color.g, this.material.color.b, num2);
			if (num2 == 0f)
			{
				this.CurrentState = POIBoundsHelper.WallVisibilityStates.None;
				this.SetSidesVisible(false);
			}
			break;
		}
		default:
			return;
		}
	}

	public void AddSideEntered(POIBoundsSideHelper side)
	{
		if (!this.ActivatedHelpers.Contains(side))
		{
			this.ActivatedHelpers.Add(side);
		}
		if (this.ActivatedHelpers.Count > 0)
		{
			this.CurrentState = POIBoundsHelper.WallVisibilityStates.Showing;
			this.SetSidesVisible(true);
		}
	}

	public void RemoveSideEntered(POIBoundsSideHelper side)
	{
		if (this.ActivatedHelpers.Contains(side))
		{
			this.ActivatedHelpers.Remove(side);
		}
		if (this.ActivatedHelpers.Count == 0)
		{
			this.CurrentState = POIBoundsHelper.WallVisibilityStates.ReadyToHide;
			this.showTime = 3f;
		}
	}

	public List<POIBoundsSideHelper> SideHelpers = new List<POIBoundsSideHelper>();

	public List<POIBoundsSideHelper> ActivatedHelpers = new List<POIBoundsSideHelper>();

	public POIBoundsHelper.WallVisibilityStates CurrentState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material material;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float showTime = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fullAlpha = -1f;

	public enum WallVisibilityStates
	{
		None,
		Showing,
		Visible,
		ReadyToHide,
		Hiding
	}
}

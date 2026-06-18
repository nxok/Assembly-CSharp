using System;

public class BoundaryProjectorTreasure : BoundaryProjector
{
	public bool WithinRadius
	{
		get
		{
			return this.withinRadius;
		}
		set
		{
			if (this.withinRadius != value)
			{
				this.withinRadius = value;
				this.HandleWithinRadiusChange();
			}
		}
	}

	public float CurrentRadius
	{
		get
		{
			return this.ProjectorList[0].Projector.orthographicSize;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetupProjectors()
	{
		base.SetAlpha(0, 1f);
		base.SetAutoRotate(0, true, 2f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleWithinRadiusChange()
	{
		base.SetGlow(0, this.withinRadius);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool withinRadius;
}

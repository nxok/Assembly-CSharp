using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorMarkerManagementComponent : AIDirectorComponent
{
	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		this.TickMarkers(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickMarkers(double _dt)
	{
		for (int i = this.markers.Count - 1; i >= 0; i--)
		{
			IAIDirectorMarker iaidirectorMarker = this.markers[i];
			iaidirectorMarker.Tick(_dt);
			if (iaidirectorMarker.TimeToLive <= 0f || (iaidirectorMarker.Player != null && iaidirectorMarker.Player.IsDead()))
			{
				this.markers.RemoveAt(i);
				iaidirectorMarker.Release();
			}
		}
	}

	public IAIDirectorMarker FindBestMarker(Vector3 _pos, ref double _inOutIntensity)
	{
		IAIDirectorMarker iaidirectorMarker = null;
		int num = -1;
		for (int i = this.markers.Count - 1; i >= 0; i--)
		{
			IAIDirectorMarker iaidirectorMarker2 = this.markers[i];
			if (iaidirectorMarker2.TimeToLive > 0f)
			{
				double num2 = iaidirectorMarker2.IntensityForPosition(_pos);
				if (num2 > 0.0 && iaidirectorMarker2.Priority > num)
				{
					num = iaidirectorMarker2.Priority;
					iaidirectorMarker = iaidirectorMarker2;
					_inOutIntensity = num2;
				}
			}
		}
		return iaidirectorMarker;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IAIDirectorMarker> markers = new List<IAIDirectorMarker>(256);
}

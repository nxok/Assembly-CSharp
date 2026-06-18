using System;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryProjector : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Projector[] componentsInChildren = base.transform.GetComponentsInChildren<Projector>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this.ProjectorList.Add(new BoundaryProjector.ProjectorEntry
			{
				Projector = componentsInChildren[i],
				EffectData = new BoundaryProjector.ProjectorEffectData()
			});
		}
		this.SetupProjectors();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupProjectors()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		for (int i = 0; i < this.ProjectorList.Count; i++)
		{
			if (this.ProjectorList[i] != null && this.ProjectorList[i].Projector.gameObject.activeSelf)
			{
				BoundaryProjector.ProjectorEntry projectorEntry = this.ProjectorList[i];
				if (projectorEntry.EffectData.AutoRotate)
				{
					Vector3 eulerAngles = projectorEntry.Projector.transform.localRotation.eulerAngles;
					projectorEntry.Projector.transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y + Time.deltaTime * projectorEntry.EffectData.RotationSpeed, eulerAngles.z);
				}
				if (projectorEntry.EffectData.targetRadius != -1f)
				{
					projectorEntry.Projector.orthographicSize = Mathf.Lerp(projectorEntry.Projector.orthographicSize, projectorEntry.EffectData.targetRadius, Time.deltaTime);
					if (projectorEntry.Projector.orthographicSize == projectorEntry.EffectData.targetRadius)
					{
						projectorEntry.EffectData.targetRadius = -1f;
					}
				}
				if (projectorEntry.EffectData.IsGlowing)
				{
					Color color = projectorEntry.Projector.material.color;
					float num = Mathf.PingPong(Time.time, 0.25f);
					projectorEntry.Projector.material.color = new Color(color.r, color.g, color.b, 0.5f + num * 2f);
				}
			}
		}
		if (this.targetPos != BoundaryProjector.invalidPos)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, this.targetPos, Time.deltaTime);
			if (base.transform.position == this.targetPos)
			{
				this.targetPos = BoundaryProjector.invalidPos;
			}
		}
	}

	public void SetRadius(int projectorID, float size)
	{
		if (projectorID < this.ProjectorList.Count && this.ProjectorList[projectorID] != null)
		{
			if (this.ProjectorList[projectorID].Projector.orthographicSize == -1f || size == 0f)
			{
				this.ProjectorList[projectorID].Projector.orthographicSize = size;
				this.ProjectorList[projectorID].EffectData.targetRadius = -1f;
				return;
			}
			this.ProjectorList[projectorID].EffectData.targetRadius = size;
		}
	}

	public void SetAlpha(int projectorID, float alpha)
	{
		if (projectorID < this.ProjectorList.Count && this.ProjectorList[projectorID] != null)
		{
			Color color = this.ProjectorList[projectorID].Projector.material.color;
			this.ProjectorList[projectorID].Projector.material.color = new Color(color.r, color.g, color.b, alpha);
		}
	}

	public void SetGlow(int projectorID, bool isGlowing)
	{
		if (projectorID < this.ProjectorList.Count && this.ProjectorList[projectorID] != null)
		{
			Color color = this.ProjectorList[projectorID].Projector.material.color;
			this.ProjectorList[projectorID].EffectData.IsGlowing = isGlowing;
		}
	}

	public void SetAutoRotate(int projectorID, bool autoRotate, float rotateSpeed)
	{
		if (projectorID < this.ProjectorList.Count && this.ProjectorList[projectorID] != null)
		{
			this.ProjectorList[projectorID].EffectData.AutoRotate = autoRotate;
			this.ProjectorList[projectorID].EffectData.RotationSpeed = rotateSpeed;
		}
	}

	public void SetMoveToPosition(Vector3 vNew)
	{
		if (base.transform.position.y == -999f)
		{
			base.transform.position = vNew;
			return;
		}
		this.targetPos = vNew;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<BoundaryProjector.ProjectorEntry> ProjectorList = new List<BoundaryProjector.ProjectorEntry>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static Vector3 invalidPos = new Vector3(-999f, -999f, -999f);

	public Vector3 targetPos = BoundaryProjector.invalidPos;

	public bool IsInitialized;

	public class ProjectorEntry
	{
		public BoundaryProjector.ProjectorEffectData EffectData;

		public Projector Projector;
	}

	public class ProjectorEffectData
	{
		public bool AutoRotate;

		public float RotationSpeed;

		public bool IsGlowing;

		public float targetRadius = -1f;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WireFrameSphere : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.lr = base.gameObject.GetComponent<LineRenderer>();
		this.lr.startWidth = 0.01f;
		this.lr.endWidth = 0.01f;
		this.positions = new List<Vector3>();
		this.player = Object.FindAnyObjectByType<LocalPlayerCamera>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.player != null)
		{
			float num = Mathf.Abs(this.player.transform.position.magnitude - base.transform.position.magnitude);
			this.lr.startWidth = (this.lr.endWidth = 0.01f * num);
		}
		if (this.radius != this.newRadius)
		{
			this.radius = this.newRadius;
			this.positions.Clear();
			this.positions.AddRange(this.RenderCircleOnPlane(true));
			this.positions.AddRange(this.RenderCircleOnPlane(false));
			this.lr.positionCount = this.positions.Count;
			this.lr.SetPositions(this.positions.ToArray());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> RenderCircleOnPlane(bool xyPlane)
	{
		this.numOfVertices = 100 + (int)Math.Pow((double)((int)this.radius), 2.0);
		List<Vector3> list = new List<Vector3>(this.numOfVertices);
		this.angle = 6.2831855f / (float)(this.numOfVertices - 1);
		for (int i = 0; i < this.numOfVertices; i++)
		{
			float num = this.center.x + this.radius * Mathf.Cos((float)i * this.angle);
			float num2 = this.center.y + (xyPlane ? (this.radius * Mathf.Sin((float)i * this.angle)) : 0f);
			float num3 = this.center.z + (xyPlane ? 0f : (this.radius * Mathf.Sin((float)i * this.angle)));
			list.Add(new Vector3(num, num2, num3));
		}
		return list;
	}

	public void KillWF()
	{
		Object.Destroy(base.gameObject);
	}

	public Vector3 center;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float TAU = 6.2831855f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float radius = 1f;

	public float newRadius = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LineRenderer lr;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> positions;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int numOfVertices = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float angle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerCamera player;
}

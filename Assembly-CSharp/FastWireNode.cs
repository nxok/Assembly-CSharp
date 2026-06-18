using System;
using System.Collections.Generic;
using UnityEngine;

public class FastWireNode : MonoBehaviour, IWireNode
{
	public Vector3 StartPosition
	{
		get
		{
			return this.startPosition;
		}
		set
		{
			this.startPosition = value;
		}
	}

	public Vector3 EndPosition
	{
		get
		{
			return this.endPosition;
		}
		set
		{
			this.endPosition = value;
		}
	}

	public void Awake()
	{
		if (FastWireNode.BaseMaterial == null)
		{
			FastWireNode.BaseMaterial = Resources.Load<Material>("Materials/WireMaterial");
		}
		if (this.meshFilter == null)
		{
			this.meshFilter = NGUITools.AddMissingComponent<MeshFilter>(base.transform.gameObject);
		}
		if (this.meshCollider == null)
		{
			this.meshCollider = NGUITools.AddMissingComponent<MeshCollider>(base.transform.gameObject);
			this.meshCollider.convex = true;
			this.meshCollider.isTrigger = true;
		}
		if (this.meshRenderer == null)
		{
			this.meshRenderer = NGUITools.AddMissingComponent<MeshRenderer>(base.transform.gameObject);
			this.meshRenderer.material = FastWireNode.BaseMaterial;
		}
		Utils.SetColliderLayerRecursively(base.gameObject, 29);
	}

	public void BuildMesh()
	{
		Vector3 vector = this.EndPosition + this.EndOffset;
		Vector3 vector2 = this.StartPosition + this.StartOffset;
		float num = Vector3.Distance(vector, vector2);
		if (num > 256f)
		{
			return;
		}
		if (num < 0.01f)
		{
			return;
		}
		Vector3 vector3 = vector2 - vector;
		if (vector3.magnitude == 0f)
		{
			return;
		}
		Vector3 vector4 = vector3 / 16f;
		if (vector4.magnitude == 0f)
		{
			return;
		}
		Vector3 vector5 = vector + vector4;
		List<Vector3> list = new List<Vector3>();
		list.Add(vector);
		for (int i = 0; i < 15; i++)
		{
			if (vector3.normalized != Vector3.up && vector3.normalized != Vector3.down)
			{
				float num2 = Mathf.Abs((8f - (float)(i + 1)) / 8f);
				num2 *= num2;
				list.Add(vector5 - new Vector3(0f, Mathf.Lerp(0f, this.maxWireDip, 1f - num2), 0f));
			}
			else
			{
				list.Add(vector5);
			}
			vector5 += vector4;
		}
		if (list.Count > 1)
		{
			list[0] = vector;
			list[list.Count - 1] = vector2;
		}
		Vector3 vector6 = Vector3.one * float.PositiveInfinity;
		Vector3 vector7 = -Vector3.one * float.PositiveInfinity;
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector2> list4 = new List<Vector2>();
		List<Vector2> list5 = new List<Vector2>();
		int[] array = new int[396];
		for (int j = 0; j < list.Count; j++)
		{
			float num3 = (float)j / (float)list.Count * (num * 0.25f);
			list4.Add(Vector2.right * num3);
			list4.Add(Vector2.right * num3 + Vector2.up);
			list4.Add(Vector2.right * num3);
			list4.Add(Vector2.right * num3 + Vector2.up);
			list5.Add(Vector2.zero);
			list5.Add(Vector2.zero);
			list5.Add(Vector2.zero);
			list5.Add(Vector2.zero);
			if (j > 0)
			{
				vector3 = list[j] - list[j - 1];
			}
			Vector3 vector8 = Vector3.Cross(Vector3.up, vector3.normalized).normalized;
			Vector3 vector9 = Vector3.Cross(vector3.normalized, vector8).normalized;
			if (vector3.normalized == Vector3.up || vector3.normalized == Vector3.down)
			{
				vector8 = Vector3.Cross(Vector3.forward, vector3.normalized).normalized;
				vector9 = Vector3.Cross(vector3.normalized, vector8).normalized;
			}
			list2.Add(vector9 * this.wireRadius + list[j]);
			list2.Add(-vector9 * this.wireRadius + list[j]);
			list2.Add(vector8 * this.wireRadius + list[j]);
			list2.Add(-vector8 * this.wireRadius + list[j]);
			if (j == 0)
			{
				vector9 = Vector3.Lerp(vector9, (vector - vector2).normalized, 0.5f).normalized;
				vector8 = Vector3.Lerp(vector8, (vector - vector2).normalized, 0.5f).normalized;
			}
			else if (j == list.Count - 1)
			{
				vector9 = Vector3.Lerp(vector9, -(vector - vector2).normalized, 0.5f).normalized;
				vector8 = Vector3.Lerp(vector8, -(vector - vector2).normalized, 0.5f).normalized;
			}
			list3.Add(vector9);
			list3.Add(-vector9);
			list3.Add(vector8);
			list3.Add(-vector8);
			if (list[j].x < vector6.x)
			{
				vector6.x = list[j].x;
			}
			if (list[j].x > vector7.x)
			{
				vector7.x = list[j].x;
			}
			if (list[j].y < vector6.y)
			{
				vector6.y = list[j].y;
			}
			if (list[j].y > vector7.y)
			{
				vector7.y = list[j].y;
			}
			if (list[j].z < vector6.z)
			{
				vector6.z = list[j].z;
			}
			if (list[j].z > vector7.z)
			{
				vector7.z = list[j].z;
			}
		}
		int num4 = 0;
		for (int k = 0; k < 15; k++)
		{
			array[num4++] = 4 * k;
			array[num4++] = 4 + 4 * k;
			array[num4++] = 7 + 4 * k;
			array[num4++] = 7 + 4 * k;
			array[num4++] = 3 + 4 * k;
			array[num4++] = 4 * k;
			array[num4++] = 4 + 4 * k;
			array[num4++] = 4 * k;
			array[num4++] = 2 + 4 * k;
			array[num4++] = 2 + 4 * k;
			array[num4++] = 6 + 4 * k;
			array[num4++] = 4 + 4 * k;
			array[num4++] = 3 + 4 * k;
			array[num4++] = 7 + 4 * k;
			array[num4++] = 5 + 4 * k;
			array[num4++] = 5 + 4 * k;
			array[num4++] = 1 + 4 * k;
			array[num4++] = 3 + 4 * k;
			array[num4++] = 6 + 4 * k;
			array[num4++] = 2 + 4 * k;
			array[num4++] = 1 + 4 * k;
			array[num4++] = 1 + 4 * k;
			array[num4++] = 5 + 4 * k;
			array[num4++] = 6 + 4 * k;
		}
		array[num4++] = 0;
		array[num4++] = 3;
		array[num4++] = 1;
		array[num4++] = 1;
		array[num4++] = 2;
		array[num4++] = 0;
		array[num4++] = 60;
		array[num4++] = 62;
		array[num4++] = 61;
		array[num4++] = 61;
		array[num4++] = 63;
		array[num4++] = 60;
		if (list2.Count < 3)
		{
			return;
		}
		if (array.Length < 3)
		{
			return;
		}
		if (this.mesh == null)
		{
			this.mesh = new Mesh();
		}
		this.mesh.SetVertices(list2);
		this.mesh.uv = list4.ToArray();
		this.mesh.uv2 = list5.ToArray();
		this.mesh.SetNormals(list3);
		this.mesh.SetIndices(array, 0, 0);
		this.mesh.RecalculateBounds();
		this.meshFilter.mesh = this.mesh;
		this.meshCollider.sharedMesh = this.mesh;
		if (this.prevWireColor != this.wireColor)
		{
			this.prevWireColor = this.wireColor;
			this.SetWireColor(this.wireColor);
		}
	}

	public void SetWireColor(Color color)
	{
		if (this.meshRenderer.material == null)
		{
			return;
		}
		this.meshRenderer.material.SetColor("_Color", color);
		this.wireColor = color;
	}

	public void SetPulseSpeed(float speed)
	{
		if (this.meshRenderer.material == null)
		{
			return;
		}
		this.meshRenderer.material.SetFloat("_PulseSpeed", speed);
	}

	public void SetPulseColor(Color color)
	{
		this.pulseColor = color;
	}

	public void TogglePulse(bool isOn)
	{
		if (this.meshRenderer.material == null)
		{
			return;
		}
		this.meshRenderer.material.SetColor("_PulseColor", isOn ? this.pulseColor : this.wireColor);
	}

	public void SetStartPosition(Vector3 pos)
	{
		this.StartPosition = pos;
	}

	public void SetStartPositionOffset(Vector3 pos)
	{
		this.StartOffset = pos;
	}

	public void SetEndPosition(Vector3 pos)
	{
		this.EndPosition = pos;
	}

	public void SetEndPositionOffset(Vector3 pos)
	{
		this.EndOffset = pos;
	}

	public void SetWireDip(float _dist)
	{
		this.maxWireDip = _dist;
	}

	public float GetWireDip()
	{
		return this.maxWireDip;
	}

	public void SetWireRadius(float _radius)
	{
		this.wireRadius = _radius;
	}

	public void SetWireCanHide(bool _canHide)
	{
		this.canHide = _canHide;
	}

	public Vector3 GetStartPosition()
	{
		return this.StartPosition;
	}

	public Vector3 GetStartPositionOffset()
	{
		return this.StartOffset;
	}

	public Vector3 GetEndPosition()
	{
		return this.EndPosition;
	}

	public Vector3 GetEndPositionOffset()
	{
		return this.EndOffset;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public void SetVisible(bool _visible)
	{
		if (this.canHide)
		{
			base.gameObject.SetActive(_visible);
			return;
		}
		base.gameObject.SetActive(true);
	}

	public Bounds GetBounds()
	{
		return this.mesh.bounds;
	}

	public void Reset()
	{
		this.maxWireDip = 0.25f;
		this.wireRadius = 0.01f;
		this.pulseColor = Color.yellow;
	}

	public const int cLayerMaskRayCast = 65537;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int NODE_COUNT = 15;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float BASE_WIRE_RADIUS = 0.01f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float BASE_MIN_WIRE_DIP = 0f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float BASE_MAX_WIRE_DIP = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static Material BaseMaterial;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float maxWireDip = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float wireRadius = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 startPosition;

	public Vector3 StartOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 endPosition;

	public Vector3 EndOffset;

	public Color pulseColor = Color.yellow;

	public Color wireColor = Color.black;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canHide = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Mesh mesh;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshFilter meshFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshCollider meshCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshRenderer meshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color prevWireColor = Color.white;
}

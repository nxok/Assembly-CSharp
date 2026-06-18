using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugDrawNormals : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.die)
		{
			Object.DestroyImmediate(this);
			return;
		}
		this.TriangleCount = 0;
		this.VertCount = 0;
		MeshFilter component = base.GetComponent<MeshFilter>();
		if (component)
		{
			this.MeshCount = 1;
			this.Draw(component);
			return;
		}
		MeshFilter[] componentsInChildren = base.GetComponentsInChildren<MeshFilter>();
		this.MeshCount = componentsInChildren.Length;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this.Draw(componentsInChildren[i]);
		}
		if (componentsInChildren.Length == 0)
		{
			this.SetDie();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Draw(MeshFilter mf)
	{
		Mesh sharedMesh = mf.sharedMesh;
		if (!sharedMesh)
		{
			return;
		}
		DebugDrawNormals.Data data;
		if (this.list.Count > 0 && !this.Record)
		{
			data = this.list[0];
		}
		else
		{
			data = new DebugDrawNormals.Data();
			this.list.Add(data);
			this.Record = false;
		}
		sharedMesh.GetVertices(data.verts);
		sharedMesh.GetNormals(data.normals);
		sharedMesh.GetTriangles(data.indices, 0);
		this.VertCount += data.verts.Count;
		this.TriangleCount += data.indices.Count / 3;
		Matrix4x4 localToWorldMatrix = mf.transform.localToWorldMatrix;
		for (int i = 0; i < this.list.Count; i++)
		{
			DebugDrawNormals.Data data2 = this.list[i];
			for (int j = 0; j < data2.normals.Count; j++)
			{
				Utils.DrawRay(localToWorldMatrix.MultiplyPoint(data2.verts[j]), localToWorldMatrix.MultiplyVector(data2.normals[j]) * this.VertexNormalScale, Color.white, Color.blue, 3, 0f);
			}
			for (int k = 0; k < data2.indices.Count - 2; k += 3)
			{
				Vector3 vector = data2.verts[data2.indices[k]];
				Vector3 vector2 = data2.verts[data2.indices[k + 1]];
				Vector3 vector3 = data2.verts[data2.indices[k + 2]];
				Vector3 vector4 = (vector + vector2 + vector3) * 0.33333334f;
				Vector3 normalized = Vector3.Cross(vector2 - vector, vector3 - vector).normalized;
				Utils.DrawRay(localToWorldMatrix.MultiplyPoint(vector4), localToWorldMatrix.MultiplyVector(normalized) * this.TriangleNormalScale, Color.yellow, Color.red, 3, 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			this.SetDie();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDie()
	{
		this.list = null;
		this.die = true;
	}

	public float VertexNormalScale = 0.05f;

	public float TriangleNormalScale = 0.05f;

	public int MeshCount;

	public int TriangleCount;

	public int VertCount;

	public bool Record;

	public List<DebugDrawNormals.Data> list = new List<DebugDrawNormals.Data>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool die;

	[Serializable]
	public class Data
	{
		public List<Vector3> verts = new List<Vector3>();

		public List<Vector3> normals = new List<Vector3>();

		public List<int> indices = new List<int>();
	}
}

using System;
using UnityEngine;

public class SkinnedCollisionHelper : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		SkinnedMeshRenderer skinnedMeshRenderer = base.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
		this.collide = base.GetComponent(typeof(MeshCollider)) as MeshCollider;
		if (this.collide != null && skinnedMeshRenderer != null)
		{
			Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
			this.mesh = new Mesh();
			this.mesh.vertices = sharedMesh.vertices;
			this.mesh.uv = sharedMesh.uv;
			this.mesh.triangles = sharedMesh.triangles;
			this.newVert = new Vector3[sharedMesh.vertices.Length];
			this.nodeWeights = new CWeightList[skinnedMeshRenderer.bones.Length];
			short num = 0;
			while ((int)num < skinnedMeshRenderer.bones.Length)
			{
				this.nodeWeights[(int)num] = new CWeightList();
				this.nodeWeights[(int)num].transform = skinnedMeshRenderer.bones[(int)num];
				num += 1;
			}
			num = 0;
			while ((int)num < sharedMesh.vertices.Length)
			{
				BoneWeight boneWeight = sharedMesh.boneWeights[(int)num];
				if (boneWeight.weight0 != 0f)
				{
					Vector3 vector = sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyPoint3x4(sharedMesh.vertices[(int)num]);
					this.nodeWeights[boneWeight.boneIndex0].weights.Add(new CVertexWeight((int)num, vector, boneWeight.weight0));
				}
				if (boneWeight.weight1 != 0f)
				{
					Vector3 vector = sharedMesh.bindposes[boneWeight.boneIndex1].MultiplyPoint3x4(sharedMesh.vertices[(int)num]);
					this.nodeWeights[boneWeight.boneIndex1].weights.Add(new CVertexWeight((int)num, vector, boneWeight.weight1));
				}
				if (boneWeight.weight2 != 0f)
				{
					Vector3 vector = sharedMesh.bindposes[boneWeight.boneIndex2].MultiplyPoint3x4(sharedMesh.vertices[(int)num]);
					this.nodeWeights[boneWeight.boneIndex2].weights.Add(new CVertexWeight((int)num, vector, boneWeight.weight2));
				}
				if (boneWeight.weight3 != 0f)
				{
					Vector3 vector = sharedMesh.bindposes[boneWeight.boneIndex3].MultiplyPoint3x4(sharedMesh.vertices[(int)num]);
					this.nodeWeights[boneWeight.boneIndex3].weights.Add(new CVertexWeight((int)num, vector, boneWeight.weight3));
				}
				num += 1;
			}
			this.UpdateCollisionMesh();
			return;
		}
		Log.Error(base.gameObject.name + ": SkinnedCollisionHelper: this object either has no SkinnedMeshRenderer or has no MeshCollider!");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCollisionMesh()
	{
		if (this.mesh != null)
		{
			for (int i = 0; i < this.newVert.Length; i++)
			{
				this.newVert[i] = new Vector3(0f, 0f, 0f);
			}
			foreach (CWeightList cweightList in this.nodeWeights)
			{
				foreach (object obj in cweightList.weights)
				{
					CVertexWeight cvertexWeight = (CVertexWeight)obj;
					this.newVert[cvertexWeight.index] += cweightList.transform.localToWorldMatrix.MultiplyPoint3x4(cvertexWeight.localPosition) * cvertexWeight.weight;
				}
			}
			for (int k = 0; k < this.newVert.Length; k++)
			{
				this.newVert[k] = base.transform.InverseTransformPoint(this.newVert[k]);
			}
			this.mesh.vertices = this.newVert;
			this.mesh.RecalculateBounds();
			this.collide.sharedMesh = null;
			this.collide.sharedMesh = this.mesh;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (this.forceUpdate)
		{
			this.forceUpdate = false;
			this.UpdateCollisionMesh();
		}
	}

	public bool forceUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CWeightList[] nodeWeights;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3[] newVert;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Mesh mesh;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshCollider collide;
}

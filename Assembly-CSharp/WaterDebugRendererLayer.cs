using System;
using Unity.Mathematics;
using UnityEngine;

public class WaterDebugRendererLayer : IMemoryPoolableObject
{
	public bool IsInitialized
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeData()
	{
		this.transforms = new Matrix4x4[4096];
		this.colors = new float4[4096];
		this.normalizedMass = new float[4096];
		this.RegenerateTransforms();
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
		this.IsInitialized = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RegenerateTransforms()
	{
		Vector3 vector = Vector3.one * 0.9f;
		Vector3 vector2 = (Vector3.one - vector) * 0.5f;
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num = this.CoordToIndex(i, j, k);
					this.transforms[num] = Matrix4x4.TRS(this.layerOrigin + new Vector3((float)i, (float)j, (float)k) - Origin.position + vector2 + Vector3.one * 0.5f, Quaternion.identity, vector);
				}
			}
		}
		Vector3 vector3 = this.layerOrigin - Origin.position;
		Vector3 vector4 = this.layerOrigin - Origin.position + new Vector3(16f, 16f, 16f);
		this.bounds = new Bounds((vector4 + vector3) / 2f, vector4 - vector3);
		this.transformsHaveChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _origin)
	{
		if (this.IsInitialized)
		{
			this.RegenerateTransforms();
		}
	}

	public void SetLayerOrigin(Vector3 _origin)
	{
		this.layerOrigin = _origin;
		if (this.IsInitialized)
		{
			this.RegenerateTransforms();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CoordToIndex(int _x, int _y, int _z)
	{
		return _x + 16 * _y + 256 * _z;
	}

	public void SetWater(int _x, int _y, int _z, float mass)
	{
		float num = mass / 19500f;
		if (!this.IsInitialized)
		{
			if (num < 0.01f)
			{
				return;
			}
			this.InitializeData();
		}
		int num2 = this.CoordToIndex(_x, _y, _z);
		if (this.normalizedMass[num2] < 0.01f && num > 0.01f)
		{
			this.totalWater++;
		}
		else if (this.normalizedMass[num2] > 0.01f && num < 0.01f)
		{
			this.totalWater--;
		}
		this.normalizedMass[num2] = num;
		float num3 = math.max((mass - 19500f) / 65535f, 0f);
		this.colors[num2] = math.lerp(WaterDebugRendererLayer.waterColor, WaterDebugRendererLayer.overfullColor, num3);
		this.massesHaveChanged = true;
	}

	public void Draw()
	{
		if (this.totalWater == 0)
		{
			return;
		}
		if (this.materialProperties == null)
		{
			this.materialProperties = new MaterialPropertyBlock();
			this.materialProperties.SetFloat("_ScaleCutoff", 0.01f);
		}
		if (this.transformsBuffer == null)
		{
			this.transformsBuffer = new ComputeBuffer(4096, 64);
			this.materialProperties.SetBuffer("_Transforms", this.transformsBuffer);
		}
		if (this.colorBuffer == null)
		{
			this.colorBuffer = new ComputeBuffer(4096, 16);
			this.materialProperties.SetBuffer("_Colors", this.colorBuffer);
		}
		if (this.transformsHaveChanged)
		{
			this.transformsBuffer.SetData(this.transforms);
			this.transformsHaveChanged = false;
		}
		if (this.massBuffer == null)
		{
			this.massBuffer = new ComputeBuffer(4096, 4);
			this.materialProperties.SetBuffer("_Scales", this.massBuffer);
		}
		if (this.massesHaveChanged)
		{
			this.massBuffer.SetData(this.normalizedMass);
			this.colorBuffer.SetData(this.colors);
			this.massesHaveChanged = false;
		}
		Graphics.DrawMeshInstancedProcedural(WaterDebugAssets.CubeMesh, 0, WaterDebugAssets.DebugMaterial, this.bounds, 4096, this.materialProperties, 1, true, 0, null, 1, null);
	}

	public void Reset()
	{
		if (this.IsInitialized)
		{
			Origin.OriginChanged = (Action<Vector3>)Delegate.Remove(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
			this.transforms = null;
			this.normalizedMass = null;
			this.IsInitialized = false;
		}
		this.totalWater = 0;
		this.transformsHaveChanged = false;
		this.massesHaveChanged = false;
	}

	public void Cleanup()
	{
		if (this.transformsBuffer != null)
		{
			this.transformsBuffer.Dispose();
			this.transformsBuffer = null;
		}
		if (this.massBuffer != null)
		{
			this.massBuffer.Dispose();
			this.massBuffer = null;
		}
		if (this.colorBuffer != null)
		{
			this.colorBuffer.Dispose();
			this.colorBuffer = null;
		}
	}

	public const int dimX = 16;

	public const int dimY = 16;

	public const int dimZ = 16;

	public const int elementsPerLayer = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SCALE_CUTOFF = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RENDER_SCALE = 0.9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float4 waterColor = new float4(0f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float4 overfullColor = new float4(1f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public MaterialPropertyBlock materialProperties;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 layerOrigin;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds bounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public Matrix4x4[] transforms;

	[PublicizedFrom(EAccessModifier.Private)]
	public float4[] colors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool transformsHaveChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] normalizedMass;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool massesHaveChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public ComputeBuffer transformsBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public ComputeBuffer massBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public ComputeBuffer colorBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalWater;
}

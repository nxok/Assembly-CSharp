using System;
using UnityEngine;

public class UpdateLightOnChunkMesh : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (GameManager.IsDedicatedServer)
		{
			base.enabled = false;
			return;
		}
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		this.checkLight();
	}

	public void SetChunkMesh(VoxelMesh _chunkMesh)
	{
		this.chunkMesh = _chunkMesh;
	}

	public void Reset()
	{
		this.chunkMesh = null;
		this.lastTimeLightBrightnessChecked = 0f;
		this.lastSunLight = (this.lastBlockLight = byte.MaxValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (Time.time - this.lastTimeLightBrightnessChecked < this.cTimeBrightnessUpdate)
		{
			return;
		}
		this.lastTimeLightBrightnessChecked = Time.time;
		this.checkLight();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkLight()
	{
		if (this.chunkMesh == null)
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		if (!instance || !instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		World world = instance.World;
		if (world == null)
		{
			return;
		}
		if (this.meshFilter == null)
		{
			this.meshFilter = base.transform.GetComponent<MeshFilter>();
		}
		if (this.meshRenderer == null)
		{
			this.meshRenderer = base.transform.GetComponent<MeshRenderer>();
		}
		if (this.meshFilter == null || this.meshRenderer == null)
		{
			return;
		}
		byte b;
		byte b2;
		world.GetSunAndBlockColors(World.worldToBlockPos(base.transform.position + Origin.position), out b, out b2);
		byte b3;
		byte b4;
		world.GetSunAndBlockColors(World.worldToBlockPos(base.transform.position + Vector3.up + Origin.position), out b3, out b4);
		byte b5 = Utils.FastMax(b, b3);
		byte b6 = Utils.FastMax(b2, b4);
		Color color = (world.IsDaytime() ? world.m_WorldEnvironment.GetSunLightColor() : world.m_WorldEnvironment.GetMoonLightColor());
		color.a = 1f;
		if (b5 != this.lastSunLight || b6 != this.lastBlockLight || !color.Equals(this.lastSunMoonLight))
		{
			this.lastSunLight = b5;
			this.lastBlockLight = b6;
			this.lastSunMoonLight = color;
			this.meshFilter.mesh.colors = this.chunkMesh.UpdateColors(b5, b6);
			this.meshRenderer.material.SetColor("_SunMoonlight", color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeLightBrightnessChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cTimeBrightnessUpdate = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public byte lastSunLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public byte lastBlockLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color lastSunMoonLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshFilter meshFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshRenderer meshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public VoxelMesh chunkMesh;
}

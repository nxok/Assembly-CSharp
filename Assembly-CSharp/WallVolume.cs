using System;
using System.IO;
using PrefabVolumes;
using UnityEngine;

public class WallVolume
{
	public static WallVolume Create(PrefabWallVolume psv, Vector3i _boxMin, Vector3i _boxMax)
	{
		WallVolume wallVolume = new WallVolume();
		wallVolume.SetMinMax(_boxMin, _boxMax);
		wallVolume.AddToPrefabInstance();
		return wallVolume;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMinMax(Vector3i _boxMin, Vector3i _boxMax)
	{
		this.BoxMin = _boxMin;
		this.BoxMax = _boxMax;
		this.Center = (this.BoxMin + this.BoxMax).ToVector3() * 0.5f;
	}

	public PrefabInstance PrefabInstance
	{
		get
		{
			return this.prefabInstance;
		}
	}

	public bool AddToPrefabInstance()
	{
		this.prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().FindVolumeOwner(PrefabVolumeAbs.EVolumeType.Wall, this.BoxMin, this.BoxMax);
		if (this.prefabInstance != null)
		{
			this.prefabInstance.AddWallVolume(this);
			return true;
		}
		return false;
	}

	public static WallVolume Read(BinaryReader _br)
	{
		WallVolume wallVolume = new WallVolume();
		_br.ReadByte();
		wallVolume.SetMinMax(new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()), new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()));
		return wallVolume;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(1);
		_bw.Write(this.BoxMin.x);
		_bw.Write(this.BoxMin.y);
		_bw.Write(this.BoxMin.z);
		_bw.Write(this.BoxMax.x);
		_bw.Write(this.BoxMax.y);
		_bw.Write(this.BoxMax.z);
	}

	public const int BinarySize = 25;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte VERSION = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance prefabInstance;

	public Vector3i BoxMin;

	public Vector3i BoxMax;

	public Vector3 Center;
}

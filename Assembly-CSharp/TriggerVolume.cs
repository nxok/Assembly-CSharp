using System;
using System.Collections.Generic;
using System.IO;
using PrefabVolumes;
using UnityEngine;

public class TriggerVolume
{
	public static TriggerVolume Create(PrefabTriggerVolume psv, Vector3i _boxMin, Vector3i _boxMax)
	{
		TriggerVolume triggerVolume = new TriggerVolume();
		triggerVolume.SetMinMax(_boxMin, _boxMax);
		triggerVolume.TriggersIndices = new List<byte>(psv.TriggersIndices);
		triggerVolume.AddToPrefabInstance();
		return triggerVolume;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMinMax(Vector3i _boxMin, Vector3i _boxMax)
	{
		this.BoxMin = _boxMin;
		this.BoxMax = _boxMax;
		this.Center = ((this.BoxMin + this.BoxMax) * 0.5f).ToVector3();
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
		this.prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().FindVolumeOwner(PrefabVolumeAbs.EVolumeType.Trigger, this.BoxMin, this.BoxMax);
		if (this.prefabInstance != null)
		{
			this.prefabInstance.AddTriggerVolume(this);
			return true;
		}
		return false;
	}

	public void Reset()
	{
		this.isTriggered = false;
	}

	public bool HasAnyTriggers()
	{
		return this.TriggersIndices.Count > 0;
	}

	public void CheckTouching(World _world, EntityPlayer _player)
	{
		if (this.isTriggered)
		{
			return;
		}
		Vector3 position = _player.position;
		position.y += 0.8f;
		if (position.x >= (float)this.BoxMin.x && position.x < (float)this.BoxMax.x && position.y >= (float)this.BoxMin.y && position.y < (float)this.BoxMax.y && position.z >= (float)this.BoxMin.z && position.z < (float)this.BoxMax.z)
		{
			this.Touch(_world, _player);
		}
	}

	public bool Intersects(Bounds bounds)
	{
		return BoundsUtils.Intersects(bounds, this.BoxMin, this.BoxMax);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Touch(World _world, EntityPlayer _player)
	{
		this.isTriggered = true;
		_world.triggerManager.TriggerBlocks(_player, this.prefabInstance, this);
	}

	public static TriggerVolume Read(BinaryReader _br)
	{
		TriggerVolume triggerVolume = new TriggerVolume();
		int num = (int)_br.ReadByte();
		triggerVolume.SetMinMax(new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()), new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()));
		int num2 = (int)_br.ReadByte();
		triggerVolume.TriggersIndices.Clear();
		for (int i = 0; i < num2; i++)
		{
			triggerVolume.TriggersIndices.Add(_br.ReadByte());
		}
		if (num > 1)
		{
			triggerVolume.isTriggered = _br.ReadBoolean();
		}
		return triggerVolume;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(2);
		_bw.Write(this.BoxMin.x);
		_bw.Write(this.BoxMin.y);
		_bw.Write(this.BoxMin.z);
		_bw.Write(this.BoxMax.x);
		_bw.Write(this.BoxMax.y);
		_bw.Write(this.BoxMax.z);
		_bw.Write((byte)this.TriggersIndices.Count);
		for (int i = 0; i < this.TriggersIndices.Count; i++)
		{
			_bw.Write(this.TriggersIndices[i]);
		}
		_bw.Write(this.isTriggered);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawVolume()
	{
		Vector3 vector = this.BoxMin.ToVector3();
		vector -= Origin.position;
		Vector3 vector2 = this.BoxMax.ToVector3();
		vector2 -= Origin.position;
		Debug.DrawLine(vector, new Vector3(vector.x, vector.y, vector2.z), Color.blue, 1f);
		Debug.DrawLine(vector, new Vector3(vector2.x, vector.y, vector.z), Color.blue, 1f);
		Debug.DrawLine(new Vector3(vector.x, vector.y, vector2.z), new Vector3(vector2.x, vector.y, vector2.z), Color.blue, 1f);
		Debug.DrawLine(new Vector3(vector2.x, vector.y, vector.z), new Vector3(vector2.x, vector.y, vector2.z), Color.blue, 1f);
		Debug.DrawLine(new Vector3(vector.x, vector2.y, vector.z), new Vector3(vector.x, vector2.y, vector2.z), Color.cyan, 1f);
		Debug.DrawLine(new Vector3(vector.x, vector2.y, vector.z), new Vector3(vector2.x, vector2.y, vector.z), Color.cyan, 1f);
		Debug.DrawLine(new Vector3(vector.x, vector2.y, vector2.z), new Vector3(vector2.x, vector2.y, vector2.z), Color.cyan, 1f);
		Debug.DrawLine(new Vector3(vector2.x, vector2.y, vector.z), new Vector3(vector2.x, vector2.y, vector2.z), Color.cyan, 1f);
	}

	public void DrawDebugLines(float _duration)
	{
		string text = string.Format("TriggerVolume{0},{1}", this.BoxMin, this.BoxMax);
		Color color;
		color..ctor(0.1f, 0.1f, 1f);
		if (this.isTriggered)
		{
			color..ctor(0f, 0f, 0.5f, 0.16f);
		}
		Vector3 vector = this.BoxMin.ToVector3();
		Vector3 vector2 = this.BoxMax.ToVector3();
		vector += DebugLines.InsideOffsetV * 2f;
		vector2 -= DebugLines.InsideOffsetV * 2f;
		DebugLines.Create(text, GameManager.Instance.World.GetPrimaryPlayer().RootTransform, color, color, 0.03f, 0.03f, _duration).AddCube(vector, vector2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte VERSION = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerYOffset = 0.8f;

	public static Vector3i chunkPadding = new Vector3i(12, 1, 12);

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance prefabInstance;

	public List<byte> TriggersIndices = new List<byte>();

	public Vector3i BoxMin;

	public Vector3i BoxMax;

	public Vector3 Center;

	public bool isTriggered;
}

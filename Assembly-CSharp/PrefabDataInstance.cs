using System;
using UnityEngine;

public class PrefabDataInstance
{
	public Vector3i boundingBoxSize
	{
		get
		{
			return this.prefab.size;
		}
	}

	public Vector2i CenterXZ
	{
		get
		{
			Vector2i vector2i;
			vector2i.x = this.boundingBoxPosition.x + this.prefab.size.x / 2;
			vector2i.y = this.boundingBoxPosition.z + this.prefab.size.z / 2;
			return vector2i;
		}
	}

	public Vector2 CenterXZV2
	{
		get
		{
			Vector2 vector;
			vector.x = (float)this.boundingBoxPosition.x + (float)this.prefab.size.x * 0.5f;
			vector.y = (float)this.boundingBoxPosition.z + (float)this.prefab.size.z * 0.5f;
			return vector;
		}
	}

	public PathAbstractions.AbstractedLocation location
	{
		get
		{
			return this.prefab.location;
		}
	}

	public PrefabDataInstance(int _id, Vector3i _position, byte _rotation, PrefabData _prefabData)
	{
		this.id = _id;
		this.prefab = _prefabData;
		this.boundingBoxPosition = _position;
		this.rotation = _rotation;
		this.previewColor = PrefabDataInstance.previewColorDefault;
	}

	public int id;

	public PrefabData prefab;

	public Vector3i boundingBoxPosition;

	public byte rotation;

	public Color32 previewColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 previewColorDefault = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
}

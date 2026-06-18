using System;
using UnityEngine;

public class PropReference : MonoBehaviour
{
	public Vector2i ChunkPos
	{
		get
		{
			return new Vector2i(this.ChunkX, this.ChunkZ);
		}
		set
		{
			this.ChunkX = value.x;
			this.ChunkZ = value.y;
		}
	}

	public PropRef PropRef
	{
		get
		{
			return new PropRef
			{
				ChunkPos = new Vector2i(this.ChunkX, this.ChunkZ),
				PropId = this.PropId
			};
		}
		set
		{
			this.ChunkX = value.ChunkPos.x;
			this.ChunkZ = value.ChunkPos.y;
			this.PropId = value.PropId;
		}
	}

	public override string ToString()
	{
		return this.PropRef.ToString();
	}

	public int ChunkX;

	public int ChunkZ;

	public int PropId;
}

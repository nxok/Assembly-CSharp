using System;
using System.Threading;
using UnityEngine;

public class VoxelMeshLayer : IMemoryPoolableObject
{
	public VoxelMeshLayer()
	{
		this.idx = 0;
		this.meshes = new VoxelMesh[MeshDescription.meshes.Length];
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			this.meshes[i] = VoxelMesh.Create(i, MeshDescription.meshes[i].meshType, 0);
		}
		Interlocked.Increment(ref VoxelMeshLayer.InstanceCount);
	}

	public void Reset()
	{
		for (int i = 0; i < this.meshes.Length; i++)
		{
			this.meshes[i].ClearMesh();
		}
		if (this.pendingCopyOperations != 0)
		{
			Log.Error("Resetting VML with pendingCopyOperations");
		}
		this.pendingCopyOperations = 0;
	}

	public void Cleanup()
	{
		if (this.cleanedUp)
		{
			return;
		}
		Interlocked.Decrement(ref VoxelMeshLayer.InstanceCount);
		this.cleanedUp = true;
	}

	public static void StaticCleanup()
	{
		VoxelMeshLayer.InstanceCount = 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~VoxelMeshLayer()
	{
		if (!this.cleanedUp)
		{
			Log.Out(string.Format("VoxelMeshLayer {0} finalized without being cleaned up.", this.idx));
		}
	}

	public void SizeToChunkDefaults()
	{
		for (int i = 0; i < this.meshes.Length; i++)
		{
			this.meshes[i].SizeToChunkDefaults(i);
		}
	}

	public bool HasContent()
	{
		for (int i = 0; i < this.meshes.Length; i++)
		{
			if (this.meshes[i].Vertices.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetTris()
	{
		int num = 0;
		for (int i = 0; i < this.meshes.Length; i++)
		{
			num += this.meshes[i].Triangles;
		}
		return num;
	}

	public int GetTrisInMesh(int _idx)
	{
		return this.meshes[_idx].Triangles;
	}

	public int GetSizeOfMesh(int _idx)
	{
		return this.meshes[_idx].Size;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public int CopyToMesh(int _meshIdx, MeshFilter _mf, MeshRenderer _mr, int _lodLevel)
	{
		this.pendingCopyOperations++;
		return this.meshes[_meshIdx].CopyToMesh(_mf, _mr, _lodLevel, delegate
		{
			this.TryFree();
		});
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void StartCopyMeshes()
	{
		if (this.pendingCopyOperations != 0)
		{
			Log.Error("StartCopyMeshes called with pendingCopyOperations != 0");
		}
		this.pendingCopyOperations++;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void EndCopyMeshes()
	{
		this.TryFree();
	}

	public bool TryFree()
	{
		this.pendingCopyOperations--;
		if (this.pendingCopyOperations > 0)
		{
			return false;
		}
		MemoryPools.poolVML.FreeSync(this);
		return true;
	}

	public int idx;

	public VoxelMesh[] meshes;

	public static int InstanceCount;

	public int pendingCopyOperations;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cleanedUp;
}

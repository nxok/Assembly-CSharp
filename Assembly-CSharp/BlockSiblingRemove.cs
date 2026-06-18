using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSiblingRemove : Block
{
	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("SiblingDirection"))
		{
			this.siblingDirection = new Vector3i(StringParsers.ParseVector3(base.Properties.Values["SiblingDirection"], 0, -1));
		}
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		Vector3i vector3i = this.siblingDirection;
		if (vector3i.Equals(Vector3i.zero))
		{
			if (_world.GetBlock(new Vector3i(_blockPos.x + 1, _blockPos.y, _blockPos.z)).Equals(this.SiblingBlock))
			{
				vector3i = new Vector3i(1, 0, 0);
			}
			else if (_world.GetBlock(new Vector3i(_blockPos.x, _blockPos.y, _blockPos.z + 1)).Equals(this.SiblingBlock))
			{
				vector3i = new Vector3i(0, 0, 1);
			}
			else if (_world.GetBlock(new Vector3i(_blockPos.x - 1, _blockPos.y, _blockPos.z)).Equals(this.SiblingBlock))
			{
				vector3i = new Vector3i(-1, 0, 0);
			}
			else if (_world.GetBlock(new Vector3i(_blockPos.x, _blockPos.y, _blockPos.z - 1)).Equals(this.SiblingBlock))
			{
				vector3i = new Vector3i(0, 0, -1);
			}
			((World)_world).SetBlock(_blockPos + vector3i, BlockValue.Air, false, true);
			return;
		}
		Vector3 vector = vector3i.ToVector3();
		switch (_blockValue.rotation)
		{
		case 0:
			vector = Quaternion.AngleAxis(180f, Vector3.up) * vector;
			break;
		case 1:
			vector = Quaternion.AngleAxis(270f, Vector3.up) * vector;
			break;
		case 3:
			vector = Quaternion.AngleAxis(90f, Vector3.up) * vector;
			break;
		}
		vector3i = default(Vector3i);
		vector3i.RoundToInt(vector);
		if (!vector3i.Equals(Vector3i.zero) && _world.GetBlock(_blockPos + vector3i).Equals(this.SiblingBlock))
		{
			((World)_world).SetBlock(_blockPos + vector3i, BlockValue.Air, false, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i siblingDirection = Vector3i.zero;
}

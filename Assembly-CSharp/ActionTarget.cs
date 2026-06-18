using System;
using System.IO;
using UnityEngine;

public struct ActionTarget : IEquatable<ActionTarget>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ActionTarget(ActionTargetType type, Vector3 position, BlockValueRef blockValueReference)
	{
		this.Type = type;
		this.Position = position;
		this.BlockValueReference = blockValueReference;
	}

	public ActionTarget(float x, float y, float z)
	{
		this = new ActionTarget(new Vector3(x, y, z));
	}

	public ActionTarget(Vector3 pos)
	{
		this = new ActionTarget(ActionTargetType.Position, pos, BlockValueRef.None);
	}

	public ActionTarget(int x, int y, int z)
	{
		this = new ActionTarget(new BlockValueRef(x, y, z));
	}

	public ActionTarget(Vector3i pos)
	{
		this = new ActionTarget(new BlockValueRef(pos));
	}

	public ActionTarget(BlockValueRef bvRef)
	{
		this = new ActionTarget(ActionTargetType.BlockValueRef, Vector3.zero, bvRef);
	}

	public static ActionTarget Read(BinaryReader br)
	{
		ActionTarget actionTarget;
		switch (br.ReadByte())
		{
		case 0:
			actionTarget = ActionTarget.None;
			break;
		case 1:
			actionTarget = new ActionTarget(StreamUtils.ReadVector3(br));
			break;
		case 2:
			actionTarget = new ActionTarget(BlockValueRef.Read(br));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return actionTarget;
	}

	public readonly void Write(BinaryWriter bw)
	{
		bw.Write((byte)this.Type);
		switch (this.Type)
		{
		case ActionTargetType.None:
			return;
		case ActionTargetType.Position:
			StreamUtils.Write(bw, this.Position);
			return;
		case ActionTargetType.BlockValueRef:
			this.BlockValueReference.Write(bw);
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static implicit operator Vector3(ActionTarget target)
	{
		if (target.Type == ActionTargetType.Position)
		{
			return target.Position;
		}
		Vector3i vector3i;
		if (target.Type == ActionTargetType.BlockValueRef && target.BlockValueReference.TryGetBlockPos(out vector3i))
		{
			return vector3i;
		}
		Log.Warning(string.Format("[PROPS] ActionTarget implicitly converted to Vector3, but failed: {0}", target));
		return Vector3.zero;
	}

	public static implicit operator Vector3i(ActionTarget target)
	{
		Vector3i vector3i;
		if (target.Type == ActionTargetType.BlockValueRef && target.BlockValueReference.TryGetBlockPos(out vector3i))
		{
			return vector3i;
		}
		Log.Warning(string.Format("[PROPS] ActionTarget implicitly converted to Vector3i, but failed: {0}", target));
		return Vector3i.zero;
	}

	public static implicit operator BlockValueRef(ActionTarget target)
	{
		if (target.Type == ActionTargetType.BlockValueRef)
		{
			return target.BlockValueReference;
		}
		Log.Warning(string.Format("[PROPS] ActionTarget implicitly converted to BlockValueRef, but failed: {0}", target));
		return BlockValueRef.None;
	}

	public static implicit operator ActionTarget(Vector3 pos)
	{
		return new ActionTarget(pos);
	}

	public static implicit operator ActionTarget(Vector3i pos)
	{
		return new ActionTarget(new BlockValueRef(pos));
	}

	public static implicit operator ActionTarget(PropRef propRef)
	{
		return new ActionTarget(new BlockValueRef(propRef));
	}

	public static implicit operator ActionTarget(BlockValueRef bvRef)
	{
		return new ActionTarget(bvRef);
	}

	public bool Equals(ActionTarget other)
	{
		return this.Type == other.Type && this.Position.Equals(other.Position) && this.BlockValueReference.Equals(other.BlockValueReference);
	}

	public override bool Equals(object obj)
	{
		if (obj is ActionTarget)
		{
			ActionTarget actionTarget = (ActionTarget)obj;
			return this.Equals(actionTarget);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<int, Vector3, BlockValueRef>((int)this.Type, this.Position, this.BlockValueReference);
	}

	public static bool operator ==(ActionTarget left, ActionTarget right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ActionTarget left, ActionTarget right)
	{
		return !left.Equals(right);
	}

	public static readonly ActionTarget None = new ActionTarget(ActionTargetType.None, Vector3.zero, BlockValueRef.None);

	public ActionTargetType Type;

	public Vector3 Position;

	public BlockValueRef BlockValueReference;
}

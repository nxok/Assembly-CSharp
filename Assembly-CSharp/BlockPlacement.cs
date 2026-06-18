using System;
using UnityEngine;

public class BlockPlacement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool supports45DegreeRotations(BlockValue _bv)
	{
		Block block = _bv.Block;
		return (!block.isMultiBlock || (block.multiBlockPos.dim.x == 1 && block.multiBlockPos.dim.z == 1)) && (block.AllowedRotations & EBlockRotationClasses.Basic45) > EBlockRotationClasses.None;
	}

	public virtual BlockPlacement.Result OnPlaceBlock(BlockPlacement.EnumPlacement _placement, BlockPlacement.EnumRotationMode _mode, int _localRot, WorldBase _world, BlockValue _bv, PropTransform _propTransform, HitInfoDetails _hitInfo, Vector3 _entityPos)
	{
		Block block = _bv.Block;
		BlockPlacement.Result result = new BlockPlacement.Result(_placement, _bv, _propTransform, _hitInfo);
		bool flag = this.supports45DegreeRotations(_bv);
		if (_hitInfo.blockFace != BlockFace.Top && _hitInfo.voxelData.BlockValue.Block.CanBlocksReplaceOrGroundCover())
		{
			_hitInfo.blockFace = BlockFace.Top;
		}
		switch (_mode)
		{
		case BlockPlacement.EnumRotationMode.ToFace:
			if (!flag || _hitInfo.blockFace != BlockFace.Top || block.HandleFace != BlockFace.None)
			{
				int num = _localRot;
				Quaternion quaternion = Quaternion.identity;
				if (block.HandleFace != BlockFace.None)
				{
					switch (block.HandleFace)
					{
					case BlockFace.Top:
						quaternion = Quaternion.AngleAxis(180f, Vector3.right);
						break;
					case BlockFace.North:
						quaternion = Quaternion.AngleAxis(90f, Vector3.right);
						break;
					case BlockFace.West:
						quaternion = Quaternion.AngleAxis(90f, Vector3.forward);
						break;
					case BlockFace.South:
						quaternion = Quaternion.AngleAxis(-90f, Vector3.right);
						break;
					case BlockFace.East:
						quaternion = Quaternion.AngleAxis(-90f, Vector3.forward);
						break;
					}
				}
				result.blockValue.rotation = (byte)(_hitInfo.blockFace << 2);
				result.blockValue.rotation = (byte)BlockShapeNew.ConvertRotationFree((int)result.blockValue.rotation, quaternion, true);
				switch (_hitInfo.blockFace)
				{
				case BlockFace.North:
					num += 2;
					break;
				case BlockFace.West:
					num += 3;
					break;
				case BlockFace.South:
					num += 2;
					break;
				case BlockFace.East:
					num++;
					break;
				}
				Vector3 vector = Vector3.up;
				switch (_hitInfo.blockFace)
				{
				case BlockFace.Top:
					vector = Vector3.up;
					break;
				case BlockFace.Bottom:
					vector = Vector3.down;
					break;
				case BlockFace.North:
					vector = Vector3.forward;
					break;
				case BlockFace.West:
					vector = Vector3.left;
					break;
				case BlockFace.South:
					vector = Vector3.back;
					break;
				case BlockFace.East:
					vector = Vector3.right;
					break;
				}
				for (int i = 0; i < num; i++)
				{
					result.blockValue.rotation = (byte)BlockShapeNew.ConvertRotationFree((int)result.blockValue.rotation, Quaternion.AngleAxis(90f, vector), false);
				}
			}
			else if ((_bv.rotation >= 0 && _bv.rotation <= 3) || (_bv.rotation >= 24 && _bv.rotation <= 27))
			{
				result.blockValue.rotation = _bv.rotation;
			}
			else
			{
				result.blockValue.rotation = 0;
			}
			break;
		case BlockPlacement.EnumRotationMode.Simple:
			if (!flag)
			{
				result.blockValue.rotation = result.blockValue.rotation & 3;
			}
			else if ((_bv.rotation >= 0 && _bv.rotation <= 3) || (_bv.rotation >= 24 && _bv.rotation <= 27))
			{
				result.blockValue.rotation = _bv.rotation;
			}
			else
			{
				result.blockValue.rotation = 0;
			}
			break;
		}
		return result;
	}

	public virtual byte LimitRotation(BlockPlacement.EnumRotationMode _mode, ref int _localRot, HitInfoDetails _hitInfo, bool _bAdd, BlockValue _bv, byte _rotation)
	{
		bool flag = this.supports45DegreeRotations(_bv);
		Block block = _bv.Block;
		switch (_mode)
		{
		case BlockPlacement.EnumRotationMode.ToFace:
			if (!flag || (_rotation >= 4 && _rotation <= 23) || block.HandleFace != BlockFace.None)
			{
				Vector3 vector = Vector3.up;
				switch (_hitInfo.blockFace)
				{
				case BlockFace.Top:
					vector = Vector3.up;
					break;
				case BlockFace.Bottom:
					vector = Vector3.down;
					break;
				case BlockFace.North:
					vector = Vector3.forward;
					break;
				case BlockFace.West:
					vector = Vector3.left;
					break;
				case BlockFace.South:
					vector = Vector3.back;
					break;
				case BlockFace.East:
					vector = Vector3.right;
					break;
				}
				_localRot = (_localRot + (_bAdd ? 1 : (-1))) & 3;
				return (byte)BlockShapeNew.ConvertRotationFree((int)_bv.rotation, Quaternion.AngleAxis(90f, vector), false);
			}
			switch (_rotation)
			{
			case 0:
				if (_bAdd)
				{
					return 24;
				}
				return 27;
			case 1:
				if (_bAdd)
				{
					return 25;
				}
				return 24;
			case 2:
				if (_bAdd)
				{
					return 26;
				}
				return 25;
			case 3:
				if (_bAdd)
				{
					return 27;
				}
				return 26;
			default:
				switch (_rotation)
				{
				case 24:
					if (_bAdd)
					{
						return 1;
					}
					return 0;
				case 25:
					if (_bAdd)
					{
						return 2;
					}
					return 1;
				case 26:
					if (_bAdd)
					{
						return 3;
					}
					return 2;
				case 27:
					if (_bAdd)
					{
						return 0;
					}
					return 3;
				default:
					return 0;
				}
				break;
			}
			break;
		case BlockPlacement.EnumRotationMode.Simple:
			if (!flag)
			{
				return (byte)(((int)_rotation + (_bAdd ? 1 : (-1))) & 3);
			}
			switch (_rotation)
			{
			case 0:
				if (_bAdd)
				{
					return 24;
				}
				return 27;
			case 1:
				if (_bAdd)
				{
					return 25;
				}
				return 24;
			case 2:
				if (_bAdd)
				{
					return 26;
				}
				return 25;
			case 3:
				if (_bAdd)
				{
					return 27;
				}
				return 26;
			default:
				switch (_rotation)
				{
				case 24:
					if (_bAdd)
					{
						return 1;
					}
					return 0;
				case 25:
					if (_bAdd)
					{
						return 2;
					}
					return 1;
				case 26:
					if (_bAdd)
					{
						return 3;
					}
					return 2;
				case 27:
					if (_bAdd)
					{
						return 0;
					}
					return 3;
				default:
					return 0;
				}
				break;
			}
			break;
		case BlockPlacement.EnumRotationMode.Advanced:
		{
			Block block2 = block;
			int num = (int)_rotation;
			bool flag2;
			do
			{
				num += (_bAdd ? 1 : (-1));
				if (num > 27)
				{
					num = 0;
				}
				else if (num < 0)
				{
					num = 27;
				}
				if (num < 4)
				{
					flag2 = (block2.AllowedRotations & EBlockRotationClasses.Basic90) > EBlockRotationClasses.None;
				}
				else if (num < 8)
				{
					flag2 = (block2.AllowedRotations & EBlockRotationClasses.Headfirst) > EBlockRotationClasses.None;
				}
				else if (num < 24)
				{
					flag2 = (block2.AllowedRotations & EBlockRotationClasses.Sideways) > EBlockRotationClasses.None;
				}
				else
				{
					flag2 = (block2.AllowedRotations & EBlockRotationClasses.Basic45) > EBlockRotationClasses.None;
				}
			}
			while (!flag2);
			return (byte)num;
		}
		default:
			return _rotation;
		}
	}

	public static BlockPlacement None = new BlockPlacement();

	public enum EnumPlacement
	{
		Voxel,
		Free
	}

	public enum EnumRotationMode
	{
		ToFace,
		Simple,
		Advanced,
		Auto
	}

	public struct Result
	{
		public Result(BlockPlacement.EnumPlacement _placement, Vector3 _pos, Vector3i _blockPos, BlockFace _blockFace, BlockValue _blockValue, PropTransform _propTransform)
		{
			this.placement = _placement;
			this.pos = _pos;
			this.blockPos = _blockPos;
			this.blockFace = _blockFace;
			this.blockValue = _blockValue;
			this.propTransform = _propTransform;
		}

		public Result(BlockPlacement.EnumPlacement _placement, BlockValue _blockValue, PropTransform _propTransform, HitInfoDetails _hitInfo)
		{
			this = new BlockPlacement.Result(_placement, _hitInfo.pos, _hitInfo.blockPos, _hitInfo.blockFace, _blockValue, _propTransform);
		}

		public BlockPlacement.EnumPlacement placement;

		public Vector3 pos;

		public Vector3i blockPos;

		public BlockFace blockFace;

		public BlockValue blockValue;

		public PropTransform propTransform;
	}
}

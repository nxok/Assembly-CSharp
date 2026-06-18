using System;
using System.IO;
using UnityEngine;

public class DecoObject : IEquatable<DecoObject>
{
	public void Init(Vector3i _pos, float _realYPos, BlockValue _bv, DecoState _state)
	{
		this.pos = _pos;
		this.realYPos = _realYPos;
		this.bv = _bv;
		this.state = _state;
		this.asyncItem = null;
		this.go = null;
	}

	public string GetModelName()
	{
		Block block = this.bv.Block;
		if (block == null)
		{
			Log.Error(string.Format("DecoObject '{0}', no block!", this.bv));
			return null;
		}
		string value = block.Properties.GetValue("Model");
		if (string.IsNullOrEmpty(value))
		{
			Log.Error("DecoObject block '" + block.GetBlockName() + "', no model!");
			return null;
		}
		return GameIO.GetFilenameFromPathWithoutExtension(value);
	}

	public void CreateGameObject(DecoChunk _decoChunk, Transform _parent)
	{
		string modelName = this.GetModelName();
		if (modelName != null)
		{
			GameObject objectForType = GameObjectPool.Instance.GetObjectForType(modelName);
			this.CreateGameObjectCallback(objectForType, _parent, false);
		}
	}

	public void CreateGameObjectCallback(GameObject _obj, Transform _parent, bool _isAsync)
	{
		this.go = _obj;
		if (_isAsync && this.asyncItem == null)
		{
			this.Destroy();
			return;
		}
		this.asyncItem = null;
		Block block = this.bv.Block;
		BlockShapeDistantDeco blockShapeDistantDeco = block.shape as BlockShapeDistantDeco;
		if (blockShapeDistantDeco == null)
		{
			Log.Error("Block '{0}' needs a deco shape assigned but has not!", new object[] { block.GetBlockName() });
			return;
		}
		Transform transform = this.go.transform;
		transform.SetParent(_parent, false);
		float y = blockShapeDistantDeco.modelOffset.y;
		transform.position = new Vector3((float)this.pos.x + DecoManager.cDecoMiddleOffset.x, this.realYPos + y, (float)this.pos.z + DecoManager.cDecoMiddleOffset.z) - Origin.position;
		int num = (int)this.bv.rotation;
		if (!blockShapeDistantDeco.Has45DegreeRotations)
		{
			num &= 3;
		}
		transform.localRotation = BlockShapeNew.GetRotationStatic(num);
		this.go.SetActive(true);
		BlockEntityData blockEntityData = new BlockEntityData();
		blockEntityData.transform = transform;
		blockShapeDistantDeco.OnBlockEntityTransformAfterActivated(null, this.pos, this.bv, blockEntityData);
	}

	public void Destroy()
	{
		this.asyncItem = null;
		if (this.go)
		{
			GameObjectPool.Instance.PoolObjectAsync(this.go);
			this.go = null;
		}
	}

	public void Write(BinaryWriter _bw, NameIdMapping _blockMap = null)
	{
		_bw.Write(GameUtils.Vector3iToUInt64(this.pos));
		_bw.Write(this.realYPos);
		_bw.Write(this.bv.rawData);
		_bw.Write((byte)this.state);
		Block block = this.bv.Block;
		if (block == null)
		{
			Log.Error(string.Format("Writing DecoObject '{0}', no block!", this.bv));
			return;
		}
		if (_blockMap != null)
		{
			_blockMap.AddMapping(block.blockID, block.GetBlockName(), false);
		}
	}

	public void Read(BinaryReader _br)
	{
		this.pos = GameUtils.UInt64ToVector3i(_br.ReadUInt64());
		this.realYPos = _br.ReadSingle();
		this.bv = new BlockValue(_br.ReadUInt32());
		this.state = (DecoState)_br.ReadByte();
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<int, DecoState, int>(this.pos.GetHashCode(), this.state, this.bv.GetHashCode());
	}

	public override bool Equals(object _obj)
	{
		if (_obj == null)
		{
			return false;
		}
		if (this == _obj)
		{
			return true;
		}
		DecoObject decoObject = _obj as DecoObject;
		return decoObject != null && this.Equals(decoObject);
	}

	public bool Equals(DecoObject _other)
	{
		return _other != null && (this == _other || (_other.pos.Equals(this.pos) && _other.state == this.state && _other.bv.Equals(this.bv)));
	}

	public Vector3i pos;

	public float realYPos;

	public BlockValue bv;

	public DecoState state;

	public GameObjectPool.AsyncItem asyncItem;

	public GameObject go;
}

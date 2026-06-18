using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageNavObject : NetPackage
{
	public NetPackageNavObject Setup(string _navObjectClass, string _displayName, Vector3 _position, bool _isAdd, bool _usingLocalizationId, int _entityId = -1)
	{
		this.navObjectClass = _navObjectClass;
		this.name = _displayName;
		this.position = _position;
		this.isAdd = _isAdd;
		this.usingLocalizationId = _usingLocalizationId;
		this.entityId = _entityId;
		return this;
	}

	public NetPackageNavObject Setup(int _entityId)
	{
		this.navObjectClass = "";
		this.name = "";
		this.position = Vector3.zero;
		this.isAdd = false;
		this.usingLocalizationId = false;
		this.entityId = _entityId;
		return this;
	}

	public NetPackageNavObject Setup(string _navObjectClass, string _displayName, Vector3 _position, bool _isAdd, Color _overrideColor, bool _usingLocalizationId)
	{
		this.navObjectClass = _navObjectClass;
		this.name = _displayName;
		this.position = _position;
		this.isAdd = _isAdd;
		this.usingLocalizationId = _usingLocalizationId;
		this.useOverrideColor = true;
		this.overrideColor = _overrideColor;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.navObjectClass = _br.ReadString();
		this.name = _br.ReadString();
		this.position = StreamUtils.ReadVector3(_br);
		this.isAdd = _br.ReadBoolean();
		this.useOverrideColor = _br.ReadBoolean();
		this.overrideColor = StreamUtils.ReadColor32(_br);
		this.usingLocalizationId = _br.ReadBoolean();
		this.entityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.navObjectClass);
		_bw.Write(this.name ?? "");
		StreamUtils.Write(_bw, this.position);
		_bw.Write(this.isAdd);
		_bw.Write(this.useOverrideColor);
		StreamUtils.WriteColor32(_bw, this.overrideColor);
		_bw.Write(this.usingLocalizationId);
		_bw.Write(this.entityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (_world.IsRemote())
		{
			if (this.isAdd)
			{
				NavObject navObject = ((this.entityId == -1) ? null : NavObjectManager.Instance.GetNavObjectByEntityID(this.entityId));
				if (navObject != null && navObject.TrackType == NavObject.TrackTypes.Entity)
				{
					return;
				}
				NavObject navObject2 = NavObjectManager.Instance.RegisterNavObject(this.navObjectClass, this.position, "", false, -1, null);
				navObject2.name = this.name;
				navObject2.usingLocalizationId = this.usingLocalizationId;
				navObject2.EntityID = this.entityId;
				if (this.useOverrideColor)
				{
					navObject2.UseOverrideColor = true;
					navObject2.OverrideColor = this.overrideColor;
					return;
				}
			}
			else
			{
				if (this.entityId != -1)
				{
					NavObjectManager.Instance.UnRegisterNavObjectByEntityID(this.entityId);
					return;
				}
				NavObjectManager.Instance.UnRegisterNavObjectByPosition(this.position, this.navObjectClass);
			}
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override int GetLength()
	{
		return 30;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string navObjectClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usingLocalizationId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useOverrideColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color overrideColor;
}

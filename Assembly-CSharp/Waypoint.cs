using System;
using System.IO;

public class Waypoint
{
	public int InviterEntityId
	{
		get
		{
			return this.inviterEntityId;
		}
		set
		{
			this.inviterEntityId = value;
			PlatformUserIdentifierAbs primaryId = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(value).PrimaryId;
			this.name.Update(this.name.Text, primaryId);
		}
	}

	public bool HiddenOnMap
	{
		get
		{
			return this.hiddenOnMap;
		}
		set
		{
			this.hiddenOnMap = value;
			this.navObject.hiddenOnMap = this.hiddenOnMap;
		}
	}

	public Waypoint()
	{
		this.name = new AuthoredText();
	}

	public Waypoint Clone()
	{
		Waypoint waypoint = new Waypoint();
		waypoint.pos = this.pos;
		waypoint.icon = this.icon;
		AuthoredText authoredText = this.name;
		waypoint.name = ((authoredText != null) ? authoredText.Clone() : null);
		waypoint.bTracked = this.bTracked;
		waypoint.ownerId = this.ownerId;
		waypoint.lastKnownPositionEntityId = this.lastKnownPositionEntityId;
		waypoint.lastKnownPositionEntityType = this.lastKnownPositionEntityType;
		waypoint.navObject = this.navObject;
		waypoint.hiddenOnCompass = this.hiddenOnCompass;
		waypoint.bIsAutoWaypoint = this.bIsAutoWaypoint;
		waypoint.bUsingLocalizationId = this.bUsingLocalizationId;
		waypoint.IsSaved = this.IsSaved;
		waypoint.inviterEntityId = this.inviterEntityId;
		waypoint.hiddenOnMap = this.hiddenOnMap;
		return waypoint;
	}

	public void Read(BinaryReader _br, int version = 7)
	{
		this.pos = StreamUtils.ReadVector3i(_br);
		this.icon = _br.ReadString();
		this.name = AuthoredText.FromStream(_br);
		this.bTracked = _br.ReadBoolean();
		if (version > 2)
		{
			this.hiddenOnCompass = _br.ReadBoolean();
		}
		if (version > 1)
		{
			this.ownerId = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			this.lastKnownPositionEntityId = _br.ReadInt32();
		}
		if (version > 3)
		{
			this.bIsAutoWaypoint = _br.ReadBoolean();
			this.bUsingLocalizationId = _br.ReadBoolean();
		}
		if (version > 4)
		{
			this.inviterEntityId = _br.ReadInt32();
		}
		if (version > 5)
		{
			this.hiddenOnMap = _br.ReadBoolean();
		}
		if (version > 6)
		{
			this.lastKnownPositionEntityType = (eLastKnownPositionEntityType)_br.ReadInt32();
			return;
		}
		if (this.lastKnownPositionEntityId > -1)
		{
			this.lastKnownPositionEntityType = eLastKnownPositionEntityType.Vehicle;
		}
	}

	public void Write(BinaryWriter _bw)
	{
		StreamUtils.Write(_bw, this.pos);
		if (this.icon == null)
		{
			_bw.Write("");
		}
		else
		{
			_bw.Write(this.icon);
		}
		AuthoredText.ToStream(this.name, _bw);
		_bw.Write(this.bTracked);
		_bw.Write(this.hiddenOnCompass);
		this.ownerId.ToStream(_bw, false);
		_bw.Write(this.lastKnownPositionEntityId);
		_bw.Write(this.bIsAutoWaypoint);
		_bw.Write(this.bUsingLocalizationId);
		_bw.Write(this.inviterEntityId);
		_bw.Write(this.hiddenOnMap);
		_bw.Write((int)this.lastKnownPositionEntityType);
	}

	public override bool Equals(object obj)
	{
		Waypoint waypoint = obj as Waypoint;
		return waypoint != null && (waypoint.pos.Equals(this.pos) && waypoint.icon == this.icon && waypoint.name.Text == this.name.Text && object.Equals(waypoint.ownerId, this.ownerId)) && waypoint.lastKnownPositionEntityId == this.lastKnownPositionEntityId;
	}

	public override int GetHashCode()
	{
		int num = (((17 * 31 + this.pos.GetHashCode()) * 31 + this.icon.GetHashCode()) * 31 + this.name.Text.GetHashCode()) * 31;
		PlatformUserIdentifierAbs platformUserIdentifierAbs = this.ownerId;
		return (num + ((platformUserIdentifierAbs != null) ? platformUserIdentifierAbs.GetHashCode() : 0)) * 31 + this.lastKnownPositionEntityId.GetHashCode();
	}

	public bool CanBeViewedBy(PlatformUserIdentifierAbs _userIdentifier)
	{
		return this.lastKnownPositionEntityId == -1 || (_userIdentifier != null && _userIdentifier.Equals(this.ownerId));
	}

	public override string ToString()
	{
		string[] array = new string[6];
		array[0] = "Waypoint name:";
		int num = 1;
		AuthoredText authoredText = this.name;
		array[num] = ((authoredText != null) ? authoredText.ToString() : null);
		array[2] = " icon:";
		array[3] = this.icon;
		array[4] = " Entity ID:";
		array[5] = this.lastKnownPositionEntityId.ToString();
		return string.Concat(array);
	}

	public Vector3i pos;

	public string icon;

	public AuthoredText name;

	public bool bTracked;

	public PlatformUserIdentifierAbs ownerId;

	public int lastKnownPositionEntityId = -1;

	public eLastKnownPositionEntityType lastKnownPositionEntityType;

	public long MapObjectKey;

	public bool hiddenOnCompass;

	public NavObject navObject;

	public bool bIsAutoWaypoint;

	public bool bUsingLocalizationId;

	public bool IsSaved = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inviterEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hiddenOnMap;
}

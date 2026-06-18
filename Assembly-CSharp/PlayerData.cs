using System;
using System.IO;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class PlayerData
{
	public PlayerData(PlatformUserIdentifierAbs _primaryId, PlatformUserIdentifierAbs _nativeId, AuthoredText _playerName, EPlayGroup _playGroup)
	{
		this.PrimaryId = _primaryId;
		this.NativeId = _nativeId;
		this.PlayGroup = _playGroup;
		this.PlayerName = _playerName;
		this.PlatformData = PlatformUserManager.GetOrCreate(_primaryId);
		this.PlatformData.NativeId = _nativeId;
	}

	public static PlayerData Read(BinaryReader _reader)
	{
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(_reader, false, false);
		PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromStream(_reader, false, false);
		AuthoredText authoredText = AuthoredText.FromStream(_reader);
		EPlayGroup eplayGroup = (EPlayGroup)_reader.ReadByte();
		GeneratedTextManager.PrefilterText(authoredText, GeneratedTextManager.TextFilteringMode.Filter);
		return new PlayerData(platformUserIdentifierAbs, platformUserIdentifierAbs2, authoredText, eplayGroup);
	}

	public void Write(BinaryWriter _writer)
	{
		this.PrimaryId.ToStream(_writer, false);
		this.NativeId.ToStream(_writer, false);
		AuthoredText.ToStream(this.PlayerName, _writer);
		_writer.Write((byte)this.PlayGroup);
	}

	public readonly IPlatformUserData PlatformData;

	public readonly PlatformUserIdentifierAbs PrimaryId;

	public readonly PlatformUserIdentifierAbs NativeId;

	public readonly EPlayGroup PlayGroup;

	public readonly AuthoredText PlayerName;
}

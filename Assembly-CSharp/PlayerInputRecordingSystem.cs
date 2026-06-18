using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerInputRecordingSystem
{
	public static PlayerInputRecordingSystem Instance
	{
		get
		{
			if (PlayerInputRecordingSystem.mInstance == null)
			{
				PlayerInputRecordingSystem.mInstance = new PlayerInputRecordingSystem();
			}
			return PlayerInputRecordingSystem.mInstance;
		}
	}

	public void Record(MovementInput _movement, int _frameNr)
	{
	}

	public void Record(EntityPlayer _player, ulong _ticks)
	{
		if (this.startTickTime == 0UL)
		{
			this.startTickTime = GameTimer.Instance.ticks;
		}
		this.recording.Add(new PlayerInputRecordingSystem.SPosRot
		{
			pos = _player.position,
			rot = _player.rotation,
			ticks = (int)(_ticks - this.startTickTime)
		});
	}

	public void Reset(bool _bClearRecordings = false)
	{
		this.index = 0;
		if (_bClearRecordings)
		{
			this.recording.Clear();
			this.startTickTime = 0UL;
		}
		this.relativeStartTickTime = 0UL;
		this.autoSaveFilename = null;
	}

	public bool Play(EntityPlayer _player, bool _bPlayRelativeToNow = false)
	{
		if (this.relativeStartTickTime == 0UL)
		{
			if (_bPlayRelativeToNow)
			{
				this.relativeStartTickTime = GameTimer.Instance.ticks;
			}
			else
			{
				this.relativeStartTickTime = this.startTickTime;
			}
			this.startFrameCount = Time.frameCount;
			this.startTime = Time.time;
		}
		while (this.index < this.recording.Count)
		{
			int ticks = this.recording[this.index].ticks;
			if (GameTimer.Instance.ticks < this.relativeStartTickTime + (ulong)((long)ticks))
			{
				break;
			}
			_player.SetPosition(this.recording[this.index].pos, true);
			_player.SetRotation(this.recording[this.index].rot);
			this.index++;
		}
		if (this.index == this.recording.Count)
		{
			Log.Out("Playing ended. Frames=" + (Time.frameCount - this.startFrameCount).ToString() + " avg fps=" + ((float)(Time.frameCount - this.startFrameCount) / (Time.time - this.startTime)).ToString("0.0"));
			GUIWindowConsole.Open();
			this.index++;
		}
		return this.index < this.recording.Count;
	}

	public void SetStartPosition(EntityPlayer _player)
	{
		if (this.recording.Count > 0)
		{
			_player.SetPosition(this.recording[0].pos, true);
			_player.SetRotation(this.recording[0].rot);
		}
	}

	public void Load(string _filename)
	{
		using (BinaryReader binaryReader = new BinaryReader(SdFile.OpenRead(GameIO.GetSaveGameDir() + "/" + _filename + ".rec")))
		{
			this.recording.Clear();
			binaryReader.ReadByte();
			this.startTickTime = binaryReader.ReadUInt64();
			int num = (int)binaryReader.ReadUInt32();
			for (int i = 0; i < num; i++)
			{
				PlayerInputRecordingSystem.SPosRot sposRot = default(PlayerInputRecordingSystem.SPosRot);
				sposRot.Read(binaryReader);
				this.recording.Add(sposRot);
			}
		}
	}

	public void SetAutoSaveTo(string _filename)
	{
		this.autoSaveFilename = _filename;
	}

	public bool AutoSave()
	{
		if (this.autoSaveFilename != null)
		{
			this.doSave(this.autoSaveFilename);
			this.autoSaveFilename = null;
			return true;
		}
		return false;
	}

	public void Save(string _filename)
	{
		this.doSave(GameIO.GetSaveGameDir() + "/" + _filename);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doSave(string _filename)
	{
		using (BinaryWriter binaryWriter = new BinaryWriter(SdFile.Open(_filename + ".rec", FileMode.Create, FileAccess.Write, FileShare.Read)))
		{
			binaryWriter.Write(1);
			binaryWriter.Write(this.startTickTime);
			binaryWriter.Write((uint)this.recording.Count);
			for (int i = 0; i < this.recording.Count; i++)
			{
				this.recording[i].Write(binaryWriter);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerInputRecordingSystem mInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlayerInputRecordingSystem.SPosRot> recording = new List<PlayerInputRecordingSystem.SPosRot>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong startTickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong relativeStartTickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startFrameCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float startTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string autoSaveFilename;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct SPosRot
	{
		public void Write(BinaryWriter _bw)
		{
			_bw.Write(this.ticks);
			StreamUtils.Write(_bw, this.pos);
			StreamUtils.Write(_bw, this.rot);
		}

		public void Read(BinaryReader _br)
		{
			this.ticks = _br.ReadInt32();
			this.pos = StreamUtils.ReadVector3(_br);
			this.rot = StreamUtils.ReadVector3(_br);
		}

		public Vector3 pos;

		public Vector3 rot;

		public int ticks;
	}
}

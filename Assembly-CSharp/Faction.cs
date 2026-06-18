using System;
using System.IO;
using UnityEngine;

public class Faction
{
	public Faction()
	{
	}

	public Faction(string _name, bool _playerFaction = false, string _icon = "")
	{
		this.Name = _name;
		this.Icon = _icon;
		this.IsPlayerFaction = _playerFaction;
		for (int i = 0; i < 255; i++)
		{
			this.Relationships[i] = 400f;
		}
	}

	public void ModifyRelationship(byte _factionId, float _modifier)
	{
		float num = this.Relationships[(int)_factionId];
		if (num != 255f)
		{
			num = Mathf.Clamp(num + _modifier, 0f, 1000f);
		}
		this.Relationships[(int)_factionId] = num;
	}

	public void SetRelationship(byte _factionId, float _value)
	{
		this.Relationships[(int)_factionId] = (float)((byte)Mathf.Clamp(_value, 0f, 1000f));
	}

	public float GetRelationship(byte _factionId)
	{
		return this.Relationships[(int)_factionId];
	}

	public void SetAlly(byte _factionId)
	{
		this.Relationships[(int)_factionId] = 1000f;
	}

	public void Write(BinaryWriter bw)
	{
		for (int i = 0; i < 255; i++)
		{
			bw.Write(this.Relationships[i]);
		}
		bw.Write(this.IsPlayerFaction);
	}

	public void Read(BinaryReader br)
	{
		this.Relationships = new float[255];
		for (int i = 0; i < 255; i++)
		{
			this.Relationships[i] = br.ReadSingle();
		}
		this.IsPlayerFaction = br.ReadBoolean();
	}

	public override string ToString()
	{
		return string.Format("{0} : {1}", this.Name, string.Join(", ", Array.ConvertAll<float, string>(this.Relationships, (float x) => x.ToCultureInvariantString())));
	}

	public byte ID;

	public string Name;

	public string Icon;

	public bool IsPlayerFaction;

	public float[] Relationships = new float[255];
}

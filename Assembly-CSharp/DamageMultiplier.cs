using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public class DamageMultiplier
{
	public DamageMultiplier()
	{
	}

	public DamageMultiplier(DynamicProperties _properties)
	{
		if (_properties.Classes.ContainsKey("DamageBonus"))
		{
			foreach (KeyValuePair<string, string> keyValuePair in _properties.Classes["DamageBonus"].Values)
			{
				float num = StringParsers.ParseFloat(keyValuePair.Value, 0, -1, NumberStyles.Any);
				this.addMultiplier(keyValuePair.Key, num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addMultiplier(string _name, float _value)
	{
		this.damageMultiplier[_name] = _value;
	}

	public float Get(string _group)
	{
		if (_group == null || this.damageMultiplier == null || !this.damageMultiplier.ContainsKey(_group))
		{
			return 1f;
		}
		return this.damageMultiplier[_group];
	}

	public void Read(BinaryReader _br)
	{
		this.damageMultiplier.Clear();
		int num = (int)_br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			string text = _br.ReadString();
			float num2 = _br.ReadSingle();
			this.damageMultiplier.Add(text, num2);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((short)this.damageMultiplier.Count);
		foreach (KeyValuePair<string, float> keyValuePair in this.damageMultiplier)
		{
			_bw.Write(keyValuePair.Key);
			_bw.Write(keyValuePair.Value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, float> damageMultiplier = new Dictionary<string, float>();
}

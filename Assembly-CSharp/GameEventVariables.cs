using System;
using System.Collections.Generic;

public class GameEventVariables
{
	public void ModifyEventVariable(string name, GameEventVariables.OperationTypes operation, int value, int minValue = -2147483648, int maxValue = 2147483647)
	{
		if (this.EventVariables == null)
		{
			this.EventVariables = new Dictionary<string, object>();
		}
		if (this.operationType == GameEventVariables.OperationTypes.Set)
		{
			this.EventVariables[name] = Utils.FastClamp(value, minValue, maxValue);
			return;
		}
		int num = 0;
		this.ParseVarInt(name, ref num);
		switch (this.operationType)
		{
		case GameEventVariables.OperationTypes.Add:
			this.EventVariables[name] = Utils.FastClamp(num + value, minValue, maxValue);
			return;
		case GameEventVariables.OperationTypes.Subtract:
			this.EventVariables[name] = Utils.FastClamp(num - value, minValue, maxValue);
			return;
		case GameEventVariables.OperationTypes.Multiply:
			this.EventVariables[name] = Utils.FastClamp(num * value, minValue, maxValue);
			return;
		default:
			return;
		}
	}

	public void ModifyEventVariable(string name, GameEventVariables.OperationTypes operation, float value, float minValue = -3.4028235E+38f, float maxValue = 3.4028235E+38f)
	{
		if (this.EventVariables == null)
		{
			this.EventVariables = new Dictionary<string, object>();
		}
		if (this.operationType == GameEventVariables.OperationTypes.Set)
		{
			this.EventVariables[name] = Utils.FastClamp(value, minValue, maxValue);
			return;
		}
		float num = 0f;
		this.ParseVarFloat(name, ref num);
		switch (this.operationType)
		{
		case GameEventVariables.OperationTypes.Add:
			this.EventVariables[name] = Utils.FastClamp(num + value, minValue, maxValue);
			return;
		case GameEventVariables.OperationTypes.Subtract:
			this.EventVariables[name] = Utils.FastClamp(num - value, minValue, maxValue);
			return;
		case GameEventVariables.OperationTypes.Multiply:
			this.EventVariables[name] = Utils.FastClamp(num * value, minValue, maxValue);
			return;
		default:
			return;
		}
	}

	public void SetEventVariable(string name, bool value)
	{
		if (this.EventVariables == null)
		{
			this.EventVariables = new Dictionary<string, object>();
		}
		this.EventVariables[name] = value;
	}

	public void SetEventVariable(string name, string value)
	{
		if (this.EventVariables == null)
		{
			this.EventVariables = new Dictionary<string, object>();
		}
		this.EventVariables[name] = value;
	}

	public void ParseVarInt(string varName, ref int optionalValue)
	{
		if (this.EventVariables == null || !this.EventVariables.ContainsKey(varName))
		{
			return;
		}
		optionalValue = (int)this.EventVariables[varName];
	}

	public void ParseVarString(string varName, ref string optionalValue)
	{
		if (this.EventVariables == null || !this.EventVariables.ContainsKey(varName))
		{
			return;
		}
		optionalValue = (string)this.EventVariables[varName];
	}

	public void ParseVarFloat(string varName, ref float optionalValue)
	{
		if (this.EventVariables == null || !this.EventVariables.ContainsKey(varName))
		{
			return;
		}
		optionalValue = (float)this.EventVariables[varName];
	}

	public void ParseString(string varName, ref string optionalValue)
	{
		if (this.EventVariables == null || !this.EventVariables.ContainsKey(varName))
		{
			return;
		}
		optionalValue = (string)this.EventVariables[varName];
	}

	public void ParseBool(string varName, ref bool optionalValue)
	{
		if (this.EventVariables == null || !this.EventVariables.ContainsKey(varName))
		{
			return;
		}
		optionalValue = (bool)this.EventVariables[varName];
	}

	public Dictionary<string, object> EventVariables = new Dictionary<string, object>();

	public GameEventVariables.OperationTypes operationType;

	public enum OperationTypes
	{
		Set,
		Add,
		Subtract,
		Multiply
	}
}

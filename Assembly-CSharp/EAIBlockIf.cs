using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class EAIBlockIf : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 1;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		this.conditions = new List<EAIBlockIf.Condition>();
		string text;
		if (data.TryGetValue("condition", out text))
		{
			string[] array = text.Split(' ', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i += 3)
			{
				EAIBlockIf.Condition condition = new EAIBlockIf.Condition
				{
					type = EnumUtils.Parse<EAIBlockIf.eType>(array[i], true)
				};
				if (condition.type == EAIBlockIf.eType.None)
				{
					Log.Warning("{0} BlockIf type None", new object[] { this.theEntity.EntityName });
				}
				condition.op = EnumUtils.Parse<EAIBlockIf.eOp>(array[i + 1], true);
				if (condition.op == EAIBlockIf.eOp.None)
				{
					Log.Warning("{0} BlockIf op None", new object[] { this.theEntity.EntityName });
				}
				condition.value = StringParsers.ParseFloat(array[i + 2], 0, -1, NumberStyles.Any);
				this.conditions.Add(condition);
			}
		}
	}

	public override bool CanExecute()
	{
		int count = this.conditions.Count;
		for (int i = 0; i < count; i++)
		{
			EAIBlockIf.Condition condition = this.conditions[i];
			float num = 0f;
			EAIBlockIf.eType type = condition.type;
			if (type != EAIBlockIf.eType.Alert)
			{
				if (type == EAIBlockIf.eType.Investigate)
				{
					num = (float)(this.theEntity.HasInvestigatePosition ? 1 : 0);
				}
			}
			else
			{
				num = (float)(this.theEntity.IsAlert ? 1 : 0);
			}
			if (this.Compare(condition.op, num, condition.value))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Compare(EAIBlockIf.eOp op, float v1, float v2)
	{
		if (op != EAIBlockIf.eOp.e)
		{
			return op == EAIBlockIf.eOp.ne && v1 != v2;
		}
		return v1 == v2;
	}

	public override bool Continue()
	{
		return this.CanExecute();
	}

	public bool canExecute;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAIBlockIf.Condition> conditions;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum eType
	{
		None,
		Alert,
		Investigate
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum eOp
	{
		None,
		e,
		ne
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Condition
	{
		public EAIBlockIf.eType type;

		public EAIBlockIf.eOp op;

		public float value;
	}
}

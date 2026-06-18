using System;

public class BuffManager
{
	public static void Cleanup()
	{
		if (BuffManager.Buffs != null)
		{
			BuffManager.Buffs.Clear();
			BuffManager.Buffs = null;
		}
	}

	public static void AddBuff(BuffClass _buffClass)
	{
		BuffManager.Buffs[_buffClass.Name] = _buffClass;
	}

	public static BuffClass GetBuff(string _name)
	{
		BuffClass buffClass;
		BuffManager.Buffs.TryGetValue(_name, out buffClass);
		return buffClass;
	}

	public static CaseInsensitiveStringDictionary<BuffClass> Buffs;
}

using System;

public class DictionaryNameIdMapping
{
	public int Add(string _name)
	{
		int num;
		this.namesToIds.TryGetValue(_name, out num);
		if (num == 0)
		{
			int num2 = this.nextId + 1;
			this.nextId = num2;
			num = num2;
			this.namesToIds[_name] = num;
		}
		return num;
	}

	public void Clear()
	{
		this.nextId = 0;
		this.namesToIds.Clear();
	}

	public int FindId(string _name)
	{
		int num;
		this.namesToIds.TryGetValue(_name, out num);
		return num;
	}

	public const int cIDNone = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextId;

	[PublicizedFrom(EAccessModifier.Private)]
	public CaseInsensitiveStringDictionary<int> namesToIds = new CaseInsensitiveStringDictionary<int>();
}

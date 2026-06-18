using System;

public class BiomeSpawningClass
{
	public static void Cleanup()
	{
		BiomeSpawningClass.list.Clear();
	}

	public static DictionarySave<string, BiomeSpawnEntityGroupList> list = new DictionarySave<string, BiomeSpawnEntityGroupList>();
}

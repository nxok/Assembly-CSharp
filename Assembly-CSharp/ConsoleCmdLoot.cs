using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLoot : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "loot" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Loot commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Loot commands:\ncontainer [name] <count> <stage> <abundance> - list loot from named container for count times";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		if (text == "c" || text == "container")
		{
			if (_params.Count >= 2)
			{
				if (!LootContainer.IsLoaded())
				{
					WorldStaticData.InitSync(true, false, false, null);
				}
				int num = 1;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out num);
				}
				int num2 = 1;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out num2);
				}
				float num3 = 1f;
				if (_params.Count >= 5)
				{
					float.TryParse(_params[4], out num3);
				}
				this.ContainerList(_params[1], num, num2, num3);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ContainerList(string _name, int _count, int _stage, float _abundance)
	{
		LootContainer lootContainer = LootContainer.GetLootContainer(_name, false);
		if (lootContainer != null)
		{
			GameRandom gameRandom = new GameRandom();
			gameRandom.SetSeed(0);
			int num = 0;
			List<ItemStack> list = new List<ItemStack>();
			for (int i = 0; i < _count; i++)
			{
				int num2 = this.CountItems(list);
				int num3 = 999999;
				LootContainer.SpawnLootItemsFromList(gameRandom, lootContainer.itemsToSpawn, 1, _abundance, list, ref num3, (float)_stage, 0f, lootContainer.lootQualityTemplate, null, FastTags<TagGroup.Global>.none, false, false, true, lootContainer.ignoreLootAbundance, null, false);
				if (num2 == this.CountItems(list))
				{
					num++;
				}
			}
			list.Sort(delegate(ItemStack a, ItemStack b)
			{
				int num4 = b.count.CompareTo(a.count);
				if (num4 == 0)
				{
					num4 = a.itemValue.ItemClass.Name.CompareTo(b.itemValue.ItemClass.Name);
					if (num4 == 0)
					{
						num4 = a.itemValue.Quality.CompareTo(b.itemValue.Quality);
					}
				}
				return num4;
			});
			for (int j = list.Count - 1; j > 0; j--)
			{
				ItemStack itemStack = list[j];
				ItemStack itemStack2 = list[j - 1];
				if (itemStack.itemValue.type == itemStack2.itemValue.type && itemStack.itemValue.Quality == itemStack2.itemValue.Quality)
				{
					itemStack2.count += itemStack.count;
					list.RemoveAt(j);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				ItemStack itemStack3 = list[k];
				this.Print("#{0} {1}, q{2}, count {3}", new object[]
				{
					k,
					itemStack3.itemValue.ItemClass.GetItemName(),
					itemStack3.itemValue.Quality,
					itemStack3.count
				});
			}
			this.Print("Loot Container {0}, unique items {1}, empties {2}", new object[] { lootContainer.Name, list.Count, num });
			return;
		}
		this.Print("Unknown container " + _name, Array.Empty<object>());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CountItems(List<ItemStack> _list)
	{
		int num = 0;
		for (int i = 0; i < _list.Count; i++)
		{
			ItemStack itemStack = _list[i];
			num += itemStack.count;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Print(string _s, params object[] _values)
	{
		string text = string.Format(_s, _values);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(text);
	}
}

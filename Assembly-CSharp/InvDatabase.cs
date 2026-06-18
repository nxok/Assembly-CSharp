using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Examples/Item Database")]
public class InvDatabase : MonoBehaviour
{
	public static InvDatabase[] list
	{
		get
		{
			if (InvDatabase.mIsDirty)
			{
				InvDatabase.mIsDirty = false;
				InvDatabase.mList = NGUITools.FindActive<InvDatabase>();
			}
			return InvDatabase.mList;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		InvDatabase.mIsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		InvDatabase.mIsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public InvBaseItem GetItem(int id16)
	{
		int i = 0;
		int count = this.items.Count;
		while (i < count)
		{
			InvBaseItem invBaseItem = this.items[i];
			if (invBaseItem.id16 == id16)
			{
				return invBaseItem;
			}
			i++;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static InvDatabase GetDatabase(int dbID)
	{
		int i = 0;
		int num = InvDatabase.list.Length;
		while (i < num)
		{
			InvDatabase invDatabase = InvDatabase.list[i];
			if (invDatabase.databaseID == dbID)
			{
				return invDatabase;
			}
			i++;
		}
		return null;
	}

	public static InvBaseItem FindByID(int id32)
	{
		InvDatabase database = InvDatabase.GetDatabase(id32 >> 16);
		if (!(database != null))
		{
			return null;
		}
		return database.GetItem(id32 & 65535);
	}

	public static InvBaseItem FindByName(string exact)
	{
		int i = 0;
		int num = InvDatabase.list.Length;
		while (i < num)
		{
			InvDatabase invDatabase = InvDatabase.list[i];
			int j = 0;
			int count = invDatabase.items.Count;
			while (j < count)
			{
				InvBaseItem invBaseItem = invDatabase.items[j];
				if (invBaseItem.name == exact)
				{
					return invBaseItem;
				}
				j++;
			}
			i++;
		}
		return null;
	}

	public static int FindItemID(InvBaseItem item)
	{
		int i = 0;
		int num = InvDatabase.list.Length;
		while (i < num)
		{
			InvDatabase invDatabase = InvDatabase.list[i];
			if (invDatabase.items.Contains(item))
			{
				return (invDatabase.databaseID << 16) | item.id16;
			}
			i++;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static InvDatabase[] mList;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool mIsDirty = true;

	public int databaseID;

	public List<InvBaseItem> items = new List<InvBaseItem>();

	public Object iconAtlas;
}

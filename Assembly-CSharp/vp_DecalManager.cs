using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class vp_DecalManager
{
	public static float MaxDecals
	{
		get
		{
			return vp_DecalManager.m_MaxDecals;
		}
		set
		{
			vp_DecalManager.m_MaxDecals = value;
			vp_DecalManager.Refresh();
		}
	}

	public static float FadedDecals
	{
		get
		{
			return vp_DecalManager.m_FadedDecals;
		}
		set
		{
			if (value > vp_DecalManager.m_MaxDecals)
			{
				Debug.LogError("FadedDecals can't be larger than MaxDecals");
				return;
			}
			vp_DecalManager.m_FadedDecals = value;
			vp_DecalManager.Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static vp_DecalManager()
	{
		vp_DecalManager.Refresh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public vp_DecalManager()
	{
	}

	public static void Add(GameObject decal)
	{
		if (vp_DecalManager.m_Decals.Contains(decal))
		{
			vp_DecalManager.m_Decals.Remove(decal);
		}
		Color color = decal.GetComponent<Renderer>().material.color;
		color.a = 1f;
		decal.GetComponent<Renderer>().material.color = color;
		vp_DecalManager.m_Decals.Add(decal);
		vp_DecalManager.FadeAndRemove();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FadeAndRemove()
	{
		if ((float)vp_DecalManager.m_Decals.Count > vp_DecalManager.m_NonFadedDecals)
		{
			int num = 0;
			while ((float)num < (float)vp_DecalManager.m_Decals.Count - vp_DecalManager.m_NonFadedDecals)
			{
				if (vp_DecalManager.m_Decals[num] != null)
				{
					Color color = vp_DecalManager.m_Decals[num].GetComponent<Renderer>().material.color;
					color.a -= vp_DecalManager.m_FadeAmount;
					vp_DecalManager.m_Decals[num].GetComponent<Renderer>().material.color = color;
				}
				num++;
			}
		}
		if (vp_DecalManager.m_Decals[0] != null)
		{
			if (vp_DecalManager.m_Decals[0].GetComponent<Renderer>().material.color.a <= 0f)
			{
				vp_Utility.Destroy(vp_DecalManager.m_Decals[0]);
				vp_DecalManager.m_Decals.Remove(vp_DecalManager.m_Decals[0]);
				return;
			}
		}
		else
		{
			vp_DecalManager.m_Decals.RemoveAt(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Refresh()
	{
		if (vp_DecalManager.m_MaxDecals < vp_DecalManager.m_FadedDecals)
		{
			vp_DecalManager.m_MaxDecals = vp_DecalManager.m_FadedDecals;
		}
		vp_DecalManager.m_FadeAmount = vp_DecalManager.m_MaxDecals / vp_DecalManager.m_FadedDecals / vp_DecalManager.m_MaxDecals;
		vp_DecalManager.m_NonFadedDecals = vp_DecalManager.m_MaxDecals - vp_DecalManager.m_FadedDecals;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugOutput()
	{
		int num = 0;
		int num2 = 0;
		using (List<GameObject>.Enumerator enumerator = vp_DecalManager.m_Decals.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetComponent<Renderer>().material.color.a == 1f)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		Debug.Log(string.Concat(new string[]
		{
			"Decal count: ",
			vp_DecalManager.m_Decals.Count.ToString(),
			", Full: ",
			num.ToString(),
			", Faded: ",
			num2.ToString()
		}));
	}

	public static readonly vp_DecalManager instance = new vp_DecalManager();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<GameObject> m_Decals = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_MaxDecals = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_FadedDecals = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_NonFadedDecals = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_FadeAmount = 0f;
}

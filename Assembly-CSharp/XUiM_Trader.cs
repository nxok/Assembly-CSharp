using System;
using SandboxOptions;
using UnityEngine;

public class XUiM_Trader : XUiModel
{
	public TraderData TraderData
	{
		get
		{
			if (this.Trader == null)
			{
				return null;
			}
			return this.Trader.TraderData;
		}
	}

	public static int GetBuyPrice(XUi _xui, ItemValue itemValue, int count, ItemClass itemClass = null, int index = -1)
	{
		bool flag = false;
		TraderData traderData = _xui.Trader.TraderData;
		TraderInfo traderInfo = ((traderData != null) ? traderData.TraderInfo : null);
		if (itemClass == null)
		{
			itemClass = itemValue.ItemClass;
		}
		float num;
		int num2;
		if (itemClass.IsBlock())
		{
			num = Block.list[itemValue.type].EconomicValue;
			num2 = Block.list[itemValue.type].EconomicBundleSize;
		}
		else
		{
			num = EffectManager.GetValue(PassiveEffects.EconomicValue, itemValue, itemClass.EconomicValue, _xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num2 = itemClass.EconomicBundleSize;
		}
		if (num == 0f)
		{
			return 0;
		}
		float num3 = 0f;
		float num4 = 0f;
		if (traderData == null)
		{
			num4 = TraderInfo.BuyMarkup;
		}
		else if (traderInfo.Rentable || traderInfo.PlayerOwned)
		{
			if (index != -1)
			{
				num4 = 1f + (float)traderData.PrimaryInventory[index].Markup * 0.2f;
				flag = true;
			}
		}
		else
		{
			flag = traderInfo.OverrideBuyMarkup != -1f;
			num4 = (flag ? traderInfo.OverrideBuyMarkup : TraderInfo.BuyMarkup);
		}
		if (itemValue.HasQuality)
		{
			num3 = num * num4;
			if (itemClass.TraderQualityMinMod > 0f || itemClass.TraderQualityMaxMod > 0f)
			{
				num3 *= Mathf.Lerp(itemClass.TraderQualityMinMod, itemClass.TraderQualityMaxMod, ((float)itemValue.Quality - 1f) / 5f);
			}
			else
			{
				num3 *= Mathf.Lerp(TraderInfo.QualityMinMod, TraderInfo.QualityMaxMod, ((float)itemValue.Quality - 1f) / 5f);
			}
			float percentUsesLeft = itemValue.PercentUsesLeft;
			num3 *= percentUsesLeft;
		}
		else if (itemClass.HasSubItems)
		{
			for (int i = 0; i < itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue2 = itemValue.Modifications[i];
				if (!itemValue2.IsEmpty())
				{
					num3 += (float)XUiM_Trader.GetBuyPrice(_xui, itemValue2, 1, null, -1);
				}
			}
		}
		else
		{
			num3 = num * num4;
		}
		if (!flag)
		{
			num3 -= num3 * EffectManager.GetValue(PassiveEffects.BarteringBuying, null, 0f, XUiM_Player.GetPlayer(), null, itemClass.ItemTags, true, true, true, true, true, 1, true, false);
		}
		return Mathf.CeilToInt((float)((int)(num3 * (float)(count / num2))) * SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.TraderBuyPrices));
	}

	public static int GetSellPrice(XUi _xui, ItemValue itemValue, int count, ItemClass itemClass = null)
	{
		bool flag = false;
		TraderData traderData = _xui.Trader.TraderData;
		if (itemClass == null)
		{
			itemClass = itemValue.ItemClass;
		}
		float num;
		int num2;
		if (itemClass.IsBlock())
		{
			Block block = Block.list[itemValue.type];
			num = block.EconomicValue;
			num *= block.EconomicSellScale;
			num2 = block.EconomicBundleSize;
		}
		else
		{
			num = itemClass.EconomicValue;
			num *= itemClass.EconomicSellScale;
			num = EffectManager.GetValue(PassiveEffects.EconomicValue, itemValue, num, _xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num2 = itemClass.EconomicBundleSize;
		}
		if (num == 0f)
		{
			return 0;
		}
		float num3 = 0f;
		float num4;
		if (traderData == null)
		{
			num4 = TraderInfo.SellMarkdown;
		}
		else
		{
			TraderInfo traderInfo = traderData.TraderInfo;
			flag = traderInfo.OverrideSellMarkdown != -1f;
			num4 = (flag ? traderInfo.OverrideSellMarkdown : TraderInfo.SellMarkdown);
		}
		if (itemValue.HasQuality)
		{
			num3 = num * num4;
			if (itemClass.TraderQualityMinMod > 0f || itemClass.TraderQualityMaxMod > 0f)
			{
				num3 *= Mathf.Lerp(itemClass.TraderQualityMinMod, itemClass.TraderQualityMaxMod, ((float)itemValue.Quality - 1f) / 5f);
			}
			else
			{
				num3 *= Mathf.Lerp(TraderInfo.QualityMinMod, TraderInfo.QualityMaxMod, ((float)itemValue.Quality - 1f) / 5f);
			}
			float percentUsesLeft = itemValue.PercentUsesLeft;
			num3 *= percentUsesLeft;
		}
		else if (itemClass.HasSubItems)
		{
			for (int i = 0; i < itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue2 = itemValue.Modifications[i];
				if (!itemValue2.IsEmpty())
				{
					num3 += (float)XUiM_Trader.GetSellPrice(_xui, itemValue2, 1, null);
				}
			}
		}
		else
		{
			num3 = num * num4;
		}
		if (!flag)
		{
			num3 += num3 * EffectManager.GetValue(PassiveEffects.BarteringSelling, null, 0f, XUiM_Player.GetPlayer(), null, itemClass.ItemTags, true, true, true, true, true, 1, true, false);
		}
		return Mathf.CeilToInt((float)((int)(num3 * (float)(count / num2))) * SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.TraderSellPrices));
	}

	public ITrader Trader;

	public XUiC_TraderWindowGroup TraderWindowGroup;
}

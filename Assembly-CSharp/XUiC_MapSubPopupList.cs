using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapSubPopupList : XUiController
{
	public override void Init()
	{
		base.Init();
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiController xuiController = this.children[i].Children[0];
			if (xuiController is XUiC_MapSubPopupEntry)
			{
				XUiC_MapSubPopupEntry xuiC_MapSubPopupEntry = (XUiC_MapSubPopupEntry)xuiController;
				xuiC_MapSubPopupEntry.SetIndex(i);
				xuiC_MapSubPopupEntry.SetSpriteName(XUiC_MapSubPopupList.sprites[i % XUiC_MapSubPopupList.sprites.Length]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ResetList()
	{
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiController xuiController = this.children[i].Children[0];
			if (xuiController is XUiC_MapSubPopupEntry)
			{
				((XUiC_MapSubPopupEntry)xuiController).Reset();
			}
		}
		this.children[0].SelectCursorElement(true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] sprites = new string[]
	{
		"ui_game_symbol_map_cave", "ui_game_symbol_map_cabin", "ui_game_symbol_map_campsite", "ui_game_symbol_map_city", "ui_game_symbol_map_fortress", "ui_game_symbol_map_civil", "ui_game_symbol_map_house", "ui_game_symbol_map_town", "ui_game_symbol_map_trader", "ui_game_symbol_x",
		"ui_game_symbol_book", "ui_game_symbol_coin", "ui_game_symbol_safe", "ui_game_symbol_treasure", "ui_game_symbol_radial_number_1", "ui_game_symbol_radial_number_2", "ui_game_symbol_radial_number_3", "ui_game_symbol_radial_number_4", "ui_game_symbol_radial_number_5", "ui_game_symbol_check"
	};
}

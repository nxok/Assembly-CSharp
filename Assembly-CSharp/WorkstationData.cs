using System;

public class WorkstationData
{
	public WorkstationData(string blockName, DynamicProperties properties)
	{
		if (properties.Values.ContainsKey("WorkstationName"))
		{
			this.WorkstationName = properties.Values["WorkstationName"];
		}
		else
		{
			this.WorkstationName = blockName;
		}
		if (properties.Values.ContainsKey("WorkstationIcon"))
		{
			this.WorkstationIcon = properties.Values["WorkstationIcon"];
		}
		else
		{
			this.WorkstationIcon = "ui_game_symbol_hammer";
		}
		if (properties.Values.ContainsKey("CraftActionName"))
		{
			this.CraftActionName = Localization.Get(properties.Values["CraftActionName"], false, null);
		}
		else
		{
			this.CraftActionName = Localization.Get("lblContextActionCraft", false, null);
		}
		if (properties.Values.ContainsKey("CraftIcon"))
		{
			this.CraftIcon = properties.Values["CraftIcon"];
		}
		else
		{
			this.CraftIcon = "ui_game_symbol_hammer";
		}
		if (properties.Values.ContainsKey("OpenSound"))
		{
			this.OpenSound = properties.Values["OpenSound"];
		}
		else
		{
			this.OpenSound = "open_workbench";
		}
		if (properties.Values.ContainsKey("CloseSound"))
		{
			this.CloseSound = properties.Values["CloseSound"];
		}
		else
		{
			this.CloseSound = "close_workbench";
		}
		if (properties.Values.ContainsKey("CraftSound"))
		{
			this.CraftSound = properties.Values["CraftSound"];
		}
		else
		{
			this.CraftSound = "craft_click_craft";
		}
		if (properties.Values.ContainsKey("CraftCompleteSound"))
		{
			this.CraftCompleteSound = properties.Values["CraftCompleteSound"];
		}
		else
		{
			this.CraftCompleteSound = "craft_complete_item";
		}
		if (properties.Values.ContainsKey("WorkstationWindow"))
		{
			this.WorkstationWindow = properties.Values["WorkstationWindow"];
			return;
		}
		this.WorkstationWindow = "";
	}

	public string WorkstationName;

	public string WorkstationIcon;

	public string CraftIcon;

	public string CraftActionName = "";

	public string WorkstationWindow = "";

	public string OpenSound;

	public string CloseSound;

	public string CraftSound;

	public string CraftCompleteSound;
}

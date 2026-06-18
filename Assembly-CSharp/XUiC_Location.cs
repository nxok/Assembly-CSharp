using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Location : XUiController
{
	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.LocalPlayer == null && this.xui != null && this.xui.playerUI != null && this.xui.playerUI.entityPlayer != null)
		{
			this.LocalPlayer = this.xui.playerUI.entityPlayer;
		}
		if (this.LocalPlayer == null)
		{
			return;
		}
		if (!XUiC_Location.ShowLocation)
		{
			if (base.ViewComponent.IsVisible)
			{
				base.ViewComponent.IsVisible = false;
			}
			return;
		}
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		if (windowManager.IsHUDEnabled() || (this.xui.DragAndDropWindow.InMenu && windowManager.IsHUDPartialHidden()))
		{
			if (base.ViewComponent.IsVisible && this.LocalPlayer.IsDead())
			{
				base.ViewComponent.IsVisible = false;
			}
			else if (!base.ViewComponent.IsVisible && !this.LocalPlayer.IsDead())
			{
				base.ViewComponent.IsVisible = true;
			}
		}
		else
		{
			base.ViewComponent.IsVisible = false;
		}
		if (!this.LocalPlayer.IsAlive())
		{
			base.ViewComponent.IsVisible = false;
			return;
		}
		PrefabInstance prefab = this.LocalPlayer.prefab;
		if (prefab != this.lastPrefabInstance || this.LocalPlayer.biomeStandingOn != this.lastBiome)
		{
			if (prefab != null && prefab.IsWithinInfoArea(this.LocalPlayer.position))
			{
				if (prefab.prefab.Tags.Test_AnySet(this.partTag) || prefab.prefab.Tags.Test_AnySet(this.streetTileTag) || prefab.prefab.Tags.Test_AnySet(this.navOnlyTileTag) || prefab.prefab.Tags.Test_AnySet(this.hideUITag))
				{
					this.lastPrefabInstance = null;
				}
				else
				{
					this.lastPrefabInstance = prefab;
				}
			}
			else
			{
				this.lastPrefabInstance = null;
			}
			this.lastPrefab = ((this.lastPrefabInstance != null) ? this.lastPrefabInstance.prefab : null);
			if (this.lastPrefab != null)
			{
				this.prefabDiff = (int)this.lastPrefab.DifficultyTier;
			}
			else
			{
				this.prefabDiff = 0;
			}
			this.lastBiome = this.LocalPlayer.biomeStandingOn;
			if (this.lastBiome != null)
			{
				float num = (float)(this.lastBiome.Difficulty - 1) * 0.5f;
				this.biomeDiff = (int)Mathf.Floor(num);
				this.showBiomeHalf = num - (float)this.biomeDiff == 0.5f;
			}
			else
			{
				this.biomeDiff = 0;
				this.showBiomeHalf = false;
			}
			base.RefreshBindings();
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1576332949U)
		{
			if (num <= 1518322684U)
			{
				if (num <= 1484767446U)
				{
					if (num != 1451212208U)
					{
						if (num == 1484767446U)
						{
							if (_bindingName == "visible3")
							{
								_value = (this.prefabDiff + this.biomeDiff >= 3).ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "visible1")
					{
						_value = (this.prefabDiff + this.biomeDiff >= 1).ToString();
						return true;
					}
				}
				else if (num != 1501545065U)
				{
					if (num == 1518322684U)
					{
						if (_bindingName == "visible5")
						{
							_value = (this.prefabDiff + this.biomeDiff >= 5).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "visible2")
				{
					_value = (this.prefabDiff + this.biomeDiff >= 2).ToString();
					return true;
				}
			}
			else if (num <= 1551877922U)
			{
				if (num != 1535100303U)
				{
					if (num == 1551877922U)
					{
						if (_bindingName == "visible7")
						{
							_value = (this.prefabDiff + this.biomeDiff >= 7).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "visible4")
				{
					_value = (this.prefabDiff + this.biomeDiff >= 4).ToString();
					return true;
				}
			}
			else if (num != 1568655541U)
			{
				if (num == 1576332949U)
				{
					if (_bindingName == "visible_loot_max")
					{
						if (this.LocalPlayer == null)
						{
							_value = "false";
							return true;
						}
						_value = this.LocalPlayer.LootAtMax.ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "visible6")
			{
				_value = (this.prefabDiff + this.biomeDiff >= 6).ToString();
				return true;
			}
		}
		else if (num <= 2327937225U)
		{
			if (num <= 2294381987U)
			{
				if (num != 2277604368U)
				{
					if (num == 2294381987U)
					{
						if (_bindingName == "difficultycolor6")
						{
							_value = this.inactiveColorFormatter.Format(this.inactiveColor);
							if (this.prefabDiff >= 6)
							{
								_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
							}
							else if (this.prefabDiff + this.biomeDiff >= 6)
							{
								_value = this.activeColorFormatter.Format(this.biomeActiveColor);
							}
							return true;
						}
					}
				}
				else if (_bindingName == "difficultycolor7")
				{
					_value = this.inactiveColorFormatter.Format(this.inactiveColor);
					if (this.prefabDiff >= 7)
					{
						_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
					}
					else if (this.prefabDiff + this.biomeDiff >= 7)
					{
						_value = this.activeColorFormatter.Format(this.biomeActiveColor);
					}
					return true;
				}
			}
			else if (num != 2311159606U)
			{
				if (num == 2327937225U)
				{
					if (_bindingName == "difficultycolor4")
					{
						_value = this.inactiveColorFormatter.Format(this.inactiveColor);
						if (this.prefabDiff >= 4)
						{
							_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
						}
						else if (this.prefabDiff + this.biomeDiff >= 4)
						{
							_value = this.activeColorFormatter.Format(this.biomeActiveColor);
						}
						return true;
					}
				}
			}
			else if (_bindingName == "difficultycolor5")
			{
				_value = this.inactiveColorFormatter.Format(this.inactiveColor);
				if (this.prefabDiff >= 5)
				{
					_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
				}
				else if (this.prefabDiff + this.biomeDiff >= 5)
				{
					_value = this.activeColorFormatter.Format(this.biomeActiveColor);
				}
				return true;
			}
		}
		else if (num <= 2361492463U)
		{
			if (num != 2344714844U)
			{
				if (num == 2361492463U)
				{
					if (_bindingName == "difficultycolor2")
					{
						_value = this.inactiveColorFormatter.Format(this.inactiveColor);
						if (this.prefabDiff >= 2)
						{
							_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
						}
						else if (this.prefabDiff + this.biomeDiff >= 2)
						{
							_value = this.activeColorFormatter.Format(this.biomeActiveColor);
						}
						return true;
					}
				}
			}
			else if (_bindingName == "difficultycolor3")
			{
				_value = this.inactiveColorFormatter.Format(this.inactiveColor);
				if (this.prefabDiff >= 3)
				{
					_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
				}
				else if (this.prefabDiff + this.biomeDiff >= 3)
				{
					_value = this.activeColorFormatter.Format(this.biomeActiveColor);
				}
				return true;
			}
		}
		else if (num != 2378270082U)
		{
			if (num != 4063875975U)
			{
				if (num == 4157284869U)
				{
					if (_bindingName == "locationname")
					{
						if (this.lastPrefab != null)
						{
							_value = this.lastPrefab.LocalizedName;
						}
						else if (this.lastBiome != null)
						{
							_value = this.lastBiome.LocalizedName;
						}
						else
						{
							_value = "";
						}
						return true;
					}
				}
			}
			else if (_bindingName == "visible_half")
			{
				_value = this.showBiomeHalf.ToString();
				return true;
			}
		}
		else if (_bindingName == "difficultycolor1")
		{
			_value = this.inactiveColorFormatter.Format(this.inactiveColor);
			if (this.prefabDiff >= 1)
			{
				_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
			}
			else if (this.prefabDiff + this.biomeDiff >= 1)
			{
				_value = this.activeColorFormatter.Format(this.biomeActiveColor);
			}
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			if (!(name == "active_color"))
			{
				if (!(name == "inactive_color"))
				{
					return false;
				}
				this.inactiveColor = StringParsers.ParseColor32(value);
			}
			else
			{
				this.difficultyActiveColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab lastPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance lastPrefabInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition lastBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal LocalPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabDiff;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeDiff;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBiomeHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color difficultyActiveColor = Color.red;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color biomeActiveColor = new Color(1f, 0.5f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color inactiveColor = Color.grey;

	public static bool ShowLocation = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> partTag = FastTags<TagGroup.Poi>.Parse("part");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> streetTileTag = FastTags<TagGroup.Poi>.Parse("streettile");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> navOnlyTileTag = FastTags<TagGroup.Poi>.Parse("navonly");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> hideUITag = FastTags<TagGroup.Poi>.Parse("hideui");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor activeColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor inactiveColorFormatter = new CachedStringFormatterXuiRgbaColor();
}

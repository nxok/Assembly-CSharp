using System;
using System.Collections.Generic;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CompassWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_CompassWindow.ID = base.WindowGroup.Id;
		for (int i = 0; i < 50; i++)
		{
			UISprite uisprite = new GameObject
			{
				transform = 
				{
					parent = base.ViewComponent.UiTransform
				}
			}.AddComponent<UISprite>();
			this.waypointSpriteList.Add(uisprite);
			uisprite.atlas = this.xui.GetAtlasByName("UIAtlas", "menu_empty");
			uisprite.transform.localScale = Vector3.one;
			uisprite.spriteName = "menu_empty";
			uisprite.SetDimensions(20, 20);
			uisprite.color = Color.clear;
			uisprite.pivot = 4;
			uisprite.depth = 12;
			uisprite.gameObject.layer = 12;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.localPlayer)
		{
			this.localPlayer = this.xui.playerUI.entityPlayer;
			if (!this.localPlayer)
			{
				return;
			}
		}
		base.ViewComponent.IsVisible = !this.localPlayer.IsDead() && this.xui.playerUI.windowManager.IsHUDEnabled() && XUiC_CompassWindow.ShowCompass;
		if (this.localPlayer.playerCamera != null && XUiC_CompassWindow.ShowCompass)
		{
			World world = GameManager.Instance.World;
			this.showSleeperVolumes = true;
			int num = 0;
			this.updateNavObjects(this.localPlayer, ref num);
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.SleepingBag));
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.LandClaim));
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.MapMarker));
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.MapQuickMarker));
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.Backpack));
			if (this.showSleeperVolumes)
			{
				this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.Quest));
			}
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.TreasureChest));
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.FetchItem));
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.HiddenCache));
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.RestorePower));
			if (this.showSleeperVolumes)
			{
				this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.SleeperVolume));
			}
			this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.VendingMachine));
			if (GameStats.GetBool(EnumGameStats.AirDropMarker))
			{
				this.updateMarkers(this.localPlayer, ref num, world.GetObjectOnMapList(EnumMapObjectType.SupplyDrop));
			}
			Color clear = Color.clear;
			for (int i = num; i < this.waypointSpriteList.Count; i++)
			{
				this.waypointSpriteList[i].color = clear;
			}
		}
		if (XUi.IsGameRunning())
		{
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1971063990U)
		{
			if (num <= 707812386U)
			{
				if (num != 85644157U)
				{
					if (num != 304163417U)
					{
						if (num == 707812386U)
						{
							if (bindingName == "outsidetemp")
							{
								value = ((this.localPlayer && World.TemperatureSurvival) ? XUiM_Player.GetOutsideTemp(this.localPlayer) : "");
								return true;
							}
						}
					}
					else if (bindingName == "playercoretemp")
					{
						value = ((this.localPlayer && World.TemperatureSurvival) ? XUiM_Player.GetCoreTemp(this.localPlayer) : "");
						return true;
					}
				}
				else if (bindingName == "showtime")
				{
					if (this.localPlayer != null)
					{
						value = (EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, this.localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 0f && SandboxOptionManager.GetBool(global::SandboxOptions.SandboxOptions.ShowDayTime)).ToString();
					}
					else
					{
						value = "true";
					}
					return true;
				}
			}
			else if (num <= 1564253156U)
			{
				if (num != 1021729756U)
				{
					if (num == 1564253156U)
					{
						if (bindingName == "time")
						{
							value = "";
							if (XUi.IsGameRunning())
							{
								ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(GameManager.Instance.World.worldTime);
								int item = valueTuple.Item2;
								int item2 = valueTuple.Item3;
								value = this.timeFormatter.Format(item, item2);
							}
							return true;
						}
					}
				}
				else if (bindingName == "daytime")
				{
					value = "";
					if (XUi.IsGameRunning())
					{
						value = this.daytimeFormatter.Format(GameManager.Instance.World.worldTime);
					}
					return true;
				}
			}
			else if (num != 1586863888U)
			{
				if (num == 1971063990U)
				{
					if (bindingName == "compass_rotation")
					{
						if (this.localPlayer != null && this.localPlayer.playerCamera != null)
						{
							value = this.localPlayer.playerCamera.transform.eulerAngles.y.ToString();
						}
						else
						{
							value = "0.0";
						}
						return true;
					}
				}
			}
			else if (bindingName == "daycolor")
			{
				value = "FFFFFF";
				if (XUi.IsGameRunning())
				{
					ulong worldTime = GameManager.Instance.World.worldTime;
					int bloodMoonWarningHour = World.BloodMoonWarningHour;
					ValueTuple<int, int, int> valueTuple2 = GameUtils.WorldTimeToElements(worldTime);
					int item3 = valueTuple2.Item1;
					int item4 = valueTuple2.Item2;
					if (bloodMoonWarningHour != -1 && GameStats.GetInt(EnumGameStats.BloodMoonDay) == item3 && bloodMoonWarningHour <= item4)
					{
						value = "FF0000";
					}
				}
				return true;
			}
		}
		else if (num <= 2235205906U)
		{
			if (num != 1978910129U)
			{
				if (num != 2107568309U)
				{
					if (num == 2235205906U)
					{
						if (bindingName == "timetitle")
						{
							value = Localization.Get("xuiTime", false, null);
							return true;
						}
					}
				}
				else if (bindingName == "outsidetempcolor")
				{
					value = "FFFFFF";
					if (XUi.IsGameRunning() && this.localPlayer)
					{
						float customVar = this.localPlayer.Buffs.GetCustomVar("_outsidetemp");
						value = this.GetTemperatureColor(customVar);
					}
					return true;
				}
			}
			else if (bindingName == "daytitle")
			{
				value = Localization.Get("xuiDay", false, null);
				return true;
			}
		}
		else if (num <= 3603887947U)
		{
			if (num != 2899617242U)
			{
				if (num == 3603887947U)
				{
					if (bindingName == "show_compass")
					{
						value = "true";
						if (XUi.IsGameRunning() && this.localPlayer)
						{
							value = XUiC_CompassWindow.ShowCompass.ToString();
						}
						return true;
					}
				}
			}
			else if (bindingName == "compass_language")
			{
				if (GamePrefs.GetBool(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections))
				{
					value = "english";
				}
				else
				{
					value = Localization.ActiveLanguage;
				}
				return true;
			}
		}
		else if (num != 3830391293U)
		{
			if (num == 4043943779U)
			{
				if (bindingName == "coretempcolor")
				{
					value = "FFFFFF";
					if (XUi.IsGameRunning() && this.localPlayer)
					{
						float customVar2 = this.localPlayer.Buffs.GetCustomVar("_coretemp");
						value = this.GetTemperatureColor(customVar2);
					}
					return true;
				}
			}
		}
		else if (bindingName == "day")
		{
			value = "0";
			if (XUi.IsGameRunning())
			{
				int num2 = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
				value = this.dayFormatter.Format(num2);
			}
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetTemperatureColor(float _temp)
	{
		string text = "FFFFFF";
		if (_temp <= 32f)
		{
			text = "0099FF";
		}
		else if (_temp <= 50f)
		{
			text = "00FFFF";
		}
		else if (_temp >= 85f && _temp < 100f)
		{
			text = "FF8000";
		}
		else if (_temp >= 100f)
		{
			text = "FF0000";
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMarkers(EntityPlayerLocal localPlayer, ref int waypointSpriteIndex, List<MapObject> _mapObjectList)
	{
		int count = _mapObjectList.Count;
		if (count == 0)
		{
			return;
		}
		float num = (float)base.ViewComponent.Size.x * 0.5f;
		float num2 = num * 1.15f;
		Transform cameraTransform = localPlayer.cameraTransform;
		Entity entity = ((localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer);
		Vector3 position = entity.GetPosition();
		Vector2 vector;
		vector..ctor(position.x, position.z);
		Vector3 forward = cameraTransform.forward;
		Vector2 vector2;
		vector2..ctor(forward.x, forward.z);
		vector2.Normalize();
		Vector3 right = cameraTransform.right;
		Vector2 vector3;
		vector3..ctor(right.x, right.z);
		vector3.Normalize();
		for (int i = 0; i < count; i++)
		{
			MapObject mapObject = _mapObjectList[i];
			mapObject.RefreshData();
			if (waypointSpriteIndex >= this.waypointSpriteList.Count)
			{
				break;
			}
			if (mapObject.IsOnCompass())
			{
				if (mapObject is MapObjectZombie)
				{
					this.showSleeperVolumes = false;
				}
				Vector3 position2 = mapObject.GetPosition();
				Vector2 vector4 = new Vector2(position2.x, position2.z) - vector;
				float magnitude = vector4.magnitude;
				bool flag = true;
				if (mapObject.type == EnumMapObjectType.TreasureChest)
				{
					float num3 = (float)(mapObject as MapObjectTreasureChest).DefaultRadius;
					float num4 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num3, localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
					num4 = Utils.FastClamp(num4, 0f, num3);
					if (magnitude < num4)
					{
						float num5 = Mathf.PingPong(Time.time, 0.25f);
						float num6 = 1.25f + num5;
						this.waypointSpriteList[waypointSpriteIndex].atlas = this.xui.GetAtlasByName("UIAtlas", mapObject.GetMapIcon());
						this.waypointSpriteList[waypointSpriteIndex].spriteName = mapObject.GetMapIcon();
						this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num6), (int)(25f * num6));
						this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
						Color mapIconColor = mapObject.GetMapIconColor();
						this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(mapIconColor, Color.red, num5 * 4f);
						waypointSpriteIndex++;
						flag = false;
					}
				}
				string text = mapObject.GetCompassIcon();
				if (mapObject.type == EnumMapObjectType.HiddenCache)
				{
					this.waypointSpriteList[waypointSpriteIndex].flip = 0;
					if (position2.y < localPlayer.GetPosition().y - 2f)
					{
						text = mapObject.GetCompassDownIcon();
					}
					else if (position2.y > localPlayer.GetPosition().y + 2f)
					{
						text = mapObject.GetCompassUpIcon();
					}
					this.waypointSpriteList[waypointSpriteIndex].depth = 100;
					this.waypointSpriteList[waypointSpriteIndex].atlas = this.xui.GetAtlasByName("UIAtlas", text);
					this.waypointSpriteList[waypointSpriteIndex].spriteName = text;
					if ((position2 - entity.GetPosition()).magnitude < 10f)
					{
						float num7 = Mathf.PingPong(Time.time, 0.25f);
						float num8 = 1.25f + num7;
						this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num8), (int)(25f * num8));
						this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
						Color mapIconColor2 = mapObject.GetMapIconColor();
						this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(mapIconColor2, Color.grey, num7 * 4f);
						waypointSpriteIndex++;
						flag = false;
					}
				}
				if (mapObject.UseUpDownCompassIcons())
				{
					this.waypointSpriteList[waypointSpriteIndex].flip = 0;
					if (position2.y < localPlayer.GetPosition().y - 2f)
					{
						text = mapObject.GetCompassDownIcon();
					}
					else if (position2.y > localPlayer.GetPosition().y + 3f)
					{
						text = mapObject.GetCompassUpIcon();
					}
					this.waypointSpriteList[waypointSpriteIndex].depth = 100;
					this.waypointSpriteList[waypointSpriteIndex].atlas = this.xui.GetAtlasByName("UIAtlas", text);
					this.waypointSpriteList[waypointSpriteIndex].spriteName = text;
				}
				if (flag)
				{
					Vector2 normalized = vector4.normalized;
					if (!mapObject.IsCompassIconClamped() && Vector2.Dot(normalized, vector2) < 0.75f)
					{
						this.waypointSpriteList[waypointSpriteIndex].color = Color.clear;
					}
					else
					{
						float num9 = mapObject.GetCompassIconScale(magnitude);
						this.waypointSpriteList[waypointSpriteIndex].color = mapObject.GetMapIconColor();
						if (mapObject.IsTracked() && mapObject.NearbyCompassBlink() && (position2 - entity.GetPosition()).magnitude <= 6f)
						{
							Color mapIconColor3 = mapObject.GetMapIconColor();
							float num10 = Mathf.PingPong(Time.time, 0.5f);
							this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(Color.grey, mapIconColor3, num10 * 4f);
							if (num10 > 0.25f)
							{
								num9 += num10 - 0.25f;
							}
						}
						this.waypointSpriteList[waypointSpriteIndex].atlas = this.xui.GetAtlasByName("UIAtlas", text);
						this.waypointSpriteList[waypointSpriteIndex].spriteName = text;
						this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num9), (int)(25f * num9));
						if (Vector2.Dot(normalized, vector2) >= 0.75f)
						{
							this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + Vector2.Dot(normalized, vector3) * num2, -16f);
						}
						else
						{
							this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + ((Vector2.Dot(normalized, vector3) < 0f) ? (-0.675f) : 0.675f) * num2, -16f);
						}
						if (mapObject.type == EnumMapObjectType.Entity)
						{
							this.waypointSpriteList[waypointSpriteIndex].depth = 12 + (int)(num9 * 100f);
						}
						if (!mapObject.IsTracked())
						{
							Color mapIconColor4 = mapObject.GetMapIconColor();
							this.waypointSpriteList[waypointSpriteIndex].color = new Color(mapIconColor4.r * 0.75f, mapIconColor4.g * 0.75f, mapIconColor4.b * 0.75f) * num9;
						}
						waypointSpriteIndex++;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateNavObjects(EntityPlayerLocal localPlayer, ref int waypointSpriteIndex)
	{
		float num = (float)base.ViewComponent.Size.x * 0.5f;
		float num2 = num * 1.15f;
		Transform cameraTransform = localPlayer.cameraTransform;
		Entity entity = ((localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer);
		Vector3 position = entity.GetPosition();
		Vector2 vector;
		vector..ctor(position.x, position.z);
		Vector3 forward = cameraTransform.forward;
		Vector2 vector2;
		vector2..ctor(forward.x, forward.z);
		vector2.Normalize();
		Vector3 right = cameraTransform.right;
		Vector2 vector3;
		vector3..ctor(right.x, right.z);
		vector3.Normalize();
		List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
		for (int i = 0; i < navObjectList.Count; i++)
		{
			NavObject navObject = navObjectList[i];
			if (!navObject.hiddenOnCompass && navObject.IsValid())
			{
				if (waypointSpriteIndex >= this.waypointSpriteList.Count)
				{
					break;
				}
				NavObjectCompassSettings currentCompassSettings = navObject.CurrentCompassSettings;
				if (currentCompassSettings != null)
				{
					Vector3 position2 = navObject.GetPosition();
					Vector2 vector4 = new Vector2(position2.x + Origin.position.x, position2.z + Origin.position.z) - vector;
					float magnitude = vector4.magnitude;
					if (magnitude >= currentCompassSettings.MinDistance)
					{
						float maxDistance = navObject.GetMaxDistance(currentCompassSettings, localPlayer);
						if (maxDistance == -1f || magnitude <= maxDistance)
						{
							bool flag = true;
							string text = navObject.GetSpriteName(currentCompassSettings);
							this.waypointSpriteList[waypointSpriteIndex].depth = 12 + currentCompassSettings.DepthOffset;
							if (currentCompassSettings.HotZone != null)
							{
								float num3 = 1f;
								if (currentCompassSettings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Treasure)
								{
									float extraData = navObject.ExtraData;
									num3 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, extraData, localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
									num3 = Utils.FastClamp(num3, 0f, extraData);
								}
								else if (currentCompassSettings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Custom)
								{
									num3 = currentCompassSettings.HotZone.CustomDistance;
								}
								if (magnitude < num3)
								{
									float num4 = Mathf.PingPong(Time.time, 0.25f);
									float num5 = 1.25f + num4;
									this.waypointSpriteList[waypointSpriteIndex].atlas = this.xui.GetAtlasByName("UIAtlas", currentCompassSettings.HotZone.SpriteName);
									this.waypointSpriteList[waypointSpriteIndex].spriteName = currentCompassSettings.HotZone.SpriteName;
									this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num5), (int)(25f * num5));
									this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
									Color color = currentCompassSettings.HotZone.Color;
									this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(color, Color.red, num4 * 4f);
									waypointSpriteIndex++;
									flag = false;
								}
							}
							if (currentCompassSettings.ShowVerticalCompassIcons)
							{
								this.waypointSpriteList[waypointSpriteIndex].flip = 0;
								float num6 = localPlayer.GetPosition().y - Origin.position.y;
								if (position2.y < num6 + currentCompassSettings.ShowDownOffset)
								{
									text = currentCompassSettings.DownSpriteName;
								}
								else if (position2.y > num6 + currentCompassSettings.ShowUpOffset)
								{
									text = currentCompassSettings.UpSpriteName;
								}
								this.waypointSpriteList[waypointSpriteIndex].depth = 100;
								this.waypointSpriteList[waypointSpriteIndex].atlas = this.xui.GetAtlasByName("UIAtlas", text);
								this.waypointSpriteList[waypointSpriteIndex].spriteName = text;
							}
							if (flag)
							{
								Vector2 normalized = vector4.normalized;
								if (!currentCompassSettings.IconClamped && Vector2.Dot(normalized, vector2) < 0.75f)
								{
									this.waypointSpriteList[waypointSpriteIndex].color = Color.clear;
								}
								else
								{
									float num7 = navObject.GetCompassIconScale(magnitude);
									this.waypointSpriteList[waypointSpriteIndex].color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
									if (currentCompassSettings.HasPulse && (position2 - entity.GetPosition()).magnitude <= 6f)
									{
										Color color2 = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
										float num8 = Mathf.PingPong(Time.time, 0.5f);
										this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(Color.grey, color2, num8 * 4f);
										if (num8 > 0.25f)
										{
											num7 += num8 - 0.25f;
										}
									}
									this.waypointSpriteList[waypointSpriteIndex].atlas = this.xui.GetAtlasByName("UIAtlas", text);
									this.waypointSpriteList[waypointSpriteIndex].spriteName = text;
									this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num7), (int)(25f * num7));
									if (Vector2.Dot(normalized, vector2) >= 0.75f)
									{
										this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + Vector2.Dot(normalized, vector3) * num2, -16f);
									}
									else
									{
										this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + ((Vector2.Dot(normalized, vector3) < 0f) ? (-0.675f) : 0.675f) * num2, -16f);
									}
									if (!navObject.IsActive)
									{
										Color color3 = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
										if (currentCompassSettings.MinFadePercent != -1f)
										{
											if (currentCompassSettings.MinFadePercent > num7)
											{
												num7 = currentCompassSettings.MinFadePercent;
											}
											this.waypointSpriteList[waypointSpriteIndex].color = color3 * num7;
										}
										else
										{
											this.waypointSpriteList[waypointSpriteIndex].color = color3;
										}
									}
									waypointSpriteIndex++;
								}
							}
						}
					}
				}
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.windowManager.Close("windowpaging");
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	public List<UISprite> waypointSpriteList = new List<UISprite>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> daytimeFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, Localization.Get("xuiDayTimeLong", false, null)));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt dayFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> timeFormatter = new CachedStringFormatter<int, int>((int _hour, int _min) => string.Format("{0:00}:{1:00}", _hour, _min));

	public static bool ShowCompass = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showSleeperVolumes;
}

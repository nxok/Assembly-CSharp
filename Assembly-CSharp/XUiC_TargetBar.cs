using System;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TargetBar : XUiController
{
	public EntityAlive Target { get; set; }

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
		this.viewComponent.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.deltaTime = _dt;
		this.checkSandboxOption -= this.deltaTime;
		if (this.checkSandboxOption <= 0f)
		{
			this.checkSandboxOption = 3f;
			this.sandboxForceShow = SandboxOptionManager.GetBool(global::SandboxOptions.SandboxOptions.ShowHealthBars);
		}
		if (this.gameEventManager == null)
		{
			this.gameEventManager = GameEventManager.Current;
		}
		if (!this.xui.playerUI.entityPlayer.IsAlive())
		{
			this.viewComponent.IsVisible = false;
			return;
		}
		if (this.gameEventManager.CurrentBossGroup != null)
		{
			this.CurrentBossGroup = this.gameEventManager.CurrentBossGroup;
			this.Target = this.CurrentBossGroup.BossEntity;
			bool flag = false;
			if (this.Target != null && this.Target.IsAlive())
			{
				flag = true;
			}
			if (this.CurrentBossGroup.MinionCount != 0)
			{
				flag = true;
			}
			if (flag)
			{
				this.viewComponent.IsVisible = true;
				this.noTargetFadeTime = 0f;
			}
			else if (this.noTargetFadeTime >= this.noTargetFadeTimeMax)
			{
				this.Target = null;
				this.viewComponent.IsVisible = false;
				this.CurrentBossGroup.BossEntity = null;
			}
			else
			{
				this.noTargetFadeTime += Time.deltaTime;
			}
		}
		else
		{
			if (this.CurrentBossGroup != null)
			{
				this.Target = null;
				this.CurrentBossGroup = null;
			}
			if (!this.sandboxForceShow)
			{
				if (this.visibility == XUiC_TargetBar.EVisibility.Never)
				{
					this.viewComponent.IsVisible = false;
					return;
				}
				if (this.visibility == XUiC_TargetBar.EVisibility.GodMode && !this.xui.playerUI.entityPlayer.IsGodMode.Value)
				{
					this.viewComponent.IsVisible = false;
					return;
				}
			}
			bool flag2 = false;
			WorldRayHitInfo hitInfo = this.xui.playerUI.entityPlayer.HitInfo;
			if (hitInfo.bHitValid && hitInfo.transform && hitInfo.tag.StartsWith("E_", StringComparison.Ordinal))
			{
				Transform hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform);
				EntityAlive entityAlive = null;
				if (hitRootTransform != null)
				{
					entityAlive = hitRootTransform.GetComponent<EntityAlive>();
				}
				if (entityAlive != null && entityAlive.IsAlive())
				{
					flag2 = true;
					this.Target = entityAlive;
				}
			}
			if (this.Target == null)
			{
				this.viewComponent.IsVisible = false;
				this.noTargetFadeTime = this.noTargetFadeTimeMax;
				return;
			}
			if (flag2)
			{
				this.viewComponent.IsVisible = true;
				this.noTargetFadeTime = 0f;
			}
			else if (this.noTargetFadeTime >= this.noTargetFadeTimeMax)
			{
				this.Target = null;
				this.viewComponent.IsVisible = false;
			}
			else
			{
				this.noTargetFadeTime += Time.deltaTime;
			}
			if (this.Target != null && (this.Target.IsDead() || this.Target.Health == 0))
			{
				this.Target = null;
				this.viewComponent.IsVisible = false;
				this.noTargetFadeTime = this.noTargetFadeTimeMax;
			}
		}
		base.RefreshBindings();
		this.IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1324577848U)
		{
			if (num <= 973435292U)
			{
				if (num != 17711523U)
				{
					if (num == 973435292U)
					{
						if (bindingName == "isboss")
						{
							if (this.gameEventManager == null)
							{
								value = "false";
								return true;
							}
							value = (this.CurrentBossGroup != null).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "isnotboss")
				{
					if (this.gameEventManager == null)
					{
						value = "false";
						return true;
					}
					value = (this.CurrentBossGroup == null).ToString();
					return true;
				}
			}
			else if (num != 1080943954U)
			{
				if (num == 1324577848U)
				{
					if (bindingName == "currentwithmax")
					{
						if (this.Target == null)
						{
							value = "";
							return true;
						}
						value = this.statcurrentWMaxFormatterAOfB.Format(this.Target.IsAlive() ? ((int)this.Target.Stats.Health.Value) : 0, (int)this.Target.Stats.Health.Max);
						return true;
					}
				}
			}
			else if (bindingName == "minioncount")
			{
				if (this.gameEventManager == null || this.CurrentBossGroup == null)
				{
					value = "";
					return true;
				}
				value = this.CurrentBossGroup.MinionCount.ToString();
				return true;
			}
		}
		else if (num <= 2984927816U)
		{
			if (num != 2369371622U)
			{
				if (num == 2984927816U)
				{
					if (bindingName == "fill")
					{
						if (this.Target == null)
						{
							value = "0";
							return true;
						}
						float num2 = (float)this.Target.Health / (float)this.Target.GetMaxHealth();
						float num3 = Math.Max(this.lastValue, 0f) * 1.01f;
						value = this.statfillFormatter.Format(num3);
						this.lastValue = Mathf.Lerp(this.lastValue, num2, this.deltaTime * 3f);
						return true;
					}
				}
			}
			else if (bindingName == "name")
			{
				if (this.CurrentBossGroup != null)
				{
					value = this.CurrentBossGroup.BossName;
				}
				else if (this.Target == null)
				{
					value = "";
				}
				else
				{
					EntityPlayer entityPlayer = this.Target as EntityPlayer;
					value = ((entityPlayer != null) ? entityPlayer.PlayerDisplayName : Localization.Get(EntityClass.list[this.Target.entityClass].entityClassName, false, null));
				}
				return true;
			}
		}
		else if (num != 3647323098U)
		{
			if (num == 3767448944U)
			{
				if (bindingName == "boss_sprite")
				{
					if (this.gameEventManager == null || this.CurrentBossGroup == null)
					{
						value = "";
						return true;
					}
					value = ((this.CurrentBossGroup.BossIcon == "") ? this.defaultBossIcon : this.CurrentBossGroup.BossIcon);
					return true;
				}
			}
		}
		else if (bindingName == "current")
		{
			if (this.Target == null)
			{
				value = "";
				return true;
			}
			value = this.statcurrentFormatterInt.Format(this.Target.Health);
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "visibility")
		{
			this.visibility = EnumUtils.Parse<XUiC_TargetBar.EVisibility>(_value, true);
			return true;
		}
		if (!(_name == "default_boss_icon"))
		{
			return base.ParseAttribute(_name, _value);
		}
		this.defaultBossIcon = _value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float noTargetFadeTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float noTargetFadeTimeMax = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TargetBar.EVisibility visibility;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultBossIcon = "ui_game_symbol_twitch_boss_bar_default";

	[PublicizedFrom(EAccessModifier.Private)]
	public GameEventManager gameEventManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public BossGroup CurrentBossGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public float checkSandboxOption;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sandboxForceShow;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statcurrentFormatterInt = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> statcurrentWMaxFormatterAOfB = new CachedStringFormatter<int, int>((int _i, int _i1) => string.Format("{0}/{1}", _i, _i1));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat statfillFormatter = new CachedStringFormatterFloat(null);

	public enum EVisibility
	{
		Never,
		GodMode,
		Always,
		Boss
	}
}

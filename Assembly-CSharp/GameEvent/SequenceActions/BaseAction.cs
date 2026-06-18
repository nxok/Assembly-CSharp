using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent.SequenceRequirements;
using UnityEngine;

namespace GameEvent.SequenceActions
{
	public class BaseAction
	{
		public GameEventActionSequence Owner
		{
			get
			{
				return this.owner;
			}
			set
			{
				this.owner = value;
				if (this.Requirements != null)
				{
					for (int i = 0; i < this.Requirements.Count; i++)
					{
						this.Requirements[i].Owner = value;
					}
				}
			}
		}

		public static BaseAction FindKey(string key)
		{
			BaseAction baseAction = null;
			if (!BaseAction.sLookupByKey.TryGetValue(key, out baseAction))
			{
				baseAction = null;
			}
			return baseAction;
		}

		public virtual void SetActionKeyData(int _actionIndex, BaseAction _parent, string prefix = "")
		{
			if (_parent != null)
			{
				this.actionKey = _parent.GetActionKey() + ":" + _actionIndex.ToString();
			}
			else
			{
				this.actionKey = prefix + _actionIndex.ToString();
			}
			BaseAction.sLookupByKey[this.actionKey] = this;
		}

		public string GetActionKey()
		{
			return this.actionKey;
		}

		public virtual bool UseRequirements
		{
			get
			{
				return true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnInit()
		{
		}

		public void Init()
		{
			this.OnInit();
			this.IsComplete = false;
		}

		public virtual bool CanPerform(Entity target)
		{
			return true;
		}

		public virtual void OnClientPerform(Entity target)
		{
		}

		public virtual BaseAction.ActionCompleteStates OnPerformAction()
		{
			return BaseAction.ActionCompleteStates.InCompleteRefund;
		}

		public BaseAction.ActionCompleteStates PerformAction()
		{
			if (this.UseRequirements && this.Requirements != null)
			{
				for (int i = 0; i < this.Requirements.Count; i++)
				{
					this.Requirements[i].Owner = this.Owner;
					if (!this.Requirements[i].CanPerform(this.Owner.Target))
					{
						return BaseAction.ActionCompleteStates.RequirementsNotMet;
					}
				}
			}
			return this.OnPerformAction();
		}

		public void Reset()
		{
			this.IsComplete = false;
			this.OnReset();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnReset()
		{
		}

		public virtual void ParseProperties(DynamicProperties properties)
		{
			this.Properties = properties;
			this.Owner.HandleVariablesForProperties(properties);
			properties.ParseInt(BaseAction.PropPhase, ref this.Phase);
			properties.ParseInt(BaseAction.PropPhaseOnComplete, ref this.PhaseOnComplete);
			properties.ParseInt(BaseAction.PropPhaseOnDenied, ref this.PhaseOnDenied);
			properties.ParseBool(BaseAction.PropIgnoreRefund, ref this.IgnoreRefund);
		}

		public virtual void HandleTemplateInit(GameEventActionSequence seq)
		{
			seq.HandleVariablesForProperties(this.Properties);
			this.Owner = seq;
			if (this.Properties != null)
			{
				this.ParseProperties(this.Properties);
			}
			this.Init();
			if (this.Requirements != null)
			{
				for (int i = 0; i < this.Requirements.Count; i++)
				{
					seq.HandleVariablesForProperties(this.Requirements[i].Properties);
					if (this.Requirements[i].Properties != null)
					{
						this.Requirements[i].ParseProperties(this.Requirements[i].Properties);
					}
					this.Requirements[i].Init();
				}
			}
		}

		public void AddRequirement(BaseRequirement req)
		{
			if (this.Requirements == null)
			{
				this.Requirements = new List<BaseRequirement>();
			}
			req.Owner = this.Owner;
			this.Requirements.Add(req);
		}

		public virtual BaseAction Clone()
		{
			BaseAction baseAction = this.CloneChildSettings();
			if (this.Properties != null)
			{
				baseAction.Properties = new DynamicProperties();
				baseAction.Properties.CopyFrom(this.Properties, null);
			}
			baseAction.Phase = this.Phase;
			baseAction.PhaseOnComplete = this.PhaseOnComplete;
			baseAction.PhaseOnDenied = this.PhaseOnDenied;
			baseAction.IsComplete = false;
			baseAction.actionKey = this.actionKey;
			baseAction.IgnoreRefund = this.IgnoreRefund;
			if (this.Requirements != null)
			{
				for (int i = 0; i < this.Requirements.Count; i++)
				{
					baseAction.AddRequirement(this.Requirements[i].Clone());
				}
			}
			return baseAction;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual BaseAction CloneChildSettings()
		{
			return null;
		}

		public IEnumerator TeleportEntity(Entity entity, Vector3 position, float teleportDelay)
		{
			yield return new WaitForSeconds(teleportDelay);
			EntityPlayer entityPlayer = entity as EntityPlayer;
			if (entityPlayer != null)
			{
				if (entityPlayer.isEntityRemote)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(entityPlayer.entityId), false, entityPlayer.entityId, -1, -1, null, 192, false);
					SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityPlayer.entityId).SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(position, null, false));
				}
				else
				{
					((EntityPlayerLocal)entityPlayer).PlayerUI.windowManager.CloseAllOpenModalWindows(null, false);
					((EntityPlayerLocal)entityPlayer).TeleportToPosition(position, false, null);
				}
			}
			else if (entity.AttachedToEntity != null)
			{
				entity.AttachedToEntity.SetPosition(position, true);
			}
			else
			{
				entity.SetPosition(position, true);
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual string ParseTextElement(string element)
		{
			return element;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string GetTextWithElements(string text)
		{
			int num = text.IndexOf("{", StringComparison.Ordinal);
			Dictionary<string, string> dictionary = null;
			while (num != -1)
			{
				int num2 = text.IndexOf("}", num, StringComparison.Ordinal);
				if (num2 == -1)
				{
					break;
				}
				string text2 = text.Substring(num + 1, num2 - num - 1);
				string text3 = this.ParseTextElement(text2);
				if (text3 != text2)
				{
					if (dictionary == null)
					{
						dictionary = new Dictionary<string, string>();
					}
					dictionary.Add(text.Substring(num, num2 - num + 1), text3);
				}
				num = text.IndexOf("{", num2, StringComparison.Ordinal);
			}
			if (dictionary != null)
			{
				foreach (string text4 in dictionary.Keys)
				{
					text = text.Replace(text4, dictionary[text4]);
				}
			}
			return text;
		}

		public virtual BaseAction HandleAssignFrom(GameEventActionSequence newSeq, GameEventActionSequence oldSeq)
		{
			BaseAction baseAction = this.Clone();
			baseAction.Properties = new DynamicProperties();
			if (this.Properties != null)
			{
				baseAction.Properties.CopyFrom(this.Properties, null);
			}
			baseAction.Owner = newSeq;
			if (baseAction.Requirements != null)
			{
				for (int i = 0; i < baseAction.Requirements.Count; i++)
				{
					baseAction.Requirements[i].Properties = new DynamicProperties();
					if (this.Requirements[i].Properties != null)
					{
						baseAction.Requirements[i].Properties.CopyFrom(this.Requirements[i].Properties, null);
					}
					baseAction.Requirements[i].Owner = newSeq;
					baseAction.Requirements[i].Init();
				}
			}
			return baseAction;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static Dictionary<string, BaseAction> sLookupByKey = new Dictionary<string, BaseAction>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public GameEventActionSequence owner;

		public int Phase;

		public int PhaseOnComplete = -1;

		public int PhaseOnDenied = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string actionKey = "";

		public bool IgnoreRefund;

		public bool IsComplete;

		public DynamicProperties Properties;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPhase = "phase";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPhaseOnComplete = "phase_on_complete";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPhaseOnDenied = "phase_on_denied";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropIgnoreRefund = "ignore_refund";

		public List<BaseRequirement> Requirements;

		public enum ActionCompleteStates
		{
			InComplete,
			InCompleteRefund,
			RequirementsNotMet,
			Complete
		}
	}
}

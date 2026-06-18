using System;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class EntityNPC : EntityAlive
{
	public override string LocalizedEntityName
	{
		get
		{
			return Localization.Get(this.EntityName, false, null);
		}
	}

	public NPCInfo NPCInfo
	{
		get
		{
			if (this.npcID != "")
			{
				return NPCInfo.npcInfoList[this.npcID];
			}
			return null;
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[this.entityClass];
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropNPCID))
		{
			this.npcID = entityClass.Properties.Values[EntityClass.PropNPCID];
		}
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version < 33)
		{
			GameUtils.ReadItemStack(_br);
		}
	}

	public override bool IsSavedToFile()
	{
		return (base.GetSpawnerSource() != EnumSpawnerSource.Dynamic || this.IsDead()) && base.IsSavedToFile();
	}

	public override float GetSeeDistance()
	{
		return 80f;
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		bool flag = _distanceSqr < (float)(_masterIsZooming ? 14400 : 8100);
		this.emodel.SetVisible(flag, false);
	}

	public override bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		return this.world.GetPlayers().Count == 0 && base.canDespawn();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public virtual void PlayVoiceSetEntry(string name, EntityPlayer player, bool ignoreTime = true, bool showReactionAnim = true)
	{
	}

	public override string GetActivationText()
	{
		GameManager instance = GameManager.Instance;
		EntityPlayerLocal entityPlayerLocal;
		if (instance == null)
		{
			entityPlayerLocal = null;
		}
		else
		{
			World world = instance.World;
			entityPlayerLocal = ((world != null) ? world.GetPrimaryPlayer() : null);
		}
		EntityPlayerLocal entityPlayerLocal2 = entityPlayerLocal;
		if (entityPlayerLocal2 == null)
		{
			return string.Empty;
		}
		string text = entityPlayerLocal2.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + entityPlayerLocal2.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		return string.Format(Localization.Get("npcTooltipTalk", false, null), text, this.LocalizedEntityName);
	}

	public string npcID = "";
}

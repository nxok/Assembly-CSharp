using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTeleportToSpecial : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				World world = GameManager.Instance.World;
				this.position = Vector3.zero;
				switch (this.pointType)
				{
				case ActionTeleportToSpecial.SpecialPointTypes.Bedroll:
					if (entityPlayer.SpawnPoints.Count == 0)
					{
						return;
					}
					this.position = entityPlayer.SpawnPoints[0];
					break;
				case ActionTeleportToSpecial.SpecialPointTypes.Landclaim:
				{
					PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityPlayer.entityId);
					if (playerDataFromEntityID.LPBlocks == null || playerDataFromEntityID.LPBlocks.Count == 0)
					{
						return;
					}
					this.position = playerDataFromEntityID.LPBlocks[0];
					break;
				}
				case ActionTeleportToSpecial.SpecialPointTypes.Backpack:
				{
					Vector3i lastDroppedBackpackPosition = entityPlayer.GetLastDroppedBackpackPosition();
					if (lastDroppedBackpackPosition == Vector3i.zero)
					{
						return;
					}
					this.position = lastDroppedBackpackPosition;
					break;
				}
				}
				GameManager.Instance.StartCoroutine(this.handleTeleport(entityPlayer));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator handleTeleport(EntityPlayer player)
		{
			yield return new WaitForSeconds(this.teleportDelay);
			if (this.position.y > 0f)
			{
				this.position += Vector3.up * 2f;
				if (player.isEntityRemote)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(player.entityId).SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(this.position, null, false));
				}
				else
				{
					((EntityPlayerLocal)player).PlayerUI.windowManager.CloseAllOpenModalWindows(null, false);
					((EntityPlayerLocal)player).TeleportToPosition(this.position, false, null);
				}
			}
			yield break;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<ActionTeleportToSpecial.SpecialPointTypes>(ActionTeleportToSpecial.PropSpecialType, ref this.pointType);
			properties.ParseFloat(ActionTeleportToSpecial.PropTeleportDelay, ref this.teleportDelay);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTeleportToSpecial
			{
				targetGroup = this.targetGroup,
				pointType = this.pointType,
				teleportDelay = this.teleportDelay
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 position;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float teleportDelay = 0.1f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionTeleportToSpecial.SpecialPointTypes pointType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSpecialType = "special_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTeleportDelay = "teleport_delay";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum SpecialPointTypes
		{
			Bedroll,
			Landclaim,
			Backpack
		}
	}
}

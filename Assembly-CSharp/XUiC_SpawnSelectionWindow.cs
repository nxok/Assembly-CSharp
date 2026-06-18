using System;
using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnSelectionWindow : XUiController
{
	public XUiC_SpawnSelectionWindow.ESpawnWindowMode WindowMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.windowMode;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.windowMode != value)
			{
				this.windowMode = value;
				this.IsDirty = true;
			}
		}
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "spawn_sound")
		{
			this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _clip)
			{
				this.spawnReadyClip = _clip;
			});
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 3003985336U)
		{
			if (num != 79689725U)
			{
				if (num != 848152551U)
				{
					if (num == 3003985336U)
					{
						if (_bindingName == "progressVisible")
						{
							_value = (this.WindowMode == XUiC_SpawnSelectionWindow.ESpawnWindowMode.Progress).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "firstTimeSpawn")
				{
					_value = this.bFirstTimeSpawn.ToString();
					return true;
				}
			}
			else if (_bindingName == "hasBedroll")
			{
				_value = this.bHasBedroll.ToString();
				return true;
			}
		}
		else if (num <= 3221349377U)
		{
			if (num != 3073415995U)
			{
				if (num == 3221349377U)
				{
					if (_bindingName == "enteringGame")
					{
						_value = this.bEnteringGame.ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "showNearFriends")
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					_value = false.ToString();
				}
				else if (XUiC_SpawnNearFriendsList.SpawnNearFriendMode == AllowSpawnNearFriend.Disabled)
				{
					_value = false.ToString();
				}
				else
				{
					_value = this.bFirstTimeSpawn.ToString();
				}
				return true;
			}
		}
		else if (num != 4053612733U)
		{
			if (num == 4166775256U)
			{
				if (_bindingName == "buttonsVisible")
				{
					_value = (this.WindowMode == XUiC_SpawnSelectionWindow.ESpawnWindowMode.SpawnSelection).ToString();
					return true;
				}
			}
		}
		else if (_bindingName == "hasBackpack")
		{
			GameServerInfo currentGameServerInfoServerOrClient = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient;
			bool flag = currentGameServerInfoServerOrClient == null || currentGameServerInfoServerOrClient.GetValue(GameInfoBool.AllowSpawnNearBackpack);
			_value = (flag && this.bHasBackpack).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override void Init()
	{
		base.Init();
		XUiC_SpawnSelectionWindow.ID = base.WindowGroup.Id;
		this.lblProgress = (XUiV_Label)base.GetChildById("lblProgress").ViewComponent;
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnLeave") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += delegate(XUiController _, int _)
			{
				this.xui.playerUI.windowManager.Close(this.windowGroup, false);
				GameManager.Instance.Disconnect();
			};
		}
		XUiC_SimpleButton xuiC_SimpleButton2 = base.GetChildById("btnSpawnFirstTime") as XUiC_SimpleButton;
		if (xuiC_SimpleButton2 != null)
		{
			xuiC_SimpleButton2.OnPressed += delegate(XUiController _, int _)
			{
				this.SpawnButtonPressed(SpawnMethod.Invalid, -1);
			};
		}
		XUiC_SimpleButton xuiC_SimpleButton3 = base.GetChildById("btnSpawn") as XUiC_SimpleButton;
		if (xuiC_SimpleButton3 != null)
		{
			xuiC_SimpleButton3.OnPressed += delegate(XUiController _, int _)
			{
				this.SpawnButtonPressed(SpawnMethod.Invalid, -1);
			};
		}
		XUiC_SimpleButton xuiC_SimpleButton4 = base.GetChildById("btnRespawn") as XUiC_SimpleButton;
		if (xuiC_SimpleButton4 != null)
		{
			xuiC_SimpleButton4.OnPressed += delegate(XUiController _, int _)
			{
				if (GameStats.GetInt(EnumGameStats.DeathPenalty) == 3)
				{
					this.SpawnButtonPressed(SpawnMethod.NewRandomSpawn, -1);
					return;
				}
				this.SpawnButtonPressed(SpawnMethod.NearDeath, -1);
			};
		}
		XUiC_SimpleButton xuiC_SimpleButton5 = base.GetChildById("btnNearBackpack") as XUiC_SimpleButton;
		if (xuiC_SimpleButton5 != null)
		{
			xuiC_SimpleButton5.OnPressed += delegate(XUiController _, int _)
			{
				this.SpawnButtonPressed(SpawnMethod.NearBackpack, -1);
			};
		}
		XUiC_SimpleButton xuiC_SimpleButton6 = base.GetChildById("btnOnBedroll") as XUiC_SimpleButton;
		if (xuiC_SimpleButton6 != null)
		{
			xuiC_SimpleButton6.OnPressed += delegate(XUiController _, int _)
			{
				this.SpawnButtonPressed(SpawnMethod.OnBedRoll, -1);
			};
		}
		XUiC_SimpleButton xuiC_SimpleButton7 = base.GetChildById("btnNearBedroll") as XUiC_SimpleButton;
		if (xuiC_SimpleButton7 != null)
		{
			xuiC_SimpleButton7.OnPressed += delegate(XUiController _, int _)
			{
				this.SpawnButtonPressed(SpawnMethod.NearBedroll, -1);
			};
		}
		XUiC_SpawnNearFriendsList childByType = base.GetChildByType<XUiC_SpawnNearFriendsList>();
		if (childByType != null)
		{
			childByType.SpawnClicked += delegate(PersistentPlayerData _ppd)
			{
				this.SpawnButtonPressed(SpawnMethod.NearFriend, _ppd.EntityId);
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtons()
	{
		if (this.WindowMode == XUiC_SpawnSelectionWindow.ESpawnWindowMode.Progress && !this.bEnteringGame)
		{
			return;
		}
		if (this.bEnteringGame)
		{
			return;
		}
		World world = GameManager.Instance.World;
		EntityPlayerLocal entityPlayerLocal = ((world != null) ? world.GetPrimaryPlayer() : null);
		if (entityPlayerLocal == null)
		{
			Log.Warning("Refresh buttons cannot process without an EntityPlayerLocal");
			return;
		}
		SpawnPosition spawnPoint;
		Vector3i vector3i;
		if (entityPlayerLocal != null)
		{
			spawnPoint = entityPlayerLocal.GetSpawnPoint();
			vector3i = entityPlayerLocal.GetLastDroppedBackpackPosition();
		}
		else
		{
			spawnPoint = new SpawnPosition(Vector3.forward, 0f);
			vector3i = Vector3i.down;
		}
		this.bHasBedroll = !spawnPoint.IsUndef();
		this.bHasBackpack = vector3i != Vector3i.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnButtonPressed(SpawnMethod _method, int _nearEntityId = -1)
	{
		this.xui.playerUI.xui.PlayMenuConfirmSound();
		this.spawnMethod = _method;
		if (this.bEnteringGame)
		{
			this.WindowMode = XUiC_SpawnSelectionWindow.ESpawnWindowMode.Progress;
			this.xui.playerUI.CursorController.SetNavigationTarget(null);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.canSpawnPlayer = true;
				return;
			}
			GameManager.Instance.RequestToSpawn(_nearEntityId);
			if (_method == SpawnMethod.NearFriend)
			{
				ThreadManager.StartCoroutine(this.sendFriendRequest(_nearEntityId));
				return;
			}
		}
		else
		{
			World world = GameManager.Instance.World;
			EntityPlayerLocal entityPlayerLocal = ((world != null) ? world.GetPrimaryPlayer() : null);
			if (entityPlayerLocal == null)
			{
				Log.Warning("Spawn button cannot process without an EntityPlayerLocal");
				return;
			}
			SpawnPosition spawnPoint;
			Vector3i vector3i;
			Vector3 vector;
			if (entityPlayerLocal != null)
			{
				spawnPoint = entityPlayerLocal.GetSpawnPoint();
				vector3i = entityPlayerLocal.GetLastDroppedBackpackPosition();
				vector = entityPlayerLocal.position;
			}
			else
			{
				spawnPoint = new SpawnPosition(Vector3.forward, 0f);
				vector3i = Vector3i.down;
				vector = Vector3.down;
			}
			SpawnPosition spawnPosition = new SpawnPosition(vector3i, 0f);
			SpawnPosition spawnPosition2 = new SpawnPosition(vector, 0f);
			SpawnPosition randomSpawnPosition = GameManager.Instance.GetSpawnPointList().GetRandomSpawnPosition(entityPlayerLocal.world, null, 0, 0);
			SpawnPosition spawnPosition3;
			switch (this.spawnMethod)
			{
			case SpawnMethod.Invalid:
				spawnPosition3 = SpawnPosition.Undef;
				break;
			case SpawnMethod.NewRandomSpawn:
				spawnPosition3 = randomSpawnPosition;
				break;
			case SpawnMethod.NearDeath:
				spawnPosition3 = spawnPosition2;
				break;
			case SpawnMethod.OnBedRoll:
				spawnPosition3 = spawnPoint;
				break;
			case SpawnMethod.NearBedroll:
				spawnPosition3 = spawnPoint;
				break;
			case SpawnMethod.NearBackpack:
				spawnPosition3 = spawnPosition;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.spawnTarget = spawnPosition3;
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator sendFriendRequest(int _entityId)
	{
		if (_entityId == -1)
		{
			yield break;
		}
		while (GameManager.Instance.World != null)
		{
			EntityPlayerLocal epl = GameManager.Instance.World.GetPrimaryPlayer();
			yield return null;
			if (!(epl == null) && epl.Spawned)
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(_entityId) as EntityPlayer;
				if (entityPlayer != null)
				{
					if (!entityPlayer.partyInvites.Contains(epl))
					{
						entityPlayer.AddPartyInvite(epl.entityId);
					}
					NetPackagePartyActions netPackagePartyActions = NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, epl.entityId, _entityId, null, null);
					if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackagePartyActions, false);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackagePartyActions, false, -1, -1, -1, null, 192, false);
					}
				}
				epl.PlayerUI.xui.GetChildByType<XUiC_PlayersList>().AddInvitePress(_entityId);
				yield break;
			}
		}
		yield break;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.RefreshButtons();
			base.RefreshBindings();
			if (this.xui.playerUI.CursorController.navigationTarget == null && !this.xui.playerUI.CursorController.CursorModeActive)
			{
				base.GetChildById("content").SelectCursorElement(true, false);
			}
			this.IsDirty = false;
		}
		this.updateLoadState();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.delayCountdownTime = 1f;
		this.spawnMethod = SpawnMethod.Invalid;
		this.lblProgress.Text = Localization.Get("msgBuildingEnvironment", false, null);
		this.showSpawningComponents(XUiC_SpawnSelectionWindow.ESpawnWindowMode.Progress);
		base.RefreshBindings();
		((XUiV_Window)base.ViewComponent).ForceVisible(1f);
		this.setCursor = false;
		this.xui.playerUI.CursorController.SetCursorHidden(true);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.bChooseSpawnPosition = false;
		this.xui.playerUI.CursorController.SetCursorHidden(false);
		this.setCursor = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLoadState()
	{
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			if (this.bEnteringGame)
			{
				this.showSpawningComponents(XUiC_SpawnSelectionWindow.ESpawnWindowMode.SpawnSelection);
			}
			return;
		}
		if (GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
			return;
		}
		if (this.delayCountdownTime > 0f)
		{
			this.delayCountdownTime -= Time.deltaTime;
			return;
		}
		int displayedChunkGameObjectsCount = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount();
		int viewDistance = GameUtils.GetViewDistance();
		int num = ((!GameManager.Instance.World.ChunkCache.IsFixedSize) ? (viewDistance * viewDistance - 10) : 0);
		if (displayedChunkGameObjectsCount < num)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.lblProgress.Text = Localization.Get("msgStartingGame", false, null);
				return;
			}
		}
		else if (DistantTerrain.Instance != null && !DistantTerrain.Instance.IsTerrainReady)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				this.lblProgress.Text = Localization.Get("msgGeneratingDistantTerrain", false, null);
				return;
			}
		}
		else
		{
			if (!LocalPlayerUI.GetUIForPrimaryPlayer().xui.IsReady)
			{
				this.lblProgress.Text = Localization.Get("msgLoadingUI", false, null);
				return;
			}
			if (this.bChooseSpawnPosition && GameManager.Instance.World.GetPrimaryPlayer() != null)
			{
				this.showSpawningComponents(XUiC_SpawnSelectionWindow.ESpawnWindowMode.SpawnSelection);
				return;
			}
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showSpawningComponents(XUiC_SpawnSelectionWindow.ESpawnWindowMode _windowMode)
	{
		GameManager.Instance.SetCursorEnabledOverride(false, false);
		if (_windowMode == XUiC_SpawnSelectionWindow.ESpawnWindowMode.SpawnSelection && AutomationRunner.Instance.IsRunning)
		{
			this.SpawnButtonPressed(SpawnMethod.Invalid, -1);
			return;
		}
		if (!this.setCursor)
		{
			this.WindowMode = _windowMode;
			this.xui.playerUI.CursorController.SetCursorHidden(false);
			this.setCursor = true;
			if (this.WindowMode == XUiC_SpawnSelectionWindow.ESpawnWindowMode.SpawnSelection)
			{
				Manager.PlayXUiSound(this.spawnReadyClip, 1f);
			}
		}
	}

	public static void Open(LocalPlayerUI _playerUi, bool _chooseSpawnPosition, bool _enteringGame, bool _firstTimeSpawn = false)
	{
		XUiC_SpawnSelectionWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_SpawnSelectionWindow.ID).GetChildByType<XUiC_SpawnSelectionWindow>();
		childByType.bChooseSpawnPosition = _chooseSpawnPosition;
		childByType.bEnteringGame = _enteringGame;
		childByType.bFirstTimeSpawn = _firstTimeSpawn;
		_playerUi.windowManager.Open(XUiC_SpawnSelectionWindow.ID, true, true);
	}

	public static void Close(LocalPlayerUI _playerUi)
	{
		_playerUi.windowManager.Close(XUiC_SpawnSelectionWindow.ID);
	}

	public static bool IsOpenInUI(LocalPlayerUI _playerUi)
	{
		return _playerUi.windowManager.IsWindowOpen(XUiC_SpawnSelectionWindow.ID);
	}

	public static bool IsInSpawnSelection(LocalPlayerUI _playerUi)
	{
		return XUiC_SpawnSelectionWindow.IsOpenInUI(_playerUi) && XUiC_SpawnSelectionWindow.GetWindow(_playerUi).WindowMode == XUiC_SpawnSelectionWindow.ESpawnWindowMode.SpawnSelection;
	}

	public static XUiC_SpawnSelectionWindow GetWindow(LocalPlayerUI _playerUi)
	{
		return _playerUi.xui.FindWindowGroupByName(XUiC_SpawnSelectionWindow.ID).GetChildByType<XUiC_SpawnSelectionWindow>();
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SpawnSelectionWindow.ESpawnWindowMode windowMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bChooseSpawnPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEnteringGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFirstTimeSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHasBedroll;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHasBackpack;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayCountdownTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip spawnReadyClip;

	public SpawnMethod spawnMethod;

	public SpawnPosition spawnTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ESpawnWindowMode
	{
		Progress,
		SpawnSelection
	}
}

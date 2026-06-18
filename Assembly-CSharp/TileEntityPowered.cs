using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class TileEntityPowered : TileEntity, IPowered
{
	public int RequiredPower
	{
		get
		{
			if (this.needBlockData && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ushort num = (ushort)GameManager.Instance.World.GetBlock(base.ToWorldPos()).type;
				this.SetValuesFromBlock(num);
				this.needBlockData = false;
			}
			return this.requiredPower;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.requiredPower = value;
		}
	}

	public virtual int PowerUsed
	{
		get
		{
			return this.RequiredPower;
		}
	}

	public int ChildCount
	{
		get
		{
			return this.wireDataList.Count;
		}
	}

	public bool IsPlayerPlaced
	{
		get
		{
			return this.isPlayerPlaced;
		}
		set
		{
			this.isPlayerPlaced = value;
			this.setModified();
		}
	}

	public bool IsPowered
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return this.PowerItem != null && this.PowerItem.IsPowered;
			}
			return this.isPowered;
		}
	}

	public Transform BlockTransform
	{
		get
		{
			return this.blockTransform;
		}
		set
		{
			this.blockTransform = value;
			BlockValue block = GameManager.Instance.World.GetBlock(base.ToWorldPos());
			if (this.blockTransform != null)
			{
				Transform transform = this.blockTransform.Find("WireOffset");
				if (transform != null)
				{
					Vector3 vector = block.Block.shape.GetRotation(block) * transform.localPosition;
					this.WireOffset = vector;
					return;
				}
			}
			if (block.Block.Properties.Values.ContainsKey("WireOffset"))
			{
				Vector3 vector2 = block.Block.shape.GetRotation(block) * StringParsers.ParseVector3(block.Block.Properties.Values["WireOffset"], 0, -1);
				this.WireOffset = vector2;
			}
		}
	}

	public TileEntityPowered(Chunk _chunk)
		: base(_chunk)
	{
	}

	public Vector3i GetParent()
	{
		return this.parentPosition;
	}

	public bool HasParent()
	{
		return this.parentPosition.y != -9999;
	}

	public PowerItem GetPowerItem()
	{
		return this.PowerItem;
	}

	public override void OnReadComplete()
	{
		base.OnReadComplete();
		this.InitializePowerData();
		this.CheckForNewWires();
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		this.isPlayerPlaced = _br.ReadBoolean();
		this.PowerItemType = (PowerItem.PowerItemTypes)_br.ReadByte();
		this.needBlockData = true;
		int num2 = (int)_br.ReadByte();
		this.wireDataList.Clear();
		for (int i = 0; i < num2; i++)
		{
			Vector3i vector3i = StreamUtils.ReadVector3i(_br);
			this.wireDataList.Add(vector3i);
		}
		this.parentPosition = StreamUtils.ReadVector3i(_br);
		if (_eStreamMode == TileEntity.StreamModeRead.FromServer)
		{
			this.isPowered = _br.ReadBoolean();
		}
		this.activateDirty = true;
		this.wiresDirty = true;
		if (num > 0)
		{
			if (_eStreamMode == TileEntity.StreamModeRead.FromServer)
			{
				bool flag = false;
				GUIWindow guiwindow;
				if (LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.TryGetWindow(XUiC_PowerCameraWindowGroup.ID, out guiwindow))
				{
					XUiC_PowerCameraWindowGroup xuiC_PowerCameraWindowGroup = (XUiC_PowerCameraWindowGroup)((XUiWindowGroup)guiwindow).Controller;
					flag = base.IsUserAccessing() && xuiC_PowerCameraWindowGroup != null && xuiC_PowerCameraWindowGroup.TileEntity == this;
				}
				if (!flag)
				{
					this.CenteredPitch = _br.ReadSingle();
					this.CenteredYaw = _br.ReadSingle();
					return;
				}
				_br.ReadSingle();
				_br.ReadSingle();
				return;
			}
			else
			{
				this.CenteredPitch = _br.ReadSingle();
				this.CenteredYaw = _br.ReadSingle();
			}
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(1);
		_bw.Write(this.isPlayerPlaced);
		_bw.Write((byte)this.PowerItemType);
		_bw.Write((byte)this.wireDataList.Count);
		for (int i = 0; i < this.wireDataList.Count; i++)
		{
			StreamUtils.Write(_bw, this.wireDataList[i]);
		}
		StreamUtils.Write(_bw, this.parentPosition);
		if (_eStreamMode == TileEntity.StreamModeWrite.ToClient)
		{
			_bw.Write(this.IsPowered);
		}
		_bw.Write(this.CenteredPitch);
		_bw.Write(this.CenteredYaw);
	}

	public void CheckForNewWires()
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			for (int i = 0; i < this.wireDataList.Count; i++)
			{
				Vector3 vector = this.wireDataList[i].ToVector3();
				if (this.PowerItem.GetChild(vector) == null)
				{
					PowerItem powerItemByWorldPos = PowerManager.Instance.GetPowerItemByWorldPos(this.wireDataList[i]);
					PowerManager.Instance.SetParent(powerItemByWorldPos, this.PowerItem);
				}
			}
		}
	}

	public void DrawWires()
	{
		if (this.BlockTransform == null)
		{
			this.wiresDirty = true;
			return;
		}
		WireManager instance = WireManager.Instance;
		bool flag = instance.ShowPulse;
		bool wiresShowing = instance.WiresShowing;
		if (this.wireDataList.Count > 0)
		{
			World world = GameManager.Instance.World;
			if (flag)
			{
				flag = world.CanPlaceBlockAt(base.ToWorldPos(), world.gameManager.GetPersistentLocalPlayer(), false);
			}
		}
		for (int i = 0; i < this.wireDataList.Count; i++)
		{
			Vector3i vector3i = this.wireDataList[i];
			if (GameManager.Instance.World.GetChunkFromWorldPos(vector3i) is Chunk)
			{
				TileEntityPowered tileEntityPowered = GameManager.Instance.World.GetTileEntity(vector3i) as TileEntityPowered;
				bool flag2 = false;
				if (tileEntityPowered != null && tileEntityPowered.BlockTransform != null)
				{
					flag2 = true;
				}
				if (!flag2)
				{
					this.wiresDirty = true;
					return;
				}
			}
		}
		int num = 0;
		for (int j = 0; j < this.wireDataList.Count; j++)
		{
			Vector3i vector3i2 = this.wireDataList[j];
			if (GameManager.Instance.World.GetChunkFromWorldPos(vector3i2) is Chunk)
			{
				TileEntityPowered tileEntityPowered2 = GameManager.Instance.World.GetTileEntity(vector3i2) as TileEntityPowered;
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || !GameManager.IsDedicatedServer || tileEntityPowered2 == null || (this.PowerItemType == PowerItem.PowerItemTypes.TripWireRelay && tileEntityPowered2.PowerItemType == PowerItem.PowerItemTypes.TripWireRelay))
				{
					if (num >= this.currentWireNodes.Count)
					{
						IWireNode wireNodeFromPool = WireManager.Instance.GetWireNodeFromPool();
						this.currentWireNodes.Add(wireNodeFromPool);
					}
					this.currentWireNodes[num].SetStartPosition(this.BlockTransform.position + Origin.position);
					this.currentWireNodes[num].SetStartPositionOffset(this.WireOffset);
					if (tileEntityPowered2 != null)
					{
						if (this.PowerItemType == PowerItem.PowerItemTypes.ElectricWireRelay && tileEntityPowered2.PowerItemType == PowerItem.PowerItemTypes.ElectricWireRelay)
						{
							this.currentWireNodes[num].SetPulseColor(new Color32(0, 97, byte.MaxValue, byte.MaxValue));
							this.currentWireNodes[num].SetWireRadius(0.005f);
							this.currentWireNodes[num].SetWireDip(0f);
							ElectricWireController electricWireController = this.currentWireNodes[num].GetGameObject().GetComponent<ElectricWireController>();
							if (electricWireController == null)
							{
								electricWireController = this.currentWireNodes[num].GetGameObject().AddComponent<ElectricWireController>();
							}
							electricWireController.TileEntityParent = this as TileEntityPoweredMeleeTrap;
							electricWireController.TileEntityChild = tileEntityPowered2 as TileEntityPoweredMeleeTrap;
							electricWireController.WireNode = this.currentWireNodes[num];
							electricWireController.Init(this.chunk.GetBlock(base.localChunkPos).Block.Properties);
							electricWireController.WireNode.SetWireCanHide(false);
						}
						else if (this.PowerItemType == PowerItem.PowerItemTypes.TripWireRelay && tileEntityPowered2.PowerItemType == PowerItem.PowerItemTypes.TripWireRelay)
						{
							this.currentWireNodes[num].SetPulseColor(Color.magenta);
							this.currentWireNodes[num].SetWireRadius(0.0035f);
							this.currentWireNodes[num].SetWireDip(0f);
							TripWireController tripWireController = this.currentWireNodes[num].GetGameObject().GetComponent<TripWireController>();
							if (tripWireController == null)
							{
								tripWireController = this.currentWireNodes[num].GetGameObject().AddComponent<TripWireController>();
							}
							tripWireController.TileEntityParent = this as TileEntityPoweredTrigger;
							tripWireController.TileEntityChild = tileEntityPowered2 as TileEntityPoweredTrigger;
							tripWireController.WireNode = this.currentWireNodes[num];
							tripWireController.WireNode.SetWireCanHide(false);
						}
						else
						{
							Object.Destroy(this.currentWireNodes[num].GetGameObject().GetComponent<ElectricWireController>());
							Object.Destroy(this.currentWireNodes[num].GetGameObject().GetComponent<TripWireController>());
							this.currentWireNodes[num].SetWireCanHide(true);
						}
					}
					this.currentWireNodes[num].SetEndPosition(vector3i2.ToVector3());
					if (tileEntityPowered2 != null)
					{
						this.currentWireNodes[num].SetEndPositionOffset(tileEntityPowered2.WireOffset + new Vector3(0.5f, 0.5f, 0.5f));
					}
					this.currentWireNodes[num].BuildMesh();
					this.currentWireNodes[num].TogglePulse(flag);
					this.currentWireNodes[num].SetVisible(wiresShowing);
					num++;
				}
			}
		}
		for (int k = num; k < this.currentWireNodes.Count; k++)
		{
			IWireNode wireNode = this.currentWireNodes[num];
			WireManager.Instance.ReturnToPool(wireNode);
			this.currentWireNodes.Remove(wireNode);
		}
		this.wiresDirty = false;
	}

	public void AddWireData(Vector3i child)
	{
		this.wireDataList.Add(child);
		this.SendWireData();
	}

	public void SendWireData()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireActions>().Setup(NetPackageWireActions.WireActions.SendWires, base.ToWorldPos(), this.wireDataList, -1), false, -1, -1, -1, null, 192, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireActions>().Setup(NetPackageWireActions.WireActions.SendWires, base.ToWorldPos(), this.wireDataList, -1), false);
	}

	public void CreateWireDataFromPowerItem()
	{
		this.wireDataList.Clear();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			for (int i = 0; i < this.PowerItem.Children.Count; i++)
			{
				this.wireDataList.Add(this.PowerItem.Children[i].Position);
			}
		}
	}

	public void RemoveWires()
	{
		for (int i = 0; i < this.currentWireNodes.Count; i++)
		{
			WireManager.Instance.ReturnToPool(this.currentWireNodes[i]);
		}
		this.currentWireNodes.Clear();
	}

	public void MarkWireDirty()
	{
		this.wiresDirty = true;
	}

	public void MarkChanged()
	{
		base.SetModified();
	}

	public void InitializePowerData()
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		ushort num = (ushort)GameManager.Instance.World.GetBlock(base.ToWorldPos()).type;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.PowerItem = PowerManager.Instance.GetPowerItemByWorldPos(base.ToWorldPos());
			if (this.PowerItem == null)
			{
				this.CreatePowerItemForTileEntity(num);
			}
			else
			{
				num = this.PowerItem.BlockID;
			}
			this.PowerItem.AddTileEntity(this);
			base.SetModified();
			this.activateDirty = true;
		}
		this.SetValuesFromBlock(num);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.DrawWires();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetValuesFromBlock(ushort blockID)
	{
		if (Block.list[(int)blockID].Properties.Values.ContainsKey("RequiredPower"))
		{
			this.RequiredPower = Convert.ToInt32(Block.list[(int)blockID].Properties.Values["RequiredPower"]);
			return;
		}
		this.RequiredPower = 5;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (this.BlockTransform != null)
		{
			if (this.wiresDirty)
			{
				this.DrawWires();
			}
			if (this.activateDirty)
			{
				this.Activate(this.PowerItem.IsPowered);
				this.activateDirty = false;
			}
		}
	}

	public PowerItem CreatePowerItemForTileEntity(ushort blockID)
	{
		if (this.PowerItem == null)
		{
			this.PowerItem = this.CreatePowerItem();
			this.PowerItem.Position = base.ToWorldPos();
			this.PowerItem.BlockID = blockID;
			this.PowerItem.SetValuesFromBlock();
			PowerManager.Instance.AddPowerNode(this.PowerItem, null);
		}
		return this.PowerItem;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual PowerItem CreatePowerItem()
	{
		return PowerItem.CreateItem(this.PowerItemType);
	}

	public override void OnUnload(World world)
	{
		base.OnUnload(world);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.PowerItem.RemoveTileEntity(this);
		}
		this.RemoveWires();
	}

	public virtual bool Activate(bool activated)
	{
		return false;
	}

	public virtual bool ActivateOnce()
	{
		return false;
	}

	public Vector3 GetWireOffset()
	{
		return this.WireOffset;
	}

	public int GetRequiredPower()
	{
		return this.RequiredPower;
	}

	public virtual bool CanHaveParent(IPowered powered)
	{
		return true;
	}

	public void SetParentWithWireTool(IPowered newParentTE, int wiringEntityID)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PowerItem powerItem = newParentTE.GetPowerItem();
			PowerItem parent = this.PowerItem.Parent;
			PowerManager.Instance.SetParent(this.PowerItem, powerItem);
			if (parent != null && parent.TileEntity != null)
			{
				parent.TileEntity.CreateWireDataFromPowerItem();
				parent.TileEntity.SendWireData();
				parent.TileEntity.RemoveWires();
				parent.TileEntity.DrawWires();
			}
			newParentTE.CreateWireDataFromPowerItem();
			newParentTE.SendWireData();
			newParentTE.RemoveWires();
			newParentTE.DrawWires();
			Manager.BroadcastPlay(base.ToWorldPos().ToVector3(), powerItem.IsPowered ? "wire_live_connect" : "wire_dead_connect", 0f);
		}
		else
		{
			this.parentPosition = ((TileEntity)newParentTE).ToWorldPos();
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireActions>().Setup(NetPackageWireActions.WireActions.SetParent, base.ToWorldPos(), new List<Vector3i> { this.parentPosition }, wiringEntityID), false);
			Manager.BroadcastPlay(base.ToWorldPos().ToVector3(), this.IsPowered ? "wire_live_connect" : "wire_dead_connect", 0f);
		}
		base.SetModified();
	}

	public void RemoveParentWithWiringTool(int wiringEntityID)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (this.PowerItem.Parent != null)
			{
				Vector3i position = this.PowerItem.Parent.Position;
				PowerItem parent = this.PowerItem.Parent;
				this.PowerItem.RemoveSelfFromParent();
				if (parent.TileEntity != null)
				{
					parent.TileEntity.CreateWireDataFromPowerItem();
					parent.TileEntity.SendWireData();
					parent.TileEntity.RemoveWires();
					parent.TileEntity.DrawWires();
				}
				Manager.BroadcastPlay(position.ToVector3(), this.PowerItem.IsPowered ? "wire_live_break" : "wire_dead_break", 0f);
			}
		}
		else
		{
			Vector3i vector3i = base.ToWorldPos();
			Vector3 vector = vector3i.ToVector3();
			this.parentPosition = new Vector3i(-9999, -9999, -9999);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireActions>().Setup(NetPackageWireActions.WireActions.RemoveParent, vector3i, new List<Vector3i>(), wiringEntityID), false);
			Manager.BroadcastPlay(vector, this.IsPowered ? "wire_live_break" : "wire_dead_break", 0f);
		}
		base.SetModified();
	}

	public void SetWireData(List<Vector3i> wireChildren)
	{
		this.wireDataList = wireChildren;
		this.RemoveWires();
		this.DrawWires();
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Powered;
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_success)
		{
			GameManager.ShowTooltip(uiforPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		((XUiC_PoweredGenericWindowGroup)((XUiWindowGroup)uiforPrimaryPlayer.windowManager.GetWindow(this.WindowGroupToOpen)).Controller).TileEntity = this;
		uiforPrimaryPlayer.windowManager.Open(this.WindowGroupToOpen, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int ver = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool wiresDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPlayerPlaced;

	public PowerItem.PowerItemTypes PowerItemType = PowerItem.PowerItemTypes.Consumer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public PowerItem PowerItem;

	public Vector3 WireOffset = Vector3.zero;

	public float CenteredPitch;

	public float CenteredYaw;

	public string WindowGroupToOpen = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool needBlockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public int requiredPower;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPowered;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform blockTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IWireNode> currentWireNodes = new List<IWireNode>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> wireDataList = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activateDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i parentPosition = new Vector3i(-9999, -9999, -9999);
}

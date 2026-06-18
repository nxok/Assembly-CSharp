using System;
using System.Collections.Generic;
using UnityEngine;

public class WireManager
{
	public static WireManager Instance
	{
		get
		{
			if (WireManager.instance == null)
			{
				WireManager.instance = new WireManager();
			}
			return WireManager.instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return WireManager.instance != null;
		}
	}

	public Transform WireManagerRoot
	{
		get
		{
			return this.wireManagerRoot;
		}
	}

	public void Init()
	{
		this.activeWires = new HashSet<IWireNode>();
		this.activePulseObjects = new HashSet<GameObject>();
		GameObject gameObject = GameObject.Find("WireManager");
		if (gameObject == null)
		{
			this.wireManagerRoot = new GameObject("WireManager").transform;
		}
		else
		{
			this.wireManagerRoot = gameObject.transform;
		}
		this.wirePool = this.wireManagerRoot.Find("Pool");
		if (this.wirePool == null)
		{
			this.wirePool = new GameObject("Pool").transform;
			this.wirePool.parent = this.wireManagerRoot;
		}
		Origin.Add(this.wireManagerRoot.transform, 0);
		if (this.wirePool.transform.childCount == 0)
		{
			for (int i = 0; i < 200; i++)
			{
				this.addNewNode();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addNewNode()
	{
		GameObject gameObject;
		if (WireManager.USE_FAST_WIRE_NODES)
		{
			gameObject = (GameObject)Object.Instantiate(Resources.Load("Prefabs/WireNode2"));
		}
		else
		{
			gameObject = (GameObject)Object.Instantiate(Resources.Load("Prefabs/WireNode"));
		}
		Object @object = gameObject;
		string text = "WireNode_{0}";
		int num = this.wireIndex;
		this.wireIndex = num + 1;
		@object.name = string.Format(text, num.ToString());
		gameObject.SetActive(false);
		gameObject.transform.parent = this.wirePool;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
	}

	public void ReturnToPool(IWireNode wireNode)
	{
		this.activeWires.Remove(wireNode);
		GameObject gameObject = wireNode.GetGameObject();
		gameObject.transform.parent = this.wirePool;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.SetActive(false);
		wireNode.Reset();
	}

	public IWireNode GetWireNodeFromPool()
	{
		if (this.wirePool.childCount < 1)
		{
			this.addNewNode();
		}
		Transform child = this.wirePool.GetChild(this.wirePool.childCount - 1);
		child.gameObject.SetActive(true);
		child.parent = this.wireManagerRoot;
		IWireNode wireNode;
		if (WireManager.USE_FAST_WIRE_NODES)
		{
			wireNode = child.gameObject.GetComponent<FastWireNode>();
		}
		else
		{
			wireNode = child.gameObject.GetComponent<WireNode>();
		}
		this.activeWires.Add(wireNode);
		return wireNode;
	}

	public bool AddActiveWire(IWireNode wire)
	{
		return this.activeWires.Add(wire);
	}

	public bool RemoveActiveWire(IWireNode wire)
	{
		return this.activeWires.Remove(wire);
	}

	public bool AddPulseObject(GameObject pulseObject)
	{
		return this.activePulseObjects.Add(pulseObject);
	}

	public bool RemovePulseObject(GameObject pulseObject)
	{
		return this.activePulseObjects.Remove(pulseObject);
	}

	public void ToggleAllWirePulse(bool isPulseOn)
	{
		World world = GameManager.Instance.World;
		this.ShowPulse = isPulseOn;
		this.WiresShowing = isPulseOn;
		if (this.activeWires == null)
		{
			return;
		}
		if (this.ShowPulse)
		{
			Dictionary<Vector3, bool> dictionary = new Dictionary<Vector3, bool>(Vector3EqualityComparer.Instance);
			foreach (IWireNode wireNode in this.activeWires)
			{
				Vector3 startPosition = wireNode.GetStartPosition();
				bool flag;
				if (dictionary.ContainsKey(startPosition))
				{
					flag = dictionary[startPosition];
				}
				else
				{
					flag = world.CanPlaceBlockAt(new Vector3i(startPosition), world.gameManager.GetPersistentLocalPlayer(), false);
					dictionary[startPosition] = flag;
				}
				if (flag)
				{
					wireNode.TogglePulse(isPulseOn);
					wireNode.SetVisible(this.WiresShowing);
				}
				else
				{
					wireNode.SetVisible(false);
				}
			}
			dictionary.Clear();
			using (HashSet<GameObject>.Enumerator enumerator2 = this.activePulseObjects.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					GameObject gameObject = enumerator2.Current;
					Vector3i vector3i = new Vector3i(gameObject.transform.position);
					if (world.CanPlaceBlockAt(vector3i, world.gameManager.GetPersistentLocalPlayer(), false))
					{
						gameObject.SetActive(isPulseOn);
					}
					gameObject.layer = 0;
				}
				return;
			}
		}
		foreach (IWireNode wireNode2 in this.activeWires)
		{
			wireNode2.TogglePulse(false);
			wireNode2.SetVisible(false);
		}
		foreach (GameObject gameObject2 in this.activePulseObjects)
		{
			gameObject2.SetActive(isPulseOn);
			gameObject2.layer = 11;
		}
	}

	public void SetWirePulse(IWireNode node)
	{
		node.TogglePulse(this.ShowPulse);
	}

	public void RefreshPulseObjects()
	{
		foreach (GameObject gameObject in this.activePulseObjects)
		{
			gameObject.SetActive(this.ShowPulse);
			gameObject.layer = (this.ShowPulse ? 0 : 11);
		}
	}

	public void Cleanup()
	{
		foreach (IWireNode wireNode in this.activeWires)
		{
			Object.Destroy(wireNode.GetGameObject());
		}
		this.activeWires.Clear();
		for (int i = this.wirePool.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(this.wirePool.GetChild(i).gameObject);
		}
		Object.Destroy(this.wirePool.gameObject);
		Origin.Remove(this.wireManagerRoot);
		Object.Destroy(this.wireManagerRoot.gameObject);
		WireManager.instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool USE_FAST_WIRE_NODES = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WireManager instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public int wireIndex;

	public bool ShowPulse;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color standardPulseColor = new Color32(0, 97, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color tripWirePulseColor = Color.magenta;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<IWireNode> activeWires;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<GameObject> activePulseObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform wireManagerRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform wirePool;

	public bool WiresShowing;
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_Input : MonoBehaviour
{
	public static vp_Input Instance
	{
		get
		{
			if (vp_Input.mIsDirty)
			{
				vp_Input.mIsDirty = false;
				if (vp_Input.m_Instance == null)
				{
					if (Application.isPlaying)
					{
						GameObject gameObject = Resources.Load("Input/vp_Input") as GameObject;
						if (gameObject == null)
						{
							vp_Input.m_Instance = new GameObject("vp_Input").AddComponent<vp_Input>();
						}
						else
						{
							vp_Input.m_Instance = gameObject.GetComponent<vp_Input>();
							if (vp_Input.m_Instance == null)
							{
								vp_Input.m_Instance = gameObject.AddComponent<vp_Input>();
							}
						}
					}
					vp_Input.m_Instance.SetupDefaults("");
				}
			}
			return vp_Input.m_Instance;
		}
	}

	public static void CreateIfNoExist()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (vp_Input.m_Instance == null)
		{
			vp_Input.m_Instance = vp_Input.Instance;
		}
	}

	public virtual void SetDirty(bool dirty)
	{
		vp_Input.mIsDirty = dirty;
	}

	public virtual void SetupDefaults(string type = "")
	{
		if ((type == "" || type == "Buttons") && this.ButtonKeys.Count == 0)
		{
			this.AddButton("Attack", 323);
			this.AddButton("SetNextWeapon", 101);
			this.AddButton("SetPrevWeapon", 113);
			this.AddButton("ClearWeapon", 8);
			this.AddButton("Zoom", 324);
			this.AddButton("Reload", 114);
			this.AddButton("Jump", 32);
			this.AddButton("Crouch", 99);
			this.AddButton("Run", 304);
			this.AddButton("Interact", 102);
			this.AddButton("Accept1", 13);
			this.AddButton("Accept2", 271);
			this.AddButton("Pause", 112);
			this.AddButton("Menu", 27);
		}
		if ((type == "" || type == "Axis") && this.AxisKeys.Count == 0)
		{
			this.AddAxis("Vertical", 119, 115);
			this.AddAxis("Horizontal", 100, 97);
		}
		if ((type == "" || type == "UnityAxis") && this.UnityAxis.Count == 0)
		{
			this.AddUnityAxis("Mouse X");
			this.AddUnityAxis("Mouse Y");
		}
		this.UpdateDictionaries();
	}

	public virtual void AddButton(string n, KeyCode k = 0)
	{
		if (this.ButtonKeys.Contains(n))
		{
			this.ButtonValues[this.ButtonKeys.IndexOf(n)] = k;
			return;
		}
		this.ButtonKeys.Add(n);
		this.ButtonValues.Add(k);
	}

	public virtual void AddAxis(string n, KeyCode pk = 0, KeyCode nk = 0)
	{
		if (this.AxisKeys.Contains(n))
		{
			this.AxisValues[this.AxisKeys.IndexOf(n)] = new vp_Input.vp_InputAxis
			{
				Positive = pk,
				Negative = nk
			};
			return;
		}
		this.AxisKeys.Add(n);
		this.AxisValues.Add(new vp_Input.vp_InputAxis
		{
			Positive = pk,
			Negative = nk
		});
	}

	public virtual void AddUnityAxis(string n)
	{
		if (this.UnityAxis.Contains(n))
		{
			this.UnityAxis[this.UnityAxis.IndexOf(n)] = n;
			return;
		}
		this.UnityAxis.Add(n);
	}

	public virtual void UpdateDictionaries()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		this.Buttons.Clear();
		for (int i = 0; i < this.ButtonKeys.Count; i++)
		{
			this.Buttons.Add(this.ButtonKeys[i], this.ButtonValues[i]);
		}
		this.Axis.Clear();
		for (int j = 0; j < this.AxisKeys.Count; j++)
		{
			this.Axis.Add(this.AxisKeys[j], new vp_Input.vp_InputAxis
			{
				Positive = this.AxisValues[j].Positive,
				Negative = this.AxisValues[j].Negative
			});
		}
	}

	public static bool GetButtonAny(string button)
	{
		return vp_Input.Instance.DoGetButtonAny(button);
	}

	public virtual bool DoGetButtonAny(string button)
	{
		if (this.Buttons.ContainsKey(button))
		{
			return Input.GetKey(this.Buttons[button]) || Input.GetKeyDown(this.Buttons[button]) || Input.GetKeyUp(this.Buttons[button]);
		}
		Debug.LogError("\"" + button + "\" is not in VP Input Manager's Buttons. You must add it for this Button to work.");
		return false;
	}

	public static bool GetButton(string button)
	{
		return vp_Input.Instance.DoGetButton(button);
	}

	public virtual bool DoGetButton(string button)
	{
		if (this.Buttons.ContainsKey(button))
		{
			return Input.GetKey(this.Buttons[button]);
		}
		Debug.LogError("\"" + button + "\" is not in VP Input Manager's Buttons. You must add it for this Button to work.");
		return false;
	}

	public static bool GetButtonDown(string button)
	{
		return vp_Input.Instance.DoGetButtonDown(button);
	}

	public virtual bool DoGetButtonDown(string button)
	{
		if (this.Buttons.ContainsKey(button))
		{
			return Input.GetKeyDown(this.Buttons[button]);
		}
		Debug.LogError("\"" + button + "\" is not in VP Input Manager's Buttons. You must add it for this Button to work.");
		return false;
	}

	public static bool GetButtonUp(string button)
	{
		return vp_Input.Instance.DoGetButtonUp(button);
	}

	public virtual bool DoGetButtonUp(string button)
	{
		if (this.Buttons.ContainsKey(button))
		{
			return Input.GetKeyUp(this.Buttons[button]);
		}
		Debug.LogError("\"" + button + "\" is not in VP Input Manager's Buttons. You must add it for this Button to work.");
		return false;
	}

	public static float GetAxisRaw(string axis)
	{
		return vp_Input.Instance.DoGetAxisRaw(axis);
	}

	public virtual float DoGetAxisRaw(string axis)
	{
		if (this.Axis.ContainsKey(axis) && this.ControlType == 0)
		{
			float num = 0f;
			if (Input.GetKey(this.Axis[axis].Positive))
			{
				num = 1f;
			}
			if (Input.GetKey(this.Axis[axis].Negative))
			{
				num = -1f;
			}
			return num;
		}
		if (this.UnityAxis.Contains(axis))
		{
			return Input.GetAxisRaw(axis);
		}
		Debug.LogError("\"" + axis + "\" is not in VP Input Manager's Unity Axis. You must add it for this Axis to work.");
		return 0f;
	}

	public static void ChangeButtonKey(string button, KeyCode keyCode, bool save = false)
	{
		if (!vp_Input.Instance.Buttons.ContainsKey(button))
		{
			Debug.LogWarning("The Button \"" + button + "\" Doesn't Exist");
			return;
		}
		if (save)
		{
			vp_Input.Instance.ButtonValues[vp_Input.Instance.ButtonKeys.IndexOf(button)] = keyCode;
		}
		vp_Input.Instance.Buttons[button] = keyCode;
	}

	public static void ChangeAxis(string n, KeyCode pk = 0, KeyCode nk = 0, bool save = false)
	{
		if (!vp_Input.Instance.AxisKeys.Contains(n))
		{
			Debug.LogWarning("The Axis \"" + n + "\" Doesn't Exist");
			return;
		}
		if (save)
		{
			vp_Input.Instance.AxisValues[vp_Input.Instance.AxisKeys.IndexOf(n)] = new vp_Input.vp_InputAxis
			{
				Positive = pk,
				Negative = nk
			};
		}
		vp_Input.Instance.Axis[n] = new vp_Input.vp_InputAxis
		{
			Positive = pk,
			Negative = nk
		};
	}

	public int ControlType;

	public Dictionary<string, KeyCode> Buttons = new Dictionary<string, KeyCode>();

	public List<string> ButtonKeys = new List<string>();

	public List<KeyCode> ButtonValues = new List<KeyCode>();

	public Dictionary<string, vp_Input.vp_InputAxis> Axis = new Dictionary<string, vp_Input.vp_InputAxis>();

	public List<string> AxisKeys = new List<string>();

	public List<vp_Input.vp_InputAxis> AxisValues = new List<vp_Input.vp_InputAxis>();

	public List<string> UnityAxis = new List<string>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static string m_FolderPath = "UltimateFPS/Content/Resources/Input";

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static string m_PrefabPath = "Assets/UltimateFPS/Content/Resources/Input/vp_Input.prefab";

	public static bool mIsDirty = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static vp_Input m_Instance;

	[Serializable]
	public class vp_InputAxis
	{
		public KeyCode Positive;

		public KeyCode Negative;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class NguiWdwTerrainEditor : MonoBehaviour, INGuiButtonOnClick
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.nguiWindowManager = base.GetComponentInParent<NGUIWindowManager>();
		this.gm = (GameManager)Object.FindAnyObjectByType(typeof(GameManager));
		if (!GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			base.gameObject.SetActive(false);
			return;
		}
		this.thisWindow = base.transform;
		this.toolGrid = this.thisWindow.Find("Toolbox/ToolGrid");
		this.sizeVal = this.thisWindow.Find("Toolbox/Sliders/1_Size/Value").GetComponent<UILabel>();
		this.falloffVal = this.thisWindow.Find("Toolbox/Sliders/2_Falloff/Value").GetComponent<UILabel>();
		this.strengthVal = this.thisWindow.Find("Toolbox/Sliders/3_Strength/Value").GetComponent<UILabel>();
		this.anchor = this.thisWindow.GetComponent<UIAnchor>();
		this.tools = new List<IBlockTool>();
		this.brush = new BlockTools.Brush(BlockTools.Brush.BrushShape.Sphere, 1, 10, 80);
		this.toolButtonPrefab = this.thisWindow.Find("Toolbox/ToolButton").gameObject;
		this.toolButtons = new List<Transform>();
		this.tools.Add(new BlockToolTerrainAdjust(this.brush, this));
		this.tools.Add(new BlockToolTerrainSmoothing(this.brush, this));
		this.tools.Add(new BlockToolTerrainPaint(this.brush, this));
		GameObject gameObject = Resources.Load("Prefabs/prefabTerrainBrush") as GameObject;
		if (gameObject == null)
		{
			return;
		}
		this.projectorParent = Object.Instantiate<GameObject>(gameObject).transform;
		for (int i = 0; i < this.tools.Count; i++)
		{
			GameObject gameObject2 = Object.Instantiate<GameObject>(this.toolButtonPrefab);
			gameObject2.SetActive(true);
			gameObject2.name = this.tools[i].ToString() + " Button";
			Transform transform = gameObject2.transform;
			transform.parent = this.toolGrid;
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
			transform.GetComponent<NGuiButtonOnClickHandler>().OnClickDelegate = this;
			this.toolButtons.Add(transform);
		}
		this.toolGrid.GetComponent<UIGrid>().repositionNow = true;
		this.gm.SetActiveBlockTool(this.tools[0]);
		this.panel = base.GetComponent<UIPanel>();
	}

	public void InGameMenuOpen(bool _isOpen)
	{
		if (_isOpen)
		{
			this.anchor.pixelOffset = new Vector2(150f, -7f);
			return;
		}
		this.anchor.pixelOffset = new Vector2(0f, -7f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
		if (this.projectorParent != null)
		{
			this.projectorParent.gameObject.SetActive(true);
		}
		if (this.gm != null)
		{
			this.gm.SetActiveBlockTool(this.tools[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDisable()
	{
		if (this.projectorParent != null)
		{
			this.projectorParent.gameObject.SetActive(false);
		}
		if (this.gm != null)
		{
			this.gm.SetActiveBlockTool(null);
		}
	}

	public void OnSizeChange()
	{
		float value = UIProgressBar.current.value;
		this.size = (int)(value * 32f);
		this.brush.Falloff = this.size;
		this.brush.Size = (int)((float)this.size * this.hardness);
		this.sizeVal.text = this.size.ToString();
	}

	public void OnFalloffChange()
	{
		float value = UIProgressBar.current.value;
		this.hardness = value;
		this.brush.Falloff = this.size;
		this.brush.Size = (int)((float)this.size * this.hardness);
		this.falloffVal.text = this.hardness.ToCultureInvariantString();
	}

	public void OnStrengthChange()
	{
		float value = UIProgressBar.current.value;
		this.flow = value;
		this.brush.Strength = (int)(this.flow * 127f);
		this.strengthVal.text = this.flow.ToCultureInvariantString();
	}

	public void HideWindow(bool _hide)
	{
		if (_hide)
		{
			this.panel.alpha = 0f;
			return;
		}
		this.panel.alpha = 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (this.projectorParent != null)
		{
			this.projectorParent.parent = null;
			this.projectorParent.position = this.lastPosition.ToVector3();
			this.projectorParent.localScale = Vector3.one;
			this.projectorParent.Find("Size").transform.localScale = Vector3.one * (float)(this.brush.Size * 2);
			this.projectorParent.Find("Falloff").transform.localScale = Vector3.one * (float)(this.brush.Falloff * 2);
		}
		this.InGameMenuOpen(this.nguiWindowManager.WindowManager.IsWindowOpen(XUiC_InGameMenuWindow.ID));
	}

	public void NGuiButtonOnClick(Transform _t)
	{
		for (int i = 0; i < this.toolButtons.Count; i++)
		{
			if (_t == this.toolButtons[i])
			{
				this.gm.SetActiveBlockTool(this.tools[i]);
				Log.Out(this.tools[i].ToString());
			}
			this.toolButtons[i].Find("Highlight").gameObject.SetActive(_t == this.toolButtons[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameManager gm;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<IBlockTool> tools;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Transform> toolButtons;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BlockTools.Brush brush;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject toolButtonPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform thisWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform toolGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform projectorParent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Projector brushSizeProjector;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Projector brushFalloffPojector;

	public Vector3i lastPosition;

	public Vector3 lastDirection;

	public Texture2D[] buttonTextures;

	public string[] buttonNames;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UILabel sizeVal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UILabel falloffVal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UILabel strengthVal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIAnchor anchor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIPanel panel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int size;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float hardness;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float flow;
}

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_Component : MonoBehaviour
{
	public vp_EventHandler EventHandler
	{
		get
		{
			if (this.m_EventHandler == null)
			{
				this.m_EventHandler = (vp_EventHandler)this.Transform.GetComponentInChildren(typeof(vp_EventHandler));
			}
			if (this.m_EventHandler == null && this.Transform.parent != null)
			{
				this.m_EventHandler = (vp_EventHandler)this.Transform.parent.GetComponentInChildren(typeof(vp_EventHandler));
			}
			if (this.m_EventHandler == null && this.Transform.parent != null && this.Transform.parent.parent != null)
			{
				this.m_EventHandler = (vp_EventHandler)this.Transform.parent.parent.GetComponentInChildren(typeof(vp_EventHandler));
			}
			if (this.m_EventHandler == null)
			{
				this.m_EventHandler = (vp_EventHandler)this.Transform.GetComponentInParent(typeof(vp_EventHandler));
			}
			return this.m_EventHandler;
		}
	}

	public Type Type
	{
		get
		{
			if (this.m_Type == null)
			{
				this.m_Type = base.GetType();
			}
			return this.m_Type;
		}
	}

	public FieldInfo[] Fields
	{
		get
		{
			if (this.m_Fields == null)
			{
				this.m_Fields = this.Type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			}
			return this.m_Fields;
		}
	}

	public vp_StateManager StateManager
	{
		get
		{
			if (this.m_StateManager == null)
			{
				this.m_StateManager = new vp_StateManager(this, this.States);
			}
			return this.m_StateManager;
		}
	}

	public vp_State DefaultState
	{
		get
		{
			return this.m_DefaultState;
		}
	}

	public float Delta
	{
		get
		{
			return Time.deltaTime * 60f;
		}
	}

	public float SDelta
	{
		get
		{
			return Time.smoothDeltaTime * 60f;
		}
	}

	public Transform Transform
	{
		get
		{
			if (this.m_Transform == null)
			{
				this.m_Transform = base.transform;
			}
			return this.m_Transform;
		}
	}

	public Transform Parent
	{
		get
		{
			if (this.m_Parent == null)
			{
				this.m_Parent = base.transform.parent;
			}
			return this.m_Parent;
		}
	}

	public Transform Root
	{
		get
		{
			if (this.m_Root == null)
			{
				this.m_Root = base.transform.root;
			}
			return this.m_Root;
		}
	}

	public AudioSource Audio
	{
		get
		{
			if (this.m_Audio == null)
			{
				this.m_Audio = base.GetComponent<AudioSource>();
			}
			return this.m_Audio;
		}
	}

	public Collider Collider
	{
		get
		{
			if (this.m_Collider == null)
			{
				this.m_Collider = base.GetComponent<Collider>();
			}
			return this.m_Collider;
		}
	}

	public bool Rendering
	{
		get
		{
			if (this.Renderers != null)
			{
				foreach (Renderer renderer in this.Renderers)
				{
					if (renderer != null && renderer.enabled)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}
		set
		{
			foreach (Renderer renderer in this.Renderers)
			{
				if (!(renderer == null))
				{
					renderer.enabled = value;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.CacheChildren();
		this.CacheSiblings();
		this.CacheFamily();
		this.CacheRenderers();
		this.CacheAudioSources();
		this.StateManager.SetState("Default", base.enabled);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.ResetState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Init()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.EventHandler != null)
		{
			this.EventHandler.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (this.EventHandler != null)
		{
			this.EventHandler.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (!this.m_Initialized)
		{
			this.Init();
			this.m_Initialized = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedUpdate()
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if (GameManager.Instance.World.GetPrimaryPlayer() == null)
		{
			return;
		}
		Color color = Color.white;
		color.a = 1f;
		if (color == Color.black)
		{
			color.a = 1f;
			if (color == Color.black)
			{
				color = Color.gray;
			}
		}
		if (this.prevSkinColor == color)
		{
			return;
		}
		this.prevSkinColor = color;
		foreach (Renderer renderer in this.Renderers)
		{
			if (!(renderer == null) && renderer.enabled)
			{
				renderer.material.SetColor("Tint", color);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
	}

	public void SetState(string state, bool enabled = true, bool recursive = false, bool includeDisabled = false)
	{
		this.StateManager.SetState(state, enabled);
		if (recursive)
		{
			foreach (vp_Component vp_Component in this.Children)
			{
				if (includeDisabled || (vp_Utility.IsActive(vp_Component.gameObject) && vp_Component.enabled))
				{
					vp_Component.SetState(state, enabled, true, includeDisabled);
				}
			}
		}
	}

	public void ActivateGameObject(bool setActive = true)
	{
		if (setActive)
		{
			this.Activate();
			foreach (vp_Component vp_Component in this.Siblings)
			{
				vp_Component.Activate();
			}
			this.VerifyRenderers();
			return;
		}
		this.DeactivateWhenSilent();
		foreach (vp_Component vp_Component2 in this.Siblings)
		{
			vp_Component2.DeactivateWhenSilent();
		}
	}

	public void ResetState()
	{
		this.StateManager.Reset();
		this.Refresh();
	}

	public bool StateEnabled(string stateName)
	{
		return this.StateManager.IsEnabled(stateName);
	}

	public void RefreshDefaultState()
	{
		vp_State vp_State = null;
		if (this.States.Count == 0)
		{
			vp_State = new vp_State(this.Type.Name, "Default", null, null);
			this.States.Add(vp_State);
		}
		else
		{
			for (int i = this.States.Count - 1; i > -1; i--)
			{
				if (this.States[i].Name == "Default")
				{
					vp_State = this.States[i];
					this.States.Remove(vp_State);
					this.States.Add(vp_State);
				}
			}
			if (vp_State == null)
			{
				vp_State = new vp_State(this.Type.Name, "Default", null, null);
				this.States.Add(vp_State);
			}
		}
		if (vp_State.Preset == null || vp_State.Preset.ComponentType == null)
		{
			vp_State.Preset = new vp_ComponentPreset();
		}
		if (vp_State.TextAsset == null)
		{
			vp_State.Preset.InitFromComponent(this);
		}
		vp_State.Enabled = true;
		this.m_DefaultState = vp_State;
	}

	public void ApplyPreset(vp_ComponentPreset preset)
	{
		vp_ComponentPreset.Apply(this, preset);
		this.RefreshDefaultState();
		this.Refresh();
	}

	public vp_ComponentPreset Load(string path)
	{
		vp_ComponentPreset vp_ComponentPreset = vp_ComponentPreset.LoadFromResources(this, path);
		this.RefreshDefaultState();
		this.Refresh();
		return vp_ComponentPreset;
	}

	public vp_ComponentPreset Load(TextAsset asset)
	{
		vp_ComponentPreset vp_ComponentPreset = vp_ComponentPreset.LoadFromTextAsset(this, asset);
		this.RefreshDefaultState();
		this.Refresh();
		return vp_ComponentPreset;
	}

	public void CacheChildren()
	{
		this.Children.Clear();
		foreach (vp_Component vp_Component in base.GetComponentsInChildren<vp_Component>(true))
		{
			if (vp_Component.transform.parent == base.transform)
			{
				this.Children.Add(vp_Component);
			}
		}
	}

	public void CacheSiblings()
	{
		this.Siblings.Clear();
		foreach (vp_Component vp_Component in base.GetComponents<vp_Component>())
		{
			if (vp_Component != this)
			{
				this.Siblings.Add(vp_Component);
			}
		}
	}

	public void CacheFamily()
	{
		this.Family.Clear();
		foreach (vp_Component vp_Component in base.transform.root.GetComponentsInChildren<vp_Component>(true))
		{
			if (vp_Component != this)
			{
				this.Family.Add(vp_Component);
			}
		}
	}

	public void CacheRenderers()
	{
		this.Renderers.Clear();
		foreach (Renderer renderer in base.GetComponentsInChildren<Renderer>(true))
		{
			this.Renderers.Add(renderer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void VerifyRenderers()
	{
		if (this.Renderers.Count == 0)
		{
			return;
		}
		if (this.Renderers[0] == null || !vp_Utility.IsDescendant(this.Renderers[0].transform, this.Transform))
		{
			this.Renderers.Clear();
			this.CacheRenderers();
		}
	}

	public void CacheAudioSources()
	{
		this.AudioSources.Clear();
		foreach (AudioSource audioSource in base.GetComponentsInChildren<AudioSource>(true))
		{
			this.AudioSources.Add(audioSource);
		}
	}

	public virtual void Activate()
	{
		this.m_DeactivationTimer.Cancel();
		vp_Utility.Activate(base.gameObject, true);
	}

	public virtual void Deactivate()
	{
		vp_Utility.Activate(base.gameObject, false);
	}

	public void DeactivateWhenSilent()
	{
		if (this == null)
		{
			return;
		}
		if (vp_Utility.IsActive(base.gameObject))
		{
			foreach (AudioSource audioSource in this.AudioSources)
			{
				if (audioSource.isPlaying && !audioSource.loop)
				{
					this.Rendering = false;
					vp_Timer.In(0.1f, delegate
					{
						this.DeactivateWhenSilent();
					}, this.m_DeactivationTimer);
					return;
				}
			}
		}
		this.Deactivate();
	}

	public virtual void Refresh()
	{
	}

	public bool Persist;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_StateManager m_StateManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_EventHandler m_EventHandler;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_State m_DefaultState;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Initialized;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Parent;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Root;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_Audio;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Collider m_Collider;

	public List<vp_State> States = new List<vp_State>();

	public List<vp_Component> Children = new List<vp_Component>();

	public List<vp_Component> Siblings = new List<vp_Component>();

	public List<vp_Component> Family = new List<vp_Component>();

	public List<Renderer> Renderers = new List<Renderer>();

	public List<AudioSource> AudioSources = new List<AudioSource>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Type m_Type;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public FieldInfo[] m_Fields;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_DeactivationTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color prevSkinColor = Color.white;
}

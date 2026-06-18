using System;
using UnityEngine;

public abstract class ScriptBase : MonoBehaviour
{
	public Transform transform
	{
		get
		{
			if (this.myTransform != null)
			{
				return this.myTransform;
			}
			return this.myTransform = base.transform;
		}
	}

	public GameObject gameObject
	{
		get
		{
			if (this.myGameObject != null)
			{
				return this.myGameObject;
			}
			return this.myGameObject = base.gameObject;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.className = base.GetType().Name;
		this.sbAwake();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void sbAwake()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		this.sbUpdate();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void sbUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedUpdate()
	{
		this.sbFixedUpdate();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void sbFixedUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		this.sbLateUpdate();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void sbLateUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ScriptBase()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform myTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject myGameObject;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string className;
}

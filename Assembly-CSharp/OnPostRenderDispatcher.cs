using System;
using System.Collections.Generic;
using UnityEngine;

public class OnPostRenderDispatcher : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		OnPostRenderDispatcher.Instance = this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnPostRender()
	{
		for (int i = 0; i < this.listeners.Count; i++)
		{
			this.listeners[i].OnPostRender();
		}
	}

	public void Add(IOnPostRender _p)
	{
		this.listeners.Add(_p);
	}

	public void Remove(IOnPostRender _p)
	{
		this.listeners.Remove(_p);
	}

	public static OnPostRenderDispatcher Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<IOnPostRender> listeners = new List<IOnPostRender>();
}

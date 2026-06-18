using System;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (this.IsPersistant)
		{
			if (!SingletonMonoBehaviour<T>.Instance)
			{
				SingletonMonoBehaviour<T>.Instance = (T)((object)this);
				SingletonMonoBehaviour<T>.Instance.singletonCreated();
				Object.DontDestroyOnLoad(base.gameObject);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			SingletonMonoBehaviour<T>.Instance = (T)((object)this);
			SingletonMonoBehaviour<T>.Instance.singletonCreated();
		}
		SingletonMonoBehaviour<T>.Instance.singletonAwake();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDestroy()
	{
		if (this == SingletonMonoBehaviour<T>.Instance)
		{
			this.singletonDestroy();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void singletonAwake()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void singletonCreated()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void singletonDestroy()
	{
	}

	public static T Instance;

	public bool IsPersistant;
}

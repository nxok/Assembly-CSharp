using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class vp_Timer : MonoBehaviour
{
	public bool WasAddedCorrectly
	{
		get
		{
			return Application.isPlaying && !(base.gameObject != vp_Timer.m_MainObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (!this.WasAddedCorrectly)
		{
			Object.Destroy(this);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		vp_Timer.m_EventBatch = 0;
		while (vp_Timer.m_Active.Count > 0 && vp_Timer.m_EventBatch < vp_Timer.MaxEventsPerFrame)
		{
			if (vp_Timer.m_EventIterator < 0)
			{
				vp_Timer.m_EventIterator = vp_Timer.m_Active.Count - 1;
				return;
			}
			if (vp_Timer.m_EventIterator > vp_Timer.m_Active.Count - 1)
			{
				vp_Timer.m_EventIterator = vp_Timer.m_Active.Count - 1;
			}
			if (Time.time >= vp_Timer.m_Active[vp_Timer.m_EventIterator].DueTime || vp_Timer.m_Active[vp_Timer.m_EventIterator].Id == 0)
			{
				vp_Timer.m_Active[vp_Timer.m_EventIterator].Execute();
			}
			else if (vp_Timer.m_Active[vp_Timer.m_EventIterator].Paused)
			{
				vp_Timer.m_Active[vp_Timer.m_EventIterator].DueTime += Time.deltaTime;
			}
			else
			{
				vp_Timer.m_Active[vp_Timer.m_EventIterator].LifeTime += Time.deltaTime;
			}
			vp_Timer.m_EventIterator--;
			vp_Timer.m_EventBatch++;
		}
	}

	public static void In(float delay, vp_Timer.Callback callback, vp_Timer.Handle timerHandle = null)
	{
		vp_Timer.Schedule(delay, callback, null, null, timerHandle, 1, -1f);
	}

	public static void In(float delay, vp_Timer.Callback callback, int iterations, vp_Timer.Handle timerHandle = null)
	{
		vp_Timer.Schedule(delay, callback, null, null, timerHandle, iterations, -1f);
	}

	public static void In(float delay, vp_Timer.Callback callback, int iterations, float interval, vp_Timer.Handle timerHandle = null)
	{
		vp_Timer.Schedule(delay, callback, null, null, timerHandle, iterations, interval);
	}

	public static void In(float delay, vp_Timer.ArgCallback callback, object arguments, vp_Timer.Handle timerHandle = null)
	{
		vp_Timer.Schedule(delay, null, callback, arguments, timerHandle, 1, -1f);
	}

	public static void In(float delay, vp_Timer.ArgCallback callback, object arguments, int iterations, vp_Timer.Handle timerHandle = null)
	{
		vp_Timer.Schedule(delay, null, callback, arguments, timerHandle, iterations, -1f);
	}

	public static void In(float delay, vp_Timer.ArgCallback callback, object arguments, int iterations, float interval, vp_Timer.Handle timerHandle = null)
	{
		vp_Timer.Schedule(delay, null, callback, arguments, timerHandle, iterations, interval);
	}

	public static void Start(vp_Timer.Handle timerHandle)
	{
		vp_Timer.Schedule(315360000f, delegate
		{
		}, null, null, timerHandle, 1, -1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Schedule(float time, vp_Timer.Callback func, vp_Timer.ArgCallback argFunc, object args, vp_Timer.Handle timerHandle, int iterations, float interval)
	{
		if (func == null && argFunc == null)
		{
			Debug.LogError("Error: (vp_Timer) Aborted event because function is null.");
			return;
		}
		if (vp_Timer.m_MainObject == null)
		{
			vp_Timer.m_MainObject = new GameObject("Timers");
			vp_Timer.m_MainObject.AddComponent<vp_Timer>();
			Object.DontDestroyOnLoad(vp_Timer.m_MainObject);
		}
		time = Mathf.Max(0f, time);
		iterations = Mathf.Max(0, iterations);
		interval = ((interval == -1f) ? time : Mathf.Max(0f, interval));
		vp_Timer.m_NewEvent = null;
		if (vp_Timer.m_Pool.Count > 0)
		{
			vp_Timer.m_NewEvent = vp_Timer.m_Pool[0];
			vp_Timer.m_Pool.Remove(vp_Timer.m_NewEvent);
		}
		else
		{
			vp_Timer.m_NewEvent = new vp_Timer.Event();
		}
		vp_Timer.m_EventCount++;
		vp_Timer.m_NewEvent.Id = vp_Timer.m_EventCount;
		if (func != null)
		{
			vp_Timer.m_NewEvent.Function = func;
		}
		else if (argFunc != null)
		{
			vp_Timer.m_NewEvent.ArgFunction = argFunc;
			vp_Timer.m_NewEvent.Arguments = args;
		}
		vp_Timer.m_NewEvent.StartTime = Time.time;
		vp_Timer.m_NewEvent.DueTime = Time.time + time;
		vp_Timer.m_NewEvent.Iterations = iterations;
		vp_Timer.m_NewEvent.Interval = interval;
		vp_Timer.m_NewEvent.LifeTime = 0f;
		vp_Timer.m_NewEvent.Paused = false;
		vp_Timer.m_Active.Add(vp_Timer.m_NewEvent);
		if (timerHandle != null)
		{
			if (timerHandle.Active)
			{
				timerHandle.Cancel();
			}
			timerHandle.Id = vp_Timer.m_NewEvent.Id;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Cancel(vp_Timer.Handle handle)
	{
		if (handle == null)
		{
			return;
		}
		if (handle.Active)
		{
			handle.Id = 0;
			return;
		}
	}

	public static void CancelAll()
	{
		for (int i = vp_Timer.m_Active.Count - 1; i > -1; i--)
		{
			vp_Timer.m_Active[i].Id = 0;
		}
	}

	public static void CancelAll(string methodName)
	{
		for (int i = vp_Timer.m_Active.Count - 1; i > -1; i--)
		{
			if (vp_Timer.m_Active[i].MethodName == methodName)
			{
				vp_Timer.m_Active[i].Id = 0;
			}
		}
	}

	public static void DestroyAll()
	{
		vp_Timer.m_Active.Clear();
		vp_Timer.m_Pool.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.NotifyLevelWasLoaded);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.NotifyLevelWasLoaded);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NotifyLevelWasLoaded(Scene scene, LoadSceneMode mode)
	{
		for (int i = vp_Timer.m_Active.Count - 1; i > -1; i--)
		{
			if (vp_Timer.m_Active[i].CancelOnLoad)
			{
				vp_Timer.m_Active[i].Id = 0;
			}
		}
	}

	public static vp_Timer.Stats EditorGetStats()
	{
		vp_Timer.Stats stats;
		stats.Created = vp_Timer.m_Active.Count + vp_Timer.m_Pool.Count;
		stats.Inactive = vp_Timer.m_Pool.Count;
		stats.Active = vp_Timer.m_Active.Count;
		return stats;
	}

	public static string EditorGetMethodInfo(int eventIndex)
	{
		if (eventIndex < 0 || eventIndex > vp_Timer.m_Active.Count - 1)
		{
			return "Argument out of range.";
		}
		return vp_Timer.m_Active[eventIndex].MethodInfo;
	}

	public static int EditorGetMethodId(int eventIndex)
	{
		if (eventIndex < 0 || eventIndex > vp_Timer.m_Active.Count - 1)
		{
			return 0;
		}
		return vp_Timer.m_Active[eventIndex].Id;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameObject m_MainObject = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<vp_Timer.Event> m_Active = new List<vp_Timer.Event>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<vp_Timer.Event> m_Pool = new List<vp_Timer.Event>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static vp_Timer.Event m_NewEvent = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int m_EventCount = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int m_EventBatch = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int m_EventIterator = 0;

	public static int MaxEventsPerFrame = 500;

	public delegate void Callback();

	public delegate void ArgCallback(object args);

	public struct Stats
	{
		public int Created;

		public int Inactive;

		public int Active;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Event
	{
		public void Execute()
		{
			if (this.Id == 0 || this.DueTime == 0f)
			{
				this.Recycle();
				return;
			}
			if (this.Function != null)
			{
				this.Function();
			}
			else
			{
				if (this.ArgFunction == null)
				{
					this.Error("Aborted event because function is null.");
					this.Recycle();
					return;
				}
				this.ArgFunction(this.Arguments);
			}
			if (this.Iterations > 0)
			{
				this.Iterations--;
				if (this.Iterations < 1)
				{
					this.Recycle();
					return;
				}
			}
			this.DueTime = Time.time + this.Interval;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Recycle()
		{
			this.Id = 0;
			this.DueTime = 0f;
			this.StartTime = 0f;
			this.CancelOnLoad = true;
			this.Function = null;
			this.ArgFunction = null;
			this.Arguments = null;
			if (vp_Timer.m_Active.Remove(this))
			{
				vp_Timer.m_Pool.Add(this);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Destroy()
		{
			vp_Timer.m_Active.Remove(this);
			vp_Timer.m_Pool.Remove(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Error(string message)
		{
			Debug.LogError("Error: (" + ((this != null) ? this.ToString() : null) + ") " + message);
		}

		public string MethodName
		{
			get
			{
				if (this.Function != null)
				{
					if (this.Function.Method != null)
					{
						if (this.Function.Method.Name[0] == '<')
						{
							return "delegate";
						}
						return this.Function.Method.Name;
					}
				}
				else if (this.ArgFunction != null && this.ArgFunction.Method != null)
				{
					if (this.ArgFunction.Method.Name[0] == '<')
					{
						return "delegate";
					}
					return this.ArgFunction.Method.Name;
				}
				return null;
			}
		}

		public string MethodInfo
		{
			get
			{
				string text = this.MethodName;
				if (!string.IsNullOrEmpty(text))
				{
					text += "(";
					if (this.Arguments != null)
					{
						if (this.Arguments.GetType().IsArray)
						{
							object[] array = (object[])this.Arguments;
							foreach (object obj in array)
							{
								text += obj.ToString();
								if (Array.IndexOf<object>(array, obj) < array.Length - 1)
								{
									text += ", ";
								}
							}
						}
						else
						{
							string text2 = text;
							object arguments = this.Arguments;
							text = text2 + ((arguments != null) ? arguments.ToString() : null);
						}
					}
					text += ")";
				}
				else
				{
					text = "(function = null)";
				}
				return text;
			}
		}

		public int Id;

		public vp_Timer.Callback Function;

		public vp_Timer.ArgCallback ArgFunction;

		public object Arguments;

		public int Iterations = 1;

		public float Interval = -1f;

		public float DueTime;

		public float StartTime;

		public float LifeTime;

		public bool Paused;

		public bool CancelOnLoad = true;
	}

	public class Handle
	{
		public bool Paused
		{
			get
			{
				return this.Active && this.m_Event.Paused;
			}
			set
			{
				if (this.Active)
				{
					this.m_Event.Paused = value;
				}
			}
		}

		public float TimeOfInitiation
		{
			get
			{
				if (this.Active)
				{
					return this.m_Event.StartTime;
				}
				return 0f;
			}
		}

		public float TimeOfFirstIteration
		{
			get
			{
				if (this.Active)
				{
					return this.m_FirstDueTime;
				}
				return 0f;
			}
		}

		public float TimeOfNextIteration
		{
			get
			{
				if (this.Active)
				{
					return this.m_Event.DueTime;
				}
				return 0f;
			}
		}

		public float TimeOfLastIteration
		{
			get
			{
				if (this.Active)
				{
					return Time.time + this.DurationLeft;
				}
				return 0f;
			}
		}

		public float Delay
		{
			get
			{
				return Mathf.Round((this.m_FirstDueTime - this.TimeOfInitiation) * 1000f) / 1000f;
			}
		}

		public float Interval
		{
			get
			{
				if (this.Active)
				{
					return this.m_Event.Interval;
				}
				return 0f;
			}
		}

		public float TimeUntilNextIteration
		{
			get
			{
				if (this.Active)
				{
					return this.m_Event.DueTime - Time.time;
				}
				return 0f;
			}
		}

		public float DurationLeft
		{
			get
			{
				if (this.Active)
				{
					return this.TimeUntilNextIteration + (float)(this.m_Event.Iterations - 1) * this.m_Event.Interval;
				}
				return 0f;
			}
		}

		public float DurationTotal
		{
			get
			{
				if (this.Active)
				{
					return this.Delay + (float)this.m_StartIterations * ((this.m_StartIterations > 1) ? this.Interval : 0f);
				}
				return 0f;
			}
		}

		public float Duration
		{
			get
			{
				if (this.Active)
				{
					return this.m_Event.LifeTime;
				}
				return 0f;
			}
		}

		public int IterationsTotal
		{
			get
			{
				return this.m_StartIterations;
			}
		}

		public int IterationsLeft
		{
			get
			{
				if (this.Active)
				{
					return this.m_Event.Iterations;
				}
				return 0;
			}
		}

		public int Id
		{
			get
			{
				return this.m_Id;
			}
			set
			{
				this.m_Id = value;
				if (this.m_Id == 0)
				{
					this.m_Event.DueTime = 0f;
					return;
				}
				this.m_Event = null;
				for (int i = vp_Timer.m_Active.Count - 1; i > -1; i--)
				{
					if (vp_Timer.m_Active[i].Id == this.m_Id)
					{
						this.m_Event = vp_Timer.m_Active[i];
						break;
					}
				}
				if (this.m_Event == null)
				{
					Debug.LogError(string.Concat(new string[]
					{
						"Error: (",
						(this != null) ? this.ToString() : null,
						") Failed to assign event with Id '",
						this.m_Id.ToString(),
						"'."
					}));
				}
				this.m_StartIterations = this.m_Event.Iterations;
				this.m_FirstDueTime = this.m_Event.DueTime;
			}
		}

		public bool Active
		{
			get
			{
				return this.m_Event != null && this.Id != 0 && this.m_Event.Id != 0 && this.m_Event.Id == this.Id;
			}
		}

		public string MethodName
		{
			get
			{
				if (this.Active)
				{
					return this.m_Event.MethodName;
				}
				return "";
			}
		}

		public string MethodInfo
		{
			get
			{
				if (this.Active)
				{
					return this.m_Event.MethodInfo;
				}
				return "";
			}
		}

		public bool CancelOnLoad
		{
			get
			{
				return !this.Active || this.m_Event.CancelOnLoad;
			}
			set
			{
				if (this.Active)
				{
					this.m_Event.CancelOnLoad = value;
					return;
				}
				Debug.LogWarning("Warning: (" + ((this != null) ? this.ToString() : null) + ") Tried to set CancelOnLoad on inactive timer handle.");
			}
		}

		public void Cancel()
		{
			vp_Timer.Cancel(this);
		}

		public void Execute()
		{
			this.m_Event.DueTime = Time.time;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public vp_Timer.Event m_Event;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_Id;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_StartIterations = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public float m_FirstDueTime;
	}
}

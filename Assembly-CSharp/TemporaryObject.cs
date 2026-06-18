using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class TemporaryObject : MonoBehaviour
{
	public void SetLife(float _life)
	{
		this.life = _life;
		float num = Utils.FastMax(_life - 1f, 0.1f);
		float num2 = 0.1f;
		ParticleSystem[] componentsInChildren = base.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			float duration = componentsInChildren[i].main.duration;
			if (duration > num2)
			{
				num2 = duration;
			}
		}
		float num3 = num / num2;
		float num4 = num2 * 0.5f;
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			float duration2 = main.duration;
			if (duration2 >= num4)
			{
				particleSystem.Stop(false);
				main.duration = duration2 * num3;
				ParticleSystem.MinMaxCurve startDelay = main.startDelay;
				ParticleSystemCurveMode particleSystemCurveMode = startDelay.mode;
				if (particleSystemCurveMode != null)
				{
					if (particleSystemCurveMode != 3)
					{
						startDelay.curveMultiplier *= num3;
					}
					else
					{
						startDelay.constantMin *= num3;
						startDelay.constantMax *= num3;
					}
				}
				else
				{
					startDelay.constant *= num3;
				}
				main.startDelay = startDelay;
				ParticleSystem.MinMaxCurve startLifetime = main.startLifetime;
				particleSystemCurveMode = startLifetime.mode;
				if (particleSystemCurveMode != null)
				{
					if (particleSystemCurveMode != 3)
					{
						startLifetime.curveMultiplier *= num3;
					}
					else
					{
						startLifetime.constantMin *= num3;
						startLifetime.constantMax *= num3;
					}
				}
				else
				{
					startLifetime.constant *= num3;
				}
				main.startLifetime = startLifetime;
				particleSystem.Play(false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		this.coroutine = base.StartCoroutine(this.DestroyLater());
	}

	public void Restart()
	{
		base.gameObject.SetActive(true);
		if (this.coroutine != null)
		{
			base.StopCoroutine(this.coroutine);
		}
		this.Start();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DestroyLater()
	{
		yield return new WaitForSeconds(this.life);
		if (this.destroyMaterials)
		{
			Utils.CleanupMaterialsOfRenderers<Renderer[]>(base.transform.GetComponentsInChildren<Renderer>());
		}
		Object.Destroy(base.gameObject);
		yield break;
	}

	[Conditional("DEBUG_TEMPOBJ")]
	public void LogTO(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} TemporaryObject {1}, id {2}, life {3}, {4}", new object[]
		{
			GameManager.frameCount,
			base.gameObject.GetGameObjectPath(),
			base.gameObject.GetInstanceID(),
			this.life,
			_format
		});
		Log.Warning(_format, _args);
	}

	public float life = 2f;

	public bool destroyMaterials;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Coroutine coroutine;
}

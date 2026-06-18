using System;
using UnityEngine;

namespace JBooth.MicroSplat
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Light))]
	public class GlitterLight : MonoBehaviour
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public void OnEnable()
		{
			this.lght = base.GetComponent<Light>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDisable()
		{
			this.lght = base.GetComponent<Light>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			Shader.SetGlobalVector("_gGlitterLightDir", -base.transform.forward);
			Shader.SetGlobalVector("_gGlitterLightWorldPos", base.transform.position);
			if (this.lght != null)
			{
				Shader.SetGlobalColor("_gGlitterLightColor", this.lght.color);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Light lght;
	}
}

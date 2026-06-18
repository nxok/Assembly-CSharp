using System;
using UnityEngine;

namespace PostEffects
{
	public sealed class NoiseTextureSet : ScriptableObject
	{
		public Texture2D GetTexture()
		{
			return this.GetTexture(Time.frameCount);
		}

		public Texture2D GetTexture(int frameCount)
		{
			return this._textures[frameCount % this._textures.Length];
		}

		[SerializeField]
		[PublicizedFrom(EAccessModifier.Private)]
		public Texture2D[] _textures;
	}
}

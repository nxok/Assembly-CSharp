using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FeatherFlutter : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.ps = base.GetComponent<ParticleSystem>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		int particleCount = this.ps.particleCount;
		if (particleCount == 0)
		{
			return;
		}
		if (this.particles == null || this.particles.Length < particleCount)
		{
			this.particles = new ParticleSystem.Particle[particleCount];
		}
		this.ps.GetParticles(this.particles, particleCount);
		for (int i = 0; i < particleCount; i++)
		{
			if (this.particles[i].velocity.y < 0f)
			{
				float num = this.particles[i].randomSeed % 1000U / 1000f * this.phaseSpread;
				float num2 = this.particles[i].startLifetime - this.particles[i].remainingLifetime;
				float num3 = Mathf.Clamp01(-this.particles[i].velocity.y / this.fallBlendSpeed);
				float num4 = Mathf.Sin(num2 * this.frequency + num) * this.amplitude * num3;
				float num5 = Mathf.Sin(num2 * this.zFrequency + num + 1.5f) * this.zAmplitude * num3;
				Vector3 velocity = this.particles[i].velocity;
				velocity.x = Mathf.Lerp(velocity.x, num4, 1f - this.lateralDrag);
				velocity.z = Mathf.Lerp(velocity.z, num5, 1f - this.lateralDrag);
				this.particles[i].velocity = velocity;
				float num6 = -velocity.z * this.rotationInfluence;
				float num7 = velocity.x * this.rotationInfluence;
				Vector3 rotation3D = this.particles[i].rotation3D;
				rotation3D.x = Mathf.Lerp(rotation3D.x, num6, 1f - this.rotationDrag);
				rotation3D.z = Mathf.Lerp(rotation3D.z, num7, 1f - this.rotationDrag);
				this.particles[i].rotation3D = rotation3D;
			}
		}
		this.ps.SetParticles(this.particles, particleCount);
	}

	public float amplitude = 0.5f;

	public float frequency = 1.5f;

	public float phaseSpread = 6f;

	public float fallBlendSpeed = 0.5f;

	public float lateralDrag = 0.85f;

	public float zAmplitude = 0.3f;

	public float zFrequency = 1.1f;

	public float rotationInfluence = 25f;

	public float rotationDrag = 0.9f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem ps;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem.Particle[] particles;
}

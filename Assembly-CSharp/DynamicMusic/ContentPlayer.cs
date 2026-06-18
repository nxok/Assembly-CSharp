using System;

namespace DynamicMusic
{
	public abstract class ContentPlayer : IPlayable
	{
		public virtual float Volume { get; set; } = 1f;

		public virtual bool IsDone
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public virtual bool IsPaused
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public virtual bool IsPlaying
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public virtual bool IsReady
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public abstract void Init();

		public virtual void Play()
		{
			this.IsDone = false;
			this.IsPlaying = true;
			this.IsPaused = false;
		}

		public virtual void Pause()
		{
			this.IsPlaying = false;
			this.IsPaused = true;
		}

		public virtual void UnPause()
		{
			this.IsPlaying = true;
			this.IsPaused = false;
		}

		public virtual void Stop()
		{
			this.IsDone = true;
			this.IsPlaying = false;
			this.IsPaused = false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ContentPlayer()
		{
		}
	}
}

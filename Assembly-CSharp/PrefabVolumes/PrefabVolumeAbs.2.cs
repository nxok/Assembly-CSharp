using System;

namespace PrefabVolumes
{
	public abstract class PrefabVolumeAbs<T> : PrefabVolumeAbs where T : PrefabVolumeAbs<T>, new()
	{
		public override PrefabVolumeAbs Clone()
		{
			return this.CloneGeneric();
		}

		public virtual T CloneGeneric()
		{
			T t = new T();
			this.CopyValues(t, false);
			return t;
		}

		public override void CopyValues(PrefabVolumeAbs _target, bool _nonBasicOnly = false)
		{
			T t = _target as T;
			if (t == null)
			{
				throw new ArgumentException("Can not copy values to volume of type " + _target.VolumeType.ToStringCached<PrefabVolumeAbs.EVolumeType>() + " from volume of type " + this.VolumeType.ToStringCached<PrefabVolumeAbs.EVolumeType>(), "_target");
			}
			this.CopyValues(t, _nonBasicOnly);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void CopyValues(PrefabVolumeAbs<T> _target, bool _nonBasicOnly = false)
		{
			_target.used = base.Used;
			_target.startPos = this.startPos;
			_target.size = this.size;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public PrefabVolumeAbs()
		{
		}
	}
}

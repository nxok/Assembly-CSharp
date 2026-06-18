using System;

namespace GamePath
{
	public class PathNavigate
	{
		public PathNavigate(EntityAlive _ea)
		{
			this.theEntity = _ea;
			this.inWater = false;
			this.canDrown = false;
		}

		public void setMoveSpeed(float _b)
		{
			this.speed = _b;
		}

		public void setCanDrown(bool _b)
		{
			this.canDrown = _b;
		}

		public bool getCanDrown()
		{
			return this.canDrown;
		}

		public bool noPath()
		{
			return this.currentPath == null || this.currentPath.isFinished();
		}

		public bool isPlanningPath()
		{
			return PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId);
		}

		public bool noPathAndNotPlanningOne()
		{
			return this.noPath() && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId);
		}

		public bool HasPath()
		{
			return this.currentPath != null && !this.currentPath.isFinished();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool canNavigate()
		{
			return this.theEntity.CanNavigatePath();
		}

		public void clearPath()
		{
			if (this.currentPath != null)
			{
				this.currentPath.Destruct();
			}
			this.currentPath = null;
		}

		public PathEntity getPath()
		{
			return this.currentPath;
		}

		public void ShortenEnd(float _distance)
		{
			if (this.currentPath != null)
			{
				this.currentPath.ShortenEnd(_distance);
			}
		}

		public virtual void GetPathTo(PathInfo _pathInfo)
		{
		}

		public virtual void GetPathToEntity(PathInfo _pathInfo, EntityAlive _entity)
		{
		}

		public virtual bool SetPath(PathInfo _pathInfo, float _speed)
		{
			return false;
		}

		public virtual void UpdateNavigation()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void CreatePath()
		{
		}

		public PathInfo pathInfo;

		[PublicizedFrom(EAccessModifier.Protected)]
		public EntityAlive theEntity;

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathEntity currentPath;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float speed;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool canBreakBlocks;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int curNavTicks;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int prevNavTicks;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool inWater;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool canDrown;
	}
}

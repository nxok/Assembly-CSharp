using System;
using UnityEngine;

namespace WaterClippingTool
{
	[Serializable]
	public class ShapeSettings : IEquatable<ShapeSettings>
	{
		public bool hasPlane
		{
			get
			{
				return this.plane != WaterClippingPlanePlacer.DisabledPlaneVec;
			}
		}

		public ShapeSettings()
		{
			this.ResetToDefault();
		}

		public void ResetToDefault()
		{
			this.shapeName = string.Empty;
			this.shapeModel = null;
			this.modelOffset = WaterClippingPlanePlacer.DefaultModelOffset;
			this.plane = WaterClippingPlanePlacer.DisabledPlaneVec;
			this.waterFlowMask = BlockFaceFlag.All;
		}

		public void CopyFrom(ShapeSettings other)
		{
			this.shapeName = other.shapeName;
			this.shapeModel = other.shapeModel;
			this.modelOffset = other.modelOffset;
			this.plane = other.plane;
			this.waterFlowMask = other.waterFlowMask;
		}

		public bool Equals(ShapeSettings other)
		{
			return !(this.plane != other.plane) && !(this.shapeName != other.shapeName) && !(this.shapeModel != other.shapeModel) && !(this.modelOffset != other.modelOffset) && this.waterFlowMask == other.waterFlowMask;
		}

		public string shapeName;

		public GameObject shapeModel;

		public Vector3 modelOffset;

		public Vector4 plane;

		public BlockFaceFlag waterFlowMask = BlockFaceFlag.All;
	}
}

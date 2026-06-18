using System;
using UnityEngine;

namespace CoverClippingTool
{
	[Serializable]
	public class BlockSettings
	{
		public BlockSettings()
		{
			this.ResetToDefault();
		}

		public void ResetToDefault()
		{
			this.shapeModel = null;
			this.savedShapeInfo = null;
		}

		public GameObject shapeModel;

		public BlockShapeInfo savedShapeInfo;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverClippingTool
{
	[Serializable]
	public struct SelectedBlockInfo
	{
		public BlockShapeInfo shapeInfo
		{
			get
			{
				return this._shapeInfo;
			}
			set
			{
				this._shapeInfo = value;
				BlockShapeInfo shapeInfo = this._shapeInfo;
				this._savedShapeReference.Name = ((shapeInfo != null) ? shapeInfo.Name : null);
				this._savedShapeReference.Source = ((this._shapeInfo != null) ? this._shapeInfo.Source : DataSource.Block);
			}
		}

		public ShapeReference savedShapeReference
		{
			get
			{
				return this._savedShapeReference;
			}
		}

		public SelectedBlockInfo(int thisIsDumb)
		{
			this._shapeInfo = null;
			this._savedShapeReference = default(ShapeReference);
			this.renderables = new List<Renderable>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public BlockShapeInfo _shapeInfo;

		[SerializeField]
		[PublicizedFrom(EAccessModifier.Private)]
		public ShapeReference _savedShapeReference;

		public readonly List<Renderable> renderables;
	}
}

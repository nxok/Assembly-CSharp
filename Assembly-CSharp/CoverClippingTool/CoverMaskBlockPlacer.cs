using System;
using UnityEngine;

namespace CoverClippingTool
{
	public class CoverMaskBlockPlacer : MonoBehaviour
	{
		public void UpdateSelectedMultiBlock(string value)
		{
			this.editMultiBlock = !value.Equals("All", StringComparison.OrdinalIgnoreCase);
			this.selectedMultiBlock = (this.editMultiBlock ? StringParsers.ParseVector3i(value, 0, -1, false) : Vector3i.zero);
		}

		public static readonly string PropCoverMask = "CoverMask";

		public static readonly string PropCoverMaskMulti = "CoverMaskMulti";

		public static readonly string PropCoverOffset = "CoverOffset";

		[SerializeField]
		public SelectedBlockInfo selectedModel = new SelectedBlockInfo(0);

		[SerializeField]
		public Vector3i selectedMultiBlock;

		[SerializeField]
		public bool editMultiBlock;

		[SerializeField]
		public string searchFilter;
	}
}

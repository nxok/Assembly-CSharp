using System;
using System.Collections.Generic;

namespace WorldGenerationEngineFinal
{
	public class StampGroup
	{
		public StampGroup(string _name)
		{
			this.Name = _name;
			this.Stamps = new List<Stamp>();
		}

		public string Name;

		public List<Stamp> Stamps;
	}
}

using System;
using MusicUtils.Enums;

namespace DynamicMusic.Legacy.ObjectModel
{
	public class ThreatLevel : EnumDictionary<LayerType, Layer>
	{
		public ThreatLevel(double _tempo, double _sigHi, double _sigLo)
		{
			this.Tempo = _tempo;
			this.SignatureHi = _sigHi;
			this.SignatureLo = _sigLo;
		}

		public readonly double Tempo;

		public readonly double SignatureHi;

		public readonly double SignatureLo;
	}
}

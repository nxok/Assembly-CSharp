using System;

namespace Platform
{
	public struct CensoredTextRequest
	{
		public readonly string Input { get; }

		public int CensoredLength { readonly get; set; }

		public readonly Action<CensoredTextResult> Callback { get; }

		public CensoredTextRequest(string _input, Action<CensoredTextResult> _callback)
		{
			this.Input = _input;
			this.CensoredLength = _input.Length;
			this.Callback = _callback;
		}
	}
}

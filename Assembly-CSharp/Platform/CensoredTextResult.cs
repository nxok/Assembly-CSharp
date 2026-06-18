using System;

namespace Platform
{
	public struct CensoredTextResult
	{
		public readonly bool Success { get; }

		public readonly string OriginalText { get; }

		public readonly string CensoredText { get; }

		public CensoredTextResult(bool _success, string _originalText, string _censoredText)
		{
			this.Success = _success;
			this.OriginalText = _originalText;
			this.CensoredText = _censoredText;
		}
	}
}

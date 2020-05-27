namespace elFinder.NetCore.Helpers
{
	// https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types
	public class MimeType
	{
		public string Type { get; set; }
		public string Subtype { get; set; }
		public string Full
		{
			get
			{
				return $"{Type}/{Subtype}";
			}
		}

		public static implicit operator string(MimeType m) => m.Full;

		public override string ToString() => Full;
	}
}

namespace CI.HttpClient
{
	public class UploadStatusMessage
	{
		public long ContentLength { get; set; }

		public long TotalContentUploaded { get; set; }

		public long ContentUploadedThisRound { get; set; }

		public int PercentageComplete
		{
			get
			{
				return (int)(TotalContentUploaded / ContentLength * 100.0);
			}
		}
	}
}

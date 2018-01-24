namespace NRun.WindowsService
{
	public sealed class WindowsServiceInstallSettings
	{
		/// <summary>
		/// The service name.
		/// </summary>
		public string ServiceName { get; set; }

		/// <summary>
		/// The service display name.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// The service description.
		/// </summary>
		public string Description { get; set; }
	}
}

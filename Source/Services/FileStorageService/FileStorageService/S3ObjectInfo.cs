namespace FileStorageService
{
	/// <summary>
	/// Сведения об объекте в S3
	/// </summary>
	public sealed class S3ObjectInfo
	{
		/// <summary>
		/// Id изображения из метаданных объекта.
		/// null — объект загружен не earchive
		/// </summary>
		public long? OwnerImageId { get; set; }
	}
}

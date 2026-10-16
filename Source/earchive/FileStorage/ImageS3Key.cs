namespace earchive.FileStorage
{
	/// <summary>
	/// Изображение документа и ключ его файла в S3
	/// </summary>
	public sealed class ImageS3Key
	{
		/// <summary>
		/// Id изображения (строки images_s3)
		/// </summary>
		public long ImageId { get; set; }

		/// <summary>
		/// Ключ объекта в S3
		/// </summary>
		public string S3Key { get; set; }
	}
}

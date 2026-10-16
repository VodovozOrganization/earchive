namespace FileStorageService
{
	/// <summary>
	/// Схема ключей изображений в S3.
	/// Ключи сохранены в таблице изображений, поэтому менять формат можно только вместе с данными в бакете
	/// </summary>
	public static class ImageS3Keys
	{
		/// <summary>
		/// MIME-тип изображений: программа сохраняет сканы в JPEG
		/// </summary>
		public const string ContentType = "image/jpeg";

		/// <summary>
		/// Ключ изображения вида docs/{doc_id}/{image_id}.jpg.
		/// Id строки изображения уникален, поэтому коллизий нет
		/// </summary>
		/// <param name="docId">Id документа</param>
		/// <param name="imageId">Id изображения</param>
		/// <returns>Ключ объекта в бакете</returns>
		public static string Build(long docId, long imageId)
		{
			return string.Format("docs/{0}/{1}.jpg", docId, imageId);
		}
	}
}

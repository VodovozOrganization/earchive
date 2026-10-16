namespace FileStorageService
{
	/// <summary>
	/// Настройки подключения к S3
	/// </summary>
	public class S3Settings
	{
		/// <summary>
		/// Ключ доступа
		/// </summary>
		public string AccessKey { get; set; }

		/// <summary>
		/// Секретный ключ
		/// </summary>
		public string SecretKey { get; set; }

		/// <summary>
		/// Имя бакета
		/// </summary>
		public string BucketName { get; set; }

		/// <summary>
		/// Адрес S3-хранилища.
		/// Не задаётся, если используется регион AWS
		/// </summary>
		public string ServiceUrl { get; set; }

		/// <summary>
		/// Регион AWS, например eu-central-1.
		/// Не задаётся, если указан адрес хранилища
		/// </summary>
		public string Region { get; set; }
	}
}

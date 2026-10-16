namespace BaseParametersService
{
	public interface IBaseParametersProvider
	{
		int ContractDocTypeId { get; }

		int UpdDocTypeId { get; }

		/// <summary>
		/// Ключ доступа к S3
		/// </summary>
		string S3AccessKey { get; }

		/// <summary>
		/// Секретный ключ S3
		/// </summary>
		string S3SecretKey { get; }

		/// <summary>
		/// Бакет S3 с изображениями
		/// </summary>
		string S3Bucket { get; }

		/// <summary>
		/// Адрес S3-хранилища.
		/// Не задаётся, если используется регион AWS
		/// </summary>
		string S3ServiceUrl { get; }

		/// <summary>
		/// Регион AWS.
		/// Не задаётся, если указан адрес хранилища
		/// </summary>
		string S3Region { get; }
	}
}
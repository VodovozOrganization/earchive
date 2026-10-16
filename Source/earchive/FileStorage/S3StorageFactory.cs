using BaseParametersService;
using FileStorageService;
using NLog;

namespace earchive.FileStorage
{
	/// <summary>
	/// Единая точка создания S3-клиента.
	/// Настройки читаются из base_parameters при первом обращении
	/// </summary>
	public static class S3StorageFactory
	{
		private static readonly object _sync = new object();
		private static IS3FileStorage _instance;
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Возвращает общий экземпляр хранилища, создавая его при первом вызове
		/// </summary>
		/// <returns>Хранилище изображений в S3</returns>
		public static IS3FileStorage Get()
		{
			if (_instance != null)
			{
				return _instance;
			}

			lock (_sync)
			{
				if (_instance != null)
				{
					return _instance;
				}

				var settings = LoadSettingsFromBaseParameters();
				_instance = new S3FileStorage(settings);
				_logger.Info(
					"S3FileStorage инициализирован. Bucket={0}, ServiceUrl={1}, Region={2}",
					settings.BucketName,
					settings.ServiceUrl,
					settings.Region);
				return _instance;
			}
		}

		/// <summary>
		/// Читает настройки S3 из base_parameters
		/// </summary>
		/// <returns>Настройки подключения к S3</returns>
		private static S3Settings LoadSettingsFromBaseParameters()
		{
			IBaseParametersProvider provider = new BaseParametersProvider();
			return new S3Settings
			{
				AccessKey = provider.S3AccessKey,
				SecretKey = provider.S3SecretKey,
				BucketName = provider.S3Bucket,
				ServiceUrl = provider.S3ServiceUrl,
				Region = provider.S3Region
			};
		}
	}
}

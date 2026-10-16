using System.IO;
using System.Threading.Tasks;

namespace FileStorageService
{
	/// <summary>
	/// Хранилище файлов изображений в S3
	/// </summary>
	public interface IS3FileStorage
	{
		/// <summary>
		/// Загружает объект в бакет, перезаписывая существующий с тем же ключом
		/// </summary>
		/// <param name="key">Ключ объекта</param>
		/// <param name="data">Содержимое</param>
		/// <param name="contentType">MIME-тип содержимого</param>
		/// <param name="ownerImageId">
		/// Id изображения, записывается в метаданные объекта (см. <see cref="S3ObjectInfo.OwnerImageId"/>)
		/// </param>
		Task UploadAsync(string key, Stream data, string contentType, long ownerImageId);

		/// <summary>
		/// Скачивает объект целиком
		/// </summary>
		/// <param name="key">Ключ объекта</param>
		/// <returns>Содержимое объекта</returns>
		Task<byte[]> DownloadAsync(string key);

		/// <summary>
		/// Удаляет объект
		/// </summary>
		/// <param name="key">Ключ объекта</param>
		Task DeleteAsync(string key);

		/// <summary>
		/// Возвращает сведения об объекте
		/// </summary>
		/// <param name="key">Ключ объекта</param>
		/// <returns>Сведения об объекте или null, если объекта нет</returns>
		Task<S3ObjectInfo> GetInfoAsync(string key);
	}
}

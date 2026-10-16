using System.IO;
using System.Threading.Tasks;

namespace FileStorageService
{
	/// <summary>
	/// Загрузка изображения без риска затереть чужой объект:
	/// бакет может быть общим с другими системами, а PutObject молча перезаписывает существующий ключ
	/// </summary>
	public static class ImageS3Upload
	{
		/// <summary>
		/// Загружает объект, только если ключ свободен или занят этим же изображением (повтор после сбоя).
		/// Иначе бросает <see cref="ForeignS3ObjectException"/>, объект не трогается
		/// </summary>
		/// <param name="s3">Хранилище</param>
		/// <param name="key">Ключ объекта</param>
		/// <param name="data">Содержимое</param>
		/// <param name="contentType">MIME-тип содержимого</param>
		/// <param name="imageId">Id изображения, записывается в метаданные объекта</param>
		public static async Task UploadImageAsync(this IS3FileStorage s3, string key, Stream data, string contentType, long imageId)
		{
			var existing = await s3.GetInfoAsync(key).ConfigureAwait(false);
			if(existing != null && existing.OwnerImageId != imageId)
			{
				throw new ForeignS3ObjectException(key, imageId, existing.OwnerImageId);
			}

			await s3.UploadAsync(key, data, contentType, imageId).ConfigureAwait(false);
		}
	}
}

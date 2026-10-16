using System;

namespace FileStorageService
{
	/// <summary>
	/// Ключ в бакете занят объектом другого изображения или другой системы
	/// </summary>
	public sealed class ForeignS3ObjectException : InvalidOperationException
	{
		/// <summary>
		/// Создаёт исключение с описанием занятого ключа
		/// </summary>
		/// <param name="key">Ключ объекта</param>
		/// <param name="imageId">Id загружаемого изображения</param>
		/// <param name="ownerImageId">Id изображения, которому принадлежит объект, или null, если объект не earchive</param>
		public ForeignS3ObjectException(string key, long imageId, long? ownerImageId)
			: base(string.Format(
				"В бакете уже есть объект {0}, принадлежащий {1}. Загрузка изображения id={2} отменена, объект не изменён.",
				key,
				ownerImageId.HasValue ? "изображению id=" + ownerImageId.Value : "не earchive (нет метки earchive-image-id)",
				imageId))
		{
		}
	}
}

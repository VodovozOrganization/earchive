using earchive.FileStorage;
using Gdk;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace earchive.Loaders
{
	/// <summary>
	/// Загрузка изображений документов: список — из таблицы images_s3, файлы — из S3
	/// </summary>
	public class ImageLoader
	{
		private readonly ILogger _logger;

		public ImageLoader(ILogger logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Загружает изображения одного документа
		/// </summary>
		/// <param name="docId">Id документа</param>
		/// <param name="connection">Соединение с БД</param>
		/// <returns>Изображения в порядке страниц</returns>
		public IList<DocumentImage> LoadImages(int docId, MySqlConnection connection)
		{
			return LoadImages(new List<int> { docId }, connection);
		}

		/// <summary>
		/// Загружает изображения нескольких документов
		/// </summary>
		/// <param name="docIds">Id документов</param>
		/// <param name="connection">Соединение с БД</param>
		/// <returns>Изображения в порядке страниц</returns>
		public IList<DocumentImage> LoadImages(IList<int> docIds, MySqlConnection connection)
		{
			var docIdsParameterValue = string.Join(",", docIds);
			var images = new List<DocumentImage>();

			if (docIds.Count == 0)
			{
				return images;
			}

			// doc_id IN (...), а не FIND_IN_SET: FIND_IN_SET не использует индекс.
			// Id — целые числа, подставлять их в текст безопасно.
			var docIdsInList = string.Join(",", docIds.Select(id => id.ToString(CultureInfo.InvariantCulture)));

			// Сами изображения лежат в S3, в таблице — только ключи.
			var sql =
				$@"SELECT id, order_num, size, type, s3_key
				FROM images_s3
				WHERE doc_id IN ({docIdsInList})
				ORDER BY order_num";

			_logger.Debug(
				"Выполняется запрос загрузки документов. Список id: ({DocIdsParameterValue}).",
				docIdsParameterValue);

			using (var cmd = new MySqlCommand(sql, connection))
			{
				cmd.CommandTimeout = 60;

				using (var rdr = cmd.ExecuteReader())
				{
					var s3KeyOrdinal = rdr.GetOrdinal("s3_key");

					while (rdr.Read())
					{
						var docImage = new DocumentImage
						{
							IsChanged = false,
							Id = rdr.GetInt32("id"),
							Order = rdr.GetInt32("order_num"),
							Size = rdr.GetInt64("size"),
							Type = rdr.GetString("type"),
							S3Key = rdr.IsDBNull(s3KeyOrdinal) ? null : rdr.GetString(s3KeyOrdinal)
						};

						if (!string.IsNullOrEmpty(docImage.S3Key))
						{
							LoadFromS3(docImage);
						}
						else
						{
							// Пустое изображение: строка без файла в S3
							docImage.File = new byte[0];
							docImage.Size = 0;
						}

						if (docImage.File.Length > 0)
						{
							docImage.Image = new Pixbuf(docImage.File);
						}

						images.Add(docImage);
					}
				}
			}

			_logger.Debug("Загружено {ImagesCounter} документов.", images.Count);
			return images;
		}

		/// <summary>
		/// Скачивает файл изображения из S3 и заполняет <see cref="DocumentImage.File"/> и <see cref="DocumentImage.Size"/>
		/// </summary>
		/// <param name="docImage">Изображение с заполненным ключом S3</param>
		private void LoadFromS3(DocumentImage docImage)
		{
			try
			{
				docImage.File = S3StorageFactory.Get().DownloadAsync(docImage.S3Key).GetAwaiter().GetResult();
				docImage.Size = docImage.File.LongLength;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Не удалось скачать изображение из S3. key={0}, id={1}", docImage.S3Key, docImage.Id);
				throw;
			}
		}
	}
}

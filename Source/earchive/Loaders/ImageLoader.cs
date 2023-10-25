using Gdk;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;

namespace earchive.Loaders
{
	public class ImageLoader
	{
		private readonly ILogger _logger;

		public ImageLoader(ILogger logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IList<DocumentImage> LoadImages(int docId, MySqlConnection connection)
		{
			var images = LoadImages(
				new List<int> { docId },
				connection);

			return images;
		}

		public IList<DocumentImage> LoadImages(IList<int> docIds, MySqlConnection connection)
		{
			var docIdsParameterValue = string.Join(",", docIds);
			var images = new List<DocumentImage>();

			var sql = 
				@"SELECT * 
				FROM images 
				WHERE FIND_IN_SET(doc_id, @docIds) 
				ORDER BY order_num";

			_logger.Debug(
					"Выполняется запрос загрузки документов. Список id: ({DocIdsParameterValue}).",
					docIdsParameterValue);

			using (var cmd = new MySqlCommand(sql, connection))
			{

				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@docIds", docIdsParameterValue);

				using (var rdr = cmd.ExecuteReader())
				{
					// Загружаем изображения
					while (rdr.Read())
					{
						DocumentImage docImage = new DocumentImage();
						docImage.IsChanged = false;
						docImage.Id = rdr.GetInt32("id");
						docImage.Order = rdr.GetInt32("order_num");
						docImage.Size = rdr.GetInt64("size");
						docImage.Type = rdr.GetString("type");
						docImage.File = new byte[docImage.Size];
						rdr.GetBytes(rdr.GetOrdinal("image"), 0, docImage.File, 0, (int)docImage.Size);
						docImage.Image = new Pixbuf(docImage.File);

						images.Add(docImage);
					}
				}
			}

			_logger.Debug("Загружено {ImagesCounter} документов.", images.Count);

			return images;
		}
	}
}

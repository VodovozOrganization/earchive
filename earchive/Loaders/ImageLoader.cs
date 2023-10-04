using Gdk;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;

namespace earchive.Loaders
{
	public class ImageLoader
	{
		private readonly Logger _logger;

		public ImageLoader(Logger logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public DocumentImage LoadImage(int docId, MySqlConnection connection)
		{
			DocumentImage docImage = new DocumentImage();

			// Загружаем изображения
			var sql = 
				@"SELECT * 
				FROM images
				WHERE doc_id = @doc_id 
				ORDER BY order_num";

			using (var cmd = new MySqlCommand(sql, connection))
			{
				cmd.Parameters.AddWithValue("@doc_id", docId);

				using (var rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						docImage.IsChanged = false;
						docImage.Id = rdr.GetInt32("id");
						docImage.Order = rdr.GetInt32("order_num");
						docImage.Size = rdr.GetInt64("size");
						docImage.Type = rdr.GetString("type");
						docImage.File = new byte[docImage.Size];
						rdr.GetBytes(rdr.GetOrdinal("image"), 0, docImage.File, 0, (int)docImage.Size);
						docImage.Image = new Pixbuf(docImage.File);
					}
					rdr.Close();
				}                    
			}

			return docImage;
		}

		public List<DocumentImage> LoadImages(List<int> docIds, MySqlConnection connection)
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

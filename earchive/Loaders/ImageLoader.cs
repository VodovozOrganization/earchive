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
						docImage.Changed = false;
						docImage.id = rdr.GetInt32("id");
						docImage.order = rdr.GetInt32("order_num");
						docImage.size = rdr.GetInt64("size");
						docImage.type = rdr.GetString("type");
						docImage.file = new byte[docImage.size];
						rdr.GetBytes(rdr.GetOrdinal("image"), 0, docImage.file, 0, (int)docImage.size);
						docImage.Image = new Pixbuf(docImage.file);
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
						docImage.Changed = false;
						docImage.id = rdr.GetInt32("id");
						docImage.order = rdr.GetInt32("order_num");
						docImage.size = rdr.GetInt64("size");
						docImage.type = rdr.GetString("type");
						docImage.file = new byte[docImage.size];
						rdr.GetBytes(rdr.GetOrdinal("image"), 0, docImage.file, 0, (int)docImage.size);
						docImage.Image = new Pixbuf(docImage.file);

						images.Add(docImage);
					}
				}
			}

			_logger.Debug("Загружено {ImagesCounter} документов.", images.Count);

			return images;
		}
	}
}

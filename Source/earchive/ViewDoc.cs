using earchive.Loaders;
using earchive.Print;
using Gdk;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QS.Print;
using QSProjectsLib;
using QSWidgetLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace earchive
{
	public partial class ViewDoc : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private DocumentInformation DocInfo;
		private Dictionary<int, Gtk.Label> FieldLables;
		private Dictionary<int, object> FieldWidgets;
		private List<DocumentImage> Images;
		private int DocId;
		private int PopupImageId;

		public ViewDoc ()
		{
			this.Build();
			Images = new List<DocumentImage>();
			this.Resize(Convert.ToInt32(Screen.Width * 0.95), Convert.ToInt32(Screen.Height * 0.9));
			this.Move(Convert.ToInt32(Screen.Width * 0.05 / 2), Convert.ToInt32(Screen.Height * 0.1 / 2));
			entryNumber.IsEditable = QSMain.User.Permissions["can_edit"];
			dateDoc.IsEditable = QSMain.User.Permissions["can_edit"];
		}

		public void Fill(int doc_id, ImageLoader imageLoader)
		{
			QSMain.CheckConnectionAlive();
			logger.Info("Запрос документа №" + doc_id +"...");
			try
			{
				DocId = doc_id;

				string sql = "SELECT docs.*, users.name as username FROM docs " +
					"LEFT JOIN users ON users.id = docs.user_id " +
					"WHERE docs.id = @id";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", DocId);
				MySqlDataReader rdr = cmd.ExecuteReader();

				if(!rdr.Read())
					return;

				entryNumber.Text = rdr.GetString ("number");
				int DocType = rdr.GetInt32("type_id");
				dateDoc.Date = rdr.GetDateTime("date");
				labelCreateDate.LabelProp = String.Format("{0}", rdr.GetDateTime("create_date"));
				labelCreateUser.LabelProp = rdr.GetString("username");
				if(rdr["comments"] != DBNull.Value)
					textviewComments.Buffer.Text = rdr.GetString("comments");

				rdr.Close();

				DocInfo = new DocumentInformation(DocType);
				this.Title = DocInfo.Name + " №" + entryNumber.Text + " от " + dateDoc.DateText;
				labelType.LabelProp = DocInfo.Name;

				//Дополнительные поля
				if(DocInfo.CountExtraFields > 0)
				{
					FieldLables = new Dictionary<int, Label>();
					FieldWidgets = new Dictionary<int, object>();
					uint Row = 5;
					foreach(DocFieldInfo field in DocInfo.FieldsList)
					{
						Label NameLable = new Label(field.Name + ":");
						NameLable.Xalign = 1;
						tableProperty.Attach(NameLable, 0, 1, Row, Row+1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
						FieldLables.Add(field.ID, NameLable);
						object ValueWidget;
						switch (field.Type) {
							case "varchar" :
							ValueWidget = new Entry();
							((Entry)ValueWidget).IsEditable = QSMain.User.Permissions["can_edit"];
							break;
							default :
							ValueWidget = new Label();
							break;
						}
						tableProperty.Attach((Widget)ValueWidget, 1, 2, Row, Row+1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
						FieldWidgets.Add(field.ID, ValueWidget);

						Row++;
					}
					tableProperty.ShowAll();

					//Заполняем данными документа
					sql = "SELECT * FROM extra_" + DocInfo.DBTableName +
						" WHERE doc_id = @doc_id";
					cmd = new MySqlCommand(sql, QSMain.connectionDB);
					cmd.Parameters.AddWithValue("@doc_id", DocId);
					rdr = cmd.ExecuteReader();
					rdr.Read();

					foreach(DocFieldInfo field in DocInfo.FieldsList)
					{
						if(rdr[field.DBName] == DBNull.Value)
							continue;

						switch (field.Type) {
							case "varchar" :
							((Entry)FieldWidgets[field.ID]).Text = rdr.GetString(field.DBName);
							((Entry)FieldWidgets[field.ID]).TooltipText = rdr.GetString(field.DBName);
							break;
							default:
							Console.WriteLine("Неизвестный тип поля");
							break;
						}
					}
					rdr.Close();
				}

				Images.Clear();

				// Загружаем изображения
				var docImages = imageLoader.LoadImages(DocId, QSMain.connectionDB);

				foreach(var docImage in docImages)
				{
					//Добавляем вижет
					ImageViewer view = new ImageViewer();
					view.VerticalFit = false;
					view.HorizontalFit = true;
					view.Pixbuf = docImage.Image;
					view.ButtonPressEvent += OnImagesButtonPressEvent;
					vboxImages.Add(view);
					docImage.Widget = view;

					Images.Add(docImage);
				}				

				vboxImages.ShowAll();
				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog(this, "Ошибка получения документа!", logger, ex);
			}
		}

		public void Fill(IList<int> docIds, int docTypeId, ImageLoader imageLoader)
		{
			logger.Debug(
				"Получен запрос открытия группы документов: {DocIds}. Общее количество документов: {DocIdsCount} Тип документов id={DocTypeId}.",
				string.Join(", ", docIds),
				docIds.Count,
				docTypeId);

			HideControls();

			DocInfo = new DocumentInformation(docTypeId);
			this.Title = DocInfo.Name + " выгрузка документов";
			labelType.LabelProp = DocInfo.Name;

			if(docIds.Count < 1)
			{
				return;
			}

			try
			{
				QSMain.CheckConnectionAlive();

				var docImages = imageLoader.LoadImages(docIds, QSMain.connectionDB);					

				foreach (var docImage in docImages)
				{
					//Добавляем вижет
					ImageViewer view = new ImageViewer();
					view.VerticalFit = false;
					view.HorizontalFit = true;
					view.Pixbuf = docImage.Image;
					view.ButtonPressEvent += OnImagesButtonPressEvent;
					vboxImages.Add(view);
					docImage.Widget = view;
				}

				Images.Clear();
				Images = docImages.ToList();
			}
			catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog(this, "Ошибка получения документа!", logger, ex);
			}
			vboxImages.ShowAll();
		}

		private void HideControls()
		{
			label2.Visible = false;
			label3.Visible = false;
			label4.Visible = false;
			label5.Visible = false;
			label6.Visible = false;
			labelCreateUser.Visible = false;
			labelCreateDate.Visible = false;
			entryNumber.Visible = false;
			dateDoc.Visible = false;
			GtkScrolledWindow.Visible = false;
			buttonCancel.Visible = false;
			buttonOk.Visible = false;
		}

		protected void OnImagesButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			if((int)args.Event.Button == 3)
			{       
				foreach(DocumentImage img in Images)
				{
					if(o == img.Widget)
					{
						PopupImageId = img.Id;
					}
				}
				Gtk.Menu jBox = new Gtk.Menu();
				Gtk.MenuItem MenuItem1 = new MenuItem("Сохранить");
				MenuItem1.Activated += OnImagePopupSave;
				jBox.Add(MenuItem1);           
				jBox.ShowAll();
				jBox.Popup();
			}
		}

		protected void OnImagePopupSave(object sender, EventArgs Arg)
		{
			FileChooserDialog fc= new FileChooserDialog(
				"Укажите файл для сохранения картинки",
				this,
				FileChooserAction.Save,
				"Отмена",ResponseType.Cancel,
				"Сохранить",ResponseType.Accept);

			fc.CurrentName = string.IsNullOrEmpty(entryNumber.Text)
				? DocInfo.Name + " изображение" + ".jpg"
				: DocInfo.Name + " " + entryNumber.Text + ".jpg";
			fc.Show(); 
			if(fc.Run() == (int) ResponseType.Accept)
			{
				fc.Hide();
				foreach(DocumentImage img in Images)
				{
					if(img.Id == PopupImageId)
					{
						FileStream fs = new FileStream(fc.Filename, FileMode.Create, FileAccess.Write);
						fs.Write(img.File, 0, (int)img.Size);
						fs.Close();
					}
				}
			}
			fc.Destroy();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			QSMain.CheckConnectionAlive();
			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction ();
			logger.Info("Записываем документ...");
			try
			{
				string sql;
				// Работаем с шаблоном
				if(QSMain.User.Permissions["can_edit"])
					sql = "UPDATE docs SET number = @number, date = @date, comments = @comments " +
						"WHERE id = @id";
				else
					sql = "UPDATE docs SET comments = @comments " +
						"WHERE id = @id";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.Parameters.AddWithValue ("@id", DocId);
				cmd.Parameters.AddWithValue ("@number", entryNumber.Text);
				cmd.Parameters.AddWithValue ("@date", dateDoc.Date);
				if(textviewComments.Buffer.Text != "")
					cmd.Parameters.AddWithValue ("@comments", textviewComments.Buffer.Text);
				else 
					cmd.Parameters.AddWithValue ("@comments", DBNull.Value);
				cmd.ExecuteNonQuery ();
				// Записываем дополнительные поля
				if(QSMain.User.Permissions["can_edit"] && DocInfo.CountExtraFields > 0)
				{
					cmd = new MySqlCommand();
					cmd.Connection = QSMain.connectionDB;
					cmd.Transaction = trans;
					sql = "UPDATE extra_" + DocInfo.DBTableName + " SET ";
					bool first = true;
					foreach(DocFieldInfo field in DocInfo.FieldsList)
					{
						if(!first)
							sql += ", ";
						sql += field.DBName + " = @" + field.DBName;
						first = false;
						switch (field.Type) {
							case "varchar" :
							cmd.Parameters.AddWithValue(field.DBName, ((Entry)FieldWidgets[field.ID]).Text);
							break;
							default :
							cmd.Parameters.AddWithValue(field.DBName, DBNull.Value);
							break;
						}
					}
					sql += " WHERE doc_id = @doc_id";
					cmd.Parameters.AddWithValue("@doc_id", DocId);
					cmd.CommandText = sql;
					cmd.ExecuteNonQuery();
				}
				trans.Commit();
				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				trans.Rollback ();
				QSMain.ErrorMessageWithLog(this, "Ошибка записи документа!", logger, ex);
			}
		}

		void TestCanSave()
		{
			bool numberok = entryNumber.Text != "";
			bool dateok = !dateDoc.IsEmpty;
			buttonOk.Sensitive = numberok && dateok;
		}

		protected void OnEntryNumberChanged (object sender, EventArgs e)
		{
			TestCanSave();
		}

		protected void OnDateDocDateChanged (object sender, EventArgs e)
		{
			TestCanSave();
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			var documentsToPrint = new List<IPrintableImage>();

			foreach (var docImage in Images)
			{
				if (docImage.Image == null)
				{
					continue;
				}

				var document = new PrintableImage(docImage.Image);
				document.Name = docImage.Order.ToString();

				documentsToPrint.Add(document);
			}

			if (documentsToPrint.Count < 1)
			{
				return;
			}

			ConfigureProgresBar(documentsToPrint.Count);

			var printer = new PrintableImagesPrinter();
			printer.PrintableImagePrinted += (s, ev) =>
			{
				UpdateProgressBarValue();
			};

			printer.SetImagesPrintList(documentsToPrint);

			try
			{
				printer.Print();
			}
			catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog(
					"Ошибка печати изображения",
					logger,
					ex);
			}
		}

		private void ConfigureProgresBar(int imagesCount)
		{
			yprogressbarPrint.Adjustment = new Adjustment(0, 0, imagesCount, 1, 100, 0);

			yprogressbarPrint.Adjustment.Upper = imagesCount;
			yprogressbarPrint.Text = string.Empty;
			yprogressbarPrint.Visible = true;
		}

		private void UpdateProgressBarValue()
		{
			yprogressbarPrint.Adjustment.Value++;
			yprogressbarPrint.Text = $"Отправлено {yprogressbarPrint.Adjustment.Value} из {yprogressbarPrint.Adjustment.Upper}";

			if(yprogressbarPrint.Adjustment.Value == yprogressbarPrint.Adjustment.Upper)
			{
				yprogressbarPrint.Text = "Отправка на печать завершена!";
			}
		}

		protected void OnButtonPDFClicked (object sender, EventArgs e)
		{
			FileChooserDialog fc= new FileChooserDialog(
				"Укажите файл для сохранения документа",
				this,
				FileChooserAction.Save,
				"Отмена",ResponseType.Cancel,
				"Сохранить",ResponseType.Accept);

			fc.CurrentName = string.IsNullOrEmpty(entryNumber.Text)
				? DocInfo.Name + " выгрузка" + ".pdf"
				: DocInfo.Name + " " + entryNumber.Text + ".pdf";

			fc.Show(); 
			if(fc.Run() == (int) ResponseType.Accept)
			{
				fc.Hide();
				iTextSharp.text.Document document = new iTextSharp.text.Document();
				using (var stream = new FileStream(fc.Filename, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
					document.Open();
					foreach(DocumentImage img in Images)
					{
						if(img.Image.Width > img.Image.Height)
							document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
						else 
							document.SetPageSize(iTextSharp.text.PageSize.A4);
						document.NewPage();
						var image = iTextSharp.text.Image.GetInstance(img.File);
						image.SetAbsolutePosition(0,0);
						image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
						document.Add(image);
					}
					document.Close();
				}
			}
			fc.Destroy();
		}

		public override void Destroy()
		{
			Images?.Clear();
			vboxImages?.Destroy();
			base.Destroy();
		}
	}
}


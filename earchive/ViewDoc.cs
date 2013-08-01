using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using Gdk;
using MySql.Data.MySqlClient;
using QSProjectsLib;

namespace earchive
{
	public partial class ViewDoc : Gtk.Dialog
	{
		private DocumentInformation DocInfo;
		private Dictionary<int, Gtk.Label> FieldLables;
		private Dictionary<int, object> FieldWidgets;
		private Dictionary<int, EventBox> EventBoxes;
		private List<DocumentImage> Images;
		private int DocId;
		private System.Timers.Timer ImageResizeTimer;
		private int PopupImageId;

		public ViewDoc ()
		{
			this.Build ();
			Images = new List<DocumentImage>();
			EventBoxes = new Dictionary<int, EventBox>();
			this.Resize(Convert.ToInt32(Screen.Width * 0.95), Convert.ToInt32(Screen.Height * 0.9));
			this.Move(Convert.ToInt32(Screen.Width * 0.05 / 2), Convert.ToInt32(Screen.Height * 0.1 / 2));
			ImageResizeTimer = new System.Timers.Timer(700);
			ImageResizeTimer.Elapsed += OnImageResizeTimerElapsed;
			entryNumber.IsEditable = QSMain.User.Permissions["can_edit"];
			dateDoc.IsEditable = QSMain.User.Permissions["can_edit"];
		}

		public void Fill(int doc_id)
		{
			MainClass.StatusMessage("Запрос документа №" + doc_id +"...");
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
				this.Title = DocInfo.TypeName + " №" + entryNumber.Text + " от " + dateDoc.DateText;
				labelType.LabelProp = DocInfo.TypeName;

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
						tableProperty.Attach(NameLable, 0, 1, Row, Row+1, 
						                         AttachOptions.Fill, AttachOptions.Fill, 0, 0);
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
						tableProperty.Attach((Widget)ValueWidget, 1, 2, Row, Row+1, 
						                         AttachOptions.Fill, AttachOptions.Fill, 0, 0);
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
				// Загружаем изображения
				sql = "SELECT * FROM images " +
					"WHERE doc_id = @doc_id " +
					"ORDER BY order_num";
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@doc_id", DocId);
				rdr = cmd.ExecuteReader();

				while( rdr.Read())
				{
					DocumentImage DocImage = new DocumentImage();
					DocImage.Changed = false;
					DocImage.id = rdr.GetInt32("id");
					DocImage.order = rdr.GetInt32("order_num");
					DocImage.size = rdr.GetInt64("size");
					DocImage.type = rdr.GetString("type");
					DocImage.file = new byte[DocImage.size];
					rdr.GetBytes(rdr.GetOrdinal("image"), 0, DocImage.file, 0, (int) DocImage.size);
					DocImage.Image = new Pixbuf(DocImage.file);
					//Добавляем вижет
					DocImage.Widget = new Gtk.Image();

					double hratio = scrolledImages.Hadjustment.PageSize / DocImage.Image.Width;
					int Heigth = Convert.ToInt32(DocImage.Image.Height * hratio);
					int Width = (int)scrolledImages.Hadjustment.PageSize;
					DocImage.Widget.Pixbuf = DocImage.Image.ScaleSimple (Width,
					                                   Heigth,
					                                   InterpType.Bilinear);
					EventBox Ebox = new EventBox();
					Ebox.Add(DocImage.Widget);
					Ebox.ButtonPressEvent += OnImagesButtonPressEvent;
					EventBoxes.Add(DocImage.id, Ebox);
					vboxImages.Add(Ebox);
					Images.Add(DocImage);
				}
				rdr.Close();
				vboxImages.ShowAll();
				MainClass.StatusMessage("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения документа!");
				QSMain.ErrorMessage(this,ex);
			}
		}

		protected void OnImagesButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			if((int)args.Event.Button == 3)
			{       
				foreach(KeyValuePair<int, EventBox> pair in EventBoxes)
				{
					if(o == pair.Value)
						PopupImageId = pair.Key;
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
			FileChooserDialog fc=
				new FileChooserDialog("Укажите файл для сохранения картинки",
				                      this,
				                      FileChooserAction.Save,
				                      "Отмена",ResponseType.Cancel,
				                      "Сохранить",ResponseType.Accept);
			//FileFilter filter = new FileFilter();
			fc.CurrentName = DocInfo.TypeName + " " + entryNumber.Text + ".jpg";
			fc.Show(); 
			if(fc.Run() == (int) ResponseType.Accept)
			{
				fc.Hide();
				foreach(DocumentImage img in Images)
				{
					if(img.id == PopupImageId)
					{
						FileStream fs = new FileStream(fc.Filename, FileMode.Create, FileAccess.Write);
						fs.Write(img.file, 0, (int)img.size);
						fs.Close();
					}
				}
			}
			fc.Destroy();
		}

		private void OnImageResizeTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.WriteLine("Image resizing");
			ImageResizeTimer.Stop();
			foreach(DocumentImage img in Images)
			{
				double hratio = scrolledImages.Hadjustment.PageSize / img.Image.Width;
				int Heigth = Convert.ToInt32(img.Image.Height * hratio);
				int Width = (int)scrolledImages.Hadjustment.PageSize;
				img.Widget.Pixbuf = img.Image.ScaleSimple (Width,
				                                           Heigth,
				                                           InterpType.Bilinear);
			}
		}

		protected void OnScrolledImagesSizeAllocated (object o, SizeAllocatedArgs args)
		{
			ImageResizeTimer.Stop();
			ImageResizeTimer.Start();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction ();
			MainClass.StatusMessage("Записываем документ...");
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
							cmd.Parameters.AddWithValue(field.DBName, 
							                            ((Entry)FieldWidgets[field.ID]).Text);
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
				MainClass.StatusMessage("Ok");
			}
			catch (Exception ex)
			{
				trans.Rollback ();
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи документа!");
				QSMain.ErrorMessage(this,ex);
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

		protected void OnButtonPDFClicked (object sender, EventArgs e)
		{
			FileChooserDialog fc=
				new FileChooserDialog("Укажите файл для сохранения документа",
				                      this,
				                      FileChooserAction.Save,
				                      "Отмена",ResponseType.Cancel,
				                      "Сохранить",ResponseType.Accept);
			fc.CurrentName = DocInfo.TypeName + " " + entryNumber.Text + ".pdf";
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
						var image = iTextSharp.text.Image.GetInstance(img.file);
						image.SetAbsolutePosition(0,0);
						image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
						document.Add(image);
					}
					document.Close();
				}
			}
			fc.Destroy();
		}
	}
}


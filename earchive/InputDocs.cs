using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using Gdk;
using QSProjectsLib;
using MySql.Data.MySqlClient;
using NLog;

namespace earchive
{
	public partial class InputDocs : Gtk.Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private bool Clearing = false;
		private ImageTreeStore ImageList;
		private TreeIter CurrentImage;
		private TreeIter CurrentDocIter;
		private Document CurrentDoc;
		private int NextDocNumber;
		private Dictionary<int, Gtk.Label> FieldLables;
		private Dictionary<int, object> FieldWidgets;
		private Dictionary<int, Gtk.Image> FieldIcons;
		private string CurrentLog;

		//Настройки значков
		string DocIconNew = Stock.New;
		string DocIconBad = Stock.No;
		string DocIconAttention = Stock.DialogWarning;
		string DocIconGood = Stock.Yes;

		public InputDocs () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();

			FieldLables = new Dictionary<int, Label>();
			FieldWidgets = new Dictionary<int, object>();
			FieldIcons = new Dictionary<int, Gtk.Image>();
			ComboWorks.ComboFillReference (comboType, "doc_types", ComboWorks.ListMode.OnlyItems, false);
			//FIXME разобраться с очищением памяти. Если закрыть окно или удалить картинки память не очищается.
			ImageList = new ImageTreeStore(typeof(int), //0 - id
			                          typeof(string), //1 - Image Name
			                          typeof(string), //2 - full path
			                          typeof(Document), //3 - document
			                          typeof(Pixbuf), //4 - thumbnails
			                          typeof(Pixbuf), //5 - full image
			                          typeof(bool), //6 - IsImage
			                          typeof(string),//7 - Doc name
			                          typeof(string) //8 - Doc Icon Name
			                          );
		
			TreeViewColumn ImageColumn = new TreeViewColumn();
			var ImageCell = new ImageListCellRenderer(Pango.FontDescription.FromString ("Tahoma 10"), IconSize.Menu);
			ImageCell.Xalign = 0;
			ImageColumn.PackStart (ImageCell, true);
			ImageColumn.AddAttribute (ImageCell, "pixbuf", 4);
			ImageColumn.AddAttribute (ImageCell, "text", 7);
			ImageColumn.AddAttribute (ImageCell, "IsImageRow", 6);
			ImageColumn.AddAttribute (ImageCell, "IconName", 8);

			treeviewImages.AppendColumn (ImageColumn);
			treeviewImages.Model = ImageList;
			treeviewImages.Reorderable = true;
			treeviewImages.TooltipColumn = 1;
			treeviewImages.ShowAll ();
			NextDocNumber = 1;
		}

		protected void OnOpenActionActivated (object sender, EventArgs e)
		{
			TreeIter iter;
			FileChooserDialog Chooser = new FileChooserDialog("Выберите изображения для загрузки...", 
			                                              this,
			                                              FileChooserAction.Open,
			                                              "Отмена", ResponseType.Cancel,
			                                              "Открыть", ResponseType.Accept );
			Chooser.SelectMultiple = true;

			FileFilter Filter = new FileFilter();
			Filter.AddPixbufFormats ();
			Filter.Name = "Все изображения";
			Chooser.AddFilter(Filter);

			if((ResponseType) Chooser.Run () == ResponseType.Accept)
			{
				Chooser.Hide();
				progresswork.Text = "Загрузка изображений...";
				progresswork.Adjustment.Upper = Chooser.Filenames.Length;
				foreach(string File in Chooser.Filenames)
				{
					Console.WriteLine(File);
					iter = ImageList.AppendValues (0,
					                               String.Format ("Документ {0}", NextDocNumber),
							                        "",
							                        null,
					                               null ,
							                        null,
							                        false,
					                               String.Format ("Тип неопределён"),
					                               DocIconNew
					                               );

					FileStream fs = new FileStream(File, FileMode.Open, FileAccess.Read);
					Pixbuf image = new Pixbuf(fs);
					double ratio = 150f / Math.Max(image.Height, image.Width);
					Pixbuf thumb = image.ScaleSimple((int)(image.Width * ratio),(int)(image.Height * ratio), InterpType.Bilinear);
					fs.Close();

					NextDocNumber++;
					ImageList.AppendValues (iter,
											0,
					                        System.IO.Path.GetFileName(File),
					                        File,
					                        null,
					                        thumb,
					                        image,
					                        true,
					                        "",
					                        "");
					progresswork.Adjustment.Value++;
					MainClass.WaitRedraw();
				}
				treeviewImages.ExpandAll ();
				progresswork.Text = "Ок";
				progresswork.Fraction = 0;
			}
			Chooser.Destroy ();
		}

		protected void OnTreeviewImagesCursorChanged (object sender, EventArgs e)
		{
			TreeIter iter, iterimage, iterdoc;
			if(treeviewImages.Selection.GetSelected(out iter))
			{
				if(ImageList.IterHasChild(iter))
				{
					ImageList.IterNthChild(out iterimage, iter, 0);
					iterdoc = iter;
				}
				else
				{
					iterimage = iter;
					ImageList.IterParent (out iterdoc, iter);
				}
				CurrentDocIter = iterdoc;
				if(ImageList.GetValue (iterimage, 5) != null)
				{
					CurrentImage = iterimage;
					zoomFitAction.Activate ();
				}
				if((Document)ImageList.GetValue (iterdoc, 3) != CurrentDoc)
				{
					CurrentDoc = (Document)ImageList.GetValue (iterdoc, 3);
					UpdateFieldsWidgets(true);
				}
			}
			else 
			{
				comboType.Sensitive = false;
				entryNumber.Sensitive = false;
				dateDoc.Sensitive = false;
			}
		}

		void UpdateFieldsWidgets(bool ChangeTypeCombo)
		{
			Console.WriteLine("Update widgets");
			//Удаляем старые виджеты
			foreach(KeyValuePair<int, Label> pair in FieldLables)
			{
				tableFieldWidgets.Remove(pair.Value);
				pair.Value.Destroy();
			}
			FieldLables.Clear();
			foreach(KeyValuePair<int, object> pair in FieldWidgets)
			{
				tableFieldWidgets.Remove((Widget) pair.Value);
				((Widget)pair.Value).Destroy();
			}
			FieldWidgets.Clear();
			foreach(KeyValuePair<int, Gtk.Image> pair in FieldIcons)
			{
				tableFieldWidgets.Remove(pair.Value);
				pair.Value.Destroy();
			}
			FieldIcons.Clear();
			Clearing = true;
			if(ChangeTypeCombo)
				comboType.Active = -1;
			entryNumber.Text = "";
			dateDoc.Clear();
			Clearing = false;

			if(CurrentDoc == null)
			{
				Console.WriteLine("Doc is empty");
				entryNumber.Sensitive = false;
				dateDoc.Sensitive = false;
				return;
			}
			//Создаем новые
			if(CurrentDoc.FieldsList != null)
			{
				uint Row = 4;
				foreach(DocFieldInfo field in CurrentDoc.FieldsList)
				{
					Label NameLable = new Label(field.Name + ":");
					NameLable.Xalign = 1;
					tableFieldWidgets.Attach(NameLable, 0, 1, Row, Row+1, 
					                         AttachOptions.Fill, AttachOptions.Fill, 0, 0);
					FieldLables.Add(field.ID, NameLable);
					object ValueWidget;
					switch (field.Type) {
					case "varchar" :
						ValueWidget = new Entry();
						((Entry)ValueWidget).Changed += OnExtraFieldEntryChanged;
						break;
					default :
						ValueWidget = new Label();
						break;
					}
					tableFieldWidgets.Attach((Widget)ValueWidget, 1, 2, Row, Row+1, 
					                         AttachOptions.Fill, AttachOptions.Fill, 0, 0);
					FieldWidgets.Add(field.ID, ValueWidget);

					Gtk.Image ConfIcon = new Gtk.Image();
					tableFieldWidgets.Attach(ConfIcon, 2, 3, Row, Row+1, 
					                         AttachOptions.Fill, AttachOptions.Fill, 0, 0);
					FieldIcons.Add(field.ID, ConfIcon);

					Row++;
				}
			}
			tableFieldWidgets.ShowAll();

			//Заполняем данными текущего документа
			TreeIter iter;
			Clearing = true;
			if(ChangeTypeCombo)
			{
				ListStoreWorks.SearchListStore((ListStore)comboType.Model, CurrentDoc.TypeId, out iter);
				comboType.SetActiveIter(iter);
			}
			entryNumber.Text = CurrentDoc.DocNumber;
			entryNumber.Sensitive = true;
			SetRecognizeIcon(IconNumber, CurrentDoc.DocNumberConfidence);
			dateDoc.Date = CurrentDoc.DocDate;
			dateDoc.Sensitive = true;
			SetRecognizeIcon(IconDate, CurrentDoc.DocDateConfidence);
			Clearing = false;

			if(CurrentDoc.FieldsList != null)
			{
				foreach(DocFieldInfo field in CurrentDoc.FieldsList)
				{
					if(CurrentDoc.FieldValues[field.ID] == null)
						continue;

					switch (field.Type) {
					case "varchar" :
						((Entry)FieldWidgets[field.ID]).Text = (string) CurrentDoc.FieldValues[field.ID];
						break;
					default:
						Console.WriteLine("Неизвестный тип поля");
						break;
					}

					SetRecognizeIcon(FieldIcons[field.ID], CurrentDoc.FieldConfidence[field.ID]);
				}
			}
		}

		protected void OnExtraFieldEntryChanged(object sender, EventArgs e)
		{
			if(CurrentDoc != null && !Clearing)
			{
				foreach(KeyValuePair <int, object> pair in FieldWidgets)
				{
					if(pair.Value == sender)
					{
						CurrentDoc.FieldValues[pair.Key] = ((Entry) pair.Value).Text;
						UpdateCurDocCanSave();
					}
				}
			}
		}

		protected void OnClearActionActivated (object sender, EventArgs e)
		{
			ImageList.Clear ();
			imageDoc.Clear ();
			NextDocNumber = 1;
		}

		protected void OnZoom100ActionActivated (object sender, EventArgs e)
		{
			if(!ImageList.IterIsValid (CurrentImage))
				return;
			Pixbuf pix = (Pixbuf)ImageList.GetValue (CurrentImage, 5);
			imageDoc.Pixbuf = pix.Copy ();
		}

		protected void OnZoomFitActionActivated (object sender, EventArgs e)
		{
			if(!ImageList.IterIsValid (CurrentImage))
				return;
			Pixbuf pix = (Pixbuf)ImageList.GetValue (CurrentImage, 5);
			double vratio = scrolledImage.Vadjustment.PageSize / pix.Height;
			double hratio = scrolledImage.Hadjustment.PageSize / pix.Width;
			int Heigth, Width;
			if(vratio < hratio)
			{
				Heigth = (int) scrolledImage.Vadjustment.PageSize;
				Width = Convert.ToInt32(pix.Width * vratio);
			}
			else 
			{
				Heigth = Convert.ToInt32(pix.Height * hratio);
				Width = (int)scrolledImage.Hadjustment.PageSize;
			}
			imageDoc.Pixbuf = pix.ScaleSimple (Width,
			                                   Heigth,
			                                   InterpType.Bilinear);
		}

		protected void OnZoomInActionActivated (object sender, EventArgs e)
		{
			if(!ImageList.IterIsValid (CurrentImage))
				return;
			if(imageDoc.Pixbuf == null)
				return;
			Pixbuf pix = (Pixbuf)ImageList.GetValue (CurrentImage, 5);
			double ratio = imageDoc.Pixbuf.Width * 1.2 / pix.Width;
			int Heigth, Width;
			Heigth = Convert.ToInt32(pix.Height * ratio);
			Width = Convert.ToInt32(pix.Width * ratio);
			imageDoc.Pixbuf = pix.ScaleSimple (Width,
			                                   Heigth,
			                                   InterpType.Bilinear);
		}

		protected void OnZoomOutActionActivated (object sender, EventArgs e)
		{
			if(!ImageList.IterIsValid (CurrentImage))
				return;
			if(imageDoc.Pixbuf == null)
				return;
			Pixbuf pix = (Pixbuf)ImageList.GetValue (CurrentImage, 5);
			double ratio = imageDoc.Pixbuf.Width * 0.8 / pix.Width;
			int Heigth, Width;
			Heigth = Convert.ToInt32(pix.Height * ratio);
			Width = Convert.ToInt32(pix.Width * ratio);
			imageDoc.Pixbuf = pix.ScaleSimple (Width,
			                                   Heigth,
			                                   InterpType.Bilinear);
		}
			
		protected void OnImageDocDragMotion (object o, DragMotionArgs args)
		{
			Console.WriteLine ("x={0} y={1}", args.X, args.Y);
		}

		protected void OnTreeviewImagesButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			if((int)args.Event.Button == 3)
			{       
				Gtk.Menu jBox = new Gtk.Menu();
				Gtk.MenuItem MenuItem1;
				if(treeviewImages.Selection.CountSelectedRows () == 1)
				{
					foreach(object[] row in (ListStore)comboType.Model)
					{
						MenuItem1 = new MenuItem((string)row[0]);
						MenuItem1.Activated += OnImageListPopupDocType;
						jBox.Add(MenuItem1);       
					}
					MenuItem1 = new SeparatorMenuItem();
					jBox.Add(MenuItem1);
					MenuItem1 = new MenuItem("Удалить");
					MenuItem1.Activated += OnImageListPopupDelete;
					jBox.Add(MenuItem1);           
				}
				MenuItem1 = new MenuItem("Выбрать для всех");
				Gtk.Menu jBox2 = new Gtk.Menu();
				Gtk.MenuItem MenuItem2;
				MenuItem1.Submenu = jBox2;
				foreach(object[] row in (ListStore)comboType.Model)
				{
					MenuItem2 = new MenuItem((string)row[0]);
					MenuItem2.ButtonPressEvent += OnImageListPopupDocTypeAll;
					jBox2.Append(MenuItem2);       
				}
				jBox.Append(MenuItem1);

				MenuItem1 = new MenuItem("Повернуть все");
				jBox2 = new Gtk.Menu();
				MenuItem1.Submenu = jBox2;
				MenuItem2 = new MenuItem("на 90°");
				MenuItem2.ButtonPressEvent += OnImageListPopupRotate90All;
				jBox2.Append(MenuItem2);
				MenuItem2 = new MenuItem("на 180°");
				MenuItem2.ButtonPressEvent += OnImageListPopupRotate180All;
				jBox2.Append(MenuItem2);
				MenuItem2 = new MenuItem("на 270°");
				MenuItem2.ButtonPressEvent += OnImageListPopupRotate270All;
				jBox2.Append(MenuItem2);
				jBox.Append(MenuItem1);

				jBox.ShowAll();
				jBox.Popup();
			}
		}

		protected void OnImageListPopupDelete(object sender, EventArgs Arg)
		{
			TreeIter iter;
			
			treeviewImages.Selection.GetSelected(out iter);
			ImageList.Remove (ref iter);
		}

		protected void OnImageListPopupDocType(object sender, EventArgs Arg)
		{
			TreeIter iter;
			treeviewImages.Selection.GetSelected(out iter);
			if(ImageList.IterDepth(iter) == 1)
			{
				TreeIter parent;
				ImageList.IterParent (out parent, iter);
				iter = parent;
			}
			int Typeid = -1;
			Console.WriteLine(((Gtk.AccelLabel)((Gtk.MenuItem)sender).Child).LabelProp);
			foreach(object[] row in (ListStore)comboType.Model)
			{
				if((string)row[0] == ((Gtk.AccelLabel)((Gtk.MenuItem)sender).Child).LabelProp)
					Typeid = (int)row[1];
			}
			if(Typeid > 0)
			{
				Document doc = new Document(Typeid);
				ImageList.SetValue(iter, 3, doc);
				ImageList.SetValue(iter, 7, doc.TypeName);
				CurrentDoc = doc;
				UpdateFieldsWidgets(true);
			}
		}

		protected void OnImageListPopupDocTypeAll(object sender, ButtonPressEventArgs arg)
		{
			int Typeid = -1;
			foreach(object[] row in (ListStore)comboType.Model)
			{
				if((string)row[0] == ((Gtk.AccelLabel)((Gtk.MenuItem)sender).Child).LabelProp)
					Typeid = (int)row[1];
			}
			if(Typeid > 0)
			{
				TreeIter iter;
				if(!ImageList.GetIterFirst(out iter))
					return;
				do
				{
					if(ImageList.IterDepth(iter) == 0)
					{
						Document doc = new Document(Typeid);
						ImageList.SetValue(iter, 3, doc);
						ImageList.SetValue(iter, 7, doc.TypeName);
					}
				}while(ImageList.IterNext(ref iter));
			}
		}

		protected void OnImageListPopupRotate90All(object sender, ButtonPressEventArgs arg)
		{
			RotateAllImages(PixbufRotation.Clockwise);
		}

		protected void OnImageListPopupRotate180All(object sender, ButtonPressEventArgs arg)
		{
			RotateAllImages(PixbufRotation.Upsidedown);
		}

		protected void OnImageListPopupRotate270All(object sender, ButtonPressEventArgs arg)
		{
			RotateAllImages(PixbufRotation.Counterclockwise);
		}

		private void RotateAllImages(PixbufRotation Rotate)
		{
			TreeIter iterdoc, iterimg;
			if(!ImageList.GetIterFirst(out iterdoc))
				return;

			progresswork.Text = "Обработка изображений...";
			int CountDoc, CountImg;
			CalculateImages(out CountDoc, out CountImg);
			progresswork.Adjustment.Upper = (double) CountImg;
			MainClass.WaitRedraw();
			do
			{
				if(!ImageList.IterChildren(out iterimg, iterdoc))
					continue;
				do
				{
					Pixbuf pix = (Pixbuf) ImageList.GetValue(iterimg, 5);
					ImageList.SetValue(iterimg, 5, pix.RotateSimple(Rotate));
					pix.Dispose();
					pix = (Pixbuf) ImageList.GetValue(iterimg, 4);
					ImageList.SetValue(iterimg, 4, pix.RotateSimple(Rotate));
					pix.Dispose();

					progresswork.Adjustment.Value++;
					MainClass.WaitRedraw();
				}while(ImageList.IterNext(ref iterimg));

			}while(ImageList.IterNext(ref iterdoc));

			OnZoomFitActionActivated(null, null);

			progresswork.Text = "Ok";
			progresswork.Fraction = 0;
		}

		protected void OnDateDocDateChanged (object sender, EventArgs e)
		{
			if(CurrentDoc != null && !Clearing )
			{
				CurrentDoc.DocDate = dateDoc.Date;
				if(!dateDoc.IsEmpty)
					CurrentDoc.DocDateConfidence = 2;
				else
					CurrentDoc.DocDateConfidence = -2;
				SetRecognizeIcon(IconDate, CurrentDoc.DocDateConfidence);
				UpdateCurDocCanSave();
			}
		}

		protected void OnEntryNumberChanged (object sender, EventArgs e)
		{
			if(CurrentDoc != null && !Clearing)
			{
				CurrentDoc.DocNumber = entryNumber.Text;
				if(CurrentDoc.DocNumber != "")
					CurrentDoc.DocNumberConfidence = 2;
				else 
					CurrentDoc.DocNumberConfidence = -2;
				SetRecognizeIcon(IconNumber, CurrentDoc.DocNumberConfidence);
				UpdateCurDocCanSave();
			}
		}

		private void UpdateCurDocCanSave()
		{
			if(CurrentDoc == null)
				return;

			ImageList.SetValue(CurrentDocIter, 8, GetDocIconByState(CurrentDoc.State));
		}

		private string GetDocIconByState(DocState state)
		{
			switch (state) {
			case DocState.Good:
				return DocIconGood;
			case DocState.New:
				return DocIconNew;
			case DocState.Bad:
				return DocIconBad;
			case DocState.Attention:
				return DocIconAttention;
			default:
				return "";
			}
		}

		protected void OnComboTypeChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			if(!ImageList.IterIsValid(CurrentDocIter))
				return;
			if(!Clearing && comboType.GetActiveIter(out iter))
			{
				int Type = (int)comboType.Model.GetValue(iter, 1);
				Document doc = new Document(Type);
				ImageList.SetValue(CurrentDocIter, 3, doc);
				ImageList.SetValue(CurrentDocIter, 7, doc.TypeName);
				CurrentDoc = doc;
				UpdateFieldsWidgets(false);
			}
		}

		private void CalculateImages( out int DocCount, out int ImgCount)
		{
			TreeIter iter;
			DocCount = 0;
			ImgCount = 0;

			if(!ImageList.GetIterFirst(out iter))
				return;
			do
			{
				ImgCount += ImageList.IterNChildren(iter);
				DocCount++;
			}
			while(ImageList.IterNext(ref iter));

		}

		protected void OnSaveActionActivated (object sender, EventArgs e)
		{
			TreeIter iter, imageiter;
			bool HasIncomplete = false;;
			int CountDoc = 0;

			if(!ImageList.GetIterFirst(out iter))
				return;
			do
			{
				if((string) ImageList.GetValue(iter, 8) != DocIconGood)
					HasIncomplete = true;
				CountDoc++;
			}
			while(ImageList.IterNext(ref iter));
			
			if(HasIncomplete)
			{
				string Message = "Не во всех документах полностью заполнены необходимые поля. В базе будут сохранены только готовые к записи документы.";
				MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
				                                      MessageType.Info, 
				                                      ButtonsType.OkCancel,
				                                      Message);
				ResponseType result = (ResponseType)md.Run ();
				md.Destroy();
				if(result == ResponseType.Cancel)
					return;
			}

			//Записываем
			List<TreeIter> ForRemove = new List<TreeIter>();
			progresswork.Text = "Записываем документы в базу данных...";
			progresswork.Adjustment.Upper = CountDoc;
			ImageList.GetIterFirst(out iter);
			do
			{
				if((string) ImageList.GetValue(iter, 8) != DocIconGood)
					continue;

				Document doc = (Document)ImageList.GetValue(iter, 3);
				MySqlTransaction trans = QSMain.connectionDB.BeginTransaction();
				try
				{
					string sql = "INSERT INTO docs(number, date, create_date, user_id, type_id) " +
						"VALUES(@number, @date, @create_date, @user_id, @type_id)";
					MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
					cmd.Parameters.AddWithValue("@number", doc.DocNumber);
					cmd.Parameters.AddWithValue("@date", doc.DocDate);
					cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
					cmd.Parameters.AddWithValue("@user_id", QSMain.User.id);
					cmd.Parameters.AddWithValue("@type_id", doc.TypeId);
					cmd.ExecuteNonQuery();
					long docid = cmd.LastInsertedId;
					if(doc.CountExtraFields > 0)
					{
						cmd = new MySqlCommand();
						cmd.Connection = QSMain.connectionDB;
						cmd.Transaction = trans;
						string sqlinsert = "INSERT INTO extra_" + doc.DBTableName + "(doc_id";
						string sqlvalues = "VALUES (@doc_id";
						cmd.Parameters.AddWithValue("@doc_id", docid);
						foreach(DocFieldInfo field in doc.FieldsList)
						{
							sqlinsert += ", " + field.DBName;
							sqlvalues += ", @" + field.DBName;
							cmd.Parameters.AddWithValue(field.DBName, doc.FieldValues[field.ID]);
						}
						sqlinsert += ") ";
						sqlvalues += ")";
						cmd.CommandText = sqlinsert + sqlvalues;
						cmd.ExecuteNonQuery();
					}
					Console.WriteLine("doc");
					sql = "INSERT INTO images(order_num, doc_id, type, size, image) " +
						"VALUES(@order_num, @doc_id, @type, @size, @image)";
					int order = 1;
					ImageList.IterChildren(out imageiter, iter);
					do
					{
						Console.WriteLine("image");
						cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
						cmd.Parameters.AddWithValue("@doc_id", docid);
						cmd.Parameters.AddWithValue("@order_num", order);
						cmd.Parameters.AddWithValue("@type", "jpg");
						Pixbuf pix = (Pixbuf) ImageList.GetValue(imageiter, 5);
						byte[] rawdata = pix.SaveToBuffer("jpeg");
						cmd.Parameters.AddWithValue("@size", rawdata.LongLength);
						cmd.Parameters.AddWithValue("@image", rawdata);
						cmd.ExecuteNonQuery();
					}
					while(ImageList.IterNext(ref imageiter));

					trans.Commit ();
					progresswork.Adjustment.Value++;
					MainClass.WaitRedraw();
					ForRemove.Add(iter);
					MainClass.StatusMessage("Ok");
				}
				catch (Exception ex)
				{
					trans.Rollback ();
					Console.WriteLine(ex.ToString());
					MainClass.StatusMessage("Ошибка сохранения документов!");
					QSMain.ErrorMessage(this,ex);
					return;
				}
			}
			while(ImageList.IterNext(ref iter));
			//удаляем записаное
			foreach(TreeIter rowiter in ForRemove)
			{
				TreeIter temp = rowiter;
				ImageList.Remove(ref temp);
			}
			progresswork.Text = "Ок";
			progresswork.Fraction = 0;
		}

		protected void OnRotate90ActionActivated (object sender, EventArgs e)
		{
			if(!ImageList.IterIsValid(CurrentImage))
				return;
			Pixbuf pix = (Pixbuf) ImageList.GetValue(CurrentImage, 5);
			ImageList.SetValue(CurrentImage, 5, pix.RotateSimple(PixbufRotation.Clockwise));
			pix = (Pixbuf) ImageList.GetValue(CurrentImage, 4);
			ImageList.SetValue(CurrentImage, 4, pix.RotateSimple(PixbufRotation.Clockwise));
			OnZoomFitActionActivated(null, null);
		}

		protected void OnRotate180ActionActivated (object sender, EventArgs e)
		{
			if(!ImageList.IterIsValid(CurrentImage))
				return;
			Pixbuf pix = (Pixbuf) ImageList.GetValue(CurrentImage, 5);
			ImageList.SetValue(CurrentImage, 5, pix.RotateSimple(PixbufRotation.Upsidedown));
			pix = (Pixbuf) ImageList.GetValue(CurrentImage, 4);
			ImageList.SetValue(CurrentImage, 4, pix.RotateSimple(PixbufRotation.Upsidedown));
			OnZoomFitActionActivated(null, null);
		}

		protected void OnRotate270ActionActivated (object sender, EventArgs e)
		{
			if(!ImageList.IterIsValid(CurrentImage))
				return;
			Pixbuf pix = (Pixbuf) ImageList.GetValue(CurrentImage, 5);
			ImageList.SetValue(CurrentImage, 5, pix.RotateSimple(PixbufRotation.Counterclockwise));
			pix = (Pixbuf) ImageList.GetValue(CurrentImage, 4);
			ImageList.SetValue(CurrentImage, 4, pix.RotateSimple(PixbufRotation.Counterclockwise));
			OnZoomFitActionActivated(null, null);
		}

		protected void OnAction1Activated (object sender, EventArgs e)
		{
			TreeIter iter, imageiter;
			CurrentLog = "Запущен новый процесс распознования...\n";
			if(!ImageList.GetIterFirst(out iter))
			{
				CurrentLog += "Список изображений пуст. Остановка.";
				return;
			}
			CurrentLog += String.Format("Всего документов: {0}\n", ImageList.IterNChildren());
			progresswork.Text = "Распознование документов...";
			progresswork.Adjustment.Upper = ImageList.IterNChildren();
			MainClass.WaitRedraw();
			do
			{
				if(ImageList.IterDepth(iter) == 0)
				{
					CurrentLog += (string) ImageList.GetValue(iter, 1) + Environment.NewLine;
					Document doc = (Document) ImageList.GetValue(iter, 3);
					if(doc == null)
					{
						CurrentLog += "Тип не определён. Переходим к следующему...\n";
						continue;
					}
					if(doc.Template == null)
					{
						CurrentLog += "Шаблон распознования не указан. Переходим к следующему...\n";
						continue;
					}
					int ImagesCount = ImageList.IterNChildren(iter);
					Pixbuf[] Images = new Pixbuf[ImagesCount];
					int i = 0;
					ImageList.IterChildren(out imageiter, iter);
					do
					{
						Images[i] = (Pixbuf) ImageList.GetValue(imageiter, 5);
						i++;
					}while(ImageList.IterNext(ref imageiter));
					CurrentLog += String.Format("Тип: {0}\n", doc.TypeName);
					CurrentLog += String.Format("Количество страниц: {0}\n", ImagesCount);
					CurrentLog += String.Format("\nИнициализация движка...\n");
					RecognizeDoc tess = new RecognizeDoc(doc, Images);
					tess.DiagnosticMode = checkDiagnostic.Active;
					//FIXME Для теста
					tess.parent = this;
					try
					{
						tess.Recognize();
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
						MainClass.StatusMessage("Ошибка в модуле распознования!");
						QSMain.ErrorMessage(this,ex);
						if(tess.log != "")
							ShowLog(tess.log);
					}
					finally
					{
						CurrentLog += tess.log;
					}
					ImageList.SetValue(iter, 8, GetDocIconByState(doc.State));
				}
				progresswork.Adjustment.Value++;
				MainClass.WaitRedraw();

			}while(ImageList.IterNext(ref iter));
			progresswork.Text = "Выполнено";
			progresswork.Fraction = 0;
			//FIXME Обновить текущий документ в окне.
			UpdateFieldsWidgets(false);
		}

		void SetRecognizeIcon(Gtk.Image img, float confidence)
		{
			if(confidence == 2) //Исправлено пользователем.
			{
				img.Pixbuf = img.RenderIcon(Stock.Apply, IconSize.Button, "");
				img.TooltipText =  String.Format("Исправлено.");
			}
			else if(confidence > 0.8)
			{
				img.Pixbuf = img.RenderIcon(Stock.Yes, IconSize.Button, "");
				img.TooltipText =  String.Format("ОК. Доверие: {0}", confidence);
			}
			else if(confidence == -1) //Распознования не проводилось
			{
				img.Pixbuf = img.RenderIcon(Stock.DialogQuestion, IconSize.Button, "");
				img.TooltipText =  String.Format("Распознование не проводилось.");
			}
			else if(confidence == -2) //Результат не прошел проверку.
			{
				img.Pixbuf = img.RenderIcon(Stock.No, IconSize.Button, "");
				img.TooltipText =  String.Format("Результат не прошёл проверку.");
			}
			else
			{
				img.Pixbuf = img.RenderIcon(Stock.DialogWarning, IconSize.Button, "");
				img.TooltipText =  String.Format("Не точно. Доверие: {0}", confidence);
			}
		}

		void ShowLog(string text)
		{
			Dialog HistoryDialog = new Dialog("Лог работы модуля распознования.", this, Gtk.DialogFlags.DestroyWithParent);
			HistoryDialog.Modal = true;
			HistoryDialog.AddButton ("Закрыть", ResponseType.Close);

			TextView HistoryTextView = new TextView();
			HistoryTextView.WidthRequest = 700;
			HistoryTextView.WrapMode = WrapMode.Word;
			HistoryTextView.Sensitive = false;
			HistoryTextView.Buffer.Text = text;
			Gtk.ScrolledWindow ScrollW = new ScrolledWindow();
			ScrollW.HeightRequest = 500;
			ScrollW.Add (HistoryTextView);
			HistoryDialog.VBox.Add (ScrollW);

			HistoryDialog.ShowAll ();
			HistoryDialog.Run ();
			HistoryDialog.Destroy ();
		}

		protected void OnButtonLogClicked (object sender, EventArgs e)
		{
			ShowLog(CurrentLog);
		}

		protected void OnScanActionActivated (object sender, EventArgs e)
		{
			ScanWorks scan = null;
			try
			{
				CurrentLog += Environment.OSVersion.Platform.ToString();
				CurrentLog += "init scan\n";
				scan = new ScanWorks(this);
				progresswork.Text = "Получение изображений со сканера...";
				MainClass.WaitRedraw();
				CurrentLog += "run scanner\n";

				scan.ImageTransfer += delegate(object s, ScanWorks.ImageTransferEventArgs arg) 
				{
					TreeIter iter;
					progresswork.Adjustment.Upper = arg.AllImages;

					iter = ImageList.AppendValues (0,
					                               String.Format ("Документ {0}", NextDocNumber),
					                               "",
					                               null,
					                               null ,
					                               null,
					                               false,
					                               String.Format ("Тип неопределён"),
					                               DocIconNew
					                               );

					Pixbuf image = arg.Image;
					double ratio = 150f / Math.Max(image.Height, image.Width);
					Pixbuf thumb = image.ScaleSimple((int)(image.Width * ratio),(int)(image.Height * ratio), InterpType.Bilinear);

					NextDocNumber++;
					ImageList.AppendValues (iter,
					                        0,
					                        null,
					                        null,
					                        null,
					                        thumb,
					                        image,
					                        true,
					                        "",
					                        "");
					progresswork.Adjustment.Value++;
					MainClass.WaitRedraw();
				};

				scan.GetImages();

				treeviewImages.ExpandAll ();
				progresswork.Text = "Ок";
				progresswork.Fraction = 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка в работе со сканером!");
				QSMain.ErrorMessage(this,ex);
			}
			finally
			{
				if(scan != null)
					scan.Close ();
			}
		}
	
	}


}
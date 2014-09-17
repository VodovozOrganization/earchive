using System;
using Gtk;
using QSProjectsLib;
using QSWidgetLib;
using earchive;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using NLog;

namespace earchive
{

	public partial class MainWindow: Gtk.Window
	{	
		private static Logger logger = LogManager.GetCurrentClassLogger();
		Gtk.ListStore DocsListStore;
		DocumentInformation CurDocType;
		int UsedExtraFields;
		InputDocs InputDocsWin;

		public MainWindow (): base (Gtk.WindowType.Toplevel)
		{
			Build ();

			MainClass.StatusBarLabel = labelStatus;
			this.Title = QSSupportLib.MainSupport.GetTitle();
			QSMain.MakeNewStatusTargetForNlog("StatusMessage", "earchive.MainClass, earchive");

			Reference.RunReferenceItemDlg += OnRunReferenceItemDialog;
			QSMain.ReferenceUpdated += OnReferenceUpdate;

			if(QSMain.User.Login == "root")
			{
				string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
				MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
				                                      MessageType.Info, 
				                                      ButtonsType.Ok,
				                                      Message);
				md.Run ();
				md.Destroy();
				OnUsersActionActivated (null, null);
				return;
			}

			//Загружаем информацию о пользователе
			if(QSMain.User.TestUserExistByLogin (true))
				QSMain.User.UpdateUserInfoByLogin ();
			UsersAction.Sensitive = QSMain.User.admin;
			labelUser.LabelProp = QSMain.User.Name;
			ActionDocTypes.Sensitive = QSMain.User.Permissions["edit_db"];
			//buttonDelete.Sensitive = QSMain.User.Permissions["can_edit"];
			buttonInput.Sensitive = QSMain.User.Permissions["can_edit"];

			// Создаем главное окно
			ComboWorks.ComboFillReference (comboDocType, "doc_types", ComboWorks.ListMode.OnlyItems);
			selectperiodDocs.ActiveRadio = SelectPeriod.Period.Week;
		}
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}

		protected void OnReferenceUpdate(object sender, QSMain.ReferenceUpdatedEventArgs e)
		{
			switch (e.ReferenceTable) {
			case "doc_types":
					ComboWorks.ComboFillReference (comboDocType, "doc_types", ComboWorks.ListMode.OnlyItems);
			break;
			}
		}

		protected void OnUsersActionActivated (object sender, EventArgs e)
		{
			Users WinUser = new QSProjectsLib.Users();
			WinUser.Show();
			WinUser.Run ();
			WinUser.Destroy ();
		}

		protected void OnDialogAuthenticationActionActivated (object sender, EventArgs e)
		{
			QSMain.User.ChangeUserPassword (this);
		}

		void PrepareDocsTable()
		{
			UsedExtraFields = 0;
			if(CurDocType.FieldsList != null)
				foreach(DocFieldInfo item in CurDocType.FieldsList)
					if(item.Display || item.Search)
						UsedExtraFields++;

			System.Type[] Types = new System.Type[4 + UsedExtraFields];
			Types[0] =	typeof (int); //0 - id
			Types[1] =	typeof (string); //1 - number
			Types[2] =	typeof (string);//2 - doc date
			Types[3] =	typeof (string);//3 - created date;

			int i = 4;

			if(CurDocType.FieldsList != null)
				foreach(DocFieldInfo item in CurDocType.FieldsList)
				{
					if(!item.Display && !item.Search)
						continue;
					switch (item.Type) {
					case "varchar":
						Types[i] = typeof(string);
						item.ListStoreColumn = i;
						break;
					}
					i++;
				}

			DocsListStore = new Gtk.ListStore (Types);

			foreach(TreeViewColumn col in treeviewDocs.Columns)
			{
				treeviewDocs.RemoveColumn (col);
			}

			treeviewDocs.AppendColumn ("Номер", new Gtk.CellRendererText (), "text", 1);
			treeviewDocs.AppendColumn ("Дата", new Gtk.CellRendererText (), "text", 2);
			if(CurDocType.FieldsList != null)
			{
				foreach(DocFieldInfo item in CurDocType.FieldsList)
				{
					if(!item.Display)
						continue;
					switch (item.Type) {
					case "varchar":
						treeviewDocs.AppendColumn (item.Name, new Gtk.CellRendererText (), "text", item.ListStoreColumn);
						break;
					}
				}
			}
			treeviewDocs.AppendColumn ("Создан", new Gtk.CellRendererText (), "text", 3);

			treeviewDocs.Model = DocsListStore;
			treeviewDocs.ShowAll();
			buttonRefresh.Sensitive = true;
		}

		protected void OnAction2Activated (object sender, EventArgs e)
		{
			Reference winref = new Reference();
			winref.SetMode(false,false,true,true,true);
			winref.FillList("doc_types","Тип документа", "Типы документов");
			winref.Show();
			winref.Run();
			winref.Destroy();
		}

		protected void OnRunReferenceItemDialog(object sender, Reference.RunReferenceItemDlgEventArgs e)
		{
			ResponseType Result;
			switch (e.TableName)
			{
			case "doc_types":
				DocsType DocTypeEdit = new DocsType();
				if(e.NewItem)
					DocTypeEdit.NewDocsType = true;
				else 
					DocTypeEdit.Fill(e.ItemId);
				DocTypeEdit.Show();
				Result = (ResponseType)DocTypeEdit.Run();
				DocTypeEdit.Destroy();
				break;
			default:
				Result = ResponseType.None;
				break;
			}
			e.Result = Result;
		}

		protected void OnComboDocTypeChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			if(comboDocType.GetActiveIter(out iter))
			{
				int CurrentTypeId = (int)comboDocType.Model.GetValue(iter,1);
				CurDocType = new DocumentInformation(CurrentTypeId);
				PrepareDocsTable ();
				UpdateDocs ();
			}
			else
				CurDocType = null;
		}

		void UpdateDocs()
		{
			if(CurDocType == null)
				return;
			logger.Info ("Запрос документов в базе...");
			DocsListStore.Clear ();

			string sqlExtra = "";
			TreeIter iter;
			if(CurDocType.DBTableExsist)
				sqlExtra = "LEFT JOIN extra_" + CurDocType.DBTableName + " ON extra_" + CurDocType.DBTableName + 
					".doc_id = docs.id ";
			string sql = "SELECT * FROM docs " + sqlExtra +"WHERE docs.type_id = @type_id";
			if(!selectperiodDocs.IsAllTime)
				sql += " AND date BETWEEN @startdate AND @enddate";
			if(entryDocNumber.Text.Length > 0)
				sql += String.Format (" AND number LIKE '%{0}%' ", entryDocNumber.Text);
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
			if(comboDocType.GetActiveIter(out iter))
			{
				cmd.Parameters.AddWithValue ("@type_id", comboDocType.Model.GetValue(iter,1));
			}
			cmd.Parameters.AddWithValue ("@startdate", selectperiodDocs.DateBegin);
			cmd.Parameters.AddWithValue ("@enddate", selectperiodDocs.DateEnd);
			MySqlDataReader rdr = cmd.ExecuteReader ();

			while(rdr.Read ())
			{
				object[] Values = new object[4 + UsedExtraFields];
				Values[0] = rdr.GetInt32 ("id");
				if(rdr["number"] != DBNull.Value)
					Values[1] = rdr.GetString ("number");
				else 
					Values[1] = "";
				if(rdr["date"] != DBNull.Value)
					Values[2] = String.Format("{0:d}", rdr.GetDateTime ("date"));
				else 
					Values[2] = "";
				Values[3] = String.Format("{0}", rdr.GetDateTime ("create_date"));

				if(CurDocType.FieldsList != null)
				{
					foreach(DocFieldInfo item in CurDocType.FieldsList)
					{
						if(!item.Display && !item.Search)
							continue;
						switch (item.Type) {
						case "varchar":
							if(rdr[item.DBName] != DBNull.Value)
								Values[item.ListStoreColumn] = rdr.GetString (item.DBName);
							else
								Values[item.ListStoreColumn] = "";
							break;
						}
					}
				}

				DocsListStore.AppendValues (Values);
			}
			rdr.Close ();
			OnTreeviewDocsCursorChanged(null, null);
			int totaldoc = DocsListStore.IterNChildren ();
			logger.Info (RusNumber.FormatCase (totaldoc, 
				"Получен {0} документ.",
				"Получено {0} документа.",
				"Получено {0} документов."));
		}

		protected void OnSelectperiodDocsDatesChanged (object sender, EventArgs e)
		{
			if (selectperiodDocs.IsAllTime && entryDocNumber.Text.Length < 2)
				entryDocNumber.GrabFocus ();
			else
				UpdateDocs ();
		}

		protected void OnButtonInputClicked (object sender, EventArgs e)
		{
			if(InputDocsWin == null)
			{
				InputDocsWin = new InputDocs();
				InputDocsWin.DeleteEvent += OnDeleteInputDocsEvent;
				Console.WriteLine ("new");
			}
			InputDocsWin.Maximize ();
			InputDocsWin.Show ();
		}

		protected void OnDeleteInputDocsEvent( object s, DeleteEventArgs arg)
		{
			InputDocsWin.Destroy ();
			InputDocsWin = null;
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			treeviewDocs.Selection.GetSelected(out iter);
			int ItemId = (int)DocsListStore.GetValue(iter, 0);
			ViewDoc win = new ViewDoc();
			win.Fill(ItemId);
			win.Show();
			if((ResponseType)win.Run() == ResponseType.Ok)
				UpdateDocs();
			win.Destroy();
		}

		protected void OnTreeviewDocsCursorChanged (object sender, EventArgs e)
		{
			bool RowSelected = treeviewDocs.Selection.CountSelectedRows() == 1;
			buttonOpen.Sensitive = RowSelected;
			buttonDelete.Sensitive = RowSelected && QSMain.User.Permissions["can_edit"];
		}

		protected void OnTreeviewDocsRowActivated (object o, RowActivatedArgs args)
		{
			OnButtonOpenClicked(null, null);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			treeviewDocs.Selection.GetSelected(out iter);
			int itemid = (int) DocsListStore.GetValue(iter, 0);

			string sql = "DELETE FROM docs WHERE id = @id";
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
			cmd.Parameters.AddWithValue ("@id", itemid);
			cmd.ExecuteNonQuery ();
			UpdateDocs();
		}

		protected void OnButtonRefreshClicked (object sender, EventArgs e)
		{
			UpdateDocs();
		}

		protected void OnAction3Activated (object sender, EventArgs e)
		{
			//throw new System.NotImplementedException ();
			Statistics stat = new Statistics();
			stat.Show();
			stat.Run();
			stat.Destroy();
		}

		protected void OnAboutActionActivated(object sender, EventArgs e)
		{
			AboutDialog dialog = new AboutDialog ();
			dialog.ProgramName = "QS: Электронный архив";

			Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			dialog.Version = version.ToString (version.Build == 0 ? 3 : 4);

			dialog.Logo = Gdk.Pixbuf.LoadFromResource ("earchive.icons.logo.png");

			dialog.Comments = "Программа позволяет создать и вести базу отсканированных документов." +
				"\nРазработана на MonoDevelop с использованием открытых технологий Mono, GTK#, MySQL." +
				"\nТелефон тех. поддержки +7(812)575-79-44";

			dialog.Copyright = "Quality Solution 2014";

			dialog.Authors = new string [] {"Ганьков Андрей <gav@qsolution.ru>"};

			dialog.Website = "http://www.qsolution.ru/";

			dialog.Run ();
			dialog.Destroy();
		}

		protected void OnQuitActionActivated(object sender, EventArgs e)
		{
			Application.Quit();
		}

		protected void OnButtonSearchClicked(object sender, EventArgs e)
		{
			UpdateDocs ();
		}

		protected void OnEntryDocNumberActivated(object sender, EventArgs e)
		{
			buttonSearch.Click ();
		}
	}
}
using System;
using System.Collections.Generic;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;

namespace earchive
{
	public partial class DocsType : Gtk.Dialog
	{
		public bool NewDocsType;
		int TypeId;
		bool ExtraTableCreated = false;
		string OriginalTableName = "";
		
		ListStore FieldsListStore;

		public DocsType ()
		{
			this.Build ();

			//Создаем таблицу "Полей"
			FieldsListStore = new Gtk.ListStore (typeof (int), //0 - ID
			                                     typeof (string), // 1 - Name
			                                     typeof (string) // 2 - DB_name
			                                     );
			treeviewFields.AppendColumn ("Имя", new Gtk.CellRendererText (), "text", 1);
			treeviewFields.AppendColumn ("Имя в БД", new Gtk.CellRendererText (), "text", 2);

			treeviewFields.Model = FieldsListStore;
			treeviewFields.ShowAll();

			//Устанавливаем права
			bool UserRight = QSMain.User.Permissions["edit_db"];
			buttonAdd.Sensitive = UserRight;
			buttonDelete.Sensitive = UserRight;
			entryDBTable.Sensitive = UserRight;

			//FIXME Убрать только для теста
			System.Data.DataTable schema = QSMain.connectionDB.GetSchema("Columns", new string[4] { null, QSMain.connectionDB.Database, "docs", "number"});
			foreach (System.Data.DataRow row in schema.Rows)
			{
				foreach (System.Data.DataColumn col in schema.Columns)
				{
					Console.WriteLine("{0} = {1}", col.ColumnName, row[col]);
				}
				Console.WriteLine("============================");
			}
		}

		public void Fill(int id)
		{
			NewDocsType = false;
			
			MainClass.StatusMessage("Запрос типа документа №" + id +"...");
			string sql = "SELECT doc_types.* FROM doc_types " +
					"WHERE doc_types.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", id);
				MySqlDataReader rdr = cmd.ExecuteReader();
				
				if(!rdr.Read())
					return;
				
				TypeId = rdr.GetInt32 ("id");
				entryID.Text = rdr["id"].ToString();
				entryName.Text = rdr["name"].ToString ();
				if(rdr["table_name"] == DBNull.Value)
					ExtraTableCreated = false;
				else
				{
					entryDBTable.Text = rdr["table_name"].ToString ();
					OriginalTableName = rdr["table_name"].ToString ();
					ExtraTableCreated = true;
				}
				if(rdr["description"] != DBNull.Value)
					textviewDescription.Buffer.Text = rdr.GetString ("description");
				
				rdr.Close();
				
				this.Title = entryName.Text;
				
				UpdateFields ();
				MainClass.StatusMessage("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения информации о типах документов!");
				QSMain.ErrorMessage(this,ex);
			}
			
			TestCanSave();
			OnTreeviewFieldsCursorChanged(null, null);

		}

		void UpdateFields()
		{
			//Получаем таблицу полей
			string sql = "SELECT extra_fields.* FROM extra_fields " +
				"WHERE extra_fields.doc_type_id = @id";
			
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
			cmd.Parameters.AddWithValue("@id", TypeId);
			MySqlDataReader rdr = cmd.ExecuteReader();

			FieldsListStore.Clear ();
			while (rdr.Read())
			{
				FieldsListStore.AppendValues(rdr.GetInt32 ("id"),
				                             rdr.GetString ("name"),
				                             rdr.GetString ("db_name"));
			}
			rdr.Close();
			TestTableField ();
		}

		protected void OnTreeviewFieldsCursorChanged (object sender, EventArgs e)
		{
			bool UserRightOk = QSMain.User.Permissions["edit_db"];
			bool SelectOk = treeviewFields.Selection.CountSelectedRows() == 1;
			buttonEdit.Sensitive = SelectOk;
			buttonDelete.Sensitive = UserRightOk && SelectOk;
		}

		protected void TestCanSave ()
		{
			bool Nameok = entryName.Text != "";
			bool TableNameOk = entryDBTable.Text == "" ||
				System.Text.RegularExpressions.Regex.IsMatch (entryDBTable.Text, "^[a-zA-Z0-9_]+$");

			buttonOk.Sensitive = Nameok && TableNameOk;
			buttonApply.Sensitive = Nameok && TableNameOk;
		}

		void TestTableField()
		{
			buttonAdd.Sensitive = ExtraTableCreated;
		}

		void SaveChanges()
		{
			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction ();
			MainClass.StatusMessage("Записываем информацию о типе документа...");
			try
			{
				//Провекра существует ли тип с таким названием. Для работы меню в окне ввода документов нужны уникальные названия.
				string sql = "SELECT id FROM doc_types WHERE name=@name";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.Parameters.AddWithValue ("@name", entryName.Text);
				bool exist = false;
				MySqlDataReader rdr = cmd.ExecuteReader ();
				while(rdr.Read ())
				{
					if(rdr.GetInt32 ("id") != TypeId)
						exist = true;
				}
				rdr.Close ();
				if(exist)
				{
					MessageDialog md = new MessageDialog ( this, DialogFlags.Modal,
					                                      MessageType.Warning, 
					                                      ButtonsType.Ok,
					                                      String.Format ("Тип документа с именем \"{0}\" уже существует.", entryName.Text));
					md.Run();
					md.Destroy ();
					trans.Rollback ();
					return;
				}

				//Работаем с таблицей БД
				if(entryDBTable.Text == "" && ExtraTableCreated)
				{
					DeleteTable (trans);
				}
				else if(!ExtraTableCreated && entryDBTable.Text != "")
				{ 
					CreateTable (trans);
				}
				else if(OriginalTableName != entryDBTable.Text && entryDBTable.Text != "" && ExtraTableCreated)
				{
					RenameTable (trans);
				}
				// Работаем с шаблоном
				if(NewDocsType)
					sql = "INSERT INTO doc_types(name, table_name, description)" +
						"VALUES (@name, @table_name, @description)";
				else
					sql = "UPDATE doc_types SET name = @name, table_name = @table_name, description = @description " +
						"WHERE id = @id";
				cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.Parameters.AddWithValue ("@id", TypeId);
				cmd.Parameters.AddWithValue ("@name", entryName.Text);
				if(entryDBTable.Text != "")
					cmd.Parameters.AddWithValue ("@table_name", entryDBTable.Text);
				else 
					cmd.Parameters.AddWithValue ("@table_name", DBNull.Value);
				if(textviewDescription.Buffer.Text != "")
					cmd.Parameters.AddWithValue ("@description", textviewDescription.Buffer.Text);
				else 
					cmd.Parameters.AddWithValue ("@description", DBNull.Value);
				cmd.ExecuteNonQuery ();
				if(NewDocsType)
					TypeId = Convert.ToInt32 (cmd.LastInsertedId);
				trans.Commit ();
				TestTableField ();
				MainClass.StatusMessage("Ok");
			}
			catch (Exception ex)
			{
				trans.Rollback ();
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи типа документов!");
				QSMain.ErrorMessage(this,ex);
			}
		}

		void CreateTable(MySqlTransaction trans)
		{
			MessageDialog md = new MessageDialog ( this, DialogFlags.Modal,
			                                      MessageType.Question, 
			                                      ButtonsType.YesNo,
			                                      String.Format ("Создать таблицу extra_{0} в БД?", entryDBTable.Text));
			if((ResponseType)md.Run () == ResponseType.Yes)
			{
				string sql = "CREATE  TABLE IF NOT EXISTS `extra_" + entryDBTable.Text + "` (" +
					"`doc_id` INT UNSIGNED NOT NULL , " +
						"PRIMARY KEY (`doc_id`) , " +
						"CONSTRAINT `fk_extra_" + entryDBTable.Text + "_1` " +
						"FOREIGN KEY (`doc_id` ) " +
						"REFERENCES `docs` (`id` ) " +
						"ON DELETE CASCADE " +
						"ON UPDATE CASCADE) " +
						"ENGINE = InnoDB " +
						"DEFAULT CHARACTER SET = utf8 " +
						"COLLATE = utf8_general_ci ";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.ExecuteNonQuery ();
				ExtraTableCreated = true;
				OriginalTableName = entryDBTable.Text;
			}
			md.Destroy();
		}

		void RenameTable(MySqlTransaction trans)
		{
			MessageDialog md = new MessageDialog ( this, DialogFlags.Modal,
			                                      MessageType.Question, 
			                                      ButtonsType.YesNo,
			                                      String.Format ("Переименовать таблицу в extra_{0}?", entryDBTable.Text));
			if((ResponseType)md.Run () == ResponseType.Yes)
			{
				string sql = "RENAME TABLE extra_" + OriginalTableName + " TO extra_" + entryDBTable.Text;
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.ExecuteNonQuery ();
				OriginalTableName = entryDBTable.Text;
			}
			md.Destroy();
		}

		void DeleteTable(MySqlTransaction trans)
		{
			MessageDialog md = new MessageDialog ( this, DialogFlags.Modal,
			                                      MessageType.Question, 
			                                      ButtonsType.YesNo,
			                                      String.Format ("Удалить таблицу extra_{0} вместе со всеми расширенными полями шаблона?", entryDBTable.Text));
			if((ResponseType)md.Run () == ResponseType.Yes)
			{
				string sql = "DROP TABLE extra_" + entryDBTable.Text;
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.ExecuteNonQuery ();
				OriginalTableName = entryDBTable.Text;

				sql = "DELETE FROM extra_fields WHERE doc_type_id = @id";
				cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.Parameters.AddWithValue ("@id", TypeId);
				FieldsListStore.Clear ();
			}
			md.Destroy();
		}

		protected void OnEntryNameChanged (object sender, EventArgs e)
		{
			TestCanSave ();
		}

		protected void OnEntryDBTableChanged (object sender, EventArgs e)
		{
			TestCanSave ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			SaveChanges ();
		}

		protected void OnButtonApplyClicked (object sender, EventArgs e)
		{
			SaveChanges ();
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ExtraField win = new ExtraField();
			win.TableName = entryDBTable.Text;
			win.DocTypeID = TypeId;
			win.Show ();
			if((ResponseType)win.Run () == ResponseType.Ok)
				UpdateFields();
			win.Destroy ();
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			treeviewFields.Selection.GetSelected(out iter);
			int ItemId = (int)FieldsListStore.GetValue(iter, 0);

			ExtraField win = new ExtraField();
			win.TableName = entryDBTable.Text;
			win.DocTypeID = TypeId;
			win.Fill (ItemId);
			win.Show ();
			if((ResponseType)win.Run () == ResponseType.Ok)
				UpdateFields();
			win.Destroy ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			MessageDialog md = new MessageDialog ( this, DialogFlags.Modal,
			                                      MessageType.Question, 
			                                      ButtonsType.YesNo,
			                                      String.Format ("Удалить поле extra_{0} из БД? Будут потеряны все внесенные в эту колонку данные.", entryDBTable.Text));
			if((ResponseType)md.Run () == ResponseType.No)
				return;

			TreeIter iter;
			treeviewFields.Selection.GetSelected(out iter);
			string FieldName = FieldsListStore.GetValue(iter, 2).ToString ();
			int FieldId = (int)FieldsListStore.GetValue(iter, 0);

			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction ();
			MainClass.StatusMessage("Удаляем поле...");
			try
			{
				//Работаем с таблицей БД
				string sql = "ALTER TABLE extra_" + entryDBTable.Text + 
					" DROP COLUMN " + FieldName;
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.ExecuteNonQuery ();

				// Работаем с внутренними данными
				sql = "DELETE FROM extra_fields WHERE id = @id";
				cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.Parameters.AddWithValue ("@id", FieldId);
				cmd.ExecuteNonQuery ();

				trans.Commit ();
				UpdateFields ();
				MainClass.StatusMessage("Ok");
			}
			catch (Exception ex)
			{
				trans.Rollback ();
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка удаления поля!");
				QSMain.ErrorMessage(this,ex);
			}
		}

		protected void OnTreeviewFieldsRowActivated (object o, RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}
	}
}
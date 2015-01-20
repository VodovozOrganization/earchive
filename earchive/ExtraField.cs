using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;

namespace earchive
{
	public partial class ExtraField : Gtk.Dialog
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public bool NewField = true;
		public string TableName;
		public int DocTypeID;
		string OriginalFieldName;
		int OriginalSize;

		public ExtraField ()
		{
			this.Build ();

			//Установка прав на редактирование
			bool UserRight = QSMain.User.Permissions["edit_db"];
			entryDBName.Sensitive = UserRight;
			comboType.Sensitive = UserRight;
			spinSize.Sensitive = UserRight;
		}

		public void Fill(int id)
		{
			NewField = false;
			logger.Info("Запрос поля №" + id +"...");
			string sql = "SELECT extra_fields.*, doc_types.table_name FROM extra_fields " +
					"LEFT JOIN doc_types ON extra_fields.doc_type_id = doc_types.id " +
					"WHERE extra_fields.id = @id";
			QSMain.CheckConnectionAlive();
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", id);
				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{
					if(!rdr.Read())
						return;

					entryID.Text = rdr.GetString ("id");
					entryName.Text = rdr.GetString ("name");
					checkbuttonDisplay.Active = rdr.GetBoolean ("display");
					checkbuttonSearch.Active = rdr.GetBoolean ("search");
					entryDBName.Text = rdr.GetString ("db_name");
					OriginalFieldName = rdr.GetString ("db_name");
					TableName = rdr.GetString ("table_name");
				}
				this.Title = entryName.Text;

				System.Data.DataTable schema = QSMain.connectionDB.GetSchema("Columns", new string[4] { null, 
					QSMain.connectionDB.Database,
					"extra_" + TableName,
					entryDBName.Text});

				//Заполняем тип
				switch (schema.Rows[0]["DATA_TYPE"].ToString ()) {
				case "varchar":
					comboType.Active = 0;
					spinSize.Value = Convert.ToDouble (schema.Rows[0]["CHARACTER_MAXIMUM_LENGTH"]);
					OriginalSize = Convert.ToInt32 (schema.Rows[0]["CHARACTER_MAXIMUM_LENGTH"]);
					break;
				default:
					throw new ApplicationException(String.Format ("Тип поля {0} не поддерживается программой.", schema.Rows[0]["DATA_TYPE"]));
				}
				//Запрещаем менять тип поля
				comboType.Sensitive = false;

				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog(this, "Ошибка получения информации о поле!", logger, ex);
			}
			
			TestCanSave();
		}

		void TestCanSave()
		{
			bool Nameok = entryName.Text != "";
			bool DBNameOk = entryDBName.Text == "" ||
				System.Text.RegularExpressions.Regex.IsMatch (entryDBName.Text, "^[a-zA-Z0-9_]+$");
			bool TypeOk = comboType.Active >= 0;

			buttonOk.Sensitive = Nameok && DBNameOk && TypeOk;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			QSMain.CheckConnectionAlive();
			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction ();
			logger.Info("Записываем информацию о поле...");
			try
			{
				//Работаем с таблицей БД
				if(NewField)
				{ 
					CreateField (trans);
				}
				else if(OriginalFieldName != entryDBName.Text || OriginalSize != spinSize.ValueAsInt)
				{
					ChangeField (trans);
				}
				// Работаем с внутренними данными
				string sql;
				if(NewField)
					sql = "INSERT INTO extra_fields(name, db_name, doc_type_id, display, search)" +
						"VALUES (@name, @db_name, @doc_type_id, @display, @search)";
				else
					sql = "UPDATE extra_fields SET name = @name, db_name = @db_name, doc_type_id = @doc_type_id, " +
						"display = @display, search = @search " +
						"WHERE id = @id";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				cmd.Parameters.AddWithValue ("@id", entryID.Text);
				cmd.Parameters.AddWithValue ("@name", entryName.Text);
				cmd.Parameters.AddWithValue ("@db_name", entryDBName.Text);
				cmd.Parameters.AddWithValue ("@doc_type_id", DocTypeID);
				cmd.Parameters.AddWithValue ("@display", checkbuttonDisplay.Active);
				cmd.Parameters.AddWithValue ("@search", checkbuttonSearch.Active);

				cmd.ExecuteNonQuery ();
				trans.Commit ();
				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				trans.Rollback ();
				QSMain.ErrorMessageWithLog(this, "Ошибка сохранения поля!", logger, ex);
			}
		}

		void CreateField(MySqlTransaction trans)
		{
			string sql = "ALTER TABLE extra_" + TableName + 
				" ADD COLUMN " + entryDBName.Text +
				" VARCHAR(" + spinSize.Text +") NULL DEFAULT NULL";
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
			cmd.ExecuteNonQuery ();
		}

		void ChangeField(MySqlTransaction trans)
		{
			string sql = "ALTER TABLE extra_" + TableName + 
				" CHANGE COLUMN " + OriginalFieldName + " " + entryDBName.Text +
				" VARCHAR(" + spinSize.Text +") NULL DEFAULT NULL";
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
			cmd.ExecuteNonQuery ();
		}
	}
}

